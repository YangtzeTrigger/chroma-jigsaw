using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    /// <summary>
    /// Basic cluster shadow effect with simple colorTween and offset.
    /// </summary>
    public class DefaultClusterShadowEffect : BaseClusterShadowEffect
    {
        [SerializeField]
        private Vector2 shadowOffset;
        [SerializeField]
        private Color shadowColor;
        [SerializeField]
        private float shadowAnimationDuration = .3f;

        private float alphaValue;
        private List<ShadowInfo> shadows;

        public bool State { get; private set; }

        private void Awake()
        {
            shadows = new List<ShadowInfo>();
        }

        private void Update()
        {
            if (State)
            {
                if (alphaValue < shadowColor.a)
                {
                    alphaValue += Time.deltaTime / shadowAnimationDuration;
                    Color color = shadowColor;
                    color.a = alphaValue;

                    SetShadowsColor(color);
                }
            }
            else
            {
                if (alphaValue > 0f)
                {
                    alphaValue -= Time.deltaTime / shadowAnimationDuration;
                    Color color = shadowColor;
                    color.a = alphaValue;

                    SetShadowsColor(color);
                }
            }
        }

        public override void OnClusterDrag(PuzzlePiece target)
        {
            if (!State)
            {
                State = true;
            }

            foreach (ShadowInfo shadow in shadows)
            {
                shadow.shadowRect.transform.localPosition = (Vector2)shadow.pieceReference.transform.localPosition + shadowOffset;
            }
        }

        public override void OnClusterDrop()
        {
            if (State)
            {
                State = false;
            }
        }

        public override void AddShadowForPuzzlePiece(PuzzlePiece puzzlePiece)
        {
            // Create a new GameObject for the shadow.
            GameObject shadowGO = new GameObject("Shadow", typeof(RectTransform));
            shadowGO.transform.SetParent(transform);
            shadowGO.transform.localScale = Vector3.one;

            // Update shadow size to puzzle piece size.
            RectTransform shadowRect = shadowGO.GetComponent<RectTransform>();
            shadowRect.sizeDelta = puzzlePiece.RectTransform.sizeDelta;

            // Get copy of puzzle piece mesh and configure it.
            PuzzlePieceMeshEffect meshEffectCopy = Instantiate(puzzlePiece.TargetMeshEffect, shadowRect);
            Graphic graphic = meshEffectCopy.GetComponent<Graphic>();
            graphic.raycastTarget = false;
            graphic.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 0f);
            RectTransform meshEffectRect = meshEffectCopy.GetComponent<RectTransform>();
            meshEffectRect.sizeDelta = Vector2.zero;

            // Remove other components.
            foreach (Component component in meshEffectCopy.GetComponents<Component>())
            {
                if (!(component is RectTransform) && !(component is Graphic) && !(component is PuzzlePieceMeshEffect) && !(component is CanvasRenderer))
                {
                    Destroy(component);
                }
            }

            // Add to shadows list.
            shadows.Add(new ShadowInfo
            {
                shadowRect = shadowRect,
                graphics = graphic,
                pieceReference = puzzlePiece
            });
        }
        private void SetShadowsColor(Color color)
        {
            foreach (ShadowInfo shadow in shadows)
            {
                shadow.graphics.color = color;
            }
        }

        protected class ShadowInfo
        {
            public RectTransform shadowRect;
            public Graphic graphics;
            public PuzzlePiece pieceReference;
        }
    }
}
