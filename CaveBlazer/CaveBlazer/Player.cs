using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Input;
using HarpEngine.Tiles;
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
	private TiledCollider<TileType> collider;

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
	public TiledArea CurrentArea;
	public Vector2 Position => position;

	public Player(GameScene gameScene, TiledArea area, Vector2 position)
	{
		this.position = position;
		CurrentArea = area;
		this.gameScene = gameScene;

		texture = Texture.Load("sprites/explorer.png");
		width = texture.Width;
		height = texture.Height;
		halfWidth = width / 2;
		halfHeight = height / 2;
		collider = new(width, height);
	}

	public override void OnUpdate()
	{
		// State checks
		collider.Update(CurrentArea, position);
		isGrounded = collider.IsTileBottom(TileType.Wall);
		CheckJump();

		// State updates
		if (isGrounded) GroundedUpdate();
		else MidairUpdate();

		// Check if in bounds
		if (collider.CenterInBounds) Movement();
		else OutOfBounds();
	}

	public override void OnDraw()
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
			bool isWallLeft = collider.IsTileLeft(TileType.Wall);
			velocity.X = isWallLeft ? 0 : -walkSpeed;
			direction = -1;
		}
		else if (Keyboard.IsKeyDown(KeyboardKey.Right))
		{
			bool isWallRight = collider.IsTileRight(TileType.Wall);
			velocity.X = isWallRight ? 0 : walkSpeed;
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
		bool isWallLeft = collider.IsTileLeft(TileType.Wall);
		bool isWallRight = collider.IsTileRight(TileType.Wall);
		bool isWallTop = collider.IsTileTop(TileType.Wall);
		if (isWallLeft && velocity.X < 0 || isWallRight && velocity.X > 0) velocity.X = 0;
		if (isWallTop && velocity.Y < 0) velocity.Y = 0;
	}

	private void Movement()
	{
		position += velocity * Engine.FrameTime;
	}

	private void OutOfBounds()
	{
		bool areaExists = gameScene.World.DoesAreaExist(collider.CenterX, collider.CenterY);

		if (areaExists)
		{
			gameScene.SwitchArea(collider.CenterX, collider.CenterY);
			Vector2 tilePosition = gameScene.World.SnapPosition(collider.CenterX, collider.CenterY);
			position = tilePosition;
		}
		else velocity.X = -velocity.X;
	}
}
