using UnityEngine;

namespace Systems.Vision
{
    public abstract class VisionShapeController : MonoBehaviour
    {
        /// <remarks>Must validate the target with HasRefreshability first.</remarks>
        /// <param name="target">a target position.</param>
        /// <returns>True if the cone target is visible to the cone</returns>
        public abstract bool HasVisibility(Vector3 target);

        /// <summary>
        /// Check if it is worth refreshing the visibility cone.
        /// </summary>
        /// <param name="target">a target position</param>
        /// <returns>true if it is worth refreshing the cone.</returns>
        public abstract bool HasRefreshability(Vector3 target);

        /// <summary>
        /// Notify the collider shape that it should be enabled.
        /// </summary>
        public abstract void Enable();

        /// <summary>
        /// Notify the collider shape that it should be disabled.
        /// </summary>
        public abstract void Disable();

        /// <returns>true is the collider shape is currently enabled.</returns>
        public abstract bool IsEnabled();

    }
}