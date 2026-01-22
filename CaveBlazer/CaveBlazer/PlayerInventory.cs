using Clockwork.Graphics.Draw2D;
using Clockwork.Graphics;
using Clockwork.Tiles;
using System.Numerics;
using Clockwork.Utilities;
using Clockwork.Raylib.Graphics.Draw2D;
using Clockwork.Raylib.Graphics;

public class PlayerInventory
{
	private Player player;

	public bool CanClimb { get; private set; }
	private float stamina;
	public float Stamina => stamina;
	private float maxStamina = 0;
	public float MaxStamina => maxStamina;
	public float StaminaRatio => stamina / maxStamina;

	public PlayerInventory(Player player)
	{
		this.player = player;
	}

	public void UnlockCrampons()
	{
		CanClimb = true;
		maxStamina = 18;
		stamina = maxStamina;
	}

	public void ResetStamina()
	{
		stamina = maxStamina;
	}

	public void UseStamina(float amount)
	{
		stamina -= amount;
	}

	public void DrawStamina(Color innerColor, Color outerColor)
	{
		// Dimensions
		const int containerWidth = 4;
		const int halfContainerWidth = containerWidth / 2;
		const int containerHeight = 8;
		const int containerOffset = -11;
		const int staminaWidth = containerWidth - 2;
		const int staminaHeight = containerHeight - 2;

		// Draw container
		Vector2 containerPosition = player.ColliderPosition + new Vector2(player.Collider.HalfWidth - halfContainerWidth, containerOffset);
		Rectangle containerRectangle = new(containerPosition, containerWidth, containerHeight);
		RaylibPrimitives2D.DrawRectangleLines((RaylibRectangle)containerRectangle, 1, (RaylibColor)outerColor);

		// Draw stamina
		int staminaRectangleHeight = (staminaHeight - StaminaRatio * staminaHeight).Floored();
		Vector2 staminaPosition = containerPosition + Vector2.One + new Vector2(0, staminaRectangleHeight);
		Rectangle staminaRectangle = new(staminaPosition, staminaWidth, staminaHeight - staminaRectangleHeight);
		Primitives2D.DrawRectangle(staminaRectangle, innerColor);
	}
}
