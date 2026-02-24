using ldtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AngelPearl.Main;
using AngelPearl.Scenes.MapScene;
using System;
using System.Collections.Generic;

using System.Linq;

namespace AngelPearl.Scenes.CrawlerScene
{
    public class MapRoom
    {
        public class RoomWall
        {
            public Direction Orientation { get; set; }
            public VertexPositionColorTexture[] Quad { get; set; }
            public Texture2D Texture { get; set; }

            public WallShader Shader { get; set; }

            public RoomWall(Direction iOrientation, Texture2D iTexture, float startU, float startV, float endU, float endV, float heightOffset = 0.0f)
            {
                Orientation = iOrientation;
                Texture = iTexture;

                VertexPositionColorTexture[] quad =
				[
					new VertexPositionColorTexture(VERTICES[Orientation][0] - new Vector3(0, heightOffset, 0), Color.White, new Vector2(startU, startV)),
					new VertexPositionColorTexture(VERTICES[Orientation][1] - new Vector3(0, heightOffset, 0), Color.White, new Vector2(startU, endV)),
					new VertexPositionColorTexture(VERTICES[Orientation][2] - new Vector3(0, heightOffset, 0), Color.White, new Vector2(endU, endV)),
					new VertexPositionColorTexture(VERTICES[Orientation][3] - new Vector3(0, heightOffset, 0), Color.White, new Vector2(endU, startV)),
				];
				Quad = quad;
            }

            public RoomWall(Direction iOrientation, Texture2D iTexture, Dictionary<Direction, Vector3[]> vertices, float startU, float startV, float endU, float endV)
            {
                Orientation = iOrientation;
                Texture = iTexture;

				VertexPositionColorTexture[] quad =
				[
					new VertexPositionColorTexture(vertices[Orientation][0], Color.White, new Vector2(startU, startV)),
					new VertexPositionColorTexture(vertices[Orientation][1], Color.White, new Vector2(startU, endV)),
					new VertexPositionColorTexture(vertices[Orientation][2], Color.White, new Vector2(endU, endV)),
					new VertexPositionColorTexture(vertices[Orientation][3], Color.White, new Vector2(endU, startV)),
				];
				Quad = quad;
            }
        }

        private const int WALL_HALF_LENGTH = 5;
        private const int CAM_HEIGHT = -1;
        private static readonly short[] INDICES = [0, 2, 1, 2, 0, 3];
        private static readonly Dictionary<Direction, Vector3[]> VERTICES = new Dictionary<Direction, Vector3[]>()
        {   
            {
                Direction.North, new Vector3[]
                {
                    new (-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
					new (-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new (WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new (WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH)
                }
            },
            {
                Direction.West, new Vector3[]
                {
                    new (-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new (-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new (-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new (-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH)
                }
            },
            {
                Direction.East, new Vector3[]
                {
                    new (WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new (WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new (WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new (WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH)
                }
            },
            {
                Direction.South, new Vector3[]
                {
                    new (WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new (WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new (-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new (-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH)
                }
            },
            {
                Direction.Up, new Vector3[]
                {
                    new (WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new (WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new (-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new (-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH)
                }
            },
            {
                Direction.Down, new Vector3[]
                {
                    new (-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new (-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new (WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new (WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH)
                }
            }
        };

        private static Texture2D minimapSprite = AssetCache.SPRITES[GameSprite.Widgets_Images_MiniMap];
        private static readonly Rectangle[] minimapSource = Extensions.BuildSources(8, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE);

		public static WallShader Shader { get; set; }

		public int RoomX { get; set; }
        public int RoomY { get; set; }


        public bool Blocked { get; set; }
        public bool Occluding { get; set; }
        public bool HasFloor { get; set; }
        public bool HasCeiling { get; set; }

        public bool Obscured { get; set; } = true;

        public bool Discovered { get; set; } = false;


        public string Label { get; set; }
        public string[] Script { get; set; }
        public string[] PreEnterScript { get; set; }
        public string[] InteractScript { get; set; }

        private GraphicsDevice graphicsDevice = CrossPlatformGame.GameInstance.GraphicsDevice;

        private CrawlerScene crawlerScene;
        private Floor parentFloor;

        int waypointTile = 1;

        private Dictionary<Direction, RoomWall> wallList = [];

		public int brightnessLevel = 0;
        private float[] lightVertices;

        public Foe Foe { get; set; }

        public Door Door { get; set; }

        private Chest chest;
        public Chest Chest
        {
            get => chest;
            set
            {
                chest = value;
                waypointTile = (chest == null) ? 1 : 4;
            }
        }

        public MapRoom(CrawlerScene mapScene, Floor iFloor, int x, int y)
        {
            crawlerScene = mapScene;
            parentFloor = iFloor;
            RoomX = x;
            RoomY = y;
        }

        public void ApplyTile(string layerName, TilesetDefinition tileset, TileInstance tile)
        {

            switch (layerName)
            {
                case "Walls":
                    {
                        Blocked = true;
                        Occluding = true;

						waypointTile = 0;

						string tilesetName = tileset.RelPath.Replace("../Graphics/", "").Replace(".png", "").Replace('/', '_');
                        var SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tilesetName)];
                        float startU = tile.Src[0] / (float)SpriteAtlas.Width;
                        float startV = tile.Src[1] / (float)SpriteAtlas.Height;
                        float endU = startU + parentFloor.TileSize / (float)SpriteAtlas.Width;
                        float endV = startV + parentFloor.TileSize / (float)SpriteAtlas.Height;

                        var westWall = this[Direction.West];
                        if (RoomX > 0 && westWall != null && !westWall.Occluding)
                            westWall.ApplyWall(Direction.East, SpriteAtlas, startU, startV, endU, endV);

                        var eastWall = this[Direction.East];
                        if (RoomX < parentFloor.MapWidth - 1 && eastWall != null && !eastWall.Occluding)
                            eastWall.ApplyWall(Direction.West, SpriteAtlas, startU, startV, endU, endV);

                        var northWall = this[Direction.North];
                        if (RoomY > 0 && northWall != null && !northWall.Occluding)
                            northWall.ApplyWall(Direction.South, SpriteAtlas, startU, startV, endU, endV);

                        var southWall = this[Direction.South];
                        if (RoomY < parentFloor.MapHeight - 1 && southWall != null && !southWall.Occluding)
                            southWall.ApplyWall(Direction.North, SpriteAtlas, startU, startV, endU, endV);
                    }
                    break;

                case "Ceiling":
                    {
                        HasCeiling = true;

                        string tilesetName = tileset.RelPath.Replace("../Graphics/", "").Replace(".png", "").Replace('/', '_');
                        var SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tilesetName)];
                        float startU = tile.Src[0] / (float)SpriteAtlas.Width;
                        float startV = tile.Src[1] / (float)SpriteAtlas.Height;
                        float endU = startU + parentFloor.TileSize / (float)SpriteAtlas.Width;
                        float endV = startV + parentFloor.TileSize / (float)SpriteAtlas.Height;
                        wallList.Add(Direction.Up, new RoomWall(Direction.Up, SpriteAtlas, startU, startV, endU, endV));
                    }
                    break;

                case "Floor":
                    {
                        HasFloor = true;

                        string tilesetName = tileset.RelPath.Replace("../Graphics/", "").Replace(".png", "").Replace('/', '_');
                        var SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tilesetName)];
                        float startU = tile.Src[0] / (float)SpriteAtlas.Width;
                        float startV = tile.Src[1] / (float)SpriteAtlas.Height;
                        float endU = startU + parentFloor.TileSize / (float)SpriteAtlas.Width;
                        float endV = startV + parentFloor.TileSize / (float)SpriteAtlas.Height;
                        wallList.Add(Direction.Down, new RoomWall(Direction.Down, SpriteAtlas, startU, startV, endU, endV));

                        var terrain = tileset.EnumTags.FirstOrDefault(x => x.TileIds.Contains(tile.T));
                        if (terrain != null)
                        {
                            switch (terrain.EnumValueId)
                            {
                                case "Blocked":
                                    Blocked = true;
                                    break;
                            }
                        }
                    }
                    break;
            }

			var customData = tileset.CustomData.FirstOrDefault(x => x.TileId == tile.T);
			if (customData != null)
			{
				foreach (string line in customData.Data.Split('\n'))
				{
					if (line.Contains("Minimap"))
					{
						waypointTile = int.Parse(line.Split(' ').Last());
						return;
					}
				}
			}
        }

        public void ApplyWall(Direction direction, Texture2D texture2D, float startU, float startV, float endU, float endV)
        {
            if (wallList.TryGetValue(direction, out var wall))
            {
                wall.Texture = texture2D;
                throw new Exception();
            }
            else
            {
                wallList.Add(direction, new RoomWall(direction, texture2D, startU, startV, endU, endV));
            }
        }

		public void SetVertices(int x, int z)
        {
            BuildShader(x, z);
        }

        private void BuildShader(int x, int z)
        {
            foreach (KeyValuePair<Direction, RoomWall> wall in wallList)
            {
                for (int i = 0; i < wall.Value.Quad.Length; i++)
                {
					wall.Value.Quad[i].Position += new Vector3(CrawlerScene.ROOM_LENGTH * x, 0, CrawlerScene.ROOM_LENGTH * (parentFloor.MapHeight - z));
				}

				switch (wall.Value.Orientation)
				{
					case Direction.Down:
                        wall.Value.Quad[0].Color = new Color(Brightness(lightVertices[0]), Brightness(lightVertices[0]), Brightness(lightVertices[0]), 1.0f);
						wall.Value.Quad[1].Color = new Color(Brightness(lightVertices[2]), Brightness(lightVertices[2]), Brightness(lightVertices[2]), 1.0f);
						wall.Value.Quad[2].Color = new Color(Brightness(lightVertices[3]), Brightness(lightVertices[3]), Brightness(lightVertices[3]), 1.0f);
						wall.Value.Quad[3].Color = new Color(Brightness(lightVertices[1]), Brightness(lightVertices[1]), Brightness(lightVertices[1]), 1.0f);
						break;

					case Direction.North:
						wall.Value.Quad[0].Color = new Color(Brightness(lightVertices[0]), Brightness(lightVertices[0]), Brightness(lightVertices[0]), 1.0f);
						wall.Value.Quad[1].Color = new Color(Brightness(lightVertices[0]), Brightness(lightVertices[0]), Brightness(lightVertices[0]), 1.0f);
						wall.Value.Quad[2].Color = new Color(Brightness(lightVertices[1]), Brightness(lightVertices[1]), Brightness(lightVertices[1]), 1.0f);
						wall.Value.Quad[3].Color = new Color(Brightness(lightVertices[1]), Brightness(lightVertices[1]), Brightness(lightVertices[1]), 1.0f);
						break;

					case Direction.East:
						wall.Value.Quad[0].Color = new Color(Brightness(lightVertices[1]), Brightness(lightVertices[1]), Brightness(lightVertices[1]), 1.0f);
						wall.Value.Quad[1].Color = new Color(Brightness(lightVertices[1]), Brightness(lightVertices[1]), Brightness(lightVertices[1]), 1.0f);
						wall.Value.Quad[2].Color = new Color(Brightness(lightVertices[3]), Brightness(lightVertices[3]), Brightness(lightVertices[3]), 1.0f);
						wall.Value.Quad[3].Color = new Color(Brightness(lightVertices[3]), Brightness(lightVertices[3]), Brightness(lightVertices[3]), 1.0f);
						break;

					case Direction.South:
						wall.Value.Quad[0].Color = new Color(Brightness(lightVertices[3]), Brightness(lightVertices[3]), Brightness(lightVertices[3]), 1.0f);
						wall.Value.Quad[1].Color = new Color(Brightness(lightVertices[3]), Brightness(lightVertices[3]), Brightness(lightVertices[3]), 1.0f);
						wall.Value.Quad[2].Color = new Color(Brightness(lightVertices[2]), Brightness(lightVertices[2]), Brightness(lightVertices[2]), 1.0f);
						wall.Value.Quad[3].Color = new Color(Brightness(lightVertices[2]), Brightness(lightVertices[2]), Brightness(lightVertices[2]), 1.0f);
						break;

					case Direction.West:
						wall.Value.Quad[0].Color = new Color(Brightness(lightVertices[2]), Brightness(lightVertices[2]), Brightness(lightVertices[2]), 1.0f);
						wall.Value.Quad[1].Color = new Color(Brightness(lightVertices[2]), Brightness(lightVertices[2]), Brightness(lightVertices[2]), 1.0f);
						wall.Value.Quad[2].Color = new Color(Brightness(lightVertices[0]), Brightness(lightVertices[0]), Brightness(lightVertices[0]), 1.0f);
						wall.Value.Quad[3].Color = new Color(Brightness(lightVertices[0]), Brightness(lightVertices[0]), Brightness(lightVertices[0]), 1.0f);
						break;

					case Direction.Up:
						wall.Value.Quad[0].Color = new Color(Brightness(lightVertices[1]), Brightness(lightVertices[1]), Brightness(lightVertices[1]), 1.0f);
						wall.Value.Quad[1].Color = new Color(Brightness(lightVertices[3]), Brightness(lightVertices[3]), Brightness(lightVertices[3]), 1.0f);
						wall.Value.Quad[2].Color = new Color(Brightness(lightVertices[2]), Brightness(lightVertices[2]), Brightness(lightVertices[2]), 1.0f);
						wall.Value.Quad[3].Color = new Color(Brightness(lightVertices[0]), Brightness(lightVertices[0]), Brightness(lightVertices[0]), 1.0f);
						break;
				}
			}
		}

        public float Brightness(float x)
        {
            return Math.Min(1.0f, Math.Max(x / 4.0f, parentFloor.AmbientLight));
        }

        public float AverageBrightness()
        {
			return Math.Min(1.0f, Math.Max(brightnessLevel / 4.0f, parentFloor.AmbientLight));
		}

        public void BlendLighting()
        {
            lightVertices = [0.25f, 0.25f, 0.25f, 0.25f];

            int[] neighborBrightness = new int[9];
            int i = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (RoomX + x < 0 || RoomY + y < 0 || RoomX + x >= parentFloor.MapWidth || RoomY + y >= parentFloor.MapHeight) neighborBrightness[i] = brightnessLevel;
                    else
                    {
                        MapRoom mapRoom = parentFloor.GetRoom(RoomX + x, RoomY + y);
                        neighborBrightness[i] = mapRoom == null || mapRoom.Blocked ? brightnessLevel : mapRoom.brightnessLevel;
                    }
                    i++;
                }
            }

            List<MapRoom> nw = [this];
            if (Neighbors.Contains(this[Direction.North]))
            {
                nw.Add(this[Direction.North]);
                if (this[Direction.North].Neighbors.Contains(this[Direction.North][Direction.West]))
                    nw.Add(this[Direction.North][Direction.West]);
            }
            if (Neighbors.Contains(this[Direction.West]))
            {
				nw.Add(this[Direction.West]);
                if (this[Direction.West].Neighbors.Contains(this[Direction.West][Direction.North]))
                    nw.Add(this[Direction.West][Direction.North]);
			}
            lightVertices[0] = (float)nw.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> ne = [this];
            if (Neighbors.Contains(this[Direction.North]))
            {
                ne.Add(this[Direction.North]);
                if (this[Direction.North].Neighbors.Contains(this[Direction.North][Direction.East]))
                    ne.Add(this[Direction.North][Direction.East]); }
            if (Neighbors.Contains(this[Direction.East]))
            {
                ne.Add(this[Direction.East]);
                if (this[Direction.East].Neighbors.Contains(this[Direction.East][Direction.North]))
                    ne.Add(this[Direction.East][Direction.North]);
            }
            lightVertices[1] = (float)ne.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> sw = [this];
            if (Neighbors.Contains(this[Direction.South]))
            {
                sw.Add(this[Direction.South]);
                if (this[Direction.South].Neighbors.Contains(this[Direction.South][Direction.West]))
                    sw.Add(this[Direction.South][Direction.West]);
            }
            if (Neighbors.Contains(this[Direction.West]))
            {
                sw.Add(this[Direction.West]);
                if (this[Direction.West].Neighbors.Contains(this[Direction.West][Direction.South]))
                    sw.Add(this[Direction.West][Direction.South]);
            }
            lightVertices[2] = (float)sw.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> se = [this];
            if (Neighbors.Contains(this[Direction.South]))
            {
                se.Add(this[Direction.South]);
                if (this[Direction.South].Neighbors.Contains(this[Direction.South][Direction.East]))
                    se.Add(this[Direction.South][Direction.East]);
            }
            if (Neighbors.Contains(this[Direction.East]))
            {
                se.Add(this[Direction.East]);
                if (this[Direction.East].Neighbors.Contains(this[Direction.East][Direction.South]))
                    se.Add(this[Direction.East][Direction.South]); }
            lightVertices[3] = (float)se.Distinct().Average(x => x.brightnessLevel);
        }

        public void Draw(Matrix viewMatrix)
        {
            foreach (KeyValuePair<Direction, RoomWall> wall in wallList)
            {
                DrawWall(wall.Value, viewMatrix);
            }
		}

        public void DrawWall(RoomWall wall, Matrix viewMatrix)
        {
            if (Obscured) return;
            //if (wall.Orientation == Direction.Up || wall.Orientation == Direction.Down) return;
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, wall.Quad, 0, 4, INDICES, 0, 2);
        }

        public void DrawMinimap(SpriteBatch spriteBatch, Vector2 offset, Color color, float depth)
        {
            if (!Discovered) return;

            spriteBatch.Draw(minimapSprite, offset, minimapSource[waypointTile], color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);

            if (Obscured) return;

            if (Foe != null) Foe.DrawMinimap(spriteBatch, offset, color, depth);
        }

        public void DrawMinimapDecal(SpriteBatch spriteBatch, Vector2 offset, Color color, float depth, int stickerID)
        {
            spriteBatch.Draw(minimapSprite, offset, new Rectangle(stickerID * Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE, Floor.MINI_CELL_SIZE), color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
        }

        public void BuildNeighbors()
        {
            if (!wallList.ContainsKey(Direction.West))
            {
                if (RoomX > 0 && parentFloor.GetRoom(RoomX - 1, RoomY) != null && !parentFloor.GetRoom(RoomX - 1, RoomY).Blocked) Neighbors.Add(parentFloor.GetRoom(RoomX - 1, RoomY));
                //else wallList.Add(Direction.West, new RoomWall() { Orientation = Direction.West, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(Direction.East))
            {
                if (RoomX < parentFloor.MapWidth - 1 && parentFloor.GetRoom(RoomX + 1, RoomY) != null && !parentFloor.GetRoom(RoomX + 1, RoomY).Blocked) Neighbors.Add(parentFloor.GetRoom(RoomX + 1, RoomY));
                //else wallList.Add(Direction.East, new RoomWall() { Orientation = Direction.East, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(Direction.North))
            {
                if (RoomY > 0 && parentFloor.GetRoom(RoomX, RoomY - 1) != null && !parentFloor.GetRoom(RoomX, RoomY - 1).Blocked) Neighbors.Add(parentFloor.GetRoom(RoomX, RoomY - 1));
                //else wallList.Add(Direction.North, new RoomWall() { Orientation = Direction.North, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(Direction.South))
            {
                if (RoomY < parentFloor.MapHeight - 1 && parentFloor.GetRoom(RoomX, RoomY + 1) != null && !parentFloor.GetRoom(RoomX, RoomY + 1).Blocked) Neighbors.Add(parentFloor.GetRoom(RoomX, RoomY + 1));
                //else if (!wallList.ContainsKey(Direction.South)) wallList.Add(Direction.South, new RoomWall() { Orientation = Direction.South, Texture = AssetCache.SPRITES[defaultWall] });
            }
        }

        public bool Activate(Direction direction)
        {
            if (Foe != null && Foe.Script != null)
            {
                EventController eventController = new EventController(crawlerScene, Foe.Script, this);
				crawlerScene.AddController(eventController);
                crawlerScene.ResetPathfinding();

                return true;
            }
            else if (InteractScript == null) return false;
            else
            {
                EventController eventController = new EventController(crawlerScene, InteractScript, this);
				Chest?.Destroy();
				crawlerScene.AddController(eventController);
                crawlerScene.ResetPathfinding();

                return true;
            }


            /*
            string[] script;
            if (ActivateScript.TryGetValue(direction, out script))
            {
                EventController eventController = new EventController(parentScene, script, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();

                return true;
            }
            else return false;
            */
        }

        public void ActivatePreScript()
        {
            if (PreEnterScript != null)
            {
                EventController eventController = new EventController(crawlerScene, PreEnterScript, this);
                crawlerScene.AddController(eventController);
                crawlerScene.ResetPathfinding();
            }

			Chest?.Destroy();
		}

        public void EnterRoom(bool finishedMove = true)
        {
            parentFloor.ResetFOV(this, crawlerScene.PartyController.PartyDirection);

            if (Script != null)
            {
                if (crawlerScene.BattleViewModel != null)
                {
                    crawlerScene.BattleViewModel.OnTerminated += new Action(() =>
                    {
                        EventController eventController = new EventController(crawlerScene, Script, this);
                        crawlerScene.AddController(eventController);
                    });
                }
                else
                {
                    EventController eventController = new EventController(crawlerScene, Script, this);
                    crawlerScene.AddController(eventController);
                    crawlerScene.ResetPathfinding();
                }
			}

			Chest?.Destroy();
		}


		public List<MapRoom> Neighbors { get; private set; } = new List<MapRoom>();

        public MapRoom this[Direction key]
        {
            get
            {
                switch (key)
                {
                    case Direction.North: return parentFloor.GetRoom(RoomX, RoomY - 1);
                    case Direction.East: return parentFloor.GetRoom(RoomX + 1, RoomY);
                    case Direction.South: return parentFloor.GetRoom(RoomX, RoomY + 1);
                    case Direction.West: return parentFloor.GetRoom(RoomX - 1, RoomY);
                    default: return null;
                }
            }
        }
    }
}
