using System;
using UnityEngine;

using Thrive.Core;

namespace Thrive.View
{
	public class MainMenuView : MonoBehaviour
	{
		public MainMenuState State { get; set; }
		private Rect _screenRect = new Rect(0.0f, 0.0f, 0.0f, 0.0f);

		private void OnGUI()
		{
			_screenRect.width = Screen.width;
			_screenRect.height = Screen.height;
			GUILayout.BeginArea(_screenRect);
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical();
			if( GUILayout.Button("User 1") ) 
			{
				State.ProfileSelected(1);
			}
			GUILayout.Space(4.0f);
			if( GUILayout.Button("User 2") ) 
			{
				State.ProfileSelected(2);
			}
			GUILayout.Space(4.0f);
			if( GUILayout.Button("User 3") ) 
			{
				State.ProfileSelected(3);
			}
			GUILayout.Space(12.0f);
			if( GUILayout.Button("Settings") ) 
			{
				State.GoToSettings();
			}

			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndArea();
		}
	}
}

