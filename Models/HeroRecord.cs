using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Models
{

	public class HeroRecord
    {

        public string Name { get; set; }
		public string Description { get; set; }
		public ClassType ClassType { get; set; }

        public string PortraitSprite { get; set; }

		public int Level { get; set; }
		public int BaseHP { get; set; }
		public int BaseMinorCharges { get; set; }
		public int BaseSignificantCharges { get; set; }
		public int BaseMajorCharges { get; set; }
		public int BasePower { get; set; }
        public int BaseFinesse { get; set; }
        public int BaseMagic { get; set; }
		public int BaseCharisma { get; set; }
		public int BaseGuts { get; set; }
		public int Weapon { get; set; }
		public int Armor { get; set; }
		public int Accessory { get; set; }

		public static List<HeroRecord> HEROES { get; set; }
    }
}
