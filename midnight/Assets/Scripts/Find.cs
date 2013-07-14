using UnityEngine;

using System;
using System.Collections.Generic;

public static class Find
{
	public static T ObjectInScene<T>() where T : UnityEngine.Object
	{
		return Component.FindObjectOfType(typeof(T)) as T;
	}
	
	public static IEnumerable<T> ObjectsInScene<T>() where T : UnityEngine.Object
	{
#if !UNITY_FLASH
		
		foreach( UnityEngine.Object obj in Component.FindObjectsOfType(typeof(T)) )
			yield return obj as T;
		
#else
		List<T> result = new List<T>();
		foreach( UnityEngine.Object obj in Component.FindObjectsOfType(typeof(T)) )
			result.Add(obj as T);
		
		return result;
#endif
	}
	
	public static T ObjectWithMatchingName<T>(string searchString) where T : UnityEngine.Component
	{
		return ObjectWithMatchingName<T>(searchString, null);
	}
	
	public static T ObjectWithMatchingName<T>(string searchString, Transform underTransform) where T : UnityEngine.Component
	{
		if( underTransform == null )
		{
			foreach(T obj in ObjectsInScene<T>())
			{
				if( obj.name.Contains(searchString) )
					return obj;
			}
		}
		else
		{
			foreach(Transform transform in underTransform.GetComponentsInChildren<Transform>())
			{
				if( transform.GetComponent<T>() && transform.name.Contains(searchString) )
					return transform.GetComponent<T>();
			}
		}
		
		return null;
	}
	
	public static IEnumerable<T> ObjectsWithMatchingName<T>(string searchString) where T : UnityEngine.Component
	{
		return ObjectsWithMatchingName<T>(searchString, null);
	}
	
	public static IEnumerable<T> ObjectsWithMatchingName<T>(string searchString, Transform underTransform) where T : UnityEngine.Component
	{
		if (underTransform == null)
		{
#if !UNITY_FLASH
			foreach (T obj in ObjectsInScene<T>())
			{
				if ( obj.name.Contains(searchString) )
					yield return obj;
			}
#else
			List<T> result = new List<T>();
			foreach (T obj in ObjectsInScene<T>())
			{
				if ( obj.name.Contains(searchString) )
					result.Add(obj);
			}
			return result;
#endif
		}
		else
		{
#if !UNITY_FLASH
			foreach (Transform transform in underTransform.GetComponentsInChildren<Transform>())
			{
				if (transform.GetComponent<T>() && transform.name.Contains(searchString))
					yield return transform.GetComponent<T>();
			}
#else
			List<T> result = new List<T>();
			foreach (Transform transform in underTransform.GetComponentsInChildren<Transform>())
			{
				if (transform.GetComponent<T>() && transform.name.Contains(searchString))
					result.Add( transform.GetComponent<T>() );
			}
			return result;
#endif
		}
	}
}

