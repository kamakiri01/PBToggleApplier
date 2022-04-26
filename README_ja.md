# PBToggleApplier

着替え AnimationClip で服の PhysBone も切り替えるツールです。

## 概要

AnimationClip に表示/非表示されている GameObject とその子に含まれる SkinnedMeshRenderer からボーン要素を抽出し、 SkinnedMeshRenderer に従ってボーンの子にあるボーンと物理ボーンを有効/無効にするキーフレームを追加するツールです。

## 対象ユースケース

- (Skinned)Mesh Renderer を表示/非表示して着替える
- (Skinned)Mesh Renderer は服ごとにヒエラルキーの中の Empty Object にまとめている

## 使い方

- unitypackage をインストール
- ヒエラルキーの右クリックから PBToggleApplier を開く
- `Avatar` と `AnimationClip` に更新対象を指定する
- `Run` を押す

## Notice

Unity の服切り替えは様々な実装方法があるため、その方法によっては本ツールで正しいキーフレームを追加できないことがあります。
利用前に AnimationClip ファイルをバックアップするか、本ツールの Save オプションでバックアップファイルを生成することを推奨します。

本ツールは、アバターの着せ替えに Unity のヒエラルキーを利用し、ボーンを入れ子にすることで服を着せる使い方向けです。
Blender による改変や、単一の SkinnedMeshRenderer の BlendShape によって服を切り替えるアバター向けではありません。
