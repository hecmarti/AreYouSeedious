using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a platform to make it a jumping platform, a trampoline or whatever.
	/// It will automatically push any character that touches it up in the air.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Environment/Jumper")]
	public class Jumper : CorgiMonoBehaviour 
	{
		/// the force of the jump induced by the platform
		[Tooltip("the force of the jump induced by the platform")]
		public float JumpPlatformBoost = 40;
		/// whether or not this jumper should be allowed to make a character jump from below
		[Tooltip("whether or not this jumper should be allowed to make a character jump from below")]
		public bool CanBeActivatedFromBelow = false;
		/// whether or not to let a character jump only if grounded
		[Tooltip("whether or not to let a character jump only if grounded")]
		public bool PreventJumpIfCharacterIsGrounded = false;

		[Header("Feedbacks")]

		public Animator animator;

		/// a feedback to play when the zone gets activated
		[Tooltip("a feedback to play when the zone gets activated")]
		public MMFeedbacks ActivationFeedback;

		protected CorgiController _controller;
		protected CharacterJump _characterJump;

		/// <summary>
		/// Triggered when a CorgiController touches the platform, applys a vertical force to it, propulsing it in the air.
		/// </summary>
		/// <param name="controller">The corgi controller that collides with the platform.</param>			
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			if ((collider.transform.position.y < this.transform.position.y) && !CanBeActivatedFromBelow)
			{
				return;
			}

			_controller = collider.GetComponent<CorgiController>();
			
			if (_controller == null)
			{
				return;
			}
			else
			{
				if (PreventJumpIfCharacterIsGrounded && (_controller.State.IsGrounded) && (_controller.State.WasGroundedLastFrame))
				{
					_controller = null;
					return;
				}
			}
		}

		/// <summary>
		/// On late update we set a force to our collider's controller if we have one
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (_controller != null)
			{
				_controller.SetVerticalForce(Mathf.Sqrt(2f * JumpPlatformBoost * -_controller.Parameters.Gravity));
				_characterJump = _controller.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterJump>();
				if (_characterJump != null)
				{
					_characterJump.SetCanJumpStop(false);
					_characterJump.SetJumpFlags();
				}

				animator.SetTrigger("JumpOn");
                ActivationFeedback?.PlayFeedbacks();
				_controller = null;
			}            
		}
	}
}