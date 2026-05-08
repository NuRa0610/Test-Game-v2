using Cinemachine;
using UnityEngine;

public class CinemachineInputBridge : MonoBehaviour
{
    private void OnEnable()
    {
        CinemachineCore.GetInputAxis = ReadAxis;
    }

    private void OnDisable()
    {
        if (CinemachineCore.GetInputAxis == ReadAxis)
        {
            CinemachineCore.GetInputAxis = null;
        }
    }

    private void LateUpdate()
    {
        CrossPlatformInput.EndFrame();
    }

    private float ReadAxis(string axisName)
    {
        return CrossPlatformInput.GetAxis(axisName);
    }
}
