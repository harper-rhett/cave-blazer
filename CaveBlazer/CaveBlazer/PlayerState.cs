using HarpEngine.Graphics;
using HarpEngine.Input;
using HarpEngine.Tiles;
using HarpEngine.Utilities;
using System.Numerics;

public class PlayerState
{
	// General
	private Player player;

	// Grounded
	public bool IsGrounded { get; private set; }
	public bool IsStandingOnPlatform { get; private set; }
	public bool IsStandingOnTile { get; private set; }
	public bool IsStandingOnWall { get; private set; }

	// Ladders
	public bool IsOverLadder { get; private set; }
	public bool CanGrabLadder { get; private set; }
	public bool IsOnLadder { get; private set; }
	public bool IsClimbingLadder { get; private set; }
	public bool IsClimbingLadderUp { get; private set; }
	public bool IsClimbingLadderDown { get; private set; }

	// Climbing
	public bool CanGrabWall { get; private set; }
	public bool DidGrabWall { get; private set; }
	public bool IsGrabbingWall {  get; private set; }
	public bool IsGrabbingLeftWall { get; private set; }
	public bool IsGrabbingRightWall { get; private set; }
	public bool IsClimbingWall { get; private set; }
	public bool IsClimbingWallUp { get; private set; }
	public bool IsClimbingWallDown { get; private set; }
	public bool DidWallJump { get; private set; }
	public bool DidVault { get; private set; }

	// Etcetera
	public bool DidJump { get; private set; }
	public bool OutOfBounds { get; private set; }

	public PlayerState(Player player)
	{
		this.player = player;
	}

	public void Update()
	{
		CheckGrounded();
		CheckForLadder();
		if (!IsGrabbingWall) CheckGrabbing();
		if (IsGrabbingWall) CheckClimbing();
		CheckForJump();
		CheckBounds();
	}

	private void CheckGrounded()
	{
		bool isOverLadder = player.Collider.IsTileBottom(TileType.Ladder) && player.Collider.CenterTile != TileType.Ladder;
		bool isOverPlatform = player.Collider.IsTileBottom(TileType.Platform);
		IsStandingOnTile = player.Collider.BottomY % GameScene.TileSize == 0;
		IsStandingOnPlatform = (isOverLadder || isOverPlatform) && IsStandingOnTile;
		IsStandingOnWall = player.Collider.IsTileBottom(TileType.Wall);
		IsGrounded = IsStandingOnWall || IsStandingOnPlatform;
		if (IsGrounded && !IsGrabbingWall) player.Inventory.ResetStamina();
	}

	private void CheckForLadder()
	{
		// Check collision
		IsOverLadder = player.Collider.CenterTile == TileType.Ladder || (player.Collider.IsTileBottom(TileType.Ladder) && !IsStandingOnTile);

		// Check interaction
		IsOnLadder = IsOnLadder && IsOverLadder;
		IsOnLadder = IsOnLadder || IsOverLadder && Keyboard.IsKeyPressed(KeyboardKey.Up);
		CanGrabLadder = IsOverLadder && !IsOnLadder;

		// Reset states
		IsClimbingLadderUp = false;
		IsClimbingLadderDown = false;
		IsClimbingLadder = false;

		// Climb up and down the ladder
		if (IsOverLadder && IsOnLadder)
		{
			if (Keyboard.IsKeyDown(KeyboardKey.Up) && !player.Collider.IsTileTop(TileType.Wall)) IsClimbingLadderUp = true;
			else if (Keyboard.IsKeyDown(KeyboardKey.Down) && !IsStandingOnWall) IsClimbingLadderDown = true;
			IsClimbingLadder = IsClimbingLadderUp || IsClimbingLadderDown;
		}
	}

	private void CheckGrabbing()
	{
		// Reset states
		DidWallJump = false;
		DidVault = false;

		// Check for walls to grab
		bool isWallLeft = player.Collider.LeftCenterTile == TileType.Wall && player.FacingLeft;
		bool isWallRight = player.Collider.RightCenterTile == TileType.Wall && player.FacingRight;
		bool isWall = isWallLeft || isWallRight;
		CanGrabWall = !IsGrounded && isWall && !IsOverLadder && player.Inventory.Stamina > 0;

		// Grab wall
		DidGrabWall = CanGrabWall && Keyboard.IsKeyPressed(KeyboardKey.Space);
		IsGrabbingWall = IsGrabbingWall || DidGrabWall;
		if (DidGrabWall)
		{
			IsGrabbingLeftWall = isWallLeft;
			IsGrabbingRightWall = isWallRight;
		}
	}

	private void CheckClimbing()
	{
		// Check stamina
		if (player.Inventory.Stamina <= 0)
		{
			Ungrab();
			return;
		}

		// Dismount
		DidWallJump = Keyboard.IsKeyPressed(KeyboardKey.Space) && IsGrabbingWall && !DidGrabWall;
		if (DidWallJump)
		{
			Ungrab();
			return;
		}

		// Climb
		IsClimbingWallUp = IsGrabbingWall && Keyboard.IsKeyDown(KeyboardKey.Up);
		IsClimbingWallDown = IsGrabbingWall && Keyboard.IsKeyDown(KeyboardKey.Down) && !IsStandingOnWall;
		IsClimbingWall = IsClimbingWallUp || IsClimbingWallDown;

		// Vault
		bool leftVaultTile = IsGrabbingLeftWall && player.Collider.LeftCenterTile == TileType.None;
		bool rightVaultTile = IsGrabbingRightWall && player.Collider.RightCenterTile == TileType.None;
		bool vaultTile = leftVaultTile || rightVaultTile;
		DidVault = IsGrabbingWall && vaultTile;
		if (DidVault) Ungrab();

		// Reset states
		DidGrabWall = false;
	}

	private void Ungrab()
	{
		IsGrabbingWall = false;
		CanGrabWall = false;
	}

	private void CheckForJump()
	{
		DidJump = IsGrounded && Keyboard.IsKeyPressed(KeyboardKey.Space);
		if (DidJump) IsGrounded = false;
	}

	private void CheckBounds()
	{
		OutOfBounds = !player.Collider.CenterInBounds;
	}

	public void Draw()
	{
		if (CanGrabLadder) DrawStatusOrb(Colors.Green, Colors.Lime);
		else if (CanGrabWall && !IsGrabbingWall) DrawStatusOrb(Colors.SkyBlue, Colors.Blue);
	}

	public void DrawStatusOrb(Color innerColor, Color outerColor)
	{
		const float offset = -5;
		Vector2 statusPosition = player.ColliderPosition + new Vector2(player.Collider.HalfWidth, offset);
		Primitives.DrawCircle(statusPosition.Floored(), 3, outerColor);
		Primitives.DrawCircle(statusPosition.Floored(), 2, innerColor);
	}
}
