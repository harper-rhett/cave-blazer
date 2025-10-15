using HarpEngine;
using HarpEngine.Animation;
using HarpEngine.Graphics;
using System.Numerics;

internal class GameScene : Scene
{
	public readonly World world;
	private Player player;
	private Camera2D camera;

	public GameScene()
	{
		world = new(this);
		player = new(this, world.SpawnArea, new(32, 32));

		camera = new Camera2D(this);
		Camera = camera;
		camera.Offset = Vector2.Zero;
	}

	public void SwitchArea(int pixelX, int pixelY)
	{
		Area nextArea = world.GetArea(pixelX, pixelY);
		player.SetArea(nextArea);
		Transform2DEaser transformEaser = new(this, camera.Transform, 1f);
		transformEaser.TargetWorldPosition = nextArea.Position;
		transformEaser.Start();
	}
}