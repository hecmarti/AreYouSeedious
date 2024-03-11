using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// This class handles the GUI in the action phases of the Retro Adventure levels
	/// </summary>
	public class RetroAdventureGUIManager : GUIManager, MMEventListener<CorgiEngineEvent>
	{
		[Header("RetroAdventure")]
		/// the text used to display collected stars number
		[Tooltip("the text used to display collected stars number")]
		public Text StarDisplayText;
		/// the splash to display when the level is complete
		[Tooltip("the splash to display when the level is complete")]
		public GameObject LevelCompleteSplash;
		/// the object to give focus to when the complete splash gets displayed
		[Tooltip("the object to give focus to when the complete splash gets displayed")]
		public GameObject LevelCompleteSplashFocus;
		/// the GUI inventory displays
		[Tooltip("the GUI inventory displays")]
		public GameObject Inventories;
		/// the representation of the collected stars
		[Tooltip("the representation of the collected stars")]
		public Image[] Stars;
		/// the color to display a collected star with
		[Tooltip("the color to display a collected star with")]
		public Color StarOnColor;
		/// the color to display a not collected star with
		[Tooltip("the color to display a not collected star with")]
		public Color StarOffColor;

		/// <summary>
		/// On Update we update our star text
		/// </summary>
		protected virtual void Update()
		{
			UpdateStars ();
		}

		/// <summary>
		/// Every frame we update our star text with the current version
		/// </summary>
		protected virtual void UpdateStars()
		{
			StarDisplayText.text = RetroAdventureProgressManager.Instance.CurrentStars.ToString();
		}

		/// <summary>
		/// When the level is complete we display our level complete splash and set its values
		/// </summary>
		public virtual void LevelComplete()
		{
			if (Inventories != null)
			{
				Inventories.SetActive (false);	
			}
			EventSystem.current.sendNavigationEvents=true;
			GameManager.Instance.Pause (PauseMethods.NoPauseMenu);
			LevelCompleteSplash.SetActive (true);

			foreach (RetroAdventureScene scene in RetroAdventureProgressManager.Instance.Scenes)
			{
				if (scene.SceneName == SceneManager.GetActiveScene().name)
				{
					for (int i=0; i<Stars.Length; i++)
					{
						Stars [i].color = (scene.CollectedStars [i]) ? StarOnColor : StarOffColor;							
					}
				}
			}

			if (LevelCompleteSplashFocus != null)
			{
				EventSystem.current.SetSelectedGameObject(LevelCompleteSplashFocus, null);
			}
		}

		/// <summary>
		/// When grabbing a level complete event, we call our LevelComplete method
		/// </summary>
		/// <param name="corgiEngineEvent">Corgi engine event.</param>
		public virtual void OnMMEvent(CorgiEngineEvent corgiEngineEvent)
		{
			switch (corgiEngineEvent.EventType)
			{
				case CorgiEngineEventTypes.LevelComplete: 
					LevelComplete ();
					break;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.MMEventStartListening<CorgiEngineEvent>();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			this.MMEventStopListening<CorgiEngineEvent>();
		}
	}
}