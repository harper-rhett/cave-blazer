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
	private Area currentArea;
	private GameScene gameScene;
	private WallChecker wallChecker;

	// Texture
	private Texture texture;
	private int width;
	private int height;

	// Settings
	private const float gravity = 90;
	private const float jumpForce = 40;
	private const float walkSpeed = 25;
	private const float midairAcceleration = 10;

	// Interface
	public Vector2 Position => position;
	public Vector2 Center => position + new Vector2(width, height);

	public Player(GameScene gameScene, Area area, Vector2 position) : base(gameScene)
	{
		this.position = position;
		currentArea = area;
		this.gameScene = gameScene;

		texture = Texture.Load("sprites/explorer.png");
		width = texture.Width;
		height = texture.Height;
		wallChecker = new(width, height);
	}

	public override void Update()
	{
		// State checks
		wallChecker.Refresh(currentArea, position);
		isGrounded = wallChecker.BottomWall;
		CheckJump();

		// State updates
		if (isGrounded) GroundedUpdate();
		else MidairUpdate();

		// Get next presumed position
		Vector2 nextPosition = position + velocity * Engine.FrameTime;
		int checkLeftX = nextPosition.X.Floored();
		int checkRightX = (nextPosition.X + width - 1).Floored();
		int checkY = nextPosition.Y.Floored();

		// Check if in bounds
		bool leftInBounds = currentArea.InBounds(checkLeftX, checkY);
		bool rightInBounds = currentArea.InBounds(checkRightX, checkY);
		bool inBounds = leftInBounds && rightInBounds;

		if (inBounds) Movement();
		else
		{
			bool areaExists = gameScene.World.DoesAreaExist(leftInBounds ? checkRightX : checkLeftX, checkY);
			if (areaExists)
			{
				if (leftInBounds) position.X += width;
				else position.X -= width;
				gameScene.SwitchArea(leftInBounds ? checkRightX : checkLeftX, checkY);
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

		if (Keyboard.IsKeyDown(KeyboardKey.Left))
		{
			if (!wallChecker.LeftWall) velocity.X = -walkSpeed;
			direction = -1;
		}
		else if (Keyboard.IsKeyDown(KeyboardKey.Right))
		{
			if (!wallChecker.RightWall) velocity.X = walkSpeed;
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
		if (wallChecker.LeftWall && velocity.X < 0 || wallChecker.RightWall && velocity.X > 0) velocity.X = 0;
	}

	private void Movement()
	{
		position += velocity * Engine.FrameTime;
	}

	public void SetArea(Area area)
	{
		currentArea = area;
	}
}
