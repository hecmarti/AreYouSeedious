using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.CorgiEngine
{
	#if UNITY_EDITOR
	/// <summary>
	/// This class lets you specify (in code, by editing it) symbols that will be added to the build settings' define symbols list automatically
	/// </summary>
	[InitializeOnLoad]
	public class CorgiEngineDefineSymbols
	{
		/// <summary>
		/// A list of all the symbols you want added to the build settings
		/// </summary>
		public static readonly string[] Symbols = new string[]
		{
			"MOREMOUNTAINS_CORGIENGINE"
		};

		/// <summary>
		/// As soon as this class has finished compiling, adds the specified define symbols to the build settings
		/// </summary>
		static CorgiEngineDefineSymbols()
		{
			string scriptingDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			List<string> scriptingDefinesStringList = scriptingDefinesString.Split(';').ToList();
			scriptingDefinesStringList.AddRange(Symbols.Except(scriptingDefinesStringList));
			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", scriptingDefinesStringList.ToArray()));
		}
	}
	#endif
}