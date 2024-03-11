using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Cinemachine;
using UnityEngine.Events;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This class lets you define the boundaries of rooms in your level.
	/// Rooms are useful if you want to cut your level into portions (think Super Metroid or Hollow Knight for example).
	/// These rooms will require their own virtual camera, and a confiner to define their size. 
	/// Note that the confiner is different from the collider that defines the room.
	/// You can see an example of rooms in action in the RetroVania demo scene.
	/// </summary>
	[RequireComponent(typeof(Collider2D))]
	public class Room : CorgiMonoBehaviour, MMEventListener<CorgiEngineEvent>
	{
		/// the collider for this room
		public Collider2D RoomCollider { get { return _roomCollider2D; } }

		[Header("Camera")]

		/// the virtual camera associated to this room
		[Tooltip("the virtual camera associated to this room")]
		public CinemachineVirtualCamera VirtualCamera;
		/// the confiner for this room, that will constrain the virtual camera, usually placed on a child object of the Room
		[Tooltip("the confiner for this room, that will constrain the virtual camera, usually placed on a child object of the Room")]
		public Collider2D Confiner;
		/// the confiner component of the virtual camera
		[Tooltip("the confiner component of the virtual camera")]
		public CinemachineConfiner CinemachineCameraConfiner;
		/// whether or not the confiner should be auto resized on start to match the camera's size and ratio
		[Tooltip("whether or not the confiner should be auto resized on start to match the camera's size and ratio")]
		public bool ResizeConfinerAutomatically = true;
		/// whether or not this Room should look at the level's start position and declare itself the current room on start or not
		[Tooltip("whether or not this Room should look at the level's start position and declare itself the current room on start or not")]
		public bool AutoDetectFirstRoomOnStart = true;

		[Header("State")]

		/// whether this room is the current room or not
		[Tooltip("whether this room is the current room or not")]
		public bool CurrentRoom = false;
		/// whether this room has already been visited or not
		[Tooltip("whether this room has already been visited or not")]
		public bool RoomVisited = false;

		[Header("Actions")]

		/// the event to trigger when the player enters the room for the first time
		[Tooltip("the event to trigger when the player enters the room for the first time")]
		public UnityEvent OnPlayerEntersRoomForTheFirstTime;
		/// the event to trigger everytime the player enters the room
		[Tooltip("the event to trigger everytime the player enters the room")]
		public UnityEvent OnPlayerEntersRoom;
		/// the event to trigger everytime the player exits the room
		[Tooltip("the event to trigger everytime the player exits the room")]
		public UnityEvent OnPlayerExitsRoom;

		[Header("Activation")]

		/// a list of gameobjects to enable when entering the room, and disable when exiting it
		[Tooltip("a list of gameobjects to enable when entering the room, and disable when exiting it")]
		public List<GameObject> ActivationList;

		protected Collider2D _roomCollider2D;
		protected Camera _mainCamera;
		protected Vector2 _cameraSize;
		protected bool _initialized = false;
        
		/// <summary>
		/// On Start we initialize our room
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs our Room collider, our main camera, and starts the confiner resize
		/// </summary>
		protected virtual void Initialization()
		{
			if (_initialized)
			{
				return;
			}
			_roomCollider2D = this.gameObject.GetComponent<Collider2D>();
			_mainCamera = Camera.main;          
			StartCoroutine(ResizeConfiner());
			_initialized = true;

			if (VirtualCamera != null)
			{
				VirtualCamera.enabled = false;
			}
            
			foreach (GameObject go in ActivationList)
			{
				go.SetActive(false);
			}
		}

		/// <summary>
		/// Resizes the confiner 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ResizeConfiner()
		{
			if ((VirtualCamera == null) || (Confiner == null) || !ResizeConfinerAutomatically)
			{
				yield break;
			}

			// we wait two more frame for Unity's pixel perfect camera component to be ready because apparently sending events is not a thing.
			yield return null;
			yield return null;

			(Confiner as BoxCollider2D).offset = _roomCollider2D.offset;
			(Confiner as BoxCollider2D).size = (_roomCollider2D as BoxCollider2D).size;

			_cameraSize.y = 2 * _mainCamera.orthographicSize;
			_cameraSize.x = _cameraSize.y * _mainCamera.aspect;

			Vector2 newSize = (Confiner as BoxCollider2D).size;

			if ((Confiner as BoxCollider2D).size.x < _cameraSize.x)
			{
				newSize.x = _cameraSize.x;
			}
			if ((Confiner as BoxCollider2D).size.y < _cameraSize.y)
			{
				newSize.y = _cameraSize.y;
			}

			(Confiner as BoxCollider2D).size = newSize;
			CinemachineCameraConfiner.InvalidatePathCache();

			HandleLevelStartDetection();
		}   
        
		/// <summary>
		/// Looks for the level start position and if it's inside the room, makes this room the current one
		/// </summary>
		protected virtual void HandleLevelStartDetection()
		{
			if (!_initialized)
			{
				Initialization();
			}

			if (AutoDetectFirstRoomOnStart)
			{
				if (LevelManager.HasInstance)
				{
					if (_roomCollider2D.bounds.Contains(LevelManager.Instance.Players[0].transform.position.MMSetZ(transform.position.z)))
					{
						MMCameraEvent.Trigger(MMCameraEventTypes.ResetPriorities);
						MMCinemachineBrainEvent.Trigger(MMCinemachineBrainEventTypes.ChangeBlendDuration, 0f);
						if (VirtualCamera != null)
						{
							VirtualCamera.Priority = 10;
						}

						MMSpriteMaskEvent.Trigger(MMSpriteMaskEvent.MMSpriteMaskEventTypes.MoveToNewPosition,
							(Vector2)_roomCollider2D.bounds.center,
							_roomCollider2D.bounds.size,
							0f, MMTween.MMTweenCurve.LinearTween);

						PlayerEntersRoom();
					}
				}
			}
		}

		/// <summary>
		/// Call this to let the room know a player entered
		/// </summary>
		public virtual void PlayerEntersRoom()
		{
			CurrentRoom = true;
			if (VirtualCamera != null)
			{
				VirtualCamera.enabled = true;
			}
			if (RoomVisited)
			{
				OnPlayerEntersRoom?.Invoke();
			}
			else
			{
				RoomVisited = true;
				OnPlayerEntersRoomForTheFirstTime?.Invoke();
			}  
			foreach(GameObject go in ActivationList)
			{
				go.SetActive(true);
			}
		}

		/// <summary>
		/// Call this to let this room know a player exited
		/// </summary>
		public virtual void PlayerExitsRoom()
		{
			if (VirtualCamera != null)
			{
				VirtualCamera.enabled = false;
			}
			CurrentRoom = false;
			OnPlayerExitsRoom?.Invoke();
			foreach (GameObject go in ActivationList)
			{
				go.SetActive(false);
			}
		}

		/// <summary>
		/// When we get a respawn event, we ask for a camera reposition
		/// </summary>
		/// <param name="corgiEngineEvent"></param>
		public virtual void OnMMEvent(CorgiEngineEvent corgiEngineEvent)
		{
			if ((corgiEngineEvent.EventType == CorgiEngineEventTypes.Respawn)
			    || (corgiEngineEvent.EventType == CorgiEngineEventTypes.LevelStart))
			{
				PlayerExitsRoom();
				HandleLevelStartDetection();
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
		/// On enable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<CorgiEngineEvent>();
		}
	}
}