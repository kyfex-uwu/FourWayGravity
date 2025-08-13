local gravityField = {}

gravityField.name = "FourWayGravity/ChangeGravityTrigger"
gravityField.placements = {
    name = "main",
    data = {
        width = 8,
        height = 8,
        direction="down"
    }
}
gravityField.fieldInformation = {
    direction = {
        options={
            "up",
            "down",
            "left",
            "right"
        },
        editable=false,    
    }
}
gravityField.triggerText = function(room, self)
    return string.format("set gravity to %s", self.direction)
end

gravityField.depth = 1000

return gravityField
