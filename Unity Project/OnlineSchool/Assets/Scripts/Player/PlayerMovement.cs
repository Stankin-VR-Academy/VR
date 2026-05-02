using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float lookSpeed = 2f;
    private Animator animator;

    private float rotationX = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(moveX, 0, moveZ) * speed * Time.deltaTime);

        if (Camera.main != null && Camera.main.transform.parent == transform)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

        }

        if (Mathf.Abs(moveZ) > 0.1f)
            animator.SetFloat("Speed", Mathf.Abs(moveZ));
        else if (Mathf.Abs(moveX) > 0.1f)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveX));
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
    }
}
