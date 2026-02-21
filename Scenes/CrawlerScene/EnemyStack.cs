using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.Main;
using AngelPearl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.CrawlerScene
{
	public class EnemyStack
	{
		private CrawlerScene crawlerScene;

		public EnemyStack(CrawlerScene iScene, EncounterStack encounterStack)
		{
			crawlerScene = iScene;

			EnemyRecord enemyRecord = EnemyRecord.ENEMIES.First(x => x.Name == encounterStack.Name);
			for (int i = 0; i < encounterStack.Count; i++)
			{
				BattlerModel enemyBattler = new BattlerModel(enemyRecord);
				Enemies.Add(enemyBattler);
			}

			Count.Value = Enemies.Count;
			Sprite.Value = Enum.Parse<GameSprite>($"Enemies_{enemyRecord.Sprite}");

			Texture2D spriteTexture = AssetCache.SPRITES[Sprite.Value];
			EnemyWidth = spriteTexture.Width;
			EnemyHeight = spriteTexture.Height;

			EnemyBounds.Value = new Rectangle(0, 0, EnemyWidth, EnemyHeight);

			Header.Value = Count.Value == 1 ? $"{enemyRecord.Name}" : $"{Count.Value} {enemyRecord.PluralName}";
		}

		public void UpdateBounds(ref Vector2 offset, int maxHeight)
		{
			PanelBounds.Value = new Rectangle((int)offset.X, (int)offset.Y + maxHeight - EnemyHeight, PanelWidth, PanelHeight);
			offset.X += PanelWidth + 8;
		}

		public int EnemyWidth { get; private set; }

		public int EnemyHeight { get; private set; }

		public int PanelWidth { get => EnemyWidth + 8; }

		public int PanelHeight { get => EnemyHeight + 8; }

		public List<BattlerModel> Enemies { get; } = [];


		public ModelProperty<int> Count { get; set; } = new ModelProperty<int>(1);

		public ModelProperty<GameSprite> Sprite { get; set; } = new ModelProperty<GameSprite>(GameSprite.Enemies_Brigand);

		public ModelProperty<Rectangle> PanelBounds { get; set; } = new ModelProperty<Rectangle>(new Rectangle());

		public ModelProperty<Rectangle> EnemyBounds { get; set; } = new ModelProperty<Rectangle>(new Rectangle());

		public ModelProperty<Color> EnemyColor { get; set; } = new ModelProperty<Color>(Color.White);

		public ModelProperty<string> Header { get; set; } = new ModelProperty<string>();

	}
}
