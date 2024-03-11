using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using UnityEngine.Serialization;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this class to a character and it'll be able to jump
	/// Animator parameters : Jumping (bool), DoubleJumping (bool), HitTheGround (bool)
	/// </summary>
	[MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Jump")] 
	public class CharacterJump : CharacterAbility
	{	
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component handles jumps. Here you can define the jump height, whether the jump is proportional to the press length or not, the minimum air time (how long a character should stay in the air before being able to go down if the player has released the jump button), a jump window duration (the time during which, after falling off a cliff, a jump is still possible), jump restrictions, how many jumps the character can perform without touching the ground again, and how long collisions should be disabled when exiting 1-way platforms or moving platforms."; }

		/// the possible jump restrictions
		public enum JumpBehavior
		{
			CanJumpOnGround,
			CanJumpOnGroundAndFromLadders,
			CanJumpAnywhere,
			CantJump,
			CanJumpAnywhereAnyNumberOfTimes
		}

		[Header("Jump Behaviour")]

		/// the maximum number of jumps allowed (0 : no jump, 1 : normal jump, 2 : double jump, etc...)
		[Tooltip("the maximum number of jumps allowed (0 : no jump, 1 : normal jump, 2 : double jump, etc...)")]
		public int NumberOfJumps = 2;
		/// defines how high the character can jump
		[Tooltip("defines how high the character can jump")]
		public float JumpHeight = 3.025f;
		/// basic rules for jumps : where can the player jump ?
		[Tooltip("basic rules for jumps : where can the player jump ?")]
		public JumpBehavior JumpRestrictions = JumpBehavior.CanJumpAnywhere;
		/// if this is true, camera offset will be reset on jump
		[Tooltip("if this is true, camera offset will be reset on jump")]
		public bool ResetCameraOffsetOnJump = false;
		/// if this is true, this character can jump down one way platforms by doing down + jump
		[Tooltip("if this is true, this character can jump down one way platforms by doing down + jump")]
		public bool CanJumpDownOneWayPlatforms = true;

		[Header("Proportional jumps")]

		/// if true, the jump duration/height will be proportional to the duration of the button's press
		[Tooltip("if true, the jump duration/height will be proportional to the duration of the button's press")]
		public bool JumpIsProportionalToThePressTime = true;
		/// the minimum time in the air allowed when jumping - this is used for pressure controlled jumps
		[Tooltip("the minimum time in the air allowed when jumping - this is used for pressure controlled jumps")]
		public float JumpMinimumAirTime = 0.1f;
		/// the amount by which we'll modify the current speed when the jump button gets released
		[Tooltip("the amount by which we'll modify the current speed when the jump button gets released")]
		public float JumpReleaseForceFactor = 2f;
		
		[Header("Quality of Life")]
		
		/// a timeframe during which, after leaving the ground, the character can still trigger a jump
		[Tooltip("a timeframe during which, after leaving the ground, the character can still trigger a jump")]
		public float CoyoteTime = 0f;
		
		/// if the character lands, and the jump button's been pressed during that InputBufferDuration, a new jump will be triggered 
		[Tooltip("if the character lands, and the jump button's been pressed during that InputBufferDuration, a new jump will be triggered")]
		public float InputBufferDuration = 0f;

		[Header("Collisions")]

		/// duration (in seconds) we need to disable collisions when jumping down a 1 way platform
		[Tooltip("duration (in seconds) we need to disable collisions when jumping down a 1 way platform")]
		public float OneWayPlatformsJumpCollisionOffDuration = 0.3f;
		/// duration (in seconds) we need to disable collisions when jumping off a moving platform
		[Tooltip("duration (in seconds) we need to disable collisions when jumping off a moving platform")]
		public float MovingPlatformsJumpCollisionOffDuration = 0.05f;

		[Header("Feedbacks")]

		/// the MMFeedbacks to play when jumping in the air
		[Tooltip("the MMFeedbacks to play when jumping in the air")]
		public MMFeedbacks AirJumpFeedbacks;
		/// the MMFeedbacks to play when double jumping
		[Tooltip("the MMFeedbacks to play when double jumping")]
		public MMFeedbacks DoubleJumpFeedbacks;

		/// the number of jumps left to the character
		[MMReadOnly]
		[Tooltip("the number of jumps left to the character")]
		public int NumberOfJumpsLeft;

		/// whether or not the jump happened this frame
		public bool JumpHappenedThisFrame { get; set;  }
		/// whether or not the jump can be stopped
		public bool CanJumpStop { get; set; }

		protected float _jumpButtonPressTime = 0;
		protected float _lastJumpAt = 0;
		protected bool _jumpButtonPressed = false;
		protected bool _jumpButtonReleased = false;
		protected bool _doubleJumping = false;
		protected CharacterWalljump _characterWallJump = null;
		protected CharacterCrouch _characterCrouch = null;
		protected CharacterButtonActivation _characterButtonActivation = null;
		protected CharacterLadder _characterLadder = null;
		protected int _initialNumberOfJumps;
		protected float _lastTimeGrounded = 0f;
		protected float _lastInputBufferJumpAt = 0f;
		protected bool _coyoteTime = false;
		protected bool _inputBuffer = false;
		protected bool _jumpingDownFromOneWayPlatform = false;
                
		// animation parameters
		protected const string _jumpingAnimationParameterName = "Jumping";
		protected const string _doubleJumpingAnimationParameterName = "DoubleJumping";
		protected const string _hitTheGroundAnimationParameterName = "HitTheGround";
		protected int _jumpingAnimationParameter;
		protected int _doubleJumpingAnimationParameter;
		protected int _hitTheGroundAnimationParameter;

		/// Evaluates the jump restrictions
		public virtual bool JumpAuthorized 
		{ 
			get 
			{ 
				if (EvaluateJumpTimeWindow())
				{
					return true;
				}

				if (_movement.CurrentState == CharacterStates.MovementStates.SwimmingIdle) 
				{
					return false;
				}

				if ( (JumpRestrictions == JumpBehavior.CanJumpAnywhere) ||  (JumpRestrictions == JumpBehavior.CanJumpAnywhereAnyNumberOfTimes) )
				{
					return true;
				}					

				if (JumpRestrictions == JumpBehavior.CanJumpOnGround)
				{
					if (_controller.State.IsGrounded
					    || (_movement.CurrentState == CharacterStates.MovementStates.Gripping)
					    || (_movement.CurrentState == CharacterStates.MovementStates.LedgeHanging))
					{
						return true;
					}
					else
					{
						// if we've already made a jump and that's the reason we're in the air, then yes we can jump
						if (NumberOfJumpsLeft < NumberOfJumps)
						{
							return true;
						}
					}
				}				

				if (JumpRestrictions == JumpBehavior.CanJumpOnGroundAndFromLadders)
				{
					if ((_controller.State.IsGrounded)
					    || (_movement.CurrentState == CharacterStates.MovementStates.Gripping)
					    || (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing)
					    || (_movement.CurrentState == CharacterStates.MovementStates.LedgeHanging))
					{
						return true;
					}
					else
					{
						// if we've already made a jump and that's the reason we're in the air, then yes we can jump
						if (NumberOfJumpsLeft < NumberOfJumps)
						{
							return true;
						}
					}
				}					
				
				return false; 
			}
		}

		/// <summary>
		/// On Start() we reset our number of jumps
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			ResetNumberOfJumps();
			_characterWallJump = _character?.FindAbility<CharacterWalljump>();
			_characterCrouch = _character?.FindAbility<CharacterCrouch>();
			_characterButtonActivation = _character?.FindAbility<CharacterButtonActivation>();
			_characterLadder = _character?.FindAbility<CharacterLadder>();
			ResetInitialNumberOfJumps();
			CanJumpStop = true;
		}	
        
		/// <summary>
		/// Stores the current NumberOfJumps
		/// </summary>
		protected virtual void ResetInitialNumberOfJumps()
		{        
			_initialNumberOfJumps = NumberOfJumps;
		}
        
		/// <summary>
		/// At the beginning of each cycle we check if we've just pressed or released the jump button
		/// </summary>
		protected override void HandleInput()
		{
			// we handle regular button presses
			if (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				JumpStart();
			}
			
			// we handle input buffer
			if ((InputBufferDuration > 0f) && (_controller.State.JustGotGrounded))
			{
				if ((_inputManager.JumpButton.TimeSinceLastButtonDown < InputBufferDuration) && (Time.time - _lastJumpAt > InputBufferDuration) && !_jumpingDownFromOneWayPlatform) 
				{
					NumberOfJumpsLeft = NumberOfJumps;	
					_doubleJumping = false;
					_inputBuffer = true;
					_jumpButtonPressed = (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed);
					_jumpButtonPressTime = Time.time;
					_jumpButtonReleased = (_inputManager.JumpButton.State.CurrentState != MMInput.ButtonStates.ButtonPressed);
					_lastInputBufferJumpAt = Time.time;
					JumpStart();
				}
			}
			
			// we handle button release
			if (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
			{
				JumpStop();
			}
		}	

		/// <summary>
		/// Every frame we perform a number of checks related to jump
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			JumpHappenedThisFrame = false;

			if (!AbilityAuthorized) { return; }

			// if we just got grounded, we reset our number of jumps
			if (_controller.State.JustGotGrounded && !_inputBuffer)
			{
				NumberOfJumpsLeft = NumberOfJumps;	
				_doubleJumping = false;
				_jumpingDownFromOneWayPlatform = false;
			}
			
			// if we're grounded, and have jumped a while back but still haven't gotten our jumps back, we reset them
			if ((_controller.State.IsGrounded) && (Time.time - _lastJumpAt > JumpMinimumAirTime) && (NumberOfJumpsLeft < NumberOfJumps) && !_inputBuffer)
			{
				ResetNumberOfJumps();
			}

			// we store the last timestamp at which the character was grounded
			if (_controller.State.IsGrounded)
			{
				_lastTimeGrounded = Time.time;
			}

			// If the user releases the jump button and the character is jumping up and enough time since the initial jump has passed, then we make it stop jumping by applying a force down.
			if ( (_jumpButtonPressTime != 0) 
			     && (Time.time - _jumpButtonPressTime >= JumpMinimumAirTime) 
			     && (_controller.Speed.y > Mathf.Sqrt(Mathf.Abs(_controller.Parameters.Gravity))) 
			     && (_jumpButtonReleased)
			     && ( !_jumpButtonPressed
			          || (_movement.CurrentState == CharacterStates.MovementStates.Jetpacking)))
			{
				_jumpButtonReleased = false;	
				if (JumpIsProportionalToThePressTime)	
				{	
					_jumpButtonPressTime = 0;
					if (JumpReleaseForceFactor == 0f)
					{
						_controller.SetVerticalForce (0f);
					}
					else
					{
						_controller.AddVerticalForce(-_controller.Speed.y/JumpReleaseForceFactor);	
					}
				}
			}

			UpdateController();
            
			_inputBuffer = false;
		}

		/// <summary>
		/// Determines if whether or not a Character is still in its Jump Window (the delay during which, after falling off a cliff, a jump is still possible without requiring multiple jumps)
		/// </summary>
		/// <returns><c>true</c>, if jump time window was evaluated, <c>false</c> otherwise.</returns>
		protected virtual bool EvaluateJumpTimeWindow()
		{
			_coyoteTime = false;

			if (_movement.CurrentState == CharacterStates.MovementStates.Jumping 
			    || _movement.CurrentState == CharacterStates.MovementStates.DoubleJumping
			    || _movement.CurrentState == CharacterStates.MovementStates.WallJumping)
			{
				return false;
			}

			if (Time.time - _lastTimeGrounded <= CoyoteTime)
			{
				_coyoteTime = true;
				return true;
			}
			else 
			{
				return false;
			}
		}

		/// <summary>
		/// Evaluates the jump conditions to determine whether or not a jump can occur
		/// </summary>
		/// <returns><c>true</c>, if jump conditions was evaluated, <c>false</c> otherwise.</returns>
		protected virtual bool EvaluateJumpConditions()
		{
			bool onAOneWayPlatform = false;
			if (_controller.StandingOn != null)
			{
				onAOneWayPlatform = (_controller.OneWayPlatformMask.MMContains(_controller.StandingOn.layer)
				                     || _controller.MovingOneWayPlatformMask.MMContains(_controller.StandingOn.layer));
			}

			if ( !AbilityAuthorized  // if the ability is not permitted
			     || !JumpAuthorized // if jumps are not authorized right now
			     || (!_controller.CanGoBackToOriginalSize() && !onAOneWayPlatform)
			     || ((_condition.CurrentState != CharacterStates.CharacterConditions.Normal) // or if we're not in the normal stance
			         && (_condition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement))
			     || (_movement.CurrentState == CharacterStates.MovementStates.Jetpacking) // or if we're jetpacking
			     || (_movement.CurrentState == CharacterStates.MovementStates.Dashing) // or if we're dashing
			     || (_movement.CurrentState == CharacterStates.MovementStates.Pushing) // or if we're pushing                
			     || ((_movement.CurrentState == CharacterStates.MovementStates.WallClinging) && (_characterWallJump != null)) // or if we're wallclinging and can walljump
			     || (_controller.State.IsCollidingAbove && !onAOneWayPlatform)) // or if we're colliding with the ceiling
			{
				return false;
			}

			// if we're in a button activated zone and can interact with it
			if (_characterButtonActivation != null)
			{
				if (_characterButtonActivation.AbilityAuthorized
				    && _characterButtonActivation.PreventJumpWhenInZone
				    && _characterButtonActivation.InButtonActivatedZone
				    && !_characterButtonActivation.InButtonAutoActivatedZone)
				{
					return false;
				}
				if (_characterButtonActivation.InJumpPreventingZone)
				{
					return false;
				}
			}

			// if we're crouching and don't have enough space to stand we do nothing and exit
			if ((_movement.CurrentState == CharacterStates.MovementStates.Crouching) || (_movement.CurrentState == CharacterStates.MovementStates.Crawling))
			{				
				if (_characterCrouch != null)
				{
					if (_characterCrouch.InATunnel && (_verticalInput >= -_inputManager.Threshold.y))
					{
						return false;
					}
				}
			}

			// if we're not grounded, not on a ladder, and don't have any jumps left, we do nothing and exit
			if ((!_controller.State.IsGrounded)
			    && !EvaluateJumpTimeWindow()
			    && (_movement.CurrentState != CharacterStates.MovementStates.LadderClimbing)
			    && (JumpRestrictions != JumpBehavior.CanJumpAnywhereAnyNumberOfTimes)
			    && (NumberOfJumpsLeft <= 0))			
			{
				return false;
			}

			if (_controller.State.IsGrounded 
			    && (NumberOfJumpsLeft <= 0))
			{
				return false;
			}           

			if (_inputManager != null)
			{
				if (_jumpingDownFromOneWayPlatform)
				{
					if ((_verticalInput > -_inputManager.Threshold.y) || (_jumpButtonReleased))
					{
						_jumpingDownFromOneWayPlatform = false;
					}
				}
				
				// if the character is standing on a one way platform and is also pressing the down button,
				if (_verticalInput < -_inputManager.Threshold.y && _controller.State.IsGrounded)
				{
					if (JumpDownFromOneWayPlatform())
					{
						return false;
					}
				}

				// if the character is standing on a moving platform and not pressing the down button,
				if (_controller.State.IsGrounded)
				{
					JumpFromMovingPlatform();
				}
			}	

			return true;
		}

		/// <summary>
		/// Causes the character to start jumping.
		/// </summary>
		public virtual void JumpStart()
		{
			if (!EvaluateJumpConditions())
			{
				return;
			}
			
			// we reset our walking speed
			if ((_movement.CurrentState == CharacterStates.MovementStates.Crawling)
			    || (_movement.CurrentState == CharacterStates.MovementStates.Crouching)
			    || (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing))
			{
				_characterHorizontalMovement.ResetHorizontalSpeed();
			}	
            
			if (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing)
			{
				_characterLadder.GetOffTheLadder();
				_characterLadder.JumpFromLadder();
			}

			_controller.ResetColliderSize();

			_lastJumpAt = Time.time;

			// if we're still here, the jump will happen
			// we set our current state to Jumping
			_movement.ChangeState(CharacterStates.MovementStates.Jumping);

			// we trigger a character event
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Jump);

			// we start our feedbacks
			if ((_controller.State.IsGrounded) || _coyoteTime) 
			{
				PlayAbilityStartFeedbacks();
			}
			else
			{
				AirJumpFeedbacks?.PlayFeedbacks();
			}

			if (ResetCameraOffsetOnJump && (_sceneCamera != null))
			{
				_sceneCamera.ResetLookUpDown();
			}            

			if (NumberOfJumpsLeft != NumberOfJumps)
			{
				_doubleJumping = true;
				DoubleJumpFeedbacks?.PlayFeedbacks();
			}

			// we decrease the number of jumps left
			NumberOfJumpsLeft = NumberOfJumpsLeft - 1;

			// we reset our current condition and gravity
			if (_controller.State.IsGrounded)
			{
				_controller.TimeAirborne = 0f;
			}
			
			_condition.ChangeState(CharacterStates.CharacterConditions.Normal);
			_controller.GravityActive(true);
			_controller.CollisionsOn ();

			// we set our various jump flags and counters
			SetJumpFlags();
			CanJumpStop = true;

			// we make the character jump
			_controller.SetVerticalForce(Mathf.Sqrt( 2f * JumpHeight * Mathf.Abs(_controller.Parameters.Gravity) ));
			JumpHappenedThisFrame = true;
		}

		/// <summary>
		/// Use this method, from any class, to prevent the current jump from being proportional (releasing won't cancel the jump/current momentum)
		/// </summary>
		/// <param name="status"></param>
		public virtual void SetCanJumpStop(bool status)
		{
			CanJumpStop = status;
		}

		/// <summary>
		/// Handles jumping down from a one way platform.
		/// </summary>
		protected virtual bool JumpDownFromOneWayPlatform()
		{
			if (!CanJumpDownOneWayPlatforms || _jumpingDownFromOneWayPlatform)
			{
				return false;
			}

			// we go through all the colliders we're standing on, and if all of them are 1way, we're ok to jump down
			bool canJumpDown = true;
			foreach (GameObject obj in _controller.StandingOnArray)
			{
				if (obj == null)
				{
					continue;
				}
				if (!_controller.OneWayPlatformMask.MMContains(obj.layer) &&
				    !_controller.MovingOneWayPlatformMask.MMContains(obj.layer) &&
				    !_controller.StairsMask.MMContains(obj.layer))
				{
					canJumpDown = false;	
				}
			}
			
			if (canJumpDown)
			{
				_movement.ChangeState(CharacterStates.MovementStates.Jumping);
				_characterHorizontalMovement.ResetHorizontalSpeed();
				// we turn the boxcollider off for a few milliseconds, so the character doesn't get stuck mid platform
				StartCoroutine(_controller.DisableCollisionsWithOneWayPlatforms(OneWayPlatformsJumpCollisionOffDuration));
				_controller.DetachFromMovingPlatform();
				_jumpingDownFromOneWayPlatform = true;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Handles jumping from a moving platform.
		/// </summary>
		protected virtual void JumpFromMovingPlatform()
		{
			if (_controller.StandingOn != null)
			{
				if ( _controller.MovingPlatformMask.MMContains(_controller.StandingOn.layer)
				     || _controller.MovingOneWayPlatformMask.MMContains(_controller.StandingOn.layer) )
				{
					// we turn the boxcollider off for a few milliseconds, so the character doesn't get stuck mid air
					StartCoroutine(_controller.DisableCollisionsWithMovingPlatforms(MovingPlatformsJumpCollisionOffDuration));
					_controller.DetachFromMovingPlatform();
				}	
			}
		}
		
		/// <summary>
		/// Causes the character to stop jumping.
		/// </summary>
		public virtual void JumpStop()
		{
			if (!CanJumpStop)
			{
				return;
			}
			_jumpButtonPressed = false;
			_jumpButtonReleased = true;
		}

		/// <summary>
		/// Resets the number of jumps.
		/// </summary>
		public virtual void ResetNumberOfJumps()
		{
			NumberOfJumpsLeft = NumberOfJumps;
		}

		/// <summary>
		/// Resets jump flags
		/// </summary>
		public virtual void SetJumpFlags()
		{
			if (Time.time - _lastInputBufferJumpAt < InputBufferDuration) 
			{
				// we do nothing
			}
			else
			{
				_jumpButtonPressTime = Time.time;
				_jumpButtonReleased = false;
				_jumpButtonPressed = true;
			}
		}

		/// <summary>
		/// Updates the controller state based on our current jumping state
		/// </summary>
		protected virtual void UpdateController()
		{
			_controller.State.IsJumping = (_movement.CurrentState == CharacterStates.MovementStates.Jumping
			                               || _movement.CurrentState == CharacterStates.MovementStates.DoubleJumping
			                               || _movement.CurrentState == CharacterStates.MovementStates.WallJumping);
		}

		/// <summary>
		/// Sets the number of jumps left.
		/// </summary>
		/// <param name="newNumberOfJumps">New number of jumps.</param>
		public virtual void SetNumberOfJumpsLeft(int newNumberOfJumps)
		{
			NumberOfJumpsLeft = newNumberOfJumps;
		}

		/// <summary>
		/// Resets the jump button released flag.
		/// </summary>
		public virtual void ResetJumpButtonReleased()
		{
			_jumpButtonReleased = false;	
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_jumpingAnimationParameterName, AnimatorControllerParameterType.Bool, out _jumpingAnimationParameter);
			RegisterAnimatorParameter (_doubleJumpingAnimationParameterName, AnimatorControllerParameterType.Bool, out _doubleJumpingAnimationParameter);
			RegisterAnimatorParameter (_hitTheGroundAnimationParameterName, AnimatorControllerParameterType.Bool, out _hitTheGroundAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, sends Jumping states to the Character's animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _jumpingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Jumping),_character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _doubleJumpingAnimationParameter, _doubleJumping,_character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool (_animator, _hitTheGroundAnimationParameter, _controller.State.JustGotGrounded, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		/// <summary>
		/// Resets parameters in anticipation for the Character's respawn.
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility ();
			NumberOfJumps = _initialNumberOfJumps;
			NumberOfJumpsLeft = _initialNumberOfJumps;
		}
	}
}