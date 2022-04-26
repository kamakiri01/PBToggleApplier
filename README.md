[日本語 README](./README_ja.md)

# PBToggleApplier

Tool to switch PhysBone with AnimationClip for changing clothes.

 ## About

 This tool extracts bones from GameObjects shown/hidden in AnimationClip and the SkinnedMeshRenderer contained in their children, and adds keyframes to Enable/Disable the bones and PhysBone in the bones' children, according to the SkinnedMeshRenderer.

## Target Use Case

- Show/Hide (Skinned)Mesh Renderer to change clothes
- (Skinned)Mesh Renderer is grouped into Empty Objects in the hierarchy for each clothes

## Usage

- Install unitypackage
- Open PBToggleApplier from right click in hierarchy
- Set `Avatar` and target `AnimationClip`
- Push `Run`

## Notice

Clothing switching in Unity can be implemented in a variety of ways, and depending on those methods, this tool may not add the correct keyframes.
We recommend backing up your AnimationClip file before using this tool or generating a backup file with the Save option of this tool.

This tool is intended for use with Unity's Hierarchy to change avatars' clothing by nesting bones.
It is not intended for avatars that switch clothes using Blender modification or BlendShape in a single SkinnedMeshRenderer.