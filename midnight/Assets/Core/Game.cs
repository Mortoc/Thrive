using System;

namespace Thrive.Core
{
	public static class Game
	{
		private static IStateMachine _root;

		// Creates the state machine structure required to begin the game
		// and returns the root state machine
		public static IStateMachine InitializeGame()
		{
			_root = new StateMachine(
				new MainMenuState(),
				null
			);

			return _root;
		}
	
		public static void DebugPrintStates(Action<string> printFunc = null)
		{
			if( printFunc == null )
				printFunc = UnityEngine.Debug.Log;

			printFunc( _root.ToString() );
		}

	}
}