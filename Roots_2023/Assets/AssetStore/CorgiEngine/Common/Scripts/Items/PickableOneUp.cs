using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A pickable one up, that gives you one extra life if picked up
	/// </summary>
	[AddComponentMenu("Corgi Engine/Items/Pickable One Up")]
	public class PickableOneUp : PickableItem
	{
		[MMInformation("Add this component to an object with a Collider2D set as trigger, and it'll become pickable by Player Characters. When picked, it'll increase the amount of lives as specified. You can decide here to have only new lives added, within the limit of current lives containers, expand this limit, fill it accordingly, or fill all containers.", MMInformationAttribute.InformationType.Info, false)]
		[Header("Normal one ups")]

		/// the amount of lives that should be added when picking this item
		[Tooltip("the amount of lives that should be added when picking this item")]
		public int NumberOfAddedLives;

		[Header("Containers")]

		/// the number of empty containers to add when picking this item
		[Tooltip("the number of empty containers to add when picking this item")]
		public int NumberOfAddedEmptyContainers;
		/// whether to fill the additional containers or not
		[Tooltip("whether to fill the additional containers or not")]
		public bool FillAddedContainers = false;
		/// whether to fill all containers or not
		[Tooltip("whether to fill all containers or not")]
		public bool FillAllContainers = false;

		/// <summary>
		/// What happens when the object gets picked
		/// </summary>
		protected override void Pick(GameObject picker)
		{
			GameManager.Instance.GainLives(NumberOfAddedLives);

			GameManager.Instance.AddLives(NumberOfAddedEmptyContainers, FillAddedContainers);

			if (FillAllContainers)
			{
				GameManager.Instance.GainLives(GameManager.Instance.MaximumLives);
			}

		}
	}
}