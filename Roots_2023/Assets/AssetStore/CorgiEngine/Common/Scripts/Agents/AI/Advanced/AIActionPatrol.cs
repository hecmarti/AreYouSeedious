using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This Action will make the Character patrols until it (optionally) hits a wall or a hole.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Patrol")]
	// [RequireComponent(typeof(CorgiController))]
	// [RequireComponent(typeof(Character))]
	// [RequireComponent(typeof(Health))]
	// [RequireComponent(typeof(CharacterHorizontalMovement))]
	public class AIActionPatrol : AIAction
	{
		[Header("Obstacle Detection")]
		/// If set to true, the agent will change direction when hitting a wall
		[Tooltip("If set to true, the agent will change direction when hitting a wall")]
		public bool ChangeDirectionOnWall = true;
		/// If set to true, the agent will try and avoid falling
		[Tooltip("If set to true, the agent will try and avoid falling")]
		public bool AvoidFalling = false;
		/// The offset the hole detection should take into account
		[Tooltip("The offset the hole detection should take into account")]
		public Vector3 HoleDetectionOffset = new Vector3(0, 0, 0);
		/// the length of the ray cast to detect holes
		[Tooltip("the length of the ray cast to detect holes")]
		public float HoleDetectionRaycastLength = 1f;

		[Header("Layermasks")]
		/// Whether to use a custom layermask, or simply use the platform mask defined at the character level
		[Tooltip("Whether to use a custom layermask, or simply use the platform mask defined at the character level")]
		public bool UseCustomLayermask = false;
		/// if using a custom layer mask, the list of layers considered as obstacles by this AI
		[Tooltip("if using a custom layer mask, the list of layers considered as obstacles by this AI")]
		[MMCondition("UseCustomLayermask", true)]
		public LayerMask ObstaclesLayermask = LayerManager.ObstaclesLayerMask;
		/// the length of the horizontal raycast we should use to detect obstacles that may cause a direction change
		[Tooltip("the length of the horizontal raycast we should use to detect obstacles that may cause a direction change")]
		[MMCondition("UseCustomLayermask", true)]
		public float ObstaclesDetectionRaycastLength = 0.5f;
		/// the origin of the raycast (if casting against the same layer this object is on, the origin should be outside its collider, typically in front of it)
		[Tooltip("the origin of the raycast (if casting against the same layer this object is on, the origin should be outside its collider, typically in front of it)")]
		[MMCondition("UseCustomLayermask", true)]
		public Vector2 ObstaclesDetectionRaycastOrigin = new Vector2(0.5f, 0f);

		[Header("Revive")] 
		/// if this is true, the character will automatically return to its initial position on revive
		[Tooltip("if this is true, the character will automatically return to its initial position on revive")]
		public bool ResetPositionOnDeath = true;

		// private stuff
		protected CorgiController _controller;
		protected Character _character;
		protected Health _health;
		protected CharacterHorizontalMovement _characterHorizontalMovement;
		protected Vector2 _direction;
		protected Vector2 _startPosition;
		protected Vector2 _initialDirection;
		protected Vector3 _initialScale;
		protected float _distanceToTarget;
		protected Vector2 _raycastOrigin;
		protected RaycastHit2D _raycastHit2D;
		protected Vector2 _obstacleDirection;

		/// <summary>
		/// On init we grab all the components we'll need
		/// </summary>
		public override void Initialization()
		{
			// we get the CorgiController2D component
			_controller = GetComponentInParent<CorgiController>();
			_character = GetComponentInParent<Character>();
			_characterHorizontalMovement = _character?.FindAbility<CharacterHorizontalMovement>();
			_health = _character.CharacterHealth;
			// initialize the start position
			_startPosition = transform.position;
			// initialize the direction
			_direction = _character.IsFacingRight ? Vector2.right : Vector2.left;

			_initialDirection = _direction;
			_initialScale = transform.localScale;
		}

		/// <summary>
		/// On PerformAction we patrol
		/// </summary>
		public override void PerformAction()
		{
			Patrol();
		}

		/// <summary>
		/// This method initiates all the required checks and moves the character
		/// </summary>
		protected virtual void Patrol()
		{
			if (_character == null)
			{
				return;
			}
			if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
			    || (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
			{
				return;
			}
			// moves the agent in its current direction
			CheckForWalls();
			CheckForHoles();
			_characterHorizontalMovement.SetHorizontalMove(_direction.x);
		}

		/// <summary>
		/// When exiting the state we reset our movement
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			_characterHorizontalMovement?.SetHorizontalMove(0f);
		}

		/// <summary>
		/// Checks for a wall and changes direction if it meets one
		/// </summary>
		protected virtual void CheckForWalls()
		{
			if (!ChangeDirectionOnWall)
			{
				return;
			}

			if (UseCustomLayermask)
			{
				if (DetectObstaclesCustomLayermask())
				{
					ChangeDirection();
				}
			}
			else
			{
				// if the agent is colliding with something, make it turn around
				if (DetectObstaclesRegularLayermask())
				{
					ChangeDirection();
				}
			}
		}

		/// <summary>
		/// Returns true if an obstacle is colliding with this AI, using its controller layer masks
		/// </summary>
		/// <returns></returns>
		protected bool DetectObstaclesRegularLayermask()
		{
			return (_direction.x < 0 && _controller.State.IsCollidingLeft) || (_direction.x > 0 && _controller.State.IsCollidingRight);
		}

		/// <summary>
		/// Returns true if an obstacle is in front of the character, using a custom layer mask
		/// </summary>
		/// <returns></returns>
		protected bool DetectObstaclesCustomLayermask()
		{
			if (_character.IsFacingRight)
			{
				_raycastOrigin = transform.position + (_controller.Bounds.x / 2 + ObstaclesDetectionRaycastOrigin.x) * transform.right + ObstaclesDetectionRaycastOrigin.y * transform.up;
				_obstacleDirection = Vector2.right;
			}
			else
			{
				_raycastOrigin = transform.position - (_controller.Bounds.x / 2 + ObstaclesDetectionRaycastOrigin.x) * transform.right + ObstaclesDetectionRaycastOrigin.y * transform.up;
				_obstacleDirection = Vector2.left;
			}

			_raycastHit2D = MMDebug.RayCast(_raycastOrigin, _obstacleDirection, ObstaclesDetectionRaycastLength, ObstaclesLayermask, Color.gray, true);

			return _raycastHit2D;
		}

		/// <summary>
		/// Checks for holes 
		/// </summary>
		protected virtual void CheckForHoles()
		{
			// if we're not grounded or if we're not supposed to check for holes, we do nothing and exit
			if (!AvoidFalling || !_controller.State.IsGrounded)
			{
				return;
			}

			// we send a raycast at the extremity of the character in the direction it's facing, and modified by the offset you can set in the inspector.

			if (_character.IsFacingRight)
			{
				_raycastOrigin = transform.position + (_controller.Bounds.x / 2 + HoleDetectionOffset.x) * transform.right + HoleDetectionOffset.y * transform.up;
			}
			else
			{
				_raycastOrigin = transform.position - (_controller.Bounds.x / 2 + HoleDetectionOffset.x) * transform.right + HoleDetectionOffset.y * transform.up;
			}

			if (UseCustomLayermask)
			{
				_raycastHit2D = MMDebug.RayCast(_raycastOrigin, -transform.up, HoleDetectionRaycastLength, ObstaclesLayermask, Color.gray, true);
			}
			else
			{
				_raycastHit2D = MMDebug.RayCast(_raycastOrigin, -transform.up, HoleDetectionRaycastLength, _controller.PlatformMask | _controller.MovingPlatformMask | _controller.OneWayPlatformMask | _controller.MovingOneWayPlatformMask, Color.gray, true);
			}
            
			// if the raycast doesn't hit anything
			if (!_raycastHit2D)
			{
				// we change direction
				ChangeDirection();
			}
		}

		/// <summary>
		/// Changes the current movement direction
		/// </summary>
		protected virtual void ChangeDirection()
		{
			_direction = -_direction;
		}

		/// <summary>
		/// When reviving we make sure our directions are properly setup
		/// </summary>
		protected virtual void OnRevive()
		{
			_direction = _character.IsFacingRight ? Vector2.right : Vector2.left;
			transform.localScale = _initialScale;
			if (ResetPositionOnDeath)
			{
				transform.position = _startPosition;
			}
		}

		/// <summary>
		/// On enable we start listening for OnRevive events
		/// </summary>
		protected virtual void OnEnable()
		{
			if (_health == null)
			{
				_health = this.gameObject.GetComponentInParent<Health>();
			}

			if (_health != null)
			{
				_health.OnRevive += OnRevive;
			}
		}

		/// <summary>
		/// On disable we stop listening for OnRevive events
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnRevive -= OnRevive;
			}
		}
	}
}