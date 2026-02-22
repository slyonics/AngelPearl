using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Widgets;

namespace AngelPearl.Scenes.CrawlerScene
{
    public class BattlePlayer : Battler
    {
        public const int HERO_WIDTH = 21;
        public const int HERO_HEIGHT = 21;

        protected enum HeroAnimation
        {
            Ready,
            Walking,
            Victory,
            Guarding,
            Stab,
            Attack,
            Shoot,
            Chanting,
            Spell,
            Item,
            Point,
            Hit,
            Hurting,
            Dead
        }

        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.Ready.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 1, 400) },
            { HeroAnimation.Victory.ToString(), new Animation(6, 1, HERO_WIDTH, HERO_HEIGHT, 2, 700) },
            { HeroAnimation.Guarding.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Attack.ToString(), new Animation(3, 1, HERO_WIDTH, HERO_HEIGHT, 3, new int[] { 100, 100, 300 }) },
            { HeroAnimation.Chanting.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 3, 1000) },
            { HeroAnimation.Hit.ToString(), new Animation(0, 4, HERO_WIDTH, HERO_HEIGHT, 1, 600) },
            { HeroAnimation.Hurting.ToString(), new Animation(6, 2, HERO_WIDTH, HERO_HEIGHT, 3, 100) },
            { HeroAnimation.Dead.ToString(), new Animation(6, 5, HERO_WIDTH, HERO_HEIGHT, 1, 1000) }
        };

        public static readonly Dictionary<string, Animation> SHADOW_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.Ready.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 3, 400) }
        };

        private BattleController enqueuedController;
        private CommandRecord enqueuedCommand;
        private ItemModel enqueuedItemModel;
        private int prepTimeLeft = 0;
        private bool healAnimation;


        private AnimatedSprite shadowSprite;

        private HeroModel heroModel;
        public HeroModel HeroModel
        {
            get => heroModel; set
            {
                heroModel = value;
            }
        }

        public BattlePlayer(CrawlerScene iBattleScene, Vector2 iPosition, HeroModel iHeroModel)
            : base(iBattleScene, iPosition, AssetCache.SPRITES[GameSprite.Portraits_Aika], HERO_ANIMATIONS)
        {
            //shader.Parameters["flashInterval"].SetValue(0.0f);

            battlerOffset = new Vector2(160, 0);
        
            stats = HeroModel = iHeroModel;

            heroModel.StatusAilments.Clear();
            // reapply poision and stone here

            if (heroModel.Weapon.Value != null && heroModel.Weapon.Value.AutoBuffs != null) foreach (BuffType buff in heroModel.Weapon.Value.AutoBuffs) Buffs.Add(buff);
            if (heroModel.Armor.Value != null && heroModel.Armor.Value.AutoBuffs != null) foreach (BuffType buff in heroModel.Armor.Value.AutoBuffs) Buffs.Add(buff);
            if (heroModel.Accessory.Value != null && heroModel.Accessory.Value.AutoBuffs != null) foreach (BuffType buff in heroModel.Accessory.Value.AutoBuffs) Buffs.Add(buff);

            AnimatedSprite.PlayAnimation("Walking");

            HeroModel.UpdateHealthColor();

            int startingInitiative = Math.Max(1, 120 + HeroModel.Reflex.Value);
            Initiative = Rng.RandomInt(startingInitiative, 200);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ParticleList.Count == 0 && healAnimation)
            {
                Idle();
                healAnimation = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            // shadowSprite?.Draw(spriteBatch, new Vector2(Position.X, Position.Y + 2) + battlerOffset, null, 0.9f);
        }

        public void DrawShader(SpriteBatch spriteBatch)
        {
            shadowSprite?.Draw(spriteBatch, new Vector2(Position.X, Position.Y + 2) + battlerOffset, null, 0.95f);

            if (flashTime > 0)
            {
				AnimatedSprite.SpriteColor = Color.Lerp(flashColor, Color.White, (float)flashTime / flashDuration);
			}
            else
            {
				AnimatedSprite.SpriteColor = Color.White;
			}

            AnimatedSprite.Draw(spriteBatch, Position + battlerOffset, null, 0.9f);
        }

        public void DrawAilment(SpriteBatch spriteBatch)
        {
            AilmentSprite.Draw(spriteBatch, Center, null, 0.1f);
        }

        public void EnqueueCommand(BattleController battleController, CommandRecord commandRecord, BattleCommand battleCommand, ItemModel itemModel)
        {
            enqueuedController = battleController;
            enqueuedCommand = commandRecord;
            enqueuedItemModel = itemModel;
        }

        public void ResetCommand()
        {
            enqueuedCommand = null;
            enqueuedController = null;
        }

        public List<DialogueRecord> GrowAfterBattle(EncounterRecord encounterRecord)
        {
            List<DialogueRecord> reports = new List<DialogueRecord>();

            return reports;
        }

        public override void Damage(int damage)
        {
            base.Damage(damage);

            if (Dead)
            {
                prepTimeLeft = 0;
                enqueuedController = null;
                enqueuedCommand = null;
                Initiative = 0;
                stats.StatusAilments.Clear();
                AilmentSprite.PlayAnimation(AilmentType.Healthy.ToString());
            }

            PlayAnimation("Hit", Idle);

            HeroModel.UpdateHealthColor();
        }

        public override void InflictAilment(Battler attacker, AilmentType ailment)
        {
            base.InflictAilment(attacker, ailment);

            if (Dead)
            {
                prepTimeLeft = 0;
                enqueuedController = null;
                enqueuedCommand = null;
                Initiative = 0;
                stats.StatusAilments.Clear();
                AilmentSprite.PlayAnimation(AilmentType.Healthy.ToString());
                HeroModel.UpdateHealthColor();
                PlayAnimation("Dead");
            }
            else Idle();
        }

        public override void HealAilment(AilmentType ailment)
        {
            base.HealAilment(ailment);

            Idle();
        }

        public override void Heal(int healing)
        {
            base.Heal(healing);

            HeroModel.UpdateHealthColor();

            healAnimation = true;
        }

        public void Idle()
        {
            if (Defending && !Dead) PlayAnimation("Guarding");
            else if (enqueuedCommand != null && !enqueuedController.Terminated) PlayAnimation(enqueuedCommand.Animation);
            else if (Stats.HP.Value > HeroModel.MaxHP.Value / 8 && !Stats.StatusAilments.Any()) PlayAnimation("Ready");
            else if (Stats.HP.Value > 0) PlayAnimation("Hurting");
            else PlayAnimation("Dead");
        }

        public override float Initiative
        {
            set => initiative = value;
            get { return initiative; }
        }

        public bool Ready { get => initiative >= 255 && !Dead && enqueuedController == null; }
    }
}
