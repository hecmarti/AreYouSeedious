using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// A simple static class that keeps track of layer names, holds ready to use layermasks for most common layers and layermasks combinations
	/// Of course if you happen to change the layer order or numbers, you'll want to udpate this class.
	/// </summary>
	public static class LayerManager
	{
		private static int PlatformsLayer = 8;
		private static int OneWayPlatformsLayer = 11;
		private static int ProjectilesLayer = 12;
		private static int PlayerLayer = 9;
		private static int EnemiesLayer = 13;
		private static int MovingPlatformsLayer = 18;
		private static int PushablesLayer = 27;
		private static int MovingObjectsLayer = 17;
		private static int MovingOneWayPlatformsLayer = 20;
		private static int StairsLayer = 28;
		private static int MidHeightOneWayPlatformsLayer = 26;

		public static int PlatformsLayerMask = 1 << PlatformsLayer;
		public static int OneWayPlatformsLayerMask = 1 << OneWayPlatformsLayer;
		public static int ProjectilesLayerMask = 1 << ProjectilesLayer;
		public static int PlayerLayerMask = 1 << PlayerLayer;
		public static int EnemiesLayerMask = 1 << EnemiesLayer;
		public static int MovingPlatformsLayerMask = 1 << MovingPlatformsLayer;
		public static int PushablesLayerMask = 1 << PushablesLayer;
		public static int MovingObjectsLayerMask = 1 << MovingObjectsLayer;
		public static int MovingOneWayPlatformsMask = 1 << MovingOneWayPlatformsLayer;
		public static int StairsLayerMask = 1 << StairsLayer;
		public static int MidHeightOneWayPlatformsLayerMask = 1 << MidHeightOneWayPlatformsLayer;

		public static int ObstaclesLayerMask = LayerManager.PlatformsLayerMask | LayerManager.MovingPlatformsLayerMask | LayerManager.OneWayPlatformsLayerMask;
	}
}