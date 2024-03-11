using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	public class OneWayLevelManager : MMSingleton<OneWayLevelManager>, MMEventListener<CorgiEngineEvent>
	{
		public enum OneWayLevelDirections { None, Left, Right, Up, Down }
		public enum OneWayLevelKillModes { NoKill, Kill }

		[Header("One Way Levels")]
		[MMInformation("Add this component to a LevelManager, and it'll let you define one way level behaviours. " +
		               "A one way level is one where scrolling is controlled in one specific direction. You can simply prevent going back in the opposite direction, " +
		               "or even force auto scrolling in a direction.", MMInformationAttribute.InformationType.Info, false)]

		/// specifies the direction of the level (from level start to finish/goal - Mario's levels would typically be Right here)
		[Tooltip("specifies the direction of the level (from level start to finish/goal - Mario's levels would typically be Right here)")]
		public OneWayLevelDirections OneWayLevelDirection = OneWayLevelDirections.None;
		/// if this is true you won't be able to go back in the level
		[Tooltip("if this is true you won't be able to go back in the level")]
		public bool PreventGoingBack = true;
		/// the maximum distance at which the NoGoingBack object should stay from the player character
		[MMCondition("PreventGoingBack", true)]
		[Tooltip("the maximum distance at which the NoGoingBack object should stay from the player character")]
		public float ThresholdDistance = 5f;
        
		[Header("Auto Scrolling")]

		/// if this is true, the level bounds will be modified so that the level auto scrolls towards the OneWayLevelDirection
		[Tooltip("if this is true, the level bounds will be modified so that the level auto scrolls towards the OneWayLevelDirection")]
		public bool OneWayLevelAutoScrolling = false;
		/// the main camera to use to compute the size of the modified constrained bounds
		[MMCondition("OneWayLevelAutoScrolling", true)]
		[Tooltip("the main camera to use to compute the size of the modified constrained bounds")]
		public Camera MainCamera;
		/// the speed at which the level should auto scroll
		[MMCondition("OneWayLevelAutoScrolling", true)]
		[Tooltip("the speed at which the level should auto scroll")]
		public float OneWayLevelAutoScrollingSpeed = 1f;

		protected Transform _cameraTarget;
		protected Vector2 _direction;
		protected float _offsetAdjuster;
		protected Transform Target;
		protected Vector3 MinBounds;
		protected Vector3 MaxBounds;
		protected Vector3 _minBoundsLastFrame;
		protected Vector3 _maxBoundsLastFrame;
		protected bool _initialOneWayLevelAutoScrolling;
		protected float _initialOneWayLevelAutoScrollingSpeed;

		/// <summary>
		/// Forces the level to prevent being able to go back in the opposite direction, or not
		/// </summary>
		/// <param name="status"></param>
		public virtual void SetPreventGoingBack(bool status)
		{
			PreventGoingBack = status;
		}

		/// <summary>
		/// Forces auto scroll to the value passed in parameters
		/// </summary>
		/// <param name="status"></param>
		public virtual void SetOneWayLevelAutoScrolling(bool status)
		{
			OneWayLevelAutoScrolling = status;
		}

		/// <summary>
		/// Sets the auto scrolling speed
		/// </summary>
		/// <param name="newSpeed"></param>
		public virtual void SetOneWayLevelAutoScrollingSpeed(float newSpeed)
		{
			OneWayLevelAutoScrollingSpeed = newSpeed;
		}

		/// <summary>
		/// Stores initial values, and performs safety checks
		/// </summary>
		protected virtual void Initialization()
		{
			Target = LevelManager.Instance.Players[0].transform;
			_minBoundsLastFrame = LevelManager.Instance.LevelBounds.min;
			_maxBoundsLastFrame = LevelManager.Instance.LevelBounds.max;
			_initialOneWayLevelAutoScrolling = OneWayLevelAutoScrolling;
			_initialOneWayLevelAutoScrollingSpeed = OneWayLevelAutoScrollingSpeed;

			if (OneWayLevelDirection == OneWayLevelDirections.None)
			{
				return;
			}

			if (OneWayLevelAutoScrolling)
			{
				PreventGoingBack = false;
				if (MainCamera == null)
				{
					Debug.LogError("Your OneWayLevelManager is set to OneWayLevelAutomovement, but you haven't set a main camera for it. Make sure you drag your main camera in its inspector.");
					return;
				}            
			}
		}

		/// <summary>
		/// On Update, we trigger our loop methods
		/// </summary>
		protected virtual void Update()
		{
			if (PreventGoingBack)
			{
				HandlePreventGoingBack();
			}

			if (OneWayLevelAutoScrolling)
			{
				HandleAutoMovement();
			}            
		}

		/// <summary>
		/// Modifies the level bounds to prevent being able to go back
		/// </summary>
		protected virtual void HandlePreventGoingBack()
		{
			MinBounds = LevelManager.Instance.LevelBounds.min;
			MaxBounds = LevelManager.Instance.LevelBounds.max;

			switch(OneWayLevelDirection)
			{
				case OneWayLevelDirections.Right:
					MinBounds.x = Target.transform.position.x - ThresholdDistance;
					MinBounds.x = (MinBounds.x <= _minBoundsLastFrame.x) ? _minBoundsLastFrame.x : MinBounds.x;
					break;
				case OneWayLevelDirections.Left:
					MaxBounds.x = Target.transform.position.x + ThresholdDistance;
					MaxBounds.x = (MaxBounds.x >= _maxBoundsLastFrame.x) ? _maxBoundsLastFrame.x : MaxBounds.x;
					break;
				case OneWayLevelDirections.Up:
					MinBounds.y = Target.transform.position.y - ThresholdDistance;
					MinBounds.y = (MinBounds.y <= _minBoundsLastFrame.y) ? _minBoundsLastFrame.y : MinBounds.y;
					break;
				case OneWayLevelDirections.Down:
					MaxBounds.y = Target.transform.position.y + ThresholdDistance;
					MaxBounds.y = (MaxBounds.y >= _maxBoundsLastFrame.y) ? _maxBoundsLastFrame.y : MaxBounds.y;
					break;
			}
			LevelManager.Instance.SetNewMinLevelBounds(MinBounds);
			LevelManager.Instance.SetNewMaxLevelBounds(MaxBounds);

			_minBoundsLastFrame = LevelManager.Instance.LevelBounds.min;
			_maxBoundsLastFrame = LevelManager.Instance.LevelBounds.max;
		}

		/// <summary>
		/// Makes the level auto scroll by modifying its level bounds dynamically, which forces the camera to "move"
		/// </summary>
		protected virtual void HandleAutoMovement()
		{
			MinBounds = LevelManager.Instance.LevelBounds.min;
			MaxBounds = LevelManager.Instance.LevelBounds.max;

			if (!OneWayLevelAutoScrolling)
			{
				return;
			}

			switch (OneWayLevelDirection)
			{
				case OneWayLevelDirections.Right:
					MinBounds.x += OneWayLevelAutoScrollingSpeed * Time.deltaTime;
					MaxBounds.x = MinBounds.x + MainCamera.MMCameraWorldSpaceWidth();
					break;
				case OneWayLevelDirections.Left:
					MinBounds.x = MaxBounds.x - MainCamera.MMCameraWorldSpaceWidth();
					MaxBounds.x -= OneWayLevelAutoScrollingSpeed * Time.deltaTime;
					break;
				case OneWayLevelDirections.Up:
					MinBounds.y += OneWayLevelAutoScrollingSpeed * Time.deltaTime;
					MaxBounds.y = MinBounds.y + MainCamera.MMCameraWorldSpaceHeight();
					break;
				case OneWayLevelDirections.Down:
					MinBounds.y = MaxBounds.y - MainCamera.MMCameraWorldSpaceHeight();
					MaxBounds.y -= OneWayLevelAutoScrollingSpeed * Time.deltaTime;
					break;
			}

			LevelManager.Instance.SetNewMinLevelBounds(MinBounds);
			LevelManager.Instance.SetNewMaxLevelBounds(MaxBounds);
		}
        
		/// <summary>
		/// On LevelStart we initialize, on Respawn we reset our bounds values
		/// </summary>
		/// <param name="corgiEngineEvent"></param>
		public virtual void OnMMEvent(CorgiEngineEvent corgiEngineEvent)
		{
			switch(corgiEngineEvent.EventType)
			{
				case CorgiEngineEventTypes.LevelStart:
					Initialization();
					break;

				case CorgiEngineEventTypes.Respawn:
					_minBoundsLastFrame = LevelManager.Instance.LevelBounds.min;
					_maxBoundsLastFrame = LevelManager.Instance.LevelBounds.max;
					OneWayLevelAutoScrolling = _initialOneWayLevelAutoScrolling;
					OneWayLevelAutoScrollingSpeed = _initialOneWayLevelAutoScrollingSpeed;
					break;
			}            
		}

		protected virtual void OnEnable()
		{
			this.MMEventStartListening<CorgiEngineEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<CorgiEngineEvent>();
		}

	}
}