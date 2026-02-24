using AngelPearl.SceneObjects.Controllers;
using AngelPearl.Scenes.CrawlerScene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;

namespace AngelPearl.SceneObjects.Widgets
{
	public class Panel : Widget
	{
		private enum TransitionType
		{
			None,
			Shrink,
			Expand,
			FadeIn,
			FadeOut
		}

		private enum ResizeType
		{
			None,
			Width,
			Height,
			Both
		}


		public override Rectangle Bounds
		{
			get => bounds;
			set
			{
				currentWindow = bounds = value;

				UpdateFrame();
				foreach (var child in ChildList) child.ApplyAlignment();
			}
		}

		private NinePatch panelFrame;

		private TransitionType TransitionIn { get; set; } = TransitionType.None;
		private TransitionType TransitionOut { get; set; } = TransitionType.None;

		private Rectangle startWindow;
		private Rectangle endWindow;
		private Color startColor;
		private Color endColor;

		private ResizeType Resize { get; set; } = ResizeType.None;

		public Panel(Widget iParent, float widgetDepth)
			: base(iParent, widgetDepth)
		{

		}

		public override void LoadAttributes(XmlNode xmlNode)
		{
			foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
			{
				ParseAttribute(xmlAttribute.Name, xmlAttribute.Value);
			}

			UpdateFrame();
		}

		public void UpdateFrame()
		{
			if (style != null)
			{
				if (panelFrame == null) panelFrame = new NinePatch(style, Depth);
				panelFrame.SetSprite(style);
				panelFrame.FrameDepth = Depth;
				panelFrame.Bounds = Bounds;
			}
		}

        public override void LoadChildren(XmlNodeList nodeList, float widgetDepth)
		{
			base.LoadChildren(nodeList, widgetDepth);

			StartTransitionIn();

			if (panelFrame != null) panelFrame.Bounds = currentWindow;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if ((!Transitioning || TransitionIn != TransitionType.Expand) && !terminated)
			{
				foreach (Widget widget in ChildList)
				{
					if (widget.Visible)
						widget.Draw(spriteBatch);
				}

				tooltipWidget?.Draw(spriteBatch);
			}

			panelFrame?.Draw(spriteBatch, Position);
		}

		public override void Close()
		{
			base.Close();

			StartTransitionOut();
		}

		public void StartTransitionIn()
		{
			switch (TransitionIn)
			{
				case TransitionType.Expand:
					endWindow = currentWindow;
					currentWindow = startWindow = new Rectangle((int)currentWindow.Center.X, (int)currentWindow.Center.Y, 0, 0);
					transition = new TransitionController(TransitionDirection.In, DEFAULT_TRANSITION_LENGTH);
					transition.UpdateTransition += UpdateTransition;
					transition.FinishTransition += FinishTransition;
					GetParent<ViewModel>().ParentScene.AddController(transition);
					break;

				case TransitionType.FadeIn:
					endWindow = startWindow = currentWindow;
					startColor = new Color(0, 0, 0, 0);
					endColor = new Color(255, 255, 255, 255);
					transition = new TransitionController(TransitionDirection.In, DEFAULT_TRANSITION_LENGTH);
					transition.UpdateTransition += UpdateTransition;
					transition.FinishTransition += FinishTransition;
					GetParent<ViewModel>().ParentScene.AddController(transition);
					break;
			}
		}

		public void StartTransitionOut()
		{
			switch (TransitionOut)
			{
				case TransitionType.Shrink:
					endWindow = currentWindow;
					startWindow = new Rectangle((int)currentWindow.Center.X, (int)currentWindow.Center.Y, 0, 0);
					transition = new TransitionController(TransitionDirection.Out, DEFAULT_TRANSITION_LENGTH);
					transition.UpdateTransition += UpdateTransition;
					transition.FinishTransition += FinishTransition;
					GetParent<ViewModel>().ParentScene.AddController(transition);
					break;
			}
		}

		private void UpdateTransition(float transitionProgress)
		{
			currentWindow = Extensions.Lerp(startWindow, endWindow, transitionProgress);

			if (panelFrame != null) panelFrame.Bounds = currentWindow;
		}

		private void FinishTransition(TransitionDirection transitionDirection)
		{
			transition = null;
			if (Closed) Terminate();
		}

		private string style;
		public string Style { get => style; set { style = value; UpdateFrame(); } }


        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            base.EndLeftClick(mouseStart, mouseEnd, otherWidget);

            if (otherWidget == this)
            {
                GetParent<ViewModel>().LeftClickChild(mouseStart, mouseEnd, this, otherWidget);
            }
        }

		private float depth = 1.0f;
		public override float Depth
		{
			get => depth;
			set
			{
				depth = value;
				UpdateFrame();
			}
		}

	}
}
