# 3D Asset Generation Pipeline

Research date: 2026-03-15

## Goal

Generate and modify 3D models (props, environments, characters with animations) from text descriptions. Models are rendered in real-time with dynamic lighting at ~800x600 or 1024x768, styled as high-res pixel art. Lighting and mood are central to the art direction.

## Constraints

- GPU: RTX 3080 10GB VRAM
- Budget: ~$10/month for external services (exploration phase)
- CLI-first workflow (editor CLI), viewable in WPF editor app
- Leverage Claude Code subscription as much as possible

## Asset Format: glTF 2.0 (.glb)

glTF/GLB is the only sensible choice:

- Open standard, no licensing issues, well-documented spec
- Stores meshes, PBR materials, skeletal rigs, and animations in one file
- Every AI generation tool outputs it
- Simpler to parse than FBX (Autodesk proprietary)
- JSON structure with binary buffers — can write a loader incrementally
- Natively supports `STEP` interpolation for animations (important for pixel-art aesthetic)

Engine needs: vertex/index buffer loading, PBR material params, joint hierarchy, animation keyframe sampling.

## Rendering Approach

Render full 3D scenes at internal resolution (800x600 or 1024x768) with:

- Standard forward or deferred lighting pipeline
- Dynamic point/directional lights with real shadows
- Normal-mapped surfaces reacting to light
- Post-process: optional color palette quantization, dithering, or outline shaders for pixel-art feel
- Nearest-neighbor texture filtering on models for crisp texel look

At 800x600+, model detail matters more than at classic 320x180 pixel art. Low-poly is still fine, but silhouettes and proportions need to be solid. PBR materials (albedo, normal, roughness, metallic) add visual richness even at modest poly counts.

## Animation Strategy: Stepped Keyframes

Use `STEP` interpolation on animation keyframes to achieve a choppy sprite-like motion while keeping full 3D lighting benefits:

- Author/import animations with standard keyframes (from Mixamo etc.)
- At runtime, use STEP interpolation — snap to nearest keyframe, no blending
- Run animation playback at 8-12 fps independent of render framerate (60fps)
- glTF natively supports `STEP` as an interpolation mode on animation samplers

This is simpler than smooth animation (skip interpolation math) and gives the pixel-art aesthetic of discrete animation frames while preserving dynamic lighting, shadows, and camera freedom.

## Generation Stack

### 1. Claude Code + Blender MCP (free — included in subscription)

Primary tool for procedural and iterative model creation.

**Setup:**
- Install [ahujasid/blender-mcp](https://github.com/ahujasid/blender-mcp) or the [official Blender Lab MCP server](https://projects.blender.org/lab/blender_mcp)
- Blender addon opens TCP socket (port 9876), MCP server bridges Claude to it
- Claude generates `bpy` Python scripts, executes them in Blender

**Best for:**
- Geometric/architectural props (buildings, furniture, dungeon pieces, weapons)
- Iterative refinement ("make it more angular", "add a handle")
- Material setup and lighting previews
- Retopology, decimation, UV unwrapping
- Batch processing and export automation

**Blender plugins worth noting:**
- **3D-Agent** — generates models with clean quad topology via Claude + MCP
- **Dream Textures** — runs Stable Diffusion locally for texture generation
- **Hunyuan3D 2.5 addon** — text/image-to-3D directly in Blender

### 2. TripoSG (free — runs locally on RTX 3080 at 8GB VRAM)

Feed-forward transformer (1.5B params) for image-to-3D.

- Input: single reference image or sketch
- Output: GLB mesh with sharp geometric features
- SDF-based output produces watertight meshes
- Trained on 2M curated image-SDF pairs
- No API costs, unlimited generations

**Best for:** Organic shapes, creatures, natural objects — things hard to script procedurally.

**Alternatives that fit 10GB VRAM:**
- **SPAR3D** (Stability AI, 7-10GB) — has built-in quad remeshing, runs on CUDA/MPS/CPU
- **SF3D** (Stability AI, 6GB) — sub-second generation, good for rapid prototyping
- **CraftsMan3D** (500M params, ~10GB) — produces regular mesh topology, MIT license

### 3. Tripo AI API (~$10/month)

Cloud API for higher-quality generation when local models fall short.

- Text-to-3D: ~$0.10/model (10 credits)
- Image-to-3D: ~$0.20-0.30/model
- **Auto-rigging included** — critical for characters
- Output: GLB, FBX, OBJ, USD with PBR textures
- Style transforms available (cartoon, clay, etc.)
- 2,000 free credits on signup
- Budget of $10/month = ~33-100 models

**Alternatives considered:**
- Meshy ($20/mo pro — over budget, but best docs and enterprise compliance)
- Hyper3D Rodin (highest quality, but $0.50-1.50/model — too expensive for exploration)
- Sloyd ($15/mo — over budget, but best game-ready topology)

### 4. Mixamo (free — Adobe)

Auto-rigging and animation library for humanoid characters.

- Upload any humanoid mesh → auto-rigs with skeleton
- Thousands of free animations (walk, run, attack, idle, etc.)
- Export as FBX with animations → convert to GLB
- No cost, no limits on usage

## Character Pipeline

```
Text description
  → Tripo AI API (text-to-3D + auto-rigging)        ~$0.30
  → OR: TripoSG locally → Mixamo for rigging         free
  → Mixamo for animation library                      free
  → Blender for cleanup/custom animations (via MCP)   free
  → Export GLB with STEP interpolation animations
  → Load in engine, render with dynamic lighting
```

## Props & Environment Pipeline

```
Text description
  → Claude Code + Blender MCP (procedural generation)  free
  → OR: TripoSG locally (organic shapes)               free
  → OR: Tripo AI API (complex assets)                  ~$0.10-0.30
  → Blender cleanup/decimation (via MCP)                free
  → Export GLB with PBR materials
  → Load in engine
```

## Monthly Budget Breakdown

| Item | Cost | What You Get |
|---|---|---|
| Claude Code subscription | (existing) | Unlimited Blender MCP scripting |
| TripoSG / SPAR3D local | $0 | Unlimited AI model generation |
| Mixamo | $0 | Auto-rigging + animation library |
| Tripo AI API | ~$10 | ~33-100 cloud-generated models with auto-rig |
| **Total** | **~$10** | |

## CLI Integration Concepts

```
pf generate asset "wooden treasure chest"        → Tripo API or local TripoSG
pf generate character "skeleton warrior"          → Tripo API + auto-rig
pf animate character <id> "walk" "idle" "attack"  → Mixamo animations
pf refine asset <id> "make it more angular"       → Blender MCP
pf render-preview <id>                            → pixel-art preview in editor app
```

## Open-Source Models Reference

Models that fit RTX 3080 10GB:

| Model | VRAM | Speed | Strength | License |
|---|---|---|---|---|
| TripoSG | 8GB | seconds | sharp geometry, clean meshes | open source |
| SPAR3D | 7-10GB | seconds | quad remeshing, cross-platform | open source |
| SF3D | 6GB | <1s | fastest, good for prototyping | open source |
| CraftsMan3D | ~10GB | 5-20s | regular topology | MIT |
| Hunyuan3D 2.1 (shape only) | 10GB | seconds | best PBR materials | open source |

Models requiring more VRAM (cloud or future GPU):

| Model | VRAM | Strength |
|---|---|---|
| TRELLIS 2 (Microsoft) | 16GB | most flexible, MIT license |
| Hunyuan3D 2.1 (full) | 29GB | best overall quality + PBR |
| Step1X-3D | 27-29GB | bridges 2D/3D, LoRA transfer |

## Next Steps

1. Install Blender MCP and test procedural prop generation via Claude Code
2. Sign up for Tripo AI (2000 free credits) and test character generation with auto-rigging
3. Test Mixamo rig/animation pipeline with a generated character
4. Run TripoSG locally on RTX 3080 to verify quality at target resolution
5. Implement glTF loader in engine (vertex buffers, joints, keyframes)
6. Build pixel-art post-processing shader (palette quantization, dithering)
