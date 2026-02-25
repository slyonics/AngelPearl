using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Models
{
    public class EncounterStack
    {
        public string Name { get; set; }
        public int OffsetX { get; set; }
		public int OffsetY { get; set; }
		public bool Flash { get; set; }
	}


	public class EncounterRecord
    {
        public string Name { get; set; }
        public EncounterStack[] Enemies { get; set; }
        public string Intro { get; set; }


        public static List<EncounterRecord> ENCOUNTERS { get; set; }
    }
}
