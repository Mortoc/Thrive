using UnityEngine;
using System.Collections.Generic;

public class TouchCharacterControls : CharacterControls 
{	
	public bool swipeWasActive = false;
	float swipeThresh = 0.3f;
	float swipeVarianceThresh = 600.0f;
	
	Vector2 swipeStart = Vector2.zero;
	Vector2 swipeEnd = Vector2.zero;
	
	void Update()
	{
		if ( Input.GetMouseButton(0) && !Gui.IsMouseOver() )
		{
			if( Character.CurrentMode == MainCharacter.Mode.Walking )
				MoveToClick(Input.mousePosition);
			else if( Character.CurrentMode == MainCharacter.Mode.ObjectPlacement )
				PlaceObject( Input.mousePosition );
		}
				
		if ( Input.touchCount == 1 ) 
		{
        	ProcessSwipe();
    	}
	}
	
	void ProcessSwipe()
	{
		 if ( Input.touchCount != 1 ) 
		{
        	return;
	    }
	 
	    Touch theTouch = Input.touches[0];
	 
	    /* skip the frame if deltaPosition is zero */
	    if ( theTouch.deltaPosition == Vector2.zero ) 
		{
	        return;
	    }
	 
	    Vector2 speedVec = theTouch.deltaPosition * theTouch.deltaTime;
	    float theSpeed  = speedVec.magnitude;
	 
	    bool swipeIsActive  = ( theSpeed > swipeThresh );
	 
	    if ( swipeIsActive ) 
		{
	 
	        if ( ! swipeWasActive ) 
			{
	            swipeStart = theTouch.position;
	        }
	    }
	 
	    else 
		{
	        if ( swipeWasActive ) 
			{
	            swipeEnd = theTouch.position;
				
				if (Mathf.Abs(swipeStart.x - swipeStart.y) < swipeVarianceThresh)
				{
					//swipe up
					if (swipeEnd.y > swipeStart.y)
					{
						Character.JumpParallaxBackward();
					}
					else
					{
						Character.JumpParallaxForward();
					}
				}
	        }
	    }
	 
	    swipeWasActive = swipeIsActive;
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
