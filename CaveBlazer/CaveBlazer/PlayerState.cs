using HarpEngine.Input;
using HarpEngine.Tiles;

public class PlayerState
{
	public bool IsGrounded { get; private set; }
	public bool IsOnPlatform { get; private set; }
	public bool IsOnWall { get; private set; }
	public bool IsOnLadder { get; private set; }
	public bool IsClimbingLadder { get; private set; }
	public bool IsClimbingUpLadder { get; private set; }
	public bool IsClimbingDownLadder { get; private set; }
	public bool DidJump { get; private set; }
	public bool OutOfBounds { get; private set; }

	public void Update(TiledCollider<TileType> collider)
	{
		CheckGrounded(collider);
		CheckForLadder(collider);
		CheckForJump(collider);
		CheckBounds(collider);
	}

	private void CheckGrounded(TiledCollider<TileType> collider)
	{
		bool isOverLadder = collider.IsTileBottom(TileType.Ladder) && collider.CenterTile != TileType.Ladder;
		bool isOverPlatform = collider.IsTileBottom(TileType.Platform);
		IsOnPlatform = (isOverLadder || isOverPlatform) && collider.BottomY % 16 == 0;
		IsOnWall = collider.IsTileBottom(TileType.Wall);
		IsGrounded = IsOnWall || IsOnPlatform;
	}

	private void CheckForLadder(TiledCollider<TileType> collider)
	{
		IsOnLadder = collider.CenterTile == TileType.Ladder || collider.IsTileBottom(TileType.Ladder);
		IsClimbingUpLadder = false;
		IsClimbingDownLadder = false;
		IsClimbingLadder = false;
		if (IsOnLadder)
		{
			if (Keyboard.IsKeyDown(KeyboardKey.Up)) IsClimbingUpLadder = true;
			else if (Keyboard.IsKeyDown(KeyboardKey.Down) && !IsOnWall) IsClimbingDownLadder = true;
			IsClimbingLadder = IsClimbingUpLadder || IsClimbingDownLadder;
		}
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
