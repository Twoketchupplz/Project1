using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public const float MAX_ENGINE_POWER = 6f;
	public const float MIN_ENGINE_POWER = 1f;
	int ParkingBrake = 1;
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
		// 사이드 브레이크 조작
		if(Input.GetKeyDown(KeyCode.KeypadEnter)) CtrlParkingBrake();
		// 기어 변환
		if (Input.GetKey(KeyCode.LeftShift)) TempGear = ShiftGear(TempGear);
		if (Input.GetKeyUp(KeyCode.LeftShift)) CurGear = TempGear;

		EngineControl();
		Accel = GetAcceleration() + GetExternalForce();
		SpeedCalculation();
		RotateHandle();
	}

	void RotateHandle()
	{
		float angle = 5f;
		float spdLvl = 40f;
		float horizontalInput = Input.GetAxis("Horizontal");
		float rotSpd = CurSpeed;

		if (rotSpd >= spdLvl) rotSpd = spdLvl;
		else if (rotSpd <= -spdLvl) rotSpd = -spdLvl;

		transform.Rotate(Vector3.up, horizontalInput * angle * rotSpd * Time.deltaTime);
	}

	void SpeedCalculation()
	{
		float brakeLvl = 0.6f;
		float prkLvl = 0.2f;

		// Brake
		if (Input.GetKey(KeyCode.S))
		{
			if (CurSpeed > brakeLvl) CurSpeed -= brakeLvl;
			else if (CurSpeed < -brakeLvl) CurSpeed += brakeLvl;
			else CurSpeed = 0f;
		}
		// Gear Parking
		else if (CurGear == Gear.P)
		{
			if (CurSpeed > prkLvl) CurSpeed -= prkLvl;
			else if (CurSpeed < -prkLvl) CurSpeed += prkLvl;
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

		CurSpeed *= ParkingBrake;

		transform.Translate(Vector3.forward * Time.deltaTime * CurSpeed);
	}
	void EngineControl()
	{
		// 엑셀을 밟고 있는 표현이 가능하면 좋겠다
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
				nPower -= 0.01f;
			}
			else
			{
				nPower += 0.01f;
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
		float frcLvl = 0.05f;
		if (CurGear == Gear.N)
		{
			if (CurSpeed > frcLvl) force = -1f * frcLvl;
			else if (CurSpeed < -1f * frcLvl) force = frcLvl;
		}
		return force;
	}

	Gear ShiftGear(Gear gear)
	{
		// P, R, N, D1, D2, D3
		Gear nextGear = gear;
		// 키보드 화살표를 활용한 기어 변환
		// 스틱 기어 위치는 추후 H스타일로 변환
		// 키패드로 조작불가 화살표로 변경
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

		// 마우스 스크롤을 활용한 기어 변환
		// if (Input.GetAxis("Mouse ScrollWheel") > 0 && nextGear != Gear.P)
		// {
		// 	nextGear--;
		// 	Debug.Log("wheel up: " + nextGear);
		// }
		// else if (Input.GetAxis("Mouse ScrollWheel") < 0 && nextGear != Gear.D3)
		// {
		// 	nextGear++;
		// 	Debug.Log("wheel down: " + nextGear);
		// }

		return nextGear;
	}

	// TODO 사이드 속도가 점진적으로 줄어들어야함
	void CtrlParkingBrake(){
		if (ParkingBrake == 1) {ParkingBrake = 0; Debug.Log("Side brake ON");}
		else {ParkingBrake = 1; Debug.Log("Side brake OFF");}
	}
}
