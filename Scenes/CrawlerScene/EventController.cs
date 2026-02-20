using Microsoft.Xna.Framework;
using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using AngelPearl.SceneObjects.Shaders;
using AngelPearl.SceneObjects.ViewModels;
using AngelPearl.Scenes.MapScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.CrawlerScene
{
    public class EventController : ScriptController
    {
        private CrawlerScene crawlerScene;

        public bool EndGame { get; private set; }

        MapRoom mapRoom = null;


        public EventController(CrawlerScene iScene, string[] script, MapRoom iMapRoom)
            : base(iScene, script, PriorityLevel.CutsceneLevel)
        {
            crawlerScene = iScene;
            mapRoom = iMapRoom;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "GameEvent": GameEvent(tokens); break;
                case "EndGame": EndGame = true; break;
                case "ChangeMap": ChangeMap(tokens); /*Audio.PlaySound(GameSound.sfx_stairs_down);*/ break;
                case "DisableEvent": mapRoom.Script = null; break;
                case "Conversation": Conversation(tokens); break;
                case "Encounter": Encounter(tokens, scriptParser); break;
                case "GiveItem": GiveItem(tokens); break;
                case "RemoveChest": RemoveChest(tokens); break;
                case "RemoveNpc": RemoveNpc(tokens); break;
                case "ShowPortrait": ShowPortrait(tokens); break;
                case "MoveBackward": crawlerScene.PartyController.MoveBackward(); break;
                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            if (parameter.Contains("Flag."))
            {
                return GameProfile.GetSaveData<bool>(parameter.Split('.')[1]).ToString();
            }
            else return base.ParseParameter(parameter);
        }

        private void GameEvent(string[] tokens)
        {
            switch (tokens[1])
            {

            }
        }

        private void ChangeMap(string[] tokens)
        {
            Task<Scene> sceneTask;

            sceneTask = new Task<Scene>(() => new CrawlerScene(tokens[1], crawlerScene.MapFileName.ToString()));

            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 800);
            transitionController.UpdateTransition += new Action<float>(t =>
            {
                crawlerScene.GlobalBrightness = (int)(t * 4) / 4.0f;
                crawlerScene.InterfacePaletteShader.SetGlobalBrightness(MathHelper.SmoothStep(-1.01f, 0.0f, t));
            });
			transitionController.FinishTransition += new Action<TransitionDirection>(t => crawlerScene.GlobalBrightness = 0.0f);

			CrossPlatformGame.Transition(crawlerScene, sceneTask, transitionController, null);

			crawlerScene.MapViewModel.InteractLabel.Value = "";
			crawlerScene.MapViewModel.ShowInteractLabel.Value = false;
		}

        private void Conversation(string[] tokens)
        {
            crawlerScene.MapViewModel.ShowMiniMap.Value = false;

            if (tokens.Length == 2)
            {
                ConversationRecord conversationRecord = ConversationRecord.CONVERSATIONS.First(x => x.Name == tokens[1]);
                ConversationViewModel conversationViewModel = crawlerScene.AddView(new ConversationViewModel(crawlerScene, conversationRecord, PriorityLevel.CutsceneLevel));
				conversationViewModel.OnTerminated += new Action(() =>
				{
					scriptParser.BlockScript();
					crawlerScene.MapViewModel.ShowMiniMap.Value = true;
				});
			}
            else
            {
                ConversationRecord conversationRecord = new ConversationRecord(string.Join(' ', tokens.Skip(1)));
                ConversationViewModel conversationViewModel = crawlerScene.AddView(new ConversationViewModel(crawlerScene, conversationRecord, PriorityLevel.CutsceneLevel));
                conversationViewModel.OnTerminated += new Action(() =>
                {
                    scriptParser.BlockScript();
					crawlerScene.MapViewModel.ShowMiniMap.Value = true;
				});
            }
        }

        public void GiveItem(string[] tokens)
        {
            ItemRecord item = ItemRecord.ITEMS.First(x => x.Name == string.Join(' ', tokens.Skip(1)));

            if (item.ItemType == ItemType.Medicine || item.ItemType == ItemType.Consumable) GameProfile.CurrentSave.AddInventory(item.Name, 1);
            else GameProfile.CurrentSave.AddInventory(item.Name, -1);

            ConversationRecord conversationRecord = new ConversationRecord($"Found @{item.Icon} {item.Name}!");
            ConversationViewModel conversationViewModel = crawlerScene.AddView(new ConversationViewModel(crawlerScene, conversationRecord, PriorityLevel.CutsceneLevel));
			conversationViewModel.OnTerminated += new Action(() =>
			{
				scriptParser.BlockScript();
				crawlerScene.MapViewModel.ShowMiniMap.Value = true;
			});

			crawlerScene.MapViewModel.ShowMiniMap.Value = false;

			// Audio.PlaySound(GameSound.sfx_item_pickup);
		}

        public static void Encounter(string[] tokens, ScriptParser scriptParser)
        {
            /*
            BattleScene.BattleScene battleScene = new BattleScene.BattleScene(tokens[1], null);
            battleScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(battleScene, true);
            */
        }

        public void RemoveChest(string[] tokens)
        {
            var chest = crawlerScene.ChestList.FirstOrDefault(x => x.Name == tokens[1]);
            chest?.Destroy();
            crawlerScene.ChestList.Remove(chest);
        }

        public void RemoveNpc(string[] tokens)
        {
            var npc = crawlerScene.NpcList.FirstOrDefault(x => x.Name == tokens[1]);
            npc?.Destroy();
            crawlerScene.NpcList.Remove(npc);
        }

        public void ShowPortrait(string[] tokens)
        {

        }
    }
}
