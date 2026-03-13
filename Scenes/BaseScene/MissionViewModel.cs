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
    public class MissionViewModel : ViewModel
    {
        private enum SubMenu
        {
            Main
		}

        private BaseScene mapScene;

        private BaseViewModel townViewModel;

        public RadioBox HireBox { get; set; }

        private bool confirmCooldown = false;

		public MissionViewModel(BaseScene iScene, BaseViewModel iTownViewModel, string[] iHeroes)
            : base(iScene, PriorityLevel.CutsceneLevel)
        {
            mapScene = iScene;
            townViewModel = iTownViewModel;

            DrawOnNewLayer = true;

            /*
			var availableHeroes = HeroRecord.HEROES.Where(x => !GameProfile.GetSaveData<bool>($"Recruited{x.Name}") && iHeroes.Any(y => y == x.Name));
            AvailableHeroes.AddRange(availableHeroes);

            if (AvailableHeroes.Count == 0)
            {
                townViewModel.Narration.Value = "No heroes are available for hire right now.";
            }
            else townViewModel.Narration.Value = AvailableHeroes[0].Description;
            */

			LoadView(GameView.HomeBase_MissionView);


            // HireBox = GetWidget<RadioBox>("HireBox");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			if (Input.CurrentInput.CommandPressed(Command.Cancel))
			{
                Terminate();
			}
            else if (Input.CurrentInput.CommandPressed(Command.Confirm) && confirmCooldown)
            {
                StartMission();
            }

            confirmCooldown = true;
        }

        public void SelectCommand(object parameter)
        {
            /*
			HeroRecord heroRecord;
            if (parameter is IModelProperty)
            {
                heroRecord = (HeroRecord)((IModelProperty)parameter).GetValue();
            }
            else heroRecord = (HeroRecord)parameter;

            long cost = long.Parse(heroRecord.Cost);
            if (GameProfile.CurrentSave.Money.Value < cost)
            {
                townViewModel.Narration.Value = "You cannot afford to hire this hero.";
                return;
            }
            else GameProfile.CurrentSave.Money.Value = GameProfile.CurrentSave.Money.Value - cost;

            HireBox.ChildList[HireBox.Selection].Enabled = false;

            GameProfile.SetSaveData($"Recruited{heroRecord.Name}", true);

            HeroModel model = new HeroModel(heroRecord);
            if (GameProfile.CurrentSave.Party.Count < 4)
            {
                GameProfile.CurrentSave.Party.Add(model);
                GetWidget<DataGrid>("PartyBox").ItemsChanged();

                townViewModel.Narration.Value = $"{heroRecord.Name} joined your active party.";
            }
            else
            {
                GameProfile.CurrentSave.BackBench.Add(model);
                GetWidget<DataGrid>("RosterBox").ItemsChanged();

                townViewModel.Narration.Value = $"{heroRecord.Name} joined and is standing by onboard the ship.";
            }
            */
		}

        public void NewSelection(object parameter)
        {
            //if (HireBox.ChildList[HireBox.Selection].Enabled)
            //    townViewModel.Narration.Value = AvailableHeroes[HireBox.Selection].Description;
            //else townViewModel.Narration.Value = "This hero has just been recruited.";
        }

        //public ModelCollection<HeroRecord> AvailableHeroes { get; set; } = new ModelCollection<HeroRecord>();


        public void StartMission()
        {
            GameProfile.CurrentSave.Party.Clear();
            foreach (var maho in GameProfile.CurrentSave.Roster.Take(4))
            {
                GameProfile.CurrentSave.Party.Add(maho.Value);
            }

            CrossPlatformGame.SetCurrentScene(new CrawlerScene.CrawlerScene(GameMap.TestAngel, 6, 13, Direction.North));
        }
    }
}
