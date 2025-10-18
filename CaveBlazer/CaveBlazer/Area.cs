using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Utilities;
using ldtk;
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

	private const int tileSize = 8;

	public Area(Scene scene, Level levelData, Texture tilesetTexture) : base(scene)
	{
		// Set level info
		int pixelX = (int)levelData.WorldX;
		int pixelY = (int)levelData.WorldY;
		Position = new(pixelX, pixelY);
		widthInPixels = (int)levelData.PxWid;
		heightInPixels = (int)levelData.PxHei;
		Name = levelData.Identifier;

		// Serialize layers
		foreach (LayerInstance layerData in levelData.LayerInstances)
			layersData[layerData.Identifier] = layerData;
		
		// Get tile information
		LayerInstance layerInstance = layersData["Tiles"];
		widthInTiles = (int)layerInstance.CWid;
		heightInTiles = (int)layerInstance.CHei;
		ProcessTexture(layerInstance, tilesetTexture);
		ProcessCollisions(layerInstance);
	}

	public override void Draw()
	{
		renderTexture.Texture.Draw(renderRectangle, Position, Colors.White);
		//DrawCollisions();
	}

	private void ProcessTexture(LayerInstance layerInstance, Texture tilesetTexture)
	{
		// Prepare the render texture
		renderTexture = RenderTexture.Load(widthInPixels, heightInPixels);
		renderRectangle = new(0, 0, widthInPixels, -heightInPixels);
		RenderTexture.BeginDrawing(renderTexture);

		// Loop through all tiles
		TileInstance[] tileInstances = layerInstance.AutoLayerTiles;
		foreach (TileInstance tileInstance in tileInstances)
		{
			// Get the local position
			int pixelX = (int)tileInstance.Px[0];
			int pixelY = (int)tileInstance.Px[1];
			Vector2 pixelPosition = new(pixelX, pixelY);

			// Get the tileset position
			int tilesetX = (int)tileInstance.Src[0];
			int tilesetY = (int)tileInstance.Src[1];
			Rectangle tileRectangle = new(tilesetX, tilesetY, tileSize, tileSize);

			// Draw the tile
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