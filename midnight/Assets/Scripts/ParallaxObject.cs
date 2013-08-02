using UnityEngine;
using System.Collections.Generic;

public class ParallaxObject : MonoBehaviour 
{
	public float MoveFactor;
	public Vector3 positionToStart;
	
	private float cameraInitialPosition;
	private bool _showing = true;
	public List<GameObject> _children = new List<GameObject>();
	
	public void AddGameObjectToLayer(GameObject go)
	{
		go.transform.parent = transform.parent;
		
		foreach( Renderer childRenderer in go.GetComponentsInChildren<Renderer>() )
			childRenderer.enabled = _showing;			
		
		_children.Add(go);
	}
	
	public void RemoveObjectFromLayer(GameObject go)
	{
		_children.Remove(go);
	}
	
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
	}
	
	void LateUpdate()
	{
		//see how much the camera has moved and move this object along with the camera scaled by the factor
		float cameraOffset = Camera.main.transform.position.x - cameraInitialPosition;
		Vector3 objectOffset = cameraOffset * MoveFactor * Vector3.right;
		
		Vector3 diff = objectOffset - transform.parent.position;
		
		if (objectOffset != Vector3.zero)
		{
			transform.parent.position += new Vector3(diff.x, 0, 0);
		}
	}
}
