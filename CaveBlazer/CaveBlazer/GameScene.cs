using HarpEngine;
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
		camera.TransitionWorldPosition(nextArea.Position, 1f); // need to rewrite camera transitions so that I can edit curve
	}
}