Introduction
-------------------------------------------------
Prefab Painter allows you to paint prefabs in the scene

Supported Unity Versions: 2018+

Usage
-------------------------------------------------

General

	* create container gameobject for the instantiated prefabs
	* create an editor gameobject, add the PrefabPainter script to it
	* in the inspector assign the container and a prefab to the prefab painter editor settings

Paint Mode

	* in scene view adjust the radius via ctrl + mouse wheel
	* start painting prefabs by clicking left mouse button and dragging the mouse

Spline Mode

	* add control points: shift + click
	* delete control points: shift + ctrl + click
	* press A to change the add (and delete) mode
		+ bounds: add (and delete) control points only to start and end control points
		+ inbetween: add (and delete) control points between control points

Gravity

	* paint prefabs, as describe in General
	* hit "Run Simulation" in the Physics Settings
	
Delete

	* Click "Remove Container Children" to quickly remove the children of the current container

Features in future updates
-------------------------------------------------	
+ multiple prefabs
+ rotation offset
+ lanes: directly on spline, left & right, multiple left & right
+ limit random scale in specific directions
+ limit rotation
+ rotation along the spline
+ consider prefab dimensions during distribution
+ random distance during distribution
+ store position/rotation in a list instead of gameobjects as control points
+ wizard to create prefabpainter gameobject and the group container

Credits
-------------------------------------------------

- Sebastian Lague
  
  Physics Simulation
  https://www.youtube.com/watch?v=SnhfcdtGM2E

  Patreon
  https://www.patreon.com/SebastianLague

- Kyle Halladay

  Spline
  http://kylehalladay.com/blog/tutorial/2014/03/30/Placing-Objects-On-A-Spline.html

- Fernando Zapata

  Interpolate.cs
  http://wiki.unity3d.com/index.php?title=Interpolate#Interpolate.cs
