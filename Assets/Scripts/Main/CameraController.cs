using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector3 initialPosition;
    [SerializeField] private Vector3 initialRotation;
    [SerializeField] private Vector3 topDownPosition;
    [SerializeField] private Vector3 topDownRotation;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float horizontalMoveSpeed = 20f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 50f;
    
    private Camera mainCamera;
    private bool isMoving = false;
    private bool isInTopDownView = false;

    private void Start()
    {
        mainCamera = Camera.main;  // This is fine but could be optimized
        // Set initial position and rotation
        mainCamera.transform.position = initialPosition;
        mainCamera.transform.eulerAngles = initialRotation;
    }

    private void Update()
    {
        HandleTopDownViewToggle();
        HandleCameraMovement();
        HandleZoom();
    }

    private void HandleTopDownViewToggle()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!isInTopDownView)
            {
                audioManager.PlaySFX(audioManager.tacticalView);
            }
            isMoving = true;
            isInTopDownView = !isInTopDownView;
        }

        if (isMoving)
        {
            Vector3 targetPosition = isInTopDownView ? topDownPosition : initialPosition;
            Vector3 targetRotation = isInTopDownView ? topDownRotation : initialRotation;

            // Smooth position movement
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime
            );

            // Smooth rotation
            mainCamera.transform.rotation = Quaternion.Lerp(
                mainCamera.transform.rotation,
                Quaternion.Euler(targetRotation),
                rotateSpeed * Time.deltaTime
            );

            // Check if we're close enough to stop
            if (Vector3.Distance(mainCamera.transform.position, targetPosition) < 0.01f &&
                Quaternion.Angle(mainCamera.transform.rotation, Quaternion.Euler(targetRotation)) < 0.01f)
            {
                isMoving = false;
                // Snap to exact position and rotation
                mainCamera.transform.position = targetPosition;
                mainCamera.transform.eulerAngles = targetRotation;
            }
        }
    }

    private void HandleCameraMovement()
    {
        if (isMoving) return; // Don't allow WASD movement while transitioning views

        // Using GetAxisRaw instead of GetAxis for more responsive movement
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Only process movement if there's actual input
        if (horizontal != 0 || vertical != 0)
        {
            Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;
            Vector3 cameraRight = mainCamera.transform.right;
            Vector3 cameraForward = Vector3.Cross(cameraRight, Vector3.up);

            Vector3 moveDirection = (cameraRight * movement.x + cameraForward * movement.z) * horizontalMoveSpeed * Time.deltaTime;
            mainCamera.transform.position += moveDirection;
        }
    }

    private void HandleZoom()
    {
        if (isMoving) return; // Don't allow zoom while transitioning views

        float scrollDelta = Input.mouseScrollDelta.y;
        // Only process zoom if there's actual scroll input
        if (scrollDelta != 0)
        {
            Vector3 zoomDirection = isInTopDownView ? Vector3.up : mainCamera.transform.forward;
            float zoomMultiplier = isInTopDownView ? -1 : 1;
            Vector3 newPosition = mainCamera.transform.position + zoomDirection * scrollDelta * zoomSpeed * zoomMultiplier;
            
            // Calculate height for zoom limits
            float height = newPosition.y;
            if (height >= minZoom && height <= maxZoom)
            {
                mainCamera.transform.position = newPosition;
            }
        }
    }
} 