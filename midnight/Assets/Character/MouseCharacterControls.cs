using UnityEngine;
using System.Collections.Generic;

public class MouseCharacterControls : CharacterControls 
{
	private ITask _moveTask = null;
	
	void Update()
	{
		if( Input.GetMouseButtonDown(0) )
		{
			MoveToClick(Input.mousePosition);
		}
		
		if( Input.GetKeyDown(KeyCode.Space) )
		{
			JumpBackParallax();
		}
	}
	
	
	private void MoveToClick(Vector3 screenPosition)
	{
		// get level-plane position of click
		Ray clickRay = Camera.main.ScreenPointToRay(screenPosition);
		float zDistToCamera = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
		Vector3 clickPosition = clickRay.GetPoint(zDistToCamera);
		
		// move to that spot
		if( _moveTask != null )
			_moveTask.Exit();
		
		_moveTask = Scheduler.Run( MoveToClickCoroutine(clickPosition) );
	}
	
	private IEnumerator<IYieldInstruction> MoveToClickCoroutine(Vector3 target)
	{
		float targetDir = target.x < transform.position.x ? -1.0f : 1.0f;
		float frameSpeed = CharacterSpeed * Time.deltaTime;
		float frameVelocity = targetDir * frameSpeed;
		
		while( (target.x < transform.position.x ? -1.0f : 1.0f) == targetDir ) 
		{
			Translate(frameVelocity);
			yield return Yield.UntilNextFrame;
		}
	}
}
