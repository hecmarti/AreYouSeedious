using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This action directs the CharacterHorizontalMovement ability to move away from the target.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Move Away From Target")]
	// [RequireComponent(typeof(CharacterHorizontalMovement))]
	public class AIActionMoveAwayFromTarget : AIActionMoveTowardsTarget
	{
		/// <summary>
		/// Moves the character in the decided direction
		/// </summary>
		protected override void Move()
		{
			if (_brain.Target == null)
			{
				_characterHorizontalMovement.SetHorizontalMove(0f);
				return;
			}
			if (Mathf.Abs(this.transform.position.x - _brain.Target.position.x) >= MinimumDistance)
			{
				_characterHorizontalMovement.SetHorizontalMove(0f);
				return;
			}

			if (this.transform.position.x < _brain.Target.position.x)
			{
				_characterHorizontalMovement.SetHorizontalMove(-1f);
			}            
			else
			{
				_characterHorizontalMovement.SetHorizontalMove(1f);
			}
		}
	}
}