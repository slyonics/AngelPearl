using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.CrawlerScene
{
	public class Battler : Entity
	{
		private const int DAMAGE_FLASH_DURATION = 400;

		public static Texture2D STATIC_TEXTURE;

		protected Effect shader;
		protected Color flashColor;
		protected int flashTime;
		protected int flashDuration;

		protected Vector2 battlerOffset;

		protected float initiative;
		protected bool turnActive;
		public virtual bool TurnActive { get => turnActive; }

		public bool Defending { get; set; }
		public bool Delaying { get; set; }
		public List<Battler> ScaredOf { get; private set; } = new List<Battler>();

		public AnimatedSprite AilmentSprite { get; protected set; }
		private int confusionTurns = 0;

		public List<Particle> ParticleList { get; } = new List<Particle>();

		protected BattlerModel stats;
		public BattlerModel Stats { get => stats; }

		public Battler(CrawlerScene iScene, Vector2 iPosition, Texture2D iSprite)
			: base(iScene, iPosition, iSprite, new Dictionary<string, Animation>() { { "Idle", new Animation(0, 0, iSprite.Width, iSprite.Height, 1, 10000) } })
		{
			priorityLevel = PriorityLevel.CutsceneLevel;
		}

		public Battler(CrawlerScene iScene, Vector2 iPosition, Texture2D iSprite, Dictionary<string, Animation> iAnimations)
			: base(iScene, iPosition, iSprite, iAnimations)
		{
			priorityLevel = PriorityLevel.CutsceneLevel;
		}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			ParticleList.RemoveAll(x => x.Terminated);
        }

		public override void Draw(SpriteBatch spriteBatch, Camera camera)
		{
			animatedSprite?.Draw(spriteBatch, position - new Vector2(0.0f, positionZ), camera, 0.9f);
		}

		public static void Initialize()
		{
			STATIC_TEXTURE = new Texture2D(CrossPlatformGame.GameInstance.GraphicsDevice, 200, 200);
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

		public virtual void ExecuteTurn()
		{
			turnActive = true;
		}

		public void FinishTurn()
		{
			turnActive = false;
		}

		public virtual void Damage(int damage)
		{
			if (Defending)
			{
				damage /= 2;
			}

			Stats.HP.Value = Math.Max(0, Stats.HP.Value - damage);

			if (Dead)
			{
				var autoReviveBuff = Buffs.FirstOrDefault(x => x.Value == BuffType.AutoRevive);
				if (autoReviveBuff != null)
				{
					Stats.HP.Value = 1;
					Buffs.Remove(autoReviveBuff);
				}
			}

			ParticleList.Add(parentScene.AddParticle(new DamageParticle(parentScene, Position, damage.ToString())));
		}

		public virtual void InflictAilment(Battler attacker, AilmentType ailment)
		{
			if (!stats.StatusAilments.Any(x => x.Value == ailment)) stats.StatusAilments.Add(ailment);

			switch (ailment)
			{
				case AilmentType.Death:
					Stats.HP.Value = 0;
					if (Dead)
					{
						var autoReviveBuff = Buffs.FirstOrDefault(x => x.Value == BuffType.AutoRevive);
						if (autoReviveBuff != null)
						{
							Stats.HP.Value = 1;
							Buffs.Remove(autoReviveBuff);
						}
					}
					break;

				case AilmentType.Fear:
					if (!ScaredOf.Contains(attacker)) ScaredOf.Add(attacker);
					AilmentSprite.PlayAnimation(AilmentType.Fear.ToString());
					break;

				case AilmentType.Confusion:
					AilmentSprite.PlayAnimation(AilmentType.Confusion.ToString());
					confusionTurns = Rng.RandomInt(2, 4);
					break;
			}
		}

		public virtual void Miss()
		{
			ParticleList.Add(parentScene.AddParticle(new DamageParticle(parentScene, Position, "MISS", Color.WhiteSmoke)));
		}

		public virtual void Heal(int healing)
		{
			if (Dead)
			{
				Initiative = 0;
			}

			Stats.HP.Value = Math.Min(Stats.MaxHP.Value, Stats.HP.Value + healing);

			ParticleList.Add(parentScene.AddParticle(new DamageParticle(parentScene, Position, healing.ToString(), new Color(28, 210, 160))));
		}

		public virtual void Replenish(int replenishment)
		{
			Stats.MP.Value = Math.Min(Stats.MaxMP.Value, Stats.MP.Value + replenishment);

			ParticleList.Add(parentScene.AddParticle(new DamageParticle(parentScene, Position, replenishment.ToString(), new Color(28, 160, 210))));
		}

		public virtual void HealAilment(AilmentType ailment)
		{
			Stats.StatusAilments.ModelList.RemoveAll(x => x.Value == ailment);

			if (Stats.StatusAilments.Count() == 0) AilmentSprite.PlayAnimation(AilmentType.Healthy.ToString());
		}

		public void FlashColor(Color color, int duration = DAMAGE_FLASH_DURATION)
		{
			//shader.Parameters["flashColor"].SetValue(flashColor.ToVector4());
			flashColor = color;
			flashTime = flashDuration = duration;
		}

		public bool Dead { get => Stats.HP.Value <= 0; }

		public virtual float Initiative
		{
			get => initiative;
			set
			{
				initiative = value;
			}
		}

		public virtual bool Busy { get => ParticleList.Count > 0 || turnActive; }

		public Vector2 Center { get => new Vector2(SpriteBounds.Center.X, SpriteBounds.Center.Y); }

		public ModelCollection<BuffType> Buffs { get; private set; } = new ModelCollection<BuffType>();
	}
}
