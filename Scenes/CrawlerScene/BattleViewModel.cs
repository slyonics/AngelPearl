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
		private Panel commandPanel;

		public List<BattlerModel> InitiativeList { get; } = new();
		public List<HeroModel> PlayerList { get; } = new();
		public List<BattlerModel> EnemyList { get; } = new();

		public BattleViewModel(CrawlerScene iScene, EncounterRecord encounterRecord)
			: base(iScene, PriorityLevel.CutsceneLevel)
		{
			crawlerScene = iScene;

			foreach (var stack in encounterRecord.EncounterStacks)
			{
				var enemyStack = new EnemyStack(crawlerScene, stack);
				EnemyStacks.Add(enemyStack);
			}

			int totalWidth = EnemyStacks.Sum(x => x.Value.PanelWidth) + (EnemyStacks.Count - 1) * 8;
			int maxHeight = EnemyStacks.Max(x => x.Value.EnemyHeight);
			Vector2 offset = new Vector2((306 - totalWidth) / 2, (176 - maxHeight) / 2);

			foreach (var stack in EnemyStacks)
			{
				stack.Value.UpdateBounds(ref offset, maxHeight);
			}

			LoadView(GameView.Crawler_BattleView);
			enemyPanel = GetWidget<Panel>("EnemyPanel");
			commandPanel = GetWidget<Panel>("CommandPanel");

			crawlerScene.MapViewModel.ShowPlayerBar.Value = false;
			ConversationRecord conversationRecord = new ConversationRecord(encounterRecord.Intro);
			ConversationViewModel conversationViewModel = crawlerScene.AddView(new ConversationViewModel(crawlerScene, conversationRecord, PriorityLevel.MenuLevel));
			conversationViewModel.OnTerminated += new Action(() =>
			{
				crawlerScene.MapViewModel.ShowPlayerBar.Value = true;
				NewRound();
			});

			Audio.PlayMusic(GameMusic.TheGrappler);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (parentScene.PriorityLevel == PriorityLevel.CutsceneLevel)
			{
				if (Input.CurrentInput.AnythingPressed())
				{
					Terminate();

					crawlerScene.EndBattle();
                }
			}
		}

		private void NewRound()
		{
			PlayerTurn.Value = true;
		}

		public List<Tuple<EnemyRecord, int>> InitialEnemies { get; set; } = [];

		public ModelCollection<EnemyStack> EnemyStacks { get; set; } = new ModelCollection<EnemyStack>();

		public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
		public ModelProperty<bool> PlayerTurn { get; set; } = new ModelProperty<bool>(false);
	}
}
