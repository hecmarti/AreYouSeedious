using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.CorgiEngine
{
    public struct LevelNameEvent
	{
		public string LevelName;

		/// <summary>
		/// Initializes a LevelNameEvent
		/// </summary>
		/// <param name="levelName"></param>
		public LevelNameEvent(string levelName)
		{
			LevelName = levelName;
		}

		static LevelNameEvent e;
		public static void Trigger(string levelName)
		{
			e.LevelName = levelName;
			MMEventManager.TriggerEvent(e);
		}
	}

	/// <summary>
	/// A struct to describe and store points of entry
	/// </summary>
	[Serializable]
	public struct PointOfEntry
	{
		public string Name;
		public Transform Position;
		public MMFeedbacks EntryFeedback;
	}

	/// <summary>
	/// Spawns the player, handles checkpoints and respawn
	/// </summary>
	[AddComponentMenu("Corgi Engine/Managers/Level Manager")]
	public class LevelManager : MMSingleton<LevelManager>, MMEventListener<CorgiEngineEvent>
	{
		/// the possible checkpoint axis
		public enum CheckpointsAxis { x, y, z, CheckpointOrder}
		public enum CheckpointDirections { Ascending, Descending }
		public enum BoundsModes { TwoD, ThreeD }

		/// the prefab you want for your player
		[Header("Instantiate Characters")]
		[MMInformation("The LevelManager is responsible for handling spawn/respawn, checkpoints management and level bounds. Here you can define one or more playable characters for your level..",MMInformationAttribute.InformationType.Info,false)]

		/// the list of player prefabs to instantiate
		[Tooltip("the list of player prefabs to instantiate")]
		public Character[] PlayerPrefabs ;
		/// should the player IDs be auto attributed (usually yes)
		[Tooltip("should the player IDs be auto attributed (usually yes)")]
		public bool AutoAttributePlayerIDs = true;

		[Header("Characters already in the scene")]
		[MMInformation("It's recommended to have the LevelManager instantiate your characters, but if instead you'd prefer to have them already present in the scene, just bind them in the list below.", MMInformationAttribute.InformationType.Info, false)]

		/// a list of Characters already present in the scene before runtime. If this list is filled, PlayerPrefabs will be ignored
		[Tooltip("a list of Characters already present in the scene before runtime. If this list is filled, PlayerPrefabs will be ignored")]
		public List<Character> SceneCharacters;

		[Header("Checkpoints")]
		[MMInformation("Here you can select a checkpoint attribution axis (if your level is horizontal go for X, Y if it's vertical), and a debug spawn where your player character will spawn from while in editor mode.",MMInformationAttribute.InformationType.Info,false)]

		/// A checkpoint to use to force the character to spawn at
		[Tooltip("A checkpoint to use to force the character to spawn at")]
		public CheckPoint DebugSpawn;
		/// the axis on which objects should be compared
		[Tooltip("the axis on which objects should be compared")]
		public CheckpointsAxis CheckpointAttributionAxis = CheckpointsAxis.x;
		/// the direction in which checkpoint order should be determined
		[Tooltip("the direction in which checkpoint order should be determined")]
		public CheckpointDirections CheckpointAttributionDirection = CheckpointDirections.Ascending;

		/// the current checkpoint
		[Tooltip("the current checkpoint")]
		[MMReadOnly]
		public CheckPoint CurrentCheckPoint;

		[Space(10)]
		[Header("Points of Entry")]

		/// a list of all the points of entry for this level
		[Tooltip("a list of all the points of entry for this level.")]
		public List<PointOfEntry> PointsOfEntry;

		[Space(10)]
		[Header("Intro and Outro durations")]
		[MMInformation("Here you can specify the length of the fade in and fade out at the start and end of your level. You can also determine the delay before a respawn.",MMInformationAttribute.InformationType.Info,false)]

		/// duration of the initial fade in (in seconds)
		[Tooltip("duration of the initial fade in (in seconds)")]
		public float IntroFadeDuration=1f;
		/// duration of the fade to black at the end of the level (in seconds)
		[Tooltip("duration of the fade to black at the end of the level (in seconds)")]
		public float OutroFadeDuration=1f;
		/// the ID to use when triggering the event (should match the ID on the fader you want to use)
		[Tooltip("the ID to use when triggering the event (should match the ID on the fader you want to use)")]
		public int FaderID = 0;
		/// the curve to use for in and out fades
		[Tooltip("the curve to use for in and out fades")]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
		/// duration between a death of the main character and its respawn
		[Tooltip("duration between a death of the main character and its respawn")]
		public float RespawnDelay = 2f;
		/// if this is true, points will be reset when this level restarts - usually after a player's death
		[Tooltip("if this is true, points will be reset when this level restarts - usually after a player's death")]
		public bool ResetPointsOnRestart = true;

		[Space(10)]
		[Header("Level Bounds")]
		[MMInformation("The level bounds are used to constrain the camera's movement, as well as the player character's. You can see it in real time in the scene view as you adjust its size (it's the yellow box).",MMInformationAttribute.InformationType.Info,false)]


		/// whether to use a 3D or 2D collider as level bounds, this will be used by Cinemachine confiners
		[Tooltip("whether to use a 3D or 2D collider as level bounds")]
		public BoundsModes BoundsMode = BoundsModes.ThreeD;
		
		/// the level limits, camera and player won't go beyond this point.
		[Tooltip("the level limits, camera and player won't go beyond this point.")]
		public Bounds LevelBounds = new Bounds(Vector3.zero,Vector3.one*10);

		[MMInspectorButton("GenerateColliderBounds")]
		public bool ConvertToColliderBoundsButton;
		public Collider BoundsCollider { get; protected set; }
		public Collider2D BoundsCollider2D { get; protected set; }
        
		[Header("Scene Loading")]
		/// the method to use to load the destination level
		[Tooltip("the method to use to load the destination level")]
		public MMLoadScene.LoadingSceneModes LoadingSceneMode = MMLoadScene.LoadingSceneModes.MMSceneLoadingManager;
		/// the name of the MMSceneLoadingManager scene you want to use
		[Tooltip("the name of the MMSceneLoadingManager scene you want to use")]
		[MMEnumCondition("LoadingSceneMode", (int) MMLoadScene.LoadingSceneModes.MMSceneLoadingManager)]
		public string LoadingSceneName = "LoadingScreen";
		/// the settings to use when loading the scene in additive mode
		[Tooltip("the settings to use when loading the scene in additive mode")]
		[MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager)]
		public MMAdditiveSceneLoadingManagerSettings AdditiveLoadingSettings;

		[Header("Feedbacks")] 
		/// if this is true, an event will be triggered on player instantiation to set the range target of all feedbacks to it
		[Tooltip("if this is true, an event will be triggered on player instantiation to set the range target of all feedbacks to it")]
		public bool SetPlayerAsFeedbackRangeCenter = false;
        
		/// the elapsed time since the start of the level
		public TimeSpan RunningTime { get { return DateTime.UtcNow - _started ;}}
		public CameraController LevelCameraController { get; set; }

		// private stuff
		public List<Character> Players { get; protected set; }
		public List<CheckPoint> Checkpoints { get; protected set; }
		protected DateTime _started;
		protected int _savedPoints;
		protected string _nextLevel = null;
		protected BoxCollider _collider;
		protected BoxCollider2D _collider2D;
		protected Bounds _originalBounds;

		/// <summary>
		/// On awake, instantiates the player
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			_originalBounds = LevelBounds;
		}

		/// <summary>
		/// Instantiate playable characters based on the ones specified in the PlayerPrefabs list in the LevelManager's inspector.
		/// </summary>
		protected virtual void InstantiatePlayableCharacters()
		{
			Players = new List<Character> ();

			if (GameManager.Instance.PersistentCharacter != null)
			{
				Players.Add(GameManager.Instance.PersistentCharacter);
				return;
			}

			// we check if there's a stored character in the game manager we should instantiate
			if (GameManager.Instance.StoredCharacter != null)
			{
				Character newPlayer = (Character)Instantiate (GameManager.Instance.StoredCharacter, new Vector3 (0, 0, 0), Quaternion.identity);
				newPlayer.name = GameManager.Instance.StoredCharacter.name;
				Players.Add(newPlayer);
				return;
			}

			if ((SceneCharacters != null) && (SceneCharacters.Count > 0))
			{
				foreach(Character character in SceneCharacters)
				{
					Players.Add(character);
				}
				return;
			}

			if (PlayerPrefabs == null) { return; }

			// player instantiation
			if (PlayerPrefabs.Count() != 0)
			{
				foreach (Character playerPrefab in PlayerPrefabs)
				{
					Character newPlayer = (Character)Instantiate (playerPrefab, new Vector3 (0, 0, 0), Quaternion.identity);
					newPlayer.name = playerPrefab.name;
					Players.Add(newPlayer);

					if (playerPrefab.CharacterType != Character.CharacterTypes.Player)
					{
						Debug.LogWarning ("LevelManager : The Character you've set in the LevelManager isn't a Player, which means it's probably not going to move. You can change that in the Character component of your prefab.");
					}
				}
			}
			else
			{
				//Debug.LogWarning ("LevelManager : The Level Manager doesn't have any Player prefab to spawn. You need to select a Player prefab from its inspector.");
				return;
			}
		}

		/// <summary>
		/// Initialization
		/// </summary>
		public virtual void Start()
		{
			InstantiatePlayableCharacters ();
			if (Players == null || Players.Count == 0) { return; }

			Initialization ();

			
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.SpawnCharacterStarts);

			// we handle the spawn of the character(s)
			if (Players.Count == 1)
			{
				SpawnSingleCharacter ();
			}
			else
			{
				SpawnMultipleCharacters ();
			}

			LevelGUIStart();
			CheckpointAssignment ();

			// we trigger a level start event
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.LevelStart);
			MMGameEvent.Trigger("Load");

			if (SetPlayerAsFeedbackRangeCenter)
			{
				MMSetFeedbackRangeCenterEvent.Trigger(Players[0].transform);
			}
			
			MMCameraEvent.Trigger(MMCameraEventTypes.SetConfiner, null, BoundsCollider, BoundsCollider2D);
			MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, Players[0]);
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
		}

		/// <summary>
		/// Gets current camera, points number, start time, etc.
		/// </summary>
		protected virtual void Initialization()
		{
			// storage
			LevelCameraController = FindObjectOfType<CameraController>();
			_savedPoints=GameManager.Instance.Points;
			_started = DateTime.UtcNow;

			// if we don't find a bounds collider we generate one
			GenerateColliderBounds();

			switch (CheckpointAttributionAxis)
			{
				case CheckpointsAxis.x:
					if (CheckpointAttributionDirection == CheckpointDirections.Ascending)
					{
						Checkpoints = FindObjectsOfType<CheckPoint> ().OrderBy (o => o.transform.position.x).ToList ();
					}
					else
					{
						Checkpoints = FindObjectsOfType<CheckPoint> ().OrderByDescending (o => o.transform.position.x).ToList ();
					}
					break;
				case CheckpointsAxis.y:
					if (CheckpointAttributionDirection == CheckpointDirections.Ascending)
					{
						Checkpoints = FindObjectsOfType<CheckPoint> ().OrderBy (o => o.transform.position.y).ToList ();
					}
					else
					{
						Checkpoints = FindObjectsOfType<CheckPoint> ().OrderByDescending (o => o.transform.position.y).ToList ();
					}
					break;
				case CheckpointsAxis.z:
					if (CheckpointAttributionDirection == CheckpointDirections.Ascending)
					{
						Checkpoints = FindObjectsOfType<CheckPoint> ().OrderBy (o => o.transform.position.z).ToList ();
					}
					else
					{
						Checkpoints = FindObjectsOfType<CheckPoint> ().OrderByDescending (o => o.transform.position.z).ToList ();
					}
					break;
				case CheckpointsAxis.CheckpointOrder:
					Checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(o => o.CheckPointOrder).ToList();
					break;
			}

			// we assign the first checkpoint
			CurrentCheckPoint = Checkpoints.Count > 0 ? Checkpoints[0] : null;
		}

		/// <summary>
		/// Assigns all respawnable objects in the scene to their checkpoint
		/// </summary>
		protected virtual void CheckpointAssignment()
		{
			// we get all respawnable objects in the scene and attribute them to their corresponding checkpoint
			IEnumerable<Respawnable> listeners = FindObjectsOfType<MonoBehaviour>().OfType<Respawnable>();
			AutoRespawn autoRespawn;
			foreach(Respawnable listener in listeners)
			{
				for (int i = Checkpoints.Count - 1; i>=0; i--)
				{
					autoRespawn = (listener as MonoBehaviour).GetComponent<AutoRespawn>();
					if (autoRespawn != null)
					{
						if (autoRespawn.IgnoreCheckpointsAlwaysRespawn)
						{
							Checkpoints[i].AssignObjectToCheckPoint(listener);
							continue;
						}
						else
						{
							if (autoRespawn.AssociatedCheckpoints.Contains(Checkpoints[i]))
							{
								Checkpoints[i].AssignObjectToCheckPoint(listener);
								continue;
							}
							continue;
						}
					}
					
					Vector3 vectorDistance = ((MonoBehaviour) listener).transform.position - Checkpoints[i].transform.position;

					float distance = 0;
					if (CheckpointAttributionAxis == CheckpointsAxis.x)
					{
						distance = vectorDistance.x;
					}
					if (CheckpointAttributionAxis == CheckpointsAxis.y)
					{
						distance = vectorDistance.y;
					}
					if (CheckpointAttributionAxis == CheckpointsAxis.z)
					{
						distance = vectorDistance.z;
					}

					// if the object is behind the checkpoint (on the attribution axis), we move on to the next checkpoint
					if ((distance < 0) && (CheckpointAttributionDirection == CheckpointDirections.Ascending))
					{
						continue;
					}
					if ((distance > 0) && (CheckpointAttributionDirection == CheckpointDirections.Descending))
					{
						continue;
					}

					// if the object is further on the attribution axis compared to the checkpoint, we assign it to the checkpoint, and proceed to the next object
					Checkpoints[i].AssignObjectToCheckPoint(listener);
					break;
				}
			}
		}

		/// <summary>
		/// Initializes GUI stuff
		/// </summary>
		protected virtual void LevelGUIStart()
		{
			// set the level name in the GUI
			LevelNameEvent.Trigger(SceneManager.GetActiveScene().name);
			// fade in
			if (Players.Count > 0)
			{
				MMFadeOutEvent.Trigger(IntroFadeDuration, FadeTween, FaderID, false, Players[0].transform.position);
			}
			else
			{
				MMFadeOutEvent.Trigger(IntroFadeDuration, FadeTween, FaderID, false, Vector3.zero);
			}
		}

		/// <summary>
		/// Spawns a playable character into the scene
		/// </summary>
		protected virtual void SpawnSingleCharacter()
		{
			// in debug mode we spawn the player on the debug spawn point
			#if UNITY_EDITOR
			if (DebugSpawn!= null)
			{
				DebugSpawn.SpawnPlayer(Players[0]);
				return;
			}
			else
			{
				RegularSpawnSingleCharacter();
			}
			#else
				RegularSpawnSingleCharacter();
			#endif
		}

		/// <summary>
		/// Spawns the character at the selected entry point if there's one, or at the selected checkpoint.
		/// </summary>
		protected virtual void RegularSpawnSingleCharacter()
		{
			PointsOfEntryStorage point = GameManager.Instance.GetPointsOfEntry(SceneManager.GetActiveScene().name);
			if ((point != null) && (PointsOfEntry.Count >= (point.PointOfEntryIndex + 1)))
			{
				Players[0].RespawnAt(PointsOfEntry[point.PointOfEntryIndex].Position, point.FacingDirection);
				PointsOfEntry[point.PointOfEntryIndex].EntryFeedback?.PlayFeedbacks();
				return;
			}

			if (CurrentCheckPoint != null)
			{
				CurrentCheckPoint.SpawnPlayer(Players[0]);
				return;
			}
		}

		/// <summary>
		/// Spawns multiple playable characters into the scene
		/// </summary>
		protected virtual void SpawnMultipleCharacters()
		{
			int checkpointCounter = 0;
			int characterCounter = 1;
			bool spawned = false;
			foreach (Character player in Players)
			{
				spawned = false;

				if (AutoAttributePlayerIDs)
				{
					player.SetPlayerID("Player"+characterCounter);
				}

				player.name += " - " + player.PlayerID;

				if (Checkpoints.Count > checkpointCounter+1)
				{
					if (Checkpoints[checkpointCounter] != null)
					{
						Checkpoints[checkpointCounter].SpawnPlayer(player);
						characterCounter++;
						spawned = true;
						checkpointCounter++;
					}
				}
				if (!spawned)
				{
					Checkpoints[checkpointCounter].SpawnPlayer(player);
					characterCounter++;
				}
			}
		}

		/// <summary>
		/// Sets the current checkpoint.
		/// </summary>
		/// <param name="newCheckPoint">New check point.</param>
		public virtual void SetCurrentCheckpoint(CheckPoint newCheckPoint)
		{
			if (newCheckPoint.ForceAssignation)
			{
				CurrentCheckPoint = newCheckPoint;
				return;
			}

			if (CurrentCheckPoint == null)
			{
				CurrentCheckPoint = newCheckPoint;
				return;
			}
			if (newCheckPoint.CheckPointOrder >= CurrentCheckPoint.CheckPointOrder)
			{
				CurrentCheckPoint = newCheckPoint;
			}
		}

		/// <summary>
		/// Sets the name of the next level this LevelManager will point to
		/// </summary>
		/// <param name="levelName"></param>
		public virtual void SetNextLevel (string levelName)
		{
			_nextLevel = levelName;
		}

		/// <summary>
		/// Loads the next level, as defined via the SetNextLevel method
		/// </summary>
		public virtual void GotoNextLevel()
		{
			GotoLevel (_nextLevel);
			_nextLevel = null;
		}

		/// <summary>
		/// Gets the player to the specified level
		/// </summary>
		/// <param name="levelName">Level name.</param>
		public virtual void GotoLevel(string levelName, bool fadeOut = true, bool save = true)
		{
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.LevelEnd);
			if (save)
			{
				MMGameEvent.Trigger("Save");	
			}
			
			if (fadeOut)
			{
				if ((Players != null) && (Players.Count > 0))
				{
					MMFadeInEvent.Trigger(OutroFadeDuration, FadeTween, FaderID, true, Players[0].transform.position);
				}
				else
				{
					MMFadeInEvent.Trigger(OutroFadeDuration, FadeTween, FaderID, true, Vector3.zero);
				}
			}
            
			StartCoroutine(GotoLevelCo(levelName, fadeOut));
		}

		/// <summary>
		/// Waits for a short time and then loads the specified level
		/// </summary>
		/// <returns>The level co.</returns>
		/// <param name="levelName">Level name.</param>
		protected virtual IEnumerator GotoLevelCo(string levelName, bool fadeOut = true)
		{
			if (Players != null && Players.Count > 0)
			{
				foreach (Character player in Players)
				{
					player.Disable ();
				}
			}

			if (fadeOut)
			{
				if (Time.timeScale > 0.0f)
				{
					yield return new WaitForSeconds(OutroFadeDuration);
				}
				else
				{
					yield return new WaitForSecondsRealtime(OutroFadeDuration);
				}
			}

			// we trigger an unPause event for the GameManager (and potentially other classes)
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.UnPause);
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.LoadNextScene);

			string destinationScene = (string.IsNullOrEmpty(levelName)) ? "StartScreen" : levelName;
			switch (LoadingSceneMode)
			{
				case MMLoadScene.LoadingSceneModes.UnityNative:
					SceneManager.LoadScene(destinationScene);			        
					break;
				case MMLoadScene.LoadingSceneModes.MMSceneLoadingManager:
					MMSceneLoadingManager.LoadScene(destinationScene, LoadingSceneName);
					break;
				case MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(levelName, AdditiveLoadingSettings);
					break;
			}
		}

		/// <summary>
		/// Kills the player.
		/// </summary>
		public virtual void PlayerDead(Character player)
		{
			Health characterHealth = player.GetComponent<Health>();
			if (characterHealth == null)
			{
				return;
			}
			else
			{
				// if we've setup our game manager to use lives (meaning our max lives is more than zero)
				if (GameManager.Instance.MaximumLives > 0)
				{
					// we lose a life
					GameManager.Instance.LoseLife ();
					// if we're out of lives, we check if we have an exit scene, and move there
					if (GameManager.Instance.CurrentLives <= 0)
					{
						Cleanup();
						
						CorgiEngineEvent.Trigger(CorgiEngineEventTypes.GameOver);
						if ((GameManager.Instance.GameOverScene != null) && (GameManager.Instance.GameOverScene != ""))
						{
							MMSceneLoadingManager.LoadScene (GameManager.Instance.GameOverScene);
						}
					}
				}

				// if we have only one player, we restart the level
				if (Players.Count < 2)
				{
					StartCoroutine (SoloModeRestart ());
				}
			}
		}

		/// <summary>
		/// Resets lives, removes persistent characters and stored ones if needed
		/// </summary>
		protected virtual void Cleanup()
		{
			if (GameManager.Instance.ResetLivesOnGameOver)
			{
				GameManager.Instance.ResetLives();
			}
			if (GameManager.Instance.ResetPersistentCharacterOnGameOver)
			{
				GameManager.Instance.DestroyPersistentCharacter();
			}
			if (GameManager.Instance.ResetStoredCharacterOnGameOver)
			{
				GameManager.Instance.ClearStoredCharacter();
			}
		}

		/// <summary>
		/// Coroutine that kills the player, stops the camera, resets the points.
		/// </summary>
		/// <returns>The player co.</returns>
		protected virtual IEnumerator SoloModeRestart()
		{
			MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);

			yield return new WaitForSeconds(RespawnDelay);

			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
			
			if (CurrentCheckPoint != null)
			{
				CurrentCheckPoint.SpawnPlayer(Players[0]);
			}
            
			_started = DateTime.UtcNow;
			// we send a new points event for the GameManager to catch (and other classes that may listen to it too)
			if (ResetPointsOnRestart)
			{
				CorgiEnginePointsEvent.Trigger(PointsMethods.Set, 0);
			}
			// we trigger a respawn event
			ResetLevelBoundsToOriginalBounds();
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.Respawn);
		}

		/// <summary>
		/// Freezes the character(s)
		/// </summary>
		public virtual void FreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.Freeze();
			}
		}

		/// <summary>
		/// Unfreezes the character(s)
		/// </summary>
		public virtual void UnFreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.UnFreeze();
			}
		}

		/// <summary>
		/// Toggles Character Pause
		/// </summary>
		public virtual void ToggleCharacterPause()
		{
			foreach (Character player in Players)
			{

				CharacterPause characterPause = player?.FindAbility<CharacterPause>();
				if (characterPause == null)
				{
					break;
				}

				if (GameManager.Instance.Paused)
				{
					characterPause.PauseCharacter();
				}
				else
				{
					characterPause.UnPauseCharacter();
				}
			}
		}

		/// <summary>
		/// Resets the level bounds to their initial value
		/// </summary>
		public virtual void ResetLevelBoundsToOriginalBounds()
		{
			SetNewLevelBounds(_originalBounds);
		}

		/// <summary>
		/// Sets the level bound's min point to the one in parameters
		/// </summary>
		public virtual void SetNewMinLevelBounds(Vector3 newMinBounds)
		{
			LevelBounds.min = newMinBounds;
			UpdateBoundsCollider();
		}

		/// <summary>
		/// Sets the level bound's max point to the one in parameters
		/// </summary>
		/// <param name="newMaxBounds"></param>
		public virtual void SetNewMaxLevelBounds(Vector3 newMaxBounds)
		{
			LevelBounds.max = newMaxBounds;
			UpdateBoundsCollider();
		}

		/// <summary>
		/// Sets the level bounds to the one passed in parameters
		/// </summary>
		/// <param name="newBounds"></param>
		public virtual void SetNewLevelBounds(Bounds newBounds)
		{
			LevelBounds = newBounds;
			UpdateBoundsCollider();
		}

		/// <summary>
		/// Updates the level collider's bounds for Cinemachine (and others that may use it)
		/// </summary>
		protected virtual void UpdateBoundsCollider()
		{
			if (_collider != null)
			{
				this.transform.position = LevelBounds.center;
				_collider.size = LevelBounds.extents * 2f;
			}
		}

		/// <summary>
		/// A temporary method used to convert level bounds from the old system to actual collider bounds
		/// </summary>
		[ExecuteAlways]
		protected virtual void GenerateColliderBounds()
		{
			BoundsCollider = this.gameObject.GetComponent<Collider>();
			BoundsCollider2D = this.gameObject.GetComponent<CompositeCollider2D>();

			if ((BoundsCollider == null) && (BoundsCollider2D == null))
			{
				// set transform
				this.transform.position = LevelBounds.center;    
			}

			if ((BoundsCollider == null) && (BoundsMode == BoundsModes.ThreeD))
			{
				// remove existing collider
				if (this.gameObject.GetComponent<BoxCollider>() != null)
				{
					DestroyImmediate(this.gameObject.GetComponent<BoxCollider>());
				}

				// create collider
				_collider = this.gameObject.AddComponent<BoxCollider>();
				// set size
				_collider.size = LevelBounds.extents * 2f;

				// set layer
				this.gameObject.layer = LayerMask.NameToLayer("NoCollision");
			}

			if ((BoundsCollider2D == null) && (BoundsMode == BoundsModes.TwoD))
			{
				// remove existing collider
				if (this.gameObject.GetComponent<BoxCollider2D>() != null)
				{
					DestroyImmediate(this.gameObject.GetComponent<BoxCollider2D>());
				}

				Rigidbody2D rb = this.gameObject.AddComponent<Rigidbody2D>();
				rb.isKinematic = true;
				rb.simulated = false;
		        
				// create collider
				_collider2D = this.gameObject.AddComponent<BoxCollider2D>();
				// set size
				_collider2D.size = LevelBounds.extents * 2f;
				_collider2D.usedByComposite = true;

				// set layer
				this.gameObject.layer = LayerMask.NameToLayer("NoCollision");

				CompositeCollider2D composite = this.gameObject.AddComponent<CompositeCollider2D>();
				composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
			}
	        
			BoundsCollider = this.gameObject.GetComponent<Collider>();
			BoundsCollider2D = this.gameObject.GetComponent<CompositeCollider2D>();
		}
		
		/// <summary>
		/// Catches CorgiEngineEvents and acts on them, playing the corresponding sounds
		/// </summary>
		/// <param name="engineEvent">CorgiEngineEvent event.</param>
		public virtual void OnMMEvent(CorgiEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case CorgiEngineEventTypes.PlayerDeath:
					PlayerDead(engineEvent.OriginCharacter);
					break;
			}
		}

		/// <summary>
		/// OnDisable, we start listening to events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<CorgiEngineEvent>();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<CorgiEngineEvent>();
		}
	}
}