using System;

namespace Thrive.Core
{
	// Default class for the top-level state machine
	// This class is responsible for setting up the
	// first scene a user sees.
	public class MainMenuState : IState
	{
		private SettingsState _settingsState;
		private SelectUserProfileState _selectUserProfileState;

		public void EnterState(IStateMachine stateMachine)
		{
			// Initialize the state for each menu state
			_settingsState = new SettingsState();
			_selectUserProfileState = new SelectUserProfileState();

			// Add reference for out-of-game background


			// Display menu GUI

		}

		public void ExitState()
		{
			// Remove reference for out-of-game background
			// Destroy menu 
		}
	}
}