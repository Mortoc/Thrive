using System;
using System.Collections.Generic;

namespace Thrive.Core.Allies
{
	// This state machine is responsible for all of the player's allied units (turrets and the like)
	public class AllyControllerStateMachine : StateMachine
	{
		private class DefaultState : IState 
		{
			public void EnterState(IStateMachine owner) {}
			public void ExitState() {}
		}

		public AllyControllerStateMachine(IStateMachine parent)
			: base(new DefaultState(), parent) 
		{
		}
	}
}

