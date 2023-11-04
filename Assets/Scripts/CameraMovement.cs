using UnityEngine;

public class CameraMovement : MonoBehaviour {
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 0.125f;

    void LateUpdate() {
        Vector3 desiredPosition = new(target.position.x + offset.x, transform.position.y, transform.position.z);
        desiredPosition.x = Mathf.Max(0, desiredPosition.x);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
