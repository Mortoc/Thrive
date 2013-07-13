#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//-------------------------------------------------------------------------
/// <summary>
/// Class to read and write AlphaMeshCollider relevant editor preference values.
/// </summary>
public class AlphaMeshColliderPreferences
{
	//-------------------------------------------------------------------------
	/// <summary> The singleton instance. </summary>
	static AlphaMeshColliderPreferences mInstance = null;	
	public static AlphaMeshColliderPreferences Instance {
		get {
			if (mInstance == null) {
				mInstance = new AlphaMeshColliderPreferences();
				mInstance.ReadAllParams();
			}
			return mInstance;
		}
	}
	
	const string INITIAL_COLLIDER_PATH = "Assets/Colliders/Generated";
	//-------------------------------------------------------------------------	
	string mDefaultColliderDirectory = INITIAL_COLLIDER_PATH;
	bool mDefaultLiveUpdate;
	int mDefaultColliderPointCount;
	int mColliderPointCountSliderMaxValue;
	bool mDefaultConvex;
	float mDefaultAbsoluteColliderThickness;
	
	
	//-------------------------------------------------------------------------
	public string DefaultColliderDirectory {
		get {
			return mDefaultColliderDirectory;
		}
		set {
			if (mDefaultColliderDirectory != value) {
				string correctedPath = value;
				if (correctedPath.Length==0) {
					correctedPath = INITIAL_COLLIDER_PATH;
				}
				mDefaultColliderDirectory = correctedPath;
				EditorPrefs.SetString("AlphaMeshCollider_DefaultColliderDirectory", mDefaultColliderDirectory);
			}
		}
	}
	public bool DefaultLiveUpdate {
		get {
			return mDefaultLiveUpdate;
		}
		set {
			if (mDefaultLiveUpdate != value) {
				mDefaultLiveUpdate = value;
				EditorPrefs.SetBool("AlphaMeshCollider_DefaultLiveUpdate", mDefaultLiveUpdate);
			}
		}
	}
	public int DefaultColliderPointCount {
		get {
			return mDefaultColliderPointCount;
		}
		set {
			if (mDefaultColliderPointCount != value) {
				mDefaultColliderPointCount = value;
				if (mDefaultColliderPointCount < 2)
					mDefaultColliderPointCount = 2;
				EditorPrefs.SetInt("AlphaMeshCollider_DefaultColliderPointCount", mDefaultColliderPointCount);
			}
		}
	}
	public int ColliderPointCountSliderMaxValue {
		get {
			return mColliderPointCountSliderMaxValue;
		}
		set {
			if (mColliderPointCountSliderMaxValue != value)  {
				mColliderPointCountSliderMaxValue = value;
				if (mColliderPointCountSliderMaxValue < 4)
					mColliderPointCountSliderMaxValue = 4;
				EditorPrefs.SetInt("AlphaMeshCollider_ColliderPointCountSliderMaxValue", mColliderPointCountSliderMaxValue);
			}
		}
	}
	public bool DefaultConvex {
		get {
			return mDefaultConvex;
		}
		set {
			if (mDefaultConvex != value) {
				mDefaultConvex = value;
				EditorPrefs.SetBool("AlphaMeshCollider_DefaultConvex", mDefaultConvex);
			}
		}
	}
	
	public float DefaultAbsoluteColliderThickness {
		get {
			return mDefaultAbsoluteColliderThickness;
		}
		set {
			if (mDefaultAbsoluteColliderThickness != value) {
				mDefaultAbsoluteColliderThickness = value;
				EditorPrefs.SetFloat("AlphaMeshCollider_DefaultAbsoluteColliderThickness", mDefaultAbsoluteColliderThickness);
			}
		}
	}
	//-------------------------------------------------------------------------
	
	//-------------------------------------------------------------------------
	void ReadAllParams()
	{
		mDefaultColliderDirectory = EditorPrefs.GetString("AlphaMeshCollider_DefaultColliderDirectory", INITIAL_COLLIDER_PATH);
		mDefaultLiveUpdate = EditorPrefs.GetBool("AlphaMeshCollider_DefaultLiveUpdate", true);
		mDefaultColliderPointCount = EditorPrefs.GetInt("AlphaMeshCollider_DefaultColliderPointCount", 20);
		mColliderPointCountSliderMaxValue = EditorPrefs.GetInt("AlphaMeshCollider_ColliderPointCountSliderMaxValue", 100);
		mDefaultConvex = EditorPrefs.GetBool("AlphaMeshCollider_DefaultConvex", false);
		mDefaultAbsoluteColliderThickness = EditorPrefs.GetFloat("AlphaMeshCollider_DefaultAbsoluteColliderThickness", 2.0f);
	}
	
	//-------------------------------------------------------------------------
	public void WriteAllParams()
	{
		EditorPrefs.SetString("AlphaMeshCollider_DefaultColliderDirectory", mDefaultColliderDirectory);
		EditorPrefs.SetBool("AlphaMeshCollider_DefaultLiveUpdate", mDefaultLiveUpdate);
		EditorPrefs.SetInt("AlphaMeshCollider_DefaultColliderPointCount", mDefaultColliderPointCount);
		EditorPrefs.SetInt("AlphaMeshCollider_ColliderPointCountSliderMaxValue", mColliderPointCountSliderMaxValue);
		EditorPrefs.SetBool("AlphaMeshCollider_DefaultConvex", mDefaultConvex);
		EditorPrefs.SetFloat("AlphaMeshCollider_DefaultAbsoluteColliderThickness", mDefaultAbsoluteColliderThickness);
	}
}

#endif