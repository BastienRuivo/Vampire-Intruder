using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class Tools
    {
        public static Vector3 Lerp(Vector3 a, Vector3 b, float alpha)
        {
            return b * alpha + a * (1 - alpha);
        }
        
        /// <param name="worldSpace">Coordinates in world space</param>
        /// <returns>Coordinates in grid space</returns>
        public static Vector3 WorldToGridCoordinates(Vector3 worldSpace)
        {
            return new Vector3(worldSpace.x, worldSpace.y / 2, worldSpace.z);
        }
        
        /// <param name="gridSpace">Coordinates in grid space</param>
        /// <returns>Coordinates in world space</returns>
        public static Vector3 GridToWorldCoordinates(Vector3 gridSpace)
        {
            return new Vector3(gridSpace.x, gridSpace.y * 2, gridSpace.z);
        }
        
        /// <param name="worldSpace">Coordinates in world space</param>
        /// <returns>Coordinates in grid space</returns>
        public static Vector2 WorldToGridCoordinates(Vector2 worldSpace)
        {
            return new Vector2(worldSpace.x, worldSpace.y / 2);
        }

        public static float WorldToGridSpaceAngle(float thetaWorldSpace)
        {
            return ComputeAngle(new Vector2(0,0),
                GridToWorldCoordinates(RotateVector(new Vector2(1,0), thetaWorldSpace))
            );
        }

        public static float GridToWorldSpaceAngle(float thetaGridSpace)
        {
            return ComputeAngle(new Vector2(0, 0),
                WorldToGridCoordinates(RotateVector(new Vector2(1, 0), thetaGridSpace))
            );
        }

        public static float WorldToGridSpaceDistance(float distanceWorldSpace, float thetaGridSpace)
        {
            return WorldToGridCoordinates(
                RotateVector(new Vector2(1,0) * distanceWorldSpace, thetaGridSpace + 2*Mathf.PI)
                ).magnitude;    
        }
        
        public static float GridToWorldSpaceDistance(float distanceGridSpace, float thetaWorldSpace)
        {
            return GridToWorldCoordinates(
                RotateVector(new Vector2(1,0) * distanceGridSpace, thetaWorldSpace + 2*Mathf.PI)
            ).magnitude;    
        }
        
        /// <param name="gridSpace">Coordinates in grid space</param>
        /// <returns>Coordinates in world space</returns>
        public static Vector2 GridToWorldCoordinates(Vector2 gridSpace)
        {
            return new Vector2(gridSpace.x, gridSpace.y * 2);
        }
        
        /// <summary>
        /// Rotate a vector 2D from its origin
        /// </summary>
        /// <param name="vector">Vector to rotate</param>
        /// <param name="angleRadians">rotation angle (Rad)</param>
        /// <returns>Rotated vector</returns>
        public static Vector2 RotateVector(Vector2 vector, float angleRadians)
        {
            float cosTheta = (float)Math.Cos(angleRadians);
            float sinTheta = (float)Math.Sin(angleRadians);

            float newX = vector.x * cosTheta - vector.y * sinTheta;
            float newY = vector.x * sinTheta + vector.y * cosTheta;

            return new Vector2(newX, newY);
        }
        
        /// <summary>
        /// Compute angle between two points in (2D) space
        /// </summary>
        /// <param name="observer">origin</param>
        /// <param name="objective">angled point</param>
        /// <returns></returns>
        public static float ComputeAngle(Vector3 observer, Vector3 objective)
        {
            Vector3 direction = objective - observer;
            const float pi2 = Mathf.PI * 2;
            return (Mathf.Atan2(direction.y, direction.x) + pi2) % pi2;
        }
        
        /// <summary>
        /// Compute angle between two 2D points
        /// </summary>
        /// <param name="observer">origin</param>
        /// <param name="objective">angled point</param>
        /// <returns></returns>
        public static float ComputeAngle(Vector2 observer, Vector2 objective)
        {
            Vector2 direction = objective - observer;
            const float pi2 = Mathf.PI * 2;
            return (Mathf.Atan2(direction.y, direction.x) + pi2) % pi2;
        }
    }
}