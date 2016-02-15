var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Gen-SQ-Libs")
    .Does(() =>
{
    Information("Hello World!");
});

Task("Default")
    .IsDependentOn("Gen-SQ-Libs");
	
RunTarget(target);