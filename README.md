# Abominable Atul

A horror escape room game in Unity, set in a real room at our university.
The player has to find 4 clues to escape while dealing with three ghosts:

- a **dangerous** ghost that kills you if it catches you
- a **good** ghost that gives hints
- a **mischievous** ghost that sabotages harmlessly (turns off lights and so on)

## Opening the project

1. Clone the repo. It uses **Git LFS** for models, textures and audio, so run `git lfs install` once before cloning (GitHub Desktop handles this for you).
2. Copy the room layout into `Assets/Models/Layout/` and name it `area_v2.fbx`.
3. In Unity Hub: **Add > Add project from disk** and pick this folder. Open it with **Unity 6.3**. If Hub says the exact version isn't installed, choose your installed 6.3 editor.
4. The first open imports the layout model, which takes a few minutes. Then the editor script builds `Assets/Scenes/Layout.unity` on its own, with:
   - the room (`Assets/Models/Layout/area_v2.fbx`), with colliders on solid surfaces
   - dark horror lighting: dim ambient light, fog and a weak moonlight
   - a `PlayerSpawn` on the floor with a camera at head height and a small player light
   - empty `Clues`, `Ghosts`, `Interactables` and `Audio` groups

   You can rebuild it any time from the menu: **Abominable Atul > Build Layout Scene**.

## Textures

The FBX doesn't embed its textures. It expects these files in a `textures` folder next to it:

- wood_table_001_diff_4k.jpg, wood_table_001_nor_gl_4k.exr, wood_table_001_rough_4k.jpg
- WoodFineDark004_COL_4K.jpg, WoodFineDark004_NRM_4K.jpg
- rail3.png, railing image.png, cealing Normal map.png, main door.png

Put them in `Assets/Models/Layout/textures/`, then right-click `area_v2.fbx` and choose **Reimport**.

## Performance note

The layout is about 3.5 million vertices. Most of it is in three objects: `chandelier` (about 2.5M vertices), `silent room lamps` (about 590k) and `bench 3` (about 280k). If the editor or the game feels slow, use a Decimate modifier on those three in Blender and re-export.
