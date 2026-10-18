# PJ8 — AR Building Model Viewer

An Android augmented-reality house viewer made with Unity, AR Foundation, and ARCore. The phone camera detects a flat surface; tap it to place a small house. Use the controls to rotate and zoom, inspect rooms, and hide/show building parts. A desktop preview mode works in the Unity Editor without AR hardware.

## Open and try the preview

1. Open Unity Hub and find **PJ8** in **Projects**.
2. Open it with Unity **6.0 LTS** and wait while Unity imports the project and packages.
3. Open `Assets/Scenes/PJ8Viewer.unity` and press the **Play** triangle.
4. In the Game view, click the floor to place the house. Drag to rotate it; use **Zoom +**, **Zoom -**, **Rooms**, **Roof**, and **Reset** along the bottom to explore it.

The desktop preview needs no server and no phone. If Windows asks whether **adb** may access public or private networks, choose **Cancel**; that network permission is not needed for the desktop preview. If Unity shows an Input System prompt, choose **Don't Enable** so the project keeps its current mouse and touch input setup.

## Try AR on Android

The project includes AR Foundation and ARCore packages. **ARCore is enabled and Android is the active build platform** in the current Unity setup. Connect a supported ARCore Android phone with USB debugging enabled, then use **File > Build Profiles > Build and Run**. On the phone, allow camera access, point at a well-lit table or floor, wait for the surface guide, and tap the surface.

## Notes

- The house is built from simple Unity primitives so no external model download is needed.
- Unity Hub is the project launcher; there is no web server to start.
- Full AR requires a supported Android phone and the Android build tools installed through Unity Hub.
- The Android app has not yet been installed or tested on a physical phone.
