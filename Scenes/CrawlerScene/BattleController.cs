using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using AngelPearl.SceneObjects.Particles;
using AngelPearl.SceneObjects.ViewModels;

namespace AngelPearl.Scenes.CrawlerScene
{
    public class BattleController : ScriptController
    {
        private CrawlerScene battleScene;
        private Battler attacker;
        private Battler target;
        CommandRecord commandRecord;
        AttackData attackData;

        public bool targetAllEnemies;
        public bool targetAllAllies;
        public bool multiTarget;

        public Battler Attacker { get { return attacker; } }

        ConversationViewModel convoScene;
        double timeleft = 0;

        public BattleController(CrawlerScene iBattleScene, Battler iAttacker, Battler iTarget, AttackData iAttackData)
           : base(iBattleScene, iAttackData.Script, PriorityLevel.CutsceneLevel)
        {
            battleScene = iBattleScene;
            attacker = iAttacker;
            target = iTarget;
            attackData = iAttackData;
            targetAllEnemies = attackData.Targetting == TargetType.AllEnemies;
            targetAllAllies = attackData.Targetting == TargetType.AllAllies;

            if (attacker.Dead)
            {
                Terminate();
                return;
            }

            FixTargetting();
        }

        public BattleController(CrawlerScene iBattleScene, Battler iAttacker, Battler iTarget, AttackData iAttackData, string[] script)
           : base(iBattleScene, script, PriorityLevel.CutsceneLevel)
        {
            battleScene = iBattleScene;
            attacker = iAttacker;
            target = iTarget;
            attackData = iAttackData;

            if (attacker.Dead)
            {
                Terminate();
                return;
            }

            FixTargetting();
        }

        public BattleController(CrawlerScene iBattleScene, Battler iAttacker, Battler iTarget, CommandRecord iCommandRecord, bool allEnemies = false, bool allAllies = false)
           : base(iBattleScene, iCommandRecord.BattleScript, PriorityLevel.CutsceneLevel)
        {
            battleScene = iBattleScene;
            attacker = iAttacker;
            target = iTarget;
            commandRecord = iCommandRecord;
            targetAllEnemies = allEnemies;
            targetAllAllies = allAllies;

            if (attacker.Dead)
            {
                Terminate();
                return;
            }

            FixTargetting();
        }

        public BattleController(CrawlerScene iBattleScene, Battler iAttacker, Battler iTarget, CommandRecord iCommandRecord, string[] script)
           : base(iBattleScene, script, PriorityLevel.CutsceneLevel)
        {
            battleScene = iBattleScene;
            attacker = iAttacker;
            target = iTarget;
            commandRecord = iCommandRecord;
            multiTarget = true;

            if (attacker.Dead)
            {
                Terminate();
                return;
            }

            FixTargetting();
        }

        public void StartExecution(ItemModel itemModel)
        {
            if (itemModel != null && itemModel.Quantity.Value > 0)
            {
                itemModel.Quantity.Value = itemModel.Quantity.Value - 1;
                if (itemModel.Quantity.Value == 0 || itemModel.Quantity.Value == -2)
                {
                    var property = GameProfile.CurrentSave.Inventory.ModelList.Find(x => x.Value == itemModel);
                    GameProfile.CurrentSave.Inventory.Remove(property);
                }
            }
		}

        public override void PreUpdate(GameTime gameTime)
        {
            if (scriptParser.Finished)
            {
                if (convoScene != null)
                {
                    if (convoScene.ReadyToProceed.Value)
                    {
                        timeleft -= gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (timeleft < 0)
                        {
                            convoScene.Terminate();
                            convoScene = null;
                        }
                    }
                }
                else Terminate();
            }
            else scriptParser.Update(gameTime);
        }

        public void FixTargetting()
        {
            if (!targetAllEnemies && !targetAllAllies && target.Dead)
            {
                if (target is BattlePlayer)
                {
                    List<BattlePlayer> eligibleTargets = battleScene.BattleViewModel.PlayerList.FindAll(x => !x.Dead);
                    target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];
                }
                else
                {
                    List<BattleEnemy> eligibleTargets = battleScene.BattleViewModel.EnemyList.FindAll(x => !x.Dead && !attacker.ScaredOf.Contains(x));
                    if (eligibleTargets.Count > 0) target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];
                    else
                    {
                        eligibleTargets = battleScene.BattleViewModel.EnemyList.FindAll(x => !x.Dead);
                        target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];
                    }
                }
            }
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                // case "Announce": battleScene.OverlayList.FirstOrDefault(x => x is AnnounceOverlay)?.Terminate(); battleScene.AddOverlay(new AnnounceOverlay(string.Join(' ', tokens.Skip(1)))); break;
                case "Animate": Animate(tokens); break;
                case "Idle": ((BattlePlayer)attacker).Idle(); break;

                case "Effect": Effect(tokens); break;
                case "CenterEffect": CenterEffect(tokens); break;
                case "Damage": CalculateDamage(tokens); break;
                case "Ailment": target.InflictAilment(attacker, (AilmentType)Enum.Parse(typeof(AilmentType), tokens[1])); break;
                case "Heal": CalculateHealing(tokens); break;
                case "Replenish": CalculateReplenish(tokens); break;
                case "Flash": Flash(tokens); break;

                case "Attack": Attack(tokens); break;
                case "Dialogue": Dialogue(tokens); break;
                case "Analyze": Analyze(tokens); return true;
                case "Flee": Flee(tokens); break;
                case "OnHit": if (!CalculateHit(tokens)) scriptParser.EndScript(); break;
                case "Multitarget": if (!Multitarget()) scriptParser.EndScript(); break;
                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            switch (parameter)
            {
                case "$targetCenterX": return target.Center.X.ToString();
                case "$targetCenterY": return target.Center.Y.ToString();
                case "$targetTop": return (target.Position.Y - target.SpriteBounds.Height).ToString();
                case "$targetBottom": return target.Position.Y.ToString();
                case "$targetName": return target.Stats.Name.Value;
                case "$attackerName": return attacker.Stats.Name.Value;
                case "$attackerNameEx": return attacker.Stats.Name.Value + "!";
                default: return base.ParseParameter(parameter);
            }
        }

        private bool Multitarget()
        {
            if (attackData != null)
            {
                if (targetAllEnemies)
                {
                    var enemyTargets = battleScene.BattleViewModel.PlayerList.Where(x => !x.Dead);
                    foreach (BattlePlayer enemy in enemyTargets) battleScene.AddController(new BattleController(battleScene, attacker, enemy, attackData, scriptParser.RemainingCommands));

                    Terminate();

                    return false;
                }
                else if (targetAllAllies)
                {
                    var playerTargets = battleScene.BattleViewModel.EnemyList.Where(x => !x.Dead);
                    foreach (BattleEnemy player in playerTargets) battleScene.AddController(new BattleController(battleScene, attacker, player, attackData, scriptParser.RemainingCommands));

                    Terminate();

                    return false;
                }
            }
            else
            {
                if (targetAllEnemies)
                {
                    var enemyTargets = battleScene.BattleViewModel.EnemyList.Where(x => !x.Dead);
                    foreach (BattleEnemy enemy in enemyTargets) battleScene.AddController(new BattleController(battleScene, attacker, enemy, commandRecord, scriptParser.RemainingCommands));

                    Terminate();

                    return false;
                }
                else if (targetAllAllies)
                {
                    var playerTargets = battleScene.BattleViewModel.PlayerList.Where(x => !x.Dead);
                    foreach (BattlePlayer player in playerTargets) battleScene.AddController(new BattleController(battleScene, attacker, player, commandRecord, scriptParser.RemainingCommands));

                    Terminate();

                    return false;
                }
            }

            return true;
        }

        private void Animate(string[] tokens)
        {
            attacker.PlayAnimation(tokens[1]);
        }

        private void CenterEffect(string[] tokens)
        {
            Vector2 position = new Vector2(int.Parse(tokens[2]), int.Parse(tokens[3]));
            AnimationType animationType = (AnimationType)Enum.Parse(typeof(AnimationType), tokens[1]);
            AnimationParticle animationParticle = new AnimationParticle(battleScene, position, animationType, true);

            if (tokens.Length > 4) animationParticle.AddFrameEvent(int.Parse(tokens[4]), new FrameFollowup(scriptParser.BlockScript()));

            battleScene.AddParticle(animationParticle);
            attacker.ParticleList.Add(animationParticle);
        }

        private void Effect(string[] tokens)
        {
            Vector2 position = new Vector2(int.Parse(tokens[2]), int.Parse(tokens[3]));
            AnimationType animationType = (AnimationType)Enum.Parse(typeof(AnimationType), tokens[1]);
            AnimationParticle animationParticle = new AnimationParticle(battleScene, position, animationType, true);
            animationParticle.Position = new Vector2(animationParticle.Position.X, animationParticle.Position.Y - animationParticle.AnimatedSprite.SpriteBounds().Height / 4);

            if (tokens.Length > 4) animationParticle.AddFrameEvent(int.Parse(tokens[4]), new FrameFollowup(scriptParser.BlockScript()));

            battleScene.AddParticle(animationParticle);
            attacker.ParticleList.Add(animationParticle);
        }

        private bool CalculateHit(string[] tokens)
        {
            int hit = 100;
            int evade = 0;

            switch (tokens[1])
            {
                case "Magic":
                    hit = commandRecord.Hit + attacker.Stats.Level.Value - target.Stats.Level.Value;
                    evade = target.Stats.MagicEvade.Value;
                    break;

                case "Ailment":
                    hit = attackData != null ? attackData.Hit : commandRecord.Hit;
                    evade = target.Stats.AilmentImmune.Any(x => x.Value.ToString() == tokens[2]) ? 100 : 0;
                    break;

                case "Monster":
                    hit = attackData.Hit;
                    evade = target.Stats.PhysicalEvade.Value;
                    break;

                case "MonsterMagic":
                    hit = attackData.Hit;
                    evade = target.Stats.MagicEvade.Value;
                    break;
            }

            if (multiTarget && commandRecord.Targetting != TargetType.All &&
                               commandRecord.Targetting != TargetType.AllEnemies &&
                               commandRecord.Targetting != TargetType.AllAllies)
            {
                hit /= 2;
            }

            if (attacker.Stats.StatusAilments.ModelList.Any(x => x.Value == AilmentType.Fear))
            {
                hit /= 2;
            }

            if (attacker.Stats.StatusAilments.ModelList.Any(x => x.Value == AilmentType.Confusion))
            {
                hit -= 10;
            }

            if (attacker == target)
            {
                hit = 100;
                evade = 0;
            }

            if (Rng.RandomInt(0, 99) > hit)
            {
                Audio.PlaySound(GameSound.Miss);
                target.Miss();

                return false;
            }
            else if (Rng.RandomInt(0, 99) < evade)
            {
                Audio.PlaySound(GameSound.Miss);
                target.Miss();

                return false;
            }
            else
            {
                return true;
            }
        }

        private void CalculateDamage(string[] tokens)
        {
            int attack = 0;
            int multiplier = 0;
            int defense = 0;
            int damage = 0;

            switch (tokens[1])
            {
                case "Bows":
                    attack = attacker.Stats.Power.Value + Rng.RandomInt(0, 3);
                    multiplier = attacker.Stats.Reflex.Value * attacker.Stats.Level.Value / 256 + 2;
                    defense = target.Stats.Guts.Value;
                    break;

                case "Magic":
                    attack = commandRecord.Power + Rng.RandomInt(0, commandRecord.Power / 8);
                    multiplier = attacker.Stats.Magic.Value * attacker.Stats.Level.Value / 256 + 4;
                    defense = target.Stats.MagicDefense.Value;
                    break;

                case "Monster":
                    attack = attackData.Power < 0 ? attacker.Stats.Power.Value : attackData.Power;
                    attack += Rng.RandomInt(0, attack / 8);
                    multiplier = attacker.Stats.Power.Value;
                    defense = target.Stats.Guts.Value;
                    break;

                case "MonsterMagic":
                    attack = attackData.Power < 0 ? attacker.Stats.Magic.Value : attackData.Power;
                    attack += Rng.RandomInt(0, attack / 8);
                    multiplier = attacker.Stats.Magic.Value;
                    defense = target.Stats.MagicDefense.Value;
                    break;

                default:
                    attack = int.Parse(tokens[1]);
                    defense = 0;
                    multiplier = 1;
                    break;
            }

            if (multiTarget && battleScene.BattleViewModel.EnemyList.Count() > 1 &&
                               commandRecord.Targetting != TargetType.All &&
                               commandRecord.Targetting != TargetType.AllEnemies &&
                               commandRecord.Targetting != TargetType.AllAllies)
            {
                if ((target is BattlePlayer && battleScene.BattleViewModel.PlayerList.Where(x => !x.Dead).Count() > 1) || (target is BattleEnemy && battleScene.BattleViewModel.EnemyList.Count() > 1))
                {
                    attack /= 2;
                }
            }

            if (tokens.Length > 2)
            {
                ElementType element = (ElementType)Enum.Parse(typeof(ElementType), tokens[2]);

                if (target.Stats.ElementWeak.Any(x => x.Value == element))
                {
                    attack *= 2;
                    defense = 0;
                }

                if (target.Stats.ElementStrong.Any(x => x.Value == element))
                {
                    attack /= 2;
                }

                if (target.Stats.ElementImmune.Any(x => x.Value == element))
                {
                    attack = 0;
                }

                if (target.Stats.ElementAbsorb.Any(x => x.Value == element))
                {
                    int healing = attack * multiplier;
                    if (healing < 1) healing = 1;
                    if (healing > 9999) healing = 9999;

                    target.Heal(healing);

                    return;
                }
            }

            damage = (attack - defense) * multiplier;
            if (damage < 1) damage = 1;
            if (damage > 9999) damage = 9999;
            target.Damage(damage);
        }

        private void CalculateHealing(string[] tokens)
        {
            if (tokens.Length == 2) target.Heal(int.Parse(tokens[1]));
            else
            {
                int power = commandRecord.Power + Rng.RandomInt(0, commandRecord.Power / 8);
                int multiplier = attacker.Stats.Level.Value * attacker.Stats.Magic.Value / 256 + 4;

                int healing = power * multiplier;

                if (multiTarget &&
                               commandRecord.Targetting != TargetType.All &&
                               commandRecord.Targetting != TargetType.AllEnemies &&
                               commandRecord.Targetting != TargetType.AllAllies)
                {
                    if ((target is BattlePlayer && battleScene.BattleViewModel.PlayerList.Where(x => !x.Dead).Count() > 1) || (target is BattleEnemy && battleScene.BattleViewModel.EnemyList.Count() > 1))
                    {
                        healing /= 2;
                    }
                }

                if (healing < 1) healing = 1;
                if (healing > 9999) healing = 9999;

                target.Heal(healing);
            }
        }

        private void CalculateReplenish(string[] tokens)
        {
            target.Replenish(int.Parse(tokens[1]));
        }

        private void Flash(string[] tokens)
        {
            target.FlashColor(new Color(int.Parse(tokens[1]), int.Parse(tokens[1]), int.Parse(tokens[1])));
        }

        private void Attack(string[] tokens)
        {
            List<BattlePlayer> eligibleTargets = battleScene.BattleViewModel.PlayerList.FindAll(x => !x.Dead);
            target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];

            scriptParser.RunScript("Animate Attack\nWait 600\nSound Slash\nCenterEffect Bash $targetCenterX $targetCenterY 2\nOnHit Monster Strength\nFlash Red\nDamage Monster Blunt");
        }

        private void Flee(string[] tokens)
        {
            //StackDialogue("You flee...");

            var convoRecord = new ConversationRecord()
            {
                DialogueRecords = [ new DialogueRecord() { Text = "Escaped from battle." } ]
            };

			convoScene = new ConversationViewModel(battleScene, convoRecord, PriorityLevel.MenuLevel);
            scriptParser.BlockScript();
            convoScene.OnTerminated += battleScene.BattleViewModel.Close;
			battleScene.AddView(convoScene);

            // timeleft = 1000;
        }

        private void Dialogue(string[] tokens)
        {
            if (tokens.Length == 2)
            {
                convoScene = new ConversationViewModel(battleScene, ConversationRecord.CONVERSATIONS.First(x => x.Name == tokens[1]), PriorityLevel.MenuLevel);
                var unblock = scriptParser.BlockScript();
                convoScene.OnDialogueScrolled += new Action(unblock);
				battleScene.AddView(convoScene);

                // timeleft = 1000;
            }
            else
            {
                StackDialogue(String.Join(' ', tokens.Skip(1)));
            }
        }

        private void Analyze(string[] tokens)
        {
            var enemy = target as BattleEnemy;

            string analysis = $"{target.Stats.Name}: HP {target.Stats.HP}/{target.Stats.MaxHP}. " + target.Stats.Description.Value;
            StackDialogue(analysis, false);
        }

        private void StackDialogue(string text, bool autoProceed = true)
        {
            var convoRecord = new ConversationRecord()
            {
                DialogueRecords = [ new DialogueRecord() { Text = text } ]
            };

            convoScene = new ConversationViewModel(battleScene, convoRecord, PriorityLevel.MenuLevel);
            var unblock = scriptParser.BlockScript();
            if (autoProceed) convoScene.OnDialogueScrolled += new Action(unblock);
            else
            {
                CommandViewModel.ConfirmCooldown = true;
                convoScene.OnTerminated += new Action(() =>
                {
                    unblock();
                    CommandViewModel.ConfirmCooldown = false;
                });
            }
            battleScene.AddView(convoScene);

            if (autoProceed) timeleft = 1000;
        }
    }
}
