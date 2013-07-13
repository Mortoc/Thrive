using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

/// <summary>
/// The parameter set at a single sprite.
/// </summary>
[System.Serializable]
public class ColliderGenTK2DParametersForSprite {
	public int mSpriteIndex;
	
	public int mOutlineVertexCount;
	public float mAlphaOpaqueThreshold;
	public bool mForceConvex;
	public bool mFlipNormals;
	public Texture2D mCustomTexture;
	public Vector2 mCustomScale;
	public Vector2 mCustomOffset;
}

//-------------------------------------------------------------------------
/// <summary>
/// Class to store collider-generation parameters of individual sprites of
/// a tk2d sprite collection in order to restore them when changing the
/// sprite selection or at application restart.
/// Will be persisted as a prefab object in the same directory as the
/// sprite collection data.
/// </summary>
[System.Serializable]
public class ColliderGenTK2DParameterStore : MonoBehaviour {
	
	public const int CURRENT_COLLIDER_GEN_VERSION = 0;
	
	public List<ColliderGenTK2DParametersForSprite> mStoredParameters;
	public int mColliderGenVersion = 0;
	protected static string mLastError = null;
	
	public string LastError {
		get {
			return mLastError;
		}
	}

	//-------------------------------------------------------------------------
	public ColliderGenTK2DParameterStore() {
		mStoredParameters = new List<ColliderGenTK2DParametersForSprite>();
	}
	
	//-------------------------------------------------------------------------
	public ColliderGenTK2DParametersForSprite GetParametersForSprite(int spriteIndex) {
		foreach (ColliderGenTK2DParametersForSprite paramObject in mStoredParameters) {
			if (paramObject.mSpriteIndex == spriteIndex) {
				return paramObject;
			}
		}
		return null;
	}
	
	//-------------------------------------------------------------------------
	public void SaveParametersForSprite(int spriteIndex, ColliderGenTK2DParametersForSprite parametersToSave) {
		for (int count = 0; count < mStoredParameters.Count; ++count) {
			ColliderGenTK2DParametersForSprite paramObject = mStoredParameters[count];
			if (paramObject.mSpriteIndex == spriteIndex) {
				mStoredParameters[count] = parametersToSave;
				return;
			}
		}
		
		// does not exist yet - add it
		mStoredParameters.Add(parametersToSave);
	}
			
			
	//-------------------------------------------------------------------------
	public static ColliderGenTK2DParameterStore EnsureParameterStorePrefabExistsForCollection(object spriteCollection) {
		
		ColliderGenTK2DParameterStore parameterStoreObject = null;
		string targetParameterStorePrefabPath = GetParameterStorePrefabFilePath(spriteCollection);
		string prefabDir = System.IO.Path.GetDirectoryName(targetParameterStorePrefabPath);
		
		System.IO.FileInfo fileInfo = new System.IO.FileInfo(prefabDir);
		if (!fileInfo.Directory.Exists) {
			SetLastError("Directory '" + prefabDir + "' for creating the ColliderGenTK2DParameterStore prefab does not exist.");
			return null;
		}
		
		parameterStoreObject = UnityEditor.AssetDatabase.LoadAssetAtPath(targetParameterStorePrefabPath, typeof(ColliderGenTK2DParameterStore)) as ColliderGenTK2DParameterStore;
		// Does not exist yet - create
		if (parameterStoreObject == null)
		{	
			GameObject go = new GameObject();
			go.AddComponent<ColliderGenTK2DParameterStore>();
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
			UnityEngine.Object p = EditorUtility.CreateEmptyPrefab(targetParameterStorePrefabPath);
			EditorUtility.ReplacePrefab(go, p);
#else
			UnityEngine.Object p = UnityEditor.PrefabUtility.CreateEmptyPrefab(targetParameterStorePrefabPath);
			PrefabUtility.ReplacePrefab(go, p);
#endif
			GameObject.DestroyImmediate(go);
			AssetDatabase.SaveAssets();

			parameterStoreObject = UnityEditor.AssetDatabase.LoadAssetAtPath(targetParameterStorePrefabPath, typeof(ColliderGenTK2DParameterStore)) as ColliderGenTK2DParameterStore;
		}
		return parameterStoreObject;
	}
	
	//-------------------------------------------------------------------------
	public static void SaveParameterStoreToPrefab(ColliderGenTK2DParameterStore parameterStore, object spriteCollection) {
		
		string targetParameterStorePrefabPath = GetParameterStorePrefabFilePath(spriteCollection);
		
		GameObject go = new GameObject();
		go.AddComponent<ColliderGenTK2DParameterStore>();
		ColliderGenTK2DParameterStore emptyComponent = go.GetComponent<ColliderGenTK2DParameterStore>();
		
		//EditorUtility.CopySerialized(parameterStore, emptyComponent);
		emptyComponent.CopyFrom(parameterStore);
		
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
		UnityEngine.Object p = EditorUtility.CreateEmptyPrefab(targetParameterStorePrefabPath);
		EditorUtility.ReplacePrefab(go, p);
#else
		UnityEngine.Object p = UnityEditor.PrefabUtility.CreateEmptyPrefab(targetParameterStorePrefabPath);
		PrefabUtility.ReplacePrefab(go, p);
#endif
		GameObject.DestroyImmediate(go);
		AssetDatabase.SaveAssets();
	}
	
	//-------------------------------------------------------------------------
	public static string GetParameterStorePrefabFilePath(object spriteCollection) {
		string prefabPath = GetSpriteCollectionPrefabFilePath(spriteCollection);
		string prefabDir = System.IO.Path.GetDirectoryName(prefabPath);
		string storePrefabPath = prefabDir + "/ColliderGenParameters.prefab";
		return storePrefabPath;
	}
	
	//-------------------------------------------------------------------------
	public static string GetSpriteCollectionPrefabFilePath(object spriteCollection) {
		
		// taken from tk2dSpriteCollectionBuilder class begin
		string path = UnityEditor.AssetDatabase.GetAssetPath((UnityEngine.Object) spriteCollection);
		string subDirName = System.IO.Path.GetDirectoryName(path.Substring(7));
		if (subDirName.Length > 0) subDirName += "/";

		string dataDirFullPath = Application.dataPath + "/" + subDirName + System.IO.Path.GetFileNameWithoutExtension(path) + " Data";
		string dataDirName = "Assets/" + dataDirFullPath.Substring( Application.dataPath.Length + 1 ) + "/";
		
		string prefabObjectPath = "";
		
		// changed for reflection begin
		// reads spriteCollection.spriteCollection via reflection.
		Type spriteCollectionType = spriteCollection.GetType();
		FieldInfo fieldSpriteCollection = spriteCollectionType.GetField("spriteCollection");
		object spriteCollectionData = fieldSpriteCollection.GetValue(spriteCollection);
		
		string spriteCollectionName = ((MonoBehaviour)spriteCollection).name;
		
		if (spriteCollectionData != null) // changed for reflection end
			prefabObjectPath = UnityEditor.AssetDatabase.GetAssetPath((UnityEngine.Object) spriteCollectionData);
		else
			prefabObjectPath = dataDirName + spriteCollectionName + ".prefab";
		return prefabObjectPath;
		// taken from tk2dSpriteCollectionBuilder class end
	}
	
	//-------------------------------------------------------------------------
	static void SetLastError(string description) {
		mLastError = description;
	}
	
	void CopyFrom(ColliderGenTK2DParameterStore src) {
		this.mStoredParameters = src.mStoredParameters;
	}
}
