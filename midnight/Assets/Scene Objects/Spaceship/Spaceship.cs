using UnityEngine;
using System.Collections.Generic;

public class Spaceship : MonoBehaviour 
{
	public int HitPoints = 10000;
	private bool _destroyed = false;
	
	public bool IsDestroyed()
	{
		return _destroyed;
	}
	
	public void TakeDamage(int damage)
	{
		HitPoints -= damage;
		
		if( HitPoints <= 0 )
		{
			ShipDestroyed();
		}
	}
	
	void Update()
	{
		transform.position += Vector3.up * Mathf.Sin(Time.time * 2.0f) * 0.5f;
	}
	
	private void ShipDestroyed()
	{
		_destroyed = true;
		renderer.enabled = false;
	}	
	
	void OnGUI()
	{
		if( _destroyed )
		{
			GUI.Window(405, new Rect(
				Screen.width * 0.35f,
				Screen.height * 0.4f,
				Screen.width * 0.3f,
				Screen.height * 0.2f), 
				LoseWindow,
				"THE END"
			);
		}
	}
	
	private void LoseWindow(int id)
	{
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		GUILayout.Label("YOU LOSE!");
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		if( GUILayout.Button("Retry Level?") )
		{
			Application.LoadLevel(Application.loadedLevelName);
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
	}
}
