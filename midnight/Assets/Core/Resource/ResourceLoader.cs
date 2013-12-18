using UnityEngine;

using System;
using System.Collections.Generic;


namespace Thrive.Core.Resource
{
	public static class Loader
	{
		// only supports paths in the form 'resources:/path/to/resource' right now
		public static IReceipt GetPrefab(string path, Action<GameObject> onload)
		{	
			// check resources first
			GameObject loadedPrefab = (GameObject)Resources.Load(path, typeof(GameObject));
			if( loadedPrefab != null ) 
			{
				onload(loadedPrefab);
			}
			else
			{
				// check our asset server (not implemented)
				throw new NotImplementedException("only local resources are implemented at this point");
				//return new Receipt(delegate() 
				//{
					// cancel the download if it's in progress	
				//});
			}

			return new Receipt(null);

		}
	}
}