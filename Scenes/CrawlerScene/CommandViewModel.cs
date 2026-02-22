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
		RadioBox subCommandList;
		Panel mpCounter;

		public CommandViewModel(CrawlerScene iScene, BattlePlayer iBattlePlayer)
			: base(iScene, PriorityLevel.GameLevel)
		{
			ConfirmCooldown = false;

			battleScene = iScene;

			ActivePlayer = iBattlePlayer;

			AvailableCommands = ActivePlayer.HeroModel.Commands;

			int commandHeight = Math.Max(60, AvailableCommands.Count() * 10 + 16);
			CommandBounds.Value = new Rectangle(-90, Math.Min(30, 90 - commandHeight), 60, commandHeight);

			LoadView(GameView.Crawler_CommandView);

			commandList = GetWidget<RadioBox>("CommandList");
			subCommandList = GetWidget<RadioBox>("SubCommandList");
			mpCounter = GetWidget<Panel>("MPCounter");

			(commandList.ChildList[0] as RadioButton).RadioSelect();

			if (ActivePlayer.Stats.StatusAilments.ModelList.Any(x => x.Value == AilmentType.Confusion))
			{
				int index = ActivePlayer.HeroModel.Commands.ModelList.FindIndex(x => x.Value == BattleCommand.Magic);
				if (index >= 0) commandList.ChildList[index].Enabled = false;
			}
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
				if (subCommandList.Enabled)
				{
					Audio.PlaySound(GameSound.Back);
					subCommandList.Enabled = false;
					subCommandList.Visible = false;
					commandList.Enabled = true;
					mpCounter.Visible = false;
				}
			}

			CancelCooldown = false;
		}

		public static bool ConfirmCooldown = false;
		public bool CancelCooldown = false;

		public void SelectCommand(object parameter)
		{
			if (targetSelector != null) return;

			ItemModel battleCommand;
			if (parameter is IModelProperty)
			{
				battleCommand = (ItemModel)((IModelProperty)parameter).GetValue();
			}
			else battleCommand = (ItemModel)parameter;

			if (battleCommand != null && !battleCommand.BattleUsable)
			{
				Audio.PlaySound(GameSound.Error);
				return;
			}

			Audio.PlaySound(GameSound.Selection);

			subCommandList.Enabled = false;
			subCommandList.Visible = false;

			targetSelector = new TargetViewModel(battleScene, ActivePlayer, battleCommand, AvailableCommands[commandList.Selection]);
			battleScene.AddOverlay(targetSelector);
		}


		public BattlePlayer ActivePlayer { get; set; }

		public ModelProperty<Rectangle> CommandBounds { get; set; } = new ModelProperty<Rectangle>();

		public ModelCollection<CommandRecord> AvailableCommands { get; set; } = new ModelCollection<CommandRecord>();

		public ModelProperty<int> CurrentMP { get => ActivePlayer.HeroModel.MP; }

		public bool SubmenuActive { get => subCommandList.Enabled; }
	}
}
