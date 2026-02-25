using System.Collections.Generic;
using System.Linq;

using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using System;

using Microsoft.Xna.Framework;
using System.Drawing;

namespace AngelPearl.Scenes.CrawlerScene
{
    public class PartyController : Controller
    {
        private CrawlerScene crawlerScene;

		// Position and Heading for navigating TileMap corridor mazes
		public int RoomX { get; private set; } = -1;
		public int RoomY { get; private set; } = -1;
		public MapRoom CurrentRoom { get => crawlerScene.Floor.GetRoom(RoomX, RoomY); }

		public MapRoom FacingRoom { get => CurrentRoom[PartyDirection]; }

		public Direction PartyDirection { get; private set; }

		// Position and Heading for 3D camera rendering
		public float CameraX { get; private set; } = 0.0f;
		public float CameraPosX { get; private set; } = 0.0f;
		public float CameraPosZ { get; private set; } = 0.0f;
		public float BillboardRotation { get; private set; }

		public TransitionController MoveController { get; private set; }
		
		public bool Moving { get => MoveController != null; }
		public bool Turning { get; private set; }
		public float MoveInterval { get => MoveController.TransitionProgress; }


		public PartyController(CrawlerScene iScene, int x, int y, Direction dir)
			: base(PriorityLevel.GameLevel)
        {
            crawlerScene = iScene;

			RoomX = x;
			RoomY = y;
			PartyDirection = dir;
			CameraX = (float)(Math.PI * (int)PartyDirection / 2.0f);
		}

        public override void PreUpdate(GameTime gameTime)
        {
            if (crawlerScene.FoeList.Any(x => x.IsMoving)) return;

            InputFrame inputFrame = Input.CurrentInput;
            if (inputFrame.CommandDown(Command.LookLeft)) { Path.Clear(); TurnLeft(); }
            else if (inputFrame.CommandDown(Command.LookRight)) { Path.Clear(); TurnRight(); }
			else if (inputFrame.CommandDown(Command.Left)) { Path.Clear(); MoveLeft(); }
			else if (inputFrame.CommandDown(Command.Right)) { Path.Clear(); MoveRight(); }
			else if (inputFrame.CommandDown(Command.Up)) { Path.Clear(); MoveForward(); }
			else if (inputFrame.CommandDown(Command.Down)) { Path.Clear(); MoveBackward(); }
			else if (Input.CurrentInput.CommandPressed(Command.Confirm))
			{
				crawlerScene.MapViewModel.InteractLabel.Value = "";
				crawlerScene.MapViewModel.ShowInteractLabel.Value = false;
				Path.Clear();
				crawlerScene.Activate(FacingRoom);
			}
            else if (Input.CurrentInput.CommandPressed(Command.Menu))
            {
                Path.Clear();


                    //StatusScene.StatusScene statusScene = new StatusScene.StatusScene(mapScene.LocationName, true);
                    //CrossPlatformGame.StackScene(statusScene, true);

                //mapScene.AddView(new MenuViewModel(mapScene));

                /*

                Controller suspendController = mapScene.AddController(new Controller(PriorityLevel.MenuLevel));

                StatusScene.StatusScene statusScene = new StatusScene.StatusScene();
                statusScene.OnTerminated += new TerminationFollowup(suspendController.Terminate);
                CrossPlatformGame.StackScene(statusScene);

                */

                return;
            }
            else if (Path.Count > 0)
            {
                MapRoom nextRoom = Path.First();
                if (RoomX == nextRoom.RoomX && RoomY == nextRoom.RoomY) Path.RemoveAt(0);
                else MoveTo(nextRoom);
            }
        }

		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);

			CheckForInteractionPrompts();
		}

		public void MoveTo(MapRoom destinationRoom)
		{
			Direction requiredDirection;
			if (destinationRoom.RoomX > RoomX) requiredDirection = Direction.East;
			else if (destinationRoom.RoomX < RoomX) requiredDirection = Direction.West;
			else if (destinationRoom.RoomY > RoomY) requiredDirection = Direction.South;
			else requiredDirection = Direction.North;

			if (requiredDirection == PartyDirection) MoveForward();
			else
			{
				if (requiredDirection == PartyDirection + 1 || (requiredDirection == Direction.North && PartyDirection == Direction.West)) TurnRight();
				else TurnLeft();
			}
		}

		public void TurnLeft()
		{
			Turning = true;

			MoveController = new TransitionController(TransitionDirection.Out, 300, PriorityLevel.TransitionLevel);
			crawlerScene.AddController(MoveController);

			MoveController.UpdateTransition += new Action<float>(t =>
			{
				CameraX = MathHelper.Lerp((float)(Math.PI * ((int)PartyDirection - 1) / 2.0f), (float)(Math.PI * (int)PartyDirection / 2.0f), t <= 0.5f ? 0.0f : 0.5f);
				if (t <= 0.5f)
				{
					var dir = (PartyDirection == Direction.North) ? Direction.West : PartyDirection - 1;
					BillboardRotation = (float)(Math.PI * (int)dir / 2.0f);
				}
			});

			MoveController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				if (PartyDirection == Direction.North) PartyDirection = Direction.West; else PartyDirection--;
				CameraX = (float)(Math.PI * (int)PartyDirection / 2.0f);

                var currentRoom = crawlerScene.Floor.GetRoom(RoomX, RoomY);
				crawlerScene.Floor.ResetFOV(currentRoom, PartyDirection);

				crawlerScene.AddController(new SkippableWaitController(PriorityLevel.CutsceneLevel, null, false, 250));

				MoveController = null;
				Turning = false;
			});
		}

		public void TurnRight()
		{
			Turning = true;

			MoveController = new TransitionController(TransitionDirection.In, 300, PriorityLevel.TransitionLevel);
			crawlerScene.AddController(MoveController);

			MoveController.UpdateTransition += new Action<float>(t =>
			{
				CameraX = MathHelper.Lerp(((float)(Math.PI * (int)PartyDirection / 2.0f)), (float)(Math.PI * ((int)PartyDirection + 1) / 2.0f), t >= 0.5f ? 1.0f : 0.5f);
				if (t >= 0.5f)
				{
					var dir = (PartyDirection == Direction.West) ? Direction.North : PartyDirection + 1;
					BillboardRotation = (float)(Math.PI * (int)dir / 2.0f);
				}
			});

			MoveController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				if (PartyDirection == Direction.West) PartyDirection = Direction.North; else PartyDirection++;
				CameraX = (float)(Math.PI * (int)PartyDirection / 2.0f);

				var currentRoom = crawlerScene.Floor.GetRoom(RoomX, RoomY);
				crawlerScene.Floor.ResetFOV(currentRoom, PartyDirection);

				crawlerScene.AddController(new SkippableWaitController(PriorityLevel.CutsceneLevel, null, false, 250));

                MoveController = null;
				Turning = false;
            });
        }

		public void MoveForward()
		{
			var destinationRoom = crawlerScene.AttemptMove(PartyDirection);
			if (destinationRoom == null) return;

			TransitionDirection transitionDirection = (PartyDirection == Direction.North || PartyDirection == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
			MoveController = new TransitionController(transitionDirection, 300, PriorityLevel.CutsceneLevel);

			switch (PartyDirection)
			{
				case Direction.North: MoveController.UpdateTransition += new Action<float>(t => CameraPosZ = t >= 0.5f ? CrawlerScene.ROOM_LENGTH : CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.East: MoveController.UpdateTransition += new Action<float>(t => CameraPosX = t >= 0.5f ? CrawlerScene.ROOM_LENGTH : CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.South: MoveController.UpdateTransition += new Action<float>(t => CameraPosZ = t < 0.5f ? -CrawlerScene.ROOM_LENGTH : -CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.West: MoveController.UpdateTransition += new Action<float>(t => CameraPosX = t < 0.5f ? -CrawlerScene.ROOM_LENGTH : -CrawlerScene.ROOM_LENGTH / 2); break;
			}

			crawlerScene.AddController(MoveController);
			MoveController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				CameraPosX = CameraPosZ = 0;
				RoomX = destinationRoom.RoomX;
				RoomY = destinationRoom.RoomY;

                if (crawlerScene.FoeInBattle == null)
				{
                    destinationRoom.EnterRoom();
                    if (!Input.CurrentInput.CommandDown(Command.Up))
					{
						crawlerScene.AddController(new SkippableWaitController(PriorityLevel.CutsceneLevel, null, false, 250));
					}
				}
				else
				{
					crawlerScene.StartBattle(crawlerScene.FoeInBattle);
                    destinationRoom.EnterRoom();
                }

				MoveController = null;
            });

			crawlerScene.Floor.ResetFOV(destinationRoom, PartyDirection, false);

			DestinationRoom = destinationRoom;
		}

		public MapRoom DestinationRoom { get; private set; }

		public void MoveLeft()
		{
			Direction moveDirection = PartyDirection;
			switch (PartyDirection)
			{
				case Direction.West: moveDirection = Direction.South; break;
				case Direction.North: moveDirection = Direction.West; break;
				case Direction.East: moveDirection = Direction.North; break;
				case Direction.South: moveDirection = Direction.East; break;
			}

			var destinationRoom = crawlerScene.AttemptMove(moveDirection);
			if (destinationRoom == null) return;

			TransitionDirection transitionDirection = (moveDirection == Direction.North || moveDirection == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
			MoveController = new TransitionController(transitionDirection, 300, PriorityLevel.CutsceneLevel);

			switch (moveDirection)
			{
				case Direction.North: MoveController.UpdateTransition += new Action<float>(t => CameraPosZ = t >= 0.5f ? CrawlerScene.ROOM_LENGTH : CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.East: MoveController.UpdateTransition += new Action<float>(t => CameraPosX = t >= 0.5f ? CrawlerScene.ROOM_LENGTH : CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.South: MoveController.UpdateTransition += new Action<float>(t => CameraPosZ = t < 0.5f ? -CrawlerScene.ROOM_LENGTH : -CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.West: MoveController.UpdateTransition += new Action<float>(t => CameraPosX = t < 0.5f ? -CrawlerScene.ROOM_LENGTH : -CrawlerScene.ROOM_LENGTH / 2); break;
			}

			crawlerScene.AddController(MoveController);
			MoveController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				CameraPosX = CameraPosZ = 0;
				RoomX = destinationRoom.RoomX;
				RoomY = destinationRoom.RoomY;

                if (crawlerScene.FoeInBattle == null)
                {
                    destinationRoom.EnterRoom();
                    if (!Input.CurrentInput.CommandDown(Command.Left))
                    {
                        crawlerScene.AddController(new SkippableWaitController(PriorityLevel.CutsceneLevel, null, false, 250));
                    }
                }
                else
                {
                    crawlerScene.StartBattle(crawlerScene.FoeInBattle);
                    destinationRoom.EnterRoom();
                }

                MoveController = null;
            });

			crawlerScene.Floor.ResetFOV(destinationRoom, PartyDirection, false);

			DestinationRoom = destinationRoom;
		}

		public void MoveRight()
		{
			Direction moveDirection = PartyDirection;
			switch (PartyDirection)
			{
				case Direction.West: moveDirection = Direction.North; break;
				case Direction.North: moveDirection = Direction.East; break;
				case Direction.East: moveDirection = Direction.South; break;
				case Direction.South: moveDirection = Direction.West; break;
			}

			var destinationRoom = crawlerScene.AttemptMove(moveDirection);
			if (destinationRoom == null) return;

			TransitionDirection transitionDirection = (moveDirection == Direction.North || moveDirection == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
			MoveController = new TransitionController(transitionDirection, 300, PriorityLevel.CutsceneLevel);

			switch (moveDirection)
			{
				case Direction.North: MoveController.UpdateTransition += new Action<float>(t => CameraPosZ = t >= 0.5f ? CrawlerScene.ROOM_LENGTH : CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.East: MoveController.UpdateTransition += new Action<float>(t => CameraPosX = t >= 0.5f ? CrawlerScene.ROOM_LENGTH : CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.South: MoveController.UpdateTransition += new Action<float>(t => CameraPosZ = t < 0.5f ? -CrawlerScene.ROOM_LENGTH : -CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.West: MoveController.UpdateTransition += new Action<float>(t => CameraPosX = t < 0.5f ? -CrawlerScene.ROOM_LENGTH : -CrawlerScene.ROOM_LENGTH / 2); break;
			}

			crawlerScene.AddController(MoveController);
			MoveController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				CameraPosX = CameraPosZ = 0;
				RoomX = destinationRoom.RoomX;
				RoomY = destinationRoom.RoomY;

                if (crawlerScene.FoeInBattle == null)
                {
                    destinationRoom.EnterRoom();
                    if (!Input.CurrentInput.CommandDown(Command.Right))
                    {
                        crawlerScene.AddController(new SkippableWaitController(PriorityLevel.CutsceneLevel, null, false, 250));
                    }
                }
                else
                {
                    crawlerScene.StartBattle(crawlerScene.FoeInBattle);
                    destinationRoom.EnterRoom();
                }

                MoveController = null;
            });

			crawlerScene.Floor.ResetFOV(destinationRoom, PartyDirection, false);

			DestinationRoom = destinationRoom;
		}

		public void MoveBackward()
		{
			Direction moveDirection = PartyDirection + 2;
			if (moveDirection > Direction.West) moveDirection -= 4;

			var destinationRoom = crawlerScene.AttemptMove(moveDirection);
			if (destinationRoom == null) return;

			TransitionDirection transitionDirection = (moveDirection == Direction.North || moveDirection == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
			MoveController = new TransitionController(transitionDirection, 300, PriorityLevel.CutsceneLevel);

			switch (moveDirection)
			{
				case Direction.North: MoveController.UpdateTransition += new Action<float>(t => CameraPosZ = t >= 0.5f ? CrawlerScene.ROOM_LENGTH : CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.East: MoveController.UpdateTransition += new Action<float>(t => CameraPosX = t >= 0.5f ? CrawlerScene.ROOM_LENGTH : CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.South: MoveController.UpdateTransition += new Action<float>(t => CameraPosZ = t < 0.5f ? -CrawlerScene.ROOM_LENGTH : -CrawlerScene.ROOM_LENGTH / 2); break;
				case Direction.West: MoveController.UpdateTransition += new Action<float>(t => CameraPosX = t < 0.5f ? -CrawlerScene.ROOM_LENGTH : -CrawlerScene.ROOM_LENGTH / 2); break;
			}

			crawlerScene.AddController(MoveController);
			MoveController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				CameraPosX = CameraPosZ = 0;
				RoomX = destinationRoom.RoomX;
				RoomY = destinationRoom.RoomY;

                if (crawlerScene.FoeInBattle == null)
                {
                    destinationRoom.EnterRoom();
                    if (!Input.CurrentInput.CommandDown(Command.Down))
                    {
                        crawlerScene.AddController(new SkippableWaitController(PriorityLevel.CutsceneLevel, null, false, 250));
                    }
                }
                else
                {
                    crawlerScene.StartBattle(crawlerScene.FoeInBattle);
                    destinationRoom.EnterRoom();
                }

                MoveController = null;
            });

			crawlerScene.Floor.ResetFOV(destinationRoom, PartyDirection, false);

			DestinationRoom = destinationRoom;
		}

		public void CheckForInteractionPrompts()
		{
			var nextRoom = crawlerScene.Floor.GetRoom(RoomX, RoomY)[PartyDirection];
			if (Moving) crawlerScene.MapViewModel.InteractLabel.Value = "";
			else if(nextRoom == null) crawlerScene.MapViewModel.InteractLabel.Value = "";
			else if (crawlerScene.Suspended) crawlerScene.MapViewModel.InteractLabel.Value = "";
			else if (crawlerScene.PriorityLevel != PriorityLevel.GameLevel) crawlerScene.MapViewModel.InteractLabel.Value = "";
			else if (nextRoom.Foe != null && !string.IsNullOrEmpty(nextRoom.Foe.Label))
			{
				crawlerScene.MapViewModel.InteractLabel.Value = nextRoom.Foe.Label;
				crawlerScene.MapViewModel.InteractBounds.Value = new Microsoft.Xna.Framework.Rectangle(0, -44, Text.GetStringLength(GameFont.Console, crawlerScene.MapViewModel.InteractLabel.Value) + 16, 19);
			}
			else if (!string.IsNullOrEmpty(nextRoom.Label))
			{
				crawlerScene.MapViewModel.InteractLabel.Value = nextRoom.Label;
				crawlerScene.MapViewModel.InteractBounds.Value = new Microsoft.Xna.Framework.Rectangle(0, -20, Text.GetStringLength(GameFont.Console, crawlerScene.MapViewModel.InteractLabel.Value) + 16, 19);
			}
			else if (nextRoom.Chest != null)
			{
				crawlerScene.MapViewModel.InteractLabel.Value = "Open";
				crawlerScene.MapViewModel.InteractBounds.Value = new Microsoft.Xna.Framework.Rectangle(0, -16, Text.GetStringLength(GameFont.Console, crawlerScene.MapViewModel.InteractLabel.Value) + 16, 19);
			}
			else
			{
				crawlerScene.MapViewModel.InteractLabel.Value = "";
			}

			crawlerScene.MapViewModel.ShowInteractLabel.Value = !string.IsNullOrEmpty(crawlerScene.MapViewModel.InteractLabel.Value);
			crawlerScene.MapViewModel.InteractButton.ApplyAlignment();
			foreach (var child in crawlerScene.MapViewModel.InteractButton.ChildList) child.ApplyAlignment();
		}

		public List<MapRoom> Path { get; set; } = new List<MapRoom>();
    }
}
