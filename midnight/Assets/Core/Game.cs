using System;

namespace Thrive.Core
{
	public static class Game
	{
		private static IController _root;

		// Creates the state machine structure required to begin the game
		// and returns the root state machine
		public static IController InitializeGame()
		{
			_root = new Controller(
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