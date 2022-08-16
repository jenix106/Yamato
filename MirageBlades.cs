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
            EffectInstance instance = Catalog.GetData<EffectData>("MirageFire").Spawn(item.transform, false);
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
        }
        public void OnCollisionEnter(Collision c)
        {
            if (c.collider.gameObject.GetComponentInParent<YamatoComponent>() != null || c.collider.gameObject.GetComponentInParent<SheathComponent>() != null) item.IgnoreObjectCollision(c.collider.gameObject.GetComponentInParent<Item>());
            else if (!item.IsHanded())
            {
                StartCoroutine(BeginDespawn());
                item.rb.useGravity = true;
            }
        }
        public IEnumerator BeginDespawn()
        {
            yield return new WaitForSeconds(0.3f);
            if (item.IsHanded()) yield break;
            foreach (Damager damager in item.GetComponentsInChildren<Damager>())
            {
                damager.UnPenetrateAll();
            }
            item.Despawn();
        }
    }
}
