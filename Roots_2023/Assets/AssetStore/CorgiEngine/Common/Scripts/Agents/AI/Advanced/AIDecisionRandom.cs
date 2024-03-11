using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This decision will roll a dice and return true if the result is below or equal the Odds value
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Decisions/AI Decision Random")]
	public class AIDecisionRandom : AIDecision
	{
		/// the maximum number to consider when rolling the dice (in '2 out of 10', that'd be 10
		[Tooltip("the maximum number to consider when rolling the dice (in '2 out of 10', that'd be 10")]
		public int TotalChance = 10;
		/// the number below which this decision will be true. In '2 out of 10', that would be 2
		[Tooltip("the number below which this decision will be true. In '2 out of 10', that would be 2")]
		public int Odds = 4;

		protected Character _targetCharacter;

		/// <summary>
		/// On Decide we check if the odds are in our favour
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return EvaluateOdds();
		}

		/// <summary>
		/// Returns true if the Brain's Target is facing us (this will require that the Target has a Character component)
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateOdds()
		{
			int dice = MMMaths.RollADice(TotalChance);
			bool result = (dice <= Odds);
			return result;
		}
	}
}