using HarpEngine;

EngineSettings settings = new()
{
	WindowName = "Cave Blazer",
	GameWidth = 192,
	GameHeight = 128
};
Engine.Initialize(settings);
CaveBlazer caveBlazer = new();
Engine.Start(caveBlazer);