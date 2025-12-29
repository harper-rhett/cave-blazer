using HarpEngine;
using HarpEngine.Graphics;
using HarpEngine.LDTKImporter;
using HarpEngine.Particles;
using HarpEngine.Utilities;
using System.Numerics;

public class UpgradeEntity : Entity
{
	private Vector2 position;
	private GameScene gameScene;
	private Rectangle rectangle;
	private Texture texture;
	private int halfWidth;
	private int halfHeight;
	private Action upgradeAction;
	private ParticleEngine2D glowParticles;

	public UpgradeEntity(LDTKEntity ldtkEntity, GameScene gameScene)
	{
		// Set dimensions
		position = ldtkEntity.Position;
		this.gameScene = gameScene;
		rectangle = new(position, GameScene.TileSize, GameScene.TileSize);

		// Set independent behavior
		string type = ldtkEntity.FieldsByID["upgrade_type"].Value;
		string texturePath = null;
		if (type == "crampons")
		{
			texturePath = "sprites/entities/crampons.png";
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
			Gradient = new Gradient(Colors.Blue, Colors.SkyBlue),
		};
		glowParticles.StreamCooldownTime = Generate.Float(0.25f, 0.35f);
	}

	public override void OnUpdate()
	{
		bool collidesWithPlayer = gameScene.Player.IntersectsWithRectangle(rectangle);
		if (collidesWithPlayer)
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
	}
}
