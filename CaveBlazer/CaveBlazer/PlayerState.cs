using HarpEngine.Input;
using HarpEngine.Tiles;

public class PlayerState
{
	// General
	private Player player;
	private TiledCollider<TileType> collider;
	private PlayerInventory inventory;

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

	public PlayerState(Player player, TiledCollider<TileType> collider, PlayerInventory inventory)
	{
		this.player = player;
		this.collider = collider;
		this.inventory = inventory;
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
		bool isOverLadder = collider.IsTileBottom(TileType.Ladder) && collider.CenterTile != TileType.Ladder;
		bool isOverPlatform = collider.IsTileBottom(TileType.Platform);
		IsStandingOnTile = collider.BottomY % GameScene.TileSize == 0;
		IsStandingOnPlatform = (isOverLadder || isOverPlatform) && IsStandingOnTile;
		IsStandingOnWall = collider.IsTileBottom(TileType.Wall);
		IsGrounded = IsStandingOnWall || IsStandingOnPlatform;
		if (IsGrounded) inventory.ResetStamina();
	}

	private void CheckForLadder()
	{
		// Check collision
		IsOverLadder = collider.CenterTile == TileType.Ladder || (collider.IsTileBottom(TileType.Ladder) && !IsStandingOnTile);

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
			if (Keyboard.IsKeyDown(KeyboardKey.Up) && !collider.IsTileTop(TileType.Wall)) IsClimbingLadderUp = true;
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
		bool isWallLeft = collider.LeftCenterTile == TileType.Wall && player.FacingLeft;
		bool isWallRight = collider.RightCenterTile == TileType.Wall && player.FacingRight;
		bool isWall = isWallLeft || isWallRight;
		CanGrabWall = !IsGrounded && isWall && !IsOverLadder && inventory.Stamina > 0;

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
		if (inventory.Stamina <= 0)
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
		bool leftVaultTile = IsGrabbingLeftWall && collider.LeftCenterTile == TileType.None;
		bool rightVaultTile = IsGrabbingRightWall && collider.RightCenterTile == TileType.None;
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
		OutOfBounds = !collider.CenterInBounds;
	}
}
