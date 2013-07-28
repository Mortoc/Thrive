using UnityEngine;
using System.Collections.Generic;

public class ParallaxManager : MonoBehaviour 
{
	public ParallaxObject[] parallaxes;
	
	private ParallaxObject currentParallax;
	public int currentParallaxIndex;
	
	public Vector3 growScale;
	public Vector3 shrinkScale;
	
	public float parallaxShiftZAmount = 100.0f;
	public float parallaxShiftYAmount = 300.0f;
	
	
	void Start()
	{
		//Log if one of the scales is 0
		if (growScale.x == 0 || growScale.x == 0 || growScale.z == 0 ||
			shrinkScale.x == 0 || shrinkScale.y == 0 || shrinkScale.z == 0)
		{
			Debug.Log("ParallaxManager scaling set to 0");	
		}
		
		if (parallaxes.Length > 0)
		{
			currentParallax = parallaxes[0];	
			currentParallaxIndex = 0;
			for (var i = 0; i < parallaxes.Length; i++)
			{
				GameObject scaler = new GameObject("Scaler - " + parallaxes[i].name);
				scaler.transform.position = parallaxes[i].initialPosition;
				scaler.transform.parent = parallaxes[i].transform.parent;
				
				parallaxes[i].transform.parent = scaler.transform;
				parallaxes[i].transform.localPosition = Vector3.zero;
				
				//shrink/grow all layers except for the initial one
				if (i < currentParallaxIndex)
				{
					//can't figure out how to raise a vector to a power, so loop to get the same effect
					for (var j = 0; j < i; j++)
					{
						parallaxes[i].transform.parent.localScale = Vector3.Scale(parallaxes[i].transform.parent.localScale, growScale);
					}
				}
				if (i > currentParallaxIndex)
				{
					//can't figure out how to raise a vector to a power, so loop to get the same effect
					for (var j = 0; j < i; j++)
					{
						parallaxes[i].transform.parent.localScale = Vector3.Scale(parallaxes[i].transform.parent.localScale, shrinkScale);
					}
				}
			}
		}
	}
	

	//going from layer 0 towards layer 1
	public void ShiftForward()
	{	
		if (currentParallaxIndex + 1 < parallaxes.Length)
		{
			Camera.main.transform.position += Vector3.forward * parallaxShiftZAmount;
			Camera.main.transform.position += Vector3.up * parallaxShiftYAmount;
			
			for (var i = 0; i < parallaxes.Length; i++)
			{
				parallaxes[i].transform.parent.localScale = Vector3.Scale(parallaxes[i].transform.parent.localScale, growScale);
			}
			
			currentParallaxIndex += 1;
		}
	}
	
	//going from layer 1 towards layer 0
	public void ShiftBackward()
	{
		if (currentParallaxIndex - 1 >= 0)
		{
			Camera.main.transform.position += Vector3.back * parallaxShiftZAmount;
			Camera.main.transform.position += Vector3.down * parallaxShiftYAmount;
			
			for (var i = 0; i < parallaxes.Length; i++)
			{
				parallaxes[i].transform.parent.localScale = Vector3.Scale(parallaxes[i].transform.parent.localScale, shrinkScale);
			}
			
			currentParallaxIndex -= 1;
		}
	}
}
