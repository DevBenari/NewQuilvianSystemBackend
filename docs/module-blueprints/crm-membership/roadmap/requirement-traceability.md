# Traceability Requirement — CRM Membership

| Field | Nilai |
|---|---|
| Revision | `1`, status `approved` (Yoga Aji Pratama, 2026-10-13) |
| Roadmap | `CRM-RM-BE-001` r1, `CRM-RM-FE-001` r1 |
| Blueprint | `crm-membership` r1 `approved` |
| SHA | BE `1aecf642`, FE `f1e5e273a` |
| Kontrak | API `v2`, state/validasi/integrasi/izin `v1` |
| Input hash | Lihat `blueprint-manifest.md` (`artifact_hashes`, `input_hashes`) |

## Requirement → Task → Bukti

| Requirement | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-CRM-001` | `CRM-DEC-017` | BE §7 langkah 0 | — | `BE-CRM-000` ✅, `BE-CRM-001` ✅ | — | Baris registry (2026-10-13); QBE checker PASS atas model di folder baru | Selesai |
| `FR-CRM-002` | `CRM-DEC-004`, `012`, `023`, `024` | BE §6 | Kamus data r1 | `BE-CRM-001` ✅ | — | Skrip SQL offline sesuai DDL ([laporan](../task/report/backend/BE-CRM-001.md)) | Selesai, diterapkan ke devYoga |
| `FR-CRM-003` | `CRM-DEC-021` | BE §7 langkah 2 | State `v1` | `BE-CRM-002` ✅ | `FE-CRM-004` | `AT-27`, `UAT-01` lewat uji kering bersemai ([laporan](../task/report/backend/BE-CRM-002.md) §8) | Selesai backend |
| `FR-CRM-004` | `INV-01` s.d. `03` | ERD | State `v1` | `BE-CRM-002` ✅ | — | `UAT-02` di devYoga (23505). `AT-12` jalur API menunggu `BE-CRM-011` | Index aktif |
| `FR-CRM-010` | `CRM-DEC-004`, `023` | BE §4.5 | API `v2` | `BE-CRM-011` 🟡 | `FE-CRM-003` | `AT-02` lulus; `UAT-03` menunggu FE | Backend selesai |
| `FR-CRM-011` | `CRM-DEC-003`, `006`, `012` | — | Validasi `VAL-01` s.d. `05` | `BE-CRM-011` 🟡, `BE-CRM-003` ✅ | `FE-CRM-003` | `AT-01`, `AT-11`, `AT-20` lulus; `UAT-04` menunggu FE | Backend selesai |
| `FR-CRM-012` | `CRM-DEC-024` | — | `VAL-07`, `VAL-08` | `BE-CRM-012` | `FE-CRM-005` | `AT-30`, `UAT-05` | Direncanakan |
| `FR-CRM-013` | `CRM-DEC-012`, `029` | — | `VAL-09` | `BE-CRM-012` | `FE-CRM-005` | `AT-14` | Direncanakan |
| `FR-CRM-014` | `CRM-DEC-010` | — | `VAL-22` | `BE-CRM-014` | `FE-CRM-003` | `AT-18` | Direncanakan |
| `FR-CRM-020` | `CRM-DEC-005` | BE §4.7 | `INT-08` | `BE-CRM-020` | — | `AT-08`, `UAT-06` | Direncanakan |
| `FR-CRM-021` | `CRM-DEC-024` | BE §4.7 | `INT-08` | `BE-CRM-020` | — | Uji data Terjadwal buatan | Direncanakan (sebagian, lihat gap `GAP-03`) |
| `FR-CRM-022` | `CRM-DEC-014` | BE §4.7 | `INT-08` | `BE-CRM-020` | — | `AT-06` | Direncanakan |
| `FR-CRM-023` | — | BE §4.7 | `INT-08` | `BE-CRM-020` | — | Uji idempotensi | Direncanakan |
| `FR-CRM-030` | `CRM-DEC-013` | — | `VAL-10`, `VAL-11` | `BE-CRM-013` | `FE-CRM-005` | `AT-15`, `AT-16`, `UAT-09` | Direncanakan |
| `FR-CRM-031` | `CRM-DEC-025` | — | State `v1` | `BE-CRM-013` | `FE-CRM-005` | Uji unit pembatalan beruntun | Direncanakan (refund menunggu S2) |
| `FR-CRM-032` | `CRM-DEC-021`, `029` | — | `VAL-13` s.d. `15` | `BE-CRM-013` | `FE-CRM-005` | `AT-28`, `UAT-08` | Direncanakan |
| `FR-CRM-033` | `CRM-DEC-022` | — | `VAL-16` | `BE-CRM-013` | `FE-CRM-005` | Uji endpoint | Direncanakan |
| `FR-CRM-034` | `CRM-DEC-013` | BE §4.3 | Izin `v1` | `BE-CRM-011`, `012`, `013`, `020` | `FE-CRM-005` | Baris riwayat di setiap AT | Direncanakan |
| `FR-CRM-035` | — | — | `VAL-17` | `BE-CRM-013` | `FE-CRM-005` | `AT-35`, `UAT-15` | Direncanakan |
| `FR-CRM-040` | `CRM-DEC-005` | J1 | `INT-01` | `BE-CRM-010` ✅ | — | `AT-03`, `AT-09` lulus ([laporan](../task/report/backend/BE-CRM-010.md)) | Selesai backend |
| `FR-CRM-041` | — | BE §4.4 | `INT-03` | `BE-CRM-010` ✅, `BE-CRM-015` | `FE-CRM-007` | Penulis proyeksi diuji; `UAT-11` menunggu `BE-CRM-015` | Sebagian |
| `FR-CRM-042` | `AG-03` | J1 | `INT-02` | `BE-CRM-022` ⛔ | — | `AT-36` | ⛔ menunggu owner rawat-jalan |
| `FR-CRM-043` | `CRM-DEC-018` | — | `INT-03` | `BE-CRM-021` | — | `AT-23` | Direncanakan |
| `FR-CRM-050` | `CRM-DEC-018` | — | API `v2` | `BE-CRM-015` | — | `AT-22`, `UAT-13` | Direncanakan |
| `FR-CRM-051` | `CRM-DEC-011` | — | Izin `v1` | `BE-CRM-011`, `013` | — | `AT-19` | Direncanakan |
| `FR-CRM-052` | `CRM-DEC-022` | — | `VAL-18` | `BE-CRM-015` | — | `AT-25` | Direncanakan |
| `FR-CRM-060` | `CRM-FE-001`, `002` | FE §1 | Izin `v1` | — | `FE-CRM-002` | E2E dua akun | Direncanakan |
| `FR-CRM-061` | `CRM-FE-004` | FE §1 | API `v2` | — | `FE-CRM-003` | `UAT-03` | Direncanakan |
| `FR-CRM-062` | — | FE §1 | API `v2` | — | `FE-CRM-005` | `UAT-08`, `UAT-15` | Direncanakan |
| `FR-CRM-063` | `CRM-DEC-018` | FE §3 | API `v2` | — | `FE-CRM-001` | `AT-24`, `UAT-14` | Direncanakan |
| `FR-CRM-070`, `FR-CRM-071` | `DEC-CRM-001` | BE §10 | `INT-05`, `INT-06` | — | — | — | **Tidak direncanakan** (`EPIC CRM-08`) |
| `FR-CRM-080`, `FR-CRM-081` | `DEC-CRM-001`, `AG-01` | J3 | `INT-04` | — | — | — | **Tidak direncanakan** (`EPIC CRM-09`) |

## Coverage Gap (requirement atau uji yang belum tercakup task)

| ID | Yang belum tercakup | Sebab | Kapan tertutup |
|---|---|---|---|
| `GAP-01` | `AT-04`, `AT-05`, `AT-07`, `UAT-07`: aktivasi dan kabar lunas | `ConfirmFeePaidAsync` milik `EPIC CRM-08` | Setelah `DEC-CRM-001` |
| `GAP-02` | `AT-13`: upgrade selesai (lama `Upgraded`, baru Aktif) | Butuh pelunasan | Setelah `DEC-CRM-001` |
| `GAP-03` | `AT-31`, `AT-32`: perpanjangan menjadi Terjadwal lalu Aktif, dan upgrade saat ada Terjadwal | Status Terjadwal hanya lahir dari pelunasan. `BE-CRM-020` hanya bisa diuji dengan data sisipan uji | Setelah `DEC-CRM-001` |
| `GAP-04` | `AT-17`: refund salah input ditolak bila manfaat terpakai | `INT-07` menunggu billing. Selama MVP, refund salah input ditolak fail-closed | Setelah `DEC-CRM-001` |
| `GAP-05` | `AT-21`, `AT-33`, `AT-34`: diskon di billing | `EPIC CRM-09` | Setelah persetujuan billing dan `AG-01` |
| `GAP-06` | `AT-10`: snapshot kunjungan IGD/rawat inap | `AG-01` belum disetujui. RJ tercakup lewat snapshot antrean yang sudah ada | Setelah owner Registration menyetujui |
| `GAP-07` | `AT-36` penuh | `BE-CRM-022` ⛔ | Setelah owner rawat-jalan menyetujui |
| `GAP-08` | `AT-37`: triage IGD tidak terpengaruh | Uji manual, tidak ada task karena modul ini tidak menyentuh triage | UAT manual saat go-live |

## Catatan Operasional

Selama `EPIC CRM-08` belum ada, setiap pendaftaran baru akan batal otomatis setelah 7 hari
karena tidak ada jalan untuk melunasinya. Owner sudah menerima konsekuensi ini (`CRM-DEC-030`).
Saran: menu **Pendaftaran Membership** tidak diberikan ke petugas di produksi sampai
`EPIC CRM-08` siap. Yang dipakai di produksi cukup Daftar Membership dan verifikasi data lama.
Keputusan pemberian izin tetap di tangan owner.
