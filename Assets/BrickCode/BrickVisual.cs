using UnityEngine;

namespace BrickCode
{
    /**
     * Creates visual for unity of basic rectangular brick.
     */
    public class BrickVisual : MonoBehaviour
    {
        private const float StudDiameter = 0.6f;

        /**
         * Generate visible brick from logical brick.
         */
        public void BuildFromBrick(Brick brick)
        {
            BrickObjectData data = GetComponent<BrickObjectData>();
            if (!data) data = gameObject.AddComponent<BrickObjectData>();
            data.Initialise(brick);
            data.ClearStuds();
            CreateBrickBody(brick, data);
            CreateStuds(brick, data);
        }

        /**
         * Create rectangular base/body of the brick.
         */
        private void CreateBrickBody(Brick brick, BrickObjectData data)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Brick Body";
            body.transform.SetParent(transform, false);
            float width = brick.GetStudWidth() * BrickObjectData.StudSpacing;
            float length = brick.GetStudLength() * BrickObjectData.StudSpacing;
            float height = brick.GetTileHeight() * BrickObjectData.BrickHeightUnit;
            body.transform.localPosition = new Vector3(0f, height / 2f, 0f);
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = new Vector3(width, height, length);
            data.SetBody(body);
            SetColour(body, brick.GetColour());
        }

        /**
         * Create round studs on top of the brick.
         */
        private void CreateStuds(Brick brick, BrickObjectData data)
        {
            int width = brick.GetStudWidth();
            int length = brick.GetStudLength();
            float bodyHeight = brick.GetTileHeight() * BrickObjectData.BrickHeightUnit;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    GameObject stud = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stud.name = $"Stud {x},{z}";
                    stud.transform.SetParent(transform, false);
                    stud.transform.localRotation = Quaternion.identity;
                    stud.transform.localScale = new Vector3(
                        StudDiameter,
                        BrickObjectData.StudHeight / 2f,
                        StudDiameter
                    );
                    float xPos = x - (width - 1) / 2f;
                    float zPos = z - (length - 1) / 2f;
                    stud.transform.localPosition = new Vector3(
                        xPos * BrickObjectData.StudSpacing,
                        bodyHeight + BrickObjectData.StudHeight / 2f,
                        zPos * BrickObjectData.StudSpacing
                    );
                    Collider studCollider = stud.GetComponent<Collider>();
                    if (!studCollider) Destroy(studCollider);
                    data.RegisterStud(stud.transform);
                    SetColour(stud, brick.GetColour());
                }
            }
        }

        /**
         * Change material colour.
         */
        private void SetColour(GameObject obj, Color colour)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            renderer.material.color = colour;
        }
    }
}