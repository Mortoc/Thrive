/**  --------------------------------------------------------  *
 *   YieldUntilFixedUpdate.cs
 *
 *   Author: Mortoc, Hangout Industries
 *   Date: 07/15/2009
 *	 
 *   --------------------------------------------------------  *
 */

using UnityEngine;

using System;
using System.Collections;

public class YieldUntilFixedUpdate : IFixedYieldInstruction
{
	public bool Ready
	{
		get{ return true; }
	}
}
