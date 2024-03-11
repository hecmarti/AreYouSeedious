using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{

	[CustomEditor (typeof(MovingPlatformFree))]
	[CanEditMultipleObjects]

	/// <summary>
	/// Adds custom labels to the CorgiController inspector
	/// </summary>

	public class MovingPlatformFreeInspector : Editor 
	{
		protected MovingPlatformFree _movingPlatformFree;
        
		protected SerializedProperty _debugCurrentSpeed;
		protected SerializedProperty _updateMethod;

		public override bool RequiresConstantRepaint()
		{
			return true;
		}

		protected virtual void OnEnable()
		{
			_movingPlatformFree = target as MovingPlatformFree;
			_debugCurrentSpeed = serializedObject.FindProperty("DebugCurrentSpeed");
			_updateMethod = serializedObject.FindProperty("UpdateMethod");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawProperties();

			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void DrawProperties()
		{
			EditorGUILayout.PropertyField(_updateMethod);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Debug");
			EditorGUILayout.PropertyField(_debugCurrentSpeed);
		}
	}
}