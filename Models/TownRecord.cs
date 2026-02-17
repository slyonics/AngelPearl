using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace AngelPearl.Models
{


	public class TownRecord
	{
		public string Name { get; set; }
		public string Background { get; set; }

		public string MainNarration { get; set; }
		public string ShipNarration { get; set; }
		public string ShopNarration { get; set; }
		public string TavernNarration { get; set; }

		public string ShopName { get; set; }

		public string[] Heroes { get; set; }

		public static List<TownRecord> TOWNS { get; set; }
	}
}
