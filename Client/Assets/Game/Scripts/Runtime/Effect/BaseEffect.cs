// using System;
// using UnityEngine;
//
// namespace Gameplay
// {
//     public class BaseEffect : PoolMonoObject
//     {
//         public string EffectName;
//         
//         // 存活时间
//         protected float _SurvivalTime = -1;
//         
//         private bool _IsActive = false;
//         private bool _IsDestroyed = false;
//         private float _CurrentTime = 0;
//         
//         public override void Init(params object[] parameters)
//         {
//             Debug.Log("Init: " + EffectName);
//             _IsActive = true;
//             _CurrentTime = 0;
//             // _SurvivalTime = (parameters != null && parameters[0] != null) ? (float)parameters[0] : -1;
//             _SurvivalTime = 20;
//             _IsDestroyed = (parameters != null && parameters[1] != null) && (bool)parameters[1];
//             gameObject.SetActive(true);
//         }
//
//         public override void Recycle()
//         {
//             Debug.Log("Recycle: " + EffectName);
//             _IsActive = false;
//             _CurrentTime = 0;
//             EffectManager.Instance.RecycleEffect(this);
//         }
//
//         public override void Destroy()
//         {
//             Debug.Log("Destroy: " + EffectName);
//             _IsActive = false;
//             _CurrentTime = 0;
//             GameObject.Destroy(gameObject);
//         }
//
//         private void Update()
//         {
//             if (_SurvivalTime <= 0 || !_IsActive) return;
//             _CurrentTime += Time.deltaTime;
//             if (_CurrentTime >= _SurvivalTime)
//             {
//                 if (_IsDestroyed)
//                 {
//                     Destroy();   
//                 }
//                 else
//                 {
//                     Recycle();
//                 }
//             }
//         }
//
//     }
// }