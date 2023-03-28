# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2023-03-28

### Removed

- Renoved the option to import skyboxes directly via their ID.

## [0.1.3] - 2023-03-07

### Fixed

- Fixed possible IndexOutOfRangeException with bilinear depth sampling when extracting meshes.

## [0.1.2] - 2023-02-24

### Fixed

- Fix errors due to invalid paths on macOS.

## [0.1.1] - 2023-02-24

### Fixed

- Added missing dependency on the Newtonsoft Json Unity Package.

## [0.1.0] - 2023-02-23

### Added

- Add option to extract mesh from a DepthSkybox object.

### Changed

- Rotate and mirror the output of depth estimation so it matches the input texture.

## [0.0.2] - 2023-02-23

### Fixed

- Fixed depth calculations. Absolute depth is now inferred from inverse depth by exposing a scale factor as a material property. This should get rid of distorted areas near the poles and depth in general should feel be more correct.

## [0.0.1] - 2023-02-22
Initial Release