using HarpEngine;

public class PlayerInventory
{
	public bool CanClimb { get; private set; }
	private float stamina;
	public float Stamina => stamina;
	private float maxStamina = 0;

	public void UnlockClimbing()
	{
		CanClimb = true;
		maxStamina = 16;
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
