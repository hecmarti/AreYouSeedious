using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this component to a character and it'll be able to be stunned. To stun a character, simply call its Stun or StunFor methods. You'll find test buttons at the bottom of this component's inspector. You can also use StunZones to stun your characters.
	/// Animator parameters : Stunned (bool)
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Stun")] 
	public class CharacterStun : CharacterAbility
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "Add this component to a character and it'll be able to be stunned. To stun a character, simply call its Stun or StunFor methods. You'll find test buttons at the bottom of this component's inspector. You can also use StunZones to stun your characters."; }
        
		[Header("Tests")]
		/// a test button to stun this character
		[MMInspectorButton("Stun")]
		public bool StunButton;
		/// a test button to exit stun on this character
		[MMInspectorButton("ExitStun")]
		public bool ExitStunButton;
        
		protected const string _stunnedAnimationParameterName = "Stunned";
		protected int _stunnedAnimationParameter;
		protected Coroutine _stunCoroutine;
		protected CharacterStates.CharacterConditions _previousCondition;

		/// <summary>
		/// Stuns the character
		/// </summary>
		public virtual void Stun()
		{
			if (_condition.CurrentState != CharacterStates.CharacterConditions.Stunned)
			{
				_previousCondition = _condition.CurrentState;	
			}
			_condition.ChangeState(CharacterStates.CharacterConditions.Stunned);
			_controller.SetForce(Vector2.zero);
			AbilityStartFeedbacks?.PlayFeedbacks();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Stun, MMCharacterEvent.Moments.Start);
		}
        
		/// <summary>
		/// Stuns the character for the specified duration
		/// </summary>
		/// <param name="duration"></param>
		public virtual void StunFor(float duration)
		{
			_stunCoroutine = StartCoroutine(StunCoroutine(duration));
		}

		/// <summary>
		/// Exits stun, resetting condition to the previous one
		/// </summary>
		public virtual void ExitStun()
		{
			AbilityStopFeedbacks?.PlayFeedbacks();
			_condition.ChangeState(_previousCondition);
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Stun, MMCharacterEvent.Moments.End);
			if (_stunCoroutine != null)
			{
				StopCoroutine(_stunCoroutine);
				_stunCoroutine = null;
			}
		}

		/// <summary>
		/// Stuns the character, waits for the specified duration, then exits stun
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		protected virtual IEnumerator StunCoroutine(float duration)
		{
			Stun();
			yield return MMCoroutine.WaitFor(duration);
			ExitStun();
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_stunnedAnimationParameterName, AnimatorControllerParameterType.Bool, out _stunnedAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our Running status to the character's animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _stunnedAnimationParameter, (_condition.CurrentState == CharacterStates.CharacterConditions.Stunned),_character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}
	}
}