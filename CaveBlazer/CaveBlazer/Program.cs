using HarpEngine;

EngineSettings settings = new()
{
	WindowName = "Cave Blazer",
	GameWidth = 192,
	GameHeight = 192
};
Engine.Initialize(settings);
CaveBlazer caveBlazer = new();
Engine.Start(caveBlazer);