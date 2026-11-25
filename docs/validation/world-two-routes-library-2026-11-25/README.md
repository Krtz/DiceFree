# World Two route and library evidence

- [Runtime review](runtime-review.json): 97 passing checks for all 18 route surfaces, real actor speed, off-road and overlapping-road behavior, centerline coverage, normal bridge/city walking, valid replacement meshes/materials, preserved service roots and collision footprints.
- [Actual tree fading](tree-fade-review.json): two passing checks using the real camera-occlusion system.
- [Route roster](routes.json), [previous survey](routes-before.txt), [replacement roster](replacements.json), [portable asset dependencies](portable-assets.json).
- [Build report](build-report.json), [delivery receipt](delivery.json), [main/QA asset hashes](source-receipt.json).
- [Catalog shortlist](catalog-shortlist.json), [ten preview candidates](preview-candidates.txt). Missing cabin meshes were discovered and rejected during review.

![World Two improvements](overview.png)

[Full foundry view](anvilreach-stocked-foundry.png) · [Market](anvilreach-stocked-market.png) · [Steamroot](steamroot-river-groves.png) · [Deepmoss](deepmoss-rooted-groves.png) · [Cinderwatch](cinderwatch-road.png)

The development-player review uses a disposable copied save, disables enemy aggression and mouse-driven input for repeatability, then issues ordinary movement orders for the crossing checks. Screenshots render the gameplay camera explicitly in batch mode and omit overlay UI. This is neither a manual whole-game playthrough nor a visual review of every catalog package.

Reproduction tools: `WorldTwoRouteSurvey.cs`, `WorldTwoRoutesReview.cs`, `WorldTwoTreeFadeReview.cs`, `WorldTwoRoutesStaticReview.cs`, and `PackageWorldTwoRoutesEvidence.ps1` under `tools/validation`. Authoring is in `WorldTwoRouteAuthoring.cs` and `WorldTwoLibraryAuthoring.cs` under `tools/authoring`. Read the project's Unity CLI skill before executing editor/runtime review helpers.
