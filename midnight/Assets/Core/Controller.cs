using System;
using System.Collections.Generic;

namespace Thrive.Core
{
	public interface IControllerState
	{
		// Enter and Exit state should only be called by the managing IStateMachine
		void EnterState(IController owner);
		void ExitState();
	}

	public interface IController : IDisposable
	{
		IControllerState CurrentState { get; }
		void Transition( IControllerState newState );

		void RegisterChild(IController child);
	}

	// Default implementation for StateMachines
	public class Controller : IController
	{
		private readonly List<IController> _children = new List<IController>();

		public Controller(IControllerState defaultState, IController parent)
		{
			if( parent != null )
			{
				parent.RegisterChild (this);
			}

			Transition(defaultState);
		}

		public void RegisterChild(IController child)
		{
			_children.Add(child);
		}

		public IControllerState CurrentState { get; private set; }

		public void Transition( IControllerState newState )
		{
			if( newState == null ) 
				throw new ArgumentNullException("newState");

			if( CurrentState != null )
				CurrentState.ExitState();
			
			CurrentState = newState;
			CurrentState.EnterState(this);
		}

		~Controller()
		{
			Dispose(true);
		}

		public virtual void Dispose() 
		{
			Dispose(false);
		} 

		protected virtual void Dispose(bool gc)
		{
			if( !gc )
				CurrentState.ExitState();
		}
	}
}