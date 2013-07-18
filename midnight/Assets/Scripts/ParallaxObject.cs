using UnityEngine;
using System.Collections.Generic;

public class ParallaxObject : MonoBehaviour 
{
	public float MoveFactor;
	
	public Vector3 initialPosition;
	
	//set in ParallaxManager
	public Vector3 initialScale;
	
	private float cameraInitialPosition;
	
	public List<GameObject> _children = new List<GameObject>();
	
	public void AddGameObjectToLayer(GameObject go)
	{
		foreach( Renderer childRenderer in go.GetComponentsInChildren<Renderer>() )
			childRenderer.enabled = _showing;			
		
		_children.Add(go);
	}
	
	public void RemoveObjectFromLayer(GameObject go)
	{
		_children.Remove(go);
	}
	
	private bool _showing = true;
	
	public void Hide()
	{
		List<GameObject> destroyedGameObjects = new List<GameObject>();
		foreach( GameObject go in ChildrenAndMe() )
		{
			if( !go )
			{
				destroyedGameObjects.Add(go);
			}
			else
			{
				foreach( Renderer childRenderer in go.GetComponentsInChildren<Renderer>() )
					childRenderer.enabled = false;
			}
		}
		
		destroyedGameObjects.ForEach( g => _children.Remove(g) );
		
		_showing = false;
	}
	
	private IEnumerable<GameObject> ChildrenAndMe()
	{
		foreach(GameObject child in _children)
			yield return child;
		
		yield return gameObject;
	}
	
	public void Show()
	{
		List<GameObject> destroyedGameObjects = new List<GameObject>();
				
		foreach( GameObject go in ChildrenAndMe() )
		{
			if( !go )
			{
				destroyedGameObjects.Add(go);
			}
			else
			{
				foreach( Renderer childRenderer in go.GetComponentsInChildren<Renderer>() )
					childRenderer.enabled = true;
			}
		}
		
		
		destroyedGameObjects.ForEach( g => _children.Remove(g) );
		
		_showing = true;
	}
	
	
	void Start()
	{
		cameraInitialPosition = Camera.main.transform.position.x;
		transform.position = initialPosition;
	}
	
	void LateUpdate()
	{
		//see how much the camera has moved and move this object along with the camera scaled by the factor
		float cameraOffset = Camera.main.transform.position.x - cameraInitialPosition;
		//TODO : Deal with other things that are on the current ParallaxObject (turrets, enemies etc)
		Vector3 objectOffset = cameraOffset * MoveFactor * Vector3.right;
		
		Vector3 diff = transform.position - objectOffset;
		transform.position = objectOffset + new Vector3(initialPosition.x, initialPosition.y, transform.position.z);
		
		//Move all the children of the layer the same amount 
		if (objectOffset != Vector3.zero)
		{
			foreach( GameObject go in _children )
			{
				go.transform.position -= new Vector3(diff.x, 0, 0);
			}
		}
		
	}
}
