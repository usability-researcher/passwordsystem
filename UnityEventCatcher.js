function TourGameManagerReady()
{
	console.log("TourGameManagerReady called!");
	var sceneObjectString = "abcdefghijkl";
	SendMessage("GameManager","OverrideSceneObjectString", sceneObjectString);
}
