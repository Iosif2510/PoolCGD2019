using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Rigidbody myRigidbody;
    Vector3 velocity;

    float red = 1;
    float green = 0;
    float blue = 0;

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

    public void ChangeColor(Material playerMat, GunController playerGC, char rgb)
    {
        switch (rgb)
        {
            case 'r':
                red = red == 1 ? 0 : 1;
                break;
            case 'g':
                green = green == 1 ? 0 : 1;
                break;
            case 'b':
                blue = blue == 1 ? 0 : 1;
                break;
            default: break;
        }
        Color changedColor = new Color(red, green, blue);
        playerMat.color = changedColor;
        playerGC.SetGunOwnerColor(changedColor);
    }
}
