using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Utilities;
using ldtk;
using System.Numerics;

internal class OldWorld
{
	private Dictionary<Coordinate, OldArea> areas = new();
	private Dictionary<string, OldArea> areasByName = new();
	private Dictionary<string, OldArea> areasByID = new();
	public OldArea SpawnArea { get; private set; }
	public Vector2 SpawnPosition { get; private set; }
	private readonly int levelWidth;
	private readonly int levelHeight;
	private GameScene gameScene;
	private LdtkJson worldData;
	public Dictionary<string, Texture> TilesetByPath { get; private set; } = new();

	public OldWorld(GameScene gameScene)
	{
		this.gameScene = gameScene;

		string ldtkJSON = File.ReadAllText("world.ldtk");
		worldData = LdtkJson.FromJson(ldtkJSON);

		levelWidth = (int)worldData.WorldGridWidth;
		levelHeight = (int)worldData.WorldGridHeight;

		SerializeTilesets();
		SerializeLevels();
		SerializeSpawn();
	}

	private void SerializeTilesets()
	{
		foreach (TilesetDefinition tilesetData in worldData.Defs.Tilesets)
		{
			if (tilesetData.RelPath is null) continue;
			TilesetByPath[tilesetData.RelPath] = Texture.Load(tilesetData.RelPath);
		}
	}

	private void SerializeLevels()
	{
		foreach (Level levelData in worldData.Levels)
		{
			int pixelX = (int)levelData.WorldX;
			int pixelY = (int)levelData.WorldY;
			Coordinate coordinate = new(pixelX / levelWidth, pixelY / levelHeight);
			OldArea area = new(gameScene, this, levelData);
			areas[coordinate] = area;
			areasByName[levelData.Identifier] = area;
			areasByID[levelData.Iid] = area;
		}
	}

	private void SerializeSpawn()
	{
		foreach (LdtkTableOfContentEntry entityData in worldData.Toc)
			if (entityData.Identifier == "Spawn")
			{
				LdtkTocInstanceData spawnInstance = entityData.InstancesData[0];
				string levelID = spawnInstance.Iids.LevelIid;
				SpawnArea = areasByID[levelID];
				SpawnPosition = new(spawnInstance.WorldX, spawnInstance.WorldY);
			}
	}

	public bool DoesAreaExist(int pixelX, int pixelY)
	{
		Coordinate coordinate = PixelsToCoordinate(pixelX, pixelY);
		return areas.ContainsKey(coordinate);
	}

	public OldArea GetArea(int pixelX, int pixelY)
	{
		Coordinate coordinate = PixelsToCoordinate(pixelX, pixelY);
		return areas[coordinate];
	}

	private Coordinate PixelsToCoordinate(int pixelX, int pixelY)
	{
		int coordinateX = ((float)pixelX / levelWidth).Floored();
		int coordinateY = ((float)pixelY / levelHeight).Floored();
		Coordinate coordinate = new(coordinateX, coordinateY);
		return coordinate;
	}
}
