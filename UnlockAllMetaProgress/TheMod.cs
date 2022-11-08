using System;
using System.Reflection;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using UnlockAllMetaProgress;

[assembly: MelonInfo(typeof(TheMod), "UnlockAllMetaProgress", "0.0.0.1", "Example's cool mods")]

namespace UnlockAllMetaProgress
{
    public class TheMod : MelonMod
    {
        static TheMod the_mod;

        void log(string message)
        {
            LoggerInstance.Msg(message);
        }

        public override void OnApplicationStart()
        {
            log("loaded");
            the_mod = this;
        }

        [HarmonyPatch(typeof(ItemAtlas), "CreateButton")]
        public static class UnlockAtlasItems
        {
            [HarmonyPrefix]
            static void prefix(GameObject item)
            {
                UnityEngine.Object.FindObjectOfType<MetaProgressSaveManager>().FindNewItem(item);
            }
        }

        [HarmonyPatch(typeof(RunTypeButton), "Start")]
        private static class RunButtonListener
        {
            static MetaProgressSaveManager.RunCompleted make_run(Character.CharacterName character, string run_type)
            {
                MetaProgressSaveManager.RunCompleted r = new MetaProgressSaveManager.RunCompleted();
                r.ironMan = true;
                r.runType = run_type;
                r.character = character;
                return r;
            }

            [HarmonyPostfix]
            static void postfix(RunType ___runType)
            {
                string mode = ___runType.name;
                bool updated_metaprogress = false;
                MetaProgressSaveManager metaprogress = UnityEngine.Object.FindObjectOfType<MetaProgressSaveManager>();
                foreach (Character.CharacterName character in Enum.GetValues(typeof(Character.CharacterName)))
                {
                    if (character != Character.CharacterName.Any)
                    {
                        RunType run_type = new RunType();
                        run_type.name = mode;
                        if (!metaprogress.RunTypeCompleted(run_type, character, true))
                        {
                            MetaProgressSaveManager.RunCompleted run = make_run(character, mode);
                            metaprogress.runsCompleted.Add(run);
                            updated_metaprogress = true;
                            the_mod.log("Added run metaprogress: " + run.runType + ", " + run.character + ", " + run.ironMan);
                        }
                    }
                }
                if (updated_metaprogress)
                {
                    metaprogress.GetType().GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(metaprogress, new object[] { });
                    the_mod.log("Saved metaprogress successfully");
                }
            }
        }
    }
}