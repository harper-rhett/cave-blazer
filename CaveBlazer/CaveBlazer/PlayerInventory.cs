using HarpEngine;

public class PlayerInventory
{
	public bool CanClimb { get; private set; }
	private float stamina;
	public float Stamina => stamina;
	private float maxStamina = 0;
	public float MaxStamina => maxStamina;
	public float StaminaRatio => stamina / maxStamina;

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
}
