using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AngelPearl.Main;
using AngelPearl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngelPearl.SceneObjects;

namespace AngelPearl.Scenes.CrawlerScene
{
	public class BattleEnemy : Battler
	{
		private CrawlerScene crawlerScene;

		public BattleEnemy(CrawlerScene iScene, EnemyRecord enemyRecord, Vector2 iPosition)
			: base(iScene, iPosition, AssetCache.SPRITES[Enum.Parse<GameSprite>($"Enemies_{enemyRecord.Sprite}")])
		{
			crawlerScene = iScene;

		}


	}
}
