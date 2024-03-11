using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This action will cause your AI character to stop running
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Run Stop")]
	public class AIActionRunStop : AIAction
	{
		protected CharacterRun _characterRun;

		/// <summary>
		/// On init we grab our CharacterDash component
		/// </summary>
		public override void Initialization()
		{
			_characterRun = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterRun>();
		}

		/// <summary>
		/// On PerformAction we dash
		/// </summary>
		public override void PerformAction()
		{
			_characterRun.RunStop();
		}
	}
}