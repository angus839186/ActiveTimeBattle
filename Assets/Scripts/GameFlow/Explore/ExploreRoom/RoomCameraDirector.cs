using UnityEngine;

public class RoomCameraDirector : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    public void MoveTo(RoomCameraPoint cameraPoint)
    {
        if (cameraPoint == null)
        {
            Debug.LogWarning("RoomCameraDirector: Camera point is null.");
            return;
        }

        if (targetCamera == null)
        {
            Debug.LogWarning("RoomCameraDirector: Target camera is not assigned.");
            return;
        }

        targetCamera.transform.SetPositionAndRotation(
            cameraPoint.transform.position,
            cameraPoint.transform.rotation);
    }
}