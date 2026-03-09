# GDD: SYNC & SHELTER

**Jam Theme:** Safety First / Beyond Stereotypes

**Diversifiers:** Creature Feature, Photograbetry

**Genre:** Cooperative "Empathy" Puzzler

**Platform:** PC (Local Co-op / Shared Keyboard)

---

## 1. THE CORE HOOK: "THE SAFETY TETHER"

The game is played by two people controlling two non-human creatures. Their survival depends not on "defeating" an enemy, but on **maintaining a shared state of calm.** \* **The Verb:** **Huddle.**

- **The Conflict:** Separation Anxiety. Environmental "static" (wind, shadows, noise) drains a shared **Safety Meter** if players are too far apart.
- **The Resolution:** Physical proximity and synchronized "Check-ins" (button presses) restore the meter and light up the world.

---

## 2. THE CHARACTERS (Creature Feature)

We subvert the "Tank/DPS" stereotype by making both roles equally vulnerable and essential.

| Character     | Visual (Anime Style)                | Role/Function                                                                                            |
| ------------- | ----------------------------------- | -------------------------------------------------------------------------------------------------------- |
| **THE BULK**  | Large, moss-covered Capybara-Golem. | **The Shield.** Immune to wind/physical pushback. Moves slowly. Cannot see in the dark.                  |
| **THE SPARK** | Tiny, iridescent Paper-Moth.        | **The Lantern.** Provides a circle of light. Moves fast. Gets blown away by wind if not behind The Bulk. |

---

## 3. THE "SAFETY FIRST" MECHANICS (MDA)

- **Mechanics:** \* **Distance Logic:** If `Distance > 5m`, the screen begins to desaturate and a "Heartbeat" sound ramps up.
- **The Huddle:** When characters overlap, they enter a "Safe State."
- **Synchronized Breath:** A prompt appears every 60 seconds. Both players must hold a specific key for 2 seconds to "Ground" themselves, clearing the screen of static.

- **Dynamics:** Players must constantly talk: _"Wait for me," "Block the wind," "I can't see, move closer."_
- **Aesthetics:** 2D Anime-styled characters layered over high-contrast, "crunchy" Photograbetry backgrounds.

---

## 4. PHOTOGRABETRY INTEGRATION (The Relics)

You will find 3 real-world objects in the level. These are the "Magic Books" of your original idea, now repurposed as **Grounding Totems**.

1. **The Smooth Stone:** (Found Object). Reaching this unlocks the ability to "Root" (The Bulk becomes unmovable).
2. **The Dried Leaf:** (Found Object). Reaching this unlocks "Glide" for the Spark.
3. **The Fabric Scrap:** (Found Object). The final goal. Representing "Comfort."

---

## 5. TECHNICAL SPECS (Unity/C#)

- **Input:** \* **P1 (Bulk):** WASD + Space (Check-in).
- **P2 (Spark):** Arrow Keys + Enter (Check-in).

- **Camera:** Single "Proximity Camera" that keeps both players in frame.
- **Asset Pipeline:** \* Hand-draw sprites in your anime style (Transparent PNGs).
- Photos of your objects processed through a "Posterize" filter in Photoshop/GIMP to match the anime aesthetic.

---

## 6. THE "JUICE" CHECKLIST (Low Cost / High Impact)

- [ ] **Dynamic Vignette:** As the Safety Meter drops, the edges of the screen crawl with dark, hand-drawn "anxiety" scribbles.
- [ ] **Particle Burst:** When "Sync Breath" is successful, trigger a burst of anime-style petals or light orbs.
- [ ] **Audio Cues:** A low-thumping heartbeat that gets faster as players separate.

---

## 7. LEVEL FLOW (The 30-Minute Build)

- **Zone 1:** Learning to walk together. Introduce the Distance Penalty.
- **Zone 2:** The Wind Corridor. The Bulk must lead; the Spark must stay in the "Slipstream" (trigger area behind Bulk).
- **Zone 3:** The Dark Maze. The Spark must lead; the Bulk follows the light.
- **Ending:** Both reach the **Fabric Scrap** and perform one final, long "Sync Breath."