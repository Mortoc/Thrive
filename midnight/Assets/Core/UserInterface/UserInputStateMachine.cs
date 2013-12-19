using System;
using System.Collections.Generic;

namespace Thrive.Core.UI
{
	public class UserInputStateMachine : Controller
	{
		private class DefaultState : IControllerState
		{
			public void EnterState(IController owner) {}
			public void ExitState() {}
		}
		
		public UserInputStateMachine(IController parent)
			: base(new DefaultState(), parent) 
		{
		}
	}
}

