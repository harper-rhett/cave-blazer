using HarpEngine;
using HarpEngine.Graphics;
using ldtk;

internal class GameScene : Scene
{
	public GameScene()
	{
		string ldtkJSON = File.ReadAllText("world.ldtk");
		LdtkJson world = LdtkJson.FromJson(ldtkJSON);
		Level testLevel = world.Levels[0];
		Texture tileset = Texture.Load("sprites/cave-tileset.png");
		Area area = new(this, testLevel, tileset);

		Player player = new(this, area, new(32, 32));
	}
}