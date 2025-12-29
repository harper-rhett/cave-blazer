using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Input;
using HarpEngine.Shapes;
using HarpEngine.Tiles;
using HarpEngine.Utilities;
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
	private PlayerState state;
	public PlayerInventory Inventory = new();
	public TiledArea CurrentArea;

	// Animation
	private PlayerAnimation animationManager = new();
	private int directionFacing = 1;
	public bool FacingLeft => directionFacing == -1;
	public bool FacingRight => directionFacing == 1;

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
	private const float midairDecceleration = 35f;
	private const float newAreaBoost = 1.5f;
	private const float climbSpeed = 25;

	#endregion

	#region Base Class

	public Player(GameScene gameScene, TiledArea area, Vector2 position)
	{
		this.position = position;
		CurrentArea = area;
		this.gameScene = gameScene;

		collider = new(ColliderWidth, ColliderHeight);
		state = new(this, collider, Inventory);
	}

	public override void OnUpdate()
	{
		// Update states
		collider.Update(CurrentArea, colliderPosition);
		state.Update();

		// Check states

		if (state.DidJump || state.DidVault) Jump();
		else if (state.DidWallJump) WallJump();
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

	#endregion

	#region Drawing

	public override void OnDraw()
	{
		// Draw player texture
		animationManager.Draw(position, new(directionFacing, 1), Colors.White);

		// Draw status
		if (state.CanGrabLadder) DrawOrb(Colors.Green, Colors.Lime);
		else if (state.CanGrabWall && !state.IsGrabbingWall) DrawOrb(Colors.SkyBlue, Colors.Blue);
		else if (Inventory.Stamina < Inventory.MaxStamina) DrawStamina();

		// Draw collider
		//collider.Draw(colliderPosition, Colors.Red);
	}

	private void DrawOrb(Color innerColor, Color outerColor)
	{
		const float offset = -5;
		Vector2 statusPosition = colliderPosition + new Vector2(collider.HalfWidth, offset);
		Primitives.DrawCircle(statusPosition, 3, outerColor);
		Primitives.DrawCircle(statusPosition, 2, innerColor);
	}

	private void DrawStamina()
	{
		// Dimensions
		const int containerWidth = 4;
		const int halfContainerWidth = containerWidth / 2;
		const int containerHeight = 8;
		const int containerOffset = -11;
		const int staminaWidth = containerWidth - 2;
		const int staminaHeight = containerHeight - 2;

		// Draw container
		Vector2 containerPosition = colliderPosition + new Vector2(collider.HalfWidth - halfContainerWidth, containerOffset);
		Rectangle containerRectangle = new(containerPosition, containerWidth, containerHeight);
		Primitives.DrawRectangleLines(containerRectangle, 1, Colors.White);

		// Draw stamina
		int staminaRectangleHeight = (staminaHeight - Inventory.StaminaRatio * staminaHeight).Floored();
		Vector2 staminaPosition = containerPosition + Vector2.One + new Vector2(0, staminaRectangleHeight);
		Rectangle staminaRectangle = new(staminaPosition, staminaWidth, staminaHeight - staminaRectangleHeight);
		Primitives.DrawRectangle(staminaRectangle, Colors.White);
	}

	public override void OnDrawGUI()
	{
		Engine.DrawDebug(5, 5);
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
		if (state.IsGrabbingRightWall)
		{
			Vector2 direction = Vector2.Normalize(new(-1, -1));
			velocity = direction * jumpForce;
			directionFacing = -1;
		}
		else if (state.IsGrabbingLeftWall)
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
		if (float.Abs(velocity.X) > 0 || state.IsClimbingLadder) animationManager.climbingLadderAnimation.IsPaused = false;
		else animationManager.climbingLadderAnimation.IsPaused = true;

		// Move up or down
		if (state.IsClimbingLadderUp) position.Y -= climbSpeed * Engine.FrameTime;
		else if (state.IsClimbingLadderDown) position.Y += climbSpeed * Engine.FrameTime;
	}

	private void GrabbingWall()
	{
		velocity.Y = 0;

		animationManager.State = PlayerAnimationState.ClimbingWall;
		if (state.IsClimbingWall)
		{
			animationManager.climbingWallAnimation.IsPaused = false;
			float stamina = climbSpeed * Engine.FrameTime;
			if (state.IsClimbingWallUp) position.Y -= stamina;
			else if (state.IsClimbingWallDown) position.Y += stamina;
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
		if (Keyboard.IsKeyPressed(KeyboardKey.Down) && state.IsStandingOnPlatform) position.Y += 1;
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
		directionFacing = -1;
	}

	private void StrafeRight()
	{
		bool isWallRight = collider.IsTileRight(TileType.Wall);
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

	#endregion

	#region Extra

	public bool IntersectsWithRectangle(Rectangle rectangle)
	{
		Rectangle playerRectangle = new(colliderPosition, ColliderWidth, ColliderHeight);
		return Intersection.RectangleOnRectangle(playerRectangle, rectangle);
	}

	#endregion
}
