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
    public class SheathModule : ItemModule
    {
        public float DashSpeed = 1000;
        public string DashDirection = "Player";
        public bool DisableGravity = true;
        public bool DisableCollision = true;
        public float DashTime = 0.5f;
        public bool StopOnEnd = false;
        public bool StopOnStart = false;
        public bool ThumbstickDash = true;
        public bool SwapButtons = false;
        public int MultiDaggerCount = 8;
        public float MultiDaggerInterval = 0.07f;
        public bool DashRealTime = false;
        public ForceMode DashForceMode = ForceMode.Impulse;
        public bool DaggerDismemberment = true;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<SheathComponent>().Setup(DashSpeed, DashDirection, DisableGravity, DisableCollision, DashTime, StopOnEnd, StopOnStart, ThumbstickDash, SwapButtons, MultiDaggerCount, MultiDaggerInterval, DashRealTime, DashForceMode, DaggerDismemberment);
        }
    }
    public class SheathComponent : MonoBehaviour
    {
        Item item;
        public float DashSpeed;
        public string DashDirection;
        public bool DisableGravity;
        public bool DisableCollision;
        public float DashTime;
        public bool StopOnEnd;
        public bool StopOnStart;
        public int MultiDaggerCount;
        public float MultiDaggerInterval;
        public bool DashRealTime;
        bool ThumbstickDash;
        bool SwapButtons;
        bool holding = false;
        bool firing = false;
        float cdH;
        GameObject handleColliders;
        bool right = false;
        bool up = false;
        public ForceMode DashForceMode;
        public bool DaggerDismemberment;
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
            item.data.category = "Utilities";
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
                if (Time.time - cdH >= 0.75f)
                {
                    StartCoroutine(ShootMultiDaggers());
                    firing = true;
                }
            }
            else cdH = Time.time;
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if ((!SwapButtons && action == Interactable.Action.AlternateUseStart) || (SwapButtons && action == Interactable.Action.UseStart))
            {
                StopCoroutine(Dash());
                StartCoroutine(Dash());
            }
            if ((!SwapButtons && action == Interactable.Action.UseStart) || (SwapButtons && action == Interactable.Action.AlternateUseStart))
            {
                right = !right;
                if (!right) up = !up;
                Catalog.GetData<ItemData>("MirageBlade").SpawnAsync(ShootDagger,
                    Player.local.head.cam.transform.position + ((right ? Player.local.head.cam.transform.right : -Player.local.head.cam.transform.right) * 0.4f) +
                    (up ? Player.local.head.cam.transform.up * 0.25f : Vector3.zero),
                    Player.local.head.cam.transform.rotation);
                GameObject effect = new GameObject();
                effect.transform.position = Player.local.head.cam.transform.position + (right ? Player.local.head.cam.transform.right : -Player.local.head.cam.transform.right) +
                    (up ? Player.local.head.cam.transform.up * 0.5f : Vector3.zero);
                effect.transform.rotation = Quaternion.identity;
                EffectInstance instance = Catalog.GetData<EffectData>("MirageBladeSpawn").Spawn(effect.transform, false);
                instance.SetIntensity(1);
                instance.Play();
                Destroy(effect, 2);
                holding = true;
            }
            if ((!SwapButtons && action == Interactable.Action.UseStop) || (SwapButtons && action == Interactable.Action.AlternateUseStop))
            {
                holding = false;
                firing = false;
            }
        }
        public IEnumerator Dash()
        {
            if (StopOnStart) Player.local.locomotion.rb.velocity = Vector3.zero;
            if (Player.local.locomotion.moveDirection.magnitude <= 0 || !ThumbstickDash)
                if (DashDirection == "Item")
                {
                    Player.local.locomotion.rb.AddForce(item.mainHandler.grip.up * (!DashRealTime? DashSpeed : DashSpeed / Time.timeScale), DashForceMode);
                }
                else
                {
                    Player.local.locomotion.rb.AddForce(Player.local.head.transform.forward * (!DashRealTime ? DashSpeed : DashSpeed / Time.timeScale), DashForceMode);
                }
            else
            {
                Player.local.locomotion.rb.AddForce(Player.local.locomotion.moveDirection.normalized * (!DashRealTime ? DashSpeed : DashSpeed / Time.timeScale), DashForceMode);
            }
            if (DisableGravity)
                Player.local.locomotion.rb.useGravity = false;
            if (DisableCollision)
            {
                Player.local.locomotion.rb.detectCollisions = false;
                item.rb.detectCollisions = false;
                item.mainHandler.rb.detectCollisions = false;
                item.mainHandler.otherHand.rb.detectCollisions = false;
            }
            if (DashRealTime) yield return new WaitForSecondsRealtime(DashTime);
            else yield return new WaitForSeconds(DashTime);
            if (DisableGravity)
                Player.local.locomotion.rb.useGravity = true;
            if (DisableCollision)
            {
                Player.local.locomotion.rb.detectCollisions = true;
                item.rb.detectCollisions = true;
                item.mainHandler.rb.detectCollisions = true;
                item.mainHandler.otherHand.rb.detectCollisions = true;
            }
            if (StopOnEnd) Player.local.locomotion.rb.velocity = Vector3.zero;
            yield break;
        }
        public IEnumerator ShootMultiDaggers()
        {
            GameObject effect = new GameObject();
            effect.transform.position = Player.local.head.cam.transform.position;
            effect.transform.rotation = Quaternion.identity;
            EffectInstance instance = Catalog.GetData<EffectData>("MirageBladeSpawn").Spawn(effect.transform, false);
            instance.SetIntensity(1);
            instance.Play();
            Destroy(effect, 2);
            for (int i = 0; i < MultiDaggerCount; i++)
            {
                right = !right;
                if(!right) up = !up;
                Catalog.GetData<ItemData>("MirageBlade").SpawnAsync(ShootDagger,
                    Player.local.head.cam.transform.position + ((right ? Player.local.head.cam.transform.right : -Player.local.head.cam.transform.right) * 0.4f) +
                    (up ? Player.local.head.cam.transform.up * 0.25f : Vector3.zero),
                    Player.local.head.cam.transform.rotation);
                yield return new WaitForSeconds(MultiDaggerInterval);
            }
            yield break;
        }
        public void ShootDagger(Item spawnedItem)
        {
            Transform creature = GetEnemy()?.ragdoll?.targetPart?.transform;
            spawnedItem.rb.useGravity = false;
            spawnedItem.rb.drag = 0;
            if (creature != null)
                spawnedItem.rb.AddForce((creature.position - spawnedItem.transform.position).normalized * 45f, ForceMode.Impulse);
            else spawnedItem.rb.AddForce(Player.local.head.transform.forward * 45f, ForceMode.Impulse);
            spawnedItem.RefreshCollision(true);
            spawnedItem.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            spawnedItem.IgnoreObjectCollision(item);
            spawnedItem.gameObject.AddComponent<DaggerDespawn>();
            spawnedItem.Throw();
            foreach(Damager damager in spawnedItem.GetComponentsInChildren<Damager>())
            {
                if (!DaggerDismemberment)
                    damager.data.dismembermentAllowed = DaggerDismemberment;
            }
        }
        public Creature GetEnemy()
        {
            Creature closestCreature = null;
            if (Creature.allActive.Count <= 0) return null;
            foreach (Creature creature in Creature.allActive)
            {
                if (creature != null && !creature.isPlayer && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && Vector3.Dot(Player.local.head.cam.transform.forward.normalized, (creature.transform.position - Player.local.transform.position).normalized) >= 0.9f && closestCreature == null &&
                    Vector3.Distance(Player.local.transform.position, creature.transform.position) <= 25)
                {
                    closestCreature = creature;
                }
                else if (creature != null && !creature.isPlayer && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && Vector3.Dot(Player.local.head.cam.transform.forward.normalized, (creature.transform.position - Player.local.transform.position).normalized) >= 0.9f && closestCreature != null &&
                    Vector3.Distance(Player.local.transform.position, creature.transform.position) <= 25)
                {
                    if (Vector3.Distance(Player.local.transform.position, creature.transform.position) < Vector3.Distance(Player.local.transform.position, closestCreature.transform.position)) closestCreature = creature;
                }
            }
            return closestCreature;
        }
        public void Setup(float speed, string direction, bool gravity, bool collision, float time, bool stop, bool start, bool thumbstick, bool swap, int count, float interval, bool realtime, ForceMode dashForceMode, bool dismember)
        {
            DashSpeed = speed;
            DashDirection = direction;
            DisableGravity = gravity;
            DisableCollision = collision;
            DashTime = time;
            if (direction.ToLower().Contains("player") || direction.ToLower().Contains("head") || direction.ToLower().Contains("sight"))
            {
                DashDirection = "Player";
            }
            else if (direction.ToLower().Contains("item") || direction.ToLower().Contains("sheath") || direction.ToLower().Contains("flyref") || direction.ToLower().Contains("weapon"))
            {
                DashDirection = "Item";
            }
            StopOnEnd = stop;
            StopOnStart = start;
            ThumbstickDash = thumbstick;
            SwapButtons = swap;
            MultiDaggerCount = count;
            MultiDaggerInterval = interval;
            DashRealTime = realtime;
            DashForceMode = dashForceMode;
            DaggerDismemberment = dismember;
        }
    }
}
