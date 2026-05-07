using UnityEngine;

public class RemotePositionSmoother : MonoBehaviour
{
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 velocityRef;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    public void SetTarget(Vector3 newPos, Quaternion newRot)
    {
        targetPosition = newPos;
        targetRotation = newRot;
    }

    void Update()
    {
        // Плавное движение
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocityRef, 0.1f);

        // Плавный поворот
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
}