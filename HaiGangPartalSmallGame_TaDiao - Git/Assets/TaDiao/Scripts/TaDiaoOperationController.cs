using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TaDiaoOperationController : MonoBehaviour {

    #region 塔吊身上物体定义
    public Transform f_rotateTrans; //塔吊的旋转部分
    public Transform f_fangxiangpanTrans; //塔吊操控室内的方向盘
    public Transform f_diaobiTrans; //塔吊的吊臂
    public Transform f_diaoshengTrans; //塔吊中吊臂的吊绳
    public Transform f_diaogouTrans; //塔吊的吊绳上的吊钩

    public Transform f_caozongganDiaobi; //操纵杆-吊臂的控制杆
    public Transform f_caozongganDiaosheng; //操纵杆-吊绳的控制杆
    public Transform f_caozongganGrab; //操纵杆-抓取的控制杆
    public Transform f_caozongganRelease; //操纵杆-释放的控制杆
    #endregion

    #region 塔吊旋转部分参数
    public float f_rotateBodyRotateSpeed = 5.0f; //塔吊旋转部分转速
    public float f_fangxiangpanRotateSpeed = 25.0f; //方向盘旋转部分转速
    public float f_rotateBodyRotationMax = 270.0f; //旋转物体的旋转上限
    public float f_rotateBodyRotationMin = -270.0f; //旋转物体的旋转下限
    private float f_rotateBodyCurrentRotation = 0; //当前已经旋转的角度
    private bool f_isRotateBodyCanRotate = true; //旋转物体是否可以旋转
    #endregion

    #region 吊臂移动参数
    public float f_diaobiMoveUp = -33.0f; //吊臂移动的起始位置角度（x轴）
    public float f_diaobiMoveDown = -55.0f; //吊臂移动的终止位置角度（x轴）
    public float f_diaobiMoveTime = 2.0f; //吊臂移动完成用时
    private bool f_isDiaobiMoving = false; //吊臂是否正在移动
    private bool f_isDiaobiDown = true; //吊臂是否要下移
    #endregion

    #region 吊绳的移动参数
    public float f_diaoshengOriginScaleY = 1; //吊绳的原始localScale.Y
    public float f_diaoshengFinalScaleY = 2; //吊绳的最终localScale.Y
    public float f_diaoshengMoveTime = 2.0f; //吊绳向下移动的用时
    private bool f_isDiaoshengMoving = false; //吊绳是否正在移动
    private bool f_isDiaoshengDown = true; //吊绳是否要下移
    #endregion

    #region 吊钩的移动参数
    public Transform f_diaogouPointTrans; //吊钩在吊绳上的位置的点
    public Transform f_touchTarget; //吊钩最先碰撞到的对象
    public Transform f_grabTarget; //吊钩的抓取目标
    private bool f_isGrabOn = false; //吊钩是否已经按下抓取
    private bool f_isGrabing = false; //是否正在抓取中
    #endregion



    // Use this for initialization
    void Start() {
        #region 获取物体空判断处理
        if (!f_rotateTrans) { //旋转部分为空
            f_rotateTrans = GameObject.Find( "CraneB_chaifen" ).transform;
        }
        if (!f_fangxiangpanTrans) { //方向盘获为空
            f_fangxiangpanTrans = transform.Find( "fangxiangpan" );
        }
        if (!f_diaobiTrans) {
            f_diaobiTrans = transform.Find( "diaobi" );
        }
        if (!f_diaoshengTrans) {
            f_diaoshengTrans = f_diaobiTrans.Find( "diaosheng" );
        }
        if (!f_diaogouTrans) {
            f_diaogouTrans = transform.Find( "diaogou" );
        }
        if (!f_caozongganDiaobi) {
            f_caozongganDiaobi = transform.Find( "caozonggan01" );
        }
        if (!f_caozongganDiaosheng) {
            f_caozongganDiaosheng = transform.Find( "caozonggan02" );
        }
        if (!f_caozongganGrab) {
            f_caozongganGrab = transform.Find( "caozonggan03" );
        }
        if (!f_caozongganRelease) {
            f_caozongganRelease = transform.Find( "caozonggan04" );
        }
        #endregion

        //原始的ScaleY和初始的localScale.Y相同
        f_diaoshengTrans.localScale = new Vector3( 1, f_diaoshengOriginScaleY, 1 );

    }

    // Update is called once per frame
    void Update() {
        #region 获取物体空判断处理
        if (!f_rotateTrans) { //旋转部分为空
            Debug.LogError( "旋转部分为空" );
            return;
        }
        if (!f_fangxiangpanTrans) { //方向盘获为空
            Debug.LogError( "方向盘为空" );
            return;
        }
        if (!f_diaobiTrans) { //吊臂获取为空
            Debug.LogError( "吊臂为空" );
            return;
        }
        if (!f_diaoshengTrans) {
            Debug.LogError( "吊绳为空" );
            return;
        }
        if (!f_diaogouTrans) {
            Debug.LogError( "吊钩为空" );
            return;
        }
        if (!f_caozongganDiaobi) {
            Debug.LogError( "吊臂操纵杆为空" );
            return;
        }
        if (!f_caozongganDiaosheng) {
            Debug.LogError( "吊绳操纵杆为空" );
            return;
        }
        if (!f_caozongganGrab) {
            Debug.LogError( "抓取操纵杆为空" );
            return;
        }
        if (!f_caozongganRelease) {
            Debug.LogError( "释放操纵杆为空" );
            return;
        }
        #endregion

        RotateBodyControll(); //检测旋转部分的旋转
        RotateDiaobiControll(); //检测吊臂的旋转
        MoveDiaoshengControl(); //检测吊绳的移动
        MoveDiaogouControl(); //检测吊钩的移动
        GrabTargetControl(); //抓取箱子的控制

    }

    /// <summary>
    /// 塔吊的旋转部分的旋转控制
    /// </summary>
    private void RotateBodyControll() {

        //获取水平移动量
        float horizontalMove = Input.GetAxis( "Horizontal" ) * Time.deltaTime;

        //赋值已经旋转的角度
        f_rotateBodyCurrentRotation += horizontalMove * f_rotateBodyRotateSpeed;
        //给是否能够旋转赋值，判断是否到达边界
        f_isRotateBodyCanRotate = f_rotateBodyCurrentRotation < f_rotateBodyRotationMax && f_rotateBodyCurrentRotation > f_rotateBodyRotationMin;
        //满足条件就旋转
        if (f_isRotateBodyCanRotate) {
            RotateBodyAndFang( horizontalMove ); //旋转塔吊和方向盘
        }

    }
    /// <summary>
    /// 旋转部分的旋转以及方向盘的旋转
    /// </summary>
    private void RotateBodyAndFang(float _rotation) {

        //将旋转量赋值给 旋转体 和 方向盘的旋转
        f_rotateTrans.Rotate( f_rotateTrans.up, _rotation * f_rotateBodyRotateSpeed, Space.Self );
        f_fangxiangpanTrans.Rotate( f_fangxiangpanTrans.up, _rotation * f_fangxiangpanRotateSpeed, Space.World ); //?

    }


    /// <summary>
    /// 吊臂的旋转控制
    /// </summary>
    private void RotateDiaobiControll() {

        if (Input.GetKeyDown( KeyCode.J ) && !f_isDiaobiMoving) {
            RotateDiaobiRotation( f_isDiaobiDown ); //吊臂旋转
            f_isDiaobiDown = !f_isDiaobiDown; //每次点击取反
            OpenCaozonggan( f_caozongganDiaobi, f_isDiaobiDown ); //调用操纵杆动画
        }

    }
    /// <summary>
    /// 吊臂的旋转
    /// </summary>
    /// <param name="_isDown">是否是向下旋转</param>
    private void RotateDiaobiRotation(bool _isDown) {

        f_isDiaobiMoving = true; //设置正在吊臂正在移动
        //根据传输进来的参数获取到旋转的最终点
        Vector3 endValue = new Vector3( _isDown ? f_diaobiMoveDown : f_diaobiMoveUp, 0, 0 );
        //使用DoTween旋转，在结束的时候，设置吊臂移动为false
        f_diaobiTrans.DOLocalRotate( endValue, f_diaobiMoveTime ).OnComplete( delegate { f_isDiaobiMoving = false; } );

    }


    /// <summary>
    /// 吊绳的移动控制
    /// </summary>
    private void MoveDiaoshengControl() {

        //控制吊绳的垂直方向
        f_diaoshengTrans.rotation = Quaternion.identity;
        if (Input.GetKeyDown( KeyCode.K ) && !f_isDiaoshengMoving) {
            MoveDiaosheng( f_isDiaoshengDown ); //绳子移动
            f_isDiaoshengDown = !f_isDiaoshengDown; //每次点击取反
            OpenCaozonggan( f_caozongganDiaosheng, f_isDiaoshengDown ); //调用操纵杆动画
        }

    }
    /// <summary>
    /// 移动吊绳，通过控制吊绳scale模拟
    /// </summary>
    private void MoveDiaosheng(bool _isDown) {

        f_isDiaoshengMoving = true; //设置吊绳正在移动
        //根据传输的参数确定最终scaleY
        float endScaleY = _isDown ? f_diaoshengFinalScaleY : f_diaoshengOriginScaleY;
        //使用DoTween拉伸，在结束的时候设置吊绳移动为false
        f_diaoshengTrans.DOScaleY( endScaleY, f_diaoshengMoveTime ).OnComplete( delegate { f_isDiaoshengMoving = false; } );

    }


    /// <summary>
    /// 吊钩的移动控制
    /// </summary>
    private void MoveDiaogouControl() {
        if (!f_diaogouPointTrans) {
            f_diaogouPointTrans = f_diaoshengTrans.Find( "diaogouPoint" );
        }
        if (!f_diaogouPointTrans) {
            Debug.LogError( "吊钩点为空" );
            return;
        }
        f_diaogouTrans.position = f_diaogouPointTrans.position;
    }


    /// <summary>
    /// 抓取集装箱控制
    /// </summary>
    private void GrabTargetControl() {

        if (Input.GetKeyDown( KeyCode.U ) && !f_isGrabOn) { //如果按下抓取键且没有抓取对象也没有打开开关
            if (f_touchTarget) { //如果有触碰的物体就调用抓取方法
                GrabPropContainer(); //抓取集装箱
            }

            f_isGrabOn = true; //抓去开关设置为已经抓取
            OpenCaozonggan( f_caozongganGrab, !f_isGrabOn, 0.02f );
            OpenCaozonggan( f_caozongganRelease, f_isGrabOn, 0.02f );
        }
        if (Input.GetKeyDown( KeyCode.I ) && f_isGrabOn) { //如果按下I就释放且有抓取对象，打开了开关，需要关上
            ReleasePropContainer(); //松开集装箱
            f_isGrabOn = false; //设置为未抓取
            OpenCaozonggan( f_caozongganGrab, !f_isGrabOn, 0.02f );
            OpenCaozonggan( f_caozongganRelease, f_isGrabOn, 0.02f );
        }

    }
    /// <summary>
    /// 抓取集装箱
    /// </summary>
    private void GrabPropContainer() {

        f_grabTarget = f_touchTarget;// 有触碰的箱子，就抓取该箱子
        f_grabTarget.SetParent( f_diaogouTrans, false );//设置父物体对象
        f_grabTarget.localPosition = Vector3.down * 2.0f;//位置调整
        //f_grabTarget.rotation = f_diaogouTrans.rotation;//集装箱方向调整
        f_grabTarget.GetComponent<PropContainerAI>().f_status = E_PropContainerStatus.InTransit;//设置集装箱的运输状态
        f_grabTarget.GetComponent<Rigidbody>().useGravity = false;//集装箱使用重力
        f_grabTarget.GetComponent<Rigidbody>().isKinematic = false; //取消运动学

    }
    /// <summary>
    /// 松开集装箱
    /// </summary>
    private void ReleasePropContainer() {

        if (f_grabTarget) { //如果抓取到了物体
            f_diaogouTrans.DetachChildren(); //脱离子物体
            f_grabTarget.GetComponent<Rigidbody>().useGravity = true;//集装箱使用重力
            f_grabTarget.GetComponent<PropContainerAI>().f_status = E_PropContainerStatus.Dropping;
            f_grabTarget.GetComponent<PropContainerAI>().BeDropped(); //被扔下
            f_grabTarget = null; //抓取设置为null
        }

    }


    /// <summary>
    /// 控制操控干的时候的移动旋转
    /// </summary>
    /// <param name="_caozonggan"></param>
    private void OpenCaozonggan(Transform _caozonggan, bool _isOpen) {
        OpenCaozonggan( _caozonggan, _isOpen, 1 ); //默认时间为1s
    }
    private void OpenCaozonggan(Transform _caozonggan, bool _isOpen, float _duration) {
        //x轴旋转的角度
        Vector3 addRotation = new Vector3( _isOpen ? 10 : -10, 0, 0 );
        //z轴移动的大小
        float addZ = _isOpen ? 0.2f : -0.2f;
        //旋转移动
        RotateAndMoveCaozonggan( _caozonggan, addRotation, addZ, _duration );
    }
    /// <summary>
    /// 旋转并移动操纵杆的方法
    /// </summary>
    /// <param name="_caozonggan"></param>
    /// <param name="_addRotation"></param>
    /// <param name="_addZ"></param>
    /// <param name="_duration"></param>
    private void RotateAndMoveCaozonggan(Transform _caozonggan, Vector3 _addRotation, float _addZ, float _duration) {
        _caozonggan.DOLocalRotate( _addRotation, _duration ).SetRelative( true );
        _caozonggan.DOLocalMoveZ( _addZ, _duration ).SetRelative( true );
    }

}
