using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    public class MirageBeam : MonoBehaviour
    {
        public Item item;
        public Item mirage;
        public Creature user;
        List<RagdollPart> parts = new List<RagdollPart>();
        public Imbue imbue;
        public bool isAnimeSlice = false;
        public YamatoComponent yamatoComponent;
        public Renderer renderer;
        public void Start()
        {
            item = GetComponent<Item>();
            item.Despawn(YamatoManager.MirageDespawnTime);
            item.disallowDespawn = true;
            renderer = item.GetCustomReference<Renderer>("Renderer");
            renderer.material.SetColor("_BaseColor", new Color(YamatoManager.MirageBeamColorR / 255, YamatoManager.MirageBeamColorG / 255, YamatoManager.MirageBeamColorB / 255, YamatoManager.MirageBeamColorA / 255) * YamatoManager.MirageBeamColorIntensity);
            renderer.material.SetColor("_EmissionColor", new Color(YamatoManager.MirageBeamEmissionR / 255, YamatoManager.MirageBeamEmissionG / 255, YamatoManager.MirageBeamEmissionB / 255, YamatoManager.MirageBeamEmissionA / 255) * YamatoManager.MirageBeamEmissionIntensity);
            renderer.gameObject.transform.localScale = new Vector3(YamatoManager.MirageBeamSizeX, YamatoManager.MirageBeamSizeY, YamatoManager.MirageBeamSizeZ);
            item.mainCollisionHandler?.ClearPhysicModifiers();
            item.physicBody.useGravity = false;
            item.physicBody.drag = 0;
            item.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            item.RefreshCollision(true);
            item.Throw();
            imbue = item.colliderGroups[0]?.imbue;
        }
        public void Update()
        {
            item.gameObject.transform.localScale += new Vector3(YamatoManager.MirageBeamScaleX, YamatoManager.MirageBeamScaleY, YamatoManager.MirageBeamScaleZ) * (Time.deltaTime * 100);
            if (parts.Count > 0)
            {
                parts[0].gameObject.SetActive(true);
                parts[0].bone.animationJoint.gameObject.SetActive(true);
                parts[0].ragdoll.TrySlice(parts[0]);
                if (parts[0].data.sliceForceKill)
                    parts[0].ragdoll.creature.Kill();
                parts.RemoveAt(0);
            }
        }
        public void OnTriggerEnter(Collider c)
        {
            if (c.GetComponentInParent<Breakable>() is Breakable breakable)
            {
                if (item.physicBody.velocity.sqrMagnitude < breakable.neededImpactForceToDamage)
                    return;
                float sqrMagnitude = item.physicBody.velocity.sqrMagnitude;
                --breakable.hitsUntilBreak;
                if (breakable.canInstantaneouslyBreak && sqrMagnitude >= breakable.instantaneousBreakVelocityThreshold)
                    breakable.hitsUntilBreak = 0;
                breakable.onTakeDamage?.Invoke(sqrMagnitude);
                if (breakable.IsBroken || breakable.hitsUntilBreak > 0)
                    return;
                breakable.Break();
            }
            if (c.GetComponentInParent<ColliderGroup>() is ColliderGroup group && group.collisionHandler.isRagdollPart)
            {
                RagdollPart part = group.collisionHandler.ragdollPart;
                if (part.ragdoll.creature != user && part.ragdoll.creature.gameObject.activeSelf == true && !part.isSliced)
                {
                    CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, YamatoManager.MirageBeamDamage))
                    {
                        targetCollider = c,
                        targetColliderGroup = group,
                        sourceColliderGroup = item.colliderGroups[0],
                        sourceCollider = item.colliderGroups[0].colliders[0],
                        casterHand = mirage?.lastHandler?.caster,
                        impactVelocity = item.physicBody.velocity,
                        contactPoint = c.transform.position,
                        contactNormal = -item.physicBody.velocity
                    };
                    instance.damageStruct.penetration = DamageStruct.Penetration.None;
                    instance.damageStruct.hitRagdollPart = part;
                    if (part.sliceAllowed && !part.ragdoll.creature.isPlayer && YamatoManager.MirageBeamDismember)
                    {
                        Vector3 direction = part.GetSliceDirection();
                        float num1 = Vector3.Dot(direction, item.transform.up);
                        float num2 = 1f / 3f;
                        if (num1 < num2 && num1 > -num2 && !parts.Contains(part))
                        {
                            parts.Add(part);
                        }
                    }
                    if (imbue?.spellCastBase?.GetType() == typeof(SpellCastLightning))
                    {
                        part.ragdoll.creature.TryElectrocute(1, 2, true, true, (imbue.spellCastBase as SpellCastLightning).imbueHitRagdollEffectData);
                        imbue.spellCastBase.OnImbueCollisionStart(instance);
                    }
                    if (imbue?.spellCastBase?.GetType() == typeof(SpellCastProjectile))
                    {
                        instance.damageStruct.damage *= 2;
                        imbue.spellCastBase.OnImbueCollisionStart(instance);
                    }
                    if (imbue?.spellCastBase?.GetType() == typeof(SpellCastGravity))
                    {
                        imbue.spellCastBase.OnImbueCollisionStart(instance);
                        part.ragdoll.creature.TryPush(Creature.PushType.Hit, item.physicBody.velocity, 3, part.type);
                        part.physicBody.AddForce(item.physicBody.velocity, ForceMode.VelocityChange);
                    }
                    else
                    {
                        if (imbue?.spellCastBase != null && imbue.energy > 0)
                        {
                            imbue.spellCastBase.OnImbueCollisionStart(instance);
                        }
                        part.ragdoll.creature.TryPush(Creature.PushType.Hit, item.physicBody.velocity, 1, part.type);
                    }
                    part.ragdoll.creature.Damage(instance);
                }
            }
        }
    }

}
