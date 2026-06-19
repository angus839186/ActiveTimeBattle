using UnityEngine;

namespace ActiveTimeBattle.Presentation.Hub
{
    [RequireComponent(typeof(Canvas))]
    public sealed class BillboardView : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float maximumVisibleDistance = 8f;

        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                return;
            }

            float distance = Vector3.Distance(
                transform.position,
                targetCamera.transform.position);
            bool shouldShow = distance <= maximumVisibleDistance;
            if (_canvas.enabled != shouldShow)
            {
                _canvas.enabled = shouldShow;
            }

            if (shouldShow)
            {
                transform.rotation = Quaternion.LookRotation(
                    transform.position - targetCamera.transform.position);
            }
        }
    }
}
