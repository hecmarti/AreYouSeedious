using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using  MoreMountains.Tools;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This component, added on an empty object in your level will handle the generation of a unique and randomized tilemap
	/// </summary>
	public class TilemapLevelGenerator : MMTilemapGenerator
	{
		[Header("Corgi Engine Settings")]
		/// Whether or not this level should be generated automatically on start
		[Tooltip("Whether or not this level should be generated automatically on start")]
		public bool GenerateOnAwake = false;

		[Header("Bindings")] 
		/// the Grid on which to work
		[Tooltip("the Grid on which to work")]
		public Grid TargetGrid;
		/// the tilemap containing the walls
		[Tooltip("the tilemap containing the walls")]
		public Tilemap PlatformsTilemap;
		/// the layermask to avoid
		[Tooltip("the layermask to avoid")] 
		public LayerMask ObstaclesLayermask;
		/// the level manager
		[Tooltip("the level manager")]
		public LevelManager TargetLevelManager;

		[Header("Spawn")] 
		/// the object at which the player will spawn
		[Tooltip("the object at which the player will spawn")]
		public Transform InitialSpawn;
		/// an offset in world space to apply to the spawn point
		[Tooltip("an offset in world space to apply to the spawn point")]
		public Vector3 SpawnOffset;
		/// the exit of the level
		[Tooltip("the exit of the level")]
		public Transform Exit;
		/// the minimum distance that should separate spawn and exit.
		[Tooltip("the minimum distance that should separate spawn and exit.")]
		public float MinDistanceFromSpawnToExit = 2f;
        
		[Header("Floating Objects")]
		/// a list of objects to reposition in empty space
		[Tooltip("a list of objects to reposition in empty space")]
		public List<GameObject> FloatingObjectsToReposition;
        
		protected const int _maxIterationsCount = 100;
        
		/// <summary>
		/// On awake we generate our level if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (GenerateOnAwake)
			{
				Generate();
			}
		}

		/// <summary>
		/// Generates a new level
		/// </summary>
		public override void Generate()
		{
			base.Generate();
			PlaceEntryAndExit();
			PlaceFloatingObjects();
			ResizeLevelManager();
		}

		/// <summary>
		/// Resizes the level manager's bounds to match the new level
		/// </summary>
		protected virtual void ResizeLevelManager()
		{
			Bounds bounds = PlatformsTilemap.localBounds;
			Vector3 extents = bounds.extents;
			extents.z = 10;
			bounds.extents = extents;
			TargetLevelManager.LevelBounds = bounds;
		}
        
		/// <summary>
		/// Moves the spawn and exit to empty places
		/// </summary>
		protected virtual void PlaceEntryAndExit()
		{
			UnityEngine.Random.InitState(GlobalSeed);
			int width = UnityEngine.Random.Range(GridWidth.x, GridWidth.y);
			int height = UnityEngine.Random.Range(GridHeight.x, GridHeight.y);
            

			Vector3 spawnPosition = MMTilemap.GetRandomPositionOnGround(PlatformsTilemap, TargetGrid, width, height, height - 2, 1, width/2 - 1, true, 1000);
			InitialSpawn.transform.position = spawnPosition + SpawnOffset;

			Vector3 exitPosition = spawnPosition;
			int iterationsCount = 0;
            
			while ((Vector3.Distance(exitPosition, spawnPosition) < MinDistanceFromSpawnToExit) && (iterationsCount < _maxIterationsCount))
			{
				exitPosition = MMTilemap.GetRandomPositionOnGround(PlatformsTilemap, TargetGrid, width, height, height - 2, width/2 + 1, width - 1, true, 1000);
				Exit.transform.position = exitPosition;
				iterationsCount++;
			}
		}

		/// <summary>
		/// Places a selection of objects at random positions, while avoiding platforms
		/// </summary>
		protected virtual void PlaceFloatingObjects()
		{
			UnityEngine.Random.InitState(GlobalSeed);
			int width = UnityEngine.Random.Range(GridWidth.x, GridWidth.y);
			int height = UnityEngine.Random.Range(GridHeight.x, GridHeight.y);
            
			foreach (GameObject prop in FloatingObjectsToReposition)
			{
				Vector3 spawnPosition =
					MMTilemap.GetRandomPosition(PlatformsTilemap, TargetGrid, width, height, false, 1000);
				prop.transform.position = spawnPosition;
			}
		}


	}    
}