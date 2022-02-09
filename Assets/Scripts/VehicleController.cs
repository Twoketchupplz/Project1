/*
FIXME: 최종속도가 엔진가속에 따라 2배수로 2번까지 증가함, 중간과정이 생략되어보임
TODO
차종 정보가 입력됨에 따라 변화해야함;
차종에 따른 그래픽, 엔진수준, 기어, 무게
가속도, 방향전환 속도, 브레이크 세기
*/

using UnityEngine;

public class VehicleController
{
    protected Transform VehicleTransform {get; set;}
    protected readonly float MIN_ENGINE_POWER; // 15f
    protected readonly float MAX_ENGINE_POWER; // 90f
    protected enum Gear { P, R, N, D1, D2, D3 };
    // TODO: Struct Gear;
    // arrow로 계산할수 있어야하며 차종마다 다름; 수동은 파킹이 없으며, 기본적으로 5단까지 존재 Reverse는 따로 버튼도 존재; 후진을 위한 더블 클러치

    // NOTE: Vehicle common variables
    protected int ParkingBrake = 1;
    protected float EnginePower = 0f;
    protected float Accel = 0f;
    protected float MaxSpeed; // +- 0 ~ 90f
    protected float CurSpeed = 0f;
    protected Gear CurGear = 0;
    protected Gear TempGear = 0;


    // NOTE: Constructor
    public VehicleController(Transform transform, float minEnginePower, float maxEnginePower)
    {
        this.VehicleTransform = transform;
        this.MIN_ENGINE_POWER = minEnginePower;
        this.MAX_ENGINE_POWER = maxEnginePower;
    }
    
    // NOTE: Methods
    public virtual void Move()
    {
        //사이드 브레이크 조작
        if (Input.GetKeyDown(KeyCode.KeypadEnter)) CtrlParkingBrake();
        // 기어변환, 클러치(LShift)를 떼어야 기어비 적용
        if (Input.GetKey(KeyCode.LeftShift)) TempGear = SetGear(TempGear);
        if (Input.GetKeyUp(KeyCode.LeftShift)) CurGear = TempGear;
        EngineControl();
        Accel = GetAcceleration() + GetExternalForce();
        SpeedCalculation();
        Debug.Log(CurSpeed);
        RotateHandle();
    }
    protected void RotateHandle()
    {
        float angle = 5f;
        float maxSpd = 40f;
        float horizontalInput = Input.GetAxis("Horizontal");
        float rotSpd = CurSpeed;

        if (rotSpd >= maxSpd) rotSpd = maxSpd;
        else if (rotSpd <= -maxSpd) rotSpd = -maxSpd;

        VehicleTransform.Rotate(Vector3.up, horizontalInput * angle * rotSpd * Time.deltaTime);
    }
    protected void SpeedCalculation()
    {
        float parkingBrakeLvl = 1.0f;
        float pedalBrakeLvl = 0.6f;
        float gearPLvl = 0.2f;

        // Brake Pedal
        if (Input.GetKey(KeyCode.S))
        {
            if (CurSpeed > pedalBrakeLvl) CurSpeed -= pedalBrakeLvl;
            else if (CurSpeed < -pedalBrakeLvl) CurSpeed += pedalBrakeLvl;
            else CurSpeed = 0f;
        }
        // Parking Brake
        else if (ParkingBrake == 1)
        {
            if (CurSpeed > parkingBrakeLvl) CurSpeed -= parkingBrakeLvl;
            else if (CurSpeed < -parkingBrakeLvl) CurSpeed += parkingBrakeLvl;
            else CurSpeed = 0f;
        }
        // Gear Parking
        else if (CurGear == Gear.P)
        {
            if (CurSpeed > gearPLvl) CurSpeed -= gearPLvl;
            else if (CurSpeed < -gearPLvl) CurSpeed += gearPLvl;
            else CurSpeed = 0f;
        }
        else if (CurGear == Gear.R)
        {
            if (CurSpeed > MaxSpeed) CurSpeed += Accel;
        }
        else
        {
            // 후진 중 중립기어에 대한 계산이 없음
            if (CurSpeed < MaxSpeed) CurSpeed += Accel;
        }
        VehicleTransform.Translate(Vector3.forward * Time.deltaTime * CurSpeed);
    }
    protected void EngineControl()
    {
        // TODO: nPower 계산 정확히
        float nPower = EnginePower;
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            if (EnginePower < MAX_ENGINE_POWER)
            {
                nPower += 0.3f;
            }
        }
        else
        {
            // 자연 감소; 유지;
            if (EnginePower > MIN_ENGINE_POWER)
            {
                nPower -= 0.15f;
            }
            else
            {
                nPower += 0.15f;
            }
        }
        EnginePower = nPower;
    }
    protected float GetAcceleration()
    {
        // 엔진 출력와 기어비 계산
        // TODO: MinSpeed가 필요함
        // NOTE
        // 엔진은 계속 돌아가는데 기어가 낮아졌을때 가속도가 너무 빠르면 안됨
        // 엔진이 다돌아가면 1단에서도 이미 가속도가 너무 빠름

        float level;

        if (CurGear == Gear.R) level = -0.4f;
        else if (CurGear == Gear.D1) level = 0.4f;
        else if (CurGear == Gear.D2) level = 0.7f;
        else if (CurGear == Gear.D3) level = 1.0f;
        else level = 0f;
        MaxSpeed = EnginePower * level;

        return EnginePower * level;
    }
    protected float GetExternalForce()
    {
        float force = 0f;
        float frcLvl = 0.05f;
        if (CurGear == Gear.N)
        {
            if (CurSpeed > frcLvl) force = -1f * frcLvl;
            else if (CurSpeed < -1f * frcLvl) force = frcLvl;
        }
        return force;
    }
    protected Gear SetGear(Gear gear)
    {
        // TODO: 차종에 따라 `Gear`가 다름, 기어 조작은 H스타일
        Gear nextGear = gear;
        if (Input.GetKeyDown(KeyCode.UpArrow) && nextGear != Gear.P)
        {
            nextGear--;
            Debug.Log("arrow up: " + nextGear);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && nextGear != Gear.D3)
        {
            nextGear++;
            Debug.Log("arrow down: " + nextGear);
        }
        return nextGear;
    }
    protected void CtrlParkingBrake()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (ParkingBrake == 0) { ParkingBrake = 1; Debug.Log("Side brake ON"); }
            else { ParkingBrake = 0; Debug.Log("Side brake OFF"); }
        }
    }

}