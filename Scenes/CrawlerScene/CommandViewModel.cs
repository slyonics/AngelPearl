using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Widgets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.CrawlerScene
{
	public class CommandViewModel : ViewModel
	{
		CrawlerScene battleScene;

		TargetViewModel targetSelector;

		RadioBox commandList;
		Panel mpCounter;

		public CommandViewModel(CrawlerScene iScene, BattlePlayer iBattlePlayer)
			: base(iScene, PriorityLevel.GameLevel)
		{
			ConfirmCooldown = false;

			battleScene = iScene;

			ActivePlayer = iBattlePlayer;

			foreach (var command in ActivePlayer.HeroModel.Commands)
				AvailableCommands.ModelList.Add(new ModelProperty<CommandRecord>(command.Value));

			int commandHeight = Math.Max(60, AvailableCommands.Count() * 10 + 16);
			CommandBounds.Value = new Rectangle(-90, Math.Min(30, 90 - commandHeight), 60, commandHeight);

			LoadView(GameView.Crawler_CommandView);

			commandList = GetWidget<RadioBox>("CommandList");
			mpCounter = GetWidget<Panel>("MPCounter");

			(commandList.ChildList[0] as RadioButton).RadioSelect();
		}

		public override void Update(GameTime gameTime)
		{
			if (battleScene.BattleViewModel == null || ActivePlayer.Dead)
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
					Audio.PlaySound(GameSound.Back);

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


			targetSelector = new TargetViewModel(battleScene, ActivePlayer, battleCommand);
			battleScene.AddOverlay(targetSelector);
		}


		public BattlePlayer ActivePlayer { get; set; }

		public ModelProperty<Rectangle> CommandBounds { get; set; } = new ModelProperty<Rectangle>();

		public ModelCollection<CommandRecord> AvailableCommands { get; set; } = new ModelCollection<CommandRecord>();

		public ModelProperty<int> CurrentMP { get => ActivePlayer.HeroModel.MP; }

	}
}
