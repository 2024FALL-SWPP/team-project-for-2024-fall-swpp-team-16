using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PitchType
{
    Do = 0,
    Re = 1,
    Mi = 2,
    Fa = 3,
    So = 4,
    La = 5,
    Ti = 6
}

public static class GameParameters
{
    public static readonly Color[] PitchColors = {
        new Color(1f, 0f, 0f), // Red
        new Color(1f, 0.5f, 0f),  // Orange
        new Color(1f, 1f, 0f), // Yellow
        new Color(0f, 1f, 0f), // Green
        new Color(0f, 1f, 1f), // Cyan
        new Color(0f, 0f, 1f), // Blue
        new Color(0.56f, 0f, 1f) // Violet
    };

    public static readonly KeyCode[] PitchKeys =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7
    };
}

