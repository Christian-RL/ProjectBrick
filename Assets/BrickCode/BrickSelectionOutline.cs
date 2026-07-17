using UnityEngine;

namespace BrickCode
{
    /**
     * Draws the yellow outline box around a selected brick.
     * Draws a LineRenderer object for each edge of a box.
     */
    public class BrickSelectionOutline : MonoBehaviour
    {
        [SerializeField] private Color outlineColour = Color.yellow;
        [SerializeField] private float lineWidth = 0.035f;

        private GameObject _outlineRoot; //parent gameobject that holds all the outline objects
        private LineRenderer[] _lines; //stores the 12 linerenderer components

        /**
         * Called to turn outline on or off.
         */
        public void SetHighlighted(bool highlighted)
        {
            EnsureCreated();
            _outlineRoot.SetActive(highlighted);
            enabled = highlighted;
        }

        /**
         * Redraws the outline if the parent object is moved.
         * Called once per frame after normal update calls.
         */
        private void LateUpdate()
        {
            if (!_outlineRoot || !_outlineRoot.activeSelf) return;
            UpdateOutline();
        }

        /**
         * Creates the outline objects the first time they are needed.
         * If the outline already exists it does nothing
         */
        private void EnsureCreated()
        {
            if (_outlineRoot) return;
            _outlineRoot = new GameObject("Selection Outline");
            _outlineRoot.transform.SetParent(transform, false);
            _lines = new LineRenderer[12];
            Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.color = outlineColour;
            for (int i = 0; i < _lines.Length; i++)
            {
                GameObject lineObject = new GameObject("Outline Line " + i);
                lineObject.transform.SetParent(_outlineRoot.transform, false);
                LineRenderer line = lineObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = 2;
                line.startWidth = lineWidth;
                line.endWidth = lineWidth;
                line.material = lineMaterial;
                line.startColor = outlineColour;
                line.endColor = outlineColour;
                _lines[i] = line;
            }
            _outlineRoot.SetActive(false);
            enabled = false;
        }

        /**
         * Recalculates 12 line positions around the brick.
         */
        private void UpdateOutline()
        {
            if (!TryGetVisibleBounds(out Bounds bounds)) return;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3 p000 = new Vector3(min.x, min.y, min.z);
            Vector3 p001 = new Vector3(min.x, min.y, max.z);
            Vector3 p010 = new Vector3(min.x, max.y, min.z);
            Vector3 p011 = new Vector3(min.x, max.y, max.z);
            Vector3 p100 = new Vector3(max.x, min.y, min.z);
            Vector3 p101 = new Vector3(max.x, min.y, max.z);
            Vector3 p110 = new Vector3(max.x, max.y, min.z);
            Vector3 p111 = new Vector3(max.x, max.y, max.z);
            SetLine(0, p000, p001);
            SetLine(1, p001, p101);
            SetLine(2, p101, p100);
            SetLine(3, p100, p000);
            SetLine(4, p010, p011);
            SetLine(5, p011, p111);
            SetLine(6, p111, p110);
            SetLine(7, p110, p010);
            SetLine(8, p000, p010);
            SetLine(9, p001, p011);
            SetLine(10, p101, p111);
            SetLine(11, p100, p110);
        }

        /**
         * Sets the start and end points of one LineRenderer.
         */
        private void SetLine(int index, Vector3 start, Vector3 end)
        {
            _lines[index].SetPosition(0, start);
            _lines[index].SetPosition(1, end);
        }

        /**
         * Calculates a bounding box around all visible renderers in the brick.
         * Returns false if no renderers were found.
         */
        private bool TryGetVisibleBounds(out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (!renderer) continue;
                if (_outlineRoot && renderer.transform.IsChildOf(_outlineRoot.transform)) continue;
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            if (hasBounds) bounds.Expand(0.05f);
            return hasBounds;
        }
    }
}