using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform player_tf;

    private void Start()
    {
        // カメラをプレイヤーに追従させる(X軸のみ)
        this.FixedUpdateAsObservable()
            .Subscribe(_ => transform.position = new Vector3(player_tf.position.x, 0f, -10f));
    }
}
