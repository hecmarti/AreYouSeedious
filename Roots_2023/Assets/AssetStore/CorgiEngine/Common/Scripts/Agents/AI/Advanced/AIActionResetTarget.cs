using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// An Action that will set the target to null, resetting it
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AIActionResetTarget")]
	public class AIActionResetTarget : AIAction
	{
		/// <summary>
		/// we reset our target
		/// </summary>
		public override void PerformAction()
		{
			_brain.Target = null;
		}
	}
}