using System;
using System.Collections.Generic;

using AngelPearl.SceneObjects;

namespace AngelPearl.Models
{
    public class AttackData
    {
        public string[] Script;
        public int Weight;
        public TargetType Targetting = TargetType.OneEnemy;
        public int Power = -1;
        public int Accuracy = 100;
        public CommandPriority Priority = CommandPriority.Melee;
    }

    public class EnemyRecord
    {
        public EnemyRecord()
        {

        }

        public EnemyRecord(EnemyRecord clone)
        {

        }

        public string Name { get; set; }

        public string PluralName { get; set; }

        public ClassType Class { get; set; }
        public int Level { get; set; }
        public string Sprite { get; set; }
        public int ShadowOffset { get; set; }
        public int Exp { get; set; }
        public string Description { get; set; }

        public long HP { get; set; }
        public int MP { get; set; }

        public int PhysicalAttack { get; set; }
		public int PhysicalDefense { get; set; }
		public int PhysicalEvade { get; set; }

        public int MagicAttack { get; set; }
        public int MagicDefense { get; set; }
        public int MagicEvade { get; set; }

		public int Skill { get; set; }
		public int Reflex { get; set; }
		public int Heart { get; set; }
		public int Mind { get; set; }

		public AttackData[] Attacks { get; set; }

        public ElementType[] ElementWeak { get; set; }
        public ElementType[] ElementStrong { get; set; }
        public ElementType[] ElementImmune { get; set; }
        public ElementType[] ElementAbsorb { get; set; }
        public AilmentType[] AilmentImmune { get; set; }

        public int BattleOffsetX { get; set; }
        public int BattleOffsetY { get; set; }
        public Alignment BattleAlignment { get; set; } = Alignment.Cascading;

        public static List<EnemyRecord> ENEMIES { get; set; }
    }
}
