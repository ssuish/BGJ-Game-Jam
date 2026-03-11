using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ProximityCamera : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform playerOne;
    [SerializeField] private Transform playerTwo;

    [Header("Position")]
    [SerializeField] private float positionSmoothTime = 0.2f;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float padding = 2f;
    [SerializeField] private float zoomSmoothTime = 0.2f;

    public Vector3 Midpoint { get; private set; }
    public float TargetOrthographicSize { get; private set; }

    private Camera cachedCamera;
    private Vector3 positionVelocity;
    private float zoomVelocity;
    private float cameraZ;

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();
        cameraZ = transform.position.z;
        cachedCamera.orthographic = true;
    }

    private void LateUpdate()
    {
        if (playerOne == null || playerTwo == null)
        {
            return;
        }

        Midpoint = (playerOne.position + playerTwo.position) * 0.5f;

        Vector3 targetPosition = new Vector3(Midpoint.x, Midpoint.y, cameraZ);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, positionSmoothTime);

        TargetOrthographicSize = CalculateTargetOrthographicSize();
        cachedCamera.orthographicSize = Mathf.SmoothDamp(cachedCamera.orthographicSize, TargetOrthographicSize, ref zoomVelocity, zoomSmoothTime);
    }

    public void SetTargets(Transform firstPlayer, Transform secondPlayer)
    {
        playerOne = firstPlayer;
        playerTwo = secondPlayer;
    }

    private float CalculateTargetOrthographicSize()
    {
        float horizontalDistance = Mathf.Abs(playerOne.position.x - playerTwo.position.x);
        float verticalDistance = Mathf.Abs(playerOne.position.y - playerTwo.position.y);

        float requiredHeightFromVertical = (verticalDistance * 0.5f) + padding;
        float requiredHeightFromHorizontal = ((horizontalDistance * 0.5f) + padding) / Mathf.Max(cachedCamera.aspect, 0.0001f);
        float requiredSize = Mathf.Max(requiredHeightFromVertical, requiredHeightFromHorizontal);

        return Mathf.Clamp(requiredSize, minZoom, maxZoom);
    }
}
