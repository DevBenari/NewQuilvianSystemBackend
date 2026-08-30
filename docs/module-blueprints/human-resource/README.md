# Blueprint Modul Human Resource

Folder ini adalah blueprint persisten modul `human-resource` Quilvian. Isinya dokumentasi, bukan
source aplikasi. Tidak ada satu berkas pun di sini yang memberi wewenang untuk mengubah
controller, entity, migration, database, maupun frontend.

## Untuk siapa dokumen ini

| Pembaca | Mulai dari |
| --- | --- |
| Pemilik produk yang perlu menyetujui | [`MODULE-STATUS.md`](./MODULE-STATUS.md) lalu [`00-business-overview.md`](./00-business-overview.md) |
| Programmer yang akan mengerjakan task | [`roadmap/00-slice-roadmap.md`](./roadmap/00-slice-roadmap.md) |
| Orang yang ingin tahu apa yang sudah ada di sistem | [`01-existing-capability-map.md`](./01-existing-capability-map.md) |
| Orang yang ingin tahu kenapa sesuatu diputuskan begitu | [`00-interview-decisions.md`](./00-interview-decisions.md) |

## Urutan baca yang disarankan

1. [`00-interview-decisions.md`](./00-interview-decisions.md) — keputusan manusia, apa yang
   sudah dikunci dan apa yang masih terbuka.
2. [`01-existing-capability-map.md`](./01-existing-capability-map.md) — audit apa yang benar-benar
   ada di source hari ini.
3. [`00-business-overview.md`](./00-business-overview.md) — tujuan, batas, dan pelaku.
4. [`01-prerequisite-readiness.md`](./01-prerequisite-readiness.md) — apa yang harus siap lebih
   dulu sebelum tiap fase boleh jalan.
5. [`roadmap/00-slice-roadmap.md`](./roadmap/00-slice-roadmap.md) — pekerjaan dipecah menjadi
   slice beserta status rilisnya.
6. [`02-backend-architecture.md`](./02-backend-architecture.md) dan
   [`03-frontend-architecture.md`](./03-frontend-architecture.md) — bentuk yang ingin dicapai.
7. [`flowcharts/`](./flowcharts/) — urutan langkah yang dikerjakan orang, beserta jalur gagalnya.
8. [`data/data-dictionary.md`](./data/data-dictionary.md) dan [`contracts/`](./contracts/) —
   kontrak kolom, endpoint, status, validasi, hak akses, dan integrasi.
9. [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) — **sampai mana modul ini dianggap selesai untuk
   rilis pertama.** Ini dokumen yang dibaca pemilik produk saat memberi approval.

## Struktur artefak canonical

Ketiga belas artefak wajib mengikuti
`design-business-module/references/blueprint-output-contract.md` versi canonical. Daftar lengkap
beserta berkas yang **bukan** bagian ketiga belas artefak ada di
[`blueprint-manifest.md`](./blueprint-manifest.md) bagian 7.

**Folder `erd/` tidak dipakai dan tidak boleh dibuat.** Relasi entity ditulis sebagai Mermaid
`classDiagram` di [`02-backend-architecture.md`](./02-backend-architecture.md); struktur tabel dan
kolom ada di [`data/data-dictionary.md`](./data/data-dictionary.md); alur kerja pengguna ada di
[`flowcharts/`](./flowcharts/); lifecycle status ada di
[`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md).

## Yang khusus pada modul ini

Modul HR berbeda dari modul lain dalam satu hal penting: **source code-nya dibuat lebih dulu,
blueprint-nya menyusul.** Backend HR sudah memuat 150 controller dan 1.343 endpoint sebelum
dokumen mana pun di folder ini ada.

Akibatnya blueprint ini tidak merancang dari nol. Sebagian besar isinya adalah menetapkan mana
yang dipakai ulang apa adanya, mana yang diperluas, mana yang diperbaiki, dan mana yang memang
belum ada. Dasar penilaian itu adalah capability map, bukan dugaan.

## Batas yang tidak boleh dilanggar

1. **Satu sumber kebenaran capability.** [`01-existing-capability-map.md`](./01-existing-capability-map.md)
   adalah satu-satunya peta kemampuan yang berlaku. Berkas `02-existing-capability-map.md` hanya
   penunjuk, bukan salinan.
2. **Blueprint bukan izin implementasi.** Menulis sesuatu di sini tidak membuat pekerjaannya
   boleh dikerjakan. Wewenang tulis backend dan frontend diberikan terpisah per task.
3. **Bagian berstatus `BLOCKED` tidak boleh dinaikkan menjadi siap berdasarkan asumsi.**
   Kenaikan status hanya sah bila dependency-nya benar-benar terpenuhi dan buktinya dicatat.
4. **Batas keselamatan klinis tidak dirancang di sini.** Kredensial, kewenangan klinis, OPPE,
   FPPE, dan kesehatan kerja menunggu `requirement-completeness-gate` dan
   `hospital-domain-architect`.

## Cara memperbarui

Setiap perubahan material menaikkan `revision` pada
[`blueprint-manifest.md`](./blueprint-manifest.md) dan memperbarui `updated_at`. Perubahan yang
hanya menyangkut status tidak menaikkan revision.

Bila SHA source yang tercatat sudah berbeda dengan HEAD repository, artefak yang bergantung
padanya ditandai `STALE` dan harus melewati impact review terbatas sebelum dipakai lagi sebagai
acuan.
