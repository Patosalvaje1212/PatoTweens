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
                
                current.Value.Update();

                if(current.Value.WasKilled)
                {
                    _toErease.Add(current.Key);
                }
                if(current.Value.IsComplete)
                {
                    current.Value.onComplete?.Invoke();

                    _toErease.Add(current.Key);
                }
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
                throw new OverflowException($"A tween with id {tween.Identifier} already exist. Do not tween the same value at the same time");                
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

        // Summary:
        // Waits a determined number of seconds
        // Parameters:
        // time: 
        // Amount of seconds
        public static Tween WaitTime(float time)
        {
            Func<object> target = () => {
                return true;
            };

            string newId = $"{target.Target.GetHashCode()}_WaitingTime - {TwCount}";

            Tween myT = new(target, newId, true, true, time, value => {});

            return myT;
        }


        // Summary:
        // Tweens and outputs a to a value on a determined time
        // Parameters:
        // target: 
        // The target of the tween (input variable)
        // result: 
        // Output of the tween (output)
        // endV: 
        // End value of the tween
        // time: 
        // Amount of seconds
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
        

        // Summary:
        // Tweens and outputs a to a Vector3 on a determined time
        // Parameters:
        // target: 
        // The target of the tween (input variable)
        // result: 
        // Output of the tween (output)
        // endV: 
        // End value of the tween 
        // time: 
        // Amount of seconds 
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



        // Summary:
        // Tweens and outputs a to a Color on a determined time
        // Parameters:
        // target: 
        // The target of the tween (input variable)
        // result: 
        // Output of the tween (output)
        // endV: 
        // End value of the tween 
        // time: 
        // Amount of seconds
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

        // Summary:
        // Tweens and sets the alpha of a SpriteRenderer on a determined time
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial value of the tween
        // endV: 
        // End value of the tween
        // time: 
        // Amount of seconds
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


        // Summary:
        // Tweens and sets the alpha of a GameObject on a determined time
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial value of the tween
        // endV: 
        // End value of the tween
        // time: 
        // Amount of seconds
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

        // Summary:
        // Tweens and sets the color of a SpriteRenderer on a determined time
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial value of the tween
        // endV: 
        // End value of the tween
        // time: 
        // Amount of seconds
        public static Tween TweenSpriteColor(SpriteRenderer target, Color startV, Color endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Alpha_{TwCount}";
            Tween myT = new(target.gameObject.name, newId, startV, endV, time, value=> {
                target.color = (Color)value;
            });

            return myT;
        }


        // Summary:
        // Tweens and sets the color of a GameObject on a determined time
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial value of the tween
        // endV: 
        // End value of the tween
        // time: 
        // Amount of seconds
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


        // Summary:
        // Waits a number of seconds and then switches the sprite of a SpriteRenderer
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial sprite of the tween
        // endV: 
        // Sprite to switch to
        // time: 
        // Amount of seconds
        public static Tween TweenSprite(SpriteRenderer target, Sprite startV, Sprite endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_SpriteChange_{TwCount}";

            Tween myT = new(target, newId, startV, endV, time, value =>{
                target.sprite = (Sprite)value;
            });
            return myT;
        }




        // Summary:
        // Waits a number of seconds and then switches the sprite of a GameObject
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial sprite of the tween
        // endV: 
        // Sprite to switch to
        // time: 
        // Amount of seconds
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

        // Summary:
        // Tweens and sets the position of a given Transform
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial position
        // endV: 
        // End position
        // time: 
        // Amount of seconds
        public static Tween TweenPosition(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Position_{TwCount}";
            Tween myT = new(target, newId, startV, endV, time, value=> {
                target.position = (Vector3)value;
            });

            return myT;
        }

        // Summary:
        // Tweens and sets the position of a given Transform localy
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial position
        // endV: 
        // End position
        // time: 
        // Amount of seconds
        public static Tween TweenLocalPosition(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Position_{TwCount}";
            Tween myT = new(target, newId, startV, endV, time, value=> {
                target.localPosition = (Vector3)value;
            });

            return myT;
        }

        // Summary:
        // Tweens and sets the scale of a given Transform localy
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial scale
        // endV: 
        // End scale
        // time: 
        // Amount of seconds
        public static Tween TweenScale(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Scale_{TwCount}";
            Tween myT = new(target, newId, startV, endV, time, value=> {
                target.localScale = (Vector3)value;
            });

            return myT;
        }

        // Summary:
        // Tweens and sets the rotation of a given Transform
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial rotation
        // endV: 
        // End rotation
        // time: 
        // Amount of seconds
        public static Tween TweenRotation(Transform target, Vector3 startV, Vector3 endV, float time)
        {
            string newId = $"{target.GetInstanceID()}_Rotation_{TwCount}";
            Tween myT = new(target, newId, startV, endV, time, value=> {
                target.eulerAngles = (Vector3)value;
            });

            return myT;
        }

        // Summary:
        // Tweens and sets the rotation of a given Transform localy
        // Parameters:
        // target: 
        // The target of the tween (input)
        // startV: 
        // Initial rotation
        // endV: 
        // End rotation
        // time: 
        // Amount of seconds
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
