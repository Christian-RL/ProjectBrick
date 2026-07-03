using UnityEngine;
using BrickCode;
/**
 * Spawns in basic brick for visual testing
 */
public class BrickTestSpawner : MonoBehaviour
{
    private void Start()
    {
        Brick brick = new BasicBrick(
            "3003",
            "2x2 Brick",
            Color.red,
            2,
            2,
            3
        );

        BrickVisual visual = gameObject.AddComponent<BrickVisual>();
        visual.BuildFromBrick(brick);
    }
}