using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This ability lets you swap entire ability nodes for the ones set in parameters
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/CharacterAbilityNodeSwap")]
	public class CharacterAbilityNodeSwap : CharacterAbility
	{
		[Header("Ability Node Swap")]
        
		/// a list of GameObjects that will replace this Character's set of ability nodes when the ability executes
		[Tooltip("a list of GameObjects that will replace this Character's set of ability nodes when the ability executes")]
		public List<GameObject> AdditionalAbilityNodes;

		/// <summary>
		/// If the player presses the SwitchCharacter button, we swap abilities.
		/// This ability reuses the SwitchCharacter input to avoid multiplying input entries, but feel free to override this method to add a dedicated one
		/// </summary>
		protected override void HandleInput()
		{
			if (_inputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				Swap();
			}
		}

		/// <summary>
		/// Swaps ability nodes
		/// </summary>
		protected virtual void Swap()
		{
			_character.Reset();
			_movement.ChangeState(CharacterStates.MovementStates.Idle);
			SwapAbilityNodes();
		}

		/// <summary>
		/// Disables the old ability nodes, swaps with the new, and enables them
		/// </summary>
		public virtual void SwapAbilityNodes()
		{
			foreach (GameObject node in _character.AdditionalAbilityNodes)
			{
				node.gameObject.SetActive(false);
			}
            
			_character.AdditionalAbilityNodes = AdditionalAbilityNodes;

			foreach (GameObject node in _character.AdditionalAbilityNodes)
			{
				node.gameObject.SetActive(true);
			}
            
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.AbilityNodeSwap);

			_character.CacheAbilities();
		}
	}
}