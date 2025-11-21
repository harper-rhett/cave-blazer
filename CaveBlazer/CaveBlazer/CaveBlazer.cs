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

	public override void OnUpdate()
	{
		gameScene.Update();
	}

	public override void OnDraw()
	{
		gameScene.Draw();
	}
}
