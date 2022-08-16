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
        public Color BeamColor;
        public Color BeamEmission;
        public Vector3 BeamSize;
        public float BeamSpeed;
        public float DespawnTime;
        public float BeamDamage;
        public bool BeamDismember;
        public Vector3 BeamScaleIncrease;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<BeamCustomization>().Setup(BeamDismember, BeamSpeed, DespawnTime, BeamDamage, BeamColor, BeamEmission, BeamSize, BeamScaleIncrease);
        }
    }
    public class BeamCustomization : MonoBehaviour
    {
        Item item;
        public Color beamColor;
        public Color beamEmission;
        public Vector3 beamSize;
        float despawnTime;
        float beamSpeed;
        float beamDamage;
        bool dismember;
        Vector3 beamScaleUpdate;
        List<RagdollPart> parts = new List<RagdollPart>();
        public void Start()
        {
            item = GetComponent<Item>();
            item.renderers[0].material.SetColor("_BaseColor", beamColor);
            item.renderers[0].material.SetColor("_EmissionColor", beamEmission * 2f);
            item.renderers[0].gameObject.transform.localScale = beamSize;
            item.rb.useGravity = false;
            item.rb.drag = 0;
            item.rb.AddForce(Player.local.head.transform.forward * beamSpeed, ForceMode.Impulse);
            item.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            item.RefreshCollision(true);
            item.Throw();
            item.Despawn(despawnTime);
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
        public void FixedUpdate()
        {
            item.gameObject.transform.localScale += beamScaleUpdate;
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
            if (c.GetComponentInParent<ColliderGroup>() != null)
            {
                ColliderGroup enemy = c.GetComponentInParent<ColliderGroup>();
                if (enemy?.collisionHandler?.ragdollPart != null && enemy?.collisionHandler?.ragdollPart?.ragdoll?.creature != Player.currentCreature)
                {
                    RagdollPart part = enemy.collisionHandler.ragdollPart;
                    if (part.ragdoll.creature != Player.currentCreature && part?.ragdoll?.creature?.gameObject?.activeSelf == true && part != null && !part.isSliced)
                    {
                        if (part.sliceAllowed && dismember)
                        {
                            if (!parts.Contains(part))
                                parts.Add(part);
                            CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, 20f));
                            instance.damageStruct.hitRagdollPart = part;
                            part.ragdoll.creature.Damage(instance);
                        }
                        else if (!part.ragdoll.creature.isKilled)
                        {
                            CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, beamDamage));
                            instance.damageStruct.hitRagdollPart = part;
                            part.ragdoll.creature.Damage(instance);
                            part.ragdoll.creature.TryPush(Creature.PushType.Hit, item.rb.velocity, 1);
                        }
                    }
                }
            }
        }
    }

}
