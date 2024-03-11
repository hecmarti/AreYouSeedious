using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This action directs the CharacterHorizontalMovement ability to move in the direction of the target.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Move Towards Target")]
	// [RequireComponent(typeof(CharacterHorizontalMovement))]
	public class AIActionMoveTowardsTarget : AIAction
	{
		/// The minimum distance to the target that this Character can reach
		[Tooltip("The minimum distance to the target that this Character can reach")]
		public float MinimumDistance = 1f;

		protected CharacterHorizontalMovement _characterHorizontalMovement;
        
		/// <summary>
		/// On init we grab our CharacterHorizontalMovement ability
		/// </summary>
		public override void Initialization()
		{
			_characterHorizontalMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHorizontalMovement>();
		}

		/// <summary>
		/// On PerformAction we move
		/// </summary>
		public override void PerformAction()
		{
			Move();
		}

		/// <summary>
		/// Moves the character in the decided direction
		/// </summary>
		protected virtual void Move()
		{
			if (_brain.Target == null)
			{
				_characterHorizontalMovement.SetHorizontalMove(0f);
				return;
			}
			if (Mathf.Abs(this.transform.position.x - _brain.Target.position.x) < MinimumDistance)
			{
				_characterHorizontalMovement.SetHorizontalMove(0f);
				return;
			}

			if (this.transform.position.x < _brain.Target.position.x)
			{
				_characterHorizontalMovement.SetHorizontalMove(1f);
			}            
			else
			{
				_characterHorizontalMovement.SetHorizontalMove(-1f);
			}
		}

		/// <summary>
		/// When entering the state we reset our movement.
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			if (_characterHorizontalMovement == null)
			{
				Initialization();
			}
			_characterHorizontalMovement.SetHorizontalMove(0f);
		}

		/// <summary>
		/// When exiting the state we reset our movement.
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			_characterHorizontalMovement?.SetHorizontalMove(0f);
		}
	}
}