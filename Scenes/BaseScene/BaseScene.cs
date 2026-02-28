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
		Texture2D nebulae = AssetCache.SPRITES[GameSprite.Background_Nebulae];


		public BaseScene()
        {


		}

		public override void BeginScene()
		{
			base.BeginScene();

		}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			if (Input.CurrentInput.AnythingPressed())
			{
				CrossPlatformGame.Transition(new Task<Scene>(() => new CrawlerScene.CrawlerScene(GameMap.TestAngel, 6, 13, Direction.North)));
			}
		}

		public override void DrawBackground(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(nebulae, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.9f);
		}

    }
}
