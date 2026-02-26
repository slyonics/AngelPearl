using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		private EnemyRecord enemyRecord;

		int index;

		public BattleEnemy(CrawlerScene iScene, EnemyRecord iEnemyRecord, Vector2 iPosition, int i, bool flash)
			: base(iScene, iPosition, AssetCache.SPRITES[Enum.Parse<GameSprite>($"Enemies_{iEnemyRecord.Sprite}")])
		{
			crawlerScene = iScene;
			index = i;

			enemyRecord = iEnemyRecord;
			stats = new BattlerModel(iEnemyRecord);

			AnimatedSprite.SpriteColor = new Color(255, 255, 255, index);

			if (!flash)
			{
				fadeInTime = FADE_IN_DURATION;
				float flashInterval = Math.Min((float)fadeInTime / FADE_IN_DURATION, 1.0f);
				crawlerScene.FLASH_INTERVAL[index] = 1.0f - flashInterval;
			}
			else
			{
				fadeInTime = 0;
				float flashInterval = Math.Min((float)fadeInTime / FADE_IN_DURATION, 1.0f);
				crawlerScene.FLASH_INTERVAL[index] = 1.0f - flashInterval;
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (fadeInTime < FADE_IN_DURATION)
			{
				fadeInTime += gameTime.ElapsedGameTime.Milliseconds;
				float flashInterval = Math.Min((float)fadeInTime / FADE_IN_DURATION, 1.0f);
				crawlerScene.FLASH_INTERVAL[index] = 1.0f - flashInterval;
			}
			else
			{
				if (flashTime > 0) crawlerScene.FLASH_INTERVAL[index] = ((float)flashTime / DAMAGE_FLASH_DURATION);
				else crawlerScene.FLASH_INTERVAL[index] = (0.0f);
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
				crawlerScene.DESTROY_INTERVAL[index] = (float)deathTimeLeft / DEATH_DURATION;
				if (deathTimeLeft <= 0) { deathTimeLeft = -1; }
			}
			else if (deathTimeLeft == -1)
			{
				Terminate();
			}
		}

        public void PlanTurn()
        {
            Dictionary<AttackData, double> attacks = enemyRecord.Attacks.ToDictionary(x => x, x => (double)x.Weight);
            AttackData attack = Rng.WeightedEntry<AttackData>(attacks);

            List<BattlePlayer> eligibleTargets = crawlerScene.BattleViewModel.PlayerList.FindAll(x => !x.Dead);
            var target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];
            BattleController battleController = new BattleController(crawlerScene, this, target, attack);

			EnqueueCommand(battleController, new CommandRecord(attack));
		}

        public override void PlayAnimation(string animationName, AnimationFollowup animationFollowup = null)
        {
			switch (animationName)
            {
                case "Attack": attackTimeLeft = ATTACK_DURATION; break;
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

		public override void FlashColor(Color flashColor, int duration = DAMAGE_FLASH_DURATION)
		{
			base.FlashColor(flashColor, duration);
			crawlerScene.FLASH_COLOR[index] = flashColor.ToVector4();

			float flashInterval = Math.Min((float)flashTime / flashDuration, 1.0f);
			crawlerScene.FLASH_INTERVAL[index] = 1.0f - flashInterval;
		}


		public override bool Busy { get => base.Busy || attackTimeLeft > 0 || Dead || fadeInTime < FADE_IN_DURATION; }
	}
}
