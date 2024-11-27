using UnityEngine;

public class RockGlow : MonoBehaviour
{
    private Light _rockLight;
    public bool isGlowing = false;

    void Start()
    {
        _rockLight = GetComponent<Light>();
        if (_rockLight != null) 
        {
            _rockLight.enabled = isGlowing;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // for test
        {
            ToggleGlow();
        }
    }

    public void ToggleGlow()
    {
        if (_rockLight)
        {
            isGlowing = !isGlowing;
            _rockLight.enabled = isGlowing;
        }
    }
}
