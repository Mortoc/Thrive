using UnityEngine;
using System;
using System.Collections.Generic;

public class Level : MonoBehaviour 
{
	[Serializable]
	public class ParallaxLayer
	{
		public GameObject _root;
		public float _parallaxFactor;
	}
	
	public ParallaxLayer[] _terrain;
	
	public Camera _levelCamera;
}
