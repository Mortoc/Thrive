using UnityEngine;
using System.Collections.Generic;

public class ParallaxObject : MonoBehaviour 
{
	public float MoveFactor;
	public float scale;
	public float depth;
	
	private float cameraInitialPosition;
	private Vector3 initialPosition;
	
	
	
	void Start()
	{
		//Record the initial offset from this object to the camera
		//Record the initial position of the camera
		//Record our initial position
		cameraInitialPosition = Camera.main.transform.position.x;
		initialPosition = transform.position;
	}
	
	void LateUpdate()
	{
		//see how much the camera has moved and move this object along with the camera scaled by the factor
		float cameraOffset = Camera.main.transform.position.x - cameraInitialPosition;
		Vector3 objectOffset = cameraOffset * MoveFactor * Vector3.right;
		
		transform.position = objectOffset + initialPosition;
	}
}
