# ZenCraft
**Version:** 0.0.1 alpha

A voxel-based sandbox game inspired by Minecraft, featuring procedural terrain generation, asynchronous chunk loading, and dynamic lighting.

## Controls

| Action | Key / Input |
| :--- | :--- |
| **Move** | `W`, `A`, `S`, `D` |
| **Look** | Mouse Movement |
| **Jump** | `Space` |
| **Fly (Toggle)** | Double-tap `Space` |
| **Ascend (Flying)** | Hold `Space` |
| **Descend (Flying)** | Hold `Left Shift` |
| **Crouch** | Hold `Left Control` (while walking) |
| **Stop Flying** | Press `Left Control` (while flying) |
| **Place Block** | `Right Click` |
| **Break Block** | `Left Click` |
| **Select Block** | Number keys `1` - `7` |
| **Toggle Debug Info** | `F3` |

## Current Development State

### Implemented Features
- **Procedural Terrain**: Infinite world generation with Perlin noise (hills, mountains, lakes).
- **Asynchronous System**: Chunk loading, mesh generation, and lighting calculations run on a background thread to minimize lag.
- **Dynamic Lighting**: 
  - Sunlight propagation (day/night cycle).
  - Block light (torches) with smooth propagation.
  - Async updates to prevent frame drops.
- **Village Generation**: Procedural villages spawn rarely (0.5% chance per chunk) with stone and wood houses.
- **Player Movement**:
  - Physics-based movement (gravity, collisions).
  - Creative Mode Flight (Double-jump).
  - Crouching/Sneaking.
- **Save System**: Region-based chunk saving and loading.

### Technical Details
- **Engine**: Custom engine using Raylib.
- **Language**: ZenC (Custom C-like language).
- **Rendering**: Optimized mesh generation with face culling.
