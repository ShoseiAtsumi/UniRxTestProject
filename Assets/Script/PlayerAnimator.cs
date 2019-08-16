using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Player.Contoroller;

namespace Player.Amimator
{
    public class PlayerAnimator : MonoBehaviour
    {
        #region serializePrivate
        [SerializeField]
        private PlayerController playerController;

        [SerializeField]
        private Rigidbody2D rb;

        [SerializeField]
        private Animator animator;
        #endregion

        #region private
        // アニメーションステート
        private ReactiveProperty<int> animatorState = new ReactiveProperty<int>(0);
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // プレイヤーの向きに応じてスプライトを反転
            this.UpdateAsObservable()
                .Subscribe(_ => Rotate());

            // animatorStateの値が変化したとき アニメーターに変化した値を投げる
            animatorState
                .Subscribe(i => animator.SetInteger("State", i));

            // ステート更新
            this.UpdateAsObservable()
                .Subscribe(_ => State());
        }

        // スプライトの反転
        private void Rotate()
        {
            // xの速度がマイナスの時反転させる
            if (rb.velocity.x < 0)
            {
                transform.rotation = new Quaternion(0, -180, 0, 1);
            }
            else
            {
                transform.rotation = new Quaternion(0, 0, 0, 0);
            }
        }

        // ステート
        private void State()
        {
            //何もないときは0
            animatorState.Value = 0;

            //  横の移動速度が一定値以上 1 (Run)
            if (Mathf.Abs(rb.velocity.x) >= 0.5f)
            {
                animatorState.Value = 1;
            }
            // 地面にいないとき 2 (Jump)
            if (!playerController.isGround)
            {
                animatorState.Value = 2;
            }
            // プレイヤーが死んでいたら 4 (Dead)
            if (!playerController.playerAlive.Value)
            {
                animatorState.Value = 4;
            }
        }
    }
}

