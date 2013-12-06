using System;

using Thrive.Core.Enemies;
using Thrive.Core.Allies;
using Thrive.Core.PlayerCharacter;
using Thrive.Core.UI;

namespace Thrive.Core
{
	public class LevelState : IState
	{
		private readonly int _levelId;

		private EnemyControllerStateMachine _enemyControllerSM;
		private PlayerCharacterStateMachine _playerCharacterSM;
		private AllyControllerStateMachine _alliedUnitSM;
		private UserInputStateMachine _userInputSM;

		public LevelState(int levelId)
		{
			_levelId = levelId;
		}

		public void EnterState(IStateMachine stateMachine)
		{
			// Load the level in Unity

			// Initialize all the in-level state machines
			_enemyControllerSM = new EnemyControllerStateMachine(stateMachine);
			_playerCharacterSM = new PlayerCharacterStateMachine(stateMachine);
			_alliedUnitSM = new AllyControllerStateMachine(stateMachine);
			_userInputSM = new UserInputStateMachine(stateMachine);
		}

		public void ExitState()
		{
			_enemyControllerSM.Dispose();
			_playerCharacterSM.Dispose();
			_alliedUnitSM.Dispose();
			_userInputSM.Dispose();
		}
	}
}