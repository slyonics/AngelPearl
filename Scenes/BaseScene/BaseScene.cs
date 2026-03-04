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

namespace AngelPearl.Scenes.BaseScene
{
    public class BaseScene : Scene
    {
		public BaseViewModel BaseViewModel { get; private set; }

		Texture2D nebulae = AssetCache.SPRITES[GameSprite.Background_Nebulae];
		Texture2D stars = AssetCache.SPRITES[GameSprite.Background_YellowStars];

		Texture2D sun = AssetCache.SPRITES[GameSprite.Widgets_Images_Sun];
		Texture2D earth = AssetCache.SPRITES[GameSprite.Widgets_Images_Earth];
		Texture2D moon = AssetCache.SPRITES[GameSprite.Widgets_Images_Moon];
		Texture2D angel = AssetCache.SPRITES[GameSprite.Widgets_Images_Comet];

		Vector2 angelPosition;
		MissionRecord missionRecord;

		public BaseScene()
        {
			missionRecord = GameProfile.CurrentSave.CurrentMission.Value;

			float interval = GameProfile.CurrentSave.DayOfYear.Value - missionRecord.MissionStart / missionRecord.TimeLimit;
			angelPosition = Vector2.Lerp(new Vector2(210, 150), new Vector2(missionRecord.AngelStartX, missionRecord.AngelStartY), interval);
		}

		public override void BeginScene()
		{
			base.BeginScene();

			BaseViewModel = AddView(new BaseViewModel(this, GameProfile.CurrentSave.CurrentMission.Value, angelPosition));

			Audio.PlayMusic(GameMusic.MissionSelect);
		}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			if (Input.CurrentInput.AnythingPressed())
			{
				//CrossPlatformGame.Transition(new Task<Scene>(() => new CrawlerScene.CrawlerScene(GameMap.TestAngel, 6, 13, Direction.North)));
			}
		}

		public override void DrawBackground(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(stars, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.95f);
			spriteBatch.Draw(nebulae, Vector2.Zero, null, new Color(255,255,255,128), 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.9f);


			spriteBatch.Draw(sun, new Vector2(40, 90), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.8f);

			spriteBatch.Draw(earth, new Vector2(160, 160), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.7f);

			spriteBatch.Draw(moon, new Vector2(210, 150), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.6f);

			spriteBatch.Draw(angel, angelPosition, null, Color.White, 0.0f, new Vector2(angel.Width / 2), 1.0f, SpriteEffects.FlipVertically, 0.4f);

		}

	}
}
