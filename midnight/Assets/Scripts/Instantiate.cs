using UnityEngine;
using System;
using System.Collections.Generic;

public static class Instantiate 
{
	// Instead of:
	// GameObject instance = (GameObject)GameObject.Instantiate(prefab);
	public static GameObject Prefab(GameObject prefab)
	{
		return (GameObject)GameObject.Instantiate(prefab);
	}
}
