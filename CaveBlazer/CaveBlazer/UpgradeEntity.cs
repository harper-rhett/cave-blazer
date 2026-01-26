using Clockwork;
using Clockwork.Graphics;
using Clockwork.Input;
using Clockwork.LDTKImporter;
using Clockwork.Particles;
using Clockwork.Utilities;
using System.Numerics;

public class UpgradeEntity : Entity
{
	private Vector2 position;
	private GameScene gameScene;
	private Rectangle rectangle;
	private ITexture texture;
	private int halfWidth;
	private int halfHeight;
	private Action upgradeAction;
	private ParticleEngine2D glowParticles;
	private bool collidingWithPlayer;

	public UpgradeEntity(LDTKEntity ldtkEntity, GameScene gameScene)
	{
		// Set dimensions
		position = ldtkEntity.Position;
		this.gameScene = gameScene;
		rectangle = new(position, GameScene.TileSize, GameScene.TileSize);

		// Set independent behavior
		string type = ldtkEntity.FieldsByName["upgrade_type"].Value;
		string texturePath = null;
		if (type == "crampons")
		{
			texturePath = "sprites/entities/lotus-stupa.png";
			upgradeAction = () => gameScene.Player.Inventory.UnlockCrampons();
		}
		texture = Texture.Load(texturePath);
		halfWidth = texture.Width / 2;
		halfHeight = texture.Height / 2;

		// Set up particle glow
		glowParticles = new(streamCooldownTime: 0.5f);
		glowParticles.IsStreaming = true;
		glowParticles.StreamFired += OnGlowParticleFired;
		glowParticles.RenderAsPixel();
		glowParticles.AddInitializer(ParticleInitializers.Scatter(halfWidth / 2f));
		glowParticles.AddInitializer(ParticleInitializers.RandomizeLifespan(1f, 1.5f));
		glowParticles.AddModifier(ParticleModifiers.ApplyMovement());
		gameScene.AddEntity(glowParticles);
	}

	private void OnGlowParticleFired(out Particle2D particleTemplate)
	{
		particleTemplate = new()
		{
			Position = position + new Vector2(halfWidth, halfHeight),
			Velocity = new Vector2(0, -15),
			Gradient = new Gradient(Colors.Blue, Colors.Cyan),
		};
		glowParticles.StreamCooldownTime = Generate.Float(0.25f, 0.35f);
	}

	public override void OnUpdate()
	{
		collidingWithPlayer = gameScene.Player.IntersectsWithRectangle(rectangle);
		if (Keyboard.IsKeyPressed(KeyboardKey.E) && collidingWithPlayer)
		{
			upgradeAction.Invoke();
			glowParticles.Remove();
			Explode();
			Remove();
		}
	}

	private void Explode()
	{
		ParticleEngine2D explosion = new();
		explosion.RemoveOnExhausted = true;
		explosion.RenderAsPixel();
		explosion.AddInitializer(ParticleInitializers.RandomizeDirection());
		explosion.AddInitializer(ParticleInitializers.ScatterByDirection(halfWidth / 2f));
		explosion.AddInitializer(ParticleInitializers.RandomizeSpeed(15, 25));
		explosion.AddInitializer(ParticleInitializers.RandomizeLifespan(0.5f, 2.0f));
		explosion.AddInitializer(ParticleInitializers.AddVelocity(new Vector2(0, -25)));
		explosion.AddModifier(ParticleModifiers.ApplyMovement());
		explosion.AddModifier(ParticleModifiers.AddVelocity(new Vector2(0, 45)));
		gameScene.AddEntity(explosion);

		Particle2D template = new()
		{
			Position = position + new Vector2(halfWidth, halfHeight),
			Gradient = new Gradient(Colors.Orange, Colors.Yellow),
		};
		explosion.SpawnBurst(template, 50);
	}

	public override void OnDraw()
	{
		texture.Draw(position, Colors.White);
		if (collidingWithPlayer) gameScene.Player.State.DrawStatusOrb(Colors.White, Colors.Black);
	}
}
