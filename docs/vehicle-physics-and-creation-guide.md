# Top Speed Vehicle Creation and Physics Guide

## Introduction

This guide explains how vehicles work in the current Top Speed rewrite and how to create custom vehicles using the new strict `.tsv` vehicle package format (TopSpeedVehicle). It is written for beginners, especially blind players and modders using screen readers, and it assumes only basic knowledge of acceleration, braking, and gears.

The main goal is practical understanding. You should be able to read this document, understand what the game is simulating, create a valid vehicle file, and tune the vehicle so it feels good in gameplay without needing advanced math or real-world engineering training.

Top Speed now uses a force-based driving model. The game calculates acceleration from engine torque, gearing, drivetrain efficiency, tire circumference, traction, drag, rolling resistance, and braking forces. Some parameter names are inherited from older versions of the game, but the custom vehicle format itself is now fully redesigned and strict. There is no backward compatibility with old vehicle files.

That means three important things for authors. First, all values are entered directly as real numbers, not encoded legacy numbers that get divided by 100. Second, every parameter must be inside a supported section. Third, invalid or extreme values are rejected with line-aware error messages instead of being silently accepted.

This guide is split into four parts. It starts with the physics model used by the game, then explains how to create a custom vehicle package and file, then explains the parser and validation rules, and finally gives a detailed parameter reference grouped by section with allowed value ranges and tuning advice.

## How Vehicle Physics Works in Top Speed

## The Big Picture

When a vehicle accelerates in Top Speed, the game is not simply adding a fixed speed amount. It builds acceleration from forces. The engine produces torque, the torque is multiplied by the current gear and final drive, that torque becomes wheel force, and the wheel force is limited by traction. After that, the game subtracts forces that resist motion, mainly rolling resistance and aerodynamic drag. The remaining force is converted into acceleration by dividing by vehicle mass.

This is why two vehicles with similar top speed caps can feel very different. If one vehicle is heavy, tall-geared, and has low torque at the RPM where it lands after a shift, it can feel lazy even if its `max_speed` is high. Another vehicle can feel very quick with a lower top speed if it has short gearing, stronger midrange torque, and lower mass.

The game also runs a separate steering and lateral movement model. Steering is influenced by `steering`, `max_steer_deg`, and `wheelbase`, while the actual sideways response is limited by grip and adjusted by `high_speed_stability`. This allows tuning high-speed cars to keep strong straight-line performance while reducing their cornering advantage.

Automatic transmission behavior is also part of the overall physics feel. The game now uses a transmission policy system. The policy decides shift timing, delays in certain gears, and anti-hunting behavior. Policy can improve shift decisions, but it cannot create engine power that is not there.

## Units Used by the Game

Most gameplay speed values are expressed in kilometers per hour. Physics calculations internally use SI-style units in many places, especially meters, seconds, kilograms, and Newton-meters.

As a vehicle author, you do not need to convert everything manually, but it is important to enter values in the expected units. Examples include `mass_kg` in kilograms, `peak_torque` in Newton-meters, `wheelbase` in meters, `max_steer_deg` in degrees, and `frontal_area` in square meters.

The new custom vehicle format does not use encoded legacy values. You write the actual value directly. For example, `max_speed=170` means 170 km/h. `steering=1.8` means 1.8, not 180. `surface_traction_factor=0.10` means 0.10, not 10.

## The Main Acceleration Flow (Plain Language)

The game uses a sequence of calculations each frame while throttle is applied.

The first step is determining the engine RPM for the current vehicle speed and gear. RPM depends on road speed, tire circumference, current gear ratio, and final drive ratio. At low speed under throttle, a launch RPM floor can help prevent the engine from dropping too low and feeling weak right off the line.

The next step is reading engine torque from the engine torque curve. The torque curve is built from four core values: `idle_torque`, `peak_torque`, `peak_torque_rpm`, and `redline_torque`, then combined with the RPM range between `idle_rpm` and `rev_limiter`.

That engine torque is multiplied by the current gear ratio and the `final_drive` ratio, then reduced by `drivetrain_efficiency`. This gives wheel torque. Wheel torque is then converted into wheel force using tire radius derived from tire circumference.

The wheel force is capped by grip. Even if the engine could theoretically push harder, the tires can only transmit so much force to the road. That is where `tire_grip`, surface behavior, and related handling values matter.

Finally, the game subtracts resistance forces. Rolling resistance acts all the time and is noticeable at lower speeds. Aerodynamic drag grows rapidly with speed and becomes dominant near top speed. The remaining force becomes acceleration after dividing by `mass_kg`.

The result is a system where low-speed acceleration, mid-speed pull, and high-speed pull can all be tuned differently using different parameters.

## RPM, Torque, and Why Some Gears Feel Stronger Than Others

## RPM

RPM is engine speed in revolutions per minute. RPM is not power by itself, but it controls where the engine is on the torque curve. If a shift drops RPM too far below the strong part of the curve, the next gear can feel weak even if the vehicle has a high torque peak on paper.

This is why RPM drop after an upshift is normal but must be usable. A good upshift causes RPM to drop and then recover. A bad upshift drops RPM into a weak zone and the vehicle feels flat or stalls in that gear.

## Torque

Torque is twisting force at the engine. In the game, torque becomes acceleration only after gearing and drivetrain efficiency are applied, and then after traction and resistance are considered.

If a vehicle feels too strong everywhere, reducing `peak_torque` can help, but it is not the only option. `power_factor`, gear ratios, final drive, and mass are often better gameplay-balance controls because they can shape the feel without destroying the engine character.

If a vehicle is fine in lower gears but too strong at high speed, lowering `redline_torque` or increasing drag is usually more targeted than lowering `peak_torque`.

## Torque Curve Shape and Shift Recovery

`idle_torque`, `peak_torque`, `peak_torque_rpm`, and `redline_torque` together define the shape of the engine curve.

Lower `peak_torque_rpm` generally makes a vehicle easier to pull in taller gears because the strong torque band starts earlier. Higher `peak_torque_rpm` makes the vehicle feel more high-rev and can punish early upshifts if the next gear lands too low.

Higher `redline_torque` makes the vehicle continue pulling strongly near the top of each gear. Lower `redline_torque` creates a more obvious fade near the rev limiter and can be used to calm high-gear acceleration without heavily changing launch.

## Gears, Final Drive, and Effective Ratio

Each forward gear has its own ratio. The game multiplies that ratio by `final_drive` to get the effective ratio for wheel torque and RPM mapping.

A higher effective ratio means stronger torque multiplication but more RPM at the same road speed. A lower effective ratio means lower RPM at the same road speed and less wheel torque.

Changing `final_drive` affects every forward gear at once. This makes it one of the strongest tuning controls in the game. A small `final_drive` increase can fix weak high gears, but it can also make lower gears too aggressive and cause the vehicle to reach the rev limiter earlier. A `final_drive` decrease can calm acceleration, but it can also make upper gears feel dead if the engine does not have enough torque at the resulting RPM.

Because of this, high-gear tuning is often a combination of final drive, torque curve shape, drag, and transmission policy.

## Drag and Rolling Resistance

Rolling resistance is a constant-like force that always fights motion. It is influenced by `rolling_resistance` and affects low, medium, and high speed, though it is usually most noticeable before aerodynamic drag becomes large.

Aerodynamic drag depends on `drag_coefficient` and `frontal_area`, and it grows much more rapidly with speed. This is why a vehicle may accelerate quickly up to a point and then slowly crawl toward top speed.

This behavior is often exactly what you want for game balance. If a vehicle is too strong near top speed, increasing drag or lowering `redline_torque` is usually a clean fix. If it feels weak at all speeds, look first at `power_factor`, `mass_kg`, and gearing before blaming drag.

## Braking and Coasting

Top Speed separates active braking from lift-off slowing.

Active braking happens when the player presses the brake. The result depends mainly on `brake_strength`, grip, and the current surface.

Lift-off slowing happens when the player releases throttle. That is engine braking, controlled by `engine_braking` and `engine_braking_torque`, along with RPM, gearing, and drivetrain efficiency.

If a vehicle feels like it slows too hard when the player stops accelerating, reduce engine braking values rather than brake strength. If the actual brake button feels weak, tune `brake_strength` instead.

## Steering, Grip, and Stability

Steering response in Top Speed is built from several parameters working together. `steering` acts like a steering strength multiplier. `max_steer_deg` limits the maximum steering angle. `wheelbase` changes how a steering angle turns into turning curvature. Grip parameters decide how much of the requested turn can actually happen, and `high_speed_stability` reduces responsiveness at speed to make fast vehicles calmer.

This means handling balance should not be tuned by one value alone. If a vehicle is unrealistically agile at high speed, lowering `max_steer_deg` or `steering` is often the first step. If it still corners too well, lower `lateral_grip` or increase `high_speed_stability`. If it loses traction too easily under power or braking, inspect `tire_grip`.

## Surface Behavior

The game applies surface-specific modifiers for asphalt, gravel, water, sand, and snow. Those modifiers interact with the vehicle baseline values.

The `surface_traction_factor` and `deceleration` parameters still exist in the format because they affect baseline behavior and some surface-related calculations, but they are not your main modern tuning tools for a vehicle's overall handling quality. In most cases, the most meaningful tuning for traction and cornering feel comes from `tire_grip`, `lateral_grip`, `brake_strength`, `engine_braking`, and the engine/drivetrain setup.

## Manual vs Automatic Transmission

Manual mode allows the player to shift whenever they choose. RPM dropping after an upshift is normal. The important question is whether the next gear still has enough pull to recover speed and continue accelerating.

Automatic mode now uses a policy-driven system. It looks at RPM, predicted acceleration in neighboring gears, shift cooldowns, and special rules for high gears and top-speed pursuit. This improves behavior in vehicles with many gears, especially 7-speed and 8-speed vehicles with overdrive gears.

Policy improves shift decisions, but it does not fix weak physics. If a gear cannot physically pull because torque, drag, or gearing are wrong, policy can only avoid that gear or delay entry into it.

## Creating a Custom Vehicle Package

## Folder Layout and Discovery

Custom vehicles are discovered from subfolders under the `Vehicles` folder, similar to track discovery. The game searches recursively and picks the first `.tsv` file it finds in each vehicle folder (sorted by file name). The file does not need to be named `vehicle.tsv`; any `.tsv` filename is valid.

A simple example structure might look like this:

```text
Vehicles/
  TouringSedan/
    touring_sedan.tsv
    engine.wav
    start.wav
    horn.wav
    crash1.wav
    crash2.wav
    brake.wav
    backfire1.wav
    backfire2.wav
```

The vehicle selection menu uses the custom vehicle metadata name from the file (`[meta] name`) for display. The `version` and `description` values are loaded and stored, but the menu item text is based on the name.

## Strict File Format Rules (Very Important)

The new custom vehicle format is strict by design. There is no backward compatibility mode for legacy vehicle files.

Every parameter must be inside a supported section. Top-level parameters are rejected. Unknown sections are rejected. Unknown keys are rejected. Duplicate sections are rejected. Duplicate keys inside a section are rejected.

Errors are line-aware and explain what is wrong, which makes debugging much easier for screen-reader users than silent fallback behavior.

The format uses `key=value` lines inside sections, and comments start with `;` or `#`.

## Required and Optional Sections

The following sections are supported by the parser.

`[meta]`, `[sounds]`, `[general]`, `[engine]`, `[drivetrain]`, `[gears]`, `[handling]`, `[dimensions]`, and `[tires]` are required. `[policy]` is optional.

If a required section is missing, the file fails to load and the vehicle is skipped from custom vehicle discovery.

## Example `.tsv` Vehicle File (Full Sectioned Format)

This example uses direct values, grouped sections, and an optional policy. It also demonstrates multi-sound lists for `crash` and `backfire`.

```ini
; Example custom vehicle package file for Top Speed
; File extension: .tsv (TopSpeedVehicle)

[meta]
name=Example Touring Sedan
version=1.0
description=Balanced front-engine touring sedan with usable 8-speed overdrive gears.

[sounds]
engine=builtin6
start=builtin1
horn=builtin4
throttle=
crash=builtin3,crash1.wav,crash2.wav
brake=brake.wav
backfire=backfire1.wav,backfire2.wav
idle_freq=9000
top_freq=42000
shift_freq=30000

[general]
surface_traction_factor=0.10
deceleration=0.40
max_speed=170
has_wipers=1

[engine]
idle_rpm=700
max_rpm=5600
rev_limiter=5000
auto_shift_rpm=4600
engine_braking=0.35
mass_kg=1500
drivetrain_efficiency=0.88
engine_braking_torque=220
peak_torque=260
peak_torque_rpm=3800
idle_torque=110
redline_torque=220
drag_coefficient=0.27
frontal_area=2.20
rolling_resistance=0.014
launch_rpm=1800
power_factor=0.64

[drivetrain]
final_drive=3.20
reverse_max_speed=35
reverse_power_factor=0.55
reverse_gear_ratio=3.20
brake_strength=1.00

[gears]
number_of_gears=8
gear_ratios=5.20,3.00,1.95,1.45,1.20,1.00,0.95,0.90

[handling]
steering=1.80
tire_grip=0.92
lateral_grip=1.00
high_speed_stability=0.15
wheelbase=2.80
max_steer_deg=32

[dimensions]
vehicle_width=1.84
vehicle_length=4.85

[tires]
tire_width=215
tire_aspect=55
tire_rim=17
; Or provide tire_circumference directly instead of width/aspect/rim.

[policy]
top_speed_gear=6
allow_overdrive_above_game_top_speed=true
auto_upshift_rpm_fraction=0.88
auto_downshift_rpm_fraction=0.35
base_auto_shift_cooldown=0.15
upshift_delay_default=0.15
upshift_delay_5_6=0.18
upshift_delay_6_7=0.24
upshift_delay_7_8=0.30
upshift_hysteresis=0.05
min_upshift_net_accel_mps2=-0.15
top_speed_pursuit_speed_fraction=0.97
prefer_intended_top_speed_gear_near_limit=true
```

## Sound Path Rules and Safety Rules

Custom vehicle sound paths are sandboxed to the custom vehicle folder. That means normal sound file paths must be relative paths inside the same folder as the `.tsv` file (or a subfolder under it). Paths that try to escape the folder, such as `..\\outside.wav`, are rejected. Absolute paths are also rejected for custom sound files.

The exception is built-in sound references using `builtinN`, such as `builtin1`, `builtin6`, and so on. Built-in references are allowed because they do not bypass the sandbox or access user file paths. They are useful for rapid prototyping and for authors who want to focus on physics first.

`crash` and `backfire` support comma-separated lists. All listed sounds are initialized when the vehicle loads, and the game randomizes among them at runtime when a crash or backfire event is played. This gives better variety without changing any physics behavior.

## Validation Behavior and Error Messages

The custom vehicle parser is intentionally strict because earlier versions of the game allowed unrealistic and extreme values that made vehicles impossible to balance. The new parser validates both structure and value ranges.

If a file has a mistake, the parser produces line-aware errors. For example, it can report that a key is unknown, that `gear_ratios` does not match `number_of_gears`, or that `max_speed` is outside the allowed range. This makes it much easier to fix configuration problems without guessing.

The parser also validates cross-parameter relationships. For example, `rev_limiter` must be between `idle_rpm` and `max_rpm`, and `peak_torque_rpm` must be between `idle_rpm` and `rev_limiter`. `shift_freq` must stay between `idle_freq` and `top_freq`. `gear_ratios` must be non-increasing from gear 1 to the last gear.

This strict behavior is not a restriction for its own sake. It exists to protect creators from invalid setups and to keep the game physics within reasonable, testable ranges.

## Practical Tuning Workflow for Beginners

The easiest way to build a good custom vehicle is to start from a clear gameplay role. Decide whether the vehicle should be a slow beginner car, a balanced sedan, a fast but hard-to-turn supercar, a van, or a bike-like high-rev vehicle. This decision helps you avoid creating a vehicle that is accidentally strong at everything.

Start by making the vehicle load and drive. Use built-in sounds if needed. Confirm it starts, moves, brakes, and shifts. After that, tune top speed and overall acceleration feel with `max_speed`, `power_factor`, mass, drag, and basic gearing. Then tune the torque curve for how it feels before and after shifts. After the physics feels correct, use `[policy]` to improve automatic shifting.

Always test manual and automatic modes separately. Manual testing tells you whether the powertrain can physically pull the gears. Automatic testing tells you whether the policy is making good choices.

When something feels wrong, change one major parameter at a time. If you change power, torque, gears, drag, and steering at once, it becomes very hard to know which change actually solved the problem.

## Parameter Reference (Grouped by Section)

## `[meta]` Section

The `[meta]` section describes the vehicle package for discovery and display. All three keys are required and must be non-empty text.

### `name`

This is the display name shown in the vehicle menu for the custom vehicle. Use a clear and human-friendly name because this is what players will hear through the menu system.

There is no numeric range because this is text, but it must not be empty.

### `version`

This is the package version string. It is not currently shown in the vehicle menu item text, but it is stored and available for future display or tooling. It is useful for managing updates to your vehicle package.

There is no numeric range because this is text, but it must not be empty.

### `description`

This is a longer text description of the vehicle package. It is not currently the menu label, but it is stored and is useful for documentation, future UI improvements, and collaboration.

There is no numeric range because this is text, but it must not be empty.

## `[sounds]` Section

This section controls vehicle audio assets and audio pitch behavior. It does not directly change acceleration, braking, or handling. However, good sound setup is important in Top Speed because the game is audio-heavy and relies on sound feedback for gameplay clarity.

### `engine`

Main engine sound. This is required. The value may be a relative path inside the vehicle folder or a built-in reference such as `builtin6`.

There is no numeric range because this is a sound reference. The path must resolve safely inside the vehicle folder unless it is a `builtinN` reference.

### `start`

Engine start sound. Required. Used when the vehicle is started.

There is no numeric range because this is a sound reference. The same path safety rules apply as `engine`.

### `horn`

Horn sound. Required. Used when the horn is triggered.

There is no numeric range because this is a sound reference. The same path safety rules apply as `engine`.

### `throttle`

Optional additional throttle layer sound. This can be left empty. When present, it adds audio richness but does not change physics.

There is no numeric range because this is a sound reference. If you provide a non-empty value, it must resolve successfully.

### `crash`

Crash sound or crash sound list. This key is required. You may provide a single sound or a comma-separated list. If you provide multiple sounds, all are initialized and one is chosen randomly at runtime for each crash event.

There is no numeric range because this is a sound reference or list of references. Each entry must be valid and must follow the same path safety rules. `builtinN` entries are allowed.

### `brake`

Brake sound. Required. Used for brake feedback and related tire/braking audio cues.

There is no numeric range because this is a sound reference. The same path safety rules apply as `engine`.

### `backfire`

Optional backfire sound or backfire sound list. If multiple entries are provided as a comma-separated list, the game randomizes among them when a backfire event is played.

There is no numeric range because this is a sound reference or list of references. If you provide entries, each must resolve successfully and follow path safety rules.

### `idle_freq`

Low engine audio pitch frequency anchor. This affects how the engine sound is pitched in low-speed and idle-like conditions.

Allowed range is 100 to 200000.

It does not change physics, but it strongly changes how slow or relaxed the vehicle sounds.

### `top_freq`

High engine audio pitch frequency anchor. This affects how the engine sound is pitched near the upper part of the RPM/speed range.

Allowed range is 100 to 200000, and it must be greater than or equal to `idle_freq`.

It does not change physics, but it strongly affects the perceived character of the engine.

### `shift_freq`

Intermediate audio frequency anchor used in engine pitch/shift-related sound behavior.

Allowed range is 100 to 200000, and it must be between `idle_freq` and `top_freq`.

It does not change acceleration or top speed, but poor values can make the vehicle sound inconsistent or unnatural.

## `[general]` Section

The `[general]` section contains general gameplay-facing values that do not fit cleanly into the engine or handling groups.

### `surface_traction_factor`

Baseline surface traction factor used by parts of the surface interaction logic.

Allowed range is 0.0 to 5.0.

This value is entered directly. Do not multiply by 100. For example, use `0.10`, not `10`.

In the current physics model this is not usually the strongest tuning lever for everyday grip feel. For meaningful traction and handling changes, `tire_grip` and `lateral_grip` are usually more important.

### `deceleration`

Baseline deceleration factor used by some surface-related behavior and legacy-style deceleration baselines.

Allowed range is 0.0 to 5.0.

This value is entered directly. Do not multiply by 100. For example, use `0.40`, not `40`.

If you need to change braking feel, tune `brake_strength`. If you need to change lift-off slowing, tune `engine_braking` and `engine_braking_torque`.

### `max_speed`

Forward speed cap in km/h for gameplay.

Allowed range is 10 to 500.

This is a hard cap. Even if the powertrain could continue accelerating, the vehicle speed is clamped. Because this cap interacts with automatic transmission policy, it is often paired with `top_speed_gear` and overdrive settings in `[policy]`.

### `has_wipers`

Boolean-like flag for wiper behavior and related weather audio behavior.

This key is parsed as a boolean integer (`0` or `1`).

There is no numeric tuning range beyond that. It does not affect physics.

## `[engine]` Section

The `[engine]` section contains engine RPM limits, torque curve shape, engine braking, mass, resistance values used in acceleration calculations, and overall power scaling.

### `idle_rpm`

Engine idle RPM baseline.

Allowed range is 300 to 3000.

This value affects the low end of the RPM range, torque curve calculations, and how RPM-based policy fractions convert into absolute RPM. Raising it changes the meaning of some policy thresholds and can change low-speed feel.

### `max_rpm`

Maximum RPM ceiling used by the engine model for RPM handling and reporting.

Allowed range is 1000 to 20000, and it must be greater than or equal to `idle_rpm`.

This is not the same as `rev_limiter`. `max_rpm` is the overall ceiling, while `rev_limiter` is the main usable limit for power and shifting behavior.

### `rev_limiter`

Usable upper RPM limit for engine power and gear pulling.

Allowed range is 800 to 18000, and it must be between `idle_rpm` and `max_rpm`.

Lower values shorten each gear and can make high gears easier to reach but may reduce flexibility. Higher values extend each gear, but only help if the torque curve stays strong at high RPM.

### `auto_shift_rpm`

Preferred automatic shift RPM anchor.

Allowed range is 0 to 18000. It must be `0` or between `idle_rpm` and `rev_limiter`.

A value of `0` means policy/default logic derives behavior from other values. A real value gives a direct automatic shift target and is also used when policy derives defaults.

### `engine_braking`

Engine braking strength multiplier.

Allowed range is 0.0 to 1.5.

This affects lift-off deceleration, not active brake-button stopping. Too high can make the vehicle feel like it drags unnaturally when the player releases throttle.

### `mass_kg`

Vehicle mass in kilograms.

Allowed range is 20 to 10000.

Higher mass reduces acceleration for the same net force and usually makes the vehicle feel calmer but heavier. Lower mass increases responsiveness and acceleration. Mass also influences some handling feel indirectly.

### `drivetrain_efficiency`

Drivetrain efficiency multiplier representing power loss through the drivetrain.

Allowed range is 0.1 to 1.0.

Higher values send more torque to the wheels. Lower values reduce acceleration and engine braking transfer. This is useful but usually not the first tuning lever for gameplay balance.

### `engine_braking_torque`

Base engine braking torque in Newton-meters.

Allowed range is 0 to 3000.

This works together with `engine_braking` to determine lift-off slowing. If off-throttle slowing is wrong, tune these two together.

### `peak_torque`

Peak engine torque in Newton-meters.

Allowed range is 10 to 3000.

This is one of the main acceleration parameters. Larger values usually increase acceleration across much of the speed range, especially where the engine spends time near `peak_torque_rpm`.

### `peak_torque_rpm`

RPM where peak torque occurs.

Allowed range is 500 to 18000, and it must be between `idle_rpm` and `rev_limiter`.

Lower values improve midrange and recovery after upshifts. Higher values create a more high-rev character and can make tall gears harder to pull if shifts land too low.

### `idle_torque`

Torque near idle RPM.

Allowed range is 0 to 3000.

Higher values improve launch and low-RPM response. Too high can make the vehicle unrealistically strong in tall gears at low RPM.

### `redline_torque`

Torque near the rev limiter.

Allowed range is 0 to 3000.

This is one of the best targeted controls for high-gear pull. Lower it to calm upper-gear acceleration without heavily affecting low-speed launch. Raise it to let the engine continue pulling harder at high RPM.

### `drag_coefficient`

Aerodynamic drag coefficient used in the drag force calculation.

Allowed range is 0.01 to 1.5.

Lower values improve high-speed pull and top-speed reach. Higher values reduce high-speed acceleration and can be used to balance fast vehicles while keeping low-speed behavior more intact than a large torque reduction would.

### `frontal_area`

Frontal area in square meters used in the drag calculation.

Allowed range is 0.05 to 10.0.

This works together with `drag_coefficient`. Larger values increase aerodynamic drag, especially at higher speeds.

### `rolling_resistance`

Rolling resistance coefficient.

Allowed range is 0.001 to 0.1.

This affects resistance across the speed range and is especially noticeable at low and medium speed. If a vehicle feels weak everywhere, inspect this along with mass and power settings.

### `launch_rpm`

Launch RPM assist floor under throttle at low speed.

Allowed range is 0 to 18000, and it must not exceed `rev_limiter`.

Higher values can make launch feel stronger and reduce bogging. Lower values can calm launches.

### `power_factor`

Global power scaling multiplier for throttle-driven acceleration calculations.

Allowed range is 0.05 to 2.0.

This is one of the best gameplay-balance controls in the entire format. It lets you adjust acceleration without fully rebuilding the torque curve. If a vehicle is too dominant, lowering `power_factor` is often the cleanest first step.

## `[drivetrain]` Section

The `[drivetrain]` section contains gearing and braking controls that are not part of the engine torque curve itself.

### `final_drive`

Final drive ratio applied to all forward gears.

Allowed range is 0.3 to 8.0.

This is one of the strongest tuning controls because it changes effective gearing in every forward gear. Increasing it makes all gears shorter and usually improves pull. Decreasing it makes all gears taller and can reduce acceleration or make upper gears harder to use.

### `reverse_max_speed`

Maximum reverse speed in km/h.

Allowed range is 1 to 100.

The game clamps reverse speed separately from forward speed. This gives you direct control over how fast the vehicle can back up.

### `reverse_power_factor`

Reverse acceleration scaling multiplier.

Allowed range is 0.05 to 2.0.

This is a gameplay tuning value. Most vehicles should use a lower reverse power value than forward power to keep reverse behavior controllable.

### `reverse_gear_ratio`

Reverse gear ratio.

Allowed range is 0.5 to 8.0.

This affects reverse RPM and reverse torque multiplication together with `final_drive` and `reverse_power_factor`.

### `brake_strength`

Active braking strength multiplier used when the brake input is pressed.

Allowed range is 0.1 to 5.0.

This works with grip and surface behavior. If brake input feels weak, increase this. If braking is too harsh or difficult to control, reduce it.

## `[gears]` Section

This section defines the number of forward gears and the exact gear ratio list. Both keys are required.

### `number_of_gears`

Number of forward gears.

Allowed range is 1 to 10.

This value must match the number of entries in `gear_ratios`. The parser hard-fails if they do not match.

### `gear_ratios`

Comma-separated list of forward gear ratios, one value per gear from 1st to last.

This key is required. There is no fallback or auto-generation in the custom vehicle format.

Each individual ratio must be between 0.20 and 8.00. The list must be non-increasing, which means each later gear must be the same or lower ratio than the previous one.

Higher values make shorter gears with stronger torque multiplication. Lower values make taller gears with less pull and lower RPM at the same speed. Gear ratios always interact with `final_drive`, so tune them together.

## `[handling]` Section

The `[handling]` section controls steering response, grip, and high-speed turning stability.

### `steering`

Steering strength multiplier.

Allowed range is 0.1 to 5.0.

This is an active and important handling control. It scales steering response before angle and curvature limits are applied. Use direct float values such as `1.80`. Do not use old integer-style scaled values.

### `tire_grip`

Base tire grip coefficient for traction and braking, with influence on overall grip behavior.

Allowed range is 0.1 to 3.0.

Increasing this can improve acceleration traction and braking grip, but too much can make the vehicle unrealistically planted and easy to drive.

### `lateral_grip`

Additional lateral grip tuning for turning behavior.

Allowed range is 0.1 to 3.0.

This is a good tool for cornering balance when you do not want to change forward traction as much as changing `tire_grip` would.

### `high_speed_stability`

High-speed steering response damping factor.

Allowed range is 0.0 to 1.0.

Higher values calm the vehicle at speed and reduce twitchiness. Too high can make the vehicle feel reluctant to turn.

### `wheelbase`

Wheelbase in meters.

Allowed range is 0.5 to 8.0.

Shorter wheelbase usually feels more responsive and more willing to rotate. Longer wheelbase usually feels calmer and less agile. It interacts strongly with `steering` and `max_steer_deg`.

### `max_steer_deg`

Maximum steering angle in degrees.

Allowed range is 5 to 60.

This is one of the most direct handling controls. Lower values are useful for limiting the agility of high-speed vehicles. Higher values improve maneuverability but can make the vehicle unstable if grip and stability are too high.

## `[dimensions]` Section

The `[dimensions]` section defines physical size values used for spatial behavior and representation.

### `vehicle_width`

Vehicle width in meters.

Allowed range is 0.2 to 5.0.

This affects spatial/audio placement and vehicle size behavior, not engine power or top speed directly.

### `vehicle_length`

Vehicle length in meters.

Allowed range is 0.3 to 20.0.

This also affects spatial representation and presence rather than direct acceleration physics.

## `[tires]` Section

The `[tires]` section defines tire circumference directly or provides the size triplet needed to calculate it.

You must provide either a valid `tire_circumference` or all three of `tire_width`, `tire_aspect`, and `tire_rim`. The parser hard-fails if neither form is complete and valid.

### `tire_circumference`

Tire circumference in meters.

If provided and greater than zero, it is used directly. Allowed range is 0.2 to 5.0 meters.

This value affects the speed-to-RPM relationship. Incorrect tire circumference can make gearing and RPM behavior feel wrong even when other parameters are correct.

### `tire_width`

Tire width in millimeters used for circumference calculation when direct circumference is not provided.

Allowed range is 20 to 450.

### `tire_aspect`

Tire aspect ratio (sidewall height percentage of tire width) used for circumference calculation fallback.

Allowed range is 5 to 150.

### `tire_rim`

Rim diameter in inches used for circumference calculation fallback.

Allowed range is 4 to 30.

If you provide the size triplet, the game calculates tire circumference automatically after validation.

## `[policy]` Section (Optional, Automatic Transmission Only)

The `[policy]` section controls automatic shifting behavior. It does not change engine power, grip, or drag directly. Policy should be used after the vehicle can physically pull the gears you intend it to use.

The parser accepts normal policy keys and also wildcard delay keys such as `upshift_delay_6_7` and `upshift_delay_g6`.

### `top_speed_gear`

Intended forward gear for reaching the game-world top speed.

Allowed range is 1 to `number_of_gears`.

This is especially important for 7-speed and 8-speed vehicles with overdrive gears. It tells automatic mode which gear should usually be treated as the main top-speed gear.

### `allow_overdrive_above_game_top_speed`

Boolean policy flag for allowing gears above `top_speed_gear` near or above the top-speed region.

This key has no numeric range because it is boolean. Use `true` or `false`.

If `true`, automatic mode can use higher gears as overdrives when appropriate. If `false`, it avoids them while pursuing top speed.

### `base_auto_shift_cooldown`

Base automatic shift cooldown in seconds.

Allowed range is 0.0 to 2.0.

Higher values make automatic mode calmer and less likely to shift rapidly. Too high can make the transmission feel lazy.

### `upshift_delay_default`

Default automatic upshift delay in seconds.

Allowed range is 0.0 to 2.0.

This is a useful way to make upshifts slower in general before adding special delays for high gears.

### `upshift_delay_X_Y`

Per-transition upshift delay in seconds for a specific adjacent upshift.

Allowed range is 0.0 to 2.0.

Use keys such as `upshift_delay_5_6=0.18` or `upshift_delay_6_7=0.24`. This is the best way to make high-gear upshifts slower without affecting lower gears.

### `upshift_delay_gX`

Per-source-gear shorthand upshift delay in seconds.

Allowed range is 0.0 to 2.0.

For example, `upshift_delay_g6=0.24` applies to upshifts out of 6th gear. Explicit transition keys are more specific and should be preferred when you need exact behavior.

### `auto_upshift_rpm_fraction`

Automatic upshift threshold as a fraction of the RPM span from idle to rev limiter.

Allowed range is 0.05 to 1.0.

This is convenient because it stays meaningful even if you later retune `idle_rpm` or `rev_limiter`.

### `auto_upshift_rpm`

Automatic upshift threshold in absolute RPM.

Allowed range is 0 to the vehicle RPM limits used by validation. In practice it must be `0` or between `idle_rpm` and `rev_limiter`.

This overrides the fraction-style threshold when present.

### `auto_downshift_rpm_fraction`

Automatic downshift threshold as a fraction of the RPM span from idle to rev limiter.

Allowed range is 0.05 to 0.95.

Higher values make the transmission more eager to downshift. Lower values make it hold higher gears longer.

### `auto_downshift_rpm`

Automatic downshift threshold in absolute RPM.

Allowed range is 0 or a valid RPM between `idle_rpm` and `rev_limiter`.

This overrides the fraction-style downshift threshold when present.

### `top_speed_pursuit_speed_fraction`

Threshold for when the automatic logic considers the vehicle near the top-speed region, expressed as a fraction of game `max_speed`.

Allowed range is 0.50 to 1.20.

Values below `1.0` let the policy begin top-speed behavior before the vehicle reaches the cap. This is useful for stable high-gear behavior near terminal speed.

### `upshift_hysteresis`

Extra hysteresis for automatic upshift decisions.

Allowed range is 0.0 to 2.0.

Higher values reduce gear hunting but can delay good shifts. Lower values make shifting more responsive but can increase oscillation.

### `min_upshift_net_accel_mps2`

Minimum acceptable net acceleration in the next gear before an upshift is allowed, unless top-speed pursuit logic decides otherwise.

Allowed range is -20.0 to 20.0.

This is an important anti-stall protection for high gears. It helps prevent automatic mode from shifting into a gear that would immediately decelerate or feel dead.

### `prefer_intended_top_speed_gear_near_limit`

Boolean policy flag for preferring the intended top-speed gear near the speed limit region.

This key has no numeric range because it is boolean. Use `true` or `false`.

When enabled, automatic mode tries to stay in or below the intended top-speed gear until it is appropriate to use overdrives.

## What Was Removed From the Old Format

The new custom format removes several legacy behaviors on purpose.

`mono_crash` sound support is removed and is not a valid parameter. `steering_factor` is also removed and is not supported. If either appears in a custom `.tsv` file, the parser will reject the file as unknown-key input.

The old divide-by-100 convention is also removed. Do not write encoded values such as `17000` for 170 km/h or `180` for `steering=1.8`. Use direct values everywhere.

Top-level parameters are no longer supported. Every key must be inside a valid section.

## Tuning Advice and Common Problems

If a vehicle is too fast in every gear, lowering only `max_speed` will not fix the real problem. The vehicle will still accelerate too hard and just hit the cap earlier. In that case, reduce `power_factor`, reduce torque values, increase mass, adjust gearing, or increase drag depending on which part of the speed range is too strong.

If a vehicle feels fine at launch but weak after an upshift, inspect where the new gear lands on the torque curve. Lowering `peak_torque_rpm`, increasing `idle_torque`, shortening gearing, increasing final drive, or reducing drag can all improve shift recovery. Policy can help avoid bad automatic shifts, but it cannot make a weak gear physically stronger.

If a vehicle turns too well for its class, reduce `max_steer_deg` or `steering` first. If it is still too strong in corners, reduce `lateral_grip` or increase `high_speed_stability`. If it becomes hard to control under braking or acceleration, revisit `tire_grip` and braking values.

If automatic mode feels worse than manual mode, the physics may already be good. In that case, tune `[policy]` instead of rebuilding the engine and gears. Use `top_speed_gear`, `upshift_delay_*`, and acceleration-protection policy values to stabilize behavior.

## Final Notes for Authors

The new `.tsv` format is strict because strictness improves quality, debugging, and fairness. It prevents accidental bad values, catches mistakes early, and makes custom vehicles much easier to maintain.

The best results come from clear goals and methodical tuning. Decide the vehicle role first, make sure the powertrain can physically pull the intended gears, then refine balance with `power_factor`, drag, handling limits, and automatic transmission policy. Test in manual and automatic modes, and pay close attention to what happens immediately after each shift. That moment usually reveals whether your tune is healthy.
