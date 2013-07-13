#if (UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
#define ONLY_SINGLE_SELECTION_SUPPORTED_IN_INSPECTOR
#endif

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR

//-------------------------------------------------------------------------
/// <summary>
/// Editor class for the AlphaMeshCollider component.
/// </summary>
[CustomEditor(typeof(AlphaMeshCollider))]
[CanEditMultipleObjects]
public class EditorScriptAlphaMeshCollider : Editor {
	
	protected float mOldAlphaThreshold = 0;
	protected bool mOldFlipNormals = false;
	protected bool mFlipHorizontalChanged = false;
	protected bool mFlipVerticalChanged = false;
	protected bool mOldConvex = false;
	protected int mOldPointCount = 0;
	protected float mOldThickness = 0;
	protected float mOldDistanceTolerance = 0;
	protected float mOldCustomRotation = 0.0f;
	protected Vector2 mOldCustomScale = Vector2.one;
	protected Vector3 mOldCustomOffset = Vector3.zero;
		
	protected bool mLiveUpdate = true;
	protected bool mShowAdvanced = false;
	protected Texture2D mOldCustomTex = null;
	protected int mPointCountSliderMax = 100;
	
	SerializedProperty targetLiveUpdate;
	SerializedProperty targetAlphaOpaqueThreshold;
	SerializedProperty targetFlipNormals;
	SerializedProperty targetConvex;
	SerializedProperty targetMaxPointCount;
	SerializedProperty targetVertexReductionDistanceTolerance;
	SerializedProperty targetThickness;
	SerializedProperty targetHasOTSpriteComponent;
	SerializedProperty targetCopyOTSpriteFlipping;
	SerializedProperty targetCustomRotation;
	SerializedProperty targetCustomScale;
	SerializedProperty targetCustomOffset;
	
	//-------------------------------------------------------------------------
	class DuplicatePermittingIntComparer : IComparer<int> {
		
		public int Compare(int x, int y) {
			if (x > y)
				return -1;
			else
				return 1;
		}
	}
	
	//-------------------------------------------------------------------------
	[MenuItem ("Component/Physics/Alpha MeshCollider", false, 51)]
	static void ComponentPhysicsAlphaMeshCollider() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			AlphaMeshCollider alphaCollider = gameObj.GetComponent<AlphaMeshCollider>();
			if (alphaCollider == null) {
				alphaCollider = gameObj.AddComponent<AlphaMeshCollider>();
			}
		}
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("Component/Physics/Alpha MeshCollider", true)]
	static bool ValidateComponentPhysicsAlphaMeshCollider() {
		
		if (Selection.gameObjects.Length == 0) {
			return false;
		}
		else {
			foreach (GameObject gameObj in Selection.gameObjects) {
				object tk2dSprite = gameObj.GetComponent("tk2dBaseSprite");
				if (tk2dSprite != null) {
					return false;
				}
			}
			return true; // no tk2dSprite selected
		}
    }
	
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/Add AlphaMeshCollider", false, 10)]
	static void ColliderGenAddAlphaMeshCollider() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			AlphaMeshCollider alphaCollider = gameObj.GetComponent<AlphaMeshCollider>();
			if (alphaCollider == null) {
				alphaCollider = gameObj.AddComponent<AlphaMeshCollider>();
			}
		}
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("2D ColliderGen/Add AlphaMeshCollider", true)]
	static bool ValidateColliderGenAddAlphaMeshCollider() {
		
		if (Selection.gameObjects.Length == 0) {
			return false;
		}
		else {
			foreach (GameObject gameObj in Selection.gameObjects) {
				Component tk2dSprite = gameObj.GetComponent("tk2dBaseSprite");
				if (tk2dSprite != null) {
					return false;
				}
			}
			return true; // no tk2dSprite selected
		}
    }
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/Select AlphaMeshCollider Children", false, 11)]
	static void ColliderGenSelectChildAlphaMeshColliders() {
		
		SelectChildAlphaMeshColliders(Selection.gameObjects);
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("2D ColliderGen/Remove AlphaMeshCollider Components", true)]
	static bool ValidateColliderGenRemoveColliderAndGenerator() {
		
		return (Selection.gameObjects.Length > 0);
    }

	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/Remove AlphaMeshCollider Components", false, 11)]
	static void ColliderGenRemoveColliderAndGenerator() {
		
		RemoveColliderAndGenerator(Selection.gameObjects);
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("2D ColliderGen/Select AlphaMeshCollider Children", true)]
	static bool ValidateColliderGenSelectChildAlphaMeshColliders() {
		
		return (Selection.gameObjects.Length > 0);
    }
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/SmoothMoves Specific/Add AlphaMeshColliders To BoneAnimation", false, 100)]
	static void ColliderGenAddAlphaMeshColliderToAllBones() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			Component boneAnimObject = gameObj.GetComponent("BoneAnimation");
			if (boneAnimObject != null) {
				AddCollidersToBoneAnimationTree(gameObj.transform);
			}
		}
		
		SelectChildAlphaMeshColliders(Selection.gameObjects);
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("2D ColliderGen/SmoothMoves Specific/Add AlphaMeshColliders To BoneAnimation", true)]
	static bool ValidateColliderGenAddAlphaMeshColliderToAllBones() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			Component boneAnimObject = gameObj.GetComponent("BoneAnimation");
			if (boneAnimObject != null) {
				return true;
			}
		}
		return false; // no BoneAnimation component found.
    }
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/Orthello Specific/Add AlphaMeshColliders To OTTileMap", false, 101)]
	static void ColliderGenAddAlphaMeshColliderToTileMap() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			Component tileMapObject = gameObj.GetComponent("OTTileMap");
			if (tileMapObject != null) {
				AddCollidersToOTTileMap(gameObj.transform, tileMapObject);
			}
		}
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("2D ColliderGen/Orthello Specific/Add AlphaMeshColliders To OTTileMap", true)]
	static bool ValidateColliderGenAddAlphaMeshColliderToTileMap() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			Component tileMapObject = gameObj.GetComponent("OTTileMap");
			if (tileMapObject != null) {
				return true;
			}
		}
		return false; // no OTTileMap component found.
    }
	
	//-------------------------------------------------------------------------
	void OnEnable() {
        // Setup the SerializedProperties
		targetLiveUpdate = serializedObject.FindProperty("mLiveUpdate");
		targetAlphaOpaqueThreshold = serializedObject.FindProperty("mAlphaOpaqueThreshold");
		targetFlipNormals = serializedObject.FindProperty("mFlipInsideOutside");
		targetConvex = serializedObject.FindProperty("mConvex");
		targetMaxPointCount = serializedObject.FindProperty("mMaxPointCount");
		targetVertexReductionDistanceTolerance = serializedObject.FindProperty("mVertexReductionDistanceTolerance");
		targetThickness = serializedObject.FindProperty("mThickness");
		targetHasOTSpriteComponent = serializedObject.FindProperty("mHasOTSpriteComponent");
		targetCopyOTSpriteFlipping = serializedObject.FindProperty("mCopyOTSpriteFlipping");
		targetCustomRotation = serializedObject.FindProperty("mCustomRotation");
		targetCustomScale = serializedObject.FindProperty("mCustomScale");
		targetCustomOffset = serializedObject.FindProperty("mCustomOffset");
		
		mPointCountSliderMax = AlphaMeshColliderPreferences.Instance.ColliderPointCountSliderMaxValue;
    }
	
    //-------------------------------------------------------------------------
	// Newer multi-selection version.
	public override void OnInspectorGUI() {
		
		EditorGUIUtility.LookLikeInspector();
		
		// Update the serializedProperty - needed in the beginning of OnInspectorGUI.
		serializedObject.Update();
		
		mOldAlphaThreshold = targetAlphaOpaqueThreshold.floatValue;
		mOldFlipNormals = targetFlipNormals.boolValue;
		mOldConvex = targetConvex.boolValue;
		mOldPointCount = targetMaxPointCount.intValue;
		mOldThickness = targetThickness.floatValue;
		mOldDistanceTolerance = targetVertexReductionDistanceTolerance.floatValue;
		mOldCustomRotation = targetCustomRotation.floatValue;
		mOldCustomScale = targetCustomScale.vector2Value;
		mOldCustomOffset = targetCustomOffset.vector3Value;

		Texture2D usedTexture = null;
		bool usedTextureIsCustomTex = false;
		bool areUsedTexturesDifferent = false;
		bool areOutputDirectoriesDifferent = false;
		bool areOutputFilenamesDifferent = false;
		bool areGroupSuffixesDifferent = false;
		float imageMinExtent = 128;
		string commonOutputDirectoryPath = null;
		string commonOutputFilename = null;
		string commonGroupSuffix = null;
		Texture2D commonCustomTexture = null;
		bool areCustomTexturesDifferent = false;
		bool isAtlas = false;
		bool canReloadAnyCollider = false;
		bool canRecalculateAnyCollider = false;
		
		
		Object[] targetObjects = serializedObject.targetObjects;
		SortedList<int, AlphaMeshCollider> sortedTargets = SortAlphaMeshColliders(targetObjects);
		areUsedTexturesDifferent = sortedTargets.Values[0].UsedTexture != sortedTargets.Values[sortedTargets.Count-1].UsedTexture;
		
		AlphaMeshCollider targetObject;
		for (int targetIndex = 0; targetIndex != targetObjects.Length; ++targetIndex) {
			targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
			Texture2D currentTexture = targetObject.UsedTexture;
			usedTextureIsCustomTex = (targetObject.CustomTex != null);
			isAtlas = targetObject.mIsAtlasUsed;
			int textureWidth = targetObject.UsedTexture != null ? targetObject.UsedTexture.width : 0;
			int textureHeight = targetObject.UsedTexture != null ? targetObject.UsedTexture.height : 0;
			imageMinExtent = Mathf.Min(imageMinExtent, Mathf.Min(textureWidth, textureHeight));
			
			if (targetObject.CanReloadCollider)
				canReloadAnyCollider = true;
			if (targetObject.CanRecalculateCollider)
				canRecalculateAnyCollider = true;
			
			// set commonOutputDirectoryPath and areOutputDirectoriesDifferent
			if (commonOutputDirectoryPath != null && !commonOutputDirectoryPath.Equals(targetObject.ColliderMeshDirectory)) {
				areOutputDirectoriesDifferent = true;
			}
			else {
				commonOutputDirectoryPath = targetObject.ColliderMeshDirectory;
			}
			
			// set commonGroupSuffix and areGroupSuffixesDifferent
			if (commonGroupSuffix != null && !commonGroupSuffix.Equals(targetObject.GroupSuffix)) {
				areGroupSuffixesDifferent = true;
			}
			else {
				commonGroupSuffix = targetObject.GroupSuffix;
			}
			
			// set commonOutputFilename and areOutputFilenamesDifferent
			if (commonOutputFilename != null && !commonOutputFilename.Equals(targetObject.mColliderMeshFilename)) {
				areOutputFilenamesDifferent = true;
			}
			else {
				commonOutputFilename = targetObject.mColliderMeshFilename;
			}
			
			// set commonCustomTexture and areCustomTexturesDifferent
			if (commonCustomTexture != null && commonCustomTexture != targetObject.CustomTex) {
				areCustomTexturesDifferent = true;
			}
			else {
				commonCustomTexture = targetObject.CustomTex;
			}
			
			if (!usedTexture) {
				usedTexture = currentTexture;
			}
		}
		if (!areUsedTexturesDifferent) {
			
			if (usedTexture == null) {
				EditorGUILayout.LabelField("No Texture Image", "Set Advanced/Custom Image");
			}
			else {
				if (usedTextureIsCustomTex) {
					EditorGUILayout.ObjectField("Custom Image", usedTexture, typeof(Texture2D), false);
				}
				else {
					if (isAtlas) {
						EditorGUILayout.ObjectField("Atlas / SpriteSheet", usedTexture, typeof(Texture2D), false);

					}
					else {
						EditorGUILayout.ObjectField("Texture Image", usedTexture, typeof(Texture2D), false);
					}
				}
				EditorGUILayout.LabelField("Texture Width x Height: ", usedTexture.width.ToString() + " x " + usedTexture.height.ToString());
			}
		}
		else {
			EditorGUILayout.LabelField("Texture Image", "<different textures selected>");
		}
		
		if (canRecalculateAnyCollider) {
			EditorGUILayout.PropertyField(targetLiveUpdate, new GUIContent("Editor Live Update"));
			
			// float [0..1] Alpha Opaque Threshold
			EditorGUILayout.Slider(targetAlphaOpaqueThreshold, 0.0f, 1.0f, "Alpha Opaque Threshold");
			
			// int [3..100] max point count
			EditorGUILayout.IntSlider(targetMaxPointCount, 3, mPointCountSliderMax, "Outline Vertex Count");
			
			// removed, since it was not too intuitive to use.
			// float [0..0.3] Accepted Distance
			//EditorGUILayout.Slider(targetVertexReductionDistanceTolerance, 0.0f, 0.3f, "Accepted Distance");
			
			// float thickness
			EditorGUILayout.PropertyField(targetThickness, new GUIContent("Z-Thickness"));
		}
		
		if (canReloadAnyCollider || canRecalculateAnyCollider) {
			// copy OT sprite flipping
			bool showFlipProperties = true;
			
			if (!targetHasOTSpriteComponent.hasMultipleDifferentValues &&
				targetHasOTSpriteComponent.boolValue == true) {
				
				//targetObject.mCopyOTSpriteFlipping = EditorGUILayout.Toggle("Copy OTSprite Flipping", targetObject.mCopyOTSpriteFlipping);
				EditorGUILayout.PropertyField(targetCopyOTSpriteFlipping, new GUIContent("Copy OTSprite Flipping"));
				
				if (targetCopyOTSpriteFlipping.boolValue == true) {
					showFlipProperties = false;
				}
			}
			
			if (showFlipProperties) {
				bool areFlipHorizontalDifferent = false;
				bool areFlipVerticalDifferent = false;
				mFlipHorizontalChanged = false;
				mFlipVerticalChanged = false;
				
				targetObject = (AlphaMeshCollider) targetObjects[0];
				bool flipHorizontal = targetObject.FlipHorizontal;
				bool flipVertical = targetObject.FlipVertical;
				
				for (int targetIndex = 1; targetIndex < targetObjects.Length; ++targetIndex) {
					targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
					bool currentFlipHorizontal = targetObject.FlipHorizontal;
					bool currentFlipVertical = targetObject.FlipVertical;
					if (currentFlipHorizontal != flipHorizontal)
						areFlipHorizontalDifferent = true;
					if (currentFlipVertical != flipVertical)
						areFlipVerticalDifferent = true;
				}
				
				// flip horizontal
				bool newFlipHorizontal = false;
				if (!areFlipHorizontalDifferent) {
					newFlipHorizontal = EditorGUILayout.Toggle("Flip Horizontal", flipHorizontal);
					
					if (newFlipHorizontal != flipHorizontal) {
						mFlipHorizontalChanged = true;
						
						for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
							targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
							targetObject.FlipHorizontal = newFlipHorizontal;
						}
					}
				}
				else {
					EditorGUILayout.LabelField("Flip Horizontal", "<different values>");
				}
				
				// flip vertical
				bool newFlipVertical = false;
				if (!areFlipVerticalDifferent) {
					newFlipVertical = EditorGUILayout.Toggle("Flip Vertical", flipVertical);
					
					if (newFlipVertical != flipVertical) {
						mFlipVerticalChanged = true;
					
						for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
							targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
							targetObject.FlipVertical = newFlipVertical;
						}
					}
				}
				else {
					EditorGUILayout.LabelField("Flip Vertical", "<different values>");
				}
			}
			
			EditorGUILayout.PropertyField(targetConvex, new GUIContent("Force Convex"));
			
			EditorGUILayout.PropertyField(targetFlipNormals, new GUIContent("Flip Normals"));
		}
		
		// output directory
		string newOutputDirectoryPath = null;
		if (!areOutputDirectoriesDifferent) {
			newOutputDirectoryPath = EditorGUILayout.TextField("Output Directory", commonOutputDirectoryPath);
		}
		else {
			newOutputDirectoryPath = EditorGUILayout.TextField("Output Directory", "<different values>");
		}
		if (!newOutputDirectoryPath.Equals(commonOutputDirectoryPath) && !newOutputDirectoryPath.Equals("<different values>")) {
			for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
				targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
				targetObject.ColliderMeshDirectory = newOutputDirectoryPath;
			}
		}
		
		// group suffix
		string newGroupSuffix = null;
		if (!areGroupSuffixesDifferent) {
			newGroupSuffix = EditorGUILayout.TextField("Group Suffix", commonGroupSuffix);
		}
		else {
			newGroupSuffix = EditorGUILayout.TextField("Group Suffix", "<different values>");
		}
		if (!newGroupSuffix.Equals(commonGroupSuffix) && !newGroupSuffix.Equals("<different values>")) {
			for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
				targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
				targetObject.GroupSuffix = newGroupSuffix;
			}
		}
		
		// output filename (read-only)
		if (!areOutputFilenamesDifferent) {
			EditorGUILayout.TextField("Output Filename", commonOutputFilename);
		}
		else {
			EditorGUILayout.TextField("Output Filename", "<different values>");
		}
		
		// Advanced settings
		mShowAdvanced = EditorGUILayout.Foldout(mShowAdvanced, "Advanced Settings");
        if(mShowAdvanced) {
			EditorGUI.indentLevel++;
			
			Texture2D newCustomTexture = null;
			if (!areCustomTexturesDifferent) {
				newCustomTexture = (Texture2D) EditorGUILayout.ObjectField("Custom Image", commonCustomTexture, typeof(Texture2D), false);
				if (newCustomTexture != commonCustomTexture) {

					for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
						targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
						targetObject.CustomTex = newCustomTexture;
					}
					
					sortedTargets = SortAlphaMeshColliders(targetObjects); // path hash values are outdated - recalculate them!
					areUsedTexturesDifferent = sortedTargets.Values[0].UsedTexture != sortedTargets.Values[sortedTargets.Count-1].UsedTexture;
					
					ReloadOrRecalculateSelectedColliders(sortedTargets);
				}
			}
			else {
				EditorGUILayout.LabelField("Custom Image", "<different images selected>");
			}

			EditorGUILayout.Slider(targetCustomRotation, 0.0f, 360.0f, new GUIContent("Custom Rotation"));
			EditorGUILayout.PropertyField(targetCustomScale, new GUIContent("Custom Scale"), true);
			EditorGUILayout.PropertyField(targetCustomOffset, new GUIContent("Custom Offset"), true);
			EditorGUI.indentLevel--;
		}
		
		// Apply changes to the serializedProperty.
        serializedObject.ApplyModifiedProperties();
		mLiveUpdate = sortedTargets.Values[0].mLiveUpdate;
		
		EditorGUILayout.BeginHorizontal();
		if (canReloadAnyCollider)
		{
			if (GUILayout.Button("Reload Collider")) {
	
				ReloadSelectedColliders(sortedTargets);
			}
		}
		if (canRecalculateAnyCollider) {
			if (GUILayout.Button("Recalculate Collider")) {
				
				RecalculateSelectedColliders(sortedTargets);
			}
		}
		EditorGUILayout.EndHorizontal();
		
		if (mLiveUpdate) {
			bool pointCountNeedsUpdate = ((mOldPointCount != targetMaxPointCount.intValue) && (targetMaxPointCount.intValue > 2)); // when typing 28, it would otherwise update at the first digit '2'.
			if (pointCountNeedsUpdate ||
				mOldConvex != targetConvex.boolValue ||
				mOldThickness != targetThickness.floatValue ||
				mOldDistanceTolerance != targetVertexReductionDistanceTolerance.floatValue) {
			
				RecalculateSelectedCollidersFromPreviousResult(sortedTargets);
			}
			
			if (mOldAlphaThreshold != targetAlphaOpaqueThreshold.floatValue ||
				mOldFlipNormals != targetFlipNormals.boolValue) {
			
				RecalculateSelectedColliders(sortedTargets);
			}
			
			if (mOldCustomRotation != targetCustomRotation.floatValue ||
				mOldCustomScale != targetCustomScale.vector2Value ||
				mOldCustomOffset != targetCustomOffset.vector3Value) {
				
				RewriteSelectedColliders(sortedTargets);
			}
		}
		
		if (GUI.changed) {
			foreach (Object target in targetObjects) {
            	EditorUtility.SetDirty(target);
			}
		}
		
		EditorGUIUtility.LookLikeControls();
	}
	
	//-------------------------------------------------------------------------
	static void SelectChildAlphaMeshColliders(GameObject[] gameObjects) {
		
		List<GameObject> newSelectionList = new List<GameObject>();
		
		foreach (GameObject gameObj in gameObjects) {
			
			AddAlphaMeshCollidersOfTreeToList(gameObj.transform, ref newSelectionList);
		}
		
		GameObject[] newSelection = newSelectionList.ToArray();
		Selection.objects = newSelection;
	}
	
	//-------------------------------------------------------------------------
	static void RemoveColliderAndGenerator(GameObject[] gameObjects) {
		foreach (GameObject gameObj in gameObjects) {
			
			AlphaMeshCollider alphaMeshColliderComponent = gameObj.GetComponent<AlphaMeshCollider>();
			if (alphaMeshColliderComponent) {
				DestroyImmediate(alphaMeshColliderComponent);
			}
			AlphaMeshColliderSmoothMovesRestore restoreComponent = gameObj.GetComponent<AlphaMeshColliderSmoothMovesRestore>();
			if (restoreComponent) {
				DestroyImmediate(restoreComponent);
			}
			MeshCollider meshColliderComponent = gameObj.GetComponent<MeshCollider>();
			if (meshColliderComponent) {
				DestroyImmediate(meshColliderComponent);
			}
		}
	}
	
	//-------------------------------------------------------------------------
	static void AddAlphaMeshCollidersOfTreeToList(Transform node, ref List<GameObject> resultList) {
		
		AlphaMeshCollider alphaCollider = node.GetComponent<AlphaMeshCollider>();
		if (alphaCollider != null) {
			resultList.Add(node.gameObject);
		}
		
		foreach (Transform child in node) {
			AddAlphaMeshCollidersOfTreeToList(child, ref resultList);
		}
	}
	
	// TODO: delete if we are done with everythying..
	//-------------------------------------------------------------------------
	/*static void AddCollidersToBoneAnimationTree(Transform node) {
		foreach (Transform child in node) {
			
			if (child.GetComponent("AnimationBoneCollider") != null) {
				AlphaMeshCollider collider = child.GetComponent<AlphaMeshCollider>();
				if (collider == null) {
					collider = child.gameObject.AddComponent<AlphaMeshCollider>();
				}
			}
			
			AddCollidersToBoneAnimationTree(child);
		}
	}*/
	
	//-------------------------------------------------------------------------
	static void AddCollidersToBoneAnimationTree(Transform node) {
		foreach (Transform child in node) {
			
			if (!child.name.EndsWith("_Sprite")) {
				AlphaMeshCollider collider = child.GetComponent<AlphaMeshCollider>();
				if (collider == null) {
					collider = child.gameObject.AddComponent<AlphaMeshCollider>();
				}
			}
			
			AddCollidersToBoneAnimationTree(child);
		}
	}
	
	//-------------------------------------------------------------------------
	static void AddCollidersToOTTileMap(Transform tileMapNode, Component otTileMap) {
		
		// OTTileMapLayer[]  otTileMap.layers
		System.Type otTileMapType = otTileMap.GetType();
		FieldInfo fieldLayers = otTileMapType.GetField("layers");
		if (fieldLayers == null) {
			Debug.LogError("Detected a missing 'layers' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}

		// add a GameObject node named "AlphaMeshColliders"
		GameObject collidersNode = new GameObject("AlphaMeshColliders");
		collidersNode.transform.parent = tileMapNode;
		collidersNode.transform.localPosition = Vector3.zero;
		collidersNode.transform.localScale = Vector3.one;
		
		IEnumerable layersArray = (IEnumerable) fieldLayers.GetValue(otTileMap);
		int layerIndex = 0;
		foreach (object otTileMapLayer in layersArray) {
		
			System.Type otTileMapLayerType = otTileMapLayer.GetType();
			FieldInfo fieldName = otTileMapLayerType.GetField("name");
			if (fieldName == null) {
				Debug.LogError("Detected a missing 'name' member variable at OTTileMapLayer component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return;
			}
			string layerName = (string) fieldName.GetValue(otTileMapLayer);
			// add a GameObject node for each tilemap layer.
			GameObject layerNode = new GameObject(layerName);
			layerNode.transform.parent = collidersNode.transform;
			layerNode.transform.localPosition = Vector3.zero;
			layerNode.transform.localScale = Vector3.one;
			
			addColliderGameObjectsForOTTileMapLayer(layerNode.transform, otTileMap, otTileMapLayer, layerIndex);
			++layerIndex;
		}
	}
	
	//-------------------------------------------------------------------------
	static void addColliderGameObjectsForOTTileMapLayer(Transform layerNode, Component otTileMap, object otTileMapLayer, int layerIndex) {
	
		// read tileMapSize = OTTileMap.mapSize (UnityEngine.Vector2)
		System.Type otTileMapType = otTileMap.GetType();
		FieldInfo fieldMapSize = otTileMapType.GetField("mapSize");
		if (fieldMapSize == null) {
			Debug.LogError("Detected a missing 'mapSize' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		Vector2 tileMapSize = (UnityEngine.Vector2) fieldMapSize.GetValue(otTileMap);
		int tileMapWidth = (int) tileMapSize.x;
		int tileMapHeight = (int) tileMapSize.y;
		// read mapTileSize = OTTileMap.mapTileSize (UnityEngine.Vector2)
		FieldInfo fieldMapTileSize = otTileMapType.GetField("mapTileSize");
		if (fieldMapTileSize == null) {
			Debug.LogError("Detected a missing 'mapTileSize' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		Vector2 mapTileSize = (UnityEngine.Vector2) fieldMapTileSize.GetValue(otTileMap);
		Vector3 mapTileScale = new Vector3(1.0f / tileMapSize.x, 1.0f / tileMapSize.y, 1.0f / tileMapSize.x);
		
		System.Collections.Generic.Dictionary<int, object> tileSetAtTileIndex = new System.Collections.Generic.Dictionary<int, object>();
		
	
		Vector2 bottomLeftTileOffset = new Vector2(-0.5f, -0.5f);
		
		// read tileIndices = otTileMapLayer.tiles (int[])
		System.Type otTileMapLayerType = otTileMapLayer.GetType();
		FieldInfo fieldTiles = otTileMapLayerType.GetField("tiles");
		if (fieldTiles == null) {
			Debug.LogError("Detected a missing 'tiles' member variable at OTTileMapLayer component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		int[] tileIndices = (int[]) fieldTiles.GetValue(otTileMapLayer);
		System.Collections.Generic.Dictionary<int, Transform> groupNodeForTileIndex = new System.Collections.Generic.Dictionary<int, Transform>();
		Transform tileGroupNode = null;
		
		object tileSet = null;
		
		for (int y = 0; y < tileMapHeight; ++y) {
			for (int x = 0; x < tileMapWidth; ++x) {
				int tileIndex = tileIndices[y * tileMapWidth + x];
				if (tileIndex != 0) {
				
					if (groupNodeForTileIndex.ContainsKey(tileIndex)) {
						tileGroupNode = groupNodeForTileIndex[tileIndex];
						tileSet = tileSetAtTileIndex[tileIndex];
					}
					else {
						// create a group node
						GameObject newTileGroup = new GameObject("Tile Type " + tileIndex);
						newTileGroup.transform.parent = layerNode;
						newTileGroup.transform.localPosition = Vector3.zero;
						newTileGroup.transform.localScale = Vector3.one;
						tileGroupNode = newTileGroup.transform;
						groupNodeForTileIndex[tileIndex] = tileGroupNode;
						// get tileset for tile index
						tileSet = AlphaMeshCollider.GetOTTileSetForTileIndex(otTileMap, tileIndex);
						tileSetAtTileIndex[tileIndex] = tileSet;
					}
					// read tileSet.tileSize (Vector2)
					System.Type otTileSetType = tileSet.GetType();
					FieldInfo fieldTileSize = otTileSetType.GetField("tileSize");
					if (fieldTileSize == null) {
						Debug.LogError("Detected a missing 'tileSize' member variable at OTTileSet class - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
						return;
					}
					Vector2 tileSize = (UnityEngine.Vector2) fieldTileSize.GetValue(tileSet);
					Vector3 tileScale = new Vector3(mapTileScale.x / mapTileSize.x * tileSize.x, mapTileScale.y / mapTileSize.y * tileSize.y, mapTileScale.z);
					Vector2 tileCenterOffset = new Vector3(tileScale.x * 0.5f, tileScale.x * 0.5f);
				
					// add a GameObject for each enabled tile with name "tile y x"
					GameObject alphaMeshColliderNode = new GameObject("tile " + y + " " + x);
					alphaMeshColliderNode.transform.parent = tileGroupNode;
					AlphaMeshCollider alphaMeshColliderComponent = alphaMeshColliderNode.AddComponent<AlphaMeshCollider>();
					alphaMeshColliderComponent.SetOTTileMap(otTileMap, layerIndex, x, y, tileMapWidth);
					
					// set the position of the tile collider according to its (x,y) pos in the map.
					alphaMeshColliderNode.transform.localPosition = new Vector3(x * mapTileScale.x + bottomLeftTileOffset.x + tileCenterOffset.x, (tileMapSize.y - 1 - y) * mapTileScale.y + bottomLeftTileOffset.y + tileCenterOffset.y, 0.0f);
					alphaMeshColliderNode.transform.localScale = tileScale;
				}
			}
		}
	}
	
	//-------------------------------------------------------------------------
	void ReloadSelectedColliders(SortedList<int, AlphaMeshCollider> sortedTargets) {
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			AlphaMeshCollider target = pair.Value;
			if (target.CanReloadCollider) {
				target.ReloadCollider();
			}
		}
	}
	
	//-------------------------------------------------------------------------
	void ReloadOrRecalculateSelectedColliders(SortedList<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.ReloadOrRecalculateColliderAndUpdateSimilar(target); // if found, just load it, if not, recalculate and update all others.
			}
			lastHash = hash;
		}
	}
	
	//-------------------------------------------------------------------------
	void RecalculateSelectedColliders(SortedList<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.RecalculateColliderAndUpdateSimilar(target);
			}
			lastHash = hash;
		}
	}
	
	//-------------------------------------------------------------------------
	void RecalculateSelectedCollidersFromPreviousResult(SortedList<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.RecalculateColliderFromPreviousResultAndUpdateSimilar(target);
			}
			lastHash = hash;
		}
	}
	
	//-------------------------------------------------------------------------
	void RewriteSelectedColliders(SortedList<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.RewriteColliderToFileAndUpdateSimilar(target);
			}
			lastHash = hash;
		}
	}
	
	//-------------------------------------------------------------------------
	SortedList<int, AlphaMeshCollider> SortAlphaMeshColliders(Object[] unsortedAlphaMeshColliders) {
		SortedList<int, AlphaMeshCollider> resultList = new SortedList<int, AlphaMeshCollider>(new DuplicatePermittingIntComparer());
		
		foreach (AlphaMeshCollider alphaMeshCollider in unsortedAlphaMeshColliders) {
			int textureHash = alphaMeshCollider.FullColliderMeshPath().GetHashCode();
			resultList.Add(textureHash, alphaMeshCollider);
		}
		
		return resultList;
	}
}

#endif // #if UNITY_EDITOR
