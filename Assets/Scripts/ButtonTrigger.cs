using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    public DoorController door;

    private bool hasActivated = false;
    private Vector3 originalPosition;
    public float pressDepth = 0.1f; // �ǂꂾ���ւ��ނ�
    public float pressSpeed = 5f;

    private bool isPressed = false;

    void Start()
    {
        originalPosition = transform.position;    
    }

    void Update()
    {
        if (isPressed)
        {
            // �ւ��񂾏�ԂփX���[�Y�Ɉړ�
            Vector3 target = originalPosition + Vector3.down * pressDepth;
            transform.position = Vector3.MoveTowards(transform.position, target, pressSpeed * Time.deltaTime);
        }
        else
        {
            // ���̈ʒu�֖߂�
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, pressSpeed * Time.deltaTime);
        }
    }


    private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
        if (hasActivated) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            var controller = collision.gameObject.GetComponent<PlayerController>();
            if (controller != null && controller.IsSquare())
            {
                hasActivated = true;
                isPressed = true;
                door.OpenDoor();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPressed = false;
        }
    }
}
