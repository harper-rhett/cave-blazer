using HarpEngine;
using HarpEngine.Graphics;
using ldtk;

internal class World
{
	private Dictionary<Coordinate, Area> areas = new();
	public readonly Area SpawnArea;

	public World(GameScene gameScene)
	{
		string ldtkJSON = File.ReadAllText("world.ldtk");
		LdtkJson worldData = LdtkJson.FromJson(ldtkJSON);
		Texture tileset = Texture.Load("sprites/cave-tileset.png");

		int levelWidth = (int)worldData.WorldGridWidth;
		int levelHeight = (int)worldData.WorldGridHeight;
		
		foreach (Level levelData in worldData.Levels)
		{
			int x = (int)levelData.WorldX;
			int y = (int)levelData.WorldY;
			Coordinate coordinate = new(x / levelWidth, y / levelHeight);
			Area area = new(gameScene, levelData, tileset);
			areas[coordinate] = area;
		}

		SpawnArea = areas[new Coordinate(0, 0)];
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
