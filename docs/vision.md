# Product vision

ProjectBrick is a desktop application for constructing LEGO-style models digitally and producing instructions that let a person recreate the same model with physical pieces.

## Intended user outcome

A user can choose from an accurate catalog of part designs and physically available colours, assemble a valid connected model, save and reopen it, and generate a clear sequence of build steps. The application should explain invalid connections or unavailable combinations rather than silently creating an impossible model.

## Product principles

- Preserve stable external identifiers and source provenance for parts, colours, and catalog relationships.
- Keep geometry, collision, occupancy, connection metadata, and physical availability as separate concepts.
- Validate instruction output deterministically before introducing AI-generated sequencing.
- Keep Unity presentation, application behavior, persistence, catalog providers, and future AI providers replaceable through documented boundaries.
- Treat external models, instructions, and catalog data as licensed inputs whose terms must be recorded before use or redistribution.

## Long-term direction

After the builder and deterministic instruction system are reliable, Python services may assist with instruction generation and evaluation. Later stages may link required parts to external marketplaces or prepared carts and provide a website for distributing the application.

Commerce, public distribution, and large-scale AI training are outside the current foundation stage.

LEGO is a trademark of the LEGO Group. ProjectBrick must not imply endorsement or affiliation.
