using AngelPearl.Main;
using AngelPearl.Models;
using AngelPearl.SceneObjects;
using AngelPearl.SceneObjects.Controllers;
using ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelPearl.Scenes.CrawlerScene
{
    public interface IBillboard
    {
        public Vector3 Position { get; }
        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, float cameraX);
    }

    public enum FoeBehavior
    {
        Idle,
        Traveling,
        Searching,
        Chasing,
        Wandering
    }

    public enum FoeAlignment
    {
        Adventurer,
        Brute,
        Tricker,
        Monster
    }

    public class Foe : IBillboard
    {
        private static Texture2D enemyIndicator = AssetCache.SPRITES[GameSprite.Widgets_Images_FoeMarker];
        private static readonly Rectangle[] enemySource = Extensions.BuildSources(4, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE);

        public static Texture2D DeferredSprite { get; set; } = null;

        private CrawlerScene crawlerScene;
        public Billboard Billboard { get; private set; }

        public Direction Direction { get; private set; }
        public MapRoom CurrentRoom { get; private set; }
        public MapRoom DestinationRoom { get; private set; }
        public float MoveInterval { get; set; }

        public string Encounter { get; set; }


        public FoeBehavior Behavior { get; private set; } = FoeBehavior.Idle;
        public FoeAlignment Alignment { get; private set; }

        public string Label { get; private set; }

        public MapRoom TargetRoom { get; private set; }

        public string[] Script { get; private set; }

        private Texture2D foeTexture;

        private int minimapId = 0;

        public Foe(CrawlerScene iScene, Floor iFloor, EntityInstance entity)
        {
            crawlerScene = iScene;

            string sprite = "";
            foreach (FieldInstance field in entity.FieldInstances)
            {
                switch (field.Identifier)
                {
                    case "Encounter": Encounter = field.Value; break;
                    case "Interact": if (!string.IsNullOrEmpty(field.Value)) Script = field.Value.Split('\n'); break;
                    case "Sprite": sprite = field.Value; break;
                    case "Label": Label = field.Value; break;
                    case "Alignment": if (field.Value != null) Alignment = Enum.Parse<FoeAlignment>((string)field.Value); break;
                }
            }

            if (Alignment == FoeAlignment.Brute)
            {
                minimapId = 1;
            }

            int TileSize = iFloor.TileSize;
            int startX = (int)((entity.Px[0] + entity.Width / 2) / TileSize);
            int startY = (int)((entity.Px[1] + entity.Height / 2) / TileSize);

            CurrentRoom = iFloor.GetRoom(startX, startY);
            CurrentRoom.Foe = this;

            foeTexture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + sprite)];
            float sizeX = foeTexture.Width / 9;
            float sizeY = foeTexture.Height / 9;
            Billboard = new Billboard(crawlerScene, iFloor, foeTexture, sizeX, sizeY);
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, float cameraX)
        {
            if (CurrentRoom.Obscured) return;

            //var battleScene = CrossPlatformGame.CurrentScene as BattleScene.BattleScene;
            //if (battleScene != null && battleScene.Foe == this) return;

            float x = 10 * CurrentRoom.RoomX;
            float z = 10 * (crawlerScene.Floor.MapHeight - CurrentRoom.RoomY);
            float brightness = CurrentRoom.AverageBrightness();

            if (IsMoving)
            {
                float t = MoveInterval;
                if (crawlerScene.PartyController.Moving)
                {
                    t = crawlerScene.PartyController.MoveInterval;
				}

                float destX = 10 * DestinationRoom.RoomX;
                float destZ = 10 * (crawlerScene.Floor.MapHeight - DestinationRoom.RoomY);
                float destBrightness = CurrentRoom.AverageBrightness();

                Billboard.Draw(viewMatrix, MathHelper.Lerp(x, destX, MoveInterval), MathHelper.Lerp(z, destZ, MoveInterval), cameraX, MathHelper.Lerp(brightness, destBrightness, MoveInterval));
            }
            else
            {
                if (!crawlerScene.PartyController.Moving && !crawlerScene.Suspended && crawlerScene.PartyController.FacingRoom?.Foe == this)
                {
                    Foe.DeferredSprite = foeTexture;
                }
                else Billboard.Draw(viewMatrix, x, z, cameraX, brightness);
            }
        }

        public void DrawMinimap(SpriteBatch spriteBatch, Vector2 offset, Color color, float depth)
        {
            spriteBatch.Draw(enemyIndicator, offset, enemySource[minimapId], color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth - 0.002f);
        }

        public void Interact()
        {
			switch (Alignment)
            {
                case FoeAlignment.Brute:
                    crawlerScene.StartBattle(this);
                    
                    break;
            }
			
            
		}

        public bool Threaten()
        {
			switch (Behavior)
			{
				case FoeBehavior.Chasing:
                    if (crawlerScene.Floor.GetPath(CurrentRoom, crawlerScene.PartyController.CurrentRoom).Skip(1).FirstOrDefault() == crawlerScene.PartyController.CurrentRoom)
                    {
                        crawlerScene.StartBattle(this);


                        return true;
                    }

					break;
			}

            return false;
		}

        public void Move(MapRoom playerDestination)
        {
            switch (Alignment)
            {
                case FoeAlignment.Brute:
                    if (Behavior == FoeBehavior.Idle && !CurrentRoom.Obscured) Behavior = FoeBehavior.Chasing;
                    break;
            }

            DestinationRoom = null;

            switch (Behavior)
            {
                case FoeBehavior.Wandering:
                    {
                        Direction moveDirection = (Direction)Rng.RandomInt(0, 3);
                        var newRoom = CurrentRoom[moveDirection];
                        if (newRoom == null || newRoom.Blocked || newRoom.Chest != null || newRoom == playerDestination) return;

                        Direction = moveDirection;
                        DestinationRoom = newRoom;
                    }
                    break;

                case FoeBehavior.Chasing:
                    {
                        var newRoom = crawlerScene.Floor.GetPath(CurrentRoom, crawlerScene.PartyController.CurrentRoom).Skip(1).FirstOrDefault();
						if (newRoom == null || newRoom.Blocked || newRoom.Chest != null) return;

                        if (newRoom == playerDestination)
                        {
                            crawlerScene.FoeInBattle = this;
                            return;
                        }

                        DestinationRoom = newRoom;
                    }
					break;
            }

            if (DestinationRoom == null) return;

            MoveInterval = 0.0f;

            TransitionController controller = new TransitionController(TransitionDirection.In, 300, PriorityLevel.CutsceneLevel);
            crawlerScene.AddController(controller);
            controller.UpdateTransition += new Action<float>(t => MoveInterval = t >= 0.5f ? 1.0f : 0.5f);
            controller.FinishTransition += new Action<TransitionDirection>(t => { CurrentRoom.Foe = null; CurrentRoom = DestinationRoom; CurrentRoom.Foe = this; DestinationRoom = null; });
        }

        public void Destroy()
        {
            crawlerScene.FoeList.Remove(this);
            CurrentRoom.Foe = null;
        }

        public Vector3 Position
        {
            get
            {
                float x = 10 * CurrentRoom.RoomX;
                float z = 10 * (crawlerScene.Floor.MapHeight - CurrentRoom.RoomY);

                if (IsMoving)
                {
                    float destX = 10 * DestinationRoom.RoomX;
                    float destZ = 10 * (crawlerScene.Floor.MapHeight - DestinationRoom.RoomY);
                    return new Vector3(MathHelper.Lerp(x, destX, MoveInterval), 0, MathHelper.Lerp(z, destZ, MoveInterval));
                }
                else
                {
                    return new Vector3(x, 0, z);
                }
            }
        }

        public bool IsMoving { get => DestinationRoom != null; }
    }
}
