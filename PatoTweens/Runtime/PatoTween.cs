using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

namespace PTween
{
    public class PatoTween : MonoBehaviour
    {
        private static PatoTween instance;

        private static int TwCount = 0;
        public static PatoTween  I
        {
            get
            {
                if(instance == null)
                {
                    GameObject manager = new("PatoTweenManager");
                    instance = manager.AddComponent<PatoTween>();
                }

                return instance;
            }
        }

        public int LogLevel = 0;

        private Dictionary<string, IPTween> _activeTweens = new();



        private void Update()
        {
            foreach(var tw in _activeTweens.ToList())
            {
                IPTween tween = tw.Value;

                tween.Update();


                if(tween.IsComplete && !tween.WasKilled)
                {
                    if(tween.onComplete != null)
                    {
                        tween.onComplete.Invoke();
                    }

                    RemoveTween(tw.Key);
                }
                
                if(tween.WasKilled)
                {
                    //RemoveTween(tw.Key);
                }
            }
        }

        public void AddTween<T>(IPTween tween)
        {
            TwCount ++;

            /*if(_activeTweens.ContainsKey(tween.Identifier))
            {
                Debug.LogWarning($"Tween with ID:{tween.Identifier} is already executing. Deleting old instance");
                _activeTweens[tween.Identifier].KillOnComplete();
            }*/

            if(LogLevel > 0)Debug.Log($"Created tween: {tween.Identifier}");

            _activeTweens[tween.Identifier] = tween;
        }

        public void RemoveTween(string id)
        {
            if(LogLevel > 0)Debug.Log($"Removed tween: {id}");

            _activeTweens.Remove(id);
        }


        #region helper methods

        /// <summary>
        /// Waits a determined number of seconds
        /// </summary>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<bool> WaitTime(float time)
        {
            Func<bool> target = () => {
                return true;
            };

            string newId = $"{target.Target.GetHashCode()}_WaitingTime - {TwCount}";

            Tween<bool> myT = new(target, newId, true, true, time, value => {});

            return myT;
        }


        /// <summary>
        /// Tweens and outputs a to a value on a determined time
        /// </summary>
        /// <param name="target">The target of the tween (input variable)</param>
        /// <param name="result">Output of the tween (output)</param>
        /// <param name="endV">End value of the tween</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<float> TweenFloat(Func<float> target, Action<float> result, float endV, float time)
        {
            string newId = $"{target.Target.GetHashCode()}_{TwCount}_Float";
            
            object targ = target.Target;

            float startVal = target();

            Tween<float> myT = new(target, newId, startVal, endV, time, value=> {
                result(value);
            });

            return myT;
        }
        

        /// <summary>
        /// Tweens and outputs a to a Vector3 on a determined time
        /// </summary>
        /// <param name="target">The target of the tween (input variable)</param>
        /// <param name="result">Output of the tween (output)</param>
        /// <param name="endV">End value of the tween </param>
        /// <param name="time">Amount of seconds </param>
        /// <returns></returns>
        public static Tween<Vector3> TweenVector3(Func<Vector3> target, Action<Vector3> result, Vector3 endV, float time)
        {
            string newId = $"{target.Target.GetHashCode()}_{TwCount}_Vector3";
            
            object targ = target.Target;

            Vector3 startVal = target();

            Tween<Vector3> myT = new(target, newId, startVal, endV, time, value=> {
                result(value);
            });

            return myT;
        }



        /// <summary>
        /// Tweens and outputs a to a Color on a determined time
        /// </summary>
        /// <param name="target">The target of the tween (input variable)</param>
        /// <param name="result">Output of the tween (output)</param>
        /// <param name="endV">End value of the tween </param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<Color> TweenColor(Func<Color> target, Action<Color> result, Color endV, float time)
        {
            string newId = $"{target.Target.GetHashCode()}_{TwCount}_Color";
            
            object targ = target.Target;

            Color startVal = target();

            Tween<Color> myT = new(target, newId, startVal, endV, time, value=> {
                result(value);
            });

            return myT;
        }



        #region SpriteRenderer

        /// <summary>
        /// Tweens and sets the alpha of a SpriteRenderer on a determined time
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial value of the tween</param>
        /// <param name="endV">End value of the tween</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<float> TweenSpriteAlpha(SpriteRenderer target, float startV, float endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";
            Tween<float> myT = new(target, newId, startV, endV, time, value=> {
                Color newCol = target.color;
                newCol.a = value;

                target.color = newCol;
            });

            return myT;
        }


        /// <summary>
        /// Tweens and sets the alpha of a GameObject on a determined time
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial value of the tween</param>
        /// <param name="endV">End value of the tween</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<float> TweenSpriteAlpha(GameObject gameobj, float startV, float endV, float time)
        {

            SpriteRenderer target = gameobj.GetComponent<SpriteRenderer>();
            if(target == null) throw new NotSupportedException($"Attempted to tween a GameObject that does not contaon a {(target.GetType())}");
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";


            Tween<float> myT = new(gameobj, newId, startV, endV, time, value=> {
                Color newCol = target.color;
                newCol.a = value;

                target.color = newCol;
            });

            return myT;
        }

        /// <summary>
        /// Tweens and sets the color of a SpriteRenderer on a determined time
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial value of the tween</param>
        /// <param name="endV">End value of the tween</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<Color> TweenSpriteColor(SpriteRenderer target, Color startV, Color endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";
            Tween<Color> myT = new(target.gameObject.name, newId, startV, endV, time, value=> {
                target.color = value;
            });

            return myT;
        }


        /// <summary>
        /// Tweens and sets the color of a GameObject on a determined time
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial value of the tween</param>
        /// <param name="endV">End value of the tween</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<Color> TweenSpriteColor(GameObject gameobj, Color startV, Color endV, float time)
        {

            SpriteRenderer target = gameobj.GetComponent<SpriteRenderer>();
            if(target == null) throw new NotSupportedException($"Attempted to tween a GameObject that does not contaon a {(target.GetType())}");
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";


            Tween<Color> myT = new(gameobj.name, newId, startV, endV, time, value=> {
                target.color = value;
            });

            return myT;
        }


        /// <summary>
        /// Waits a number of seconds and then switches the sprite of a SpriteRenderer
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial sprite of the tween</param>
        /// <param name="endV">Sprite to switch to</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<Sprite> TweenSprite(SpriteRenderer target, Sprite startV, Sprite endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_SpriteChange_{TwCount}";

            Tween<Sprite> myT = new(target, newId, startV, endV, time, value =>{
                target.sprite = value;
            });
            return myT;
        }




        /// <summary>
        /// Waits a number of seconds and then switches the sprite of a GameObject
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial sprite of the tween</param>
        /// <param name="endV">Sprite to switch to</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<Sprite> TweenSprite(GameObject gameobj, Sprite startV, Sprite endV, float time)
        {

            SpriteRenderer target = gameobj.GetComponent<SpriteRenderer>();
            if(target == null) throw new NotSupportedException($"Attempted to tween a GameObject that does not contaon a {(target.GetType())}");
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";


            Tween<Sprite> myT = new(gameobj.name, newId, startV, endV, time, value=> {
                target.sprite = value;
            });

            return myT;
        }


        



        #endregion

        #region Transform

        /// <summary>
        /// Tweens and sets the position of a given Transform
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial position</param>
        /// <param name="endV">End position</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<Vector3> TweenPosition(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Position_{TwCount}";
            Tween<Vector3> myT = new(target, newId, startV, endV, time, value=> {
                target.position = value;
            });

            return myT;
        }

        /// <summary>
        /// Tweens and sets the position of a given Transform localy
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial position</param>
        /// <param name="endV">End position</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<Vector3> TweenLocalPosition(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Position_{TwCount}";
            Tween<Vector3> myT = new(target, newId, startV, endV, time, value=> {
                target.localPosition = value;
            });

            return myT;
        }

        /// <summary>
        /// Tweens and sets the scale of a given Transform localy
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial scale</param>
        /// <param name="endV">End scale</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<Vector3> TweenScale(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Scale_{TwCount}";
            Tween<Vector3> myT = new(target, newId, startV, endV, time, value=> {
                target.localScale = value;
            });

            return myT;
        }

        /// <summary>
        /// Tweens and sets the rotation of a given Transform
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial rotation</param>
        /// <param name="endV">End rotation</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<Vector3> TweenRotation(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Rotation_{TwCount}";
            Tween<Vector3> myT = new(target, newId, startV, endV, time, value=> {
                target.eulerAngles = value;
            });

            return myT;
        }

        /// <summary>
        /// Tweens and sets the rotation of a given Transform localy
        /// </summary>
        /// <param name="target">The target of the tween (input)</param>
        /// <param name="startV">Initial rotation</param>
        /// <param name="endV">End rotation</param>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween<Vector3> TweenLocalRotation(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Rotation_{TwCount}";
            Tween<Vector3> myT = new(target, newId, startV, endV, time, value=> {
                target.localEulerAngles = value;
            });

            return myT;
        }

        #endregion
        
        #endregion
    }

}
