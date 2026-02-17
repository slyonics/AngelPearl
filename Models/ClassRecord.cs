using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Models
{
	public enum ClassType
	{
		Sailor,
		Scout,
		Trader,
		Explorer,
		Herder,
		Doctor,

		Monster,
		Undead,
		Beast,
		Boss
	}

	public enum BattleCommand
	{
		Fight,
		Defend,
		Magic,
		Item,
	}

	public class LearnableAbility
	{
		public string Ability;
		public int Level;
	}

	public class ClassRecord
	{
		public ClassType Name { get; set; }

		public List<BattleCommand> Commands { get; set; } = new List<BattleCommand>();
		public List<LearnableAbility> LearnedAbilities { get; set; } = new List<LearnableAbility>();

		public static List<ClassRecord> CLASSES { get; set; }
	}
}
