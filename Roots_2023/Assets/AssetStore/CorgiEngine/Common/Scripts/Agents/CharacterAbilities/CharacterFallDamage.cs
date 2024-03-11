using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This component will apply damage to the character if it falls from a height higher than the specified MinimumDamageFallHeight
	/// How much damage that is will be remapped between the specified min and max damage values.
	/// Animation parameter : FallDamage, bool, true the frame the character takes fall damage
	/// </summary>
	[MMHiddenProperties("AbilityStartFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Fall Damage")]
	public class CharacterFallDamage : CharacterAbility
	{
		public override string HelpBoxText() { return "This component will apply damage to the character if it falls from a height " +
		                                              "higher than the specified MinimumDamageFallHeight." +
		                                              "Use the min and max damage fall heights to define the remap rules." +
		                                              "You can also decide to clamp damage to the max damage, or just have it proportional."; }

		/// the minimum height at which a character has to fall for damage to be applied
		[Tooltip("the minimum height at which a character has to fall for damage to be applied")]
		public float MinimumDamageFallHeight = 5f;
		/// the height at which you'd have to fall to apply the highest damage
		[Tooltip("the height at which you'd have to fall to apply the highest damage")]
		public float MaximumDamageFallHeight = 10f;
		/// the damage to apply when falling from the min height
		[Tooltip("the damage to apply when falling from the min height")]
		public int MinimumDamage = 10;
		/// the damage to apply when falling from the max height
		[Tooltip("the damage to apply when falling from the max height")]
		public int MaximumDamage = 50;
		/// whether or not to clamp the damage to MaximumDamage. If not clamped, falling from an even higher height will apply even more damage.
		[Tooltip("whether or not to clamp the damage to MaximumDamage. If not clamped, falling from an even higher height will apply even more damage.")]
		public bool ClampedDamage = true;

		protected bool _airborneLastFrame = false;
		protected float _takeOffAltitude;
		protected bool _damageThisFrame = false;

		// animation parameters
		protected const string _fallDamageAnimationParameterName = "FallDamage";
		protected int _fallDamageAnimationParameter;

		/// <summary>
		/// On Update, we check if we're taking flight, and if we should take damage
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();

			_damageThisFrame = false;

			// if we were not airborne last frame and are now, we're taking off, we log that altitude
			if (!_airborneLastFrame && _character.Airborne)
			{
				_takeOffAltitude = this.transform.position.y;
			}

			ResetTakeOffAltitude();

			// if we were airborne and are not anymore, we just touched the ground
			if (_airborneLastFrame && !_character.Airborne && CanTakeDamage())
			{
				float distance = _takeOffAltitude - this.transform.position.y;

				// if we're above the minimum fall height to apply damage, we apply damage
				if (distance > MinimumDamageFallHeight)
				{
					ApplyDamage(distance);
				}
			}

			_airborneLastFrame = _character.Airborne;
		}

		/// <summary>
		/// Every frame, we check if we're in a state that should reset the altitude (fall, glide to the ground, touch the ground shouldn't trigger damage, for example)
		/// </summary>
		public virtual void ResetTakeOffAltitude()
		{
			if (!CanTakeDamage())
			{
				_takeOffAltitude = this.transform.position.y;
			}
		}

		/// <summary>
		/// This method returns true if the character is in a state that can take damage.
		/// Don't hesitate to extend and override this method to specify your own rules
		/// </summary>
		/// <returns></returns>
		protected virtual bool CanTakeDamage()
		{
			return (_character.MovementState.CurrentState != CharacterStates.MovementStates.LadderClimbing
			        && _character.MovementState.CurrentState != CharacterStates.MovementStates.SwimmingIdle
			        && _character.MovementState.CurrentState != CharacterStates.MovementStates.Diving
			        && _character.MovementState.CurrentState != CharacterStates.MovementStates.Flying
			        && _character.MovementState.CurrentState != CharacterStates.MovementStates.Gliding
			        && _character.MovementState.CurrentState != CharacterStates.MovementStates.WallClinging
			        && _character.MovementState.CurrentState != CharacterStates.MovementStates.Jetpacking);
		}

		/// <summary>
		/// Applies fall damage
		/// </summary>
		/// <param name="distance"></param>
		protected virtual void ApplyDamage(float distance)
		{
			int damageToApply = (int)Mathf.Round(MMMaths.Remap(distance, MinimumDamageFallHeight, MaximumDamageFallHeight,
				(float)MinimumDamage, (float)MaximumDamage));
			if (ClampedDamage)
			{
				damageToApply = (int)Mathf.Clamp(damageToApply, (float)MinimumDamage, (float)MaximumDamage);
			}

			if (!_startFeedbackIsPlaying)
			{
				PlayAbilityStartFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.FallDamage);
			}
			_health.Damage(damageToApply, this.gameObject, 0.2f, 0.2f, Vector3.up);
			_damageThisFrame = true;
		}
        
		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_fallDamageAnimationParameterName, AnimatorControllerParameterType.Bool, out _fallDamageAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our character's animator the current fall damage status
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _fallDamageAnimationParameter, _damageThisFrame, _character._animatorParameters, _character.PerformAnimatorSanityChecks);            
		}
	}
}