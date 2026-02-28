using AngelPearl.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AngelPearl.Models
{
	public class HeroModel : BattlerModel
	{
		public HeroModel()
		{
			
		}

		public HeroModel(MuseRecord heroRecord)
		{
			Name.Value = heroRecord.Name;
			Class.Value = heroRecord.ClassType;

			Portrait.Value = $"Portraits_{heroRecord.PortraitSprite}";

			Level.Value = heroRecord.Level;
            HP.Value = MaxHP.Value = heroRecord.BaseHP;
			MP.Value = MaxMP.Value = heroRecord.BaseMP;

			Skill.Value = heroRecord.BaseSkill;
			Reflex.Value = heroRecord.BaseReflex;
			Heart.Value = heroRecord.BaseHeart;
			Mind.Value = heroRecord.BaseMind;

			Power.Value = heroRecord.CosmoEngine.BasePower;
			Magic.Value = heroRecord.CosmoEngine.BaseMagic;
			Armor.Value = heroRecord.CosmoEngine.BaseArmor;
			Resist.Value = heroRecord.CosmoEngine.BaseResist;

			EquipWeapon(heroRecord.Weapon);
			EquipAccessory(heroRecord.Accessory);
			if (heroRecord.CosmoEngine.ActiveModules != null) foreach(var moduleName in heroRecord.CosmoEngine.ActiveModules) EquipModule(moduleName);
			if (heroRecord.CosmoEngine.PassiveModules != null) foreach (var moduleName in heroRecord.CosmoEngine.PassiveModules) EquipModule(moduleName);
		}

		public HeroModel(BinaryReader binaryReader)
        {

        }

		public void WriteToFile(BinaryWriter binaryWriter)
		{

		}

		public void EquipWeapon(string weaponName)
		{
			Weapon.Value = ItemRecord.ITEMS.First(x => x.Name == weaponName);
			PopulateCommands();
		}

		public void EquipAccessory(string accessoryName)
		{
			Accessory.Value = ItemRecord.ITEMS.First(x => x.Name == accessoryName);

		}

		public void EquipModule(string moduleName)
		{
			var module = ItemRecord.ITEMS.First(x => x.Name == moduleName);

			if (module.ItemType == ItemType.ActiveModule)
			{
				ActiveModules.Add(module);
				PopulateCommands();
			}
			else
			{

			}
		}

		public void CalculateStats()
		{

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

		public ModelProperty<Rectangle> WindowBounds { get; set; } = new ModelProperty<Rectangle>(new Rectangle(0, 0, 117, 180));
		public ModelProperty<Color> NameColor { get; set; } = new ModelProperty<Color>(Color.White);
		public ModelProperty<Color> HealthColor { get; set; } = new ModelProperty<Color>(Color.White);

		public ModelProperty<string> Portrait { get; set; } = new ModelProperty<string>();

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
