using System.Collections;
using UnityEngine;

public class RoomCameraDirector : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float moveDuration = 0.35f;

    private Coroutine moveRoutine;

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

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(MoveRoutine(cameraPoint.transform));
    }

    private IEnumerator MoveRoutine(Transform target)
    {
        Vector3 startPosition = targetCamera.transform.position;
        Quaternion startRotation = targetCamera.transform.rotation;

        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            targetCamera.transform.SetPositionAndRotation(
                Vector3.Lerp(startPosition, targetPosition, t),
                Quaternion.Slerp(startRotation, targetRotation, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        targetCamera.transform.SetPositionAndRotation(targetPosition, targetRotation);
        moveRoutine = null;
    }
}