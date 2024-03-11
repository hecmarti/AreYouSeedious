using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a character and it'll be able to "roll" along surfaces, with options to go through enemies, and keep controlling direction
	/// Animator parameters : Rolling, StartedRolling
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Roll")] 
	public class CharacterRoll : CharacterAbility
	{		
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "Add this class to a character and it'll be able to 'roll' along surfaces, with options to go through enemies, and keep controlling direction."; }

		[Header("Roll")]

		/// the duration of the roll, in seconds
		[Tooltip("the duration of the roll, in seconds")]
		public float RollDuration = 0.5f;
		/// the speed of the roll (a multiplier of the regular walk speed)
		[Tooltip("the speed of the roll, a multiplier of the regular walk speed")]
		public float RollSpeed = 3f;
		/// if this is true, horizontal input won't be read, and the character won't be able to change direction during a roll
		[Tooltip("if this is true, horizontal input won't be read, and the character won't be able to change direction during a roll")]
		public bool BlockHorizontalInput = false;
		/// if this is true, no damage will be applied during the roll, and the character will be able to go through enemies
		[Tooltip("if this is true, no damage will be applied during the roll, and the character will be able to go through enemies")]
		public bool PreventDamageCollisionsDuringRoll = false;

		[Header("Direction")]

		/// the roll's aim properties
		[Tooltip("the roll's aim properties")]
		public MMAim Aim;
		/// the minimum amount of input required to apply a direction to the roll
		[Tooltip("the minimum amount of input required to apply a direction to the roll")]
		public float MinimumInputThreshold = 0.1f;
		/// if this is true, the character will flip when rolling and facing the roll's opposite direction
		[Tooltip("if this is true, the character will flip when rolling and facing the roll's opposite direction")]
		public bool FlipCharacterIfNeeded = true;

		public enum SuccessiveRollsResetMethods { Grounded, Time }

		[Header("Cooldown")]
		/// the duration of the cooldown between 2 rolls (in seconds)
		[Tooltip("the duration of the cooldown between 2 rolls (in seconds)")]
		public float RollCooldown = 1f;

		[Header("Uses")]
		/// whether or not rolls can be performed infinitely
		[Tooltip("whether or not rolls can be performed infinitely")]
		public bool LimitedRolls = false;
		/// the amount of successive rolls a character can perform, only if rolls are not infinite
		[Tooltip("the amount of successive rolls a character can perform, only if rolls are not infinite")]
		[MMCondition("LimitedRolls", true)]
		public int SuccessiveRollsAmount = 1;
		/// the amount of rollss left (runtime value only), only if rolls are not infinite
		[Tooltip("the amount of rolls left (runtime value only), only if rolls are not infinite")]
		[MMCondition("LimitedRolls", true)]
		[MMReadOnly]
		public int SuccessiveRollsLeft = 1;
		/// when in time reset mode, the duration, in seconds, after which the amount of rolls left gets reset, only if rolls are not infinite
		[Tooltip("when in time reset mode, the duration, in seconds, after which the amount of rolls left gets reset, only if rolls are not infinite")]
		[MMCondition("LimitedRolls", true)]
		public float SuccessiveRollsResetDuration = 2f;

		protected float _cooldownTimeStamp = 0;
		protected Vector2 _rollDirection;
		protected bool _shouldKeepRolling = true;
		protected IEnumerator _rollCoroutine;
		protected float _lastRollAt = 0f;
		protected float _currentDirection;
		protected float _drivenInput;
		protected float _originalMultiplier = 1f;
		protected bool _originalInvulnerability = true;

		// animation parameters
		protected const string _rollingAnimationParameterName = "Rolling";
		protected int _rollingAnimationParameter;
		protected const string _startedRollingAnimationParameterName = "StartedRolling";
		protected int _startedRollingAnimationParameter;
        

		/// <summary>
		/// Initializes our aim instance
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			Aim.Initialization();
			SuccessiveRollsLeft = SuccessiveRollsAmount;
		}

		/// <summary>
		/// At the start of each cycle, we check if we're pressing the roll button. If we
		/// </summary>
		protected override void HandleInput()
		{
			if (_inputManager.RollButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				StartRoll();
			}
		}

		/// <summary>
		/// The second of the 3 passes you can have in your ability. Think of it as Update()
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();

			HandleAmountOfRollsLeft();
		}

		/// <summary>
		/// Causes the character to roll or dive (depending on the vertical movement at the start of the roll)
		/// </summary>
		public virtual void StartRoll()
		{
			if (!RollAuthorized())
			{
				return; 
			}

			if (!RollConditions())
			{
				return;
			}

			InitiateRoll();
		}

		/// <summary>
		/// This method evaluates the internal conditions for a roll (cooldown between rolls, amount of rolls left) and returns true if a roll can be performed, false otherwise
		/// </summary>
		/// <returns></returns>
		public virtual bool RollConditions()
		{
			// if we're in cooldown between two rolls, we prevent roll
			if (_cooldownTimeStamp > Time.time)
			{
				return false;
			}

			// if we don't have rolls left, we prevent roll
			if (SuccessiveRollsLeft <= 0)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if conditions are met to reset the amount of rolls left
		/// </summary>
		protected virtual void HandleAmountOfRollsLeft()
		{
			if ((SuccessiveRollsLeft >= SuccessiveRollsAmount) || (Time.time - _lastRollAt < RollCooldown))
			{
				return;
			}

			if (Time.time - _lastRollAt > SuccessiveRollsResetDuration)
			{
				SetSuccessiveRollsLeft(SuccessiveRollsAmount);
			}
		}

		/// <summary>
		/// A method to reset the amount of successive rolls left
		/// </summary>
		/// <param name="newAmount"></param>
		public virtual void SetSuccessiveRollsLeft(int newAmount)
		{
			SuccessiveRollsLeft = newAmount;
		}

		/// <summary>
		/// This method evaluates the external conditions (state, other abilities) for a roll, and returns true if a roll can be performed, false otherwise
		/// </summary>
		/// <returns></returns>
		public virtual bool RollAuthorized()
		{
			// if the roll action is enabled in the permissions, we continue, if not we do nothing
			if (!AbilityAuthorized
			    || (!_controller.State.IsGrounded)
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			    || (_movement.CurrentState == CharacterStates.MovementStates.LedgeHanging)
			    || (_movement.CurrentState == CharacterStates.MovementStates.Gripping))
				return false;

			return true;
		}

		/// <summary>
		/// initializes all parameters prior to a roll and triggers the pre roll feedbacks
		/// </summary>
		public virtual void InitiateRoll()
		{
			// we set its rolling state to true
			_movement.ChangeState(CharacterStates.MovementStates.Rolling);
            
			// we start our sounds
			PlayAbilityStartFeedbacks();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Roll, MMCharacterEvent.Moments.Start);

			// we initialize our various counters and checks
			_shouldKeepRolling = true;
			_cooldownTimeStamp = Time.time + RollCooldown;
			_lastRollAt = Time.time;
			_originalMultiplier = _characterHorizontalMovement.AbilityMovementSpeedMultiplier;
			if (PreventDamageCollisionsDuringRoll)
			{
				if (_health != null)
				{
					_originalInvulnerability = _health.Invulnerable;    
					_health.Invulnerable = true;    
				}
			}
            
			if (LimitedRolls)
			{
				SuccessiveRollsLeft -= 1;
			}            

			ComputeRollDirection();
			CheckFlipCharacter();

			// we launch the boost corountine with the right parameters
			_rollCoroutine = RollCoroutine();
			StartCoroutine(_rollCoroutine);
		}

		/// <summary>
		/// Computes the roll direction based on the selected options
		/// </summary>
		protected virtual void ComputeRollDirection()
		{
			// we compute our direction
			if (_character.LinkedInputManager != null)
			{
				Aim.PrimaryMovement = _character.LinkedInputManager.PrimaryMovement;
				Aim.SecondaryMovement = _character.LinkedInputManager.SecondaryMovement;
			}
            
			Aim.CurrentPosition = this.transform.position;
			_rollDirection = Aim.GetCurrentAim();
            
			if (_rollDirection.magnitude < MinimumInputThreshold)
			{
				_rollDirection = _character.IsFacingRight ? Vector2.right : Vector2.left;
			}
			else
			{
				_rollDirection = _rollDirection.normalized;
			}
            
			_currentDirection = _rollDirection.x > 0f ? 1f : -1f;
		}

		/// <summary>
		/// Checks whether or not a character flip is required, and flips the character if needed
		/// </summary>
		protected virtual void CheckFlipCharacter()
		{
			// we flip the character if needed
			if (FlipCharacterIfNeeded && (Mathf.Abs(_rollDirection.x) > 0.05f))
			{
				if (_character.IsFacingRight != (_rollDirection.x > 0f))
				{
					_character.Flip();
				}
			}
		}


		/// <summary>
		/// Coroutine used to move the player in a direction over time
		/// </summary>
		protected virtual IEnumerator RollCoroutine()
		{
			// if the character is not in a position where it can move freely, we do nothing.
			if ( !AbilityAuthorized
			     || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				yield break;
			}

			_characterHorizontalMovement.ReadInput = false;
			_characterHorizontalMovement.AbilityMovementSpeedMultiplier = RollSpeed;

			if (_animator != null)
			{
				MMAnimatorExtensions.SetAnimatorTrigger(_animator, _startedRollingAnimationParameter,
					_character._animatorParameters, _character.PerformAnimatorSanityChecks);    
			}

			float rollStartedAt = Time.time;

			// we keep rolling until we've reached our target distance or until we get interrupted
			while ( (Time.time - rollStartedAt < RollDuration) 
			        && _shouldKeepRolling 
			        && !_controller.State.TouchingLevelBounds
			        && _movement.CurrentState == CharacterStates.MovementStates.Rolling)
			{
				if (!BlockHorizontalInput)
				{
					_drivenInput = _horizontalInput;
				}
            
				bool gravityShouldReverseInput = false;
				if (_characterGravity != null)
				{
					gravityShouldReverseInput = _characterGravity.ShouldReverseInput();
				}

				if (_drivenInput != 0f)
				{
					_drivenInput = gravityShouldReverseInput ? -_drivenInput : _drivenInput;
					_currentDirection = (_drivenInput < 0f) ? -1f : 1f;
				}

				_characterHorizontalMovement.SetHorizontalMove(gravityShouldReverseInput ? -_currentDirection : _currentDirection);
	            
				yield return null;
			}
			StopRoll();				
		}

		/// <summary>
		/// Stops the roll coroutine and resets all necessary parts of the character
		/// </summary>
		public virtual void StopRoll()
		{
			_characterHorizontalMovement.ReadInput = true;
			_characterHorizontalMovement.AbilityMovementSpeedMultiplier = _originalMultiplier;
	        
			if (PreventDamageCollisionsDuringRoll)
			{
				if (_health != null)
				{
					_health.Invulnerable = _originalInvulnerability;    
				}
			}
	        
			if (_rollCoroutine != null)
			{
				StopCoroutine(_rollCoroutine);    
			}

			// we play our exit sound
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Roll, MMCharacterEvent.Moments.End);

			// once the boost is complete, if we were rolling, we make it stop and start the roll cooldown
			if (_movement.CurrentState == CharacterStates.MovementStates.Rolling)
			{
				if (_controller.State.IsGrounded)
				{
					_movement.ChangeState(CharacterStates.MovementStates.Idle);
				}
				else
				{
					_movement.RestorePreviousState();
				}                
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_rollingAnimationParameterName, AnimatorControllerParameterType.Bool, out _rollingAnimationParameter);
			RegisterAnimatorParameter(_startedRollingAnimationParameterName, AnimatorControllerParameterType.Trigger, out _startedRollingAnimationParameter);
		}

		/// <summary>
		/// At the end of the cycle, we update our animator's Rolling state 
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _rollingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Rolling), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			if (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			{
				StopRoll();	
			}
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _rollingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}
	}
}