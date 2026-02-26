using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Models
{
    public enum ItemType
    {
        Weapon,
        ActiveModule,
        PassiveModule,
        Upgrade,
        Accessory,
        Consumable,
        Plot
    }

    public enum CommandPriority
    {
        Ritual,
        Melee,
        Ranged,
        Word,
        Thought
    }

    public class ItemRecord
    {
        public ItemRecord()
        {

        }

		public ItemRecord(ItemRecord clone)
		{

		}

		public string Name { get; set; }
        public string Animation { get; set; }
		public string Description { get; set; }
		public ItemType ItemType { get; set; }
		public string Icon { get; set; }

        public int SkillModifier { get; set; }
        public int ReflexModifier { get; set; }
        public int MagicModifier { get; set; }
        public int TechModifier { get; set; }
		public int HeartModifier { get; set; }

		public int ControlModifier { get; set; }
		public int SensorsModifier { get; set; }
		public int ResonanceModifier { get; set; }
		public int EnergyModifier { get; set; }
		public int ShieldingModifier { get; set; }

		public int DefenseModifier { get; set; }
		public int MagicDefenseModifier { get; set; }
		public int EvadeModifier { get; set; }
		public int MagicEvadeModifier { get; set; }

		public int Power { get; set; }
        public int Accuracy { get; set; }
        public int Critical { get; set; }
        public CommandPriority Priority { get; set; }

        public ElementType AttackElement { get; set; }
        public ElementType[] ElementsWeak { get; set; }
        public ElementType[] ElementsStrong { get; set; }
        public ElementType[] ElementsAbsorb { get; set; }
        public ElementType[] ElementsImmune { get; set; }
        public AilmentType[] AilmentsImmune { get; set; }

        public ElementType[] ElementsBoost { get; set; }
        public BuffType[] AutoBuffs { get; set; }

		public TargetType Targetting { get; set; }
		public bool TargetDead { get; set; } // true if this can target dead allies

		public string Conditions { get; set; }
		public string[] BattleScript { get; set; }
		public string[] FieldScript { get; set; }

		public static List<ItemRecord> ITEMS { get; set; }
    }
}
