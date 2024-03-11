using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this component to a character and it'll be able to run
	/// Animator parameters : Running
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Run")] 
	public class CharacterRun : CharacterAbility
	{	
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component allows your character to change speed (defined here) when pressing the run button."; }

		[Header("Speed")]
		/// the speed of the character when it's running
		[Tooltip("the speed of the character when it's running")]
		public float RunSpeed = 16f;

		[Header("Input")]
		/// if this is set to false, will ignore input (use methods via script instead)
		[Tooltip("if this is set to false, will ignore input (use methods via script instead)")]
		public bool ReadInput = true;
		public bool ShouldRun { get; protected set; }
        
		[Header("AutoRun")]
		/// whether or not run should auto trigger if you move the joystick far enough
		[Tooltip("whether or not run should auto trigger if you move the joystick far enough")]
		public bool AutoRun = false;
		/// the input threshold on the joystick (normalized)
		[Tooltip("the input threshold on the joystick (normalized)")]
		public float AutoRunThreshold = 0.6f;

		// animation parameters
		protected const string _runningAnimationParameterName = "Running";
		protected int _runningAnimationParameter;
		protected bool _runningStarted = false;

		/// <summary>
		/// At the beginning of each cycle, we check if we've pressed or released the run button
		/// </summary>
		protected override void HandleInput()
		{
			if (!ReadInput)
			{
				if (!ShouldRun && (_movement.CurrentState == CharacterStates.MovementStates.Running))
				{
					RunStop();
				}

				if ((_movement.CurrentState != CharacterStates.MovementStates.Running) && ShouldRun)
				{
					RunStart();
				}
				return;
			}

			if (_inputManager.RunButton.State.CurrentState == MMInput.ButtonStates.ButtonDown || _inputManager.RunButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed)
			{
				RunStart();
			}				
			if (_inputManager.RunButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
			{
				RunStop();
			}
            
			if (AutoRun)
			{
				if (_inputManager.PrimaryMovement.magnitude > AutoRunThreshold)
				{
					_inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed);
				}
				else
				{
					_inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
					RunStop();
				}
			}
		}

		public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleRunningExit();
		}

		/// <summary>
		/// Tests if run state should be exited
		/// </summary>
		protected virtual void HandleRunningExit()
		{
			// if we're running and not grounded, we change our state to Falling
			if (!_controller.State.IsGrounded && (_movement.CurrentState == CharacterStates.MovementStates.Running) && _startFeedbackIsPlaying)
			{
				_movement.ChangeState(CharacterStates.MovementStates.Falling);
				StopFeedbacks ();
			}

			// if we're not moving fast enough, we go back to idle

			float movingSpeed = _controller.Speed.x;
			if (_controller.CurrentSurfaceModifier != null)
			{
				movingSpeed -= _controller.CurrentSurfaceModifier.AddedForce.x;
				movingSpeed = Mathf.Clamp(movingSpeed, 0f, Single.MaxValue);
			}
			
			if ((Mathf.Abs(movingSpeed) < RunSpeed / 10) && (_movement.CurrentState == CharacterStates.MovementStates.Running) && _startFeedbackIsPlaying)
			{
				_movement.ChangeState (CharacterStates.MovementStates.Idle);
				StopFeedbacks ();
			}

			if ((!_controller.State.IsGrounded) && _startFeedbackIsPlaying)
			{
				StopFeedbacks ();
			}

			if ((_movement.CurrentState != CharacterStates.MovementStates.Running) && _startFeedbackIsPlaying)
			{
				StopFeedbacks();
			}
		}

		/// <summary>
		/// Causes the character to start running.
		/// </summary>
		public virtual void RunStart()
		{
			if ( !AbilityAuthorized // if the ability is not permitted
			     || (!_controller.State.IsGrounded) // or if we're not grounded
			     || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal) // or if we're not in normal conditions
			     || (_movement.CurrentState != CharacterStates.MovementStates.Walking) ) // or if we're not walking
			{
				// we do nothing and exit
				return;
			}
			
			// if the player presses the run button and if we're on the ground and not crouching and we can move freely, 
			// then we change the movement speed in the controller's parameters.
			if (_characterHorizontalMovement != null)
			{
				_characterHorizontalMovement.MovementSpeed = RunSpeed;
			}

			// if we're not already running, we trigger our sounds
			if (_movement.CurrentState != CharacterStates.MovementStates.Running)
			{
				PlayAbilityStartFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Run, MMCharacterEvent.Moments.Start);
			}

			_movement.ChangeState(CharacterStates.MovementStates.Running);
		}
		
		/// <summary>
		/// Causes the character to stop running.
		/// </summary>
		public virtual void RunStop()
		{
			// if the run button is released, we revert back to the walking speed.
			if ((_characterHorizontalMovement != null) && (_movement.CurrentState != CharacterStates.MovementStates.Crouching))
			{
				_characterHorizontalMovement.ResetHorizontalSpeed ();
			}
			if (_movement.CurrentState == CharacterStates.MovementStates.Running)
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}
			StopFeedbacks ();
		}

		/// <summary>
		/// Forces run state or not (if we're not in ReadInput mode)
		/// </summary>
		/// <param name="state"></param>
		public virtual void ForceRun(bool state)
		{
			ShouldRun = state;
		}

		/// <summary>
		/// Stops all run feedbacks
		/// </summary>
		protected virtual void StopFeedbacks()
		{
			if (_startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Run, MMCharacterEvent.Moments.End);
			}            
		}
        
		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_runningAnimationParameterName, AnimatorControllerParameterType.Bool, out _runningAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our Running status to the character's animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _runningAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Running), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			if (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			{
				RunStop();
			}
			if (_animator != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _runningAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);	
			}
		}
	}
}