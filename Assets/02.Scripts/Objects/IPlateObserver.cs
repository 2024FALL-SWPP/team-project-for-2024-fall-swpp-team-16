using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlateObserver
{
    void OnPlateStateChanged(bool isPressed);
}


/* 구현 예시
 1. Object
 public class Door : MonoBehaviour, IPlateObserver
{
    public void OnPlateStateChanged(bool isPressed)
    {
        if (isPressed)
        {
            Debug.Log("Door is opening!");
            // 문 열기 로직
        }
        else
        {
            Debug.Log("Door is closing!");
            // 문 닫기 로직
        }
    }
}

2. Manager
public class PlayManager : MonoBehaviour
{
    public PlateController plateController;
    public Door door;

    void Start()
    {
        if (plateController != null && door != null)
        {
            plateController.RegisterObserver(door);
        }
    }
}

*/
