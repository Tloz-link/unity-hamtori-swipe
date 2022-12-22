using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Ball : MonoBehaviour
{
    private float _speed;
    private Vector3 _direction;
    private Vector3 _destPos;

    // todo (뭔가 다른 방식으로 고쳐야 할듯)
    private bool _colBlock;

    private enum BallState
    {
        Idle,
        Move,
        Wait
    }
    private BallState _state;

    private float _initSpeed;

    void Start()
    {
        _speed = 1000.0f;
        _initSpeed = 1200f;
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
            case BallState.Wait:
                UpdateWait();
                break;
        }
    }

    void UpdateIdle()
    {
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.MoveTowards(transform.position, _destPos, _initSpeed * Time.deltaTime);
    }

    void UpdateMove()
    {
        _colBlock = false;
        transform.position += _direction * _speed * Time.deltaTime;
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

        // 땅에 도착
        if (transform.position.y <= Managers.Game.Hamster.transform.position.y - 60)
        {
            transform.position -= new Vector3(0, transform.position.y - (Managers.Game.Hamster.transform.position.y - 60), 0);
            _state = BallState.Wait;
            Managers.Game.AddBall(this);
        }
    }

    void UpdateWait()
    {
        transform.rotation = Quaternion.identity;
    }


    public void Shoot(Vector3 direction)
    {
        _direction = direction;
        _state = BallState.Move;
    }

    public void Ready(Vector3 dest)
    {
        _destPos = dest;
        _state = BallState.Idle;
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