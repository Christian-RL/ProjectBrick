using UnityEngine;
using System.Collections.Generic;

/**
 * Abstract class representing common brick attributes for simple System bricks.
 */
public abstract class Brick
{
    private string PartId { get; } //The ID of the brick
    private string PartName { get; } //The common part name of the brick
    private Color PartColour { get; set; } //The colour of the brick
    
    private Stud[,] BrickStuds { get; set; } //2D array of top studs
    private int StudWidth { get; } //Represents the width measured in studs
    private int StudLength { get; } //Represents the length measured in studs
    private int TileHeight { get; } //Represents the height measures in tiles


    protected Brick(string partId, string partName, Color partColour, int StudWidth, int StudLength, int TileHeight)
    {
        this.PartId = partId;
        this.PartName = partName;
        this.PartColour = partColour;
        this.StudWidth = StudWidth;
        this.StudLength = StudLength;
        this.TileHeight = TileHeight;
        BrickStuds = new Stud[StudWidth, StudLength];
        InitialiseStuds();
    }
    
    private void InitialiseStuds()
    {
        for (var x = 0; x < StudWidth; x++)
        {
            for (var y = 0; y < StudLength; y++)
            {
                BrickStuds[x, y] = new Stud(this);
            }
        }
    }

   
    public virtual string GetDescription()
    {
        return $"{PartColour} {PartName} ({StudWidth}x{StudLength}x{TileHeight})";
    }

    public virtual string GetPartID()
    {
        return PartId;
    }

    public virtual string GetName()
    {
        return PartName;
    }
    
    public virtual Color GetColour()
    {
        return PartColour;
    }
    
    public virtual int GetStudWidth()
    {
        return StudWidth;
    }
    
    public virtual int GetStudLength()
    {
        return StudLength;
    }

    public virtual int GetTileHeight()
    {
        return TileHeight;
    }

    public virtual Stud[,] GetStuds()
    {
        return BrickStuds;
    }

    public virtual Stud GetStud(int x, int y)
    {
        return BrickStuds[x, y];
    }
}

