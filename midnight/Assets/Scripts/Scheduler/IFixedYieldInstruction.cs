/**  --------------------------------------------------------  *
 *   IFixedYieldInstruction.cs  
 *
 *   Author: Mortoc
 *   Date: 08/01/2009
 *	 
 *   --------------------------------------------------------  *
 */


using UnityEngine;
using System;


// Same interface as IYieldInstruction, the only difference is that
// the Scheduler will run these on FixedUpdate instead
public interface IFixedYieldInstruction : IYieldInstruction
{
}
