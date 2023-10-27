using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class PrototypeLevelManager : LevelManager
{
    public float GroundSurfacePositionY => GroundTransform.position.y + _groundMeshRenderer.bounds.size.y / 2f;

    [Header("Ground")]
    public Transform GroundTransform;

    private MeshRenderer _groundMeshRenderer;

    protected override void Initialization()
    {
        base.Initialization();

        _groundMeshRenderer = GroundTransform.gameObject.MMGetComponentNoAlloc<MeshRenderer>();

        var mainPlayer = LevelManager.Instance.Players[0];
        if (mainPlayer != default)
        {
            var handleWeapon = mainPlayer.FindAbility<CharacterHandleWeapon>();
            var handleSecondaryWeapon = mainPlayer.FindAbility<CharacterHandleSecondaryWeapon>();

            handleWeapon.ForceAlwaysShoot = false;
            handleSecondaryWeapon.ForceAlwaysShoot = false;
        }
    }
}
