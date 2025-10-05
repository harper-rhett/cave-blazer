using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Input;
using System.Numerics;

internal class Player : Entity
{
	// General
	private Vector2 position;
	private Vector2 velocity;
	private bool isGrounded;
	private Area currentArea;
	
	// Texture
	private Texture texture;
	private int width;
	private int height;
	private int halfWidth;

	// Settings
	private const float gravity = 45;
	private const float jumpForce = 30;
	private const float walkSpeed = 10;
	private const float floatAcceleration = 5;

	// Interface
	public Vector2 Position => position;

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
		CheckGrounded();
		CheckJump();

		if (isGrounded)
		{
			velocity.Y = 0;

			if (Keyboard.IsKeyDown(KeyboardKey.Left)) velocity.X = -walkSpeed;
			else if (Keyboard.IsKeyDown(KeyboardKey.Right)) velocity.X = walkSpeed;
			else velocity.X = 0;
		}
		else
		{
			velocity.Y += gravity * Engine.FrameTime;

			if (Keyboard.IsKeyDown(KeyboardKey.Left)) velocity.X -= floatAcceleration * Engine.FrameTime;
			else if (Keyboard.IsKeyDown(KeyboardKey.Right)) velocity.X += floatAcceleration * Engine.FrameTime;
		}

		position += velocity * Engine.FrameTime;
	}

	public override void Draw()
	{
		texture.Draw(Position, Colors.White);
	}

	private void CheckGrounded()
	{
		int leftFootX = (int)position.X;
		int rightFootX = (int)position.X + width;
		int feetY = (int)position.Y + height;
		isGrounded = currentArea.IsWall(leftFootX, feetY) || currentArea.IsWall(rightFootX, feetY);
	}

	private void CheckJump()
	{
		bool didJump = isGrounded && Keyboard.IsKeyPressed(KeyboardKey.Space);
		if (didJump)
		{
			isGrounded = false;
			velocity.Y -= jumpForce;
		}
	}
}
