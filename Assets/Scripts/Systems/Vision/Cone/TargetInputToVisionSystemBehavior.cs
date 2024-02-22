using System.Collections;
using System.Collections.Generic;
using Systems.Vision;
using UnityEngine;

public class TargetInputToVisionSystemBehavior : InputToVisionSystemBehavior
{
    public Vector3 targetPosition;

    // Update is called once per frame
    private void Update()
    {
        Vector3 characterPosition = transform.position;
        Vector3 direction = targetPosition - characterPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        visionSystemObject.transform.localRotation = Quaternion.Euler(0, 0, angle + 90);
    }
}
