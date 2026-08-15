# M1 Tasks: Volumetric Steering Gray-box

- [x] 1. Add Axis6 enum to Game.Core
  - [x] Create `Axis6.cs` with PosX, NegX, PosY, NegY, PosZ, NegZ (paired layout for IsOpposite)
  - [x] Static helper `Axis6Extensions.ToVector(Axis6) → Vector3Int`
  - [x] `IsOpposite(Axis6 a, Axis6 b) → bool` helper (same axis, different sign via `(int)a / 2 == (int)b / 2`)
  - _Requirements: REQ-M1-1_

- [x] 2. Implement SnakeHead in Game.Core
  - [x] `SnakeHead(Vector3Int startPos, Axis6 startHeading)` constructor
  - [x] `SetIntent(Axis6)` — queues a direction change; ignores 180° reversals silently
  - [x] `Advance()` — applies pending intent (if any), then moves position one cell along current heading
  - _Requirements: REQ-M1-1_

- [x] 3. Add SnakeHeadState snapshot struct and expand TickSimulation
  - [x] `[Serializable] SnakeHeadState` struct (position, heading)
  - [x] Expand `GameState` to include `playerHead SnakeHeadState`; added `Clone()` for snapshot capture
  - [x] Expand `TickSimulation`: constructor accepts start position/heading; `Tick()` drives head; `SetPlayerIntent()` forwards to head
  - _Requirements: REQ-M1-1_

- [x] 4. Add `SimulationRunner` and `TestFixtures` (testing-strategy tasks 3–4)
  - [x] `SimulationRunner.RunToCompletion()` — runs ticks with scheduled inputs, no round-status check (M3)
  - [x] `SimulationRunner.RunCapturing()` — captures deep-cloned snapshots per tick (avoids aliasing trap)
  - [x] `TestFixtures.DefaultSimulation()`, `OriginSimulation()` — factory methods for test setup
  - _Requirements: REQ-TEST-2_

- [x] 5. Write edit-mode tests for head movement (`SnakeHeadTests.cs`)
  - [x] Advance N times from origin along all six directions → position correct
  - [x] Set intent PosZ on tick 2; position changes axis at tick 2 (turn-at-boundary)
  - [x] Set 180° intent (NegX when heading PosX) → intent ignored, heading unchanged (all 6 axes)
  - [x] State snapshot includes correct head position and heading after each tick (via RunCapturing)
  - [x] Snapshots are independent clones (aliasing trap avoided)
  - [x] Serialize → deserialize GameState with non-default head state → fields match
  - [x] Clone() produces deep copy unaffected by original mutation
  - _Requirements: REQ-M1-1_

- [ ] 6. Implement GameLoop in Game.App
  - Fixed-interval `InvokeRepeating` drives `TickSimulation.Tick()`
  - `QueueIntent(Axis6)` stores one pending intent; overwrites if called twice before next tick
  - `OnTick` C# event fired after each tick with current `GameState`
  - Tick interval exposed as serialized field (default 0.3s); tuned in M4
  - _Requirements: REQ-M1-2_

- [ ] 7. Implement WorldAxisInput in Game.Presentation
  - Create `InputActionAsset` with actions for thumbstick (Vector2) and face buttons
  - Map thumbstick quadrant and buttons to Axis6 intents
  - On each input event, call `GameLoop.QueueIntent()`
  - _Requirements: REQ-M1-2_

- [ ] 8. Implement HeadRenderer in Game.Presentation
  - Subscribe to `GameLoop.OnTick`
  - Move `headVisual` transform to `LogicalToWorld(state.playerHead.position)` each tick
  - `LogicalToWorld`: multiply cell coords by `cellSize` (0.15m default), transform by play volume root
  - Render a simple cube or distinct marker — nothing else
  - _Requirements: REQ-M1-3_

- [ ] 9. Build and run turn-around test on-device
  - Deploy to Quest 3
  - Follow turn-around test protocol in design.md
  - If world-locked feels correct at all orientations → proceed to M2 with world-locked
  - If world-locked feels persistently wrong-handed → update `WorldAxisInput` to body-relative mapping
  - _Requirements: REQ-M1-4_

- [ ] 10. Record M1 outcome in DECISIONS.md (update D4)
  - Write the actual outcome: world-locked survives / body-relative adopted
  - If body-relative: describe the new input mapping (HMD yaw snap to nearest 90°)
  - _Requirements: REQ-M1-5_

- [ ] 11. Verify M1 gate criteria before continuing
  - [ ] Steering feels correct while the player is turned away from their start orientation
  - [ ] Control model is decided and recorded in DECISIONS.md (D4 updated)
  - **Do not start M2 until both criteria are checked**
