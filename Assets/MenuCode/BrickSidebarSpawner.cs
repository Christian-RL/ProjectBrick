using UnityEngine;
using UnityEngine.InputSystem;

using BrickCode;
namespace MenuCode
{
    public class BrickSidebarSpawner : MonoBehaviour
    {
        [SerializeField] private float defaultSpawnDistance = 8f;

        // Made taller so the button definitely appears.
        private readonly Rect _sidebarRect = new Rect(10, 10, 260, 430);

        private string _widthText = "2";
        private string _lengthText = "2";
        private string _heightText = "3";

        private Color _selectedColour = Color.red;
        private string _selectedColourName = "Red";

        private bool _placingBrick;
        private bool _ignoreCurrentMousePress;
        public static bool IsMouseOverSidebar { get; private set; }

        private GameObject _previewBrick;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;

            if (_camera == null)
            {
                Debug.LogError("No MainCamera found. Make sure your camera is tagged MainCamera.");
            }
        }

        private void Update()
        {
            if (Mouse.current != null)
            {
                IsMouseOverSidebar = MouseIsOverSidebar();
            }
            if (_camera == null || Mouse.current == null)
            {
                return;
            }

            if (!_placingBrick)
            {
                return;
            }

            UpdatePreviewPosition();

            // Prevents the same click used on the sidebar button
            // from instantly placing the brick.
            if (_ignoreCurrentMousePress)
            {
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    _ignoreCurrentMousePress = false;
                }

                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && !MouseIsOverSidebar())
            {
                PlacePreviewBrick();
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPlacement();
            }
        }

        private void OnGUI()
        {
            GUI.Box(_sidebarRect, "Brick Menu");

            GUILayout.BeginArea(new Rect(
                _sidebarRect.x + 10,
                _sidebarRect.y + 25,
                _sidebarRect.width - 20,
                _sidebarRect.height - 35
            ));

            GUILayout.Label("Basic Brick Dimensions");

            GUILayout.Label("Stud Width");
            _widthText = GUILayout.TextField(_widthText);

            GUILayout.Label("Stud Length");
            _lengthText = GUILayout.TextField(_lengthText);

            GUILayout.Label("Tile Height");
            _heightText = GUILayout.TextField(_heightText);

            GUILayout.Space(10);

            if (GUILayout.Button("Load Brick Onto Cursor", GUILayout.Height(35)))
            {
                Debug.Log("Load Brick Onto Cursor clicked.");
                StartPlacingBrick();
            }

            GUILayout.Space(10);

            GUILayout.Label("Colour: " + _selectedColourName);

            GUILayout.BeginHorizontal();
            DrawColourButton("Red", Color.red);
            DrawColourButton("Blue", Color.blue);
            DrawColourButton("Green", Color.green);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawColourButton("Yellow", Color.yellow);
            DrawColourButton("White", Color.white);
            DrawColourButton("Black", Color.black);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (_placingBrick)
            {
                GUILayout.Label("Preview loaded.");
                GUILayout.Label("Click in viewport to place.");
                GUILayout.Label("Esc = cancel.");
            }

            GUILayout.EndArea();
        }

        private void DrawColourButton(string colourName, Color colour)
        {
            Color oldBackground = GUI.backgroundColor;
            GUI.backgroundColor = colour;

            if (GUILayout.Button(colourName, GUILayout.Height(25)))
            {
                _selectedColour = colour;
                _selectedColourName = colourName;
            }

            GUI.backgroundColor = oldBackground;
        }

        private void StartPlacingBrick()
        {
            if (!TryReadDimensions(out int width, out int length, out int height))
            {
                Debug.LogWarning("Invalid brick dimensions. Use positive whole numbers.");
                return;
            }

            if (_previewBrick != null)
            {
                Destroy(_previewBrick);
            }

            Brick brick = new BasicBrick(
                "custom",
                $"{width}x{length} Custom Brick",
                _selectedColour,
                width,
                length,
                height
            );

            _previewBrick = CreateVisualBrick(brick, "Preview Brick");

            // Disable colliders while previewing so it does not interfere with clicks.
            SetCollidersEnabled(_previewBrick, false);

            _placingBrick = true;
            _ignoreCurrentMousePress = true;

            UpdatePreviewPosition();

            Debug.Log(
                $"Preview brick created: {width}x{length}x{height}, children: {_previewBrick.transform.childCount}"
            );
        }

        private bool TryReadDimensions(out int width, out int length, out int height)
        {
            bool validWidth = int.TryParse(_widthText, out width);
            bool validLength = int.TryParse(_lengthText, out length);
            bool validHeight = int.TryParse(_heightText, out height);

            if (!validWidth || !validLength || !validHeight)
            {
                return false;
            }

            width = Mathf.Clamp(width, 1, 20);
            length = Mathf.Clamp(length, 1, 20);
            height = Mathf.Clamp(height, 1, 20);

            return true;
        }

        private GameObject CreateVisualBrick(Brick brick, string objectName)
        {
            GameObject brickObject = new GameObject(objectName);

            // Place it in front of the camera immediately.
            brickObject.transform.position =
                _camera.transform.position + _camera.transform.forward * defaultSpawnDistance;

            BrickVisual visual = brickObject.AddComponent<BrickVisual>();
            visual.BuildFromBrick(brick);

            return brickObject;
        }

        private void UpdatePreviewPosition()
        {
            if (_previewBrick == null)
            {
                return;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _camera.ScreenPointToRay(mousePosition);

            float spawnDistance = GetGoodSpawnDistance();

            _previewBrick.transform.position = ray.GetPoint(spawnDistance);
        }

        private float GetGoodSpawnDistance()
        {
            if (!TryReadDimensions(out int width, out int length, out int height))
            {
                return defaultSpawnDistance;
            }

            int biggestDimension = Mathf.Max(width, length, height);

            return Mathf.Max(defaultSpawnDistance, biggestDimension * 2.5f);
        }

        private void PlacePreviewBrick()
        {
            if (_previewBrick == null)
            {
                return;
            }

            _previewBrick.name = "Placed Brick";

            SetCollidersEnabled(_previewBrick, true);

            if (_previewBrick.GetComponent<DraggableBrick3D>() == null)
            {
                _previewBrick.AddComponent<DraggableBrick3D>();
            }

            Debug.Log("Placed brick.");

            _previewBrick = null;
            _placingBrick = false;
            _ignoreCurrentMousePress = false;
        }

        private void CancelPlacement()
        {
            if (_previewBrick != null)
            {
                Destroy(_previewBrick);
            }

            _previewBrick = null;
            _placingBrick = false;
            _ignoreCurrentMousePress = false;

            Debug.Log("Cancelled brick placement.");
        }

        private void SetCollidersEnabled(GameObject obj, bool enabled)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();

            foreach (Collider collider in colliders)
            {
                collider.enabled = enabled;
            }
        }

        private bool MouseIsOverSidebar()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // GUI coordinates start at top-left.
            // Mouse coordinates start at bottom-left.
            Vector2 guiMousePosition = new Vector2(
                mousePosition.x,
                Screen.height - mousePosition.y
            );

            return _sidebarRect.Contains(guiMousePosition);
        }
    }
}