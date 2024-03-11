using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This component lets you permit the jetpack ability when picked
	/// </summary>
	[AddComponentMenu("Corgi Engine/Items/Pickable Jetpack")]
	public class PickableJetpack : PickableItem
	{
		/// <summary>
		/// Checks if the object is pickable (we want our picker to be a Player character).
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
			return true;
		}

		/// <summary>
		/// on pick, we activate our jetpack ability
		/// of course that's assuming it was false before, and you could have this behave in a more complex way if you want
		/// </summary>
		protected override void Pick(GameObject picker)
		{
			_character.gameObject.MMGetComponentNoAlloc<CharacterJetpack>()?.PermitAbility(true);
		}
	}
}