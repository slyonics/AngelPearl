using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using AngelPearl.SceneObjects.Shaders;
using AngelPearl.Scenes.CrawlerScene;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.LoadingScene
{
    public class LoadingScene : Scene
    {
		private enum LoadingPhase
		{
			InitializingProgram,
			LoadingMaps,
			LoadingMusic,
			LoadingShaders,
			LoadingSounds,
			LoadingSprites,
			LoadingViews,
			LoadingData,
			InitializingScenes,
			Done
		}

		private Array mapValues = Enum.GetValuesAsUnderlyingType(typeof(GameMap));
		private Array musicValues = Enum.GetValuesAsUnderlyingType(typeof(GameMusic));
		private Array shaderValues = Enum.GetValuesAsUnderlyingType(typeof(GameShader));
		private Array soundValues = Enum.GetValuesAsUnderlyingType(typeof(GameSound));
		private Array spriteValues = Enum.GetValuesAsUnderlyingType(typeof(GameSprite));
		private Array viewValues = Enum.GetValuesAsUnderlyingType(typeof(GameView));

		private System.Collections.IEnumerator assetEnumerator;

		LoadingPhase currentPhase = LoadingPhase.InitializingProgram;
		string loadingMessage = "Initializing scenes ...";
		int currentIndex = 0;
		int maxIndex;

		public LoadingScene()
        {
			foreach (GameFont font in Enum.GetValuesAsUnderlyingType(typeof(GameFont)))
			{
				if (font == GameFont.None) continue;

				SpriteFont mapData = CrossPlatformGame.ContentManager.Load<SpriteFont>($"Fonts/{font.ToString().Replace('_', '/')}");
				AssetCache.FONTS.Add(font, mapData);
			}

			maxIndex = mapValues.Length + musicValues.Length + shaderValues.Length + soundValues.Length + spriteValues.Length + viewValues.Length;
		}

		public override void BeginScene()
		{
			sceneStarted = true;
		}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			LoadNextAsset();

			if (currentPhase == LoadingPhase.Done)
			{
				if (GameProfile.SaveAvailable())
				{
					
				}
				else
				{
					GameProfile.NewState();

					// CrossPlatformGame.SetCurrentScene(new MapScene.MapScene(GameMap.Overworld, "Default"));
                    CrossPlatformGame.SetCurrentScene(new CrawlerScene.CrawlerScene(GameMap.TestAngel, 6, 13, Direction.North));
                }
			}
        }

		public override void DrawBackground(SpriteBatch spriteBatch)
		{
			int progress = (int)(currentIndex * 100.0f / maxIndex);

			Text.DrawCenteredText(spriteBatch, new Vector2(CrossPlatformGame.SCREEN_WIDTH / 2, CrossPlatformGame.SCREEN_HEIGHT / 2 - 6), GameFont.Dialogue, loadingMessage);
			Text.DrawCenteredText(spriteBatch, new Vector2(CrossPlatformGame.SCREEN_WIDTH / 2, CrossPlatformGame.SCREEN_HEIGHT / 2 + 6), GameFont.Dialogue, $"{progress}%");
		}

		private void LoadNextAsset()
		{
			switch (currentPhase)
			{
				case LoadingPhase.InitializingProgram:
					currentPhase = LoadingPhase.LoadingMaps;
					loadingMessage = "Loading maps ...";
					assetEnumerator = mapValues.GetEnumerator();
					currentIndex = 1;
					break;

				case LoadingPhase.LoadingMaps:
					if (assetEnumerator.MoveNext())
					{
						AssetCache.LoadMap(CrossPlatformGame.ContentManager, (GameMap)assetEnumerator.Current);						
					}
					else
					{
						currentPhase = LoadingPhase.LoadingMusic;
						loadingMessage = "Loading music ...";
						assetEnumerator = musicValues.GetEnumerator();
					}
					break;

				case LoadingPhase.LoadingMusic:
					if (assetEnumerator.MoveNext())
					{
						AssetCache.LoadMusic(CrossPlatformGame.ContentManager, (GameMusic)assetEnumerator.Current);
						currentIndex++;
					}
					else
					{
						currentPhase = LoadingPhase.LoadingShaders;
						loadingMessage = "Loading shaders ...";
						assetEnumerator = shaderValues.GetEnumerator();
					}
					break;

				case LoadingPhase.LoadingShaders:
					if (assetEnumerator.MoveNext())
					{
						AssetCache.LoadShader(CrossPlatformGame.ContentManager, (GameShader)assetEnumerator.Current);
						currentIndex++;
					}
					else
					{
						currentPhase = LoadingPhase.LoadingSounds;
						loadingMessage = "Loading sounds ...";
						assetEnumerator = soundValues.GetEnumerator();
					}
					break;

				case LoadingPhase.LoadingSounds:
					if (assetEnumerator.MoveNext())
					{
						AssetCache.LoadSound(CrossPlatformGame.ContentManager, (GameSound)assetEnumerator.Current);
						currentIndex++;
					}
					else
					{
						currentPhase = LoadingPhase.LoadingSprites;
						loadingMessage = "Loading sprites ...";
						assetEnumerator = spriteValues.GetEnumerator();
					}
					break;

				case LoadingPhase.LoadingSprites:
					if (assetEnumerator.MoveNext())
					{
						AssetCache.LoadSprite(CrossPlatformGame.ContentManager, (GameSprite)assetEnumerator.Current);
						currentIndex++;
					}
					else
					{
						currentPhase = LoadingPhase.LoadingViews;
						loadingMessage = "Loading views ...";
						assetEnumerator = viewValues.GetEnumerator();
					}
					break;

				case LoadingPhase.LoadingViews:
					if (assetEnumerator.MoveNext())
					{
						AssetCache.LoadView(CrossPlatformGame.ContentManager, (GameView)assetEnumerator.Current);
						currentIndex++;
					}
					else
					{
						currentPhase = LoadingPhase.LoadingData;
						loadingMessage = "Loading game data ...";
					}
					break;

				case LoadingPhase.LoadingData:
					ConversationRecord.CONVERSATIONS = AssetCache.LoadRecords<ConversationRecord>("Data/ConversationData");
					MuseRecord.MUSES = AssetCache.LoadRecords<MuseRecord>("Data/MuseData");
					EnemyRecord.ENEMIES = AssetCache.LoadRecords<EnemyRecord>("Data/EnemyData");
					EncounterRecord.ENCOUNTERS = AssetCache.LoadRecords<EncounterRecord>("Data/EncounterData");
					ItemRecord.ITEMS = AssetCache.LoadRecords<ItemRecord>("Data/ItemData");
                    currentPhase = LoadingPhase.InitializingScenes;
					loadingMessage = "Initializing scenes ...";
					break;

				case LoadingPhase.InitializingScenes:
					//BattleScene.BattleScene.Initialize();
					currentPhase = LoadingPhase.Done;
					loadingMessage = "Done!";
					break;
			}
		}
    }
}
