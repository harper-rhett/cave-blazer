using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.Tiles;
using System.Numerics;
using HarpEngine.Utilities;

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
		maxStamina = 16;
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

	public void Draw()
	{
		if (Stamina < MaxStamina) DrawStamina();
	}

	private void DrawStamina()
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
		Primitives.DrawRectangleLines(containerRectangle, 1, Colors.White);

		// Draw stamina
		int staminaRectangleHeight = (staminaHeight - StaminaRatio * staminaHeight).Floored();
		Vector2 staminaPosition = containerPosition + Vector2.One + new Vector2(0, staminaRectangleHeight);
		Rectangle staminaRectangle = new(staminaPosition, staminaWidth, staminaHeight - staminaRectangleHeight);
		Primitives.DrawRectangle(staminaRectangle, Colors.White);
	}
}
