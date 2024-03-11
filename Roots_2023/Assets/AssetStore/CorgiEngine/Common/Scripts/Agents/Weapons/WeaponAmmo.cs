using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.CorgiEngine
{	
	[RequireComponent(typeof(Weapon))]
	[AddComponentMenu("Corgi Engine/Weapons/Weapon Ammo")]
	public class WeaponAmmo : CorgiMonoBehaviour, MMEventListener<MMStateChangeEvent<MoreMountains.CorgiEngine.Weapon.WeaponStates>>, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
	{
		[Header("Ammo")]

		/// the AmmoID that matches the one on the ammo this weapon should use
		[Tooltip("the AmmoID that matches the one on the ammo this weapon should use")]
		public string AmmoID;
		/// the name of the inventory where the system should look for ammo
		[Tooltip("the name of the inventory where the system should look for ammo")]
		public string AmmoInventoryName = "MainInventory";
		/// the theoretical maximum of ammo
		[Tooltip("the theoretical maximum of ammo")]
		public int MaxAmmo = 100;
		/// if this is true, everytime you equip this weapon, it'll auto fill with ammo
		[Tooltip("if this is true, everytime you equip this weapon, it'll auto fill with ammo")]
		public bool ShouldLoadOnStart = true;
		/// if this is true, everytime you equip this weapon, it'll auto fill with ammo
		[Tooltip("if this is true, everytime you equip this weapon, it'll auto fill with ammo")]
		public bool ShouldEmptyOnSave = true;

		/// the current amount of ammo available in the inventory
		[MMReadOnly]
		[Tooltip("the current amount of ammo available in the inventory")]
		public int CurrentAmmoAvailable;

		public Inventory AmmoInventory { get; set; }

		protected Weapon _weapon;
		protected InventoryItem _ammoItem;
		protected bool _emptied = false;

		protected virtual void Start()
		{
			// we grab the ammo inventory if it exists
			GameObject ammoInventoryTmp = GameObject.Find (AmmoInventoryName);
			if (ammoInventoryTmp != null) { AmmoInventory = ammoInventoryTmp.GetComponent<Inventory> (); }

			_weapon = this.gameObject.GetComponent<Weapon> ();
			if (ShouldLoadOnStart)
			{
				LoadOnStart ();	
			}
		}

		protected virtual void LoadOnStart()
		{
			FillWeaponWithAmmo ();
		}

		protected virtual void RefreshCurrentAmmoAvailable()
		{
			CurrentAmmoAvailable = AmmoInventory.GetQuantity (AmmoID);
		}

		public virtual bool EnoughAmmoToFire()
		{
			if (AmmoInventory == null)
			{
				Debug.LogWarning (this.name + " couldn't find the associated inventory. Is there one present in the scene? It should be named '" + AmmoInventoryName + "'.");
				return false;
			}

			RefreshCurrentAmmoAvailable ();

			if (_weapon.MagazineBased)
			{
				if (_weapon.CurrentAmmoLoaded >= _weapon.AmmoConsumedPerShot)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if (CurrentAmmoAvailable >= _weapon.AmmoConsumedPerShot)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		protected virtual void ConsumeAmmo()
		{
			if (_weapon.MagazineBased)
			{
				_weapon.CurrentAmmoLoaded = _weapon.CurrentAmmoLoaded - _weapon.AmmoConsumedPerShot;
			}
			else
			{
				for (int i = 0; i < _weapon.AmmoConsumedPerShot; i++)
				{
					AmmoInventory.UseItem (AmmoID);	
					CurrentAmmoAvailable--;
				}
			}	

			if (CurrentAmmoAvailable  < _weapon.AmmoConsumedPerShot)
			{
				if (_weapon.AutoDestroyWhenEmpty)
				{
					StartCoroutine (_weapon.WeaponDestruction ());
				}
			}
		}

		public virtual void FillWeaponWithAmmo()
		{
			if (AmmoInventory != null)
			{
				RefreshCurrentAmmoAvailable ();
			}
			
			if (_ammoItem == null)
			{
				List<int> list = AmmoInventory.InventoryContains(AmmoID);
				if (list.Count > 0)
				{
					_ammoItem = AmmoInventory.Content[list[list.Count - 1]];
				}
			}

			if (_weapon.MagazineBased)
			{
				int counter = 0;
				int stock = CurrentAmmoAvailable - _weapon.CurrentAmmoLoaded;
				for (int i = _weapon.CurrentAmmoLoaded; i < _weapon.MagazineSize; i++)
				{
					if (stock > 0)
					{
						stock--;
						counter++;		
						AmmoInventory.UseItem (AmmoID);	
					}									
				}
				_weapon.CurrentAmmoLoaded += counter;
			}
			
			RefreshCurrentAmmoAvailable();
		}
		
		/// <summary>
		/// Empties the weapon's magazine and puts the ammo back in the inventory
		/// </summary>
		public virtual void EmptyMagazine(bool shouldSave)
		{
			if (AmmoInventory != null)
			{
				RefreshCurrentAmmoAvailable ();
			}

			if ((_ammoItem == null) || (AmmoInventory == null))
			{
				return;
			}

			if (_emptied)
			{
				return;
			}

			if (_weapon.MagazineBased)
			{
				int stock = _weapon.CurrentAmmoLoaded;
				int counter = 0;

				for (int i = 0; i < stock; i++)
				{
					AmmoInventory.AddItem(_ammoItem, 1);
					counter++;
				}
				_weapon.CurrentAmmoLoaded -= counter;

				if (AmmoInventory.Persistent && shouldSave)
				{
					AmmoInventory.SaveInventory();
				}
			}
			RefreshCurrentAmmoAvailable();
			_emptied = true;
		}

		public virtual void OnMMEvent(MMStateChangeEvent<MoreMountains.CorgiEngine.Weapon.WeaponStates> weaponEvent)
		{
			// if this event doesn't concern us, we do nothing and exit
			if (weaponEvent.Target != this.gameObject)
			{
				return;
			}

			switch (weaponEvent.NewState)
			{
				case MoreMountains.CorgiEngine.Weapon.WeaponStates.WeaponUse:
					ConsumeAmmo ();
					break;

				case MoreMountains.CorgiEngine.Weapon.WeaponStates.WeaponReloadStop:
					FillWeaponWithAmmo();
					break;
			}
		}

		/// <summary>
		/// On pick we refresh our ammo if needed
		/// </summary>
		/// <param name="inventoryEvent"></param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Pick:
					if (inventoryEvent.EventItem.ItemClass == ItemClasses.Ammo)
					{
						StartCoroutine(DelayedRefreshCurrentAmmoAvailable());
					}
					break;				
			}
		}

		protected IEnumerator DelayedRefreshCurrentAmmoAvailable()
		{
			yield return null;
			RefreshCurrentAmmoAvailable ();
		}
		
		/// <summary>
		/// Grabs inventory events and refreshes ammo if needed
		/// </summary>
		/// <param name="inventoryEvent"></param>
		public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			switch (gameEvent.EventName)
			{
				case "Save":
					if (ShouldEmptyOnSave)
					{
						EmptyMagazine(true);    
					}
					break;				
			}
		}

		/// <summary>
		// on destroy we put our ammo back in the inventory
		/// </summary>
		protected void OnDestroy()
		{
			EmptyMagazine(false);
		}

		/// <summary>
		/// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMStateChangeEvent<MoreMountains.CorgiEngine.Weapon.WeaponStates>>();
			this.MMEventStartListening<MMInventoryEvent> ();
			this.MMEventStartListening<MMGameEvent>();
		}

		/// <summary>
		/// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMStateChangeEvent<MoreMountains.CorgiEngine.Weapon.WeaponStates>>();
			this.MMEventStopListening<MMInventoryEvent> ();
			this.MMEventStopListening<MMGameEvent>();
		}
	}
}