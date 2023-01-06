using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_Ball : BaseController
{
    enum State
    {
        idle,
        shoot,
        move
    }
    State _state;

    //shoot
    Action<UI_Ball> _shootCallBack;
    float _startLine;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _state = State.idle;
        return true;
    }

    void Update()
    {
        if (_state == State.idle)
            return;

        switch(_state)
        {
            case State.shoot:
                {
                    Vector2 dir = GetComponent<Rigidbody2D>().velocity;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle - 90);
                    if (transform.localPosition.y <= _startLine)
                    {
                        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                        transform.localPosition = new Vector3(transform.localPosition.x, _startLine, 0);
                        transform.rotation = Quaternion.identity;
                        _state = State.idle;
                        _shootCallBack.Invoke(this);
                    }
                }
                break;
            case State.move:
                {
                    Vector3 dir = _dest - transform.localPosition;
                    if (dir.magnitude < 0.0001f)
                    {
                        _state = State.idle;
                    }
                    else
                    {
                        float delta = 1 + dir.magnitude / 400f;
                        float moveDist = Mathf.Clamp(1200f * Time.deltaTime * delta, 0, dir.magnitude);
                        transform.localPosition += dir.normalized * moveDist;
                    }
                }
                break;
        }
    }

    public override void Move(Vector3 dest)
    {
        base.Move(dest);
        Init();
        _state = State.move;
    }

    public void Shoot(Vector3 dir, float delta, float startLine, Action<UI_Ball> callBack)
    {
        Init();

        GetComponent<Rigidbody2D>().AddForce(dir.normalized * Managers.Game.BallSpeed * delta);
        _startLine = startLine;
        _shootCallBack = callBack;
        _state = State.shoot;
    }
}