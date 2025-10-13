using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Input;
using System.Diagnostics;
using System.Numerics;

internal class Player : Entity
{
	// General
	private Vector2 position;
	private Vector2 velocity;
	private bool isGrounded;
	private int direction = 1;
	private Area currentArea;

	// Texture
	private Texture texture;
	private int width;
	private int height;

	// Settings
	private const float gravity = 45;
	private const float jumpForce = 30;
	private const float walkSpeed = 15;
	private const float floatAcceleration = 10;

	// Interface
	public Vector2 Position => position;
	public Vector2 Center => position + new Vector2(width, height);

	// Note to Harper:
	// A box collider might make more since. Think about it.

	public Player(Scene scene, Area area, Vector2 position) : base(scene)
	{
		this.position = position;
		currentArea = area;

		texture = Texture.Load("sprites/explorer.png");
		width = texture.Width;
		height = texture.Height;
	}

	public override void Update()
	{
		// State checks
		CheckGrounded();
		CheckJump();

		// State updates
		if (isGrounded) GroundedUpdate();
		else MidairUpdate();

		// Get next presumed position
		Vector2 nextPosition = position + velocity * Engine.FrameTime;
		int checkLeftX = (int)float.Round(nextPosition.X);
		int checkRightX = (int)float.Round(nextPosition.X + width - 1);

		// Check if in bounds
		bool leftInBounds = currentArea.InBounds(checkLeftX, (int)nextPosition.Y);
		bool rightInBounds = currentArea.InBounds(checkRightX, (int)nextPosition.Y);
		bool inBounds = leftInBounds && rightInBounds;

		if (inBounds)
		{
			WallCollision(checkLeftX, checkRightX);
			Movement();
		}
		else
		{

		}
	}

	public override void Draw()
	{
		Rectangle sourceRectangle = new(0, 0, width * direction, height);
		Rectangle destinationRectangle = new(float.Round(position.X), float.Round(position.Y), width, height);
		texture.Draw(sourceRectangle, destinationRectangle, Vector2.Zero, 0, Colors.White);
	}

	private void CheckGrounded()
	{
		int leftFootX = (int)position.X + 1;
		int rightFootX = (int)position.X + width - 1;
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

	private void GroundedUpdate()
	{
		velocity.Y = 0;

		if (Keyboard.IsKeyDown(KeyboardKey.Left))
		{
			velocity.X = -walkSpeed;
			direction = -1;
		}
		else if (Keyboard.IsKeyDown(KeyboardKey.Right))
		{
			velocity.X = walkSpeed;
			direction = 1;
		}
		else velocity.X = 0;
	}

	private void MidairUpdate()
	{
		velocity.Y += gravity * Engine.FrameTime;

		if (Keyboard.IsKeyDown(KeyboardKey.Left))
		{
			velocity.X -= floatAcceleration * Engine.FrameTime;
			direction = -1;
		}
		else if (Keyboard.IsKeyDown(KeyboardKey.Right))
		{
			velocity.X += floatAcceleration * Engine.FrameTime;
			direction = 1;
		}
	}

	private void WallCollision(int checkLeftX, int checkRightX)
	{
		bool isLeftWall = currentArea.IsWall(checkLeftX, (int)position.Y + height - 1);
		bool isRightWall = currentArea.IsWall(checkRightX, (int)position.Y + height - 1);
		if (isLeftWall || isRightWall) velocity.X = 0;
	}

	private void Movement()
	{
		position += velocity * Engine.FrameTime;
	}
}
