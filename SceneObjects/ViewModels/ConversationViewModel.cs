using System;
using System.Linq;

using Microsoft.Xna.Framework;

using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using AngelPearl.SceneObjects.Widgets;

namespace AngelPearl.SceneObjects.ViewModels
{
    public class ConversationViewModel : ViewModel, ISkippableWait
    {
        private ConversationRecord conversationRecord;
        private DialogueRecord currentDialogue;
        private int dialogueIndex;

        private CrawlText crawlText;

        public ScriptController ScriptController { get; set; }

        public bool AutoProceed { get; set; }
        public int AutoProceedLength { get; set; } = -1;
        public bool Skippable { get; set; } = true;
        public bool TerminateOnEnd { get; set; } = true;

		public event Action OnDialogueScrolled;

		public ConversationViewModel(Scene iScene, ConversationRecord iConversationRecord, PriorityLevel priorityLevel)
            : base(iScene, priorityLevel)
        {
            conversationRecord = iConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Portrait.Value = string.IsNullOrEmpty(currentDialogue.Portrait) ? GameSprite.Widgets_Ninepatches_Blank : Enum.Parse<GameSprite>(currentDialogue.Portrait);
            ShowPortrait.Value = Portrait.Value != GameSprite.Widgets_Ninepatches_Blank;
            Dialogue.Value = currentDialogue.Text;
            Window.Value = conversationRecord.Bounds;
			TerminateOnEnd = conversationRecord.TerminateOnEnd;
			Skippable = conversationRecord.Skippable;
            AutoProceedLength = conversationRecord.AutoProceedDuration;
            AutoProceed = AutoProceedLength > 0;

			if (currentDialogue.Script != null) RunScript(currentDialogue.Script);

			if (conversationRecord.SkipTransitions) LoadView(GameView.Conversation_FastConversationView);
			else LoadView(GameView.Conversation_ConversationView);

            crawlText = GetWidget<CrawlText>("ConversationText");

			if (AutoProceed && !Skippable)
			{
				parentScene.AddController(new SkippableWaitController(this.PriorityLevel, this, false, AutoProceedLength));
			}
        }

		public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // if (parentScene.PriorityLevel > PriorityLevel.GameLevel) return;

            if (crawlText.ReadyToProceed)
            {
                if (!ReadyToProceed.Value && !IsScriptRunning)
                {
                    if (!AutoProceed && Skippable && TerminateOnEnd) ShowProceed.Value = true;
                    ReadyToProceed.Value = true;
                }

                OnDialogueScrolled?.Invoke();
                OnDialogueScrolled = null;
            }

            if (!Closed && !ChildList.Any(x => x.Transitioning))
            {
                if (Input.CurrentInput.CommandPressed(Command.Confirm) && Skippable)
                {
                    Proceed();
                }
            }
        }

        public void Proceed()
        {
            if (!crawlText.ReadyToProceed)
            {
                crawlText.FinishText();
                FinishDialogue();
            }
            else NextDialogue();
        }

        public override void LeftClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget)
        {
            if (!Skippable) return;

            switch (clickWidget.Name)
            {
                case "ConversationText":
                    if (!crawlText.ReadyToProceed)
                    {
                        crawlText.FinishText();
                        FinishDialogue();
                    }
                    else if (!AutoProceed) NextDialogue();
                    break;
            }
        }

        private void FinishDialogue()
        {
            
        }

        public void NextDialogue()
        {
            dialogueIndex++;
            if (dialogueIndex >= conversationRecord.DialogueRecords.Length)
            {
                EndConversation();
                return;
            }

            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Dialogue.Value = currentDialogue.Text;
            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Portrait.Value = string.IsNullOrEmpty(currentDialogue.Portrait) ? GameSprite.Widgets_Ninepatches_Blank : (GameSprite)Enum.Parse(typeof(GameSprite), currentDialogue.Portrait);
            ShowPortrait.Value = Portrait.Value != GameSprite.Widgets_Ninepatches_Blank;
            ShowProceed.Value = ReadyToProceed.Value = false;

            if (currentDialogue.Script != null) RunScript(currentDialogue.Script);

            if (AutoProceed && !Skippable)
            {
                parentScene.AddController(new SkippableWaitController(PriorityLevel.GameLevel, this, false, AutoProceedLength));
            }
        }

        public void EndConversation()
        {
            if (conversationRecord.EndScript != null)
            {
                parentScene.AddController(new ConversationController(parentScene, this.PriorityLevel, conversationRecord.EndScript)).OnTerminated += new TerminationFollowup(() =>
                {
					if (TerminateOnEnd) Close();
				});
			}
            else if (TerminateOnEnd) Close();
        }

        public void ChangeConversation(ConversationRecord newConversationRecord)
        {
            dialogueIndex = 0;

            conversationRecord = newConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Portrait.Value = string.IsNullOrEmpty(currentDialogue.Portrait) ? GameSprite.Widgets_Ninepatches_Blank : (GameSprite)Enum.Parse(typeof(GameSprite), currentDialogue.Portrait);
            ShowPortrait.Value = Portrait.Value != GameSprite.Widgets_Ninepatches_Blank;
            Dialogue.Value = currentDialogue.Text;

            ShowProceed.Value = ReadyToProceed.Value = false;

            ScriptController?.Terminate();
            if (currentDialogue.Script != null) RunScript(currentDialogue.Script);
            else ScriptController = null;

        }

        public void Notify(SkippableWaitController sender)
        {
            Proceed();
        }

        private void RunScript(string[] script)
        {
            ScriptController = parentScene.AddController(new ConversationController(parentScene, this.PriorityLevel, script));
        }

        private bool IsScriptRunning => ScriptController != null && !ScriptController.Terminated; 

        public ModelProperty<Rectangle> Window { get; set; } = new ModelProperty<Rectangle>(ConversationRecord.DEFAULT_CONVO_BOUNDS);
        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<GameFont> ConversationFont { get; set; } = new ModelProperty<GameFont>(GameFont.Dialogue);
        public ModelProperty<string> Dialogue { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Speaker { get; set; } = new ModelProperty<string>("");
        public ModelProperty<GameSprite> Portrait { get; set; } = new ModelProperty<GameSprite>(GameSprite.Widgets_Ninepatches_Blank);

        public ModelProperty<bool> ShowPortrait { get; set; } = new ModelProperty<bool>(false);
		public ModelProperty<bool> ShowProceed { get; set; } = new ModelProperty<bool>(false);
	}
}
