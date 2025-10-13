using HarpEngine;
using HarpEngine.Graphics;

internal class GameScene : Scene
{
	private World world;

	public GameScene()
	{
		world = new(this);
		Player player = new(this, world.SpawnArea, new(32, 32));
		Camera = new Camera2D(this);
	}
}