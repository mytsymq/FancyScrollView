﻿/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2019 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using EasingCore;

namespace FancyScrollView
{
    /// <summary>
    /// グリッドレイアウトのスクロールビューを実装するための抽象基底クラス.
    /// 無限スクロールおよびスナップには対応していません.
    /// <see cref="FancyScrollView{TItemData, TContext}.Context"/> が不要な場合は
    /// 代わりに <see cref="FancyGridView{TItemData}"/> を使用します.
    /// </summary>
    /// <typeparam name="TItemData">アイテムのデータ型.</typeparam>
    /// <typeparam name="TContext"><see cref="FancyScrollView{TItemData, TContext}.Context"/> の型.</typeparam>
    /// <typeparam name="TGroup">セルグループの型.</typeparam>
    public abstract class FancyGridView<TItemData, TContext, TGroup> : FancyScrollRect<TItemData[], TContext>
        where TContext : class, IFancyGridViewContext, new()
        where TGroup : FancyCellGroup<TItemData, TContext>
    {
        /// <summary>
        /// デフォルトのセルグループクラス.
        /// </summary>
        public abstract class DefaultGroup : FancyCellGroup<TItemData, TContext> { }

        /// <summary>
        /// 最初にセルを配置する軸方向のセル同士の余白.
        /// </summary>
        [SerializeField] protected float startAxisSpacing = 0f;

        /// <summary>
        /// セルのサイズ.
        /// </summary>
        [SerializeField] protected Vector2 cellSize = new Vector2(100f, 100f);

        /// <summary>
        /// セルのグループ Prefab.
        /// </summary>
        /// <remarks>
        /// <see cref="FancyGridView{TItemData, TContext}"/> では,
        /// <see cref="FancyScrollView{TItemData, TContext}.CellPrefab"/> を最初にセルを配置する軸方向のセルコンテナとして使用します.
        /// </remarks>
        protected sealed override GameObject CellPrefab => cellGroupTemplate;

        /// <inheritdoc/>
        protected override float CellSize => Scroller.ScrollDirection == ScrollDirection.Horizontal
            ? cellSize.x
            : cellSize.y;

        /// <summary>
        /// 最初にセルを配置する軸方向のセル数.
        /// </summary>
        protected abstract int StartAxisCellCount { get; }

        /// <summary>
        /// セルのテンプレート.
        /// </summary>
        protected abstract FancyCell<TItemData, TContext> CellTemplate { get; }

        /// <summary>
        /// アイテムの総数.
        /// </summary>
        public int DataCount { get; private set; }

        GameObject cellGroupTemplate;

        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();

            Debug.Assert(CellTemplate != null);
            Debug.Assert(StartAxisCellCount > 0);

            cellGroupTemplate = new GameObject("Group").AddComponent<TGroup>().gameObject;
            cellGroupTemplate.transform.SetParent(cellContainer, false);
            cellGroupTemplate.SetActive(false);

            Context.CellTemplate = CellTemplate.gameObject;
            Context.ScrollDirection = Scroller.ScrollDirection;
            Context.GetGroupCount = () => StartAxisCellCount;
            Context.GetStartAxisSpacing = () => startAxisSpacing;
            Context.GetCellSize = () => Scroller.ScrollDirection == ScrollDirection.Horizontal
                ? cellSize.y
                : cellSize.x;
        }

        /// <summary>
        /// 渡されたアイテム一覧に基づいて表示内容を更新します.
        /// </summary>
        /// <param name="items">アイテム一覧.</param>
        public virtual void UpdateContents(IList<TItemData> items)
        {
            DataCount = items.Count;

            var itemGroups = items
                .Select((item, index) => (item, index))
                .GroupBy(
                    x => x.index / StartAxisCellCount,
                    x => x.item)
                .Select(group => group.ToArray())
                .ToArray();

            UpdateContents(itemGroups);
        }

        /// <summary>
        /// 指定したアイテムの位置までジャンプします.
        /// </summary>
        /// <param name="itemIndex">アイテムのインデックス.</param>
        /// <param name="alignment">ビューポート内におけるセル位置の基準. 0f(先頭) ~ 1f(末尾).</param>
        protected override void JumpTo(int itemIndex, float alignment = 0.5f)
        {
            var groupIndex = itemIndex / StartAxisCellCount;
            base.JumpTo(groupIndex, alignment);
        }

        /// <summary>
        /// 指定したアイテムの位置まで移動します.
        /// </summary>
        /// <param name="itemIndex">アイテムのインデックス.</param>
        /// <param name="duration">移動にかける秒数.</param>
        /// <param name="alignment">ビューポート内におけるセル位置の基準. 0f(先頭) ~ 1f(末尾).</param>
        /// <param name="onComplete">移動が完了した際に呼び出されるコールバック.</param>
        protected override void ScrollTo(int itemIndex, float duration, float alignment = 0.5f, Action onComplete = null)
        {
            var groupIndex = itemIndex / StartAxisCellCount;
            base.ScrollTo(groupIndex, duration, alignment, onComplete);
        }

        /// <summary>
        /// 指定したアイテムの位置まで移動します.
        /// </summary>
        /// <param name="itemIndex">アイテムのインデックス.</param>
        /// <param name="duration">移動にかける秒数.</param>
        /// <param name="easing">移動に使用するイージング.</param>
        /// <param name="alignment">ビューポート内におけるセル位置の基準. 0f(先頭) ~ 1f(末尾).</param>
        /// <param name="onComplete">移動が完了した際に呼び出されるコールバック.</param>
        protected override void ScrollTo(int itemIndex, float duration, Ease easing, float alignment = 0.5f, Action onComplete = null)
        {
            var groupIndex = itemIndex / StartAxisCellCount;
            base.ScrollTo(groupIndex, duration, easing, alignment, onComplete);
        }
    }

    /// <summary>
    /// グリッドレイアウトのスクロールビューを実装するための抽象基底クラス.
    /// 無限スクロールおよびスナップには対応していません.
    /// </summary>
    /// <typeparam name="TItemData">アイテムのデータ型.</typeparam>
    /// <typeparam name="TGroup">セルグループの型.</typeparam>
    /// <seealso cref="FancyGridView{TItemData, TContext}"/>
    public abstract class FancyGridView<TItemData, TGroup> : FancyGridView<TItemData, FancyGridViewContext, TGroup> 
        where TGroup : FancyCellGroup<TItemData, FancyGridViewContext> { }
}
