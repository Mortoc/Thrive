using System;
using System.Collections.Generic;

namespace Thrive.Core.PlayerCharacter
{
	// Top-level state machine for the character
	public class PlayerCharacterStateMachine : Controller
	{
		private class DefaultState : IControllerState
		{
			public void EnterState(IController owner) {}
			public void ExitState() {}
		}
		
		public PlayerCharacterStateMachine(IController parent)
			: base(new DefaultState(), parent) 
		{
			
		}
	}
}

