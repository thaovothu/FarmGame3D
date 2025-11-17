using UnityEngine;

namespace FarmGame.UI
{
    /// <summary>
    /// Controller for camera movement with WASD or Arrow keys
    /// Moves camera on X and Z axis with boundary constraints
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Movement Settings")]
        [Tooltip("Camera movement speed")]
        [SerializeField] private float moveSpeed = 10f;

        [Header("Boundary Settings")]
        [Tooltip("Minimum X position camera can reach")]
        [SerializeField] private float minX = -20f;
        
        [Tooltip("Maximum X position camera can reach")]
        [SerializeField] private float maxX = 20f;
        
        [Tooltip("Minimum Z position camera can reach")]
        [SerializeField] private float minZ = -20f;
        
        [Tooltip("Maximum Z position camera can reach")]
        [SerializeField] private float maxZ = 20f;

        private void Update()
        {
            HandleCameraMovement();
        }

        private void HandleCameraMovement()
        {
            // Get input from WASD or Arrow keys
            float horizontal = 0f;
            float vertical = 0f;

            // WASD input
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                vertical = 1f;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                vertical = -1f;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                horizontal = -1f;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                horizontal = 1f;
            }

            // Calculate movement direction
            Vector3 movement = new Vector3(horizontal, 0f, vertical);
            
            // Apply movement
            if (movement.magnitude > 0.01f)
            {
                Vector3 newPosition = transform.position + movement.normalized * moveSpeed * Time.deltaTime;
                
                // Clamp position within boundaries
                newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
                newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
                
                transform.position = newPosition;
            }
        }

        // Optional: Visualize boundaries in Scene view
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            
            // Draw boundary rectangle at camera Y position
            float y = transform.position.y;
            
            // Bottom line
            Gizmos.DrawLine(new Vector3(minX, y, minZ), new Vector3(maxX, y, minZ));
            // Top line
            Gizmos.DrawLine(new Vector3(minX, y, maxZ), new Vector3(maxX, y, maxZ));
            // Left line
            Gizmos.DrawLine(new Vector3(minX, y, minZ), new Vector3(minX, y, maxZ));
            // Right line
            Gizmos.DrawLine(new Vector3(maxX, y, minZ), new Vector3(maxX, y, maxZ));
        }
    }
}
