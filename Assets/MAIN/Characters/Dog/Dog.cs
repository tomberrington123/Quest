using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour
{
    public Transform target;         // Usually the player
    public float followSpeed = 3f;
    public float stoppingDistance = 1.5f;

    public float bobbingHeight = 0.1f;
    public float bobbingSpeed = 6f;

    private Vector3 originalLocalPos;

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (target == null) return;

        // Move towards the player
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > stoppingDistance)
        {
            // Face the target
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0; // Don't tilt up/down
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

            // Move forward
            transform.position += transform.forward * followSpeed * Time.deltaTime;

            // Bobbing up and down (local position)
            Vector3 pos = transform.localPosition;
            pos.y = originalLocalPos.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
            transform.localPosition = pos;
        }
    }
}