using System;
using System.Collections.Generic;
using UnityEngine;

namespace PTween
{
    public interface IPTween 
    {
        void Update();

        void FullKill();
        bool IsTargetDestroyed();
        void KillOnComplete();
        void Pause();
        void Resume();

        object Target { get; }
        bool IsComplete { get; }
        bool WasKilled { get; }
        bool IsPaused { get; }
        bool IgnoreTimeScale { get; }

        string Identifier { get; }
        float Delay { get; }
        Action onComplete { get; set; }
    }
}