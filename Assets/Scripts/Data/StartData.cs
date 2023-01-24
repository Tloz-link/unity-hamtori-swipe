using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public class StartData
{
    [XmlAttribute]
    public int volume;

    [XmlAttribute]
    public int score;
    [XmlAttribute]
    public int fullBallCount;
    [XmlAttribute]
    public int ballSpeed;
    [XmlAttribute]
    public float hamsterPosX;
    [XmlAttribute]
    public float blockGapX;
    [XmlAttribute]
    public float blockGapY;
    [XmlAttribute]
    public float blockStartX;
    [XmlAttribute]
    public float blockStartY;

    // 스킬
    [XmlAttribute]
    public int lineCount;
    [XmlAttribute]
    public int ballDamage;
    [XmlAttribute]
    public int glassesFullColltime;
    [XmlAttribute]
    public int powerUpFullCooltime;
    [XmlAttribute]
    public int glassesValue;
    [XmlAttribute]
    public int powerUpValue;

    // 스킬 스프라이트 경로
    [XmlAttribute]
    public string glassesOnSpritePath;
    [XmlAttribute]
    public string glassesOffSpritePath;
    [XmlAttribute]
    public string powerUpOnSpritePath;
    [XmlAttribute]
    public string powerUpOffSpritePath;

    // 핵폭탄
    [XmlAttribute]
    public float nuclearStartRatio;
    [XmlAttribute]
    public int nuclearDivisionFullCount;

    // 배경 스프라이트 경로
    [XmlAttribute]
    public string backgroundFirstPath;
    [XmlAttribute]
    public string backgroundsecondPath;
    [XmlAttribute]
    public string backgroundLastPath;
}
