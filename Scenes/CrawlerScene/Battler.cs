using AngelPearl.Models;
using AngelPearl.SceneObjects;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.CrawlerScene
{
	public class Battler : Entity
	{
		protected Effect shader;
		protected int flashTime;
		protected int flashDuration;

		protected int actionTime;
		protected bool turnActive;

		protected BattlerModel stats;
		public BattlerModel Stats { get => stats; }

		public Battler(CrawlerScene iScene, Vector2 iPosition, Texture2D iSprite)
			: base(iScene, iPosition, iSprite, new Dictionary<string, Animation>() { { "Idle", new Animation(0, 0, iSprite.Width, iSprite.Height, 1, 10000) } })
		{

		}

		public Battler(CrawlerScene iScene, Vector2 iPosition, Texture2D iSprite, Dictionary<string, Animation> iAnimations)
			: base(iScene, iPosition, iSprite, iAnimations)
		{

		}
	}
}
