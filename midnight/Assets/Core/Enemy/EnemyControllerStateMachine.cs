using System;
using System.Collections.Generic;

namespace Thrive.Core.Enemies
{
	// This state machine is responsible for all the enemies in a single level
	public class EnemyControllerStateMachine : StateMachine
	{
		private class DefaultState : IState 
		{
			public void EnterState(IStateMachine owner) {}
			public void ExitState() {}
		}

		private readonly List<EnemyStateMachine> _enemies = new List<EnemyStateMachine>();

		public EnemyControllerStateMachine(IStateMachine parent)
			: base(new DefaultState(), parent) 
		{

		}

	}
}

