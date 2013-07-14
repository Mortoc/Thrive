using UnityEngine;
using System;
using System.Collections.Generic;

public class SceneGui : MonoBehaviour 
{
	public GUIStyle PlaceObjectsButton;
	private GUIContent _placeObjectsContent = new GUIContent(" ", "PlaceObjectsButton");
	public GUIStyle moveButton;
	private GUIContent _moveContent = new GUIContent(" ", "MoveButton");
	
	private bool _mouseIsOver = false;
	private string _previousTooltip = "";
	
	public Rect[] DeadZones;
	
	private MainCharacter _mainCharacter;
	
	void Start()
	{
		_mainCharacter = Find.ObjectInScene<MainCharacter>();
	}
	
	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0.0f, 0.0f, Screen.width, Screen.height));
		
		GUILayout.BeginHorizontal();
		
		if( GUILayout.Toggle( _mainCharacter.CurrentMode == MainCharacter.Mode.ObjectPlacement, _placeObjectsContent, PlaceObjectsButton ) )
		{
			_mainCharacter.CurrentMode = MainCharacter.Mode.ObjectPlacement;
		}
		
		if( GUILayout.Toggle(_mainCharacter.CurrentMode == MainCharacter.Mode.Walking, _moveContent, moveButton) )
		{
			_mainCharacter.CurrentMode = MainCharacter.Mode.Walking;
		}
		
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
		
		if (Event.current.type == EventType.Repaint && GUI.tooltip != _previousTooltip) 
		{
		    if (_previousTooltip != "") 
				_mouseIsOver = false;
		    
		 
		    if (GUI.tooltip != "") 
				_mouseIsOver = true;
		    
		 
		    _previousTooltip = GUI.tooltip; 
		}
	}
		
	public bool IsMouseOver()
	{
		return _mouseIsOver;
	}
}
