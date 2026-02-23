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
			
			Power.Value = heroRecord.BaseSkill;
			Reflex.Value = heroRecord.BaseReflex;
			Magic.Value = heroRecord.BaseSong;
			Charisma.Value = heroRecord.BaseTech;
			Guts.Value = heroRecord.BaseGuts;

			EquipWeapon(heroRecord.Weapon);
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

		public void PopulateCommands()
		{
			Commands.Clear();
			Commands.ModelList.Add(new ModelProperty<CommandRecord>(new CommandRecord(Weapon.Value)));
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
		public ModelProperty<ItemRecord> ActiveModules { get; private set; } = new ModelProperty<ItemRecord>();
		public ModelProperty<ItemRecord> PassiveModules { get; private set; } = new ModelProperty<ItemRecord>();

		public ModelCollection<CommandRecord> Commands { get; private set; } = new ModelCollection<CommandRecord> { };

		public ModelProperty<int> Attack { get; set; } = new ModelProperty<int>(0);
		public ModelProperty<int> Hit { get; set; } = new ModelProperty<int>(100);
    }
}
