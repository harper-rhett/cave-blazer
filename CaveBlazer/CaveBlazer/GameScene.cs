using Clockwork;
using Clockwork.Animation;
using Clockwork.Graphics;
using Clockwork.Tiles;
using System.Numerics;
using Clockwork.LDTKImporter;

public class GameScene : Scene
{
	public readonly LDTKWorld World;
	public readonly Player Player;
	private Camera2D camera;
	public const int TileSize = 16;
	private Transform2DEaser cameraEaser;
	private const string spawnAreaID = "area_5_1";

	public GameScene() : base(Colors.SkyBlue)
	{
		// Import world
		LDTKImporter importer = new("world.ldtk", 8);
		importer.AreaImported += ProcessArea;
		World = AddEntity(importer.GenerateWorld());

		// Process player spawn
		LDTKArea originArea = World.AreasByID["origin"];
		LDTKArea spawnArea = World.AreasByID[spawnAreaID];
		World.AddFocus(spawnArea);
		Vector2 spawnPosition = spawnArea.EntitiesByID["spawn"][0].Position;
		Player = AddEntity(new Player(this, spawnArea, spawnPosition));
		
		// Initialize camera
		camera = AddEntity(new Camera2D());
		Camera = camera;
		camera.Offset = Vector2.Zero;
		camera.Transform.WorldPosition = spawnArea.Position;

		// Initialize parallax
		MountainParallax parallax = AddEntity(new MountainParallax(camera, originArea.Position, new(0, -256)));
		parallax.RepeatY = false;
		parallax.DrawLayer = -1;
	}

	private void ProcessArea(LDTKArea area)
	{
		foreach (LDTKEntity ldtkEntity in area.Entities)
		{
			// Get relevant entity
			Entity entity = null;
			if (ldtkEntity.ID == "dialogue") entity = new DialogueEntity(ldtkEntity, this);
			else if (ldtkEntity.ID == "upgrade") entity = new UpgradeEntity(ldtkEntity, this);

			// Register entity
			if (entity is null) continue;
			AddEntity(entity);
			area.RegisterEntity(entity);
		}
	}

	public void SwitchArea(int pixelX, int pixelY)
	{
		// Cache areas
		TiledArea previousArea = Player.CurrentArea;
		TiledArea nextArea = World.GetArea(pixelX, pixelY);

		// Set new area
		if (cameraEaser is not null) cameraEaser.Finish();
		World.AddFocus(nextArea);
		Player.CurrentArea = nextArea;

		// Ease camera to next area
		cameraEaser = AddEntity(new Transform2DEaser(camera.Transform, 1f));
		cameraEaser.UpdateLayer = -1;
		cameraEaser.Curve = Curves.SmoothStep;
		cameraEaser.TargetWorldPosition = nextArea.Position;
		cameraEaser.Start();
		cameraEaser.Finished += () => AreaSwitched(previousArea);
	}

	private void AreaSwitched(TiledArea previousArea)
	{
		World.RemoveFocus(previousArea);
	}
}