using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using AngelPearl.SceneObjects.Widgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.CrawlerScene
{
    public class MapViewModel : ViewModel
    {
        private CrawlerScene crawlerScene;

        public Button InteractButton { get; private set; }

        public MapViewModel(CrawlerScene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel)
        {
            crawlerScene = iScene;

            if (Text.GetStringLength(GameFont.Console, crawlerScene.Floor.LocationName) >= 92)
            {
                var tokens = new Queue<string>(crawlerScene.Floor.LocationName.Split(' '));
                MiniMapHeader1.Value += $" {tokens.Dequeue()}";
                while (Math.Abs(Text.GetStringLength(GameFont.Console, MiniMapHeader1.Value) - Text.GetStringLength(GameFont.Console, string.Join(' ', tokens))) >
                       Math.Abs(Text.GetStringLength(GameFont.Console, MiniMapHeader1.Value + $" {tokens.Peek()}") - Text.GetStringLength(GameFont.Console, string.Join(' ', tokens.Skip(1)))))
                {
                    MiniMapHeader1.Value += $" {tokens.Dequeue()}";
                }
                MiniMapHeader2.Value = string.Join(' ', tokens);
            }
            else MiniMapHeader1.Value = crawlerScene.Floor.LocationName;

            LoadView(GameView.Crawler_MapView);

			InteractButton = GetWidget<Button>("InteractLabel");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


        }

        public override void LeftClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget)
        {
            switch (clickWidget.Name)
            {
                case "MiniMapPanel":
                    crawlerScene.PartyController.Path.Clear();
                    crawlerScene.MiniMapClick(mouseEnd - clickWidget.AbsolutePosition);
                    break;
            }
        }

        public void ActivatePrompt()
        {
			crawlerScene.Activate(crawlerScene.PartyController.FacingRoom);
		}

        public void MoveForward()
        {
            if (crawlerScene.FoeList.Any(x => x.IsMoving)) return;
            crawlerScene.PartyController.Path.Clear();
            crawlerScene.PartyController.MoveForward();
        }

        public void MoveBackward()
        {
            if (crawlerScene.FoeList.Any(x => x.IsMoving)) return;
            crawlerScene.PartyController.Path.Clear();
            crawlerScene.PartyController.MoveBackward();
        }

		public void StrafeLeft()
		{
			if (crawlerScene.FoeList.Any(x => x.IsMoving)) return;
			crawlerScene.PartyController.Path.Clear();
			crawlerScene.PartyController.MoveLeft();
		}

		public void StrafeRight()
		{
			if (crawlerScene.FoeList.Any(x => x.IsMoving)) return;
			crawlerScene.PartyController.Path.Clear();
			crawlerScene.PartyController.MoveRight();
		}

		public void TurnLeft()
		{
			if (crawlerScene.FoeList.Any(x => x.IsMoving)) return;
			crawlerScene.PartyController.Path.Clear();
			crawlerScene.PartyController.TurnLeft();
		}

		public void TurnRight()
		{
			if (crawlerScene.FoeList.Any(x => x.IsMoving)) return;
			crawlerScene.PartyController.Path.Clear();
			crawlerScene.PartyController.TurnRight();
		}

        public ModelProperty<string> MiniMapHeader1 { get; set; } = new ModelProperty<string>("");
		public ModelProperty<string> MiniMapHeader2 { get; set; } = new ModelProperty<string>("");

		public ModelProperty<Color> MapColor { get; set; } = new ModelProperty<Color>(Color.White);

		public ModelProperty<bool> ShowPlayerBar { get; set; } = new ModelProperty<bool>(true);

		public ModelProperty<bool> ShowInstructions { get; set; } = new ModelProperty<bool>(true);

		public ModelProperty<bool> ShowMiniMap { get; set; } = new ModelProperty<bool>(true);

		public ModelProperty<bool> ShowInteractLabel { get; set; } = new ModelProperty<bool>(false);
		public ModelProperty<string> InteractLabel { get; set; } = new ModelProperty<string>("");
		public ModelProperty<Rectangle> InteractBounds { get; set; } = new ModelProperty<Rectangle>(new Rectangle());
	}
}
