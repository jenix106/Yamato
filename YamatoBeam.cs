using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    public class BeamModule : ItemModule
    {
        public Color BeamColor = new Color(0.0823529412f, 1, 1, 0.392156863f);
        public Color BeamEmission = new Color(0, 5.24313725f, 5.24313725f, 0);
        public Vector3 BeamSize = new Vector3(0.0375f, 1.65f, 0.0375f);
        public float BeamSpeed = 50;
        public float DespawnTime = 1.5f;
        public float BeamDamage = 5;
        public bool BeamDismember = true;
        public Vector3 BeamScaleIncrease = new Vector3(0, 0.2f, 0);
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<BeamCustomization>().Setup(BeamDismember, BeamSpeed, DespawnTime, BeamDamage, BeamColor, BeamEmission, BeamSize, BeamScaleIncrease);
        }
    }
    public class BeamCustomization : MonoBehaviour
    {
        Item item;
        public Item yamato;
        public Color beamColor;
        public Color beamEmission;
        public Vector3 beamSize;
        float despawnTime;
        float beamSpeed;
        float beamDamage;
        bool dismember;
        Vector3 beamScaleUpdate;
        List<RagdollPart> parts = new List<RagdollPart>();
        Imbue imbue;
        public void Start()
        {
            item = GetComponent<Item>();
            item.renderers[0].material.SetColor("_BaseColor", beamColor);
            item.renderers[0].material.SetColor("_EmissionColor", beamEmission * 2f);
            item.renderers[0].gameObject.transform.localScale = beamSize;
            item.mainCollisionHandler.ClearPhysicModifiers();
            item.rb.useGravity = false;
            item.rb.drag = 0;
            if(yamato?.mainHandler != null && yamato?.mainHandler?.creature?.brain.currentTarget is Creature target) item.rb.AddForce(-(yamato.mainHandler.creature.transform.position - target.transform.position).normalized * beamSpeed, ForceMode.Impulse);
            else if (yamato?.mainHandler != null) item.rb.AddForce(Player.local.head.transform.forward * beamSpeed, ForceMode.Impulse);
            item.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            item.RefreshCollision(true);
            item.Throw();
            item.Despawn(despawnTime);
            item.disallowDespawn = true;
            imbue = item.colliderGroups[0].imbue; 
        }
        public void Setup(bool beamDismember, float BeamSpeed, float BeamDespawn, float BeamDamage, Color color, Color emission, Vector3 size, Vector3 scaleUpdate)
        {
            dismember = beamDismember;
            beamSpeed = BeamSpeed;
            despawnTime = BeamDespawn;
            beamDamage = BeamDamage;
            beamColor = color;
            beamEmission = emission;
            beamSize = size;
            beamScaleUpdate = scaleUpdate;
        }
        public void Update()
        {
            item.gameObject.transform.localScale += beamScaleUpdate * (Time.deltaTime * 100);
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
            if (c.GetComponentInParent<ColliderGroup>() is ColliderGroup group && group.collisionHandler.isRagdollPart)
            {
                RagdollPart part = group.collisionHandler.ragdollPart;
                if (part.ragdoll.creature != yamato?.lastHandler?.creature && part.ragdoll.creature.gameObject.activeSelf == true && !part.isSliced)
                {
                    CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, beamDamage))
                    {
                        targetCollider = c,
                        targetColliderGroup = group,
                        sourceColliderGroup = item.colliderGroups[0],
                        sourceCollider = item.colliderGroups[0].colliders[0],
                        casterHand = yamato?.lastHandler?.caster,
                        impactVelocity = item.rb.velocity,
                        contactPoint = c.transform.position,
                        contactNormal = -item.rb.velocity
                    };
                    instance.damageStruct.penetration = DamageStruct.Penetration.None;
                    instance.damageStruct.hitRagdollPart = part;
                    if (part.sliceAllowed && !part.ragdoll.creature.isPlayer && dismember)
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
                        part.ragdoll.creature.TryPush(Creature.PushType.Hit, item.rb.velocity, 3, part.type);
                        part.rb.AddForce(item.rb.velocity, ForceMode.VelocityChange);
                    }
                    else
                    {
                        if(imbue?.spellCastBase != null && imbue.energy > 0)
                        {
                            imbue.spellCastBase.OnImbueCollisionStart(instance);
                        }
                        part.ragdoll.creature.TryPush(Creature.PushType.Hit, item.rb.velocity, 1, part.type);
                    }
                    part.ragdoll.creature.Damage(instance);
                }
            }
        }
    }

}
