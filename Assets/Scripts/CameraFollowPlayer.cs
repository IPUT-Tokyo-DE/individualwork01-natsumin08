using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;
    private Vector3 offset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = gameObject.transform.position - Player.transform.position;    
    }

    // Update is called once per frame
    void LateUpdate()
    {
        gameObject.transform.position = Player.transform.position + offset;
    }
}
