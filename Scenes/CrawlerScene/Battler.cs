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
		protected const int DAMAGE_FLASH_DURATION = 400;

		protected Color flashColor;
		protected int flashTime;
		protected int flashDuration;

		protected Vector2 battlerOffset;

		protected bool turnActive;
		public virtual bool TurnActive { get => turnActive; }

		protected BattleController enqueuedController;
		protected CommandRecord enqueuedCommand;

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
			priorityLevel = PriorityLevel.TransitionLevel;
		}

		public Battler(CrawlerScene iScene, Vector2 iPosition, Texture2D iSprite, Dictionary<string, Animation> iAnimations)
			: base(iScene, iPosition, iSprite, iAnimations)
		{
			priorityLevel = PriorityLevel.TransitionLevel;
		}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			ParticleList.RemoveAll(x => x.Terminated);

			if (flashTime > 0)
            {
                flashTime -= gameTime.ElapsedGameTime.Milliseconds;
            }
        }

		public override void Draw(SpriteBatch spriteBatch, Camera camera)
		{
			animatedSprite?.Draw(spriteBatch, position - new Vector2(0.0f, positionZ), camera, 0.9f);
		}

		public void ResetCommand()
		{
			enqueuedCommand = null;
			enqueuedController = null;
		}

		public void EnqueueCommand(BattleController battleController, CommandRecord commandRecord)
		{
			enqueuedController = battleController;
			enqueuedCommand = commandRecord;
		}

		public void ExecuteTurn()
		{
			turnActive = true;

			enqueuedController.FixTargetting();
			enqueuedController.OnTerminated += new TerminationFollowup(() => FinishTurn());
			parentScene.AddController(enqueuedController);

			ResetCommand();
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

				((CrawlerScene)parentScene).BattleViewModel.InitiativeList.Remove(this);
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

		public virtual void FlashColor(Color color, int duration = DAMAGE_FLASH_DURATION)
		{
			flashColor = color;
			flashTime = flashDuration = duration;
		}

		public bool Dead { get => Stats.HP.Value <= 0; }

		public virtual int Initiative
		{
			get => 0;
		}

		public virtual bool Busy { get => ParticleList.Count > 0 || turnActive; }

		public Vector2 Center { get => new Vector2(SpriteBounds.Center.X, SpriteBounds.Center.Y); }

		public ModelCollection<BuffType> Buffs { get; private set; } = new ModelCollection<BuffType>();
	}
}
