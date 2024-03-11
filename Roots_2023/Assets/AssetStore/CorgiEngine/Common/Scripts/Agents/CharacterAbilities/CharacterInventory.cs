using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this component to a character and it'll be able to control an inventory
	/// Animator parameters : none
	/// Note that its start feedback will play on weapon change
	/// </summary>

	[MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Inventory")] 
	public class CharacterInventory : CharacterAbility, MMEventListener<MMInventoryEvent>, MMEventListener<CorgiEngineEvent>
	{
		/// <summary>
		/// A struct used to store inventory items to add on init
		/// </summary>
		[System.Serializable]
		public struct InventoryItemsToAdd
		{
			public InventoryItem Item;
			public int Quantity;
		}

		public enum WeaponRotationModes { Normal, AddEmptySlot, AddInitialWeapon }

		[Header("Inventories")]
		/// the unique ID of this player as far as the InventoryEngine is concerned. This has to match all its Inventory and InventoryEngine UI components' PlayerID for that player. If you're not going for multiplayer here, just leave Player1.
		[Tooltip("the unique ID of this player as far as the InventoryEngine is concerned. This has to match all its Inventory and InventoryEngine UI components' PlayerID for that player. If you're not going for multiplayer here, just leave Player1.")]
		public string PlayerID = "Player1";
		/// the name of the main inventory
		[Tooltip("the name of the main inventory")]
		public string MainInventoryName;
		/// the name of the weapon inventory
		[Tooltip("the name of the weapon inventory")]
		public string WeaponInventoryName;
		/// the name of the hotbar inventory
		[Tooltip("the name of the hotbar inventory")]
		public string HotbarInventoryName;
		/// if this is true, when switching to this character, if there's a main weapon equipped, it'll be equipped
		[Tooltip("if this is true, when switching to this character, if there's a main weapon equipped, it'll be equipped")]
		public bool AutoEquipWeaponOnCharacterSwitch;
		/// the rotation mode for weapons : Normal will cycle through all weapons, AddEmptySlot will return to empty hands, AddOriginalWeapon will cycle back to the original weapon
		[Tooltip("the rotation mode for weapons : Normal will cycle through all weapons, AddEmptySlot will return to empty hands, AddOriginalWeapon will cycle back to the original weapon")]
		public WeaponRotationModes WeaponRotationMode = WeaponRotationModes.Normal;
		/// the target handle weapon ability - if left empty, will pick the first one it finds
		[Tooltip("the target handle weapon ability - if left empty, will pick the first one it finds")]
		public CharacterHandleWeapon CharacterHandleWeapon;
		/// a transform to pass to the inventories, will be passed to the inventories and used as reference for drops. If left empty, this.transform will be used.
		[Tooltip("a transform to pass to the inventories, will be passed to the inventories and used as reference for drops. If left empty, this.transform will be used.")]
		public Transform InventoryTransform;

		[Header("Start")]
		/// a list of items and associated quantities to add to the main inventory
		public List<InventoryItemsToAdd> AutoAddItemsMainInventory;
		/// a list of items to add to the hotbar
		public List<InventoryItemsToAdd> AutoAddItemsHotbar;
		/// a weapon to automatically add to the inventory and equip on init
		public InventoryEngineWeapon AutoEquipWeapon;
		
		public List<string> AvailableWeaponsIDs => _availableWeaponsIDs;

		/// the reference to the main inventory
		public Inventory MainInventory { get; set; }
		/// the reference to the weapon inventory
		public Inventory WeaponInventory { get; set; }
		/// the reference to the hotbar inventory
		public Inventory HotbarInventory { get; set; }

		protected List<int> _availableWeapons;
		protected List<string> _availableWeaponsIDs;
		protected string _nextWeaponID;
		protected bool _nextFrameWeapon = false;
		protected string _nextFrameWeaponName;
		protected const string _emptySlotWeaponName = "_EmptySlotWeaponName";
		protected const string _initialSlotWeaponName = "_InitialSlotWeaponName";
		protected bool _autoAdded = false;

		/// <summary>
		/// On init, we trigger our setup
		/// </summary>
		protected override void Initialization () 
		{
			base.Initialization();
			Setup ();
			StartCoroutine(AutoAddAndEquip());
		}

		/// <summary>
		/// On process ability, we equip our next weapon if needed
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			if (_nextFrameWeapon)
			{
				EquipWeapon(_nextFrameWeaponName);
				_nextFrameWeapon = false;
			}
		}

		/// <summary>
		/// Setup grabs inventories, component, and fills the weapon lists
		/// </summary>
		protected virtual void Setup()
		{
			if (InventoryTransform == null)
			{
				InventoryTransform = this.transform;
			}
			GrabInventories ();
			if (CharacterHandleWeapon == null)
			{
				CharacterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();
			}
			FillAvailableWeaponsLists ();
		}

		/// <summary>
		/// Automatically adds items and equips a weapon if needed
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator AutoAddAndEquip()
		{
			yield return MMCoroutine.WaitForFrames(1);

			if (_autoAdded)
			{
				yield break;
			}

			foreach (InventoryItemsToAdd item in AutoAddItemsMainInventory)
			{
				MainInventory?.AddItem(item.Item, item.Quantity);
			}
			foreach (InventoryItemsToAdd item in AutoAddItemsHotbar)
			{
				HotbarInventory?.AddItem(item.Item, item.Quantity);
			}
			if (AutoEquipWeapon != null)
			{
				MainInventory.AddItem(AutoEquipWeapon, 1);
				EquipWeapon(AutoEquipWeapon.ItemID);
			}
			_autoAdded = true;
		}

		/// <summary>
		/// Grabs references to all inventories
		/// </summary>
		protected virtual void GrabInventories()
		{
			if (MainInventory == null)
			{
				GameObject mainInventoryTmp = GameObject.Find (MainInventoryName);
				if (mainInventoryTmp != null) { MainInventory = mainInventoryTmp.GetComponent<Inventory> (); }	
			}
			if (WeaponInventory == null)
			{
				GameObject weaponInventoryTmp = GameObject.Find (WeaponInventoryName);
				if (weaponInventoryTmp != null) { WeaponInventory = weaponInventoryTmp.GetComponent<Inventory> (); }	
			}
			if (HotbarInventory == null)
			{
				GameObject hotbarInventoryTmp = GameObject.Find (HotbarInventoryName);
				if (hotbarInventoryTmp != null) { HotbarInventory = hotbarInventoryTmp.GetComponent<Inventory> (); }	
			}
			if (MainInventory != null) { MainInventory.SetOwner (this.gameObject); MainInventory.TargetTransform = InventoryTransform;}
			if (WeaponInventory != null) { WeaponInventory.SetOwner (this.gameObject); WeaponInventory.TargetTransform = InventoryTransform;}
			if (HotbarInventory != null) { HotbarInventory.SetOwner (this.gameObject); HotbarInventory.TargetTransform = InventoryTransform;}
		}

		/// <summary>
		/// We watch for a switch weapon input
		/// </summary>
		protected override void HandleInput()
		{
			if (_inputManager.SwitchWeaponButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwitchWeapon ();
			}
		}

		/// <summary>
		/// Fills a list with all available weapons in the inventories
		/// </summary>
		protected virtual void FillAvailableWeaponsLists()
		{
			_availableWeaponsIDs = new List<string> ();
			if ((CharacterHandleWeapon == null) || (WeaponInventory == null))
			{
				return;
			}
			_availableWeapons = MainInventory.InventoryContains (ItemClasses.Weapon);
			foreach (int index in _availableWeapons)
			{
				_availableWeaponsIDs.Add (MainInventory.Content [index].ItemID);
			}
			if (!InventoryItem.IsNull(WeaponInventory.Content[0]))
			{
				_availableWeaponsIDs.Add (WeaponInventory.Content [0].ItemID);
			}

			_availableWeaponsIDs.Sort ();
		}

		/// <summary>
		/// Determines the name of the next weapon
		/// </summary>
		protected virtual void DetermineNextWeaponName ()
		{
			if (InventoryItem.IsNull(WeaponInventory.Content[0]))
			{
				_nextWeaponID = _availableWeaponsIDs [0];
				return;
			}

			if ((_nextWeaponID == _emptySlotWeaponName) || (_nextWeaponID == _initialSlotWeaponName))
			{
				_nextWeaponID = _availableWeaponsIDs[0];
				return;
			}

			for (int i = 0; i < _availableWeaponsIDs.Count; i++)
			{
				if (_availableWeaponsIDs[i] == WeaponInventory.Content[0].ItemID)
				{
					if (i == _availableWeaponsIDs.Count - 1)
					{
						switch (WeaponRotationMode)
						{
							case WeaponRotationModes.AddEmptySlot:
								_nextWeaponID = _emptySlotWeaponName;
								return;
							case WeaponRotationModes.AddInitialWeapon:
								_nextWeaponID = _initialSlotWeaponName;
								return;
						}
						_nextWeaponID = _availableWeaponsIDs [0];
					}
					else
					{
						_nextWeaponID = _availableWeaponsIDs [i+1];
					}
				}
			}
		}

		/// <summary>
		/// Equips a weapon specified in parameters
		/// </summary>
		/// <param name="weaponID"></param>
		protected virtual void EquipWeapon(string weaponID)
		{
			if ((weaponID == _emptySlotWeaponName) && (CharacterHandleWeapon != null))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
				CharacterHandleWeapon.ChangeWeapon(null, _emptySlotWeaponName, false);
				MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, WeaponInventory.name, null, 0, 0, PlayerID);
			}

			if ((weaponID == _initialSlotWeaponName) && (CharacterHandleWeapon != null))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
				CharacterHandleWeapon.ChangeWeapon(CharacterHandleWeapon.InitialWeapon, _initialSlotWeaponName, false);
				MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, WeaponInventory.name, null, 0, 0, PlayerID);
				return;
			}

			for (int i = 0; i < MainInventory.Content.Length ; i++)
			{
				if (InventoryItem.IsNull(MainInventory.Content[i]))
				{
					continue;
				}
				if (MainInventory.Content[i].ItemID == weaponID)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, null, MainInventory.name, MainInventory.Content[i], 0, i, PlayerID);
					break;
				}
			}
		}

		/// <summary>
		/// Switches to the next weapon in line
		/// </summary>
		protected virtual void SwitchWeapon()
		{
			// if there's no character handle weapon component, we can't switch weapon, we do nothing and exit
			if ((CharacterHandleWeapon == null) || (WeaponInventory == null))
			{
				return;
			}

			FillAvailableWeaponsLists ();

			// if we only have 0 or 1 weapon, there's nothing to switch, we do nothing and exit
			if (_availableWeaponsIDs.Count <= 0)
			{
				return;
			}

			DetermineNextWeaponName ();
			EquipWeapon (_nextWeaponID);
			PlayAbilityStartFeedbacks();
		}

		/// <summary>
		/// Watches for InventoryLoaded events
		/// When an inventory gets loaded, if it's our WeaponInventory, we check if there's already a weapon equipped, and if yes, we equip it
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryLoaded)
			{
				if (!AbilityInitialized)
				{
					Initialization();
				}
				
				if (inventoryEvent.TargetInventoryName == WeaponInventoryName)
				{
					this.Setup ();
					if (WeaponInventory != null)
					{
						if (!InventoryItem.IsNull (WeaponInventory.Content [0]))
						{
							if (CharacterHandleWeapon == null)
							{
								CharacterHandleWeapon = _character.FindAbility<CharacterHandleWeapon>();
							}
							CharacterHandleWeapon.Setup ();
							WeaponInventory.Content [0].Equip (PlayerID);
						}
					}
				}
			}
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.Pick)
			{
				bool isSubclass = (inventoryEvent.EventItem.GetType().IsSubclassOf(typeof(InventoryEngineWeapon)));
				bool isClass = (inventoryEvent.EventItem.GetType() == typeof(InventoryEngineWeapon));
				if (  isClass || isSubclass )
				{
					InventoryEngineWeapon inventoryWeapon = (InventoryEngineWeapon)inventoryEvent.EventItem;
					switch (inventoryWeapon.AutoEquipMode)
					{
						case InventoryEngineWeapon.AutoEquipModes.NoAutoEquip:
							// we do nothing
							break; 

						case InventoryEngineWeapon.AutoEquipModes.AutoEquip:
							_nextFrameWeapon = true;
							_nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
							break;

						case InventoryEngineWeapon.AutoEquipModes.AutoEquipIfEmptyHanded:
							if (CharacterHandleWeapon.CurrentWeapon == null)
							{
								_nextFrameWeapon = true;
								_nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
							}
							break;
					}
				}
			}
		}

		/// <summary>
		/// When we detect a character switch, we equip the current weapon if AutoEquipWeaponOnCharacterSwitch is true
		/// </summary>
		/// <param name="corgiEngineEvent"></param>
		public virtual void OnMMEvent(CorgiEngineEvent corgiEngineEvent)
		{
			if (corgiEngineEvent.EventType == CorgiEngineEventTypes.CharacterSwitch)
			{
				if (!AutoEquipWeaponOnCharacterSwitch)
				{
					return;
				}
				this.Setup();
				if (WeaponInventory != null)
				{
					if (!InventoryItem.IsNull(WeaponInventory.Content[0]))
					{
						if (CharacterHandleWeapon == null)
						{
							CharacterHandleWeapon = GetComponent<CharacterHandleWeapon>();
						}
						CharacterHandleWeapon.Setup();
						WeaponInventory.Content[0].Equip(PlayerID);
					}
				}
			}
		}

		/// <summary>
		/// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
		/// </summary>
		protected override void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
			this.MMEventStartListening<CorgiEngineEvent>();
		}

		/// <summary>
		/// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable ();
			this.MMEventStopListening<MMInventoryEvent>();
			this.MMEventStopListening<CorgiEngineEvent>();
		}
	}
}