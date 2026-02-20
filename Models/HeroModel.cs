using AngelPearl.Main;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AngelPearl.Models
{
	public class HeroModel : BattlerModel
	{
		public HeroModel()
		{
			
		}

		public HeroModel(HeroRecord heroRecord)
		{
			Name.Value = heroRecord.Name;
			Class.Value = heroRecord.ClassType;

			Portrait.Value = $"Portraits_{heroRecord.PortraitSprite}";

			Level.Value = heroRecord.Level;
            HP.Value = MaxHP.Value = heroRecord.BaseHP;
			
			Power.Value = heroRecord.BasePower;
			Finesse.Value = heroRecord.BaseFinesse;
			Magic.Value = heroRecord.BaseMagic;
			Charisma.Value = heroRecord.BaseCharisma;
			Guts.Value = heroRecord.BaseGuts;
		}

		public HeroModel(BinaryReader binaryReader)
        {

        }

		public void WriteToFile(BinaryWriter binaryWriter)
		{

		}

		public void Remove(ItemType itemType)
		{
			switch (itemType)
			{
				case ItemType.Weapon: Weapon.Value = null; break;
				case ItemType.Armor: Armor.Value = null; break;
				case ItemType.Accessory: Accessory.Value = null; break;
			}
		}

		public void Equip(string itemName)
		{
			ItemRecord itemRecord = ItemRecord.ITEMS.First(x => x.Name == itemName);
			Equip(itemRecord);
		}

		public void Equip(ItemRecord itemRecord)
		{
			switch (itemRecord.ItemType)
			{
				case ItemType.Weapon: Weapon.Value = itemRecord; break;
				case ItemType.Armor: Armor.Value = itemRecord; break;
				case ItemType.Accessory: Accessory.Value = itemRecord; break;
			}
		}

		public ModelProperty<Rectangle> WindowBounds { get; set; } = new ModelProperty<Rectangle>(new Rectangle(0, 0, 117, 180));
		public ModelProperty<string> Portrait { get; set; } = new ModelProperty<string>();

		public ModelProperty<ItemRecord> Weapon { get; private set; } = new ModelProperty<ItemRecord>();
		public ModelProperty<ItemRecord> Armor { get; private set; } = new ModelProperty<ItemRecord>();
		public ModelProperty<ItemRecord> Accessory { get; private set; } = new ModelProperty<ItemRecord>();


		public ModelProperty<int> Attack { get; set; } = new ModelProperty<int>(0);
		public ModelProperty<int> Hit { get; set; } = new ModelProperty<int>(100);
    }
}
