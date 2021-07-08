using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
    public interface IProjectile
    {
        void Fire(Vector3 position, Vector3 velocity, float gravity, IAmmoEffect effect, Transform ignoreRoot, LayerMask layers, IDamageSource damageSource = null);
        void Teleport(Vector3 position, Quaternion rotation, bool relativeRotation = true);

        event UnityAction onTeleported;

        GameObject gameObject { get; }
    }
}