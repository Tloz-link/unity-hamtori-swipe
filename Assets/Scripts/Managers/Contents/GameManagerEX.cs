using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEX
{
    public int CurrentBallCount { get; private set; }
    public int FullBallCount { get; private set; }
    private GameObject _hamster;
    private GameObject _shootRoot;
    private GameObject _waitRoot;

    private List<GameObject> _balls = new List<GameObject>();
    public Vector3 BallDirection { get; set; }

    public void Init()
    {
        CurrentBallCount = 5;
        FullBallCount = 5;
        _hamster = GameObject.Find("Hamster");
        _shootRoot = GameObject.Find("@SHOOT");
        _waitRoot = GameObject.Find("@WAIT");
        //todo : 못찾을 경우 자동으로 추가하기 (프로퍼티 쓰는것도 생각해야함)

        for (int i = 1; i <= FullBallCount; ++i)
        {
            GameObject ball = Managers.Resource.Instantiate("Contents/Ball", _waitRoot.transform);
            _balls.Add(ball);
            ball.transform.position = new Vector3((_hamster.transform.position.x - 50) - (50 * i), _hamster.transform.position.y - 50, 0);
            ball.transform.localPosition -= new Vector3(0, 0, ball.transform.localPosition.z);
        }
    }

    public void ShootBall()
    {
        GameObject ball = _balls[FullBallCount - CurrentBallCount];
        ball.transform.parent = _shootRoot.transform;
        ball.transform.position = _hamster.transform.position;
        ball.transform.localPosition -= new Vector3(0, 0, ball.transform.localPosition.z);
        ball.GetComponent<Ball>().Shoot(BallDirection);

        --CurrentBallCount;
    }
}

