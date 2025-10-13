using HarpEngine;
using HarpEngine.Graphics;
using ldtk;
using System.Numerics;

internal class Area : Entity
{
	private int pixelX;
	private int pixelY;
	private int widthInPixels;
	private int heightInPixels;
	private int widthInTiles;
	private int heightInTiles;
	private RenderTexture renderTexture;
	private Rectangle renderRectangle;
	private bool[,] walls;

	private const int tileSize = 8;

	public Area(Scene scene, Level levelData, Texture tilesetTexture) : base(scene)
	{
		// Set area coordinates
		pixelX = (int)levelData.WorldX;
		pixelY = (int)levelData.WorldY;
		widthInPixels = (int)levelData.PxWid;
		heightInPixels = (int)levelData.PxHei;
		
		// Get tile information
		LayerInstance layerInstance = levelData.LayerInstances[0];
		widthInTiles = (int)layerInstance.CWid;
		heightInTiles = (int)layerInstance.CHei;
		ProcessTexture(layerInstance, tilesetTexture);
		ProcessCollisions(layerInstance);
	}

	public override void Draw()
	{
		renderTexture.Texture.Draw(renderRectangle, Vector2.Zero, Colors.White);
		//DrawCollisions();
	}

	private void ProcessTexture(LayerInstance layerInstance, Texture tilesetTexture)
	{
		// Prepare the render texture
		renderTexture = RenderTexture.Load(widthInPixels, heightInPixels);
		renderRectangle = new(pixelX, pixelY, widthInPixels, -heightInPixels);
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
				int worldX = pixelX + x * tileSize;
				int worldY = pixelY + y * tileSize;
				if (walls[x, y]) Primitives.DrawSquare(worldX, worldY, tileSize, Colors.Red.SetAlpha(100));
			}
	}

	public bool IsWall(int pixelX, int pixelY)
	{
		int tileX = (pixelX - this.pixelX) / tileSize;
		int tileY = (pixelY - this.pixelY) / tileSize;
		return walls[tileX, tileY];
	}

	public bool InBounds(int pixelX, int pixelY)
	{
		bool xCheck = pixelX >= 0 && pixelX < widthInPixels;
		bool yCheck = pixelY >= 0 && pixelY < heightInPixels;
		return xCheck && yCheck;
	}
}