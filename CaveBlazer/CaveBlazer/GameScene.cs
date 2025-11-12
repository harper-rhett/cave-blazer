using HarpEngine;
using HarpEngine.Animation;
using HarpEngine.Graphics;
using System.Numerics;
using Tiles;

internal class GameScene : Scene
{
	public readonly TiledWorld<TileType> World;
	private Player player;
	private Camera2D camera;

	public GameScene() : base(Colors.SkyBlue)
	{
		LDTKImporter<TileType> importer = new("world.ldtk", 8);
		World = importer.GenerateWorld();
		player = new(this, World.SpawnArea, World.SpawnPosition);

		camera = new Camera2D(this);
		Camera = camera;
		camera.Offset = Vector2.Zero;
		camera.Transform.WorldPosition = World.SpawnArea.Position;
	}

	public void TempDraw()
	{
		World.Draw();
	}

	public void SwitchArea(int pixelX, int pixelY)
	{
		OldArea nextArea = World.GetArea(pixelX, pixelY);
		player.CurrentArea = nextArea;
		Transform2DEaser transformEaser = new(this, camera.Transform, 1f);
		transformEaser.TargetWorldPosition = nextArea.Position;
		transformEaser.Start();
	}
}