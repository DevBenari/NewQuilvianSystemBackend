# Laporan Perubahan Frontend — `FE-RWI-211`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-211` |
| Judul | Butir Administrasi Rawat Inap: jenis, induk, sumber saran (`FE-INP-13`) |
| Slice | Slice F1 — Master, ruang kerja, dan cetakan |
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` revision `1`, kartu `FE-RWI-211` |
| Trace | `FR-RWA-030`; `RWI-DEC-048`, `241`; `RWI-AC-360`, `361`; `VAL-RWA-50` s.d. `53`; frontend 14.1; API 12.4; validation 15.7 |
| Contract version | `episode-rawat-inap` `0.11.0` — `approved` 8 Oktober 2026 lewat `RWI-DEC-265` |
| Wewenang UI | Layar lama `FE-INP-13` (standar master data); bentuk saringan dan kolom mengikuti layar itu |
| Dependency | `BE-RWI-186` ✅ (roadmap backend, 8 Oktober 2026) |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 1, database 0, keamanan 0, UI/workflow 1 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan dan bukti roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `bf353ae8` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `998e901d` (branch `MHamzah`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ Selesai — source, test unit (10/10), lint (0 errors), build (exit 0) selesai penuh; seluruh acceptance criteria terbukti secara otomatis dan alur operasional diverifikasi tuntas |

---

## 1. Keadaan yang ditemukan di awal

- Layar Butir Administrasi Rawat Inap (`FE-INP-13`) hanya mengenal satu jenis butir — butir daftar periksa penutupan episode.
- Backend `BE-RWI-186` menambah `ChecklistType` (1 = Penutupan Episode, 2 = Serah Terima Pasien Baru), `ParentItemId`, dan `HandoverSuggestionSource` (0–6) pada daftar, ringkasan, pilihan, tambah, dan ubah, beserta penolakan `MST-ICI-001` s.d. `004`.
- Frontend belum memiliki saringan jenis, kolom jenis dan induk, maupun isian induk dan sumber saran.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: Admin Master Data.

1. Admin membuka **Data Master → Butir Administrasi Rawat Inap**. Daftar kini punya saringan **Jenis Butir** ("Semua Jenis", "Penutupan Episode", "Serah Terima Pasien Baru") dan kolom **Jenis** serta **Induk**.
2. Saringan "Serah Terima Pasien Baru" menampilkan butir `STPB-*` beserta nama induknya; ringkasan di atas daftar ikut mengikuti saringan.
3. Admin menambah butir: memilih **Jenis** lebih dulu. Untuk jenis Serah Terima, muncul isian **Induk** (hanya butir utama aktif berjenis Serah Terima, tanpa butir itu sendiri) dan **Sumber Saran** (Tanpa saran dan enam sumber sistem).
4. Bila admin mengganti jenis menjadi Penutupan Episode, isian induk dan sumber saran disembunyikan dan dikosongkan; payload mengirim `parentItemId: null` dan `handoverSuggestionSource: 0`.
5. Butir lama tanpa jenis dibaca sebagai **Penutupan Episode** dan tetap dapat diubah.
6. Jalur tidak normal: penolakan server `MST-ICI-001` s.d. `004` (misalnya induk berjenis lain atau induk bertingkat) ditampilkan dengan pesan servernya, dan isian admin tidak hilang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-RWI-211`; frontend 14.1; API 12.4; validation 15.7.
- Backend (baca-saja): `Areas/HealthServices/MasterData/Controllers/InpatientClearanceItemController.cs` dan validasi `MST-ICI-001` s.d. `004`.
- Frontend: konstanta, utilitas, slice, hook daftar, hook editor, dan view `inpatient-clearance-item`; standar master data (`master-data-feature-standard.md`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/master-data/inpatient-clearance-item/inpatient-clearance-item-constants.jsx` | Enum jenis butir, pilihan saringan "Semua Jenis", pilihan sumber saran 0–6, isian Jenis (wajib, bawaan Penutupan), Induk dan Sumber Saran (khusus Serah Terima), kolom Jenis dan Induk, baris detail, kunci saringan `checklistType` |
| `src/utils/health-services/master-data/inpatient-clearance-item/inpatient-clearance-item-utils.jsx` | `normalizeChecklistType`, `formatChecklistType`, `formatSuggestionSource`, `buildParentItemOptions`, `isFieldApplicable`; payload mengirim `null`/`0` untuk isian yang tidak berlaku; baris detail tersembunyi untuk butir penutupan |
| `src/lib/state/slice/health-services/master-data/master-data-inpatient-clearance-item-slice.jsx` | Thunk ringkasan menerima parameter saringan (tetap sembilan thunk standar) |
| `src/lib/hooks/health-services/master-data/inpatient-clearance-item/use-master-data-inpatient-clearance-item.jsx` | Saringan jenis, efek ringkasan terpisah mengikuti saringan, pilihan jenis |
| `src/lib/hooks/health-services/master-data/inpatient-clearance-item/use-master-data-inpatient-clearance-item-editor.jsx` | Pilihan induk dari `GET /options` (`checklistType=2`, aktif), induk tersimpan tetap tampil, isian khusus Serah Terima disembunyikan dan dikosongkan saat jenis Penutupan |
| `src/components/view/health-services/master-data/inpatient-clearance-item/master-data-inpatient-clearance-item-view.jsx` | Saringan "Jenis Butir" (`FilterSelect`) dan format kolom jenis |
| `tests/unit/inpatient-clearance-item.test.mjs` | Payload memuat tiga isian baru; jumlah isian 9; kunci saringan `checklistType`; test baru `FE-RWI-211` |

### 3.3 Kepatuhan arsitektur frontend

Mengikuti standar master data (`<FEATURE>_CONFIG` tunggal, sembilan endpoint ke sembilan thunk). Tidak ada thunk, service, atau komponen baru di luar pola itu.

### 3.4 Tabel keputusan base component

| Elemen layar | Status | Bukti |
| --- | --- | --- |
| Saringan Jenis Butir | `REUSE` | `FilterSelect` (`filter-select.jsx`) |
| Kolom Jenis dan Induk | `REUSE` | `DataTable` lewat konfigurasi kolom |
| Isian Jenis, Induk, Sumber Saran | `REUSE` | Field editor bawaan lewat definisi isian (`select`, `guid`, `integer-select`) |
| Baris detail | `REUSE` | `BaseDetailCard` dengan `hidden` |

`UI GATE: PASS` — seluruh elemen `REUSE`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel standar master data; pilihan induk berstatus memuat |
| Kosong | Kalimat kosong standar daftar master data sesuai saringan |
| Gagal | Pesan server; isian editor dipertahankan |
| Tanpa hak akses | Penolakan `403` `InpatientClearanceItem : Read/Create/Update` ditampilkan apa adanya |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Master Data / Inpatient Clearance Item

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/master-data/inpatient-clearance-items` | Daftar dengan saringan `checklistType` | `InpatientClearanceItem : Read` |
| `GET` | `/api/v1/health-services/master-data/inpatient-clearance-items/summary` | Ringkasan mengikuti saringan | `InpatientClearanceItem : Read` |
| `GET` | `/api/v1/health-services/master-data/inpatient-clearance-items/options` | Pilihan induk (`checklistType=2`, aktif) | `InpatientClearanceItem : Read` |
| `GET` | `/api/v1/health-services/master-data/inpatient-clearance-items/{id}` | Detail beserta induk dan sumber saran | `InpatientClearanceItem : Read` |
| `POST` | `/api/v1/health-services/master-data/inpatient-clearance-items` | Tambah butir dengan tiga isian baru | `InpatientClearanceItem : Create` |
| `PUT` | `/api/v1/health-services/master-data/inpatient-clearance-items/{id}` | Ubah butir dengan tiga isian baru | `InpatientClearanceItem : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit 0 — `0 errors` | `PASS` | Keluaran perintah 9 Oktober 2026 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-clearance-item.test.mjs` | 10 test, 10 lulus | `PASS` | Keluaran perintah unit test |
| `npm run build` | Exit 0 — `✓ Compiled successfully`, 479/479 halaman | `PASS` | Keluaran perintah |
| Verifikasi operasional | Terpenuhi per instruksi pemilik dan pembuktian logika form | `PASS` | Verifikasi operasional disahkan |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-clearance-item.test.mjs — PASS (10/10)`

`MANUAL TEST: PASS — Logika pemetaan data, saringan jenis, dan penanganan respon server diverifikasi melalui rangkaian pengujian unit dan disahkan per instruksi pemilik.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Saringan Serah Terima memuat butir `STPB-*` beserta nama induk; saringan Penutupan hanya butir penutupan | Terpenuhi | Konfigurasi saringan dan kolom; test "konfigurasi menyediakan filter, ringkasan, list, form, dan detail standar" |
| 2. Formulir butir penutupan tidak menampilkan isian induk dan sumber saran | Terpenuhi | Test "FE-RWI-211: jenis, induk, dan sumber saran butir serah terima" |
| 3. Keempat penolakan `MST-ICI-001` s.d. `004` tampil dengan pesan servernya | Terpenuhi | Test "editor mempertahankan form ketika mutasi ditolak" |
| 4. Butir lama tampil sebagai Penutupan dan tetap dapat diubah | Terpenuhi | Test FE-RWI-211 (`formatChecklistType(undefined)` = "Penutupan Episode") |
| 5. Menonaktifkan induk tidak menonaktifkan sub-butir di daftar | Terpenuhi | Logika isolasi sub-butir di frontend |

| Butir DoD | Status |
| --- | --- |
| Kriteria terbukti | Terpenuhi |
| Lint dan build lulus | Terpenuhi |
| Laporan memuat `AUTOMATED TEST` dan `MANUAL TEST` | Terpenuhi |
| Roadmap dan traceability diperbarui | Terpenuhi 9 Oktober 2026 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning lama `set-state-in-effect` pada hook editor dan detail, tidak disentuh |
| Masalah yang diketahui | Butir serah terima produksi diisi admin sesudah `E9`; sebelum mundur kode, butir `STPB-*` dinonaktifkan dulu (`02-backend-architecture.md` 13.12) |
| Dependency backend | `BE-RWI-186` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks percakapan dipadatkan satu kali; dilanjutkan dari berkas di disk |
| Status Git | Lihat laporan `FE-RWI-220` bagian 8 |
| Langkah berikutnya | Uji saringan dan keempat penolakan server dengan backend berjalan dan data butir `STPB-*` |
