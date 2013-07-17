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
		
		//Make sure camera is on correct Z
		Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -1);
		
		if (parallaxes.Length > 0)
		{
			currentParallax = parallaxes[0];	
			currentParallaxIndex = 0;
		}
		
		//scale down layers
		if (parallaxes.Length > 1)
		{
			for (var i = 1; i < parallaxes.Length; i++)
			{
				for (var j = 0; j < i; j++)
				{
					parallaxes[i].transform.localScale = Vector3.Scale(parallaxes[i].transform.localScale, shrinkScale);	
				}
				
			}
		}
	}
	
	void Update()
	{
	
		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
		{
			ShiftForward();
		}
		
		if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
		{
			ShiftBackward();
		}
	}
	
	//going from layer 0 towards layer 1
	public void ShiftForward()
	{
	
		Camera.main.transform.position += Vector3.forward * parallaxShiftZAmount;
		Camera.main.transform.position += Vector3.up * parallaxShiftYAmount;
		
		if (currentParallaxIndex + 1 < parallaxes.Length)
		{
			for (var i = 0; i < parallaxes.Length; i++)
			{
				parallaxes[i].transform.localScale = Vector3.Scale(parallaxes[i].transform.localScale, growScale);
			}
			
			currentParallaxIndex += 1;
		}
	}
	
	public void ShiftBackward()
	{
		Camera.main.transform.position += Vector3.back * parallaxShiftZAmount;
		Camera.main.transform.position += Vector3.down * parallaxShiftYAmount;
		if (currentParallaxIndex - 1 >= 0)
		{
			for (var i = 0; i < parallaxes.Length; i++)
			{
				parallaxes[i].transform.localScale = Vector3.Scale(parallaxes[i].transform.localScale, shrinkScale);
			}
			
			currentParallaxIndex -= 1;
		}
	}
}
