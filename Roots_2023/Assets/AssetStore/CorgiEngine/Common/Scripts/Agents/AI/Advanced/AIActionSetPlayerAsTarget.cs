using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// An AIACtion used to set the current Player character as the target
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AIActionSetPlayerAsTarget")]
	public class AIActionSetPlayerAsTarget : AIAction
	{
		public bool OnlyRunOnce = true;
    
		protected bool _alreadyRan = false;
    
		/// <summary>
		/// On init we initialize our action
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			_alreadyRan = false;
		}

		/// <summary>
		/// Sets a new target
		/// </summary>
		public override void PerformAction()
		{
			if (OnlyRunOnce && _alreadyRan)
			{
				return;
			}

			if (LevelManager.HasInstance)
			{
				_brain.Target = LevelManager.Instance.Players[0].transform;
			}
		}

		/// <summary>
		/// On enter state we reset our flag
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			_alreadyRan = false;
		}
	}
}