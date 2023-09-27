using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    public class MirageModule : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<MirageComponent>();
        }
    }
    public class MirageComponent : MonoBehaviour
    {
        Item item;
        bool isThrown;
        Holder lastHolder;
        RagdollHand lastHandler;
        bool startUpdate;
        float cdH;
        bool beam;
        public void Start()
        {
            item = GetComponent<Item>();
            item.OnUnSnapEvent += Item_OnUnSnapEvent;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;
            item.OnTelekinesisReleaseEvent += Item_OnTelekinesisReleaseEvent;
            EffectInstance instance = Catalog.GetData<EffectData>("MirageFire").Spawn(item.transform, null, false);
            instance.SetRenderer(item.colliderGroups[0].imbueEffectRenderer, false);
            instance.SetIntensity(1);
            instance.Play();
            item.data.category = "Utilities";
        }
        public Creature GetEnemy()
        {
            Creature closestCreature = null;
            if (Creature.allActive.Count <= 0) return null;
            foreach (Creature creature in Creature.allActive)
            {
                if (creature != null && !creature.isPlayer && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && Vector3.Angle(item.physicBody.velocity.normalized, (creature.ragdoll.targetPart.transform.position - item.transform.position).normalized) <= 20 && closestCreature == null &&
                    Vector3.Distance(item.transform.position, creature.ragdoll.targetPart.transform.position) <= 25)
                {
                    closestCreature = creature;
                }
                else if (creature != null && !creature.isPlayer && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && Vector3.Angle(item.physicBody.velocity.normalized, (creature.ragdoll.targetPart.transform.position - item.transform.position).normalized) <= 20 && closestCreature != null &&
                    Vector3.Distance(item.transform.position, creature.ragdoll.targetPart.transform.position) <= 25)
                {
                    if (Vector3.Distance(item.transform.position, creature.ragdoll.targetPart.transform.position) < Vector3.Distance(item.transform.position, closestCreature.ragdoll.targetPart.transform.position)) closestCreature = creature;
                }
            }
            return closestCreature;
        }

        private void Item_OnTelekinesisReleaseEvent(Handle handle, SpellTelekinesis teleGrabber)
        {
            lastHandler = null;
        }

        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            lastHandler = ragdollHand;
            beam = false;
            if (throwing)
            {
                float magnitude = item.physicBody.velocity.magnitude;
                Creature enemy = GetEnemy();
                if(enemy != null)
                {
                    item.physicBody.velocity = -(item.transform.position - enemy.ragdoll.targetPart.transform.position).normalized * magnitude;
                }
            }
        }

        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            item.physicBody.useGravity = true;
            isThrown = false;
            startUpdate = false;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if ((!YamatoManager.MirageSwapButtons && action == Interactable.Action.AlternateUseStart) || (YamatoManager.MirageSwapButtons && action == Interactable.Action.UseStart))
            {
                StopCoroutine(Dash());
                StartCoroutine(Dash());
            }
            if (!YamatoManager.MirageToggleSwordBeams)
            {
                if ((!YamatoManager.MirageSwapButtons && action == Interactable.Action.UseStart) || (YamatoManager.MirageSwapButtons && action == Interactable.Action.AlternateUseStart))
                {
                    beam = true;
                }
                else if ((!YamatoManager.MirageSwapButtons && action == Interactable.Action.UseStop) || (YamatoManager.MirageSwapButtons && action == Interactable.Action.AlternateUseStop))
                {
                    beam = false;
                }
            }
            else
            {
                if ((!YamatoManager.MirageSwapButtons && action == Interactable.Action.UseStart) || (YamatoManager.MirageSwapButtons && action == Interactable.Action.AlternateUseStart))
                {
                    beam = !beam;
                }
            }
        }
        public IEnumerator Dash()
        {
            if (YamatoManager.MirageStopOnStart) Player.local.locomotion.rb.velocity = Vector3.zero;
            if (Player.local.locomotion.moveDirection.magnitude <= 0 || !YamatoManager.MirageThumbstickDash)
                if (YamatoManager.MirageDashDirection == "Item")
                {
                    Player.local.locomotion.rb.AddForce(item.mainHandler.grip.up * (!YamatoManager.MirageDashRealTime ? YamatoManager.MirageDashSpeed : YamatoManager.MirageDashSpeed / Time.timeScale), YamatoManager.MirageDashForceMode);
                }
                else
                {
                    Player.local.locomotion.rb.AddForce(Player.local.head.transform.forward * (!YamatoManager.MirageDashRealTime ? YamatoManager.MirageDashSpeed : YamatoManager.MirageDashSpeed / Time.timeScale), YamatoManager.MirageDashForceMode);
                }
            else
            {
                Player.local.locomotion.rb.AddForce(Player.local.locomotion.moveDirection.normalized * (!YamatoManager.MirageDashRealTime ? YamatoManager.MirageDashSpeed : YamatoManager.MirageDashSpeed / Time.timeScale), YamatoManager.MirageDashForceMode);
            }
            if (YamatoManager.MirageDisableGravity)
                Player.local.locomotion.rb.useGravity = false;
            if (YamatoManager.MirageDisableBodyCollision)
            {
                Player.local.locomotion.rb.detectCollisions = false;
            }
            if (YamatoManager.MirageDisableWeaponCollision)
            {
                item.physicBody.rigidBody.detectCollisions = false;
                item.mainHandler.physicBody.rigidBody.detectCollisions = false;
                item.mainHandler.otherHand.physicBody.rigidBody.detectCollisions = false;
            }
            if (YamatoManager.MirageDashRealTime) yield return new WaitForSecondsRealtime(YamatoManager.MirageDashTime);
            else yield return new WaitForSeconds(YamatoManager.MirageDashTime);
            if (YamatoManager.MirageDisableGravity)
                Player.local.locomotion.rb.useGravity = true;
            if (YamatoManager.MirageDisableBodyCollision)
            {
                Player.local.locomotion.rb.detectCollisions = true;
            }
            if (YamatoManager.MirageDisableWeaponCollision)
            {
                item.physicBody.rigidBody.detectCollisions = true;
                item.mainHandler.physicBody.rigidBody.detectCollisions = true;
                item.mainHandler.otherHand.physicBody.rigidBody.detectCollisions = true;
            }
            if (YamatoManager.MirageStopOnEnd) Player.local.locomotion.rb.velocity = Vector3.zero;
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
                item.flyDirRef.Rotate(new Vector3(0, YamatoManager.MirageRotateDegreesPerSecond, 0) * Time.fixedDeltaTime);
                item.physicBody.useGravity = false;
                item.physicBody.mass = 10000;
                item.physicBody.AddForce(-(item.transform.position - lastHandler.transform.position).normalized * YamatoManager.MirageReturnSpeed * item.physicBody.mass, YamatoManager.MirageReturnForceMode);
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
                item.physicBody.mass = 1;
                item.flyDirRef.localRotation = Quaternion.identity;
                item.physicBody.useGravity = true;
                isThrown = false;
                startUpdate = false;
            }
            if (lastHandler != null && Vector3.Dot(item.physicBody.velocity.normalized, (item.transform.position - lastHandler.transform.position).normalized) < 0 &&
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
                item.physicBody.useGravity = true;
                isThrown = false;
                startUpdate = false;
            }
            if (Time.time - cdH <= YamatoManager.MirageBeamCooldown || !beam || item.physicBody.velocity.magnitude - Player.local.locomotion.rb.velocity.magnitude < YamatoManager.MirageSwordSpeed)
            {
                return;
            }
            else
            {
                cdH = Time.time;
                Catalog.GetData<ItemData>("MirageBeam").SpawnAsync(beam =>
                {
                    MirageBeam beamCustomization = beam.gameObject.AddComponent<MirageBeam>();
                    beamCustomization.mirage = item;
                    beamCustomization.user = item.mainHandler != null ? item.mainHandler?.creature : item.lastHandler?.creature;
                    if (beamCustomization.user?.player != null) beam.physicBody.AddForce(Player.local.head.transform.forward * YamatoManager.MirageBeamSpeed, ForceMode.Impulse);
                    else if (beamCustomization.user?.brain?.currentTarget is Creature target) beam.physicBody.AddForce(-(beam.transform.position - target.ragdoll.targetPart.transform.position).normalized * YamatoManager.MirageBeamSpeed, ForceMode.Impulse);
                    else beam.physicBody.AddForce(beamCustomization.user.ragdoll.headPart.transform.forward * YamatoManager.MirageBeamSpeed, ForceMode.Impulse);
                    beam.physicBody.angularVelocity = Vector3.zero;
                    if (item.colliderGroups[0].imbue is Imbue imbue && imbue.spellCastBase != null && imbue.energy > 0)
                        beam.colliderGroups[0].imbue.Transfer(imbue.spellCastBase, beam.colliderGroups[0].imbue.maxEnergy);
                }, item.flyDirRef.position, Quaternion.LookRotation(item.flyDirRef.forward, item.physicBody.GetPointVelocity(item.flyDirRef.position).normalized), null, false);
            }
        }
    }
}
