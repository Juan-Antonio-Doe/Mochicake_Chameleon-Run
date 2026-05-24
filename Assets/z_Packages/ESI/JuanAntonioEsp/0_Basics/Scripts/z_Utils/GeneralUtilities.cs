using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralUtilities {
	
    /// <summary>
    /// Returns the distance between two Vector3 points without the square root operation.
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="targetPos"></param>
    /// <returns></returns>
	public static float Vector3SqrDistance(Vector3 startPos, Vector3 targetPos) {
        return (startPos - targetPos).sqrMagnitude;
    }

    /// <summary>
    /// Returns the closest point from an array of Vector3 points to a current point.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="currentPoint"></param>
    /// <returns></returns>
    public static Vector3 GetClosestPoint(Vector3[] points, Vector3 currentPoint) {
        Vector3 pMin = Vector3.zero;
        float minDist = Mathf.Infinity;

        foreach (Vector3 p in points) {
            float dist = Vector3SqrDistance(p, currentPoint);
            if (dist < minDist) {
                pMin = p;
                minDist = dist;
            }
        }
        return pMin;
    }

    /// <summary>
    /// Returns a random number between minInclusive and maxExclusive.
    /// </summary>
    /// <param name="minInclusive"></param>
    /// <param name="maxExclusive"></param>
    /// <returns></returns>
    public static int GenerateRandomNumber(int minInclusive, int maxExclusive) {
        int seed = System.DateTime.Now.Millisecond + Time.frameCount;
        Random.InitState(seed);
        return Random.Range(minInclusive, maxExclusive);
    }

    /// <summary>
    /// Sets the cursor state. True for locked, false for unlocked.
    /// </summary>
    /// <param name="locked"></param>
    /// <returns></returns>
    public static void EnableDisableCursor(bool locked) {
        /*if (PauseManager.onPause)
            locked = false;*/

        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    /// <summary>
    /// Converts a LayerMask to its corresponding layer index. 
    /// Note that this method assumes the LayerMask contains only one layer.
    /// </summary>
    public static int ToLayerIndex(LayerMask mask) {
        return Mathf.RoundToInt(Mathf.Log(mask.value, 2));
    }

    /// <summary>
    /// Formats a time value in seconds to a string based on the specified format.
    /// </summary>
    /// <param name="time">Time in seconds.</param>
    /// <param name="format">The desired display format.</param>
    public static string FormatTime(float time, TimeFormat format = TimeFormat.MinutesSeconds) {
        return format switch {
            TimeFormat.SecondsOnly =>
                $"{(int)time:00}",

            TimeFormat.SecondsMilliseconds =>
                $"{(int)time:00}.{(int)(time * 1000f) % 1000:000}",

            TimeFormat.SecondsCentiseconds =>
                $"{(int)time:00}.{(int)(time * 100f) % 100:00}",

            TimeFormat.MinutesSeconds =>
                $"{(int)(time / 60f):00}:{(int)time % 60:00}",

            TimeFormat.MinutesSecondsMilliseconds =>
                $"{(int)(time / 60f):00}:{(int)time % 60:00}.{(int)(time * 1000f) % 1000:000}",

            TimeFormat.MinutesSecondsCentiseconds =>
                $"{(int)(time / 60f):00}:{(int)time % 60:00}.{(int)(time * 100f) % 100:00}",

            TimeFormat.HoursMinutesSeconds =>
                $"{(int)(time / 3600f):00}:{(int)(time % 3600f / 60f):00}:{(int)time % 60:00}",

            _ => $"{(int)(time / 60f):00}:{(int)time % 60:00}"
        };
    }
}

public enum TimeFormat {
    SecondsOnly,
    SecondsMilliseconds,
    SecondsCentiseconds,
    MinutesSeconds,
    MinutesSecondsMilliseconds,
    MinutesSecondsCentiseconds,
    HoursMinutesSeconds
}