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

        }

		public override void DrawBackground(SpriteBatch spriteBatch)
		{

		}

    }
}
