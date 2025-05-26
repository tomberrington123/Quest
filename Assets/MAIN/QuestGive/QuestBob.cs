using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBob : MonoBehaviour
{
    public float bounceHeight = 0.5f;
    public float bounceSpeed = 2f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        transform.position = new Vector3(startPosition.x, startPosition.y + newY, startPosition.z);
    }
}