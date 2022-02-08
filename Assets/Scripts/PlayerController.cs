using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float MinEP;
	public float MaxEP;
	VehicleController Vctrl;
	// Start is called before the first frame update
	void Start()
	{
		Vctrl = new VehicleController(transform, MinEP, MaxEP);
		Debug.Log("시작");
		Debug.Log("파킹브레이크를 풀어주세요.");
	}
	// Update is called once per frame
	void Update()
	{
		Vctrl.Move();
	}
}
