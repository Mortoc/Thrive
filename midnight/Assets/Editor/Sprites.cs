using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace Thrive.Tools
{
	public class Sprites : EditorWindow 
	{
		[MenuItem("Window/Thrive Sprites")]
		public static void ShowWindow()
		{
        	EditorWindow.GetWindow(typeof(Sprites));
		}
		
		
		private Texture2D _texture;
		private int _width = 0;
		private int _height = 0;
		private bool _generateCollider = true;
		
		void OnGUI()
		{
			EditorGUILayout.BeginVertical();
			
			GUILayout.Label("Sprite Texture:");
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			
			Texture originalTexture = _texture;
			_texture = (Texture2D)EditorGUILayout.ObjectField(_texture, typeof(Texture2D), GUILayout.Width(128.0f), GUILayout.Height (128.0f));
			
			if( _texture != originalTexture )
			{
				_width = _texture.width;
				_height = _texture.height;
			}
			
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.IntField("Width: ", _width);
			EditorGUILayout.IntField("Height: ", _height);
			
			_generateCollider = EditorGUILayout.Toggle("Generate Collider", _generateCollider);
			
			GUILayout.FlexibleSpace();
			
			if( _texture && GUILayout.Button("Generate") )
			{
				GenerateSprite();
			}
			
			EditorGUILayout.EndVertical();
		}
		
		private void GenerateSprite()
		{
			var sprite = new GameObject("Sprite: " + _texture.name);
			
			var meshFilter = sprite.AddComponent<MeshFilter>();
			var mesh = new Mesh();
			
			mesh.vertices = new Vector3[]{
				new Vector3(_width * -0.5f, _height * 0.5f, 0.0f),
				new Vector3(_width * 0.5f, _height * 0.5f, 0.0f),
				new Vector3(_width * 0.5f, _height * -0.5f, 0.0f),
				new Vector3(_width * -0.5f, _height * -0.5f, 0.0f)
			};
			
			mesh.normals = new Vector3[]{
				Vector3.forward,
				Vector3.forward,
				Vector3.forward,
				Vector3.forward
			};
			
			mesh.uv = new Vector2[]{
				new Vector2(0.0f, 1.0f),
				new Vector2(1.0f, 1.0f),
				new Vector2(1.0f, 0.0f),
				new Vector2(0.0f, 0.0f)
			};
			
			mesh.triangles = new int[]{
				0, 3, 1,
				1, 3, 2
			};
			
			mesh.name = sprite.name;
			meshFilter.mesh = mesh;
			
			var renderer = sprite.AddComponent<MeshRenderer>();
			var material = new Material(Shader.Find ("Unlit/Transparent"));
			material.mainTexture = _texture;
			renderer.material = material;
			
			sprite.transform.position = Vector3.zero;
			sprite.transform.rotation = Quaternion.identity;
			sprite.transform.localScale = Vector3.one;
			
			if( _generateCollider )
			{
				AlphaMeshCollider alphaCollider = sprite.AddComponent<AlphaMeshCollider>();
				alphaCollider.mThickness = 100;
				alphaCollider.CustomTex = _texture;
			}
		}
	}
}
