using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Drag")]
    public float dragSpeed = 5f;

    [Header("Zoom")]
    public float zoomSpeed = 30f;
    public float minZoom = 5f;
    public float maxZoom = 30f;

    [Header("Tiles")]
    public float tileSize = 2.5f; // Tamaño de tile en mundo

    private Vector3 dragOrigin;
    private bool isDragging = false;
    private Camera cam;
    private Coroutine moveRoutine;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (UIUtils.IsPointerOverUI()) return;

        HandleDrag();
        HandleZoom();
    }

    // 🟢 Arrastrar cámara
    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 currentPos = Input.mousePosition;
            Vector3 delta = dragOrigin - currentPos;

            Vector3 move = cam.transform.right * delta.x + cam.transform.up * delta.y;
            move.y = 0;
            cam.transform.position += move * dragSpeed * Time.deltaTime;

            dragOrigin = currentPos;
        }
    }

    // 🟢 Zoom cámara
    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    // 🟢 Centrar cámara en una celda manteniendo rotación
    public void FocusOnCell(Vector2Int cell)
    {
        Vector3 worldPos = GridUtils.GridToWorld(cell);

        Vector3 camPos = cam.transform.position;
        Vector3 target = new Vector3(worldPos.x, camPos.y, worldPos.z);

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveCameraSmooth(target));
    }

    private IEnumerator MoveCameraSmooth(Vector3 target)
    {
        Vector3 start = cam.transform.position;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f; // velocidad de centrado
            cam.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        cam.transform.position = target;
    }
}
