using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This component allows the definition of a level that can then be accessed and loaded. Used mostly in the level map scene.
	/// </summary>
	public class LevelSelector : CorgiMonoBehaviour
	{
		/// the exact name of the target level
		[Tooltip("the exact name of the target level")]
		public string LevelName;
		/// whether or not changing level should trigger a fade
		[Tooltip("whether or not changing level should trigger a fade")]
		public bool Fade = true;
		/// whether or not changing level should trigger a save event
		[Tooltip("whether or not changing level should trigger a save event")]
		public bool Save = true;


		/// <summary>
		/// Loads the level specified in the inspector
		/// </summary>
		public virtual void GoToLevel()
		{
			LevelManager.Instance.GotoLevel(LevelName, Fade, Save);
		}

		/// <summary>
		/// Restarts the current level
		/// </summary>
		public virtual void RestartLevel()
		{
			LevelManager.Instance.GotoLevel(SceneManager.GetActiveScene().name, Fade, Save);
			// we trigger an unPause event for the GameManager (and potentially other classes)
			//CorgiEngineEvent.Trigger(CorgiEngineEventTypes.UnPause);
			//LoadingSceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}		
	}
}