using Clockwork;
using Clockwork.Animation;
using Clockwork.Graphics;
using Clockwork.Tiles;
using System.Numerics;
using Clockwork.LDTKImporter;

public class GameScene : Scene
{
	// World
	public LDTKGameLayer World { get; private set; }
	public const int TileSize = 16;
	private const string spawnAreaName = "origin";
	private const string originAreaName = "origin";

	// Decoration
	public TiledLayer Foreground { get; private set; }
	public Dictionary<string, TiledArea> ForegroundsByName;
	public TiledLayer Background { get; private set; }
	public Dictionary<string, TiledArea> BackgroundsByName;

	// Player
	public Player Player { get; private set; }
	private Camera2D camera;
	private Transform2DEaser cameraEaser;

	public GameScene() : base(Colors.SkyBlue)
	{
		ImportWorld();

		LDTKGameArea originArea = World.AreasByName["origin"];
		LDTKGameArea spawnArea = World.AreasByName[spawnAreaName];
		World.AddFocus(spawnArea);
		Foreground.AddFocus(ForegroundsByName[spawnAreaName]);
		Background.AddFocus(BackgroundsByName[spawnAreaName]);

		SpawnPlayer(spawnArea);
		InitializeCamera(spawnArea);
		InitializeCamera(spawnArea);
		InitializeParallax(originArea);
	}

	private void ImportWorld()
	{
		LDTKImporter importer = new("world.ldtk", 8);
		importer.GameAreaImported += ProcessArea;
		World = AddEntity(importer.ImportWorld("tiles", "entities"));
		World.DrawLayer = -1;

		Foreground = AddEntity(importer.ImportDecorationLayer("foreground", out ForegroundsByName));
		Foreground.DrawLayer = 1;
		Background = AddEntity(importer.ImportDecorationLayer("background", out BackgroundsByName));
		Background.DrawLayer = -1;
	}

	private void SpawnPlayer(LDTKGameArea spawnArea)
	{
		Vector2 spawnPosition = spawnArea.EntitiesByID["spawn"][0].Position;
		Player = AddEntity(new Player(this, spawnArea, spawnPosition));
		Player.Inventory.UnlockCrampons();
	}

	private void InitializeCamera(LDTKGameArea spawnArea)
	{
		camera = AddEntity(new Camera2D());
		Camera = camera;
		camera.Offset = Vector2.Zero;
		camera.Transform.WorldPosition = spawnArea.Position;
	}

	private void InitializeParallax(LDTKGameArea originArea)
	{
		MountainParallax parallax = AddEntity(new MountainParallax(camera, originArea.Position));
		parallax.RepeatY = false;
		parallax.DrawLayer = -2;
	}

	private void ProcessArea(LDTKGameArea area)
	{
		foreach (LDTKEntity ldtkEntity in area.Entities)
		{
			// Get relevant entity
			Entity entity = null;
			if (ldtkEntity.Name == "dialogue") entity = new DialogueEntity(ldtkEntity, this);
			else if (ldtkEntity.Name == "upgrade") entity = new UpgradeEntity(ldtkEntity, this);

			// Register entity
			if (entity is null) continue;
			AddEntity(entity);
			area.RegisterEntity(entity);
		}
	}

	public void SwitchArea(int pixelX, int pixelY)
	{
		// Cache areas
		TiledGameArea previousArea = Player.CurrentArea;
		TiledGameArea nextArea = World.GetArea(pixelX, pixelY);

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

	private void AreaSwitched(TiledGameArea previousArea)
	{
		World.RemoveFocus(previousArea);
	}
}