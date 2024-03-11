using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A struct used to store character animation parameter definitions, to be used by the CharacterAnimationParametersInitializer class
	/// </summary>
	public struct CorgiEngineCharacterAnimationParameter
	{
		/// the name of the parameter
		public string ParameterName;
		/// the type of the parameter
		public AnimatorControllerParameterType ParameterType;

		public CorgiEngineCharacterAnimationParameter(string name, AnimatorControllerParameterType type)
		{
			ParameterName = name;
			ParameterType = type;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class CharacterAnimationParametersInitializer : CorgiMonoBehaviour
	{
		[Header("Initialization")]
		/// if this is true, this component will remove itself after adding the character parameters
		[Tooltip("if this is true, this component will remove itself after adding the character parameters")]
		public bool AutoRemoveAfterInitialization = true;
		[MMInspectorButton("AddAnimationParameters")]
		public bool AddAnimationParametersButton;

		protected CorgiEngineCharacterAnimationParameter[] ParametersArray = new CorgiEngineCharacterAnimationParameter[]
		{
			new CorgiEngineCharacterAnimationParameter("Activating", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Airborne", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Alive", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("CollidingAbove", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("CollidingBelow", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("CollidingLeft", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("CollidingRight", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Crawling", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Crouching", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Damage", AnimatorControllerParameterType.Trigger),
			new CorgiEngineCharacterAnimationParameter("Dangling", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Dashing", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Death", AnimatorControllerParameterType.Trigger),
			new CorgiEngineCharacterAnimationParameter("Diving", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("FacingRight", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("FallDamage", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Flying", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("FlySpeed", AnimatorControllerParameterType.Float),
			new CorgiEngineCharacterAnimationParameter("Gliding", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Grounded", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Gripping", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Idle", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Jetpacking", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Jumping", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("DoubleJumping", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("HitTheGround", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("LadderClimbing", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("LadderClimbingSpeedX", AnimatorControllerParameterType.Float),
			new CorgiEngineCharacterAnimationParameter("LadderClimbingSpeedY", AnimatorControllerParameterType.Float),
			new CorgiEngineCharacterAnimationParameter("LedgeHanging", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("LedgeClimbing", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("LookingUp", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Pulling", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Pushing", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Random", AnimatorControllerParameterType.Float),
			new CorgiEngineCharacterAnimationParameter("RandomConstant", AnimatorControllerParameterType.Int),
			new CorgiEngineCharacterAnimationParameter("Running", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Speed", AnimatorControllerParameterType.Float),
			new CorgiEngineCharacterAnimationParameter("xSpeed", AnimatorControllerParameterType.Float),
			new CorgiEngineCharacterAnimationParameter("ySpeed", AnimatorControllerParameterType.Float),
			new CorgiEngineCharacterAnimationParameter("Swimming", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("SwimmingIdle", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("Walking", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("WallClinging", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("WallJumping", AnimatorControllerParameterType.Bool),
			new CorgiEngineCharacterAnimationParameter("WorldXSpeed", AnimatorControllerParameterType.Float),
			new CorgiEngineCharacterAnimationParameter("WorldYSpeed", AnimatorControllerParameterType.Float)
		};

		protected Animator _animator;
		#if UNITY_EDITOR
		protected AnimatorController _controller;
		#endif
		protected List<string> _parameters = new List<string>();

		/// <summary>
		/// Adds all the default animation parameters on your character's animator
		/// </summary>
		public virtual void AddAnimationParameters()
		{
			// we grab the animator
			_animator = this.gameObject.GetComponent<Animator>();
			if (_animator == null)
			{
				Debug.LogError("You need to add the AnimationParameterInitializer class to a gameobject with an Animator.");
			}

			// we grab the controller
			#if UNITY_EDITOR
			_controller = _animator.runtimeAnimatorController as AnimatorController;
			if (_controller == null)
			{
				Debug.LogError("You need an animator controller on this Animator.");
			}
			#endif

			// we store its parameters
			_parameters.Clear();
			foreach (AnimatorControllerParameter param in _animator.parameters)
			{
				_parameters.Add(param.name);
			}

			// we add all the listed parameters
			foreach (CorgiEngineCharacterAnimationParameter parameter in ParametersArray)
			{
				if (!_parameters.Contains(parameter.ParameterName))
				{
					#if UNITY_EDITOR
					_controller.AddParameter(parameter.ParameterName, parameter.ParameterType);
					#endif
				}
			}

			// we remove this component if needed
			if (AutoRemoveAfterInitialization)
			{
				DestroyImmediate(this);
			}
		}
	}
}