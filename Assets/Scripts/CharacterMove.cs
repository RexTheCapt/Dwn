#region usings

using UnityEngine;

#endregion

public class CharacterMove : MonoBehaviour
{
    [Header("Movement")]
    public float Senstivity = 1;
    public KeyCode UpKeyCode;
    public KeyCode DownKeyCode;
    public KeyCode LeftKeyCode;
    public KeyCode RightKeyCode;

    void Start()
    {

    }

    void Update()
    {
        Vector3 moveVector3 = Vector3.zero;

        if (Input.GetKey(UpKeyCode))
            moveVector3.y += Senstivity * Time.deltaTime;

        if (Input.GetKey(DownKeyCode))
            moveVector3.y -= Senstivity * Time.deltaTime;

        if (Input.GetKey(LeftKeyCode))
            moveVector3.x -= Senstivity * Time.deltaTime;

        if (Input.GetKey(RightKeyCode))
            moveVector3.x += Senstivity * Time.deltaTime;

        transform.position += moveVector3;
    }
}