using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;

namespace Player.Main
{
    public class PlayerController : MonoBehaviour
    {

        #region serializePrivate
        [SerializeField]
        private GameController gameController;

        [SerializeField]
        private Rigidbody2D rb;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private CircleCollider2D collider;

        //// 横軸入力
        //[SerializeField]
        //private float forceX;

        //// 移動スピード
        //[SerializeField]
        //private float speed;

        //// ジャンプ時に与える力
        //[SerializeField]
        //private float jumpPower;

        //// ジャンプ中さらに高く飛べるリミット
        //[SerializeField]
        //private float jumpLimit;

        #endregion

        #region public
        // プレイヤーの生死判定 ゲームコントローラースクリプトなどから見れるようにpublic
        public ReactiveProperty<bool> playerAlive = new ReactiveProperty<bool>(true);
        #endregion

        #region private
       

        // アニメーションステート
        private ReactiveProperty<int> animatorState = new ReactiveProperty<int>(0);

        private bool pause;
        #endregion

        #region protected
        // 接地判定
        public bool isGround;
        #endregion

        void Start()
        {
            gameController._Pause
               .Subscribe(b => pause = b);

            //// 横入力 プレイヤーが生きていれば入力をとる
            //this.UpdateAsObservable()
            //    .Where(_ => playerAlive.Value == true)
            //    .Select(x => Input.GetAxisRaw("Horizontal"))
            //    .Subscribe(x => forceX = x);

            ////ジャンプ プレイヤーが生きている && 地面にいる && スペースキーが押されたとき
            //this.UpdateAsObservable()
            //    .Where(_ => playerAlive.Value == true && isGround == true && Input.GetKeyDown(KeyCode.Space))
            //    .Subscribe(_ => Jump());

            //// ジャンプの高さの調整 プレイヤーがジャンプ中 && スペースが押されている間、JumpLimitを上げすぎると降りてこなくなるので重力が減算するyより高くはしない
            //this.FixedUpdateAsObservable()
            //   .Where(_ => rb.velocity.y >= 2.5 && Input.GetKey(KeyCode.Space))
            //   .Subscribe(_ => rb.velocity += new Vector2(0, jumpLimit));

            // プレイヤーの向きに応じてスプライトを反転
            this.UpdateAsObservable()
                .Subscribe(_ => Rotate());

            //// 移動
            //this.FixedUpdateAsObservable()
            //    .Where(_ => playerAlive.Value == true)
            //    .Subscribe(_ => Move(forceX));

            // 地面に当たった時 isGround = true
            this.OnCollisionEnter2DAsObservable()
                .Where(t => t.collider.tag == "Ground")
                .Subscribe(_ => isGround = true);

            // プレイヤーが一定以上下に落ちた場合 playerAlive.Value = false
            this.UpdateAsObservable()
                .Where(_ => transform.position.y < -5f)
                .Subscribe(_ => playerAlive.Value = false);

            // animatorStateの値が変化したとき アニメーターに変化した値を投げる
            animatorState
                .Subscribe(i => animator.SetInteger("State", i));

            // ステート更新
            this.UpdateAsObservable()
                .Subscribe(_ => State());

            // playerAliveが更新されたとき 更新した値がfalseであれば StartCoroutine("Dead"))
            playerAlive
                .Where(b => b == false)
                .Subscribe(_ => StartCoroutine("Dead"));
        }

        ////　移動
        //private void Move(float x)
        //{
        //    // xが入力されていて横の移動速度がリミット以下の時力を加える
        //    if (Mathf.Abs(x) > 0.1 && Mathf.Abs(rb.velocity.x) <= 4.5)
        //    {
        //        rb.AddForce(new Vector2(x, 0) * speed);
        //    }
        //    // 地面にいない時といる時で減速するスピードを変更
        //    else if (!isGround)
        //    {
        //        rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime * 1), rb.velocity.y);
        //    }
        //    else
        //    {
        //        rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime * 10), rb.velocity.y);
        //    }
        //}

        //// ジャンプ
        //private void Jump()
        //{
        //    isGround = false;
        //    rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        //}

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
            if (!isGround)
            {
                animatorState.Value = 2;
            }
            // プレイヤーが死んでいたら 4 (Dead)
            if (!playerAlive.Value)
            {
                animatorState.Value = 4;
            }
        }

        // 死亡時
        private IEnumerator Dead()
        {
            // プレイヤーの当たり判定をすり抜けるように
            collider.isTrigger = true;
            // 上へ飛ばす
            rb.velocity = new Vector2(0, 6);

            // 1.5秒待って
            yield return new WaitForSeconds(1.5f);

            // シーンのリロード 
            // 現在のシーン名を取得する
            Scene loadScene = SceneManager.GetActiveScene();
            // シーンのの読み直し
            SceneManager.LoadScene(loadScene.name);
        }
    }
}
