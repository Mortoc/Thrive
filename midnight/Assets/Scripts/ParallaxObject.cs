using UnityEngine;
using System.Collections.Generic;

public class ParallaxObject : MonoBehaviour 
{
	public float MoveFactor;
	
	public float scale;
	public Vector3 initialPosition;
	public Vector3 initialScale;
	
	private float cameraInitialPosition;
	
	
	
	
	void Start()
	{
		cameraInitialPosition = Camera.main.transform.position.x;
		transform.position = initialPosition;
	}
	
	void LateUpdate()
	{
		//see how much the camera has moved and move this object along with the camera scaled by the factor
		float cameraOffset = Camera.main.transform.position.x - cameraInitialPosition;
		Vector3 objectOffset = cameraOffset * MoveFactor * Vector3.right;
		
		//maintain current Z position
		transform.position = objectOffset + new Vector3(initialPosition.x, initialPosition.y, transform.position.z);
	}
}
