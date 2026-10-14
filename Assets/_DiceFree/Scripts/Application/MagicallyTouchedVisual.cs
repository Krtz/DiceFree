using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Items;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Visuals
{
    /// <summary>Optional licensed cloth caster; the portable authored form remains a fallback.</summary>
    [DisallowMultipleComponent]
    public sealed class MagicallyTouchedVisual : MonoBehaviour
    {
        public const string ResourcePath="MagicalLicensed/MagicalHero";
        [SerializeField] private string resourcePath=ResourcePath;
        [SerializeField,HideInInspector] private GameObject model;
        public bool LicensedModelLoaded=>model!=null;
        public Animator VisualAnimator=>model!=null?model.GetComponentInChildren<Animator>(true):GetComponentInChildren<Animator>(true);
        private void Awake()
        {
            if(model!=null)return;
            var prefab=Resources.Load<GameObject>(resourcePath);if(prefab==null)return;
            var oldAnimator=GetComponentInChildren<Animator>(true);if(oldAnimator==null)return;
            oldAnimator.Rebind();oldAnimator.Play("Base Layer.Idle",0,0);oldAnimator.Update(0);
            var oldRenderers=GetComponentsInChildren<Renderer>(true);
            var equipment=GetComponentInParent<EquipmentPresentation>();
            var gear=equipment==null?Array.Empty<GameObject>():equipment.Bindings.SelectMany(b=>b.visuals)
                .Where(g=>g!=null&&g.transform.IsChildOf(transform)).ToArray();
            model=Instantiate(prefab,transform);model.name="Magically Touched cloth caster";model.transform.SetAsFirstSibling();
            var animator=model.GetComponentInChildren<Animator>(true);
            animator.Rebind();animator.Play("Base Layer.Idle",0,0);animator.Update(0);
            var sockets=new HashSet<Transform>();
            foreach(var visual in gear)
            {
                var socket=visual.transform;var mapped=HumanBodyBones.LastBone;
                for(var ancestor=socket.parent;ancestor!=null&&ancestor!=transform;ancestor=ancestor.parent)
                {
                    for(int i=0;i<(int)HumanBodyBones.LastBone;i++)
                        if(oldAnimator.GetBoneTransform((HumanBodyBones)i)==ancestor){mapped=(HumanBodyBones)i;break;}
                    if(mapped==HumanBodyBones.LastBone)continue;
                    while(socket.parent!=ancestor)socket=socket.parent;break;
                }
                if(mapped==HumanBodyBones.LastBone||!sockets.Add(socket))continue;
                var target=animator.GetBoneTransform(mapped);if(target==null)continue;
                var offset=socket.position-oldAnimator.GetBoneTransform(mapped).position;var rotation=socket.rotation;
                socket.SetParent(target,true);socket.position=target.position+offset;socket.rotation=rotation;
            }
            foreach(var r in oldRenderers)
                if(!gear.Any(g=>r.transform==g.transform||r.transform.IsChildOf(g.transform)))r.enabled=false;
            oldAnimator.enabled=false;
            // Existing one-handed item art must follow this rig too, including the old bronze dagger.
            if(equipment!=null)
            {
                foreach(var binding in equipment.Bindings)
                {
                    if(binding.slot!=EquipmentSlot.MainHand||binding.definition?.modelTestingItem==true||
                       binding.visuals.Any(g=>g!=null&&g.transform.IsChildOf(transform)))continue;
                    var source=binding.visuals.FirstOrDefault(g=>g!=null);
                    var sourceAnimator=source!=null?source.GetComponentInParent<Animator>():null;
                    if(sourceAnimator==null)sourceAnimator=equipment.GetComponent<ClassPresentationSwitcher>()?.Resolve("class.novice")?.GetComponentInChildren<Animator>(true);
                    var sourceHand=sourceAnimator!=null&&sourceAnimator.isHuman?sourceAnimator.GetBoneTransform(HumanBodyBones.RightHand):null;
                    var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);if(sourceHand==null||hand==null)continue;
                    var copy=Instantiate(source,hand);copy.name="Magical "+source.name;
                    copy.transform.position=hand.position+source.transform.position-sourceHand.position;copy.transform.rotation=source.transform.rotation;
                    var scale=source.transform.lossyScale;var parentScale=hand.lossyScale;
                    copy.transform.localScale=new Vector3(scale.x/parentScale.x,scale.y/parentScale.y,scale.z/parentScale.z);
                    binding.visuals=binding.visuals.Concat(new[]{copy}).ToArray();
                }
                var staff=model.transform.Find("Caster Staff");var cape=model.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="PT_Male_Armor_01_A_cape");
                foreach(var binding in equipment.Bindings)
                {
                    var baseline=binding.slot==EquipmentSlot.MainHand?staff:binding.slot==EquipmentSlot.Back?cape:null;
                    if(baseline!=null)binding.baselineVisuals=binding.baselineVisuals.Concat(new[]{baseline.gameObject}).Distinct().ToArray();
                }
                equipment.Configure(equipment.Bindings);
            }
        }
    }
}
