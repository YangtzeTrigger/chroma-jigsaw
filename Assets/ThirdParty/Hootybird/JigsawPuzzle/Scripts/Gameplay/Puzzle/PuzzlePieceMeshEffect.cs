using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    public class PuzzlePieceMeshEffect : BaseMeshEffect
    {
        [SerializeField]
        [ColorUsageAttribute(true, true)]
        private Color colorMultiplier;
        [Header("Don't change in editor.")]
        [SerializeField]
        private Vector2[] rectPoints;

        public Rect MaskRect { get; private set; }

        public void SetColorIntensity(float value)
        {
            colorMultiplier = new Color(value, value, value, value);
            graphic.SetVerticesDirty();
        }

        public void SetMaskRect(Rect maskRect)
        {
            MaskRect = maskRect;

            rectPoints = new Vector2[]
            {
                new Vector2(maskRect.x, maskRect.y),
                new Vector2(maskRect.x, maskRect.max.y),
                new Vector2(maskRect.max.x, maskRect.max.y),
                new Vector2(maskRect.max.x, maskRect.y),
            };
            graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (rectPoints == null || rectPoints.Length == 0)
            {
                return;
            }

            UIVertex v = default;
            for (int i = 0; i < 4; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                v.uv1 = rectPoints[i];
                v.uv2 = colorMultiplier;
                vh.SetUIVertex(v, i);
            }
        }
    }
}