using Clockwork;
using Clockwork.Graphics;
using System.Numerics;

public class MountainParallax : Parallax
{
	public MountainParallax(Camera2D camera, Vector2 originPosition)
		: base(camera, originPosition, new(0, -256))
	{
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/sky.png"), new(0, 80), -0.3f, -0.975f);
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/clouds.png"), new(0, 64), -0.25f, -0.95f);
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/large-mountains.png"), new(0, 48), -0.2f, -0.925f);
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/small-mountains.png"), new(0, 32), -0.15f, -0.9f);
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/plains.png"), new(0, 16), -0.1f, -0.875f);
		AddLayer(Texture.Load("sprites/backgrounds/mountain/old/grass.png"), new(0, 0), -0.05f, -0.85f);
	}
}
