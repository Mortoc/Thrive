using System;
using System.Collections.Generic;

namespace Thrive.Core.Enemies
{
	// This state machine is responsible for the state of a single enemy
	public class EnemyStateMachine : StateMachine
	{
		
		public EnemyStateMachine(IState defaultState, IStateMachine parent)
			: base(defaultState, parent) 
		{
		}
	}
}

