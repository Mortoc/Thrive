using System;

namespace Thrive.Core
{
	public class SettingsState : IControllerState
	{
		public void EnterState(IController stateMachine)
		{
			// Display settings
		}

		public void ExitState()
		{
			// Destroy settings 
		}
	}
}