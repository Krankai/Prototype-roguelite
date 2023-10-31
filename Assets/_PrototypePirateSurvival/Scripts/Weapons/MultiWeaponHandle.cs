using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MultiWeaponHandle : MonoBehaviour, MMEventListener<MMGameEvent>
{
    #region Enum
    [System.Serializable]
    struct HandleWeaponControl
    {
        public CharacterHandleWeapon Ability;
        public bool IsActivated;
        public bool IsFaceRight;
    }
    #endregion Enum


    [SerializeField]
    private Character _character;
    [SerializeField]
    [FormerlySerializedAs("ListWeaponDirections")]
    private List<HandleWeaponControl> ListHandleWeaponControls;

    public List<CharacterHandleWeapon> ListActivatedHandleWeaponAbilities { get; set; } = new();


    protected virtual void Awake()
    {
        Initialization();
    }

    protected void OnEnable()
    {
        if (_character != default && _character.CharacterType == Character.CharacterTypes.Player)
        {
            this.MMEventStartListening();
        }
    }

    protected void OnDisable()
    {
        if (_character != default && _character.CharacterType == Character.CharacterTypes.Player)
        {
            this.MMEventStopListening();
        }
    }

    public void ToggleStateWeapon(Vector3 direction, bool isActivated)
    {
        if (ListHandleWeaponControls.Count < 4)
        {
            Debug.LogError("Error: not enough CharacterHandleWeapon abilities");
            return;
        }

        if (direction == Vector3.right)
        {
            ChangeActivateState(0, isActivated);
        }
        else if (direction == Vector3.left)
        {
            ChangeActivateState(1, isActivated);
        }
        else if (direction == Vector3.forward)
        {
            ChangeActivateState(2, isActivated);
        }
        else if (direction == Vector3.back)
        {
            ChangeActivateState(3, isActivated);
        }

        UpdateVisualization();

        MMGameEvent.Trigger("UpdateActivatedWeapons");
    }

    public bool IsWeaponOn(Vector3 direction)
    {
        if (ListHandleWeaponControls.Count < 4)
        {
            Debug.LogError("Error: not enough CharacterHandleWeapon abilities");
            return false;
        }

        if (direction == Vector3.right)
        {
            return ListHandleWeaponControls[0].IsActivated;
        }


        return false;
    }

    private void ChangeActivateState(int index, bool value)
    {
        var controlData = ListHandleWeaponControls[index];
        controlData.IsActivated = value;
        ListHandleWeaponControls[index] = controlData;
    }

    protected virtual void Initialization()
    {
        if (_character == default)
        {
            _character = transform.parent.GetComponentInParent<Character>();
        }

        for (int i = 0, count = ListHandleWeaponControls.Count; i < count; ++i)
        {
            var controlData = ListHandleWeaponControls[i];
            var handleWeaponAbility = controlData.Ability;

            if (handleWeaponAbility == default)
            {
                continue;
            }

            handleWeaponAbility.OnWeaponChange += () =>
            {
                var weaponAim = handleWeaponAbility.WeaponAimComponent as WeaponAim3DHorizontal;
                if (weaponAim != default)
                {
                    weaponAim.IsFaceRight = controlData.IsFaceRight;
                }

                var visualization = handleWeaponAbility.WeaponAttachment.gameObject.GetComponentInChildren<ShootingRangeVisualization>();
                if (visualization != default)
                {
                    visualization.IsFaceRight = controlData.IsFaceRight;
                    visualization.IsActivated = controlData.IsActivated;
                }
            };

            if (controlData.IsActivated)
            {
                ListActivatedHandleWeaponAbilities.Add(controlData.Ability);
            }
        }
    }

    protected virtual void UpdateVisualization()
    {
        ListActivatedHandleWeaponAbilities.Clear();

        for (int i = 0, count = ListHandleWeaponControls.Count; i < count; ++i)
        {
            var controlData = ListHandleWeaponControls[i];
            var handleWeaponAbility = controlData.Ability;

            if (handleWeaponAbility == default)
            {
                continue;
            }

            var visualization = handleWeaponAbility.WeaponAttachment.gameObject.GetComponentInChildren<ShootingRangeVisualization>();
            if (visualization != default)
            {
                visualization.IsFaceRight = controlData.IsFaceRight;
                visualization.IsActivated = controlData.IsActivated;
            }

            if (controlData.IsActivated)
            {
                ListActivatedHandleWeaponAbilities.Add(controlData.Ability);
            }
        }
    }

    protected virtual void ShootAllWeapons()
    {
        for (int i = 0, count = ListActivatedHandleWeaponAbilities.Count; i < count; ++i)
        {
            var handleWeaponAbility = ListActivatedHandleWeaponAbilities[i];
            handleWeaponAbility.ShootStart();
        }
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        if (eventType.EventName.Equals("DualShoot", System.StringComparison.OrdinalIgnoreCase))
        {
            ShootAllWeapons();
        }
    }
}
