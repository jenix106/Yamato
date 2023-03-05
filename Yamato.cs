using System;
using System.Collections.Generic;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    public class YamatoModule : ItemModule
    {
        public float BeamCooldown = 0.15f;
        public float SwordSpeed = 7;
        public bool SwapButtons = false;
        public bool StopOnJudgementCut = true;
        public bool ToggleAnimeSlice = false;
        public bool ToggleSwordBeams = false;
        public bool SwapJudgementCutActivation = false;
        public bool NoJudgementCut = false;
        public bool PierceDismemberment = true;
        public bool SlashDismemberment = true;
        public bool JudgementCutDismemberment = true;
        public float JudgementCutDamage = 20;
        public bool JudgementCutEndDismemberment = true;
        public float JudgementCutEndDamage = 20;
        public bool AnimeSliceDismemberment = true;
        public float AnimeSliceDamage = 20;
        public bool AnimeSliceOnSpin = true;
        public bool Motivation = true;
        /*public float DTStrengthMult = 2f;
        public float DTSpeedMult = 2f;
        public float DTJumpMult = 2f;*/
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<YamatoComponent>().Setup(SwordSpeed, BeamCooldown, SwapButtons, StopOnJudgementCut, ToggleAnimeSlice, ToggleSwordBeams, 
                SwapJudgementCutActivation, NoJudgementCut, PierceDismemberment, SlashDismemberment, Motivation, JudgementCutDismemberment, JudgementCutDamage, 
                JudgementCutEndDismemberment, JudgementCutEndDamage, AnimeSliceDismemberment, AnimeSliceDamage, AnimeSliceOnSpin/*, DTStrengthMult, DTSpeedMult, DTJumpMult*/);
        }
    }
    public class YamatoComponent : MonoBehaviour
    {
        Item item;
        public List<RagdollPart> parts = new List<RagdollPart>();
        List<Creature> creatures = new List<Creature>();
        bool active = false;
        bool sheathed = true;
        bool beam = false;
        bool judgementCutEnd = false;
        float cdH;
        Damager pierce;
        Damager slash;
        GameObject blades;
        GameObject triggers;
        SpellTelekinesis telekinesis;
        public float swordSpeed;
        public float cooldown;
        public bool swapButtons;
        bool stopOnJC;
        bool toggleAnimeSlice;
        bool toggleSwordBeams;
        bool swapJCActivation;
        bool noJC;
        bool pierceDismember;
        bool slashDismember;
        float springMult = 1;
        bool motivation;
        bool judgementCutDismemberment;
        float judgementCutDamage;
        bool judgementCutEndDismemberment;
        float judgementCutEndDamage;
        bool animeSliceDismemberment;
        float animeSliceDamage;
        bool animeSliceOnSpin;
        /*Coroutine coroutine = null;
        float DTStrengthMult;
        float DTSpeedMult;
        float DTJumpMult;
        bool devil = false;*/

        public void Start()
        {
            item = GetComponent<Item>();
            item.OnSnapEvent += Item_OnSnapEvent;
            item.OnUnSnapEvent += Item_OnUnSnapEvent;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnTelekinesisReleaseEvent += Item_OnTelekinesisReleaseEvent;
            item.OnTelekinesisGrabEvent += Item_OnTelekinesisGrabEvent;
            item.OnTKSpinStart += Item_OnTKSpinStart;
            item.OnTKSpinEnd += Item_OnTKSpinEnd;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            pierce = item.GetCustomReference("Pierce")?.gameObject?.GetComponent<Damager>();
            slash = item.GetCustomReference("Slash")?.gameObject?.GetComponent<Damager>();
            blades = item.GetCustomReference("Blades")?.gameObject;
            triggers = item.GetCustomReference("Triggers")?.gameObject;
            if (!pierceDismember) pierce.data.dismembermentAllowed = pierceDismember;
            if (!slashDismember) slash.data.dismembermentAllowed = slashDismember;
        }

        private void Item_OnTKSpinEnd(Handle held, bool spinning, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart && active && animeSliceOnSpin)
            {
                Deactivate();
            }
        }

        private void Item_OnTKSpinStart(Handle held, bool spinning, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart && !active && animeSliceOnSpin)
            {
                Activate();
            }
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if(collisionInstance.IsDoneByPlayer() && collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart != null && collisionInstance.damageStruct.damage >= 1)
            {
                springMult += 0.2f;
            }
            /*if((collisionInstance.IsDoneByPlayer() || item.lastHandler?.creature?.player != null) && collisionInstance.targetCollider.GetComponentInParent<RagdollPart>() is RagdollPart part 
                && part?.ragdoll?.creature?.player != null && part.type == RagdollPart.Type.Torso && collisionInstance.damageStruct.damageType == DamageType.Pierce)
            {
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = StartCoroutine(DevilTrigger(collisionInstance));
            }*/
        }
        /*public IEnumerator DevilTrigger(CollisionInstance collisionInstance)
        {
            left.SetJointModifier(this, DTStrengthMult, 1, DTStrengthMult, 1, 1);
            right.SetJointModifier(this, DTStrengthMult, 1, DTStrengthMult, 1, 1);
            Player.local.locomotion.SetSpeedModifier(this, DTSpeedMult, DTSpeedMult, DTSpeedMult, DTSpeedMult, DTJumpMult);
            springMult = DTStrengthMult;
            item.mainHandleRight.SetJointModifier(this, springMult, 1, springMult, 1);
            Player.local.creature.currentHealth += collisionInstance.damageStruct.damage;
            devil = true;
            yield return new WaitForSeconds(60);
            right.RemoveJointModifier(this);
            left.RemoveJointModifier(this);
            Player.local.locomotion.RemoveSpeedModifier(this);
            springMult = 1f;
            item.mainHandleRight.SetJointModifier(this, springMult, 1, springMult, 1);
            devil = false;
            yield break;
        }*/

        public void Setup(float speed, float cd, bool swap, bool stop, bool toggle, bool toggleBeam, bool swapJC, bool noJudgementCut, 
            bool pierce, bool slash, bool motivate, bool jcDismember, float jcDamage, bool jceDismember, float jceDamage, 
            bool animeDismember, float animeDamage, bool spin/*, float dtStrength, float dtSpeed, float dtJump*/)
        {
            swordSpeed = speed;
            cooldown = cd;
            swapButtons = swap;
            stopOnJC = stop;
            toggleAnimeSlice = toggle;
            toggleSwordBeams = toggleBeam;
            swapJCActivation = swapJC;
            noJC = noJudgementCut;
            pierceDismember = pierce;
            slashDismember = slash;
            motivation = motivate;
            judgementCutDismemberment = jcDismember;
            judgementCutDamage = jcDamage;
            judgementCutEndDismemberment = jceDismember;
            judgementCutEndDamage = jceDamage;
            animeSliceDismemberment = animeDismember;
            animeSliceDamage = animeDamage;
            animeSliceOnSpin = spin;
            /*DTStrengthMult = dtStrength;
            DTSpeedMult = dtSpeed;
            DTJumpMult = dtJump;*/
        }
        private void Item_OnUnSnapEvent(Holder holder)
        {
            sheathed = false;
            if ((PlayerControl.GetHand(PlayerControl.handLeft.side).castPressed && PlayerControl.GetHand(PlayerControl.handLeft.side).alternateUsePressed) || (PlayerControl.GetHand(PlayerControl.handRight.side).castPressed && PlayerControl.GetHand(PlayerControl.handRight.side).alternateUsePressed))
            {
                StartCoroutine(JCE());
            }
            if (swapJCActivation && !noJC)
                StartCoroutine(JudgementCut(holder.GetComponentInParent<Item>()));
            beam = false;
        }
        public IEnumerator JCE()
        {
            EffectInstance instance = Catalog.GetData<EffectData>("JudgementCutEnd").Spawn(Player.local.transform, false);
            instance.SetIntensity(1);
            instance.Play();
            Catalog.GetData<ItemData>("JCESlashes").SpawnAsync(spawnedItem =>
            {
                spawnedItem.rb.isKinematic = true;
                spawnedItem.Despawn(3);
            }, item.transform.position, Quaternion.identity);
            judgementCutEnd = true;
            foreach (Creature creature in Creature.allActive)
            {
                if (!creature.isKilled && creature != Player.local.creature && creature.loaded)
                {
                    if (Level.current.dungeon == null || (Level.current.dungeon != null && creature.currentRoom == Player.local.creature.currentRoom))
                    {
                        creatures.Add(creature);
                        creature.TryPush(Creature.PushType.Hit, Player.local.creature.transform.position - creature.transform.position, 1);
                        creature.animator.speed = 0f;
                        creature.locomotion.allowMove = false;
                        creature.locomotion.allowTurn = false;
                        creature.locomotion.allowJump = false;
                    }
                }
            }
            yield break;
        }
        private void Item_OnSnapEvent(Holder holder)
        {
            if (judgementCutEnd)
            {
                StartCoroutine(AnimeSlice());
                judgementCutEnd = false;
            }
            sheathed = true;
            if (!swapJCActivation && !noJC)
                StartCoroutine(JudgementCut(holder.GetComponentInParent<Item>()));
            if (parts != null)
            {
                StartCoroutine(AnimeSlice());
            }
            Deactivate();
            beam = false;
        }
        private void Item_OnTelekinesisReleaseEvent(Handle handle, SpellTelekinesis teleGrabber)
        {
            telekinesis = null;
            Deactivate();
            beam = false;
        }
        private void Item_OnTelekinesisGrabEvent(Handle handle, SpellTelekinesis teleGrabber)
        {
            telekinesis = teleGrabber;
            Deactivate();
            beam = false;
        }
        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            Deactivate();
            beam = false;
        }
        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            Deactivate();
            beam = false;
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (!toggleAnimeSlice)
            {
                if ((!swapButtons && action == Interactable.Action.AlternateUseStart) || (swapButtons && action == Interactable.Action.UseStart))
                {
                    Activate();
                }
                else if ((!swapButtons && action == Interactable.Action.AlternateUseStop) || (swapButtons && action == Interactable.Action.UseStop))
                {
                    Deactivate();
                }
            }
            else
            {
                if ((!swapButtons && action == Interactable.Action.AlternateUseStart) || (swapButtons && action == Interactable.Action.UseStart))
                {
                    if (!active) Activate();
                    else Deactivate();
                }
            }
            if (!toggleSwordBeams)
            {
                if ((!swapButtons && action == Interactable.Action.UseStart) || (swapButtons && action == Interactable.Action.AlternateUseStart))
                {
                    beam = true;
                }
                else if ((!swapButtons && action == Interactable.Action.UseStop) || (swapButtons && action == Interactable.Action.AlternateUseStop))
                {
                    beam = false;
                }
            }
            else
            {
                if ((!swapButtons && action == Interactable.Action.UseStart) || (swapButtons && action == Interactable.Action.AlternateUseStart))
                {
                    beam = !beam;
                }
            }
        }
        public void FixedUpdate()
        {
            if (Time.time - cdH <= cooldown || !beam || item.rb.GetPointVelocity(item.flyDirRef.position).magnitude - item.rb.GetPointVelocity(item.holderPoint.position).magnitude < swordSpeed)
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
                }, slash.transform.position, Quaternion.LookRotation(item.flyDirRef.forward, item.rb.GetPointVelocity(item.flyDirRef.position).normalized));
            }
            if (springMult > 1/* && !devil*/)
            {
                springMult = Mathf.Clamp(springMult - (0.05f * Time.fixedDeltaTime), 1, 5);
                if (motivation)
                {
                    item.mainHandleRight.SetJointModifier(this, springMult, 1, springMult, 1);
                }
            }
            /*if (devil) Player.local.creature.Heal(Player.local.creature.maxHealth / 50 * Time.fixedDeltaTime, Player.local.creature);*/
        }
        public void Deactivate()
        {
            blades.SetActive(true);
            triggers.SetActive(false);
            active = false;
        }
        public void Activate()
        {
            blades.SetActive(false);
            triggers.SetActive(true);
            pierce.UnPenetrateAll();
            slash.UnPenetrateAll();
            active = true;
        }
        public IEnumerator JCEAnimeSlice(Creature creature)
        {
            foreach (RagdollPart part in creature.ragdoll.parts)
            {
                part.gameObject.SetActive(true);
                CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, judgementCutEndDamage))
                {
                    targetCollider = part.colliderGroup.colliders[0],
                    targetColliderGroup = part.colliderGroup,
                    sourceCollider = item.colliderGroups[0].colliders[0],
                    sourceColliderGroup = item.colliderGroups[0],
                    casterHand = item.lastHandler.caster,
                    impactVelocity = item.rb.velocity,
                    contactPoint = part.transform.position,
                    contactNormal = -item.rb.velocity
                };
                instance.damageStruct.hitRagdollPart = part;
                if (item.colliderGroups[0].imbue.energy > 0 && item.colliderGroups[0].imbue is Imbue imbue)
                {
                    imbue.spellCastBase.OnImbueCollisionStart(instance);
                    yield return null;
                }
                if (part.sliceAllowed && judgementCutEndDismemberment)
                {
                    part.ragdoll.TrySlice(part);
                    if (part.data.sliceForceKill)
                        part.ragdoll.creature.Kill();
                    yield return null;
                }
                part.ragdoll.creature.Damage(instance);
            }
            yield break;
        }
        public IEnumerator AnimeSlice()
        {
            if (judgementCutEnd)
            {
                foreach (Creature creature in creatures)
                {
                    if (creature != Player.local.creature)
                    {
                        creature.animator.speed = 1;
                        creature.locomotion.allowMove = true;
                        creature.locomotion.allowTurn = true;
                        creature.locomotion.allowJump = true;
                        if (creature.loaded)
                        {
                            StartCoroutine(JCEAnimeSlice(creature));
                        }
                    }
                }
                creatures.Clear();
            }
            foreach (RagdollPart part in parts)
            {
                if (part?.ragdoll?.creature?.gameObject?.activeSelf == true && part != null && !part.isSliced && part?.ragdoll?.creature != Player.currentCreature)
                {
                    part.gameObject.SetActive(true);
                    CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, animeSliceDamage))
                    {
                        targetCollider = part.colliderGroup.colliders[0],
                        targetColliderGroup = part.colliderGroup,
                        sourceCollider = item.colliderGroups[0].colliders[0],
                        sourceColliderGroup = item.colliderGroups[0],
                        casterHand = item.lastHandler.caster,
                        impactVelocity = item.rb.velocity,
                        contactPoint = part.transform.position,
                        contactNormal = -item.rb.velocity
                    };
                    instance.damageStruct.hitRagdollPart = part;
                    if(item.colliderGroups[0].imbue.energy > 0 && item.colliderGroups[0].imbue is Imbue imbue)
                    {
                        imbue.spellCastBase.OnImbueCollisionStart(instance);
                        yield return null;
                    }
                    if (part.sliceAllowed && animeSliceDismemberment)
                    {
                        part.ragdoll.TrySlice(part);
                        if (part.data.sliceForceKill)
                            part.ragdoll.creature.Kill();
                        yield return null;
                    }
                    part.ragdoll.creature.Damage(instance);
                }
            }
            parts.Clear();
            yield break;
        }
        public IEnumerator JudgementCut(Item otherItem)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if ((sheathed == true && !swapJCActivation) || (sheathed == false && swapJCActivation) || (otherItem.GetComponent<YamatoSheathFrameworkComponent>() != null && item.mainHandler == null)
                || (otherItem.GetComponent<YamatoSheathFrameworkComponent>() != null && item.mainHandler != null && !item.mainHandler.playerHand.controlHand.usePressed)) yield break;
            GameObject creature = new GameObject();
            JudgementCutPosition position = creature.AddComponent<JudgementCutPosition>();
            if (GetEnemy()?.ragdoll?.targetPart != null)
                position.position = GetEnemy().ragdoll.targetPart.transform.position;
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(Player.local.head.transform.position, Player.local.head.cam.transform.forward, out hit, Mathf.Infinity, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    position.position = hit.point;
                }
            }
            position.yamato = item;
            position.damage = judgementCutDamage;
            position.dismember = judgementCutDismemberment;
            if (stopOnJC && !Player.local.locomotion.isGrounded)
            {
                Player.local.locomotion.rb.velocity = Vector3.zero;
                Player.local.locomotion.rb.AddForce(Vector3.up * 200, ForceMode.Impulse);
            }
            EffectInstance instance = Catalog.GetData<EffectData>("JudgementCutStart").Spawn(item.transform, false);
            instance.SetIntensity(1);
            instance.Play();
            yield break;
        }
        public Creature GetEnemy()
        {
            Creature closestCreature = null;
            if (Creature.allActive.Count <= 0) return null;
            foreach (Creature creature in Creature.allActive)
            {
                if (creature != null && !creature.isPlayer && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && Vector3.Dot(Player.local.head.cam.transform.forward.normalized, (creature.transform.position - Player.local.transform.position).normalized) >= 0.75f && closestCreature == null &&
                    Vector3.Distance(Player.local.transform.position, creature.transform.position) <= 25 && ((Level.current.dungeon != null && creature.currentRoom == Player.local.creature.currentRoom) || Level.current.dungeon == null))
                {
                    closestCreature = creature;
                }
                else if (creature != null && !creature.isPlayer && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && Vector3.Dot(Player.local.head.cam.transform.forward.normalized, (creature.transform.position - Player.local.transform.position).normalized) >= 0.75f && closestCreature != null &&
                    Vector3.Distance(Player.local.transform.position, creature.transform.position) <= 25 && ((Level.current.dungeon != null && creature.currentRoom == Player.local.creature.currentRoom) || Level.current.dungeon == null))
                {
                    if (Vector3.Dot(Player.local.head.cam.transform.forward.normalized, (creature.transform.position - Player.local.transform.position).normalized) >
                    Vector3.Dot(Player.local.head.cam.transform.forward.normalized, (closestCreature.transform.position - Player.local.transform.position).normalized))
                        closestCreature = creature;
                }
            }
            return closestCreature;
        }
        public void OnTriggerEnter(Collider c)
        {
            if (item.holder == null && c.GetComponentInParent<ColliderGroup>() is ColliderGroup group && group.collisionHandler.isRagdollPart)
            {
                group.collisionHandler.ragdollPart.gameObject.SetActive(true);
                if (!parts.Contains(group.collisionHandler.ragdollPart))
                {
                    parts.Add(group.collisionHandler.ragdollPart);
                }
            }
        }
        public void OnCollisionEnter(Collision c)
        {
            if (c.collider.gameObject.GetComponentInParent<SheathComponent>() != null)
            {
                item.IgnoreObjectCollision(c.collider.gameObject.GetComponentInParent<Item>());
            }
        }
    }
}
