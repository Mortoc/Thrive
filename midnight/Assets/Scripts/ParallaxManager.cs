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
		if (parallaxes.Length > 0)
		{
			currentParallax = parallaxes[0];	
			currentParallaxIndex = 0;
		}
	}
	
	void Update()
	{
		if( Input.GetKeyDown(KeyCode.Space) )
		{
			ShiftForward();
		}
	}
	
	public void ShiftForward()
	{
		if (currentParallaxIndex + 1 < parallaxes.Length)
		{
			//increase the size and decrease depth of the current parallax
			
			//Debug.Log("Current level is " + currentParallaxIndex.ToString());
			for (var i = 0; i <= currentParallaxIndex; i++)
			{
				parallaxes[i].transform.localScale = new Vector3(0,0,0);
			}
			
			for (var i = currentParallaxIndex + 1; i < parallaxes.Length; i++)
			{
				parallaxes[i].transform.localScale = Vector3.Scale(parallaxes[i].transform.localScale, moveForwardScale);
				parallaxes[i].transform.position -= (Vector3.forward * depthChange);
			}
			
			currentParallaxIndex += 1;
			currentParallax = parallaxes[currentParallaxIndex];
		}
	}
	
	public void ShiftBackward()
	{
		if (currentParallaxIndex > 0)
		{
			//decrease the size and increase depth of all parallaxes
			for (var i = 0; i < parallaxes.Length; i++)
			{
				parallaxes[i].transform.localScale = Vector3.Scale(parallaxes[i].transform.localScale, moveBackwardScale);
				parallaxes[i].transform.position += (Vector3.forward * depthChange);
			}
			
			currentParallaxIndex -= 1;
			currentParallax = parallaxes[currentParallaxIndex];
		}
	}
}
