using System;
using System.Collections.Generic;

using UnityEngine;
using Thrive.Core;
using Thrive.Core.UI;

namespace Thrive.Core.PlayerCharacter
{
	// Top-level state machine for the character
	public class PlayerCharacterController : Controller
	{
		private class PlayerSettings
		{
			public float Speed { get; set; }
			public float JumpPower { get; set; }
		}

		private PlayerSettings _settings = new PlayerSettings(){ 
			// Hard-coded for now
			Speed = 100.0f,
			JumpPower = 200.0f
		};
		public event Action OnPlayerCreated;
		private GameObject _playerRoot;

		private class DefaultState : IControllerState
		{
			public void EnterState(IController owner) {}
			public void ExitState() {}
		}
		
		public PlayerCharacterController(IController parent)
			: base(new DefaultState(), parent) 
		{
		}

		public void CreatePlayerCharacter()
		{
			Resource.Loader.GetPrefab("MainCharacter/Character", characterPrefab => 
			{
				_playerRoot = Instantiate.Prefab(characterPrefab);
				OnPlayerCreated();
			});
		}

		public void Move(Vector3 direction)
		{
			_playerRoot.rigidbody2D.AddForce(direction * Time.deltaTime * _settings.Speed);
		}

		public void Jump()
		{
			_playerRoot.rigidbody2D.AddForce(Vector3.up * _settings.JumpPower);

		}

		protected override void Dispose (bool gc)
		{
			if( !gc ) 
			{
				Debug.Log ("Kill");
				GameObject.Destroy(_playerRoot);
			}

			base.Dispose(gc);
		}
	}
}

