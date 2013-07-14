using UnityEngine;
using System.Collections.Generic;

public abstract class CharacterControls : MonoBehaviour 
{
	public float CharacterSpeed = 1.0f;
	
	
	// Move the character (negative left, positive right)
	protected void Translate(float amount)
	{
		CharacterController controller = GetComponent<CharacterController>();
		controller.SimpleMove( Vector3.right * amount );
	}
	
	protected void ForceTranslate(float amount)
	{
		CharacterController controller = GetComponent<CharacterController>();
		controller.Move( Vector3.right * amount );
	}
	
	protected void JumpBackParallax()
	{
		
	}
	
	protected void JumpForwardParallax()
	{
		
	}
	
}
