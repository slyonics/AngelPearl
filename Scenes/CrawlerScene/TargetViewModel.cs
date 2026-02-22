using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AngelPearl.Models;
using AngelPearl.Main;
using AngelPearl.SceneObjects;

namespace AngelPearl.Scenes.CrawlerScene
{
    public class TargetViewModel : ViewModel
    {
        private static readonly Dictionary<string, Animation> POINTER_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Valid", new Animation(0, 0, 16, 16, 3, 600) },
            { "Invalid", new Animation(0, 1, 16, 16, 3, 600) }
        };

        private static Battler lastEnemyTarget;

        public static void ClearLastTarget()
        {
            lastEnemyTarget = null;
        }

        private uint targetFrames = 0;

        CrawlerScene battleScene;

        public Battler target;
        int confirmCooldown = 100;

        private AnimatedSprite pointerSprite;

        private bool targetAllEnemies;
        private bool targetAllAllies;

        private ItemModel itemModel;
        private BattleCommand battleCommand;

        private NinePatch warningBox;
        private string warningMessage;
        private Color warningColor = new Color(252, 224, 168);

        public TargetViewModel(CrawlerScene iScene, BattlePlayer iPlayer, ItemModel iItemModel, BattleCommand iBattleCommand)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;
            Player = iPlayer;
            itemModel = iItemModel;            
            Command = itemModel.ItemRecord;
            battleCommand = iBattleCommand;

            pointerSprite = new AnimatedSprite(AssetCache.SPRITES[GameSprite.Widgets_Images_Pointer], POINTER_ANIMATIONS);

            switch (Command.Targetting)
            {
                case TargetType.OneEnemy:
                    if (lastEnemyTarget != null && !lastEnemyTarget.Dead) target = lastEnemyTarget;
                    else target = battleScene.BattleViewModel.EnemyList.Where(x => !x.Dead).MinBy(new Func<Battler, float>(t => Vector2.Distance(new Vector2(CrossPlatformGame.SCREEN_WIDTH, CrossPlatformGame.SCREEN_HEIGHT), t.Position)));
                    ValidatePointer();
                    break;

                case TargetType.OneAlly:
                    target = battleScene.BattleViewModel.PlayerList.Where(x => !x.Dead || Command.TargetDead).First();
                    ValidatePointer();
                    break;
            }

            if (!string.IsNullOrEmpty(itemModel.ItemRecord.Description))
            {
                Description = itemModel.ItemRecord.Description;
                LoadView(GameView.Crawler_TargetView);
            }
        }

        public TargetViewModel(CrawlerScene iScene, BattlePlayer iPlayer, CommandRecord iCommandRecord, BattleCommand iBattleCommand)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;
            Player = iPlayer;
            Command = iCommandRecord;
            battleCommand = iBattleCommand;

            pointerSprite = new AnimatedSprite(AssetCache.SPRITES[GameSprite.Widgets_Images_Pointer], POINTER_ANIMATIONS);

            switch (Command.Targetting)
            {
                case TargetType.OneEnemy:
                    if (lastEnemyTarget != null && !lastEnemyTarget.Dead) target = lastEnemyTarget;
                    else target = battleScene.BattleViewModel.EnemyList.Where(x => !x.Dead).MinBy(new Func<Battler, float>(t => Vector2.Distance(new Vector2(CrossPlatformGame.SCREEN_WIDTH, CrossPlatformGame.SCREEN_HEIGHT), t.Position)));
                    ValidatePointer();
                    break;

                case TargetType.OneAlly:
                    target = battleScene.BattleViewModel.PlayerList.Where(x => !x.Dead || Command.TargetDead).First();
                    ValidatePointer();
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Player.Dead || battleScene.BattleViewModel == null)
            {
                Terminate();
                return;
            }

            if (target != null && target.Dead && !Command.TargetDead && !targetAllEnemies && !targetAllAllies)
            {
                switch (Command.Targetting)
                {
                    case TargetType.OneEnemy:
                        if (battleScene.BattleViewModel.EnemyList.Count == 0) target = null;
                        else target = battleScene.BattleViewModel.EnemyList.Where(x => !x.Dead).MinBy(new Func<Battler, float>(t => Vector2.Distance(new Vector2(CrossPlatformGame.SCREEN_WIDTH, CrossPlatformGame.SCREEN_HEIGHT), t.Position)));
                        break;

                    case TargetType.OneAlly:
                        target = battleScene.BattleViewModel.PlayerList.Where(x => !x.Dead || Command.TargetDead).FirstOrDefault();
                        break;
                }
            }

            if (target == null && !targetAllEnemies && !targetAllAllies)
            {
                Terminate();
                return;
            }

            var oldTarget = target;

            var input = Input.CurrentInput;
            if (input.CommandPressed(Main.Command.Left)) TargetUp();
            else if (input.CommandPressed(Main.Command.Right)) TargetDown();
            else if (input.CommandPressed(Main.Command.Confirm) && confirmCooldown <= 0) SelectCurrentTarget();
            else if (input.CommandPressed(Main.Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);
                Terminate();
            }

            if (target != oldTarget)
            {
                if (target != null) ValidatePointer();
                Audio.PlaySound(GameSound.Cursor);
            }

            if (confirmCooldown > 0) confirmCooldown -= gameTime.ElapsedGameTime.Milliseconds;

            pointerSprite.Update(gameTime);
        }

        private void TargetUp()
        {
            if (Command.Targetting == TargetType.All || Command.Targetting == TargetType.Self) return;

            if (target is BattleEnemy)
            {
                List<Battler> targetList = new List<Battler>();
                targetList.AddRange(battleScene.BattleViewModel.EnemyList.Where(x => !x.Dead && x.Center.Y < target.Center.Y));

                if (targetList.Count > 0) target = targetList.MinBy(new Func<Battler, float>(t => Vector2.Distance(target.Position, t.Position)));
            }
            else if (target is BattlePlayer)
            {
                List<Battler> targetList = new List<Battler>();
                targetList.AddRange(battleScene.BattleViewModel.PlayerList.Where(x => (!x.Dead || Command.TargetDead) && x.Center.Y < target.Center.Y));

                if (targetList.Count > 0) target = targetList.MinBy(new Func<Battler, float>(t => Vector2.Distance(target.Position, t.Position)));
            }
        }

        private void TargetDown()
        {
            if (Command.Targetting == TargetType.All || Command.Targetting == TargetType.Self) return;

            if (target is BattleEnemy)
            {
                List<Battler> targetList = new List<Battler>();
                targetList.AddRange(battleScene.BattleViewModel.EnemyList.Where(x => !x.Dead && x.Center.Y > target.Center.Y));

                if (targetList.Count > 0) target = targetList.MinBy(new Func<Battler, float>(t => Vector2.Distance(target.Position, t.Position)));
            }
            else if (target is BattlePlayer)
            {
                List<Battler> targetList = new List<Battler>();
                targetList.AddRange(battleScene.BattleViewModel.PlayerList.Where(x => (!x.Dead || Command.TargetDead) && x.Center.Y > target.Center.Y));

                if (targetList.Count > 0) target = targetList.MinBy(new Func<Battler, float>(t => Vector2.Distance(target.Position, t.Position)));
            }
        }

        private void ValidatePointer()
        {
            if (target != null && battleCommand == BattleCommand.Fight && Player.ScaredOf.Contains(target))
            {
                pointerSprite.PlayAnimation("Invalid");

                warningMessage = "Fear";
                int width = Text.GetStringLength(GameFont.Interface, warningMessage);
                int height = Text.GetStringHeight(GameFont.Interface);
                if (warningBox == null) warningBox = new NinePatch("DarkFrame", 0.05f);
                warningBox.Bounds = new Rectangle(0, 0, width + 10, height + 7);
            }
            else
            {
                pointerSprite.PlayAnimation("Valid");
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            targetFrames++;

            if (targetAllEnemies && !targetAllAllies)
            {
                pointerSprite.SpriteEffects = SpriteEffects.None;

                var aliveEnemies = battleScene.BattleViewModel.EnemyList.Where(x => !x.Dead);
                int targetCount = aliveEnemies.Count();
                uint i = 0;
                foreach (BattleEnemy enemy in aliveEnemies)
                {
                    if (targetFrames % (targetCount + 1) != i)
                    {
                        Vector2 pointerPosition = enemy.Position + new Vector2(enemy.AnimatedSprite.SpriteBounds().Width / 2, -enemy.AnimatedSprite.SpriteBounds().Height / 3);
                        pointerSprite.Draw(spriteBatch, pointerPosition, null, 0.01f);
                    }
                    i++;
                }
            }
            else if (targetAllAllies && !targetAllEnemies)
            {
                pointerSprite.SpriteEffects = SpriteEffects.FlipHorizontally;

                var aliveAllies = battleScene.BattleViewModel.PlayerList.Where(x => !x.Dead);
                int targetCount = aliveAllies.Count();
                uint i = 0;
                foreach (BattlePlayer player in aliveAllies)
                {
                    if (targetFrames % (targetCount + 1) != i) pointerSprite.Draw(spriteBatch, new Vector2(player.SpriteBounds.Left, player.SpriteBounds.Center.Y), null, 0.01f);
                    i++;
                }
            }
            else
            {
                Vector2 pointerPos;
                if (target is BattlePlayer)
                {
                    pointerSprite.SpriteEffects = SpriteEffects.FlipHorizontally;
                    pointerPos = new Vector2(target.SpriteBounds.Left, target.SpriteBounds.Center.Y);
                    pointerSprite.Draw(spriteBatch, pointerPos, null, 0.01f);
                }
                else
                {
                    pointerSprite.SpriteEffects = SpriteEffects.None;
                    pointerPos = target.Position + new Vector2(target.AnimatedSprite.SpriteBounds().Width / 2, -target.AnimatedSprite.SpriteBounds().Height / 3);
                    pointerSprite.Draw(spriteBatch, pointerPos, null, 0.01f);
                }

                if (pointerSprite.AnimationName == "Invalid")
                {
                    warningBox.Draw(spriteBatch, pointerPos - new Vector2(warningBox.Bounds.Width / 2, 4));
                    Text.DrawCenteredText(spriteBatch, pointerPos + new Vector2(1, 3), GameFont.Interface, warningMessage, warningColor, 0.03f);
                }
            }
        }


        private void SelectCurrentTarget()
        {
            if (pointerSprite.AnimationName == "Invalid")
            {
                Audio.PlaySound(GameSound.Error);
                return;
            }

            Audio.PlaySound(GameSound.Selection);

            Terminate();

            if (Command.Targetting == TargetType.OneEnemy)
            {
                lastEnemyTarget = target as BattleEnemy;

            }

            Player.PlayAnimation(Command.Animation);

            switch (Command.Targetting)
            {
                case TargetType.OneEnemy:
                case TargetType.OneAlly:
                    {
                        BattleController battleController = new BattleController(battleScene, Player, target, Command, targetAllEnemies, targetAllAllies);
                        Player.EnqueueCommand(battleController, Command, battleCommand, itemModel);
                    }
                    break;
            }
        }

        public BattlePlayer Player { get; set; }
        public CommandRecord Command { get; set; }

        public string Description { get; set; }
    }
}
