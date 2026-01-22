using Clockwork;
using Clockwork.Graphics;
using Clockwork.Windowing;

internal class CaveBlazer : Game
{
	private GameScene gameScene;

	public CaveBlazer() : base("Cave Blazer", 368, 192)
	{
		gameScene = new();
		Window.SetResizable(true);
		WindowRenderer.SetUnclipped(Colors.Black);
		Window.Resize(368 * 5, 192 * 5);
		Window.Position = new(32, 32);
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
