using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A class with a simple method to load the next level
	/// </summary>
	public class RetroNextLevel : CorgiMonoBehaviour 
	{
		/// <summary>
		/// Asks the level manager to load the next level
		/// </summary>
		public virtual void NextLevel()
		{
			LevelManager.Instance.GotoNextLevel ();
		}		
	}
}