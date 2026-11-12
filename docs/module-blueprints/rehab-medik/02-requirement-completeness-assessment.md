# Rehab Medik — Penilaian Kelengkapan Requirement

| Field | Nilai |
|---|---|
| Blueprint ID | `RHB-BP-001` |
| Revision bukti | Decision log r1 (RHB-DEC-001..021), capability map r1 |
| Backend / Frontend SHA | `6bb3fba7` / `0c06c093` |
| Tanggal | 5 November 2026 |
| Domain architecture | `DOMAIN_ARCHITECTURE_NOT_RUN`. Alasannya: pola agregat sudah teruji di Hemodialisa (permintaan → episode → resep/program → sesi). Master data bersama hanya dibaca, tidak dimiliki. Billing dan keselamatan klinis sudah diputuskan lewat RHB-DEC-005, 007, 013, 019, dan 021. |
| Baseline rujukan | Tidak memakai `indonesia-hospital-domain-reference`. Sumber bisnis berupa `rehab-medik-deep-analysis.md` (regulasi SRC-001..014) dan keputusan owner. |

## 1. Scope penilaian

Rilis 1 Rehab Medik = Phase 1 tanpa FEAT-011, ditambah FEAT-032. Penilaian dipecah per slice
agar satu gap tidak memblokir seluruh modul.

| Slice | Feature |
|---|---|
| S1 Permintaan, validasi, disposisi | FEAT-001, 002, 003 |
| S2 Episode dan asesmen (Sp.KFR + profesi) | FEAT-007, 008, 009, 010 |
| S3 Tujuan, program, edukasi/persetujuan | FEAT-012, 013, 015 |
| S4 Sesi terapi, keselamatan, deviasi | FEAT-016, 017, 018 |
| S5 Reasesmen dan keputusan | FEAT-024, 025 |
| S6 Ringkasan akhir dan tindak lanjut | FEAT-026, 027 |
| S7 Finalisasi dan addendum | FEAT-028, 029 |
| S8 Handoff billing | FEAT-032 |

## 2. Bukti yang dipakai

1. Keputusan owner (`00-interview-decisions.md`): RHB-DEC-001..021. Wewenangnya **tingkat 1**
   (requirement eksplisit owner). Butir klinis tetap butuh sign-off pengesah klinis untuk produksi.
2. Sumber bisnis `rehab-medik-deep-analysis.md`: aturan bisnis dan acceptance criteria
   per FEAT, dari regulasi resmi (Permenkes 65/2015, 76/2014, 81/2014, 24/2022, KMK 1596/2024).
3. Bukti implementasi V2 (`01-existing-capability-map.md`) untuk hal yang sudah ada.

## 3. Temuan per dimensi

Keterangan: **C** = `CONFIRMED`, **P** = `PROPOSED`, **M** = `MISSING`, **X** = `CONFLICT`.

| Dimensi | S1 | S2 | S3 | S4 | S5 | S6 | S7 | S8 |
|---|---|---|---|---|---|---|---|---|
| 01 Tujuan | C | C | C | C | C | C | C | C |
| 02 Aktor | C (DEC-002, §3) | C (DEC-005, 011) | C (DEC-011) | C | C | C | C (DEC-007) | C (DEC-021) |
| 03 Pemicu/prasyarat | C (DEC-008) | C (DEC-005) | C (INV-03) | C (INV-04, DEC-009) | C | C | C | C |
| 04 Alur utama | C | C | C | C | C | C | C (map §2) | C (pola HD) |
| 05 Exception | C (FEAT-002/003) | C | C | C (FEAT-018) | C | P (putus layanan, OQ-005) | C | C |
| 06 Data minimum | C (FEAT-001 AC) | P (instrumen per DEC-017) | C | C | C | C | C | P (kode tindakan, OQ-016) |
| 07 Aturan bisnis | C (INV-01, 02, 07) | C (INV-03, 09) | C (INV-10) | C (INV-04, 06) | C (FEAT-024) | C (FEAT-026) | C (INV-05) | C (INV-06) |
| 08 Status | C (§5) | C | C | C | C | C | C (map §2) | C |
| 09 Authorization | C (INV-02) | C (INV-08) | C (DEC-011) | C | C | C | C | C |
| 10 Dependency antarmodul | C (DEC-016) | C | C | C (DEC-013) | C | C | C (DEC-019) | P (OQ-015) |
| 11 Integrasi | Tidak berlaku: tidak ada sistem luar di rilis 1 | — | — | — | — | — | — | Internal Billing saja |
| 12 Hasil akhir | C | C | C | C | C | C | C | C |
| 13 Batal/koreksi | C (`cancel`, `return`) | C (addendum) | C (versi program) | C (addendum) | C | C | C | C (void sumber, pola HD) |
| 14 Audit | C | C | C | C | C | C | C | C |
| 15 Notifikasi | Tidak berlaku material di rilis 1: tidak ada notifikasi otomatis yang diminta; pengingat jadwal adalah Phase 2 | | | | | | | |
| 16 Billing | Tidak berlaku | Tidak berlaku | Tidak berlaku | C (DEC-021) | Tidak berlaku | Tidak berlaku | Tidak berlaku | C |
| 17 Keselamatan klinis | C | C (DEC-005) | C (refusal menghentikan) | C (FEAT-016, 018) | C | C | C | — |
| 18 Traceability | C | C | C | C | C | C | C | C |

## 4. Gap material dan dampaknya

| ID | Gap | Status | Dampak | Keterangan |
|---|---|---|---|---|
| G-01 | Definisi "putus layanan" (jumlah upaya kontak) | P | `CONFIGURABLE_DEFAULT` | Rilis 1 hanya mencatat alasan penutupan `LostToFollowUp` beserta catatan. Tidak ada aturan otomatis. |
| G-02 | COPM, MMT/ROM, dan TADIR belum bisa dimuat mesin instrumen | P | `NON_BLOCKING_STANDARD` | Rilis 1 mencatatnya sebagai pengukuran terstruktur (DEC-017). |
| G-03 | Isi checklist keselamatan sesi per intervensi | P | `CONFIGURABLE_DEFAULT` | Rilis 1 memakai lima butir umum dari FEAT-016: identitas, program aktif, kondisi terkini, kontraindikasi, dan keputusan lanjut/tunda/henti. Butir khusus per intervensi menyusul. |
| G-04 | Jenis tindakan yang butuh persetujuan khusus (FEAT-015) | P | `CONFIGURABLE_DEFAULT` | Rilis 1 mencatat edukasi + status persetujuan per program. Formulir *informed consent* hukum tetap milik modul consent existing. |
| G-05 | Persetujuan pemakaian `TrxPatientProcedure` + fakta charge (S8) | M | `BLOCKING` — **hanya untuk implementasi S8**, bukan desain | DEC-RHB-G01 sudah diputuskan (DEC-021). Sisanya izin lintas modul (OQ-015). |
| G-06 | Bukti tertulis persetujuan Rekam Medis (S7) | M | `BLOCKING` — **hanya untuk menandai ✅ task S7** | OQ-012 |
| G-07 | `TrxPatientProcedure.DoctorId` wajib, sedangkan terapis bukan dokter | P | `NON_BLOCKING_STANDARD` | Usulan: Sp.KFR penanggung jawab episode dicatat sebagai `DoctorId` dan `InstructingDoctorId`, sedangkan terapis sebagai pelaksana di sesi rehab. Dampaknya ke jasa medis milik modul Medical Fee dan **tidak** dirancang di sini. |

Tidak ada `CONFLICT` yang tersisa. C-20 (CPPT) diselesaikan oleh DEC-013.

## 5. Kesiapan per slice

| Slice | Kesiapan | Catatan |
|---|---|---|
| S1–S6 | `READY_FOR_DOMAIN_DESIGN` | — |
| S7 | `READY_FOR_DOMAIN_DESIGN` | Implementasi menunggu bukti OQ-012 sebelum ✅ |
| S8 | `READY_FOR_DOMAIN_DESIGN` | Implementasi menunggu OQ-015 |

**Kesimpulan:** seluruh slice rilis 1 boleh masuk `design-business-module`. Domain architecture
dilewati (`DOMAIN_ARCHITECTURE_NOT_RUN`) dengan alasan di kepala dokumen.
