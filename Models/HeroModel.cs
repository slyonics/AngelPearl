using AngelPearl.Main;
using ldtk;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AngelPearl.Models
{
	public class HeroModel : BattlerModel
	{
        public static Dictionary<int, long> EXP_TABLE = new Dictionary<int, long>()
        {
            { 1, 0 },
            { 2, 10 },
            { 3, 33 },
            { 4, 74 },
            { 5, 140 },
            { 6, 241 },
            { 7, 389 },
            { 8, 599 },
            { 9, 888 },
            { 10, 1276 },
            { 11, 1786 },
            { 12, 2441 },
            { 13, 3269 },
            { 14, 4299 },
            { 15, 5564 },
            { 16, 7097 },
            { 17, 8936 },
            { 18, 11120 },
            { 19, 13691 },
            { 20, 16693 },
            { 21, 20173 },
            { 22, 24180 },
            { 23, 28765 },
            { 24, 33983 },
            { 25, 39890 },
            { 26, 46546 },
            { 27, 54012 },
            { 28, 62352 },
            { 29, 71632 },
            { 30, 81921 }
        };


        public HeroModel()
		{
			
		}

		public HeroModel(MuseRecord heroRecord)
		{
			Name.Value = heroRecord.Name;
			Class.Value = heroRecord.ClassType;
			Description.Value = heroRecord.Description;

			Portrait.Value = $"Portraits_{heroRecord.PortraitSprite}";
			FullBody.Value = $"Portraits_{heroRecord.FullBodySprite}";

			Level.Value = heroRecord.Level;
            HP.Value = MaxHP.Value = 40 + (4 * heroRecord.BaseHeart);
			MP.Value = MaxMP.Value = 20 + (2 * heroRecord.BaseMind);

			Skill.Value = heroRecord.BaseSkill;
			Reflex.Value = heroRecord.BaseReflex;
			Heart.Value = heroRecord.BaseHeart;
			Mind.Value = heroRecord.BaseMind;

			Power.Value = heroRecord.CosmoEngine.BasePower;
			Magic.Value = heroRecord.CosmoEngine.BaseMagic;
			Armor.Value = heroRecord.CosmoEngine.BaseArmor;
			Resist.Value = heroRecord.CosmoEngine.BaseResist;

			EquipWeapon(heroRecord.Weapon, false);
			EquipAccessory(heroRecord.Accessory, false);
			if (heroRecord.CosmoEngine.ActiveModules != null) foreach(var moduleName in heroRecord.CosmoEngine.ActiveModules) EquipModule(moduleName, false);
			if (heroRecord.CosmoEngine.PassiveModules != null) foreach (var moduleName in heroRecord.CosmoEngine.PassiveModules) EquipModule(moduleName, false);

            GrowAfterBattle(EXP_TABLE[heroRecord.Level]);

            CalculateStats();
			PopulateCommands();
		}

		public HeroModel(BinaryReader binaryReader)
        {

        }

		public void WriteToFile(BinaryWriter binaryWriter)
		{

		}

		public void EquipWeapon(string weaponName, bool calculateStats = true)
		{
			Weapon.Value = ItemRecord.ITEMS.First(x => x.Name == weaponName);

			if (calculateStats)
			{
				CalculateStats();
				PopulateCommands();
			}
		}

		public void EquipAccessory(string accessoryName, bool calculateStats = true)
		{
			Accessory.Value = ItemRecord.ITEMS.First(x => x.Name == accessoryName);
			if (calculateStats) CalculateStats();
		}

		public void EquipModule(string moduleName, bool calculateStats = true)
		{
			var module = ItemRecord.ITEMS.First(x => x.Name == moduleName);

			if (module.ItemType == ItemType.ActiveModule)
			{
				ActiveModules.Add(module);
				if (calculateStats) PopulateCommands();
			}
			else
			{
				if (calculateStats) CalculateStats();
			}
		}

		public void CalculateStats()
		{
			Attack.Value = Weapon.Value.Power;
			Accuracy.Value = Weapon.Value.Accuracy;
			Critical.Value = Weapon.Value.Critical + (Skill.Value / 4);

			PhysicalDefense.Value = Armor.Value;

			MagicDefense.Value = Resist.Value;
		}

		public void PopulateCommands()
		{
			Commands.Clear();
			Commands.ModelList.Add(new ModelProperty<CommandRecord>(new CommandRecord(Weapon.Value)));
			foreach (var module in ActiveModules) Commands.ModelList.Add(new ModelProperty<CommandRecord>(new CommandRecord(module.Value)));
		}

		public void UpdateHealthColor()
		{
			if (HP.Value > MaxHP.Value / 8) HealthColor.Value = new Color(252, 252, 252, 255);
			else if (HP.Value > 0) HealthColor.Value = new Color(228, 0, 88, 255);
			else HealthColor.Value = new Color(136, 20, 0, 255);
		}

        public List<DialogueRecord> GrowAfterBattle(long expGained)
        {
            int oldLevel = Level.Value;

            Exp.Value = Exp.Value + expGained;

            long expThreshold = EXP_TABLE[Level.Value + 1];
            while (Exp.Value >= expThreshold)
            {
                Level.Value = Level.Value + 1;
                expThreshold = EXP_TABLE[Level.Value + 1];
                CalculateStats();
            }

            NextLevel.Value = EXP_TABLE[Level.Value + 1] - Exp.Value;

            int newLevel = Level.Value;

            List<DialogueRecord> reports = new List<DialogueRecord>();

            if (oldLevel != newLevel)
            {
                DialogueRecord dialogueRecord = new DialogueRecord()
                {
                    Text = Name + " reached level " + newLevel + "!",
                    Script = new string[] { "Sound LevelUp" }
                };

                reports.Add(dialogueRecord);
            }
            ;

            /*
            var classRecord = ClassRecord.CLASSES.First(x => x.Name == Class.Value);
            if (classRecord.LearnableAbilities != null)
            {
                var newAbility = classRecord.LearnableAbilities.FirstOrDefault(x => x.Level <= Level.Value && !Abilities.Any(y => y.Value.Name == x.Ability));
                if (newAbility != null)
                {
                    var ability = AbilityRecord.ABILITIES.First(x => x.Name == newAbility.Ability);
                    Abilities.Add(ability);

                    DialogueRecord dialogueRecord = new DialogueRecord()
                    {
                        Text = Name + " learned @" + ability.Icon + " " + newAbility.Ability + "!"
                    };

                    reports.Add(dialogueRecord);
                }
            }
            */

            return reports;
        }


        public ModelProperty<Rectangle> WindowBounds { get; set; } = new ModelProperty<Rectangle>(new Rectangle(0, 0, 117, 180));
		public ModelProperty<Color> NameColor { get; set; } = new ModelProperty<Color>(Color.White);
		public ModelProperty<Color> HealthColor { get; set; } = new ModelProperty<Color>(Color.White);

		public ModelProperty<long> Exp { get; set; } = new ModelProperty<long>(0);
        public ModelProperty<long> NextLevel { get; set; } = new ModelProperty<long>(0);

        public ModelProperty<string> Portrait { get; set; } = new ModelProperty<string>();
		public ModelProperty<string> FullBody { get; set; } = new ModelProperty<string>();

		public ModelProperty<ItemRecord> Weapon { get; private set; } = new ModelProperty<ItemRecord>();
		public ModelProperty<ItemRecord> Accessory { get; private set; } = new ModelProperty<ItemRecord>();

		public ModelProperty<int> Power { get; set; } = new ModelProperty<int>(3);
		public ModelProperty<int> Magic { get; set; } = new ModelProperty<int>(3);
		public ModelProperty<int> Armor { get; set; } = new ModelProperty<int>(3);
		public ModelProperty<int> Resist { get; set; } = new ModelProperty<int>(3);

		public ModelCollection<ItemRecord> ActiveModules { get; private set; } = new ModelCollection<ItemRecord>() { };
		public ModelCollection<ItemRecord> PassiveModules { get; private set; } = new ModelCollection<ItemRecord>() { };

		public ModelCollection<CommandRecord> Commands { get; private set; } = new ModelCollection<CommandRecord> { };

		public ModelProperty<int> Attack { get; set; } = new ModelProperty<int>(1);
		public ModelProperty<int> Accuracy { get; set; } = new ModelProperty<int>(100);
		public ModelProperty<int> Critical { get; set; } = new ModelProperty<int>(5);

	}
}
