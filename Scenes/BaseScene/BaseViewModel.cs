using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using AngelPearl.SceneObjects.Maps;
using AngelPearl.SceneObjects.Widgets;
using AngelPearl.Scenes.CrawlerScene;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.BaseScene
{
    public class BaseViewModel : ViewModel
    {
        private enum SubMenu
        {
            Main,

            Shipyard,
			Marketplace,
            Tavern,

            BuyCargo,
            SellCargo,
            UpgradeShip,

            BuyEquipment,
            SellEquipment,
            OutfitHero,
            
            Mission
		}

        public delegate void NarrationFinished();

        private BaseScene mapScene;

        private MissionRecord missionRecord;

        public ViewModel ChildViewModel { get; set; }

        public RadioBox CommandBox { get; private set; }
		public CrawlText NarrationText { get; private set; }

        public event NarrationFinished OnFinishNarration;

        private SubMenu CurrentMenu { get; set; } = SubMenu.Main;

		public BaseViewModel(BaseScene iScene, MissionRecord iMissionRecord, Vector2 angelPosition)
            : base(iScene, PriorityLevel.CutsceneLevel)
        {
            mapScene = iScene;
            missionRecord = iMissionRecord;

            Header.Value = missionRecord.Name;

            int headerWidth = Text.GetStringLength(GameFont.Console, Header.Value);

			AngelHeaderBounds.Value = new Rectangle((int)angelPosition.X - (headerWidth / 2), (int)angelPosition.Y - 32, headerWidth, 20);

            Narration.Value = missionRecord.Description;

            AvailableCommands.Add("Prepare for Mission");
            AvailableCommands.Add("Modify Cosmo Engine");
			AvailableCommands.Add("Visit the Muse Lounge");

            DaysLeft.Value = iMissionRecord.TimeLimit - GameProfile.CurrentSave.DayOfYear.Value - iMissionRecord.MissionStart;

			LoadView(GameView.HomeBase_BaseView);

			CommandBox = GetWidget<RadioBox>("CommandBox");
			NarrationText = GetWidget<CrawlText>("NarrationText");

            OnFinishNarration += new NarrationFinished(() => CommandBox.Visible = true);
        }

        public override void Update(GameTime gameTime)
        {
            bool oldProceed = NarrationText.ReadyToProceed;

            base.Update(gameTime);

            if (NarrationText.ReadyToProceed && !oldProceed)
            {
                OnFinishNarration?.Invoke();
                OnFinishNarration = null;
            }

            if (ChildViewModel != null)
            {
                if (ChildViewModel.Terminated)
                {
                    if (ChildViewModel is MissionViewModel)
                    {
						CurrentMenu = SubMenu.Tavern;
						CommandBox.Visible = false;
						CommandBounds.Value = new Rectangle(0, 0, 100, 37);
						var newCommands = new List<ModelProperty<string>>
						{
							new("Recruit Hero"),
							new("Hear Rumors"),
							new("Rest Overnight"),
						};
						AvailableCommands.ModelList = newCommands;
						CommandBox.Selection = 0; (CommandBox.ChildList[0] as RadioButton)?.RadioSelect();
						//Narration.Value = townRecord.TavernNarration;
						OnFinishNarration += new NarrationFinished(() => CommandBox.Visible = true);
					}

                    ChildViewModel = null;
                }
                return;
            }

			if (Input.CurrentInput.CommandPressed(Command.Cancel) && CommandBox.Visible)
			{
				switch (CurrentMenu)
				{
                    case SubMenu.Shipyard:
                    case SubMenu.Tavern:
                    case SubMenu.Marketplace:
                        {
                            CurrentMenu = SubMenu.Main;
                            var newCommands = new List<ModelProperty<string>>
                            {
                                new("Shipyard"),
                                new("Marketplace"),
                                new("Tavern"),
                                new("Depart (ground)")
                            };
                            if (false)
                            {
                                newCommands.Add(new("Depart (ship)"));
								CommandBounds.Value = new Rectangle(0, 0, 100, 55);
							}
                            else CommandBounds.Value = new Rectangle(0, 0, 100, 46);
							AvailableCommands.ModelList = newCommands;
                            CommandBox.Selection = 0; (CommandBox.ChildList[0] as RadioButton)?.RadioSelect();
							CommandBox.Visible = false;
							//Narration.Value = townRecord.MainNarration;
                            OnFinishNarration += new NarrationFinished(() => CommandBox.Visible = true);
                        }
                        break;

                    case SubMenu.Mission:
						{
							CurrentMenu = SubMenu.Tavern;
							CommandBox.Visible = false;
							CommandBounds.Value = new Rectangle(0, 0, 100, 37);
							var newCommands = new List<ModelProperty<string>>
						    {
							    new("Recruit Hero"),
							    new("Hear Rumors"),
							    new("Rest Overnight"),
						    };
							AvailableCommands.ModelList = newCommands;
							CommandBox.Selection = 0; (CommandBox.ChildList[0] as RadioButton)?.RadioSelect();
							//Narration.Value = townRecord.TavernNarration;
							OnFinishNarration += new NarrationFinished(() => CommandBox.Visible = true);
						}
						break;
				}
			}
		}

        public void SelectCommand(object parameter)
        {
            string menuCommand;
            if (parameter is IModelProperty)
            {
                menuCommand = (string)((IModelProperty)parameter).GetValue();
            }
            else menuCommand = (string)parameter;

            switch (menuCommand)
            {
				case "Shipyard":
					{
						CurrentMenu = SubMenu.Shipyard;
						CommandBox.Visible = false;
						CommandBounds.Value = new Rectangle(0, 0, 100, 37);
						var newCommands = new List<ModelProperty<string>>
						{
							new("Buy Cargo"),
							new("Sell Cargo"),
							new("Upgrade Ship"),
						};
						AvailableCommands.ModelList = newCommands;
						CommandBox.Selection = 0; (CommandBox.ChildList[0] as RadioButton)?.RadioSelect();
						//Narration.Value = townRecord.ShipNarration;
						OnFinishNarration += new NarrationFinished(() => CommandBox.Visible = true);
					}
					break;

				case "Marketplace":
                    {
                        CurrentMenu = SubMenu.Marketplace;
                        CommandBox.Visible = false;
                        CommandBounds.Value = new Rectangle(0, 0, 100, 37);
                        var newCommands = new List<ModelProperty<string>>
                        {
                            new("Buy Equipment"),
                            new("Sell Equipment"),
                            new("Outfit Hero"),
                        };
                        AvailableCommands.ModelList = newCommands;
                        CommandBox.Selection = 0; (CommandBox.ChildList[0] as RadioButton)?.RadioSelect();
                        //Narration.Value = townRecord.ShopNarration;
                        OnFinishNarration += new NarrationFinished(() => CommandBox.Visible = true);
                    }
					break;

				case "Tavern":
					{
						CurrentMenu = SubMenu.Tavern;
						CommandBox.Visible = false;
						CommandBounds.Value = new Rectangle(0, 0, 100, 37);
						var newCommands = new List<ModelProperty<string>>
						{
							new("Recruit Hero"),
							new("Hear Rumors"),
							new("Rest Overnight"),
						};
						AvailableCommands.ModelList = newCommands;
						CommandBox.Selection = 0; (CommandBox.ChildList[0] as RadioButton)?.RadioSelect();
						//Narration.Value = townRecord.TavernNarration;
						OnFinishNarration += new NarrationFinished(() => CommandBox.Visible = true);
					}
					break;

                case "Depart (ground)":
				case "Depart (ship)":
					this.Terminate();
                    break;

                case "Prepare for Mission":
                    {
						CurrentMenu = SubMenu.Mission;
						CommandBox.Visible = false;
						Header.Value = "Assemble Squad";

						ChildViewModel = mapScene.AddView(new MissionViewModel(mapScene, this, []));
					}
                    break;

			}
        }


		public ModelProperty<Rectangle> AngelHeaderBounds { get; set; } = new ModelProperty<Rectangle>();
		public ModelProperty<string> Header { get; set; } = new ModelProperty<string>();
        public ModelProperty<string> Narration { get; set; } = new ModelProperty<string>();

        public ModelCollection<string> AvailableCommands { get; set; } = new ModelCollection<string>();
        public ModelProperty<Rectangle> CommandBounds { get; set; } = new ModelProperty<Rectangle>(new Rectangle(0, 0, 100, 55));

		public ModelCollection<string> AvailableItems { get; set; } = new ModelCollection<string>();

		public ModelProperty<int> DaysLeft { get; set; } = new ModelProperty<int>(1);
	}
}
