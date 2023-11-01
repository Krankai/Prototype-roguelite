using MoreMountains.Tools;
using System.Collections;
using UnityEngine;

public class DelayShootTriggerDetection : MMTriggerAndCollision
{
    private readonly float _delayChargingShoot = .3f;

    private bool _isChargingShoot = false;
    private bool _isFinishedCharge = false;

    private Coroutine _coroutineTrigger;
    private WaitForSeconds _wfsTrigger;


    private void Start()
    {
        _wfsTrigger = new WaitForSeconds(_delayChargingShoot);
    }

    protected override void OnTriggerStay(Collider collider)
    {
        if (TriggerLayerMask.MMContains(collider.gameObject))
        {
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

        if (!TriggerLayerMask.MMContains(collider.gameObject))
        {
            return;
        }

        CancelChargingShoot();
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
