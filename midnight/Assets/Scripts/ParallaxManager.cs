using UnityEngine;
using System.Collections.Generic;

public class ParallaxManager : MonoBehaviour 
{
	public ParallaxObject[] parallaxes;
	
	private ParallaxObject currentParallax;
	public int currentParallaxIndex;
	
	public Vector3 growScale = Vector3.one;
	public Vector3 shrinkScale = Vector3.one;
	
	public float parallaxShiftZAmount = 100.0f;
	public float parallaxShiftYAmount = 300.0f;
	
	
	void Start()
	{
		if (growScale != Vector3.zero && shrinkScale == Vector3.zero)
		{
			shrinkScale = new Vector3(1.0f/growScale.x, 1.0f/growScale.y, 1.0f/growScale.z);
		}
		else if (growScale == Vector3.zero && shrinkScale != Vector3.zero)
		{
			growScale = new Vector3(1.0f/shrinkScale.x, 1.0f/shrinkScale.y, 1.0f/shrinkScale.z);
		}
		
		if (parallaxes.Length > 0)
		{
			currentParallax = parallaxes[0];	
			currentParallaxIndex = 0;
			for (var i = 0; i < parallaxes.Length; i++)
			{
				//Create a parent object to do all the scaling.
				GameObject scalingParent = new GameObject("scalingParent - " + parallaxes[i].name);
				scalingParent.transform.position = parallaxes[i].positionToStart;
				scalingParent.transform.parent = parallaxes[i].transform.parent;
				parallaxes[i].transform.parent = scalingParent.transform;
				//Unity is defaulting world position to 0. We want localPosition to be 0.
				parallaxes[i].transform.localPosition = Vector3.zero;
				
				//shrink/grow all layers except for the initial one
				if (i < currentParallaxIndex)
				{
					for (var j = 0; j < i; j++)
					{
						parallaxes[i].transform.parent.localScale = Vector3.Scale(parallaxes[i].transform.parent.localScale, growScale);
					}
				}
				if (i > currentParallaxIndex)
				{
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
