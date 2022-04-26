using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace PBToggleApplier
{
    public class SetupHelper : EditorWindow
    {
        private GameObject _avatar;
        private string _fileName;
        #if UNITY_EDITOR
        private DefaultAsset _saveDirectory;
        #endif
        private bool isGenerateBackup = true;
        private bool _isValidSettings = false;
        private SerializedProperty listProperty;

        [SerializeField]
        private List<AnimationClip> targetAnimationClipList = new List<AnimationClip>();

        Vector2 scrollPosition = new Vector2(0, 0);

        [MenuItem("GameObject/PBToggleApplier", false, 20)]
        public static void Create()
        {
            var window = GetWindow<SetupHelper>("PBToggleApplier");
        }

        private void OnGUI()
        {

            EditorUtility.ClearProgressBar();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            _avatar = EditorGUILayout.ObjectField("Avatar", _avatar, typeof(GameObject), true) as GameObject;
            --EditorGUI.indentLevel;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("AnimationClip List", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            var so = new SerializedObject(this);
            so.Update();
            GUIContent content = new GUIContent();
            content.text = "Drag AnimationClip Here";

            listProperty = so.FindProperty("targetAnimationClipList");
            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Add")) {
                    listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
                }
                if (GUILayout.Button("Remove")) {
                    if (listProperty.arraySize >= 1) {
                        listProperty.DeleteArrayElementAtIndex(listProperty.arraySize - 1);
                    }
                }
                if (GUILayout.Button("Clear")) {
                    listProperty.ClearArray();
                }
            }
            EditorGUILayout.PropertyField(listProperty, content, true);
            so.ApplyModifiedProperties();
            --EditorGUI.indentLevel;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Save Option", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            isGenerateBackup = EditorGUILayout.ToggleLeft("Generate XXXX_backup.anim(XXXX.anim clone)", isGenerateBackup);
            EditorGUILayout.LabelField("Backup Save Dir");
            --EditorGUI.indentLevel;
            EditorGUILayout.Space();

            _ValidateSettings();
            using (new EditorGUI.DisabledScope(!_isValidSettings))
            {
                if (GUILayout.Button("Run"))
                {
                    _Run();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        private void _Run()
        {
            foreach (var clip in targetAnimationClipList)
            {
                Applier.OverwriteAnimationClip<VRCPhysBone>(clip, isGenerateBackup, _avatar);
            }
        }

        private void _ValidateSettings()
        {
            _isValidSettings = !!_avatar && (targetAnimationClipList.FindAll(c => c != null).Count >= 1);
        }
    }
}