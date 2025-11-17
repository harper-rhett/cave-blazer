using System.Numerics;
using HarpEngine.Tiles;
using HarpEngine.Utilities;

internal class EntityTileCollider
{
	public bool LeftInBounds { get; private set; }
	public bool RightInBounds { get; private set; }
	public bool TopInBounds { get; private set; }
	public bool BottomInBounds { get; private set; }

	public bool LeftWall { get; private set; }
	public bool RightWall { get; private set; }
	public bool TopWall { get; private set; }
	public bool BottomWall { get; private set; }

	private int width;
	private int height;

	// Need to be able to check tile type of center left, center right, center top, center bottom, center

	public EntityTileCollider(int width, int height)
	{
		this.width = width;
		this.height = height;
	}

	public void Update(TiledArea area, Vector2 position)
	{
		CheckLeftRight(area, position);
		CheckUpperLower(area, position);
	}

	private void CheckUpperLower(TiledArea area, Vector2 position)
	{
		int leftX = (position.X).Floored();
		int rightX = (position.X + width - 1).Floored();
		int topY = (position.Y - 1).Floored();
		int bottomY = (position.Y + height).Floored();

		TopInBounds = area.InBounds(leftX, topY) && area.InBounds(rightX, topY);
		BottomInBounds = area.InBounds(leftX, bottomY) && area.InBounds(rightX, bottomY);
		TopWall = TopInBounds && (
			area.GetTileType<TileType>(leftX, topY) == TileType.Wall 
			|| area.GetTileType<TileType>(rightX, topY) == TileType.Wall
		);
		BottomWall = BottomInBounds && (
			area.GetTileType<TileType>(leftX, bottomY) == TileType.Wall
			|| area.GetTileType<TileType>(rightX, bottomY) == TileType.Wall
		);
	}

	private void CheckLeftRight(TiledArea area, Vector2 position)
	{
		int leftX = (position.X - 1).Floored();
		int rightX = (position.X + width).Floored();
		int topY = (position.Y).Floored();
		int bottomY = (position.Y + height - 1).Floored();

		LeftInBounds = area.InBounds(leftX, topY) && area.InBounds(leftX, bottomY);
		RightInBounds = area.InBounds(rightX, topY) && area.InBounds(rightX, bottomY);
		LeftWall = LeftInBounds && (
			area.GetTileType<TileType>(leftX, topY) == TileType.Wall
			|| area.GetTileType<TileType>(leftX, bottomY) == TileType.Wall
		);
		RightWall = RightInBounds && (
			area.GetTileType<TileType>(rightX, topY) == TileType.Wall
			|| area.GetTileType<TileType>(rightX, bottomY) == TileType.Wall
		);
	}
}
