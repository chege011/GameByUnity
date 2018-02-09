using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Battle.Template
{
    public class AlliesFootMan : MonoBehaviour
    {
        //----------Unity--------------------------
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {

        }

        private void OnTriggerExit(Collider other)
        {

        }

        //--------------------------------------------
        // 基础属性
        [SerializeField] private int attackDamageAtr;
        [SerializeField] private int defenceAtr;
        [SerializeField] private int heathAtr;
        [SerializeField] private int recoveryAtr;
        [SerializeField] private int moveSpeedAtr;
        [SerializeField] private int attackFrequecyAtr;
        public int Id { get; set; }

        public void Init(int id)
        {
            this.Id = id;
            InitMovement();
            InitAttributeData();
            InitInteractive();
            // 同步发包
        }

        //-------------------------------------------------
        // 移动和导航
        public Transform EnemyHome { get; private set; }
        public Transform Target { get; private set; }
        public NavMeshAgent Nav { get; private set; }

        private void InitMovement()
        {
            EnemyHome = GameObject.FindGameObjectWithTag("EnemyHome").transform;
            Target = EnemyHome;
            Nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }

        private void UpdateMovement()
        {
            Nav.SetDestination(Target.position);
        }

        //子寻路对象中的OnTrigerEnter 调用
        public void ChangeTarget(Transform enemy)
        {
            Target = enemy;
        }

        public void ResetTarget()
        {
            Target = EnemyHome;
        }

        //-------------------------------------------
        // 属性数据（计算时用，会变动）
        public int AttackDamage{ get; set; }
        public int Defence { get; set; }
        public int Heath { get; set; }
        public int Recovery { get; set; }
        public int MoveSpeed { get; set; }
        public int AttackFrequecy { get; set; }


        private void InitAttributeData()
        {
            AttackDamage = attackDamageAtr;
            Defence = defenceAtr;
            Heath = heathAtr;
            Recovery = recoveryAtr;
            MoveSpeed = moveSpeedAtr;
            AttackFrequecy = attackFrequecyAtr;
        }

        private void UpdateAttributeData()
        {

        }

        private void RecoverHealth()
        {
            if (Heath < heathAtr)
            {

            }
        }

        // TODO 填充每个Reset
        public void ResetAttackDamage()
        {
            AttackDamage = attackDamageAtr;
        }

        public void ResetDefence()
        {
            Defence = defenceAtr;
        }

        
        //-----------------------------------------
        // 交互
        private bool _targetInRange;
        private bool _isDead;
        private int _timer;

        // 需要用到的Gameobj
        private Rigidbody _rigidbody;
        private Animator _interactiveAm;

        // 音频

        private void InitInteractive()
        {
            _targetInRange = false;
            _isDead = false;
            _timer = 0;
            _rigidbody = GetComponent<Rigidbody>();
            _interactiveAm = GetComponent<Animator>();
        }

        private void UpdateInteractive()
        {
            if (_targetInRange && _timer >= AttackFrequecy)
            {
                Attack();
            }
        }

        private void Attack()
        {
            var enemy = Target.gameObject.GetComponent<EnemyFootMan>();
            int take = AttackDamage;
            // 使用MsgHandler传给远端 take 数据
        }

        // 当收到伤害时被Msg
        public void TakeDamage(int damage)
        {
            int take = damage * (1 - Defence / 100);
            Heath -= take;

            // 血量条

            // 播音频

            // 记录动画反馈

            if (Heath <= 0 && !_isDead)
            {
                Death();
            }
        }

        private void Death()
        {
            _isDead = true;

            // 固定行动
            Nav.enabled = false;
            GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
            GetComponent<Rigidbody>().isKinematic = true;
            // 设置动画
            // 播放音频

            // 在playerManager dictionary 把此东西移除
            Destroy(gameObject, 2f);
        }

        //同步---------------------------------------

    }

}
