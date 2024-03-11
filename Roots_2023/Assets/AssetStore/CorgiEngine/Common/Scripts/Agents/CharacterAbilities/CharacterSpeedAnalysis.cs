using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this ability to a character and it'll compute and expose various speeds at runtime
	/// It doesn't serve any purpose on its own, and is provided more as an example of how you can create abilities to analyze and extract data from gameplay, at runtime
	/// These exposed values could then be used by other abilities, for example
	/// </summary>
	[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	public class CharacterSpeedAnalysis : CharacterAbility
	{
		/// the possible modes this analysis can run on
		public enum Modes { Framecount, Time }
        
		public override string HelpBoxText() { return "This component computes and exposes various speeds at runtime. It doesn't serve any purpose on its own, " +
		                                              "and is provided more as an example of how you can create abilities to analyze and extract data from gameplay, at runtime." +
		                                              " These exposed values could then be used by other abilities, for example"; }

		[Header("General Settings")] 
		/// whether rolling average should be computed over the past X frames, or over time spans
		[Tooltip("whether rolling average should be computed over the past X frames, or over time spans")]
		public Modes RollingAverageMode = Modes.Framecount;
		/// the amount of samples to consider
		[Tooltip("the amount of samples to consider")]
		public int RollingAverageSamplesCount = 10;
		/// the amount of frames to ignore between two recordings for the rolling average
		[Tooltip("the amount of frames to ignore between two recordings for the rolling average")]
		[MMEnumCondition("RollingAverageMode", (int)Modes.Framecount)]
		public int RollingAverageFrameInterval = 10;
		/// the duration (in seconds) to wait for between two recordings
		[Tooltip("the duration (in seconds) to wait for between two recordings")]
		[MMEnumCondition("RollingAverageMode", (int)Modes.Time)]
		public float RollingAverageTimeInterval = 0.5f;
		/// the current frame counter
		[Tooltip("the current frame counter")]
		[MMReadOnly]
		public int FrameCounter;
        
		[Header("Controller Speed")] 
		/// the current speed of the controller, this frame
		[Tooltip("the current speed of the controller, this frame")]
		[MMFReadOnly]
		public Vector2 ControllerSpeed;
		/// the speed of the controller last frame
		[Tooltip("the speed of the controller last frame")]
		[MMFReadOnly]
		public Vector2 ControllerSpeedLastFrame;
		/// the rolling average speed of the controller, over the selected samples / duration / frame count
		[Tooltip("the rolling average speed of the controller, over the selected samples / duration / frame count")]
		[MMFReadOnly]
		public Vector2 ControllerSpeedRollingAverage;
        
		[Header("Transform Speed")]
		/// the speed of the transform this frame
		[Tooltip("the speed of the transform this frame")]
		[MMFReadOnly]
		public Vector2 TransformSpeed;
		/// the speed of the transform last frame
		[Tooltip("the speed of the transform last frame")]
		[MMFReadOnly]
		public Vector2 TransformSpeedLastFrame;
		/// the rolling average speed of the transform, over the selected samples / duration / frame count
		[Tooltip("the rolling average speed of the transform, over the selected samples / duration / frame count")]
		[MMFReadOnly]
		public Vector2 TransformSpeedRollingAverage;

		protected Vector2[] _controllerSpeedLog;
		protected Vector2[] _transformSpeedLog;
		protected Vector2 _controllerSpeedLastFrame, _transformSpeedLastFrame;
		protected Vector2 _positionLastFrame;
		protected Vector2 _controllerRollingAverage, _transformRollingAverage;
		protected int _frameCountLastRecord = 0;
		protected float _timeLastRecord = 0f;
        
		/// <summary>
		/// On initialization we initialize our arrays 
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_controllerSpeedLog = new Vector2[RollingAverageSamplesCount];
			_transformSpeedLog = new Vector2[RollingAverageSamplesCount];
            
			for (int i = 0; i < _controllerSpeedLog.Length; i++)
			{
				_controllerSpeedLog[i] = Vector2.zero;
				_transformSpeedLog[i] = Vector2.zero;    
			}
		}

		/// <summary>
		/// On late process, we log our speed and record if needed
		/// </summary>
		public override void LateProcessAbility()
		{
			FrameCounter = Time.frameCount;
            
			ControllerSpeed = _controller.Speed;
			ControllerSpeedLastFrame = _controllerSpeedLastFrame;
			_controllerSpeedLastFrame = ControllerSpeed;

			TransformSpeed = ((Vector2)_controller.transform.position - _positionLastFrame) / _controller.DeltaTime;
			TransformSpeedLastFrame = _transformSpeedLastFrame;
			_transformSpeedLastFrame = TransformSpeed;

			Record();

			_positionLastFrame = _controller.transform.position;
		}

		/// <summary>
		/// Stores rolling average values and computes them if needed
		/// </summary>
		protected virtual void Record()
		{
			if (RollingAverageMode == Modes.Framecount)
			{
				if (FrameCounter - _frameCountLastRecord < RollingAverageFrameInterval)
				{
					return;
				}

				_frameCountLastRecord = FrameCounter;
			}
			else
			{
				if (Time.time - _timeLastRecord < RollingAverageTimeInterval)
				{
					return;
				}

				_timeLastRecord = Time.time;
			}
            
			_controllerSpeedLog[0] = ControllerSpeed;
			_transformSpeedLog[0] = TransformSpeed;
            
			Array.Copy(_controllerSpeedLog, 0, _controllerSpeedLog, 1, _controllerSpeedLog.Length - 1);
			Array.Copy(_transformSpeedLog, 0, _transformSpeedLog, 1, _transformSpeedLog.Length - 1);
            
            
			_controllerRollingAverage = Vector2.zero;
			_transformRollingAverage = Vector2.zero;
			for (int i = 0; i < _controllerSpeedLog.Length; i++)
			{
				_controllerRollingAverage += _controllerSpeedLog[i];
				_transformRollingAverage += _transformSpeedLog[i];
			}

			ControllerSpeedRollingAverage = _controllerRollingAverage / RollingAverageSamplesCount;
			TransformSpeedRollingAverage = _transformRollingAverage / RollingAverageSamplesCount;
		}
	}
}