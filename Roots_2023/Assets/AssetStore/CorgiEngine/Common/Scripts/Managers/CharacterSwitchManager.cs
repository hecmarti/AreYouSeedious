using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this component to an empty object in your scene, and when you'll press the SwitchCharacter button (P by default, change that in Unity's InputManager settings), 
	/// your main character will be replaced by one of the prefabs in the list set on this component. You can decide the order (sequential or random), and have as many as you want.
	/// Note that this will change the whole prefab, not just the visuals. 
	/// If you're just after a visual change, look at the CharacterSwitchModel ability.
	/// If you want to swap characters between a bunch of characters within a scene, look at the CharacterSwap ability and CharacterSwapManager
	/// </summary>
	public class CharacterSwitchManager : CorgiMonoBehaviour
	{
		/// the possible orders the next character can be selected from
		public enum NextCharacterChoices { Sequential, Random }

		[Header("Character Switch")]
		[MMInformation("Add this component to an empty object in your scene, and when you'll press the SwitchCharacter button (P by default, change that in Unity's InputManager settings), your main character will be replaced by one of the prefabs in the list set on this component. You can decide the order (sequential or random), and have as many as you want.", MMInformationAttribute.InformationType.Info, false)]
		/// the ID of the Player we want this CharacterSwitchManager to control
		[Tooltip("the ID of the Player we want this CharacterSwitchManager to control")]
		public string PlayerID = "Player1";
		/// the index of the Player in the LevelManager we want to target
		[Tooltip("the index of the Player in the LevelManager we want to target")]
		public int PlayerIndex = 0;
		/// the list of possible characters prefabs to switch to
		[Tooltip("the list of possible characters prefabs to switch to")]
		public Character[] CharacterPrefabs;
		/// the order in which to pick the next character
		[Tooltip("the order in which to pick the next character")]
		public NextCharacterChoices NextCharacterChoice = NextCharacterChoices.Sequential;
		/// the initial (and at runtime, current) index of the character prefab
		[Tooltip("the initial (and at runtime, current) index of the character prefab")]
		public int CurrentIndex = 0;
		/// if this is true, current health value will be passed from character to character
		[Tooltip("if this is true, current health value will be passed from character to character")]
		public bool CommonHealth;
		/// if this is true, newly switched to characters will flip (if needed) to face the direction the previous character was facing
		[Tooltip("if this is true, newly switched to characters will flip (if needed) to face the direction the previous character was facing")]
		public bool MaintainPreviousCharacterFacingDirection = false;

		[Header("Visual Effects")]

		/// a particle system to play when a character gets changed
		[Tooltip("a particle system to play when a character gets changed")]
		public ParticleSystem CharacterSwitchVFX;

		[Header("Debug")]
		/// a test button that forces a character switch
		[MMInspectorButton("ForceCharacterSwitch")]
		public bool ForceCharacterSwitchButton;
		/// the index that will be used when pressing the DebugCharacterSwitchToTargetIndex button below
		[Tooltip("the index that will be used when pressing the DebugCharacterSwitchToTargetIndex button below")]
		public int DebugTargetIndex = 1;
		/// a test button that forces a character switch to the specified index in DebugTargetIndex
		[MMInspectorButton("DebugCharacterSwitchToTargetIndex")]
		public bool DebugCharacterSwitchToTargetIndexButton;
		/// the input manager we'll read input on
		[MMReadOnly]
		public InputManager LinkedInputManager;

		protected Character[] _instantiatedCharacters;
		protected ParticleSystem _instantiatedVFX;
		protected CorgiEngineEvent _switchEvent = new CorgiEngineEvent(CorgiEngineEventTypes.CharacterSwitch, null);

		/// <summary>
		/// A public method you can use to switch to the next character in the rotation
		/// </summary>
		public virtual void ForceCharacterSwitch()
		{
			StartCoroutine(SwitchCharacter());
		}

		/// <summary>
		/// A public method you can use to switch to a specific character in the array
		/// </summary>
		/// <param name="newIndex"></param>
		public virtual void ForceCharacterSwitchTo(int newIndex)
		{
			StartCoroutine(SwitchCharacter(newIndex));
		}

		/// <summary>
		/// On Awake we grab our input manager and instantiate our characters and VFX
		/// </summary>
		protected virtual void Start()
		{
			GetInputManager();
			InstantiateCharacters();
			InstantiateVFX();
		}

		/// <summary>
		/// We grab the corresponding input manager
		/// </summary>
		protected virtual void GetInputManager()
		{
			// we get the corresponding input manager
			if (!string.IsNullOrEmpty(PlayerID))
			{
				LinkedInputManager = null;
				InputManager[] foundInputManagers = FindObjectsOfType(typeof(InputManager)) as InputManager[];
				foreach (InputManager foundInputManager in foundInputManagers)
				{
					if (foundInputManager.PlayerID == PlayerID)
					{
						LinkedInputManager = foundInputManager;
					}
				}
			}
		}

		/// <summary>
		/// Instantiates and disables all characters in our list
		/// </summary>
		protected virtual void InstantiateCharacters()
		{
			_instantiatedCharacters = new Character[CharacterPrefabs.Length];

			for (int i = 0; i < CharacterPrefabs.Length; i++)
			{
				Character newCharacter = Instantiate(CharacterPrefabs[i]);
				newCharacter.name = "CharacterSwitch_" + i;
				newCharacter.gameObject.SetActive(false);
				newCharacter.transform.position = this.transform.position;
				_instantiatedCharacters[i] = newCharacter;
			}            
		}

		/// <summary>
		/// Instantiates and disables the particle system if needed
		/// </summary>
		protected virtual void InstantiateVFX()
		{
			if (CharacterSwitchVFX != null)
			{
				_instantiatedVFX = Instantiate(CharacterSwitchVFX);
				_instantiatedVFX.Stop();
				_instantiatedVFX.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// On Update we watch for our input
		/// </summary>
		protected virtual void Update()
		{
			if (LinkedInputManager == null)
			{
				return;
			}

			if (LinkedInputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				StartCoroutine(SwitchCharacter());
			}
		}

		/// <summary>
		/// A test method that switches to the specified debug index
		/// </summary>
		protected virtual void DebugCharacterSwitchToTargetIndex()
		{
			StartCoroutine(SwitchCharacter(DebugTargetIndex));
		}

		/// <summary>
		/// Determines the next index to go to based on the choice mode settings
		/// </summary>
		protected virtual void DetermineNextIndex()
		{
			// we determine the next index
			if (NextCharacterChoice == NextCharacterChoices.Random)
			{
				CurrentIndex = Random.Range(0, _instantiatedCharacters.Length);
			}
			else if (NextCharacterChoice == NextCharacterChoices.Sequential)
			{
				CurrentIndex = CurrentIndex + 1;
				if (CurrentIndex >= _instantiatedCharacters.Length)
				{
					CurrentIndex = 0;
				}
			}
		}

		/// <summary>
		/// Switches to the next character in the list
		/// </summary>
		protected virtual IEnumerator SwitchCharacter()
		{
			if (_instantiatedCharacters.Length <= 1)
			{
				yield break;
			}

			DetermineNextIndex();

			StartCoroutine(OperateSwitch());
		}

		/// <summary>
		/// A coroutine to switch to a specific index
		/// </summary>
		/// <param name="newIndex"></param>
		/// <returns></returns>
		protected virtual IEnumerator SwitchCharacter(int newIndex)
		{
			if (_instantiatedCharacters.Length <= 1)
			{
				yield break;
			}

			CurrentIndex = newIndex;
			if ((CurrentIndex <0) || (CurrentIndex >= _instantiatedCharacters.Length))
			{
				CurrentIndex = 0;
			}

			StartCoroutine(OperateSwitch());
		}

		/// <summary>
		/// A coroutine that changes the character, triggers an event, and sets its Health
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator OperateSwitch()
		{
			float newHealth = LevelManager.Instance.Players[PlayerIndex].gameObject.MMGetComponentNoAlloc<Health>().CurrentHealth;
			bool facingRight = LevelManager.Instance.Players[PlayerIndex].IsFacingRight;

			// we disable the old main character, and enable the new one
			LevelManager.Instance.Players[PlayerIndex].gameObject.SetActive(false);
			_instantiatedCharacters[CurrentIndex].SetPlayerID(PlayerID);
			_instantiatedCharacters[CurrentIndex].gameObject.SetActive(true);

			// we move the new one at the old one's position
			_instantiatedCharacters[CurrentIndex].transform.position = LevelManager.Instance.Players[PlayerIndex].transform.position;
			_instantiatedCharacters[CurrentIndex].transform.rotation = LevelManager.Instance.Players[PlayerIndex].transform.rotation;

			// we put it in the same state the old one was in
			_instantiatedCharacters[CurrentIndex].MovementState.ChangeState(LevelManager.Instance.Players[PlayerIndex].MovementState.CurrentState);
			_instantiatedCharacters[CurrentIndex].ConditionState.ChangeState(LevelManager.Instance.Players[PlayerIndex].ConditionState.CurrentState);

			// we make it the current character
			LevelManager.Instance.Players[PlayerIndex] = _instantiatedCharacters[CurrentIndex];

			// we play our vfx
			if (_instantiatedVFX != null)
			{
				_instantiatedVFX.gameObject.SetActive(true);
				_instantiatedVFX.transform.position = _instantiatedCharacters[CurrentIndex].transform.position;
				_instantiatedVFX.Play();
			}

			// we trigger a switch event (for the camera to know, mostly)
			MMEventManager.TriggerEvent(_switchEvent);

			yield return null;

			// we keep the health if needed
			if (CommonHealth)
			{
				LevelManager.Instance.Players[PlayerIndex].gameObject.MMGetComponentNoAlloc<Health>().SetHealth(newHealth, this.gameObject);
			}
			
			// we force facing direction if needed
			if (MaintainPreviousCharacterFacingDirection)
			{
				Character.FacingDirections facingDirection = facingRight ? Character.FacingDirections.Right : Character.FacingDirections.Left;
				LevelManager.Instance.Players[PlayerIndex].Face(facingDirection);
			}
		}
	}
}