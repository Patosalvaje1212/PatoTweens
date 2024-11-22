using System;
using UnityEngine;


namespace PTween
{

    public class Tween : IPTween
    {

        private object _startVal;
        private object _endVal;
        private float _time;
        private Action<object> _onTweenUpdate;
        private float _elapsedTime = 0f;



        private EaseType _currentEase = EaseType.Linear;
        private float _delayElapsedTime = 0f;

        private bool _reverse;

        private bool _pingPong;
        private int _loops = 0;
        private int _loopsDone = 0;

        private float _percentThreshold, _originPercentThreshold;

        private Action _onThreshold;
        private Action _onUpdate;
        
        private Tween _appendedTween;


        private object currentVal;
        private float t = 0, easedT = 0;

        public Tween(object target, string id, object startV, object endV, float time, Action<object> tweenUpdate)
        {
            if(time <= 0f)
            {
                throw new IndexOutOfRangeException($"Cannot Tween a value out of bounds ({time} is less or equal to 0)");
            }

            Target = target;
            Identifier = id;

            _startVal = startV;
            _endVal = endV;
            _time = time;
            _onTweenUpdate = tweenUpdate;

            PatoTween.I.AddTween(this);
        }

        /// <summary> Returns the Target of this tween. </summary>
        /// <remarks> Cant change its value after creating the tween. </remarks> 
        public object Target { get; private set; }
        
        /// <summary> Returns true if the tween has finished executing. </summary>
        public bool IsComplete { get; private set; }
        
        /// <summary> Returns true if the Target of this tween was destroyed while it was executing. </summary>
        /// <remarks> You can access the Target via the Target variable. </remarks> 
        public bool WasKilled { get; private set; }
        
        /// <summary> Returns true if the Tween is paused. </summary>
        /// <remarks> You can pause/resume a tween with Pause()/Resume() methods respectively. </remarks> 
        public bool IsPaused { get; private set; }
        
        /// <summary> Returns true if this tween ignores the time scale. </summary>
        /// <remarks> You can change this value via the SetIgnoreTimeScale() method. </remarks> 
        public bool IgnoreTimeScale { get; private set; }

        /// <summary> Returns the identifier of this tween. </summary>
        /// <remarks> Cant change its value after creating the tween. </remarks> 
        public string Identifier { get; private set; }
        /// <summary> Returns the Delay of this tween. </summary>
        /// <remarks> You can set this value with the SetStartDelay() method. </remarks> 
        public float Delay { get; private set; }

        /// <summary> Returns the methods to execute when the tween completes. </summary>
        /// <remarks> You can set this value with the OnComplete() method. </remarks> 
        public Action onComplete { get; set; }


        
        public bool IsTargetDestroyed()
        {
            
            if(Target is MonoBehaviour mono && mono == null)
            {
                return true;
            }

            if(Target is GameObject go && go == null)
            {
                return true;
            }

            if(Target is Delegate del && del == null)
            {
                return true;
            }


            return false;
        }


        public void FullKill()
        {
            WasKilled = true;
            onComplete = null;

            KillOnComplete();    
        }


        public void KillOnComplete()
        {
            if(PatoTween.I.LogLevel > 2)  Debug.Log($"Killed tween: {Identifier}");
            
            IsComplete = true;

            _onUpdate = null;
            _onThreshold = null;
            _onTweenUpdate = null;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            if(!IsPaused) Debug.LogWarning("A playing Tween has been Resumed, this is redundant and may impact performance");
            IsPaused = false;
        }

        public void Update()
        {
            
            if(IsTargetDestroyed())
            {
                FullKill();
                return;
            }


            if(Delay > 0 && Delay >= _delayElapsedTime)
            {
                if(IgnoreTimeScale)
                    _delayElapsedTime += Time.unscaledDeltaTime;
                else
                    _delayElapsedTime += Time.deltaTime;
            
                return;
            }

            if(IsComplete || IsPaused) return;



            if(IgnoreTimeScale)
                _elapsedTime += Time.unscaledDeltaTime;
            else
                _elapsedTime += Time.deltaTime;


            if(_elapsedTime >= _time)
            {

                _elapsedTime = _time;
                
                if(_pingPong) _reverse = !_reverse;

                _percentThreshold = _originPercentThreshold;

                if(_loops == 0)
                {
                    _loopsDone ++;
                    _elapsedTime = 0f;
                    
                    if(_loopsDone > _loops)
                    {
                        if(_appendedTween == null)KillOnComplete();
                        IsComplete = true;
                    }
                }
            } else
            {
                t = _elapsedTime / _time;
                easedT = EaseMult(_currentEase, t);

                if(_reverse)
                {
                    currentVal = Interpolate(_endVal, _startVal, easedT);

                } else
                {
                    currentVal = Interpolate(_startVal, _endVal, easedT);
                    
                }
                
                _onUpdate?.Invoke();
                _onTweenUpdate?.Invoke(currentVal);

                if(_percentThreshold >= 0 && t >= _percentThreshold)
                {
                    _onThreshold?.Invoke();
                    _percentThreshold = -1;
                }
            }

            
        }

        private static float startFloat = 0;

        private object Interpolate(object start, object end, float Tm)
        {
            if(start is float && end is float) 
                return Mathf.LerpUnclamped((float)start, (float)end, Tm);
            else
            if(start is Vector3 && end is Vector3) 
                return Vector3.LerpUnclamped((Vector3)start, (Vector3)end, Tm);

            else
            if(start is Color && end is Color) 
                return Color.Lerp((Color)start, (Color)end, Tm);

            else
            if(start is bool && end is bool) 
                return IsComplete ? !(bool)start : (bool)end ;
            else
            throw new NotImplementedException($"Interpolation for given type is not defined. --");
        }

        #region  Property methods

        /// <summary>
        /// Sets the Ease type of the tween to a given EaseType
        /// </summary>
        /// <returns></returns>
        public Tween SetEase(EaseType easeSet)
        {
            _currentEase = easeSet;
            return this;
        }

        /// <summary>
        /// Manage the loop mode and number
        /// </summary>
        /// <param name="pingPong">
        /// Mode of the Pingpong
        /// <br></br>true: After the tween ends, it restarts in the opposite direction
        /// <br></br>false: After the tween ends, it restarts the given number of times
        ///</param>
        /// <returns></returns>
        public Tween SetLoops(bool pingPong = false, int loops = 2)
        {
            if(loops < 2)
            {
                throw new IndexOutOfRangeException($"Loop number ({loops}) is less than the minimun (2) for it to have effect.");
            }
            _loops = loops;
            _pingPong = pingPong;

            return this;
        }
        
        /// <summary>
        /// Execute the given function when the tween finishes
        /// </summary>
        /// <param name="onCompletion">
        /// Event (function) to execute
        ///</param>
        /// <returns></returns>
        public Tween OnComplete(Action onCompletion)
        {
            this.onComplete += onCompletion;
            return this;
        }

        /// <summary>
        /// Set the Ignore time scale value
        /// </summary>
        /// <param name="setTo">
        /// Mode to set
        /// <br></br>true: DOES NOT take into account the time scale
        /// <br></br>false: DOES take into account the time scale 
        ///</param>
        /// <returns></returns>
        public Tween SetIgnoreTimeScale(bool setTo = true)
        {
            IgnoreTimeScale = setTo;
            return this;
        }


        /// <summary>
        /// Execute the given function each tick the tween is active
        /// </summary>
        /// <param name="onUpdate">
        /// Event (function) to execute
        ///</param>
        /// <returns></returns>
        public Tween OnUpdate(Action onUpdate)
        {
            _onUpdate = onUpdate;
            return this;
        }

        /// <summary>
        /// Execute the given function when the tween reaches a certain percentage.
        /// </summary>
        /// <remarks> 
        /// NOTE: This variable will be set to the last value give. Multiple calls of this function will override previous values.
        /// <br></br> You can create mirror tweens (WaitTime()) to check for more percentages.
        /// </remarks> 
        /// <param name="percentage">
        /// Percentage at wich the even will be executed (0, 1)
        /// <br></br> 
        ///</param>
        /// <param name="onThreshold">
        /// <remarks> 
        /// NOTE: This variable will be set to the last value give. Multiple calls of this function will override previous values.
        /// You can create mirror tweens (WaitTime()) to check for more percentages.
        /// </remarks> 
        /// Event (function) to execute
        ///</param>
        /// <returns></returns>
        public Tween OnReachPrecentage(float percentage, Action onThreshold)
        {
            _originPercentThreshold = Mathf.Clamp01(percentage);
            _percentThreshold = _originPercentThreshold;
            _onThreshold = onThreshold;
            return this;
        }

        /// <summary>
        /// Make the tween wait a time in seconds BEFORE executing
        /// </summary>
        /// <param name="delay">
        /// Time to wait
        ///</param>
        /// <returns></returns>
        public Tween SetStartDelay(float delay)
        {
            Delay = delay;
            return this;
        }

        /// <summary>
        /// Execute the given Tween after the current one has ended.
        /// </summary>
        /// <remarks> 
        /// NOTE: Multiple calls of this function, will result on a chain. If you want multiple functions to execute, use OnComplete()
        /// </remarks> 
        /// <param name="onUpdate">
        /// Event (function) to execute
        ///</param>
        /// <returns></returns>
        public Tween AppendTween(Tween tweenT)
        {
            tweenT.Pause();

            Tween _tweenCheck;

            if(_appendedTween == null)
            {
                _appendedTween = tweenT;
                _tweenCheck = this;
                
            }
            else
            {
                _tweenCheck = _appendedTween;

                while(_tweenCheck._appendedTween != null)
                {
                    _tweenCheck = _tweenCheck._appendedTween;
                }
            
                if(PatoTween.I.LogLevel > 2) Debug.Log($"Created subtween for {Identifier} --- Id: {tweenT.Identifier}");

            }

            _tweenCheck._appendedTween = tweenT;
            _tweenCheck.OnComplete(tweenT.Resume);


            tweenT.OnComplete(() =>{
                _tweenCheck._appendedTween = null;
            });        

            return this;
        }
        #endregion

        #region Ease logic
        
        
        public static float Linear(float t) => t;

        public static float EaseInSine(float t) => 1 - Mathf.Cos(t * Mathf.PI * .5f);

        public static float EaseOutSine(float t) => Mathf.Sin(t * Mathf.PI * .5f);

        public static float EaseInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1) * .5f;

        public static float EaseInQuad(float t) => t * t;

        public static float EaseOutQuad(float t) => t * (2 - t);

        public static float EaseInOutQuad(float t) => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? EaseInQuad(t * 2f)  * .5f
            : .5f + EaseOutQuad((t -.5f) * 2f)  * .5f ;

        public static float EaseInCubic(float t) => t * t * t;

        public static float EaseOutCubic(float t) => 1 - (1 - t) * (1 - t) * (1 - t);

        public static float EaseInOutCubic(float t) => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? EaseInCubic(t * 2f)  * .5f
            : .5f + EaseOutCubic((t -.5f) * 2f)  * .5f ;

        public static float EaseInQuart(float t) => t * t * t * t;

        public static float EaseOutQuart(float t) => 1 - (1 - t) * (1 - t) * (1 - t) * (1 - t);

        public static float EaseInOutQuart(float t)  => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? EaseInQuart(t * 2f)  * .5f
            : .5f + EaseOutQuart((t -.5f) * 2f)  * .5f ;

        public static float EaseInQuint(float t) => t * t * t * t * t;

        public static float EaseOutQuint(float t) => 1 - (1 - t) * (1 - t) * (1 - t) * (1 - t) * (1 - t);

        public static float EaseInOutQuint(float t) => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? EaseInQuint(t * 2f)  * .5f
            : .5f + EaseOutQuint((t -.5f) * 2f)  * .5f ;

        public static float EaseInExpo(float t) => t == 0 ? 0 : Mathf.Pow(2, 10 * (t - 1));

        public static float EaseOutExpo(float t) => t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);

        public static float EaseInOutExpo(float t) => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? Mathf.Pow(2, (20 * t) - 10) / 2
            : (2 - Mathf.Pow(2, (-20 * t) + 10)) * .5f;

        public static float EaseInCirc(float t) => 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));

        public static float EaseOutCirc(float t) => Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));

        public static float EaseInOutCirc(float t) => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? EaseInCirc(t * 2f)  * .5f
            : .5f + EaseOutCirc((t -.5f) * 2f)  * .5f ;

        public static float EaseInElastic(float t) => t == 0 ? 0 : t == 1 ? 1 : Mathf.Pow(2, 10 * (t - 1)) * Mathf.Sin((t - 1.1f) * -5 * Mathf.PI);

        public static float EaseOutElastic(float t) => t == 0 ? 0 : t == 1 ? 1 : Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.1f) * 5 * Mathf.PI) + 1;

        public static float EaseInOutElastic(float t) => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? EaseInElastic(t * 2f)  * .5f
            : .5f + EaseOutElastic((t -.5f) * 2f)  * .5f ;

        public static float EaseInBack(float t) => t * t * t - t * Mathf.Sin(t * Mathf.PI);

        public static float EaseOutBack(float t) => 1 - (1 - t) * (1 - t) * (1 - t) - Mathf.Sin(t * Mathf.PI);

        public static float EaseInOutBack(float t) => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? EaseInBack(t * 2f)  * .5f
            : .5f + EaseOutBack((t -.5f) * 2f)  * .5f ;

        public static float EaseInBounce(float t) => 1 - EaseOutBounce(1 - t);

        public static float EaseOutBounce(float t) => t < 1 / 2.75f
            ? 7.5625f * t * t
            : t < 2 / 2.75f
            ? 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f
            : t < 2.5 / 2.75f
            ? 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f
            : 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;

        public static float EaseInOutBounce(float t) => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? EaseInBounce(t * 2f)  * .5f
            : .5f + EaseOutBounce((t -.5f) * 2f)  * .5f ;

        public static float EaseInElasticOvershoot(float t) => t == 0 ? 0 : t == 1 ? 1 : Mathf.Pow(2, 10 * (t - 1)) * Mathf.Sin((t - 1.1f) * -5 * Mathf.PI) * 1.5f;

        public static float EaseOutElasticOvershoot(float t) => t == 0 ? 0 : t == 1 ? 1 : Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.1f) * 5 * Mathf.PI) * 1.5f + 1;

        public static float EaseInOutElasticOvershoot(float t) => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? EaseInElasticOvershoot(t * 2f)  * .5f
            : .5f + EaseOutElasticOvershoot((t -.5f) * 2f)  * .5f ;

        public static float EaseInSinusoidal(float t) => 1 - Mathf.Cos(t * Mathf.PI * .5f);

        public static float EaseOutSinusoidal(float t) => Mathf.Sin(t * Mathf.PI * .5f);

        public static float EaseInOutSinusoidal(float t)  => t == 0
            ? 0
            : t == 1
            ? 1
            : t < 0.5f
            ? EaseInSinusoidal(t * 2f)  * .5f
            : .5f + EaseOutSinusoidal((t -.5f) * 2f)  * .5f ;


        public static float EaseMult(EaseType easingType, float t)
        {
            return easingType switch
            {
                EaseType.Linear => Linear(t),
                EaseType.InSine => EaseInSine(t),
                EaseType.OutSine => EaseOutSine(t),
                EaseType.InOutSine => EaseInOutSine(t),
                EaseType.InQuad => EaseInQuad(t),
                EaseType.OutQuad => EaseOutQuad(t),
                EaseType.InOutQuad => EaseInOutQuad(t),
                EaseType.InCubic => EaseInCubic(t),
                EaseType.OutCubic => EaseOutCubic(t),
                EaseType.InOutCubic => EaseInOutCubic(t),
                EaseType.InQuart => EaseInQuart(t),
                EaseType.OutQuart => EaseOutQuart(t),
                EaseType.InOutQuart => EaseInOutQuart(t),
                EaseType.InQuint => EaseInQuint(t),
                EaseType.OutQuint => EaseOutQuint(t),
                EaseType.InOutQuint => EaseInOutQuint(t),
                EaseType.InExpo => EaseInExpo(t),
                EaseType.OutExpo => EaseOutExpo(t),
                EaseType.InOutExpo => EaseInOutExpo(t),
                EaseType.InCirc => EaseInCirc(t),
                EaseType.OutCirc => EaseOutCirc(t),
                EaseType.InOutCirc => EaseInOutCirc(t),
                EaseType.InElastic => EaseInElastic(t),
                EaseType.OutElastic => EaseOutElastic(t),
                EaseType.InOutElastic => EaseInOutElastic(t),
                EaseType.InBack => EaseInBack(t),
                EaseType.OutBack => EaseOutBack(t),
                EaseType.InOutBack => EaseInOutBack(t),
                EaseType.InBounce => EaseInBounce(t),
                EaseType.OutBounce => EaseOutBounce(t),
                EaseType.InOutBounce => EaseInOutBounce(t),
                /*EaseType.InElasticOvershoot => EaseInElasticOvershoot(t),
                EaseType.OutElasticOvershoot => EaseOutElasticOvershoot(t),
                EaseType.InOutElasticOvershoot => EaseInOutElasticOvershoot(t),*/
                EaseType.InSinusoidal => EaseInSinusoidal(t),
                EaseType.OutSinusoidal => EaseOutSinusoidal(t),
                EaseType.InOutSinusoidal => EaseInOutSinusoidal(t),
                _ => throw new NotImplementedException($"Ease type {easingType} is not implemented"),
            };
        }

        #endregion

    }


    #region Ease sets

    public enum EaseType
    {
        Linear,
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        InElastic,
        OutElastic,
        InOutElastic,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce,
        InElasticOvershoot,
        OutElasticOvershoot,
        InOutElasticOvershoot,
        InSinusoidal,
        OutSinusoidal,
        InOutSinusoidal
    }

    #endregion


}