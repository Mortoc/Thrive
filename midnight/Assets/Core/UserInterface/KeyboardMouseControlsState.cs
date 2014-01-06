using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thrive.Core.UI
{
	public class KeyboardMouseControlsState : IControllerState
	{
		private ITask _updateLoop = null;
		private readonly UserInputController _owner;

		public KeyboardMouseControlsState(UserInputController owner)
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
			Vector3 direction = Vector3.zero;
			bool hasMoved = false;

			
			if( Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) )
			{
				direction = Vector3.up;
				hasMoved = true;
			}

			if( Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) )
			{
				if( hasMoved )
				{
					direction = (direction + Vector3.left) * 0.5f;
				}
				else
				{
					direction = Vector3.left;
					hasMoved = true;
				}
			}
			
			if( Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) )
			{
				if( hasMoved )
				{
					direction = (direction + Vector3.down) * 0.5f;
				}
				else
				{
					direction = Vector3.down;
					hasMoved = true;
				}
			}
			
			if( Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) )
			{
				if( hasMoved )
				{
					direction = (direction + Vector3.right) * 0.5f;
				}
				else
				{
					direction = Vector3.right;
					hasMoved = true;
				}
			}


			if( hasMoved )
			{
				_owner.MoveInput(direction);
			}
		}

		private void HandleJump()
		{
			if( Input.GetKeyDown(KeyCode.Space) )
			{
				_owner.JumpInput();
			}
		}

		public void ExitState()
		{
			if( _updateLoop != null )
				_updateLoop.Exit();
		}

	}
}
