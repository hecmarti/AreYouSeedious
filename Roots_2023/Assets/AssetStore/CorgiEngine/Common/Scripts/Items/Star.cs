using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A pickable star, that triggers a CorgiEngineStarEvent if picked
	/// It's up to you to implement something that will handle that event.
	/// You can look at the RetroStar and RetroAdventureProgressManager for examples of that.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Items/Star")]
	public class Star : PickableItem
	{
		/// the ID of this star, used by the progress manager to know which one got unlocked
		[Tooltip("the ID of this star, used by the progress manager to know which one got unlocked")]
		public int StarID;

		/// <summary>
		/// Triggered when something collides with the star
		/// </summary>
		/// <param name="collider">Other.</param>
		protected override void Pick(GameObject picker) 
		{
			// we send a new star event for anyone to catch 
			CorgiEngineStarEvent.Trigger(SceneManager.GetActiveScene().name, StarID);
		}
	}
}