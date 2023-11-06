using UnityEngine;
using UnityEngine.UI;

// Based on the Unity built-in image source (Unity Reference-Only License, https://unity3d.com/legal/licenses/Unity_Reference_Only_License)
// Modified to add full-mesh UVs by Brian MacIntosh

namespace UI
{
    /// <summary>
    /// Image override that fills the second UV channel with uniform UVs over the entire image.
    /// These are the same as the normal UVs for Simple images, but different for Tiled and Sliced images.
    /// </summary>
    /// <remarks>Much code duplicated from UnityEngine.UI.Image.</remarks>
    public class SlicedGradientImage: Image
    {
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (canvas)
            {
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            }
        }
#endif

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (overrideSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }

            switch (type)
            {
                case Type.Simple:
                    base.OnPopulateMesh(toFill);
                    return;
                case Type.Sliced:
                    GenerateSlicedSprite(toFill);
                    return;
                case Type.Tiled:
                    //TODO:
                    base.OnPopulateMesh(toFill);
                    return;
                case Type.Filled:
                    //TODO?
                    base.OnPopulateMesh(toFill);
                    return;
            }
        }

        static readonly Vector2[] s_VertScratch = new Vector2[4];
        static readonly Vector2[] s_UVScratch = new Vector2[4];
        static readonly Vector2[] s_UV2Scratch = new Vector2[4];

        private void GenerateSlicedSprite(VertexHelper toFill)
        {
            // COPIED from base class Image

            if (!hasBorder)
            {
                //HACK:
                base.OnPopulateMesh(toFill);
                return;
            }

            Vector4 outer, inner, padding, border;

            if (overrideSprite != null)
            {
                outer = UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite);
                inner = UnityEngine.Sprites.DataUtility.GetInnerUV(overrideSprite);
                padding = UnityEngine.Sprites.DataUtility.GetPadding(overrideSprite);
                border = overrideSprite.border;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect rect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = GetAdjustedBorders(border / multipliedPixelsPerUnit, rect);
            padding = padding / multipliedPixelsPerUnit;

            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = adjustedBorders.x;
            s_VertScratch[1].y = adjustedBorders.y;

            s_VertScratch[2].x = rect.width - adjustedBorders.z;
            s_VertScratch[2].y = rect.height - adjustedBorders.w;

            for (int i = 0; i < 4; ++i)
            {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }

            s_UVScratch[0] = new Vector2(outer.x, outer.y);
            s_UVScratch[1] = new Vector2(inner.x, inner.y);
            s_UVScratch[2] = new Vector2(inner.z, inner.w);
            s_UVScratch[3] = new Vector2(outer.z, outer.w);

            // ADDED: Produce UV2 based on vertex positions

            Vector2 vertexSize = s_VertScratch[3] - s_VertScratch[0];
            s_UV2Scratch[0] = Vector3.zero;
            s_UV2Scratch[1] = (s_VertScratch[1] - s_VertScratch[0]) / vertexSize;
            s_UV2Scratch[2] = (s_VertScratch[2] - s_VertScratch[0]) / vertexSize;
            s_UV2Scratch[3] = (s_VertScratch[3] - s_VertScratch[0]) / vertexSize;

            // END ADDED

            toFill.Clear();

            for (int x = 0; x < 3; ++x)
            {
                int x2 = x + 1;

                for (int y = 0; y < 3; ++y)
                {
                    if (!fillCenter && x == 1 && y == 1)
                        continue;

                    int y2 = y + 1;

                    // MODIFIED: Added UV2s
                    AddQuad(toFill,
                        new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                        new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                        color,
                        new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                        new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y),
                        new Vector2(s_UV2Scratch[x].x, s_UV2Scratch[y].y),
                        new Vector2(s_UV2Scratch[x2].x, s_UV2Scratch[y2].y));
                }
            }
        }

        static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color,
            Vector2 uvMin, Vector2 uvMax,
            Vector2 uv2Min, Vector2 uv2Max)
        {
            int startIndex = vertexHelper.currentVertCount;

            // MODIFIED to add uv2
            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y), new Vector2(uv2Min.x, uv2Min.y), Vector3.forward, Vector4.zero);
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y), new Vector2(uv2Min.x, uv2Max.y), Vector3.forward, Vector4.zero);
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y), new Vector2(uv2Max.x, uv2Max.y), Vector3.forward, Vector4.zero);
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y), new Vector2(uv2Max.x, uv2Min.y), Vector3.forward, Vector4.zero);

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
        {
            // COPIED from base class Image

            Rect originalRect = rectTransform.rect;

            for (int axis = 0; axis <= 1; axis++)
            {
                float borderScaleRatio;

                // The adjusted rect (adjusted for pixel correctness)
                // may be slightly larger than the original rect.
                // Adjust the border to match the adjustedRect to avoid
                // small gaps between borders (case 833201).
                if (originalRect.size[axis] != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }

                // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
                // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
                float combinedBorders = border[axis] + border[axis + 2];
                if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }
    }
}