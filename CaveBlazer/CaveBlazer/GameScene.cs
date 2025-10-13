using HarpEngine;
using HarpEngine.Graphics;
using System.Numerics;

internal class GameScene : Scene
{
	private World world;
	private Camera2D camera;

	public GameScene()
	{
		world = new(this);
		Player player = new(this, world.SpawnArea, new(32, 32));

		camera = new Camera2D(this);
		Camera = camera;
		camera.Offset = Vector2.Zero;
	}
}