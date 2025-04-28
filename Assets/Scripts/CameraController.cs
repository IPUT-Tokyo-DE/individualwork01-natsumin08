using UnityEngine;
using UnityEngine.InputSystem.XR;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;
    public float horizontalMargin = 0.5f;

    private Camera cam;
    private Vector3 targetPos;
    private PlayerController playerController;
    private bool isMoving = false;
    private float cameraWidth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
        playerController = player.GetComponent<PlayerController>();
        cameraWidth = cam.orthographicSize * cam.aspect * 2f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player == null || playerController == null) return;

        if (isMoving)
        {
            // カメラ移動中
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                transform.position = targetPos;
                isMoving = false;
                playerController.canControl = true;
            }
            return;
        }
        
        Vector3 camPos = transform.position;
        Vector3 playerPos = player.position;
        float halfWidth = cameraWidth / 2f;
        float rightEdge = camPos.x + halfWidth - horizontalMargin;

        // プレイヤーが画面右端に接したら
        if (playerPos.x >= rightEdge && !isMoving)
        {
            playerController.canControl = false;

            // プレイヤーを画面左端に強制移動
            targetPos = new Vector3(camPos.x + cameraWidth, camPos.y, camPos.z);
            isMoving = true;
        }
    }

}
