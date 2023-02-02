using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using static Define;


public class DataTransformer : EditorWindow
{
    [MenuItem("Tools/RemoveSaveData")]
    public static void RemoveSaveData()
    {
        string path = Application.persistentDataPath + "/SaveData.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("SaveFile Deleted");
        }
        else
        {
            Debug.Log("No SaveFile Detected");
        }
    }

    [MenuItem("Tools/ParseExcel")]
    public static void ParseExcel()
    {
        ParseStartData();
        ParseSpineData();
    }

    static void ParseStartData()
    {
        StartData startData;

        #region ExcelData
        string[] lines = Resources.Load<TextAsset>($"Data/Excel/StartData").text.Split("\n");

        // 두번째 라인까지 스킵
        string[] row = lines[2].Replace("\r", "").Split(',');

        startData = new StartData()
        {
            score = int.Parse(row[0]),
            fullBallCount = int.Parse(row[1]),
            ballSpeed = int.Parse(row[2]),
            hamsterPosX = float.Parse(row[3]),
            blockGapX = float.Parse(row[4]),
            blockGapY = float.Parse(row[5]),
            blockStartX = float.Parse(row[6]),
            blockStartY = float.Parse(row[7]),
            lineCount = int.Parse(row[8]),
            ballDamage = int.Parse(row[9]),
            glassesFullColltime = int.Parse(row[10]),
            powerUpFullCooltime = int.Parse(row[11]),
            glassesValue = int.Parse(row[12]),
            powerUpValue = int.Parse(row[13]),
            glassesOnSpritePath = row[14],
            glassesOffSpritePath = row[15],
            powerUpOnSpritePath = row[16],
            powerUpOffSpritePath = row[17],
            nuclearStartRatio = float.Parse(row[18]),
            nuclearDivisionFullCount = int.Parse(row[19]),
            backgroundFirstPath = row[20],
            backgroundsecondPath = row[21],
            backgroundLastPath = row[22],
            volume = int.Parse(row[23]),
            vibrate = bool.Parse(row[24]),
        };
        #endregion

        string xmlString = ToXML(startData);
        File.WriteAllText($"{Application.dataPath}/Resources/Data/StartData.xml", xmlString);
        AssetDatabase.Refresh();
    }

    static void ParseSpineData()
    {
        SpineData spineData;

        #region ExcelData
        string[] linesBlock = Resources.Load<TextAsset>($"Data/Excel/SpineData/Block").text.Split("\n");
        string[] linesBall = Resources.Load<TextAsset>($"Data/Excel/SpineData/Ball").text.Split("\n");
        string[] linesHamster = Resources.Load<TextAsset>($"Data/Excel/SpineData/Hamster").text.Split("\n");
        string[] linesPurple = Resources.Load<TextAsset>($"Data/Excel/SpineData/Purple").text.Split("\n");
        string[] linesStarlight = Resources.Load<TextAsset>($"Data/Excel/SpineData/Starlight").text.Split("\n");
        string[] linesTutoHamster = Resources.Load<TextAsset>($"Data/Excel/SpineData/TutoHamster").text.Split("\n");

        string[] block = linesBlock[2].Replace("\r", "").Split(',');
        string[] ball = linesBall[2].Replace("\r", "").Split(',');
        string[] hamster = linesHamster[2].Replace("\r", "").Split(',');
        string[] purple = linesPurple[2].Replace("\r", "").Split(',');
        string[] starlight = linesStarlight[2].Replace("\r", "").Split(',');
        string[] tutoHamster = linesStarlight[2].Replace("\r", "").Split(',');

        spineData = new SpineData()
        {
            blockIdle = block[0],
            blockTarget = block[1],
            blockDamaged = block[2],
            blockDestory = block[3],
            blockPrologue = block[4],

            ballIdle = ball[0],
            ballJump = ball[1],
            ballGameover = ball[2],

            hamsterIdle = hamster[0],
            hamsterCharge = hamster[1],
            hamsterShoot = hamster[2],
            hamsterWait = hamster[3],
            hamsterGameover = hamster[4],
            hamsterSeedAfter = hamster[5],
            hamsterSeedEat = hamster[6],
            hamsterPrologue = hamster[7],

            purpleIdle = purple[0],
            purpleCreate = purple[1],

            starlightIdle = starlight[0],
            starlightPlus = starlight[1],

            tutoHamsterIdle = tutoHamster[0],
            tutoHamsterTouch = tutoHamster[1],
        };
        #endregion

        string xmlString = ToXML(spineData);
        File.WriteAllText($"{Application.dataPath}/Resources/Data/SpineData.xml", xmlString);
        AssetDatabase.Refresh();
    }

    #region XML 유틸
    public sealed class ExtentedStringWriter : StringWriter
    {
        private readonly Encoding stringWriterEncoding;

        public ExtentedStringWriter(StringBuilder builder, Encoding desiredEncoding) : base(builder)
        {
            this.stringWriterEncoding = desiredEncoding;
        }

        public override Encoding Encoding
        {
            get
            {
                return this.stringWriterEncoding;
            }
        }
    }

    public static string ToXML<T>(T obj)
    {
        using (ExtentedStringWriter stringWriter = new ExtentedStringWriter(new StringBuilder(), Encoding.UTF8))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            xmlSerializer.Serialize(stringWriter, obj);
            return stringWriter.ToString();
        }
    }
    #endregion
}
