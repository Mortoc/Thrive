using UnityEngine;
using System.Collections.Generic;

public class CameraControls : MonoBehaviour 
{
	// percent of the screen the character can be in without the camera moving
	public float CharacterHotspot = 0.46f;
	
	public float AccelerationPower = 10.0f;
	public float DecelerationPower = 50.0f;
	private float _velocity = 0.0f;
	
	
	void Update()
	{
		float charMin = (1.0f - CharacterHotspot) * 0.5f;
		float charMax = charMin + CharacterHotspot;
		
		MainCharacter character = Find.ObjectInScene<MainCharacter>();
		
		Vector3 screenPosition = camera.WorldToViewportPoint(character.transform.position);
		
		if( screenPosition.x < charMin )
		{
			_velocity -= AccelerationPower * Time.deltaTime;
		}
		else if( screenPosition.x > charMax )
		{
			_velocity += AccelerationPower * Time.deltaTime;
		}
		else // Decelerating
		{
			if( _velocity > 0.0f )
				_velocity = Mathf.Clamp(_velocity - (DecelerationPower * Time.deltaTime), 0.0f, _velocity);
			else
				_velocity = Mathf.Clamp(_velocity + (DecelerationPower * Time.deltaTime), _velocity, 0.0f);
		}
		
		
		transform.Translate(Vector3.right * _velocity);
	}
}
