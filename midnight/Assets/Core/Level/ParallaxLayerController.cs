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
            Layers.Add(Find.ObjectWithMatchingName<Component>("_Layer1"));
            Layers.Add(Find.ObjectWithMatchingName<Component>("_Layer2"));
            Layers.Add(Find.ObjectWithMatchingName<Component>("_Layer3"));

            CurrentLayerIndex = 0;
		}

        public void NextLayer()
        {
			Debug.Log("Layer 2/3 now in background");
            var layer2Collider = Layers[1].GetComponent<Collider2D>();
			var layer3Collider = Layers[2].GetComponent<Collider2D>();

			layer2Collider.enabled = false;
			layer3Collider.enabled = false;

			/*
            CurrentLayerIndex++;
			if (CurrentLayerIndex >= Layers.Count)
			{
                CurrentLayerIndex = 0;
			}
			*/
        }

        public void PreviousLayer()
        {
			Debug.Log("Layer 2/3 now in foreground");
			var layer2Collider = Layers[1].GetComponent<Collider2D>();
			var layer3Collider = Layers[2].GetComponent<Collider2D>();
			
			layer2Collider.enabled = true;
			layer3Collider.enabled = true;
			/*
            CurrentLayerIndex--;
            if (CurrentLayerIndex < 0)
            {
                CurrentLayerIndex = Layers.Count - 1;
            }
            */
        }
	}
}