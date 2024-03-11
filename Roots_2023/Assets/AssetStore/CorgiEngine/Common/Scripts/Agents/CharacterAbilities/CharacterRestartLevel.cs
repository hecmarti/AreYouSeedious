using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this component to a character and you'll be able to restart the level at the press of a button, either killing the player, transitioning to another scene, or teleporting to the initial, last, or current checkpoint
	/// </summary>
	[MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Restart Level")] 
	public class CharacterRestartLevel : CharacterAbility 
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "Add this component to a character and you'll be able to restart the level at the press of a button, either killing the player, transitioning to another scene, or teleporting to the initial, last, or current checkpoint"; }
		
		/// the possible restart modes
		public enum RestartModes { KillPlayer, GoToScene, CurrentCheckpoint, FirstCheckpoint, LastCheckpoint }

		[Header("Restart")]
		/// the selected restart mode
		[Tooltip("the selected restart mode")]
		public RestartModes RestartMode;
		/// the scene to go to if RestartMode is GoToScene
		[Tooltip("the scene to go to if RestartMode is GoToScene")]
		[MMEnumCondition("RestartMode", (int)RestartModes.GoToScene)]
		public string TargetSceneName;
        
		/// <summary>
		/// Every frame, we check the input to see if we should restart 
		/// </summary>
		protected override void HandleInput()
		{
			if (_inputManager.RestartButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) 				
			{
				Restart();
			}
		}

		/// <summary>
		/// Restarts according to the specified choice
		/// </summary>
		public virtual void Restart()
		{
			PlayAbilityStartFeedbacks();
			switch (RestartMode)
			{
				case RestartModes.KillPlayer:
					_character.CharacterHealth.Kill();
					break;
				case RestartModes.GoToScene:
					MMSceneLoadingManager.LoadScene(TargetSceneName);
					break;
				case RestartModes.CurrentCheckpoint:
					_character.transform.position = LevelManager.Instance.CurrentCheckPoint.transform.position;
					break;
				case RestartModes.FirstCheckpoint:
					_character.transform.position = LevelManager.Instance.Checkpoints[0].transform.position;
					break;
				case RestartModes.LastCheckpoint:
					_character.transform.position = LevelManager.Instance.Checkpoints[LevelManager.Instance.Checkpoints.Count - 1].transform.position;
					break;
			}
		}
	}
}