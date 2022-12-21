using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hamster : MonoBehaviour
{
    private string[] _animationStates;
    private SkeletonAnimation _spine;
    private LineRenderer _line;

    enum Anims
    {
        charge_ing,
        charge_ready,
        idle1,
        idle2,
        seed_after,
        seed_eat
    }

    public enum HamState
    {
        Idle,
        Ready,
        Shoot,
        Wait
    }

    private HamState _state;
    public HamState State
    {
        get { return _state; }
        set
        {
            _state = value;
            switch (_state)
            {
                case HamState.Idle:
                    _spine.AnimationName = _animationStates[(int)Anims.idle1];
                    break;
                case HamState.Ready:
                    _spine.AnimationName = _animationStates[(int)Anims.charge_ing];
                    break;
                case HamState.Shoot:
                    _spine.AnimationName = _animationStates[(int)Anims.charge_ready];
                    break;
                case HamState.Wait:
                    _spine.AnimationName = _animationStates[(int)Anims.idle2];
                    break;
            }
        }
    }

    // shot delay
    private float _shotTick;
    private const float SHOT_DELAY = 0.1f;

    // Move ball first drop pos
    private float _initSpeed;
    private Vector3 _destPos;

    void Start()
    {
        _spine = GetComponent<SkeletonAnimation>();
        _line = GetComponent<LineRenderer>();
        _line.enabled = false;
        _animationStates = Enum.GetNames(typeof(Anims));
        Managers.Game.OnIdleHandler -= OnIdle;
        Managers.Game.OnIdleHandler += OnIdle;
        _initSpeed = 2000f;
        State = HamState.Idle;
    }

    void Update()
    {
        switch (_state)
        {
            case HamState.Idle:
                updateIdle();
                break;
            case HamState.Ready:
                updateReady();
                break;
            case HamState.Shoot:
                updateShoot();
                break;
            case HamState.Wait:
                updateWait();
                break;
        }
    }

    private void updateIdle()
    {
        _line.enabled = false;
        if (Input.GetMouseButtonDown(0))
        {
            State = HamState.Ready;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                State = HamState.Ready;
            }
        }
    }

    private void updateReady()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
        mousePos -= new Vector3(0, 0, mousePos.z - transform.position.z);
        Vector3 dir = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 15 || angle > 165)
        {
            State = HamState.Idle;
        }

        Vector3 dest = Physics2D.Raycast(transform.position, dir, 10000, 1 << LayerMask.NameToLayer("Wall")).centroid;
        _line.SetPosition(0, transform.position);
        _line.SetPosition(1, dest);
        _line.enabled = true;

        if (Input.GetMouseButtonUp(0))
        {
            _line.enabled = false;
            Managers.Game.BallDirection = dir;
            _shotTick = 0;
            State = HamState.Shoot;

            //todo (state 병합)
            Managers.Game.State = GameManagerEX.GameState.Shoot;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                Managers.Game.BallDirection = dir;
                _shotTick = 0;
                State = HamState.Shoot;

                //todo (state 병합)
                Managers.Game.State = GameManagerEX.GameState.Shoot;
            }
        }
    }

    private void updateShoot()
    {
        _shotTick += Time.deltaTime;
        if (_shotTick > SHOT_DELAY)
        {
            if (Managers.Game.CurrentBallCount == 0)
            {
                _destPos = transform.position;
                State = HamState.Wait;
                return;
            }
            Managers.Game.ShootBall();
            _shotTick = 0;
        }
    }

    private void updateWait()
    {
        //todo (테스트용 매니저 참조)
        transform.position = Vector3.MoveTowards(transform.position, _destPos, _initSpeed * Time.deltaTime);
    }

    public void ChangePos(Vector3 pos)
    {
        _destPos = pos;
    }

    private void OnIdle()
    {
        State = HamState.Idle;
    }
}
