using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using usercmd;
using UI;

namespace Battle
{
    public class FootMan : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (action)
            {
                if (timer > ActionTime)
                {
                    action = false;
                    active = true;
                    StartMove();
                    timer = 0;
                }

                timer += Time.deltaTime;
            }

            if (move)
            {
                if (timer > moveDel)
                {
                    _ftRigid.velocity = moveDirection;
                    move = false;
                }
                timer += Time.deltaTime;
            }

            if (active)
            {
                if (Math.Abs(transform.position.x - centerPoint.x) < 5)
                {
                    PlayAttackAnim();
                    StartDestroy(deleteTime);
                    active = false;
                }
            }
        }

        //--------------------------------------------
        // 开关
        public bool action;
        private bool active;
        private bool move;

        // 调试属性
        private Vector3 moveDirection;
        public Vector3 centerPoint;
        public float deleteTime;
        public float ActionTime;
        private float timer;
        private float moveDel;

        // 组件
        private Transform _ftTrans;
        private Rigidbody _ftRigid;
        private Animator _ftAnim;


        public void Init()
        {
            //action = false;
            active = false;
            timer = 0;
            move = false;
            moveDel = 1;
            _ftTrans = GetComponent<Transform>();
            _ftRigid = GetComponent<Rigidbody>();
            _ftAnim = GetComponent<Animator>();
        }

        public void InitPDR(Vector3 position, Vector3 direction, Vector3 rotation)
        {
            _ftTrans.position = position;
            moveDirection = direction;
            _ftTrans.Rotate(rotation);
        }

        public void StartAction()
        {
            action = true;
        }

        private void StartMove()
        {
            _ftAnim.SetTrigger("run");
            move = true;
        }

        private void PlayAttackAnim()
        {
            UIManager.audioManager.PlayAudioAttack();
            _ftRigid.velocity = new Vector3(0, 0, 0);
            _ftAnim.SetTrigger("atk");
        }

        private void StartDestroy(float time)
        {
            Destroy(gameObject, time);
        }

        public void DestroyIm()
        {
            UIManager.audioManager.StopAudioAttack();
            Destroy(gameObject);
        }
    }
}

