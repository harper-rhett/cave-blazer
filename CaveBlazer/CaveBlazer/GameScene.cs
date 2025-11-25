using HarpEngine;
using HarpEngine.Animation;
using HarpEngine.Graphics;
using HarpEngine.Tiles;
using System.Numerics;
using HarpEngine.LDTKImporter;
using HarpEngineLDTKImporter;

public class GameScene : Scene
{
	public readonly LDTKWorld World;
	private Player player;
	private Camera2D camera;
	public const int TileSize = 16;

	public GameScene() : base(Colors.SkyBlue)
	{
		LDTKImporter importer = new("world.ldtk", 8);
		World = AddEntity(importer.GenerateWorld());
		LDTKArea spawnArea = World.AreasByID["spawn"];
		World.FocusArea = spawnArea;
		Vector2 spawnPosition = spawnArea.EntitiesByID["spawn"][0].Position;
		player = AddEntity(new Player(this, spawnArea, spawnPosition));
		
		camera = AddEntity(new Camera2D());
		Camera = camera;
		camera.Offset = Vector2.Zero;
		camera.Transform.WorldPosition = spawnArea.Position;
	}

	public void SwitchArea(int pixelX, int pixelY)
	{
		TiledArea nextArea = World.GetArea(pixelX, pixelY);
		World.FocusArea = nextArea;
		player.CurrentArea = nextArea;
		Transform2DEaser cameraEaser = AddEntity(new Transform2DEaser(camera.Transform, 1f));
		cameraEaser.Curve = Curves.SmoothStep;
		cameraEaser.TargetWorldPosition = nextArea.Position;
		cameraEaser.Start();
	}
}