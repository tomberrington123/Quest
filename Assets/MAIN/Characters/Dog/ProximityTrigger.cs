using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityTrigger : MonoBehaviour
{
    public GameObject targetObject;        // The object to measure distance from
    public GameObject objectToDisable1;    // First object to disable
    public GameObject objectToDisable2;    // Second object to disable
    public GameObject objectToEnable;      // Object to enable
    public GameObject objectToEnable2;      // Object to enable

    public float triggerDistance = 5f;     // Distance threshold

    void Update()
    {
        if (targetObject == null) return;

        float distance = Vector3.Distance(transform.position, targetObject.transform.position);

        if (distance <= triggerDistance)
        {
            if (objectToDisable1 != null) objectToDisable1.SetActive(false);
            if (objectToDisable2 != null) objectToDisable2.SetActive(false);
            if (objectToEnable != null) objectToEnable.SetActive(true);
            if (objectToEnable2 != null) objectToEnable2.SetActive(true);
        }
    }
}