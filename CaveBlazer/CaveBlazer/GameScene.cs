using HarpEngine;
using HarpEngine.Animation;
using HarpEngine.Graphics;
using HarpEngine.Tiles;
using System.Numerics;

internal class GameScene : Scene
{
	public readonly TiledWorld World;
	private Player player;
	private Camera2D camera;

	public GameScene() : base(Colors.SkyBlue)
	{
		LDTKImporter importer = new("world.ldtk", 16);
		World = AddEntity(importer.GenerateWorld());
		player = AddEntity(new Player(this, importer.SpawnArea, importer.SpawnPosition));
		
		camera = AddEntity(new Camera2D());
		Camera = camera;
		camera.Offset = Vector2.Zero;
		camera.Transform.WorldPosition = importer.SpawnArea.Position;
	}

	public void SwitchArea(int pixelX, int pixelY)
	{
		TiledArea nextArea = World.GetArea(pixelX, pixelY);
		player.CurrentArea = nextArea;
		Transform2DEaser transformEaser = AddEntity(new Transform2DEaser(camera.Transform, 1f));
		transformEaser.TargetWorldPosition = nextArea.Position;
		transformEaser.Start();
	}
}