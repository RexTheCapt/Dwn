#region usings

using UnityEngine;

#endregion

public class KeepCameraOnObject : MonoBehaviour
{
    public Camera camera;

    void Update()
    {
        Vector3 pos = transform.position;
        pos.z = -100;

        camera.gameObject.transform.position = pos;
    }
}