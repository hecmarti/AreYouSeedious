using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this ability to a Character to allow it to swim in Water by pressing the Swim button (by default the same binding as the Jump button, but separated for convenience)
	/// 
	/// Animator parameters : Swimming (bool), SwimmingIdle (bool)
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Swim")]
	public class CharacterSwim : CharacterAbility
	{
		public override string HelpBoxText() { return "This component allows a Character to swim in Water by pressing the Swim button (by default the same binding as the Jump button, but separated for convenience). Here you can define the swim force to apply, the duration of the associated animation, as well as VFX to instantiate when entering/exiting the water, and the force to apply when exiting."; }

		/// whether or not the character is in water
		[MMReadOnly]
		[Tooltip("whether or not the character is in water")]
		public bool InWater = false;

		[Header("Swim")]

		/// defines how high the character can jump
		[Tooltip("defines how high the character can jump")]
		public float SwimHeight = 3.025f;
		/// the duration (in seconds) of the swim animation before it reverts back to swim idle
		[Tooltip("the duration (in seconds) of the swim animation before it reverts back to swim idle")]
		public float SwimAnimationDuration = 0.8f;

		[Header("Splash Effects")]

		/// the effect that will be instantiated everytime the character enters the water
		[Tooltip("the effect that will be instantiated everytime the character enters the water")]
		public GameObject WaterEntryEffect;
		/// the effect that will be instantiated everytime the character exits the water
		[Tooltip("the effect that will be instantiated everytime the character exits the water")]
		public GameObject WaterExitEffect;
		/// the force to apply to the character when exiting water
		[Tooltip("the force to apply to the character when exiting water")]
		public Vector2 WaterExitForce = new Vector2(0f, 12f);

		protected float _swimDurationLeft = 0f;

		// animation parameters
		protected const string _inWaterAnimationParameterName = "InWater";
		protected const string _swimmingAnimationParameterName = "Swimming";
		protected const string _swimmingIdleAnimationParameterName = "SwimmingIdle";
		protected int _inWaterAnimationParameter;
		protected int _swimmingAnimationParameter;
		protected int _swimmingIdleAnimationParameter;

		/// <summary>
		/// On Update we decrease our counter
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			_swimDurationLeft -= Time.deltaTime;
		}

		/// <summary>
		/// At the beginning of each cycle we check if we've just pressed or released the swim button
		/// </summary>
		protected override void HandleInput()
		{
			if (!InWater)
			{
				return;
			}

			if (_inputManager.SwimButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				Swim();
			}
		}

		/// <summary>
		/// When swimming we apply our swim force
		/// </summary>
		protected virtual void Swim()
		{
			_movement.ChangeState(CharacterStates.MovementStates.SwimmingIdle);
			_controller.SetVerticalForce(Mathf.Sqrt(2f * SwimHeight * Mathf.Abs(_controller.Parameters.Gravity)));
			_swimDurationLeft = SwimAnimationDuration;
		}

		/// <summary>
		/// When entering the water we instantiate a splash if needed and change our state
		/// </summary>
		public virtual void EnterWater()
		{
			InWater = true;
			PlayAbilityStartFeedbacks();
			_movement.ChangeState(CharacterStates.MovementStates.SwimmingIdle);
			if (WaterEntryEffect != null)
			{
				Instantiate(WaterEntryEffect, this.transform.position, Quaternion.identity);
			}            
		}

		/// <summary>
		/// When exiting the water we instantiate a splash if needed and change our state
		/// </summary>
		public virtual void ExitWater()
		{
			InWater = false;
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			_movement.ChangeState(CharacterStates.MovementStates.Jumping);
			_controller.SetForce(WaterExitForce);

			if (WaterExitEffect != null)
			{
				Instantiate(WaterExitEffect, this.transform.position, Quaternion.identity);
			}            
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_inWaterAnimationParameterName, AnimatorControllerParameterType.Bool, out _inWaterAnimationParameter);
			RegisterAnimatorParameter(_swimmingAnimationParameterName, AnimatorControllerParameterType.Bool, out _swimmingAnimationParameter);
			RegisterAnimatorParameter(_swimmingIdleAnimationParameterName, AnimatorControllerParameterType.Bool, out _swimmingIdleAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our Running status to the character's animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _inWaterAnimationParameter, InWater, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _swimmingAnimationParameter, (_swimDurationLeft > 0f), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _swimmingIdleAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.SwimmingIdle), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		protected override void OnDeath()
		{
			base.OnDeath();

			InWater = false;
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			if (_animator != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _inWaterAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _swimmingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _swimmingIdleAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);    
			}
		}
	}
}