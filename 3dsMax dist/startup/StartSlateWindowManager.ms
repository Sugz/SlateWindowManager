(
	directory = (GetDir #userScripts) + "\\SlateWindowManager"
	dotnet.loadAssembly @"$userscripts\SlateWindowManager\SlateWindowManager.dll"
	slatePositionManager = dotNetObject "SlateWindowManager.PositionManager"
	slatePositionManager.Init directory
	slatePositionManager.Start (windows.getMAXHWND())
)
