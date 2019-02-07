#region usings

using UnityEngine;

#endregion

public class FPSCounter : MonoBehaviour
{
    // Update is called once per frame
    private void Update()
    {
        Debug.Log((int)(1 / Time.deltaTime) + " - " + Time.deltaTime);
    }
}