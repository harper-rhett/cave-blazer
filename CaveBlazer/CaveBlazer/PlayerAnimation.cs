using HarpEngine.Animation;
using HarpEngine.Graphics;

public enum PlayerAnimationState
{
	Idle,
	Walking,
	Jumping,
	Falling,
	ClimbingLadder
}

public class PlayerAnimation : TextureAnimationManager<PlayerAnimationState>
{
	public readonly TextureAnimation idleAnimation;
	public readonly TextureAnimation walkingAnimation;
	public readonly TextureAnimation jumpingAnimation;
	public readonly TextureAnimation fallingAnimation;
	public readonly TextureAnimation climbingLadderAnimation;

	public PlayerAnimation()
	{
		Texture idleTexture = Texture.Load("sprites/player/idle.png");
		idleAnimation = new(idleTexture, 4, 16, 16, 0.3f);
		RegisterAnimation(idleAnimation, PlayerAnimationState.Idle);

		Texture walkTexture = Texture.Load("sprites/player/walking.png");
		walkingAnimation = new(walkTexture, 4, 16, 16, 0.15f);
		RegisterAnimation(walkingAnimation, PlayerAnimationState.Walking);

		Texture jumpTexture = Texture.Load("sprites/player/jumping.png");
		jumpingAnimation = new(jumpTexture, 4, 16, 16, 0.1f);
		jumpingAnimation.PlayOnce = true;
		RegisterAnimation(jumpingAnimation, PlayerAnimationState.Jumping);

		Texture fallingTexture = Texture.Load("sprites/player/falling.png");
		fallingAnimation = new(fallingTexture, 2, 16, 16, 0.4f);
		fallingAnimation.PlayOnce = true;
		RegisterAnimation(fallingAnimation, PlayerAnimationState.Falling);

		Texture climbingLadderTexture = Texture.Load("sprites/player/climbing-ladder.png");
		climbingLadderAnimation = new(climbingLadderTexture, 4, 16, 16, 0.2f);
		RegisterAnimation(climbingLadderAnimation, PlayerAnimationState.ClimbingLadder);
	}
}
