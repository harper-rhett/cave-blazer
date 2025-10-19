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
	private bool isGrounded;
	private int direction = 1;
	private Area currentArea;
	private GameScene gameScene;

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
		int checkLeftX = nextPosition.X.Floored();
		int checkRightX = (nextPosition.X + width - 1).Floored();
		int checkY = nextPosition.Y.Floored();

		// Check if in bounds
		bool leftInBounds = currentArea.InBounds(checkLeftX, checkY);
		bool rightInBounds = currentArea.InBounds(checkRightX, checkY);
		bool inBounds = leftInBounds && rightInBounds;

		if (inBounds)
		{
			WallCollision(checkLeftX, checkRightX);
			Movement();
		}
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

	private void CheckGrounded()
	{
		int leftFootX = (position.X + 1).Floored();
		int rightFootX = (position.X + width - 2).Floored();
		int feetY = (position.Y + height).Floored();
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
	}

	private void WallCollision(int checkLeftX, int checkRightX)
	{
		bool isLeftWall = currentArea.IsWall(checkLeftX, position.Y.Floored() + height - 1);
		bool isRightWall = currentArea.IsWall(checkRightX, position.Y.Floored() + height - 1);
		if (isLeftWall || isRightWall) velocity.X = 0;
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
