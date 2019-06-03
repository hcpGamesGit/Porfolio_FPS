using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class MyDebug
    {
        public static void Log(object str)
        {
#if UNITY_EDITOR
            Debug.Log(str);
#endif
        }
        public static void LogFormat(string format, params object[] args)
        {
#if UNITY_EDITOR
            Debug.LogFormat( format,  args);
#endif
        }
    }
}