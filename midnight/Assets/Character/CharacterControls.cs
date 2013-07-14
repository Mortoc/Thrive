using UnityEngine;
using System.Collections.Generic;

public abstract class CharacterControls : MonoBehaviour 
{
	public float CharacterSpeed = 1.0f;
	
	protected MainCharacter Character { get; set; }
	protected SceneGui Gui { get; set; }
	public float jumpParallaxHeight = 100.0f;
	
	protected virtual void Start()
	{
		Character = Find.ObjectInScene<MainCharacter>();
		Gui = Find.ObjectInScene<SceneGui>();
	}
	
	
	// Move the character (negative left, positive right)
	protected void Translate(Vector2 translation)
	{
		CharacterController controller = GetComponent<CharacterController>();
		controller.SimpleMove( translation );
	
		PostTranslate(translation);
	}
	
	protected void ForceTranslate(Vector2 translation)
	{
		CharacterController controller = GetComponent<CharacterController>();
		controller.Move( translation );
		
		PostTranslate(translation);
	}
	
	protected virtual void PostTranslate(Vector3 translation)
	{
		OTSprite sprite = GetComponent<OTSprite>();
		sprite.flipHorizontal = translation.x > 0.0f;
	}
	
	
	protected void JumpParallax()
	{
		Vector3 target = transform.position + (Vector3.up * jumpParallaxHeight);
		Scheduler.Run(JumpCoroutine(target));
	}
	
	private IEnumerator<IYieldInstruction> JumpCoroutine(Vector3 target)
	{
		float frameSpeed = CharacterSpeed * Time.deltaTime;
		float frameVelocity = frameSpeed;
		
		while( (target.y > transform.position.y ) )
		{
			Translate(Vector2.up * frameVelocity);
			yield return Yield.UntilNextFrame;
		}
	}
}
