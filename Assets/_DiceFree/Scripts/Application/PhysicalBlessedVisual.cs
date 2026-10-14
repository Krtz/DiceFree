using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Visuals
{
    /// <summary>Optional locally licensed Physical art, with the authored portable form as fallback.</summary>
    [DisallowMultipleComponent]
    public sealed class PhysicalBlessedVisual : MonoBehaviour
    {
        public const string ResourcePath = "PhysicalLicensed/PhysicalHero";
        [SerializeField] private string resourcePath = ResourcePath;
        // Serialized runtime reference is remapped when the HUD clones this already prepared form.
        [SerializeField, HideInInspector] private GameObject model;
        public bool LicensedModelLoaded => model != null;
        public Animator VisualAnimator => model != null ? model.GetComponentInChildren<Animator>(true) : GetComponentInChildren<Animator>(true);

        private void Awake()
        {
            if (model != null) return;
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null) return;
            var oldAnimator = GetComponentInChildren<Animator>(true);
            if (oldAnimator == null) return;
            oldAnimator.Rebind();
            oldAnimator.Play("Base Layer.Idle", 0, 0);
            oldAnimator.Update(0);
            var oldRenderers = GetComponentsInChildren<Renderer>(true);
            var equipment = GetComponentInParent<EquipmentPresentation>();
            var gear = equipment == null ? Array.Empty<GameObject>() :
                equipment.Bindings.SelectMany(b => b.visuals).Where(g => g != null && g.transform.IsChildOf(transform)).ToArray();

            model = Instantiate(prefab, transform);
            model.name = "Physically Blessed starter armour";
            model.transform.SetAsFirstSibling();
            var animator = model.GetComponentInChildren<Animator>(true);
            animator.Rebind();
            animator.Play("Base Layer.Idle", 0, 0);
            animator.Update(0);
            // Transfer existing form-authored sockets in their relaxed pose. Their item bindings stay authoritative.
            var sockets = new HashSet<Transform>();
            foreach (var visual in gear)
            {
                Transform socket = visual.transform;
                HumanBodyBones mapped = HumanBodyBones.LastBone;
                for (var ancestor = socket.parent; ancestor != null && ancestor != transform; ancestor = ancestor.parent)
                {
                    for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                        if (oldAnimator.GetBoneTransform((HumanBodyBones)i) == ancestor) { mapped = (HumanBodyBones)i; break; }
                    if (mapped == HumanBodyBones.LastBone) continue;
                    while (socket.parent != ancestor) socket = socket.parent;
                    break;
                }
                if (mapped == HumanBodyBones.LastBone || !sockets.Add(socket)) continue;
                var target = animator.GetBoneTransform(mapped);
                if (target == null) continue;
                var oldBone = oldAnimator.GetBoneTransform(mapped);
                Vector3 offset = socket.position - oldBone.position;
                Quaternion rotation = socket.rotation;
                socket.SetParent(target, true);
                socket.position = target.position + offset;
                socket.rotation = rotation;
            }
            foreach (var renderer in oldRenderers)
                if (!gear.Any(g => renderer.transform == g.transform || renderer.transform.IsChildOf(g.transform)))
                    renderer.enabled = false;
            oldAnimator.enabled = false;
            // Novice's one-handed dagger previously had no Physical binding. Keep its modest item art.
            if (equipment != null)
            {
                bool changed = false;
                foreach (var binding in equipment.Bindings)
                {
                    if (binding.slot != DiceFree.Items.EquipmentSlot.MainHand || binding.definition?.modelTestingItem == true || binding.visuals.Any(g => g != null && g.transform.IsChildOf(transform))) continue;
                    var source = binding.visuals.FirstOrDefault(g => g != null);
                    var sourceAnimator = source != null ? source.GetComponentInParent<Animator>() : null;
                    if (sourceAnimator == null)
                    {
                        var novice = equipment.GetComponent<ClassPresentationSwitcher>()?.Resolve("class.novice");
                        sourceAnimator = novice != null ? novice.GetComponentInChildren<Animator>(true) : null;
                    }
                    var sourceHand = sourceAnimator != null && sourceAnimator.isHuman ? sourceAnimator.GetBoneTransform(HumanBodyBones.RightHand) : null;
                    var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    if (sourceHand == null || hand == null) continue;
                    var copy = Instantiate(source, hand);
                    copy.name = "Physical " + source.name;
                    copy.transform.position = hand.position + source.transform.position - sourceHand.position;
                    copy.transform.rotation = source.transform.rotation;
                    Vector3 scale = source.transform.lossyScale, parentScale = hand.lossyScale;
                    copy.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
                    binding.visuals = binding.visuals.Concat(new[] { copy }).ToArray();
                    changed = true;
                }
                if (changed) equipment.Configure(equipment.Bindings);
            }
        }
    }
}
