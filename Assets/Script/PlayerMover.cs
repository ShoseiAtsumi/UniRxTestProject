using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Player.Contoroller;

namespace Player.Mover
{
    public class PlayerMover : MonoBehaviour
    {
        #region serializePrivate
        [SerializeField]
        private PlayerController playerController;

        [SerializeField]
        private Rigidbody2D rb;

        // 横軸入力
        [SerializeField]
        private float forceX;

        // 移動スピード
        [SerializeField]
        private float speed;

        // ジャンプ時に与える力
        [SerializeField]
        private float jumpPower;

        // ジャンプ中さらに高く飛べるリミット
        [SerializeField]
        private float jumpLimit;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // 横入力 プレイヤーが生きていれば入力をとる
            this.UpdateAsObservable()
                .Where(_ => playerController.playerAlive.Value == true)
                .Select(x => Input.GetAxisRaw("Horizontal"))
                .Subscribe(x => forceX = x);

            // 移動
            this.FixedUpdateAsObservable()
                .Where(_ => playerController.playerAlive.Value == true)
                .Subscribe(_ => Move(forceX));

            //ジャンプ プレイヤーが生きている && 地面にいる && スペースキーが押されたとき
            this.UpdateAsObservable()
                .Where(_ => playerController.playerAlive.Value == true && playerController.isGround == true && Input.GetKeyDown(KeyCode.Space))
                .Subscribe(_ => Jump());

            // ジャンプの高さの調整 プレイヤーがジャンプ中 && スペースが押されている間、JumpLimitを上げすぎると降りてこなくなるので重力が減算するyより高くはしない
            this.FixedUpdateAsObservable()
               .Where(_ => rb.velocity.y >= 2.5 && Input.GetKey(KeyCode.Space))
               .Subscribe(_ => rb.velocity += new Vector2(0, jumpLimit));
        }

        //　移動
        private void Move(float x)
        {
            // xが入力されていて横の移動速度がリミット以下の時力を加える
            if (Mathf.Abs(x) > 0.1 && Mathf.Abs(rb.velocity.x) <= 4.5)
            {
                rb.AddForce(new Vector2(x, 0) * speed);
            }
            // 地面にいない時といる時で減速するスピードを変更
            else if (!playerController.isGround)
            {
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime * 1), rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime * 10), rb.velocity.y);
            }
        }

        // ジャンプ
        private void Jump()
        {
            playerController.isGround = false;
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }
    }
}