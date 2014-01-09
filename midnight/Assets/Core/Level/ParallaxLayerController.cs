using System;
using System.Collections.Generic;
using UnityEngine;
using Thrive.Core.Enemies;
using Thrive.Core.Allies;
using Thrive.Core.PlayerCharacter;
using Thrive.Core.UI;

namespace Thrive.Core
{
	public class ParallaxLayerController : Controller
	{
		private List<Component> Layers;
		public int CurrentLayerIndex;

		private class DefaultState : IControllerState
		{
			public void EnterState(IController owner) {}
			public void ExitState() {}
		}
		
		public ParallaxLayerController(IController parent)
			: base(new DefaultState(), parent) 
		{
            Layers = new List<Component>();

            int layerCount = 1;
            while (true)
            {
                var tempLayer = Find.ObjectWithMatchingName<Component>("_Layer" + layerCount.ToString());
                if (tempLayer != null)
                {
                    Layers.Add(tempLayer);
                }
                else
                {
                    break;
                }

				layerCount++;
            }
            
            CurrentLayerIndex = 0;
		}

        public void NextLayer()
        {
            var tempLayer = Layers[Layers.Count - 1].gameObject.layer;
            var tempSortingOrder = Layers[Layers.Count - 1].gameObject.GetComponent<SpriteRenderer>().sortingOrder;

            for (var i = Layers.Count - 1; i > 0; i--)
            {
                Layers[i].gameObject.layer = Layers[i - 1].gameObject.layer;
                Layers[i].gameObject.GetComponent<SpriteRenderer>().sortingOrder = Layers[i - 1].gameObject.GetComponent<SpriteRenderer>().sortingOrder;
            }

            Layers[0].gameObject.layer = tempLayer;
            Layers[0].gameObject.GetComponent<SpriteRenderer>().sortingOrder = tempSortingOrder;

			
            CurrentLayerIndex++;
			if (CurrentLayerIndex >= Layers.Count)
			{
                CurrentLayerIndex = 0;
			}
        }

        public void PreviousLayer()
        {
            var tempLayer = Layers[0].gameObject.layer;
            var tempSortingOrder = Layers[0].gameObject.GetComponent<SpriteRenderer>().sortingOrder;

            for (var i = 0; i < Layers.Count - 1; i++)
            {
                Layers[i].gameObject.layer = Layers[i + 1].gameObject.layer;
                Layers[i].gameObject.GetComponent<SpriteRenderer>().sortingOrder = Layers[i + 1].gameObject.GetComponent<SpriteRenderer>().sortingOrder;
            }

            Layers[Layers.Count - 1].gameObject.layer = tempLayer;
            Layers[Layers.Count - 1].gameObject.GetComponent<SpriteRenderer>().sortingOrder = tempSortingOrder;

            CurrentLayerIndex--;
            if (CurrentLayerIndex < 0)
            {
                CurrentLayerIndex = Layers.Count - 1;
            }
        }
	}
}