# SUPERSEDED — ERD terpisah tidak lagi digunakan oleh blueprint-output-contract terbaru

Folder `erd/` **tidak lagi menjadi artefak blueprint**. Jangan membuatnya untuk modul baru, dan
jangan menambahkannya ke daftar expected artifacts modul mana pun.

## Penggantinya

| Kebutuhan lama yang dijawab `erd/` | Tempatnya sekarang |
| --- | --- |
| Relasi antar entity per bounded context | `02-backend-architecture.md` — Mermaid `classDiagram` |
| Struktur tabel, kolom, nullability, index, unique, delete behavior, kolom sensitif | `data/data-dictionary.md` |
| Alur kerja pengguna dan percabangan proses bisnis | `flowcharts/**` |
| Lifecycle dan perpindahan status | `contracts/state-transition-matrix.md` |

Ringkasnya: `02-backend-architecture.md` classDiagram + `data/data-dictionary.md` +
`flowcharts/**`.

## Dasar

Authority: `design-business-module/SKILL.md` dan
`design-business-module/references/blueprint-output-contract.md` versi canonical pada repository
`QuilvianEngineeringSkills` (plugin `quilvian-engineering-skills`). Kontrak itu menetapkan tiga
belas berkas yang **MUST** ada, dan `erd/` tidak termasuk di dalamnya.

## Blueprint lama yang masih punya folder `erd/`

Modul yang blueprint-nya disusun sebelum kontrak ini berlaku — antara lain `igd`,
`billing-kasir`, `operations`, `pharmacy`, dan `rawat-jalan` — masih menyimpan folder `erd/`
beserta rujukannya di manifest masing-masing. Isi itu **historis dan tidak dihapus**. Yang tidak
berlaku lagi adalah kewajibannya, bukan keberadaannya. Modul baru dan revisi berikutnya mengikuti
struktur pengganti di atas.
