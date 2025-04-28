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

        // Dictionaryに形ごとのGameObjectを登録
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

        // 壁についてる間、落下速度を落とす
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

        // 壁に接触したらフラグを立てる
        if (collision.gameObject.CompareTag("Wall") && currentShape == ShapeType.Triangle)
        {
            isTouchingWall = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            // 最初の接触点の法線を取得（坂道の傾き）
            ContactPoint2D contact = collision.contacts[0];
            surfaceNormal = contact.normal;
            isOnslope = Vector2.Angle(surfaceNormal, Vector2.up) > 1f;  // 1度以上傾いてたら坂
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

        // カメラ左端チェック
        float cameraLeftEdge = Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect;
        float playerLeftBound = transform.position.x + (moveInput * Time.deltaTime * speed);

        // 移動方向が左、かつ画面外にでそうなら止める
        if (moveInput < 0 && playerLeftBound < cameraLeftEdge + 0.1f)
        {
            moveInput = 0;
        }

        // circleの挙動（スピード重視）
        if (currentShape == ShapeType.Circle)
        {
            speed *= 1.5f;

            if (isOnslope)
            {
                // 坂の角度に応じてスピード調整
                float slopeAngle = Vector2.Angle(surfaceNormal, Vector2.up);

                // 傾きと入力方向で補正を計算
                float slopeFactor = Mathf.Cos(slopeAngle * Mathf.Deg2Rad);

                if (moveInput != 0)
                {
                    // 進行方向と法線から坂の上下を判定
                    float slopeDirection = Mathf.Sign(surfaceNormal.x);  // 左上りなら正、右上りなら負
                    bool isUphill = Mathf.Sign(moveInput) == slopeDirection;

                    if (isUphill)
                    {
                        float slopeMultiplier = Mathf.Pow(slopeFactor, 3f);
                        speed *= slopeMultiplier;
                    }
                    else
                    {
                        // 右下り：スピードを強化
                        speed *= 1f + (1f - slopeFactor);
                    }
                    rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
                }
                else
                {
                    // 入力なし & 坂の上 → 下方向に滑らせる
                    Vector2 downSlopeDir = new Vector2(-surfaceNormal.y, surfaceNormal.x);  // 法線に垂直なベクトル
                    
                    // 下り方向かどうかを判定（重力方向と同じ向きか）
                    if (Vector2.Dot(downSlopeDir, Vector2.down) < 0)
                    {
                        downSlopeDir = -downSlopeDir;  // 逆向きだったら反転
                    }
                    float slideForce = 0.3f;
                    rb.AddForce(downSlopeDir.normalized * slideForce);
                }
            }
            else
            {
                // 通常移動
                rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
            }
        }
        else
        {
            // 〇以外の形の通常移動
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

                // triangleの挙動（ジャンプ重視）
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
        // 全て非表示にしてから、対象を表示
        foreach (var pair in shapeObjects)
        {
            pair.Value.SetActive(false);
        }

        shapeObjects[newShape].SetActive(true);
        currentShape = newShape;

        // squareの挙動（重さ重視）
        rb.gravityScale = (newShape == ShapeType.Square) ? 3f : 1f;

        // 回転をリセット
        Vector3 rotation = transform.eulerAngles;
        rotation.z = 0f;
        transform.eulerAngles = rotation;
    }

    public bool IsSquare()
    {
        return currentShape == ShapeType.Square;
    }
}
