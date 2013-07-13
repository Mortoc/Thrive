using UnityEngine;
using UnityEditor;
using System.Collections;

//-------------------------------------------------------------------------
/// <summary>
/// Editor window for the AlphaMeshCollider preference values.
/// </summary>
public class EditorScriptAlphaMeshColliderPreferencesWindow : EditorWindow {
	
	GUIContent mDefaultColliderDirectoryLabel = new GUIContent("Collider Directory", "Set the default output directory for generated collider mesh files.");
	GUIContent mDefaultLiveUpdateLabel = new GUIContent("Live Update", "Recalculate the collider mesh when changing parameters in the inspector.");
	GUIContent mDefaultColliderPointCountLabel = new GUIContent("Outline Vertex Count", "Default point count of the collider shape.");
	GUIContent mColliderPointCountSliderMaxValueLabel = new GUIContent("Vertex Count Slider Max", "Maximum value of the outline vertex count slider.");
	GUIContent mDefaultColliderThicknessLabel = new GUIContent("Z-Thickness", "Default thickness of a collider.");
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/Collider Preferences", false, 10000)]
	static void ColliderPreferences() {
		
		// Get existing open window or if none, make a new one:
		EditorScriptAlphaMeshColliderPreferencesWindow window = EditorWindow.GetWindow<EditorScriptAlphaMeshColliderPreferencesWindow>();
		window.title = "Default Values";
    }
	
	//-------------------------------------------------------------------------
	void OnGUI()
	{
		EditorGUIUtility.LookLikeControls(150.0f);
		
		AlphaMeshColliderPreferences.Instance.DefaultColliderDirectory = EditorGUILayout.TextField(mDefaultColliderDirectoryLabel, AlphaMeshColliderPreferences.Instance.DefaultColliderDirectory);
		AlphaMeshColliderPreferences.Instance.DefaultLiveUpdate = EditorGUILayout.Toggle(mDefaultLiveUpdateLabel, AlphaMeshColliderPreferences.Instance.DefaultLiveUpdate);
		AlphaMeshColliderPreferences.Instance.DefaultColliderPointCount = EditorGUILayout.IntField(mDefaultColliderPointCountLabel, AlphaMeshColliderPreferences.Instance.DefaultColliderPointCount);
		AlphaMeshColliderPreferences.Instance.ColliderPointCountSliderMaxValue = EditorGUILayout.IntField(mColliderPointCountSliderMaxValueLabel, AlphaMeshColliderPreferences.Instance.ColliderPointCountSliderMaxValue);
		AlphaMeshColliderPreferences.Instance.DefaultAbsoluteColliderThickness = EditorGUILayout.FloatField(mDefaultColliderThicknessLabel, AlphaMeshColliderPreferences.Instance.DefaultAbsoluteColliderThickness);	
	}
}
