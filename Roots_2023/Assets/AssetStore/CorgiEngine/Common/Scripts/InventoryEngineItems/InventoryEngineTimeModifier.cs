using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{	
	[CreateAssetMenu(fileName = "InventoryEngineTimeModifier", menuName = "MoreMountains/CorgiEngine/InventoryEngineTimeModifier", order = 2)]
	[Serializable]
	/// <summary>
	/// Pickable health item
	/// </summary>
	public class InventoryEngineTimeModifier : InventoryItem 
	{
		[Header("Time Modifier")]
		[MMInformation("Here you need to specify the new time speed and the duration for which the timescale will be changed.",MMInformationAttribute.InformationType.Info,false)]

		/// the time speed to apply while the effect lasts
		[Tooltip("the time speed to apply while the effect lasts")]
		public float TimeSpeed = 0.5f;
		/// how long the duration will last , in seconds
		[Tooltip("how long the duration will last , in seconds")]
		public float Duration = 1.0f;

		protected WaitForSeconds _changeTimeWFS;

		public override bool Use(string playerID)
		{
			base.Use(playerID);
			MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloseRequest, null, this.name, null, 0, 0, playerID);
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeSpeed, Duration, true, 5f, false);
			return true;
		}	
	}
}