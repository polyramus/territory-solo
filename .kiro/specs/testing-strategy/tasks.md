# Testing Strategy Tasks

These tasks establish the test infrastructure. Most slot into M0; the rest are tied to the
milestone that builds the system they test.

Task numbers are stable — other specs reference them by number, so a task that moves between
milestones keeps its number and changes section.

---

## Infrastructure (M0)

- [ ] 1. Create `Game.Core.Tests` EditMode assembly
  - Create `Assets/Tests/EditMode/Game.Core.Tests.asmdef`
  - References: `Game.Core`, `Unity.TestFramework.NUnit`, `Unity.PerformanceTesting`
  - `includePlatforms: ["Editor"]`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`
  - Delete or rename the existing `Assets/Tests/Tests.asmdef` (it's a MRUK sample placeholder — keep `TestsExamples.cs` only if needed, it references MRUK internals)
  - _Requirements: REQ-TEST-1_

- [x] 2. Create `Game.Presentation.Tests` PlayMode assembly
  - Create `Assets/Tests/PlayMode/Game.Presentation.Tests.asmdef`
  - References: `Game.Core`, `Game.Presentation`, `Game.App`, `Unity.TestFramework.NUnit`, MRUK tests assembly
  - `includePlatforms: []` (runs both Editor and Standalone, but tested in Play Mode)
  - `defineConstraints: ["UNITY_INCLUDE_TESTS"]`
  - _Requirements: REQ-TEST-5_

- [x] 5. Add `IPlayVolumeSource` interface and `MRUKPlayVolumeSource` implementation
  - [x] Create `Assets/Scripts/Presentation/XR/IPlayVolumeSource.cs` (interface)
  - [x] Create `Assets/Scripts/Presentation/XR/MRUKPlayVolumeSource.cs` (wraps MRUK)
  - [x] Refactored `PlayVolumeAnchor` to depend on `IPlayVolumeSource`, not directly on MRUK
  - _Requirements: REQ-TEST-5_

- [x] 6. Create TestRoom.json fixture
  - [x] Copied MRUK sample room (`MultiFloor.json`) to `Assets/Tests/Rooms/TestRoom.json`
  - [ ] Verify room dimensions ≥ 3m × 3m × 3m (comfortably fits a 2m play volume)
  - _Requirements: REQ-TEST-7_

- [x] 7. Write CLI test runner script
  - Create `runtests.ps1` at project root (see design.md for implementation)
  - Test: `.\runtests.ps1 -mode editmode` discovers Unity and exits 0 when tests pass
  - Create `TestResults/` directory (add to `.gitignore`)
  - _Requirements: REQ-TEST-6_

---

## Simulation harness (M1)

Moved out of the M0 infrastructure block: both depend on types M1 introduces
(`Axis6`, `SetPlayerIntent`, the multi-arg `TickSimulation` constructor), so neither can be
written at M0. Build them after M1 tasks 1–3, before the scenario tests that consume them.

- [ ] 3. Add `SimulationRunner` to Game.Core
  - Create `Assets/Scripts/Core/Simulation/SimulationRunner.cs`
  - Implement `RunToCompletion(sim, inputs, maxTicks)` and `RunCapturing(sim, ticks, inputs)`
  - See design.md for full implementation
  - The design's API also reads `state.roundStatus` / `RoundStatus.Playing`, which do not exist
    until M3. At M1, run the plain tick loop and add the early-exit when M3 lands
  - `RunCapturing` returns a `List<GameState>`, but `GameState` is a class — capturing
    `sim.State` each tick stores N references to the same mutable object, so every entry ends
    up holding the final state. Capture a copy per tick (round-trip through `JsonUtility`, or
    give `GameState` a `Clone()`), or the per-tick assertions in design.md silently pass/fail
    on the wrong data
  - _Requirements: REQ-TEST-2_

- [ ] 4. Create `TestFixtures` helper class in Game.Core.Tests
  - `Assets/Tests/EditMode/TestFixtures.cs`
  - `DefaultSimulation()`, `TinyGridSimulation()`, `HeadOnCollisionSimulation()`
  - The AI-snake arguments (`aiStart`, `aiHeading`) and `HeadOnCollisionSimulation` only become
    meaningful at M3 — at M1 seed the player snake only and extend the fixtures then
  - _Requirements: REQ-TEST-2_

---

## Core unit tests (M0 + M1)

- [ ] 8. Write SnakeHead unit tests
  - `Assets/Tests/EditMode/SnakeHeadTests.cs`
  - Advance N ticks → correct position; turn at tick boundary; 180° blocked; PeekNextPosition
  - _Requirements: REQ-TEST-1_

- [ ] 9. Write TickSimulation unit tests
  - `Assets/Tests/EditMode/TickSimulationTests.cs`
  - Tick increments `state.tick`; player intent applied; state snapshot accurate; round-trip serialization
  - _Requirements: REQ-TEST-1_

---

## Simulation scenario tests (M1 + M3)

- [ ] 10. Write SimulationScenarioTests
  - `Assets/Tests/EditMode/SimulationScenarioTests.cs`
  - Scenarios (add each as the relevant Core feature is built):
    - M1: Player drives in a straight line for N ticks → position correct at tick N
    - M3: Player hits boundary → AIWins
    - M3: AI hits player's trail → PlayerWins
    - M3: Same-tick collision → Draw
    - M3: Full round plays to completion (no infinite loop)
    - M3: Trail grows exactly one cell per tick
  - _Requirements: REQ-TEST-2_

- [ ] 11. Write AiControllerTests
  - `Assets/Tests/EditMode/AiControllerTests.cs`
  - AI facing open grid → advances without self-collision (first 5 ticks); all-blocked → no exception; randomness does not break convergence
  - _Requirements: REQ-TEST-1_

- [ ] 12. Write RoundLifecycleTests
  - `Assets/Tests/EditMode/RoundLifecycleTests.cs`
  - One snake dead → correct winner; double death → Draw; `ResetRound()` clears cells and revives snakes
  - _Requirements: REQ-TEST-1_

- [ ] 13. Write GameStateSerializationTests
  - `Assets/Tests/EditMode/SerializationTests.cs`
  - Empty state round-trips; state with head + occupied cells round-trips; round status preserved
  - _Requirements: REQ-TEST-1_

---

## Algorithm tests (M2)

- [ ] 14. Write GreedyMesherTests
  - `Assets/Tests/EditMode/GreedyMesherTests.cs`
  - Single cell → 6 quads; two adjacent cells → 10 quads; full 8×8×8 chunk → no exception; chunk boundary cells handled
  - _Requirements: REQ-TEST-3_

- [ ] 15. Write CoordinateMathTests
  - `Assets/Tests/EditMode/CoordinateMathTests.cs`
  - `LogicalToWorld(Vector3Int.zero, ...)` → volume origin; `LogicalToWorld((1,0,0), ...)` → origin + cellSize on X
  - `ChunkKeyFor(Vector3Int.zero)` → (0,0,0); `ChunkKeyFor((8,0,0))` → (1,0,0)
  - _Requirements: REQ-TEST-4_

- [ ] 16. Write GreedyMesher performance benchmark
  - `Assets/Tests/EditMode/GreedyMesherBenchmarks.cs`
  - `[Test, Performance]`: build a fully-filled 8×8×8 chunk, measure median < 2ms
  - Requires `Unity.PerformanceTesting` reference in asmdef
  - _Requirements: REQ-TEST-8_

---

## Presentation integration tests (M2 + M3)

- [ ] 17. Write PlayVolumeAnchorTests (Play Mode)
  - `Assets/Tests/PlayMode/PlayVolumeAnchorTests.cs`
  - Extend `MRUKTestBase`; load `TestRoom.json` via `LoadSceneFromJsonStringAndWait`
  - Assert play volume root Y position ≈ floor center Y + half-height (within 0.01m)
  - _Requirements: REQ-TEST-5_

- [ ] 18. Write ChunkManagerTests (Play Mode)
  - `Assets/Tests/PlayMode/ChunkManagerTests.cs`
  - In a lightweight test scene (no XR rig): add 1 cell → 1 dirty chunk rebuild; add 9 cells in same chunk → still 1 rebuild that tick; add cells in 2 chunks → 2 rebuilds
  - _Requirements: REQ-TEST-5_

---

## Verification

- [ ] 19. Verify all Edit Mode tests pass from CLI
  - `.\runtests.ps1 -mode editmode` exits 0
  - `TestResults\TestResults-editmode.xml` is created and shows 0 failures
  - _Requirements: REQ-TEST-6_

- [ ] 20. Verify all Play Mode tests pass headlessly
  - `.\runtests.ps1 -mode playmode` exits 0 (no Quest connected)
  - `TestResults\TestResults-playmode.xml` shows 0 failures
  - _Requirements: REQ-TEST-5, REQ-TEST-6_
