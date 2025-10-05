using HarpEngine;

internal class CaveBlazer : Game
{
	private GameScene gameScene = new();

	public CaveBlazer()
	{

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
