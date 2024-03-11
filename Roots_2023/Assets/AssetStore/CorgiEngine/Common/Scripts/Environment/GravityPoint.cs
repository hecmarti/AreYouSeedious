using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this component to an object and it'll impact graviy able characters within its DistanceOfEffect
	/// </summary>
	[AddComponentMenu("Corgi Engine/Environment/Gravity Point")]
	public class GravityPoint : CorgiMonoBehaviour 
	{
		/// the distance within which objects are impact by this gravity point
		[Tooltip("the distance within which objects are impact by this gravity point")]
		public float DistanceOfEffect;

		protected virtual void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere (this.transform.position, DistanceOfEffect);
		}
	}
}