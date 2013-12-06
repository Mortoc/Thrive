using System;
using System.Collections.Generic;

namespace Thrive.Core
{
	public interface IState
	{
		// Enter and Exit state should only be called by the managing IStateMachine
		void EnterState(IStateMachine owner);
		void ExitState();
	}

	public interface IStateMachine : IDisposable
	{
		IState CurrentState { get; }
		void Transition( IState newState );

		void RegisterChild(IStateMachine child);
	}

	// Default implementation for StateMachines
	public class StateMachine : IStateMachine
	{
		private readonly List<IStateMachine> _children = new List<IStateMachine>();

		public StateMachine(IState defaultState, IStateMachine parent)
		{
			parent.RegisterChild (this);
			Transition(defaultState);
		}

		public void RegisterChild(IStateMachine child)
		{
			_children.Add(child);
		}

		public IState CurrentState { get; private set; }

		public void Transition( IState newState )
		{
			if( newState == null ) 
				throw new ArgumentNullException("newState");

			if( CurrentState != null )
				CurrentState.ExitState();
			
			CurrentState = newState;
			CurrentState.EnterState(this);
		}

		~StateMachine()
		{
			Dispose(true);
		}

		public virtual void Dispose() 
		{
			Dispose(false);
		} 

		protected virtual void Dispose(bool gc)
		{
			CurrentState.ExitState();
		}
	}
}