using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	[CustomEditor(typeof(Weapon), true)]
	[CanEditMultipleObjects]

	/// <summary>
	/// Adds weapon state display to the Weapons inspector
	/// </summary>

	public class WeaponEditor : MMMonoBehaviourDrawer
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			Weapon weapon = (Weapon)target;
			if (weapon.WeaponState != null)
			{
				EditorGUILayout.LabelField("Weapon State", weapon.WeaponState.CurrentState.ToString());
			}
		}

	}
}