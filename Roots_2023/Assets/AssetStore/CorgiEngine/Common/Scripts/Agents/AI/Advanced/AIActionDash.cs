using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This action performs one dash.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Dash")]
	// [RequireComponent(typeof(CharacterDash))]
	public class AIActionDash : AIAction
	{
		protected CharacterDash _characterDash;

		/// <summary>
		/// On init we grab our CharacterDash component
		/// </summary>
		public override void Initialization()
		{
			_characterDash = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterDash>();
		}

		/// <summary>
		/// On PerformAction we dash
		/// </summary>
		public override void PerformAction()
		{
			Dash();
		}

		/// <summary>
		/// Calls CharacterDash's StartDash method to initiate the dash
		/// </summary>
		protected virtual void Dash()
		{
			_characterDash.StartDash();
		}
	}
}