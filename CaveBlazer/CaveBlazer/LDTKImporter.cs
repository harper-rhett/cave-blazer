using HarpEngine.Graphics;
using HarpEngine.Tiles;
using HarpEngine.Utilities;
using ldtk;
using System.Numerics;
using Tiles;

internal class LDTKImporter<TileType> where TileType : Enum
{
	private LdtkJson ldtkData;
	private Dictionary<string, Texture> tilesetsByPath = new();
	private int tileSize;

	public LDTKImporter(string filePath, int tileSize)
	{
		string ldtkJSON = File.ReadAllText(filePath);
		ldtkData = LdtkJson.FromJson(ldtkJSON);
		this.tileSize = tileSize;
	}

	public TiledWorld<TileType> GenerateWorld()
	{
		DeserializeTilesets();
		List<TiledArea<TileType>> areas = DeserializeLevels();
		TiledWorld<TileType> world = new(areas, tileSize);
		return world;
	}

	private void DeserializeTilesets()
	{
		foreach (TilesetDefinition tilesetData in ldtkData.Defs.Tilesets)
		{
			if (tilesetData.RelPath is null) continue;
			tilesetsByPath[tilesetData.RelPath] = Texture.Load(tilesetData.RelPath);
		}
	}

	private Dictionary<string, LayerInstance> DeserializeLayers(Level levelData)
	{
		Dictionary<string, LayerInstance> layersData = new();
		foreach (LayerInstance layerData in levelData.LayerInstances) layersData[layerData.Identifier] = layerData;
		return layersData;
	}

	private Tile<TileType>[,] DeserializeTiles(LayerInstance layerData, int widthInTiles, int heightInTiles, int tileSize)
	{
		Tile<TileType>[,] tiles = new Tile<TileType>[widthInTiles, heightInTiles];

		TileInstance[] tilesData = layerData.AutoLayerTiles;
		long[] tileTypes = layerData.IntGridCsv;
		for (int x = 0; x < widthInTiles; x++)
			for (int y = 0; y < heightInTiles; y++)
			{
				// Get the data
				int tileIndex = x + y * widthInTiles;
				TileInstance tileData = tilesData[tileIndex];
				TileType tileType = (TileType)(object)tileTypes[tileIndex];

				// Get the tileset information
				int tilesetX = (int)tileData.Src[0];
				int tilesetY = (int)tileData.Src[1];
				bool xFlipped = (tileData.F & 1) != 0;
				bool yFlipped = (tileData.F & (1L << 1)) != 0;

				// Create the tile
				Texture tilesetTexture = tilesetsByPath[layerData.TilesetRelPath];
				Tile<TileType> tile = new(tilesetTexture, tilesetX, tilesetY, tileSize, tileType, xFlipped, yFlipped);
			}

		return tiles;
	}

	private List<TiledArea<TileType>> DeserializeLevels()
	{
		List<TiledArea<TileType>> areas = new();
		foreach (Level levelData in ldtkData.Levels)
		{
			// Get tiles layer
			Dictionary<string, LayerInstance> layersData = DeserializeLayers(levelData);
			LayerInstance tileLayerData = layersData["Tiles"];

			// Extract some basic information
			int widthInTiles = (int)tileLayerData.CWid;
			int heightInTiles = (int)tileLayerData.CHei;

			// Create area
			Tile<TileType>[,] tiles = DeserializeTiles(tileLayerData, widthInTiles, heightInTiles, tileSize);
			Vector2 position = new((int)levelData.WorldX, (int)levelData.WorldY);
			TiledArea<TileType> area = new(position, widthInTiles, heightInTiles, tileSize, tiles);
			areas.Add(area);
		}
		return areas;
	}

	private void DeserializeSpawn() // change to deserialize entities? spawn should be temporary
	{
		foreach (LdtkTableOfContentEntry entityData in ldtkData.Toc)
			if (entityData.Identifier == "Spawn")
			{
				LdtkTocInstanceData spawnInstance = entityData.InstancesData[0];
				string levelID = spawnInstance.Iids.LevelIid;
				SpawnArea = areasByID[levelID];
				SpawnPosition = new(spawnInstance.WorldX, spawnInstance.WorldY);
			}
	}
}
