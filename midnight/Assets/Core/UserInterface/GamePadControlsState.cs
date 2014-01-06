using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thrive.Core.UI
{
	public class GamePadControlsState : IControllerState
	{
		private ITask _updateLoop = null;
		private readonly UserInputController _owner;
		
		public GamePadControlsState(UserInputController owner)
		{
			_owner = owner;
		}
		
		public void EnterState(IController controller)
		{
			Scheduler.Run( UpdateLoop() );
		}
		
		private IEnumerator<IYieldInstruction> UpdateLoop()
		{
			while(true)
			{
				HandleMove();
				HandleJump();
				
				yield return Yield.UntilNextFrame;
			}
		}
		
		private void HandleMove()
		{
			throw new NotImplementedException();
		}
		
		private void HandleJump()
		{
			throw new NotImplementedException();
		}
		
		public void ExitState()
		{
			if( _updateLoop != null )
				_updateLoop.Exit();
		}
		
	}
}
