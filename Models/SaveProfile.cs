using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection.PortableExecutable;
using AngelPearl.Scenes.CrawlerScene;

namespace AngelPearl.Models
{
    [Serializable]
    public class SaveProfile
    {
        private static Dictionary<string, object> DEFAULT_SAVE_VALUES = new Dictionary<string, object>()
        {
            { "NewGame", true }
        };

        public SaveProfile()
        {
            SaveData = new Dictionary<string, object>(DEFAULT_SAVE_VALUES);
        }

        public SaveProfile(BinaryReader reader)
        {
            reader.ReadString();
            reader.ReadInt32();
            reader.ReadString();
            reader.ReadString();

            int partyCount = reader.ReadInt32();
            for (int i = 0; i < partyCount; i++) Party.Add(new HeroModel(reader));

            int inventoryCount = reader.ReadInt32();
            for (int i = 0; i < inventoryCount; i++)
            {
                string itemName = reader.ReadString();
                var itemRecord = ItemRecord.ITEMS.First(x => x.Name == itemName);
                int itemQuantity = reader.ReadInt32();
                AddInventory(itemName, itemQuantity);
            }

            LocationName.Value = reader.ReadString();
            SaveMapName = reader.ReadString();
            SaveMapX = reader.ReadInt32();
			SaveMapY = reader.ReadInt32();
			SaveDirection = (Direction)reader.ReadInt32();

			SaveData = new Dictionary<string, object>();
            int saveCount = reader.ReadInt32();
            for (int i = 0; i < saveCount; i++)
            {
                string saveName = reader.ReadString();
                byte typeByte = reader.ReadByte();
                switch (typeByte)
                {
                    case 0: SaveData.Add(saveName, reader.ReadBoolean()); break;
                    case 1: SaveData.Add(saveName, reader.ReadByte()); break;
                    case 2: SaveData.Add(saveName, reader.ReadInt32()); break;
                    case 3: SaveData.Add(saveName, reader.ReadInt64()); break;
                    case 4: SaveData.Add(saveName, reader.ReadSingle()); break;
                    case 5: SaveData.Add(saveName, reader.ReadString()); break;
                }
            }
        }

        public void AddInventory(string itemName, int quantity)
        {
            var itemEntry = Inventory.FirstOrDefault(x => x.Value.ItemRecord.Name == itemName);
            if (itemEntry == null || quantity < 0)
            {
                Inventory.Add(new ItemModel(ItemRecord.ITEMS.First(x => x.Name == itemName), quantity));
            }
            else
            {
                itemEntry.Value.Quantity.Value = itemEntry.Value.Quantity.Value + quantity;
                if (itemEntry.Value.Quantity.Value < 1) Inventory.Remove(itemEntry);
            }
        }

        public void RemoveInventory(ItemModel itemModel, int quantity)
        {
            var itemEntry = Inventory.FirstOrDefault(x => x.Value == itemModel);

            itemEntry.Value.Quantity.Value = itemEntry.Value.Quantity.Value + quantity;
            if (itemEntry.Value.Quantity.Value < 1)
            {
                Inventory.Remove(itemEntry);
            }
        }

        public int GetInventoryCount(string itemName)
        {
            var itemEntry = Inventory.FirstOrDefault(x => x.Value.ItemRecord.Name == itemName);
            if (itemEntry == null) return 0;
            else return itemEntry.Value.Quantity.Value;
        }

        public void WriteToFile(BinaryWriter writer)
        {
            writer.Write(LocationName.Value);
            writer.Write(GameProfile.SaveSlot);

            writer.Write(Party.Count);
            foreach (var hero in Party) hero.Value.WriteToFile(writer);

            writer.Write(Inventory.Count);
            foreach (var item in Inventory)
            {
                writer.Write(item.Value.ItemRecord.Name);
                writer.Write(item.Value.Quantity.Value);
            }

            writer.Write(LocationName.Value);
            writer.Write(SaveMapName);
            writer.Write(SaveMapX);
			writer.Write(SaveMapY);
			writer.Write((int)SaveDirection);

			writer.Write(SaveData.Count);
            foreach (var item in SaveData)
            {
                writer.Write(item.Key);
                switch (item.Value)
                {
                    case bool: writer.Write((byte)0); writer.Write((bool)item.Value); break;
                    case byte: writer.Write((byte)1); writer.Write((byte)item.Value); break;
                    case int: writer.Write((byte)2); writer.Write((int)item.Value); break;
                    case long: writer.Write((byte)3); writer.Write((long)item.Value); break;
                    case float: writer.Write((byte)4); writer.Write((float)item.Value); break;
                    case string: writer.Write((byte)5); writer.Write((string)item.Value); break;
                }
            }
        }


		public ModelCollection<HeroModel> Party { get; set; } = new ModelCollection<HeroModel>();
		public ModelCollection<HeroModel> Roster { get; set; } = new ModelCollection<HeroModel>();
		public ModelProperty<HeroModel> Mascot { get; set; } = new ModelProperty<HeroModel>();
		public ModelCollection<ItemModel> Inventory { get; set; } = new ModelCollection<ItemModel>();

        public ModelProperty<string> LocationName { get; set; } = new ModelProperty<string>("S.T.A.R. Base");
        public string SaveMapName { get; set; } = "";
		public int SaveMapX { get; set; } = 12;
		public int SaveMapY { get; set; } = 12;
		public Direction SaveDirection { get; set; } = Direction.North;

		public ModelProperty<MissionRecord> CurrentMission { get; set; } = new ModelProperty<MissionRecord>();

		public ModelProperty<int> DayOfYear { get; set; } = new ModelProperty<int>(1);



        public Dictionary<string, object> SaveData;
	}
}
