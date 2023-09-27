using System.Collections;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    public class DaggerModule : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<DaggerEffects>();
        }
    }
    public class DaggerEffects : MonoBehaviour
    {
        Item item;
        public void Start()
        {
            item = GetComponent<Item>();
            EffectInstance instance = Catalog.GetData<EffectData>("MirageFire").Spawn(item.transform, null, false);
            instance.SetRenderer(item.colliderGroups[0].imbueEffectRenderer, false);
            instance.SetIntensity(1);
            instance.Play();
        }
    }
    public class DaggerDespawn : MonoBehaviour
    {
        Item item;
        public void Start()
        {
            item = GetComponent<Item>();
            StartCoroutine(BeginDespawn(30));
        }
        public void OnCollisionEnter(Collision c)
        {
            if (c.collider.gameObject.GetComponentInParent<YamatoComponent>() != null || c.collider.gameObject.GetComponentInParent<SheathComponent>() != null) item.IgnoreObjectCollision(c.collider.gameObject.GetComponentInParent<Item>());
            else if (!item.IsHanded() && c.collider.gameObject.GetComponentInParent<Creature>() == null)
            {
                StartCoroutine(BeginDespawn(0.3f));
                item.physicBody.useGravity = true;
            }
            else if (!item.IsHanded())
            {
                StartCoroutine(BeginDespawn(10));
                item.physicBody.useGravity = true;
            }
        }
        public IEnumerator BeginDespawn(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (item.IsHanded()) yield break;
            foreach (Damager damager in item.GetComponentsInChildren<Damager>())
            {
                damager.UnPenetrateAll();
            }
            GameObject glass = new GameObject();
            glass.transform.position = item.transform.position;
            EffectInstance shatter = Catalog.GetData<EffectData>("GlassShatter").Spawn(glass.transform, null, false);
            shatter.SetIntensity(1);
            shatter.Play();
            Destroy(glass, 5);
            item.Despawn();
        }
    }
    public class HeavyRainBlades : MonoBehaviour
    {
        Item item;
        Damager pierce;
        Creature enemy;
        public void Start()
        {
            item = GetComponent<Item>();
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            pierce = item.GetComponentsInChildren<Damager>().FirstOrDefault(match => match.data.damageModifierData.damageType == DamageType.Pierce);
            StartCoroutine(BeginDespawn(15));
        }
        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (collisionInstance?.targetColliderGroup?.collisionHandler?.ragdollPart is RagdollPart part && collisionInstance?.damageStruct.damager == pierce && part.ragdoll.creature != Player.local.creature)
            {
                if (part?.ragdoll?.creature?.animator?.speed == 1)
                {
                    part.ragdoll.creature.animator.speed = 0.1f;
                    enemy = part.ragdoll.creature;
                }
                StartCoroutine(BeginDespawn(10));
            }
            else StartCoroutine(BeginDespawn(2));
        }
        public IEnumerator BeginDespawn(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            foreach (Damager damager in item.GetComponentsInChildren<Damager>())
            {
                damager.UnPenetrateAll();
            }
            if (enemy?.animator?.speed == 0.1f) enemy.animator.speed = 1;
            GameObject glass = new GameObject();
            glass.transform.position = item.transform.position;
            EffectInstance shatter = Catalog.GetData<EffectData>("GlassShatter").Spawn(glass.transform, null, false);
            shatter.SetIntensity(1);
            shatter.Play();
            Destroy(glass, 5);
            item.Despawn();
        }
    }
}
