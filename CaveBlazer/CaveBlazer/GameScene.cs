using HarpEngine;
using HarpEngine.Animation;
using HarpEngine.Graphics;
using HarpEngine.Tiles;
using System.Numerics;
using HarpEngine.LDTKImporter;

public class GameScene : Scene
{
	public readonly TiledWorld World;
	private Player player;
	private Camera2D camera;
	public const int TileSize = 16;

	public GameScene() : base(Colors.SkyBlue)
	{
		LDTKImporter importer = new("world.ldtk", 8);
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
		Transform2DEaser cameraEaser = AddEntity(new Transform2DEaser(camera.Transform, 1f));
		cameraEaser.Curve = Curves.SmoothStep;
		cameraEaser.TargetWorldPosition = nextArea.Position;
		cameraEaser.Start();
	}
}