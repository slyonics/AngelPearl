using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects.ViewModels;

namespace AngelPearl.SceneObjects.Controllers
{
    public class ConversationController : ScriptController
    {
        private Scene conversationScene;

        public bool EndGame { get; private set; }

        public ConversationController(Scene iScene, PriorityLevel priorityLevel, string script)
            : base(iScene, script, priorityLevel)
        {
            conversationScene = iScene;
        }

        public ConversationController(Scene iScene, PriorityLevel priorityLevel, string[] script)
            : base(iScene, script, priorityLevel)
        {
            conversationScene = iScene;
        }

        public ConversationViewModel ConversationViewModel { get; set; }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
				case "EndGame": EndGame = true; break;

                case "WaitForText": WaitForText(tokens); return false;
                case "ProceedText": ConversationViewModel.NextDialogue(); break;
                case "SelectionPrompt": SelectionPrompt(tokens); return false;
				case "ChangeConversation": ChangeConversation(tokens); break;
                case "EndConversation": ConversationViewModel.Close(); break;
                case "ChangeScene": ChangeScene(tokens); break;
                case "SetAutoProceed": ConversationViewModel.AutoProceedLength = int.Parse(tokens[1]); break;
                case "SetSkippable": ConversationViewModel.Skippable = bool.Parse(tokens[1]); break;

                    /*
                case "ChangeMap": MapScene.EventController.ChangeMap(tokens, MapScene.MapScene.Instance); break;
                //case "SpawnMonster": MapScene.EventController.SpawnMonster(tokens, MapScene.MapScene.Instance); break;
                case "TurnParty": MapScene.EventController.Turn(tokens, MapScene.MapScene.Instance); break;
                case "MoveParty": MapScene.EventController.Move(tokens, MapScene.MapScene.Instance); break;
                case "WaitParty": MapScene.MapScene.Instance.CaterpillarController.FinishMovement = scriptParser.BlockScript(); return false;
                case "Idle": MapScene.MapScene.Instance.CaterpillarController.Idle(); break;
                case "AnimateHero": MapScene.MapScene.Instance.Party[int.Parse(tokens[1])].PlayAnimation(tokens[2], new AnimationFollowup(() => { })); break;
                case "RestoreMP": foreach (var hero in GameProfile.CurrentSave.Party) hero.Value.MP.Value = hero.Value.MaxMP.Value; break;
                case "ResetTrigger": EventTrigger.LastTrigger.Terminated = false; MapScene.MapScene.Instance.EventTriggers.Add(EventTrigger.LastTrigger); break;
                    */

                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            switch (parameter)
            {
                case "$leftPortraitX": return (((int)(CrossPlatformGame.SCREEN_WIDTH / 1.6) - 60) / 2).ToString();
                case "$rightPortraitX": return (CrossPlatformGame.SCREEN_WIDTH - ((int)(CrossPlatformGame.SCREEN_WIDTH / 1.6) - 60) / 2).ToString();
                case "$portraitY": return ((int)(CrossPlatformGame.SCREEN_HEIGHT)).ToString();
                case "$portraitScaleX": return (CrossPlatformGame.SCREEN_WIDTH / 1920.0f / 1.6f).ToString();
                case "$portraitScaleY": return (CrossPlatformGame.SCREEN_HEIGHT / 1080.0f / 1.6f).ToString();
                default: return base.ParseParameter(parameter);
            }
        }
                
		private void WaitForText(string[] tokens)
        {
            ScriptParser.UnblockFollowup followup = scriptParser.BlockScript();
            ConversationViewModel.OnDialogueScrolled += new Action(followup);
        }

        private void SelectionPrompt(string[] tokens)
        {
            List<string> options = new List<string>();
            string skipLine;
            do
            {
                skipLine = scriptParser.DequeueNextCommand();
                options.Add(skipLine);
            } while (skipLine != "End");
            options.RemoveAt(options.Count - 1);

            SelectionViewModel selectionViewModel = new SelectionViewModel(conversationScene, options);
            conversationScene.AddOverlay(selectionViewModel);

            ScriptParser.UnblockFollowup followup = scriptParser.BlockScript();
            selectionViewModel.OnTerminated += new Action(followup);
        }

		private void ChangeConversation(string[] tokens)
        {
            var conversationData = ConversationRecord.CONVERSATIONS.FirstOrDefault(x => x.Name == tokens[1]);

            string[] conversationScript = conversationData.DialogueRecords[0].Script;
            if (conversationScript != null) scriptParser.RunScript(conversationData.DialogueRecords[0].Script);

            ConversationViewModel.ChangeConversation(conversationData);
        }

        private void ChangeScene(string[] tokens)
        {
            switch (tokens[1])
            {
                case "Conversation":
					//CrossPlatformGame.Transition(new Task<Scene>(() => new ConversationScene(tokens[2])));
					break;

                    /*
                case "Title":
					CrossPlatformGame.Transition(new Task<Scene>(() => new TitleScene.TitleScene(tokens[2])));
					break;
                    */
            }
        }
	}
}
