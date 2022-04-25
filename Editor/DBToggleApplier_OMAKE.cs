/**
  * 対象の GameObject と含まれる DynamicBone を一括して Enable/Disable する AnimationClip を生成します。
  * MenuItem 行のコメントアウトを外してエディタ拡張として利用できます。
  */
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace PBToggleApplier
{
    public class PBToggleApplier_OMAKE: EditorWindow
    {
        private GameObject _avatar;
        private string _fileName;
        #if UNITY_EDITOR
        private DefaultAsset _saveDirectory;
        #endif
        private bool isGameObject = false;
        private bool isDynamicBone = true;

        [SerializeField]
        private List<GameObject> targetGameObjects = new List<GameObject>();

        Vector2 scrollPosition = new Vector2(0, 0);

        // [MenuItem("GameObject/PBToggleApplier_OMAKE", false, 20)]
        public static void Create()
        {
            var window = GetWindow<PBToggleApplier_OMAKE>("PBToggleApplier_OMAKE");
        }

        private void OnGUI()
        {

            EditorUtility.ClearProgressBar();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;

            _avatar = EditorGUILayout.ObjectField("Avatar Root", _avatar, typeof(GameObject), true) as GameObject;

            isGameObject = EditorGUILayout.ToggleLeft("Toggle Target GameObjects", isGameObject);
            isDynamicBone = EditorGUILayout.ToggleLeft("Toggle DynamicBones related to GameObjects", isDynamicBone);

            --EditorGUI.indentLevel;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Target List", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;

            var so = new SerializedObject(this);
            so.Update();

            GUIContent content = new GUIContent();
            content.text = "[Drag GameObject Here]";

            var listProperty = so.FindProperty("targetGameObjects");
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
            EditorGUILayout.LabelField("Save", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            _fileName = EditorGUILayout.TextField("Animation FileName", _fileName);
            _saveDirectory = ( DefaultAsset ) EditorGUILayout.ObjectField("Save Directory", _saveDirectory, typeof( DefaultAsset ), true);

            --EditorGUI.indentLevel;
            EditorGUILayout.Space();

            if (GUILayout.Button("Run"))
            {
                _Run();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        private void _Run()
        {
            if (!_validate()) return;

            List<SkinnedMeshRenderer> skinnedMeshRenderers = Util.ListSkinnedMeshRenderersInChildren(targetGameObjects);

            List<GameObject> boneList = new List<GameObject>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.bones == null || skinnedMeshRenderer.bones.Length == 0) return;
                Transform[] bones = skinnedMeshRenderer.bones;
                boneList.AddRange(bones.Select(e => e.gameObject));
            }
            var distinctedBoneList = boneList.Distinct();

            List<DynamicBone> dynamicBoneList = new List<DynamicBone>();
            foreach (GameObject bone in distinctedBoneList)
            {
                DynamicBone[] dynamicBones = bone.GetComponentsInChildren<DynamicBone>(true);
                dynamicBoneList.AddRange(dynamicBones);
            }
            var distinctedDynamicBoneList = dynamicBoneList.Distinct();

            _exportAnimationClip(distinctedDynamicBoneList, targetGameObjects);
        }

        private bool _validate()
        {
            if (_avatar == null)
            {
                EditorUtility.DisplayDialog("PBToggleApplier_OMAKE", "Select \"Avatar Root\"", "ok");
                return false;
            }

            if (!Directory.Exists(AssetDatabase.GetAssetPath(_saveDirectory)))
            {
                EditorUtility.DisplayDialog("PBToggleApplier_OMAKE", "Select \"Save Directory\"", "ok");
                return false;
            }

            if (_fileName.Length == 0)
            {
                EditorUtility.DisplayDialog("PBToggleApplier_OMAKE", "Input \"Animation FileName\"", "ok");
                return false;
            }
            return true;
        }

        private void _exportAnimationClip(IEnumerable<DynamicBone> dynamicBoneList, IEnumerable<GameObject> boneObjectList)
        {

            var editor = new AnimationClipEditor(_avatar, AssetDatabase.GetAssetPath(_saveDirectory));
            if (isDynamicBone)
            {
                var frameTimes = new List<float>{0};
                foreach (DynamicBone db in dynamicBoneList)
                {
                    editor.SetCurveDisableDB(db, frameTimes);
                }
            }
            if (isGameObject)
            {
                foreach (var gameObject in boneObjectList)
                {
                    editor.SetCurveDisable(gameObject);
                }
            }
            editor.ExportClip(_fileName + "_disable");

            editor = new AnimationClipEditor(_avatar, AssetDatabase.GetAssetPath(_saveDirectory));
            if (isDynamicBone)
            {
                var frameTimes = new List<float>{0};
                foreach (DynamicBone db in dynamicBoneList)
                {
                    editor.SetCurveEnableDB(db, frameTimes);
                }
            }
            if (isGameObject)
            {
                foreach (var gameObject in boneObjectList)
                {
                    editor.SetCurveEnable(gameObject);
                }
            }
            editor.ExportClip(_fileName + "_enable");
        }
    }
}
