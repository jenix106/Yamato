using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    public class SheathModule : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<SheathComponent>();
        }
    }
    public class SheathComponent : MonoBehaviour
    {
        Item item;
        bool holding = false;
        bool firing = false;
        float cdH;
        GameObject handleColliders;
        bool right = false;
        bool up = false;
        bool blisteringBlades = false;
        bool heavyRainBlades = false;
        bool stormBlades = false;
        bool spiralBlades = false;
        bool holderSwap = false;
        bool handleSwap = false;
        public void Start()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            handleColliders = item.GetCustomReference("HandleColliders").gameObject;
            if (item.GetComponent<YamatoSheathFrameworkComponent>() == null)
            {
                item.GetComponentInChildren<Holder>().Snapped += SheathComponent_Snapped;
                item.GetComponentInChildren<Holder>().UnSnapped += SheathComponent_UnSnapped;
            }
            else
            {
                handleColliders.SetActive(false);
            }
            item.data.category = "Utilities";
        }
        public void Update()
        {
            if (YamatoManager.SheathHeldOrientation && !holderSwap)
            {
                holderSwap = true;
                item.mainHandleRight.gameObject.transform.Rotate(new Vector3(0, 180, 0));
                RagdollHand ragdollHand = item.mainHandler;
                ragdollHand?.TryRelease();
                ragdollHand?.Grab(item.mainHandleRight, true);
            }
            else if (!YamatoManager.SheathHeldOrientation && holderSwap)
            {
                holderSwap = false;
                item.mainHandleRight.gameObject.transform.Rotate(new Vector3(0, -180, 0));
                RagdollHand ragdollHand = item.mainHandler;
                ragdollHand?.TryRelease();
                ragdollHand?.Grab(item.mainHandleRight, true);
            }
            if (YamatoManager.SheathHolsteredOrientation && !handleSwap)
            {
                handleSwap = true;
                item.holderPoint.Rotate(new Vector3(-180, 60, 0));
                Holder holder = item.holder;
                holder?.UnSnap(item, true, false);
                holder?.Snap(item, true, false);
            }
            else if (!YamatoManager.SheathHolsteredOrientation && handleSwap)
            {
                handleSwap = false;
                item.holderPoint.Rotate(new Vector3(180, 60, 0));
                Holder holder = item.holder;
                holder?.UnSnap(item, true, false);
                holder?.Snap(item, true, false);
            }
        }

        private void SheathComponent_UnSnapped(Item item)
        {
            handleColliders.SetActive(false);
        }

        private void SheathComponent_Snapped(Item item)
        {
            handleColliders.SetActive(true);
        }

        public void FixedUpdate()
        {
            if (holding && !firing)
            {
                if (Time.time - cdH >= 0.15f)
                {
                    if (blisteringBlades)
                    {
                        StartCoroutine(BlisteringBlades());
                    }
                    else if (heavyRainBlades)
                    {
                        StartCoroutine(HeavyRainBlades());
                    }
                    else if (stormBlades)
                    {
                        StartCoroutine(StormBlades());
                    }
                    else if (spiralBlades)
                    {
                        StartCoroutine(SpiralBlades());
                    }
                    blisteringBlades = false;
                    heavyRainBlades = false;
                    stormBlades = false;
                    spiralBlades = false;
                    firing = true;
                }
            }
            else cdH = Time.time;
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if ((YamatoManager.SwapSheathButtons && action == Interactable.Action.AlternateUseStart) || (!YamatoManager.SwapSheathButtons && action == Interactable.Action.UseStart))
            {
                StopCoroutine(Dash());
                StartCoroutine(Dash());
            }
            if ((YamatoManager.SwapSheathButtons && action == Interactable.Action.UseStart) || (!YamatoManager.SwapSheathButtons && action == Interactable.Action.AlternateUseStart))
            {
                right = !right;
                if (!right) up = !up;
                Catalog.GetData<ItemData>("MirageBlade").SpawnAsync(ShootBlades,
                    Player.local.head.cam.transform.position + ((right ? Player.local.head.cam.transform.right : -Player.local.head.cam.transform.right) * 0.25f) +
                    ((up ? Player.local.head.cam.transform.up : -Player.local.head.cam.transform.up) * 0.1f),
                    Player.local.head.cam.transform.rotation);
                GameObject effect = new GameObject();
                effect.transform.position = Player.local.head.cam.transform.position + ((right ? Player.local.head.cam.transform.right : -Player.local.head.cam.transform.right) * 0.25f) +
                    ((up ? Player.local.head.cam.transform.up : -Player.local.head.cam.transform.up) * 0.1f);
                effect.transform.rotation = Quaternion.identity;
                EffectInstance instance = Catalog.GetData<EffectData>("MirageBladeSpawn").Spawn(effect.transform, false);
                instance.SetIntensity(1);
                instance.Play();
                Destroy(effect, 2);
                holding = true;
                if (item.physicBody.velocity.sqrMagnitude >= 0.01f)
                {
                    blisteringBlades = Vector3.Angle((item.physicBody.velocity - item.mainHandler.creature.currentLocomotion.velocity).normalized, item.mainHandler.creature.player.head.cam.transform.forward.normalized) <= 45;
                    heavyRainBlades = Vector3.Angle((item.physicBody.velocity - item.mainHandler.creature.currentLocomotion.velocity).normalized, item.mainHandler.creature.player.head.cam.transform.forward.normalized) > 45 &&
                        Vector3.Angle((item.physicBody.velocity - item.mainHandler.creature.currentLocomotion.velocity).normalized, -item.mainHandler.creature.player.head.cam.transform.forward.normalized) > 45;
                    stormBlades = Vector3.Angle((item.physicBody.velocity - item.mainHandler.creature.currentLocomotion.velocity).normalized, -item.mainHandler.creature.player.head.cam.transform.forward.normalized) <= 45;
                }
                spiralBlades = item.physicBody.velocity.sqrMagnitude < 0.01f;
            }
            if ((YamatoManager.SwapSheathButtons && action == Interactable.Action.UseStop) || (!YamatoManager.SwapSheathButtons && action == Interactable.Action.AlternateUseStop))
            {
                holding = false;
                firing = false;
            }
        }
        public IEnumerator Dash()
        {
            if (YamatoManager.StopOnStart) Player.local.locomotion.rb.velocity = Vector3.zero;
            if (Player.local.locomotion.moveDirection.magnitude <= 0 || !YamatoManager.ThumbstickDash)
                if (YamatoManager.DashDirection == "Item")
                {
                    Player.local.locomotion.rb.AddForce(item.mainHandler.grip.up * (!YamatoManager.DashRealTime ? YamatoManager.DashSpeed : YamatoManager.DashSpeed / Time.timeScale), YamatoManager.DashForceMode);
                }
                else
                {
                    Player.local.locomotion.rb.AddForce(Player.local.head.transform.forward * (!YamatoManager.DashRealTime ? YamatoManager.DashSpeed : YamatoManager.DashSpeed / Time.timeScale), YamatoManager.DashForceMode);
                }
            else
            {
                Player.local.locomotion.rb.AddForce(Player.local.locomotion.moveDirection.normalized * (!YamatoManager.DashRealTime ? YamatoManager.DashSpeed : YamatoManager.DashSpeed / Time.timeScale), YamatoManager.DashForceMode);
            }
            if (YamatoManager.DisableGravity)
                Player.local.locomotion.rb.useGravity = false;
            if (YamatoManager.DisableBodyCollision)
            {
                Player.local.locomotion.rb.detectCollisions = false;
            }
            if (YamatoManager.DisableWeaponCollision)
            {
                item.physicBody.rigidBody.detectCollisions = false;
                item.mainHandler.physicBody.rigidBody.detectCollisions = false;
                item.mainHandler.otherHand.physicBody.rigidBody.detectCollisions = false;
            }
            if (YamatoManager.DashRealTime) yield return new WaitForSecondsRealtime(YamatoManager.DashTime);
            else yield return new WaitForSeconds(YamatoManager.DashTime);
            if (YamatoManager.DisableGravity)
                Player.local.locomotion.rb.useGravity = true;
            if (YamatoManager.DisableBodyCollision)
            {
                Player.local.locomotion.rb.detectCollisions = true;
            }
            if (YamatoManager.DisableWeaponCollision)
            {
                item.physicBody.rigidBody.detectCollisions = true;
                item.mainHandler.physicBody.rigidBody.detectCollisions = true;
                item.mainHandler.otherHand.physicBody.rigidBody.detectCollisions = true;
            }
            if (YamatoManager.StopOnEnd) Player.local.locomotion.rb.velocity = Vector3.zero;
            yield break;
        }
        public IEnumerator BlisteringBlades()
        {
            GameObject effect = new GameObject();
            effect.transform.position = Player.local.head.cam.transform.position;
            effect.transform.rotation = Quaternion.identity;
            EffectInstance instance = Catalog.GetData<EffectData>("MirageBladeSpawn").Spawn(effect.transform, false);
            instance.SetIntensity(1);
            instance.Play();
            Destroy(effect, 2);
            for (int i = 0; i < YamatoManager.BlisteringBladesCount; i++)
            {
                right = !right;
                if(!right) up = !up;
                Catalog.GetData<ItemData>("MirageBlade").SpawnAsync(ShootBlades,
                    Player.local.head.cam.transform.position + ((right ? Player.local.head.cam.transform.right : -Player.local.head.cam.transform.right) * 0.25f) +
                    ((up ? Player.local.head.cam.transform.up : -Player.local.head.cam.transform.up) * 0.1f),
                    Player.local.head.cam.transform.rotation);
                yield return new WaitForSeconds(YamatoManager.BlisteringBladesInterval);
            }
            yield break;
        }
        public IEnumerator HeavyRainBlades()
        {
            Transform enemy = GetEnemy()?.ragdoll?.targetPart?.transform;
            if (enemy != null)
            {
                GameObject effect = new GameObject();
                effect.transform.position = enemy.position + (Vector3.up * 5);
                effect.transform.rotation = Quaternion.identity;
                EffectInstance instance = Catalog.GetData<EffectData>("MirageBladeSpawn").Spawn(effect.transform, false);
                instance.SetIntensity(1);
                instance.Play();
                Destroy(effect, 2);
                Catalog.GetData<ItemData>("HeavyRainBlades").SpawnAsync(ShootHeavyRainBlades, enemy.position + (Vector3.up * 5), Quaternion.LookRotation(Vector3.down));
                yield return new WaitForSeconds(YamatoManager.HeavyRainBladesInterval);
                for (int i = 0; i < YamatoManager.HeavyRainBladesCount - 1; i++)
                {
                    float randomX = enemy.position.x + UnityEngine.Random.Range(-2.5f, 2.5f);
                    float randomZ = enemy.position.z + UnityEngine.Random.Range(-2.5f, 2.5f);
                    Catalog.GetData<ItemData>("HeavyRainBlades").SpawnAsync(ShootHeavyRainBlades, new Vector3(randomX, enemy.position.y + 5, randomZ), Quaternion.LookRotation(Vector3.down));
                    yield return new WaitForSeconds(YamatoManager.HeavyRainBladesInterval);
                }
            }
            yield break;
        }
        public IEnumerator StormBlades()
        {
            Transform creature = GetEnemy()?.ragdoll?.targetPart?.transform;
            if(creature != null)
            {
                GameObject effect = new GameObject();
                effect.transform.position = creature.position;
                effect.transform.rotation = Quaternion.identity;
                EffectInstance instance = Catalog.GetData<EffectData>("MirageBladeSpawn").Spawn(effect.transform, false);
                instance.SetIntensity(1);
                instance.Play();
                Destroy(effect, 2);
                float rotation = 0;
                for (int i = 0; i < YamatoManager.StormBladesCount; i++)
                {
                    Catalog.GetData<ItemData>("MirageBlade").SpawnAsync(spawnedItem =>
                    {
                        spawnedItem.transform.Rotate(new Vector3(0, 0, rotation));
                        rotation += 360/ YamatoManager.StormBladesCount;
                        spawnedItem.transform.position = creature.position + (spawnedItem.transform.up * 2f);
                        ShootStormBlades(spawnedItem, creature);
                    }, creature.position, Quaternion.LookRotation(Vector3.up));
                }
            }
            yield break;
        }
        public IEnumerator SpiralBlades()
        {
            Transform creature = item.mainHandler?.ragdoll?.targetPart?.transform;
            if(creature != null)
            {
                GameObject effect = new GameObject();
                effect.transform.position = creature.position;
                effect.transform.rotation = Quaternion.identity;
                EffectInstance instance = Catalog.GetData<EffectData>("MirageBladeSpawn").Spawn(effect.transform, false);
                instance.SetIntensity(1);
                instance.Play();
                Destroy(effect, 2);
                float rotation = 0;
                for (int i = 0; i < YamatoManager.StormBladesCount; i++)
                {
                    Catalog.GetData<ItemData>("MirageBlade").SpawnAsync(spawnedItem =>
                    {
                        spawnedItem.transform.Rotate(new Vector3(0, 0, rotation));
                        rotation += 360 / YamatoManager.SpiralBladesCount;
                        spawnedItem.transform.position = creature.position + (spawnedItem.transform.up * 2f);
                        ShootSpiralBlades(spawnedItem, creature);
                    }, creature.position, Quaternion.LookRotation(Vector3.up));
                }
            }
            yield break;
        }
        public void ShootSpiralBlades(Item spawnedItem, Transform player)
        {
            spawnedItem.transform.rotation = Quaternion.LookRotation(-(player.position - spawnedItem.transform.position).normalized, Vector3.up);
            spawnedItem.physicBody.useGravity = false;
            spawnedItem.physicBody.drag = 0;
            spawnedItem.physicBody.AddForce(-(player.position - spawnedItem.transform.position).normalized * YamatoManager.DaggerForce, ForceMode.Impulse);
            spawnedItem.RefreshCollision(true);
            spawnedItem.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            spawnedItem.IgnoreObjectCollision(item);
            spawnedItem.gameObject.AddComponent<DaggerDespawn>();
            spawnedItem.Throw();
            foreach (Damager damager in spawnedItem.GetComponentsInChildren<Damager>())
            {
                if (!YamatoManager.DaggerDismemberment)
                    damager.data.dismembermentAllowed = YamatoManager.DaggerDismemberment;
            }
        }
        public void ShootStormBlades(Item spawnedItem, Transform enemy)
        {
            spawnedItem.transform.LookAt(enemy);
            spawnedItem.physicBody.useGravity = false;
            spawnedItem.physicBody.drag = 0;
            spawnedItem.physicBody.AddForce((enemy.position - spawnedItem.transform.position).normalized * YamatoManager.DaggerForce, ForceMode.Impulse);
            spawnedItem.RefreshCollision(true);
            spawnedItem.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            spawnedItem.IgnoreObjectCollision(item);
            spawnedItem.gameObject.AddComponent<DaggerDespawn>();
            spawnedItem.Throw();
            foreach (Damager damager in spawnedItem.GetComponentsInChildren<Damager>())
            {
                if (!YamatoManager.DaggerDismemberment)
                    damager.data.dismembermentAllowed = YamatoManager.DaggerDismemberment;
            }
        }
        public void ShootHeavyRainBlades(Item spawnedItem)
        {
            spawnedItem.physicBody.drag = 0;
            spawnedItem.physicBody.AddForce(Vector3.down * YamatoManager.DaggerForce, ForceMode.Impulse);
            spawnedItem.RefreshCollision(true);
            spawnedItem.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            spawnedItem.IgnoreObjectCollision(item);
            spawnedItem.gameObject.AddComponent<HeavyRainBlades>();
            spawnedItem.Throw();
            foreach (Damager damager in spawnedItem.GetComponentsInChildren<Damager>())
            {
                if (!YamatoManager.DaggerDismemberment)
                    damager.data.dismembermentAllowed = YamatoManager.DaggerDismemberment;
            }
        }
        public void ShootBlades(Item spawnedItem)
        {
            Transform creature = GetEnemy()?.ragdoll?.targetPart?.transform;
            spawnedItem.physicBody.useGravity = false;
            spawnedItem.physicBody.drag = 0;
            if (creature != null)
                spawnedItem.physicBody.AddForce((creature.position - spawnedItem.transform.position).normalized * YamatoManager.DaggerForce, ForceMode.Impulse);
            else spawnedItem.physicBody.AddForce(Player.local.head.transform.forward * YamatoManager.DaggerForce, ForceMode.Impulse);
            spawnedItem.RefreshCollision(true);
            spawnedItem.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            spawnedItem.IgnoreObjectCollision(item);
            spawnedItem.gameObject.AddComponent<DaggerDespawn>();
            spawnedItem.Throw();
            foreach(Damager damager in spawnedItem.GetComponentsInChildren<Damager>())
            {
                if (!YamatoManager.DaggerDismemberment)
                    damager.data.dismembermentAllowed = YamatoManager.DaggerDismemberment;
            }
        }
        public Creature GetEnemy()
        {
            Creature closestCreature = null;
            if (Creature.allActive.Count <= 0) return null;
            foreach (Creature creature in Creature.allActive)
            {
                if (creature != null && !creature.isPlayer && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && Vector3.Angle(Player.local.head.cam.transform.forward.normalized, (creature.ragdoll.targetPart.transform.position - Player.local.head.cam.transform.position).normalized) <= 20 && closestCreature == null &&
                    Vector3.Distance(Player.local.transform.position, creature.ragdoll.targetPart.transform.position) <= 25)
                {
                    closestCreature = creature;
                }
                else if (creature != null && !creature.isPlayer && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && Vector3.Angle(Player.local.head.cam.transform.forward.normalized, (creature.ragdoll.targetPart.transform.position - Player.local.head.cam.transform.position).normalized) <= 20 && closestCreature != null &&
                    Vector3.Distance(Player.local.transform.position, creature.ragdoll.targetPart.transform.position) <= 25)
                {
                    if (Vector3.Distance(Player.local.head.cam.transform.position, creature.ragdoll.targetPart.transform.position) < Vector3.Distance(Player.local.head.cam.transform.position, closestCreature.ragdoll.targetPart.transform.position)) closestCreature = creature;
                }
            }
            return closestCreature;
        }
    }
}
