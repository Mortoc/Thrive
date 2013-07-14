using UnityEngine;
using System.Collections.Generic;

public abstract class CharacterControls : MonoBehaviour 
{
	public float CharacterSpeed = 1.0f;
	
	private int _groundLayerMask;
	protected MainCharacter Character { get; set; }
	protected SceneGui Gui { get; set; }
	
	protected virtual void Start()
	{
		Character = Find.ObjectInScene<MainCharacter>();
		Gui = Find.ObjectInScene<SceneGui>();
		
		_groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
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
	
	protected virtual void PlaceObject(Vector3 position)
	{
		// Get the world space point at character depth
		float charDepth = transform.position.z - Camera.main.transform.position.z;
		
		// Get the scene position
		Vector3 worldPosition = Camera.main.ScreenPointToRay(position).GetPoint(charDepth);
		
		// Shoot a ray down to see if we hit ground
		RaycastHit rh;
		if( Physics.Raycast(new Ray(worldPosition, Vector3.down), out rh, Mathf.Infinity, _groundLayerMask) )
		{
			GameObject turret = (GameObject)Instantiate(Character.SelectedTurret);
			turret.transform.position = rh.point;
			turret.transform.up = rh.normal;
			
			Character.CurrentMode = MainCharacter.Mode.Walking;
		}		
	}
	
}
