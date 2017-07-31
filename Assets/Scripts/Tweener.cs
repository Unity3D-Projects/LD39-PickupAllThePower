using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UniqueInstanceObject), typeof(GameLifetimeObject))]
public class Tweener : MonoBehaviour
{
    public delegate void TweenerTickAction<in TTarget>(TTarget target, float currentValue);

    public delegate void TwennerOnCompleted<in TCompletedParam>(TCompletedParam param);

    private readonly List<TweenAction> _actions = new List<TweenAction>();
    private readonly List<TweenAction> _completedActions = new List<TweenAction>();

    public float? EffectAmplitude = null;
    public float? EffectPeriod = null;
    public float? EffectOvershoot = null;

    void Update()
    {
        _completedActions.Clear();
        foreach (var tweenAction in _actions)
        {
            if (tweenAction.Mode == TweenTimeMode.Normal && tweenAction.StartTime + tweenAction.Duration < Time.time ||
                tweenAction.Mode == TweenTimeMode.Unscaled && tweenAction.StartTime + tweenAction.Duration < Time.unscaledTime)
            {
                _completedActions.Add(tweenAction);
            }
            else
            {
                tweenAction.OnTick(tweenAction.Target,
                    tweenAction.Effect.Execute(
                        (tweenAction.Mode == TweenTimeMode.Normal ? Time.time : Time.unscaledTime) - tweenAction.StartTime,
                        tweenAction.StartValue, tweenAction.ChangeValue, tweenAction.Duration));
            }
        }
        foreach (var completedAction in _completedActions)
        {
            completedAction.OnTick(completedAction.Target,completedAction.EndValue);
            StopTween(completedAction.Id, true);
        }
    }

    public Guid AddTween(
        object target, 
        float duration, 
        float startValue, 
        float endValue,
        TweenerTickAction<object> onTickAction, 
        ITweenEffect effect = null,
        TwennerOnCompleted<object> onCompleteAction = null,
        object onCompleteParam = null, 
        TweenTimeMode timeMode = TweenTimeMode.Normal)
    {
        var newAction = new TweenAction
        {
            Id = Guid.NewGuid(),
            Target = target,
            Duration = duration,
            StartValue = startValue,
            EndValue = endValue,
            ChangeValue = endValue - startValue,
            OnTick = onTickAction,
            Effect = effect ?? TweenEffectsFactory.LinearEffect,
            OnComplete = onCompleteAction,
            OnCompleteParam = onCompleteParam,
            Mode = timeMode,
            StartTime = (timeMode == TweenTimeMode.Normal) ? Time.time : Time.unscaledTime,
        };
        _actions.Add(newAction);
        return newAction.Id;
    }

    public bool StopTween(Guid id, bool executeOnComplete = false)
    {
        var tweenAction = _actions.FirstOrDefault(x => x.Id == id);
        if (tweenAction == null) 
            return false;

        if (executeOnComplete && tweenAction.OnComplete != null)
        {
            tweenAction.OnComplete(tweenAction.OnCompleteParam);
        }
        _actions.Remove(tweenAction);
        return true;
    }

    public void StopAll(bool executeOnComplete = false)
    {
        if (executeOnComplete)
        {
            foreach (var tweenAction in _actions)
            {
                if (tweenAction.OnComplete != null)
                {
                    tweenAction.OnComplete(tweenAction.OnCompleteParam);
                }
            }
        }
        _actions.Clear();
    }

    public static Tweener Instance
    {
        get { return FindObjectOfType<Tweener>(); }
    }
}

public class TweenAction
{
    public Guid Id;
    public object Target;
    public float Duration;
    public float StartTime;
    public float StartValue;
    public float EndValue;
    public float ChangeValue;
    public object OnCompleteParam;
    public Tweener.TwennerOnCompleted<object> OnComplete;
    public Tweener.TweenerTickAction<object> OnTick;
    public TweenTimeMode Mode;
    public ITweenEffect Effect;
}

public enum TweenTimeMode
{
    Normal,
    Unscaled
}

#region [ Effects ]

public interface ITweenEffect
{
    float Execute(float currentTime, float startValue, float changeNeeded, float duration);
}

public class TweenEffectsFactory
{
    #region [ Linear ]

    private static TweenLinearEffect _linearEffect;

    /// <summary>
    /// Easing equation function for a simple linear tweening, with no easing
    /// </summary>
    public static TweenLinearEffect LinearEffect
    {
        get { return _linearEffect ?? (_linearEffect = new TweenLinearEffect()); }
    }

    #endregion

    #region [ EaseInQuad ]

    private static TweenEaseInQuadEffect _easeInQuadEffect;

    /// <summary>
    /// Easing equation function for a quadratic (t^2) easing in: accelerating from zero velocity.
    /// </summary>
    public static TweenEaseInQuadEffect EaseInQuadEffect
    {
        get { return _easeInQuadEffect ?? (_easeInQuadEffect = new TweenEaseInQuadEffect()); }
    }

    #endregion

    #region [ EaseOutQuad ]

    private static TweenEaseOutQuadEffect _easeOutQuadEffect;

    /// <summary>
    /// Easing equation function for a quadratic (t^2) easing out: decelerating to zero velocity.
    /// </summary>
    public static TweenEaseOutQuadEffect EaseOutQuadEffect
    {
        get { return _easeOutQuadEffect ?? (_easeOutQuadEffect = new TweenEaseOutQuadEffect()); }
    }

    #endregion

    #region [ EaseInOutQuad ]

    private static TweenEaseInOutQuadEffect _easeInOutQuadEffect;

    /// <summary>
    /// Easing equation function for a quadratic (t^2) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    public static TweenEaseInOutQuadEffect EaseInOutQuadEffect
    {
        get { return _easeInOutQuadEffect ?? (_easeInOutQuadEffect = new TweenEaseInOutQuadEffect()); }
    }

    #endregion

    #region [ EaseOutInQuad ]

    private static TweenEaseOutInQuadEffect _easeOutInQuadEffect;

    /// <summary>
    /// Easing equation function for a quadratic (t^2) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    public static TweenEaseOutInQuadEffect EaseOutInQuadEffect
    {
        get { return _easeOutInQuadEffect ?? (_easeOutInQuadEffect = new TweenEaseOutInQuadEffect()); }
    }

    #endregion

    #region [ EaseInCubic ]

    private static TweenEaseInCubicEffect _easeInCubicEffect;

    /// <summary>
    /// Easing equation function for a cubic (t^3) easing in: accelerating from zero velocity.
    /// </summary>
    public static TweenEaseInCubicEffect EaseInCubicEffect
    {
        get { return _easeInCubicEffect ?? (_easeInCubicEffect = new TweenEaseInCubicEffect()); }
    }

    #endregion

    #region [ EaseOutCubic ]

    private static TweenEaseOutCubicEffect _easeOutCubicEffect;

    /// <summary>
    /// Easing equation function for a cubic (t^3) easing out: decelerating from zero velocity.
    /// </summary>
    public static TweenEaseOutCubicEffect EaseOutCubicEffect
    {
        get { return _easeOutCubicEffect ?? (_easeOutCubicEffect = new TweenEaseOutCubicEffect()); }
    }

    #endregion

    #region [ EaseInOutCubic ]

    private static TweenEaseInOutCubicEffect _easeInOutCubicEffect;

    /// <summary>
    /// Easing equation function for a cubic (t^3) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    public static TweenEaseInOutCubicEffect EaseInOutCubicEffect
    {
        get { return _easeInOutCubicEffect ?? (_easeInOutCubicEffect = new TweenEaseInOutCubicEffect()); }
    }

    #endregion

    #region [ EaseOutInCubic ]

    private static TweenEaseOutInCubicEffect _easeOutInCubicEffect;

    /// <summary>
    /// Easing equation function for a cubic (t^3) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    public static TweenEaseOutInCubicEffect EaseOutInCubicEffect
    {
        get { return _easeOutInCubicEffect ?? (_easeOutInCubicEffect = new TweenEaseOutInCubicEffect()); }
    }

    #endregion

    #region [ EaseInQuart ]

    private static TweenEaseInQuartEffect _easeInQuartEffect;

    /// <summary>
    /// Easing equation function for a quartic (t^4) easing in: accelerating from zero velocity.
    /// </summary>
    public static TweenEaseInQuartEffect EaseInQuartEffect
    {
        get { return _easeInQuartEffect ?? (_easeInQuartEffect = new TweenEaseInQuartEffect()); }
    }

    #endregion

    #region [ EaseOutQuart ]

    private static TweenEaseOutQuartEffect _easeOutQuartEffect;

    /// <summary>
    /// Easing equation function for a quartic (t^4) easing out: decelerating from zero velocity.
    /// </summary>
    public static TweenEaseOutQuartEffect EaseOutQuartEffect
    {
        get { return _easeOutQuartEffect ?? (_easeOutQuartEffect = new TweenEaseOutQuartEffect()); }
    }

    #endregion

    #region [ EaseInOutQuart ]

    private static TweenEaseInOutQuartEffect _easeInOutQuartEffect;

    /// <summary>
    /// Easing equation function for a quartic (t^4) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    public static TweenEaseInOutQuartEffect EaseInOutQuartEffect
    {
        get { return _easeInOutQuartEffect ?? (_easeInOutQuartEffect = new TweenEaseInOutQuartEffect()); }
    }

    #endregion

    #region [ EaseOutInQuart ]

    private static TweenEaseOutInQuartEffect _easeOutInQuartEffect;

    /// <summary>
    /// Easing equation function for a quartic (t^4) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    public static TweenEaseOutInQuartEffect EaseOutInQuartEffect
    {
        get { return _easeOutInQuartEffect ?? (_easeOutInQuartEffect = new TweenEaseOutInQuartEffect()); }
    }

    #endregion

    #region [ EaseInQuint ]

    private static TweenEaseInQuintEffect _easeInQuintEffect;

    /// <summary>
    /// Easing equation function for a quintic (t^5) easing in: accelerating from zero velocity.
    /// </summary>
    public static TweenEaseInQuintEffect EaseInQuintEffect
    {
        get { return _easeInQuintEffect ?? (_easeInQuintEffect = new TweenEaseInQuintEffect()); }
    }

    #endregion

    #region [ EaseOutQuint ]

    private static TweenEaseOutQuintEffect _easeOutQuintEffect;

    /// <summary>
    /// Easing equation function for a quintic (t^5) easing out: decelerating from zero velocity.
    /// </summary>
    public static TweenEaseOutQuintEffect EaseOutQuintEffect
    {
        get { return _easeOutQuintEffect ?? (_easeOutQuintEffect = new TweenEaseOutQuintEffect()); }
    }

    #endregion

    #region [ EaseInOutQuint ]

    private static TweenEaseInOutQuintEffect _easeInOutQuintEffect;

    /// <summary>
    /// Easing equation function for a quintic (t^5) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    public static TweenEaseInOutQuintEffect EaseInOutQuintEffect
    {
        get { return _easeInOutQuintEffect ?? (_easeInOutQuintEffect = new TweenEaseInOutQuintEffect()); }
    }

    #endregion

    #region [ EaseOutInQuint ]

    private static TweenEaseOutInQuintEffect _easeOutInQuintEffect;

    /// <summary>
    /// Easing equation function for a quintic (t^5) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    public static TweenEaseOutInQuintEffect EaseOutInQuintEffect
    {
        get { return _easeOutInQuintEffect ?? (_easeOutInQuintEffect = new TweenEaseOutInQuintEffect()); }
    }

    #endregion

    #region [ EaseInSine ]

    private static TweenEaseInSineEffect _easeInSineEffect;

    /// <summary>
    /// Easing equation function for a sinusoidal (sin(t)) easing in: accelerating from zero velocity.
    /// </summary>
    public static TweenEaseInSineEffect EaseInSineEffect
    {
        get { return _easeInSineEffect ?? (_easeInSineEffect = new TweenEaseInSineEffect()); }
    }

    #endregion

    #region [ EaseOutSine ]

    private static TweenEaseOutSineEffect _easeOutSineEffect;

    /// <summary>
    /// Easing equation function for a sinusoidal (sin(t)) easing out: decelerating from zero velocity.
    /// </summary>
    public static TweenEaseOutSineEffect EaseOutSineEffect
    {
        get { return _easeOutSineEffect ?? (_easeOutSineEffect = new TweenEaseOutSineEffect()); }
    }

    #endregion

    #region [ EaseInOutSine ]

    private static TweenEaseInOutSineEffect _easeInOutSineEffect;

    /// <summary>
    /// Easing equation function for a sinusoidal (sin(t)) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    public static TweenEaseInOutSineEffect EaseInOutSineEffect
    {
        get { return _easeInOutSineEffect ?? (_easeInOutSineEffect = new TweenEaseInOutSineEffect()); }
    }

    #endregion

    #region [ EaseOutInSine ]

    private static TweenEaseOutInSineEffect _easeOutInSineEffect;

    /// <summary>
    /// Easing equation function for a sinusoidal (sin(t)) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    public static TweenEaseOutInSineEffect EaseOutInSineEffect
    {
        get { return _easeOutInSineEffect ?? (_easeOutInSineEffect = new TweenEaseOutInSineEffect()); }
    }

    #endregion

    #region [ EaseInExpo ]

    private static TweenEaseInExpoEffect _easeInExpoEffect;

    /// <summary>
    /// Easing equation function for an exponential (2^t) easing in: accelerating from zero velocity.
    /// </summary>
    public static TweenEaseInExpoEffect EaseInExpoEffect
    {
        get { return _easeInExpoEffect ?? (_easeInExpoEffect = new TweenEaseInExpoEffect()); }
    }

    #endregion

    #region [ EaseOutExpo ]

    private static TweenEaseOutExpoEffect _easeOutExpoEffect;

    /// <summary>
    /// Easing equation function for an exponential (2^t) easing out: decelerating from zero velocity.
    /// </summary>
    public static TweenEaseOutExpoEffect EaseOutExpoEffect
    {
        get { return _easeOutExpoEffect ?? (_easeOutExpoEffect = new TweenEaseOutExpoEffect()); }
    }

    #endregion

    #region [ EaseInOutExpo ]

    private static TweenEaseInOutExpoEffect _easeInOutExpoEffect;

    /// <summary>
    /// Easing equation function for an exponential (2^t) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    public static TweenEaseInOutExpoEffect EaseInOutExpoEffect
    {
        get { return _easeInOutExpoEffect ?? (_easeInOutExpoEffect = new TweenEaseInOutExpoEffect()); }
    }

    #endregion

    #region [ EaseOutInExpo ]

    private static TweenEaseOutInExpoEffect _easeOutInExpoEffect;

    /// <summary>
    /// Easing equation function for an exponential (2^t) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    public static TweenEaseOutInExpoEffect EaseOutInExpoEffect
    {
        get { return _easeOutInExpoEffect ?? (_easeOutInExpoEffect = new TweenEaseOutInExpoEffect()); }
    }

    #endregion

    #region [ EaseInCirc ]

    private static TweenEaseInCircEffect _easeInCircEffect;

    /// <summary>
    /// Easing equation function for a circular (sqrt(1-t^2)) easing in: accelerating from zero velocity.
    /// </summary>
    public static TweenEaseInCircEffect EaseInCircEffect
    {
        get { return _easeInCircEffect ?? (_easeInCircEffect = new TweenEaseInCircEffect()); }
    }

    #endregion

    #region [ EaseOutCirc ]

    private static TweenEaseOutCircEffect _easeOutCircEffect;

    /// <summary>
    /// Easing equation function for a circular (sqrt(1-t^2)) easing out: decelerating from zero velocity.
    /// </summary>
    public static TweenEaseOutCircEffect EaseOutCircEffect
    {
        get { return _easeOutCircEffect ?? (_easeOutCircEffect = new TweenEaseOutCircEffect()); }
    }

    #endregion

    #region [ EaseInOutCirc ]

    private static TweenEaseInOutCircEffect _easeInOutCircEffect;

    /// <summary>
    /// Easing equation function for a circular (sqrt(1-t^2)) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    public static TweenEaseInOutCircEffect EaseInOutCircEffect
    {
        get { return _easeInOutCircEffect ?? (_easeInOutCircEffect = new TweenEaseInOutCircEffect()); }
    }

    #endregion

    #region [ EaseOutInCirc ]

    private static TweenEaseOutInCircEffect _easeOutInCircEffect;

    /// <summary>
    /// Easing equation function for a circular (sqrt(1-t^2)) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    public static TweenEaseOutInCircEffect EaseOutInCircEffect
    {
        get { return _easeOutInCircEffect ?? (_easeOutInCircEffect = new TweenEaseOutInCircEffect()); }
    }

    #endregion

    #region [ EaseInElastic ]

    private static TweenEaseInElasticEffect _easeInElasticEffect;

    /// <summary>
    /// Easing equation function for an elastic (exponentially decaying sine wave) easing in: accelerating from zero velocity.
    /// </summary>
    public static TweenEaseInElasticEffect EaseInElasticEffect
    {
        get { return _easeInElasticEffect ?? (_easeInElasticEffect = new TweenEaseInElasticEffect()); }
    }

    #endregion

    #region [ EaseOutElastic ]

    private static TweenEaseOutElasticEffect _easeOutElasticEffect;

    /// <summary>
    /// Easing equation function for an elastic (exponentially decaying sine wave) easing out: decelerating from zero velocity.
    /// </summary>
    public static TweenEaseOutElasticEffect EaseOutElasticEffect
    {
        get { return _easeOutElasticEffect ?? (_easeOutElasticEffect = new TweenEaseOutElasticEffect()); }
    }

    #endregion

    #region [ EaseInOutElastic ]

    private static TweenEaseInOutElasticEffect _easeInOutElasticEffect;

    /// <summary>
    /// Easing equation function for an elastic (exponentially decaying sine wave) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    public static TweenEaseInOutElasticEffect EaseInOutElasticEffect
    {
        get { return _easeInOutElasticEffect ?? (_easeInOutElasticEffect = new TweenEaseInOutElasticEffect()); }
    }

    #endregion

    #region [ EaseOutInElastic ]

    private static TweenEaseOutInElasticEffect _easeOutInElasticEffect;

    /// <summary>
    /// Easing equation function for an elastic (exponentially decaying sine wave) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    public static TweenEaseOutInElasticEffect EaseOutInElasticEffect
    {
        get { return _easeOutInElasticEffect ?? (_easeOutInElasticEffect = new TweenEaseOutInElasticEffect()); }
    }

    #endregion

    #region [ EaseInBack ]

    private static TweenEaseInBackEffect _easeInBackEffect;

    /// <summary>
    /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in: accelerating from zero velocity.
    /// </summary>
    public static TweenEaseInBackEffect EaseInBackEffect
    {
        get { return _easeInBackEffect ?? (_easeInBackEffect = new TweenEaseInBackEffect()); }
    }

    #endregion

    #region [ EaseOutBack ]

    private static TweenEaseOutBackEffect _easeOutBackEffect;

    /// <summary>
    /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out: decelerating from zero velocity.
    /// </summary>
    public static TweenEaseOutBackEffect EaseOutBackEffect
    {
        get { return _easeOutBackEffect ?? (_easeOutBackEffect = new TweenEaseOutBackEffect()); }
    }

    #endregion

    #region [ EaseInOutBack ]

    private static TweenEaseInOutBackEffect _easeInOutBackEffect;

    /// <summary>
    /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    public static TweenEaseInOutBackEffect EaseInOutBackEffect
    {
        get { return _easeInOutBackEffect ?? (_easeInOutBackEffect = new TweenEaseInOutBackEffect()); }
    }

    #endregion

    #region [ EaseOutInBack ]

    private static TweenEaseOutInBackEffect _easeOutInBackEffect;

    /// <summary>
    /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    public static TweenEaseOutInBackEffect EaseOutInBackEffect
    {
        get { return _easeOutInBackEffect ?? (_easeOutInBackEffect = new TweenEaseOutInBackEffect()); }
    }

    #endregion

    #region [ EaseInBounce ]

    private static TweenEaseInBounceEffect _easeInBounceEffect;

    /// <summary>
    /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in: accelerating from zero velocity.
    /// </summary>
    public static TweenEaseInBounceEffect EaseInBounceEffect
    {
        get { return _easeInBounceEffect ?? (_easeInBounceEffect = new TweenEaseInBounceEffect()); }
    }

    #endregion

    #region [ EaseOutBounce ]

    private static TweenEaseOutBounceEffect _easeOutBounceEffect;

    /// <summary>
    /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: decelerating from zero velocity.
    /// </summary>
    public static TweenEaseOutBounceEffect EaseOutBounceEffect
    {
        get { return _easeOutBounceEffect ?? (_easeOutBounceEffect = new TweenEaseOutBounceEffect()); }
    }

    #endregion

    #region [ EaseInOutBounce ]

    private static TweenEaseInOutBounceEffect _easeInOutBounceEffect;

    /// <summary>
    /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    public static TweenEaseInOutBounceEffect EaseInOutBounceEffect
    {
        get { return _easeInOutBounceEffect ?? (_easeInOutBounceEffect = new TweenEaseInOutBounceEffect()); }
    }

    #endregion

    #region [ EaseOutInBounce ]

    private static TweenEaseOutInBounceEffect _easeOutInBounceEffect;

    /// <summary>
    /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    public static TweenEaseOutInBounceEffect EaseOutInBounceEffect
    {
        get { return _easeOutInBounceEffect ?? (_easeOutInBounceEffect = new TweenEaseOutInBounceEffect()); }
    }

    #endregion
}

public class TweenLinearEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return changeNeeded * currentTime / duration + startValue;
    }


    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInQuadEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return changeNeeded * (currentTime /= duration) * currentTime + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutQuadEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return -changeNeeded * (currentTime /= duration) * (currentTime - 2) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInOutQuadEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if ((currentTime /= duration / 2) < 1) return changeNeeded / 2 * currentTime * currentTime + startValue;
        return -changeNeeded / 2 * ((--currentTime) * (currentTime - 2) - 1) + startValue;
    }
    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutInQuadEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseOutQuadEffect.Ease(currentTime * 2, startValue, changeNeeded / 2, duration);
        return TweenEaseInQuadEffect.Ease((currentTime * 2) - duration, startValue + changeNeeded / 2, changeNeeded / 2, duration);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInCubicEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return changeNeeded * (currentTime /= duration) * currentTime * currentTime + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutCubicEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return changeNeeded * ((currentTime = currentTime / duration - 1) * currentTime * currentTime + 1) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInOutCubicEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if ((currentTime /= duration / 2) < 1) return changeNeeded / 2 * currentTime * currentTime * currentTime + startValue;
        return changeNeeded / 2 * ((currentTime -= 2) * currentTime * currentTime + 2) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutInCubicEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseOutCubicEffect.Ease(currentTime * 2, startValue, changeNeeded / 2, duration);
        return TweenEaseInCubicEffect.Ease((currentTime * 2) - duration, startValue + changeNeeded / 2, changeNeeded / 2, duration);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInQuartEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return changeNeeded * (currentTime /= duration) * currentTime * currentTime * currentTime + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutQuartEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return -changeNeeded * ((currentTime = currentTime / duration - 1) * currentTime * currentTime * currentTime - 1) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInOutQuartEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if ((currentTime /= duration / 2) < 1) return changeNeeded / 2 * currentTime * currentTime * currentTime * currentTime + startValue;
        return -changeNeeded / 2 * ((currentTime -= 2) * currentTime * currentTime * currentTime - 2) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutInQuartEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseOutQuartEffect.Ease(currentTime * 2, startValue, changeNeeded / 2, duration);
        return TweenEaseInQuartEffect.Ease((currentTime * 2) - duration, startValue + changeNeeded / 2, changeNeeded / 2, duration);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInQuintEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return changeNeeded * (currentTime /= duration) * currentTime * currentTime * currentTime * currentTime + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutQuintEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return changeNeeded * ((currentTime = currentTime / duration - 1) * currentTime * currentTime * currentTime * currentTime + 1) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInOutQuintEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if ((currentTime /= duration / 2) < 1) return changeNeeded / 2 * currentTime * currentTime * currentTime * currentTime * currentTime + startValue;
        return changeNeeded / 2 * ((currentTime -= 2) * currentTime * currentTime * currentTime * currentTime + 2) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutInQuintEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseOutQuintEffect.Ease(currentTime * 2, startValue, changeNeeded / 2, duration);
        return TweenEaseInQuintEffect.Ease((currentTime * 2) - duration, startValue + changeNeeded / 2, changeNeeded / 2, duration);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInSineEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return -changeNeeded * Mathf.Cos(currentTime / duration * (Mathf.PI / 2)) + changeNeeded + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutSineEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return changeNeeded * Mathf.Sin(currentTime / duration * (Mathf.PI / 2)) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInOutSineEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return -changeNeeded / 2 * (Mathf.Cos(Mathf.PI * currentTime / duration) - 1) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutInSineEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseOutSineEffect.Ease(currentTime * 2, startValue, changeNeeded / 2, duration);
        return TweenEaseInSineEffect.Ease((currentTime * 2) - duration, startValue + changeNeeded / 2, changeNeeded / 2, duration);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInExpoEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return (currentTime == 0) ? startValue : changeNeeded * Mathf.Pow(2, 10 * (currentTime / duration - 1)) + startValue - changeNeeded * 0.001f;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutExpoEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return (currentTime == duration) ? startValue + changeNeeded : changeNeeded * 1.001f * (-Mathf.Pow(2, -10 * currentTime / duration) + 1) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInOutExpoEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime == 0) return startValue;
        if (currentTime == duration) return startValue + changeNeeded;
        if ((currentTime /= duration / 2) < 1) return changeNeeded / 2 * Mathf.Pow(2, 10 * (currentTime - 1)) + startValue - changeNeeded * 0.0005f;
        return changeNeeded / 2 * 1.0005f * (-Mathf.Pow(2, -10 * --currentTime) + 2) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutInExpoEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseOutExpoEffect.Ease(currentTime * 2, startValue, changeNeeded / 2, duration);
        return TweenEaseInExpoEffect.Ease((currentTime * 2) - duration, startValue + changeNeeded / 2, changeNeeded / 2, duration);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInCircEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return -changeNeeded * (Mathf.Sqrt(1 - (currentTime /= duration) * currentTime) - 1) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutCircEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return changeNeeded * Mathf.Sqrt(1 - (currentTime = currentTime / duration - 1) * currentTime) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInOutCircEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if ((currentTime /= duration / 2) < 1) return -changeNeeded / 2 * (Mathf.Sqrt(1 - currentTime * currentTime) - 1) + startValue;
        return changeNeeded / 2 * (Mathf.Sqrt(1 - (currentTime -= 2) * currentTime) + 1) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutInCircEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseOutCircEffect.Ease(currentTime * 2, startValue, changeNeeded / 2, duration);
        return TweenEaseInCircEffect.Ease((currentTime * 2) - duration, startValue + changeNeeded / 2, changeNeeded / 2, duration);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInElasticEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        var a = Tweener.Instance.EffectAmplitude;
        var p = Tweener.Instance.EffectPeriod;
        if (currentTime == 0) return startValue;
        if ((currentTime /= duration) == 1) return startValue + changeNeeded;
        if (p == null) p = duration * 0.3f;
        float s;
        if (a == null || a < Mathf.Abs(changeNeeded)) { a = changeNeeded; s = p.GetValueOrDefault() / 4f; }
        else s = p.GetValueOrDefault() / (2 * Mathf.PI) * Mathf.Asin(changeNeeded / a.GetValueOrDefault());
        return -(a.GetValueOrDefault() * Mathf.Pow(2, 10 * (currentTime -= 1)) * Mathf.Sin((currentTime * duration - s) * (2 * Mathf.PI) / p.GetValueOrDefault())) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutElasticEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        var a = Tweener.Instance.EffectAmplitude;
        var p = Tweener.Instance.EffectPeriod;
        if (currentTime == 0) return startValue;
        if ((currentTime /= duration) == 1) return startValue + changeNeeded;
        if (p == null) p = duration * 0.3f;
        float s;
        if (a == null || a < Mathf.Abs(changeNeeded)) { a = changeNeeded; s = p.GetValueOrDefault() / 4; }
        else s = p.GetValueOrDefault() / (2 * Mathf.PI) * Mathf.Asin(changeNeeded / a.GetValueOrDefault());
        return (a.GetValueOrDefault() * Mathf.Pow(2, -10 * currentTime) * Mathf.Sin((currentTime * duration - s) * (2 * Mathf.PI) / p.GetValueOrDefault()) + changeNeeded + startValue);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInOutElasticEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        var a = Tweener.Instance.EffectAmplitude;
        var p = Tweener.Instance.EffectPeriod;
        if (currentTime == 0) return startValue;
        if ((currentTime /= duration / 2) == 2) return startValue + changeNeeded;
        if (p == null) p = duration * (0.3f * 1.5f);
        float s;
        if (a == null || a < Mathf.Abs(changeNeeded)) { a = changeNeeded; s = p.GetValueOrDefault() / 4; }
        else s = p.GetValueOrDefault() / (2 * Mathf.PI) * Mathf.Asin(changeNeeded / a.GetValueOrDefault());
        if (currentTime < 1) return -0.5f * (a.GetValueOrDefault() * Mathf.Pow(2, 10 * (currentTime -= 1)) * Mathf.Sin((currentTime * duration - s) * (2 * Mathf.PI) / p.GetValueOrDefault())) + startValue;
        return a.GetValueOrDefault() * Mathf.Pow(2, -10 * (currentTime -= 1)) * Mathf.Sin((currentTime * duration - s) * (2 * Mathf.PI) / p.GetValueOrDefault()) * 0.5f + changeNeeded + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutInElasticEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseOutElasticEffect.Ease(currentTime * 2, startValue, changeNeeded / 2, duration);
        return TweenEaseInElasticEffect.Ease((currentTime * 2) - duration, startValue + changeNeeded / 2, changeNeeded / 2, duration);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInBackEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        var s = Tweener.Instance.EffectOvershoot ?? 1.70158f;
        return changeNeeded * (currentTime /= duration) * currentTime * ((s + 1) * currentTime - s) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutBackEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        var s = Tweener.Instance.EffectOvershoot ?? 1.70158f;
        return changeNeeded * ((currentTime = currentTime / duration - 1) * currentTime * ((s + 1) * currentTime + s) + 1) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInOutBackEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        var s = Tweener.Instance.EffectOvershoot ?? 1.70158f;
        if ((currentTime /= duration / 2) < 1) return changeNeeded / 2 * (currentTime * currentTime * (((s *= (1.525f)) + 1) * currentTime - s)) + startValue;
        return changeNeeded / 2 * ((currentTime -= 2) * currentTime * (((s *= (1.525f)) + 1) * currentTime + s) + 2) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutInBackEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseOutBackEffect.Ease(currentTime * 2, startValue, changeNeeded / 2, duration);
        return TweenEaseInBackEffect.Ease((currentTime * 2) - duration, startValue + changeNeeded / 2, changeNeeded / 2, duration);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInBounceEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return changeNeeded - TweenEaseOutBounceEffect.Ease(duration - currentTime, 0, changeNeeded, duration) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutBounceEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if ((currentTime /= duration) < (1 / 2.75))
        {
            return changeNeeded * (7.5625f * currentTime * currentTime) + startValue;
        }
        if (currentTime < (2 / 2.75))
        {
            return changeNeeded * (7.5625f * (currentTime -= (1.5f / 2.75f)) * currentTime + 0.75f) + startValue;
        }
        if (currentTime < (2.5 / 2.75))
        {
            return changeNeeded * (7.5625f * (currentTime -= (2.25f / 2.75f)) * currentTime + 0.9375f) + startValue;
        }
        return changeNeeded * (7.5625f * (currentTime -= (2.625f / 2.75f)) * currentTime + 0.984375f) + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseInOutBounceEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseInBounceEffect.Ease(currentTime * 2, 0, changeNeeded, duration) * 0.5f + startValue;
        return TweenEaseOutBounceEffect.Ease(currentTime * 2 - duration, 0, changeNeeded, duration) * 0.5f + changeNeeded * 0.5f + startValue;
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

public class TweenEaseOutInBounceEffect : ITweenEffect
{
    public static float Ease(float currentTime, float startValue, float changeNeeded, float duration)
    {
        if (currentTime < duration / 2) return TweenEaseOutBounceEffect.Ease(currentTime * 2, startValue, changeNeeded / 2, duration);
        return TweenEaseInBounceEffect.Ease((currentTime * 2) - duration, startValue + changeNeeded / 2, changeNeeded / 2, duration);
    }

    public float Execute(float currentTime, float startValue, float changeNeeded, float duration)
    {
        return Ease(currentTime, startValue, changeNeeded, duration);
    }
}

#endregion
