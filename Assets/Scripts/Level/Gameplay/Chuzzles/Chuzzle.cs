﻿#region

using System;
using System.Collections;
using UnityEngine;

#endregion

[RequireComponent(typeof (TeleportableEntity))]
public abstract class Chuzzle : MonoBehaviour
{
    public float Alpha;

    public GameObject Arrow;
    public ChuzzleColor Color;

    public Cell Current;
    public GameObject Explosion;
    public bool IsCheckedForSearch;
    public bool IsDiying;
    public Cell MoveTo;
    public Cell Real;

    public Vector3 Velocity;

    public bool _shine;

    #region Events

    public event Action<Chuzzle> Died;
    public event Action<Chuzzle> AnimationFinished;
    public static event Action<Chuzzle> AnimationStarted;

    #endregion

    #region Event Handlers

    private void OnDeathAnimationEnd()
    {
        InvokeDied();
        InvokeAnimationFinished();
        Destroy(gameObject);
    }

    private void OnAnimateMoveEnd(object obj)
    {
        InvokeAnimationFinished();
    }

    #endregion

    #region Event Invokators

    protected virtual void InvokeAnimationFinished()
    {
        var handler = AnimationFinished;
        if (handler != null) handler(this);
    }

    protected virtual void InvokeDied()
    {
        var handler = Died;
        if (handler != null) handler(this);
    }

    protected void InvokeAnimationStarted()
    {
        var handler = AnimationStarted;
        if (handler != null) handler(this);
    }

    #endregion

    public bool Shine
    {
        get { return _shine; }
        set
        {
            _shine = value;
            Alpha = 1;
            if (!_shine && Arrow != null)
            {
                Destroy(Arrow);
            }
        }
    }

    public static Vector3 Scale
    {
        get { return Vector3.one; }
    }

    public override string ToString()
    {
        return string.Format("{0} ({1},{2}) - {3}", Color, Current.x, Current.y, GetType());
    }

    public bool Frozen { get; set; }

    protected virtual void Die()
    {
        InvokeAnimationStarted();
        IsDiying = true;
        //TODO Do Explosion
        if (Math.Abs(transform.localScale.x) > 0.01f)
        {
            iTween.ScaleTo(gameObject,
                iTween.Hash(
                    "x", 0,
                    "y", 0,
                    "z", 0,
                    "time", 0.2f));
            var ps = Instantiate(Explosion) as GameObject;
            //  Debug.Log("Ps: "+ps);
            ps.transform.position = transform.position;
            StartCoroutine("CheckIfAlive");
        }
        else
        {
            OnDeathAnimationEnd();
        }
    }

    public void AnimateMoveTo(Vector3 targetPosition)
    {
        InvokeAnimationStarted();
        if (Vector3.Distance(targetPosition, transform.position) > 0.01f)
        {
            iTween.MoveTo(gameObject,
                iTween.Hash(
                    "x", targetPosition.x,
                    "y", targetPosition.y,
                    "z", targetPosition.z,
                    "time", 0.3f,
                    "easetype", iTween.EaseType.easeInOutQuad,
                    "oncomplete", new Action<object>(OnAnimateMoveEnd),
                    "oncompleteparams", gameObject
                    ));
        }
        else
        {
            InvokeAnimationFinished();
        }
    }

    public virtual void Destroy()
    {
        Die();
    }

    private IEnumerator CheckIfAlive()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (!Explosion.particleSystem.IsAlive(true))
            {
                OnDeathAnimationEnd();
            }
        }
    }

    void Awake()
    {
        OnAwake();
    }

    protected abstract void OnAwake();
}