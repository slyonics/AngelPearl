using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using AngelPearl.Main;
using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using AngelPearl.SceneObjects.ViewModels;
using AngelPearl.SceneObjects.Widgets;
using AngelPearl.SceneObjects.Widgets;
using AngelPearl.Scenes.CrawlerScene;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AngelPearl.Scenes.CrawlerScene
{
	public class BattleViewModel : ViewModel
	{
		private CrawlerScene crawlerScene;

		private Panel enemyPanel;

		private CommandViewModel commandViewModel;

		public List<BattlerModel> InitiativeList { get; } = new();
		public List<BattlePlayer> PlayerList { get; } = [];
		public List<BattleEnemy> EnemyList { get; set; } = [];

		public BattleViewModel(CrawlerScene iScene, EncounterRecord encounterRecord)
			: base(iScene, PriorityLevel.CutsceneLevel)
		{
			crawlerScene = iScene;

			LoadView(GameView.Crawler_BattleView);
			enemyPanel = GetWidget<Panel>("EnemyPanel");

			Vector2 center = enemyPanel.AbsolutePosition + new Vector2(enemyPanel.OuterBounds.Width / 2, enemyPanel.OuterBounds.Height - 4);
			foreach (var enemy in encounterRecord.Enemies)
			{
				//Vector2 offset = new Vector2((306 - totalWidth) / 2, maxHeight / 2 + 24 + (104 - maxHeight));
				Vector2 offset = new(enemy.OffsetX, enemy.OffsetY);
				var enemyStack = new BattleEnemy(crawlerScene, EnemyRecord.ENEMIES.First(x => x.Name == enemy.Name), center + offset);
				EnemyList.Add(enemyStack);
			}

			foreach (var player in GameProfile.CurrentSave.Party)
			{
				PlayerList.Add(new BattlePlayer(crawlerScene, new Vector2(player.Value.WindowBounds.Value.Center.X, player.Value.WindowBounds.Value.Bottom), player.Value));
			}

			crawlerScene.MapViewModel.ShowMiniMap.Value = false;
			ConversationRecord conversationRecord = new ConversationRecord(encounterRecord.Intro);
			ConversationViewModel conversationViewModel = crawlerScene.AddView(new ConversationViewModel(crawlerScene, conversationRecord, PriorityLevel.MenuLevel));
			conversationViewModel.OnTerminated += new Action(() =>
			{
				crawlerScene.MapViewModel.ShowMiniMap.Value = true;
				NewRound();
			});

			Audio.PlayMusic(GameMusic.TheGrappler);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			

		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
		}

		private void NewRound()
		{
			PlayerTurn.Value = true;

			IdlePlayers.Clear();
			foreach (var player in PlayerList)
			{
				IdlePlayers.Add(player);
			}

			commandViewModel = new CommandViewModel(crawlerScene, IdlePlayers[0]);
		}

		public List<EnemyRecord> InitialEnemies { get; set; } = [];

		public List<BattlePlayer> IdlePlayers { get; set; } = [];


		public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
		public ModelProperty<bool> PlayerTurn { get; set; } = new ModelProperty<bool>(false);
	}
}
