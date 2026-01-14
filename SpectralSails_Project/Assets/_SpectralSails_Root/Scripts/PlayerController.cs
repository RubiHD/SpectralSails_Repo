using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float speedX = Input.GetAxis("Horizontal")*Time.deltaTime*speed;

        Vector2 position = transform.position;


        transform.position = new Vector2(speedX + position.x, position.y);
    }
}
