using UnityEngine;

public class MousePosition : MonoBehaviour
{
    Camera mainCamera;
    Plane groundPlane;
    public Vector3 currentMousePosition;
    private bool hasHit;

    public Color color = Color.red;

    void Start()
    {
        mainCamera = Camera.main;
        // Create a mathematical plane at y=0, facing up
        groundPlane = new Plane(Vector3.up, 0);
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;

        hasHit = groundPlane.Raycast(ray, out distance);
        if (hasHit)
        {
            currentMousePosition = ray.GetPoint(distance);
        }
        
        // Draw the ray in scene view (red line)
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
    }

    // This will draw a green sphere at the mouse position
    private void OnDrawGizmos()
    {
        if (hasHit)
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(currentMousePosition, 0.2f);
        }

    }
} 