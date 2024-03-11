using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.CorgiEngine
{	
	[CreateAssetMenu(fileName = "InventoryEngineKey", menuName = "MoreMountains/CorgiEngine/InventoryEngineKey", order = 1)]
	[Serializable]
	/// <summary>
	/// Pickable key item
	/// </summary>
	public class InventoryEngineKey : InventoryItem 
	{
		/// <summary>
		/// When the item is used, we simply return true
		/// </summary>
		public override bool Use(string playerID)
		{
			base.Use(playerID);

			return true;
		}
	}
}