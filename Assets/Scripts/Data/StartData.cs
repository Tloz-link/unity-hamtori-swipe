using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public class StartData
{
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
}
