using HarpEngine.Input;
using HarpEngine.Tiles;

public class PlayerState
{
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
	public bool IsClimbingUpLadder { get; private set; }
	public bool IsClimbingDownLadder { get; private set; }

	// Climbing
	public bool CanGrabWall { get; private set; }
	public bool IsGrabbingWall {  get; private set; }
	public bool IsGrabbingLeftWall { get; private set; }
	public bool IsGrabbingRightWall { get; private set; }
	public bool IsClimbingWall { get; private set; }
	public bool IsClimbingUpWall { get; private set; }
	public bool IsClimbingDownWall { get; private set; }
	public bool DidDismount { get; private set; }

	// Etcetera
	public bool DidJump { get; private set; }
	public bool OutOfBounds { get; private set; }

	public void Update(TiledCollider<TileType> collider)
	{
		CheckGrounded(collider);
		CheckForLadder(collider);
		CheckClimbing(collider);
		CheckForJump(collider);
		CheckBounds(collider);
	}

	private void CheckGrounded(TiledCollider<TileType> collider)
	{
		bool isOverLadder = collider.IsTileBottom(TileType.Ladder) && collider.CenterTile != TileType.Ladder;
		bool isOverPlatform = collider.IsTileBottom(TileType.Platform);
		IsStandingOnTile = collider.BottomY % GameScene.TileSize == 0;
		IsStandingOnPlatform = (isOverLadder || isOverPlatform) && IsStandingOnTile;
		IsStandingOnWall = collider.IsTileBottom(TileType.Wall);
		IsGrounded = IsStandingOnWall || IsStandingOnPlatform;
	}

	private void CheckForLadder(TiledCollider<TileType> collider)
	{
		// Check collision
		IsOverLadder = collider.CenterTile == TileType.Ladder || (collider.IsTileBottom(TileType.Ladder) && !IsStandingOnTile);

		// Check interaction
		IsOnLadder = IsOnLadder && IsOverLadder;
		IsOnLadder = IsOnLadder || IsOverLadder && Keyboard.IsKeyPressed(KeyboardKey.Up);
		CanGrabLadder = IsOverLadder && !IsOnLadder;

		// Reset states
		IsClimbingUpLadder = false;
		IsClimbingDownLadder = false;
		IsClimbingLadder = false;

		// Climb up and down the ladder
		if (IsOverLadder && IsOnLadder)
		{
			if (Keyboard.IsKeyDown(KeyboardKey.Up) && !collider.IsTileTop(TileType.Wall)) IsClimbingUpLadder = true;
			else if (Keyboard.IsKeyDown(KeyboardKey.Down) && !IsStandingOnWall) IsClimbingDownLadder = true;
			IsClimbingLadder = IsClimbingUpLadder || IsClimbingDownLadder;
		}
	}

	private void CheckClimbing(TiledCollider<TileType> collider)
	{
		// Dismount
		DidDismount = Keyboard.IsKeyPressed(KeyboardKey.Space) && IsGrabbingWall;
		if (DidDismount)
		{
			IsGrabbingWall = false;
			return;
		}

		// Check for walls to grab
		bool isWallLeft = collider.IsTileLeft(TileType.Wall);
		bool isWallRight = collider.IsTileRight(TileType.Wall);
		CanGrabWall = !IsGrounded && (isWallLeft || isWallRight);

		// Grab wall
		bool grabbedWall = CanGrabWall && Keyboard.IsKeyPressed(KeyboardKey.Space);
		IsGrabbingWall = IsGrabbingWall || grabbedWall;
		IsGrabbingLeftWall = grabbedWall && isWallLeft;
		IsGrabbingRightWall = grabbedWall && isWallRight;

		// Climb
		IsClimbingUpWall = IsGrabbingWall && Keyboard.IsKeyDown(KeyboardKey.Up);
		IsClimbingDownWall = IsGrabbingWall && Keyboard.IsKeyDown(KeyboardKey.Down) && !IsStandingOnWall;
		IsClimbingWall = IsClimbingUpWall || IsClimbingDownWall;
	}

	private void CheckForJump(TiledCollider<TileType> collider)
	{
		DidJump = IsGrounded && Keyboard.IsKeyPressed(KeyboardKey.Space);
		if (DidJump) IsGrounded = false;
	}

	private void CheckBounds(TiledCollider<TileType> collider)
	{
		OutOfBounds = !collider.CenterInBounds;
	}
}
