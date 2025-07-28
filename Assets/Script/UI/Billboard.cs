using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        var cam = Camera.main;
        Vector3 lookPos = transform.position + cam.transform.rotation * Vector3.forward;
        lookPos.y = transform.position.y; // mantener vertical
        transform.LookAt(lookPos);
    }
}
