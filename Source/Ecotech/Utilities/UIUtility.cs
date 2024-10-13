using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Ecotech
{
    public static class UIUtility
    {
        #region Scroll View

        /// <summary>
        /// will be subtracted from width for inner windows in scroll views
        /// </summary>
        public const float DefaultSliderWidth = 16f;

        /// <summary>
        /// Convenience method to create a rectangle that fits perfectly as the inner rectangle (the content) of a scroll view rect
        /// </summary>
        /// <param name="outerRect">Outer rectangle that will hold the slider and bounding box</param>
        /// <param name="height">Height of the inner rectangle, you must calculate and preferrably cache it</param>
        /// <param name="sliderWidth">The width of the slider on the side</param>
        /// <returns></returns>
        public static Rect CreateInnerScrollRect(Rect outerRect, float height, float sliderWidth = DefaultSliderWidth)
        {
            return new Rect
            (
                0,
                0,
                outerRect.width - sliderWidth,
                height
            );
        }
        /// <summary>
        /// Convenience method to properly begin a scroll view rect and produce a Listing_Standard for it
        /// </summary>
        /// <param name="inRect">Rect that contains the full scroll view (bounding box)</param>
        /// <param name="requiredHeight">Height required to display all the contents of the inner scroll view. You should initialize this with a high number and then overwrite with the result of <see cref="EndScrollView(Listing_Standard, ref float)"/></param>
        /// <param name="scrollPosition">keeps track of the current scroll position</param>
        /// <param name="list">Produced Listing_Standard, which will keep track of the used height for <see cref="EndScrollView(Listing_Standard, ref float)"/></param>
        public static void MakeAndBeginScrollView(Rect inRect, float requiredHeight, ref Vector2 scrollPosition, out Listing_Standard list)
        {
            MakeAndBeginScrollView(inRect, requiredHeight, ref scrollPosition, out Rect innerRect);
            list = new Listing_Standard()
            {
                ColumnWidth = innerRect.width,
                maxOneColumn = true
            };
            list.Begin(innerRect);
        }

        /// <summary>
        /// Convenience method to properly begin a scroll view rect
        /// </summary>
        /// <param name="inRect">Rect that contains the full scroll view (bounding box)</param>
        /// <param name="requiredHeight">Height required to display all the contents of the inner scroll view. You should initialize this with a high number and then overwrite with the result of <see cref="EndScrollView(Listing_Standard, ref float)"/></param>
        /// <param name="scrollPosition">keeps track of the current scroll position</param>
        /// <param name="outRect">Inner scrollView rectangle</param>
        public static void MakeAndBeginScrollView(Rect inRect, float requiredHeight, ref Vector2 scrollPosition, out Rect outRect)
        {
            outRect = CreateInnerScrollRect(inRect, requiredHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, outRect);
        }

        /// <summary>
        /// Convenience method to wrap up <see cref="MakeAndBeginScrollView(Rect, float, ref Vector2, out Listing_Standard)"/>
        /// </summary>
        /// <param name="list">The Listing_Standard used by the scroll views inner rectangle</param>
        /// <param name="requiredHeight">The height used by the Listing_Standard</param>
        public static void EndScrollView(this Listing_Standard list, out float requiredHeight)
        {
            requiredHeight = list.CurHeight;
            list.End();
            Widgets.EndScrollView();
        }
        #endregion

        #region Vertical Fillable Bar
        public static Rect VerticalFillableBar(Rect inRect, float fillPercent, bool doBorder, Texture2D fillTex = null, bool drawBackground = true, Texture2D bgTex = null)
        {
            if(fillTex == null)
            {
                fillTex = Widgets.BarFullTexHor;
            }
            if(bgTex == null)
            {
                bgTex = BaseContent.BlackTex;
            }
            if (doBorder)
            {
                GUI.DrawTexture(inRect, BaseContent.BlackTex);
                inRect = inRect.ContractedBy(3f);
            }
            if (drawBackground)
            {
                GUI.DrawTexture(inRect, bgTex);
            }

            Rect filledRect = inRect.BottomPart(fillPercent);
            GUI.DrawTexture(filledRect, fillTex);

            return inRect;
        }

        #endregion

        #region Label drawing
        public static void Label(Rect inRect, string label, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            GameFont previousFont = Text.Font;
            TextAnchor previousAnchor = Text.Anchor;
            Text.Font = font;
            Text.Anchor = anchor;
            Widgets.Label(inRect, label);
            Text.Anchor = previousAnchor;
            Text.Font = previousFont;
        }
        #endregion
    }
}
