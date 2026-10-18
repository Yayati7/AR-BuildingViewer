using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

/// <summary>Creates the viewer controls and a simple house that can be explored in preview or AR.</summary>
public class PJ8Viewer : MonoBehaviour
{
    private Transform houseRoot;
    private readonly List<GameObject> roofs = new List<GameObject>();
    private readonly List<GameObject> selectedRoomItems = new List<GameObject>();
    private bool placed;
    private bool roofsVisible = true;
    private Vector2 previousPointer;
    private bool dragging;
    private float scale = 1f;
    private Text status;

#if UNITY_ANDROID && !UNITY_EDITOR
    private ARRaycastManager raycaster;
    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();
#endif

    private void Start()
    {
        BuildHouse();
        BuildControls();
        status.text = "Preview: click the floor to place the house";
#if UNITY_ANDROID && !UNITY_EDITOR
        SetupAR();
        status.text = "Point at a table or floor, then tap to place the house";
#endif
    }

    private void Update()
    {
        if (houseRoot == null) return;
        HandlePointer();
    }

    private void BuildHouse()
    {
        houseRoot = new GameObject("PJ8 House").transform;
        houseRoot.position = new Vector3(0, -0.55f, 2.4f);
        houseRoot.localScale = Vector3.one * 0.38f;

        Make("Foundation", new Vector3(0, 0.1f, 0), new Vector3(5.4f, 0.2f, 4.2f), new Color32(185, 177, 160, 255));
        Make("Living room floor", new Vector3(-1.1f, 0.24f, 0), new Vector3(3.1f, 0.08f, 3.8f), new Color32(219, 193, 152, 255));
        Make("Bedroom floor", new Vector3(1.55f, 0.24f, 0.95f), new Vector3(2, 0.08f, 1.9f), new Color32(196, 204, 223, 255));
        Make("Kitchen floor", new Vector3(1.55f, 0.24f, -1.05f), new Vector3(2, 0.08f, 1.9f), new Color32(197, 222, 204, 255));

        var wall = new Color32(242, 231, 210, 255);
        Make("Back wall", new Vector3(0, 1.45f, 1.9f), new Vector3(5.1f, 2.5f, 0.16f), wall);
        Make("Left wall", new Vector3(-2.55f, 1.45f, 0), new Vector3(0.16f, 2.5f, 3.9f), wall);
        Make("Right wall", new Vector3(2.55f, 1.45f, 0), new Vector3(0.16f, 2.5f, 3.9f), wall);
        Make("Front left wall", new Vector3(-2.05f, 1.45f, -1.9f), new Vector3(1.1f, 2.5f, 0.16f), wall);
        Make("Front right wall", new Vector3(2.05f, 1.45f, -1.9f), new Vector3(1.1f, 2.5f, 0.16f), wall);
        Make("Room divider", new Vector3(0.05f, 1.4f, 0.35f), new Vector3(0.14f, 2.35f, 3.1f), new Color32(231, 221, 202, 255));
        Make("Bedroom divider", new Vector3(1.5f, 1.4f, 0), new Vector3(2.1f, 2.35f, 0.14f), wall);

        roofs.Add(Make("Roof left", new Vector3(-1.35f, 2.95f, 0), new Vector3(2.9f, 0.18f, 4.5f), new Color32(178, 91, 64, 255)));
        roofs.Add(Make("Roof right", new Vector3(1.35f, 2.95f, 0), new Vector3(2.9f, 0.18f, 4.5f), new Color32(178, 91, 64, 255)));

        selectedRoomItems.Add(Make("Bed", new Vector3(1.6f, 0.55f, 1.0f), new Vector3(1.2f, 0.45f, 1.5f), new Color32(79, 130, 174, 255)));
        selectedRoomItems.Add(Make("Kitchen counter", new Vector3(1.65f, 0.62f, -1.1f), new Vector3(1.5f, 0.62f, 0.6f), new Color32(115, 147, 115, 255)));
        selectedRoomItems.Add(Make("Sofa", new Vector3(-1.35f, 0.65f, 0.9f), new Vector3(1.7f, 0.7f, 0.8f), new Color32(198, 132, 86, 255)));
        Make("Front door", new Vector3(0, 0.95f, -1.94f), new Vector3(0.8f, 1.5f, 0.1f), new Color32(111, 76, 54, 255));
        foreach (Transform child in houseRoot) child.gameObject.SetActive(false);
    }

    private GameObject Make(string objectName, Vector3 position, Vector3 size, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objectName;
        go.transform.SetParent(houseRoot, false);
        go.transform.localPosition = position;
        go.transform.localScale = size;
        var renderer = go.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        renderer.material.color = color;
        return go;
    }

    private void BuildControls()
    {
        var canvasObject = new GameObject("PJ8 Controls", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280, 720);
        if (Object.FindFirstObjectByType<EventSystem>() == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        status = MakeLabel(canvas.transform, "Preview: click the floor to place the house", new Vector2(0.5f, 1), new Vector2(0, -35), new Vector2(800, 48), 24, TextAnchor.MiddleCenter);
        Button(canvas.transform, "Rotate", new Vector2(0, 0), new Vector2(95, 52), () => { if (placed) houseRoot.Rotate(Vector3.up, 35f); });
        Button(canvas.transform, "Zoom +", new Vector2(105, 0), new Vector2(95, 52), () => SetScale(scale * 1.15f));
        Button(canvas.transform, "Zoom -", new Vector2(210, 0), new Vector2(95, 52), () => SetScale(scale / 1.15f));
        Button(canvas.transform, "Rooms", new Vector2(315, 0), new Vector2(95, 52), ToggleRooms);
        Button(canvas.transform, "Roof", new Vector2(420, 0), new Vector2(95, 52), ToggleRoof);
        Button(canvas.transform, "Reset", new Vector2(525, 0), new Vector2(95, 52), ResetHouse);
    }

    private Text MakeLabel(Transform parent, string value, Vector2 anchor, Vector2 offset, Vector2 size, int fontSize, TextAnchor alignment)
    {
        var go = new GameObject("Status", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = offset;
        rect.sizeDelta = size;
        var text = go.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private void Button(Transform parent, string label, Vector2 offset, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0, 0);
        rect.anchoredPosition = offset + new Vector2(14, 16);
        rect.sizeDelta = size;
        var image = go.GetComponent<Image>();
        image.color = new Color32(35, 103, 143, 230);
        var button = go.GetComponent<Button>();
        button.onClick.AddListener(action);
        var text = MakeLabel(go.transform, label, new Vector2(0.5f, 0.5f), Vector2.zero, size, 20, TextAnchor.MiddleCenter);
        text.raycastTarget = false;
    }

    private void HandlePointer()
    {
        if (Input.GetMouseButtonDown(0))
        {
            previousPointer = Input.mousePosition;
            dragging = placed && Input.mousePosition.y > Screen.height * 0.18f;
        }
        if (Input.GetMouseButton(0) && dragging && placed)
        {
            var delta = (Vector2)Input.mousePosition - previousPointer;
            houseRoot.Rotate(Vector3.up, -delta.x * 0.35f, Space.World);
            previousPointer = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            var isTap = ((Vector2)Input.mousePosition - previousPointer).sqrMagnitude < 100f;
            if (!placed && isTap) TryPlace(Input.mousePosition);
            dragging = false;
        }
        var wheel = Input.mouseScrollDelta.y;
        if (placed && Mathf.Abs(wheel) > 0.01f) SetScale(scale * (wheel > 0 ? 1.08f : 0.92f));
    }

    private void TryPlace(Vector2 screenPoint)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (raycaster != null && raycaster.Raycast(screenPoint, Hits, TrackableType.PlaneWithinPolygon) && Hits.Count > 0)
        {
            houseRoot.position = Hits[0].pose.position;
            houseRoot.rotation = Quaternion.identity;
            ShowHouse();
        }
#else
        var ray = Camera.main.ScreenPointToRay(screenPoint);
        var ground = new Plane(Vector3.up, new Vector3(0, -0.56f, 0));
        if (ground.Raycast(ray, out var distance)) houseRoot.position = ray.GetPoint(distance);
        ShowHouse();
#endif
    }

    private void ShowHouse()
    {
        placed = true;
        foreach (Transform child in houseRoot) child.gameObject.SetActive(true);
        status.text = "Drag to rotate • use buttons to zoom and explore";
    }

    private void SetScale(float value)
    {
        scale = Mathf.Clamp(value, 0.55f, 2.2f);
        houseRoot.localScale = Vector3.one * 0.38f * scale;
    }

    private void ToggleRooms()
    {
        foreach (var item in selectedRoomItems) item.SetActive(!item.activeSelf);
        status.text = "Rooms inspected: furniture shown / hidden";
    }

    private void ToggleRoof()
    {
        roofsVisible = !roofsVisible;
        foreach (var roof in roofs) roof.SetActive(roofsVisible);
        status.text = roofsVisible ? "Roof shown" : "Roof hidden — look inside!";
    }

    private void ResetHouse()
    {
        placed = false;
        scale = 1f;
        houseRoot.position = new Vector3(0, -0.55f, 2.4f);
        houseRoot.rotation = Quaternion.identity;
        houseRoot.localScale = Vector3.one * 0.38f;
        foreach (Transform child in houseRoot) child.gameObject.SetActive(false);
        status.text = "Tap a surface to place the house";
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void SetupAR()
    {
        var session = FindObjectOfType<ARSession>();
        if (session == null) new GameObject("AR Session", typeof(ARSession));
        var cameraObject = Camera.main != null ? Camera.main.gameObject : new GameObject("AR Camera", typeof(Camera));
        cameraObject.tag = "MainCamera";
        if (cameraObject.GetComponent<ARCameraManager>() == null) cameraObject.AddComponent<ARCameraManager>();
        if (cameraObject.GetComponent<ARCameraBackground>() == null) cameraObject.AddComponent<ARCameraBackground>();
        var origin = FindObjectOfType<ARSessionOrigin>();
        if (origin == null) origin = new GameObject("XR Origin", typeof(ARSessionOrigin)).GetComponent<ARSessionOrigin>();
        origin.camera = cameraObject.GetComponent<Camera>();
        origin.camera.transform.SetParent(origin.transform, false);
        var planeManager = origin.GetComponent<ARPlaneManager>() ?? origin.gameObject.AddComponent<ARPlaneManager>();
        planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        raycaster = origin.GetComponent<ARRaycastManager>() ?? origin.gameObject.AddComponent<ARRaycastManager>();
    }
#endif
}
