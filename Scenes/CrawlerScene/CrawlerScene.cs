using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using AngelPearl.SceneObjects.Shaders;
using AngelPearl.SceneObjects.ViewModels;
using AngelPearl.SceneObjects.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.CrawlerScene
{
    public enum Direction
    {
        None = -1,
        North, East, South, West, Up, Down
    }

    public class CrawlerScene : Scene
    {
		public const int ROOM_LENGTH = 10;



		private const int CRAWLER_VIEWPORT_WIDTH = 306;
		private const int CRAWLER_VIEWPORT_HEIGHT = 176;
		private const int CRAWLER_VIEWPORT_OFFSETX = (CrossPlatformGame.SCREEN_WIDTH - CRAWLER_VIEWPORT_WIDTH) / 2;
		private const int CRAWLER_VIEWPORT_OFFSETY = 54;

		public static CrawlerScene Instance;

        public string LocationName { get; set; } = "Test Map";
        public GameMusic Music { get; set; } = GameMusic.None;

		public MapViewModel MapViewModel { get; private set; }

		public BattleViewModel BattleViewModel { get; private set; }
        public Foe FoeInBattle { get; set; }

		public PartyController PartyController { get; private set; }


        private Floor floor;
        public List<Foe> FoeList { get; set; } = new List<Foe>();
        public List<Chest> ChestList { get; set; } = new List<Chest>();
        public List<Npc> NpcList { get; set; } = new List<Npc>();

        public Panel MapPanel { get; set; }

        private int bumpCooldown;

        public float GlobalBrightness { get; set; } = 0.0f;

		private static Texture2D STATIC_TEXTURE;
        private Effect enemyShader;
		public float[] DESTROY_INTERVAL = new float[5] { 1.1f, 1.1f, 1.1f, 1.1f, 1.1f };
		public float[] FLASH_INTERVAL = new float[5];
		public Vector4[] FLASH_COLOR = new Vector4[5] { new(1.0f, 1.0f, 1.0f, 0.0f), new(1.0f, 1.0f, 1.0f, 0.0f), new(1.0f, 1.0f, 1.0f, 0.0f), new(1.0f, 1.0f, 1.0f, 0.0f), new(1.0f, 1.0f, 1.0f, 0.0f) };



		public CrawlerScene()
        {
            Instance = this;

			//PaletteShader shader = new PaletteShader();
			//shader.SetGlobalBrightness(-1.01f);
			//interfaceShader = InterfacePaletteShader = shader;

			enemyShader = AssetCache.SHADERS[GameShader.BattleEnemy].Clone();
			enemyShader.Parameters["noise"].SetValue(STATIC_TEXTURE);
		}

        public CrawlerScene(GameMap gamemap, int x, int y, Direction dir) : this()
        {
            MapFileName = gamemap;

			floor = new Floor(this, gamemap);
			LocationName = floor.LocationName;

			PartyController = AddController(new PartyController(this, x, y, dir));
		    MapViewModel = AddView(new MapViewModel(this, GameView.Crawler_MapView));
            MapPanel = MapViewModel.GetWidget<Panel>("MapPanel");

			floor.GetRoom(PartyController.RoomX, PartyController.RoomY).EnterRoom(false);
		}

        public CrawlerScene(string iMapName, string spawnName) : this()
        {
            MapFileName = (GameMap)Enum.Parse(typeof(GameMap), iMapName);

            floor = new Floor(this, MapFileName);
            LocationName = floor.LocationName;
            Spawn spawn = floor.Spawns[spawnName];

			PartyController = AddController(new PartyController(this, spawn.RoomX, spawn.RoomY, spawn.Direction));
			MapViewModel = AddView(new MapViewModel(this, GameView.Crawler_MapView));
            MapPanel = MapViewModel.GetWidget<Panel>("MapPanel");

			floor.GetRoom(PartyController.RoomX, PartyController.RoomY).EnterRoom(false);
		}

		public static void Initialize(GraphicsDevice graphicsDevice)
		{
			STATIC_TEXTURE = new Texture2D(graphicsDevice, 160, 160);
			Color[] colorData = new Color[STATIC_TEXTURE.Width * STATIC_TEXTURE.Height];
			for (int y = 0; y < STATIC_TEXTURE.Height; y++)
			{
				for (int x = 0; x < STATIC_TEXTURE.Width; x++)
				{
					colorData[y * STATIC_TEXTURE.Width + x] = new Color(Rng.RandomInt(0, 255), 255, 255, 255);
				}
			}
			STATIC_TEXTURE.SetData<Color>(colorData);
		}

		public void ResetPathfinding()
        {
            PartyController?.Path.Clear();
        }

        public void SaveData()
        {
            GameProfile.SetSaveData<GameMap>("LastMap", MapFileName);
            GameProfile.SetSaveData<int>("LastRoomX", PartyController.RoomX);
            GameProfile.SetSaveData<int>("LastRoomY", PartyController.RoomY);
            GameProfile.SetSaveData<Direction>("LastDirection", PartyController.PartyDirection);
            GameProfile.SetSaveData<string>("PlayerLocation", LocationName);

            GameProfile.SaveState();
        }

        public override void BeginScene()
        {
            sceneStarted = true;

            GlobalBrightness = 0.0f;

            TransitionController transitionController = new TransitionController(TransitionDirection.In, 800);
            transitionController.UpdateTransition += new Action<float>(t =>
            {
                GlobalBrightness = t;
                //((PaletteShader)interfaceShader).SetGlobalBrightness(MathHelper.SmoothStep(-1.01f, 0.0f, t));
            });
			transitionController.FinishTransition += new Action<TransitionDirection>(t => GlobalBrightness = 1.0f);
			AddController(transitionController);

            Audio.PlayMusic(Music);

			OnStart?.Invoke();
		}

        public override void Update(GameTime gameTime)
        {
            if (Input.CurrentInput.CommandPressed(Command.Up) ||
                Input.CurrentInput.CommandPressed(Command.Down) ||
                Input.CurrentInput.CommandPressed(Command.Right) ||
                Input.CurrentInput.CommandPressed(Command.Left))
                ResetPathfinding();

            base.Update(gameTime);

            if (bumpCooldown > 0) bumpCooldown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        /*
        private void InitiateBattle(MapRoom currentRoom, MapRoom destinationRoom)
        {
            TransitionDirection transitionDirection = (direction == Direction.North || direction == Direction.East) ? TransitionDirection.In : TransitionDirection.Out;
            TransitionController transitionController = new TransitionController(transitionDirection, 300, PriorityLevel.CutsceneLevel);

            switch (direction)
            {
                case Direction.North: transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(0, 2.95f, t)); break;
                case Direction.East: transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(0, 2.95f, t)); break;
                case Direction.South: transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(-2.95f, 0, t)); break;
                case Direction.West: transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(-2.95f, 0, t)); break;
            }

            AddController(transitionController);
            transitionController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                switch (direction)
                {
                    case Direction.North: cameraPosZ = 2.95f; break;
                    case Direction.East: cameraPosX = 2.95f; break;
                    case Direction.South: cameraPosZ = -2.95f; break;
                    case Direction.West: cameraPosX = -2.95f; break;
                }

                destinationRoom.Foe.Threaten(direction);
            });
        }
        */

        public MapRoom AttemptMove(Direction moveDirection)
        {
			var currentRoom = floor.GetRoom(PartyController.RoomX, PartyController.RoomY);
			var destinationRoom = currentRoom[moveDirection];
			if (destinationRoom == null || destinationRoom.Blocked || !currentRoom.Neighbors.Contains(destinationRoom))
			{
				if (!Activate(destinationRoom)) WallBump();
				return null;
			}

			if (destinationRoom.PreEnterScript != null)
			{
				destinationRoom.ActivatePreScript();
				destinationRoom.Chest?.Destroy();
				return null;
			}

			if (destinationRoom.Foe != null)
			{
				ResetPathfinding();

                destinationRoom.Foe.Interact();

				return null;
			}
			else
			{
                if (MoveFoes(destinationRoom)) destinationRoom = null;
			}

            return destinationRoom;
		}
                
        private void WallBump()
        {
            if (bumpCooldown <= 0)
            {
                //Audio.PlaySound(GameSound.wall_bump);
                bumpCooldown = 350;
            }
        }

		public override void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderTarget2D pixelRender)
        {
            // Messy 1st person 3D dungeon crawler renderer

            float cameraX = PartyController.CameraX;
			Vector3 cameraUp = new Vector3(0, -1, 0);
			Vector3 cameraPos = new Vector3(PartyController.CameraPosX + ROOM_LENGTH * PartyController.RoomX, 0, PartyController.CameraPosZ + ROOM_LENGTH * (floor.MapHeight - PartyController.RoomY));
			Matrix viewMatrix = Matrix.CreateLookAt(cameraPos, cameraPos + Vector3.Transform(new Vector3(0, 0, 1), Matrix.CreateRotationY(cameraX)), cameraUp);

			graphicsDevice.SetRenderTarget(pixelRender);
			graphicsDevice.Viewport = new Viewport(CRAWLER_VIEWPORT_OFFSETX, CRAWLER_VIEWPORT_OFFSETY, CRAWLER_VIEWPORT_WIDTH, CRAWLER_VIEWPORT_HEIGHT);
			graphicsDevice.Clear(CrossPlatformGame.CLEAR_COLOR);
			graphicsDevice.BlendState = BlendState.Opaque;

			floor.Skybox?.Draw(graphicsDevice, viewMatrix, new Vector3(ROOM_LENGTH * PartyController.RoomX, 0, ROOM_LENGTH * (floor.MapHeight - PartyController.RoomY)));
			floor.DrawMap(graphicsDevice, MapViewModel.GetWidget<Panel>("MapPanel"), viewMatrix, cameraPos);

			graphicsDevice.BlendState = BlendState.AlphaBlend;

            Foe.DeferredSprite = null;
			List<IBillboard> billboardList = [.. FoeList, .. ChestList, .. NpcList];
            foreach (IBillboard billboard in billboardList.OrderByDescending(x => Vector3.Distance(cameraPos, x.Position)))
            {
                billboard.Draw(graphicsDevice, viewMatrix, cameraX);
            }

			graphicsDevice.Viewport = new Viewport(0, 0, CrossPlatformGame.SCREEN_WIDTH, CrossPlatformGame.SCREEN_HEIGHT);

            graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            if (BattleViewModel != null)
            {
				enemyShader.Parameters["destroyInterval"].SetValue(DESTROY_INTERVAL);
				enemyShader.Parameters["flashInterval"].SetValue(FLASH_INTERVAL);
				enemyShader.Parameters["flashColor"].SetValue(FLASH_COLOR);

				spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, enemyShader, null);
				foreach (var enemy in BattleViewModel.EnemyList)
				{
					enemy.Draw(spriteBatch, Camera);
				}
				spriteBatch.End();
			}

			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, null, null);
            DrawOverlay(spriteBatch);

			if (!overlayList.Any(x => x is ConversationViewModel || x is BattleViewModel))
			{
				var miniMapPanel = MapViewModel.GetWidget<Panel>("MiniMapPanel");
				Rectangle miniMapBounds = miniMapPanel.InnerBounds;
				miniMapBounds.X += (int)miniMapPanel.Position.X;
				miniMapBounds.Y += (int)miniMapPanel.Position.Y;
				floor.DrawMiniMap(spriteBatch, miniMapBounds, Color.White, 0.1f, PartyController.RoomX, PartyController.RoomY, PartyController.PartyDirection);
			}

            if (Foe.DeferredSprite != null && BattleViewModel == null && !PartyController.Turning)
			{
				float brightness = PartyController.FacingRoom.AverageBrightness();
				spriteBatch.Draw(Foe.DeferredSprite, new Vector2((CRAWLER_VIEWPORT_WIDTH - Foe.DeferredSprite.Width) / 2 + CRAWLER_VIEWPORT_OFFSETX, CRAWLER_VIEWPORT_OFFSETY + (160 - Foe.DeferredSprite.Height)), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.35f);
			}

			foreach (Particle particle in particleList) particle.Draw(spriteBatch, Camera);

			spriteBatch.End();
		}

		public bool Activate(MapRoom roomAhead)
        {
            if (roomAhead == null) return false;

            return roomAhead.Activate(PartyController.PartyDirection);
        }

        public bool MoveFoes(MapRoom playerDestination)
        {
            foreach (Foe foe in FoeList)
            {
                if (foe.Threaten())
                {
                    return true;
                }
            }

			foreach (Foe foe in FoeList)
            {
                foe.Move(playerDestination);
            }

            return false;
        }

        public void MiniMapClick(Vector2 clickPosition)
        {
            if (PriorityLevel != PriorityLevel.GameLevel || controllerList.Any(x => x.Any(y => y is EventController))) return;

            Panel miniMapPanel = MapViewModel.GetWidget<Panel>("MiniMapPanel");
            int newRoomX = (int)clickPosition.X / Floor.MINI_CELL_SIZE + floor.MinimapStartX;
            int newRoomY = (int)clickPosition.Y / Floor.MINI_CELL_SIZE + floor.MinimapStartY;

            var destinationRoom = floor.GetRoom(newRoomX, newRoomY);
            var currentRoom = floor.GetRoom(PartyController.RoomX, PartyController.RoomY);

            if (destinationRoom != null && !destinationRoom.Blocked)
                {
                    PartyController.Path = floor.GetPath(currentRoom, destinationRoom);
                }

        }

        public void StartBattle(Foe foe)
        {
            var facingRoom = PartyController.FacingRoom;
            if (facingRoom != foe.CurrentRoom)
            {
                PartyController.MoveTo(foe.CurrentRoom);
                PartyController.MoveController.OnTerminated += new TerminationFollowup(() =>
                {
                    StartBattle(foe);
                });

                return;
            }

            FoeInBattle = foe;
			EncounterRecord record = EncounterRecord.ENCOUNTERS.First(x => x.Name == foe.Encounter);
            BattleViewModel = AddView(new BattleViewModel(this, record));
            //BattleViewModel.NewRound();
			MapViewModel.ShowMiniMap.Value = false;
			MapViewModel.ShowInstructions.Value = false;
		}

        public void EndBattle()
        {
            FoeInBattle.Destroy();
            FoeInBattle = null;

            BattleViewModel = null;
            MapViewModel.ShowMiniMap.Value = true;
            MapViewModel.ShowInstructions.Value = true;

            Audio.PlayMusic(Music);
        }

        public GameMap MapFileName { get; set; }

        public Floor Floor { get => floor; }
    }
}
