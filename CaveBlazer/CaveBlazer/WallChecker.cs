using System.Numerics;
using HarpEngine.Utilities;

internal class WallChecker
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

	public WallChecker(int width, int height)
	{
		this.width = width;
		this.height = height;
	}

	public void Refresh(Area area, Vector2 position)
	{
		CheckLeftRight(area, position);
		CheckUpperLower(area, position);
	}

	private void CheckUpperLower(Area area, Vector2 position)
	{
		int leftX = (position.X).Floored();
		int rightX = (position.X + width - 1).Floored();
		int topY = (position.Y - 1).Floored();
		int bottomY = (position.Y + height).Floored();

		TopInBounds = area.InBounds(leftX, topY) && area.InBounds(rightX, topY);
		BottomInBounds = area.InBounds(leftX, bottomY) && area.InBounds(rightX, bottomY);
		TopWall = TopInBounds && (area.IsWall(leftX, topY) || area.IsWall(rightX, topY));
		BottomWall = BottomInBounds && (area.IsWall(leftX, bottomY) || area.IsWall(rightX, bottomY));
	}

	private void CheckLeftRight(Area area, Vector2 position)
	{
		int leftX = (position.X - 1).Floored();
		int rightX = (position.X + width).Floored();
		int topY = (position.Y).Floored();
		int bottomY = (position.Y + height - 1).Floored();

		LeftInBounds = area.InBounds(leftX, topY) && area.InBounds(leftX, bottomY);
		RightInBounds = area.InBounds(rightX, topY) && area.InBounds(rightX, bottomY);
		LeftWall = LeftInBounds && (area.IsWall(leftX, topY) || area.IsWall(leftX, bottomY));
		RightWall = RightInBounds && (area.IsWall(rightX, topY) || area.IsWall(rightX, bottomY));
	}
}
