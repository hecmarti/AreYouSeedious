using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A class used to go from one level to the next while specifying an entry point in the target level. 
	/// Entry points are defined in each level's LevelManager component. They're simply Transforms in a list. 
	/// The index in the list is the identifier for the entry point. 
	/// </summary>
	[AddComponentMenu("Corgi Engine/Spawn/Go to level entry point")]
	public class GoToLevelEntryPoint : FinishLevel
	{
		[MMInspectorGroup("Points of Entry", true, 23)]

		/// the index of the target point of entry to spawn at in the next level
		[Tooltip("the index of the target point of entry to spawn at in the next level")]
		public int PointOfEntryIndex;
		/// the direction to face in the next level
		[Tooltip("the direction to face in the next level")]
		public Character.FacingDirections FacingDirection = Character.FacingDirections.Right;

		/// <summary>
		/// Loads the next level and stores the target entry point index in the game manager
		/// </summary>
		public override void GoToNextLevel()
		{
			GameManager.Instance.StorePointsOfEntry (LevelName, PointOfEntryIndex, FacingDirection);
			base.GoToNextLevel ();
		}
	}
}