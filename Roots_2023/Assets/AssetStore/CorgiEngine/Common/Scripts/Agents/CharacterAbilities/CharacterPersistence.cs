using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this component to a Character and it'll persist with its exact current state when transitioning to a new scene.
	/// It'll be automatically passed to the new scene's LevelManager to be used as this scene's main character.
	/// It'll keep the exact state all its components are in at the moment they finish the level.
	/// Its health, enabled abilities, component values, equipped weapons, new components you may have added, etc, will all remain once in the new scene. 
	/// Animator parameters : None
	/// </summary>
	[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Persistence")]
	public class CharacterPersistence : CharacterAbility, MMEventListener<MMGameEvent>, MMEventListener<CorgiEngineEvent>
	{
		public bool Initialized { get; set; }
        
		/// <summary>
		/// On Start(), we prevent our character from being destroyed if needed
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();

			if (AbilityAuthorized)
			{
				DontDestroyOnLoad(this.gameObject);
			}

			Initialized = true;
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			Initialized = false;
		}

		/// <summary>
		/// When we get a save request, we store our character in the game manager for future use
		/// </summary>
		/// <param name="gameEvent"></param>
		public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			if (gameEvent.EventName == "Save")
			{
				SaveCharacter();
			}
		}

		/// <summary>
		/// When we get a Corgi Engine event, we act on it
		/// </summary>
		/// <param name="gameEvent"></param>
		public virtual void OnMMEvent(CorgiEngineEvent engineEvent)
		{
			if (!AbilityAuthorized)
			{
				return;
			}

			switch (engineEvent.EventType)
			{
				case CorgiEngineEventTypes.LoadNextScene:
					this.gameObject.SetActive(false);
					break;
				case CorgiEngineEventTypes.SpawnCharacterStarts:
					this.gameObject.SetActive(true);
					Character character = this.gameObject.GetComponentInParent<Character>(); 
					character.enabled = true;
					character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
					character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
					character.GetMainCamera();
					character.SetInputManager();
					CorgiController controller = this.gameObject.GetComponentInParent<CorgiController>();
					if (controller != null)
					{
						controller.enabled = true;	
					}
					break;
				case CorgiEngineEventTypes.LevelStart:
					if (_health != null)
					{
						_health.StoreInitialPosition();    
					}
					break;
				case CorgiEngineEventTypes.Respawn:
					Initialized = true;
					break;
			}
		}

		/// <summary>
		/// Saves to the game manager a reference to our character
		/// </summary>
		protected virtual void SaveCharacter()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			GameManager.Instance.SetPersistentCharacter(_character);
		}

		/// <summary>
		/// Clears any saved character that may have been stored in the GameManager
		/// </summary>
		public virtual void ClearSavedCharacter()
		{
			if (!AbilityAuthorized)
			{
				return;
			}

			GameManager.Instance.SetPersistentCharacter(null);
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			this.MMEventStartListening<MMGameEvent>();
			this.MMEventStartListening<CorgiEngineEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDestroy()
		{
			this.MMEventStopListening<MMGameEvent>();
			this.MMEventStopListening<CorgiEngineEvent>();
		}
	}
}