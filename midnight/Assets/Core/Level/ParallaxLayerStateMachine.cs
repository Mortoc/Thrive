using System;

using Thrive.Core.Enemies;
using Thrive.Core.Allies;
using Thrive.Core.PlayerCharacter;
using Thrive.Core.UI;

namespace Thrive.Core
{
	public class ParallaxLayerStateMachine : Controller
	{
		private class DefaultState : IControllerState
		{
			public void EnterState(IController owner) {}
			public void ExitState() {}
		}
		
		public ParallaxLayerStateMachine(IController parent)
			: base(new DefaultState(), parent) 
		{
			
		}
	}
}