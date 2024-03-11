using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This ability will make your character move automatically, without having to touch the left or right inputs
	/// </summary>
	[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Auto Movement")]
	[RequireComponent(typeof(CharacterHorizontalMovement))]
	public class CharacterAutoMovement : CharacterAbility
	{
		/// the possible directions the character can start moving in
		public enum InitialDirections { Left, Right, None }

		[Header("Initialization")]

		/// if this is true, will set other abilities in the appropriate configuration on initialization automatically
		[Tooltip("if this is true, will set other abilities in the appropriate configuration on initialization automatically")]
		public bool AutoSetup = true;
		/// the initial direction the character should start moving towards
		[Tooltip("the initial direction the character should start moving towards")]
		public InitialDirections InitialDirection = InitialDirections.Right;
		/// if this is true, the character 
		[Tooltip("if this is true, the character ")]
		public bool RunningOnStart = false;
        
		[Header("Input")]

		/// if this is true, input won't be read
		[Tooltip("if this is true, input won't be read")]
		public bool NoHorizontalInput = false;
		/// if NoHorizontalInput is true, the initial value to apply to input
		[Tooltip("if NoHorizontalInput is true, the initial value to apply to input")]
		[MMCondition("NoHorizontalInput", true)]
		[Range(-1f,1f)]
		public float InitialInput = 1f;

		[Header("Automatic Direction Changes")]

		/// if this is true, doing a walljump will also cause a direction change
		[Tooltip("if this is true, doing a walljump will also cause a direction change")]
		public bool ChangeDirectionOnWalljump = true;
		/// if this is true, the character will change direction when colliding with a wall
		[Tooltip("if this is true, the character will change direction when colliding with a wall")]
		public bool ChangeDirectionOnWallCollision = false;
		/// if this is true, the character will change direction when colliding with a wall, but only if grounded - requires ChangeDirectionOnWallCollision to be true 
		[Tooltip("if this is true, the character will change direction when colliding with a wall, but only if grounded - requires ChangeDirectionOnWallCollision to be true")]
		[MMCondition("ChangeDirectionOnWallCollision", true)]
		public bool ChangeDirectionOnWallCollisionOnlyIfGrounded = false;
		/// if this is true, the character will change direction when falling down, at the start of the fall
		[Tooltip("if this is true, the character will change direction when falling down, at the start of the fall")]
		public bool ChangeDirectionOnFallingDown = false;

		[Header("Tests")]

		/// Test button for ToggleRun
		[MMInspectorButton("ToggleRun")]
		public bool ToggleRunButton;
		/// Test button for ChangeDirection
		[MMInspectorButton("ChangeDirection")]
		public bool ChangeDirectionButton;

		protected CharacterRun _characterRun;
		protected CharacterWalljump _characterWallJump;
		protected float _currentDirection = 1f;
		protected bool _running = false;
		protected float _directionBeforePause = 0f;
		protected float _drivenInput = 1f;
		protected CharacterStates.MovementStates _movementStateLastFrame;
        
        
		/// <summary>
		/// On init we grab our components and set them if needed, set our initial direction and run state
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			if (AutoSetup)
			{
				_characterHorizontalMovement.ReadInput = false;
			}

			switch (InitialDirection)
			{
				case InitialDirections.Left:
					_currentDirection = -1f;
					break;
				case InitialDirections.Right:
					_currentDirection = 1f;
					break;
				case InitialDirections.None:
					_currentDirection = 0f;
					break;
			}

			_characterRun = _character?.FindAbility<CharacterRun>();
			if (_characterRun != null)
			{
				if (AutoSetup)
				{
					_characterRun.ReadInput = false;
				}
				if (RunningOnStart)
				{
					_running = true;
					_characterRun.ForceRun(true);
				}
			}
            
			_characterWallJump = _character?.FindAbility<CharacterWalljump>();
			if (_characterWallJump != null)
			{
				_characterWallJump.OnWallJump += OnWallJump;
			}

			_drivenInput = InitialInput;
		}

		/// <summary>
		/// Forces a current direction and passes it to the CharacterHorizontalMovement ability
		/// </summary>
		protected override void HandleInput()
		{
			if (!NoHorizontalInput)
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
		}

		/// <summary>
		/// On process ability we do nothing
		/// </summary>
		public override void ProcessAbility()
		{
			TestChangeDirectionOnWallCollision();
			TestChangeDirectionOnFallingDown();
		}

		/// <summary>
		/// Checks if we should change direction if colliding with a wall
		/// </summary>
		protected virtual void TestChangeDirectionOnWallCollision()
		{
			if (ChangeDirectionOnWallCollision)
			{
				if (ChangeDirectionOnWallCollisionOnlyIfGrounded && !_controller.State.IsGrounded)
				{
					return;
				}
                
				if ((_currentDirection < 0 && _controller.State.IsCollidingLeft) || (_currentDirection > 0 && _controller.State.IsCollidingRight))
				{
					ChangeDirection();    
				}
			}
		}

		/// <summary>
		/// Checks if we should be changing direction if falling down
		/// </summary>
		protected virtual void TestChangeDirectionOnFallingDown()
		{
			if (ChangeDirectionOnFallingDown)
			{
				if ((_character.MovementState.CurrentState == CharacterStates.MovementStates.Falling) && (_movementStateLastFrame != CharacterStates.MovementStates.Falling) && (_controller.Speed.y < 0) )
				{
					ChangeDirection();
				}
				_movementStateLastFrame = _character.MovementState.CurrentState;
			}
		}

		/// <summary>
		/// On late process, we reset our driven input
		/// </summary>
		public override void LateProcessAbility()
		{
			base.LateProcessAbility();
			_drivenInput = 0f;
		}

		/// <summary>
		/// When the Character walljumps, we change direction if needed
		/// </summary>
		protected virtual void OnWallJump()
		{
			if (ChangeDirectionOnWalljump)
			{
				ChangeDirection();
			}
		}

		/// <summary>
		/// Forces a pause in the movement, that can then be resumed
		/// </summary>
		public virtual void PauseMovement()
		{
			_directionBeforePause = _currentDirection;
			_currentDirection = 0f;
		}

		/// <summary>
		/// Resumes movement
		/// </summary>
		public virtual void ResumeMovement()
		{
			_currentDirection = _directionBeforePause;
		}

		/// <summary>
		/// A method you can call to force a new input when in NoHorizontalInput mode
		/// </summary>
		/// <param name="newDrivenInput"></param>
		public virtual void SetDrivenInput(float newDrivenInput)
		{
			_drivenInput = newDrivenInput;
		}

		/// <summary>
		/// Changes direction (left to right, right to left)
		/// </summary>
		public virtual void ChangeDirection()
		{
			_currentDirection = -_currentDirection;
		}

		/// <summary>
		/// Forces a direction (-1 left, 1 right)
		/// </summary>
		/// <param name="newDirection"></param>
		public virtual void ForceDirection(float newDirection)
		{
			_currentDirection = newDirection;
		}

		/// <summary>
		/// Changes the run state (walk > run, run > walk)
		/// </summary>
		public virtual void ToggleRun()
		{            
			_running = !_running;
			_characterRun.ForceRun(_running);
		}

		/// <summary>
		/// Forces run (true) or walk (false)
		/// </summary>
		/// <param name="status"></param>
		public virtual void ForceRun(bool status)
		{
			_running = status;
			_characterRun.ForceRun(status);
		}
	}
}