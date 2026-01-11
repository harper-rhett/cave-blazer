using Clockwork;
using Clockwork.Graphics;
using System.Numerics;

public class MountainParallax : Parallax
{
	public MountainParallax(Camera2D camera, Vector2 originPosition)
		: base(camera, originPosition, new(0, -256))
	{
		int yOffset = 0;
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/sky.png"), new(0, yOffset), 0.3f);
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/clouds.png"), new(0, yOffset), 0.25f);
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/large-mountains.png"), new(0, yOffset), 0.2f);
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/small-mountains.png"), new(0, yOffset), 0.15f);
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/plains.png"), new(0, yOffset), 0.1f);
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/grass.png"), new(0, yOffset), 0.05f);
	}
}
