using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.CorgiEngine
{	
	[System.Serializable]
	/// <summary>
	/// A serializable entity to store retro adventure scenes, whether they've been completed, unlocked, how many stars were collected, and which ones
	/// </summary>
	public class RetroAdventureScene
	{
		public string SceneName;
		public bool LevelComplete = false;
		public bool LevelUnlocked = false;
		public int MaxStars;
		public bool[] CollectedStars;
	}

	[System.Serializable]
	/// <summary>
	/// A serializable entity used to store progress : a list of scenes with their internal status (see above), how many lives are left, and how much we can have
	/// </summary>
	public class Progress
	{
		public int InitialMaximumLives = 0;
		public int InitialCurrentLives = 0;
		public int MaximumLives = 0;
		public int CurrentLives = 0;
		public RetroAdventureScene[] Scenes;
	}

	/// <summary>
	/// The RetroAdventureProgressManager class acts as an example of how you can implement progress in your game.
	/// There's no general class for that in the engine, for the simple reason that no two games will want to save the exact same things.
	/// But this should show you how it's done, and you can then copy and paste that into your own class (or extend this one, whatever you prefer).
	/// </summary>
	public class RetroAdventureProgressManager : MMSingleton<RetroAdventureProgressManager>, MMEventListener<CorgiEngineStarEvent>, MMEventListener<CorgiEngineEvent>
	{
		public int InitialMaximumLives { get; set; }
		public int InitialCurrentLives { get; set; }

		/// the list of scenes that we'll want to consider for our game
		[Tooltip("the list of scenes that we'll want to consider for our game")]
		public RetroAdventureScene[] Scenes;

		[MMInspectorButton("CreateSaveGame")]
		/// A button to test creating the save file
		public bool CreateSaveGameBtn;

		/// the current amount of collected stars
		public float CurrentStars { get; protected set; }

		protected const string _saveFolderName = "MMRetroAdventureProgress";
		protected const string _saveFileName = "Progress.data";

		/// <summary>
		/// On awake, we load our progress and initialize our stars counter
		/// </summary>
		protected override void Awake()
		{
			base.Awake ();
			LoadSavedProgress ();
			InitializeStars ();
		}

		/// <summary>
		/// When a level is completed, we update our progress
		/// </summary>
		protected virtual void LevelComplete()
		{
			for (int i = 0; i < Scenes.Length; i++)
			{
				if (Scenes[i].SceneName == SceneManager.GetActiveScene().name)
				{
					Scenes[i].LevelComplete = true;
					Scenes[i].LevelUnlocked = true;
					if (i < Scenes.Length - 1)
					{
						Scenes [i + 1].LevelUnlocked = true;
					}
				}
			}
		}

		/// <summary>
		/// Goes through all the scenes in our progress list, and updates the collected stars counter
		/// </summary>
		protected virtual void InitializeStars()
		{
			foreach (RetroAdventureScene scene in Scenes)
			{
				if (scene.SceneName == SceneManager.GetActiveScene().name)
				{
					int stars = 0;
					foreach (bool star in scene.CollectedStars)
					{
						if (star) { stars++; }
					}
					CurrentStars = stars;
				}
			}
		}

		/// <summary>
		/// Saves the progress to a file
		/// </summary>
		protected virtual void SaveProgress()
		{
			Progress progress = new Progress ();
			progress.MaximumLives = GameManager.Instance.MaximumLives;
			progress.CurrentLives = GameManager.Instance.CurrentLives;
			progress.InitialMaximumLives = InitialMaximumLives;
			progress.InitialCurrentLives = InitialCurrentLives;
			progress.Scenes = Scenes;

			MMSaveLoadManager.Save(progress, _saveFileName, _saveFolderName);
		}

		/// <summary>
		/// A test method to create a test save file at any time from the inspector
		/// </summary>
		protected virtual void CreateSaveGame()
		{
			SaveProgress();
		}

		/// <summary>
		/// Loads the saved progress into memory
		/// </summary>
		protected virtual void LoadSavedProgress()
		{
			Progress progress = (Progress)MMSaveLoadManager.Load(typeof(Progress), _saveFileName, _saveFolderName);
			if (progress != null)
			{
				GameManager.Instance.MaximumLives = progress.MaximumLives;
				GameManager.Instance.CurrentLives = progress.CurrentLives;
				InitialMaximumLives = progress.InitialMaximumLives;
				InitialCurrentLives = progress.InitialCurrentLives;
				Scenes = progress.Scenes;	
			}
			else
			{
				InitialMaximumLives = GameManager.Instance.MaximumLives;
				InitialCurrentLives = GameManager.Instance.CurrentLives;
			}
		}

		/// <summary>
		/// When we grab a star event, we update our scene status accordingly
		/// </summary>
		/// <param name="corgiStarEvent">Corgi star event.</param>
		public virtual void OnMMEvent(CorgiEngineStarEvent corgiStarEvent)
		{
			foreach (RetroAdventureScene scene in Scenes)
			{
				if (scene.SceneName == corgiStarEvent.SceneName)
				{
					scene.CollectedStars [corgiStarEvent.StarID] = true;
					CurrentStars++;
				}
			}
		}

		/// <summary>
		/// When we grab a level complete event, we update our status, and save our progress to file
		/// </summary>
		/// <param name="gameEvent">Game event.</param>
		public virtual void OnMMEvent(CorgiEngineEvent gameEvent)
		{
			switch (gameEvent.EventType)
			{
				case CorgiEngineEventTypes.LevelComplete:
					LevelComplete ();
					SaveProgress ();
					break;
				case CorgiEngineEventTypes.GameOver:
					GameOver ();
					break;
			}
		} 

		/// <summary>
		/// This method describes what happens when the player loses all lives. In this case, we reset its progress and all lives will be reset.
		/// </summary>
		protected virtual void GameOver()
		{
			ResetProgress ();
			ResetLives ();
		}

		/// <summary>
		/// Resets the number of lives to its initial values
		/// </summary>
		protected virtual void ResetLives()
		{
			GameManager.Instance.MaximumLives = InitialMaximumLives;
			GameManager.Instance.CurrentLives = InitialCurrentLives;
		}

		/// <summary>
		/// A method used to remove all save files associated to progress
		/// </summary>
		public virtual void ResetProgress()
		{
			MMSaveLoadManager.DeleteSaveFolder ("MMRetroAdventureProgress");			
		}

		/// <summary>
		/// OnEnable, we start listening to events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<CorgiEngineStarEvent> ();
			this.MMEventStartListening<CorgiEngineEvent>();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<CorgiEngineStarEvent> ();
			this.MMEventStopListening<CorgiEngineEvent>();
		}		
	}
}