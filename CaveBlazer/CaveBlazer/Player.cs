using Clockwork;
using Clockwork.Graphics;
using Clockwork.Input;
using Clockwork.LDTKImporter;
using Clockwork.Shapes;
using Clockwork.Tiles;
using Clockwork.Utilities;
using System.Numerics;

public class Player : Entity, IIntersectsWithRectangle
{
	#region Fields

	// Position
	private Vector2 position;
	public Vector2 Position => position;
	private Vector2 velocity;
	public Vector2 Velocity => velocity;

	// General
	private GameScene gameScene;
	public PlayerState State;
	public PlayerInventory Inventory;
	public TiledGameArea CurrentArea;

	// Animation
	private PlayerAnimation animationManager = new();
	private int directionFacing = 1;
	public bool FacingLeft => directionFacing == -1;
	public bool FacingRight => directionFacing == 1;

	// Collision
	public readonly TiledCollider<TileType> Collider;
	private Vector2 colliderOffset = new(4, 1);
	public Vector2 ColliderPosition => position + colliderOffset;
	public const int ColliderWidth = 8;
	public const int ColliderHeight = 15;

	// Settings
	private const float gravity = 135;
	private const float jumpForce = 75;
	private const float walkSpeed = 45;
	private const float midairAcceleration = 10;
	private const float midairDecceleration = 35f;
	private const float newAreaBoost = 1.5f;
	private const float climbSpeed = 25;

	#endregion

	#region Base Class

	public Player(GameScene gameScene, LDTKGameArea area, Vector2 position)
	{
		this.position = position;
		CurrentArea = area;
		this.gameScene = gameScene;

		Collider = new(ColliderWidth, ColliderHeight);
		State = new(this);
		Inventory = new(this);
	}

	public override void OnUpdate()
	{
		// Update states
		Collider.CaptureState(CurrentArea, ColliderPosition);
		State.Update();

		// Check states

		if (State.DidJump || State.DidVault) Jump();
		else if (State.DidWallJump) WallJump();
		else
		{
			if (State.IsOnLadder) OnLadder();
			else if (State.IsGrabbingWall) GrabbingWall();
			else
			{
				if (State.IsGrounded) Grounded();
				else Midair();
			}
		}

		if (State.OutOfBounds) OutOfBounds();
		ApplyMovement();
		Collider.CaptureState(CurrentArea, ColliderPosition);
		CheckCollision();
	}

	public override void OnDraw()
	{
		// Draw player texture
		animationManager.Draw(position, new(directionFacing, 1), Colors.White);

		// Draw status
		State.Draw();
		Inventory.Draw();

		// Draw collider
		//collider.Draw(colliderPosition, Colors.Red);
	}

	public override void OnDrawGUI()
	{
		Engine.DrawDebug(5, 5);
		//Text.Draw($"Grounded: {State.IsGrounded}", 5, 15, 5, Colors.White);
	}

	#endregion

	#region Behavior

	private void Jump()
	{
		velocity.Y = -jumpForce;
		animationManager.State = PlayerAnimationState.Jumping;
		animationManager.jumpingAnimation.Reset();
		animationManager.fallingAnimation.Reset();
	}

	private void WallJump()
	{
		// Jump
		if (State.IsGrabbingRightWall)
		{
			Vector2 direction = Vector2.Normalize(new(-1, -1));
			velocity = direction * jumpForce;
			directionFacing = -1;
		}
		else if (State.IsGrabbingLeftWall)
		{
			Vector2 direction = Vector2.Normalize(new(1, -1));
			velocity = direction * jumpForce;
			directionFacing = 1;
		}

		// Animation
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
		if (float.Abs(velocity.X) > 0 || State.IsClimbingLadder) animationManager.climbingLadderAnimation.IsPaused = false;
		else animationManager.climbingLadderAnimation.IsPaused = true;

		// Move up or down
		if (State.IsClimbingLadderUp) position.Y -= climbSpeed * Engine.FrameTime;
		else if (State.IsClimbingLadderDown) position.Y += climbSpeed * Engine.FrameTime;
	}

	private void GrabbingWall()
	{
		velocity.Y = 0;

		animationManager.State = PlayerAnimationState.ClimbingWall;
		if (State.IsClimbingWall)
		{
			animationManager.climbingWallAnimation.IsPaused = false;
			float stamina = climbSpeed * Engine.FrameTime;
			if (State.IsClimbingWallUp) position.Y -= stamina;
			else if (State.IsClimbingWallDown) position.Y += stamina;
			Inventory.UseStamina(stamina);
		}
		else animationManager.climbingWallAnimation.IsPaused = true;
	}

	private void Grounded()
	{
		// Zero velocity
		if (velocity.Y > 0) velocity.Y = 0;
		animationManager.fallingAnimation.Reset();

		// Correct tile clipping
		if (Collider.IsTileInner(TileType.Wall))
		{
			int tileY = (position.Y.Floored() / GameScene.TileSize) - 1;
			position.Y = tileY * GameScene.TileSize;
		}
		else position.Y = position.Y.Floored();

		// Walk
		StrafeCheck();
		WalkAnimation();

		// Drop through platforms
		if (Keyboard.IsKeyPressed(KeyboardKey.Down) && State.IsStandingOnPlatform) position.Y += 1;
	}

	private void StrafeCheck()
	{
		if (Keyboard.IsKeyDown(KeyboardKey.Left)) StrafeLeft();
		else if (Keyboard.IsKeyDown(KeyboardKey.Right)) StrafeRight();
		else velocity.X = 0;
	}

	private void StrafeLeft()
	{
		bool isWallLeft = Collider.IsTileLeft(TileType.Wall);
		velocity.X = isWallLeft ? 0 : -walkSpeed;
		directionFacing = -1;
	}

	private void StrafeRight()
	{
		bool isWallRight = Collider.IsTileRight(TileType.Wall);
		velocity.X = isWallRight ? 0 : walkSpeed;
		directionFacing = 1;
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
			directionFacing = -1;
		}
		else if (Keyboard.IsKeyDown(KeyboardKey.Right))
		{
			float acceleration = velocity.X > 0 ? midairAcceleration : midairDecceleration;
			velocity.X += acceleration * Engine.FrameTime;
			directionFacing = 1;
		}

		// Check for wall collision
		bool isWallLeft = Collider.IsTileLeft(TileType.Wall);
		bool isWallRight = Collider.IsTileRight(TileType.Wall);
		bool isWallTop = Collider.IsTileTop(TileType.Wall);
		if (isWallLeft && velocity.X < 0 || isWallRight && velocity.X > 0) velocity.X = 0;
		if (isWallTop && velocity.Y < 0) velocity.Y = 0;
	}

	private void ApplyMovement()
	{
		position += velocity * Engine.FrameTime;
	}

	private void OutOfBounds()
	{
		bool areaExists = gameScene.World.DoesAreaExist(Collider.CenterX, Collider.CenterY);

		if (areaExists)
		{
			gameScene.SwitchArea(Collider.CenterX, Collider.CenterY);
			velocity *= newAreaBoost;
		}
		else
		{
			velocity.X = -float.Sign(velocity.X) * jumpForce;
			velocity.Y = -jumpForce;
		}
	}

	private void CheckCollision()
	{
		// I need to create a PlatformPlayer.cs for the ClockworkEngine examples
		// that is surefire to work with basic walking, jumping, and colliding.
		// Then, I inherit from there.

		// Also, I fear I need to optimize TiledCollider. I would like to update states without cost of performance.
	}

	#endregion

	#region Extra

	public bool IntersectsWithRectangle(Rectangle rectangle)
	{
		Rectangle playerRectangle = new(ColliderPosition, ColliderWidth, ColliderHeight);
		return Intersection.RectangleOnRectangle(playerRectangle, rectangle);
	}

	#endregion
}
