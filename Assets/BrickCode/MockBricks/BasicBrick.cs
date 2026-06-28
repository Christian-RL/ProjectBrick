using UnityEngine;

public class BasicBrick : Brick
{
    public BasicBrick(string partId, string partName, Color partColour, int studWidth, int studLength, int tileHeight)
        : base(partId, partName, partColour, studWidth, studLength, tileHeight)
    {
    }
}