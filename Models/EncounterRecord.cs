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
        public int Count { get; set; }
    }


	public class EncounterRecord
    {
        public string Name { get; set; }
        public EncounterStack[] EncounterStacks { get; set; }
        public string Intro { get; set; }


        public static List<EncounterRecord> ENCOUNTERS { get; set; }
    }
}
