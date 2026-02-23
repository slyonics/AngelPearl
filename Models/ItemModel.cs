using System.Linq;

namespace AngelPearl.Models
{
    public class ItemModel
    {
        public ItemModel(ItemRecord itemRecord, int quantity)
        {
            ItemRecord = itemRecord;
            Quantity.Value = quantity;
        }

        public ItemRecord ItemRecord { get; set; }
        public ModelProperty<int> Quantity { get; set; } = new ModelProperty<int>(1);


		public bool Consumable { get => Quantity.Value > 0; }
    }
}

