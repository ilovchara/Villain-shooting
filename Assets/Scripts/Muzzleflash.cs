using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour
{
    public ParticleSystem muzzleFlash;  // 🔥 枪口火焰粒子



    public void Activate()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmitting); // 只停止发射，不清除已有粒子
            muzzleFlash.Play();  // 重新播放
        }

    }

}
