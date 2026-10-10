using UnityEngine;

namespace Visu
{
    // VisuCameraFollow.cs
    // Last Update: April 20th, 2024
    // Author: Cassio Polegatto
    //
    // Description:
    // Enhance your projects with smooth and dynamic camera tracking functionality. 
    // This script ensures that the camera smoothly follows its target.
    // Integrate it into your projects and customize it to fit your specific needs.
    // Your support is greatly appreciated!
    // Thank you!

    public class VisuCameraFollow : MonoBehaviour
    {
        [Tooltip("The target object followed by the Camera")]
        public Transform target;
        [Tooltip("The camera that will follow the target. " +
                 "If empty, it will check if this object has a camera component. " +
                 "If not, then Main Camera is assigned automatically.")]
        public Camera myCamera;

        private Vector3 velocity = Vector3.zero;
        private Vector3 initialCameraPosition;
        public Vector3 cameraOffset;
        public float smoothFactor = 0.2f;

        void Awake()
        {
            // Check if target is set
            if (target == null)
            {
                Debug.LogError("Target not selected.");
                enabled = false; // Disable the script if target is not set
                return;
            }

            // Call the method to find the camera
            FindCamera();

            // Find the initial position of the camera
            initialCameraPosition = myCamera.transform.position - target.position;
        }

        void FindCamera()
        {
            // If Camera is already set, return
            if (myCamera != null)
                return;

            // Attempt to get the Camera component attached to this object
            if (TryGetComponent(out Camera foundCamera))
            {
                myCamera = foundCamera;
                Debug.Log("Camera assigned automatically to the component of the object.");
                return;
            }
            // Attempt to assign the main camera
            else if ((myCamera = Camera.main) != null)
            {
                Debug.Log("Main Camera assigned automatically.");
                return;
            }

            // If neither Camera component nor main camera is found, log an error
            Debug.LogError("No camera found!");
            enabled = false; // Disable the script if camera is not found
        }

        private void LateUpdate()
        {
            // Check if target or camera is null
            if (target == null || myCamera == null)
                return;

            // Find next position based on target's position
            Vector3 nextPosition = target.position + initialCameraPosition + cameraOffset;

            // Smooth movement
            Vector3 smoothedPosition = Vector3.SmoothDamp(myCamera.transform.position, nextPosition, ref velocity, smoothFactor);

            // Update camera position to follow target
            myCamera.transform.position = smoothedPosition;

        }
    }
}
