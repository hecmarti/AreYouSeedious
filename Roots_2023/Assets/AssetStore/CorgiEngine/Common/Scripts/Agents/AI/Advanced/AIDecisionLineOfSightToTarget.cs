using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Use this decision to make sure there is an unobstructed line of sight between this AI and its current target
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Decisions/AI Decision Line of Sight to Target")]
	// [RequireComponent(typeof(Collider2D))]
	public class AIDecisionLineOfSightToTarget : AIDecision
	{
		/// the layermask containing the layers that should be considered as obstacles blocking sight
		[Tooltip("the layermask containing the layers that should be considered as obstacles blocking sight")]
		public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
		/// the offset to apply (from the collider's center)
		[Tooltip("the offset to apply (from the collider's center)")]
		public Vector3 LineOfSightOffset = new Vector3(0, 0, 0);

		protected Vector2 _directionToTarget;
		protected Collider2D _collider;
		protected Vector3 _raycastOrigin;

		/// <summary>
		/// On init we grab our collider component
		/// </summary>
		public override void Initialization()
		{
			_collider = this.gameObject.GetComponentInParent<Collider2D>();
		}

		/// <summary>
		/// On Decide we check whether we've got a line of sight or not
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return CheckLineOfSight();
		}

		/// <summary>
		/// Casts a ray towards the target to see if there's an obstacle in between or not
		/// </summary>
		/// <returns></returns>
		protected virtual bool CheckLineOfSight()
		{
			if (_brain.Target == null)
			{
				return false;
			}

			_raycastOrigin = _collider.bounds.center + LineOfSightOffset / 2;
			_directionToTarget = _brain.Target.transform.position - _raycastOrigin;
                        
			RaycastHit2D hit = MMDebug.RayCast(_raycastOrigin, _directionToTarget.normalized, _directionToTarget.magnitude, ObstacleLayerMask, Color.yellow, true);

			if (hit.collider == null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}