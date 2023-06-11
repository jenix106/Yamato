using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    public class JudgementCutPosition : MonoBehaviour
    {
        public Vector3 position = new Vector3();
        public Item yamato;
        float time = 0;
        bool spawning = false;
        public float damage;
        public bool dismember;
        public void Update()
        {
            time += Time.deltaTime;
            if (time >= 0.2 && !spawning)
            {
                Catalog.GetData<ItemData>("JudgementCut").SpawnAsync(ShootJudgementCut, position, Quaternion.identity);
                spawning = true;
            }
        }
        public void ShootJudgementCut(Item spawnedItem)
        {
            JudgementCutHit hit = spawnedItem.gameObject.AddComponent<JudgementCutHit>();
            hit.yamato = yamato;
            hit.damage = damage;
            hit.dismember = dismember;
            Destroy(gameObject);
        }
    }
    public class JudgementCutHit : MonoBehaviour
    {
        Item item;
        public Item yamato;
        Imbue imbue;
        public float damage;
        public bool dismember;
        public void Start()
        {
            item = GetComponent<Item>();
            imbue = yamato.colliderGroups[0].imbue;
            Item.allThrowed.Add(item);
            StartCoroutine(AnimeSlice());
            GameObject effect = new GameObject();
            effect.transform.position = item.transform.position;
            effect.transform.rotation = Quaternion.identity;
            EffectInstance instance = Catalog.GetData<EffectData>("JudgementCutHit").Spawn(effect.transform, false);
            instance.SetIntensity(2);
            instance.Play();
            Destroy(effect, 2);
            item.Despawn(5);
        }
        public IEnumerator AnimeSlice()
        {
            List<Creature> creaturesPushed = new List<Creature>();
            List<Rigidbody> rigidbodiesPushed = new List<Rigidbody>();
            yield return null;
            foreach (Collider collider in Physics.OverlapSphere(item.transform.position, 1.5f))
            {
                if (collider.GetComponentInParent<Breakable>() is Breakable breakable)
                {
                    float sqrMagnitude = item.physicBody.velocity.sqrMagnitude;
                    --breakable.hitsUntilBreak;
                    if (breakable.canInstantaneouslyBreak)
                        breakable.hitsUntilBreak = 0;
                    breakable.onTakeDamage?.Invoke(sqrMagnitude);
                    if (!breakable.IsBroken && breakable.hitsUntilBreak <= 0)
                        breakable.Break();
                }
                if (collider.GetComponentInParent<RagdollPart>() is RagdollPart part && part.ragdoll.creature != Player.local.creature && part?.ragdoll?.creature?.gameObject?.activeSelf == true && !part.isSliced)
                {
                    part.gameObject.SetActive(true);
                    CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, damage))
                    {
                        targetCollider = part.colliderGroup.colliders[0],
                        targetColliderGroup = part.colliderGroup,
                        sourceCollider = yamato.colliderGroups[0].colliders[0],
                        sourceColliderGroup = yamato.colliderGroups[0],
                        casterHand = yamato.lastHandler.caster,
                        impactVelocity = yamato.physicBody.velocity,
                        contactPoint = part.transform.position,
                        contactNormal = -yamato.physicBody.velocity
                    };
                    instance.damageStruct.hitRagdollPart = part;
                    if (imbue.energy > 0)
                    {
                        imbue.spellCastBase.OnImbueCollisionStart(instance);
                        yield return null;
                    }
                    if (part.sliceAllowed && dismember)
                    {
                        part.ragdoll.TrySlice(part);
                        if (part.data.sliceForceKill)
                            part.ragdoll.creature.Kill();
                        yield return null;
                    }
                    part.ragdoll.creature.Damage(instance);
                }
                if (collider.attachedRigidbody && !collider.attachedRigidbody.isKinematic)
                {
                    if (collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.NPC) || collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.Ragdoll))
                    {
                        RagdollPart component = collider.attachedRigidbody.gameObject.GetComponent<RagdollPart>();
                        if (component && !creaturesPushed.Contains(component.ragdoll.creature))
                        {
                            if (component.ragdoll.creature.locomotion.isGrounded)
                                component.ragdoll.creature.TryPush(Creature.PushType.Magic, (component.ragdoll.rootPart.transform.position - item.transform.position).normalized, 1);
                            else
                            {
                                component.ragdoll.creature.locomotion.rb.velocity = Vector3.zero;
                                foreach(RagdollPart ragdollPart in component.ragdoll.parts)
                                {
                                    ragdollPart.physicBody.velocity = Vector3.zero;
                                }
                                component.ragdoll.creature.locomotion.rb.AddForce(Vector3.up * 2, ForceMode.Impulse);
                            }
                            creaturesPushed.Add(component.ragdoll.creature);
                        }
                    }
                    if (collider.attachedRigidbody.gameObject.layer != GameManager.GetLayer(LayerName.NPC) && !rigidbodiesPushed.Contains(collider.attachedRigidbody))
                    {
                        collider.attachedRigidbody.AddExplosionForce(2, item.transform.position, 1.5f, 0, ForceMode.VelocityChange);
                        rigidbodiesPushed.Add(collider.attachedRigidbody);
                    }
                }
            }
            Item.allThrowed.Remove(item);
            yield break;
        }
    }
}
