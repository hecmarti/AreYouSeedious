using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Adding this script to a trigger, volume box collider2D will make sure that any character that enters it with a CharacterGravity ability on itself will
	/// have its "reverse input when upside down" property set to false. This was created for the purposes of the FeaturesGravity demo but feel free to use it in your game if needed.
	/// </summary>
	public class UncheckReverseGravityInput : CorgiMonoBehaviour 
	{
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			CharacterGravity characterGravity = collider.gameObject.MMGetComponentNoAlloc<CharacterGravity> ();
			if (characterGravity == null)
			{
				return;
			}
			else
			{
				characterGravity.ReverseHorizontalInputWhenUpsideDown = false;
			}
		}			
	}
}