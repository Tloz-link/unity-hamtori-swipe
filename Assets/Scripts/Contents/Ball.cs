using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private float _speed;
    private Vector3 _direction;
    private bool _colBlock;

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
            mousePos -= new Vector3(0, 0, mousePos.z);
            transform.position -= new Vector3(0, 0, transform.position.z);
            _direction = (mousePos - transform.position).normalized;
            _state = BallState.Move;
        }
    }

    void UpdateMove()
    {
        _colBlock = false;
        transform.position += _direction * _speed * Time.deltaTime;
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Block")
        {
            if (_colBlock == true)
                return;
            Vector3 reflectVector = Vector3.Reflect(_direction, collision.contacts[0].normal);
            _direction = reflectVector;
            _colBlock = true;
        }
    }
}