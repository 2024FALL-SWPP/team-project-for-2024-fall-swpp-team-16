using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController_TutorialPlate : MonoBehaviour
{
    public PlateController plateBreakObstacle;
    public PlateController plateAdjustWater;
    public BreakableObject breakable;
    public WaterController water;
    
    private void Awake()
    {
        plateBreakObstacle.RegisterObserver(new ObserverDestroyObstacle(breakable));
        plateAdjustWater.RegisterObserver(new ObserverWaterIncrease(water, -2.2f));
    }
}
