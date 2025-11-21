using HarpEngine.Input;
using HarpEngine.Tiles;

public class PlayerState
{
	private Player player;

	public bool IsGrounded { get; private set; }
	public bool IsOnPlatform { get; private set; }
	public bool IsClimbingLadder { get; private set; }
	public bool DidJump { get; private set; }
	public bool OutOfBounds { get; private set; }

	public PlayerState(Player player)
	{
		this.player = player;
	}

	public void Update(TiledCollider<TileType> collider)
	{
		CheckForPlatform(collider);
		CheckGrounded(collider);
		CheckForLadder(collider);
		CheckForJump(collider);
		CheckBounds(collider);
	}

	private void CheckForPlatform(TiledCollider<TileType> collider)
	{
		IsOnPlatform = collider.IsTileBottom(TileType.Platform) && collider.BottomY % 16 == 0;
	}

	private void CheckGrounded(TiledCollider<TileType> collider)
	{
		IsGrounded = collider.IsTileBottom(TileType.Wall) || IsOnPlatform;
	}

	private void CheckForLadder(TiledCollider<TileType> collider)
	{
		bool isOnLadder = collider.CenterTile == TileType.Ladder;
		IsClimbingLadder = isOnLadder && Keyboard.IsKeyDown(KeyboardKey.Up);
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
