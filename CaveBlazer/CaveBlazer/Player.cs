using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Input;
using HarpEngine.Utilities;
using System.Diagnostics;
using System.Numerics;

internal class Player : Entity
{
	// General
	private Vector2 position;
	private Vector2 velocity;
	private int direction = 1;
	private bool isGrounded;
	private GameScene gameScene;
	private TileChecker tileChecker;

	// Texture
	private Texture texture;
	private int width;
	private int height;
	private int halfWidth;
	private int halfHeight;

	// Settings
	private const float gravity = 100;
	private const float jumpForce = 45;
	private const float walkSpeed = 25;
	private const float midairAcceleration = 15;

	// Interface
	public Area CurrentArea;
	public Vector2 Position => position;

	public Player(GameScene gameScene, Area area, Vector2 position) : base(gameScene)
	{
		this.position = position;
		CurrentArea = area;
		this.gameScene = gameScene;

		texture = Texture.Load("sprites/explorer.png");
		width = texture.Width;
		height = texture.Height;
		halfWidth = width / 2;
		halfHeight = height / 2;
		tileChecker = new(width, height);
	}

	public override void Update()
	{
		// State checks
		tileChecker.Refresh(CurrentArea, position);
		isGrounded = tileChecker.BottomWall;
		CheckJump();

		// State updates
		if (isGrounded) GroundedUpdate();
		else MidairUpdate();

		// Check if in bounds
		int boundsCheckX = position.X.Floored() + halfWidth;
		int boundsCheckY = position.Y.Floored() + halfHeight;
		bool inBounds = CurrentArea.InBounds(boundsCheckX, boundsCheckY);

		if (inBounds) Movement();
		else
		{
			bool areaExists = gameScene.World.DoesAreaExist(boundsCheckX, boundsCheckY);
			if (areaExists)
			{
				gameScene.SwitchArea(boundsCheckX, boundsCheckY);
				Vector2 tilePosition = CurrentArea.SnapPosition(boundsCheckX, boundsCheckY);
				position = tilePosition;
			}
			else
			{
				velocity.X = -velocity.X;
			}
		}
	}

	public override void Draw()
	{
		Rectangle sourceRectangle = new(0, 0, width * direction, height);
		Rectangle destinationRectangle = new(position.X.Floored(), position.Y.Floored(), width, height);
		texture.Draw(sourceRectangle, destinationRectangle, Vector2.Zero, 0, Colors.White);
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
		position.Y = position.Y.Floored();

		if (Keyboard.IsKeyDown(KeyboardKey.Left))
		{
			velocity.X = tileChecker.LeftWall ? 0 : -walkSpeed;
			direction = -1;
		}
		else if (Keyboard.IsKeyDown(KeyboardKey.Right))
		{
			velocity.X = tileChecker.RightWall ? 0 : walkSpeed;
			direction = 1;
		}
		else velocity.X = 0;
	}

	private void MidairUpdate()
	{
		// Apply gravity
		velocity.Y += gravity * Engine.FrameTime;

		// Get midair movement
		if (Keyboard.IsKeyDown(KeyboardKey.Left))
		{
			float acceleration = velocity.X < 0 ? midairAcceleration : midairAcceleration * 2f;
			velocity.X -= acceleration * Engine.FrameTime;
			direction = -1;
		}
		else if (Keyboard.IsKeyDown(KeyboardKey.Right))
		{
			float acceleration = velocity.X > 0 ? midairAcceleration : midairAcceleration * 2f;
			velocity.X += acceleration * Engine.FrameTime;
			direction = 1;
		}

		// Check for wall collision
		if (tileChecker.LeftWall && velocity.X < 0 || tileChecker.RightWall && velocity.X > 0) velocity.X = 0;
		if (tileChecker.TopWall && velocity.Y < 0) velocity.Y = 0;
	}

	private void Movement()
	{
		position += velocity * Engine.FrameTime;
	}
}
