using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This Action will make the Character fly until it hits a wall or a hole while following a path.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Fly Patrol")]
	// [RequireComponent(typeof(MMPath))]
	// [RequireComponent(typeof(CharacterFly))]
	// [RequireComponent(typeof(CorgiController))]
	// [RequireComponent(typeof(Character))]
	// [RequireComponent(typeof(Health))]
	public class AIActionFlyPatrol : AIAction
	{        
		[Header("Obstacle Detection")]
		/// if set to true, the agent will change direction when hitting an obstacle
		[Tooltip("if set to true, the agent will change direction when hitting an obstacle")]
		public bool ChangeDirectionOnObstacle = true;
        
		[Header("Revive")] 
		/// if this is true, the character will automatically return to its initial position on revive
		[Tooltip("if this is true, the character will automatically return to its initial position on revive")]
		public bool ResetPositionOnDeath = true;

		// private stuff
		protected CorgiController _controller;
		protected Character _character;
		protected CharacterFly _characterFly;
		protected Health _health;
		protected Vector2 _direction;
		protected Vector2 _startPosition;
		protected Vector2 _initialDirection;
		protected Vector3 _initialScale;
		protected float _distanceToTarget;
		protected Vector3 _initialPosition;
		protected MMPath _mmPath;
		protected Vector3 _mmPathPointLastFrame;
		protected int _mmPathIndexLastFrame;
		protected float _waitDelay = 0f;

		/// <summary>
		/// On init we grab all the components we'll need
		/// </summary>
		public override void Initialization()
		{
			// we get the CorgiController2D component
			_controller = this.gameObject.GetComponentInParent<CorgiController>();
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterFly = _character?.FindAbility<CharacterFly>();
			_health = this.gameObject.GetComponentInParent<Health>();
			_mmPath = this.gameObject.GetComponent<MMPath>();
			// initialize the start position
			_startPosition = transform.position;
			// initialize the direction
			_direction = _character.IsFacingRight ? Vector2.right : Vector2.left;

			_initialPosition = this.transform.position;
			_initialDirection = _direction;
			_initialScale = transform.localScale;
			if (!_mmPath.Initialized)
			{
				_mmPath.Initialization();
			}
			_mmPathPointLastFrame = _mmPath.CurrentPoint();
			_mmPathIndexLastFrame = _mmPath.CurrentIndex();
		}

		/// <summary>
		/// On PerformAction we patrol
		/// </summary>
		public override void PerformAction()
		{
			FlyPatrol();
		}

		/// <summary>
		/// This method initiates all the required checks and moves the character
		/// </summary>
		protected virtual void FlyPatrol()
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

			if (_mmPath.CurrentPoint() != _mmPathPointLastFrame)
			{                
				_waitDelay = _mmPath.PathElements[_mmPathIndexLastFrame].Delay;
				_mmPathPointLastFrame = _mmPath.CurrentPoint();
				_mmPathIndexLastFrame = _mmPath.CurrentIndex();
			}

			if (_waitDelay > 0f)
			{
				_waitDelay -= Time.deltaTime;
				_characterFly.SetHorizontalMove(0f);
				_characterFly.SetVerticalMove(0f);
				return;
			}

			// moves the agent in its current direction
			CheckForObstacles();

			_direction = _mmPath.CurrentPoint() - this.transform.position;
			_direction = _direction.normalized;

			_characterFly.SetHorizontalMove(_direction.x);
			_characterFly.SetVerticalMove(_direction.y);

			_mmPathPointLastFrame = _mmPath.CurrentPoint();
			_mmPathIndexLastFrame = _mmPath.CurrentIndex();
		}

		/// <summary>
		/// Draws bounds gizmos
		/// </summary>
		protected virtual void OnDrawGizmosSelected()
		{
			if (_mmPath == null)
			{
				return;
			}
			Gizmos.color = MoreMountains.Tools.MMColors.IndianRed;
			Gizmos.DrawLine(this.transform.position, _mmPath.CurrentPoint());
		}

		/// <summary>
		/// When exiting the state we reset our movement
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			_characterFly?.SetHorizontalMove(0f);
			_characterFly?.SetVerticalMove(0f);
		}

		/// <summary>
		/// Checks for a wall and changes direction if it meets one
		/// </summary>
		protected virtual void CheckForObstacles()
		{
			if (!ChangeDirectionOnObstacle)
			{
				return;
			}
            
			// if the agent is colliding with something, make it turn around
			if (
				(_direction.x < 0 && _controller.State.IsCollidingLeft) 
				|| (_direction.x > 0 && _controller.State.IsCollidingRight)
				|| (_direction.y < 0 && _controller.State.IsCollidingBelow)
				|| (_direction.y > 0 && _controller.State.IsCollidingAbove)
			)
			{
				ChangeDirection();
			}
		}
        
		/// <summary>
		/// Changes the current movement direction
		/// </summary>
		protected virtual void ChangeDirection()
		{
			_direction = -_direction;
			_mmPath.ChangeDirection();
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