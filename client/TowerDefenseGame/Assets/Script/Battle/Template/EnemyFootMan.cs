using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Template
{
    public class EnemyFootMan : MonoBehaviour
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
        [SerializeField] private int recoverAtr;
        [SerializeField] private int moveSpeedAtr;
        [SerializeField] private int attackFrequecyAtr;
        public int Id { get; set; }

        //-------------------------------------------
        // 属性数据（计算时用，会变动）
        public int Defence { get; set; }
        public int Heath { get; set; }

        //交互
        public void TakeDamage() { }
    }

}
