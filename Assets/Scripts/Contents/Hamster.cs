using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Hamster : MonoBehaviour
{
    private string[] _animationStates;
    private SkeletonAnimation _spine;

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
    private float shotTick;
    private const float SHOT_DELAY = 0.1f;

    void Start()
    {
        _spine = GetComponent<SkeletonAnimation>();
        _animationStates = Enum.GetNames(typeof(Anims));
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
        if (Input.GetMouseButtonDown(0))
        {
            State = HamState.Ready;
        }
    }

    private void updateReady()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
            mousePos -= new Vector3(0, 0, mousePos.z);
            transform.position -= new Vector3(0, 0, transform.position.z);
            Managers.Game.BallDirection = (mousePos - transform.position).normalized;
            shotTick = 0;
            State = HamState.Shoot;
        }
    }

    private void updateShoot()
    {
        shotTick += Time.deltaTime;
        if (shotTick > SHOT_DELAY)
        {
            if (Managers.Game.CurrentBallCount == 0)
            {
                State = HamState.Wait;
                return;
            }
            Managers.Game.ShootBall();
            shotTick = 0;
        }
    }

    private void updateWait()
    {

    }
}
