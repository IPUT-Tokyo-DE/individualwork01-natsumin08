using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpPower = 5f;

    [HideInInspector] public bool canControl = true;

    public GameObject circle;
    public GameObject square;
    public GameObject triangle;
    private enum ShapeType { Circle, Square, Triangle };
    
    private Dictionary<ShapeType, GameObject> shapeObjects;
    private ShapeType currentShape;

    private Vector2 surfaceNormal = Vector2.up;
    private bool isJump = false;
    private bool isTouchingWall = false;
    private bool isOnslope = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Dictionary�Ɍ`���Ƃ�GameObject��o�^
        shapeObjects = new Dictionary<ShapeType, GameObject>()
        {
            { ShapeType.Circle, circle },
            { ShapeType.Square, square },
            { ShapeType.Triangle, triangle }
        };

        SetActiveShape(ShapeType.Circle);
    }

    void Update()
    {
        if (!canControl)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
       
        Move();
        Jump();
        ChangeShape();

        // �ǂɂ��Ă�ԁA�������x�𗎂Ƃ�
        if (currentShape == ShapeType.Triangle && isTouchingWall && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -0.5f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            isJump = false;
        }

        // �ǂɐڐG������t���O�𗧂Ă�
        if (collision.gameObject.CompareTag("Wall") && currentShape == ShapeType.Triangle)
        {
            isTouchingWall = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            // �ŏ��̐ڐG�_�̖@�����擾�i�⓹�̌X���j
            ContactPoint2D contact = collision.contacts[0];
            surfaceNormal = contact.normal;
            isOnslope = Vector2.Angle(surfaceNormal, Vector2.up) > 1f;  // 1�x�ȏ�X���Ă����
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") && currentShape == ShapeType.Triangle)
        {
            isTouchingWall = false;
        }

        if (collision.gameObject.CompareTag("Floor"))
        {
            surfaceNormal = Vector2.up;
            isOnslope = false;
        }
    }

    void Move()
    {
        float moveInput = Input.GetAxis("Horizontal");
        float speed = moveSpeed;

        if (!canControl) return;

        // �J�������[�`�F�b�N
        float cameraLeftEdge = Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect;
        float playerLeftBound = transform.position.x + (moveInput * Time.deltaTime * speed);

        // �ړ����������A����ʊO�ɂł����Ȃ�~�߂�
        if (moveInput < 0 && playerLeftBound < cameraLeftEdge + 0.1f)
        {
            moveInput = 0;
        }

        // circle�̋����i�X�s�[�h�d���j
        if (currentShape == ShapeType.Circle)
        {
            speed *= 1.5f;

            if (isOnslope)
            {
                // ��̊p�x�ɉ����ăX�s�[�h����
                float slopeAngle = Vector2.Angle(surfaceNormal, Vector2.up);

                // �X���Ɠ��͕����ŕ␳���v�Z
                float slopeFactor = Mathf.Cos(slopeAngle * Mathf.Deg2Rad);

                if (moveInput != 0)
                {
                    // �i�s�����Ɩ@�������̏㉺�𔻒�
                    float slopeDirection = Mathf.Sign(surfaceNormal.x);  // �����Ȃ琳�A�E���Ȃ畉
                    bool isUphill = Mathf.Sign(moveInput) == slopeDirection;

                    if (isUphill)
                    {
                        float slopeMultiplier = Mathf.Pow(slopeFactor, 3f);
                        speed *= slopeMultiplier;
                    }
                    else
                    {
                        // �E����F�X�s�[�h������
                        speed *= 1f + (1f - slopeFactor);
                    }
                    rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
                }
                else
                {
                    // ���͂Ȃ� & ��̏� �� �������Ɋ��点��
                    Vector2 downSlopeDir = new Vector2(-surfaceNormal.y, surfaceNormal.x);  // �@���ɐ����ȃx�N�g��
                    
                    // ����������ǂ����𔻒�i�d�͕����Ɠ����������j
                    if (Vector2.Dot(downSlopeDir, Vector2.down) < 0)
                    {
                        downSlopeDir = -downSlopeDir;  // �t�����������甽�]
                    }
                    float slideForce = 0.3f;
                    rb.AddForce(downSlopeDir.normalized * slideForce);
                }
            }
            else
            {
                // �ʏ�ړ�
                rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
            }
        }
        else
        {
            // �Z�ȊO�̌`�̒ʏ�ړ�
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        }
    }

    void Jump()
    {
        if (!canControl) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isJump || (currentShape == ShapeType.Triangle && isTouchingWall))
            {
                float jump = jumpPower;

                // triangle�̋����i�W�����v�d���j
                if (currentShape == ShapeType.Triangle)
                {
                    jump *= 1.5f;
                }

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
                isJump = true;
            }
            
        }
    }

    void ChangeShape()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SetActiveShape(ShapeType.Circle);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            SetActiveShape(ShapeType.Square);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            SetActiveShape(ShapeType.Triangle);
        }
    }
    
    void SetActiveShape(ShapeType newShape)
    {
        // �S�Ĕ�\���ɂ��Ă���A�Ώۂ�\��
        foreach (var pair in shapeObjects)
        {
            pair.Value.SetActive(false);
        }

        shapeObjects[newShape].SetActive(true);
        currentShape = newShape;

        // square�̋����i�d���d���j
        rb.gravityScale = (newShape == ShapeType.Square) ? 3f : 1f;

        // ��]�����Z�b�g
        Vector3 rotation = transform.eulerAngles;
        rotation.z = 0f;
        transform.eulerAngles = rotation;
    }

    public bool IsSquare()
    {
        return currentShape == ShapeType.Square;
    }
}
