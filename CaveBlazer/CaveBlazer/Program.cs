using HarpEngine;

EngineSettings settings = new()
{
	WindowName = "Cave Blazer",
	GameWidth = 112,
	GameHeight = 112
};
Engine.Initialize(settings);
CaveBlazer caveBlazer = new();
Engine.Start(caveBlazer);