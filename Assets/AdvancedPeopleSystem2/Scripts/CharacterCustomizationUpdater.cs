using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AdvancedPeopleSystem
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class CharacterSystemUpdater
    {
#if UNITY_EDITOR
        private static CharacterCustomization _previewSelectionCharacter;
        static CharacterSystemUpdater()
        {
            EditorApplication.delayCall += () => { UpdateCharactersOnScene(); };
            EditorApplication.playModeStateChanged += LogPlayModeState;
            EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
            EditorSceneManager.sceneSaving += EditorSceneManager_sceneSaving;
            EditorSceneManager.sceneClosing += EditorSceneManager_sceneClosing;
            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdate;
        }

        private static void EditorSceneManager_sceneClosing(Scene scene, bool removingScene)
        {
            UpdateCharactersOnScene();
        }

        private static void EditorSceneManager_sceneSaving(Scene scene, string path)
        {
            UpdateCharactersOnScene();
        }


#endif
        [RuntimeInitializeOnLoadMethod]
        private static void updateCharacters()
        {
            UpdateCharactersOnScene();
        }
        public static void UpdateCharactersOnScene(bool revertPrefabs = false, CharacterCustomization reverbObject = null)
        {
            CharacterCustomization[] characterSystems = GameObject.FindObjectsOfType<CharacterCustomization>();
            if (characterSystems == null)
                return;
            foreach (CharacterCustomization cs in characterSystems)
            {
                if (cs == null)
                    continue;
#if UNITY_EDITOR
                if (!Application.isPlaying &&
                    (cs.GetCharacterInstanceStatus() == CharacterInstanceStatus.Connected
                    || cs.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabEditingInProjectView
                    || cs.GetCharacterInstanceStatus() == CharacterInstanceStatus.PrefabStageSceneOpened)
                    && (revertPrefabs || (reverbObject != null && cs.prefabPath == reverbObject.prefabPath)))
                {
                    PrefabUtility.RevertPrefabInstance(cs.gameObject, InteractionMode.UserAction);
                    PrefabUtility.RevertObjectOverride(cs.gameObject, InteractionMode.UserAction);
                }
#endif
                cs.InitColors();
            }
        }
#if UNITY_EDITOR

        private static void OnPrefabInstanceUpdate(GameObject instance)
        {
            CharacterCustomization characterSysten = instance.GetComponent<CharacterCustomization>();
            if (characterSysten != null)
                characterSysten.InitColors();
        }
        private static void EditorSceneManager_sceneOpened(Scene scene, OpenSceneMode mode)
        {
            UpdateCharactersOnScene();
        }
        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                UpdateCharactersOnScene();
                InternalEditorUtility.RepaintAllViews();
            }
        }
#endif
    }
}