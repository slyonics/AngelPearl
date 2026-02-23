using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AngelPearl.Main;
using AngelPearl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngelPearl.SceneObjects;

namespace AngelPearl.Scenes.CrawlerScene
{
	public class BattleEnemy : Battler
	{
		private const int FADE_IN_DURATION = 600;
		private const int ATTACK_DURATION = 500;
		private const int DEATH_DURATION = 600;

		private CrawlerScene crawlerScene;

		private int fadeInTime;
		private int attackTimeLeft;
		private int deathTimeLeft;

		public BattleEnemy(CrawlerScene iScene, EnemyRecord enemyRecord, Vector2 iPosition)
			: base(iScene, iPosition, AssetCache.SPRITES[Enum.Parse<GameSprite>($"Enemies_{enemyRecord.Sprite}")])
		{
			crawlerScene = iScene;

			stats = new BattlerModel(enemyRecord);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (fadeInTime < FADE_IN_DURATION)
			{
				fadeInTime += gameTime.ElapsedGameTime.Milliseconds;
				float flashInterval = Math.Min((float)fadeInTime / FADE_IN_DURATION, 1.0f);
				//shader.Parameters["flashInterval"].SetValue(1.0f - flashInterval);
			}

			if (attackTimeLeft > 0)
			{
				attackTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
				if (attackTimeLeft > 0 && (attackTimeLeft / (ATTACK_DURATION / 4)) % 2 == 0) AnimatedSprite.SpriteEffects = SpriteEffects.FlipHorizontally;
				else AnimatedSprite.SpriteEffects = SpriteEffects.None;
			}

			if (deathTimeLeft > 0)
			{
				deathTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
				//shader.Parameters["destroyInterval"].SetValue((float)deathTimeLeft / DEATH_DURATION);
				if (deathTimeLeft <= 0) { deathTimeLeft = -1; }
			}
			else if (deathTimeLeft == -1)
			{
				Terminate();
			}
		}

		public override void Damage(int damage)
		{
			base.Damage(damage);

			if (Dead)
			{
				deathTimeLeft = DEATH_DURATION;
				Audio.PlaySound(GameSound.EnemyDeath);
			}
		}


		public override bool Busy { get => base.Busy || attackTimeLeft > 0 || Dead || fadeInTime < FADE_IN_DURATION; }
	}
}
