# SYNC & SHELTER - Implementation Plan

**Game Type:** 2-Player Local Co-op Empathy Puzzler  
**Theme:** Safety First / Beyond Stereotypes  
**Target Duration:** 10-15 minute playthrough  
**Platform:** PC (Shared Keyboard)

---

## Overview

Build a cooperative game where two players control asymmetric creatures (The Bulk & The Spark) who must maintain proximity to survive. Success requires constant communication and synchronized actions. The game subverts combat stereotypes by making vulnerability and mutual dependency the core mechanics.

---

## PHASE 1: FOUNDATION (Core Systems)

### 1.1 Configure Input System for 2-Player Local Co-op

**Files to Modify:**

- `Assets/InputSystem_Actions.inputactions`

**Tasks:**

- Create two separate Action Maps: "Player1" and "Player2"
- **Player 1 Bindings:**
  - Move: WASD (Vector2)
  - CheckIn: Space (Button, Hold interaction)
- **Player 2 Bindings:**
  - Move: Arrow Keys (Vector2)
  - CheckIn: Enter (Button, Hold interaction)
- Set both action maps to active simultaneously
- Test that both input schemes work without conflicts

**Verification:**

- Both players can move simultaneously on shared keyboard
- Hold interactions work for CheckIn buttons

---

### 1.2 Create Base Character Controllers

**Files to Create:**

- `Assets/Scripts/Characters/BaseCreatureController.cs`
- `Assets/Scripts/Characters/TheBulkController.cs`
- `Assets/Scripts/Characters/TheSparkController.cs`

**BaseCreatureController.cs:**

- Accept Input System actions via inspector
- Handle 2D movement with Rigidbody2D
- Expose movement speed variable
- Provide virtual methods for child classes to override
- Track CheckIn button press state

**TheBulkController.cs:**

- Inherit from BaseCreatureController
- Set movement speed to slow (e.g., 3 units/sec)
- Add tag "TheBulk" for wind immunity
- Add larger collider for blocking

**TheSparkController.cs:**

- Inherit from BaseCreatureController
- Set movement speed to fast (e.g., 6 units/sec)
- Add tag "TheSpark" for wind vulnerability
- Add 2D light component (URP Light 2D)

**Verification:**

- Both characters move at different speeds
- Input System correctly wired to both controllers
- Characters have proper tags and colliders

---

### 1.3 Implement Safety Meter Core System

**Files to Create:**

- `Assets/Scripts/Core/SafetyMeterManager.cs`

**SafetyMeterManager.cs (Singleton):**

- Track references to both player transforms
- Calculate distance between players every frame
- Safety Meter: 0-100 float value
- **Drain Logic:**
  - If distance > 5m: drain at rate (e.g., 10 points/sec)
  - If distance ≤ 5m: restore at rate (e.g., 5 points/sec)
- **Huddle State:**
  - Detect when players overlap (distance < 1m)
  - Set isHuddling flag
  - Boost restoration rate in huddle (e.g., 15 points/sec)
- Expose public properties:
  - `float SafetyMeterValue { get; }`
  - `float SafetyMeterPercent { get; }` (0-1 normalized)
  - `bool IsHuddling { get; }`
  - `float DistanceBetweenPlayers { get; }`
- Invoke events on state changes

**Verification:**

- Meter drains when players separate beyond 5m
- Meter restores when players come close
- Huddle state detected correctly
- Values accessible from other scripts

---

### 1.4 Build Basic UI

**Files to Create:**

- `Assets/Scripts/UI/SafetyMeterUI.cs`
- `Assets/Scripts/UI/CheckInPromptUI.cs`

**SafetyMeterUI.cs:**

- Reference SafetyMeterManager
- Use UI Slider or Image fill for visual meter
- Update fill amount based on SafetyMeterPercent
- Change color gradient: Green (100%) → Yellow (50%) → Red (0%)
- Always visible on screen (top-center recommended)

**CheckInPromptUI.cs:**

- Show/hide prompt panel
- Display text: "Synchronized Breath: Hold Space + Enter"
- Show progress bars for both players' hold duration
- Display success/failure feedback

**Verification:**

- Safety Meter UI updates in real-time as players move
- Color gradient changes smoothly
- CheckIn prompt can be toggled on/off

---

### 1.5 Implement Proximity Camera System

**Files to Create:**

- `Assets/Scripts/Core/ProximityCamera.cs`

**ProximityCamera.cs:**

- Attach to Main Camera
- Track both player transforms
- Calculate midpoint between players
- Move camera to midpoint with smooth damping
- Calculate required orthographic size to fit both players
- Set min zoom (e.g., 5) and max zoom (e.g., 15)
- Add padding so players aren't right at screen edge
- Smooth zoom transitions

**Verification:**

- Camera follows both players smoothly
- Both players always visible on screen
- Camera doesn't zoom too far in or out
- Works smoothly when players move in opposite directions

---

## PHASE 2: CORE MECHANICS

### 2.1 Implement "Synchronized Breath" Mechanic

**Files to Create:**

- `Assets/Scripts/Core/SyncBreathManager.cs`

**SyncBreathManager.cs (Singleton):**

- Timer that triggers every 60 seconds
- When triggered:
  - Show CheckInPromptUI
  - Monitor both players' CheckIn button states
  - Require both to hold for 2 seconds simultaneously
  - Track individual hold times with progress bars
- On Success:
  - Clear visual static effects
  - Add bonus to Safety Meter (e.g., +20 points)
  - Trigger particle burst
  - Play success sound
- On Failure:
  - If either player releases early or timeout occurs
  - Apply penalty: increase Safety Meter drain rate by 1.5x until next success
  - Play failure sound
- Reset timer and prepare for next prompt

**Verification:**

- Prompt appears every 60 seconds
- Both players must hold simultaneously
- Early release counts as failure
- Success triggers visual/audio feedback

---

### 2.2 Add Visual Feedback System

**Files to Create:**

- `Assets/Scripts/Managers/VisualFeedbackManager.cs`
- `Assets/Materials/DesaturationEffect.mat` (URP Post-Process)
- `Assets/Sprites/VFX/AnxietyVignette.png`
- `Assets/Prefabs/VFX/SuccessParticles.prefab`

**VisualFeedbackManager.cs (Singleton):**

- Reference SafetyMeterManager
- **Desaturation Effect:**
  - Use URP Color Adjustments (Post-Processing Volume)
  - Set saturation based on Safety Meter: 0% → -100 saturation
  - Smooth transitions
- **Anxiety Vignette:**
  - Create UI overlay image with hand-drawn scribbles
  - Scale vignette intensity with inverse of Safety Meter
  - At 0% meter: full vignette coverage
  - At 100% meter: no vignette
- **Success Particle Burst:**
  - Instantiate particle system at midpoint between players
  - Anime-style petals or light orbs
  - Triggered by SyncBreathManager on success

**Verification:**

- Screen desaturates as Safety Meter drops
- Vignette intensifies at low meter values
- Particle burst appears on sync breath success
- All effects transition smoothly

---

### 2.3 Implement Audio System

**Files to Create:**

- `Assets/Scripts/Managers/AudioManager.cs`
- `Assets/Audio/Heartbeat.wav`
- `Assets/Audio/SyncSuccess.wav`
- `Assets/Audio/SyncFail.wav`

**AudioManager.cs (Singleton):**

- Use AudioSource components for different layers
- **Heartbeat System:**
  - Looping heartbeat sound
  - Pitch increases with distance (1.0 at close, 1.5 at max distance)
  - Volume increases with distance (0.3 at close, 1.0 at max)
  - Controlled by Safety Meter value
- **SFX Layer:**
  - Play one-shot sounds for sync breath success/fail
  - Collectible pickup sounds
  - Footstep sounds (optional)
- **Music Layer (optional):**
  - Ambient music that becomes more distorted at low Safety Meter

**Verification:**

- Heartbeat speeds up when players separate
- Volume increases with separation
- Sync breath SFX play correctly
- Audio doesn't overlap annoyingly

---

### 2.4 Create "Huddle" State Detection

**Files to Modify:**

- `Assets/Scripts/Core/SafetyMeterManager.cs`
- `Assets/Scripts/Characters/BaseCreatureController.cs`

**Enhancements:**

- Add CircleCollider2D (trigger) to both characters
- When triggers overlap, set isHuddling = true
- Create visual indicator:
  - Soft glow sprite around both characters
  - Particle system (gentle sparkles)
- Increase Safety Meter restoration rate (15 points/sec in huddle vs 5 normal)
- Play soft ambient sound when entering huddle
- Show UI text: "Safe" or heart icon

**Verification:**

- Huddle state activates when characters overlap
- Visual glow appears around both
- Meter restores faster in huddle
- UI shows huddle status

---

## PHASE 3: CHARACTER-SPECIFIC ABILITIES

### 3.1 Implement The Spark's Light System

**Files to Create:**

- `Assets/Scripts/Characters/LightController.cs`
- `Assets/Scripts/Environment/DarknessOverlay.cs`

**LightController.cs:**

- Attach to The Spark
- Use URP 2D Light component (Point Light)
- Base radius: 8 units
- Light radius scales with Safety Meter (8 units at 100%, 4 units at 0%)
- Warm color (yellow/orange)
- Intensity pulsates gently

**DarknessOverlay.cs:**

- Attach to main camera
- Create UI Canvas overlay completely black
- Use sprite mask or shader to reveal areas near The Spark
- Only active in Zone 3 (Dark Maze)
- The Bulk cannot see beyond The Spark's light

**Verification:**

- Light follows The Spark
- Radius changes with Safety Meter
- Darkness overlay reveals only lit areas
- The Bulk must stay near to see path

---

### 3.2 Implement The Bulk's Wind Immunity

**Files to Create:**

- `Assets/Scripts/Environment/WindZone.cs`

**WindZone.cs:**

- BoxCollider2D or PolygonCollider2D as trigger
- Apply constant force to Rigidbody2D in direction (configurable)
- **Tag Filtering:**
  - Check if entering object has tag "TheSpark"
  - Apply force only to The Spark
  - Ignore "TheBulk" tag completely
- Wind strength configurable (e.g., 50 force units)
- Visual indicator: particle system showing wind direction

**Verification:**

- The Bulk stays still in wind zones
- The Spark gets pushed by wind
- Wind force is noticeable but not overwhelming

---

### 3.3 Create "Slipstream" Mechanic

**Files to Create:**

- `Assets/Scripts/Characters/SlipstreamZone.cs`

**SlipstreamZone.cs:**

- Attach trigger collider BEHIND The Bulk (child object)
- BoxCollider2D positioned at back, extending 3-4 units
- Detect when The Spark enters trigger
- Set flag on The Spark: `isInSlipstream = true`
- **In WindZone.cs:**
  - Check if The Spark has `isInSlipstream == true`
  - If true, don't apply wind force
- Visual indicator:
  - Trail particle effect showing safe zone behind The Bulk
  - Subtle cone shape particles

**Verification:**

- The Spark is protected from wind when behind The Bulk
- Visual trail clearly indicates safe zone
- Positioning must be precise (directly behind)

---

### 3.4 Implement Grounding Totems Collectibles

**Files to Create:**

- `Assets/Scripts/Collectibles/GroundingTotem.cs`
- `Assets/Scripts/Collectibles/SmoothStone.cs`
- `Assets/Scripts/Collectibles/DriedLeaf.cs`
- `Assets/Scripts/Collectibles/FabricScrap.cs`
- `Assets/Scripts/Managers/CollectibleManager.cs`

**GroundingTotem.cs (Base Class):**

- Sprite renderer for object
- Trigger collider
- OnTriggerEnter: check if both players are in range (huddle required)
- Call virtual method `OnCollected()`
- Destroy self after collection
- Play collection sound and particle effect

**SmoothStone.cs:**

- Inherits GroundingTotem
- On collection: unlock Root ability for The Bulk
- Visual: smooth stone sprite (photogrammetry)

**DriedLeaf.cs:**

- Inherits GroundingTotem
- On collection: unlock Glide ability for The Spark
- Visual: dried leaf sprite

**FabricScrap.cs:**

- Inherits GroundingTotem
- On collection: trigger final sync breath and end sequence
- Visual: fabric scrap sprite

**CollectibleManager.cs (Singleton):**

- Track which totems have been collected (bool flags)
- **Root Ability:**
  - Add to TheBulkController
  - Press ability key to become immovable (Rigidbody2D.bodyType = Static)
  - Visual indicator: The Bulk glows, particles show grounding
  - Useful for anchoring in strong wind
- **Glide Ability:**
  - Add to TheSparkController
  - Press ability key to extend horizontal movement while in air
  - Reduce gravity temporarily
  - Visual: wings spread animation

**Verification:**

- Totems only collectible when both players are huddling
- Abilities unlock correctly
- Root makes The Bulk immovable
- Glide extends The Spark's air time
- FabricScrap triggers ending

---

## PHASE 4: LEVEL DESIGN & ENVIRONMENTS

### 4.1 Create Zone 1: Learning to Walk Together

**Scene Setup:**

- Simple horizontal corridor (50 units long)
- No environmental hazards
- Distance markers every 5 units (visual guides)
- Tutorial text (optional): "Stay close. Distance drains your safety."

**Flow:**

1. Players spawn side-by-side
2. Natural movement teaches base controls
3. If they separate too far, meter drains (teaches consequence)
4. First sync breath prompt occurs here
5. Smooth Stone totem at end (requires huddle to collect)
6. Transition to Zone 2

**Assets Needed:**

- Ground tilemap
- Background sprite (neutral, calming)
- Distance marker sprites
- Smooth Stone sprite

**Verification:**

- Players understand movement and safety meter
- First sync breath tutorial clear
- Root ability unlocks from totem

---

### 4.2 Create Zone 2: Wind Corridor

**Scene Setup:**

- Narrow passages (3-4 units wide)
- Multiple WindZone objects with horizontal forces
- Obstacles that block wind (walls, rocks)
- Requires coordination: The Bulk blocks, The Spark follows in slipstream

**Flow:**

1. Wind introduced gradually (light → strong)
2. First section: single wind source, simple blocking
3. Second section: multiple wind directions, requires repositioning
4. Third section: continuous strong wind with The Bulk leading
5. Dried Leaf totem at end (collect in huddle)
6. Transition to Zone 3

**Assets Needed:**

- Wind particle effects
- Rock/wall sprites for obstacles
- Dried Leaf sprite
- Background: windy outdoor environment

**Verification:**

- The Spark cannot proceed without The Bulk's protection
- Slipstream mechanic clearly visible
- Glide ability unlocks at end

---

### 4.3 Create Zone 3: Dark Maze

**Scene Setup:**

- Branching maze (5-6 room layout)
- Complete darkness (DarknessOverlay active)
- Only The Spark's light reveals paths
- Dead ends require backtracking
- The Bulk must trust The Spark's navigation

**Flow:**

1. Darkness overlay activates (tutorial: "Follow the light")
2. Initial corridor is straightforward (teaches mechanic)
3. Maze branches with 2-3 wrong paths
4. Safety Meter pressure forces staying close despite slow navigation
5. Fabric Scrap at maze exit (final collectible)
6. Final sync breath prompt with extended duration (4 seconds)
7. Fade to ending screen

**Assets Needed:**

- Maze wall sprites
- Darkness overlay shader
- Fabric Scrap sprite
- Background: mysterious indoor/cave environment

**Verification:**

- The Bulk cannot navigate without The Spark's light
- Maze solvable but requires communication
- Final sync breath feels ceremonial
- Ending triggers properly

---

### 4.4 Integrate Photogrammetry Backgrounds

**Asset Creation:**

1. Photograph 3 real-world objects:
   - Smooth stone
   - Dried leaf
   - Piece of fabric
2. Process in Photoshop/GIMP:
   - Apply Posterize filter (4-6 levels)
   - Apply slight blur
   - Adjust contrast to match anime aesthetic
   - Export as PNG
3. Import to Unity as sprites
4. Layer behind gameplay as parallax backgrounds
5. Use in respective zones as thematic elements

**Verification:**

- Photos match anime art style
- Don't distract from gameplay
- Enhance atmosphere

---

## PHASE 5: POLISH & JUICE

### 5.1 Implement Remaining Visual Effects

**Tasks:**

- **Screen Shake:**
  - Create ScreenShake.cs for camera
  - Trigger when Safety Meter < 20%
  - Shake intensity increases as meter approaches 0
- **Zone Transitions:**
  - Fade to black between zones
  - 1-2 second pause with optional text
- **Character Animations:**
  - Idle, Walk cycles for both characters (2-3 frames each)
  - Huddle animation (special pose when overlapping)
  - Use Unity's 2D Animation system
- **Particle Polish:**
  - Footstep dust clouds
  - Glow aura when abilities active
  - Environmental particles (leaves in wind, dust in darkness)

**Verification:**

- Screen shake feels urgent but not nauseating
- Transitions are smooth
- Animations play correctly at all times

---

### 5.2 Audio Polish

**Tasks:**

- **Ambient Sounds:**
  - Zone 1: Peaceful outdoor ambience
  - Zone 2: Wind howls, leaves rustling
  - Zone 3: Eerie cave echoes, dripping water
- **SFX:**
  - Footsteps (different for Bulk vs Spark)
  - Wind whoosh when entering WindZone
  - Collectible pickup (musical chime)
  - Ability activation sounds
- **Music (Optional):**
  - Layered ambient track
  - Add distortion/static layer at low Safety Meter
  - Remove distortion in huddle
- **Mixing:**
  - Balance all audio levels
  - Heartbeat should be prominent but not overwhelming
  - SFX should support, not dominate

**Verification:**

- Audio enhances mood without being jarring
- Heartbeat clearly audible as warning
- Music layers transition smoothly

---

### 5.3 UI/UX Improvements

**Tasks:**

- **Safety Meter UI:**
  - Replace placeholder with polished bar
  - Add gradient: Green → Yellow → Red
  - Pulsing animation at critical levels
  - Icon or label: "Safety" or heart symbol
- **Ability Icons:**
  - Show unlocked abilities with icons
  - Visual feedback when activated
  - Cooldown indicators if applicable
- **Control Prompts:**
  - On-screen display: "Player 1: WASD + Space" / "Player 2: Arrows + Enter"
  - Fade out after 10 seconds
  - Toggle with button (e.g., Tab)
- **Pause Menu:**
  - Press Escape to pause
  - Resume, Restart, Quit options
  - Show current zone progress
- **End Screen:**
  - "You found safety together."
  - Display time taken
  - Restart button

**Verification:**

- UI is clear and legible
- Pause menu functional
- End screen triggers correctly

---

### 5.4 Final Integration & Bugfixes

**Tasks:**

- **Full Playthrough Testing:**
  - Complete game from start to finish
  - Test with two real players on shared keyboard
  - Note any confusion points or bugs
- **Edge Case Testing:**
  - One player tries to go offscreen (camera bounds)
  - Rapid separation and reunion (meter calculations)
  - Sync breath while moving
  - Collectible pickup during windstorm
- **Balance Tuning:**
  - Safety Meter drain rate (too fast/slow?)
  - Wind force strength (too strong/weak?)
  - Light radius (too small/large?)
  - Sync breath timer (60 sec good or adjust?)
- **Bug Fixes:**
  - Collision issues (players getting stuck)
  - Camera bugs (jittering, zooming incorrectly)
  - Input conflicts (both players pressing same keys)
  - Audio overlaps or cutoffs
- **Performance:**
  - Check frame rate (target 30+ FPS)
  - Optimize particle counts if needed
  - Reduce texture sizes if needed

**Verification:**

- Game completable without bugs
- Meter rates feel fair
- Controls responsive
- Audio/visual feedback clear
- Performance stable

---

## Technical Architecture Summary

### Core Managers (Singletons)

- **SafetyMeterManager**: Central state for distance and meter logic
- **SyncBreathManager**: 60-second timer and dual-input mechanic
- **AudioManager**: Dynamic audio based on game state
- **VisualFeedbackManager**: Post-processing and screen effects
- **CollectibleManager**: Track totem collection and ability unlocks

### Character System

- **BaseCreatureController**: Shared movement, input handling
  - **TheBulkController**: Slow, wind-immune, can Root
  - **TheSparkController**: Fast, light source, can Glide

### Environment Systems

- **ProximityCamera**: Dual player tracking
- **WindZone**: Directional force on The Spark
- **DarknessOverlay**: Light-based visibility in Zone 3
- **SlipstreamZone**: Protected zone behind The Bulk

### UI Components

- **SafetyMeterUI**: Visual meter display
- **CheckInPromptUI**: Sync breath interface
- **HUDManager**: Ability icons, prompts, pause menu

### Collectibles

- **GroundingTotem** (Base)
  - **SmoothStone**: Unlocks Root
  - **DriedLeaf**: Unlocks Glide
  - **FabricScrap**: Triggers ending

---

## Asset Checklist

### Sprites

- [ ] The Bulk: Idle, Walk, Huddle animations
- [ ] The Spark: Idle, Walk, Huddle animations
- [ ] Smooth Stone collectible
- [ ] Dried Leaf collectible
- [ ] Fabric Scrap collectible
- [ ] Anxiety vignette overlay
- [ ] Distance markers
- [ ] UI meter bar graphics

### Particle Effects

- [ ] Success burst (petals/orbs)
- [ ] Wind particles
- [ ] Slipstream trail
- [ ] Huddle glow
- [ ] Footstep dust
- [ ] Ability activation effects

### Audio

- [ ] Heartbeat loop
- [ ] Sync breath success SFX
- [ ] Sync breath fail SFX
- [ ] Collectible pickup SFX
- [ ] Ability activation SFX
- [ ] Footsteps (2 variants)
- [ ] Wind ambience
- [ ] Cave ambience
- [ ] Background music (optional)

### Photogrammetry

- [ ] Smooth stone photo (processed)
- [ ] Dried leaf photo (processed)
- [ ] Fabric scrap photo (processed)

### Scenes & Prefabs

- [ ] Main scene with all three zones
- [ ] Player prefabs (The Bulk, The Spark)
- [ ] WindZone prefab
- [ ] Collectible prefabs (3 types)
- [ ] UI Canvas prefab

---

## Scope Decisions

### Included

✅ Local co-op (shared keyboard)
✅ 2D side-view with URP 2D renderer
✅ Three linear zones
✅ Safety Meter with full feedback
✅ Two asymmetric characters
✅ Three collectibles with two unlockable abilities
✅ Visual and audio "juice"
✅ Single-screen camera
✅ Photogrammetry integration

### Excluded (Out of Scope)

❌ Online multiplayer
❌ Gamepad support
❌ Save system / level select
❌ Multiple difficulty modes
❌ Branching paths or choice systems
❌ Tutorial overlays (learning through play)
❌ Achievements or scoring
❌ Accessibility options (colorblind mode, etc.)

---

## Key Design Questions & Recommendations

### 1. Camera Zoom Behavior

**Question:** Should camera zoom out indefinitely to fit separated players, or impose a maximum separation distance?

**Recommendation:** Set max camera zoom (e.g., orthographic size 15) and use soft screen-edge barriers. When a player approaches screen boundary, apply subtle force pushing them back toward center. This prevents accidental death-by-separation while maintaining tension.

### 2. Safety Meter Game Over

**Question:** What happens when Safety Meter reaches 0?

**Recommendation:**

- Fade screen to black over 1 second
- Display "Reconnect" prompt
- Give 3-second countdown timer
- If players don't close distance, respawn both at zone start
- This adds urgency without harsh instant-death punishment

### 3. Sync Breath Failure Penalty

**Question:** Should failing a sync breath prompt have consequences?

**Recommendation:** Yes. Failed sync breath increases Safety Meter drain rate by 1.5x until next successful sync. This creates pressure to succeed but doesn't instantly kill players. Adds to the "anxiety" theme.

### 4. Ability Design

**Question:** Should Root and Glide have cooldowns or be freely usable?

**Recommendation:**

- Root: Toggle ability (press to anchor, press again to release). No cooldown but requires deliberate positioning.
- Glide: Cooldown of 3-5 seconds. Prevents spam but allows creative navigation.

---

## Development Timeline Estimate

**Phase 1 (Foundation):** 4-6 hours

- Input setup, basic controllers, Safety Meter, camera

**Phase 2 (Core Mechanics):** 6-8 hours

- Sync breath, visual feedback, audio, huddle state

**Phase 3 (Abilities):** 4-6 hours

- Light system, wind zones, slipstream, collectibles

**Phase 4 (Level Design):** 6-8 hours

- Building three zones, placing objects, photogrammetry integration

**Phase 5 (Polish):** 4-6 hours

- Animations, audio mixing, UI improvements, testing

**Total Estimated Time:** 24-34 hours

For a game jam, prioritize Phases 1-3 first (core gameplay loop), then build one complete zone. Add remaining zones and polish if time permits.

---

## Success Metrics

The game is successful if:

1. ✅ Two players can complete all three zones in 10-15 minutes
2. ✅ Players must verbally communicate to succeed (dependency is real)
3. ✅ Safety Meter creates tension through distance separation
4. ✅ Sync breath mechanic creates moments of cooperation
5. ✅ Both characters feel essential (no "carry" dynamic)
6. ✅ Visual and audio feedback clearly communicate game state
7. ✅ Controls are responsive and intuitive on shared keyboard
8. ✅ Photogrammetry assets integrate aesthetically

---

## Next Steps

1. Start with Phase 1.1: Configure input system
2. Implement Phase 1.2-1.3 in parallel: character controllers and safety meter
3. Build Phase 1.4-1.5: UI and camera
4. Test complete foundation before moving to Phase 2
5. Proceed sequentially through phases, testing after each
6. Build Zone 1 first, then expand to Zones 2-3
7. Reserve final 20% of time for polish and bugfixing

---

**Good luck! Remember: "Safety First" is about dependency and cooperation, not combat. Make separation feel uncomfortable, and proximity feel safe. The game should teach players to value each other's presence.**
