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
	private int halfWidth;
	private int halfHeight;

	// Settings
	private const float gravity = 90;
	private const float jumpForce = 40;
	private const float walkSpeed = 25;
	private const float midairAcceleration = 10;

	// Interface
	public Vector2 Position => position;

	public Player(GameScene gameScene, Area area, Vector2 position) : base(gameScene)
	{
		this.position = position;
		currentArea = area;
		this.gameScene = gameScene;

		texture = Texture.Load("sprites/explorer.png");
		width = texture.Width;
		height = texture.Height;
		halfWidth = width / 2;
		halfHeight = height / 2;
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

		// NEED TO REPLACE NEXT SEVERAL LINES WITH NEW WALL CHECKER CODE

		// I believe we should only check out of bounds for the center of the character.
		// We will teleport the character to the grid space they move out of bounds to.

		// Check if in bounds
		int boundsCheckX = position.X.Floored() + halfWidth;
		int boundsCheckY = position.Y.Floored() + halfHeight;
		bool inBounds = currentArea.InBounds(boundsCheckX, boundsCheckY);

		if (inBounds) Movement();
		else
		{
			bool areaExists = gameScene.World.DoesAreaExist(boundsCheckX, boundsCheckY);
			if (areaExists)
			{
				// TELEPORT PLAYER TO GRID SPACE THE MOVE OUT OF BOUNDS TO
				// OR CREATE ENTRANCES IN LEVEL EDITOR, AND TELEPORT TO CLOSEST ENTRANCE
				gameScene.SwitchArea(boundsCheckX, boundsCheckY);
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
