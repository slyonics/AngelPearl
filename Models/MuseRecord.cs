using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Models
{
	public class EngineRecord
	{
		public string Description { get; set; }
		public string Sprite { get; set; }

		public int BasePower { get; set; }
		public int BaseMagic { get; set; }
		public int BaseArmor { get; set; }
		public int BaseResist { get; set; }

		public string[] ActiveModules { get; set; }
		public string[] PassiveModules { get; set; }
	}


	public class MuseRecord
    {

        public string Name { get; set; }
		public string Description { get; set; }

		public ClassType ClassType { get; set; }
		public int Level { get; set; }

		public string PortraitSprite { get; set; }

		public int BaseHP { get; set; }
		public int BaseMP { get; set; }

		public int BaseSkill { get; set; }
        public int BaseReflex { get; set; }
		public int BaseHeart { get; set; }
		public int BaseMind { get; set; }

		public string Weapon {  get; set; }
		public string Accessory { get; set; }

		public EngineRecord CosmoEngine { get; set; }

		public static List<MuseRecord> MUSES { get; set; }
    }
}
