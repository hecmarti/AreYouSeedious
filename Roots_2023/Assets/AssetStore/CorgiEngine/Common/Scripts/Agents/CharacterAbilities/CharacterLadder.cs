using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this component to a Character and it'll be able to go up and down ladders.
	/// Animator parameters : LadderClimbing (bool), LadderClimbingSpeed (float)
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Ladder")] 
	public class CharacterLadder : CharacterAbility 
	{
		[Header("Ladder Climbing")]
		/// the speed of the character when climbing a ladder
		[Tooltip("the speed of the character when climbing a ladder")]
		public float LadderClimbingSpeed = 2f;
		/// force face right when on a ladder (useful for 3D characters)
		[Tooltip("force face right when on a ladder (useful for 3D characters)")]
		public bool ForceRightFacing = false;
		/// forces the character to teleport to the ladder platform when reaching the ladder's top
		[Tooltip("forces the chaForceTeleportOnExitracter to teleport to the ladder platform when reaching the ladder's top")]
		public bool ForceAnchorToGroundOnExit = false;
		/// the time (in seconds) between jumping from a ladder and being able to climb another one (or the same one)
		[Tooltip("the time (in seconds) between jumping from a ladder and being able to climb another one (or the same one)")]
		public float MinimumTimeAfterLadderJump = 0.5f;
		
		/// the current ladder climbing speed of the character
		public Vector2 CurrentLadderClimbingSpeed{get; set;}
		/// true if the character is colliding with a ladder
		public bool LadderColliding
		{
			get
			{
				return (_colliders.Count > 0);
			}
		}
		/// the ladder the character is currently on
		public Ladder CurrentLadder { get; set; }
		/// the highest ladder the character is currently colliding with
		public Ladder HighestLadder { get; set; }
		/// the lowest ladder the character is currently colliding with
		public Ladder LowestLadder { get; set; }
		
		const float _climbingDownInitialYTranslation = 0.1f;
		const float _ladderTopSkinHeight = 0.01f;

		protected BoxCollider2D _boxCollider;
		protected List<Collider2D> _colliders;
		protected CharacterHandleWeapon _characterHandleWeapon;

		// animation parameters
		protected const string _ladderClimbingUpAnimationParameterName = "LadderClimbing";
		protected const string _ladderClimbingSpeedXAnimationParameterName = "LadderClimbingSpeedX";
		protected const string _ladderClimbingSpeedYpAnimationParameterName = "LadderClimbingSpeedY";
		protected int _ladderClimbingUpAnimationParameter;
		protected int _ladderClimbingSpeedXAnimationParameter;
		protected int _ladderClimbingSpeedYAnimationParameter;
		protected float _lastJumpFromLadderAt = -float.MaxValue;

		/// <summary>
		/// On Start(), we initialize our various flags
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			CurrentLadderClimbingSpeed = Vector2.zero;
			_boxCollider = this.gameObject.GetComponentInParent<BoxCollider2D>();
			_colliders = new List<Collider2D>();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
		}

		/// <summary>
		/// Every frame, we check if we need to do something about ladders
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			ComputeClosestLadder();
			HandleLadderClimbing();
		}

		/// <summary>
		/// Adds a new ladder to the list of colliding ladders
		/// </summary>
		/// <param name="newCollider"></param>
		public virtual void AddCollidingLadder(Collider2D newCollider)
		{
			if (_colliders == null)
			{
				Initialization();
			}
			_colliders.Add(newCollider);
		}

		/// <summary>
		/// Removes a ladder from the list of colliding ladders
		/// </summary>
		public virtual void RemoveCollidingLadder(Collider2D newCollider)
		{
			_colliders.Remove(newCollider);
		}

		/// <summary>
		/// Determines the current, highest and lowest ladders if there are any
		/// </summary>
		protected virtual void ComputeClosestLadder()
		{
			CurrentLadder = null;
			HighestLadder = null;
			LowestLadder = null;

			if (_colliders.Count > 0)
			{
				float closestHorizontalDistance = Mathf.Infinity;
				int closestHorizontalIndex = 0;

				float highestPosition = -Mathf.Infinity;
				int highestLadderIndex = 0;

				float lowestPosition = Mathf.Infinity;
				int lowestLadderIndex = 0;

				for (int i = 0; i < _colliders.Count; i++)
				{
					float distance = Mathf.Abs(_colliders[i].bounds.center.x - _controller.BoundsCenter.x);
					float yPosition = _colliders[i].bounds.center.y;

					if (distance < closestHorizontalDistance)
					{
						closestHorizontalIndex = i;
						closestHorizontalDistance = distance;
					}

					if (yPosition > highestPosition)
					{
						highestPosition = yPosition;
						highestLadderIndex = i;
					}

					if (yPosition < lowestPosition)
					{
						lowestPosition = yPosition;
						lowestLadderIndex = i;
					}

				}
				CurrentLadder = _colliders[closestHorizontalIndex].gameObject.MMGetComponentNoAlloc<Ladder>();
				LowestLadder = _colliders[lowestLadderIndex].gameObject.MMGetComponentNoAlloc<Ladder>();
				HighestLadder = _colliders[highestLadderIndex].gameObject.MMGetComponentNoAlloc<Ladder>();
			}
		}

		/// <summary>
		/// Called at ProcessAbility(), checks if we're colliding with a ladder and if we need to do something about it
		/// </summary>	
		protected virtual void HandleLadderClimbing()
		{
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal && _condition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement ))
			{
				return;
			}

			// if the character is colliding with a ladder
			if (LadderColliding) 
			{
				if ((_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing) // if the character is climbing
				    && _controller.State.IsGrounded) // and is grounded
				{			
					// we make it get off the ladder
					GetOffTheLadder();	
				}

				if (_inputManager == null)
				{
					return;
				}

				if (_verticalInput > _inputManager.Threshold.y// if the player is pressing up
				    && (_movement.CurrentState != CharacterStates.MovementStates.LadderClimbing) // and we're not climbing a ladder already
				    && (_movement.CurrentState != CharacterStates.MovementStates.Gliding) // and we're not gliding
				    && (_movement.CurrentState != CharacterStates.MovementStates.Jetpacking)
				    && (Time.time - _lastJumpFromLadderAt >= MinimumTimeAfterLadderJump)) // and we're not jetpacking
				{			
					// then the character starts climbing
					StartClimbing();
				}	

				// if the character is climbing the ladder (which means it previously connected with it)
				if (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing)
				{
					Climbing();
				}

				if (CurrentLadder == null)
				{
					return;
				}

				// if the current ladder does have a ladder platform associated to it
				if (LowestLadder.LadderPlatform != null)
				{
					if ((_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing) // if the character is climbing
					    && AboveLadderPlatform()
					    && (LowestLadder == HighestLadder)) // and is above the ladder platform
					{			
						// we make it get off the ladder
						GetOffTheLadder();
						if (ForceAnchorToGroundOnExit)
						{
							_controller.AnchorToGround();	
						}
					}

					if ((_movement.CurrentState != CharacterStates.MovementStates.LadderClimbing) // if the character is climbing
					    && (_movement.CurrentState != CharacterStates.MovementStates.Flying)
					    && (_verticalInput < -_inputManager.Threshold.y) // and is pressing down
					    && (AboveLadderPlatform()) // and is above the ladder's platform
					    && _controller.State.IsGrounded) // and is grounded
					{
						// we make it get off the ladder
						StartClimbingDown();
					}
				}

			}
			else
			{
				// if we're not colliding with a ladder, but are still in the LadderClimbing state
				if (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing)
				{
					GetOffTheLadder();
				}
			}

			HandleFeedbacks();
		}

		protected virtual void HandleFeedbacks()
		{
			if (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing)
			{
				if ((CurrentLadderClimbingSpeed == Vector2.zero) && _startFeedbackIsPlaying)
				{
					StopStartFeedbacks();
					PlayAbilityStopFeedbacks();
					MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Ladder, MMCharacterEvent.Moments.End);
				}
				if ((CurrentLadderClimbingSpeed != Vector2.zero) && !_startFeedbackIsPlaying)
				{
					PlayAbilityStartFeedbacks();
					MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Ladder, MMCharacterEvent.Moments.Start);
				}
			}
			else
			{
				if (_startFeedbackIsPlaying)
				{
					StopStartFeedbacks();
					PlayAbilityStopFeedbacks();
					MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Ladder, MMCharacterEvent.Moments.End);
				}                
			}
		}

		/// <summary>
		/// Puts the character on the ladder
		/// </summary>
		protected virtual void StartClimbing()
		{
			if (CurrentLadder.LadderPlatform != null)
			{
				if (AboveLadderPlatform()
				    && (LowestLadder == HighestLadder)) 
				{
					return;
				}
			}

			// we rotate our character if requested
			if (ForceRightFacing)
			{
				_character.Face(Character.FacingDirections.Right);
			}

			SetClimbingState();

			// we set collisions
			_controller.CollisionsOn();

			if ((_characterHandleWeapon != null) && (!_characterHandleWeapon.CanShootFromLadders))
			{
				_characterHandleWeapon.ForceStop();
			}

			if (CurrentLadder.CenterCharacterOnLadder)
			{
				_controller.SetTransformPosition(new Vector3(CurrentLadder.transform.position.x, _controller.transform.position.y, _controller.transform.position.z));
			}
		}

		/// <summary>
		/// Puts the character on the ladder if it's standing on top of it
		/// </summary>
		protected virtual void StartClimbingDown()
		{
			SetClimbingState();
			_controller.CollisionsOff();
			_controller.ResetColliderSize ();
			_controller.GravityActive(false);

			// we rotate our character if requested
			if (ForceRightFacing)
			{
				_character.Face(Character.FacingDirections.Right);
			}

			if (_characterGravity != null)
			{
				if (_characterGravity.ShouldReverseInput ())
				{
					if (LowestLadder.CenterCharacterOnLadder)
					{
						_controller.SetTransformPosition(new Vector3(LowestLadder.transform.position.x, _controller.transform.position.y + _climbingDownInitialYTranslation, _controller.transform.position.z));
					}
					else
					{
						_controller.SetTransformPosition(new Vector3(transform.position.x, _controller.transform.position.y + _climbingDownInitialYTranslation, _controller.transform.position.z));
					}
					return;
				}
			}

			// we force its position to be a bit lower 
			if (LowestLadder.CenterCharacterOnLadder)
			{
				_controller.SetTransformPosition(new Vector3(LowestLadder.transform.position.x, _controller.transform.position.y - _climbingDownInitialYTranslation, _controller.transform.position.z));
			}
			else
			{
				_controller.SetTransformPosition(new Vector3(transform.position.x, _controller.transform.position.y - _climbingDownInitialYTranslation, _controller.transform.position.z));
			}
		}

		/// <summary>
		/// Sets the various flags and states 
		/// </summary>
		protected virtual void SetClimbingState()
		{

			// we set its state to LadderClimbing
			_movement.ChangeState(CharacterStates.MovementStates.LadderClimbing);			
			// it can't move freely anymore
			_condition.ChangeState(CharacterStates.CharacterConditions.ControlledMovement);
			// we initialize the ladder climbing speed to zero
			CurrentLadderClimbingSpeed = Vector2.zero;
			// we make sure the controller won't move
			_controller.SetHorizontalForce(0);
			_controller.SetVerticalForce(0);
			// we disable the gravity
			_controller.GravityActive(false);

		}

		/// <summary>
		/// Handles movement on the ladder
		/// </summary>
		protected virtual void Climbing()
		{
			// we disable the gravity
			_controller.GravityActive(false);

			if (CurrentLadder.LadderPlatform != null)
			{
				if (!AboveLadderPlatform())
				{
					_controller.CollisionsOn();
				}
			}
			else
			{
				_controller.CollisionsOn();
			}				
			
			// we set the force according to the ladder climbing speed
			if (CurrentLadder.LadderType == Ladder.LadderTypes.Simple)
			{
				_controller.SetVerticalForce(_verticalInput * LadderClimbingSpeed);
				// we set the climbing speed state.
				CurrentLadderClimbingSpeed = Mathf.Abs(_verticalInput ) * transform.up;	
			}
			if (CurrentLadder.LadderType == Ladder.LadderTypes.BiDirectional)
			{
				_controller.SetHorizontalForce(_horizontalInput * LadderClimbingSpeed);
				_controller.SetVerticalForce(_verticalInput * LadderClimbingSpeed);
				CurrentLadderClimbingSpeed = Mathf.Abs(_horizontalInput ) * transform.right;	
				CurrentLadderClimbingSpeed += Mathf.Abs(_verticalInput ) * (Vector2)transform.up;	
			}

		}

		/// <summary>
		/// Resets various states so that the Character isn't climbing anymore
		/// </summary>
		public virtual void GetOffTheLadder()
		{
			// we make it stop climbing, it has reached the ground.
			_condition.ChangeState(CharacterStates.CharacterConditions.Normal);
			_movement.ChangeState(CharacterStates.MovementStates.Idle);
			CurrentLadderClimbingSpeed = Vector2.zero;	
			_controller.GravityActive(true);	
			_controller.CollisionsOn();
			PlayAbilityStopFeedbacks();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Ladder, MMCharacterEvent.Moments.End);
			if (_characterHorizontalMovement != null)
			{
				_characterHorizontalMovement.ResetHorizontalSpeed();
			}			
		}

		/// <summary>
		/// Determines if the player is above the ladder's platform (usually positioned near the top)
		/// </summary>
		/// <returns><c>true</c>, if the player is above the ladder's platform, <c>false</c> otherwise.</returns>
		protected virtual bool AboveLadderPlatform()
		{
			// we make sure we have a current ladder and that it has a ladder platform associated to it
			if (LowestLadder == null)
			{
				return false;
			}
			if (LowestLadder.LadderPlatform == null)
			{
				return false;
			}

			float ladderColliderY = 0;

			if (LowestLadder.LadderPlatformBoxCollider2D != null)
			{
				ladderColliderY = LowestLadder.LadderPlatformBoxCollider2D.bounds.center.y + LowestLadder.LadderPlatformBoxCollider2D.bounds.extents.y ;
			}
			if (LowestLadder.LadderPlatformEdgeCollider2D != null)
			{
				ladderColliderY = LowestLadder.LadderPlatform.transform.position.y 
				                  + LowestLadder.LadderPlatformEdgeCollider2D.offset.y ;
			}

			bool conditionAboveLadderPlatform = (ladderColliderY < _controller.ColliderBottomPosition.y + _ladderTopSkinHeight);

			if (_characterGravity != null)
			{
				if (_characterGravity.ShouldReverseInput())
				{
					if (LowestLadder.LadderPlatformBoxCollider2D != null)
					{
						ladderColliderY = LowestLadder.LadderPlatformBoxCollider2D.bounds.center.y - LowestLadder.LadderPlatformBoxCollider2D.bounds.extents.y ;
					}

					if (LowestLadder.LadderPlatformEdgeCollider2D != null)
					{
						ladderColliderY = LowestLadder.LadderPlatform.transform.position.y 
						                  - LowestLadder.LadderPlatformEdgeCollider2D.offset.y ;
					}
					conditionAboveLadderPlatform = (ladderColliderY > _controller.ColliderTopPosition.y - _ladderTopSkinHeight);
				}	
			}

			// if the bottom of the player's collider is above the ladder platform, we return true
			if (conditionAboveLadderPlatform)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Called externally when jumping while on a ladder
		/// </summary>
		public virtual void JumpFromLadder()
		{
			_lastJumpFromLadderAt = Time.time;
		}

		/// <summary>
		/// When the character dies, we make sure it gets off the ladder first
		/// </summary>
		protected override void OnDeath()
		{
			base.OnDeath();
			GetOffTheLadder();
		}
        
		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_ladderClimbingUpAnimationParameterName, AnimatorControllerParameterType.Bool, out _ladderClimbingUpAnimationParameter);
			RegisterAnimatorParameter (_ladderClimbingSpeedXAnimationParameterName, AnimatorControllerParameterType.Float, out _ladderClimbingSpeedXAnimationParameter);
			RegisterAnimatorParameter (_ladderClimbingSpeedYpAnimationParameterName, AnimatorControllerParameterType.Float, out _ladderClimbingSpeedYAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we update our animator with our various states
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _ladderClimbingUpAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _ladderClimbingSpeedXAnimationParameter, CurrentLadderClimbingSpeed.x,_character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _ladderClimbingSpeedYAnimationParameter, CurrentLadderClimbingSpeed.y,_character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			if ( (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			     || (_condition.CurrentState == CharacterStates.CharacterConditions.ControlledMovement) )
			{
				GetOffTheLadder();	
			}
			Initialization();

			if (_animator != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _ladderClimbingUpAnimationParameter, false, _character._animatorParameters);    
			}
		}
	}
}