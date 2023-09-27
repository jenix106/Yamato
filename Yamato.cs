using System.Collections.Generic;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    public class YamatoModule : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<YamatoComponent>();
        }
    }
    public class YamatoComponent : MonoBehaviour
    {
        Item item;
        public Dictionary<RagdollPart, SpellCastCharge> parts = new Dictionary<RagdollPart, SpellCastCharge>();
        public Dictionary<Creature, SpellCastCharge> creatures = new Dictionary<Creature, SpellCastCharge>();
        Dictionary<Breakable, float> breakables = new Dictionary<Breakable, float>();
        public bool active = false;
        bool sheathed = true;
        bool beam = false;
        bool judgementCutEnd = false;
        float cdH;
        Damager pierce;
        Damager slash;
        GameObject blades;
        GameObject triggers;
        float springMult = 1;
        bool cleanThreshold = false;
        bool cleanStarted = false;
        bool reduced = false;
        float cleanTime;
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
        }
        public void Update()
        {
            pierce.data.dismembermentAllowed = YamatoManager.PierceDismemberment;
            slash.data.dismembermentAllowed = YamatoManager.SlashDismemberment;
            if(YamatoManager.ReducedSharpness && !reduced)
            {
                reduced = true;
                slash.Load(Catalog.GetData<DamagerData>("SwordSlash1H"), item.mainCollisionHandler);
                pierce.Load(Catalog.GetData<DamagerData>("SwordPierce"), item.mainCollisionHandler);
            }
            else if (!YamatoManager.ReducedSharpness && reduced)
            {
                reduced = false;
                slash.Load(Catalog.GetData<DamagerData>("YamatoSlash"), item.mainCollisionHandler);
                pierce.Load(Catalog.GetData<DamagerData>("YamatoPierce"), item.mainCollisionHandler);
            }
        }
        private void Item_OnTKSpinEnd(Handle held, bool spinning, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart && active && YamatoManager.AnimeSliceOnSpin)
            {
                Deactivate();
            }
        }

        private void Item_OnTKSpinStart(Handle held, bool spinning, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart && !active && YamatoManager.AnimeSliceOnSpin)
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
        }
        private void Item_OnUnSnapEvent(Holder holder)
        {
            sheathed = false;
            if ((PlayerControl.GetHand(PlayerControl.handLeft.side).castPressed && PlayerControl.GetHand(PlayerControl.handLeft.side).alternateUsePressed) || (PlayerControl.GetHand(PlayerControl.handRight.side).castPressed && PlayerControl.GetHand(PlayerControl.handRight.side).alternateUsePressed))
            {
                StartCoroutine(JCE());
            }
            else if (YamatoManager.JudgementCutTriggerUnsheathe && !YamatoManager.NoJudgementCut && (PlayerControl.GetHand(PlayerControl.handLeft.side).castPressed || PlayerControl.GetHand(PlayerControl.handRight.side).castPressed))
            {
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
                position.damage = YamatoManager.JudgementCutDamage;
                position.dismember = YamatoManager.JudgementCutDismemberment;
                if (YamatoManager.StopOnJudgementCut && !Player.local.locomotion.isGrounded)
                {
                    Player.local.locomotion.rb.velocity = Vector3.zero;
                    Player.local.locomotion.rb.AddForce(Vector3.up * 200, ForceMode.Impulse);
                }
                EffectInstance instance = Catalog.GetData<EffectData>("JudgementCutStart").Spawn(item.transform, null, false);
                instance.SetIntensity(1);
                instance.Play();
            }
            else if (YamatoManager.SwapJudgementCutActivation && !YamatoManager.NoJudgementCut)
                StartCoroutine(JudgementCut(holder.GetComponentInParent<Item>()));
            beam = false;
        }
        public IEnumerator JCE()
        {
            EffectInstance instance = Catalog.GetData<EffectData>("JudgementCutEnd").Spawn(Player.local.transform, null, false);
            instance.SetIntensity(1);
            instance.Play();
            CameraEffects.SetSepia(0.5f);
            Catalog.GetData<ItemData>("JCESlashes").SpawnAsync(spawnedItem =>
            {
                spawnedItem.physicBody.isKinematic = true;
                spawnedItem.Despawn(3);
            }, item.transform.position, Quaternion.identity);
            judgementCutEnd = true;
            foreach (Creature creature in Creature.allActive)
            {
                if (!creature.isKilled && creature != Player.local.creature && creature.loaded)
                {
                    if (Level.current.mode.HasModule<LevelAreaModule>() == false || (Level.current.mode.HasModule<LevelAreaModule>() == true && creature.currentArea == Player.local.creature.currentArea))
                    {
                        creatures.Add(creature, item.colliderGroups[0].imbue.spellCastBase);
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
            if (!YamatoManager.SwapJudgementCutActivation && !YamatoManager.NoJudgementCut)
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
            Deactivate();
            beam = false;
        }
        private void Item_OnTelekinesisGrabEvent(Handle handle, SpellTelekinesis teleGrabber)
        {
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
            if (!YamatoManager.ToggleAnimeSlice)
            {
                if ((!YamatoManager.SwapSwordButtons && action == Interactable.Action.AlternateUseStart) || (YamatoManager.SwapSwordButtons && action == Interactable.Action.UseStart))
                {
                    Activate();
                }
                else if ((!YamatoManager.SwapSwordButtons && action == Interactable.Action.AlternateUseStop) || (YamatoManager.SwapSwordButtons && action == Interactable.Action.UseStop))
                {
                    Deactivate();
                }
            }
            else
            {
                if ((!YamatoManager.SwapSwordButtons && action == Interactable.Action.AlternateUseStart) || (YamatoManager.SwapSwordButtons && action == Interactable.Action.UseStart))
                {
                    if (!active) Activate();
                    else Deactivate();
                }
            }
            if (!YamatoManager.ToggleSwordBeams)
            {
                if ((!YamatoManager.SwapSwordButtons && action == Interactable.Action.UseStart) || (YamatoManager.SwapSwordButtons && action == Interactable.Action.AlternateUseStart))
                {
                    beam = true;
                }
                else if ((!YamatoManager.SwapSwordButtons && action == Interactable.Action.UseStop) || (YamatoManager.SwapSwordButtons && action == Interactable.Action.AlternateUseStop))
                {
                    beam = false;
                }
            }
            else
            {
                if ((!YamatoManager.SwapSwordButtons && action == Interactable.Action.UseStart) || (YamatoManager.SwapSwordButtons && action == Interactable.Action.AlternateUseStart))
                {
                    beam = !beam;
                }
            }
        }
        public void FixedUpdate()
        {
            if (springMult > 1)
            {
                springMult = Mathf.Clamp(springMult - (0.05f * Time.fixedDeltaTime), 1, 5);
                if (YamatoManager.Motivation)
                {
                    item.mainHandleRight.SetJointModifier(this, springMult, 1, springMult, 1);
                }
            }
            if(item.physicBody.velocity.sqrMagnitude >= YamatoManager.BloodCleanVelocity && item.holder == null)
            {
                cleanThreshold = true;
                cleanStarted = false;
            }
            else if(cleanThreshold && item.physicBody.velocity.sqrMagnitude < YamatoManager.BloodCleanVelocity && item.holder == null)
            {
                cleanTime = Time.time;
                cleanStarted = true;
                cleanThreshold = false;
            }
            else if(Time.time - cleanTime <= 0.02f && cleanStarted && item.physicBody.velocity.sqrMagnitude <= 10 && item.holder == null)
            {
                cleanThreshold = false;
                cleanStarted = false;
                foreach (RevealDecal decal in item.revealDecals)
                {
                    decal.Reset();
                }
            }
            if (Time.time - cdH <= YamatoManager.BeamCooldown || !beam || item.physicBody.GetPointVelocity(item.flyDirRef.position).magnitude - item.physicBody.GetPointVelocity(item.holderPoint.position).magnitude < YamatoManager.SwordSpeed)
            {
                return;
            }
            else
            {
                cdH = Time.time;
                Catalog.GetData<ItemData>("YamatoBeam").SpawnAsync(beam =>
                {
                    YamatoBeam beamCustomization = beam.gameObject.AddComponent<YamatoBeam>();
                    beamCustomization.yamato = item;
                    beamCustomization.user = item.mainHandler != null ? item.mainHandler?.creature : item.lastHandler?.creature;
                    if (beamCustomization.user?.player != null) beam.physicBody.AddForce(Player.local.head.transform.forward * YamatoManager.BeamSpeed, ForceMode.Impulse);
                    else if (beamCustomization.user?.brain?.currentTarget is Creature target) beam.physicBody.AddForce(-(beam.transform.position - target.ragdoll.targetPart.transform.position).normalized * YamatoManager.BeamSpeed, ForceMode.Impulse);
                    else beam.physicBody.AddForce(beamCustomization.user.ragdoll.headPart.transform.forward * YamatoManager.BeamSpeed, ForceMode.Impulse);
                    beam.physicBody.angularVelocity = Vector3.zero;
                    if (item.colliderGroups[0].imbue is Imbue imbue && imbue.spellCastBase != null && imbue.energy > 0)
                        beam.colliderGroups[0].imbue.Transfer(imbue.spellCastBase, beam.colliderGroups[0].imbue.maxEnergy);
                }, slash.transform.position, Quaternion.LookRotation(item.flyDirRef.forward, item.physicBody.GetPointVelocity(item.flyDirRef.position).normalized), null, false);
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
        public IEnumerator JCEAnimeSlice(Creature creature, SpellCastCharge spell)
        {
            foreach (RagdollPart part in creature.ragdoll.parts)
            {
                part.gameObject.SetActive(true);
                CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, YamatoManager.JudgementCutEndDamage))
                {
                    targetCollider = part.colliderGroup.colliders[0],
                    targetColliderGroup = part.colliderGroup,
                    sourceCollider = item.colliderGroups[0].colliders[0],
                    sourceColliderGroup = item.colliderGroups[0],
                    casterHand = item.lastHandler.caster,
                    impactVelocity = item.physicBody.velocity,
                    contactPoint = part.transform.position,
                    contactNormal = -item.physicBody.velocity
                };
                instance.damageStruct.hitRagdollPart = part;
                if (spell != null)
                {
                    spell.imbue = item.colliderGroups[0].imbue;
                    spell.OnImbueCollisionStart(instance);
                    yield return null;
                }
                if (part.sliceAllowed && YamatoManager.JudgementCutEndDismemberment)
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
                foreach (Creature creature in creatures.Keys)
                {
                    if (creature != Player.local.creature)
                    {
                        creature.animator.speed = 1;
                        creature.locomotion.allowMove = true;
                        creature.locomotion.allowTurn = true;
                        creature.locomotion.allowJump = true;
                        if (creature.loaded)
                        {
                            StartCoroutine(JCEAnimeSlice(creature, creatures[creature]));
                        }
                    }
                }
                EffectInstance shatter = Catalog.GetData<EffectData>("GlassShatter").Spawn(Player.local.creature.transform, null, false);
                shatter.SetIntensity(1);
                shatter.Play();
                CameraEffects.SetSepia(0);
                creatures.Clear();
            }
            foreach (Breakable breakable in breakables.Keys)
            {
                --breakable.hitsUntilBreak;
                if (breakable.canInstantaneouslyBreak)
                    breakable.hitsUntilBreak = 0;
                breakable.onTakeDamage?.Invoke(breakables[breakable]);
                if (breakable.IsBroken || breakable.hitsUntilBreak > 0)
                    continue;
                breakable.Break();
            }
            breakables.Clear();
            foreach (RagdollPart part in parts.Keys)
            {
                if (part?.ragdoll?.creature?.gameObject?.activeSelf == true && part != null && !part.isSliced && part?.ragdoll?.creature != Player.currentCreature)
                {
                    part.gameObject.SetActive(true);
                    CollisionInstance instance = new CollisionInstance(new DamageStruct(DamageType.Slash, YamatoManager.AnimeSliceDamage))
                    {
                        targetCollider = part.colliderGroup.colliders[0],
                        targetColliderGroup = part.colliderGroup,
                        sourceCollider = item.colliderGroups[0].colliders[0],
                        sourceColliderGroup = item.colliderGroups[0],
                        casterHand = item.lastHandler.caster,
                        impactVelocity = item.physicBody.velocity,
                        contactPoint = part.transform.position,
                        contactNormal = -item.physicBody.velocity
                    };
                    instance.damageStruct.hitRagdollPart = part;
                    if(parts[part] != null)
                    {
                        parts[part].imbue = item.colliderGroups[0].imbue;
                        parts[part].OnImbueCollisionStart(instance);
                        yield return null;
                    }
                    if (part.sliceAllowed && YamatoManager.AnimeSliceDismemberment)
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
            if ((sheathed == true && !YamatoManager.SwapJudgementCutActivation) || (sheathed == false && YamatoManager.SwapJudgementCutActivation) || (otherItem.GetComponent<YamatoSheathFrameworkComponent>() != null && item.mainHandler == null)
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
            position.damage = YamatoManager.JudgementCutDamage;
            position.dismember = YamatoManager.JudgementCutDismemberment;
            if (YamatoManager.StopOnJudgementCut && !Player.local.locomotion.isGrounded)
            {
                Player.local.locomotion.rb.velocity = Vector3.zero;
                Player.local.locomotion.rb.AddForce(Vector3.up * 200, ForceMode.Impulse);
            }
            EffectInstance instance = Catalog.GetData<EffectData>("JudgementCutStart").Spawn(item.transform, null, false);
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
                    Vector3.Distance(Player.local.transform.position, creature.transform.position) <= 25 && ((Level.current.mode.HasModule<LevelAreaModule>() == false || (Level.current.mode.HasModule<LevelAreaModule>() == true && creature.currentArea == Player.local.creature.currentArea))))
                {
                    closestCreature = creature;
                }
                else if (creature != null && !creature.isPlayer && creature.ragdoll.isActiveAndEnabled && !creature.isKilled && Vector3.Dot(Player.local.head.cam.transform.forward.normalized, (creature.transform.position - Player.local.transform.position).normalized) >= 0.75f && closestCreature != null &&
                    Vector3.Distance(Player.local.transform.position, creature.transform.position) <= 25 && (Level.current.mode.HasModule<LevelAreaModule>() == false || (Level.current.mode.HasModule<LevelAreaModule>() == true && creature.currentArea == Player.local.creature.currentArea)))
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
            if (item.holder == null && c.GetComponentInParent<Breakable>() is Breakable breakable)
            {
                if(!breakables.ContainsKey(breakable) || (breakables.ContainsKey(breakable) && item.physicBody.velocity.sqrMagnitude > breakables[breakable]))
                {
                    breakables.Remove(breakable);
                    breakables.Add(breakable, item.physicBody.velocity.sqrMagnitude);
                }
            }
            if (item.holder == null && c.GetComponentInParent<ColliderGroup>() is ColliderGroup group && group.collisionHandler.isRagdollPart)
            {
                group.collisionHandler.ragdollPart.gameObject.SetActive(true);
                if (!parts.ContainsKey(group.collisionHandler.ragdollPart))
                {
                    parts.Add(group.collisionHandler.ragdollPart, item.colliderGroups[0].imbue.spellCastBase);
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
