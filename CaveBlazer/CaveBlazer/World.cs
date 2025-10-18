using HarpEngine;
using HarpEngine.Graphics;
using ldtk;

internal class World
{
	private Dictionary<Coordinate, Area> areas = new();
	private Dictionary<string, Area> areasByIdentifier = new();
	public readonly Area SpawnArea;
	private readonly int levelWidth;
	private readonly int levelHeight;

	public World(GameScene gameScene)
	{
		string ldtkJSON = File.ReadAllText("world.ldtk");
		LdtkJson worldData = LdtkJson.FromJson(ldtkJSON);
		Texture tileset = Texture.Load("sprites/cave-tileset.png");

		levelWidth = (int)worldData.WorldGridWidth;
		levelHeight = (int)worldData.WorldGridHeight;
		
		foreach (Level levelData in worldData.Levels)
		{
			int pixelX = (int)levelData.WorldX;
			int pixelY = (int)levelData.WorldY;
			Coordinate coordinate = new(pixelX / levelWidth, pixelY / levelHeight);
			Area area = new(gameScene, levelData, tileset);
			areas[coordinate] = area;
			areasByIdentifier[levelData.Identifier] = area;
		}

		SpawnArea = areasByIdentifier["Start"];
	}

	public Area GetArea(int pixelX, int pixelY)
	{
		Coordinate coordinate = new(pixelX / levelWidth, pixelY / levelHeight);
		return areas[coordinate];
	}

	private struct Coordinate
	{
		public int X;
		public int Y;

		public Coordinate(int x, int y)
		{
			X = x;
			Y = y;
		}
	}
}
