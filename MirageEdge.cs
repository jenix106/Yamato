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
    public class MirageModule : ItemModule
    {
        public float DashSpeed = 1000;
        public string DashDirection = "Item";
        public bool DisableGravity = true;
        public bool DisableCollision = false;
        public float DashTime = 0.5f;
        public float BeamCooldown = 0.15f;
        public float SwordSpeed = 7;
        public float RotateDegreesPerSecond = 2160;
        public float ReturnSpeed = 10;
        public bool StopOnEnd = false;
        public bool StopOnStart = false;
        public bool ThumbstickDash = true;
        public bool SwapButtons = false;
        public bool ToggleSwordBeams = false;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<MirageComponent>().Setup(DashSpeed, DashDirection, DisableGravity, DisableCollision, DashTime, SwordSpeed, BeamCooldown, RotateDegreesPerSecond, ReturnSpeed, StopOnEnd, StopOnStart, ThumbstickDash, SwapButtons, ToggleSwordBeams);
        }
    }
    public class MirageComponent : MonoBehaviour
    {
        Item item;
        bool isThrown;
        Holder lastHolder;
        RagdollHand lastHandler;
        bool startUpdate;
        public float DashSpeed;
        public string DashDirection;
        public bool DisableGravity;
        public bool DisableCollision;
        public float DashTime;
        float cdH;
        float cooldown;
        float swordSpeed;
        bool beam;
        public float RotationSpeed;
        public float ReturnSpeed;
        public bool StopOnEnd;
        public bool StopOnStart;
        bool ThumbstickDash;
        bool SwapButtons;
        bool ToggleSwordBeams;
        public void Start()
        {
            item = GetComponent<Item>();
            item.OnUnSnapEvent += Item_OnUnSnapEvent;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;
            item.OnTelekinesisReleaseEvent += Item_OnTelekinesisReleaseEvent;
            EffectInstance instance = Catalog.GetData<EffectData>("MirageFire").Spawn(item.transform, false);
            instance.SetRenderer(item.colliderGroups[0].imbueEffectRenderer, false);
            instance.SetIntensity(1);
            instance.Play();
            item.data.category = "Utilities";
        }

        private void Item_OnTelekinesisReleaseEvent(Handle handle, SpellTelekinesis teleGrabber)
        {
            lastHandler = null;
        }

        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            lastHandler = ragdollHand;
            beam = false;
        }

        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            item.rb.useGravity = true;
            isThrown = false;
            startUpdate = false;
        }

        public void Setup(float speed, string direction, bool gravity, bool collision, float time, float SwordSpeed, float BeamCooldown, float rotationSpeed, float returnSpeed, bool stop, bool start, bool thumbstick, bool swap, bool toggle)
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
            swordSpeed = SwordSpeed;
            cooldown = BeamCooldown;
            RotationSpeed = rotationSpeed;
            ReturnSpeed = returnSpeed;
            StopOnEnd = stop;
            StopOnStart = start;
            ThumbstickDash = thumbstick;
            SwapButtons = swap;
            ToggleSwordBeams = toggle;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if ((!SwapButtons && action == Interactable.Action.AlternateUseStart) || (SwapButtons && action == Interactable.Action.UseStart))
            {
                StopCoroutine(Dash());
                StartCoroutine(Dash());
            }
            if (!ToggleSwordBeams)
            {
                if ((!SwapButtons && action == Interactable.Action.UseStart) || (SwapButtons && action == Interactable.Action.AlternateUseStart))
                {
                    beam = true;
                }
                else if ((!SwapButtons && action == Interactable.Action.UseStop) || (SwapButtons && action == Interactable.Action.AlternateUseStop))
                {
                    beam = false;
                }
            }
            else
            {
                if ((!SwapButtons && action == Interactable.Action.UseStart) || (SwapButtons && action == Interactable.Action.AlternateUseStart))
                {
                    beam = !beam;
                }
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

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (isThrown && collisionInstance.damageStruct.damager != null && collisionInstance.damageStruct.damageType != DamageType.Blunt)
            {
                collisionInstance.damageStruct.damager.UnPenetrateAll();
            }
        }

        private void Item_OnUnSnapEvent(Holder holder)
        {
            lastHolder = holder;
        }

        public void FixedUpdate()
        {
            if (item.isTelekinesisGrabbed) lastHandler = null;
            if (item.isFlying && lastHandler != null)
            {
                item.flyDirRef.Rotate(new Vector3(0, RotationSpeed, 0) * Time.fixedDeltaTime);
                item.rb.useGravity = false;
                item.rb.AddForce(-(item.transform.position - lastHandler.transform.position).normalized * ReturnSpeed, ForceMode.Force);
                item.IgnoreRagdollCollision(Player.local.creature.ragdoll);
                isThrown = true;
                startUpdate = true;
            }
            else if (isThrown && !item.IsHanded() && item.holder == null && !item.isTelekinesisGrabbed && lastHandler != null)
            {
                item.Throw(1, Item.FlyDetection.Forced);
            }
            else
            {
                item.flyDirRef.localRotation = Quaternion.identity;
                item.rb.useGravity = true;
                isThrown = false;
                startUpdate = false;
            }
            if (lastHandler != null && Vector3.Dot(item.rb.velocity.normalized, (item.transform.position - lastHandler.transform.position).normalized) < 0 &&
                Vector3.Distance(item.GetMainHandle(lastHandler.side).transform.position, lastHandler.transform.position) <= 1 && !item.IsHanded() && isThrown && !item.isTelekinesisGrabbed &&
                startUpdate)
            {
                if (lastHandler.grabbedHandle == null)
                {
                    lastHandler.Grab(item.GetMainHandle(lastHandler.side), true);
                }
                else if (lastHandler.grabbedHandle != null && lastHolder != null && lastHolder.HasSlotFree())
                {
                    Common.MoveAlign(item.transform, item.holderPoint, lastHolder.slots[0]);
                    lastHolder.Snap(item);
                }
                else if (lastHandler.grabbedHandle != null && (lastHolder == null || !lastHolder.HasSlotFree()) && Player.local.creature.equipment.GetFirstFreeHolder() != null)
                {
                    Holder holder = Player.local.creature.equipment.GetFirstFreeHolder();
                    Common.MoveAlign(item.transform, item.holderPoint, holder.slots[0]);
                    holder.Snap(item);
                }
                else if (lastHandler.grabbedHandle != null && (lastHolder == null || !lastHolder.HasSlotFree()) && Player.local.creature.equipment.GetFirstFreeHolder() == null)
                {
                    BackpackHolder.instance.StoreItem(item);
                }
                item.rb.useGravity = true;
                isThrown = false;
                startUpdate = false;
            }
            if (Time.time - cdH <= cooldown || !beam || item.rb.velocity.magnitude - Player.local.locomotion.rb.velocity.magnitude < swordSpeed)
            {
                return;
            }
            else
            {
                cdH = Time.time;
                Catalog.GetData<ItemData>("YamatoBeam").SpawnAsync(beam =>
                {
                    beam.GetComponent<BeamCustomization>().yamato = item;
                    if (item.colliderGroups[0].imbue is Imbue imbue && imbue.spellCastBase != null && imbue.energy > 0)
                        beam.colliderGroups[0].imbue.Transfer(imbue.spellCastBase, beam.colliderGroups[0].imbue.maxEnergy);
                }, item.flyDirRef.position, Quaternion.LookRotation(item.flyDirRef.forward, item.rb.velocity));
            }
        }
    }
}
