using System.Collections.Generic;
using UnityEngine;
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
        private List<string> _toErease = new();

        private void Update()
        {
            foreach(var current in _activeTweens)
            {
                if(current.Value.WasKilled)
                {
                    _toErease.Add(current.Key);
                }
                else
                if(current.Value.IsComplete)
                {
                    current.Value?.onComplete?.Invoke();

                    _toErease.Add(current.Key);
                }
                else
                current.Value.Update();
                
            }

            for (int i = _toErease.Count - 1; i >= 0; i--)
            {
                RemoveTween(_toErease[i]);
            }
            _toErease.Clear();
        }

        public void AddTween(IPTween tween)
        {
            
            if(_activeTweens.ContainsKey(tween.Identifier))
            {
                Debug.LogWarning($"A tween with id {tween.Identifier} already exist. Do not tween the same value at the same time");                
            }

            _activeTweens.Add(tween.Identifier, tween);

            if(LogLevel > 0)Debug.Log($"Created tween: {tween.Identifier}");
            
        }

        public void RemoveTween(string id)
        {
            _activeTweens.Remove(id);

            if(LogLevel > 0)Debug.Log($"Removed tween: {id}");
        }


        #region helper methods

        /// <summary>
        /// Waits a determined number of seconds
        /// </summary>
        /// <param name="time">Amount of seconds</param>
        /// <returns></returns>
        public static Tween WaitTime(float time)
        {
            int target = new System.Random.Next();

            string newId = $"{target}_{TwCount}_WaitingTime";

            Tween myT = new(target, newId, true, true, time, value => {});

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
        public static Tween TweenFloat(Func<float> target, Action<float> result, float endV, float time)
        {
            string newId = $"{target.Target.GetHashCode()}_{TwCount}_Float";
            
            object targ = target.Target;

            float startVal = target();

            Tween myT = new(target, newId, startVal, endV, time, value=> {
                result((float)value);
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
        public static Tween TweenVector3(Func<Vector3> target, Action<Vector3> result, Vector3 endV, float time)
        {
            string newId = $"{target.Target.GetHashCode()}_{TwCount}_Vector3";
            
            object targ = target.Target;

            Vector3 startVal = target();

            Tween myT = new(target, newId, startVal, endV, time, value=> {
                result((Vector3)value);
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
        public static Tween TweenColor(Func<Color> target, Action<Color> result, Color endV, float time)
        {
            string newId = $"{target.Target.GetHashCode()}_{TwCount}_Color";
            
            object targ = target.Target;

            Color startVal = target();

            Tween myT = new(target, newId, startVal, endV, time, value=> {
                result((Color)value);
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
        public static Tween TweenSpriteAlpha(SpriteRenderer target, float startV, float endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";
            Tween myT = new(target, newId, startV, endV, time, value=> {
                Color newCol = target.color;
                newCol.a = (float)value;

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
        public static Tween TweenSpriteAlpha(GameObject gameobj, float startV, float endV, float time)
        {

            SpriteRenderer target = gameobj.GetComponent<SpriteRenderer>();
            if(target == null) throw new NotSupportedException($"Attempted to tween a GameObject that does not contaon a {(target.GetType())}");
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";


            Tween myT = new(gameobj, newId, startV, endV, time, value=> {
                Color newCol = target.color;
                newCol.a = (float)value;

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
        public static Tween TweenSpriteColor(SpriteRenderer target, Color startV, Color endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";
            Tween myT = new(target.gameObject.name, newId, startV, endV, time, value=> {
                target.color = (Color)value;
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
        public static Tween TweenSpriteColor(GameObject gameobj, Color startV, Color endV, float time)
        {

            SpriteRenderer target = gameobj.GetComponent<SpriteRenderer>();
            if(target == null) throw new NotSupportedException($"Attempted to tween a GameObject that does not contaon a {(target.GetType())}");
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";


            Tween myT = new(gameobj.name, newId, startV, endV, time, value=> {
                target.color = (Color)value;
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
        public static Tween TweenSprite(SpriteRenderer target, Sprite startV, Sprite endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_SpriteChange_{TwCount}";

            Tween myT = new(target, newId, startV, endV, time, value =>{
                target.sprite = (Sprite)value;
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
        public static Tween TweenSprite(GameObject gameobj, Sprite startV, Sprite endV, float time)
        {

            SpriteRenderer target = gameobj.GetComponent<SpriteRenderer>();
            if(target == null) throw new NotSupportedException($"Attempted to tween a GameObject that does not contaon a {(target.GetType())}");
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";


            Tween myT = new(gameobj.name, newId, startV, endV, time, value=> {
                target.sprite = (Sprite)value;
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
        public static Tween TweenPosition(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Position_{TwCount}";
            Tween myT = new(target, newId, startV, endV, time, value=> {
                target.position = (Vector3)value;
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
        public static Tween TweenLocalPosition(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Position_{TwCount}";
            Tween myT = new(target, newId, startV, endV, time, value=> {
                target.localPosition = (Vector3)value;
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
        public static Tween TweenScale(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Scale_{TwCount}";
            Tween myT = new(target, newId, startV, endV, time, value=> {
                target.localScale = (Vector3)value;
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
        public static Tween TweenRotation(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Rotation_{TwCount}";
            Tween myT = new(target, newId, startV, endV, time, value=> {
                target.eulerAngles = (Vector3)value;
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
        public static Tween TweenLocalRotation(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Rotation_{TwCount}";
            Tween myT = new(target, newId, startV, endV, time, value=> {
                target.localEulerAngles = (Vector3)value;
            });

            return myT;
        }

        #endregion
        
        #endregion
    }

}
