using System;

using Thrive.Core.Enemies;
using Thrive.Core.Allies;
using Thrive.Core.PlayerCharacter;
using Thrive.Core.UI;

namespace Thrive.Core
{
	public class ParallaxLayerStateMachine : StateMachine
	{
		private class DefaultState : IState 
		{
			public void EnterState(IStateMachine owner) {}
			public void ExitState() {}
		}
		
		public ParallaxLayerStateMachine(IStateMachine parent)
			: base(new DefaultState(), parent) 
		{
			
		}
	}
}