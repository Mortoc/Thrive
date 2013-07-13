using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

//-------------------------------------------------------------------------
/// <summary>
/// Editor window for the 2D Toolkit (TK2D) specific version of the
/// AlphaMeshCollider.
/// 
/// A ColliderGenTK2DParameterStore object is used to store parameter values
/// across selection changes and closing and reopening the SpriteCollection
/// editor window.
/// The store is used as follows:
/// - after RecalculateSelectedColliders() was called -> save values to store (not in prefab yet).
/// - if collection == oldCollection && spriteIDs != oldSpriteIDs -> load from store (not from prefab)
/// - if collection != oldCollection -> load store from prefab
/// - if "Commit" was hit -> persist store to prefab
/// </summary>
public class EditorScriptAlphaMeshColliderTK2DWindow : EditorWindow {
	
	const int COLLIDER_EDIT_MODE_INT_VALUE = 3; // Note: keep up to date with tk2dEditor.SpriteCollectionEditor.TextureEditor.Mode enum.	
	
	protected bool mWasInitialized = false;
	
	GUIContent mColliderPointCountLabel = new GUIContent("Outline Vertex Count");
	GUIContent mEditorLiveUpdateLabel = new GUIContent("Editor Live Update");
	GUIContent mAlphaOpaqueThresholdLabel = new GUIContent("Alpha Opaque Threshold");
	GUIContent mConvexLabel = new GUIContent("Force Convex");
	GUIContent mFlipInsideOutsideLabel = new GUIContent("Flip Normals");
	GUIContent mAdvancedSettingsLabel =  new GUIContent("Advanced Settings");
	GUIContent mCustomImageLabel = new GUIContent("Custom Image");
	GUIContent mCalculateOutlineVerticesLabel = new GUIContent("Update Collider");
	
	
	protected int mColliderPointCount = 20;
	protected bool mConvex = false;
	protected bool mFlipInsideOutside = false;
	protected bool mLiveUpdate = true;
	protected float mNormalizedAlphaOpaqueThreshold = 0.1f;
	protected bool mShowAdvanced = false;
	protected Texture2D mCustomTex = null;
	protected float mCustomRotation = 0.0f;
	protected Vector2 mCustomScale = Vector2.one;
	protected Vector2 mCustomOffset = Vector2.zero;
	protected int mPointCountSliderMax = 100;
	
	protected int mOldColliderPointCount;
	protected bool mOldFlipInsideOutside;
	protected bool mOldConvex;
	protected float mOldDistanceTolerance;
	protected float mOldNormalizedAlphaOpaqueThreshold;
	protected Vector2 mOldCustomScale;
	protected Vector2 mOldCustomOffset;
	protected Texture2D mOldCustomTex;
	
	GenerateColliderTK2DHelper mAlgorithmHelper = new GenerateColliderTK2DHelper();
	ColliderGenTK2DParameterStore mParameterStore = null;
	bool mSaveParameterStoreToPrefabAtNextUpdate = false;
	SortedList<int, Component> mDifferentSprites = null;
	
	object mSpriteCollectionProxyToEdit = null;
	object mOldSpriteCollectionProxyToEdit = null;
	int[] mSpriteIDsToEdit = null;
	int[] mOldSpriteIDsToEdit = null;
	uint mRepaintCounter = 0;
	
	static string mAtlasPathToMonitorForCommit = "";
	//-------------------------------------------------------------------------
	public static string AtlasPathToMonitorForCommit {
		get {
			return mAtlasPathToMonitorForCommit;
		}
	}
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/2D Toolkit Specific/Show ColliderGen TK2D Window", true)]
	static bool ValidateGenerateColliderVerticesMenuEntry() {
		// no special selection criteria needed.
		return true;
    }
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/2D Toolkit Specific/Show ColliderGen TK2D Window", false, 101)]
	static void GenerateColliderVerticesMenuEntry() {
		
		// Get existing open window or if none, make a new one:
		EditorScriptAlphaMeshColliderTK2DWindow window = EditorWindow.GetWindow<EditorScriptAlphaMeshColliderTK2DWindow>();
		window.title = "ColliderGen TK2D";
    }
	
	//-------------------------------------------------------------------------
	void OnGUI() {

		AssetPostprocessorDetectTK2DCommit.TargetColliderGenTK2DWindow = this;
		
		CheckForSelectedSpriteCollectionAndSprites();
		CheckForValuesToUpdate();
		
		EditorGUIUtility.LookLikeControls(150.0f);
		
		mOldColliderPointCount = mColliderPointCount;
		mOldFlipInsideOutside = mFlipInsideOutside;
		mOldConvex = mConvex;
		mOldNormalizedAlphaOpaqueThreshold = mNormalizedAlphaOpaqueThreshold;
		mOldCustomScale = mCustomScale;
		mOldCustomOffset = mCustomOffset;
		mOldCustomTex = mCustomTex;
		
		mLiveUpdate = EditorGUILayout.Toggle(mEditorLiveUpdateLabel, mLiveUpdate);
		
		// int [3..100] max point count
		mColliderPointCount = EditorGUILayout.IntSlider(mColliderPointCountLabel, mColliderPointCount, 3, mPointCountSliderMax);
		
		// Note: Removed since it was not intuitive enough to use.
		// float [0..max(width, height)] Accepted Distance
		//float imageMinExtent = Mathf.Min(targetObject.mTextureWidth, targetObject.mTextureHeight);
		//targetObject.mVertexReductionDistanceTolerance = EditorGUILayout.Slider("Accepted Distance", targetObject.mVertexReductionDistanceTolerance, 0.0f, imageMinExtent/2);
		
		// float [0..1] Alpha Opaque Threshold
		mNormalizedAlphaOpaqueThreshold = EditorGUILayout.Slider(mAlphaOpaqueThresholdLabel, mNormalizedAlphaOpaqueThreshold, 0.0f, 1.0f);
		
		mConvex = EditorGUILayout.Toggle(mConvexLabel, mConvex);
		
		mFlipInsideOutside = EditorGUILayout.Toggle(mFlipInsideOutsideLabel, mFlipInsideOutside);
		
		// Advanced settings
		mShowAdvanced = EditorGUILayout.Foldout(mShowAdvanced, mAdvancedSettingsLabel);
        if(mShowAdvanced) {
			EditorGUI.indentLevel++;
			
			mCustomTex = (Texture2D) EditorGUILayout.ObjectField(mCustomImageLabel, mCustomTex, typeof(Texture2D), false);
			
			mCustomScale = EditorGUILayout.Vector2Field("Custom Scale", mCustomScale);
			mCustomOffset = EditorGUILayout.Vector2Field("Custom Offset", mCustomOffset);
			
			EditorGUI.indentLevel--;
		}
		
		mAlgorithmHelper.mNormalizedAlphaOpaqueThreshold = this.mNormalizedAlphaOpaqueThreshold;
		mAlgorithmHelper.mMaxPointCount = this.mColliderPointCount;
		mAlgorithmHelper.mConvex = this.mConvex;
		mAlgorithmHelper.mFlipInsideOutside = this.mFlipInsideOutside;
		mAlgorithmHelper.mCustomTex = this.mCustomTex;
		mAlgorithmHelper.mCustomScale = this.mCustomScale;
		mAlgorithmHelper.mCustomOffset = this.mCustomOffset;
		
		if(GUILayout.Button(mCalculateOutlineVerticesLabel)) {

            RecalculateSelectedColliders();
		}
		
		if (mLiveUpdate) {
			bool pointCountNeedsUpdate = ((mOldColliderPointCount != mColliderPointCount) && (mColliderPointCount > 2)); // when typing 28, it would otherwise update at the first digit '2'.
			if (pointCountNeedsUpdate ||
				mOldFlipInsideOutside != mFlipInsideOutside ||
				mOldConvex != mConvex ||
				mOldNormalizedAlphaOpaqueThreshold != mNormalizedAlphaOpaqueThreshold ||
				mOldCustomTex != mCustomTex ||
				mOldCustomScale != mCustomScale ||
				mOldCustomOffset != mCustomOffset) {

                RecalculateSelectedColliders();
			}
		}
	}
	
	//-------------------------------------------------------------------------
	void OnDestroy() {
		AssetPostprocessorDetectTK2DCommit.TargetColliderGenTK2DWindow = null;
	}
	
	//-------------------------------------------------------------------------
	public void OnSpriteCollectionCommit() {
		mSaveParameterStoreToPrefabAtNextUpdate = true;
	}
	
	//-------------------------------------------------------------------------
	/// <summary>
	/// We need to periodically update the window since we cannot create hook
	/// to repaint if the selected sprites of the SpriteCollection editor
	/// window have changed.
	/// </summary>
	void Update() {
	    if (!EditorApplication.isPlaying) {
			if ((mRepaintCounter % 50) == 0) {
				Repaint();
			}
			++mRepaintCounter;
		}
	}
	
	//-------------------------------------------------------------------------
	protected void CheckForSelectedSpriteCollectionAndSprites() {
		
		mOldSpriteCollectionProxyToEdit = mSpriteCollectionProxyToEdit;
		mOldSpriteIDsToEdit = mSpriteIDsToEdit;
		
		mSpriteCollectionProxyToEdit = GetSelectedSpriteCollectionProxy();
		mSpriteIDsToEdit = GetSelectedSpriteEntries();
	}
	
	//-------------------------------------------------------------------------
	protected void CheckForValuesToUpdate() {
		if (!mWasInitialized)
			InitWithPreferencesValues();
		
		if (mSaveParameterStoreToPrefabAtNextUpdate) {
			SaveParameterStoreToPrefab();
			mSaveParameterStoreToPrefabAtNextUpdate = false;
		}
		
		// The following actions are performed:
		// - storeNeedsReload = collection != oldCollection -> load store from prefab
		// - loadDifferentSpriteOfSameCollection = collection == oldCollection && spriteIDs != oldSpriteIDs -> load params from store (not from prefab)
		
		
		if (mSpriteCollectionProxyToEdit != null && mSpriteIDsToEdit != null) {
			
			// SpriteCollection editor window is already open, sprites are selected.
			
			bool activeSpriteCollectionChanged = (mSpriteCollectionProxyToEdit != mOldSpriteCollectionProxyToEdit);
			bool storeNeedsReload = (mParameterStore == null || activeSpriteCollectionChanged);
			if (storeNeedsReload) {
				mParameterStore = CreateOrLoadParameterStore();
				if (mParameterStore != null) {
					bool parametersFound = LoadValuesFromParameterStore(mParameterStore, mSpriteIDsToEdit);
					if (!parametersFound) {
						InitWithPreferencesValues();
					}
				}
				mAtlasPathToMonitorForCommit = GetAtlasPathForActiveSpriteCollection();
			}
			else {
				bool spriteSelectionChanged = !AreArraysEqual(mSpriteIDsToEdit, mOldSpriteIDsToEdit);
				bool loadDifferentSpriteOfSameCollection = (spriteSelectionChanged && !activeSpriteCollectionChanged);
				if (loadDifferentSpriteOfSameCollection) {
					bool parametersFound = LoadValuesFromParameterStore(mParameterStore, mSpriteIDsToEdit);
					if (!parametersFound) {
						InitWithPreferencesValues();
					}
				}
			}
		}
	}
	
	//-------------------------------------------------------------------------
	protected void InitWithPreferencesValues() {
		
		this.mLiveUpdate = AlphaMeshColliderPreferences.Instance.DefaultLiveUpdate;
		this.mColliderPointCount = AlphaMeshColliderPreferences.Instance.DefaultColliderPointCount;
		this.mConvex = AlphaMeshColliderPreferences.Instance.DefaultConvex;
		this.mPointCountSliderMax = AlphaMeshColliderPreferences.Instance.ColliderPointCountSliderMaxValue;
		
		
		this.mNormalizedAlphaOpaqueThreshold = 0.1f;
		this.mFlipInsideOutside = false;
		this.mCustomTex = null;
		this.mCustomScale = Vector2.one;
		this.mCustomOffset = Vector2.zero;
		
		mWasInitialized = true;
	}
	
	//-------------------------------------------------------------------------
	protected string GetAtlasPathForActiveSpriteCollection() {
		object spriteCollection = GetSelectedSpriteCollection();
		string prefabPath = ColliderGenTK2DParameterStore.GetSpriteCollectionPrefabFilePath(spriteCollection);
		string directory = System.IO.Path.GetDirectoryName(prefabPath);
		string atlasPath = directory + "/atlas0.png";
		return atlasPath;
	}
	
	//-------------------------------------------------------------------------
	protected object GetSelectedSpriteCollection() {
		Type spriteCollectionEditorType = Type.GetType("tk2dSpriteCollectionEditorPopup");
		if (spriteCollectionEditorType == null) {
			return null;
		}
		EditorWindow window = EditorWindow.GetWindow(spriteCollectionEditorType, false, "Sprite Collection Editor", false);
		if (window == null) {
			return null;
		}
		
		object spriteCollection = GetSpriteCollection(window);
		return spriteCollection;
	}
	
	//-------------------------------------------------------------------------
	protected object GetSelectedSpriteCollectionProxy() {
		Type spriteCollectionEditorType = Type.GetType("tk2dSpriteCollectionEditorPopup");
		if (spriteCollectionEditorType == null) {
			return null;
		}
		
		EditorWindow window = EditorWindow.GetWindow(spriteCollectionEditorType, false, "Sprite Collection Editor", false);
		if (window == null) {
			return null;
		}
		
		object spriteCollectionProxy = GetSpriteCollectionProxy(window);
		return spriteCollectionProxy;
	}
	
	//-------------------------------------------------------------------------
	protected void SaveParameterStoreToPrefab() {
		object spriteCollection = GetSelectedSpriteCollection();
		ColliderGenTK2DParameterStore.SaveParameterStoreToPrefab(mParameterStore, spriteCollection);
	}
	
	//-------------------------------------------------------------------------
	protected ColliderGenTK2DParameterStore CreateOrLoadParameterStore() {
		
		object spriteCollection = GetSelectedSpriteCollection();
		ColliderGenTK2DParameterStore result = ColliderGenTK2DParameterStore.EnsureParameterStorePrefabExistsForCollection(spriteCollection);
		return result;
	}
	
	//-------------------------------------------------------------------------
	protected bool LoadValuesFromParameterStore(ColliderGenTK2DParameterStore parameterStore, int[] spriteIDs) {
		if (spriteIDs.Length == 0 || parameterStore == null)
			return false;
		
		ColliderGenTK2DParametersForSprite parameters = parameterStore.GetParametersForSprite(spriteIDs[0]);
		if (parameters == null) {
			return false;
		}
		
		mColliderPointCount = parameters.mOutlineVertexCount;
		mFlipInsideOutside = parameters.mFlipNormals;
		mConvex = parameters.mForceConvex;
		mNormalizedAlphaOpaqueThreshold = parameters.mAlphaOpaqueThreshold;
		mCustomScale = parameters.mCustomScale;
		mCustomOffset = parameters.mCustomOffset;
		mCustomTex = parameters.mCustomTexture;
		return true;
	}
	
	//-------------------------------------------------------------------------
	protected void SaveValuesToParameterStore(ColliderGenTK2DParameterStore parameterStore, int[] spriteIDs) {
		if (spriteIDs.Length == 0 || parameterStore == null)
			return;
		
		foreach (int spriteID in spriteIDs) {
			ColliderGenTK2DParametersForSprite parameters = new ColliderGenTK2DParametersForSprite();
		
			parameters.mOutlineVertexCount = mColliderPointCount;
			parameters.mFlipNormals = mFlipInsideOutside;
			parameters.mForceConvex = mConvex;
			parameters.mAlphaOpaqueThreshold = mNormalizedAlphaOpaqueThreshold;
			parameters.mCustomScale = mCustomScale;
			parameters.mCustomOffset = mCustomOffset;
			parameters.mCustomTexture = mCustomTex;
			
			parameters.mSpriteIndex = spriteID;
			
			parameterStore.SaveParametersForSprite(spriteID, parameters);
		}
	}	
	
	//-------------------------------------------------------------------------
	void RecalculateSelectedColliders() {
		
		Type spriteCollectionEditorType = Type.GetType("tk2dSpriteCollectionEditorPopup");
		if (spriteCollectionEditorType == null) {
			return;
		}
		EditorWindow window = EditorWindow.GetWindow(spriteCollectionEditorType, false, "Sprite Collection Editor", false);
		
		object spriteCollection = GetSpriteCollection(window);
		object spriteCollectionProxy = GetSpriteCollectionProxy(window);
		mSpriteIDsToEdit = GetSelectedSpriteEntries(window);
		
		bool isSelectionInWindowOK = (mSpriteIDsToEdit != null && mSpriteIDsToEdit.Length > 0) && (spriteCollection != null);
		if (!isSelectionInWindowOK) {
			RecalculateCollidersOfSceneSelection(window);
		}
		else {
			RecalculateCollidersOf2DToolkitWindowSelection(window, spriteCollection, spriteCollectionProxy);
		}

        window.Repaint();
		SaveValuesToParameterStore(mParameterStore, mSpriteIDsToEdit);
	}

	//-------------------------------------------------------------------------
	void RecalculateCollidersOfSceneSelection(EditorWindow window) {
		
		mDifferentSprites = GetSpriteIDsAndContainerIDFromSelectedSprites();
		mSpriteIDsToEdit = new int[mDifferentSprites.Count];
		mDifferentSprites.Keys.CopyTo(mSpriteIDsToEdit, 0);
		
		LoadCollectionInSpriteCollectionEditorWindow(window, mDifferentSprites);
		SelectSpritesInSpriteCollectionEditor(window, mSpriteIDsToEdit);
		object spriteCollection = GetSpriteCollection(window);
		object spriteCollectionProxy = GetSpriteCollectionProxy(window);
		
		RecalculateCollidersOf2DToolkitWindowSelection(window, spriteCollection, spriteCollectionProxy);
	}
	
	//-------------------------------------------------------------------------
	void RecalculateCollidersOf2DToolkitWindowSelection(EditorWindow window, object spriteCollection, object spriteCollectionProxy) {
		EnsureColliderTypePolyCollider(spriteCollectionProxy, mSpriteIDsToEdit);
		GenerateColliderVertices(spriteCollectionProxy, mSpriteIDsToEdit);
		
		float editorScale = 1.0f;
		PropertyInfo propertyEditorDisplayScale = null;
		object textureEditor = null;
		bool scaleReadSuccessfully = GetEditorDisplayScale(out editorScale, out propertyEditorDisplayScale, out textureEditor, window);
		
		SelectSpritesInSpriteCollectionEditor(window, mSpriteIDsToEdit);
		SetViewModeToColliderMode(window);
		
		if (scaleReadSuccessfully) {
			propertyEditorDisplayScale.SetValue(textureEditor, editorScale, null);
		}
	}
	
	//-------------------------------------------------------------------------
	object GetSpriteCollection(EditorWindow window) {
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldSpriteCollection = spriteCollectionEditorType.GetField("_spriteCollection", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSpriteCollection == null) {
			Debug.LogError("Detected a missing '_spriteCollection' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		return fieldSpriteCollection.GetValue(window);
	}
	
	//-------------------------------------------------------------------------
	object GetSpriteCollectionProxy(EditorWindow window) {
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldSpriteCollectionProxy = spriteCollectionEditorType.GetField("spriteCollectionProxy", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSpriteCollectionProxy == null) {
			Debug.LogError("Detected a missing 'spriteCollectionProxy' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		return fieldSpriteCollectionProxy.GetValue(window);
	}
	
	//-------------------------------------------------------------------------
	int[] GetSelectedSpriteEntries() {
		Type spriteCollectionEditorType = Type.GetType("tk2dSpriteCollectionEditorPopup");
		if (spriteCollectionEditorType == null) {
			return null;
		}
		
		EditorWindow window = EditorWindow.GetWindow(spriteCollectionEditorType, false, "Sprite Collection Editor", false);
		if (window == null)
			return null;
		
		return GetSelectedSpriteEntries(window);
	}
	
	//-------------------------------------------------------------------------
	int[] GetSelectedSpriteEntries(EditorWindow window) {
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldSelectedEntries = spriteCollectionEditorType.GetField("selectedEntries", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSelectedEntries == null) {
			Debug.LogError("Detected a missing 'selectedEntries' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		
		IList selectedEntries = (IList) fieldSelectedEntries.GetValue(window);
		
		int[] resultArray = new int[selectedEntries.Count];
		for (int i = 0; i < selectedEntries.Count; ++i) {
			object entry = selectedEntries[i];
			
			Type entryType = entry.GetType();
			FieldInfo fieldSpriteIndex = entryType.GetField("index");
			if (fieldSpriteIndex == null) {
				Debug.LogError("Detected a missing 'index' member variable at a TK2D sprite collection entry - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return null;
			}
			int spriteIndex = (int) fieldSpriteIndex.GetValue(entry);
			resultArray[i] = spriteIndex;
		}
		return resultArray;
	}
	
	//-------------------------------------------------------------------------
	void RestoreSpriteSelection(EditorWindow window, int[] spriteIDsToSelect) {
		
		// Does the following via reflection:
		//
		// foreach (object entry in window.entries)
		// {
		//   if (spriteIDsToSelect.Contains(entry.index))
		//   {
		//     entry.selected = true;
		//   }
		// }
		// window.UpdateSelection();
		
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldEntries = spriteCollectionEditorType.GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldEntries == null) {
			Debug.LogError("Detected a missing 'entries' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		IList entries = (IList) fieldEntries.GetValue(window);
		
		foreach (object entry in entries)
		{
			Type entryType = entry.GetType();
			FieldInfo fieldSpriteIndex = entryType.GetField("index");
			if (fieldSpriteIndex == null) {
				Debug.LogError("Detected a missing 'index' member variable at a TK2D sprite collection entry - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return;
			}
			
			int spriteIndex = (int) fieldSpriteIndex.GetValue(entry);
			bool shallSelect = (Array.IndexOf(spriteIDsToSelect, spriteIndex) != -1);
			if (shallSelect) {
				FieldInfo fieldSelected = entryType.GetField("selected");
				if (fieldSelected == null) {
					Debug.LogError("Detected a missing 'selected' member variable at a TK2D sprite collection entry - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
					return;
				}
				fieldSelected.SetValue(entry, true);
			}
		}
		
		MethodInfo methodUpdateSelection = spriteCollectionEditorType.GetMethod("UpdateSelection", BindingFlags.Instance | BindingFlags.NonPublic);
		methodUpdateSelection.Invoke(window, null);
	}
	
	//-------------------------------------------------------------------------
	void LoadCollectionInSpriteCollectionEditorWindow(EditorWindow window, SortedList<int, Component> differentSprites) {
		
		if (differentSprites.Count <= 0)
			return;
		
		// Does the following via reflection:
		// window.SetGeneratorAndSelectedSprite((tk2dSpriteCollection) spriteCollectionToDisplay, spriteIDToDisplay);
		Type spriteCollectionEditorType = window.GetType();
		MethodInfo methodSetGeneratorAndSelectedSprite = spriteCollectionEditorType.GetMethod("SetGeneratorAndSelectedSprite");
		
		int lastIndex = differentSprites.Count-1;
		int spriteIDToDisplay = differentSprites.Keys[lastIndex];
		Component sprite = differentSprites.Values[lastIndex];
		object spriteCollectionToDisplay = mAlgorithmHelper.GetTK2DSpriteCollection(sprite);
		
		object[] methodParams = new object[] { spriteCollectionToDisplay, spriteIDToDisplay };
		methodSetGeneratorAndSelectedSprite.Invoke(window, methodParams);
	}
	
	//-------------------------------------------------------------------------
	void SelectSpritesInSpriteCollectionEditor(EditorWindow window, int[] spriteIDs) {
		
		// Does the following via reflection:
		// window.SelectSpritesFromList(spriteIDs);
		Type spriteCollectionEditorType = window.GetType();
		MethodInfo methodSelectSpritesFromList = spriteCollectionEditorType.GetMethod("SelectSpritesFromList");
		
		object[] methodParams = new object[] { spriteIDs };
		methodSelectSpritesFromList.Invoke(window, methodParams);
	}
	
	//-------------------------------------------------------------------------
	void SetViewModeToColliderMode(EditorWindow window) {
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldSpriteView = spriteCollectionEditorType.GetField("spriteView", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSpriteView == null) {
			Debug.LogError("Detected a missing 'spriteView' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		object spriteView = fieldSpriteView.GetValue(window);
		
		Type spriteViewType = spriteView.GetType();
		FieldInfo fieldTextureEditor = spriteViewType.GetField("textureEditor", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldTextureEditor == null) {
			Debug.LogError("Detected a missing 'textureEditor' member variable at the TK2D editor window sprite view class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		object textureEditor = fieldTextureEditor.GetValue(spriteView);
		
		Type textureEditorType = textureEditor.GetType();
		FieldInfo fieldMode = textureEditorType.GetField("mode", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldMode == null) {
			Debug.LogError("Detected a missing 'mode' member variable at the TK2D editor window texture editor class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		object enumValue = fieldMode.GetValue(textureEditor);
		Type enumType = enumValue.GetType();
		int oldIntValue = (int) enumValue;
		bool isInColliderMode = oldIntValue == COLLIDER_EDIT_MODE_INT_VALUE;
		if (!isInColliderMode) {
			object newEnumValue = Enum.ToObject(enumType, COLLIDER_EDIT_MODE_INT_VALUE);
			fieldMode.SetValue(textureEditor, newEnumValue);
		}
	}
	
	//-------------------------------------------------------------------------
	bool GetEditorDisplayScale(out float editorScale, out PropertyInfo propertyEditorDisplayScale, out object textureEditor, EditorWindow window) {
		
		propertyEditorDisplayScale = null;
		textureEditor = null;
		editorScale = 1.0f;
		
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldSpriteView = spriteCollectionEditorType.GetField("spriteView", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSpriteView == null) {
			Debug.LogError("Detected a missing 'spriteView' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		object spriteView = fieldSpriteView.GetValue(window);
		
		Type spriteViewType = spriteView.GetType();
		FieldInfo fieldTextureEditor = spriteViewType.GetField("textureEditor", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldTextureEditor == null) {
			Debug.LogError("Detected a missing 'textureEditor' member variable at the TK2D editor window sprite view class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		textureEditor = fieldTextureEditor.GetValue(spriteView);
		
		Type textureEditorType = textureEditor.GetType();
		PropertyInfo propertySpriteCollection = textureEditorType.GetProperty("SpriteCollection", BindingFlags.Instance | BindingFlags.NonPublic);
		object spriteCollection = propertySpriteCollection.GetValue(textureEditor, null);
		if (spriteCollection != null) {
			propertyEditorDisplayScale = textureEditorType.GetProperty("editorDisplayScale", BindingFlags.Instance | BindingFlags.NonPublic);
			editorScale = (float) propertyEditorDisplayScale.GetValue(textureEditor, null);
			return true;
		}
		return false;
	}
	
	//-------------------------------------------------------------------------
	SortedList<int, Component> GetSpriteIDsAndContainerIDFromSelectedSprites() {
		
		object spriteCollectionToDisplay = 0;
		SortedList<int, Component> differentSprites = new SortedList<int, Component>(); // SortedList<spriteID, tk2dSprite>
		
		for (int index = Selection.gameObjects.Length - 1; index >= 0; --index) {
			Component tk2dSpriteObject = Selection.gameObjects[index].GetComponent("tk2dSprite");
			if (tk2dSpriteObject != null) {
				spriteCollectionToDisplay = mAlgorithmHelper.GetTK2DSpriteCollection(tk2dSpriteObject);
				break;
			}
		}
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			Component tk2dSpriteObject = gameObj.GetComponent("tk2dSprite");
			if (tk2dSpriteObject != null) {
				int spriteID = mAlgorithmHelper.GetSpriteID(tk2dSpriteObject);
				object collection = mAlgorithmHelper.GetTK2DSpriteCollection(tk2dSpriteObject);
				
				if (collection == spriteCollectionToDisplay && !differentSprites.ContainsKey(spriteID)) {
					differentSprites.Add(spriteID, tk2dSpriteObject);
				}
			}
		}
		return differentSprites;
	}
	
	//-------------------------------------------------------------------------
	void EnsureColliderTypePolyCollider(object spriteCollection, int[] spriteIDs) {
		
		bool wasSuccessful = mAlgorithmHelper.EnsureColliderTypePolyCollider(spriteCollection, spriteIDs);
		if (!wasSuccessful) {
			Debug.LogError(mAlgorithmHelper.LastError);
		}
	}
	
	//-------------------------------------------------------------------------
	void GenerateColliderVertices(object spriteCollection, int[] spriteIDs) {
		
		bool wasSuccessful = mAlgorithmHelper.GenerateColliderVertices(spriteCollection, spriteIDs);
		if (!wasSuccessful) {
			Debug.LogError(mAlgorithmHelper.LastError);
		}
	}
	
	//-------------------------------------------------------------------------
	bool AreArraysEqual<T>(T[] a, T[] b) {
	    return AreArraysEqual(a, b, EqualityComparer<T>.Default);
	}
	
	//-------------------------------------------------------------------------
	bool AreArraysEqual<T>(T[] a, T[] b, IEqualityComparer<T> comparer) {
	    if(a.Length != b.Length) {
	        return false;
	    }
	    for(int i = 0; i < a.Length; i++) {
	        if(!comparer.Equals(a[i], b[i])) {
	            return false;
	        }
	    }
	    return true;
	}
}
