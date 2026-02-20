# Zen Craft

**Zen Craft** is a highly optimized voxel engine and sandbox game, built entirely from scratch in **pure Zen-C** (a modern language that transpiles to C) and powered by the **Raylib** graphics library.

This project has been rigorously refactored to push the **Zen-C** language to its absolute limits, avoiding raw C blocks (`raw`) and unsafe pointer arithmetic wherever possible. The result is a modular, memory-safe, and blazingly fast codebase.

## Core Features

* **Built in Pure Zen-C**: Modern, strongly-typed, and structured architecture that leverages the native speed of C without its verbosity.
* **Infinite Voxel World**: Procedural chunk-based terrain generation with seamless background loading and unloading.
* **High-Performance Architecture**: 
  * Bitwise operations for ultra-fast chunk coordinate math.
  * Asynchronous multi-threading (`zthread`) for file reading/writing, mesh generation, and light calculations to ensure zero frame drops (FPS stutter).
  * Custom memory allocation (`zalloc`) to prevent memory fragmentation.
* **Dynamic Lighting Engine**: Smooth flood-fill light propagation supporting sunlight (with day/night cycles) and artificial light sources (torches, glowing blocks).
* **Advanced Rendering & Shaders**:
  * Volumetric beam shaders for the magic scepter.
  * Procedural LOD foliage and grass using Fractional Brownian Motion noise, featuring "False Ambient Occlusion" via vertex-color gradients.
  * Transparency support for water and glass panes.
  * Custom physics-based particle and explosion system.
* **Gameplay & Magic Mechanics**:
  * Block mining and placement system.
  * Mana Management: Consume mana to destroy blocks with the scepter; recharge by finding Power Blocks.
  * Spellbook System to select different building abilities.
  * Dynamic entities ("Zibis") and procedural 3D skull decorations.

## Controls

* **W, A, S, D**: Move
* **Space**: Jump
* **Left Click**: Destroy block / Fire scepter beam (Consumes Mana)
* **Right Click**: Place block (Consumes Mana)
* **1 - 8**: Select block type (Dirt, Grass, Stone, Log, Leaves, Torch, Water, Glass Pane)
* **E**: Open/Close Spellbook
* **F1**: Open Configuration Menu (Adjust render distance, toggle LOD grass)
* **F3**: Toggle Debug HUD (FPS, chunk count, async thread queues, profiling stats)
* **F11**: Toggle Fullscreen

## Architecture & File Structure

The engine is designed in a highly modular way, separating game logic, rendering, and asynchronous processing across multiple Zen-C files:

### Orchestration & Game Loop
* `main.zc` - Main loop, state machine, and engine orchestrator.
* `game_loading.zc` - Loading screen logic, save file reading, and initial world preloading.
* `game_ui.zc` - Rendering for the HUD, settings menu, and performance profiling stats (Debug).
* `player_interaction.zc` - Raycasting logic, block destruction, and volumetric beam mathematics.

### Voxel Logic (Chunks)
* `chunk_core.zc` - Base chunk data structures and ultra-fast bitwise math operations.
* `chunk_gen.zc` - Procedural terrain generation algorithms.
* `chunk_mesh.zc` - Visual geometry construction algorithm (Greedy Meshing).
* `chunk_light.zc` - Lighting calculation and propagation engine.

### Multi-threading & Persistence (I/O)
* `persistence.zc` - Manager for asynchronous work queues and thread pools (workers).
* `persistence_defs.zc` - Data structure definitions and states for IO requests.
* `persistence_io.zc` - Binary region file (`.zcr`) reader and writer.
* `persistence_mesh.zc` - Delegated worker for background mesh construction.
* `persistence_light.zc` - Delegated worker for background light propagation.

### Entities & Player
* `player.zc` - Player collision physics, first-person camera, and movement.
* `zibi.zc` - Basic AI, physics, and rendering for "Zibi" entities.

### Environment & Visual Effects
* `rendering.zc` - Asset loading (textures, materials, shaders) and sky/cloud rendering.
* `grass_deco.zc` - Procedural tall grass generation system and LOD optimization.
* `particles.zc` - Independent physics engine for particles and explosions.
* `raycast.zc` - Ray-marching algorithm for visual block detection.
* `blocks.zc` - Dictionary for block IDs and properties.

### Extra Interfaces
* `spellbook.zc` - Interface and logic for the magic/summoning system.
* `title_generator.zc` - Automatic aesthetic world generation for the title screen.

## Compilation

To compile this project, you will need the official **Zen-C** transpiler and a standard C compiler (such as GCC or MinGW).

**Dependencies:**
* [Raylib](https://github.com/raysan5/raylib) (v4.0+)
* Integrated z-libs (`zthread.h`, `zalloc.h`)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.