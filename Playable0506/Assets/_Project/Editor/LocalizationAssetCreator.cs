using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace RecruitPlayable.EditorTools {
    public class LocalizationAssetCreator : EditorWindow {
        [MenuItem("RecruitPlayable/Create Localization Asset")]
        public static void CreateAsset() {
            string path = "Assets/_Project/Data/LocalizationData.asset";
            
            // 确保目录存在
            System.IO.Directory.CreateDirectory("Assets/_Project/Data");

            LocalizationData data = AssetDatabase.LoadAssetAtPath<LocalizationData>(path);
            if (data == null) {
                data = ScriptableObject.CreateInstance<LocalizationData>();
                AssetDatabase.CreateAsset(data, path);
            }

            Undo.RecordObject(data, "Initialize Localization Data");
            data.entries.Clear();

            // 填充中文数据
            AddEntry(data, "BTN_TALK", "Talk", "对话");
            AddEntry(data, "BTN_APPRAISE", "Appraise", "鉴定");
            AddEntry(data, "BTN_DISMISS", "Dismiss", "拒绝");
            AddEntry(data, "BTN_RECRUIT", "Recruit", "招募");
            AddEntry(data, "BTN_PLAYNOW", "Play Now", "立即试玩");
            
            AddEntry(data, "STAT_LOOKS", "Looks", "颜值");
            AddEntry(data, "STAT_SKILL", "Skill", "技能");
            AddEntry(data, "STAT_GROWTH", "Growth", "潜力");

            AddEntry(data, "HERO_A_TALK", "By Excalibur — my blade is yours!", "以圣剑之名——我的利刃为你而战！");
            AddEntry(data, "HERO_B_TALK", "I've united kingdoms. Together, we conquer!", "我曾统一诸国。让我们并肩征服世界！");
            AddEntry(data, "HERO_C_TALK", "Heroes are forged like marble — strike by strike.", "英雄如大理石般经受磨砺——一锤一凿，方显本色。");

            // 处理图片
            data.spriteEntries.Clear();
            AddSpriteEntry(data, "SPRITE_ELITE", "ELITE");

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Debug.Log("MCP: Localization asset created and populated at " + path);

            // 自动关联到场景中的 Manager
            var manager = GameObject.FindObjectOfType<LocalizationManager>();
            if (manager != null) {
                Undo.RecordObject(manager, "Link Loc Data");
                manager.locData = data;

                // 自动寻找并关联 UIManager 中的文本槽位
                var ui = GameObject.FindObjectOfType<UIManager>();
                if (ui != null) {
                    Undo.RecordObject(ui, "Link UI Text Slots");
                    // 通过名字寻找对应的文本组件
                    ui.statLabelTL = GameObject.Find("Canvas/UiLayer/StatPanelTL/Label")?.GetComponent<TMPro.TextMeshProUGUI>();
                    ui.statLabelTR = GameObject.Find("Canvas/UiLayer/StatPanelTR/Label")?.GetComponent<TMPro.TextMeshProUGUI>();
                    ui.statLabelBR = GameObject.Find("Canvas/UiLayer/StatPanelBR/Label")?.GetComponent<TMPro.TextMeshProUGUI>();
                }

                EditorUtility.SetDirty(manager);
                if (ui != null) EditorUtility.SetDirty(ui);
                Debug.Log("MCP: Linked asset and UI slots automatically.");
            }
        }

        static void AddEntry(LocalizationData data, string key, string en, string zh) {
            data.entries.Add(new LocEntry { key = key, en = en, zh = zh });
        }

        static void AddSpriteEntry(LocalizationData data, string key, string spriteName) {
            string[] guids = AssetDatabase.FindAssets(spriteName + " t:Sprite");
            if (guids.Length > 0) {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                data.spriteEntries.Add(new SpriteLocEntry { key = key, en = s, zh = s });
            }
        }
    }
}
