using DiceFree.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DefaultExecutionOrder(-30)]
    [DisallowMultipleComponent]
    public sealed class EscapeMenuController : MonoBehaviour
    {
        private InventoryPanel inventory;
        private NoviceSkillPanel novice;
        private PhysicalSkillPanel physical;
        private MagicalSkillPanel magical;
        private OptionsPanel options;
        private CodexPanel codex;
        private SkillTargetingController targeting;
        private Interactor interactor;
        private QuestJournalPanel journal;

        private void Awake()
        {
            inventory = GetComponent<InventoryPanel>();
            novice = GetComponent<NoviceSkillPanel>();
            physical = GetComponent<PhysicalSkillPanel>();
            magical = GetComponent<MagicalSkillPanel>();
            options = FindAnyObjectByType<OptionsPanel>();
            codex = GetComponent<CodexPanel>();
            targeting = GetComponent<SkillTargetingController>();
            interactor = GetComponent<Interactor>();
            journal=GetComponent<QuestJournalPanel>();
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

            // Targeting owns Escape first so one press does one obvious thing.
            if (targeting != null && (targeting.Active || targeting.InputConsumedThisFrame)) return;

            if(journal!=null&&journal.Open){journal.Close();}

            if (inventory != null && inventory.IsOpen)
            {
                inventory.Close();
            }

            if (novice != null && novice.Open)
            {
                novice.Close();
            }

            if (physical != null && physical.Open)
            {
                physical.Close();
            }

            if (magical != null && magical.Open)
            {
                magical.Close();
            }

            if (options != null && options.Open)
            {
                options.Close();
            }

            if (codex != null && codex.Open)
            {
                codex.Close();
            }

            if (interactor != null && interactor.Active != null)
            {
                interactor.Cancel();
            }

            var hud = HudLayoutManager.Current;
            if (hud != null && hud.EditMode)
            {
                hud.SetEditMode(false);
            }

            // Deliberately no combat/movement cancellation here.
            // Escape is UI/cancel semantics, not "stop" or "clear target".
        }
    }
}
