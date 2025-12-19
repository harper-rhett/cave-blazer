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
	private const int fontSize = 14;
	private const int fontSpacing = 0;
	private Vector2 textSize;
	private Vector2 textPosition;
	private const int leftBorder = 1;
	private const int rightBorder = 0;
	private const int topBorder = -4;
	private const int bottomBorder = -1;
	private Vector2 boxSize;
	private Vector2 boxPosition;
	private static Font font;

	static DialogueEntity()
	{
		font = Font.Load("fonts/Tiny5-Regular.fnt");
		font.Texture.SetFilter(TextureFilter.Point);
	}

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
		textSize = Text.MeasureSize(font, text, fontSize, fontSpacing);
		int halfTextWidth = (textSize.X / 2f).Floored();
		textPosition = position + new Vector2(-halfTextWidth + halfWidth, -textSize.Y);
		boxSize = textSize + new Vector2(leftBorder + rightBorder, topBorder + bottomBorder);
		boxPosition = textPosition + new Vector2(-leftBorder, -topBorder);
	}

	public override void OnDraw()
	{
		texture.Draw(position, Colors.White);

		if (gameScene.Player.IntersectsWithRectangle(rectangle))
		{
			Primitives.DrawRectangle(boxPosition, boxSize, Colors.Black.SetAlpha(0.15f));
			Text.Draw(font, text, textPosition, Vector2.Zero, 0, fontSize, fontSpacing, Colors.White);
		}
	}
}
