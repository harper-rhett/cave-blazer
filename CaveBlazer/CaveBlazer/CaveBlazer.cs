using HarpEngine;
using HarpEngine.Windowing;

internal class CaveBlazer : Game
{
	private GameScene gameScene = new();

	public CaveBlazer()
	{
		Window.SetResizable(true);
		Window.SetRendererClipped();
	}

	public override void Update()
	{
		gameScene.Update();
	}

	public override void Draw()
	{
		gameScene.Draw();
	}
}
