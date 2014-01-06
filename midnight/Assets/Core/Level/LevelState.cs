using System;
using System.Collections.Generic;
using UnityEngine;

using Thrive.Core.Enemies;
using Thrive.Core.Allies;
using Thrive.Core.PlayerCharacter;
using Thrive.Core.UI;

namespace Thrive.Core
{
	public class LevelState : IControllerState
	{
		private readonly string _levelName;
		private ITask _loadingLevelTask;

		private ParallaxLayerController _parallaxLayerController;
		private EnemyControllerController _enemysController;
		private PlayerCharacterController _playerCharacterController;
		private AllyControllerController _alliedUnitsController;
		private UserInputController _userInputController;

		public LevelState(string levelName)
		{
			_levelName = levelName;
		}

		public void EnterState(IController controller)
		{
			_loadingLevelTask = Scheduler.Run( LoadLevel(delegate() {
							
				// Initialize all the in-level state machines
				_enemysController = new EnemyControllerController(controller);
				_playerCharacterController = new PlayerCharacterController(controller);
				_alliedUnitsController = new AllyControllerController(controller);
				_userInputController = new UserInputController(controller);
				_parallaxLayerController = new ParallaxLayerController(controller);
				
				// Create Player 
				_playerCharacterController.OnPlayerCreated += SetupPlayerInput;
				_playerCharacterController.CreatePlayerCharacter();
			}));
		}

		private IEnumerator<IYieldInstruction> LoadLevel(Action onComplete)
		{
			UnityEngine.Application.LoadLevel(_levelName);

			while( Application.loadedLevelName != _levelName ) {
				yield return Yield.UntilNextFrame;
			}

			onComplete();
		}

		private void SetupPlayerInput()
		{
			_userInputController.OnPlayerMove += _playerCharacterController.Move;
			_userInputController.OnPlayerJump += _playerCharacterController.Jump;
		}

		public void ExitState()
		{
			if( _loadingLevelTask != null )
				_loadingLevelTask.Exit();

			_enemysController.Dispose();
			_playerCharacterController.Dispose();
			_alliedUnitsController.Dispose();
			_userInputController.Dispose();
			_parallaxLayerController.Dispose();
		}
	}
}