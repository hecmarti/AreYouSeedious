using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A class that looks for objects with a ProximityManaged class on them, and enables/disables them based on their settings.
	/// This class is meant as an example of how you can deal with large scenes with a lot of objects, disabling the ones that are far 
	/// away from the action to save on performance.
	/// Note that there are many ways to do it, this one is simple and generic, there may be better choices for your specific use case.
	/// </summary>
	public class ProximityManager : CorgiMonoBehaviour, MMEventListener<CorgiEngineEvent>
	{
		[Header("Target")]

		/// whether or not to automatically grab the player from the LevelManager once the scene loads
		[Tooltip("whether or not to automatically grab the player from the LevelManager once the scene loads")]
		public bool AutomaticallySetPlayerAsTarget = true;
		/// the target to detect proximity with
		[Tooltip("the target to detect proximity with")]
		public Transform ProximityTarget;

		[Header("EnableDisable")]

		/// whether or not to automatically grab all ProximityManaged objects in the scene
		[Tooltip("whether or not to automatically grab all ProximityManaged objects in the scene")]
		public bool AutomaticallyGrabControlledObjects = true;
		/// the list of objects to check proximity with
		[Tooltip("the list of objects to check proximity with")]
		public List<ProximityManaged> ControlledObjects;

		[Header("Tick")]

		/// the frequency, in seconds, at which to evaluate distances and enable/disable stuff
		[Tooltip("the frequency, in seconds, at which to evaluate distances and enable/disable stuff")]
		public float EvaluationFrequency = 0.5f;

		protected float _lastEvaluationAt = 0f;

		/// <summary>
		/// On start we grab our controlled objects
		/// </summary>
		protected virtual void Start()
		{
			GrabControlledObjects();
		}

		/// <summary>
		/// Grabs all proximity managed objects in the scene
		/// </summary>
		protected virtual void GrabControlledObjects()
		{
			if (AutomaticallyGrabControlledObjects)
			{
				ProximityManaged[] items = FindObjectsOfType<ProximityManaged>();
				foreach (ProximityManaged managed in items)
				{
					ControlledObjects.Add(managed);
				}
			}
		}

		/// <summary>
		/// A public method used to add new controlled objects at runtime
		/// </summary>
		/// <param name="newObject"></param>
		public virtual void AddControlledObject(ProximityManaged newObject)
		{
			ControlledObjects.Add(newObject);
		}

		/// <summary>
		/// Grabs the player from the level manager
		/// </summary>
		protected virtual void SetPlayerAsTarget()
		{
			if (AutomaticallySetPlayerAsTarget)
			{
				ProximityTarget = LevelManager.Instance.Players[0].transform;
			}
		}

		/// <summary>
		/// On Update we check our distances
		/// </summary>
		protected virtual void Update()
		{
			EvaluateDistance();
		}

		/// <summary>
		/// Checks distances if needed
		/// </summary>
		protected virtual void EvaluateDistance()
		{
			if (Time.time - _lastEvaluationAt > EvaluationFrequency)
			{
				_lastEvaluationAt = Time.time;
			}
			else
			{
				return;
			}
			foreach (ProximityManaged proxy in ControlledObjects)
			{
				float distance = Vector3.Distance(proxy.transform.position, ProximityTarget.position);
				if (proxy.gameObject.activeInHierarchy && (distance > proxy.DisableDistance))
				{
					proxy.gameObject.SetActive(false);
					proxy.DisabledByManager = true;
				}
				if (!proxy.gameObject.activeInHierarchy && proxy.DisabledByManager && (distance < proxy.EnableDistance))
				{
					proxy.gameObject.SetActive(true);
					proxy.DisabledByManager = false;
				}
			}
		}

		/// <summary>
		/// When we get a level start event, we assign our player as a target
		/// </summary>
		/// <param name="engineEvent"></param>
		public virtual void OnMMEvent(CorgiEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case CorgiEngineEventTypes.LevelStart:
					SetPlayerAsTarget();
					break;
                
				case CorgiEngineEventTypes.CharacterSwap:
					SetPlayerAsTarget();
					break;
                
				case CorgiEngineEventTypes.CharacterSwitch:
					SetPlayerAsTarget();
					break;
			}
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<CorgiEngineEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<CorgiEngineEvent>();
		}
	}
}