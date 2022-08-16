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
    public class JudgementCutPosition : MonoBehaviour
    {
        public Vector3 position = new Vector3();
        float time = 0;
        bool spawning = false;
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
            spawnedItem.gameObject.AddComponent<JudgementCutHit>();
            Destroy(gameObject);
        }
    }
    public class JudgementCutHit : MonoBehaviour
    {
        Item item;
        public void Start()
        {
            item = GetComponent<Item>();
            item.rb.isKinematic = true;
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
                if (collider.GetComponentInParent<RagdollPart>() != null && collider.GetComponentInParent<RagdollPart>().ragdoll.creature != Player.local.creature && collider.GetComponentInParent<RagdollPart>()?.ragdoll?.creature?.gameObject?.activeSelf == true && !collider.GetComponentInParent<RagdollPart>().isSliced)
                {
                    RagdollPart part = collider.GetComponentInParent<RagdollPart>();
                    if (part.sliceAllowed)
                    {
                        CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, 20f));
                        instance.damageStruct.hitRagdollPart = part;
                        part.ragdoll.creature.Damage(instance);
                        part.ragdoll.TrySlice(part);
                        if (part.data.sliceForceKill)
                            part.ragdoll.creature.Kill();
                        yield return null;
                    }
                    else if (!part.ragdoll.creature.isKilled)
                    {
                        CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, 20f));
                        instance.damageStruct.hitRagdollPart = part;
                        part.ragdoll.creature.Damage(instance);
                    }
                }
                if (collider.attachedRigidbody && !collider.attachedRigidbody.isKinematic)
                {
                    if (collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.NPC) || collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.Ragdoll))
                    {
                        RagdollPart component = collider.attachedRigidbody.gameObject.GetComponent<RagdollPart>();
                        if (component && !creaturesPushed.Contains(component.ragdoll.creature))
                        {
                            component.ragdoll.creature.TryPush(Creature.PushType.Magic, (component.ragdoll.rootPart.transform.position - item.transform.position).normalized, 1);
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
