using System;
using System.Collections.Generic;

namespace Thrive.Core.Enemies
{
	// This state machine is responsible for all the enemies in a single level
	public class EnemyControllerStateMachine : Controller
	{
		private class DefaultState : IControllerState
		{
			public void EnterState(IController owner) {}
			public void ExitState() {}
		}

		private readonly List<EnemyStateMachine> _enemies = new List<EnemyStateMachine>();

		public EnemyControllerStateMachine(IController parent)
			: base(new DefaultState(), parent) 
		{

		}

	}
}

