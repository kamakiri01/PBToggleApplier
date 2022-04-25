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

        [SerializeField]
        private List<AnimationClip> targetAnimationClips = new List<AnimationClip>();

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
            content.text = "[Drag AnimationClip Here]";

            var listProperty = so.FindProperty("targetAnimationClips");
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
            isGenerateBackup = EditorGUILayout.ToggleLeft("Generate XXXX_bkup.anim(XXXX.anim clone)", isGenerateBackup);
            EditorGUILayout.LabelField("Save Dir");
            --EditorGUI.indentLevel;
            EditorGUILayout.Space();

            if (GUILayout.Button("Run")) _Run();

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        private void _Run()
        {
            foreach (var clip in targetAnimationClips)
            {
                _Apply(clip);
            }
        }

        private void _Apply(AnimationClip clip)
        {
            if (isGenerateBackup) _CreateBackupFile(clip);

            (float latestKeyframeTime, List<(GameObject GameObject, float Value)> keys) = AnimationClipUtil.ListGameObjectFromAnimation(clip, _avatar);

            List<GameObject> enableList = keys.Where(o => o.Value == 1).Select(o => o.GameObject).ToList();
            List<GameObject> disableList = keys.Where(o => o.Value == 0).Select(o => o.GameObject).ToList();
            (List<DynamicBone> enableDBList, List<DynamicBone> disableDBList) = _ListTargetDynamicBones<DynamicBone>(enableList, disableList);
            // List<VRCPhysBone> hoge = new List<VRCPhysBone>();

            var frameTimes = new List<float>{0};
            if (latestKeyframeTime != 0) frameTimes.Add(latestKeyframeTime); // SDK2のAnimation Overrideを流用している場合など、フレームが2つあるケース
            _SetToggleKeyFrames(enableDBList, disableDBList, clip, frameTimes);
        }

        private (
            List<T> enableDBList,
            List<T> disableDBList
        ) _ListTargetDynamicBones<T>(List<GameObject> enableList, List<GameObject> disableList)
        {
            List<SkinnedMeshRenderer> enableSMR = Util.ListSkinnedMeshRenderersInChildren(enableList);
            List<SkinnedMeshRenderer> disableSMR = Util.ListSkinnedMeshRenderersInChildren(disableList);

            // SkinnedMeshRenderer 階層に置かれる DynamicBone に対応
            var bs1 = Util.ListBoneFromSkinnedMeshRenderers(enableSMR).Union(enableList).ToList();
            var bs2 = Util.ListBoneFromSkinnedMeshRenderers(disableSMR).Union(disableList).ToList();
            // 共通の bone を参照する SkinnedMeshRenderer （素体など）の DynamicBone には干渉しない
            var enableUniqueList = bs1.Except(bs2).ToList();
            var disableUniqueList = bs2.Except(bs1).ToList();
            List<T> enableDBList =  Util.ListComponentFromGameObject<T>(enableUniqueList);
            List<T> disableDBList = Util.ListComponentFromGameObject<T>(disableUniqueList);

            return (enableDBList, disableDBList);
        }

        private void _SetToggleKeyFrames(List<DynamicBone> enableDBList, List<DynamicBone> disableDBList, AnimationClip clip, List<float> frameTimes)
        {
            var editor = new AnimationClipEditor(_avatar, "", clip); // exportしないためsave pathは使用しない
            foreach (DynamicBone db in enableDBList)
            {
                editor.SetCurveEnableDB(db, frameTimes);
            };
            foreach (DynamicBone db in disableDBList)
            {
                editor.SetCurveDisableDB(db, frameTimes);
            };
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
        }

        private void _CreateBackupFile(AnimationClip clip)
        {
            var assetPath = AssetDatabase.GetAssetPath(clip);
            var dirPath = Path.GetDirectoryName(assetPath);
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            var bkupPath = Path.Combine(dirPath, fileName) + "_bkup.anim";
            AssetDatabase.CopyAsset(assetPath, bkupPath);
        }
    }
}