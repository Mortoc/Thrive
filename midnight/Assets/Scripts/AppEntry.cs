using UnityEngine;
using System;
using System.Collections.Generic;

public class AppEntry : MonoBehaviour 
{
	void Awake()
	{
		GameObject.DontDestroyOnLoad(gameObject);

		Thrive.Core.Game.InitializeGame();
	}
}
