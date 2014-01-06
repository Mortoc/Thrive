using System;
using System.Collections.Generic;

namespace Thrive.Core.Allies
{
	// This state machine is responsible for all of the player's allied units (turrets and the like)
	public class AllyControllerController : Controller
	{
		private class DefaultState : IControllerState
		{
			public void EnterState(IController owner) {}
			public void ExitState() {}
		}

		public AllyControllerController(IController parent)
			: base(new DefaultState(), parent) 
		{
		}
	}
}

