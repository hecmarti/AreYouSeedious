using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEditor;

namespace MoreMountains.CorgiEngine
{	
	public static class RetroAdventureProgressManagerMenu 
	{
		[MenuItem("Tools/More Mountains/Reset all progress",false,21)]
		/// <summary>
		/// Adds a menu item to enable help
		/// </summary>
		private static void ResetProgress()
		{
			RetroAdventureProgressManager.Instance.ResetProgress ();
		}
	}
}