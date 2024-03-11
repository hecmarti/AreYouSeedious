using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Checkpoint class. Will make the player respawn at this point if it dies.
	/// </summary>
	[RequireComponent(typeof(BoxCollider2D))]
	[AddComponentMenu("Corgi Engine/Spawn/Checkpoint")]
	public class CheckPoint : CorgiMonoBehaviour 
	{
		[Header("Spawn")]
		[MMInformation("Add this script to a (preferrably empty) GameObject and it'll be added to the level's checkpoint list, allowing you to respawn from there. If you bind it to the LevelManager's starting point, that's where your character will spawn at the start of the level. And here you can decide whether the character should spawn facing left or right.",MMInformationAttribute.InformationType.Info,false)]

		/// the direction the character should face when spawning at this checkpoint
		[Tooltip("the direction the character should face when spawning at this checkpoint")]
		public Character.FacingDirections FacingDirection = Character.FacingDirections.Right ;
		/// whether or not this checkpoint should override any order and assign itself on entry
		[Tooltip("whether or not this checkpoint should override any order and assign itself on entry")]
		public bool ForceAssignation = false;
		/// the order of the checkpoint
		[Tooltip("the order of the checkpoint")]
		public int CheckPointOrder;
		/// whether or not this checkpoint can be reached more than once
		[Tooltip("whether or not this checkpoint can be reached more than once")]
		public bool CanBeReachedMoreThanOnce = true;
		/// an event to trigger when this checkpoint is reached
		[Tooltip("an event to trigger when this checkpoint is reached")]
		public UnityEvent OnCheckpointReached;

		protected bool _reached = false;
		protected List<Respawnable> _listeners;

		[SerializeField]
        private bool endPoint = false;

        /// <summary>
        /// Initializes the list of listeners
        /// </summary>
        protected virtual void Awake () 
		{
			_listeners = new List<Respawnable>();
		}
				
		/// <summary>
		/// Spawns the player at the checkpoint.
		/// </summary>
		/// <param name="player">Player.</param>
		public virtual void SpawnPlayer(Character player)
		{
			player.RespawnAt(transform, FacingDirection);
			
			foreach(Respawnable listener in _listeners)
			{
				listener.OnPlayerRespawn(this,player);
			}
		}
		
		public virtual void AssignObjectToCheckPoint (Respawnable listener) 
		{
			_listeners.Add(listener);
		}

		/// <summary>
		/// Describes what happens when something enters the checkpoint
		/// </summary>
		/// <param name="collider">The Collider2D colliding with the checkpoint.</param>
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			Character character = collider.GetComponent<Character>();

			if (character == null) { return; }
			if (character.CharacterType != Character.CharacterTypes.Player) { return; }
			if (_reached && !CanBeReachedMoreThanOnce) { return; }
			if (!LevelManager.HasInstance) { return; }
			OnCheckpointReached?.Invoke();

			if (!endPoint)
			{
                LevelManager.Instance.SetCurrentCheckpoint(this);
            }

			_reached = true;
		}

		/// <summary>
		/// On DrawGizmos, we draw lines to show the path the object will follow
		/// </summary>
		protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR

			if (!LevelManager.HasInstance)
			{
				return;
			}

			if (LevelManager.Instance.Checkpoints == null)
			{
				return;
			}

			if (LevelManager.Instance.Checkpoints.Count == 0)
			{
				return;
			}

			for (int i=0; i < LevelManager.Instance.Checkpoints.Count; i++)
			{
				// we draw a line towards the next point in the path
				if ((i+1) < LevelManager.Instance.Checkpoints.Count)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(LevelManager.Instance.Checkpoints[i].transform.position,LevelManager.Instance.Checkpoints[i+1].transform.position);
				}
			}
			#endif
		}
	}
}