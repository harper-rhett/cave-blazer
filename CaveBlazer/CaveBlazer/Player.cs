using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Input;
using HarpEngine.Shapes;
using HarpEngine.Tiles;
using HarpEngine.Utilities;
using System.Numerics;

public class Player : Entity, IIntersectsWithRectangle
{
	// Position
	private Vector2 position;
	public Vector2 Position => position;
	private Vector2 velocity;

	// General
	private GameScene gameScene;
	private PlayerState state = new();
	private PlayerInventory inventory = new();
	public TiledArea CurrentArea;

	// Animation
	private PlayerAnimation animationManager = new();
	private int direction = 1;

	// Collision
	private TiledCollider<TileType> collider;
	private Vector2 colliderOffset = new(4, 1);
	private Vector2 colliderPosition => position + colliderOffset;
	public const int ColliderWidth = 8;
	public const int ColliderHeight = 15;

	// Settings
	private const float gravity = 135;
	private const float jumpForce = 75;
	private const float walkSpeed = 45;
	private const float midairAcceleration = 10;
	private const float midairDecceleration = 25f;
	private const float newAreaBoost = 1.5f;
	private const float ladderClimbSpeed = 25;

	public Player(GameScene gameScene, TiledArea area, Vector2 position)
	{
		this.position = position;
		CurrentArea = area;
		this.gameScene = gameScene;

		collider = new(ColliderWidth, ColliderHeight);
		inventory.UnlockClimbing();
	}

	public override void OnUpdate()
	{
		// Update states
		collider.Update(CurrentArea, colliderPosition);
		state.Update(collider);

		// Check states

		if (state.DidJump) Jump();
		else
		{
			if (state.IsOnLadder) OnLadder();
			else if (state.IsGrabbingWall) GrabbingWall();
			else
			{
				if (state.IsGrounded) Grounded();
				else Midair();
			}
		}

		if (state.OutOfBounds) OutOfBounds();
		Movement();
	}

	public override void OnDraw()
	{
		// Draw player texture
		animationManager.Draw(position, new(direction, 1), Colors.White);

		// Draw status
		if (state.CanGrabLadder || state.CanGrabWall)
		{
			Vector2 statusPosition = colliderPosition + new Vector2(collider.HalfWidth, -collider.HalfHeight / 2f);
			Primitives.DrawCircle(statusPosition, 3, Colors.Blue);
			Primitives.DrawCircle(statusPosition, 2, Colors.SkyBlue);
		}

		// Draw collider
		//collider.Draw(colliderPosition, Colors.Red);
	}

	public override void OnDrawGUI()
	{
		Engine.DrawDebug(5, 5);
	}

	private void Jump()
	{
		velocity.Y = -jumpForce;
		animationManager.State = PlayerAnimationState.Jumping;
		animationManager.jumpingAnimation.Reset();
		animationManager.fallingAnimation.Reset();
	}

	private void OnLadder()
	{
		// Move left or right
		StrafeCheck();
		velocity.Y = 0;

		// Animation
		animationManager.State = PlayerAnimationState.ClimbingLadder;
		if (float.Abs(velocity.X) > 0 || state.IsClimbingLadder) animationManager.climbingLadderAnimation.IsPaused = false;
		else animationManager.climbingLadderAnimation.IsPaused = true;

		// Move up or down
		if (state.IsClimbingUpLadder) position.Y -= ladderClimbSpeed * Engine.FrameTime;
		else if (state.IsClimbingDownLadder) position.Y += ladderClimbSpeed * Engine.FrameTime;
	}

	private void GrabbingWall()
	{

	}

	private void Grounded()
	{
		// Zero velocity
		if (velocity.Y > 0) velocity.Y = 0;
		animationManager.fallingAnimation.Reset();

		// Correct tile clipping
		if (collider.IsTileInner(TileType.Wall))
		{
			int tileY = (position.Y.Floored() / GameScene.TileSize) - 1;
			position.Y = tileY * GameScene.TileSize;
		}
		else position.Y = position.Y.Floored();

		// Walk
		StrafeCheck();
		WalkAnimation();

		// Drop through platforms
		if (Keyboard.IsKeyPressed(KeyboardKey.Down) && state.IsOnPlatform) position.Y += 1;
	}

	private void StrafeCheck()
	{
		if (Keyboard.IsKeyDown(KeyboardKey.Left)) StrafeLeft();
		else if (Keyboard.IsKeyDown(KeyboardKey.Right)) StrafeRight();
		else velocity.X = 0;
	}

	private void StrafeLeft()
	{
		bool isWallLeft = collider.IsTileLeft(TileType.Wall);
		velocity.X = isWallLeft ? 0 : -walkSpeed;
		direction = -1;
	}

	private void StrafeRight()
	{
		bool isWallRight = collider.IsTileRight(TileType.Wall);
		velocity.X = isWallRight ? 0 : walkSpeed;
		direction = 1;
	}

	private void WalkAnimation()
	{
		if (float.Abs(velocity.X) > 0) animationManager.State = PlayerAnimationState.Walking;
		else animationManager.State = PlayerAnimationState.Idle;
	}

	private void Midair()
	{
		// Apply gravity
		velocity.Y += gravity * Engine.FrameTime;
		if (velocity.Y > 0) animationManager.State = PlayerAnimationState.Falling;

		// Get midair movement
		if (Keyboard.IsKeyDown(KeyboardKey.Left))
		{
			float acceleration = velocity.X < 0 ? midairAcceleration : midairDecceleration;
			velocity.X -= acceleration * Engine.FrameTime;
			direction = -1;
		}
		else if (Keyboard.IsKeyDown(KeyboardKey.Right))
		{
			float acceleration = velocity.X > 0 ? midairAcceleration : midairDecceleration;
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
			velocity *= newAreaBoost;
		}
		else
		{
			velocity.X = -float.Sign(velocity.X) * jumpForce;
			velocity.Y = -jumpForce;
		}
	}

	public bool IntersectsWithRectangle(Rectangle rectangle)
	{
		Rectangle playerRectangle = new(colliderPosition, ColliderWidth, ColliderHeight);
		return Intersection.RectangleOnRectangle(playerRectangle, rectangle);
	}
}
