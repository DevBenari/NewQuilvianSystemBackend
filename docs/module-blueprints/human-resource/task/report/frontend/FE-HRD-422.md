# Laporan Perubahan Frontend — `FE-HRD-422`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-422` |
| Nomor lama | `FE-HRD-414` (dinomori ulang saat merge `QuilvianIntegrationBackEnd` 27 November 2026 agar `FE-HRD-414` milik `A52-B` tidak berubah) |
| Judul | Detail unit menampilkan nama, bukan ID |
| Slice | `S-FIN-M05` amandemen — Menu Organisasi (`PLAN-REPAIR-HRD-008`, `FIX-HRD-008-07`) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` bagian 33 (nomor lama bagian 32) |
| Trace | `ISS-HRD-008-T3`; aturan output 5.3; skema `PLAN-REPAIR-HRD-008` 3.6 |
| Contract version | Ruas aditif `legalEntityName`, `hospitalSiteName`, `departmentName` dari `BE-HRD-469` (nomor lama `BE-HRD-463`) |
| Wewenang UI | Susunan detail sesuai skema 3.6 |
| Dependency | `FE-HRD-421` ✅ (lama `FE-HRD-413`), `BE-HRD-469` ✅ (lama `BE-HRD-463`) |
| Klasifikasi | `LIGHT` — 1 berkas konfigurasi |
| Task mode | `FRONTEND` (disetujui pengguna 24 November 2026) |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `fe8aebf2f` (branch `HamzahV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `3ef27c96` + perubahan `BE-HRD-469` (lama `BE-HRD-463`, belum di-commit) |
| Tanggal | 2026-11-24 |
| Status | ✅ Selesai — 2/2 AC dengan respons API dimock sesuai DTO `BE-HRD-469`. Tampilan dengan backend nyata menunggu backend lokal di-restart |

---

## 1. Keadaan yang ditemukan di awal

`detailFields` unit menampilkan "ID Legal Entity", "ID Fasilitas Rumah Sakit", "ID Parent Unit", dan "ID Departemen" berisi UUID (`organization-unit-constants.jsx:377-396` pada `fe8aebf2f`), karena respons server belum membawa nama ketiga relasi itu.

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka detail unit, dari daftar (klik dua kali) atau dari pohon ("Buka Detail Unit").
2. Bagian Informasi Utama menampilkan Unit Induk, Legal Entity, Fasilitas Rumah Sakit, dan Departemen Terkait dengan **nama**.
3. Unit tanpa fasilitas atau departemen menampilkan "-".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`organization-unit-constants.jsx` (`detailFields`), `organization-unit-utils.jsx` (`buildDetailRows`), `OrganizationUnitDtos.cs` setelah `BE-HRD-469`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/master-data/organization-unit/organization-unit-constants.jsx` | Baris ID diganti "Legal Entity", "Fasilitas Rumah Sakit", "Departemen Terkait" dari `legalEntityName`, `hospitalSiteName`, `departmentName`; baris "ID Parent Unit" dibuang; "Parent Unit" → "Unit Induk"; "Jumlah Child Unit" → "Jumlah Sub-Unit" |

### 3.3 Kepatuhan arsitektur frontend

Hanya konfigurasi `detailFields`; render tetap lewat `buildDetailRows` dan halaman detail bawaan.

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Kartu informasi detail | Halaman detail bawaan (`base-detail`) | `REUSE` | — |

`UI GATE: PASS — 1 elemen REUSE, 0 COMPOSE, 0 NEW`

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/hr/master-data/organization-unit/<token>` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/organization-menu-1124/FE-HRD-414-detail-nama.png` | Sesuai skema 3.6; tanpa UUID *(nama berkas historis `FE-HRD-414`)* |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Keadaan memuat halaman detail (perilaku lama) |
| Kosong | Relasi kosong → "-" |
| Gagal | Pesan gagal memuat detail (perilaku lama) |
| Tanpa hak akses | `AccessDeniedGate` (perilaku lama) |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Master Data / Organization Unit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/organization-units/{id}` | Detail unit beserta nama relasi | `OrganizationUnit : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | exit 0 | `PASS` | Keluaran perintah |
| `npm run build` | Sukses, 927 halaman | `PASS` | Log build |
| Playwright › FE-HRD-414 *(deskripsi spec historis)* (respons detail dimock dengan ruas `BE-HRD-469`) | Lulus: Legal Entity, Fasilitas, Departemen Terkait tampil dengan nama; teks halaman di luar sidebar tanpa pola UUID dan tanpa "ID Departemen"/"ID Legal Entity" | `PASS` | 5/5 spec lulus |
| Tampilan dengan backend nyata | Tidak dijalankan | `NOT RUN` | Backend lokal yang berjalan masih build lama tanpa `BE-HRD-469`; perlu restart |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Detail unit menampilkan nama departemennya; tidak ada UUID di halaman | Terpenuhi | Playwright FE-HRD-414; screenshot |
| 2. Unit tanpa fasilitas menampilkan "Fasilitas Rumah Sakit: -" | Terpenuhi di source | `buildDetailRows` → `safeString("")` menampilkan "-", sama dengan baris kosong lain |
| DoD: laporan dan tanda roadmap | Terpenuhi | Laporan ini; roadmap frontend bagian 33 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | — |
| Dependency backend | `BE-HRD-469` ✅ *(nomor lama `BE-HRD-463`)* di source; backend lokal harus di-restart agar ruas nama terkirim. Sebelum itu ketiga baris menampilkan "-" |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `FE-HRD-420` *(lama `FE-HRD-412`)* |
| Langkah berikutnya | Restart backend lokal, buka detail satu unit yang punya departemen |
