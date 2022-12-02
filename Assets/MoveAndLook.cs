using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MoveAndLook : MonoBehaviour
{
    GameObject MainCamera = null;
    GameObject Character = null;
    private float xRotate, yRotate, xRotateMove, yRotateMove, zRotate;
    public float rotateSpeed = 0f;
    public int place = -1;
    public float Xsave,Ysave;
    // Start is called before the first frame update
    void Start()
    {
        MainCamera = GameObject.Find("Main Camera");
        Character = GameObject.Find("Character");
        xRotate = MainCamera.transform.eulerAngles.x;
        yRotate = MainCamera.transform.eulerAngles.y;
        zRotate = MainCamera.transform.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        if ((Xsave != Input.GetAxis("Mouse X") || Ysave != Input.GetAxis("Mouse Y")) && Input.GetMouseButton(1))
        {
            xRotateMove = -Input.GetAxis("Mouse Y") * Time.deltaTime * (Input.GetMouseButton(1) ? rotateSpeed : 0);
            yRotateMove = Input.GetAxis("Mouse X") * Time.deltaTime * (Input.GetMouseButton(1) ? rotateSpeed : 0);
            yRotate += yRotateMove;
            xRotate += xRotateMove;
            //xRotate = Mathf.Clamp(xRotate, -90, 90); // 위, 아래 고정
            MainCamera.transform.eulerAngles = new Vector3(xRotate, yRotate, zRotate);
            Xsave = Input.GetAxis("Mouse X");
            Ysave = Input.GetAxis("Mouse Y");
        }

        float speed = 0.05f;
        float PlayerRefAngle = MainCamera.transform.eulerAngles.y;
        float AngleToRadian = (PlayerRefAngle / 180) * Mathf.PI;
        Vector3 MovingVector = Vector3.zero;
        Vector3 LookVector = Vector3.zero;
        Vector3 RotationVector = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            LookVector += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            LookVector += -transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            LookVector += -transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            LookVector += transform.right;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            LookVector += transform.up*4;
        }


        float fMove = Time.deltaTime * speed;

        MovingVector = LookVector * speed;

        Character.transform.position += (MovingVector);
        MainCamera.transform.position = Character.transform.position;
        Character.transform.eulerAngles = new Vector3(0,MainCamera.transform.eulerAngles.y,0);//MainCamera.transform.rotation;
    }

}
