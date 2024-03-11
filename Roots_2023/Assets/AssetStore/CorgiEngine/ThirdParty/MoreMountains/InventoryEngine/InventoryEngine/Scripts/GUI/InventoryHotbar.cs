using UnityEngine;
using MoreMountains.Tools;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.InventoryEngine
{	
	/// <summary>
	/// Special kind of inventory display, with a dedicated key associated to it, to allow for shortcuts for use and equip
	/// </summary>
	public class InventoryHotbar : InventoryDisplay 
	{
		/// the possible actions that can be done on objects in the hotbar
		public enum HotbarPossibleAction { Use, Equip }
		[Header("Hotbar")]

		[MMInformation("Here you can define the keys your hotbar will listen to to activate the hotbar's action.",MMInformationAttribute.InformationType.Info,false)]
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		/// the key used to open/close the inventory
		public Key HotbarKey = Key.I;
		#else
		/// the key associated to the hotbar, that will trigger the action when pressed
		public string HotbarKey;
		/// the alt key associated to the hotbar
		public string HotbarAltKey;	
		#endif
		/// the action associated to the key or alt key press
		public HotbarPossibleAction ActionOnKey	;

		/// <summary>
		/// Executed when the key or alt key gets pressed, triggers the specified action
		/// </summary>
		public virtual void Action()
		{
			for (int i = TargetInventory.Content.Length-1 ; i>=0 ; i--)
			{
				if (!InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					if ((ActionOnKey == HotbarPossibleAction.Equip) && (SlotContainer[i] != null))
					{
						SlotContainer[i].Equip();
					}
					if ((ActionOnKey == HotbarPossibleAction.Use) && (SlotContainer[i] != null))
					{
						SlotContainer[i].Use();
					}
					return;
				}
			}
		}
	}
}