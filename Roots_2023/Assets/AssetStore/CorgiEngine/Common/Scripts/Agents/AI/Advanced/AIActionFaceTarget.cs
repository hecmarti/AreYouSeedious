using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// An AIACtion used to have an AI face its AI Brain's Target
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AIActionFaceTarget")]
	public class AIActionFaceTarget : AIAction
	{
		public bool OnlyRunOnce = false;
        
		protected Character _character;
		protected bool _alreadyRan = false;
		protected bool _isCharacterNull;
		protected Character.FacingDirections _facingDirection;

		/// <summary>
		/// On init we grab our Character component
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			_character = this.gameObject.GetComponentInParent<Character>();
			_isCharacterNull = _character == null;
		}

		/// <summary>
		/// Makes the Character face the Target's direction
		/// </summary>
		public override void PerformAction()
		{
			if (OnlyRunOnce && _alreadyRan)
			{
				return;
			}
			_alreadyRan = true;
			if ((_brain.Target == null) || (_isCharacterNull))
			{
				return;
			}
			bool facingRight = _brain.Target.transform.position.x > _character.transform.position.x;
			_facingDirection =
				facingRight ? Character.FacingDirections.Right : Character.FacingDirections.Left;
			_character.Face(_facingDirection);
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