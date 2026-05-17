using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    public class PuzzlePieceRenderer : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 50f)]
        private float padding = 1f;

        public virtual RawImage RenderTarget { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public PuzzlePieceMeshEffect TargetMeshEffect { get; private set; }
        public RectTransform TargetRectTransform { get; private set; }

        protected virtual void Awake()
        {
            if (!RenderTarget)
            {
                RenderTarget = GetComponent<RawImage>() ?? gameObject.AddComponent<RawImage>();
            }

            RectTransform = GetComponent<RectTransform>();
            TargetRectTransform = RenderTarget.GetComponent<RectTransform>();
            TargetMeshEffect =
                RenderTarget.GetComponent<PuzzlePieceMeshEffect>() ??
                RenderTarget.gameObject.AddComponent<PuzzlePieceMeshEffect>();
        }

        public void ConfigureTexture(
            Texture puzzleTexture, 
            Rect textureRect, 
            Rect maskRect)
        {
            RenderTarget.texture = puzzleTexture;
            RenderTarget.uvRect = textureRect;

            Rect copy = new Rect(maskRect);
            copy.size *= 1f + padding * .01f;
            copy.center = maskRect.center;
            TargetMeshEffect.SetMaskRect(copy);
        }

        public void SetMaterial(Material material)
        {
            RenderTarget.material = material;
        }

        public void UpdatePivotPoint(Vector2 min, Vector2 max)
        {
            Rect anchorRect = new Rect(min, max - min);
            Rect copy = new Rect(anchorRect);
            copy.size *= 1f + padding * .01f;
            copy.center = anchorRect.center;

            TargetRectTransform.anchorMin = copy.min;
            TargetRectTransform.anchorMax = copy.max;
            TargetRectTransform.anchoredPosition = Vector2.zero;
            TargetRectTransform.sizeDelta = Vector2.zero;
        }

        public void SetSize(float size)
        {
            RectTransform.sizeDelta = Vector3.one * size;
        }
    }
}
