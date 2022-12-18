using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
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
                Managers.Resource.Destroy(gameObject);
        }
    }

    private Text _text;

    void Start()
    {
        _text = GetComponentInChildren<Text>();
        Hp = 10;
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            --Hp;
        }
    }
}
