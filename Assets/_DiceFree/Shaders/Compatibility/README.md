# URP GPU Resident Drawer debug shaders

These two shaders are derived from com.unity.render-pipelines.core 17.6.0, under the Unity Companion License (see LICENSE.md). Their shared Core rendering code and includes are preserved. The shader names use Hidden/DiceFree and the RenderPipeline tag is UniversalPipeline instead of HDRenderPipeline.

URP 17.6 includes these resources for the GPU Resident Drawer, but the upstream HDRP-only tags cause their subshaders to be stripped from a URP player build. The URP global settings reference these compatible copies so the actual debug materials remain available. Normal occlusion culling and GPU Resident Drawer settings are unchanged.

Reproduce the copies and resource references with dicefree.build-warnings.repair. The foundation Windows builder runs that repair before building. Reassess this compatibility patch when upgrading the Render Pipeline packages; remove it once the upstream resources support URP.
