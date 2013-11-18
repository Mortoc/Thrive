using System;
using System.Collections.Generic;

namespace Thrive.Core.PlayerCharacter
{
	// Top-level state machine for the character
	public class PlayerCharacterStateMachine : StateMachine
	{
		private class DefaultState : IState 
		{
			public void EnterState(IStateMachine owner) {}
			public void ExitState() {}
		}
		
		public PlayerCharacterStateMachine(IStateMachine parent)
			: base(new DefaultState(), parent) 
		{
			
		}
	}
}

