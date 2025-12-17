using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Tiles;
using HarpEngine.Utilities;
using HarpEngineLDTKImporter;
using System.Numerics;

public class DialogueEntity : Entity
{
	private Vector2 position;
	private Rectangle rectangle;
	private Texture texture;
	private GameScene gameScene;
	private string text;
	private const int fontSize = 10;
	private const int fontSpacing = 1;
	private Vector2 textSize;
	private Vector2 textPosition;
	private Vector2 borderSize = new(2, 2);
	private Vector2 boxSize;

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
		int halfWidth = texture.Width / 2;

		text = ldtkEntity.FieldsByID["text"].Value;
		textSize = Text.MeasureSize(Font.Default, text, fontSize, fontSpacing);
		int halfTextWidth = (textSize.X / 2f).Floored();
		textPosition = position + new Vector2(-halfTextWidth + halfWidth, -fontSize * 2);
		boxSize = textSize + borderSize;
	}

	public override void OnDraw()
	{
		texture.Draw(position, Colors.White);

		if (gameScene.Player.IntersectsWithRectangle(rectangle))
		{
			Vector2 boxOffset = -borderSize / 2f;
			Primitives.DrawRectangle(textPosition + boxOffset, boxSize, Colors.White.SetAlpha(0.5f));
			Text.Draw(Font.Default, text, textPosition, Vector2.Zero, 0, fontSize, fontSpacing, Colors.Black);
		}
	}
}
