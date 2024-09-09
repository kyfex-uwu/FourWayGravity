local drawableSpriteStruct = require("structs.drawable_sprite")
local gravityArrow = {}
gravityArrow.name = "GravityHelperGV/GravityArrow"
gravityArrow.depth = 0
gravityArrow.placements = {
  {
    name = "arrow_left",
    data = {
      direction = "left"
    },
  },
  {
    name = "arrow_right",
    data = {
      direction = "right"
    },
  },
  {
    name = "arrow_up",
    data = {
      direction = "up"
    }
  },
  {
    name = "arrow_down",
    data = {
      direction = "down"
    },
  },
}
function gravityArrow.sprite(room, entity)
  -- entity.color = {1, 0, 0}
  if entity.direction == "left" then
    entity.rotation = 3.141 * 1.5
    -- entity.color = {0, 1, 0}
  end
  if entity.direction == "right" then
    entity.rotation = 3.141 / 2
    -- entity.color = {1, 1, 0}
  end
  if entity.direction == "down" then
    entity.rotation = 3.141
    -- entity.color = {0, 0, 1}
  end
  return drawableSpriteStruct.fromTexture("objects/GravityHelperGV/gravityArrow", entity)
end
return gravityArrow
