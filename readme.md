# Genesis

Experimental Unity package that auto-generates depth textures for skyboxes created with [Skybox Lab](https://skybox.blockadelabs.com/).

https://user-images.githubusercontent.com/26555424/221649787-1bf45ce8-4c71-4647-a9be-c02128687d2a.mp4

## Disclaimer
As of right now, this project's main purpose is experimentation. Feel free to give it a go with your own imaginations (but expect things to break).

The image-based rendering techniques used here do not do any sophisticated inpainting techniques. The depth estimation still has a lot of limitations and artifacts (outlined below). How well it performs also depends highly on the content (e.g. indoor vs outdoor scenes). Especially when generating highly non-realistic images, depth estimation can fail badly.

The motivation here in some way is to see how far you can push current 2D-based Image generation models for building 3D worlds and prototype workflows for the inevitable future of AI-assisted game development.

You're welcome to provide feedback here on GitHub, [Twitter](https://twitter.com/julien_kaye), or join the discussion on the [Blockade Labs Discord](https://discord.gg/kqKB3X4TJz)

## Requirements
This package requires Unity 2020 or newer and uses the Built-in Render Pipeline.

## Installation
Go to the [release section](https://github.com/julienkay/genesis/releases), download the Unity Package, and import it into any Unity project.
The package is a 'Hybrid Package', so it will install into your Packages folder as a local package.

## Usage
- Generate a skybox using [SkyboxLabs](https://skybox.blockadelabs.com/). Use the 'Share' Button to get a link with the ID.
- Back in Unity, use the menu *Genesis -> Import from Skybox Lab via ID*, enter the ID and hit *Import*

![menu_crop](https://user-images.githubusercontent.com/26555424/222116668-8b705af4-6674-4225-9664-f6313b1a72c2.png)

On the generated prefab, there is a material that has two properties: *Depth Multiplier* and *Max Depth Cutoff*
![skybox_material](https://user-images.githubusercontent.com/26555424/222119231-f8d8b2c6-5bf7-40a9-b8bc-378e677853ce.jpg)

- The depth estimation does not give information about absolute depth. So you can use the *Depth Multiplier* to get the scale right, but you can also just scale the Unity object normally. Good values for a realistic scale of the scene depend on the contents of the image.
- *Max Depth Cutoff* you'll probably rarely have to touch, but it clamps any larger depth values to that number. It is sometimes needed to remove artifacts when very high depth values appear in the estimation. Other than that you can use it to simply bring far away surfaces in closer.

## Ideas
Not a roadmap, just some general thoughts and ideas I have for the future.

- A next step could be to segment panoramas into layers using the generated depth. I'd like to experiment with sending a masked panorama back to SkyBox Labs as an input image for a new generation with the same prompt and automatically build a layered depth panorama that way. The idea here is that these are not real images, so we can let the AI hallucinate details where usually complex inpainting methods would be needed.
- Another interesting experiment could be done by rendering the scene from a different viewpoint using the naively extruded sphere and then again use that as an input image for a new generation to continually build out a larger 3D world from a given prompt.
- Add some export formats like .obj and glTF

## Known Issues / Limitations
### Depth Quality
The generated depth is not very high quality right now. The resolution for the depth maps we generate is only 256x256. Some limitations in [Barracuda](https://docs.unity3d.com/Packages/com.unity.barracuda@latest/index.html) require us to use the smaller version of an older depth estimation model ([MiDaS v2.1](https://github.com/isl-org/MiDaS)), but that is only part of the issue. MiDaS alone is not really suitable to be used with high-resolution, 360° images with lots of distortion.

There is active research going on, on how to extend the capabilities of depth estimation models through various approaches. Naively, one could run the model on patches of the whole panoramic image. But from what I've read, realignment and blending the results back together becomes tricky fast, so I would rather use some existing approach. Notably, [360monodepth](https://github.com/manurare/360monodepth) does just that and seems to be the state of the art for generating high-resolution depth for panoramic images.

The neat thing about using Unity's Baracuda is, that we can run the inference directly in the Unity Editor, or even do it at runtime. Still, if we want better depth, looking at the setup instructions of the 360monodepth repository, it might require building a web service to do the depth estimation instead.

### Depth Discontinuities
The seams and depth discontinuities could probably be fixed in a quick-and-dirty way, but I'm not sure it's worth doing, before tackling the larger issue of figuring out a better way to do the depth estimation.

## Acknowledgements
Thanks go out to these wonderful projects:
- Skybox Lab: https://skybox.blockadelabs.com/
- MiDaS v2 integration: https://github.com/GeorgeAdamon/monocular-depth-unity
- Octahedron Sphere: https://catlikecoding.com/unity/tutorials/octahedron-sphere/
