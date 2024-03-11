using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{
	public class InventoryPickableItem : ItemPicker, Respawnable 
	{
		[Header("Inventory Pickable Item")]

		/// the MMFeedback to play when the object gets picked
		[Tooltip("the MMFeedback to play when the object gets picked")]
		public MMFeedbacks PickFeedbacks;
		/// whether or not this should reset RemainingQuantity when the player respawns
		[Tooltip("whether or not this should reset RemainingQuantity when the player respawns")]
		public bool ResetQuantityOnPlayerRespawn = true;

		protected override void PickSuccess()
		{
			base.PickSuccess ();
			Effects ();
		}

		/// <summary>
		/// Triggers the various pick effects
		/// </summary>
		protected virtual void Effects()
		{
			if (!Application.isPlaying)
			{
				return;
			}				
			else
			{
				PickFeedbacks?.PlayFeedbacks();
			}
		}

		/// <summary>
		/// Triggered when the player respawns, resets the block if needed
		/// </summary>
		/// <param name="checkpoint">Checkpoint.</param>
		/// <param name="player">Player.</param>
		public virtual void OnPlayerRespawn(CheckPoint checkpoint, Character player)
		{
			if (ResetQuantityOnPlayerRespawn)
			{
				RemainingQuantity = Quantity;
			}
		}
	}
}