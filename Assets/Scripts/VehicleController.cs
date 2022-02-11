/*
FIXME: 최종속도가 엔진가속에 따라 2배수로 2번까지 증가함, 중간과정이 생략되어보임
엔진 동작에 이상 없음, Accel 값에 이상 없음
{(EnginePower * GLevel) + External}
MaxSpeed와 CurSpeed가 최신화 순서가 이상한듯
!! 혹시 Update(), LateUpdate()에서 문제를 해결할 수 있지 않을까?
FIXME: 자연 속도 감소가 일어나지 않음
FIXME: 완전 제동 이후 엔진 RPM이 미쳐 낮아지지 않아 브레이크를 해제하면 급발진함 이게 정상작동인가?
TODO
차종에 따른 그래픽, 엔진수준, 기어, 무게
가속도, 방향전환 속도, 브레이크 세기
*/

using UnityEngine;

public class VehicleController
{
    protected Transform VehicleTransform {get; set;}
    protected float MinEnginePower; // 15f
    protected float MaxEnginePower; // 90f
    protected enum Gear { P, R, N, D1, D2, D3 };
    // TODO: Struct Gear; P 없애야함
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
    public VehicleController(Transform trans)
    {
        this.VehicleTransform = trans;
        this.MinEnginePower = 15;
        this.MaxEnginePower = 90;
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
        RotateHandle();
        // Debug.Log("Engine: " + EnginePower);
        // Debug.Log("Accel: " + Accel);
        // Debug.Log("MaxSpd: " + MaxSpeed);
        // Debug.Log("Speed: " + CurSpeed);
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
    protected void SpeedCalculation() // FIXME: 속도 문제 이곳에서 발생
    {
        float parkingBrakeLvl = 1.0f;
        float pedalBrakeLvl = 0.6f;
        float gearPLvl = 0.2f;

        if (Input.GetKey(KeyCode.S)) // Brake Pedal
        {
            if (CurSpeed > pedalBrakeLvl) CurSpeed -= pedalBrakeLvl;
            else if (CurSpeed < -pedalBrakeLvl) CurSpeed += pedalBrakeLvl;
            else CurSpeed = 0f;
        }
        else if (ParkingBrake == 1) // Parking Brake
        {
            if (CurSpeed > parkingBrakeLvl) CurSpeed -= parkingBrakeLvl;
            else if (CurSpeed < -parkingBrakeLvl) CurSpeed += parkingBrakeLvl;
            else CurSpeed = 0f;
        }
        else
        {
            if (CurGear == Gear.P) // Gear Parking
                {
                    if (CurSpeed > gearPLvl) CurSpeed -= gearPLvl;
                    else if (CurSpeed < -gearPLvl) CurSpeed += gearPLvl;
                    else CurSpeed = 0f;
                }
            else if (CurGear == Gear.R) // Gear Reverse
                {
                    if (CurSpeed > MaxSpeed) CurSpeed += Accel;
                }
            else // Gear Neutral and Driving FIXME:
            {
                    // 후진 중 중립기어에 대한 계산이 없음
                    if (CurSpeed < MaxSpeed) CurSpeed += Accel; // ?????
                    // Debug.Log("가속도 계산");
                }
        }
        Debug.Log("curspd"+CurSpeed);
        VehicleTransform.Translate(Vector3.forward * Time.deltaTime * CurSpeed);
        // Debug.Log("spd 적용");
    }
    protected void EngineControl()
    {
        // TODO: nPower 계산 정확히
        float nPower = EnginePower;
        float incRate = 0.2f;
        float naturalRate = 0.15f;
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            if (EnginePower < MaxEnginePower) nPower += incRate;
            else EnginePower = MaxEnginePower;
        }
        else
        {
            // 자연 감소; 유지;
            if (EnginePower > MinEnginePower)
            {
                nPower -= naturalRate;
            }
            else if (EnginePower < MinEnginePower)
            {
                nPower += naturalRate;
            }
            else
            {
                EnginePower = MinEnginePower;
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
        MaxSpeed = EnginePower * level; //FIXME: MaxSpeed의 의미에 맞게 변경; 가속 속도 제한이 풀려버릴것임

        return EnginePower * level;
    }
    protected float GetExternalForce() //FIXME: it doesn't work.
    {
        float force = 0f;
        float frcLvl = 0.1f;
        if (CurGear == Gear.N)
        {
            if (CurSpeed > frcLvl) force = -frcLvl;
            else if (CurSpeed < -frcLvl) force = frcLvl;
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