using UnityEngine;
using System.Collections;

public class FollowMouse : MonoBehaviour {

	void Update () {
		transform.position = Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(9.5f);
	}
}
