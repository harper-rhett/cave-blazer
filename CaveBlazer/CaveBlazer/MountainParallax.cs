using HarpEngine;
using HarpEngine.Graphics;
using System.Numerics;

public class MountainParallax : Parallax
{
	public MountainParallax(Camera2D camera, Vector2 startPosition) : base(camera, startPosition)
	{
		int yOffset = 0;
		AddLayer(Texture.Load("sprites/backgrounds/sky.png"), new(0, yOffset), 0.8f);
		AddLayer(Texture.Load("sprites/backgrounds/clouds.png"), new(0, yOffset),0.7f);
		AddLayer(Texture.Load("sprites/backgrounds/large-mountains.png"), new(0, yOffset), 0.6f);
		AddLayer(Texture.Load("sprites/backgrounds/small-mountains.png"), new(0, yOffset), 0.5f);
		AddLayer(Texture.Load("sprites/backgrounds/plains.png"), new(0, yOffset), 0.4f);
		AddLayer(Texture.Load("sprites/backgrounds/grass.png"), new(0, yOffset), 0.3f);
	}
}
