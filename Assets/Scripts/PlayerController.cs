using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public const float MAX_ENGINE_POWER = 6f;
    public const float MIN_ENGINE_POWER = 1f;
    float EnginePower = 0f;
    float Accel = 0f;
    float MaxSpeed; // +- 0 ~ 90f
    float CurSpeed = 0f;
    enum Gear { P, R, N, D1, D2, D3 };
    Gear CurGear = 0;
    Gear TempGear = 0;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("시작");
    }

    // Update is called once per frame
    void Update()
    {
        // 기어 변환
        if (Input.GetKey(KeyCode.LeftShift)) TempGear = shiftGear(TempGear);
        if (Input.GetKeyUp(KeyCode.LeftShift)) CurGear = TempGear;

        EngineControl();
        Accel = GetAcceleration() + GetExternalForce();
        SpeedCalculation();

        transform.Rotate(Vector3.up, RotateHandle() * CurSpeed * Time.deltaTime);

    }

    float RotateHandle()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        // if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)){

        // }
        // else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
        return horizontalInput;
    }

    void SpeedCalculation()
    {
        if (Input.GetKey(KeyCode.S)) // 브레이크
        {
            if (CurSpeed > 0.4f) CurSpeed -= 0.4f;
            else if (CurSpeed < -0.4f) CurSpeed += 0.4f;
            else CurSpeed = 0f;
        }
        else if (CurGear == Gear.P)
        {
            if (CurSpeed > 0.1f) CurSpeed -= 0.1f;
            else if (CurSpeed < -0.1f) CurSpeed += 0.1f;
            else CurSpeed = 0f;
        }
        else if (CurGear == Gear.R)
        {
            if (CurSpeed > MaxSpeed) CurSpeed += Accel;
        }
        else
        {
            if (CurSpeed < MaxSpeed) CurSpeed += Accel;
        }

        transform.Translate(Vector3.forward * Time.deltaTime * CurSpeed);
    }
    void EngineControl()
    {
        float nPower = EnginePower;
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            if (EnginePower < MAX_ENGINE_POWER)
            {
                nPower += 0.02f;
            }
        }
        else
        {
            // 자연 감소; 유지;
            if (EnginePower > MIN_ENGINE_POWER)
            {
                nPower -= 0.005f;
            }
            else
            {
                nPower += 0.005f;
            }
        }
        EnginePower = nPower;
    }

    float GetAcceleration()
    {
        // 엔진 출력와 기어비 계산
        float level;

        if (CurGear == Gear.R) level = -0.4f;
        else if (CurGear == Gear.D1) level = 0.4f;
        else if (CurGear == Gear.D2) level = 0.7f;
        else if (CurGear == Gear.D3) level = 1.0f;
        else level = 0f;
        MaxSpeed = EnginePower * level * 15;

        return EnginePower * level;
    }

    float GetExternalForce()
    {
        float force = 0f;
        if (CurGear == Gear.N)
        {
            if (CurSpeed > 0.02f) force = -0.02f;
            else if (CurSpeed < -0.02f) force = 0.02f;
        }
        return force;
    }

    Gear shiftGear(Gear gear)
    {
        // P, R, N, D1, D2, D3
        Gear nextGear = gear;

        if (Input.GetAxis("Mouse ScrollWheel") > 0 && nextGear != Gear.P)
        {
            nextGear--;
            Debug.Log("wheel up: " + nextGear);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && nextGear != Gear.D3)
        {
            nextGear++;
            Debug.Log("wheel down: " + nextGear);
        }
        return nextGear;
    }

}
