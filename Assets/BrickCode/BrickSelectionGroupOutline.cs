using System.Collections.Generic;
using UnityEngine;

namespace BrickCode
{
    public class BrickSelectionGroupOutline : MonoBehaviour
    {
        [SerializeField] private Color outlineColour = Color.yellow;
        [SerializeField] private float lineWidth = 0.04f;

        private readonly List<BrickObjectData> _targets = new();

        private GameObject _outlineRoot;
        private LineRenderer[] _lines;

        public void Show(List<BrickObjectData> targets)
        {
            EnsureCreated();

            _targets.Clear();

            foreach (BrickObjectData target in targets)
            {
                if (target)
                {
                    _targets.Add(target);
                }
            }

            bool hasTargets = _targets.Count > 0;

            _outlineRoot.SetActive(hasTargets);
            enabled = hasTargets;

            if (hasTargets)
            {
                UpdateOutline();
            }
        }

        public void Hide()
        {
            _targets.Clear();

            if (_outlineRoot)
            {
                _outlineRoot.SetActive(false);
            }

            enabled = false;
        }

        private void LateUpdate()
        {
            if (!_outlineRoot || !_outlineRoot.activeSelf)
            {
                return;
            }

            UpdateOutline();
        }

        private void EnsureCreated()
        {
            if (_outlineRoot)
            {
                return;
            }

            _outlineRoot = new GameObject("Selection Group Outline");
            _outlineRoot.transform.SetParent(transform, false);

            _lines = new LineRenderer[12];

            Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.color = outlineColour;

            for (int i = 0; i < _lines.Length; i++)
            {
                GameObject lineObject = new GameObject("Group Outline Line " + i);
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

        private void UpdateOutline()
        {
            if (!TryGetCombinedBounds(out Bounds bounds))
            {
                Hide();
                return;
            }

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

        private void SetLine(int index, Vector3 start, Vector3 end)
        {
            _lines[index].SetPosition(0, start);
            _lines[index].SetPosition(1, end);
        }

        private bool TryGetCombinedBounds(out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;

            foreach (BrickObjectData target in _targets)
            {
                if (!target)
                {
                    continue;
                }

                Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in renderers)
                {
                    if (!renderer)
                    {
                        continue;
                    }

                    if (renderer is LineRenderer)
                    {
                        continue;
                    }

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
            }

            if (hasBounds)
            {
                bounds.Expand(0.08f);
            }

            return hasBounds;
        }
    }
}