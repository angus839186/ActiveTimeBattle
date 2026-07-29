# GameFlow Notes

## RunSession
Stores per-run data:
- selected class
- seed
- explore map
- current explore room id
- completed rooms
- completed nodes
- pending battle room/node
- explore return position

## ExploreMapSpawner
Applies ExploreMapData to the current room shell:
- selects room data
- updates current room id
- initializes RoomController
- enables the matching ExploreEventNode
- moves player by door entry point or battle return position

## RoomController
Owns one room shell:
- room id
- doors
- entry points
- completed/open state

## RoomDoorTransition
Detects player entering an opened door and asks ExploreMapSpawner to apply the connected room.

## ExploreEventNode
Base class for interactable room events. Child nodes decide what happens on interaction.