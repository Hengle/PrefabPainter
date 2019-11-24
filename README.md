# YAPP - Yet Another Prefab Painter

## Introduction

There are already many prefab painters available for Unity, this is yet another one. In case you have use for this one, feel free. 

It's Open Source, so also feel free to fork, modify and add more features to it.

Here's a quick demo use case video to get you started:

[![Usage](http://img.youtube.com/vi/-FZct3dVOW4/0.jpg)](https://www.youtube.com/watch?v=-FZct3dVOW4)

## Requirements

Unity 2018.3+

## Quick Setup

Installation

* create a new Unity project

* add the Yapp folder of this repository to your project

* in Hierarchy view open the context menu and press Yapp -> Create PrefabPainter
 
  ![context menu](https://user-images.githubusercontent.com/10963432/69490355-ed6d6f00-0e86-11ea-91a7-1762cee5b5c9.png)

## First Steps

The created prefab painter looks like this initially in the inspector:

![initial setup](https://user-images.githubusercontent.com/10963432/69490407-a2079080-0e87-11ea-838a-7be118d61267.png)

There are 2 things that are now required:

* a container to put your prefabs in. You can use any gameobject for that. For convenience when you hit the New button, a container is generated automatically as child object of your prefab painter gameobject. But you can use whatever you prefer, it doesn't necessarily have to be a child. Just drag your container gameobject into the container slot.

* prefabs which should be instantiated in the scene by this prefab painter and put into the specified container. The container can be cleared, i. e. the container children deleted, by using the Clear button.

Now you can already start painting. Please read the scene view instructions about how to use the current brush, e. g. shift + mouse drag is painting with the brush.

## The Idea

When I started Unity I thought this might be a good idea to learn coding. Creating a prefab painter is something rather trivial. All you need to do is analyze an area around the mouse cursor and instantiate a prefab there.

However as usual one thing leads to the other and it turned out that there's always something that one wants more. The existing prefab painters - free and commercial - didn't meet my requirements. So I created my own. 

A big shoutout to the awesome people of the Unity Community who provided tutorials which I used for the more advanced features. Please see the credits section below.

## Features

#### Container based

Operates on gameobjects which are considered containers (or folders) for grouping and contain the actual prefab instances

![container](https://user-images.githubusercontent.com/10963432/69490829-67a0f200-0e8d-11ea-9c08-f8129a99ab7e.png)

#### Paint modes

  * Brush 
  * Spline

![modes](https://user-images.githubusercontent.com/10963432/69490836-78e9fe80-0e8d-11ea-86f0-aeb41ca64206.png) 

#### Brush settings

  * Size
  * Rotation
  * Align to Terrain
  * Allow Overlap
  * Slope Dependency
  * Distribution Mode
    * Center
    * Poisson
    * Fall off (work in progress)
    * Fall off 2d (work in progress)

![brush](https://user-images.githubusercontent.com/10963432/69490834-78516800-0e8d-11ea-9b2f-3632b93e74ef.png)

####  Spline settings

  * Curve Resolution: sharpness, from sharp corners to round curves
  * Loop: connect start with end point
  * Separation
    * Fixed distance
    * Prefab Bounds determine distance
  * Multiple Lanes along a single Spline
    * Lane Distance
  * Rotation
    * Use the Prefab rotation
    * Rotate along the Spline (e. g. like a fence)
  * Attach Mode
    * Bounds: attach new Spline points at the start or end
    * Between: attach new Spline points anywhere on the curve
  * Snap: snap spline points at terrain height

![spline](https://user-images.githubusercontent.com/10963432/69490838-78e9fe80-0e8d-11ea-8fff-2d2d58b2cc72.png)

####  Prefab Instantiation
  * Active: enable/disable the use of this prefab
  * Probability: the probability that this prefab will be instantiated
  * Change Scale: vary the scale
  * Position Offset: offset this prefab by the specified x/y/z coordinates
  * Rotation Offset: rotate this prefab by the specified x/y/z angles
  * Random Rotation: apply random rotation when this prefab is instantiated

![prefabs](https://user-images.githubusercontent.com/10963432/69490835-78516800-0e8d-11ea-89d0-aeb3bd16a114.png)

####  Batch operations

Batch operations are performed on the children of the selected container.

  * apply physics (e. g. gravity, forces, etc) 
  * copy/paste transforms

![operations](https://user-images.githubusercontent.com/10963432/69490837-78e9fe80-0e8d-11ea-8248-0a46063ab913.png)


## Beta

Please note that this is still beta and work in progress, so features may change. Also the UI might change. Currently I used standard Unity UI setup, it was rather trivial and straightforward to do so.

## Integrations

 - Vegetation Studio 
 - Vegetation Studio Pro 

This option allows you to paint into the persistent storage of Vegetation Studio and Vegetation Studio Pro.

## Future updates

+ consider prefab dimensions during distribution, align them next to each other
+ random distance during distribution
+ limit random scale in specific directions
+ limit rotation
+ undo operations
+ prefab thumbnail overview with filter by selecting a prefab thumbnail (or multiple ones)
+ ...

## Credits

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

- Unity

  Getting the SerializedProperty from an object instead of a string. See example:
  https://github.com/Unity-Technologies/PostProcessing/blob/v2/PostProcessing/Editor/PostProcessVolumeEditor.cs

- Gregory Schlomoff

  Poisson Disc Sampling in Unity
  http://gregschlom.com/devlog/2014/06/29/Poisson-disc-sampling-Unity.html