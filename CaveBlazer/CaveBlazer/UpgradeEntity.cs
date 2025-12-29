using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.LDTKImporter;
using System.Numerics;

public class UpgradeEntity : Entity
{
	private Vector2 position;
	private GameScene gameScene;
	private Rectangle rectangle;
	private Texture texture;
	private Action upgradeAction;

	public UpgradeEntity(LDTKEntity ldtkEntity, GameScene gameScene)
	{
		// Set dimensions
		position = ldtkEntity.Position;
		this.gameScene = gameScene;
		rectangle = new(position, GameScene.TileSize, GameScene.TileSize);

		// Set independent behavior
		string type = ldtkEntity.FieldsByID["upgrade_type"].Value;
		string texturePath = null;
		if (type == "crampons")
		{
			texturePath = "sprites/entities/crampons.png";
			upgradeAction = () => gameScene.Player.Inventory.UnlockCrampons();
		}
		texture = Texture.Load(texturePath);
		int halfWidth = texture.Width / 2;
	}

	public override void OnUpdate()
	{
		bool collidesWithPlayer = gameScene.Player.IntersectsWithRectangle(rectangle);
		if (collidesWithPlayer)
		{
			upgradeAction.Invoke();
			Remove();
		}
	}

	public override void OnDraw()
	{
		texture.Draw(position, Colors.White);
	}
}
