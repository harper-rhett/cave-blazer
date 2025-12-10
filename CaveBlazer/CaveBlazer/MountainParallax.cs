using HarpEngine;
using HarpEngine.Graphics;

public class MountainParallax : Parallax
{
	public MountainParallax(Camera2D camera) : base(camera)
	{
		int yOffset = -320;
		AddLayer(Texture.Load("sprites/backgrounds/sky.png"), new(0, yOffset), 0.01f);
		AddLayer(Texture.Load("sprites/backgrounds/clouds.png"), new(0, yOffset), 0.02f);
		AddLayer(Texture.Load("sprites/backgrounds/large-mountains.png"), new(0, yOffset), 0.03f);
		AddLayer(Texture.Load("sprites/backgrounds/small-mountains.png"), new(0, yOffset), 0.04f);
		AddLayer(Texture.Load("sprites/backgrounds/plains.png"), new(0, yOffset), 0.05f);
		AddLayer(Texture.Load("sprites/backgrounds/grass.png"), new(0, yOffset), 0.06f);
	}
}
