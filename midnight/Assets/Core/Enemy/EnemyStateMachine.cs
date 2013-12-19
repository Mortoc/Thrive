using System;
using System.Collections.Generic;

namespace Thrive.Core.Enemies
{
	// This state machine is responsible for the state of a single enemy
	public class EnemyStateMachine : Controller
	{
		
		public EnemyStateMachine(IControllerState defaultState, IController parent)
			: base(defaultState, parent) 
		{
		}
	}
}

