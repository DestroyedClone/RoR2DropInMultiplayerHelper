using BepInEx;
using UnityEngine;
using RoR2;
using RoR2.UI;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RoR2DropInMultiplayerHelper
{
    [BepInPlugin("com.DestroyedClone.DropInMultiplayerHelper", "Drop In Multiplayer Helper", "1.0.1")]
    [BepInDependency("pseudopulse.Survariants", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.Chat.AddMessage_ChatMessageBase += Chat_AddMessage_ChatMessageBase;
            On.RoR2.UI.SurvivorIconController.Awake += SurvivorIconController_Awake;
            Run.onClientGameOverGlobal += Run_onClientGameOverGlobal;
            On.RoR2.CharacterSelectBarController.ShouldDisplaySurvivor += CharacterSelectBarController_ShouldDisplaySurvivor;
            On.RoR2.Console.SubmitCmd_CmdSender_string_bool += Console_SubmitCmd_CmdSender_string_bool;

            ModCompat.Init();
        }

        private void Console_SubmitCmd_CmdSender_string_bool(On.RoR2.Console.orig_SubmitCmd_CmdSender_string_bool orig, RoR2.Console self, RoR2.Console.CmdSender sender, string cmd, bool recordSubmit)
        {
            //Console.instance.SubmitCmd(null, "pause", false);
            if (cmd == "pause" && !recordSubmit)
            {
                if (displayInstance)
                {
                    DestroyDisplayInstance();
                    return;
                }
            }
            orig(self, sender, cmd, recordSubmit);
        }

        private bool CharacterSelectBarController_ShouldDisplaySurvivor(On.RoR2.CharacterSelectBarController.orig_ShouldDisplaySurvivor orig, CharacterSelectBarController self, SurvivorDef survivorDef)
        {
            if (Run.instance && ModCompat.loaded_Survariants && ModCompat.Survariants_IsVariant(survivorDef))
            {
                return true;
            }
            return orig(self, survivorDef);
        }

        private void Run_onClientGameOverGlobal(Run run, RunReport runReport)
        {
            DestroyDisplayInstance();
        }

        private void SurvivorIconController_Awake(On.RoR2.UI.SurvivorIconController.orig_Awake orig, SurvivorIconController self)
        {
            orig(self);
            if (!Run.instance) return;
            self.hgButton.onClick.RemoveAllListeners(); //???
            //https://forum.unity.com/threads/cannot-implicitly-convert-type-void-to-unityengine-ui-button-buttonclickedevent.1237870/
            var localUser = self.GetLocalUser();
            self.hgButton.onClick.AddListener(() => OnClick(self.survivorDef, localUser));
        }

        public void OnClick(SurvivorDef survivorDef, LocalUser localUser)
        {
            if (localUser.cachedMaster && localUser.currentNetworkUser.bodyIndexPreference == SurvivorCatalog.GetBodyIndexFromSurvivorIndex(survivorDef.survivorIndex))
            {
                DestroyDisplayInstance();
                return;
            }
            RoR2.Console.instance.SubmitCmd(localUser.currentNetworkUser, "say \"/join_as " + survivorDef.cachedName + "\"", true);
            DestroyDisplayInstance();
        }

        public static GameObject displayInstance = null;

        private void Chat_AddMessage_ChatMessageBase(On.RoR2.Chat.orig_AddMessage_ChatMessageBase orig, ChatMessageBase message)
        {
            orig(message);
            if (message is Chat.UserChatMessage chatMsg)
            {
                if (chatMsg.sender == LocalUserManager.GetFirstLocalUser().currentNetworkUser.gameObject && chatMsg.text.ToUpperInvariant() == "/LIST_SURVIVORS")
                {
                    DisplayCharacters();
                    return;
                }
            }
        }

        public static void DestroyDisplayInstance()
        {
            if (displayInstance)
                UnityEngine.Object.Destroy(displayInstance);
        }

        public static void DisplayCharacters()
        {
            if (displayInstance) return;
            var prefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/CharacterSelectUI.prefab").WaitForCompletion();
            //var chose = prefab.transform.Find("SafeArea/LeftHandPanel (Layer: Main)/SurvivorChoiceGrid, Panel/");
            var copy = UnityEngine.Object.Instantiate(prefab);
            copy.GetComponent<CharacterSelectController>().enabled = false;
            copy.transform.parent = RoR2.UI.AchievementNotificationPanel.GetUserCanvas(LocalUserManager.GetFirstLocalUser()).transform;
            copy.AddComponent<DestroyOnEsc>();
            copy.GetComponent<CursorOpener>().enabled = false; //keep or remove?
            copy.transform.Find("BottomSideFade").gameObject.SetActive(false);
            copy.transform.Find("TopSideFade").gameObject.SetActive(false);
            var safeArea = copy.transform.Find("SafeArea");
            safeArea.Find("ReadyPanel").gameObject.SetActive(false);
            safeArea.Find("RightHandPanel").gameObject.SetActive(false);
            safeArea.Find("ChatboxPanel").gameObject.SetActive(false);
            safeArea.Find("FooterPanel").gameObject.SetActive(false);
            safeArea.Find("ConfigPanel(Clone)")?.gameObject.SetActive(false);
            var leftPanel = safeArea.Find("LeftHandPanel (Layer: Main)");
            leftPanel.Find("BlurPanel").gameObject.SetActive(false);
            leftPanel.Find("BorderImage").gameObject.SetActive(false);
            leftPanel.Find("SurvivorInfoPanel, Active (Layer: Secondary)").gameObject.AddComponent<KeepInactive>();
            //leftPanel.Find("").gameObject.SetActive(false);
            //leftPanel.Find("").gameObject.SetActive(false);
            copy.transform.localPosition = Vector3.zero;
            copy.transform.localScale = Vector3.one;
            displayInstance = copy;
        }

        public class KeepInactive : MonoBehaviour
        {
            public void Update()
            {
                gameObject.SetActive(false);
            }
        }

        public class DestroyOnEsc : MonoBehaviour
        {
            public void Update()
            {
                if (Input.GetKey(KeyCode.Escape))
                    Destroy(gameObject);
            }
        }
    }
}