# Requirement Traceability — Nurse View

| Field | Nilai |
|---|---|
| Blueprint | `nurse-view` revision `1` `approved` (`NV-DEC-028`) |
| Roadmap revision | `1` — 2026-10-28 |
| Contract version | `0.1.0` `approved` |
| SHA | BE `a6f977a3`, FE `76b0c6e4b` |
| Hash masukan | Lihat `blueprint-manifest.md` `artifact_hashes` |

Kolom **Status** diisi build skill sesudah laporan task ada. Saat roadmap ini ditulis, seluruhnya
belum dikerjakan.

## 1. Functional requirement → task → bukti

| Requirement | Decision | Desain/kontrak | Task backend | Task frontend | Bukti | Status |
|---|---|---|---|---|---|---|
| `FR-NV-001` daftarkan perangkat, kode number series | `NV-DEC-016` | api §1, `VAL-NV-01`–`03` | `BE-NV-002` | `FE-NV-002` | `UAT-01`, `VAL-NV-02` | belum |
| `FR-NV-002` buat login sekali | `NV-DEC-022` | state §1, `VAL-NV-05` | `BE-NV-002` | `FE-NV-002` | `NV-AC-018`, `UAT-03` | belum |
| `FR-NV-003` atur ulang login | `NV-DEC-022` | state §1, `VAL-NV-06` | `BE-NV-002` | `FE-NV-002` | uji HTTP token lama | belum |
| `FR-NV-004` nonaktifkan | `NV-DEC-016` | state §1, `VAL-NV-07`, `08`, `15` | `BE-NV-002`, `BE-NV-003` | `FE-NV-002`, `FE-NV-003` | `NV-AC-016`, `UAT-02` | belum |
| `FR-NV-005` online/offline | `G-07` | api §1 `IsOnline` | `BE-NV-002`, `BE-NV-003` | `FE-NV-001` | tangkapan layar | belum |
| `FR-NV-010` bed per lantai per bangsal | `NV-DEC-006`, `025` | api §3 | `BE-NV-005` | `FE-NV-005` | `NV-AC-001`, `021` | belum |
| `FR-NV-011` identitas samaran | `NV-DEC-008` | §4.7 masking | `BE-NV-005` | `FE-NV-005` | `NV-AC-002`, `UAT-04`, `UAT-05` | belum |
| `FR-NV-012` perawat PJ | `NV-DEC-012` | api §3 | `BE-NV-005` | `FE-NV-005` | `NV-AC-008` | belum |
| `FR-NV-013` TTV sah terakhir | `NV-DEC-011` | §4.6 langkah 6 | `BE-NV-005` | `FE-NV-005` | `NV-AC-007` | belum |
| `FR-NV-014` kartu kritis | `NV-DEC-013` | api §3 `IsCritical` | `BE-NV-005` | `FE-NV-005` | `NV-AC-003` | belum |
| `FR-NV-015` ikon keselamatan | `NV-DEC-018`–`020` | §4.6 langkah 7–8 | `BE-NV-005` | `FE-NV-005` | `NV-AC-011`–`013` | belum |
| `FR-NV-016` bed tak berpenghuni | `NV-DEC-015` | api §3 `StatusLabel` | `BE-NV-005` | `FE-NV-005` | `NV-AC-010` | belum |
| `FR-NV-017` header perawat jaga | `NV-DEC-012`, `021` | §4.6 langkah 9 | `BE-NV-006` | `FE-NV-006` | `NV-AC-009`, `017` | belum |
| `FR-NV-018` hanya lantai sendiri | `NV-DEC-009` | permission §3 | `BE-NV-003`, `BE-NV-005` | `FE-NV-003` | `NV-AC-015`, `UAT-06` | belum |
| `FR-NV-020` ganti-seluruh interval | `NV-DEC-010` | api §2, `VAL-NV-09`–`13` | `BE-NV-004` | `FE-NV-004` | `UAT-08` | belum |
| `FR-NV-021` tanda lewat jadwal | `NV-DEC-010` | api §3 `IsOverdue` | `BE-NV-005` | `FE-NV-005` | `NV-AC-005`, `UAT-07` | belum |
| `FR-NV-022` aman-gagal tanpa interval | `NV-DEC-010`, `G-01` | api §2 `IsConfigured` | `BE-NV-004`, `BE-NV-005` | `FE-NV-004`, `FE-NV-005` | `NV-AC-004` | belum |
| `FR-NV-030` muat ulang 60 detik | `NV-DEC-023`, `026` | api §3 `/current` | `BE-NV-003` | `FE-NV-005` | `NV-AC-014`, `020`, `UAT-09` | belum |
| `FR-NV-031` pita data basi | `NV-DEC-014` | fe §5 | — | `FE-NV-005` | `NV-AC-006`, `UAT-10` | belum |
| `FR-NV-032` push TTV | `NV-DEC-023`, `027` | api §4 | `BE-NV-007` | `FE-NV-007` | `NV-AC-019`, `022` | belum |
| Regresi login (syarat lead) | `NV-DEC-030` | §4.10, fe §7 | `BE-NV-003` | `FE-NV-003` | `NV-AC-023`–`027`, `UAT-11` | belum |
| Registry `Nvw` | `NV-DEC-029` | §5 | `BE-NV-001` | — | baris registry ditulis; QBE Strict `PASS`; [`BE-NV-001.md`](../task/report/backend/BE-NV-001.md) | ✅ 2026-10-28 — migration `20261028082048_AddNurseViewManagement` diterapkan |

## 2. Acceptance criteria tanpa task — coverage gap

| Butir | Sebab | Penanganan |
|---|---|---|
| `G-06` prasyarat data go-live (`FloorCode`, versi instrumen `Approved`, `OrganizationUnitId` bangsal) | Pekerjaan data/operasional, bukan kode | Daftar periksa go-live sebelum UAT; dicatat di laporan `FE-NV-005` sebagai kondisi uji |
| `NV-OQ-010` angka interval sah | Kebijakan klinis Komite Keperawatan | Langkah operasional sesudah `FE-NV-004`; tanpa angka papan aman-gagal |
| Pemberian hak akses peran Admin IT | Konfigurasi Akses Role, bukan kode | Langkah operasional sesudah `BE-NV-002`/`BE-NV-004`; tambahkan, jangan timpa policy peran |
| `NFR-002` (±2 detik untuk 40 bed) | Butuh data uji sebanyak itu | Diukur di `BE-NV-005` bila data cukup; bila tidak, dicatat sebagai belum terbukti, bukan lulus |
| `G-02` papan sebagai alat bantu | Materi pelatihan/UAT | Dicantumkan di skenario UAT oleh tim UAT |

## 3. Dokumen lintas modul

| Task | Dokumen | Kewajiban |
|---|---|---|
| `BE-NV-006` | `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001` permintaan 2 | Laporan task dirujuk dari folder TLM; status TLM diperbarui |
| `BE-NV-007` | `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001` permintaan 1 | Idem |
| `BE-NV-005` | `rawat-inap/episode-rawat-inap/docs/task-lintas-modul/TLM-NV-001` | Hanya baca; tidak ada perubahan kode Rawat Inap |
