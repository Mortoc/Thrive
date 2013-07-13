using System;
using System.Collections.Generic;

public interface IReceipt
{
	void Exit();
}	

public interface ITask : IReceipt
{
	bool IsRunning { get; }
	void AddOnExitAction(Action onExit);
}

public class Receipt : IReceipt
{
	private readonly Action exitCallback;
	
	
	public Receipt(Action onExit)
	{
		if( onExit == null )
			throw new ArgumentNullException("onExit");
		
		exitCallback = onExit;
	}
	
	public void Exit()
	{
		exitCallback();
	}
}	

