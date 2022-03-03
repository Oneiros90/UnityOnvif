# UnityOnvif
Basic library for Onvif protocol in Unity.

You project must be compiled with `.Net Framework`, the classic `.Net Standard 2.1` will generate errors.

# Core
The core module provides the `OnvifClient` class, which can be used to access to a specific camera device, control it through PTZ commands and retrieve the video streaming url.

# Discovery
The discovery module (which is optional and totally detached from the core) provides the `DiscoveryComponent` class, which can be used to scan your local network and automatically find Onvif devices
