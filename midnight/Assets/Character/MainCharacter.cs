using UnityEngine;
using System.Collections.Generic;

public class MainCharacter : MonoBehaviour 
{
	public enum Mode { Walking, ObjectPlacement }
	
	public Mode CurrentMode { get; set; }
	
	void Start()
	{
		CurrentMode = Mode.Walking;
	}
}
