using UnityEngine;
using System.Collections.Generic;

public class MainCharacter : MonoBehaviour 
{
	public enum Mode { Walking, ObjectPlacement }
	
	public Mode CurrentMode { get; set; }
	public float floatiness = 0.2f;
	public float floatSpeed = 3.0f;
	
	private float _initialHeight;
	private CharacterController _controller;
	
	
	public AudioClip jetpackSound;
	
	public float verticalForceFromTap = 1.0f;
	public float verticalVelocity = 0.0f;
	public float verticalAcceleration = 0.0f;
	public float maxVerticalAcceleration = 3.0f;
	public float maxVerticalVelocity = 3.0f;
	public float gravity = 0.2f;
	
	
	public float horizontalForceFromTap = 1.0f;
	public float horizontalVelocity = 0.0f;
	public float horizontalAcceleration = 0.0f;
	public float horizontalFriction = 0.1f;
	public float maxHorizontalAcceleration = 3.0f;
	public float maxHorizontalVelocity = 3.0f;
	
	public float jumpParallaxSpeed = 10.0f;
	
	public float maxHeight = 500.0f;
	
	void Start()
	{
		CurrentMode = Mode.Walking;
		_controller = GetComponent<CharacterController>();
		
		_initialHeight = _controller.height;
		
		if (!audio)
		{
			gameObject.AddComponent<AudioSource>();
			audio.clip = jetpackSound;
			audio.Play();
		}
	}
	
	void Update()
	{
		//make sure her Z is 0
		transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
		
		
		if (!audio.isPlaying)
		{
			Debug.Log("test");
			audio.Play();
		}
		
		//hover
		/*
		float t = (0.5f * Mathf.Sin (Time.time * floatSpeed)) + 0.5f;
		_controller.height = Mathf.Lerp(1.0f - floatiness, 1.0f + floatiness, t) * _initialHeight;
		*/
	}
	
	void FixedUpdate()
	{
		verticalAcceleration -= gravity;
		if (verticalAcceleration < -1.0f * maxVerticalAcceleration)
		{
			verticalAcceleration = -1.0f * maxVerticalAcceleration;	
		}
		
		verticalVelocity += verticalAcceleration;	
		
		if (verticalVelocity > maxVerticalVelocity)
		{
			verticalVelocity = maxVerticalVelocity;	
		}
		if (verticalVelocity < -1.0f * maxVerticalVelocity)
		{
			verticalVelocity = -1.0f * maxVerticalVelocity;	
		}
		
		
		if (horizontalVelocity > 0)
		{
			horizontalVelocity -= horizontalFriction;
			if (horizontalVelocity < 0)
			{
				horizontalVelocity = 0;	
			}
		}
		else if (horizontalVelocity < 0)
		{
			horizontalVelocity += horizontalFriction;
			if (horizontalVelocity > 0)
			{
				horizontalVelocity = 0;	
			}
		}
		
		OTSprite sprite = GetComponent<OTSprite>();
		sprite.flipHorizontal = horizontalVelocity > 0.0f;
		
		
		if (transform.position.y + verticalVelocity > maxHeight)
		{
			_controller.Move(new Vector3(horizontalVelocity, 0, 0));
		}
		else
		{
			_controller.Move(new Vector3(horizontalVelocity, verticalVelocity, 0));
		}
	}
	
	public void AccelerateUp()
	{
		verticalAcceleration += verticalForceFromTap;
		
		if (verticalAcceleration > maxVerticalAcceleration)
		{
			verticalAcceleration = maxVerticalAcceleration;	
		}
	}
	
	public void AccelerateRight()
	{
		horizontalVelocity += horizontalForceFromTap;
		if (horizontalVelocity > maxHorizontalVelocity)	
		{
			horizontalVelocity = maxHorizontalVelocity;
		}
	}
	
	public void AccelerateLeft()
	{
		horizontalVelocity -= horizontalForceFromTap;
		if (horizontalVelocity < -1.0f * maxHorizontalVelocity)
		{
			horizontalVelocity = -1.0f * maxHorizontalVelocity;
		}
	}
	
	public void JumpParallax()
	{
		verticalAcceleration += jumpParallaxSpeed;	
	}
	
}
