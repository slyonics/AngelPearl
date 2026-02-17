using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace AngelPearl.Models
{
	public class CostRecord
	{
		public string Item { get; set; }
		public string Icon { get; set; }

		public Color CostColor { get; set; }
		public int Have { get; set; }
		public int Need { get; set; }
	}

	public class VoucherRecord
	{
		public string Name { get; set; }
		public string Icon { get; set; }
		public string Description { get; set; }
		public long Price { get; set; }
		public CostRecord[] Materials { get; set; }

		public bool HasMaterialCost { get => Materials.Any(x => x.Item != null); }
		public bool Affordable { get => Price <= GameProfile.CurrentSave.Money.Value; }
	}

	public class ShopRecord
	{
		public string Name { get; set; }
		public string Intro { get; set; }

		public VoucherRecord[] Vouchers { get; set; }

		public static List<ShopRecord> SHOPS { get; set; }
	}
}
