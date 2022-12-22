using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hamster : MonoBehaviour
{
    private string[] _animationStates;
    private SkeletonAnimation _spine;

    private List<GameObject> _stars = new List<GameObject>();
    private GameObject _arrowMoon;
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
        Clean
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
                    ClearLine();
                    Managers.Game.State = GameManagerEX.GameState.Shoot;
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

    // Clean Timer
    private float _cleanTick;
    private const float CLEAN_DELAY = 1.0f;

    void Start()
    {
        _spine = GetComponent<SkeletonAnimation>();
        _animationStates = Enum.GetNames(typeof(Anims));
        Managers.Game.OnIdleHandler -= OnIdle;
        Managers.Game.OnIdleHandler += OnIdle;
        Managers.Game.OnCleanHandler -= OnClean;
        Managers.Game.OnCleanHandler += OnClean;
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
            case HamState.Clean:
                updateClean();
                break;
        }
    }

    private void updateIdle()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Managers.Scene.LoadScene(Define.Scene.DevScene);
        }

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
        ClearLine();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
        mousePos -= new Vector3(0, 0, mousePos.z - transform.position.z);
        Vector3 dir = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 15 || angle > 165)
        {
            State = HamState.Idle;
            return;
        }

        Vector3 dest = Physics2D.CircleCast(transform.position, 40, dir, 10000, 1 << LayerMask.NameToLayer("Wall")).centroid;
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
        transform.position = Vector3.MoveTowards(transform.position, _destPos, _initSpeed * Time.deltaTime);
    }

    private void updateClean()
    {
        _cleanTick += Time.deltaTime;
        if (_cleanTick > CLEAN_DELAY)
        {
            Managers.Game.State = GameManagerEX.GameState.Idle;
        }
    }

    public void ChangePos(Vector3 pos)
    {
        _destPos = pos;
    }

    private void ClearLine()
    {
        if (_arrowMoon == null)
        {
            _arrowMoon = Managers.Resource.Instantiate("Contents/ArrowMoon", RootArrow.transform);
        }

        for (int i = 0; i < _stars.Count; ++i)
        {
            _stars[i].SetActive(false);
        }
        _arrowMoon.SetActive(false);
    }

    private void GenerateLine(Vector3 line, float angle)
    {
        float dist = line.magnitude;
        int need = (int)dist / 40;
        int lack = need - _stars.Count;
        for (int i = 0; i < lack; ++i)
        {
            GameObject star = Managers.Resource.Instantiate("Contents/ArrowStar", RootArrow.transform);
            _stars.Add(star);
            star.SetActive(false);
        }

        Debug.Assert(need <= _stars.Count, $"need : {need}   _starts.Count : {_stars.Count}");

        for (int i = 0; i < need; ++i)
        {
            _stars[i].transform.localPosition = new Vector3(0, dist - (need - i) * 40, 0);
            _stars[i].SetActive(true);

            if (i <= 2)
            {
                Color color = _stars[i].GetComponent<SpriteRenderer>().color;
                color.a = 0.4f;
                _stars[i].GetComponent<SpriteRenderer>().color = color;
            }
        }
        _arrowMoon.transform.localPosition = new Vector3(0, dist, 0);
        _arrowMoon.SetActive(true);

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
}
