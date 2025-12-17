using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Tiles;
using HarpEngineLDTKImporter;
using System.Numerics;

public class DialogueEntity : Entity
{
	private Vector2 position;
	private Rectangle rectangle;
	private Texture texture;
	private GameScene gameScene;
	private string text;

	public DialogueEntity(LDTKEntity ldtkEntity, GameScene gameScene)
	{
		position = ldtkEntity.Position;
		this.gameScene = gameScene;
		rectangle = new(position, GameScene.TileSize, GameScene.TileSize);

		string type = ldtkEntity.FieldsByID["dialogue_type"].Value;
		string texturePath = null;
		if (type == "monk") texturePath = "sprites/entities/monk.png";
		else if (type == "sign") texturePath = "sprites/entities/sign.png";
		texture = Texture.Load(texturePath);

		text = ldtkEntity.FieldsByID["text"].Value;
	}

	public override void OnDraw()
	{
		texture.Draw(position, Colors.White);

		if (gameScene.Player.IntersectsWithRectangle(rectangle)) Console.WriteLine(text);
	}
}
