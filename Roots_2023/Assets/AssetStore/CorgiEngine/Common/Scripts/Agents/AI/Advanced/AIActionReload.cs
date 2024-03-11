using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// An AIACtion used to request a reload on the weapon
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AIActionReload")]
	public class AIActionReload : AIAction
	{
		[FormerlySerializedAs("OnlyReloadOnceInThisSate")] 
		public bool OnlyReloadOnceInThisState = true;

		protected CharacterHandleWeapon _characterHandleWeapon;
		protected bool _reloadedOnce = false;

		/// <summary>
		/// On init we grab our components
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
		}

		/// <summary>
		/// Requests a reload
		/// </summary>
		public override void PerformAction()
		{
			if (OnlyReloadOnceInThisState && _reloadedOnce)
			{
				return;
			}
			if (_characterHandleWeapon == null)
			{
				return;
			}
			_characterHandleWeapon.Reload();
			_reloadedOnce = true;
		}

		/// <summary>
		/// On enter state we reset our counter
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			_reloadedOnce = false;
		}
	}
}