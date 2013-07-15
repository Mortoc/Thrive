using UnityEngine;
using System.Collections.Generic;

public class ParallaxManager : MonoBehaviour 
{
	public ParallaxObject[] parallaxes;
	
	private ParallaxObject currentParallax;
	private int currentParallaxIndex;
	
	public Vector3 moveForwardScale;
	public Vector3 moveBackwardScale;
	
	public float depthChange;
	
	void Start()
	{
		Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -1);
		
		if (parallaxes.Length > 0)
		{
			currentParallax = parallaxes[0];	
			currentParallaxIndex = 0;
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
	
	public void ShiftForward()
	{
	
		Camera.main.transform.position += Vector3.forward * 100.0f;

		
		/*
		if (currentParallaxIndex + 1 < parallaxes.Length)
		{
			//increase the size and decrease depth of the current parallax
			
			for (var i = 0; i <= currentParallaxIndex; i++)
			{				
				parallaxes[i].Hide();
			}
			
			for (var i = currentParallaxIndex + 1; i < parallaxes.Length; i++)
			{
				parallaxes[i].transform.localScale = Vector3.Scale(parallaxes[i].transform.localScale, moveForwardScale);
				parallaxes[i].transform.position -= (Vector3.forward * depthChange);
				parallaxes[i].Show();
			}
			
			currentParallaxIndex += 1;
			
			currentParallax = parallaxes[currentParallaxIndex];
			currentParallax.Show();
		}
		*/
	}
	
	public void ShiftBackward()
	{
		Camera.main.transform.position += Vector3.back * 100.0f;
		/*
		if (currentParallaxIndex > 0)
		{
			//decrease the size and increase depth of all parallaxes
			for (var i = currentParallaxIndex; i < parallaxes.Length; i++)
			{
				parallaxes[i].transform.localScale = Vector3.Scale(parallaxes[i].transform.localScale, moveBackwardScale);
				parallaxes[i].transform.position += (Vector3.forward * depthChange);
			}	
			
			currentParallaxIndex -= 1;
			
			currentParallax = parallaxes[currentParallaxIndex];
			currentParallax.Show();
			
			currentParallax.transform.position = new Vector3(currentParallax.transform.position.x, currentParallax.transform.position.y, 0);
		}
		*/
	}
}
