using System;
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
        public List<RagdollPart> parts = new List<RagdollPart>();
        bool active = false;
        bool sheath = false;
        bool sheathed = true;
        bool beam = false;
        bool judgementCutEnd = false;
        float cdH;
        Damager pierce;
        Damager slash;
        GameObject blades;
        GameObject triggers;
        GameObject jce;
        Rigidbody otherHand;
        AudioSource judgementCutEndSfx;
        AudioSource judgementCutSfx;
        SpellTelekinesis telekinesis;
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
            if (item.itemId == "YamatoSheath") sheath = true;
            if (!sheath)
            {
                pierce = item.GetCustomReference("Pierce")?.gameObject?.GetComponent<Damager>();
                slash = item.GetCustomReference("Slash")?.gameObject?.GetComponent<Damager>();
                blades = item.GetCustomReference("Blades")?.gameObject;
                triggers = item.GetCustomReference("Triggers")?.gameObject;
                judgementCutEndSfx = item.GetCustomReference("JudgementCutEndSfx")?.gameObject?.GetComponent<AudioSource>();
                judgementCutSfx = item.GetCustomReference("JudgementCutSfx")?.gameObject?.GetComponent<AudioSource>();
                jce = item.GetCustomReference("JudgementCutEnd")?.gameObject;
                triggers?.SetActive(false);
                jce.SetActive(false);
            }
        }
        private void Item_OnUnSnapEvent(Holder holder)
        {
            if (!sheath)
            {
                sheathed = false;
            }
            if (!sheath && ((PlayerControl.GetHand(PlayerControl.handLeft.side).castPressed && PlayerControl.GetHand(PlayerControl.handLeft.side).alternateUsePressed) || (PlayerControl.GetHand(PlayerControl.handRight.side).castPressed && PlayerControl.GetHand(PlayerControl.handRight.side).alternateUsePressed)))
            {
                judgementCutEndSfx.Play();
                StartCoroutine(JCE());
                judgementCutEnd = true;
            }
            beam = false;
        }
        public IEnumerator JCE()
        {
            jce.SetActive(true);
            yield return new WaitForSeconds(0.6f);
            jce.SetActive(false);
        }
        private void Item_OnSnapEvent(Holder holder)
        {
            if (!sheath)
            {
                if (judgementCutEnd)
                {
                    if(Creature.list != null)
                    {
                        foreach(Creature creature in Creature.list)
                        {
                            if (creature.gameObject.activeSelf && creature != Player.currentCreature)
                            {
                                foreach(RagdollPart part in creature.ragdoll.parts)
                                {
                                    if(!parts.Contains(part) && part != null)
                                    {
                                        parts.Add(part);
                                    }
                                }
                            }
                        }
                    }
                    judgementCutEnd = false;
                }
                sheathed = true;
                StartCoroutine(JudgementCut());
                if (parts != null) StartCoroutine(AnimeSlice());
                Deactivate();
            }
            beam = false;
        }
        private void Item_OnTelekinesisReleaseEvent(Handle handle, SpellTelekinesis teleGrabber)
        {
            telekinesis = null;
            if (!sheath) Deactivate();
            beam = false;
        }
        private void Item_OnTelekinesisGrabEvent(Handle handle, SpellTelekinesis teleGrabber)
        {
            telekinesis = teleGrabber;
            if (!sheath) Deactivate();
            beam = false;
        }
        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            if (!sheath) Deactivate();
            beam = false;
        }
        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            if (!sheath) Deactivate();
            beam = false;
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (!sheath)
            {
                if (action == Interactable.Action.AlternateUseStart)
                {
                    Activate();
                }
                else if (action == Interactable.Action.AlternateUseStop)
                {
                    Deactivate();
                }
                if (action == Interactable.Action.UseStart)
                {
                    beam = true;
                }
                else if (action == Interactable.Action.UseStop)
                {
                    beam = false;
                }
            }
            if (sheath)
            {
                otherHand = ragdollHand.otherHand.rb;
                if (action == Interactable.Action.AlternateUseStart)
                {
                    StopCoroutine(Dash());
                    StartCoroutine(Dash());
                }
                if (action == Interactable.Action.UseStart)
                {
                    Catalog.GetData<ItemData>("MirageBlade").SpawnAsync(ShootDagger);
                }
            }
        }
        public IEnumerator Dash()
        {
            Player.local.locomotion.rb.AddForce(Player.local.head.transform.forward * 1000f, ForceMode.Impulse);
            Player.local.locomotion.rb.detectCollisions = false;
            Player.local.locomotion.rb.useGravity = false;
            item.rb.detectCollisions = false;
            otherHand.detectCollisions = false;
            yield return new WaitForSeconds(0.5f);
            Player.local.locomotion.rb.detectCollisions = true;
            Player.local.locomotion.rb.useGravity = true;
            item.rb.detectCollisions = true;
            otherHand.detectCollisions = true;
            yield break;
        }
        public void ShootBeam(Item spawnedItem)
        {
            spawnedItem.transform.position = item.flyDirRef.position;
            spawnedItem.transform.rotation = item.flyDirRef.rotation;
            spawnedItem.rb.useGravity = false;
            spawnedItem.rb.drag = 0;
            spawnedItem.rb.AddForce(Player.local.head.transform.forward * 50f, ForceMode.Impulse);
            spawnedItem.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            spawnedItem.IgnoreObjectCollision(item);
            spawnedItem.RefreshCollision(true);
            spawnedItem.gameObject.AddComponent<YamatoBeam>();
            spawnedItem.Despawn(1.5f);
        }
        public void ShootDagger(Item spawnedItem)
        {
            Vector3 v;
            v.x = UnityEngine.Random.Range(-0.15f, 0.15f);
            v.y = UnityEngine.Random.Range(-0.15f, 0.15f);
            v.z = UnityEngine.Random.Range(-0.15f, 0.15f);
            spawnedItem.transform.rotation = new Quaternion(Player.local.head.transform.rotation.x, Player.local.head.transform.rotation.y, Player.local.head.transform.rotation.z, Player.local.head.transform.rotation.w);
            spawnedItem.transform.position = new Vector3(Player.local.head.transform.position.x + v.x, Player.local.head.transform.position.y + v.y, Player.local.head.transform.position.z + v.z);
            spawnedItem.rb.useGravity = false;
            spawnedItem.rb.drag = 0;
            spawnedItem.rb.AddForce(Player.local.head.transform.forward * 45f, ForceMode.Impulse);
            spawnedItem.RefreshCollision(true);
            spawnedItem.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            spawnedItem.IgnoreObjectCollision(item);
            spawnedItem.gameObject.AddComponent<DaggerDespawn>();
        }

        public void FixedUpdate()
        {
            if (telekinesis != null && telekinesis.spinMode && !active && !sheath)
            {
                Activate();
            }
            else if (telekinesis != null && !telekinesis.spinMode && active && !sheath)
            {
                Deactivate();
            }
            if (Time.time - cdH <= 0.15 || !beam || item.rb.velocity.magnitude - Player.local.locomotion.rb.velocity.magnitude < 7f)
            {
                return;
            }
            else
            {
                cdH = Time.time;
                Catalog.GetData<ItemData>("YamatoBeam").SpawnAsync(ShootBeam);
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
        public IEnumerator AnimeSlice()
        {
            yield return new WaitForEndOfFrame();
            foreach (RagdollPart part in parts)
            {
                
                if (part?.ragdoll?.creature?.gameObject?.activeSelf == true && part != null && !part.isSliced && part?.ragdoll?.creature != Player.currentCreature)
                {
                    if (part.ripBreak && part.gameObject.activeSelf)
                    {
                        part.EnableCharJointBreakForce(0);
                        part.ragdoll.creature.Kill();
                    }
                    else if (!part.ripBreak && !part.ragdoll.creature.isKilled && part.gameObject.activeSelf)
                    {
                        part.ragdoll.creature.Damage(new CollisionInstance(new DamageStruct(DamageType.Slash, 20f)));
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
            parts.Clear();
            yield break;
        }
        public IEnumerator JudgementCut()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (sheathed == true) yield break;
            Catalog.GetData<ItemData>("JudgementCut").SpawnAsync(ShootJudgementCut);
            yield break;
        }
        public void ShootJudgementCut(Item spawnedItem)
        {
            spawnedItem.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
            spawnedItem.IgnoreObjectCollision(item);
            spawnedItem.transform.rotation = new Quaternion(Player.local.head.transform.rotation.x, Player.local.head.transform.rotation.y, Player.local.head.transform.rotation.z, Player.local.head.transform.rotation.w);
            spawnedItem.transform.position = new Vector3(Player.local.head.transform.position.x, Player.local.head.transform.position.y, Player.local.head.transform.position.z);
            spawnedItem.rb.useGravity = false;
            spawnedItem.rb.drag = 0;
            spawnedItem.rb.AddForce(Player.local.head.transform.forward * 30f, ForceMode.Impulse);
            spawnedItem.RefreshCollision(true);
            spawnedItem.gameObject.AddComponent<JudgementCutHit>();
            judgementCutSfx.Play();
        }
        public void OnTriggerEnter(Collider c)
        {
            if (item.holder == null && c.GetComponentInParent<ColliderGroup>() != null)
            {
                ColliderGroup enemy = c.GetComponentInParent<ColliderGroup>();
                if (enemy?.collisionHandler?.ragdollPart != null && enemy?.collisionHandler?.ragdollPart?.ragdoll?.creature != Player.currentCreature)
                {
                    RagdollPart part = enemy.collisionHandler.ragdollPart;
                    if (part.ragdoll.creature != Player.currentCreature && parts.Contains(part) == false)
                    {
                        parts.Add(part);
                    }
                }
            }
        }
        public void OnCollisionEnter(Collision c)
        {
            if (c.collider.gameObject.GetComponentInParent<YamatoComponent>() != null)
            {
                item.IgnoreObjectCollision(c.collider.gameObject.GetComponentInParent<Item>());
            }
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
            if (c.collider.gameObject.GetComponent<YamatoComponent>() != null) item.IgnoreObjectCollision(c.collider.gameObject.GetComponentInParent<Item>());
            else
            {
                StartCoroutine(BeginDespawn());
                item.rb.useGravity = true;
            }
        }
        public IEnumerator BeginDespawn()
        {
            yield return new WaitForSeconds(0.3f);
            item.Despawn();
        }
    }
    public class YamatoBeam : MonoBehaviour
    {
        Item item;
        public void Start()
        {
            item = GetComponent<Item>();
        }
        public void FixedUpdate()
        {
            item.gameObject.transform.localScale += new Vector3(0, 0.2f, 0);
        }
        public void OnTriggerEnter(Collider c)
        {
            if (c.GetComponentInParent<ColliderGroup>() != null)
            {
                ColliderGroup enemy = c.GetComponentInParent<ColliderGroup>();
                if (enemy?.collisionHandler?.ragdollPart != null && enemy?.collisionHandler?.ragdollPart?.ragdoll?.creature != Player.currentCreature)
                {
                    RagdollPart part = enemy.collisionHandler.ragdollPart;
                    if (part.ragdoll.creature != Player.currentCreature && part?.ragdoll?.creature?.gameObject?.activeSelf == true && part != null && !part.isSliced)
                    {
                        if (part.ripBreak && part.gameObject.activeSelf)
                        {
                            part.EnableCharJointBreakForce(0);
                            part.ragdoll.creature.Kill();
                        }
                        else if (!part.ripBreak && !part.ragdoll.creature.isKilled && part.gameObject.activeSelf)
                        {
                            part.ragdoll.creature.Damage(new CollisionInstance(new DamageStruct(DamageType.Slash, 5f)));
                        }
                    }
                }
            }
        }
    }
    public class JudgementCutHit : MonoBehaviour
    {
        Item item;
        GameObject triggers;
        GameObject colliders;
        List<RagdollPart> parts = new List<RagdollPart>();
        public void Start()
        {
            item = GetComponent<Item>();
            triggers = item.GetCustomReference("Triggers").gameObject;
            colliders = item.GetCustomReference("Colliders").gameObject;
            triggers.SetActive(false);
        }
        public void OnCollisionEnter(Collision c)
        {
            if (c.collider.gameObject.GetComponent<YamatoComponent>() != null) item.IgnoreObjectCollision(c.collider.gameObject.GetComponentInParent<Item>());
            else if (c.collider.gameObject.GetComponentInParent<Player>() == null)
            {
                triggers.SetActive(true);
                colliders.SetActive(false);
                item.rb.isKinematic = true;
                StartCoroutine(DisableTrigger());
            }
        }
        public void OnTriggerEnter(Collider c)
        {
            if (c.GetComponentInParent<ColliderGroup>() != null)
            {
                ColliderGroup enemy = c.GetComponentInParent<ColliderGroup>();
                if (enemy?.collisionHandler?.ragdollPart != null && enemy?.collisionHandler?.ragdollPart?.ragdoll?.creature != Player.currentCreature)
                {
                    RagdollPart part = enemy.collisionHandler.ragdollPart;
                    if (part.ragdoll.creature != Player.currentCreature && parts.Contains(part) == false)
                    {
                        parts.Add(part);
                    }
                }
            }
        }
        public IEnumerator DisableTrigger()
        {
            yield return new WaitForSeconds(0.07f);
            triggers.SetActive(false);
            yield return new WaitForEndOfFrame();
            StartCoroutine(AnimeSlice());
            yield break;
        }
        public IEnumerator AnimeSlice()
        {
            if (parts != null)
            {
                foreach (RagdollPart part in parts)
                {
                    if (part?.ragdoll?.creature?.gameObject?.activeSelf == true && part != null && !part.isSliced && part?.ragdoll?.creature != Player.currentCreature)
                    {
                        if (part.ripBreak && part.gameObject.activeSelf)
                        {
                            part.EnableCharJointBreakForce(0);
                            part.ragdoll.creature.Kill();
                            yield return new WaitForEndOfFrame();
                        }
                        else if (!part.ripBreak && !part.ragdoll.creature.isKilled && part.gameObject.activeSelf)
                        {
                            part.ragdoll.creature.Damage(new CollisionInstance(new DamageStruct(DamageType.Slash, 20f)));
                        }
                    }
                }
            }
            parts.Clear();
            item.Despawn();
            yield break;
        }
    }
}
