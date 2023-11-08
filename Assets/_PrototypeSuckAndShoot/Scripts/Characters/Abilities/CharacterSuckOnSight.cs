using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    /// <summary>
    /// To be used with CharacterSuckable. Object with this component will suck 
    /// </summary>
    public class CharacterSuckOnSight : MonoBehaviour
    {
        [Header("Sucking")]
        // the cone of vision used to detect suckable objects
        [Tooltip("the cone of vision used to detect suckable objects")]
        public MMConeOfVision SuckConeOfVision;

        // the number of objects can be sucked at a time
        [Tooltip("the number of objects can be sucked at a time")]
        [Min(1)]
        public int CountSucking = 1;

        // the maximum number of objects that can be sucked
        [Tooltip("the maximum number of objects that can be sucked")]
        public int SuckLimit = 5;

        // the frame interval to scan for sucking targets
        [Tooltip("the frame interval to scan for sucking targets")]
        [Min(1), Range(1, 30)]
        public int FrameInterval = 3;


        [Header("Callbacks")]
        [SerializeField]
        protected UnityEvent<int> OnGainPointsEvent;


        [Header("Debug")]
        [MMReadOnly]
        public int CurrentSuckedCount = 0;


        protected CharacterSuckableDistanceComparer _comparer = new();
        protected List<CharacterSuckable> _listSucking = new();
        protected List<CharacterSuckable> _tempListSucked = new();


        protected virtual void LateUpdate()
        {
            if (Time.frameCount % FrameInterval == 0 && SuckConeOfVision != default)
            {
                ScanForSuckTargets();
            }
        }

        protected virtual void ScanForSuckTargets()
        {
            var listTargets = SuckConeOfVision.VisibleTargets;

            _tempListSucked.Clear();

            for (int i = 0, count = listTargets.Count; i < count; ++i)
            {
                var target = listTargets[i];
                if (target.gameObject == default)
                {
                    continue;
                }

                var suckable = target.gameObject.MMGetComponentNoAlloc<CharacterSuckable>();
                if (suckable == default)
                {
                    suckable = target.gameObject.GetComponentInParent<CharacterSuckable>();
                    if (suckable == default)
                    {
                        continue;
                    }
                }

                //if (_tempListSucked.Count < CountSucking)
                //{
                //    _tempListSucked.Add(suckable);
                //}
                _tempListSucked.Add(suckable);
            }

            for (int i = 0, count = _listSucking.Count; i < count; ++i)
            {
                var sucking = _listSucking[i];
                if (!_tempListSucked.Contains(sucking))
                {
                    sucking.CancelSucking();
                }
            }

            _tempListSucked.Sort(_comparer);
            //while (_tempListSucked.Count > CountSucking)
            //{
            //    var suckable = _tempListSucked[0];
            //    if (suckable != default)
            //    {
            //        suckable.CancelSucking();
            //    }
            //    //_tempListSucked.RemoveAt(0);
            //}

            for (int i = 0, count = _tempListSucked.Count; i < count; ++i)
            {
                var sucked = _tempListSucked[i];
                //if (!_listSucking.Contains(sucked))
                //{
                //    Suck(sucked);
                //}

                if (i < CountSucking && CurrentSuckedCount < SuckLimit)
                {
                    Suck(sucked);
                }
                else
                {
                    sucked.CancelSucking();
                }
            }

            _listSucking.Clear();
            _listSucking.AddRange(_tempListSucked);
        }

        protected virtual void Suck(CharacterSuckable suckable)
        {
            if (suckable.gameObject != default)
            {
                //StartCoroutine(CoroutineSuckWithDelay(0.5f, suckable));
                suckable.OnSucking(this);
            }
        }

        protected virtual IEnumerator CoroutineSuckWithDelay(float delay, CharacterSuckable suckable)
        {
            yield return new WaitForSeconds(delay);
            suckable.OnSucking(this);
        }

        //public virtual void OnGainPoints(int points)
        //{
        //    // TODO
        //    Debug.Log($"Gain points: {points}");

        //    OnGainPointsEvent?.Invoke(points);
        //}

        public virtual void OnSuckComplete(CharacterSuckable suckable)
        {
            CurrentSuckedCount++;
            OnGainPointsEvent?.Invoke(suckable.Points);
            //_listSucking.Remove(suckable);
        }

        public virtual void OnReleaseAllSucked()
        {
            CurrentSuckedCount = 0;
        }
    }

    // Compare to sort in decending order (i.e. farther distance first)
    public class CharacterSuckableDistanceComparer : IComparer<CharacterSuckable>
    {
        public int Compare(CharacterSuckable x, CharacterSuckable y)
        {
            return y.DistanceToSucker.CompareTo(x.DistanceToSucker);
        }
    }
}
