# Traceability Requirement — Dokter Rawat Inap, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Berkas | `dokter-rawat-inap/roadmap/requirement-traceability-finishing.md` — revision `1` |
| Status | **`DRAFT`**, mengikuti status kedua roadmap pendamping |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `dokter-rawat-inap`, kontrak `0.7.0` `approved` 2026-10-02 (`RWI-DEC-221`) |
| Roadmap | `backend-roadmap-finishing.md` revision `1` (`BE-RWI-160` s.d. `164`); `frontend-roadmap-finishing.md` revision `1` (`FE-RWI-172` s.d. `179`) |
| Sumber requirement | `PRD-RWI-FINISHING-001` v`0.4` (rumusan FR); `04-prd-to-mvp.md` bagian 23; decision log revision `32` |
| Masukan dan hash | Seperti metadata roadmap; hash lengkap pada `../blueprint-manifest.md` bagian 10 |
| Source SHA | Backend `bf5c6bde`; frontend `f74758af5` |
| Gate | `evidence/02-requirement-completeness-gate.md` revision `1.10` bagian 19 dan koreksi 19.12 |

**Cara membaca bukti.** Backend: verifikasi API/kontrak, pencarian kode, verifikasi proses bisnis, dan runtime bila tersedia — bukan automated test (`rules/backend/TEST_POLICY.md`). Frontend: lint, build, dan verifikasi manual kontrol interaktif. Butir yang tidak dijalankan ditulis `NOT RUN`.

## 1. Matriks requirement → desain → kontrak → task → bukti

| Requirement | Keputusan | Desain | Kontrak `0.7.0` | Task backend | Task frontend | Bukti verifikasi | Status |
|---|---|---|---|---|---|---|---|
| `FR-RWF-030` Perawat memesan Lab/Radiologi atas instruksi dokter | `RWI-DEC-114`, `153`, `168` | Backend 12.1 | API 13.1 | `BE-RWI-104` ✅ (`backend-roadmap-v2.md`; regresi `NOT RUN`) | `FE-RWI-172` | Runtime: pesanan masuk worklist Lab (`AC-RWF-030`); tanpa dokter ditolak (`AC-RWF-031`, `UAT-RWF-04`) | Belum dikerjakan |
| `FR-RWF-031` Konsultasi Gizi lewat modul Gizi | `RWI-DEC-171` | Backend 12.3–12.5 | API 13.2, 13.3 | `BE-RWI-161`, `BE-RWI-164` | `FE-RWI-174` | Pesanan tampil di Gizi, peminta akun dokter (`AC-RWF-032`, `UAT-RWF-07`) | Belum dikerjakan |
| `FR-RWF-032` Pesanan darah lewat modul Bank Darah | `RWI-DEC-171` | Backend 12.3–12.5 | API 13.2, 13.4 | `BE-RWI-162`, `BE-RWI-164` | `FE-RWI-174` | Pesanan `Pending`, penginput perawat (`AC-RWF-033`, `UAT-RWF-08`) | Belum dikerjakan |
| `FR-RWF-033` Status dan hasil dibaca dari modul pemilik | `RWI-DEC-171` (7) | Backend 12.11 | Endpoint baca modul pemilik (sudah ada) | — (`EXISTING / REUSE`) | `FE-RWI-173`, `FE-RWI-174` | Manual: daftar pesanan dan hasil dari modul pemilik | Belum dikerjakan |
| `FR-RWF-034` Status tanggungan dan harga saat memilih pemeriksaan | `RWI-DEC-218`, `219` (menggantikan tafsiran G-05) | Backend 12.13 | API 13.2 `coverage-status` | `BE-RWI-163` | `FE-RWI-172`, `173`, `176`, `177`; Pemesanan Ruangan Bedah: `episode-rawat-inap` `FE-RWI-193` | API dua akun (`NOT_PERMITTED` tanpa field harga); `RWI-AC-335`, `337`; `UAT-RWF-36`, `37` | Belum dikerjakan |
| `FR-RWF-035` Rehab Medik *placeholder*; Hemodialisa tetap | `RWI-DEC-108`; `RWI-DEC-222` (menutup `DEC-INP-019`) | `04-prd-to-mvp.md` 23.8 | — | — | `FE-RWI-179` | Pencarian kode: nol `procedureId` buatan; kartu kembali 'Integrasi belum tersedia' (`RWI-AC-341`, `342`) | Belum dikerjakan |
| `FR-RWF-036` Aturan pemesan seragam | `RWI-DEC-171`, `188` | Backend 12.2 (`INV-RWF-20`, `21`) | API 13.2; validasi `VAL-RWF-60`, `61`, `65` | `BE-RWI-164`; Lab/Rad: `BE-RWI-104` ✅ | `FE-RWI-172`, `FE-RWI-174` | Dokter tanpa penugasan → 403, tanpa pesanan di modul tujuan (`AC-RWF-034`, `UAT-RWF-30`) | Belum dikerjakan |
| `FR-RWF-037` Verifikasi dokter | `RWI-DEC-188`, `191` | Backend 12.2 (`INV-RWF-22`, `23`) | API 13.3, 13.4; validasi `VAL-RWF-63`, `64` | `BE-RWI-161`, `BE-RWI-162`; diet: `keperawatan` `BE-RWI-166` | `FE-RWI-175` | Verifikasi tersimpan di Bank Darah (`AC-RWF-035`); dokter lain 403 | Belum dikerjakan |
| `FR-RWF-038` Pesanan bukan tagihan; pesanan ganda ikut modul pemilik | `RWI-DEC-171` (5) | Backend 12.2 (`INV-RWF-24`) | API 13.2 (`confirm-duplicate`) | `BE-RWI-164` | `FE-RWI-174` | Tidak ada baris tagihan saat pesan; regresi poliklinik (`AC-RWF-036`) | Belum dikerjakan |
| `FR-RWF-070` Katalog tindakan rawat inap | `RWI-DEC-165` (1) | Backend 12.5, 12.6 | API 13.5 | `BE-RWI-160` | `FE-RWI-176` | Tampil di bangsal, tidak di poliklinik (`AC-RWF-070`, `UAT-RWF-31`); pemanggil lama identik | Belum dikerjakan |
| `FR-RWF-081`, `082` (sisi dokter) Ringkasan operasi di ruang kerja dokter | `RWI-DEC-213` | Backend 12.13 (penanda klinis) | `episode-rawat-inap` API 11 (`post-operative-summary`) | `episode-rawat-inap` `BE-RWI-180` | `FE-RWI-178` | Penanda hanya pada pasien ber-kasus `Completed`; delapan tab tetap (`RWI-AC-339`, `UAT-RWF-38`) | Belum dikerjakan |
| `NFR-RWF-10` Pemeriksaan penugasan gagal tertutup untuk pesanan darah | `RWI-DEC-171` | — | API 13.2 | `BE-RWI-164` | — | Konteks penugasan tidak terbaca → ditolak (`INT-RWF-16`) | Belum dikerjakan |
| `NFR-RWF-11` Daftar verifikasi tetap tampil sebagian | `RWI-DEC-188` | FE 11.4 | — | — | `FE-RWI-175` | Satu sumber gagal, lima tampil | Belum dikerjakan |

## 2. Definition of Done PRD → bukti

| Butir DoD (`04-prd-to-mvp.md` 23.19) | Task | Bukti |
|---|---|---|
| Gizi dan Bank Darah tidak lagi *placeholder* | `BE-RWI-164`, `FE-RWI-174` | `UAT-RWF-07`, `08` |
| Pesanan perawat selalu punya dokter berpenugasan dan terverifikasi | `BE-RWI-161`, `162`, `164`; `FE-RWI-175` | `UAT-RWF-08`, `30`; `INV-RWF-23` lewat verifikasi API |
| Alur pesanan poliklinik tidak berubah | `BE-RWI-161`, `BE-RWI-162` | Verifikasi proses bisnis regresi `AC-RWF-036` |
| Katalog rawat inap benar | `BE-RWI-160`, `FE-RWI-176` | `UAT-RWF-31` |

## 3. Gerbang rilis implementasi

| Temuan | Task | Status |
|---|---|---|
| IMP-RWF-01, IMP-RWF-02 (Gizi, Bank Darah) | `FE-RWI-174` | Belum dikerjakan |
| IMP-RWF-03 (Rehab Medik) | `FE-RWI-179` | Belum dikerjakan (Bebas Blokir — `DEC-INP-019` ditutup `RWI-DEC-222`) |
| IMP-RWF-05 (Lab/Radiologi harga tetap dan katalog contoh) | `FE-RWI-173` | Belum dikerjakan |
| IMP-RWF-06 (order tindakan "Ditanggung" bawaan dan harga `0`) — temuan perencanaan 2 Oktober 2026 | `FE-RWI-176` | Belum dikerjakan |

## 4. Gap dan catatan

| ID | Gap | Penanganan | Pemilik |
|---|---|---|---|
| `DEC-INP-019` | Rehab Medik dibuka atau tetap *placeholder* (KK-3 vs `RWI-DEC-108`/`FR-RWF-035`) | **TERTUTUP** lewat `grill-me` 2026-10-02 (`RWI-DEC-222`: tetap *placeholder*); `FE-RWI-179` unblocked | Muhammad Hamzah |
| IMP-RWF-04 | Daftar pemicu capability map 19.9 tidak lengkap | `trace-existing-capabilities` impact scan ruang kerja dokter; bukan task delivery | Muhammad Hamzah |
| IMP-RWF-06 | Belum tercatat di gate `1.10` | Dicatat di sini; dimasukkan pada evaluasi gate berikutnya | Muhammad Hamzah |
| — | Regresi `BE-RWI-104` `NOT RUN`, padahal `RWI-DEC-168` mensyaratkan "terbukti berjalan" | Langkah pertama `FE-RWI-172` | Muhammad Hamzah |
| — | Register 🟡 dan kartu ✅ berbeda untuk `FE-RWI-143` s.d. `146` di `frontend-roadmap-v2.md` | Dicatat untuk pemilik roadmap itu; tidak diubah di sini | Muhammad Hamzah |
| — | `BE-RWI-128` dipakai dua task (`backend-roadmap-v2.md` dan `integrasi-billing/roadmap/backend-roadmap.md`) | Dicatat pada manifest modul (`task_id_integrity`) | Pemilik peta modul |

Tidak adanya automated test backend **bukan** gap (`rules/backend/TEST_POLICY.md`).
