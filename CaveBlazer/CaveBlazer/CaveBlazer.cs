using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Windowing;

internal class CaveBlazer : Game
{
	private GameScene gameScene = new();

	public CaveBlazer()
	{
		Window.SetResizable(true);
		Window.SetRendererUnclipped(Colors.Black);
	}

	public override void Update()
	{
		gameScene.UpdateSystems();
	}

	public override void Draw()
	{
		gameScene.DrawSystems();
	}
}
