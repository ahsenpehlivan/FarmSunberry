using UnityEngine;

public class MovingScript : MonoBehaviour
{
    public float speed = 0.0f;
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            speed = 1.0f;
            Debug.Log("Hiz 1.0f");
        }
        else
        {
            speed = 0.0f;
            Debug.Log("Hiz 0.0f");
        }
    }
}