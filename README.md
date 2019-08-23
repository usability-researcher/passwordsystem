[unity-download]:                 https://unity3d.com/get-unity/download/archive
[unity-version-badge]:            https://img.shields.io/badge/Unity%20Editor%20Version-2017.1.1f1-green.svg
[![Github Release][unity-version-badge]][unity-download]

# Unity -> JS -> Unity communication

# Prepocessors:
* DEBUG_GAMEMANAGER -> Enables overridable charset and logs output
* DEBUG_FINISH_URI -> Launches TourDone JS method @ index.html

## Example Usage
1. in index.hml reference file via src brackets. fe. ``<script src="UnityEventCatcher.js"></script>``
2. move content within ``<script type='text/javascript'>``.

### How it should work ?
1. Unity (WebGL) calls js function 'TourGameManagerReady' Application.ExternalExal from GameManager. Make sure its defined.
2. TourGameManagerReady should handle string building and send it to Unity (WebGL) using SendMessage("GameManager","OverrideSceneObjectString", sceneObjectString);
3. In Unity (WebGL) GameManager.cs method OverrideSceneObjectString is called and sceneObjectString is overriden.
