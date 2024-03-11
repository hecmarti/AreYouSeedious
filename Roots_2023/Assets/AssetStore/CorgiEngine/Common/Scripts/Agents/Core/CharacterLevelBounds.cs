using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// This class handles what happens when the player reaches the level bounds.
	/// For each bound (above, below, left, right), you can define if the player will be killed, or if its movement will be constrained, or if nothing happens
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Core/Character Level Bounds")] 
	public class CharacterLevelBounds : CorgiMonoBehaviour 
	{
		/// the possible consequences of touching the level bounds
		public enum BoundsBehavior 
		{
			Nothing,
			Constrain,
			Kill
		}

		[MMInformation("Here you can define what happens when the character reaches each side of the level bounds. The level bounds are defined in the LevelManager of each scene.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]

		/// what to do to the player when it reaches the top level bound
		[Tooltip("what to do to the player when it reaches the top level bound")]
		public BoundsBehavior Top = BoundsBehavior.Constrain;
		/// what to do to the player when it reaches the bottom level bound
		[Tooltip("what to do to the player when it reaches the bottom level bound")]
		public BoundsBehavior Bottom = BoundsBehavior.Kill;
		/// what to do to the player when it reaches the left level bound
		[Tooltip("what to do to the player when it reaches the left level bound")]
		public BoundsBehavior Left = BoundsBehavior.Constrain;
		/// what to do to the player when it reaches the right level bound
		[Tooltip("what to do to the player when it reaches the right level bound")]
		public BoundsBehavior Right = BoundsBehavior.Constrain;
		/// if this is true, controller forces will also be reset (on the appropriate axis) when constraining
		[Tooltip("if this is true, controller forces will also be reset (on the appropriate axis) when constraining")]
		public bool ResetForcesOnConstrain = true;

		protected Bounds _bounds;
		protected CorgiController _controller;
		protected Character _character;
		protected Vector2 _constrainedPosition;
		protected OneWayLevelManager _oneWayLevelManager;
		
		/// <summary>
		/// Initialization
		/// </summary>
		public virtual void Start () 
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_controller = this.gameObject.GetComponentInParent<CorgiController>();
			if (LevelManager.HasInstance)
			{
				_bounds = LevelManager.Instance.LevelBounds;
				_oneWayLevelManager = LevelManager.Instance.gameObject.GetComponent<OneWayLevelManager>();
			}
		}
		
		/// <summary>
		/// Every frame, we check if the player is colliding with a level bound
		/// </summary>
		public virtual void LateUpdate ()
		{
			_controller.State.TouchingLevelBounds = false;
			// if the player is dead, we do nothing
			if ( (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
			     || (!LevelManager.HasInstance) )
			{
				return;
			}
			Physics2D.SyncTransforms();
			_bounds = LevelManager.Instance.LevelBounds;

			if (_bounds.size != Vector3.zero)
			{		
				// when the player reaches a bound, we apply the specified bound behavior
				if ((Top != BoundsBehavior.Nothing) && (_controller.ColliderTopPosition.y > _bounds.max.y))
				{
					_constrainedPosition.x = transform.position.x;
					_constrainedPosition.y = _bounds.max.y - _controller.ColliderSize.y / 2 - _controller.ColliderOffset.y;
					if (ResetForcesOnConstrain) { _controller.SetVerticalForce(0f); }
					ApplyBoundsBehavior(Top, _constrainedPosition);
				}
									
				if ((Bottom != BoundsBehavior.Nothing) && (_controller.ColliderBottomPosition.y < _bounds.min.y))
				{
					_constrainedPosition.x = transform.position.x;
					_constrainedPosition.y = _bounds.min.y + _controller.ColliderSize.y / 2 + _controller.ColliderOffset.y;
					if (ResetForcesOnConstrain) { _controller.SetVerticalForce(0f); }
					ApplyBoundsBehavior(Bottom, _constrainedPosition);
				}					
				
				if ((Right != BoundsBehavior.Nothing) && (_controller.ColliderRightPosition.x > _bounds.max.x))
				{
					_constrainedPosition.x = _bounds.max.x - _controller.ColliderSize.x / 2 - _controller.ColliderOffset.x;
					_constrainedPosition.y = transform.position.y;
					if (ResetForcesOnConstrain) { _controller.SetHorizontalForce(0f); }
					ApplyBoundsBehavior(Right, _constrainedPosition);		
				}					
				
				if ((Left != BoundsBehavior.Nothing) && (_controller.ColliderLeftPosition.x < _bounds.min.x))
				{
					_constrainedPosition.x = _bounds.min.x + _controller.ColliderSize.x / 2 + _controller.ColliderOffset.x;
					_constrainedPosition.y = transform.position.y;
					if (ResetForcesOnConstrain) { _controller.SetHorizontalForce(0f); }
					ApplyBoundsBehavior(Left, _constrainedPosition);
				}					
			}	
            
			// if we're in auto scroll and we get crushed, we kill the player
			if ((_oneWayLevelManager != null) && _oneWayLevelManager.OneWayLevelAutoScrolling)
			{
				bool colliding = false;
				switch (_oneWayLevelManager.OneWayLevelDirection)
				{
					case OneWayLevelManager.OneWayLevelDirections.Right:
						colliding = _controller.State.IsCollidingRight;
						break;
					case OneWayLevelManager.OneWayLevelDirections.Left:
						colliding = _controller.State.IsCollidingLeft;
						break;
					case OneWayLevelManager.OneWayLevelDirections.Up:
						colliding = _controller.State.IsCollidingAbove;
						break;
					case OneWayLevelManager.OneWayLevelDirections.Down:
						colliding = _controller.State.IsCollidingBelow;
						break;
				}
				if (colliding && _controller.State.TouchingLevelBounds)
				{
					_character.CharacterHealth.Kill();
					_oneWayLevelManager.SetOneWayLevelAutoScrolling(false);
				}
			}
		}

		/// <summary>
		/// Applies the specified bound behavior to the player
		/// </summary>
		/// <param name="behavior">Behavior.</param>
		/// <param name="constrainedPosition">Constrained position.</param>
		protected virtual void ApplyBoundsBehavior(BoundsBehavior behavior, Vector2 constrainedPosition)
		{
			_controller.State.TouchingLevelBounds = true;

			if ( (_character == null)
			     || (!LevelManager.HasInstance) )
			{
				return;	
			}

			if (behavior== BoundsBehavior.Kill)
			{
				if (_character.CharacterType == Character.CharacterTypes.Player)
				{
					_character.CharacterHealth.Kill();
				}
				else
				{
					Health health = _character.gameObject.MMGetComponentNoAlloc<Health>();
					if (health != null)
					{
						health.Kill();
					}
				}
				return;
			}	

			if (behavior == BoundsBehavior.Constrain)
			{
				transform.position = constrainedPosition;
				Physics2D.SyncTransforms();
				return;	
			}
		}
	}
}