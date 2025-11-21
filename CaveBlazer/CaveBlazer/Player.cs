using HarpEngine;
using HarpEngine.Animation;
using HarpEngine.Graphics;
using HarpEngine.Input;
using HarpEngine.Tiles;
using HarpEngine.Utilities;
using System.Diagnostics;
using System.Numerics;

public class Player : Entity
{
	// General
	private Vector2 position;
	private Vector2 velocity;
	private int direction = 1;
	private GameScene gameScene;
	private TiledCollider<TileType> collider;
	private PlayerState playerState;
	private PlayerAnimationManager animationManager = new();
	private Vector2 colliderOffset = new(4, 1);

	// Settings
	private const float gravity = 135;
	private const float jumpForce = 75;
	private const float walkSpeed = 35;
	private const float midairAcceleration = 25;
	private const float newLevelBoost = 1.5f;
	private const int colliderWidth = 8;
	private const int colliderHeight = 15;
	private const float ladderClimbSpeed = 25;

	// Interface
	public TiledArea CurrentArea;
	public Vector2 Position => position;

	public Player(GameScene gameScene, TiledArea area, Vector2 position)
	{
		this.position = position;
		CurrentArea = area;
		this.gameScene = gameScene;

		collider = new(colliderWidth, colliderHeight);
		playerState = new();
	}

	public override void OnUpdate()
	{
		// Update states
		collider.Update(CurrentArea, position + colliderOffset);
		playerState.Update(collider);

		// Check states

		if (playerState.DidJump) Jump();
		else
		{
			if (playerState.IsOnLadder) LadderUpdate();
			else
			{
				if (playerState.IsGrounded) GroundedUpdate();
				else MidairUpdate();
			}
		}

		if (playerState.OutOfBounds) OutOfBounds();
		Movement();
	}

	public override void OnDraw()
	{
		animationManager.Draw(position, new(direction, 1), Colors.White);
		//collider.Draw(position + colliderOffset, Colors.Red);
	}

	private void Jump()
	{
		velocity.Y = -jumpForce;
		animationManager.State = PlayerAnimation.Jumping;
		animationManager.CurrentAnimation.Reset();
	}

	private void LadderUpdate()
	{
		WalkingUpdate(); // need to add climbing animation for side to side movement here
		velocity.Y = 0;

		if (!playerState.IsGrounded)
		{
			animationManager.State = PlayerAnimation.ClimbingLadder;
			animationManager.CurrentAnimation.IsPaused = !playerState.IsClimbingLadder;
		}

		if (playerState.IsClimbingUpLadder) position.Y -= ladderClimbSpeed * Engine.FrameTime;
		else if (playerState.IsClimbingDownLadder) position.Y += ladderClimbSpeed * Engine.FrameTime;
	}

	private void GroundedUpdate()
	{
		if (velocity.Y > 0) velocity.Y = 0;
		position.Y = position.Y.Floored();

		WalkingUpdate();

		if (Keyboard.IsKeyPressed(KeyboardKey.Down) && playerState.IsOnPlatform) position.Y += 1;
	}

	private void WalkingUpdate()
	{
		if (Keyboard.IsKeyDown(KeyboardKey.Left))
		{
			bool isWallLeft = collider.IsTileLeft(TileType.Wall);
			velocity.X = isWallLeft ? 0 : -walkSpeed;
			direction = -1;
			animationManager.State = PlayerAnimation.Walking;
		}
		else if (Keyboard.IsKeyDown(KeyboardKey.Right))
		{
			bool isWallRight = collider.IsTileRight(TileType.Wall);
			velocity.X = isWallRight ? 0 : walkSpeed;
			direction = 1;
			animationManager.State = PlayerAnimation.Walking;
		}
		else
		{
			velocity.X = 0;
			animationManager.State = PlayerAnimation.Idle;
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
