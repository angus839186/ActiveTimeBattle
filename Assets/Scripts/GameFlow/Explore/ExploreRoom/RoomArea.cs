using UnityEngine;

public class RoomArea : MonoBehaviour
{
    [SerializeField] private RoomCameraDirector cameraDirector;
    [SerializeField] private RoomCameraPoint cameraPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<CharacterController>())
        {
            return;
        }

        cameraDirector.MoveTo(cameraPoint);
        Debug.Log($"Entered room area: {name}");
    }
}