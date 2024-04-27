using UnityEngine;

public class LookAtCam : MonoBehaviour
{
    private Camera _cam;

    private void Start()
    {
        _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        var directionToCamera = _cam.transform.position - transform.position;
        directionToCamera.y += 180;
        transform.LookAt(transform.position + directionToCamera, Vector3.up);
    }
}
