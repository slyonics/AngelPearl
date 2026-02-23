using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Models
{



	public class MuseRecord
    {

        public string Name { get; set; }
		public string Description { get; set; }
		public ClassType ClassType { get; set; }

        public string PortraitSprite { get; set; }

		public int Level { get; set; }
		public int BaseHP { get; set; }
		public int BaseMP { get; set; }
		public int BaseSkill { get; set; }
        public int BaseReflex { get; set; }
        public int BaseSong { get; set; }
		public int BaseTech { get; set; }
		public int BaseGuts { get; set; }
		public string Weapon {  get; set; }

		public static List<MuseRecord> PILOTS { get; set; }
    }
}
