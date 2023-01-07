using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public class SpineData
{
    //block
    [XmlAttribute]
    public string blockIdle;

    //ball
    [XmlAttribute]
    public string ballIdle;

    //hamster
    [XmlAttribute]
    public string hamsterIdle;
    [XmlAttribute]
    public string hamsterCharge;
    [XmlAttribute]
    public string hamsterShoot;
    [XmlAttribute]
    public string hamsterWait;
    [XmlAttribute]
    public string hamsterGameover;
    [XmlAttribute]
    public string hamsterSeedAfter;
    [XmlAttribute]
    public string hamsterSeedEat;

}
