# Door + Player Animator Setup

## 1) Door (Trigger in Animator)
1. Create a door object with `Animator`, `BoxCollider` (door body), and `DoorController`.
2. Add a child object `DoorTriggerZone` with `BoxCollider` (`Is Trigger = true`) and set script object to receive trigger events (either on parent with trigger collider or copy script to trigger object).
3. Set player tag to `Player`.
4. In door Animator create:
- State `Closed` (default)
- State `Open`
- Parameter `DoorTrigger` (Trigger)
5. Transitions:
- `Closed -> Open` with condition `DoorTrigger`.
- (Optional) `Open -> Closed` by Exit Time or separate trigger.
6. In `DoorController` set `Open Trigger Name = DoorTrigger`, cooldown and interaction distance.

## 2) Player Idle / Run / Jump
1. Add `Rigidbody`, `CapsuleCollider`, `Animator`, `SimplePlayerController` to player.
2. Animator parameters:
- `Speed` (Float)
- `Jump` (Bool)
3. States and transitions:
- `Idle` (default)
- `Run`
- `Jump`
- `Idle -> Run` if `Speed > 0.1`
- `Run -> Idle` if `Speed <= 0.1`
- `Any State -> Jump` if `Jump == true`
- `Jump -> Idle` if `Jump == false` and `Speed <= 0.1`
- `Jump -> Run` if `Jump == false` and `Speed > 0.1`

## 3) Why Trigger and not Collision
- `Trigger` is best for intent/actions (enter zone, press E), no physical pushback.
- `Collision` is best for physical contact response (impulse, blocking, friction).
- For interactive doors we usually combine both:
  - Trigger zone for "can interact".
  - Non-trigger collider for real blocking geometry.

## 4) Cooldown logic
`DoorController` stores `nextInteractTime` and ignores input while `Time.time < nextInteractTime`.
This prevents spam-triggering and animation glitches from repeated key presses.
