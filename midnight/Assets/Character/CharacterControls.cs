using UnityEngine;
using System.Collections.Generic;

public abstract class CharacterControls : MonoBehaviour 
{
	public float CharacterSpeed = 1.0f;
	
	protected MainCharacter Character { get; set; }
	protected SceneGui Gui { get; set; }
	
	protected virtual void Start()
	{
		Character = Find.ObjectInScene<MainCharacter>();
		Gui = Find.ObjectInScene<SceneGui>();
	}
	
	
	// Move the character
	protected void Translate(float translation)
	{
		CharacterController controller = GetComponent<CharacterController>();
		controller.Move( new Vector3(translation, 0, 0));
		
		PostTranslate(translation);
	}
	
	protected virtual void PostTranslate(float translation)
	{ 
		OTSprite sprite = GetComponent<OTSprite>();
		sprite.flipHorizontal = translation > 0.0f;
	}
	
}
