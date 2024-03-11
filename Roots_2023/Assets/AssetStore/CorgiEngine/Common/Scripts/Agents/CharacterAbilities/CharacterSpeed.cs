using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A class used to store possible speeds for any state you need
	/// </summary>
	[System.Serializable]
	public class CharacterSpeedState
	{
		/// the selected movement state
		[Tooltip("the selected movement state")]
		public CharacterStates.MovementStates State;
		/// the speed modifier to apply when in that state
		[Tooltip("the speed modifier to apply when in that state")]
		public float SpeedModifier;
	}

	/// <summary>
	/// Add this ability to a Character and you'll be able to define speed modifiers for each of its possible states
	/// This modifier will be applied to the horizontal speed of the character as long as the character is in that state
	/// You can also define a default speed multiplier to apply if none of the defined states were found
	/// Animator parameters : none
	/// </summary>
	[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Speed")]
	public class CharacterSpeed : CharacterAbility
	{
		/// a list of states and their corresponding speed modifiers
		[Tooltip("a list of states and their corresponding speed modifiers")]
		public List<CharacterSpeedState> States;
		/// whether or not to apply the DefaultSpeedMultiplier when none of the above states is found
		[Tooltip("whether or not to apply the DefaultSpeedMultiplier when none of the above states is found")]
		public bool ApplyDefaultSpeedMultiplier = true;
		/// the default speed multiplier to apply when no other state is found
		[Tooltip("the default speed multiplier to apply when no other state is found")]
		public float DefaultSpeedMultiplier = 1f;
                
		/// <summary>
		/// On late update we check our states to see if we need to apply a speed multiplier
		/// </summary>
		protected virtual void LateUpdate()
		{
			CheckStates();
		}

		/// <summary>
		/// Compares the current state against our list of speed modifiers and apply it if needed
		/// </summary>
		protected virtual void CheckStates()
		{
			bool stateFound = false;

			foreach(CharacterSpeedState state in States)
			{
				if (state.State == _movement.CurrentState)
				{
					stateFound = true;
					_characterHorizontalMovement.StateSpeedMultiplier = state.SpeedModifier;
				}
			}

			if (!stateFound && ApplyDefaultSpeedMultiplier)
			{
				_characterHorizontalMovement.StateSpeedMultiplier = DefaultSpeedMultiplier;
			}
		}
	}
}