using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 集装箱状态
/// </summary>
public enum E_PropContainerStatus {
    Static, //静止状态
    InTransit, //运输状态
    Dropping, //下落状态
    Finished, //完成状态
}

/// <summary>
/// 集装箱AI控制
/// </summary>
public class PropContainerAI : MonoBehaviour {

    public E_PropContainerStatus f_status = E_PropContainerStatus.Static; //集装箱状态，默认静止状态

    private Transform f_diaogouTrans; //吊钩对象
    private Vector3 f_originPosition; //集装箱的原始位置
    private Quaternion f_originRotation; //集装箱的原始角度
    private Transform f_originParent; //集装箱的原始父物体

    private bool f_isPropContainerFinish = false;

    // Use this for initialization
    void Start() {
        f_originPosition = transform.position; //确定初始的位置
        f_originParent = transform.parent; //确定原始的父物体
        f_originRotation = transform.rotation; //确定原始的角度
    }

    // Update is called once per frame
    void Update() {
        if (f_status == E_PropContainerStatus.InTransit) { //在运输中的方向固定
            //固定集装箱的朝向和位置
            transform.eulerAngles = Vector3.forward;
            transform.localPosition = Vector3.down * 2;
        }
    }

    /// <summary>
    /// 初始化集装箱状态信息等
    /// </summary>
    private void InitPropContainer() {

        transform.parent = f_originParent; //改变父物体
        transform.position = f_originPosition; //位置初始化
        transform.rotation = f_originRotation; //角度初始化
        //状态初始化
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
        f_status = E_PropContainerStatus.Static;

    }

    #region Trigger
    private void OnTriggerEnter(Collider _collider) {
        if (f_status == E_PropContainerStatus.Finished) { //如果已经运输完成，不允许再次托运
            return;
        }
        if (f_status == E_PropContainerStatus.Static) { //碰撞的时候如果是静止状态的就设置接触对象
            SetTouchObjet( _collider );
        }
    }

    private void OnTriggerStay(Collider _collider) {
        if (f_status == E_PropContainerStatus.Finished) { //如果已经运输完成，不允许再次托运
            return;
        }
        //防止掉下后，一直处于stay状态，没法检测
        if (!f_diaogouTrans && f_status == E_PropContainerStatus.Static) {
            SetTouchObjet( _collider );
        }
    }

    private void OnTriggerExit(Collider _collider) {
        if (f_status == E_PropContainerStatus.Finished) { //如果已经运输完成，不允许再次托运
            return;
        }
        if (_collider.tag == TagManager.DiaoGou) { //如果是吊钩碰撞
            Transform tadiao = _collider.transform.parent; //获取到塔吊对象
            f_diaogouTrans = null; //吊钩对象清空
            if (tadiao) {
                tadiao.GetComponent<TaDiaoOperationController>().f_touchTarget = null; //赋值碰撞对象
            }
        }
    }

    /// <summary>
    /// 设置碰撞的物体对象
    /// </summary>
    private void SetTouchObjet(Collider _collider) {
        if (f_status == E_PropContainerStatus.Finished) { //如果已经运输完成，不允许再次托运
            return;
        }
        if (_collider.tag == TagManager.DiaoGou) { //如果是吊钩碰撞
            Transform tadiao = _collider.transform.parent; //获取到塔吊对象
            f_diaogouTrans = _collider.transform; //获取吊钩对象
            if (tadiao) {
                tadiao.GetComponent<TaDiaoOperationController>().f_touchTarget = transform; //赋值碰撞对象
            }
        }
    }
    #endregion

    #region Collision
    private void OnCollisionEnter(Collision _collision) {
        //如果到检测时间后，箱子碰撞的是船，重新归位
        if (f_status == E_PropContainerStatus.Finished && _collision.collider.tag == TagManager.Ship) {
            InitPropContainer();
        }
        if (f_status == E_PropContainerStatus.Finished) { //如果已经运输完成，不允许再次托运
            return;
        }
        if (_collision.collider.tag == TagManager.PropContainer) { //如果碰撞到的是集装箱

            //如果碰撞的集装箱不再船上，不会影响
            if (_collision.collider.GetComponent<PropContainerAI>().f_status != E_PropContainerStatus.Static) {
                return;
            }
            if (f_status == E_PropContainerStatus.InTransit) { //如果正在运输中，直接默认为失败
                Transform tadiao = null;
                if (f_diaogouTrans) {
                    tadiao = f_diaogouTrans.parent; //获取到塔吊对象
                    f_diaogouTrans = null; //吊钩对象清空
                }
                if (tadiao) {
                    tadiao.GetComponent<TaDiaoOperationController>().f_grabTarget = null; //赋值抓取对象
                }
                //吧集装箱赋值会原来的位置，恢复初始状态
                InitPropContainer();
            }
            if (f_status == E_PropContainerStatus.Dropping) { //如果正在下降，表示落在了箱子上，设置状态为静态
                //吧集装箱赋值会原来的位置，恢复初始状态
                InitPropContainer();
            }
        } //如果碰到了集装箱
        if (_collision.collider.tag == TagManager.Ship) { //如果碰到了船，直接归位
            InitPropContainer();
        }

    }

    #endregion

    /// <summary>
    /// 集装箱被扔下
    /// </summary>
    public void BeDropped() {
        BeDropped( 8 );
    }
    /// <summary>
    /// 延迟时间最小为8
    /// </summary>
    /// <param name="_delayTime"></param>
    public void BeDropped(float _delayTime) {
        _delayTime = _delayTime > 8 ? _delayTime : 8; //最小为8
        StartCoroutine( AfterDrop( _delayTime ) );
    }
    /// <summary>
    /// 控制集装箱被扔下之后的指定时间后的操作
    /// </summary>
    /// <returns></returns>
    private IEnumerator AfterDrop(float _delayTime) {
        yield return new WaitForSeconds( _delayTime );

        //如果规定时间后，集装箱还是下落状态，没有被重新归位，设置为运输完成状态
        if (f_status == E_PropContainerStatus.Dropping) {
            if (transform.position.y < 0) { //如果落下了地面，则销毁
                Destroy( gameObject );
            }

            //设置集装箱的状态为运输完成状态
            f_status = E_PropContainerStatus.Finished;
        }

        yield return null;

    }

}
