using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using AngelPearl.Main;
using System;
using System.Xml;

namespace AngelPearl.SceneObjects.Widgets
{
	public class Image : Widget
	{
		public delegate void ImageDrawFunction(SpriteBatch spriteBatch, Rectangle bounds, Color color, float depth);

		public const int ICON_SIZE = 8;

		private Rectangle? iconSource;
		public string Icon { set { UpdateIconSource(value); } }

		public AnimatedSprite Sprite { get; set; }

		private Texture2D picture;
		private Texture2D Picture { get => picture; set { picture = value; } }

		public ImageDrawFunction DrawDelegate { get; set; }

		private GameSprite GameSprite
		{
			set
			{
				picture = AssetCache.SPRITES[value];
			}
		}

		public float SpriteScale { get; set; } = 1.0f;

		public Image(Widget iParent, float widgetDepth)
			: base(iParent, widgetDepth)
		{

		}

		private void UpdateIconSource(string coords)
		{
			var tokens = coords.Split(',');
			int x = int.Parse(tokens[0]);
			int y = int.Parse(tokens[1]);
			iconSource = new Rectangle(x * ICON_SIZE, y * ICON_SIZE, ICON_SIZE, ICON_SIZE);
		}

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

			if (bounds.Width == 0 || bounds.Height == 0)
			{
				bounds = new Rectangle(0, 0, picture.Width, picture.Height);
			}
        }

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			Sprite?.Update(gameTime);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			Color drawColor = (parent.Enabled) ? Color : new Color(255, 0, 64, 255);

			if (iconSource != null)
			{
				var iconSprite = AssetCache.SPRITES[GameSprite.Widgets_Images_Icons];
				spriteBatch.Draw(iconSprite, new Vector2(currentWindow.Center.X - ICON_SIZE / 2, currentWindow.Center.Y - ICON_SIZE / 2) + Position, iconSource.Value, drawColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, Depth);
			}
			else if (Picture != null)
			{
				if (Alignment == Alignment.Bottom)
					spriteBatch.Draw(Picture, new Rectangle(currentWindow.Left + (int)Position.X, (int)Position.Y + currentWindow.Top, currentWindow.Width, currentWindow.Height), null, drawColor, 0.0f, Vector2.Zero, SpriteEffects.None, Depth);
				// spriteBatch.Draw(picture, new Rectangle(currentWindow.Left + (int)Position.X, -currentWindow.Height + (int)Position.Y + parent.InnerBounds.Height / 2 - parent.InnerMargin.Y * CrossPlatformGame.Scale, currentWindow.Width, currentWindow.Height), null, color, 0.0f, Vector2.Zero, SpriteEffects.None, depth - 0.0001f);
				else if (Alignment == Alignment.BottomRight)
					spriteBatch.Draw(Picture, new Rectangle(currentWindow.Right + (int)Position.X, (int)Position.Y + currentWindow.Top, currentWindow.Width, currentWindow.Height), null, drawColor, 0.0f, Vector2.Zero, SpriteEffects.None, Depth);
				else
					spriteBatch.Draw(Picture, new Rectangle(currentWindow.X + (int)Position.X, currentWindow.Y + (int)Position.Y, currentWindow.Width, currentWindow.Height), null, drawColor, 0.0f, Vector2.Zero, SpriteEffects.None, Depth);
			}
			else if (Sprite != null)
			{
				Sprite.Scale = new Vector2(SpriteScale);

				switch (Alignment)
				{
					case Alignment.BottomRight:
						Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X - (Sprite.SpriteBounds().Width * SpriteScale) / 2, currentWindow.Center.Y) + Position, null, Depth);
						break;

					case Alignment.Center:
						Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y) + Position, null, Depth);
						break;

					case Alignment.Bottom:
						Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Bottom - parent.InnerMargin.Height) + Position, null, Depth);
						break;

					default:
						Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X - (Sprite.SpriteBounds().Width * SpriteScale) / 2, currentWindow.Center.Y - (Sprite.SpriteBounds().Height * SpriteScale) / 2) + Position, null, Depth);
						break;
				}
			}
			else if (DrawDelegate != null)
			{
				DrawDelegate.Invoke(spriteBatch, currentWindow, Color.White, Depth);
			}
		}

		public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
		{
			base.EndLeftClick(mouseStart, mouseEnd, otherWidget);

			if (otherWidget == this)
			{
				GetParent<ViewModel>().LeftClickChild(mouseStart, mouseEnd, this, otherWidget);
			}
		}
	}
}
