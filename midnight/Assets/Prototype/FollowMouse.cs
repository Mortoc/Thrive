using UnityEngine;
using System.Collections;

public class FollowMouse : MonoBehaviour 
{

	void Update () 
	{
		Vector3 point = Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(1.0f);
		transform.position = new Vector3(point.x, point.y, transform.position.z);
	}
}