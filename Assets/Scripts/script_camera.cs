using UnityEngine;
using Vuforia;

public class script_camera : MonoBehaviour
{
    void Start()
    {
        VuforiaBehaviour.Instance.enabled = true;
    }
}
