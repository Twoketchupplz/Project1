/*
FIXME: 완전 제동 이후 엔진 RPM이 미쳐 낮아지지 않아 브레이크를 해제하면 급발진함 이게 정상작동인가?
엔진 RPM이 낮아지지않아도 관성에 의해 빠르게 출발하면 안됨. 실제와 비교 실제로 엔진을 미리 굴리면..
FIXME: 계산의 정확도를 위해 Update와 LateUpdate에 넣을 메소드를 구분
TODO
차종에 따른 그래픽, 엔진수준, 기어, 무게
가속도, 방향전환 속도, 브레이크 세기
현재속도 = 엔진파워 * 기어
*/

using UnityEngine;

public class VehicleController
{
    protected Transform VehicleTransform {get; set;}
    protected float MinEnginePower;
    protected float MaxEnginePower;
    protected enum Gear { P, R, N, D1, D2, D3 };
    // TODO: Struct Gear; P 없애야함
    // arrow로 계산할수 있어야하며 차종마다 다름; 수동은 파킹이 없으며, 기본적으로 5단까지 존재 Reverse는 따로 버튼도 존재; 후진을 위한 더블 클러치

    // NOTE: Vehicle common variables
    protected int ParkingBrake = 1;
    protected float EnginePower = 0f;
    protected float Accel = 0f;
    protected float MaxSpeed;
    protected float CurSpeed = 0f;
    protected Gear CurGear = 0;
    protected Gear TempGear = 0;

    public VehicleController(Transform trans)
    {
        this.VehicleTransform = trans;
        this.MinEnginePower = 5;
        this.MaxEnginePower = 50;
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
        CalculateCurSpeed();
        RotateHandle();
        // Debug.Log("Eng: " + ((int)(EnginePower*10))*0.1f + "; Spd: " + ((int)(CurSpeed*10))*0.1f + " Max: " + ((int)(MaxSpeed*10))*0.1f);
        Debug.Log("Eng: " + EnginePower + "; Spd: " + CurSpeed + " Max: " + MaxSpeed);
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
    protected void CalculateCurSpeed()
    {
        float parkingBrakeAmt = 1.0f;
        float pedalBrakeAmt = 0.6f;
        float gearParkingAmt = 0.4f;

        if (Input.GetKey(KeyCode.S)) // Brake Pedal
        {
            if (CurSpeed > pedalBrakeAmt) CurSpeed -= pedalBrakeAmt;
            else if (CurSpeed < -pedalBrakeAmt) CurSpeed += pedalBrakeAmt;
            else CurSpeed = 0f;
        }
        else if (ParkingBrake == 1) // Parking Brake
        {
            if (CurSpeed > parkingBrakeAmt) CurSpeed -= parkingBrakeAmt;
            else if (CurSpeed < -parkingBrakeAmt) CurSpeed += parkingBrakeAmt;
            else CurSpeed = 0f;
        }
        else
        {
            if (CurGear == Gear.P) // Gear P
            {
                if (CurSpeed > gearParkingAmt) CurSpeed -= gearParkingAmt;
                else if (CurSpeed < -gearParkingAmt) CurSpeed += gearParkingAmt;
                else CurSpeed = 0f;
            }
            else if (CurGear == Gear.R) // Gear Reverse
            {
                if (CurSpeed > MaxSpeed) CurSpeed += Accel;
                else CurSpeed = MaxSpeed;
            }
            else // Gear R, N, D
            {
                if (CurSpeed < MaxSpeed) CurSpeed += Accel;
                else CurSpeed = MaxSpeed;
            }
        }
        VehicleTransform.Translate(Vector3.forward * Time.deltaTime * CurSpeed);
    }
    protected void EngineControl()
    {
        // 엔진은 계속 돌아가는데 기어가 낮아졌을때 가속도가 너무 빠르면 안됨
        // 엔진이 다돌아가면 1단에서도 이미 가속도가 너무 빠름
        float nPower = EnginePower;
        float accPedalAmt = 0.4f;
        float naturalDecAmt = 0.1f; // 관성 이전 임시로 감소 속도 낮춤
        float naturalIncAmt = 0.2f;

        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            if (EnginePower < MaxEnginePower) nPower += accPedalAmt;
            else nPower = MaxEnginePower;
        }
        else
        {
            if (EnginePower > MinEnginePower) // EnginePower 자연 감소
            {
                nPower -= naturalDecAmt;
            }
            else if (EnginePower < MinEnginePower) // EnginePower 자연증가
            {
                nPower += naturalIncAmt;
            }
            else
            {
                nPower = MinEnginePower;
            }
        }
        EnginePower = nPower;
    }
    protected float GetAcceleration()
    {
        // 엔진 출력와 기어비 계산
        // TODO: MinSpeed가 필요함?
        float level;
        if (CurGear == Gear.R) level = -0.4f;
        else if (CurGear == Gear.D1) level = 0.4f;
        else if (CurGear == Gear.D2) level = 0.7f;
        else if (CurGear == Gear.D3) level = 1.0f;
        else level = 0f;
        MaxSpeed = EnginePower * level;

        return EnginePower * level;
    }
    protected float GetExternalForce() //FIXME: it doesn't work.
    // 관성을 추가해야함. 직전 속도를 가져와야함
    {
        float force = 0f;
        float extFrcAmt = 0.1f;
        if (CurGear == Gear.N)
        {
            if (CurSpeed > extFrcAmt) force = -extFrcAmt;
            else if (CurSpeed < -extFrcAmt) force = extFrcAmt;
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