---
name: Gallery Nocturne
colors:
  surface: '#131313'
  surface-dim: '#131313'
  surface-bright: '#3a3939'
  surface-container-lowest: '#0e0e0e'
  surface-container-low: '#1c1b1b'
  surface-container: '#201f1f'
  surface-container-high: '#2a2a2a'
  surface-container-highest: '#353534'
  on-surface: '#e5e2e1'
  on-surface-variant: '#d3c4b2'
  inverse-surface: '#e5e2e1'
  inverse-on-surface: '#313030'
  outline: '#9c8f7e'
  outline-variant: '#4f4537'
  surface-tint: '#f4bd61'
  primary: '#f4bd61'
  on-primary: '#432c00'
  primary-container: '#c8963e'
  on-primary-container: '#4a3100'
  inverse-primary: '#7e5700'
  secondary: '#e2c375'
  on-secondary: '#3d2e00'
  secondary-container: '#5b4601'
  on-secondary-container: '#d3b568'
  tertiary: '#9ecaff'
  on-tertiary: '#003258'
  tertiary-container: '#71a2da'
  on-tertiary-container: '#003861'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#ffdead'
  primary-fixed-dim: '#f4bd61'
  on-primary-fixed: '#281900'
  on-primary-fixed-variant: '#604100'
  secondary-fixed: '#ffdf91'
  secondary-fixed-dim: '#e2c375'
  on-secondary-fixed: '#241a00'
  on-secondary-fixed-variant: '#584400'
  tertiary-fixed: '#d1e4ff'
  tertiary-fixed-dim: '#9ecaff'
  on-tertiary-fixed: '#001d36'
  on-tertiary-fixed-variant: '#05497c'
  background: '#131313'
  on-background: '#e5e2e1'
  surface-variant: '#353534'
typography:
  display-title:
    fontFamily: EB Garamond
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: 0.2em
  headline-italic:
    fontFamily: EB Garamond
    fontSize: 20px
    fontWeight: '400'
    lineHeight: 28px
  body-ui:
    fontFamily: Montserrat
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
    letterSpacing: 0.02em
  label-caps:
    fontFamily: Montserrat
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.1em
  metadata-sm:
    fontFamily: Montserrat
    fontSize: 13px
    fontWeight: '300'
    lineHeight: 18px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 8px
  margin-screen: 32px
  gutter-frame: 16px
  section-gap: 48px
---

## Brand & Style

The design system is centered on the concept of a "Private Art Gallery at Midnight." It targets a sophisticated audience seeking a meditative, low-stimulation escape. The aesthetic is a fusion of **Minimalism** and **Tactile Luxury**, emphasizing quality over quantity.

The UI should feel like a physical space—quiet, hushed, and expensive. Every interaction is intentional, eschewing common "gamey" tropes like vibrant particles or urgent notifications in favor of soft transitions, thin gold leaf accents, and generous museum-like breathability. The emotional response is one of calm, focus, and intellectual indulgence.

## Colors

The palette is strictly nocturnal and warm, rooted in deep shadows and metallic highlights. 

- **Primary & Secondary:** Gold tones are used sparingly for interaction and titles. Gold is never used for large surfaces, only for "illumination."
- **Neutral Base:** The background is an absolute deep charcoal, ensuring the "art" (the puzzle pieces) remains the sole focus.
- **Surface & Borders:** Surfaces use a subtle lift from the background. Borders represent ornate wood or bronze frames, providing a physical boundary to the digital canvas.
- **Constraints:** No red is permitted in the system. Critical states or errors should be communicated through typography or muted gold shifts, never through high-energy alert colors.

## Typography

Typography follows an editorial, literary hierarchy. 

- **Titles:** Use `ebGaramond` with wide tracking and All-Caps to mimic the engraved brass plaques found in galleries.
- **Narrative/Subtitles:** Use the italic variant of `ebGaramond` to provide a warm, human touch to descriptions or quotes.
- **Interface Elements:** `montserrat` provides the necessary legibility for functional UI. It should be used in lighter weights (300/400) to maintain a delicate appearance. 
- **Diamond Markers:** The character `◇` (small diamond) is used as a separator between metadata or to indicate active states in navigation.

## Layout & Spacing

The layout philosophy is "Museum Breathability." Content is never crowded; white space (or in this case, "dark space") is treated as a premium luxury.

- **The Frame Model:** Elements are grouped into "framed" units. Each frame has a 1px `border_color_hex` and internal padding of at least 16px.
- **Safe Zones:** Use a generous 32px margin on the left and right of the mobile screen to prevent the UI from feeling claustrophobic.
- **Dividers:** Use 1px horizontal gold lines (`primary_color_hex`) that do not touch the screen edges, often anchored by a center `◇` marker.
- **Vertical Rhythm:** Large gaps (48px+) between major sections help pace the user's journey, encouraging a slow, meditative interaction.

## Elevation & Depth

Depth is conveyed through lighting rather than traditional drop shadows.

- **Spotlight Halos:** Key elements (like the active puzzle or a featured gallery card) feature a soft `spotlight_glow` radial gradient behind them, suggesting an overhead gallery lamp.
- **The Veil:** Incomplete or locked content is covered by the `overlay_veil`. This is not a simple "gray out" but a deep, translucent layer that feels like a room with the lights turned off.
- **Tonal Layering:** 
    - Level 0: Background (`#0a0a0a`) - The "Wall."
    - Level 1: Surface (`#141414`) - The "Plinth" or "Frame."
    - Level 2: Gold Accents - The "Illumination."

## Shapes

The shape language is architectural and precise. 

We use **Soft (1)** roundedness (0.25rem) to avoid the clinical feel of sharp corners while maintaining a formal, structured look. This slight rounding suggests hand-finished wood or polished stone. Circular shapes are reserved exclusively for the "Spotlight" effects and never for structural UI containers.

## Components

- **Buttons:** Ghost-style buttons with a 1px gold border. Text is `label-caps`. On tap, the button fills with a very subtle gold tint (10% opacity), never a solid block of color.
- **Gallery Cards:** Large-format containers with a `#3d2b1a` frame. A `spotlight_glow` should appear behind the card currently in focus during horizontal scrolls.
- **Lists:** Items are separated by thin 1px gold dividers that fade out at the ends. Metadata is aligned to the right in `metadata-sm`.
- **Navigation:** A minimalist bottom bar with simple gold icons. The active state is indicated by a single `◇` marker centered beneath the icon.
- **Input Fields:** Bottom-border only (1px gold). No background fill. The label floats above the line in `label-caps`.
- **Puzzle Pieces:** Should have a subtle highlight on the "top-left" edge to suggest a 3D physical piece catching the gallery light.
- **Checkboxes/Radios:** Small gold diamonds (`◇`). An active state fills the diamond with `secondary_color_hex`.