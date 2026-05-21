---
name: ux
description: "Use for UX/UI work on landing pages, dashboards, navigation systems, component styling, layout principles, design tokens, responsive behavior, accessibility, microcopy, and interaction flow improvements."
---

<!-- Tip: Use /create-skill in chat to generate content with agent assistance -->

# UX Skill

This skill provides the default UX workflow, visual language, and implementation guardrails for this workspace.

Use it when the task is about:
- Designing or refining a landing page, dashboard, list view, or detail page
- Choosing layout structure, spacing, typography, colors, and motion
- Improving usability, navigation, breadcrumbs, filters, tables, or call-to-action hierarchy
- Building non-standard UI that should not look like a default Bootstrap template
- Applying the workspace design tokens consistently

## When To Use
Use this skill for any prompt that mentions:
- UX, UI, usability, interaction design, visual hierarchy, accessibility, IA, microcopy
- landing page, dashboard, navigation, breadcrumb, list view, detail page, card, table, chart
- design system, design tokens, responsive layout, custom styling, polished UI

If the request is not UX-related, do not force this skill. Use the default agent instead.

## UX Direction
The preferred style is a professional operations dashboard with a custom, non-generic feel.

### Visual Tone
- Dark navigation shell and light content canvas
- Confident contrast and clear data hierarchy
- Crisp, information-dense sections with generous spacing
- Practical, operational, and slightly premium

### Color System
Use these colors as the default palette:
- Sidebar / navigation: `#142638`
- App background: `#F4F7FB`
- Surfaces / cards: `#FFFFFF`
- Border / separators: `#E6ECF2`
- Primary accent: `#27B1E6`
- KPI yellow: `#F0C419`
- KPI violet: `#6E62C6`
- KPI green: `#22B455`
- KPI sky: `#2EA9DF`
- Headings: `#1D2A3A`
- Body text: `#4A5A6A`
- Muted text: `#7E8B98`

### Typography
- Prefer a modern geometric sans stack such as `Manrope`, `Plus Jakarta Sans`, or `Segoe UI`
- Use strong hierarchy: 700 for primary headings, 600 for section titles, 500 for labels, 400 for body text
- Keep headings tight and content readable

### Layout Principles
- Use a dashboard grid with clear zones: navigation, utilities, filters, KPIs, analytics, detail content
- Prioritize progressive disclosure: summary first, detail second
- Keep an 8px spacing rhythm
- Make desktop the primary layout, then adapt to stacked mobile behavior
- Collapse navigation on smaller screens and preserve chart/table readability

### Component Principles
- Navigation: dark vertical sidebar with icons and compact labels
- Filters: aligned, compact controls with equal heights
- Forms: use rounded corners for inputs, selects, and textareas; default to `var(--ux-radius-md)` unless a component needs a stronger shape
- KPI cards: color-block cards with large values and short captions
- Tables: minimal borders, readable headers, strong column rhythm
- Charts: restrained grid, one dominant accent color, simple line or area presentation
- Actions: icon-led row actions and clear primary buttons

### Motion and Interaction
- Use subtle transitions only, around 150-250ms
- Animate state changes like hover, selected, filtered, and expanded
- Avoid decorative motion that does not improve clarity

### Accessibility Baseline
- Keep contrast readable for text and controls
- Preserve visible focus states
- Avoid color-only meaning
- Use explicit labels and clear affordances

## Design Tokens
Use these tokens as the default implementation target:

```css
:root {
	--ux-nav-bg: #142638;
	--ux-app-bg: #F4F7FB;
	--ux-surface: #FFFFFF;
	--ux-border: #E6ECF2;
	--ux-accent: #27B1E6;

	--ux-kpi-yellow: #F0C419;
	--ux-kpi-violet: #6E62C6;
	--ux-kpi-green: #22B455;
	--ux-kpi-sky: #2EA9DF;

	--ux-text-heading: #1D2A3A;
	--ux-text-body: #4A5A6A;
	--ux-text-muted: #7E8B98;
	--ux-text-on-dark: #DCE6F2;

	--ux-font-sans: "Manrope", "Plus Jakarta Sans", "Segoe UI", sans-serif;
	--ux-font-size-xs: 0.75rem;
	--ux-font-size-sm: 0.875rem;
	--ux-font-size-md: 1rem;
	--ux-font-size-lg: 1.25rem;
	--ux-font-size-xl: 1.75rem;
	--ux-font-weight-regular: 400;
	--ux-font-weight-medium: 500;
	--ux-font-weight-semibold: 600;
	--ux-font-weight-bold: 700;

	--ux-space-1: 0.25rem;
	--ux-space-2: 0.5rem;
	--ux-space-3: 0.75rem;
	--ux-space-4: 1rem;
	--ux-space-5: 1.5rem;
	--ux-space-6: 2rem;

	--ux-radius-sm: 0.375rem;
	--ux-radius-md: 0.625rem;
	--ux-radius-lg: 0.875rem;
	--ux-shadow-soft: 0 2px 8px rgba(20, 38, 56, 0.08);
	--ux-shadow-card: 0 6px 18px rgba(20, 38, 56, 0.10);

	--ux-motion-fast: 150ms;
	--ux-motion-base: 220ms;
	--ux-ease-standard: cubic-bezier(0.2, 0, 0, 1);
}
```

## Recommended Workflow
1. Define the UX problem in one sentence.
2. Decide the main user goal and the primary screen state.
3. Choose the layout pattern and component structure.
4. Apply the workspace tokens and palette.
5. Validate responsiveness, accessibility, and visual uniqueness.

## Output Format
When using this skill, prefer this output:
- UX diagnosis
- Visual direction
- Component and layout spec
- Implementation details
- Validation checklist

## Example Prompts
- Design a non-default landing page for the support console using the workspace token palette.
- Improve the bookings list so it feels like a custom operations dashboard with breadcrumbs and detail links.
- Redesign the ticket detail screen with better hierarchy, spacing, and actions.
- Propose a responsive navigation system that does not look like Bootstrap.

## Notes
- This skill is a UX playbook, not a backend or data skill.
- For implementation, keep the UI unique and intentional rather than generic or template-like.
- If a UX Specialist subagent exists in the workspace, use it for actual UX implementation tasks and keep this skill aligned with its design system.