using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Ball : UI_Base
{
    private float _speed;
    private Vector3 _destPos;

    private enum BallState
    {
        Idle,
        Move,
        Wait
    }
    private BallState _state;

    private float _initSpeed;

    public override void Init()
    {
        _speed = 6f;
        _initSpeed = 1200f;
        _state = BallState.Idle;
    }

    void Start()
    {
        Init();
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
        float distance = Mathf.Abs(transform.localPosition.x - _destPos.x);
        if (distance < 0.0001f)
        {
            return;
        }

        float delta = 1 + distance / 400f;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, _destPos, _initSpeed * Time.deltaTime * delta);
    }

    void UpdateMove()
    {
        Vector2 dir = GetComponent<Rigidbody2D>().velocity;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        if (transform.localPosition.y <= Managers.Game.Hamster.transform.localPosition.y - 60)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            transform.localPosition -= new Vector3(0, transform.localPosition.y - (Managers.Game.Hamster.transform.localPosition.y - 60), 0);
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
        GetComponent<Rigidbody2D>().AddForce(direction.normalized * _speed * Managers.Game.CanvasScale);
        _state = BallState.Move;
    }

    public void Ready(Vector3 dest)
    {
        _destPos = dest;
        _state = BallState.Idle;
    }
}