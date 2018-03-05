// ##############################################
//              WRITTEN BY: TALY
// ##############################################


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Supporting
{

    public static void Log(string message)
    {
        Log(message, 3);
    }

    public static void Log(string message, int level)
    {
        string debugMessage = string.Format("Time: {0}  - Frame: {1} >> {2}", System.DateTime.Now, Time.frameCount, message);

        if (level == 1)
        {
            Debug.LogError(debugMessage);
        }
        else if (level == 2)
        {
            Debug.LogWarning(debugMessage);
        }
        else
        {
            Debug.Log(debugMessage);
        }
    }

    public static bool CheckRequiredProperty(GameObject parent, GameObject toBeChecked, string label = "an object")
    {
        if (toBeChecked)
        {
            return true;
        }
        else
        {
            Log(string.Format("{0} - could not find {1}", parent.name, label), 1);
            return false;
        }
    }

    public static bool CheckRequiredProperty(GameObject parent, Component toBeChecked, string label = "an object")
    {
        if (toBeChecked)
        {
            return true;
        }
        else
        {
            Log(string.Format("{0} - could not find {1}", parent.name, label), 1);
            return false;
        }
    }

    public static bool CheckRequiredProperty(GameObject parent, Object toBeChecked, string label = "an object")
    {
        if (toBeChecked)
        {
            return true;
        }
        else
        {
            Log(string.Format("{0} - could not find {1}", parent.name, label), 1);
            return false;
        }
    }
}
