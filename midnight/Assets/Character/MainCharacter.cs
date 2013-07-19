using UnityEngine;
using System.Collections.Generic;

public class MainCharacter : MonoBehaviour 
{
	public enum Mode { Walking, ObjectPlacement }
	public ParallaxManager _ParallaxManager;
	
	public Mode CurrentMode { get; set; }
	public float floatiness = 0.2f;
	public float floatSpeed = 3.0f;
	
	private CharacterController _controller;
	private float _zIndex = 0;
	public GameObject SelectedTurret;
	
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
	public float horizontalFriction = 0.02f;
	public float maxHorizontalAcceleration = 3.0f;
	public float maxHorizontalVelocity = 3.0f;
	
	public float jumpParallaxSpeed = 30.0f;
	public float jumpParallaxHeight = 450.0f;
	
	
	public float floatyNumber = 10.0f;
	void Start()
	{
		CurrentMode = Mode.Walking;
		_controller = GetComponent<CharacterController>();
		
		if (!audio)
		{
			gameObject.AddComponent<AudioSource>();
			audio.clip = jetpackSound;
			audio.Play();
		}
	}
	
	void Update()
	{
		//Keep her Z to the closest 100 to make up for wobble
		transform.position = new Vector3(transform.position.x, transform.position.y, _zIndex);
		
		
		if (!audio.isPlaying)
		{
			audio.Play();
		}
	}
	
	void FixedUpdate()
	{
		ApplyPhysics();
		
	}
	
	
	protected void ApplyPhysics()
	{	
				
		ApplyHorizontalPhysics();
		ApplyVerticalPhysics();
			
		_controller.Move(new Vector3(horizontalVelocity, verticalVelocity, 0));
		
	}
	
	protected void ApplyVerticalPhysics()
		
	{
		//verticalAcceleration += Mathf.Sin(Time.time * floatyNumber);
		
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
	}
		
	
	protected void ApplyHorizontalPhysics()
	{
		if (horizontalAcceleration > 0)
		{
			horizontalAcceleration -= horizontalAcceleration * horizontalFriction;
			if (horizontalAcceleration < .01)
			{
				horizontalAcceleration = 0;	
			}
		}
		else if (horizontalAcceleration < 0)
		{
			horizontalAcceleration -= horizontalAcceleration * horizontalFriction;
			if (horizontalAcceleration > -.01)
			{
				horizontalAcceleration = 0;	
			}
		}
		
		
		if (horizontalAcceleration > maxHorizontalAcceleration)
		{
			horizontalAcceleration = maxHorizontalAcceleration;	
		}
		if (horizontalAcceleration < -1.0f * maxHorizontalAcceleration)
		{
			horizontalAcceleration = -1.0f * maxHorizontalAcceleration;	
		}
		
		horizontalVelocity += horizontalAcceleration;
		if (horizontalVelocity > 0 )
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
		
		if (horizontalVelocity > maxHorizontalVelocity)
		{
			horizontalVelocity = maxHorizontalVelocity;	
		}
		if (horizontalVelocity < -1.0f * maxHorizontalVelocity)
		{
			horizontalVelocity = -1.0f * maxHorizontalVelocity;	
		}
		
		OTSprite sprite = GetComponent<OTSprite>();
		sprite.flipHorizontal = horizontalVelocity > 0.0f;

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
		horizontalAcceleration += horizontalForceFromTap;
		if (horizontalAcceleration > maxHorizontalAcceleration)	
		{
			horizontalAcceleration = maxHorizontalAcceleration;
		}
	}
	
	public void AccelerateLeft()
	{
		horizontalAcceleration -= horizontalForceFromTap;
		if (horizontalAcceleration < -1.0f * maxHorizontalAcceleration)
		{
			horizontalAcceleration = -1.0f * maxHorizontalAcceleration;
		}
	}
	
	public void JumpParallaxBackward()
	{
		_ParallaxManager.ShiftBackward();
		//change the characters Z, then make her jump
		//TODO: Grab the next parallax layer. Shift to it's Z.
		if (_zIndex - 100 >= 0)
		{
			_zIndex -= 100;
			verticalAcceleration += jumpParallaxSpeed;	
		}
	}
	
	public void JumpParallaxForward()
	{
		_ParallaxManager.ShiftForward();
		//change the characters Z, then make her jump
		//TODO: Grab the next parallax layer. Shift to it's Z.
		if (_zIndex + 100 < Find.ObjectInScene<ParallaxManager>().parallaxes.Length * 100)
		{
			_zIndex += 100;
			verticalAcceleration += jumpParallaxSpeed;	
		}
	}
	
}
