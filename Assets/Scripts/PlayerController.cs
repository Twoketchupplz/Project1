using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    bool ToggleKeyO = true; // Truck or Tank
	GameObject TankPrefab, TruckPrefab;

    VehicleController Vctrl;
    void Start()
    {
		TankPrefab = transform.GetChild(0).gameObject;
		TruckPrefab = transform.GetChild(1).gameObject;
		TankPrefab.SetActive(false);
		TruckPrefab.SetActive(true);
        Vctrl = new VehicleController(transform);
        Debug.Log("시작");
        Debug.Log("파킹브레이크를 풀어주세요.");
    }
    // Update is called once per frame
    void Update()
    {
        Vctrl.Move();
		if (Input.GetKeyDown(KeyCode.O)) SwitchVehicle(ToggleKeyO);
    }

    // TODO: 플레이어가 Vehicle을 선택
    void SwitchVehicle(bool toggle)
    {
        if (toggle) // change to Tank
        {
			Vctrl = new TankController(transform);
			// 프리펩 전환
			TruckPrefab.SetActive(false);
			TankPrefab.SetActive(true);
			ToggleKeyO  = false;
        }
        else // change to Truck
        {
            Vctrl = new VehicleController(transform);
            // 프리펩 전환
            TruckPrefab.SetActive(true);
            TankPrefab.SetActive(false);
			ToggleKeyO = true;
        }
    }
}
