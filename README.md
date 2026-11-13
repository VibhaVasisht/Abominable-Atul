# Abominable Atul

A VR horror escape room for Meta Quest Pro, set in a real room at our university.
The player has to find 4 clues to escape while dealing with three ghosts:

- a **dangerous** ghost that kills you if it catches you
- a **good** ghost that gives hints
- a **mischievous** ghost that sabotages harmlessly (turns off lights and so on)

## Opening the project

1. Clone the repo. It uses **Git LFS** for models, textures and audio, so run `git lfs install` once before cloning (GitHub Desktop handles this for you).
2. In Unity Hub: **Add > Add project from disk** and pick this folder. Open it with **Unity 6.3**. If Hub says the exact version isn't installed, choose your installed 6.3 editor.
3. The first open imports the layout model, which takes a few minutes. Then the editor script builds `Assets/Scenes/Layout.unity` on its own, with:
   - the room (`Assets/Models/Layout/area_v2.fbx`), with colliders on solid surfaces
   - dark horror lighting: dim ambient light, fog and a weak moonlight
   - a `PlayerSpawn` on the floor with a placeholder camera and a small player light
   - empty `Clues`, `Ghosts`, `Interactables` and `Audio` groups

   You can rebuild it any time from the menu: **Abominable Atul > Build Layout Scene**.

## Textures

The FBX doesn't embed its textures. It expects these files in a `textures` folder next to it:

- wood_table_001_diff_4k.jpg, wood_table_001_nor_gl_4k.exr, wood_table_001_rough_4k.jpg
- WoodFineDark004_COL_4K.jpg, WoodFineDark004_NRM_4K.jpg
- rail3.png, railing image.png, cealing Normal map.png, main door.png

Put them in `Assets/Models/Layout/textures/`, then right-click `area_v2.fbx` and choose **Reimport**.

## Setting up VR for Quest Pro

1. **File > Build Profiles**: switch the platform to **Android**.
2. **Edit > Project Settings > XR Plug-in Management**: install it, then tick **OpenXR** on the Android tab. Add the **Meta Quest Support** feature and the Meta Quest Pro interaction profile.
3. **Window > Package Manager**: install **XR Interaction Toolkit** and import its **Starter Assets** sample.
4. In the Layout scene, delete the placeholder camera and drop an **XR Origin (XR Rig)** under `PlayerSpawn`.

## Performance note

The layout is about 3.5 million vertices, which is heavy for a standalone headset. Most of it is in three objects: `chandelier` (about 2.5M vertices), `silent room lamps` (about 590k) and `bench 3` (about 280k). The walls are only about 38k. Before building for Quest, use a Decimate modifier on those three in Blender and re-export.
