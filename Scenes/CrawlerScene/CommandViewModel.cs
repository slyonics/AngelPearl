using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.CrawlerScene
{
	public class CommandViewModel : ViewModel
	{
		CrawlerScene crawlerScene;

		TargetViewModel targetSelector;

		RadioBox commandList;

		public CommandViewModel(CrawlerScene iScene, BattlePlayer iBattlePlayer)
			: base(iScene, PriorityLevel.GameLevel)
		{
			ConfirmCooldown = false;

			crawlerScene = iScene;

			ActivePlayer = iBattlePlayer;
			ActivePlayer.HeroModel.NameColor.Value = Color.Red;

			foreach (var command in ActivePlayer.HeroModel.Commands)
				AvailableCommands.ModelList.Add(new ModelProperty<CommandRecord>(command.Value));

			Description.Value = AvailableCommands[0].Description;

			LoadView(GameView.Crawler_CommandView);

			commandList = GetWidget<RadioBox>("CommandList");

			(commandList.ChildList[0] as RadioButton).RadioSelect();

			crawlerScene.BattleViewModel.SetHeader($"What will {ActivePlayer.Stats.Name.Value} do?");
		}

		public override void Update(GameTime gameTime)
		{
			if (crawlerScene.BattleViewModel == null || ActivePlayer.Dead)
			{
				Terminate();
				return;
			}

			if (targetSelector != null)
			{
				if (targetSelector.Terminated)
				{
					targetSelector = null;
					CancelCooldown = true;
				}
				else return;
			}

			base.Update(gameTime);

			if (Input.CurrentInput.CommandPressed(Command.Cancel) && !CancelCooldown)
			{				
				var lastOrderedPlayer = crawlerScene.BattleViewModel.PlayerList.LastOrDefault(x => !x.Dead && !x.AwaitingOrders);
				if (lastOrderedPlayer != null)
				{
					Audio.PlaySound(GameSound.Back);
					Terminate();

					ActivePlayer.HeroModel.NameColor.Value = Color.White;

					lastOrderedPlayer.ResetCommand();
					crawlerScene.BattleViewModel.CommandViewModel = crawlerScene.AddView(new CommandViewModel(crawlerScene, lastOrderedPlayer));
				}
			}

			CancelCooldown = false;
		}

		public static bool ConfirmCooldown = false;
		public bool CancelCooldown = false;

		public void SelectCommand(object parameter)
		{
			if (targetSelector != null) return;

			CommandRecord battleCommand;
			if (parameter is IModelProperty)
			{
				battleCommand = (CommandRecord)((IModelProperty)parameter).GetValue();
			}
			else battleCommand = (CommandRecord)parameter;

			if (battleCommand != null && !battleCommand.Usable)
			{
				Audio.PlaySound(GameSound.Error);
				return;
			}

			Audio.PlaySound(GameSound.Selection);

			targetSelector = new TargetViewModel(crawlerScene, ActivePlayer, battleCommand);
			crawlerScene.AddOverlay(targetSelector);

			ShowCommandSummary.Value = false;
		}

		public void CommandChanged(object parameter)
		{
			Description.Value = AvailableCommands[(int)parameter].Description;
		}


		public BattlePlayer ActivePlayer { get; set; }

		public ModelCollection<CommandRecord> AvailableCommands { get; set; } = new ModelCollection<CommandRecord>();

		public ModelProperty<string> Description { get; set; } = new ModelProperty<string>("");

		public ModelProperty<bool> ShowCommandSummary { get; set; } = new ModelProperty<bool>(true);


		public ModelProperty<int> CurrentMP { get => ActivePlayer.HeroModel.MP; }

	}
}
