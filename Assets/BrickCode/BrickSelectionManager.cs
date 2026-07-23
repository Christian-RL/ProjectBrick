using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ModelCode;
using MenuCode;

namespace BrickCode
{
    /**
     * Controls:
     *  -   Which bricks are currently selected.
     *  -   Whether a click is a single or double click.
     *  -   Whether selected bricks should be highlighted.
     */
    public class BrickSelectionManager : MonoBehaviour
    {
        public static BrickSelectionManager Instance { get; private set; } //static singleton
        [SerializeField] private float doubleClickSeconds = 0.3f; //how quickly two clicks must happen to count as a double click
        private readonly List<BrickObjectData> _selectedObjects = new(); //store selected bricks
        private BrickObjectData _lastClickedBrick; //store which brick was clicked last
        private float _lastClickTime = -999f; //-999 so first click never counts as a double click
        private BrickObjectData _activeSelectedBrick;
        private BrickSelectionGroupOutline _groupOutline;

        public bool HasSelection => _selectedObjects.Count > 0; //true if at least one brick is selected

        /**
         * Create a BrickSelectionManager on scene load if one does not already exist.
         */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateManagerIfNeeded()
        {
            if (FindObjectOfType<BrickSelectionManager>() != null) return;
            GameObject managerObject = new GameObject("Brick Selection Manager");
            managerObject.AddComponent<BrickSelectionManager>();
        }

        /**
         * Sets up the singleton.
         * If another BrickSelectionManager already exist, then this duplicate destroys itself.
         * Otherwise this object becomes the global instance.
         */
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;_groupOutline = GetComponent<BrickSelectionGroupOutline>();

            if (!_groupOutline)
            {
                _groupOutline = gameObject.AddComponent<BrickSelectionGroupOutline>();
            }
        }

        /**
         * Runs each frame. Detects clicks on blank space to clear selection.
         * Does not handle brick clicks directly.
         */
        private void Update()
        {
            if (Mouse.current == null || !Camera.main) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            if (BrickSidebarSpawner.IsMouseOverSidebar) return;
            if (BrickInput.CameraControlInputIsActive()) return;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                BrickObjectData clickedBrick = hit.collider.GetComponentInParent<BrickObjectData>();
                if (clickedBrick) return;
            }
            ClearSelection();
        }

        /**
         * handles selection when a brick is clicked.
         * Returns true if a double click,  false if a single click.
         */
        public bool HandleBrickClicked(BrickObjectData clickedBrick)
        {
            if (!clickedBrick)
            {
                ClearSelection();
                return false;
            }
            _activeSelectedBrick = clickedBrick;
            bool isDoubleClick =
                _lastClickedBrick == clickedBrick &&
                Time.unscaledTime - _lastClickTime <= doubleClickSeconds;

            _lastClickedBrick = clickedBrick;
            _lastClickTime = Time.unscaledTime;
            if (isDoubleClick)
            {
                BrickModelRegistry.DisconnectBrick(clickedBrick);
                SelectSingleBrick(clickedBrick);
                Debug.Log("Double clicked brick. Disconnected and selected single brick.");
                return true;
            }
            SelectConnectedStructure(clickedBrick);
            return false;
        }

        /**
         * Selects a single brick.
         */
        public void SelectSingleBrick(BrickObjectData brick)
        {
            ClearSelection();

            if (!brick)
            {
                return;
            }

            _activeSelectedBrick = brick;

            _selectedObjects.Add(brick);
            SetHighlighted(brick, true);
        }

        /**
         * Selects the whole connected model a clicked brick belongs to.
         */
        public void SelectConnectedStructure(BrickObjectData clickedBrick)
        {
            ClearSelection();

            if (!clickedBrick)
            {
                return;
            }

            _activeSelectedBrick = clickedBrick;

            List<BrickObjectData> connectedObjects =
                BrickModelRegistry.GetConnectedObjects(clickedBrick.Brick);

            if (connectedObjects.Count == 0)
            {
                connectedObjects.Add(clickedBrick);
            }

            foreach (BrickObjectData connectedObject in connectedObjects)
            {
                if (!connectedObject)
                {
                    continue;
                }

                if (_selectedObjects.Contains(connectedObject))
                {
                    continue;
                }

                _selectedObjects.Add(connectedObject);
            }

            if (_selectedObjects.Count == 1)
            {
                SetHighlighted(_selectedObjects[0], true);
            }
            else
            {
                _groupOutline.Show(_selectedObjects);
            }
        }

        /**
         * Turns off highlighting and clears the selection list.
         */
        public void ClearSelection()
        {
            foreach (BrickObjectData selectedObject in _selectedObjects)
            {
                SetHighlighted(selectedObject, false);
            }

            _selectedObjects.Clear();
            _activeSelectedBrick = null;

            if (_groupOutline)
            {
                _groupOutline.Hide();
            }
        }

        /**
         * Check if a brick is currently selected.
         * Returns false if the brick is null.
         */
        public bool IsSelected(BrickObjectData brick)
        {
            return brick  && _selectedObjects.Contains(brick);
        }

        /**
         * Returns a copy of the current selection list.
         */
        public List<BrickObjectData> GetSelectedObjects()
        {
            return new List<BrickObjectData>(_selectedObjects);
        }

        /**
         * Sets the visual highlight on or off for a brick.
         */
        private void SetHighlighted(BrickObjectData brick, bool highlighted)
        {
            if (!brick) return;
            BrickSelectionOutline outline = brick.GetComponent<BrickSelectionOutline>();
            if (!outline) outline = brick.gameObject.AddComponent<BrickSelectionOutline>();
            outline.SetHighlighted(highlighted);
        }

        public void DeleteActiveSelectedBrick()
        {
            if (!_activeSelectedBrick)
            {
                Debug.Log("No active selected brick to delete.");
                return;
            }

            BrickObjectData brickToDelete = _activeSelectedBrick;

            List<BrickObjectData> connectedBeforeDelete = new();

            if (brickToDelete.Brick != null)
            {
                connectedBeforeDelete = BrickModelRegistry.GetConnectedObjects(brickToDelete.Brick);
            }

            Dictionary<BrickObjectData, Pose> originalPoses = new();

            foreach (BrickObjectData brickData in connectedBeforeDelete)
            {
                if (!brickData || brickData == brickToDelete)
                {
                    continue;
                }

                originalPoses[brickData] = new Pose(
                    brickData.transform.position,
                    brickData.transform.rotation
                );
            }

            ClearSelection();

            BrickModelRegistry.DisconnectBrick(brickToDelete);

            Destroy(brickToDelete.gameObject);

            foreach (KeyValuePair<BrickObjectData, Pose> pair in originalPoses)
            {
                BrickObjectData brickData = pair.Key;

                if (!brickData)
                {
                    continue;
                }

                brickData.transform.SetPositionAndRotation(
                    pair.Value.position,
                    pair.Value.rotation
                );
            }

            Physics.SyncTransforms();

            _lastClickedBrick = null;
            _lastClickTime = -999f;

            Debug.Log("Deleted active selected brick.");
        }
    }
}