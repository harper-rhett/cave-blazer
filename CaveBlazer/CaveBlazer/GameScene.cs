using HarpEngine;
using HarpEngine.Animation;
using HarpEngine.Graphics;
using System.Numerics;

internal class GameScene : Scene
{
	public readonly World World;
	private Player player;
	private Camera2D camera;

	public GameScene() : base(Colors.SkyBlue)
	{
		World = new(this);
		player = new(this, World.SpawnArea, World.SpawnPosition);

		camera = new Camera2D(this);
		Camera = camera;
		camera.Offset = Vector2.Zero;
		camera.Transform.WorldPosition = World.SpawnArea.Position;
	}

	public void SwitchArea(int pixelX, int pixelY)
	{
		Area nextArea = World.GetArea(pixelX, pixelY);
		player.SetArea(nextArea);
		Transform2DEaser transformEaser = new(this, camera.Transform, 1f);
		transformEaser.TargetWorldPosition = nextArea.Position;
		transformEaser.Start();
	}
}