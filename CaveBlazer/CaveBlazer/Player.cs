using HarpEngine;
using HarpEngine.Animation;
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
	private bool isPlatformed;
	private GameScene gameScene;
	private TiledCollider<TileType> collider;
	private TextureAnimationManager<Animation> animationManager = new();
	private Vector2 colliderOffset = new(4, 1);

	// Settings
	private const float gravity = 135;
	private const float jumpForce = 75;
	private const float walkSpeed = 35;
	private const float midairAcceleration = 25;
	private const float newLevelBoost = 1.5f;
	private const int colliderWidth = 8;
	private const int colliderHeight = 15;

	// Interface
	public TiledArea CurrentArea;
	public Vector2 Position => position;

	public enum Animation
	{
		Idle,
		Walk,
		Jump,
	}

	public Player(GameScene gameScene, TiledArea area, Vector2 position)
	{
		this.position = position;
		CurrentArea = area;
		this.gameScene = gameScene;

		RegisterAnimations();
		collider = new(colliderWidth, colliderHeight);
	}

	private void RegisterAnimations()
	{
		Texture idleTexture = Texture.Load("sprites/player/idle.png");
		TextureAnimation idleAnimation = new(idleTexture, 4, 16, 16, 0.4f);
		animationManager.RegisterAnimation(idleAnimation, Animation.Idle);

		Texture walkTexture = Texture.Load("sprites/player/walk.png");
		TextureAnimation walkAnimation = new(walkTexture, 4, 16, 16, 0.2f);
		animationManager.RegisterAnimation(walkAnimation, Animation.Walk);

		Texture jumpTexture = Texture.Load("sprites/player/jump.png");
		TextureAnimation jumpAnimation = new(jumpTexture, 4, 16, 16, 0.1f);
		jumpAnimation.PlayOnce = true;
		animationManager.RegisterAnimation(jumpAnimation, Animation.Jump);
	}

	public override void OnUpdate()
	{
		// State checks
		collider.Update(CurrentArea, position + colliderOffset);
		isPlatformed = collider.IsTileBottom(TileType.Platform) && collider.BottomY % 16 == 0;
		isGrounded = collider.IsTileBottom(TileType.Wall) || isPlatformed;
		CheckJump();

		// State updates
		if (isGrounded) GroundedUpdate();
		else MidairUpdate();

		// Check if in bounds
		if (!collider.CenterInBounds) OutOfBounds();
		Movement();
	}

	public override void OnDraw()
	{
		animationManager.Draw(position, new(direction, 1), Colors.White);
		//collider.Draw(position + colliderOffset, Colors.Red);
	}

	private void CheckJump()
	{
		bool didJump = isGrounded && Keyboard.IsKeyPressed(KeyboardKey.Space);
		if (didJump) Jump();
	}

	private void Jump()
	{
		isGrounded = false;
		velocity.Y = -jumpForce;
		animationManager.CurrentAnimationID = Animation.Jump;
		animationManager.CurrentAnimation.Reset();
	}

	private void GroundedUpdate()
	{
		if (velocity.Y > 0) velocity.Y = 0;
		position.Y = position.Y.Floored();

		WalkingUpdate();

		if (Keyboard.IsKeyPressed(KeyboardKey.Down) && isPlatformed) position.Y += 1;
	}

	private void WalkingUpdate()
	{
		if (Keyboard.IsKeyDown(KeyboardKey.Left))
		{
			bool isWallLeft = collider.IsTileLeft(TileType.Wall);
			velocity.X = isWallLeft ? 0 : -walkSpeed;
			direction = -1;
			animationManager.CurrentAnimationID = Animation.Walk;
		}
		else if (Keyboard.IsKeyDown(KeyboardKey.Right))
		{
			bool isWallRight = collider.IsTileRight(TileType.Wall);
			velocity.X = isWallRight ? 0 : walkSpeed;
			direction = 1;
			animationManager.CurrentAnimationID = Animation.Walk;
		}
		else
		{
			velocity.X = 0;
			animationManager.CurrentAnimationID = Animation.Idle;
		}
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
			velocity *= newLevelBoost;
		}
		else velocity.X = -velocity.X;
	}
}
