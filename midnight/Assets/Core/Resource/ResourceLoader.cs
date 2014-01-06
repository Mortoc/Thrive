using UnityEngine;

using System;
using System.Collections.Generic;


namespace Thrive.Core.Resource
{
	public static class Loader
	{
		public static IReceipt GetResource<T>(string path, Action<T> onLoad) where T : UnityEngine.Object
		{
			// check resources first
			T loadedPrefab = (T)Resources.Load(path, typeof(T));
			if( loadedPrefab != null ) 
			{
				onLoad(loadedPrefab);
			}
			else
			{
				throw new Exception("Asset not found:" + path);
			}
			return new Receipt(null);
		}

		// only supports paths in the form 'resources:/path/to/resource' right now
		public static IReceipt GetPrefab(string path, Action<GameObject> onLoad)
		{
			return Loader.GetResource<GameObject>(path, onLoad);
		}

		public static IReceipt GetConfig(string levelName, Action<string> onLoad)
		{
			return Loader.GetResource<TextAsset>(
				levelName + "/config", 
				asset => onLoad(asset.text)
			);
		}
	}
}