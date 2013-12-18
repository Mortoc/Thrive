using System;
using UnityEngine;


using Thrive.View;

namespace Thrive.Core
{
	// Default class for the top-level state machine
	// This class is responsible for setting up the
	// first scene a user sees.
	public class MainMenuState : IState
	{
		private SettingsState _settingsState;
		private SelectUserProfileState _selectUserProfileState;
		private IReceipt _getBackgroundReceipt = null;
		private GameObject _guiObject;
		private GameObject _background;
		private IStateMachine _stateMachine;

		public void EnterState(IStateMachine stateMachine)
		{
			_stateMachine = stateMachine;

			// Initialize the state for each menu state
			_settingsState = new SettingsState();
			_selectUserProfileState = new SelectUserProfileState();

			// Create out-of-game background
			_getBackgroundReceipt = Resource.Loader.GetPrefab(
				"MainMenu/MenuBackground", 
				g => _background = Instantiate.Prefab(g)
			);

			// Create menu GUI
			_guiObject = new GameObject("MainMenuView", typeof(MainMenuView));
			_guiObject.GetComponent<MainMenuView>().State = this;
		}

		public void ExitState()
		{
			// Destroy out-of-game background
			if( _getBackgroundReceipt != null )
				_getBackgroundReceipt.Exit();
			GameObject.Destroy(_background);

			// Destroy menu GUI
			GameObject.Destroy(_guiObject);
		}

		public void ProfileSelected(int profile)
		{
			// Go to the first level
			_stateMachine.Transition(new LevelState("Level1"));
		}

		public void GoToSettings()
		{

		}
	}
}