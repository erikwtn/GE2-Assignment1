# EDEN VR Birds

Name: Erik Watchorn

Student Number: C21449374

Class Group: TU984/3

# Description
A VR simulation of birds in a clearing in a forest, where you play as a drone observer. The birds have a few personality traits that change depending on their circumstances (socialbility, energy, fear, are they goofy etc.).
The personality traits can be altered by the player.

## Video:

[![YouTube]([http://img.youtube.com/vi/J2kHSSFA4NU/0.jpg](https://i9.ytimg.com/vi_webp/U1-_z3jo3jQ/mq1.webp?sqp=CPTPubEG-oaymwEmCMACELQB8quKqQMa8AEB-AH-CYAC0AWKAgwIABABGGUgXyg1MA8=&rs=AOn4CLB_QEwugGgDFMDizWStGmGeCQfN6A))]([https://www.youtube.com/watch?v=J2kHSSFA4NU](https://youtu.be/U1-_z3jo3jQ))

# Instructions
Made for Meta Quest 2. Movement and rotation are controlled via the thumbsticks. Press X or A to open parameters GUI. Press fire button to go up, grip button to go down.

# How it works
The birds are controlled by a custom AI script that manages their movement and animations which is based on a number of variables (personality traits mentioned previously). 

# List of classes/assets

| Class/asset | Source |
|-----------|-----------|
| AnimalAI.cs | Self written |
| ShuttleController.cs | Self written |
| Skybox | From [AllSky Free](https://assetstore.unity.com/packages/2d/textures-materials/sky/allsky-free-10-sky-skybox-set-146014) |
| Drone Model | From [Simple Drone](https://assetstore.unity.com/packages/3d/vehicles/air/simple-drone-190684) |
| Grass Texture | From [Geo Grass](https://assetstore.unity.com/packages/tools/terrain/geo-grass-auto-terrain-material-easy-fast-grass-urp-geometry-sha-202496) |
| Other Textures | From [Hand Painted Seamless Grass Texture](https://assetstore.unity.com/packages/p/hand-painted-seamless-grass-texture-vol-3-159522) |
| Rock Models | From [Stylized Rocks Lite](https://assetstore.unity.com/packages/3d/environments/landscapes/stylized-low-poly-rocks-271334) |
| Trees | From [Nature Pack](https://assetstore.unity.com/packages/p/nature-pack-low-poly-trees-bushes-210184) |
| Bird Models & Animations | From [Quirky Series](https://assetstore.unity.com/packages/p/quirky-series-free-animals-pack-178235) |
| Water Texture | From [Stylized Water](https://assetstore.unity.com/packages/p/stylize-water-texture-153577) |

This was a solo project so all contributions are from Erik Watchorn with the help of assets from the asset store. I am most proud of the AnimalAI script as it was a first for me to do an AI without the help of the built in NavMeshAgent in Unity.
I learned a lot about state machines and movement as I did not use rigidbodies or charactercontrollers.
