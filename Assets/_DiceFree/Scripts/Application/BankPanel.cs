using System;
using System.Linq;
using System.Text.RegularExpressions;
using DiceFree.Items;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Banking
{
    [RequireComponent(typeof(EchoSharedBank))]
    public sealed class BankPanel : HudWidget, DiceFree.Foundation.IRemoteBankPanel
    {
        private bool open, regex, descending;
        private string query = "", category = "", slot = "", rarity = "", classId = "", gold = "", message = "";
        private Vector2 bankScroll, carryScroll;
        public void Show() => open = true;
        public override Rect Bounds => open ? new Rect(Mathf.Max(8, Screen.width / 2 - 380), 80, 760, Mathf.Min(590, Screen.height - 100)) : default;
        public override bool BlocksPointer => open;
        public static bool Matches(ItemDefinition d, string text, bool useRegex, out string error)
        {
            error = null;
            string haystack = $"{d.displayName} {d.stableId} {d.slot} {d.rarityId} {string.Join(" ", d.allowedClassIds ?? Array.Empty<string>())} {d.flavor}";
            if (useRegex)
            {
                try { return Regex.IsMatch(haystack, text, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(30)); }
                catch (ArgumentException) { error = "Invalid regular expression."; return false; }
                catch (RegexMatchTimeoutException) { error = "Regular expression took too long."; return false; }
            }
            foreach (string raw in text.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string token = raw;
                int colon = token.IndexOf(':');
                if (colon > 0)
                {
                    string key = token.Substring(0, colon), value = token.Substring(colon + 1);
                    string field = key == "slot" ? d.slot.ToString() : key == "rarity" ? d.rarityId : key == "class" ? string.Join(" ", d.allowedClassIds ?? Array.Empty<string>()) : key == "type" ? Category(d) : haystack;
                    if (field.IndexOf(value, StringComparison.OrdinalIgnoreCase) < 0) return false;
                    continue;
                }
                if (token == "weapons" || token == "weapon") { if (d.slot != EquipmentSlot.MainHand) return false; continue; }
                if (token == "armor" || token == "armour") { if (Category(d) != "armor") return false; continue; }
                if (token == "potions") token = "potion";
                if (token == "swords") token = "sword";
                if (haystack.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0) return false;
            }
            return true;
        }
        private static string Category(ItemDefinition d) => (d.stableId + " " + d.displayName).IndexOf("potion", StringComparison.OrdinalIgnoreCase) >= 0 ? "potions" : d.slot == EquipmentSlot.MainHand ? "weapons" : d.slot is EquipmentSlot.Ring or EquipmentSlot.Amulet ? "accessories" : "armor";
        private void OnGUI()
        {
            if (!open || HudPointerBlocker.ModalOpen) return;
            var bank = GetComponent<EchoSharedBank>(); var inv = GetComponent<CarriedInventory>(); var wallet = GetComponent<GoldWallet>();
            var r = Bounds; GUI.Box(r, GUIContent.none);
            GUILayout.BeginArea(new Rect(r.x + 12, r.y + 10, r.width - 24, r.height - 20));
            GUILayout.BeginHorizontal(); GUILayout.Label("Peter Banker — your friendly neighborhood banker-man");
            if (GUILayout.Button("Close", GUILayout.Width(70))) { open = false; GetComponent<Interactor>()?.Cancel(); }
            GUILayout.EndHorizontal();
            GUILayout.Label(bank.TownAccess ? "Town banking • shared across your Echo" : "Remote items: deposit anywhere • withdraw and gold at banker in town");
            GUILayout.BeginHorizontal(); GUILayout.Label("Search", GUILayout.Width(50)); query = GUILayout.TextField(query); regex = GUILayout.Toggle(regex, "Regex", GUILayout.Width(70)); descending = GUILayout.Toggle(descending, "Z–A", GUILayout.Width(55)); GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(); GUILayout.Label("Type:"); category = GUILayout.TextField(category, GUILayout.Width(95)); GUILayout.Label("Slot:"); slot = GUILayout.TextField(slot, GUILayout.Width(95)); GUILayout.Label("Rarity:"); rarity = GUILayout.TextField(rarity, GUILayout.Width(95)); GUILayout.Label("Class:"); classId = GUILayout.TextField(classId); GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(350)); GUILayout.Label($"Bank {bank.Items.Length}/{bank.Capacity} slots • {bank.Gold} gold"); bankScroll = GUILayout.BeginScrollView(bankScroll);
            DrawItems(bank.Items, false, bank, inv); GUILayout.EndScrollView(); GUILayout.EndVertical();
            GUILayout.BeginVertical(); GUILayout.Label($"Carried inventory • {wallet.Gold} gold"); carryScroll = GUILayout.BeginScrollView(carryScroll);
            DrawItems(inv.Items, true, bank, inv); GUILayout.EndScrollView(); GUILayout.EndVertical(); GUILayout.EndHorizontal();
            bool prior = GUI.enabled; GUI.enabled = prior && bank.TownAccess;
            GUILayout.BeginHorizontal(); gold = GUILayout.TextField(gold, GUILayout.Width(100));
            if (GUILayout.Button("Deposit gold")) { if (long.TryParse(gold, out var amount)) bank.DepositGold(amount, out message); else message = "Enter a positive whole number."; }
            if (GUILayout.Button("Withdraw gold")) { if (long.TryParse(gold, out var amount)) bank.WithdrawGold(amount, out message); else message = "Enter a positive whole number."; }
            if (GUILayout.Button("Deposit All gold")) bank.DepositGold(wallet.Gold, out message);
            GUILayout.EndHorizontal();
            if (GUILayout.Button($"Buy {bank.ExpansionSlots} slots: {bank.ExpansionPrice} gold (provisional)")) bank.Expand(out message);
            GUI.enabled = prior; GUILayout.Label(message ?? "Transfer complete."); GUILayout.EndArea();
        }
        private void DrawItems(ItemInstance[] items, bool deposit, EchoSharedBank bank, CarriedInventory inv)
        {
            var sorted = items.OrderBy(i => inv.Resolve(i.definitionId)?.displayName ?? i.definitionId, StringComparer.OrdinalIgnoreCase).ThenBy(i => i.instanceId);
            foreach (var item in descending ? sorted.Reverse() : sorted)
            {
                var d = inv.Resolve(item.definitionId);
                if (d != null)
                {
                    if (!Matches(d, query, regex, out var error)) { if (error != null) message = error; continue; }
                    if (Category(d).IndexOf(category, StringComparison.OrdinalIgnoreCase) < 0 || d.slot.ToString().IndexOf(slot, StringComparison.OrdinalIgnoreCase) < 0 || d.rarityId.IndexOf(rarity, StringComparison.OrdinalIgnoreCase) < 0 || string.Join(" ", d.allowedClassIds ?? Array.Empty<string>()).IndexOf(classId, StringComparison.OrdinalIgnoreCase) < 0) continue;
                }
                GUILayout.BeginHorizontal(); var icon = GUILayoutUtility.GetRect(30, 30, GUILayout.Width(30)); if (d != null) ItemIconGUI.Draw(icon, d);
                GUILayout.Label(d?.displayName ?? item.definitionId);
                bool prior = GUI.enabled; GUI.enabled = prior && d != null && (deposit ? GetComponent<Equipment>()?.IsEquipped(item.instanceId) != true && bank.Items.Length < bank.Capacity : bank.TownAccess);
                if (GUILayout.Button(deposit ? "Deposit" : "Withdraw", GUILayout.Width(78))) { if (deposit) bank.Deposit(item.instanceId, out message); else bank.Withdraw(item.instanceId, out message); }
                GUI.enabled = prior; GUILayout.EndHorizontal();
            }
        }
    }
}
