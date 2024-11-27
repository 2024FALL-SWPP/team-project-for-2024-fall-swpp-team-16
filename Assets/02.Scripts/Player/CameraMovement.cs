using UnityEngine;

namespace _12.Tests.PlayerDev
{
    public class CameraMovement : MonoBehaviour
    {
        private void Start()
        {
            // locks cursor and makes it invisible
            Cursor.lockState = Cursor.lockState;
            Cursor.visible = false;
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
