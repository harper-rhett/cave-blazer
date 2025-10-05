using HarpEngine;
using HarpEngine.Graphics;
using System.Numerics;

internal class Player : Entity
{
	private Vector2 position;
	public Vector2 Position => position;

	private Area currentArea;
	private Texture texture;
	private int width;
	private int height;
	private int halfWidth;

	public Player(Scene scene, Area area, Vector2 position) : base(scene)
	{
		this.position = position;
		currentArea = area;

		texture = Texture.Load("sprites/explorer.png");
		width = texture.Width;
		height = texture.Height;
		halfWidth = width / 2;
	}

	public override void Update()
	{
		int feetX = (int)position.X + halfWidth;
		int feetY = (int)position.Y + height;
		bool isGrounded = currentArea.IsWall(feetX, feetY);

		if (!isGrounded) position.Y += 15f * Engine.FrameTime;
	}

	public override void Draw()
	{
		texture.Draw(Position, Colors.White);
	}
}
