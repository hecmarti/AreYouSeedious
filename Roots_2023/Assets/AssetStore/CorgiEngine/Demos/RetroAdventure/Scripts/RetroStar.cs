using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A pickable star, that triggers an event if picked, and disables itself if it's been previously collected
	/// </summary>
	[AddComponentMenu("Corgi Engine/Items/Retro Star")]
	public class RetroStar : Star
	{
		/// <summary>
		/// On Start we disable our star if needed
		/// </summary>
		protected override void Start()
		{
			base.Start ();
			DisableIfAlreadyCollected ();
		}

		/// <summary>
		/// Disables the star if it's already been collected in the past.
		/// </summary>
		protected virtual void DisableIfAlreadyCollected ()
		{
			foreach (RetroAdventureScene scene in RetroAdventureProgressManager.Instance.Scenes)
			{
				if (scene.SceneName == SceneManager.GetActiveScene().name)
				{
					if (scene.CollectedStars.Length >= StarID)
					{
						if (scene.CollectedStars[StarID])
						{
							Disable ();
						}
					}
				}
			}
		}

		/// <summary>
		/// Disable this star.
		/// </summary>
		protected virtual void Disable()
		{
			this.gameObject.SetActive (false);
		}
	}
}