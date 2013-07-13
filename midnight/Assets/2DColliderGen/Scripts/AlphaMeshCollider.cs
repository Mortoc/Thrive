using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Reflection;

#if UNITY_EDITOR

//-------------------------------------------------------------------------
/// <summary>
/// A component to generate a MeshCollider from an image with alpha channel.
/// </summary>
[ExecuteInEditMode]
public class AlphaMeshCollider : MonoBehaviour {
	
	public enum InitState {
		NotChecked = 0,
		Yes = 1,
		No = 2
	}
	
	public static string DEFAULT_OUTPUT_DIR = "Assets/Colliders/Generated";
	
	[SerializeField] protected bool mWasInitialized = false;
	
	public bool mLiveUpdate = true;
	public float mAlphaOpaqueThreshold = 0.1f;
	public float mVertexReductionDistanceTolerance = 0.0f;
	public int mMaxPointCount = 20;
	public float mThickness = 1.0f;
	[SerializeField] protected bool mFlipHorizontal = false;
	[SerializeField] protected bool mFlipVertical = false;
	public bool mConvex = false;
	public bool mFlipInsideOutside = false;

	public Vector2 mOutlineScale = Vector2.one;
	public Vector3 mOutlineOffset = Vector3.zero;
	
	public float   mCustomRotation = 0.0f;
	public Vector2 mCustomScale = Vector2.one;
	public Vector3 mCustomOffset = Vector3.zero;
	[SerializeField] protected string mColliderMeshDirectory = ""; // only the directory without the filename. Without a trailing slash. E.g. "Assets/Colliders/Generated"
	[SerializeField] protected string mGroupSuffix = ""; // an optional suffix to append to the filename (before the extension) to distinguish groups of the same sprite with different parameters.
	public string mColliderMeshFilename = "";  // the filename of the mesh without the directory but including the extension. E.g. "Island2_446_flipped_h.dae"
	
	public Texture2D mMainTex = null;
	[SerializeField] protected Texture2D mCustomTex;
	public bool mIsAtlasUsed = false;
	public int mAtlasFrameIndex = 0;
	public string mAtlasFrameTitle = null;
	/// mAtlasFramePositionInPixels describes the offset of the top-left corner of the sub-texture from the top-left origin of the atlasTex
	public Vector2 mAtlasFramePositionInPixels = Vector2.zero;
	public Vector2 mAtlasFrameSizeInPixels = Vector2.zero;
	public float mAtlasFrameRotation = 0.0f;
	public bool [,] mBinaryImage = null;
	
	public bool mInactiveBaseImageIsAtlas = false;
	public int mInactiveBaseImageWidth = 0;
	public int mInactiveBaseImageHeight = 0;
	public Vector2 mInactiveBaseImageOutlineScale = Vector2.one;
	public Vector3 mInactiveBaseImageOutlineOffset = Vector3.zero;
	
	protected bool mCurrentFlipHorizontal = false;
	protected bool mCurrentFlipVertical = false;
	
	public bool mHasOTSpriteComponent = false;
	protected Component mOTSpriteComponent = null; // read via reflection, therefore of type 'Component' instead of 'OTSprite'.
	public bool mHasSmoothMovesSpriteComponent = false;
	protected Component mSmoothMovesSpriteComponent = null; // read via reflection, therefore of type 'Component'.
	protected InitState mHasSmoothMovesBoneAnimationParent = InitState.NotChecked;
	protected string mFullSmoothMovesNodeString = null; // e.g. "Root/Torso/ArmLeft/Weapon"
	public bool mIsSmoothMovesNodeWithoutSprite = false;
	public bool mHasSmoothMovesAnimBoneColliderComponent = false;
	protected Component mSmoothMovesAnimBoneColliderComponent = null; // read via reflection, therefore of type 'Component'.
	protected Component mSmoothMovesBoneAnimation = null; // read via reflection, therefore of type 'Component'.
	protected string mFullSmoothMovesAssemblyName = "SmoothMoves_Runtime, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null";
	protected Type mSmoothMovesAtlasType = null;
	protected Type mSmoothMovesBoneAnimationDataType = null;
	protected bool mHasTK2DSpriteComponent;
	protected Component mTK2DSpriteComponent; // read via reflection, therefore of type 'Component'.
	
	public bool mCopyOTSpriteFlipping = true;
	public bool mCopySmoothMovesSpriteDimensions = true;
	
	protected List<Vector2> mIntermediateOutlineVertices = null; // temporary result, stored to run the algorithm from a common intermediate point instead of from the start.
	protected PolygonOutlineFromImageFrontend mOutlineAlgorithm = null;
	protected IslandDetector mIslandDetector = null;
	protected Vector3[] mResultVertices = null;
	protected int[] mResultTriangleIndices = null;
	
	// OTTileMap support
	protected bool mHasOTTileMapComponent = false;
	protected Component mOTTileMapComponent = null; // read via reflection, therefore of type 'Component'.
	public int mOTTileMapLayerIndex = 0;
	public int mOTTileMapMapPosX = 0;
	public int mOTTileMapMapPosY = 0;
	public int mOTTileMapWidth = 0;
	
	
	// Setters and Getters
	public bool FlipHorizontal {
	    get { return this.mFlipHorizontal; }
	    set { this.mFlipHorizontal = value; } // don't call UpdateColliderMeshFilename() here, otherwise the filename does not fit the calculated collider for the registry!
	}
	public bool FlipVertical {
	    get { return this.mFlipVertical; }
	    set { this.mFlipVertical = value; } // don't call UpdateColliderMeshFilename() here, otherwise the filename does not fit the calculated collider for the registry!
	}
	
	public string ColliderMeshDirectory {
	    get { return this.mColliderMeshDirectory; }
	    set {
			string directory =  value;
			if (directory == null || directory.Equals("")) {
				directory = DEFAULT_OUTPUT_DIR;
			}
			
			char[] charsToRemove = {'/', '\\', ' '};
			this.mColliderMeshDirectory = directory.TrimEnd(charsToRemove);
		}
	}
	
	public string GroupSuffix {
		get { return this.mGroupSuffix; }
	    set { this.mGroupSuffix = value; UpdateColliderMeshFilename(); }
	}
	
	public bool CanRecalculateCollider {
		get { return UsedTexture != null && !mColliderMeshFilename.Equals(""); }
	}
	
	public bool CanReloadCollider {
		get { return !mColliderMeshFilename.Equals(""); }
	}
	
	public bool CanRewriteCollider {
		get { return !mColliderMeshFilename.Equals(""); }
	}
	
	public Texture2D UsedTexture
	{
	    get {
			if (mCustomTex)
				return mCustomTex;
			else
				return mMainTex;
		}
	}
	
	public Texture2D CustomTex {
		get { return this.mCustomTex; }
		set { this.mCustomTex = value; UpdateColliderMeshFilename(); }
	}
	
	public List<Vector2> IntermediateOutlineVertices {
		get { return this.mIntermediateOutlineVertices; }
		set { this.mIntermediateOutlineVertices = value; }
	}
	
	public PolygonOutlineFromImageFrontend OutlineAlgorithm {
		get { return mOutlineAlgorithm; }
		set { mOutlineAlgorithm = value; }
	}
	
	public Mesh ColliderMesh {
		get { 
			MeshCollider collider = this.GetComponent<MeshCollider>();
			if (collider == null) {
				return null;
			}
			return collider.sharedMesh;
		}
		set {
			
			MeshCollider collider = this.GetComponent<MeshCollider>();
			if (collider == null) {
				collider = AddEmptyMeshColliderComponent();
			}
			collider.sharedMesh = null;
			collider.sharedMesh = value;
		}
	}
	
	//-------------------------------------------------------------------------
	public void SetOTTileMap(Component otTileMapComponent, int layerIndex, int mapPosX, int mapPosY, int mapWidth) {
		mHasOTTileMapComponent = true;
		mOTTileMapComponent = otTileMapComponent; // read via reflection, therefore of type 'Component'.
		mOTTileMapLayerIndex = layerIndex;
		mOTTileMapMapPosX = mapPosX;
		mOTTileMapMapPosY = mapPosY;
		mOTTileMapWidth =  mapWidth;
	}
	
	//-------------------------------------------------------------------------
	void RemoveOTTileMap() {
		mHasOTTileMapComponent = false;
		mOTTileMapComponent = null; // read via reflection, therefore of type 'Component'.
		mOTTileMapLayerIndex = 0;
		mOTTileMapMapPosX = 0;
		mOTTileMapMapPosY = 0;
		mOTTileMapWidth = 0;
	}
	
	//-------------------------------------------------------------------------
	// Use this for initialization - we use this script from the editor only
	void Update() {
		
		if (!Application.isEditor || Application.isPlaying)
			return;
		
		if (!mWasInitialized)
			InitWithPreferencesValues();
		
		CheckForOTSpriteComponent(out mHasOTSpriteComponent, out mOTSpriteComponent);
		CheckForSmoothMovesSpriteComponent(out mHasSmoothMovesSpriteComponent, out mSmoothMovesSpriteComponent);
		if (mHasSmoothMovesBoneAnimationParent == InitState.NotChecked) { // new part for SmoothMoves v2.x
			CheckForSmoothMovesBoneAnimationParent(out mHasSmoothMovesBoneAnimationParent, out mSmoothMovesBoneAnimation, out mFullSmoothMovesNodeString);
		}
		
		if (mHasOTSpriteComponent) {
			EnsureOTSpriteCustomPhysicsMode(mOTSpriteComponent);
		}
		
		if (mHasOTSpriteComponent && mCopyOTSpriteFlipping) {
			GetOTSpriteFlipParameters(mOTSpriteComponent, out mFlipHorizontal, out mFlipVertical);
		}
		
		if (UsedTexture == null) {
			InitTextureParams();
		}
		
		if (mColliderMeshDirectory.Equals("")) {
			mColliderMeshDirectory = DEFAULT_OUTPUT_DIR;
		}
		if (mColliderMeshFilename.Equals("")) {
			UpdateColliderMeshFilename();
		}
		
		
		bool hasChangedFlipState = (mCurrentFlipHorizontal != mFlipHorizontal) || (mCurrentFlipVertical != mFlipVertical);
		if (hasChangedFlipState) {
			UpdateColliderMeshFilename();
			mCurrentFlipHorizontal = mFlipHorizontal;
			mCurrentFlipVertical = mFlipVertical;
		}
		
		MeshCollider collider = this.GetComponent<MeshCollider>();
		bool isMeshColliderMissing = (collider == null || collider.sharedMesh == null);
		
		if (isMeshColliderMissing || hasChangedFlipState) {
			
			AlphaMeshColliderRegistry.Instance.ReloadOrRecalculateSingleCollider(this);
		}
	}
	
	//-------------------------------------------------------------------------
	protected MeshCollider AddEmptyMeshColliderComponent() {
		
		// Note: we have to intermediately clear the meshFilter.sharedMesh,
		// otherwise we get the warning message "Compute mesh inertia tensor
		// failed for one of the actor's mesh shapes" because the flat
		// sprite-quad is used for volume calculations.
		MeshFilter meshFilter = null;
		Mesh mesh = null;
		Rigidbody rigidbody = this.GetComponent<Rigidbody>();
		if (rigidbody != null) {
			meshFilter = this.GetComponent<MeshFilter>();
			if (meshFilter != null) {
				mesh = meshFilter.sharedMesh;
				meshFilter.sharedMesh = null;
			}
		}
		
		MeshCollider resultCollider = this.gameObject.AddComponent<MeshCollider>();
		
		if (rigidbody != null && meshFilter != null) {
			meshFilter.sharedMesh = mesh;
		}
		return resultCollider;
	}
	
	//-------------------------------------------------------------------------
	protected void InitWithPreferencesValues() {
		
		this.ColliderMeshDirectory = AlphaMeshColliderPreferences.Instance.DefaultColliderDirectory;
		this.mLiveUpdate = AlphaMeshColliderPreferences.Instance.DefaultLiveUpdate;
		this.mMaxPointCount = AlphaMeshColliderPreferences.Instance.DefaultColliderPointCount;
		this.mConvex = AlphaMeshColliderPreferences.Instance.DefaultConvex;
		this.mThickness = AlphaMeshColliderPreferences.Instance.DefaultAbsoluteColliderThickness;
		
		mWasInitialized = true;
	}
	
	//-------------------------------------------------------------------------
	/// <returns>
	/// The collider mesh path including the directory and the filename plus extension.
	/// E.g. "Assets/Colliders/Generated/Island2_446_flipped_h.dae".
	/// </returns>
	public string FullColliderMeshPath() {
		return mColliderMeshDirectory + "/" + mColliderMeshFilename;
	}
	
	//-------------------------------------------------------------------------
	public void RecalculateCollider() {
		GenerateAndStoreColliderMesh();
		UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
		
		if (!LoadAlreadyGeneratedColliderMesh()) {
			if (!LoadAlreadyGeneratedColliderMesh()) {
				Debug.LogError("Unable to load the generated Collider Mesh '" + FullColliderMeshPath() + "'!");
			}
		}
	}
	
	//-------------------------------------------------------------------------
	public void RecalculateColliderFromPreviousResult() {
		ReduceAndStoreColliderMesh();
		UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
		
		if (!LoadAlreadyGeneratedColliderMesh()) {
			if (!LoadAlreadyGeneratedColliderMesh()) {
				Debug.LogError("Unable to load the generated Collider Mesh '" + FullColliderMeshPath() + "'!");
			}
		}
	}
	
	//-------------------------------------------------------------------------
	public void RewriteColliderToFile() {
		ExportMeshToFile();
		UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
		
		if (!LoadAlreadyGeneratedColliderMesh()) {
			if (!LoadAlreadyGeneratedColliderMesh()) {
				Debug.LogError("Unable to load the generated Collider Mesh '" + FullColliderMeshPath() + "'!");
			}
		}
	}
	
	//-------------------------------------------------------------------------
	static public void ReloadAllSimilarCollidersInScene(string colliderMeshPathToReload) {
		object[] alphaMeshColliders = GameObject.FindSceneObjectsOfType(typeof(AlphaMeshCollider));
		foreach (AlphaMeshCollider collider in alphaMeshColliders)
		{
			if (collider.FullColliderMeshPath().Equals(colliderMeshPathToReload)) {
				collider.ReloadCollider();
			}
		}
	}
	
	//-------------------------------------------------------------------------
	static public void UpdateSimilarCollidersIntermediateOutlineVertices(string colliderMeshPathToReload, List<Vector2> intermediateOutlineVertices) {
		object[] alphaMeshColliders = GameObject.FindSceneObjectsOfType(typeof(AlphaMeshCollider));
		foreach (AlphaMeshCollider collider in alphaMeshColliders)
		{
			if (collider.FullColliderMeshPath().Equals(colliderMeshPathToReload)) {
				collider.IntermediateOutlineVertices = intermediateOutlineVertices;
			}
		}
	}
	
	//-------------------------------------------------------------------------
	public void ReloadCollider() {
		bool alreadyGenerated = LoadAlreadyGeneratedColliderMesh();
		if (!alreadyGenerated) {
			UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
			
			if (!LoadAlreadyGeneratedColliderMesh()) {
				Debug.LogError("Unable to load the Collider Mesh '" + FullColliderMeshPath() + "'!");
			}
		}
	}
	
	//-------------------------------------------------------------------------
	bool LoadAlreadyGeneratedColliderMesh() {
		
		Mesh loadedColliderMesh = (Mesh) UnityEditor.AssetDatabase.LoadAssetAtPath(FullColliderMeshPath(), typeof(Mesh));
		if (loadedColliderMesh == null)
			return false; // unable to load the collider mesh.
		
		MeshCollider collider = this.GetComponent<MeshCollider>();
		if (collider == null) {
			collider = AddEmptyMeshColliderComponent();
		}
		
		collider.sharedMesh = null;
		collider.sharedMesh = loadedColliderMesh;
		
		return true;
	}
	
	//-------------------------------------------------------------------------
	public void UpdateColliderMeshFilename() {
		mColliderMeshFilename = GetColliderMeshFilename();
	}
	
	//-------------------------------------------------------------------------
	void CheckForOTSpriteComponent(out bool hasOTSpriteComponent, out Component otSpriteComponent) {
		otSpriteComponent = this.GetComponent("OTSprite");
		if (otSpriteComponent != null) {
			hasOTSpriteComponent = true;
		}
		else {
			hasOTSpriteComponent = false;
		}
	}
	
	//-------------------------------------------------------------------------
	void CheckForSmoothMovesSpriteComponent(out bool hasSmoothMovesSpriteComponent, out Component smoothMovesSpriteComponent) {
		smoothMovesSpriteComponent = this.GetComponent("Sprite");
		
		if (smoothMovesSpriteComponent != null) {
			Type spriteComponentType = smoothMovesSpriteComponent.GetType();
			FieldInfo fieldTextureGUID = spriteComponentType.GetField("textureGUID");
			FieldInfo fieldAtlas = spriteComponentType.GetField("atlas");
			FieldInfo fieldPivotOffsetOverride = spriteComponentType.GetField("pivotOffsetOverride");
			
			if (fieldTextureGUID != null &&
				fieldAtlas != null &&
				fieldPivotOffsetOverride != null) {
				
				hasSmoothMovesSpriteComponent = true;
			}
			else {
				hasSmoothMovesSpriteComponent = false;
			}
		}
		else {
			hasSmoothMovesSpriteComponent = false;
		}
	}
	
	//-------------------------------------------------------------------------
	void CheckForSmoothMovesBoneAnimationParent(out InitState hasSmoothMovesBoneAnimationParent,
												out Component smoothMovesBoneAnimationParent,
												out string fullNodeString) {
		string tempNodeString = "";
		smoothMovesBoneAnimationParent = FindBoneAnimationParent(this.transform, ref tempNodeString);
		if (smoothMovesBoneAnimationParent != null) {
			hasSmoothMovesBoneAnimationParent = InitState.Yes;
			fullNodeString = tempNodeString;
			return;
		}
		else {
			hasSmoothMovesBoneAnimationParent = InitState.No;
			fullNodeString = null;
			return;
		}
	}
	
	//-------------------------------------------------------------------------
	void CheckForSmoothMovesAnimBoneColliderComponent(out bool hasSmoothMovesAnimBoneColliderComponent,
													  out Component smoothMovesAnimBoneColliderComponent,
													  out Component smoothMovesBoneAnimation,
													  out string nodeHierarchyString) {
		
		smoothMovesAnimBoneColliderComponent = this.GetComponent("AnimationBoneCollider");
		
		if (smoothMovesAnimBoneColliderComponent != null) {
			Type componentType = smoothMovesAnimBoneColliderComponent.GetType();
			FieldInfo fieldBoneAnimation = componentType.GetField("_boneAnimation", BindingFlags.NonPublic | BindingFlags.Instance);
			
			if (fieldBoneAnimation != null) {
				string tempNodeString = "";
				smoothMovesBoneAnimation = FindBoneAnimationParent(smoothMovesAnimBoneColliderComponent.transform, ref tempNodeString);
				nodeHierarchyString = tempNodeString;
				
				if (smoothMovesBoneAnimation != null) {
					hasSmoothMovesAnimBoneColliderComponent = true;
					return;
				}
			}
		}
		smoothMovesAnimBoneColliderComponent = null;
		smoothMovesBoneAnimation = null;
		hasSmoothMovesAnimBoneColliderComponent = false;
		nodeHierarchyString = null;
	}
	
	//-------------------------------------------------------------------------
	void CheckForTK2DSpriteComponent(out bool hasTK2DSpriteComponent, out Component tk2dSpriteComponent) {
		tk2dSpriteComponent = this.GetComponent("tk2dSprite");
		
		if (tk2dSpriteComponent != null) {
			Type componentType = tk2dSpriteComponent.GetType();
			FieldInfo fieldSpriteId = componentType.GetField("_spriteId", BindingFlags.Instance | BindingFlags.NonPublic);
			
			if (fieldSpriteId != null) {
				hasTK2DSpriteComponent = true;
				return;
			}
		}
		
		hasTK2DSpriteComponent = false;
		tk2dSpriteComponent = null;
	}
	
	//-------------------------------------------------------------------------
	static Component FindBoneAnimationParent(Transform searchStartNode, ref string nodeHierarchyString) {
		Transform boneNode = searchStartNode;
		nodeHierarchyString = "";
		Component result = boneNode.GetComponent("BoneAnimation");
		
		while (result == null && boneNode.parent != null) {
			
			if (nodeHierarchyString == "")
				nodeHierarchyString = boneNode.name;
			else
				nodeHierarchyString = boneNode.name + "/" + nodeHierarchyString;
			
			boneNode = boneNode.parent;
			result = boneNode.GetComponent("BoneAnimation");
		}
		
		if (result == null) {
			nodeHierarchyString = null;
		}
		return result;
	}
	
	//-------------------------------------------------------------------------
	static bool EnsureOTSpriteCustomPhysicsMode(Component otSpriteComponent) {
		Type otSpriteType = otSpriteComponent.GetType();
		
		FieldInfo fieldPhysics = otSpriteType.GetField("_physics");
		if (fieldPhysics == null) {
			Debug.LogError("Detected a missing '_physics' member variable at an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		object enumValue = fieldPhysics.GetValue(otSpriteComponent);
		Type enumType = enumValue.GetType();
		FieldInfo enumValueCustomPhysics = enumType.GetField("Custom");
		if (enumValueCustomPhysics == null) {
			Debug.LogError("Detected a missing 'Custom' member variable at an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		int customPhysicsIntValue = (int)enumValueCustomPhysics.GetValue(enumType);
		int oldPhysicsIntValue = (int) enumValue;
		
		bool wasCustomPhysicsBefore = (oldPhysicsIntValue == customPhysicsIntValue);
		if (!wasCustomPhysicsBefore) {
			object newEnumValue = Enum.ToObject(enumType, customPhysicsIntValue);
			fieldPhysics.SetValue(otSpriteComponent, newEnumValue);
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	static void GetOTSpriteFlipParameters(Component otSpriteComponent, out bool flipHorizontal, out bool flipVertical) {
		Type otSpriteType = otSpriteComponent.GetType();
		
		FieldInfo fieldFlipHorizontal = otSpriteType.GetField("_flipHorizontal");
		FieldInfo fieldFlipVertical = otSpriteType.GetField("_flipVertical");
		if (fieldFlipHorizontal == null || fieldFlipVertical == null) {
			Debug.LogError("Detected a missing '_flipHorizontal' or '_flipVertical' member variable at an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			flipHorizontal = false;
			flipVertical = false;
			return;
		}
		
		flipHorizontal = (bool) fieldFlipHorizontal.GetValue(otSpriteComponent);
		flipVertical = (bool) fieldFlipVertical.GetValue(otSpriteComponent);
	}
	
	//-------------------------------------------------------------------------
	static void GetSmoothMovesSpriteDimensions(Component smoothMovesSpriteComponent, out Vector2 customScale, out Vector3 customOffset) {
		Type spriteType = smoothMovesSpriteComponent.GetType();
		
		FieldInfo fieldSize = spriteType.GetField("size");
		FieldInfo fieldBottomLeft = spriteType.GetField("_bottomLeft");
		if (fieldSize == null || fieldBottomLeft == null) {
			Debug.LogError("Detected a missing 'size' or '_bottomLeft' member variable at an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			customScale = Vector2.one;
			customOffset = Vector3.zero;
			return;
		}
		
		customScale = (Vector2) fieldSize.GetValue(smoothMovesSpriteComponent);
		Vector2 offset2D = (Vector2) fieldBottomLeft.GetValue(smoothMovesSpriteComponent);
		customOffset = new Vector3(offset2D.x + (customScale.x / 2), offset2D.y + (customScale.y / 2), 0);
	}
	
	//-------------------------------------------------------------------------
	void InitTextureParams() {
		mIntermediateOutlineVertices = null;
		bool mHasNoTextureAtGameObjectButSomewhereElse = (mHasSmoothMovesAnimBoneColliderComponent || (mHasSmoothMovesBoneAnimationParent == InitState.Yes && !mIsSmoothMovesNodeWithoutSprite));
		if (!mHasNoTextureAtGameObjectButSomewhereElse) {
			if (this.renderer && this.renderer.sharedMaterial) {
				mMainTex = (Texture2D) this.renderer.sharedMaterial.mainTexture;
			}
			else {
				mMainTex = null;
			}
		}
		
		mOutlineScale = Vector2.one;
		mOutlineOffset = Vector3.zero;
		
		mIsAtlasUsed = false;
		
		mInactiveBaseImageIsAtlas = false;
		mInactiveBaseImageWidth = 100;
		mInactiveBaseImageHeight = 100;
		if (mMainTex != null) {
			mInactiveBaseImageWidth = mMainTex.width;
			mInactiveBaseImageHeight = mMainTex.height;
		}
		
		mInactiveBaseImageOutlineScale = Vector2.one;
		mInactiveBaseImageOutlineOffset = Vector2.zero;
		
		if (mCustomTex == null) {
			ReadNormalImageParametersFromComponents();
		}
		else {
			ReadCustomImageParametersFromComponents();
		}
		
		if (mHasSmoothMovesAnimBoneColliderComponent || mHasSmoothMovesBoneAnimationParent == InitState.Yes) {
			EnsureSmoothMovesBoneAnimHasRestoreComponent(mSmoothMovesBoneAnimation);
		}
	}
	
	//-------------------------------------------------------------------------
	void ReadNormalImageParametersFromComponents() {
		if (mHasOTTileMapComponent) {
			mIsAtlasUsed = true;
			ReadOTTileMapParams(mOTTileMapComponent, mOTTileMapLayerIndex, mOTTileMapMapPosX, mOTTileMapMapPosY, out mMainTex, out mAtlasFrameIndex, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation);
		}
		if (mHasOTSpriteComponent) {
			mIsAtlasUsed = ReadOTSpriteContainerParams(mOTSpriteComponent, out mAtlasFrameIndex, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation);
		}
		if (mHasSmoothMovesSpriteComponent) {
			if (mCopySmoothMovesSpriteDimensions) {
				GetSmoothMovesSpriteDimensions(mSmoothMovesSpriteComponent, out mOutlineScale, out mOutlineOffset);
			}
			mIsAtlasUsed = ReadSmoothMovesSpriteAtlasParams(mSmoothMovesSpriteComponent, UsedTexture, out mAtlasFrameIndex, out mAtlasFrameTitle, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation);
		}
		if (mHasSmoothMovesAnimBoneColliderComponent) {
			mIsAtlasUsed = ReadSmoothMovesAnimatedSpriteAtlasParams(mFullSmoothMovesNodeString, mSmoothMovesBoneAnimation, out mMainTex, out mAtlasFrameTitle, out mAtlasFrameIndex, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation, out mOutlineScale, out mOutlineOffset);
		}
		// TODO: get rid of the above code-branch, remove the old mHasSmoothMovesAnimBoneColliderComponent part.
		if (mHasSmoothMovesBoneAnimationParent == InitState.Yes) {
			mIsAtlasUsed = ReadSmoothMovesAnimatedSpriteAtlasParams(mFullSmoothMovesNodeString, mSmoothMovesBoneAnimation, out mMainTex, out mAtlasFrameTitle, out mAtlasFrameIndex, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation, out mOutlineScale, out mOutlineOffset);
			if (!mIsAtlasUsed)
				mIsSmoothMovesNodeWithoutSprite = true;
			else
				mIsSmoothMovesNodeWithoutSprite = false;
		}
	}
	
	//-------------------------------------------------------------------------
	void ReadCustomImageParametersFromComponents() {
		int discardOutInt = 0;
		Texture2D discardOutTexture = null;
		Vector2 discardOutVector;
		string discardOutString;
		Vector2 frameSize = Vector2.zero;
		float frameRotation = 0.0f;
		
		
		if (mHasOTSpriteComponent) {
			mInactiveBaseImageIsAtlas = ReadOTSpriteContainerParams(mOTSpriteComponent, out discardOutInt, out discardOutVector, out frameSize, out frameRotation);
		}
		if (mHasSmoothMovesSpriteComponent) {
			if (mCopySmoothMovesSpriteDimensions) {
				GetSmoothMovesSpriteDimensions(mSmoothMovesSpriteComponent, out mInactiveBaseImageOutlineScale, out mInactiveBaseImageOutlineOffset);
				mOutlineOffset = mInactiveBaseImageOutlineOffset;
			}
			mInactiveBaseImageIsAtlas = ReadSmoothMovesSpriteAtlasParams(mSmoothMovesSpriteComponent, mMainTex, out discardOutInt, out discardOutString, out discardOutVector, out frameSize, out frameRotation);
		}
		if (mHasSmoothMovesAnimBoneColliderComponent) {
			mInactiveBaseImageIsAtlas = ReadSmoothMovesAnimatedSpriteAtlasParams(mFullSmoothMovesNodeString, mSmoothMovesBoneAnimation, out discardOutTexture, out discardOutString, out discardOutInt, out discardOutVector, out frameSize, out frameRotation, out mInactiveBaseImageOutlineScale, out mInactiveBaseImageOutlineOffset);
			mOutlineOffset = mInactiveBaseImageOutlineOffset;
		}
		if (mHasSmoothMovesBoneAnimationParent == InitState.Yes) {
			mInactiveBaseImageIsAtlas = ReadSmoothMovesAnimatedSpriteAtlasParams(mFullSmoothMovesNodeString, mSmoothMovesBoneAnimation, out discardOutTexture, out discardOutString, out discardOutInt, out discardOutVector, out frameSize, out frameRotation, out mInactiveBaseImageOutlineScale, out mInactiveBaseImageOutlineOffset);
			mOutlineOffset = mInactiveBaseImageOutlineOffset;
		}
		
		if (mInactiveBaseImageIsAtlas) {
			bool isRotated90Degrees = frameRotation == 90.0f || frameRotation == 270.0f || frameRotation == -90.0f;
			if (!isRotated90Degrees) {
				mInactiveBaseImageWidth = (int) frameSize.x;
				mInactiveBaseImageHeight = (int) frameSize.y;
			}
			else {
				mInactiveBaseImageWidth = (int) frameSize.y;
				mInactiveBaseImageHeight = (int) frameSize.x;
			}
		}
	}
	
	//-------------------------------------------------------------------------
	bool ReadOTSpriteContainerParams(object otSprite, out int atlasFrameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation) {
		framePositionInPixels = frameSizeInPixels = Vector2.zero;
		frameRotation = 0.0f;
		atlasFrameIndex = 0;
		
		// Check if we use a texture atlas instead of a normal image.
		Type otSpriteType = otSprite.GetType();
		
		FieldInfo fieldSpriteContainer = otSpriteType.GetField("_spriteContainer");
		FieldInfo fieldframeIndex = otSpriteType.GetField("_frameIndex");
		if (fieldSpriteContainer == null || fieldframeIndex == null) {
			Debug.LogWarning("Failed to read _spriteContainer or _frameIndex field of the OTSprite component. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			return false;
		}
		System.Object otSpriteContainer = fieldSpriteContainer.GetValue(otSprite);
		atlasFrameIndex = (int)fieldframeIndex.GetValue(otSprite);
		
		if (otSpriteContainer != null) {
			// we have a texture atlas or sprite sheet attached.
			Type containerType = otSpriteContainer.GetType();
			FieldInfo fieldAtlasData = containerType.GetField("atlasData");
			FieldInfo fieldFramesXY = containerType.GetField("_framesXY");
			if (fieldAtlasData != null) {
				return ReadOTSpriteAtlasParams(otSpriteContainer, atlasFrameIndex, out framePositionInPixels, out frameSizeInPixels, out frameRotation);
			}
			else if (fieldFramesXY != null) {
				ReadOTSpriteSheetParams(otSpriteContainer, atlasFrameIndex, out framePositionInPixels, out frameSizeInPixels, out frameRotation);
			}
			else {
				Debug.LogWarning("_spriteContainer of OTSprite is neither of type OTSpriteContainer nor OTSpriteAtlas (neither 'atlasData' nor '_framesXY' members were found). Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
				return false;
			}
			
			return true;
		}
		return false;
	}
	
	//-------------------------------------------------------------------------
	bool ReadOTTileMapParams(object otTileMap, int otTileMapLayerIndex, int otTileMapMapPosX, int otTileMapMapPosY, out Texture2D mMainTex, out int atlasFrameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation) {
		framePositionInPixels = frameSizeInPixels = Vector2.zero;
		frameRotation = 0.0f;
		atlasFrameIndex = 0;
		mMainTex = null;
		
		System.Type otTileMapType = otTileMap.GetType();
		FieldInfo fieldLayers = otTileMapType.GetField("layers");
		if (fieldLayers == null) {
			Debug.LogError("Detected a missing 'layers' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		Array layersArray = (Array) fieldLayers.GetValue(otTileMap);
		if (otTileMapLayerIndex >= layersArray.Length) {
			Debug.LogError("Error: found a layer index that is larger than the OTTileMap.layers array - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		object otTileMapLayer = layersArray.GetValue(otTileMapLayerIndex);
		System.Type otTileMapLayerType = otTileMapLayer.GetType();
		FieldInfo fieldTiles = otTileMapLayerType.GetField("tiles");
		if (fieldTiles == null) {
			Debug.LogError("Detected a missing 'tiles' member variable at OTTileMapLayer class - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		int[] tileIndices = (int[]) fieldTiles.GetValue(otTileMapLayer);
		int tileIndex = tileIndices[otTileMapMapPosY * mOTTileMapWidth +  otTileMapMapPosX];
		atlasFrameIndex = tileIndex;
		
		object tileSet = GetOTTileSetForTileIndex(otTileMap, tileIndex);
		
		// read OTTileSet.image (Texture)
		System.Type tileSetType = tileSet.GetType();
		FieldInfo fieldImage = tileSetType.GetField("image");
		if (fieldImage == null) {
			Debug.LogError("Detected a missing 'image' member variable at OTTileSet class - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		Texture2D texture = (Texture2D) fieldImage.GetValue(tileSet);
		mMainTex = texture;
		
		// Own version of OTTileMap::GetUV(int tile), returns an array of 4 uv vectors.
		Vector2[] uvs = GetOTTileMapUVCoords(otTileMap, tileSet, tileIndex);
		
		float normalizedX = uvs[0].x;
		float normalizedY = 1.0f - uvs[0].y;
		float normalizedWidth = uvs[2].x - uvs[0].x;
		float normalizedHeight = uvs[0].y - uvs[2].y;
		
		frameSizeInPixels = new Vector2(normalizedWidth * texture.width, normalizedHeight * texture.height);
		framePositionInPixels = new Vector2(Mathf.Floor(normalizedX * texture.width),
											Mathf.Clamp(Mathf.Floor(normalizedY * texture.height), 0, texture.height-1));
		
		// read OTTileMap.layers[0].rotation (int[])
		FieldInfo fieldRotation = otTileMapLayerType.GetField("rotation");
		if (fieldRotation == null) {
			// OK. This parameter is only present in newer Orthello versions.
		}
		else {
		    // we directly set the GameObject's transform eulerAngles value, since the object has nothing else attached.
			int[] rotationValues = (int[]) fieldRotation.GetValue(otTileMapLayer);
			int rotation = rotationValues[otTileMapMapPosY * mOTTileMapWidth +  otTileMapMapPosX];
			this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, rotation);
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	// Note: In this method we manually search through the tile sets.
	//       It is needed because the tileSetLookup member variable is cleared
	//       when we would like to read from it.
	public static object GetOTTileSetForTileIndex(object otTileMap, int tileIndex) {
		System.Type otTileMapType = otTileMap.GetType();
		FieldInfo fieldTileSets = otTileMapType.GetField("tileSets");
		if (fieldTileSets == null) {
			Debug.LogError("Detected a missing 'tileSets' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		Array tileSets = (Array) fieldTileSets.GetValue(otTileMap);
		int tileSetIndex = 0;
		object tileSet = null;
		for ( ; tileSetIndex < tileSets.Length; ++tileSetIndex) {
			object otTileSet = tileSets.GetValue(tileSetIndex);
			System.Type otTileSetType = otTileSet.GetType();
			FieldInfo fieldFirstGid = otTileSetType.GetField("firstGid"); // int
			FieldInfo fieldTilesXY = otTileSetType.GetField("tilesXY"); // Vector2
			int firstGid = (int) fieldFirstGid.GetValue(otTileSet);
			Vector2 tilesXY = (Vector2) fieldTilesXY.GetValue(otTileSet);
			int numTilesInSet = (int)(tilesXY.x * tilesXY.y);
			if ((firstGid <= tileIndex) && (tileIndex < firstGid + numTilesInSet)) {
				tileSet = tileSets.GetValue(tileSetIndex);
				return tileSet;
			}
		}
		return null;
	}
	
	//-------------------------------------------------------------------------
	// Note: This is a functional copy of OTTileMap.GetUV(int tile).
	//       It is needed because the tileSetLookup member variable is cleared
	//       when we would like to read from it, leading to an exception thrown
	//       in the GetUV() method.
	Vector2[] GetOTTileMapUVCoords(object otTileMap, object tileSet, int tileIndex) {
		int tile = tileIndex;
		
		// The following code does this through reflection:
        // int ty = (int)Mathf.Floor((float)(tile-ts.firstGid) / ts.tilesXY.x);
        // int tx = (tile-ts.firstGid+1) - (int)((float)ty * ts.tilesXY.x) - 1;
		System.Type otTileSetType = tileSet.GetType();
		FieldInfo fieldFirstGid = otTileSetType.GetField("firstGid"); // int
		FieldInfo fieldTilesXY = otTileSetType.GetField("tilesXY"); // Vector2
		int tsFirstGid = (int) fieldFirstGid.GetValue(tileSet);
		Vector2 tsTilesXY = (Vector2) fieldTilesXY.GetValue(tileSet);
		int ty = (int)Mathf.Floor((float)(tile-tsFirstGid) / tsTilesXY.x);
		int tx = (tile-tsFirstGid+1) - (int)((float)ty * tsTilesXY.x) - 1;
		
		// The following code does this through reflection:
		// float ux = (1f / ts.imageSize.x);
        // float uy = (1f / ts.imageSize.y);
        // float usx = ux *  ts.tileSize.x;
        // float usy = uy *  ts.tileSize.y;		
		FieldInfo fieldImageSize = otTileSetType.GetField("imageSize"); // Vector2
		FieldInfo fieldTileSize = otTileSetType.GetField("tileSize"); // Vector2
		if (fieldImageSize == null || fieldTileSize == null) {
			Debug.LogError("Detected a missing 'fieldImageSize' or 'fieldTileSize' member variable at OTTileSet class - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		Vector2 tsImageSize = (Vector2) fieldImageSize.GetValue(tileSet);
		Vector2 tsTileSize = (Vector2) fieldTileSize.GetValue(tileSet);
		float ux = (1f / tsImageSize.x);
        float uy = (1f / tsImageSize.y);
        float usx = ux *  tsTileSize.x;
        float usy = uy *  tsTileSize.y;
        
        // float utx = (ux * tx); // this was a comment in the original code part.
		// The following code does this through reflection:
        //float utx = (ux * ts.margin)+(tx * usx);
		//if (tx>0)utx+=(tx * ts.spacing * ux);
		//
        //float uty = (uy * ts.margin)+(ty * usy);
		//if (ty>0)uty+=(ty * ts.spacing * uy);
		FieldInfo fieldMargin = otTileSetType.GetField("margin"); // int
		FieldInfo fieldSpacing = otTileSetType.GetField("spacing"); // int
		int tsMargin = 0;
		int tsSpacing = 0;
		if (fieldMargin == null || fieldSpacing == null) {
			Debug.LogError("Detected a missing 'fieldMargin' or 'fieldSpacing' member variable at OTTileSet class - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			// this is a non-fatal error, we continue with margin and spacing of 0.
		}
		else {
			tsMargin = (int) fieldMargin.GetValue(tileSet);
			tsSpacing = (int) fieldSpacing.GetValue(tileSet);
		}
		
		float utx = (ux * tsMargin)+(tx * usx);
		if (tx>0)utx+=(tx * tsSpacing * ux);
		float uty = (uy * tsMargin)+(ty * usy);
		if (ty>0)uty+=(ty * tsSpacing * uy);
		
		// Read otTileMap.reduceBleeding field.
		System.Type otTileMapType = otTileMap.GetType();
		FieldInfo fieldReduceBleeding = otTileMapType.GetField("reduceBleeding");
		bool tileMapReduceBleeding = true;
		if (fieldReduceBleeding == null) {
			Debug.LogError("Detected a missing 'reduceBleeding' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			// this is a non-fatal error, we continue with reduceBleeding set to true.
		}
		else {
			tileMapReduceBleeding = (bool) fieldReduceBleeding.GetValue(otTileMap);
		}
		
		
		// create a tiny fraction (uv size / 25 )
		// that will be removed from the UV coords
		// to reduce bleeding.
		int dv = 25;
        float dx = usx / dv;
        float dy = usy / dv;
		if (!tileMapReduceBleeding)
		{
			dx = 0; dy = 0;
		}
		
		return new Vector2[] { 
            new Vector2(utx + dx,1 - uty - dy ), new Vector2(utx + usx - dx,1 - uty - dy), 
            new Vector2(utx + usx - dx ,1- uty - usy + dy), new Vector2(utx + dx,1 - uty - usy + dy) 
        };
	}
	
	//-------------------------------------------------------------------------
	bool ReadSmoothMovesSpriteAtlasParams(object sprite, Texture2D atlasTexture, out int frameIndex, out string frameTitle, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation) {
		bool isAtlasUsed = false;
		frameIndex = 0;
		frameTitle = null;
		framePositionInPixels = Vector2.zero;
		frameSizeInPixels = Vector2.zero;
		frameRotation = 0;
		
		Type spriteType = sprite.GetType();
		FieldInfo fieldTextureIndex = spriteType.GetField("_textureIndex");
		FieldInfo fieldAtlas = spriteType.GetField("atlas");
		object atlas = null;
		if (fieldTextureIndex == null || fieldAtlas == null) {
			Debug.LogError("Detected a missing '_textureIndex' or 'atlas' member variable at an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
		}
		else {
			// member found - it can still be set to null if no atlas is used, though.
			atlas = fieldAtlas.GetValue(sprite);
		}
		
		if (atlas != null) {
			frameIndex = (int) fieldTextureIndex.GetValue(sprite);
			frameTitle = GetTextureNameAtAtlasFrameIndex(atlas, frameIndex);
			
			Type atlasType = atlas.GetType();
			FieldInfo fieldUVs = atlasType.GetField("uvs");
			if (fieldUVs == null) {
				Debug.LogError("Detected a missing 'uvs' member variable at an altas of an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			}
			else {
				List<UnityEngine.Rect> uvList = (List<UnityEngine.Rect>) fieldUVs.GetValue(atlas);
				float normalizedX = uvList[mAtlasFrameIndex].x;
				float normalizedY = 1.0f - uvList[mAtlasFrameIndex].y;
				float normalizedWidth = uvList[mAtlasFrameIndex].width;
				float normalizedHeight = uvList[mAtlasFrameIndex].height;
				
				frameSizeInPixels = new Vector2(normalizedWidth * atlasTexture.width, normalizedHeight * atlasTexture.height);
				framePositionInPixels = new Vector2(Mathf.Floor(normalizedX * atlasTexture.width),
													Mathf.Clamp(Mathf.Floor(normalizedY * atlasTexture.height) - frameSizeInPixels.y, 0, atlasTexture.height-1));
				
				frameRotation = 0;
				isAtlasUsed = true;
			}
		}
		return isAtlasUsed;
	}
	
	//-------------------------------------------------------------------------
	string GetTextureNameAtAtlasFrameIndex(object atlas, int frameIndex) {
		
		if (frameIndex < 0) {
			return null;
		}
		Type atlasType = atlas.GetType();
		FieldInfo fieldTextureNames = atlasType.GetField("textureNames");
		if (fieldTextureNames == null) {
			Debug.LogError("Detected a missing 'textureNames' member variable at an altas of an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		IEnumerable textureNamesList = (IEnumerable) fieldTextureNames.GetValue(atlas);
		if (textureNamesList == null) {
			return null;
		}
			
		int index = 0;
		foreach (string textureName in textureNamesList) {
			if (index++ == frameIndex) {
				return textureName;
			}
		}
		return null;
	}
	
	//-------------------------------------------------------------------------
	void EnsureSmoothMovesBoneAnimHasRestoreComponent(Component smoothMovesBoneAnimation) {
		AlphaMeshColliderSmoothMovesRestore restoreComponent = smoothMovesBoneAnimation.GetComponent<AlphaMeshColliderSmoothMovesRestore>();
		if (restoreComponent == null) {
			smoothMovesBoneAnimation.gameObject.AddComponent<AlphaMeshColliderSmoothMovesRestore>();
		}
	}
	
	//-------------------------------------------------------------------------
	bool ReadSmoothMovesAnimatedSpriteAtlasParams(string fullTargetBoneName, Component smoothMovesBoneAnimation,
									      		  out Texture2D atlasImage, out string frameTitle, out int frameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation,
												  out Vector2 customScale, out Vector3 customOffset) {
		
		int boneIndex = 0;
		atlasImage = null;
		frameTitle = null;
		frameIndex = 0;
		framePositionInPixels = frameSizeInPixels = customOffset = Vector2.zero;
		frameRotation = 0.0f;
		customScale = Vector3.one;
		
		Type boneAnimType = smoothMovesBoneAnimation.GetType();
		mFullSmoothMovesAssemblyName = boneAnimType.Assembly.FullName;
		
		object animationData = GetSmoothMovesAnimationData(smoothMovesBoneAnimation);
		
		Type animationDataType = animationData.GetType();
		
		FieldInfo fieldBoneTransformPathsList = animationDataType.GetField("boneTransformPaths");
		if (fieldBoneTransformPathsList == null) {
			Debug.LogError("Detected a missing 'boneTransformPaths' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		IEnumerable boneTransformPathsList = (IEnumerable) fieldBoneTransformPathsList.GetValue(animationData);
		int index = 0;
		foreach (string fullBoneName in boneTransformPathsList) {
			
			if (fullBoneName.Equals(fullTargetBoneName)) {
				boneIndex = index;
				break;
			}
			
			++index;
		}
		
		string shortBoneName = System.IO.Path.GetFileName(fullTargetBoneName);
		frameTitle = shortBoneName;
		
		float importScale = 1.0f;
		FieldInfo fieldImportScale = animationDataType.GetField("importScale");
		if (fieldImportScale != null) {
			importScale = (float) fieldImportScale.GetValue(animationData);
		}
		
		FieldInfo fieldBoneSourceArray = boneAnimType.GetField("mBoneSource");
		if (fieldBoneSourceArray == null) {
			Debug.LogError("Detected a missing 'mBoneSource' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		IEnumerable boneSourceArray = (IEnumerable) fieldBoneSourceArray.GetValue(smoothMovesBoneAnimation);
		// the following lines actually do this:
		//   object boneSource = boneSourceArray[boneIndex];
		object boneSource = null;
		index = 0;
		foreach (object currentBoneSource in boneSourceArray) {
			if (index++ == boneIndex) {
				boneSource = currentBoneSource;
				break;
			}
		}
		
		Type boneSourceType = boneSource.GetType();
		FieldInfo fieldMaterialIndex = boneSourceType.GetField("materialIndex");
		if (fieldMaterialIndex == null) {
			Debug.LogError("Detected a missing 'materialIndex' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		int materialIndex = (int) fieldMaterialIndex.GetValue(boneSource);
		if (materialIndex < 0) {
            // This branch is entered at a SmoothMoves node without a sprite (transform bone only).
            atlasImage = null;
            frameIndex = 0;
            framePositionInPixels = Vector2.zero;
            frameSizeInPixels = Vector2.zero;
            frameRotation = 0.0f;
            customScale = Vector3.one;
            customOffset = Vector2.zero;
			return false;
		}
		FieldInfo fieldBoneQuad = boneSourceType.GetField("boneQuad");
		if (fieldBoneQuad == null) {
			Debug.LogError("Detected a missing 'boneQuad' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		object boneQuad = fieldBoneQuad.GetValue(boneSource);
		Type boneQuadType = boneQuad.GetType();
		FieldInfo fieldVertexIndices = boneQuadType.GetField("vertexIndices");
		if (fieldVertexIndices == null) {
			Debug.LogError("Detected a missing 'vertexIndices' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		IEnumerable vertexIndices = (IEnumerable) fieldVertexIndices.GetValue(boneQuad);
		// the following lines actually do this:
		//   int uvIndexBottomLeft = vertexIndices[2];
		//   int uvIndexTopRight = vertexIndices[3];
		int uvIndexBottomLeft = 0;
		int uvIndexTopRight = 0;
		index = 0;
		foreach (int vertexIndex in vertexIndices) {
			if (index == 2) {
				uvIndexBottomLeft = vertexIndex;
			}
			else if (index == 3) {
				uvIndexTopRight = vertexIndex;
				break; // we are done after index 3.
			}
			++index;
		}
		
		FieldInfo fieldMaterialsArray = boneAnimType.GetField("mMaterials");
		if (fieldMaterialsArray == null) {
			Debug.LogError("Detected a missing 'mMaterials' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		IEnumerable materialsArray = (IEnumerable) fieldMaterialsArray.GetValue(smoothMovesBoneAnimation);
		
		index = 0;
		UnityEngine.Material targetMaterial = null;
		foreach (UnityEngine.Material material in materialsArray) {
			if (index++ == materialIndex) {
				targetMaterial = material;
				break;
			}
		}
		
		atlasImage = (Texture2D) targetMaterial.mainTexture;
		
		FieldInfo fieldUVs = boneAnimType.GetField("mUVs");
		if (fieldUVs == null) {
			Debug.LogError("Detected a missing 'mUVs' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		Vector2[] uvs = (Vector2[]) fieldUVs.GetValue(smoothMovesBoneAnimation);
		
		
		Vector2 textureSize = new Vector2(atlasImage.width, atlasImage.height);
		customScale = uvs[uvIndexTopRight] - uvs[uvIndexBottomLeft];
		if (customScale.x == 0.0f || customScale.y == 0.0f) {
			// At some nodes without sprites we have a zero-size dummy boneSource.
			atlasImage = null;
			frameIndex = 0;
			framePositionInPixels = frameSizeInPixels = customOffset = Vector2.zero;
			frameRotation = 0.0f;
			customScale = Vector3.one;
			return false;
		}
		
		customScale.Scale(textureSize);
		
		frameSizeInPixels = customScale;
		customScale *= importScale;
		
		
		float normalizedX = uvs[uvIndexBottomLeft].x;
		float normalizedY = 1.0f - uvs[uvIndexBottomLeft].y;
		framePositionInPixels = new Vector2(Mathf.Floor(normalizedX * textureSize.x),
												Mathf.Clamp(Mathf.Floor(normalizedY * textureSize.y) - frameSizeInPixels.y, 0, textureSize.y-1));
		
		frameRotation = 0.0f;
		GetSmoothMovesAtlasPivotOffset(atlasImage, frameSizeInPixels, uvs[uvIndexBottomLeft], uvs[uvIndexTopRight], out frameIndex, out customOffset);
		customOffset *= importScale;
		return true;
	}
	
	//-------------------------------------------------------------------------
	object GetSmoothMovesAnimationData(Component smoothMovesBoneAnimation) {
		Type boneAnimType = smoothMovesBoneAnimation.GetType();
		FieldInfo fieldBoneAnimationData = boneAnimType.GetField("animationData");
		if (fieldBoneAnimationData == null) {
			return GetSmoothMovesAnimationDataFromGUID(smoothMovesBoneAnimation);
		}
		object animationData = fieldBoneAnimationData.GetValue(smoothMovesBoneAnimation);
		if (animationData == null) {
			// newer version of SmoothMoves (v2.2.0 and up)
			return GetSmoothMovesAnimationDataFromGUID(smoothMovesBoneAnimation);
		}
		else {
			return animationData;
		}
	}
	
	//-------------------------------------------------------------------------
	object GetSmoothMovesAnimationDataFromGUID(Component smoothMovesBoneAnimation) {
		Type boneAnimType = smoothMovesBoneAnimation.GetType();
		FieldInfo fieldAnimationDataGUID = boneAnimType.GetField("animationDataGUID");
		if (fieldAnimationDataGUID == null) {
			Debug.LogError("Detected a missing 'animationDataGUID' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		string animationDataGUID = (string) fieldAnimationDataGUID.GetValue(smoothMovesBoneAnimation);
		if (string.IsNullOrEmpty(animationDataGUID)) {
			Debug.LogError("animationDataGUID member variable at SmoothMoves BoneAnimation component contains an empty string - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		else {
			string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(animationDataGUID);
			if (assetPath == "") {
				Debug.LogError("No animation data object found in AssetDatabase for BoneAnimation.animationDataGUID - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return null;
			}
			
			if (mSmoothMovesBoneAnimationDataType == null) {
				mSmoothMovesBoneAnimationDataType = Type.GetType("SmoothMoves.BoneAnimationData, " + mFullSmoothMovesAssemblyName);
				if (mSmoothMovesBoneAnimationDataType == null) {
					mSmoothMovesBoneAnimationDataType = Type.GetType("SmoothMoves.BoneAnimationData, SmoothMoves_Runtime, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null");
					if (mSmoothMovesBoneAnimationDataType == null) {
						mSmoothMovesBoneAnimationDataType = Type.GetType("SmoothMoves.TextureAtlas, SmoothMoves_Runtime, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null");
					}
				}
				if (mSmoothMovesBoneAnimationDataType == null) {
					Debug.LogError("Unable to query SmoothMoves.BoneAnimationData type - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
					return null;
				}
				}
			
			UnityEngine.Object loadedBoneAnimationDataObject = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, mSmoothMovesBoneAnimationDataType);
			if (loadedBoneAnimationDataObject == null) {
				Debug.LogError("Unable to query SmoothMoves.BoneAnimationData type - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return null;
			}
			else {
				return loadedBoneAnimationDataObject;
			}
		}
	}
	
	//-------------------------------------------------------------------------
	bool GetSmoothMovesAtlasPivotOffset(Texture2D atlasTexture, Vector2 frameSize, Vector2 uvBottomLeft, Vector2 uvTopRight, out int frameIndex, out Vector3 pivotOffset) {
		string texturePath = UnityEditor.AssetDatabase.GetAssetPath(atlasTexture);
		
		string atlasDescriptionPath = System.IO.Path.GetDirectoryName(texturePath) + "/" + System.IO.Path.GetFileNameWithoutExtension(texturePath) + ".asset";
		if (!System.IO.File.Exists(atlasDescriptionPath)) {
			pivotOffset = Vector3.zero;
			frameIndex = 0;
			return false;
		}
		
		mSmoothMovesAtlasType = Type.GetType("SmoothMoves.TextureAtlas, " + mFullSmoothMovesAssemblyName);
		if (mSmoothMovesAtlasType == null) {
			mSmoothMovesAtlasType = Type.GetType("SmoothMoves.TextureAtlas, SmoothMoves_Runtime, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null");
			if (mSmoothMovesAtlasType == null) {
				mSmoothMovesAtlasType = Type.GetType("SmoothMoves.TextureAtlas, SmoothMoves_Runtime, Version=1.9.7.0, Culture=neutral, PublicKeyToken=null");
			}
		}
		
		UnityEngine.Object loadedAtlasObject = UnityEditor.AssetDatabase.LoadAssetAtPath(atlasDescriptionPath, mSmoothMovesAtlasType);
		if (loadedAtlasObject == null) {
			pivotOffset = Vector3.zero;
			frameIndex = 0;
			return false;
		}
		
		FieldInfo fieldUVs = mSmoothMovesAtlasType.GetField("uvs");
		if (fieldUVs == null) {
			Debug.LogError("Detected a missing 'mUVs' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			frameIndex = 0;
			pivotOffset = Vector3.zero;
			return false;
		}
		IEnumerable uvsList = (IEnumerable) fieldUVs.GetValue(loadedAtlasObject);
		frameIndex = 0;
		int index = 0;
		foreach (Rect uvRect in uvsList) {
			if (Mathf.Approximately(uvRect.xMin, uvBottomLeft.x) &&
				Mathf.Approximately(uvRect.yMin, uvBottomLeft.y) &&
				Mathf.Approximately(uvRect.xMax, uvTopRight.x) &&
				Mathf.Approximately(uvRect.yMax, uvTopRight.y)) {
				
				frameIndex = index;
				break;
			}
			++index;
		}
		
		FieldInfo fieldPivotOffsetsArray = mSmoothMovesAtlasType.GetField("defaultPivotOffsets");
		if (fieldPivotOffsetsArray == null) {
			Debug.LogError("Detected a missing 'defaultPivotOffsets' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			pivotOffset = Vector3.zero;
			return false;
		}
		IEnumerable pivotOffsetsArray = (IEnumerable) fieldPivotOffsetsArray.GetValue(loadedAtlasObject);
		index = 0;
		Vector2 normalizedOffset = Vector2.zero;
		foreach (Vector2 offset in pivotOffsetsArray) {
			if (index == frameIndex) {
				normalizedOffset = offset;
				break;
			}
			++index;
		}
		
		pivotOffset = new Vector3(-normalizedOffset.x * frameSize.x, -normalizedOffset.y * frameSize.y, 0);
		return true;
	}

	//-------------------------------------------------------------------------
	bool ReadOTSpriteAtlasParams(System.Object otSpriteContainer, int frameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation) {
		FieldInfo fieldAtlasData = otSpriteContainer.GetType().GetField("atlasData");
		if (fieldAtlasData == null) {
			Debug.LogWarning("Failed to access 'atlasData' member of the sprite atlas. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			framePositionInPixels = frameSizeInPixels = Vector2.zero;
			frameRotation = 0.0f;
		}
		Array atlasDataArray = (Array) fieldAtlasData.GetValue(otSpriteContainer);
		if (atlasDataArray == null) { // unlikely
			Debug.LogWarning("Failed to access 'atlasData' member of the sprite atlas as an array. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			framePositionInPixels = frameSizeInPixels = Vector2.zero;
			frameRotation = 0.0f;
			return false;
		}
   		System.Object atlasFrameData = atlasDataArray.GetValue(frameIndex);
		return GetOTAtlasDataFrameDimensions(atlasFrameData, out framePositionInPixels, out frameSizeInPixels, out frameRotation);
	}
	
	//-------------------------------------------------------------------------
	bool GetOTAtlasDataFrameDimensions(System.Object frameOTAtlasData, out Vector2 positionInPixels, out Vector2 sizeInPixels, out float rotation) {
		Type otAtlasDataType = frameOTAtlasData.GetType();
		
		FieldInfo fieldPosition = otAtlasDataType.GetField("position");
		FieldInfo fieldSize = otAtlasDataType.GetField("size");
		FieldInfo fieldRotated = otAtlasDataType.GetField("rotated");
		if (fieldPosition == null || fieldSize == null || fieldRotated == null) {
			Debug.LogWarning("Failed to read 'position' or 'size' or 'rotated' member(s) of OTSprite's sprite atlas frame. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			positionInPixels = sizeInPixels = Vector2.zero;
			rotation = 0.0f;
			return false;
		}
		positionInPixels = (Vector2) fieldPosition.GetValue(frameOTAtlasData);
		bool isRotated90DegreesCW = (bool) fieldRotated.GetValue(frameOTAtlasData);
		rotation = isRotated90DegreesCW ? 270.0f : 0.0f;
		sizeInPixels = (Vector2) fieldSize.GetValue(frameOTAtlasData);
		if (rotation != 0.0f && rotation != 180.0f) {
			sizeInPixels = new Vector2(sizeInPixels.y, sizeInPixels.x); // swap x and y.
		}
		return true;
	}

	//-------------------------------------------------------------------------
	bool ReadOTSpriteSheetParams(System.Object otSpriteContainer, int frameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation) {
		Type containerType = otSpriteContainer.GetType();
		FieldInfo fieldFramesXY = containerType.GetField("_framesXY");
		FieldInfo fieldFrameSize = containerType.GetField("_frameSize");
		if (fieldFramesXY == null || fieldFrameSize == null) {
			Debug.LogWarning("Failed to read '_framesXY' or '_frameSize' member(s) of OTSprite's sprite sheet. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			framePositionInPixels = frameSizeInPixels = Vector2.zero;
			frameRotation = 0.0f;
			return false;
		}
		Vector2 framesXY = (Vector2) fieldFramesXY.GetValue(otSpriteContainer);
		Vector2 frameSize = (Vector2) fieldFrameSize.GetValue(otSpriteContainer);
		int framesPerRow = (int) framesXY.x;
		
		int xIndex = frameIndex % framesPerRow;
		int yIndex = frameIndex / framesPerRow;
		
		framePositionInPixels = new Vector2(xIndex * frameSize.x, yIndex * frameSize.y);
		frameSizeInPixels = frameSize;
		frameRotation = 0.0f; // never has any rotation
		return true;
	}
	
	//-------------------------------------------------------------------------
	void GenerateAndStoreColliderMesh() {
		GenerateUnreducedColliderMesh();
		ReduceAndStoreColliderMesh();
	}
	
	//-------------------------------------------------------------------------
	bool GenerateUnreducedColliderMesh() {
		// just in case the texture has changed.
		InitTextureParams();
		
		if (UsedTexture == null) {
			return false;
		}
		
		UpdateColliderMeshFilename();
		
		if (mOutlineAlgorithm == null) {
			mOutlineAlgorithm = new PolygonOutlineFromImageFrontend();
		}
		bool wasSuccessful = mOutlineAlgorithm.BinaryAlphaThresholdImageFromTexture(out mBinaryImage, UsedTexture, mAlphaOpaqueThreshold,
															   mIsAtlasUsed,
															   (int) mAtlasFramePositionInPixels.x, (int) mAtlasFramePositionInPixels.y,
															   (int) mAtlasFrameSizeInPixels.x, (int) mAtlasFrameSizeInPixels.y);
		if (!wasSuccessful) {
			Debug.LogError(mOutlineAlgorithm.LastError);
			return false;
		}
		
		IntVector2[] islandStartingPoints = CalculateIslandStartingPoints(mBinaryImage);
		
		// Calculate polygon bounds
		mOutlineAlgorithm.Backend.mVertexReductionDistanceTolerance = this.mVertexReductionDistanceTolerance;
		mOutlineAlgorithm.Backend.mMaxPointCount = this.mMaxPointCount;
		mOutlineAlgorithm.Backend.mConvex = this.mConvex;
		mOutlineAlgorithm.Backend.mXOffsetNormalized = -0.5f;//-this.transform.localScale.x / 2.0f;
		mOutlineAlgorithm.Backend.mYOffsetNormalized = -0.5f;//-this.transform.localScale.y / 2.0f;
		mOutlineAlgorithm.Backend.mThickness = this.mThickness;
		bool outputInNormalizedSpace = true;
		
		mOutlineAlgorithm.Backend.UnreducedOutlineFromBinaryImage(out mIntermediateOutlineVertices, mBinaryImage, islandStartingPoints, outputInNormalizedSpace, false);
		return true;
	}
	
	//-------------------------------------------------------------------------
	bool ScaleRequiresReverseVertexOrder() {
		float scaleX = GetOutputScaleX();
		float scaleY = GetOutputScaleY();
		if ((scaleX * scaleY) > 0.0f) { // scaleX is negative when normal.
			return true;
		}
		else {
			return false;
		}
	}
	
	//-------------------------------------------------------------------------
	IntVector2[] CalculateIslandStartingPoints(bool [,] binaryImage) {
		IntVector2[] islandStartingPoints = null;
		int[,] islandClassificationImage = null;
		IslandDetector.Region[] islands = null;
		IslandDetector.Region[] seaRegions = null;
		
		mIslandDetector = new IslandDetector();
		mIslandDetector.DetectIslandsFromBinaryImage(binaryImage, out islandClassificationImage, out islands, out seaRegions);
		if (islands != null && islands.Length > 0 && islands[0] != null) {
			islandStartingPoints = new IntVector2[1];
			islandStartingPoints[0] = islands[0].mPointAtBorder;
		}
		return islandStartingPoints;
	}
	
	//-------------------------------------------------------------------------
	bool ReduceAndStoreColliderMesh() {
		
		if (mIntermediateOutlineVertices == null ||
			mOutlineAlgorithm == null) {
			if (!GenerateUnreducedColliderMesh())
				return false;
		}
		
		Vector3[] vertices;
		int[] triangleIndices;
		
		mOutlineAlgorithm.Backend.mVertexReductionDistanceTolerance = this.mVertexReductionDistanceTolerance;
		mOutlineAlgorithm.Backend.mMaxPointCount = this.mMaxPointCount;
		mOutlineAlgorithm.Backend.mConvex = this.mConvex;
		mOutlineAlgorithm.Backend.mXOffsetNormalized = -0.5f;//-this.transform.localScale.x / 2.0f;
		mOutlineAlgorithm.Backend.mYOffsetNormalized = -0.5f;//-this.transform.localScale.y / 2.0f;
		mOutlineAlgorithm.Backend.mThickness = this.mThickness;
		
		List<Vector2> reducedOutline = mOutlineAlgorithm.Backend.ReduceOutline(mIntermediateOutlineVertices);
		
		bool reverseVertexOrder = !mFlipInsideOutside;
		if (ScaleRequiresReverseVertexOrder()) {
			reverseVertexOrder = !reverseVertexOrder; // scaled -1 -> flip inside out, the vertex order changes when mirrored.
		}
		
		mOutlineAlgorithm.Backend.TriangleFenceFromOutline(out vertices, out triangleIndices, reducedOutline, reverseVertexOrder);
		
		mResultVertices = vertices;
		mResultTriangleIndices = triangleIndices;
		
		return ExportMeshToFile();
	}
	
	//-------------------------------------------------------------------------
	bool ExportMeshToFile() {
		if (mResultVertices == null || mResultTriangleIndices == null) {
			if (!ReduceAndStoreColliderMesh())
				return false;
		}
		
		ColladaExporter colladaWriter = new ColladaExporter();
		ColladaExporter.GeometryNode rootGeometryNode = new ColladaExporter.GeometryNode();
		rootGeometryNode.mName = "Collider";
		rootGeometryNode.mAreVerticesLeftHanded = true;
		rootGeometryNode.mVertices = mResultVertices;
		rootGeometryNode.mTriangleIndices = mResultTriangleIndices;
		rootGeometryNode.mGenerateNormals = true;
		float scaleX = GetOutputScaleX();
		float scaleY = GetOutputScaleY();
		
		colladaWriter.mVertexScaleAfterInitialRotation.x = scaleX; // the mesh is imported in a way that we end up correct this way.
		colladaWriter.mVertexScaleAfterInitialRotation.y = scaleY;
		colladaWriter.mVertexScaleAfterSecondRotation = Vector3.one;
		
		float atlasFrameRotation = mAtlasFrameRotation;
		if (mCustomTex != null) {
			colladaWriter.mVertexScaleAfterInitialRotation.Scale(GetCustomImageScale());
			atlasFrameRotation = 0.0f;
		}
		
		// In order to rotate well, we need to compensate for the gameobject's
		// transform.scale that is applied automatically after all of our transforms.
		Vector3 automaticallyAppliedScale = this.transform.localScale;
		Vector3 rotationCompensationScaleBefore = new Vector3(automaticallyAppliedScale.x, automaticallyAppliedScale.y, 1.0f);
		Vector3 rotationCompensationScaleAfter = new Vector3(1.0f / automaticallyAppliedScale.x, 1.0f / automaticallyAppliedScale.y, 1.0f);
		colladaWriter.mVertexScaleAfterInitialRotation.Scale(rotationCompensationScaleBefore);
		colladaWriter.mVertexScaleAfterSecondRotation.Scale(rotationCompensationScaleAfter);
		
		colladaWriter.mVertexOffset.x = -mOutlineOffset.x -mCustomOffset.x;
		colladaWriter.mVertexOffset.y = mOutlineOffset.y + mCustomOffset.y;
		colladaWriter.mVertexOffset.z = -mOutlineOffset.z -mCustomOffset.z;
		colladaWriter.mVertexTransformationCenter = new Vector3(0, 0, 0);
		colladaWriter.mVertexInitialRotationQuaternion = Quaternion.Euler(0, 0, -atlasFrameRotation);
		colladaWriter.mVertexSecondRotationQuaternion = Quaternion.Euler(0, 0,  -mCustomRotation);
		
		System.IO.Directory.CreateDirectory(mColliderMeshDirectory);
		colladaWriter.ExportTriangleMeshToFile(FullColliderMeshPath(), rootGeometryNode);
		return true;
	}
	
	//-------------------------------------------------------------------------
	float GetOutputScaleX() {
		float scaleX = mFlipHorizontal ? 1.0f : -1.0f;
		scaleX *= mOutlineScale.x * mCustomScale.x;
		return scaleX;
	}
	
	//-------------------------------------------------------------------------
	float GetOutputScaleY() {
		float scaleY = mFlipVertical ? -1.0f : 1.0f;
		scaleY *= mOutlineScale.y * mCustomScale.y;
		return scaleY;
	}
	
	//-------------------------------------------------------------------------
	/// <returns>
	/// A scale vector to compensate for the game-object's transform.scale
	/// value in case of a custom image.
	/// </returns>
	Vector3 GetCustomImageScale() {
		
		float baseImageWidth = mInactiveBaseImageWidth;
		float baseImageHeight = mInactiveBaseImageHeight;
		float customImageWidth = mCustomTex.width;
		float customImageHeight = mCustomTex.height;
		
		if (mHasOTSpriteComponent) {
			return new Vector3(customImageWidth / baseImageWidth, customImageHeight / baseImageHeight, 1.0f);
		}
		else if (mHasSmoothMovesSpriteComponent) {
			return new Vector3(customImageWidth / baseImageWidth * mInactiveBaseImageOutlineScale.x, customImageHeight / baseImageHeight * mInactiveBaseImageOutlineScale.y, 1.0f);
		}
		else if (mHasSmoothMovesAnimBoneColliderComponent) {
			return new Vector3(customImageWidth / baseImageWidth * mInactiveBaseImageOutlineScale.x, customImageHeight / baseImageHeight * mInactiveBaseImageOutlineScale.y, 1.0f);
		}
		else {
			// nothing at all
			return new Vector3(customImageWidth, customImageHeight, 1.0f);
		}
	}
	
	//-------------------------------------------------------------------------
	/// <returns>
	/// The name of the file to be generated.
	/// Follows the form:
	/// "TextureName[_AtlasIndex][_c]_PathHash[_FlipSuffix][groupSuffix].dae"
	/// or    "Atlas_[FrameTitle][_c]_PathHash[_FlipSuffix][groupSuffix].dae".
	/// E.g.: "Island2_FBAAACD3_flipped_h.dae" or "TexAtlas_12_FBA64CD3.dae"
	/// PathHash is added to the name to prevent name-collisions that could
	/// occur if the texture's name was used without considering its full path,
	/// such as "dir1/main.png" colliding with "dir2/main.png".
	/// </returns>
	string GetColliderMeshFilename() {
		if (this.renderer && this.renderer.sharedMaterial) {
			mMainTex = (Texture2D) this.renderer.sharedMaterial.mainTexture;
		}
		
		if (UsedTexture == null) {
			return "";
		}
		
		string nameString = "";
		if (!mIsAtlasUsed) {
			nameString = UsedTexture.name;
		}
		else {
			if (!string.IsNullOrEmpty(mAtlasFrameTitle)) {
				nameString = "Atlas_" + mAtlasFrameIndex.ToString() + "_" + mAtlasFrameTitle;
			}
			else {
				nameString = UsedTexture.name + "_" + mAtlasFrameIndex.ToString();
			}
		}
		
		string customString = "";
		if (mCustomTex != null) {
			customString = "_c";
		}
		
		string flipSuffix = "";
		if (mFlipHorizontal || mFlipVertical) {
			flipSuffix += "_flipped_";
			if (mFlipHorizontal) {
				flipSuffix += "h";
			}
			if (mFlipVertical) {
				flipSuffix += "v";
			}
		}
		string uniqueHashID = "_" + GetHashStringForTexturePath(UsedTexture);
		string name = nameString + customString + uniqueHashID + flipSuffix + mGroupSuffix + ".dae";
		return name;
	}
	
	//-------------------------------------------------------------------------
	public static int GetHashForTexturePath(Texture2D texture) {
		string texturePath = UnityEditor.AssetDatabase.GetAssetPath(texture);
		return texturePath.GetHashCode();
	}
	
	//-------------------------------------------------------------------------
	public static string GetHashStringForTexturePath(Texture2D texture) {
		int hash = GetHashForTexturePath(texture);
		return hash.ToString("X8");
	}
	
	//-------------------------------------------------------------------------
	static void LogAttributesOfObject(object target, int childLevels) {
		LogAttributesOfObject(target, childLevels, "");
	}
	
	//-------------------------------------------------------------------------
	static void LogAttributesOfObject(object target, int childLevels, string indent) {
		
		string childIndent = indent + new string(' ', 4);
		
		Type targetType = target.GetType();
		PropertyInfo[] properties = targetType.GetProperties();
		foreach (PropertyInfo propertyInfo in properties) {
			Debug.Log(indent + "prop found: " + propertyInfo.Name);
		}
		FieldInfo[] fields = targetType.GetFields();
		foreach (FieldInfo fieldInfo in fields) {
			Debug.Log(indent + "field found: " + fieldInfo.Name + "=" + fieldInfo.GetValue(target).ToString());
			Debug.Log(indent + "{ child begin------");
			if (childLevels > 0) {
				object obj = fieldInfo.GetValue(target);
				LogAttributesOfObject(obj, childLevels -1, childIndent);
			}
			Debug.Log(indent + "} child end ------");
		}
		MethodInfo[] methods = targetType.GetMethods();
		foreach (MethodInfo methodInfo in methods) {
			Debug.Log(indent + "method found: " + methodInfo.ToString());
		}
	}
}

#endif // #if UNITY_EDITOR