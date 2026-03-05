using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Models
{
	public class MissionCheckpoint
	{
		public string Name { get; set; }
		public string SpawnMap { get; set; }
		public int SpawnX { get; set; }
		public int SpawnY { get; set; }
		public string SpawnDirection { get; set; }
		public string Description { get; set; }
	}

	public class MissionRecord
    {

        public string Name { get; set; }
		public string Description { get; set; }

		public int MissionStart { get; set; }
		public int TimeLimit { get; set; }

		public string AngelSprite { get; set; }
		public int AngelStartX { get; set; }
		public int AngelStartY { get; set; }

		public MissionCheckpoint[] Checkpoints { get; set; }

		public static List<MissionRecord> MISSIONS { get; set; }
    }
}
