# Data-source register

No external catalog, model, colour, instruction, or training dataset may enter production or generated assets until its register entry is reviewed. Small evaluation fixtures follow the same provenance rules.

Create one Markdown file per source using a lowercase provider name. Record:

| Field | Required information |
| --- | --- |
| Provider and dataset | Official name and responsible organization |
| Source URL | Authoritative download or API URL |
| Version | Release identifier or retrieval date |
| Integrity | Published checksum or locally recorded SHA-256 |
| Licence | Exact licence name and authoritative text URL |
| Attribution | Required notices and where they appear |
| Permitted use | Import, modification, redistribution, generated derivatives, and commercial use |
| Restrictions | Share-alike, rate limits, branding, access, or retention terms |
| Update process | Repeatable retrieval and verification steps |
| Internal consumers | Importers, generated assets, tests, training, or runtime features |
| Approval | Proposed, approved for evaluation, approved for distribution, or rejected |
| Review evidence | Issue, ADR, and pull request links |

Keep downloaded full datasets and large generated catalogs out of Git until storage and redistribution have been approved. Tests use small deterministic fixtures and retain their source and licence metadata.
