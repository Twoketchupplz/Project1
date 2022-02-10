/* 
is as

발사 기능
Barrel = Turret.GetChild
*/

using UnityEngine;

public class TankController : VehicleController
{
    Transform TurretTransform;
    public TankController(Transform trans, float minPower, float maxPower) : base(trans, minPower, maxPower)
    {
        TurretTransform = VehicleTransform.GetChild(0).transform;
    }

    public override void Move()
    {
        base.Move();
        RotateTurret();
        Fire();
    }
    private void Fire()
    {
        // 사이즈를 줄이고
        // 서서히 복구한다
    }
    private void RotateTurret()
    {
        // <, > 키로 포신을 돌린다
        float spd = 10;
        if (Input.GetKey(KeyCode.Comma))
        {
            //rotate left
            TurretTransform.Rotate(Vector3.up, -spd * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.Period))
        {
            //rotate right
            TurretTransform.Rotate(Vector3.up, spd * Time.deltaTime);
        }
        // 위치는 고정
    }
}