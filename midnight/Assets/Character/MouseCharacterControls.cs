using UnityEngine;
using System.Collections.Generic;

public class MouseCharacterControls : CharacterControls 
{
	private ITask _moveTask = null;
	
	void Update()
	{
		if ( Input.GetMouseButton(0) && !Gui.IsMouseOver() )
		{
			MoveToClick(Input.mousePosition);
		}
		
		if( Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow ) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) )
		{
			Character.JumpParallax();
		}
		
	}
	
	
	private void MoveToClick(Vector3 screenPosition)
	{
		
		if( Character.CurrentMode != MainCharacter.Mode.Walking )
			return; // ignore, the character isn't walking now
		
		if( screenPosition.x > (Screen.width - 1) || screenPosition.x < 1 ||
			screenPosition.y > (Screen.height - 1) || screenPosition.y < 1 )
			return; // ignore it, click was outside the game window
		
		// get level-plane position of click
		Ray clickRay = Camera.main.ScreenPointToRay(screenPosition);
		float zDistToCamera = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
		Vector3 clickPosition = clickRay.GetPoint(zDistToCamera);
		
		
		//float frameSpeed = CharacterSpeed * Time.deltaTime;
		float dir = transform.position.x - clickPosition.x > 0 ? -1.0f : 1.0f;
		
		if (dir < 0)
		{
			Character.AccelerateLeft();	
		}
		else
		{
			Character.AccelerateRight();
		}
		Character.AccelerateUp();
	}
	

}
