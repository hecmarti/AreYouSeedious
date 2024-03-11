using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this class to a collectible to have the player change weapon when collecting it
	/// </summary>
	[AddComponentMenu("Corgi Engine/Items/Pickable Weapon")]
	public class PickableWeapon : PickableItem
	{
		/// the new weapon the player gets when collecting this object
		[Tooltip("the new weapon the player gets when collecting this object")]
		public Weapon WeaponToGive;
		/// the ID of the CharacterHandleWeapon ability you want this weapon to go to (1 by default)
		[Tooltip("the ID of the CharacterHandleWeapon ability you want this weapon to go to (1 by default)")]
		public int HandleWeaponID = 1;

		protected CharacterHandleWeapon _characterHandleWeapon = null;

		/// <summary>
		/// What happens when the weapon gets picked
		/// </summary>
		protected override void Pick(GameObject picker)
		{
			Character character = _collidingObject.gameObject.MMGetComponentNoAlloc<Character>();

			if (character == null)
			{
				return;
			}

			if (_characterHandleWeapon != null)
			{
				_characterHandleWeapon.ChangeWeapon(WeaponToGive, null);
			}
		}

		/// <summary>
		/// Checks if the object is pickable.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		protected override bool CheckIfPickable()
		{
			_character = _pickingCollider.GetComponent<Character>();

			// if what's colliding with the coin ain't a characterBehavior, we do nothing and exit
			if (_character == null)
			{
				return false;
			}
			if (_character.CharacterType != Character.CharacterTypes.Player)
			{
				return false;
			}

			// we equip the weapon to the chosen CharacterHandleWeapon
			CharacterHandleWeapon[] handleWeapons = _character.GetComponentsInChildren<CharacterHandleWeapon>();
			foreach (CharacterHandleWeapon handleWeapon in handleWeapons)
			{
				if ((handleWeapon.HandleWeaponID == HandleWeaponID) && (handleWeapon.CanPickupWeapons))
				{
					_characterHandleWeapon = handleWeapon;
				}
			}

			if (_characterHandleWeapon == null)
			{
				return false;
			}
			
			return true;
		}
	}
}