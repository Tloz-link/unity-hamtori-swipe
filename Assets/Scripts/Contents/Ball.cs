using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private float _speed;
    private Vector3 _direction;

    private enum BallState
    {
        Idle,
        Move
    }
    private BallState _state;

    void Start()
    {
        _speed = 1000.0f;
        _state = BallState.Idle;
    }

    void Update()
    {
        switch (_state)
        {
            case BallState.Idle:
                UpdateIdle();
                break;
            case BallState.Move:
                UpdateMove();
                break;
        }
    }

    void UpdateIdle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
            _direction = (mousePos - transform.position).normalized;
            _state = BallState.Move;
        }
    }

    void UpdateMove()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Block")
        {
            Vector3 reflectVector = Vector3.Reflect(_direction, collision.contacts[0].normal);
            _direction = reflectVector;
        }
    }
}