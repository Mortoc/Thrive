using UnityEngine;
using System.Collections.Generic;

public abstract class CharacterControls : MonoBehaviour 
{
	public float CharacterSpeed = 1.0f;
	
	
	// Move the character (negative left, positive right)
	public void Translate(float amount)
	{
		CharacterController controller = GetComponent<CharacterController>();
		Vector3 translation = Vector3.right * amount;
		controller.SimpleMove( translation );
		
		PostTranslate(translation);
	}
	
	public void ForceTranslate(float amount)
	{
		CharacterController controller = GetComponent<CharacterController>();
		Vector3 translation = Vector3.right * amount;
		controller.Move( translation );
		
		PostTranslate(translation);
	}
	
	protected virtual void PostTranslate(Vector3 translation)
	{
		OTSprite sprite = GetComponent<OTSprite>();
		sprite.flipHorizontal = translation.x > 0.0f;
	}
	
	protected void JumpBackParallax()
	{
		
	}
}
