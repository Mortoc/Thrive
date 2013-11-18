using System;
using System.Collections.Generic;

namespace Thrive.Core.UI
{
	public class UserInputStateMachine : StateMachine
	{
		private class DefaultState : IState 
		{
			public void EnterState(IStateMachine owner) {}
			public void ExitState() {}
		}
		
		public UserInputStateMachine(IStateMachine parent)
			: base(new DefaultState(), parent) 
		{
		}
	}
}

