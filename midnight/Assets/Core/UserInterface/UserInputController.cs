using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thrive.Core.UI
{
	public class UserInputController : Controller
	{
		public event Action<Vector3> OnPlayerMove;
		public event Action OnPlayerJump;

		private class DefaultState : IControllerState
		{
			public DefaultState()
			{
			}

			public void EnterState(IController owner) 
			{
				if( !(owner is UserInputController) ) 
					throw new ArgumentException("owner must be a UserInputController");

				UserInputController inputController = (UserInputController)owner;

				// Figure out what platform we're on and transition to that state
				switch( UnityEngine.Application.platform )
				{
				case UnityEngine.RuntimePlatform.Android:
				case UnityEngine.RuntimePlatform.IPhonePlayer:
				case UnityEngine.RuntimePlatform.BB10Player:
				case UnityEngine.RuntimePlatform.WP8Player:
					owner.Transition(new TouchControlsState(inputController));
					break;
				case UnityEngine.RuntimePlatform.PS3:
				case UnityEngine.RuntimePlatform.XBOX360:
				case UnityEngine.RuntimePlatform.WiiPlayer:
					owner.Transition(new GamePadControlsState(inputController));
					break;
				case UnityEngine.RuntimePlatform.NaCl:
				case UnityEngine.RuntimePlatform.LinuxPlayer:
				case UnityEngine.RuntimePlatform.OSXEditor:
				case UnityEngine.RuntimePlatform.OSXPlayer:
				case UnityEngine.RuntimePlatform.OSXWebPlayer:
				case UnityEngine.RuntimePlatform.WindowsEditor:
				case UnityEngine.RuntimePlatform.WindowsPlayer:
				case UnityEngine.RuntimePlatform.WindowsWebPlayer:
					owner.Transition(new KeyboardMouseControlsState(inputController));
					break;
				default:
					throw new Exception("Unsupported platform: " + UnityEngine.Application.platform);
				}
			}
			public void ExitState() {}
		}
		
		public UserInputController(IController parent)
			: base(new DefaultState(), parent) 
		{
		}

		public void MoveInput(Vector3 direction)
		{
			OnPlayerMove(direction);
		}

		
		public void JumpInput()
		{
			OnPlayerJump();
		}
	}
}

