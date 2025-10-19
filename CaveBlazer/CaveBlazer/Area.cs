using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Utilities;
using ldtk;
using Newtonsoft.Json.Linq;
using System.Numerics;

internal class Area : Entity
{
	public Vector2 Position { get; private set; }
	private int widthInPixels;
	private int heightInPixels;
	private int widthInTiles;
	private int heightInTiles;
	private RenderTexture renderTexture;
	private Rectangle renderRectangle;
	private bool[,] walls;
	private Dictionary<string, LayerInstance> layersData = new();
	private string Name;
	private World world;

	private const int tileSize = 8;

	public Area(Scene scene, World world, Level levelData) : base(scene)
	{
		// Set level info
		this.world = world;
		int pixelX = (int)levelData.WorldX;
		int pixelY = (int)levelData.WorldY;
		Position = new(pixelX, pixelY);
		widthInPixels = (int)levelData.PxWid;
		heightInPixels = (int)levelData.PxHei;
		Name = levelData.Identifier;

		// Serialize layers
		SerializeLayers(levelData);
		
		// Get tile information
		LayerInstance layerData = layersData["Tiles"];
		
		widthInTiles = (int)layerData.CWid;
		heightInTiles = (int)layerData.CHei;
		ProcessTexture(layerData);
		ProcessCollisions(layerData);
	}

	public override void Draw()
	{
		renderTexture.Texture.Draw(renderRectangle, Position, Colors.White);
		//DrawCollisions();
	}

	private void SerializeLayers(Level levelData)
	{
		foreach (LayerInstance layerData in levelData.LayerInstances)
		{
			layersData[layerData.Identifier] = layerData;
			Console.WriteLine(layerData.TilesetRelPath);
		}
	}

	private void ProcessTexture(LayerInstance layerData)
	{
		// Prepare the render texture
		renderTexture = RenderTexture.Load(widthInPixels, heightInPixels);
		renderRectangle = new(0, 0, widthInPixels, -heightInPixels);
		RenderTexture.BeginDrawing(renderTexture);

		// Loop through all tiles
		TileInstance[] tileInstances = layerData.AutoLayerTiles;
		foreach (TileInstance tileInstance in tileInstances)
		{
			// Get the local position
			int pixelX = (int)tileInstance.Px[0];
			int pixelY = (int)tileInstance.Px[1];
			Vector2 pixelPosition = new(pixelX, pixelY);

			// Get the tileset position
			int tilesetX = (int)tileInstance.Src[0];
			int tilesetY = (int)tileInstance.Src[1];
			bool xFlipped = (tileInstance.F & 1) != 0;
			bool yFlipped = (tileInstance.F & (1L << 1)) != 0;
			int tileWidth = xFlipped ? -tileSize : tileSize;
			int tileHeight = yFlipped ? -tileSize : tileSize;
			Rectangle tileRectangle = new(tilesetX, tilesetY, tileWidth, tileHeight);

			// Draw the tile
			Texture tilesetTexture = world.TilesetByPath[layerData.TilesetRelPath];
			tilesetTexture.Draw(tileRectangle, pixelPosition, Colors.White);
		}

		// Finalize texture
		RenderTexture.EndDrawing();
	}

	private void ProcessCollisions(LayerInstance layerInstance)
	{
		walls = new bool[widthInTiles, heightInTiles];
		long[] tileValues = layerInstance.IntGridCsv;
		for (int x = 0; x < widthInTiles; x++)
			for (int y = 0; y < heightInTiles; y++)
			{
				int tileValue = (int)tileValues[x + y * widthInTiles];
				walls[x, y] = tileValue == 1 ? true : false;
			}
	}

	private void DrawCollisions()
	{
		for (int x = 0; x < widthInTiles; x++)
			for (int y = 0; y < heightInTiles; y++)
			{
				int worldX = Position.X.Floored() + x * tileSize;
				int worldY = Position.Y.Floored() + y * tileSize;
				if (walls[x, y]) Primitives.DrawSquare(worldX, worldY, tileSize, Colors.Red.SetAlpha(100));
			}
	}

	public bool IsWall(int pixelX, int pixelY)
	{
		int tileX = (pixelX - Position.X.Floored()) / tileSize;
		int tileY = (pixelY - Position.Y.Floored()) / tileSize;
		return walls[tileX, tileY];
	}

	public bool InBounds(int pixelX, int pixelY)
	{
		bool xCheck = pixelX >= Position.X.Floored() && pixelX < Position.X.Floored() + widthInPixels;
		bool yCheck = pixelY >= Position.Y.Floored() && pixelY < Position.Y.Floored() + heightInPixels;
		return xCheck && yCheck;
	}
}