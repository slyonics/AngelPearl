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
using AngelPearl.Scenes.BaseScene;
using AngelPearl.Scenes.CrawlerScene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.CrawlerScene
{
	public class BattleViewModel : ViewModel
	{
		private CrawlerScene crawlerScene;

		private Panel enemyPanel;
		public CrawlText crawlText;

		public CommandViewModel CommandViewModel { get; set; }

		public List<Battler> InitiativeList { get; private set; } = new();
		public List<BattlePlayer> PlayerList { get; } = [];
		public List<BattleEnemy> EnemyList { get; set; } = [];

		public event Action OnNarrationDone;


		EncounterRecord record;

		public BattleViewModel(CrawlerScene iScene, EncounterRecord encounterRecord)
			: base(iScene, PriorityLevel.CutsceneLevel)
		{
			crawlerScene = iScene;
			record = encounterRecord;

			LoadView(GameView.Crawler_BattleView);
			enemyPanel = GetWidget<Panel>("EnemyPanel");
			crawlText = GetWidget<CrawlText>("ConversationText");

			int i = 0;
			Vector2 center = enemyPanel.AbsolutePosition + new Vector2(enemyPanel.OuterBounds.Width / 2, enemyPanel.OuterBounds.Height);
			foreach (var enemy in encounterRecord.Enemies)
			{
				InitialEnemies.Add(EnemyRecord.ENEMIES.First(x => x.Name == enemy.Name));

				if (!enemy.Flash)
				{
					crawlerScene.FLASH_COLOR[i] = new Vector4(1.0f);
				}

				Vector2 offset = new(enemy.OffsetX, enemy.OffsetY - 64);
				var battleEnemy = new BattleEnemy(crawlerScene, EnemyRecord.ENEMIES.First(x => x.Name == enemy.Name), center + offset, i, enemy.Flash);
				EnemyList.Add(battleEnemy);
				crawlerScene.AddEntity(battleEnemy);
				i++;
			}

			i = 0;
			Vector2[] offsets = [ new(55, 37), new(55, 100), new(55, 163), new(222, 95) ];
			foreach (var player in GameProfile.CurrentSave.Party)
			{
				var battlePlayer = new BattlePlayer(crawlerScene, offsets[i], player.Value);
				PlayerList.Add(battlePlayer);
				crawlerScene.AddEntity(battlePlayer);
				i++;
			}

			DrawOnNewLayer = true;

			ConversationRecord conversationRecord = new ConversationRecord(encounterRecord.Intro);
			ConversationViewModel conversationViewModel = crawlerScene.AddView(new ConversationViewModel(crawlerScene, conversationRecord, PriorityLevel.MenuLevel));
			conversationViewModel.OnTerminated += new Action(() =>
			{
				NewRound();
			});

			if (encounterRecord.Boss) Audio.PlayMusic(GameMusic.BossTheme1);
			else Audio.PlayMusic(GameMusic.Battle1);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			EnemyList.RemoveAll(x => x.Terminated);
			if (!crawlerScene.ControllerStack.Any(y => y.Any(x => x is BattleController)) && !PlayerList.Any(x => x.Busy) && !EnemyList.Any(x => x.Busy))
			{
				if (EnemyList.Count == 0)
				{
					if (!crawlerScene.OverlayList.Any(x => x is ConversationViewModel))
					{
						Victory();
					}

					return;
				}

				if (PlayerList.All(x => x.Dead))
				{
					if (!crawlerScene.OverlayList.Any(x => x is ConversationViewModel))
					{
						Defeat();
					}

					return;
				}

				if (InitiativeList.Count == 0)
				{
					if (TurnExecuting.Value)
					{
						TurnExecuting.Value = false;
						NewRound();
					}
				}
				else
				{
					InitiativeList.First().ExecuteTurn();
					InitiativeList.RemoveAt(0);
				}
			}

			if (crawlText.ReadyToProceed && OnNarrationDone != null)
			{
				OnNarrationDone.Invoke();
				OnNarrationDone = null;
			}
        }

		public override void Draw(SpriteBatch spriteBatch)
		{
			ShowHeader.Value = !crawlerScene.OverlayList.Any(x => x is ConversationViewModel);

			base.Draw(spriteBatch);
		}

		public void NewRound()
		{
			TurnExecuting.Value = false;
			Narration.Value = "";

			CommandViewModel = crawlerScene.AddView(new CommandViewModel(crawlerScene, PlayerList.First(x => !x.Dead && x.AwaitingOrders)));
		}

		public void ExecuteRound()
		{
			TurnExecuting.Value = true;
			CommandHeader1.Value = "";
			CommandHeader2.Value = "";

			foreach (BattleEnemy enemy in EnemyList)
			{
				enemy.PlanTurn();
			}

			InitiativeList.Clear();
			InitiativeList.AddRange(PlayerList.Where(x => !x.Dead));
			InitiativeList.AddRange(EnemyList);

			InitiativeList = InitiativeList.OrderByDescending(x => x.Initiative).ToList();
		}

        private void Victory()
        {
            int expGain = 0;
            foreach (var enemy in InitialEnemies)
            {
                EnemyRecord enemyRecord = EnemyRecord.ENEMIES.First(x => x.Name == enemy.Name);
                expGain += enemyRecord.Exp;
            }

            List<DialogueRecord> victoryRecords = new List<DialogueRecord>();
            victoryRecords.Add(new DialogueRecord { Text = $"Good work, all foes have been dispatched! The party gained {expGain} experience points." });

            foreach (var battlePlayer in PlayerList)
            {
                if (battlePlayer.Dead) continue;
                List<DialogueRecord> reports = battlePlayer.HeroModel.GrowAfterBattle(expGain);
                foreach (DialogueRecord report in reports) victoryRecords.Add(report);
                battlePlayer.HealAilment(AilmentType.Confusion);
                battlePlayer.HeroModel.BattleOrdersVisible.Value = false;
            }
            var victoryConversation = new ConversationRecord() { DialogueRecords = victoryRecords.ToArray() };

            ConversationViewModel conversationViewModel = crawlerScene.AddView(new ConversationViewModel(crawlerScene, victoryConversation, PriorityLevel.MenuLevel));
            conversationViewModel.OnTerminated += new Action(() =>
            {
                crawlerScene.EndBattle();
                Terminate();

				if (record.Boss)
				{
					crawlerScene.AddView(new ConversationViewModel(crawlerScene, ConversationRecord.CONVERSATIONS.First(x => x.Name == "PostGabriel"), PriorityLevel.MenuLevel));
				}
            });
        }

		private void Defeat()
		{
			var defeatConversation = new ConversationRecord("The party was wiped out... returning to Home Base screen.");
			ConversationViewModel conversationViewModel = crawlerScene.AddView(new ConversationViewModel(crawlerScene, defeatConversation, PriorityLevel.MenuLevel));
			conversationViewModel.OnTerminated += new Action(() =>
			{
				Terminate();
				CrossPlatformGame.Transition(new Task<Scene>(() => new BaseScene.BaseScene()));
			});
		}


		public void SetHeader(string header)
		{
			CommandHeader1.Value = "";
			CommandHeader2.Value = "";

			if (Text.GetStringLength(GameFont.Console, header) >= 92)
			{
				var tokens = new Queue<string>(header.Split(' '));
				CommandHeader1.Value += $" {tokens.Dequeue()}";
				while (Math.Abs(Text.GetStringLength(GameFont.Console, CommandHeader1.Value) - Text.GetStringLength(GameFont.Console, string.Join(' ', tokens))) >
					   Math.Abs(Text.GetStringLength(GameFont.Console, CommandHeader1.Value + $" {tokens.Peek()}") - Text.GetStringLength(GameFont.Console, string.Join(' ', tokens.Skip(1)))))
				{
					CommandHeader1.Value += $" {tokens.Dequeue()}";
				}
				CommandHeader2.Value = string.Join(' ', tokens);
			}
			else CommandHeader1.Value = header;
		}

		public List<EnemyRecord> InitialEnemies { get; set; } = [];

        public ModelProperty<bool> ShowHeader { get; set; } = new ModelProperty<bool>(false);
        
        public ModelProperty<string> CommandHeader1 { get; set; } = new ModelProperty<string>("");
		public ModelProperty<string> CommandHeader2 { get; set; } = new ModelProperty<string>("");

		public ModelProperty<string> Narration { get; set; } = new ModelProperty<string>("");

		public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
		public ModelProperty<bool> TurnExecuting { get; set; } = new ModelProperty<bool>(false);
	}
}
