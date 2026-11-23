# Laporan Perubahan Frontend — `FE-HRD-401`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-401` |
| Nomor lama | `FE-HRD-393` — dinomori ulang saat merge `QuilvianIntegrationBackend` 23 November 2026; nomor lama kini milik amandemen `S-A7-V2` (lihat `blueprint-manifest.md` revisi `69`) |
| Judul | Bagian disiplin Employee 360 dari `disciplinaryRecords` |
| Slice | `S-PKB` / `MVP-75` · `EPIC HRD-129` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 31 |
| Trace | `FR-PKB-14`, `FR-PKB-15`, `FR-PKB-16`; `HRD-DEC-238` butir 3, `HRD-DEC-246`; API 36.3; `VR-PKB-09`, `VR-PKB-10` |
| Contract version | `v30`, disetujui 23 November 2026 |
| Wewenang UI | Bagian existing Employee 360; komposisi base component existing (frontend 35.2) |
| Dependency | `BE-HRD-453` ✅ |
| Klasifikasi | `STANDARD` — satu view existing, tanpa endpoint baru |
| Task mode | `CROSS-REPO` (source frontend; laporan dan tanda status di repository backend) |
| Target tulis | `employee-360-view.jsx`, spec Playwright Paket B; laporan, roadmap, traceability HRD |
| Model | Claude Opus 5.5 (melanjutkan sesi Codex GPT-5 yang terhenti karena batas pemakaian) |
| Commit frontend saat dikerjakan | `6c05df212` (branch `HamzahV2`); perbaikan sesi ini di-commit pengguna sebagai `6c1d4e368` |
| Commit backend yang dijadikan rujukan | `a9c1af31` (branch `MHamzah`) |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Sesi sebelumnya sudah mengubah `employee-360-view.jsx` lalu terhenti saat uji browser headed berjalan. Pengguna meng-commit perubahan itu bersama layar Paket B lain sebagai `6c05df212`, sehingga sesi ini mulai dari source yang sudah memakai `disciplinaryRecords`, tanpa bukti build dan browser.

Saat uji headed diulang, skenario atasan gagal: halaman berpindah ke `/login`. Trace Playwright menunjukkan `GET /api/v1/system/features` dan `GET /api/v1/auth/permissions` tidak dimock sehingga diteruskan ke backend lokal yang sedang menyala (`https://localhost:7184`), dijawab `401`, lalu `POST /auth/refresh` gagal dan sesi uji dikeluarkan. Ini cacat skenario uji, bukan cacat layar; skenario Paket B sebelumnya lolos karena saat itu backend lokal tidak menyala.

---

## 2. Proses bisnis dari sisi pengguna

Petugas HR Relations membuka Employee 360 dr. Dodi, lalu tab **Pribadi & Disiplin (Rahasia)**. Tabel menampilkan dua surat peringatan alur resmi (nomor, tingkat, tanggal terbit, berlaku sampai, status) dan satu catatan profil lama berlabel "Catatan lama" tanpa tombol apa pun.

Atasan langsung yang membuka tab yang sama melihat "Data disiplin hanya untuk HR." di tempat tabel. Pengguna HR tanpa hak `EmployeeDisciplinary : ViewSanctions` (backend mengirim `disciplineHidden = true`) melihat pesan yang sama, bukan daftar kosong.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/components/view/hr/workforce-core/employee-360-view.jsx` (diff `6c05df212`)
- DTO backend `Employee360DisciplinaryItemDto` dan `Employee360ConfidentialDto` (`Areas/Corporate/HumanResource/WorkforceCore/DTOs/Employee360Dtos.cs`)
- Kontrak `v30` API 36.3 dan frontend 35.2
- `src/components/features/base-features/data-table.jsx` (tanda tangan `rowKey(item, index)`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan | Status Git |
| --- | --- | --- |
| `src/components/view/hr/workforce-core/employee-360-view.jsx` | Kolom tabel disiplin dari `disciplinaryRecords`; baris `LEGACY` berlabel "Catatan lama" dan "Arsip" tanpa aksi; pesan "Data disiplin hanya untuk HR." bila `disciplineHidden` atau mode Atasan Langsung; ruas usang `disciplinarySanctions` tidak dibaca lagi. | Di-commit `6c05df212` |
| `src/components/view/hr/workforce-core/employee-360-view.jsx` | `rowKey` memakai indeks baris agar dua catatan lama berjudul sama tidak bentrok. | Di-commit `6c1d4e368` |
| `tests/e2e/human-resource-package-b.spec.mjs` | `beforeEach`: route cadangan tingkat context untuk `**/api/v1/**` agar panggilan yang tidak dimock tidak jatuh ke backend asli; dua skenario `FE-HRD-401` diberi mock `/auth/permissions`; skenario ketiga untuk `disciplineHidden = true`; screenshot untuk ketiganya; NIK dikosongkan dari data mock. | Di-commit `6c1d4e368` |

Route cadangan di `beforeEach` berlaku untuk seluruh skenario di berkas spec. Route tingkat page milik skenario selalu didahulukan Playwright, sehingga skenario lain tidak berubah perilakunya.

### 3.3 Kepatuhan arsitektur frontend

Tidak ada service, hook, Redux, atau CSS baru. Data tetap dari satu panggilan `Employee360Service.getEmployee360`, sehingga keadaan memuat dan galat bagian disiplin mengikuti `BaseDetailView` yang sama dengan bagian lain.

### 3.4 Gerbang tampilan

**Modul referensi visual:** bagian Riwayat SK pada layar Employee 360 yang sama (tabel `DataTable` di dalam `BaseDetailSection`).

| Elemen | Sumber | Keputusan | Alasan |
| --- | --- | --- | --- |
| Tabel riwayat disiplin | `DataTable` + `tableStyles.regionDataTable` | `REUSE` | Pola tabel Riwayat SK di layar yang sama. |
| Tingkat dan status sanksi | `HrStatusBadge` + `SANCTION_LEVEL`, `SANCTION_STATUS` | `REUSE` | Peta label yang sama dengan layar Sanksi (`FE-HRD-403`). |
| Penanda "Catatan lama" dan "Arsip" | `StatusBadge` (`status="info"`, `status="inactive"`) | `REUSE` | Nilai prop sah menurut katalog. |
| Pesan disembunyikan | `InformationAlert` `variant="warning"` | `REUSE` | Sama dengan pesan akses terbatas di bagian lain Employee 360. |

`UI GATE: PASS — 4 REUSE, 0 EXTEND, 0 COMPOSE, 0 NEW.`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs src/components/view/hr/workforce-core/employee-360-view.jsx
OK — 1 berkas diperiksa, 0 temuan.
```

Grep anti-regresi (`ui-consistency-checklist.md` bagian G) pada `employee-360-view.jsx`:

```text
<button / btn-*            : 0
<table                     : 0
fw-* / fs-*                : 0
class Tailwind             : 0
fixed inset-0              : 0
<input|select|textarea>/UUID: 1 — baris 3, komentar lama "tanpa UUID yang tampil" (bukan elemen)
variant tidak sah          : 0
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Employee 360 — akun HR | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/FE-HRD-393-employee-360-discipline.png` | SP-001 Kedaluwarsa, SP-002 Dicabut Lewat Banding, satu baris Catatan Lama; tanpa NIK. |
| Employee 360 — Atasan Langsung | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/FE-HRD-393-employee-360-atasan.png` | "Data disiplin hanya untuk HR." di tempat tabel. |
| Employee 360 — HR tanpa `ViewSanctions` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/FE-HRD-393-employee-360-hr-tanpa-hak.png` | Pesan yang sama; teks "Tidak ada catatan disiplin." tidak muncul. |

Folder `QuilvianSystemFrontendDev/test-with-agy/` di-ignore Git (aturan skill `live-browser-testing`). Screenshot tidak disimpan di `test-results/` karena Playwright mengosongkan folder itu setiap kali dijalankan.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat / galat | Mengikuti `BaseDetailView` Employee 360 (satu panggilan untuk seluruh bagian). |
| Ada catatan | Tabel; baris `OFFICIAL` dengan nomor, tingkat, terbit, berlaku s.d., status. |
| Catatan lama | Lencana "Catatan lama", judul catatan, status "Arsip", tanpa tombol. |
| Kosong (HR berhak) | "Tidak ada catatan disiplin." |
| `disciplineHidden = true` | "Data disiplin hanya untuk HR." |
| Mode Atasan Langsung | Pesan akses terbatas bagian pribadi + "Data disiplin hanya untuk HR." |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Workforce Core / Employee 360

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employee-360` | `confidentialPersonalAndDiscipline.disciplinaryRecords`, `disciplineHidden` | `HrdEmployee360 : Read`; isi disiplin bergantung `EmployeeDisciplinary : ViewSanctions` |

---

## 6. Verifikasi

| Skenario/perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run build` (sesudah perbaikan `rowKey`) | Exit code `0`; 893 halaman statis | `PASS` | Production build |
| `npm run lint:errors` | Exit code `0` | `PASS` | ESLint seluruh repository |
| Pemeriksa CSS | 1 berkas, 0 temuan | `PASS` | Bagian 3.4 |
| Playwright Chromium headed, run pertama | 1 lulus, 1 gagal (dialihkan ke `/login`) | `FAIL` — cacat skenario | Bagian 1 |
| Playwright Chromium headed, sesudah perbaikan skenario | 3/3 lulus | `PASS` | HR, Atasan Langsung, HR tanpa `ViewSanctions` |

`AUTOMATED TEST: npx playwright test tests/e2e/human-resource-package-b.spec.mjs --grep "FE-HRD-393" --headed --workers=1 — PASS (3/3)`

`AUTOMATED TEST: SKIPPED (opsional) — test unit util pemetaan tidak ditulis karena tidak ada util; pemetaan kolom ada di definisi kolom view.`

`MANUAL TEST: PASS` lewat Chromium headed terotomasi; screenshot diperiksa satu per satu.

**Tidak dijalankan:** akun nyata per peran dan DB dev. Response dimock mengikuti DTO backend dan kontrak 36.3; uji dengan akun nyata adalah cakupan `FE-HRD-409` (gerbang G3 dan G4).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC1 — HR Relations: 2 `OFFICIAL` + 1 `LEGACY`; `LEGACY` berlabel "Catatan lama" tanpa tombol | Terpenuhi | Skenario headed 1; `legacyRow.getByRole("button")` = 0; screenshot |
| AC2 — Atasan langsung: "Data disiplin hanya untuk HR." di tempat daftar | Terpenuhi | Skenario headed 2; skenario 3 untuk `disciplineHidden` |
| AC3 — Layar tidak lagi membaca `disciplinarySanctions` | Terpenuhi | Pencarian `src`: tidak ada rujukan `disciplinarySanctions` |
| AC4 — Memuat dan galat mengikuti bagian lain Employee 360 | Terpenuhi | Satu sumber data dan `BaseDetailView` yang sama (3.3) |
| Lint, build, uji browser, laporan, roadmap, traceability | Terpenuhi | Bagian 6 dan dokumen terkait |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Mode Atasan Langsung menampilkan dua peringatan bertumpuk (akses terbatas bagian pribadi, lalu pesan disiplin). Keduanya benar; bila dianggap terlalu ramai, pesan pertama dapat dihapus pada task lain. |
| Masalah yang diketahui | Screenshot `FE-HRD-402` s.d. `FE-HRD-406` yang dahulu dicatat di `test-results/human-resource-package-b/` terhapus oleh run Playwright berikutnya. Seluruh spec Paket B dijalankan ulang (15/15 lulus) dan screenshot-nya disalin ke `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/`; tautan di kelima laporan itu sudah diganti. |
| Dependency backend | `BE-HRD-453` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi sebelumnya terhenti karena batas pemakaian; dilanjutkan dari `6c05df212` tanpa mengulang pekerjaan yang sudah ada. |
| Status Git | Di-commit pengguna sebagai `6c1d4e368` (23 November 2026, 15.17 WIB). |
| Langkah berikutnya | `FE-HRD-407`, `FE-HRD-408`, lalu `FE-HRD-409`. |
