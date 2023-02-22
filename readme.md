# Genesis

Experimental package that auto-generates depth textures for skyboxes created with [Skybox Lab](https://skybox.blockadelabs.com/).

## Disclaimer
As of right now, this project's main purpose is experimentation. Feel free to try it out, but expect things to break.
You're welcome to provide feedback here on GitHub or [Twitter](https://twitter.com/julien_kaye) or join the discussion on the [BlockadeLabs Discord](https://discord.gg/kqKB3X4TJz)

## Requirements
This package was created with Unity 2021.3

## Installation
Go to the release section, download the Unity Package and import it into any Unity project.
The package is a 'Hybrid Package', so it will install into your Packages folder as a local package.

## Usage
- Go to https://skybox.blockadelabs.com/ to generate a new panorama.
- Use the menu *Genesis -> Import from Skybox Lab via ID*, enter the ID and hit *Import*

## Ideas
Not a roadmap, just some general thoughts and ideas I have for the future.

- A next step could be to segment panoramas into layers using the generated depth. I'd like to experiment with sending a masked panorama back to SkyBox Labs as an input image for a new generation with the same prompt and automatically build a layered depth panorama that way. The idea here is that these are not real images, so we can let the AI hallucinate details where usually complex inpainting methods would be needed.
- Another interesting experiment could be done with rendering the scene from a different viewpoint using the naively extruded sphere and then again use that as an input image for a new generation to continually build out a larger 3D world from a given prompt.
- Add some export formats like .obj and glTF
- The seams and depth discontinuities could probably be fixed in a quick-and-dirty way, but I'd rather wait until I've figured out a better way to do the depth estimation (see below)

## Known Issues / Limitations
- The generated depth is not very high quality right now. The resolution for the depth maps we generate is only 256x256. Some limitations in [Barracuda](https://docs.unity3d.com/Packages/com.unity.barracuda@latest/index.html) require us to use the smaller version of an older depth estimation model ([MiDaS v2.1](https://github.com/isl-org/MiDaS)), but that is only part of the issue. MiDaS in and of itself is not really made for high-resolution, 360° images, which is why people have been researching on how to better approacj high resolution depth estimation . Notably, [360monodepth](https://github.com/manurare/360monodepth) seems to be the state of the art when it comes to generating high resolution depth for panoramic images. The neat thing about Baracuda is that we can run the inference directly in the Unity Editor, or even do it at runtime, but if we want better depth, looking at the setup instructions of the 360monodepth repository, it might require building a web service to do the depth estimation.

## Acknowledgements
Thanks goes out to these wonderful projects:
- Skybox Lab: https://skybox.blockadelabs.com/
- MiDaS v2 integraton: https://github.com/GeorgeAdamon/monocular-depth-unity
- Octahedron Sphere: https://catlikecoding.com/unity/tutorials/octahedron-sphere/