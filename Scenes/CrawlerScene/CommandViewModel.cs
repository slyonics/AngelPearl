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
		Panel mpCounter;

		public CommandViewModel(CrawlerScene iScene, BattlePlayer iBattlePlayer)
			: base(iScene, PriorityLevel.GameLevel)
		{
			ConfirmCooldown = false;

			crawlerScene = iScene;

			ActivePlayer = iBattlePlayer;

			foreach (var command in ActivePlayer.HeroModel.Commands)
				AvailableCommands.ModelList.Add(new ModelProperty<CommandRecord>(command.Value));

			LoadView(GameView.Crawler_CommandView);

			commandList = GetWidget<RadioBox>("CommandList");
			mpCounter = GetWidget<Panel>("MPCounter");

			(commandList.ChildList[0] as RadioButton).RadioSelect();

			DrawOnNewLayer = true;
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
					if (!ActivePlayer.Ready)
					{
						Terminate();
						return;
					}
					else
					{
						CancelCooldown = true;

						
					}
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
		}


		public BattlePlayer ActivePlayer { get; set; }

		public ModelCollection<CommandRecord> AvailableCommands { get; set; } = new ModelCollection<CommandRecord>();

		public ModelProperty<int> CurrentMP { get => ActivePlayer.HeroModel.MP; }

	}
}
