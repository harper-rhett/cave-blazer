using HarpEngine;
using HarpEngine.Graphics;
using ldtk;
using System.Numerics;

internal class Area : Entity
{
	private int x;
	private int y;
	private int widthInPixels;
	private int heightInPixels;
	private int widthInTiles;
	private int heightInTiles;
	private RenderTexture renderTexture;
	private Rectangle renderRectangle;
	private bool[,] walls;

	private const int tileSize = 8;

	public Area(Scene scene, Level level, Texture tilesetTexture) : base(scene)
	{
		// Set area coordinates
		x = (int)level.WorldX;
		y = (int)level.WorldY;
		widthInPixels = (int)level.PxWid;
		heightInPixels = (int)level.PxHei;
		
		// Get tile information
		LayerInstance layerInstance = level.LayerInstances[0];
		widthInTiles = (int)layerInstance.CWid;
		heightInTiles = (int)layerInstance.CHei;
		DrawTexture(layerInstance, tilesetTexture);
		ProcessCollisions(layerInstance);
	}

	private void DrawTexture(LayerInstance layerInstance, Texture tilesetTexture)
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

	public override void Draw()
	{
		renderTexture.Texture.Draw(renderRectangle, Vector2.Zero, Colors.White);
		DrawCollisions();
	}

	private void DrawCollisions()
	{
		for (int x = 0; x < widthInTiles; x++)
			for (int y = 0; y < heightInTiles; y++)
			{
				if (walls[x, y]) Primitives.DrawSquare(x * tileSize, y * tileSize, tileSize, Colors.Red.SetAlpha(100));
			}
	}
}