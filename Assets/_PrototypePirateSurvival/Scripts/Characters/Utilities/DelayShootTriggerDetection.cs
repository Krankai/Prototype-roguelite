using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using UnityEngine;

public class DelayShootTriggerDetection : MMTriggerAndCollision
{
    private readonly float _delayChargingShoot = .3f;

    private bool _isChargingShoot = false;
    private bool _isFinishedCharge = false;

    private Coroutine _coroutineTrigger;
    private WaitForSeconds _wfsTrigger;

    private Character _ownerCharacter;


    private void Start()
    {
        _wfsTrigger = new WaitForSeconds(_delayChargingShoot);
    }

    private void RefreshOwner()
    {
        if (_ownerCharacter != default)
        {
            return;
        }

        _ownerCharacter = gameObject.GetComponentInParent<Character>();
    }

    protected override void OnTriggerStay(Collider collider)
    {
        if (TriggerLayerMask.MMContains(collider.gameObject))
        {
            RefreshOwner();

            if (_ownerCharacter == default)
            {
                return;
            }

            bool isAlly = _ownerCharacter.gameObject.layer == collider.gameObject.layer;
            if (isAlly)
            {
                return;
            }

            if (_isFinishedCharge)
            {
                OnTriggerStayEvent?.Invoke();
            }
            else if (!_isChargingShoot)
            {
                _coroutineTrigger = StartCoroutine(CoroutineTriggerDetectEvent());
            }
        }
    }

    protected override void OnTriggerExit(Collider collider)
    {
        base.OnTriggerExit(collider);

        if (TriggerLayerMask.MMContains(collider.gameObject))
        {
            //if (collider.gameObject == LevelManager.Instance.Players[0].gameObject)
            //{
            //    CancelChargingShoot();
            //    OnTriggerExitEvent?.Invoke();
            //}

            CancelChargingShoot();
        }
    }

    private IEnumerator CoroutineTriggerDetectEvent()
    {
        _isChargingShoot = true;
        _isFinishedCharge = false;

        Debug.LogError("Start locking aim...");
        yield return _wfsTrigger;
        Debug.LogError("Finish locking aim");

        OnTriggerStayEvent?.Invoke();

        _isFinishedCharge = true;
        _isChargingShoot = false;
    }

    private void CancelChargingShoot()
    {
        if (_isChargingShoot || _isFinishedCharge)
        {
            if (_coroutineTrigger != default)
            {
                StopCoroutine(_coroutineTrigger);
            }

            _isChargingShoot = false;
            _isFinishedCharge = false;


            Debug.LogError("Cancel locking aim and/or shooting");
        }
    }
}
