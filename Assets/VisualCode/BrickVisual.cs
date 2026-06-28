using UnityEngine;

public class BrickVisual : MonoBehaviour
{
    private const float StudSpacing = 1.0f;
    private const float BrickHeightUnit = 0.4f;
    private const float StudHeight = 0.15f;
    private const float StudDiameter = 0.6f;

    public void BuildFromBrick(Brick brick)
    {
        CreateBrickBody(brick);
        CreateStuds(brick);
    }

    private void CreateBrickBody(Brick brick)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Brick Body";
        body.transform.SetParent(transform);

        float width = brick.GetStudWidth() * StudSpacing;
        float length = brick.GetStudLength() * StudSpacing;
        float height = brick.GetTileHeight() * BrickHeightUnit;

        body.transform.localScale = new Vector3(width, height, length);
        body.transform.localPosition = new Vector3(0, height / 2f, 0);

        SetColour(body, brick.GetColour());
    }

    private void CreateStuds(Brick brick)
    {
        int width = brick.GetStudWidth();
        int length = brick.GetStudLength();

        float bodyHeight = brick.GetTileHeight() * BrickHeightUnit;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                GameObject stud = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stud.name = $"Stud {x},{z}";
                stud.transform.SetParent(transform);

                stud.transform.localScale = new Vector3(
                    StudDiameter,
                    StudHeight / 2f,
                    StudDiameter
                );

                float xPos = x - (width - 1) / 2f;
                float zPos = z - (length - 1) / 2f;

                stud.transform.localPosition = new Vector3(
                    xPos * StudSpacing,
                    bodyHeight + StudHeight / 2f,
                    zPos * StudSpacing
                );

                SetColour(stud, brick.GetColour());
            }
        }
    }

    private void SetColour(GameObject obj, Color colour)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.material.color = colour;
    }
}