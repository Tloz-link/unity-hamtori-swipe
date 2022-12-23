using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Block : UI_Base
{
    public int Id { get; set; }

    private int _hp;
    public int Hp
    {
        get { return _hp; }
        set
        {
            _hp = value;
            if (_text != null)
                _text.text = $"{_hp}";
            if (_hp <= 0)
                Managers.Game.RemoveBlock(Id);
        }
    }

    private Text _text;

    private bool _isClean;
    private Vector3 _movePos;
    private float _moveSpeed;

    public override void Init()
    {
    }

    private void Awake()
    {
        _text = GetComponentInChildren<Text>();
        _isClean = false;
        _moveSpeed = 1200;
    }

    void Update()
    {
        if (_isClean == false)
        {
            return;
        }

        Vector3 dir = _movePos - transform.position;
        if (dir.magnitude < 0.0001f)
        {
            _isClean = false;
        }
        else
        {
            float moveDist = Mathf.Clamp(_moveSpeed * Time.deltaTime, 0, dir.magnitude);
            transform.position += dir.normalized * moveDist;
        }
    }

    public void MoveNextPos()
    {
        _movePos = transform.position - new Vector3(0, 142, 0);
        _isClean = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            if (Hp > 0)
            {
                --Hp;
            }
        }
    }
}
