using UnityEngine;

/**
 * Abstract class representing common brick attributes for simple System bricks.
 */
public abstract class Brick
{
    public string PartId { get; } //The ID of the brick
    public string PartName { get; } //The common part name of the brick
    public Color PartColour { get; set; } //The colour of the brick

    protected Brick(string partId, string partName, Color partColour)
    {
        this.PartId = partId;
        this.PartName = partName;
        this.PartColour = partColour;
    }

    public abstract int StudWidth { get; } //Represents the width measured in studs
    public abstract int StudLength { get; } //Represents the length measured in studs
    public abstract int TileHeight { get; } //Represents the height measures in tiles
    
    public virtual string GetDescription()
    {
        return $"{PartColour} {PartName} ({StudWidth}x{StudLength}x{TileHeight})";
    }
}

