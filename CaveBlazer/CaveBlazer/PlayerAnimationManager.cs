using HarpEngine.Animation;
using HarpEngine.Graphics;

public enum PlayerAnimation
{
	Idle,
	Walking,
	Jumping,
	Falling,
	ClimbingLadder
}

public class PlayerAnimationManager : TextureAnimationManager<PlayerAnimation>
{
	public PlayerAnimationManager()
	{
		Texture idleTexture = Texture.Load("sprites/player/idle.png");
		TextureAnimation idleAnimation = new(idleTexture, 4, 16, 16, 0.3f);
		RegisterAnimation(idleAnimation, PlayerAnimation.Idle);

		Texture walkTexture = Texture.Load("sprites/player/walking.png");
		TextureAnimation walkingAnimation = new(walkTexture, 4, 16, 16, 0.2f);
		RegisterAnimation(walkingAnimation, PlayerAnimation.Walking);

		Texture jumpTexture = Texture.Load("sprites/player/jumping.png");
		TextureAnimation jumpingAnimation = new(jumpTexture, 4, 16, 16, 0.1f);
		jumpingAnimation.PlayOnce = true;
		RegisterAnimation(jumpingAnimation, PlayerAnimation.Jumping);

		Texture fallingTexture = Texture.Load("sprites/player/falling.png");
		TextureAnimation fallingAnimation = new(fallingTexture, 4, 16, 16, 0.1f);
		fallingAnimation.PlayOnce = true;
		RegisterAnimation(fallingAnimation, PlayerAnimation.Falling);

		Texture climbingLadderTexture = Texture.Load("sprites/player/climbing-ladder.png");
		TextureAnimation climbingLadderAnimation = new(climbingLadderTexture, 4, 16, 16, 0.15f);
		RegisterAnimation(climbingLadderAnimation, PlayerAnimation.ClimbingLadder);
	}
}
