using System;
using System.Collections.Generic;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    public class YamatoModule : ItemModule
    {
        public float BeamCooldown;
        public float SwordSpeed;
        public bool SwapButtons;
        public bool StopOnJudgementCut;
        public bool ToggleAnimeSlice;
        public bool ToggleSwordBeams;
        public bool SwapJudgementCutActivation;
        public bool NoJudgementCut;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<YamatoComponent>().Setup(SwordSpeed, BeamCooldown, SwapButtons, StopOnJudgementCut, ToggleAnimeSlice, ToggleSwordBeams, SwapJudgementCutActivation, NoJudgementCut);
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

        public void Awake()
        {
            item = GetComponent<Item>();
            item.OnSnapEvent += Item_OnSnapEvent;
            item.OnUnSnapEvent += Item_OnUnSnapEvent;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnTelekinesisReleaseEvent += Item_OnTelekinesisReleaseEvent;
            item.OnTelekinesisGrabEvent += Item_OnTelekinesisGrabEvent;
            pierce = item.GetCustomReference("Pierce")?.gameObject?.GetComponent<Damager>();
            slash = item.GetCustomReference("Slash")?.gameObject?.GetComponent<Damager>();
            blades = item.GetCustomReference("Blades")?.gameObject;
            triggers = item.GetCustomReference("Triggers")?.gameObject;
            triggers?.SetActive(false);
        }
        public void Setup(float speed, float cd, bool swap, bool stop, bool toggle, bool toggleBeam, bool swapJC, bool noJudgementCut)
        {
            swordSpeed = speed;
            cooldown = cd;
            swapButtons = swap;
            stopOnJC = stop;
            toggleAnimeSlice = toggle;
            toggleSwordBeams = toggleBeam;
            swapJCActivation = swapJC;
            noJC = noJudgementCut;
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
            if (telekinesis != null && telekinesis.spinMode && !active)
            {
                Activate();
            }
            else if (telekinesis != null && !telekinesis.spinMode && active)
            {
                Deactivate();
            }
            if (Time.time - cdH <= cooldown || !beam || item.rb.velocity.magnitude - Player.local.locomotion.rb.velocity.magnitude < swordSpeed)
            {
                return;
            }
            else
            {
                cdH = Time.time;
                Catalog.GetData<ItemData>("YamatoBeam").SpawnAsync(null, item.flyDirRef.position, Quaternion.LookRotation(item.flyDirRef.forward, item.rb.velocity));
            }
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
                    else if (!part.sliceAllowed && !part.ragdoll.creature.isKilled)
                    {
                        CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, 20f));
                        instance.damageStruct.hitRagdollPart = part;
                        part.ragdoll.creature.Damage(instance);
                    }
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
            if(GetEnemy()?.ragdoll?.targetPart != null)
            creature.AddComponent<JudgementCutPosition>().position = GetEnemy().ragdoll.targetPart.transform.position;
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(Player.local.head.transform.position, Player.local.head.cam.transform.forward, out hit, Mathf.Infinity, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    creature.AddComponent<JudgementCutPosition>().position = hit.point;
                }
            }
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
            if (item.holder == null && c.GetComponentInParent<ColliderGroup>() != null)
            {
                ColliderGroup enemy = c.GetComponentInParent<ColliderGroup>();
                if (enemy?.collisionHandler?.ragdollPart != null && enemy?.collisionHandler?.ragdollPart?.ragdoll?.creature != Player.currentCreature)
                {
                    RagdollPart part = enemy.collisionHandler.ragdollPart;
                    part.gameObject.SetActive(true);
                    Creature creature = part.ragdoll.creature;
                    if (creature != Player.currentCreature && parts.Contains(part) == false)
                    {
                        parts.Add(part);
                    }
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
