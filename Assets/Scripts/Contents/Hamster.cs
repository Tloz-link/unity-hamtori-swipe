using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hamster : MonoBehaviour
{
    private string[] _animationStates;
    private SkeletonGraphic _spine;

    private List<UI_ArrowStar> _stars = new List<UI_ArrowStar>();
    private UI_ArrowMoon _arrowMoon;
    private GameObject _rootArrow;
    public GameObject RootArrow
    {
        get
        {
            if (_rootArrow == null)
                _rootArrow = GameObject.Find("@ArrowList");
            return _rootArrow;
        }
    }

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
        Wait,
        Clean,

        Over
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
                    _spine.AnimationState.SetAnimation(0, _animationStates[(int)Anims.idle1], true);
                    break;
                case HamState.Ready:
                    _spine.AnimationState.SetAnimation(0, _animationStates[(int)Anims.charge_ing], true);
                    break;
                case HamState.Shoot:
                    BeginShoot();
                    _spine.AnimationState.SetAnimation(0, _animationStates[(int)Anims.charge_ready], true);
                    break;
                case HamState.Wait:
                    _spine.AnimationState.SetAnimation(0, _animationStates[(int)Anims.idle2], true);
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

    // Clean Timer
    private float _cleanTick;
    private const float CLEAN_DELAY = 0.2f;

    void Start()
    {
        _spine = GetComponent<SkeletonGraphic>();
        _animationStates = Enum.GetNames(typeof(Anims));

        Managers.Game.OnIdleHandler -= OnIdle;
        Managers.Game.OnIdleHandler += OnIdle;
        Managers.Game.OnCleanHandler -= OnClean;
        Managers.Game.OnCleanHandler += OnClean;
        Managers.Game.OnOverHandler -= OnOver;
        Managers.Game.OnOverHandler += OnOver;

        _initSpeed = 2000f;
        State = HamState.Idle;
    }

    #region Update
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
            case HamState.Clean:
                updateClean();
                break;
        }
    }

    private void updateIdle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(checkValidInput() == false)
            {
                return;
            }

            State = HamState.Ready;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (checkValidInput() == false)
                {
                    return;
                }

                State = HamState.Ready;
            }
        }
    }

    private bool checkValidInput()
    {
        Vector3 floorPos = Managers.Game.BoardFloorPos;
        float minX = floorPos.x - 250 * Managers.Game.CanvasScale;
        float maxX = floorPos.x + 250 * Managers.Game.CanvasScale;

        Vector3 mousePos = Input.mousePosition;
        if (mousePos.y >= floorPos.y || mousePos.x < minX || mousePos.x > maxX)
        {
            return false;
        }
        return true;
    }

    private void updateReady()
    {
        ClearLine();

        Vector3 floorPos = Managers.Game.BoardFloorPos;
        Vector3 mousePos = Input.mousePosition;
        float ratio = (mousePos.x - (floorPos.x - 250 * Managers.Game.CanvasScale)) / (500 * Managers.Game.CanvasScale);
        float angle = 10f + (160f * ratio);
        if (angle < 10f || angle > 170f || mousePos.y >= floorPos.y)
        {
            State = HamState.Idle;
            return;
        }

        Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        Vector3 dest = Physics2D.CircleCast(transform.position, 40 * Managers.Game.CanvasScale, dir, 10000, 1 << LayerMask.NameToLayer("Wall")).centroid;
        GenerateLine((dest - transform.position).Get2D(), angle);

        if (Input.GetMouseButtonUp(0))
        {
            Managers.Game.BallDirection = dir;
            _shotTick = 0;
            State = HamState.Shoot;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                Managers.Game.BallDirection = dir;
                _shotTick = 0;
                State = HamState.Shoot;
            }
        }
    }

    private void updateShoot()
    {
        _shotTick += Time.deltaTime;
        if (_shotTick > SHOT_DELAY)
        {
            if (Managers.Game.ShootBallCount == Managers.Game.FullBallCount)
            {
                State = HamState.Wait;
                return;
            }
            Managers.Game.ShootBall();
            _shotTick = 0;
        }

        updateWait();
    }

    private void updateWait()
    {
        float distance = Mathf.Abs(transform.localPosition.x - _destPos.x);
        if (distance < 0.0001f)
        {
            return;
        }

        float delta = 1 + distance / 300f;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, _destPos, _initSpeed * Time.deltaTime * delta);
    }

    private void updateClean()
    {
        _cleanTick += Time.deltaTime;
        if (_cleanTick > CLEAN_DELAY)
        {
            _cleanTick = 0f;
            if (Managers.Game.CheckBlocks() == false)
            {
                return;
            }
            Managers.Game.State = GameManagerEX.GameState.Idle;
        }
    }
    #endregion

    public void ChangePos(Vector3 pos)
    {
        _destPos = pos;
    }

    private void BeginShoot()
    {
        ClearLine();
        Managers.Game.ShootBallCount = 0;
        Managers.Game.ReturnBallCount = 0;
        Managers.Game.ShootPos = transform.localPosition;
        _destPos = transform.localPosition;
        Managers.Game.State = GameManagerEX.GameState.Shoot;
    }

    private void ClearLine()
    {
        if (_arrowMoon == null)
        {
            _arrowMoon = Managers.UI.makeSubItem<UI_ArrowMoon>(RootArrow.transform);
        }

        for (int i = 0; i < _stars.Count; ++i)
        {
            _stars[i].gameObject.SetActive(false);
        }
        _arrowMoon.gameObject.SetActive(false);
    }

    private void GenerateLine(Vector3 line, float angle)
    {
        float dist = line.magnitude / Managers.Game.CanvasScale;
        int need = (int)dist / 40;
        int lack = need - _stars.Count;
        for (int i = 0; i < lack; ++i)
        {
            UI_ArrowStar star = Managers.UI.makeSubItem<UI_ArrowStar>(RootArrow.transform);
            _stars.Add(star);
            star.gameObject.SetActive(false);
        }

        Debug.Assert(need <= _stars.Count, $"need : {need}   _starts.Count : {_stars.Count}");

        for (int i = 0; i < need; ++i)
        {
            _stars[i].transform.localPosition = new Vector3(0, dist - (need - i) * 40, 0);
            _stars[i].transform.localRotation = Quaternion.identity;
            _stars[i].gameObject.SetActive(true);

            if (i <= 2)
            {
                Color color = _stars[i].GetComponent<Image>().color;
                color.a = 0.4f;
                _stars[i].GetComponent<Image>().color = color;
            }
        }
        _arrowMoon.transform.localPosition = new Vector3(0, dist, 0);
        _arrowMoon.gameObject.SetActive(true);

        RootArrow.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }

    private void OnClean()
    {
        State = HamState.Clean;
    }

    private void OnIdle()
    {
        State = HamState.Idle;
    }

    private void OnOver()
    {
        State = HamState.Over;
    }
}
