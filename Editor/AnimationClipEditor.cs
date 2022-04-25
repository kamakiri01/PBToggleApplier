using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using System.Collections.Generic;

namespace PBToggleApplier
{
    public class AnimationClipEditor
    {
        private readonly AnimationClip _animationClip;
        private readonly string _exportDirPath;
        private readonly GameObject _avatarRoot;

        public AnimationClipEditor(GameObject rootObject, string exportDirPath)
        {
            _animationClip = new AnimationClip();
            _exportDirPath = exportDirPath;
            _avatarRoot = rootObject;
        }

        public AnimationClipEditor(GameObject rootObject, string exportDirPath, AnimationClip animationClip)
        {
            _animationClip = animationClip;
            _exportDirPath = exportDirPath;
            _avatarRoot = rootObject;
        }

        public AnimationClip ExportClip(string name)
        {
            AssetDatabase.CreateAsset(_animationClip, _exportDirPath + name + ".anim");
            return _animationClip;
        }

        public void SetCurvePageSync(GameObject gameObject, int page)
        {
            var path = PathUtil.DropLastSlash(PathUtil.GetHierarchyPath(gameObject, _avatarRoot));
            var curve = new AnimationCurve();
            curve.AddKey(0, (float)page);
            _animationClip.SetCurve(path, typeof(MeshRenderer), "material._PageNumber", curve);
        }

        public void SetCurveEnable(GameObject gameObject)
        {
            var path = PathUtil.DropLastSlash(PathUtil.GetHierarchyPath(gameObject, _avatarRoot));
            AnimationUtility.SetEditorCurve(
                   _animationClip,
                 EditorCurveBinding.FloatCurve(path, typeof(GameObject), "m_IsActive"),
                 AnimationCurve.Linear(0, 1, 1 / 60f, 1)
            );
            var keys = new List<Keyframe>();
            keys.Add(new Keyframe(0, 1, Mathf.Infinity, Mathf.Infinity));
            AnimationCurve curve =  new AnimationCurve(keys.ToArray());
            _animationClip.SetCurve(path, typeof(GameObject), "m_IsActive", curve);
        }

        public void SetCurveDisable(GameObject gameObject)
        {
            var path = PathUtil.DropLastSlash(PathUtil.GetHierarchyPath(gameObject, _avatarRoot));
            AnimationUtility.SetEditorCurve(
                  _animationClip,
                EditorCurveBinding.FloatCurve(path, typeof(GameObject), "m_IsActive"),
                AnimationCurve.Linear(0, 0, 1 / 60f, 0)
            );
            var keys = new List<Keyframe>();
            keys.Add(new Keyframe(0, 0));
            keys.Add(new Keyframe(3, 0));
            AnimationCurve curve =  new AnimationCurve(keys.ToArray());
            _animationClip.SetCurve(path, typeof(GameObject), "m_IsActive", curve);
        }

        public void SetCurveEnableDB(DynamicBone component, List<float> frameTimes)
        {
            GameObject gameObject = component.gameObject;
            var path = PathUtil.DropLastSlash(PathUtil.GetHierarchyPath(gameObject, _avatarRoot));
            /*
            AnimationUtility.SetEditorCurve(
                   _animationClip,
                 EditorCurveBinding.FloatCurve(path, typeof(GameObject), "m_IsActive"),
                 AnimationCurve.Linear(0, 1, 1 / 60f, 1)
            );*/
            var keys = new List<Keyframe>();
            foreach (float frameTime in frameTimes)
            {
                keys.Add(new Keyframe(frameTime, 1));
            }
            AnimationCurve curve =  new AnimationCurve(keys.ToArray());
            _animationClip.SetCurve(path, typeof(DynamicBone), "m_Enabled", curve);
        }

        public void SetCurveDisableDB(DynamicBone component, List<float> frameTimes)
        {
            GameObject gameObject = component.gameObject;
            var path = PathUtil.DropLastSlash(PathUtil.GetHierarchyPath(gameObject, _avatarRoot));
            /*
            AnimationUtility.SetEditorCurve(
                  _animationClip,
                EditorCurveBinding.FloatCurve(path, typeof(DynamicBone), "m_Enabled"),
                AnimationCurve.Linear(0, 0, 1 / 60f, 0)
            );
            */
            var keys = new List<Keyframe>();
            foreach (float frameTime in frameTimes)
            {
                keys.Add(new Keyframe(frameTime, 0));
            }
            AnimationCurve curve =  new AnimationCurve(keys.ToArray());
            _animationClip.SetCurve(path, typeof(DynamicBone), "m_Enabled", curve);
        }

        // ParentConstraint専用
        // NOTE: IConstraint系でラップしたい
        public void SetConstraintComponent(GameObject gameObject, int value)
        {
            var path = PathUtil.DropLastSlash(PathUtil.GetHierarchyPath(gameObject, _avatarRoot));
            ParentConstraint component = gameObject.GetComponent(typeof(ParentConstraint)) as ParentConstraint;

            AnimationUtility.SetEditorCurve(
                  _animationClip,
                EditorCurveBinding.FloatCurve(path, typeof(ParentConstraint), "m_Active"),
                AnimationCurve.Linear(0, value, 1 / 60f, value)
            );
            var keys = new List<Keyframe>();
            keys.Add(new Keyframe(0, value, Mathf.Infinity, Mathf.Infinity));
            AnimationCurve curve =  new AnimationCurve(keys.ToArray());
            _animationClip.SetCurve(path, typeof(ParentConstraint), "m_Active", curve);
        }
    }
}
