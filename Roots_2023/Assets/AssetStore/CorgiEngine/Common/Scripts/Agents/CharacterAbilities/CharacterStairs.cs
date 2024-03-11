using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Serialization;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this ability to a character and it'll be able to climb stairs
	/// Animation parameter : OnStairs, boolean
	/// </summary>
	[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Stairs")]
	public class CharacterStairs : CharacterAbility
	{
		public override string HelpBoxText() { return "This component allows your character to climb stairs, which are basically one way platforms that can intersect with the ground and will be ignored unless you press up while walking towards them."; }

		[Header("Status")]

		/// true if the character is on stairs this frame, false otherwise
		[MMReadOnly]
		[Tooltip("true if the character is on stairs this frame, false otherwise")]
		public bool OnStairs = false;
		/// true if there are stairs below our character
		[MMReadOnly]
		[Tooltip("true if there are stairs below our character")]
		public bool StairsBelow = false;
		/// true if there are stairs ahead of our character
		[MMReadOnly]
		[Tooltip("true if there are stairs ahead of our character")]
		public bool StairsAhead = false;
		[MMReadOnly]
		[Tooltip("true if climbing up stairs is currently prevented by a raycast")]
		public bool ClimbingUpPrevented = false;

		[Header("Stairs detection : ahead")]
		/// the offset to apply when raycasting for stairs
		[Tooltip("the offset to apply when raycasting for stairs")]
		public Vector3 StairsAheadDetectionRaycastOrigin = new Vector3(-0.75f, 0f, 0f);
		/// the length of the raycast looking for stairs
		[Tooltip("the length of the raycast looking for stairs")]
		public float StairsAheadDetectionRaycastLength = 2f;
        
		[Header("Stairs detection : below")]
		/// the offset to apply when raycasting for stairs
		[Tooltip("the offset to apply when raycasting for stairs")]
		public Vector3 StairsBelowDetectionRaycastOrigin = new Vector3(-0.2f, 0f, 0f);
		/// the length of the raycast looking for stairs
		[Tooltip("the length of the raycast looking for stairs")]
		public float StairsBelowDetectionRaycastLength = 0.25f;
		/// the duration, in seconds, during which collisions with one way platforms should be ignored when starting to get down a stair
		[Tooltip("the duration, in seconds, during which collisions with one way platforms should be ignored when starting to get down a stair")]
		public float StairsBelowLockTime = 0.2f;
        
		[Header("Constraints")]
		/// the minimum horizontal speed at which the character must be moving to be able to start climbing stairs 
		[Tooltip("the minimum horizontal speed at which the character must be moving to be able to start climbing stairs")]
		public float MinimumSpeedToBoardStairs = 0.1f;
		/// if this is true, a ray will be cast downwards from PreventClimbingRaycastOffset, for PreventClimbingRaycastLength
		/// if that ray hits stairs, you won't be able to get up on them.
		/// usually you'll want to position this ray's origin just in front of your character's head, and have it be slightly shorter than your character's height
		[Tooltip("if this is true, a ray will be cast downwards from PreventClimbingRaycastOffset, for PreventClimbingRaycastLength " +
		         "if that ray hits stairs, you won't be able to get up on them." +
		         " usually you'll want to position this ray's origin just in front of your character's head, and have it be slightly shorter than your character's height")] 
		public bool PreventClimbingPastStairs = false;
		/// the offset to apply to the character's position to determine the prevent climbing raycast's origin
		[Tooltip("the offset to apply to the character's position to determine the prevent climbing raycast's origin")]
		[MMCondition("PreventClimbingPastStairs", true)]
		public Vector3 PreventClimbingRaycastOffset = new Vector3(0.22f, 0.2f, 0f);
		/// the length of the raycast used to prevent climbing past stairs
		[Tooltip("the length of the raycast used to prevent climbing past stairs")]
		[MMCondition("PreventClimbingPastStairs", true)]
		public float PreventClimbingRaycastLength = 0.5f;
        
		protected bool _stairsInputUp = false;
		protected bool _stairsInputDown = false;
		protected float _stairsAheadAngle;
		protected float _stairsBelowAngle;
		protected Vector3 _raycastOrigin;
		protected Vector3 _raycastDirection;
		protected Collider2D _goindDownEntryBoundsCollider;
		protected float _goindDownEntryAt;
		protected LayerMask _initialPlatformLayermask;

		// animation parameters
		protected const string _stairsAnimationParameterName = "OnStairs";
		protected int _stairsAnimationParameter;

		/// <summary>
		/// Initialization method
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			CachePlatformMask();
		}

		/// <summary>
		/// Stores the current controller platform mask
		/// </summary>
		public virtual void CachePlatformMask()
		{
			_initialPlatformLayermask = _controller.PlatformMask;
		}

		/// <summary>
		/// Every frame, we check the input to see if we should initiate a stair climb
		/// </summary>
		protected override void HandleInput()
		{
			_stairsInputUp = (_verticalInput > _inputManager.Threshold.y);
			_stairsInputDown = (_verticalInput < -_inputManager.Threshold.y);
		}

		/// <summary>
		/// Every frame, we check for stairs in front and below the character, we handle input and decide whether or not we should climb stairs or go down them
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleEntryBounds();
			CheckPreventClimbing();
			CheckIfStairsAhead();
			CheckIfStairsBelow();
			CheckIfOnStairways();
			HandleStairsAuthorization();
		}

		/// <summary>
		/// This methods casts a ray downwards, if needed, to check whether or not climbing up stairs should be prevented
		/// </summary>
		protected virtual void CheckPreventClimbing()
		{
			ClimbingUpPrevented = false;
			if (PreventClimbingPastStairs)
			{
				float xOffset = _character.IsFacingRight ? PreventClimbingRaycastOffset.x : -PreventClimbingRaycastOffset.x;
				_raycastOrigin = transform.position;
				_raycastOrigin.x += xOffset;
				_raycastOrigin.y += PreventClimbingRaycastOffset.y;
				_raycastDirection = -transform.up;
                
				// we cast our ray in front of us
				RaycastHit2D hit = MMDebug.RayCast(_raycastOrigin, _raycastDirection, PreventClimbingRaycastLength, _controller.StairsMask, MMColors.Orange, _controller.Parameters.DrawRaycastsGizmos);
				if (hit)
				{
					ClimbingUpPrevented = true;
				}
			}
		}

		/// <summary>
		/// Sets the character in looking up state and asks the camera to look up
		/// </summary>
		protected virtual void HandleStairsAuthorization()
		{
			if (!AbilityAuthorized) 
			{
				return;
			}

			bool authorize = true;
			bool wrongAngle = false;
            
			if (_controller.State.IsGrounded
			    && (_condition.CurrentState == CharacterStates.CharacterConditions.Normal) 
			    && (_movement.CurrentState != CharacterStates.MovementStates.Jumping) 
			    && (_movement.CurrentState != CharacterStates.MovementStates.WallJumping) 
			    && (_movement.CurrentState != CharacterStates.MovementStates.LadderClimbing) 
			    && (_movement.CurrentState != CharacterStates.MovementStates.Dashing))
			{ 
				// if there are stairs in front of us and we're not on stairs already
				if (StairsAhead && !OnStairs) 
				{
					// if we're not pressing up, we can't climb
					if (!_stairsInputUp) 
					{
						authorize = false;
					}
					// if stairs are not at an angle we can climb
					if ((_stairsAheadAngle < 0) || (_stairsAheadAngle >= 90f)) 
					{
						wrongAngle = true;
						authorize = false;
					}
					// if we're not moving fast enough to climb, we don't climb
					if (!wrongAngle && Mathf.Abs(_controller.Speed.x) < MinimumSpeedToBoardStairs) 
					{
						authorize = false;
					}
                    
					if (ClimbingUpPrevented)
					{
						authorize = false;
					}
				}

				if (StairsBelow 
				    && !OnStairs 
				    && (_controller.StandingOn != null)
				    && _controller.OneWayPlatformMask.MMContains(_controller.StandingOn.layer))
				{
					if (_stairsInputDown)
					{
						if ((_stairsBelowAngle > 0) && (_stairsBelowAngle <= 90f))
						{
							_goindDownEntryBoundsCollider = _controller.StandingOnCollider;
							_controller.PlatformMask = _initialPlatformLayermask;
							_controller.PlatformMask -= _controller.OneWayPlatformMask;
							_controller.PlatformMask -= _controller.MovingOneWayPlatformMask;
							_controller.PlatformMask -= _controller.MidHeightOneWayPlatformMask;
							_controller.PlatformMask |= _controller.StairsMask;
							_goindDownEntryAt = Time.time;
						}
					}
				}
			}           

			if (authorize)
			{
				AuthorizeStairs();
			}
			else
			{
				DenyStairs();
			}
		}

		/// <summary>
		/// Restores collisions once we're out of the stairs and if enough time has passed
		/// </summary>
		protected virtual void HandleEntryBounds()
		{
			if (_goindDownEntryBoundsCollider == null)
			{
				return;
			}
			if (Time.time - _goindDownEntryAt < StairsBelowLockTime)
			{
				return;
			}
			if (!_goindDownEntryBoundsCollider.bounds.Contains(_controller.ColliderBottomPosition))
			{
				_controller.CollisionsOn();
				_goindDownEntryBoundsCollider = null;
			}
		}

		/// <summary>
		/// Authorizes collisions with stairs
		/// </summary>
		protected virtual void AuthorizeStairs()
		{
			_controller.CollisionsOnWithStairs();
		}

		/// <summary>
		/// Prevents collisions with stairs
		/// </summary>
		protected virtual void DenyStairs()
		{
			_controller.CollisionsOffWithStairs();
		}

		/// <summary>
		/// Casts a ray to see if there are stairs in front of the character
		/// </summary>
		protected virtual void CheckIfStairsAhead()
		{
			StairsAhead = false;
            
			if (_character.IsFacingRight)
			{
				_raycastOrigin = transform.position + StairsAheadDetectionRaycastOrigin.x * transform.right + StairsAheadDetectionRaycastOrigin.y * transform.up;
				_raycastDirection = transform.right;
			}
			else
			{
				_raycastOrigin = transform.position - StairsAheadDetectionRaycastOrigin.x * transform.right + StairsAheadDetectionRaycastOrigin.y * transform.up;
				_raycastDirection = -transform.right;
			}

			// we cast our ray in front of us
			RaycastHit2D hit = MMDebug.RayCast(_raycastOrigin, _raycastDirection, StairsAheadDetectionRaycastLength, _controller.StairsMask, Color.yellow, _controller.Parameters.DrawRaycastsGizmos);

			if (hit)
			{
				_stairsAheadAngle = Mathf.Abs(Vector2.Angle(hit.normal, transform.up));
				StairsAhead = true;                              
			}
		}

		/// <summary>
		/// Casts a ray to see if there are stairs below the character
		/// </summary>
		protected virtual void CheckIfStairsBelow()
		{
			StairsBelow = false;

			_raycastOrigin = _controller.BoundsCenter;
            
			_raycastOrigin = _controller.ColliderBottomPosition + StairsBelowDetectionRaycastOrigin.x * transform.right + StairsBelowDetectionRaycastOrigin.y * transform.up;
			RaycastHit2D hitA = MMDebug.RayCast(_raycastOrigin, -transform.up, StairsBelowDetectionRaycastLength, _controller.StairsMask, Color.yellow, _controller.Parameters.DrawRaycastsGizmos);
            
            
			_raycastOrigin = _controller.ColliderBottomPosition - StairsBelowDetectionRaycastOrigin.x * transform.right + StairsBelowDetectionRaycastOrigin.y * transform.up;
			RaycastHit2D hitB = MMDebug.RayCast(_raycastOrigin, -transform.up, StairsBelowDetectionRaycastLength, _controller.StairsMask, Color.yellow, _controller.Parameters.DrawRaycastsGizmos);


			if (hitA && hitB)
			{
				if (_character.IsFacingRight)
				{
					_stairsBelowAngle = Mathf.Abs(Vector2.Angle(hitA.normal, transform.right));
				}
				else
				{
					_stairsBelowAngle = Mathf.Abs(Vector2.Angle(hitA.normal, -transform.right));
				}
                
				StairsBelow = true;
			}
		}

		/// <summary>
		/// Checks if the character is currently standing on stairs
		/// </summary>
		protected virtual void CheckIfOnStairways()
		{
			OnStairs = false;
			if (_controller.StandingOn != null)
			{
				if (_controller.StairsMask.MMContains(_controller.StandingOn.layer))
				{
					OnStairs = true;
				}
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_stairsAnimationParameterName, AnimatorControllerParameterType.Bool, out _stairsAnimationParameter);
		}

		/// <summary>
		/// At the end of the ability's cycle, we send our current crouching and crawling states to the animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _stairsAnimationParameter, OnStairs, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			if (_animator != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _stairsAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);    
			}
		}
	}
}