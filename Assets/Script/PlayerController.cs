using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;

namespace Player.Contoroller
{
    public class PlayerController : MonoBehaviour
    {

        #region serializePrivate
        [SerializeField]
        private GameController gameController;

        [SerializeField]
        private Rigidbody2D rb;

        [SerializeField]
        private CircleCollider2D collider;
        #endregion

        #region public
        // 接地判定
        public bool isGround;
        // プレイヤーの生死判定 ゲームコントローラースクリプトなどから見れるようにpublic
        public ReactiveProperty<bool> playerAlive = new ReactiveProperty<bool>(true);
        #endregion

        #region private
        private bool pause;
        #endregion

        void Start()
        {
            gameController._Pause
               .Subscribe(b => pause = b);

            // 地面に当たった時 isGround = true
            this.OnCollisionEnter2DAsObservable()
                .Where(t => t.collider.tag == "Ground")
                .Subscribe(_ => isGround = true);

            // プレイヤーが一定以上下に落ちた場合 playerAlive.Value = false
            this.UpdateAsObservable()
                .Where(_ => transform.position.y < -5f)
                .Subscribe(_ => playerAlive.Value = false);

            // playerAliveが更新されたとき 更新した値がfalseであれば StartCoroutine("Dead"))
            playerAlive
                .Where(b => b == false)
                .Subscribe(_ => StartCoroutine("Dead"));
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

