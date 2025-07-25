using UnityEngine;
using UnityEngine.UIElements;

public class CameraDrag : MonoBehaviour
{
    public float dragSpeed = 0.5f;
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 30f;

    private Vector3 dragOrigin;
    private bool isDragging = false;
    private Camera cam;

    public UIDocument uiDocument; // asignar desde el editor si querés

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {

     //   if (UIUtils.IsPointerOverUI()) return; 
        HandleDrag();
        HandleZoom();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
           // if (UIUtils.IsPointerOverUI())
          //  {
          //      isDragging = false;
           //     return;
           // }

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

    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}
