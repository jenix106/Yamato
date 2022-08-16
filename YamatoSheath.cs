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
        public float DashSpeed;
        public string DashDirection;
        public bool DisableGravity;
        public bool DisableCollision;
        public float DashTime;
        public bool StopOnEnd;
        public bool StopOnStart;
        public bool ThumbstickDash;
        public bool SwapButtons;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<SheathComponent>().Setup(DashSpeed, DashDirection, DisableGravity, DisableCollision, DashTime, StopOnEnd, StopOnStart, ThumbstickDash, SwapButtons);
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
        bool ThumbstickDash;
        bool SwapButtons;
        bool holding = false;
        bool firing = false;
        float cdH;
        GameObject handleColliders;
        public void Start()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            handleColliders = item.GetCustomReference("HandleColliders").gameObject;
            item.GetComponentInChildren<Holder>().Snapped += SheathComponent_Snapped;
            item.GetComponentInChildren<Holder>().UnSnapped += SheathComponent_UnSnapped;
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
                Vector3 v;
                v.x = UnityEngine.Random.Range(-0.15f, 0.15f);
                v.y = UnityEngine.Random.Range(-0.15f, 0.15f);
                v.z = UnityEngine.Random.Range(-0.15f, 0.15f);
                Catalog.GetData<ItemData>("MirageBlade").SpawnAsync(ShootDagger, new Vector3(Player.local.head.cam.transform.position.x + v.x, Player.local.head.cam.transform.position.y + v.y, Player.local.head.cam.transform.position.z + v.z),
                    Player.local.head.cam.transform.rotation);
                GameObject effect = new GameObject();
                effect.transform.position = new Vector3(Player.local.head.cam.transform.position.x + v.x, Player.local.head.cam.transform.position.y + v.y, Player.local.head.cam.transform.position.z + v.z);
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
                    Player.local.locomotion.rb.AddForce(item.mainHandler.grip.up * DashSpeed, ForceMode.Impulse);
                }
                else
                {
                    Player.local.locomotion.rb.AddForce(Player.local.head.transform.forward * DashSpeed, ForceMode.Impulse);
                }
            else
            {
                Player.local.locomotion.rb.AddForce(Player.local.locomotion.moveDirection.normalized * DashSpeed, ForceMode.Impulse);
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
            yield return new WaitForSeconds(DashTime);
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
            for (int i = 0; i < 8; i++)
            {
                Vector3 v;
                v.x = UnityEngine.Random.Range(-0.15f, 0.15f);
                v.y = UnityEngine.Random.Range(-0.15f, 0.15f);
                v.z = UnityEngine.Random.Range(-0.15f, 0.15f);
                Catalog.GetData<ItemData>("MirageBlade").SpawnAsync(ShootDagger, new Vector3(Player.local.head.cam.transform.position.x + v.x, Player.local.head.cam.transform.position.y + v.y, Player.local.head.cam.transform.position.z + v.z),
                    Player.local.head.cam.transform.rotation);
                yield return new WaitForSeconds(0.07f);
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
                    if (Vector3.Dot(Player.local.head.cam.transform.forward, (creature.transform.position - Player.local.transform.position)) >
                    Vector3.Dot(Player.local.head.cam.transform.forward, (closestCreature.transform.position - Player.local.transform.position)))
                        closestCreature = creature;
                }
            }
            return closestCreature;
        }
        public void Setup(float speed, string direction, bool gravity, bool collision, float time, bool stop, bool start, bool thumbstick, bool swap)
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
        }
    }
}
