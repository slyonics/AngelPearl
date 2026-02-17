using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using AngelPearl.SceneObjects.Shaders;
using AngelPearl.Scenes.MapScene;

namespace AngelPearl.Main
{
	public partial class CrossPlatformGame : Game
	{
		public const string GAME_NAME = "AngelPearl";
		public const int SCREEN_WIDTH = 640;
		public const int SCREEN_HEIGHT = 360;

		public static readonly Color CLEAR_COLOR = Main.Graphics.PURE_BLACK;

		private GraphicsDeviceManager graphicsDeviceManager;
		private SpriteBatch spriteBatch;
		private RenderTarget2D gameRender;

		private static Scene pendingScene;

		public static Scene CurrentScene { get; private set; }
		public static List<Scene> SceneStack { get; } = new List<Scene>();
		public static Shader TransitionShader { get; set; }
		public static Vector2 ScreenShake { get; set; }

		public static GraphicsDevice Graphics { get => gameInstance.GraphicsDevice; }
		public static ContentManager ContentManager { get => gameInstance.Content; }
		public static CrossPlatformGame GameInstance { get => gameInstance; }

		private static int scaledScreenWidth = SCREEN_WIDTH;
		private static int scaledScreenHeight = SCREEN_HEIGHT;

		public static int ScreenScale = 2;

        private static CrossPlatformGame gameInstance;

		public CrossPlatformGame()
		{
			gameInstance = this;
			
			graphicsDeviceManager = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = scaledScreenWidth * ScreenScale,
				PreferredBackBufferHeight = scaledScreenHeight * ScreenScale,
				GraphicsProfile = GraphicsProfile.HiDef
			};

			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			scaledScreenWidth *= ScreenScale;
			scaledScreenHeight *= ScreenScale;

			graphicsDeviceManager.PreferredBackBufferWidth = scaledScreenWidth;
			graphicsDeviceManager.PreferredBackBufferHeight = scaledScreenHeight;
			graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;


			// AssetCache.LoadAssets(Content);

			Settings.DEFAULT_PROGRAM_SETTINGS.Add("Fullscreen", false);
			Settings.DEFAULT_PROGRAM_SETTINGS.Add("TargetResolution", "Best Fit");
			Settings.LoadSettings();

			Input.Initialize();

			base.Initialize();

			bool fullscreen = Settings.GetProgramSetting<bool>("Fullscreen");
			//originalHeight = GraphicsDevice.Adapter.CurrentDisplayMode.TitleSafeArea.Height;

			//ApplySettings();
			Audio.ApplySettings();

			spriteBatch = new SpriteBatch(GraphicsDevice);
			gameRender = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, SCREEN_WIDTH, SCREEN_HEIGHT, false, SurfaceFormat.Color, DepthFormat.Depth24);

			IsMouseVisible = true;

			StartGame();
		}

		protected override void LoadContent()
		{
			Debug.Initialize(GraphicsDevice);
		}

		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
			gameRender?.Dispose();
		}

		private void StartGame()
		{
			//var mapScene = new MapScene(GameMap.Segments, "Default");
			CurrentScene = new Scenes.LoadingScene.LoadingScene();
			CurrentScene.BeginScene();
		}

		protected override void Update(GameTime gameTime)
		{
			if (CurrentScene == null) return;

			Input.Update(gameTime);

			if (TransitionShader != null)
			{
				CurrentScene.Update(gameTime);
				TransitionShader.Update(gameTime, null);
				if (TransitionShader.Terminated) TransitionShader = null;
			}
			else
			{
				int i = 0;
				while (i < SceneStack.Count)
				{
					SceneStack[i].Update(gameTime);
					i++;
				}

				CurrentScene.Update(gameTime);
			}

			if (pendingScene != null)
			{
				SceneStack.Clear();
				SetCurrentScene(pendingScene);
				pendingScene = null;
			}

			while (CurrentScene.SceneEnded && SceneStack.Count > 0)
			{
				var previousScene = CurrentScene;

				CurrentScene = SceneStack.Last();
				CurrentScene.ResumeScene();
				SceneStack.Remove(CurrentScene);
				TransitionShader = null;

				previousScene.OnRemoval?.Invoke();
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			if (CurrentScene == null) return;

			lock (SceneStack)
			{
				if (SceneStack.Count > 0)
				{
					foreach (Scene scene in SceneStack)
					{
						scene.Draw(GraphicsDevice, spriteBatch, gameRender);
					}
				}
			}

			CurrentScene.Draw(GraphicsDevice, spriteBatch, gameRender);

			/*
			GraphicsDevice.SetRenderTarget(null);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
			spriteBatch.Draw(gameRender, ScreenShake, Color.White);
			spriteBatch.End();
			*/

			
			int currentWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
			int currentHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
			int scale = currentHeight / SCREEN_HEIGHT;
			Matrix matrix = Matrix.CreateScale(scale) * Matrix.CreateTranslation(currentWidth % SCREEN_WIDTH / 2, currentHeight % SCREEN_HEIGHT / 2, 0);

			Effect shader = TransitionShader == null ? null : TransitionShader.Effect;
			GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(CLEAR_COLOR);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, shader, matrix);
			spriteBatch.Draw(gameRender, ScreenShake, Color.White);
			spriteBatch.End();
			

			base.Draw(gameTime);
		}

		public static void Transition(Task<Scene> sceneTask)
        {
            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
            ColorFade colorFade = new ColorFade(Color.Black, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => colorFade.Amount = t);
            
			Transition(CurrentScene, sceneTask, transitionController, colorFade);
        }

		public static void Transition(Scene controllerScene, Task<Scene> sceneTask, TransitionController transitionController, Shader transitionShader)
		{
			controllerScene.AddController(transitionController);
			TransitionShader = transitionShader;

			transitionController.FinishTransition += new Action<TransitionDirection>(t =>
			{
				if (sceneTask.IsCompleted) pendingScene = sceneTask.Result;
				transitionShader?.Terminate();
			});

			sceneTask.ContinueWith(t =>
			{
				if (transitionController.Terminated)
				{
					pendingScene = sceneTask.Result;
					transitionShader?.Terminate();
				}
			});

			sceneTask.Start();
		}

		public static void SetCurrentScene(Scene newScene)
		{
			CurrentScene.EndScene();

			TransitionShader?.Terminate();
			CurrentScene = newScene;

			if (newScene.Suspended) newScene.ResumeScene();
			else newScene.BeginScene();
		}

		public static void StackScene(Scene newScene, bool suspended = false)
		{
			lock (SceneStack)
			{
				SceneStack.Add(CurrentScene);
			}

			CurrentScene.Suspended = suspended;
			var oldScene = CurrentScene;

			newScene.OnTerminated += new TerminationFollowup(() => oldScene.Suspended = false);
			newScene.BeginScene();

			CurrentScene = newScene;
		}

		public static T GetScene<T>() where T : Scene
		{
			if (CurrentScene is T) return (T)CurrentScene;
			else
			{
				T result;
				lock (SceneStack)
				{
					result = (T)SceneStack.FirstOrDefault(x => x is T);
				}

				return result;
			}
		}
	}
}
