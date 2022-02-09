/* 
TODO: 부모 타입의 인스턴스에서 자식인 TankCtrl클래스의 함수를 불러야함!
NOTE 캐스팅으로 해결 가능; 다이나믹하게 해보자; as is
근데 왜 되냐???

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