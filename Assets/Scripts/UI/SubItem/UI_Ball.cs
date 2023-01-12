using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Ball : UI_Spine
{
    enum State
    {
        idle,
        shoot,
        move,
        create
    }
    State _state;

    //shoot
    Action<UI_Ball> _shootCallBack;
    Action<UI_Ball> _createCallBack;

    RaycastHit2D _hit;
    Vector3 _lineEnd;
    Vector3 _dir;
    Vector3 _normal;

    //로컬 변환
    Vector2 _boardPos;
    float _canvasSize;

    float _startLine;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _state = State.idle;
        return true;
    }

    public void SetInfo(Vector3 initPos, float startLine, Action<UI_Ball> shootCallBack, Action<UI_Ball> createCallBack)
    {
        PlayAnimation(Managers.Data.Spine.ballIdle);
        transform.localPosition = initPos;
        transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        _startLine = startLine;
        _shootCallBack = shootCallBack;
        _createCallBack = createCallBack;
    }

    protected override void Update()
    {
        if (_state == State.idle)
            return;

        switch(_state)
        {
            case State.create:
                {
                    transform.localPosition += Vector3.down * Time.deltaTime * 800.0f;
                    transform.Rotate(new Vector3(0, 0, -400.0f * Time.deltaTime));
                    if (transform.localPosition.y <= _startLine)
                    {
                        GetComponent<CircleCollider2D>().enabled = true;
                        transform.localPosition = new Vector3(transform.localPosition.x, _startLine, 0);
                        transform.rotation = Quaternion.identity;
                        _state = State.idle;
                        _createCallBack.Invoke(this);
                    }
                }
                break;
            case State.shoot:
                {
                    Vector3 delta = _lineEnd - transform.localPosition;
                    float moveDist = Mathf.Clamp(Managers.Game.BallSpeed * Time.deltaTime, 0, delta.magnitude);
                    transform.localPosition += delta.normalized * moveDist;

                    delta = _lineEnd - transform.localPosition;
                    if (delta.magnitude < 0.0001f)
                    {
                        _dir = Vector3.Reflect(_dir, _normal);

                        UI_Block block = _hit.collider.GetComponent<UI_Block>();
                        if (block != null)
                            block.Damaged();

                        CalcLine();
                    }

                    float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle - 90);

                    if (transform.localPosition.y <= _startLine)
                    {
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
        Init();

        _dest = dest;
        _state = State.move;
    }

    public void Shoot(GameObject board, Vector3 dir, float canvasSize)
    {
        Init();

        _dir = dir;
        _boardPos = board.transform.position;
        _canvasSize = canvasSize;
        CalcLine();
        _state = State.shoot;
    }

    public void Create(float delta)
    {
        Init();

        GetComponent<CircleCollider2D>().enabled = false;
        transform.rotation = Quaternion.identity;
        _state = State.create;
    }

    public void CalcLine()
    {
        _hit = Physics2D.CircleCast(_dir.normalized + transform.position, 50 * 0.8f * _canvasSize, _dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
        _lineEnd = (_hit.centroid - _boardPos) / _canvasSize;
        _normal = _hit.normal;
    }
}