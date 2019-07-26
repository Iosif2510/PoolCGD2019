using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Rigidbody myRigidbody;
    Vector3 velocity;

    public Color playerColor = new Color(0,0,0,1);
    public event System.Action onColorChange;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 _velocity) {
        velocity = _velocity;
    }

    public void LookAt(Vector3 lookPoint) {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

    void FixedUpdate() {
        myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
    }

    public void ChangeColor(Material playerMat, char rgb)
    {
        switch (rgb)
        {
            case 'r':
                playerColor.r = playerColor.r == 1 ? 0 : 1;
                break;
            case 'g':
                playerColor.g = playerColor.g == 1 ? 0 : 1;
                break;
            case 'b':
                playerColor.b = playerColor.b == 1 ? 0 : 1;
                break;
            default: break;
        }
        playerMat.color = playerColor;
        onColorChange();
    }
}
