using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Ball : UI_Spine
{
    Action<UI_Ball> _callBack;

    RaycastHit2D _hit;
    Vector3 _lineEnd;
    Vector3 _dir;
    Vector3 _normal;

    float _startLine;

    Sequence _idleSequence;
    Sequence _createSequence;
    Sequence _rollSequence;

    //���� ��ȯ
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
        _idleSequence.Kill();
        _idleSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .AppendInterval(0.4f)
            .Append(transform.DOLocalMoveY(60, 0.3f).SetRelative().SetEase(Ease.OutQuad))
            .Append(transform.DOLocalMoveY(-60, 0.3f).SetRelative().SetEase(Ease.InQuad))
            .AppendCallback(() =>
            {
                int rand = UnityEngine.Random.Range(0, 100);
                if (rand <= 25)
                {
                    RefreshAnim();
                    CreateRollSequence();
                    _rollSequence.Restart();
                }
            })
            .AppendInterval(1.0f)
            .AppendCallback(() => { _idleSequence.Restart(); });
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
                _idleSequence.Restart();
                _callBack.Invoke(this);
            });
    }

    public void CreateRollSequence()
    {
        int rand;
        while (true)
        {
            rand = UnityEngine.Random.Range(-400, 400);
            if (Mathf.Abs(transform.localPosition.x - rand) > 80)
                break;
        }
        float dir = (transform.localPosition.x - rand < 0) ? -360 : 360;

        _rollSequence.Kill();
        _rollSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Append(transform.DOLocalMoveX((float)rand, 2.0f).SetEase(Ease.Linear))
            .Join(transform.DORotate(new Vector3(0, 0, dir), 2.0f, RotateMode.FastBeyond360).SetEase(Ease.Linear))
            .OnComplete(() =>
            {
                transform.rotation = Quaternion.identity;
                _idleSequence.Restart();
            });
    }

    void RefreshAnim()
    {
        _idleSequence.Pause();
        _rollSequence.Pause();
        _createSequence.Pause();
        transform.rotation = Quaternion.identity;
    }
    #endregion

    public void SetInfo(Vector3 initPos, float startLine, Action<UI_Ball> callBack)
    {
        Init();

        PlayAnimation(Managers.Data.Spine.ballIdle);
        transform.localPosition = initPos;
        transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        _startLine = startLine;
        _callBack = callBack;

        CreateIdleSequence();
        _idleSequence.Restart();
    }

    private bool _shoot;
    protected override void Update()
    {
        if (_shoot == true)
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
                _idleSequence.Restart();
                _callBack.Invoke(this);
                _shoot = false;
            }
        }
    }

    public void Shoot(GameObject board, Vector3 dir, float canvasSize)
    {
        Init();

        _dir = dir;
        _boardPos = board.transform.position;
        _canvasSize = canvasSize;
        CalcLine();

        RefreshAnim();
        _shoot = true;
    }

    public void Create()
    {
        Init();

        GetComponent<CircleCollider2D>().enabled = false;
        transform.rotation = Quaternion.identity;

        RefreshAnim();
        CreateCreateSequence(1.0f);
        _createSequence.Restart();
    }

    public void CalcLine()
    {
        _hit = Physics2D.CircleCast(_dir.normalized + transform.position, 50 * 0.8f * _canvasSize, _dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
        _lineEnd = (_hit.centroid - _boardPos) / _canvasSize;
        _normal = _hit.normal;
    }
}