using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Ball : UI_Spine
{
    public int Attack { get; set; }

    Action<UI_Ball> _shootCallback;
    Action<UI_Ball> _createCallback;

    Collider2D _target;
    Vector3 _lineEnd;
    Vector3 _dir;
    Vector3 _normal;

    float _startLine;

    Sequence _idleSequence;
    Sequence _createSequence;
    Sequence _rollSequence;

    //로컬 변환
    Vector2 _boardPos;
    float _canvasSize;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _shoot = false;
        return true;
    }

    #region DOTween
    public void CreateIdleSequence()
    {
        int posX = UnityEngine.Random.Range(-390, 390);
        float dir = (transform.localPosition.x - posX < 0) ? -180 : 180;
        PlayAnimation(Managers.Data.Spine.ballJump, false);

        _idleSequence.Kill();
        _idleSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .AppendInterval(0.4f)
            .Append(transform.DOLocalMoveX(posX, 1.0f).SetEase(Ease.Linear))
            .AppendInterval(1.0f)
            .AppendCallback(() =>
            {
                PlayAnimation(Managers.Data.Spine.ballIdle);

                int rand = UnityEngine.Random.Range(0, 100);
                if (rand <= 25)
                {
                    RefreshSequence();
                    CreateRollSequence();
                }
                else
                {
                    CreateIdleSequence();
                }
            });

        _idleSequence.Restart();
    }

    public void CreateCreateSequence(float duration)
    {
        _createSequence.Kill();
        _createSequence = DOTween.Sequence()
            .Append(transform.DOLocalMoveY(_startLine, duration).SetEase(Ease.Linear))
            .Join(transform.DORotate(new Vector3(0, 0, -360f), duration, RotateMode.FastBeyond360).SetEase(Ease.Linear))
            .OnComplete(() =>
            {
                GetComponent<CircleCollider2D>().enabled = true;
                CreateIdleSequence();
                _createCallback.Invoke(this);
            });
    }

    public void CreateRollSequence()
    {
        int rand;
        while (true)
        {
            rand = UnityEngine.Random.Range(-390, 390);
            if (Mathf.Abs(transform.localPosition.x - rand) > 80)
                break;
        }

        if (transform.localPosition.x - rand < 0)
            PlayAnimation(Managers.Data.Spine.ballRIghtRoll, false);
        else
            PlayAnimation(Managers.Data.Spine.ballLeftRoll, false);

        _rollSequence.Kill();
        _rollSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .AppendInterval(0.4f)
            .Append(transform.DOLocalMoveX((float)rand, 2.0f).SetEase(Ease.Linear))
            .AppendInterval(1.0f)
            .OnComplete(() =>
            {
                PlayAnimation(Managers.Data.Spine.ballIdle);
                CreateIdleSequence();
            });
        _rollSequence.Restart();
    }

    void RefreshSequence()
    {
        _idleSequence.Kill();
        _rollSequence.Kill();
        _createSequence.Kill();
        transform.rotation = Quaternion.identity;
    }
    #endregion

    public void SetInfo(Vector3 initPos, float startLine, Action<UI_Ball> shootCallback)
    {
        Init();

        PlayAnimation(Managers.Data.Spine.ballIdle);
        transform.localPosition = initPos;
        _startLine = startLine;
        _shootCallback = shootCallback;

        CreateIdleSequence();
    }

    private bool _shoot;
    private float _extra;
    private void FixedUpdate()
    {
        if (_shoot == true)
        {
            Vector3 delta = _lineEnd - transform.localPosition;
            float moveDist = (Managers.Game.BallSpeed * Time.deltaTime) + _extra;
            _extra = (moveDist > delta.magnitude) ? moveDist - delta.magnitude : 0;
            moveDist = Mathf.Clamp(moveDist, 0, delta.magnitude);
            transform.localPosition += delta.normalized * moveDist;

            delta = _lineEnd - transform.localPosition;
            if (delta.magnitude < 0.0001f)
            {
                transform.localPosition = _lineEnd;
                _dir = Vector3.Reflect(_dir, _normal);
                _dir = Utils.ClampDir(_dir);

                UI_Block block = _target.GetComponent<UI_Block>();
                if (block != null)
                {
                    Managers.Sound.Play(Define.Sound.Effect, "ballBounce");
                    block.Damaged(Attack);
                }

                CalcLine();
            }

            float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);

            if (transform.localPosition.y <= _startLine)
            {
                PlayAnimation(Managers.Data.Spine.ballIdle);

                transform.localPosition = new Vector3(transform.localPosition.x, _startLine, 0);
                transform.rotation = Quaternion.identity;
                CreateIdleSequence();
                _shootCallback.Invoke(this);
                _shoot = false;
            }
        }
    }

    public void Shoot(GameObject board, Vector3 dir, float canvasSize)
    {
        Init();

        _target = null;
        _dir = dir;
        _boardPos = board.transform.position;
        _canvasSize = canvasSize;
        CalcLine();

        RefreshSequence();
        PlayAnimation(Managers.Data.Spine.ballIdle);
        _shoot = true;
        _extra = 0;
    }

    public void Create(float duration, Action<UI_Ball> createCallback)
    {
        Init();

        GetComponent<CircleCollider2D>().enabled = false;
        transform.rotation = Quaternion.identity;
        _createCallback = createCallback;

        PlayAnimation(Managers.Data.Spine.ballIdle);
        RefreshSequence();
        CreateCreateSequence(duration);
        _createSequence.Restart();
    }

    public void CalcLine()
    {
        RaycastHit2D hit = Physics2D.CircleCast(_dir.normalized + transform.position, 50 * 0.8f * _canvasSize, _dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
        if (_target == hit.collider)
            return;

        _target = hit.collider;
        _lineEnd = (hit.centroid - _boardPos) / _canvasSize;
        _normal = hit.normal;
    }
}