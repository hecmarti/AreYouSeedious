using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;

namespace MoreMountains.CorgiEngine
{
	public class SaveResetButton : CorgiMonoBehaviour
	{
		public virtual void ResetAllSaves()
		{
			MMSaveLoadManager.DeleteSaveFolder("MMAchievements");
			MMSaveLoadManager.DeleteSaveFolder("MMRetroAdventureProgress");
			MMSaveLoadManager.DeleteSaveFolder("InventoryEngine");
			MMSaveLoadManager.DeleteSaveFolder("CorgiEngine");
		}		
	}
}