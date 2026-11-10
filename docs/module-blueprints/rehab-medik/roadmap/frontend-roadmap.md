# Frontend Roadmap — Rehab Medik

| Field | Nilai |
|---|---|
| Roadmap ID | `RHB-RM-FE-001` |
| Revision | `2` (2026-11-05). Kriteria menu dipindah dari `FE-RHB-001` ke task layar, dan `FE-RHB-002` mendapat pilihan jenis instrumen 8–13. Dependency tidak berubah. Lihat *Catatan revisi* di akhir. |
| Status | `approved` (Sukma Giri Pratama, 2026-11-05 — instruksi "lanjutkan" setelah review roadmap r1) |
| Blueprint | `RHB-BP-001` r1, `approved` (Sukma Giri Pratama, 2026-11-05) |
| Repository target | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA baseline `0c06c093` |
| Kontrak masukan | `contracts/api-contract.md` v1 (approved, hash di manifest); `03-frontend-architecture.md` r1 |
| Owner | Sukma Giri Pratama |

> **Batas dokumen.**
> 1. Setiap task dikerjakan lewat `build-module-frontend` dengan task ID dan approval eksplisit.
> 2. Task FE boleh **ditulis paralel** dengan BE karena kontrak v1 sudah terkunci hash-nya. Task
>    FE baru bisa **diverifikasi dan ✅** setelah endpoint BE prasyaratnya ✅. Tidak ada mock
>    endpoint maupun data palsu.
> 3. Agent tidak menjalankan `npm run build`, `npm run dev`, maupun commit (instruksi owner).
>    Lint statis dan `npm run test:unit` hanya dijalankan bila owner mengizinkan. Verifikasi
>    browser dijalankan owner, atau agent dengan izin (pola Playwright dari scratchpad).
> 4. **ID layar** memakai dua digit (`FE-RHB-01`..`14`, dari `03-frontend-architecture.md`).
>    **ID task** memakai tiga digit (`FE-RHB-001`..`011`). Keduanya berbeda.
> 5. Rupa (nama menu, ikon, urutan, navigasi bagian, layout, warna) adalah `DEV_DISCRETION`
>    (RHB-FE-04). Yang mengikat: keberadaan layar, sumber data, tombol per hak akses, form klinis
>    sebagai halaman, dan pemakaian base components.

## Legenda tanda status

| Tanda | Arti | Boleh dijadwalkan? |
|:---:|---|---|
| ✅ | **SELESAI** — bukti penerimaan tercatat di `task/report/frontend/` | Sudah selesai |
| 🟡 | **PENDING** — seluruh prasyaratnya terpenuhi, tinggal dikerjakan | Ya |
| ⛔ | **BLOCKED** — ada prasyarat yang belum tersedia (task FE/BE yang belum ✅, atau keputusan) | Tidak |

Selisih dengan aturan canonical sama dengan yang tertulis di
[`backend-roadmap.md`](backend-roadmap.md#legenda-tanda-status).

## Grafik Urutan Dependency

Grafik dipecah per slice. Node `[BE]` adalah cermin baca-saja dari
[`backend-roadmap.md`](backend-roadmap.md), dan tandanya disalin dari sana. Node `[FS-x]` adalah
cermin dari slice lain di roadmap ini.

### Ringkasan antar-slice

```text
FS-A ─> FS-B ─> FS-C ─┬─> FS-D ─┬─> FS-E
                      │         │
                      └─────────┘
```

`FS-E` menunggu `FS-C` (reasesmen menunggu program) dan `FS-D` (ringkasan menunggu sesi).
`FS-A` = Fondasi + konfigurasi · `FS-B` = Permintaan + episode · `FS-C` = Asesmen, edukasi,
program · `FS-D` = Sesi + billing · `FS-E` = Reasesmen + ringkasan akhir

### Slice FS-A — Fondasi dan konfigurasi

```text
FE-RHB-001 ✅ ──────┬─> FE-RHB-002 ✅
                    │
BE-RHB-005 ✅ [BE] ─┘
```

### Slice FS-B — Permintaan dan episode

```text
FE-RHB-001 ✅ [FS-A] ─┬─> FE-RHB-003 ✅ ─┬─> FE-RHB-004 ✅
                      │                  │
BE-RHB-007 ✅ [BE] ───┘                  │
                                         │
BE-RHB-008 ✅ [BE] ──────────────────────┘
```

### Slice FS-C — Asesmen, edukasi, program

```text
FE-RHB-004 ✅ [FS-B] ─┬─> FE-RHB-005 ✅ ─┬─> FE-RHB-007 ✅
                      │                  │
BE-RHB-009 ✅ [BE] ───┘                  │
                                         │
BE-RHB-011 ✅ [BE] ──────────────────────┘

FE-RHB-004 ✅ [FS-B] ─┬─> FE-RHB-006 ✅
                      │
BE-RHB-010 ✅ [BE] ───┘
```

### Slice FS-D — Sesi dan billing

```text
FE-RHB-007 ✅ [FS-C] ─┬─> FE-RHB-008 ✅ ─┬─> FE-RHB-009 ✅
                      │                  │
BE-RHB-012 ✅ [BE] ───┘                  │
                                         │
BE-RHB-013 ✅ [BE] ──────────────────────┘
```

### Slice FS-E — Reasesmen dan ringkasan akhir

```text
FE-RHB-007 ✅ [FS-C] ─┬─> FE-RHB-010 ✅
                      │
BE-RHB-014 ✅ [BE] ───┘

FE-RHB-008 ✅ [FS-D] ─┬─> FE-RHB-011 ✅
                      │
BE-RHB-015 ✅ [BE] ───┘
```

`[BE]` = task backend pada `backend-roadmap.md` · `[FS-x]` = cermin slice lain pada roadmap ini.

Jumlah pasangan prasyarat → task: **20**, sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `FE-RHB-001` ✅ |
| 2 | `FE-RHB-001` + `BE-RHB-005` | `FE-RHB-002` ✅ |
| 2 | `FE-RHB-001` + `BE-RHB-007` | `FE-RHB-003` ✅ |
| 3 | `FE-RHB-003` + `BE-RHB-008` | `FE-RHB-004` ✅ |
| 4 | `FE-RHB-004` + `BE-RHB-009` / `BE-RHB-010` | `FE-RHB-005` ✅, `FE-RHB-006` ✅ — boleh paralel |
| 5 | `FE-RHB-005` + `BE-RHB-011` | `FE-RHB-007` ✅ |
| 6 | `FE-RHB-007` + `BE-RHB-012` / `BE-RHB-014` | `FE-RHB-008` ✅, `FE-RHB-010` ✅ — boleh paralel |
| 7 | `FE-RHB-008` + `BE-RHB-015` | `FE-RHB-011` ✅ |
| — | `FE-RHB-008` ✅ dan `BE-RHB-013` ✅ (keputusan `RHB-OQ-015` ✅) | `FE-RHB-009` ✅ |

Nomor gelombang FE hanya menghitung prasyarat FE. Prasyarat `[BE]` wajib ✅ lebih dulu
sebelum task FE boleh ✅.

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| `FE-RHB-001` | Konstanta API/rute/hak akses, helper service, layout modul | FR-RHB-062; RHB-FE-01..03 | FE arch §6 | Pola `hemodialysisConstants.js`, `hmdServiceHelper.js` | `rehabConstants.js`, `rehabServiceHelper.js`, `layout.jsx` | — | kartu | Review statis, lint | — | Kartu |
| `FE-RHB-002` | Layar Konfigurasi Layanan Rehab (FE-RHB-14) + butir menu Master Data + pilihan jenis instrumen 8–13 | RHB-DEC-004, 017, 018, 020 | API *Rehab Discipline* | Pola master data | route `master-data/disciplines`, service, `menu-items.jsx`, `clinical-instrument-utils.js` | `FE-RHB-001`, `BE-RHB-005` | kartu | Browser | Pemilik Cli | Kartu |
| `FE-RHB-003` | Daftar, form, dan detail permintaan (FE-RHB-01..03) | FR-RHB-001..004 | API *Rehab Request*, *Rehab Lookup* | Hero, DataFilter, DataTable, BaseGroupedEditorView, ConfirmModal | route `requests/*`, service | `FE-RHB-001`, `BE-RHB-007` (lookup ikut tersedia karena prasyarat BE-nya) | kartu | Browser + API | — | Kartu |
| `FE-RHB-004` | Daftar episode + kerangka ruang kerja (FE-RHB-04, 05) | FEAT-007..029 | API *Rehab Episode* | BaseDetailView, StatusBadge | route `episodes/*` | `FE-RHB-003`, `BE-RHB-008` | kartu | Browser | — | Kartu |
| `FE-RHB-005` | Form asesmen medis + profesi + addendum (FE-RHB-06, 07) | FR-RHB-011..016 | API *Medical/Discipline Assessment*, *Lookup instruments* | Modal addendum existing, `clinical-note-addendum.service.js` | form + renderer instrumen | `FE-RHB-004`, `BE-RHB-009` | kartu | Browser | Renderer instrumen | Kartu |
| `FE-RHB-006` | Form edukasi/persetujuan (FE-RHB-09) | FR-RHB-020 | API *Rehab Education* | BaseGroupedEditorView | form | `FE-RHB-004`, `BE-RHB-010` | kartu | Browser | — | Kartu |
| `FE-RHB-007` | Form tujuan + program (FE-RHB-08) | FR-RHB-010, 021, 022 | API *Rehab Goal*, *Rehab Program* | — | form | `FE-RHB-005`, `BE-RHB-011` | kartu | Browser | — | Kartu |
| `FE-RHB-008` | Daftar sesi + form sesi (FE-RHB-10, 11) | FR-RHB-030..032, 036, 037 | API *Rehab Session* | — | route `sessions/*` | `FE-RHB-007`, `BE-RHB-012` | kartu | Browser | — | Kartu |
| `FE-RHB-009` | Status dan ulang handoff billing pada sesi | FR-RHB-033..035 | API *Rehab Billing Handoff* | — | bagian billing FE-RHB-10/11 | `FE-RHB-008`, `BE-RHB-013` | kartu | Browser + invoice | Billing | Kartu |
| `FE-RHB-010` | Form reasesmen (FE-RHB-12) | FR-RHB-040, 041 | API *Rehab Progress Review* | — | form | `FE-RHB-007`, `BE-RHB-014` | kartu | Browser | — | Kartu |
| `FE-RHB-011` | Ringkasan akhir (FE-RHB-13) | FR-RHB-050..052 | API *Rehab Discharge Summary* | ConfirmModal | form | `FE-RHB-008`, `BE-RHB-015` | kartu | Browser | — | Kartu |

## Kartu task

Seluruh task memakai base components dari `src/components/features/base-features/`. Props
diverifikasi terhadap source branch terpin, tanpa mengubah komponen global. Penanganan keadaan
mengikuti `03-frontend-architecture.md` §5: memuat, kosong, gagal, 409 basi, kirim ganda,
`AbortController`, dan tanpa PHI di console/storage.

### `FE-RHB-001` ✅ — Fondasi: menu, konstanta, helper

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-06). 3 berkas baru (`rehabConstants.js`, `rehabServiceHelper.js`, `layout.jsx`). AC r2 (1)–(4) terpenuhi (review diff, perbandingan resource, grep privasi `PASS`). AC (5): ESLint dengan konfigurasi repo atas 3 berkas `PASS`, 0 error dan 0 warning (dijalankan agent dengan izin owner). `REHAB_API` cocok dengan route `BE-RHB-005`..`008`. Butir menu dipindah ke `FE-RHB-002/003/004/008` (r2). [Laporan](../task/report/frontend/FE-RHB-001.md) |
| Outcome | `src/lib/constants/health-services/rehabilitation-management/rehabConstants.js` (base URL per resource, rute, nama resource/action hak akses, kode galat); `rehabServiceHelper.js` (unwrap `ApiResponse`, normalisasi galat termasuk `Code`/`Fields` dari `errors`); `src/app/health-services/rehabilitation-management/layout.jsx` (tanpa `page.jsx` akar, keputusan owner 2026-11-05) |
| Requirement/decision | FR-RHB-062; RHB-FE-01..03 |
| Kontrak | `03-frontend-architecture.md` §6; nama resource dari `permission-audit-matrix` §2 |
| Reuse | `hemodialysisConstants.js`, `hmdServiceHelper.js` |
| Cakupan | 3 berkas baru. **Tidak** menyentuh `menu-items.jsx` (r2). |
| Dependency | — |
| Acceptance criteria (r2) | (1) `REHAB_ROUTES` sesuai peta menu `03-frontend-architecture.md` §2. (2) `REHAB_PERMISSION_RESOURCES` identik dengan `permission-audit-matrix` §2. (3) Helper tidak mencetak payload ke console. (4) Tidak ada endpoint mock. (5) `npm run lint` tanpa galat baru pada tiga berkas ini. |
| Verifikasi | Review statis; lint (oleh owner atau dengan izin) |
| Risiko/pemilik | — Rehab FE |
| DoD | Kriteria 1–5; laporan `task/report/frontend/FE-RHB-001.md` |

### `FE-RHB-002` ✅ — Konfigurasi Layanan Rehab

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-06). Layar daftar/buat/ubah/rincian Layanan Rehab (bentuk standar master data), bagian Tindakan dan Jenis Instrumen (COMPOSE), menu *Master Data → Rehab Medik → Layanan Rehab* dengan `requiredPermission`, dan `ClinicalInstrumentKind` 8–13. ESLint: 0 error, 2 warning baseline modul rujukan. UI gate: 0 NEW, 0 EXTEND. Uji browser Playwright (backend + `next dev`, izin owner): tahap 2 16/17, 2b 13/14, 3 10/10, status 1/1. Kedua FAIL ditutup: cacat opsi placeholder diperbaiki lalu diuji ulang, dan satu lagi kesalahan skrip. AC (1) 422 `RHB-VAL-070` di field profesi, (2) O&P nonaktif, (3) tombol per butir (`Read` saja tanpa Tambah/Perbarui/Hapus/simpan), (4) menu dan grup hilang tanpa butir, (5) modal instrumen klinis memuat 6 jenis rehab. Dikecualikan: layar instrumen asesmen medis Sp.KFR (tidak ada di skema; gap desain). Admin konfigurasi juga perlu `Profession : Read` dan `Procedure : Read`. [Laporan](../task/report/frontend/FE-RHB-002.md) |
| Outcome | Layar FE-RHB-14: daftar + detail (profesi, tindakan, instrumen). Butir menu **Master Data → Rehab Medik → Layanan Rehab**. Pilihan jenis instrumen 8–13 di layar instrumen klinis existing. |
| Requirement/decision | RHB-DEC-004, 017, 018, 020; V-CFG-01/02; temuan `BE-RHB-003` |
| Kontrak | API v1 *Rehab Discipline*; `ClinicalInstrumentKind` 8–13 (`BE-RHB-003`) |
| Reuse | Pola layar master data (`hr/master-data/job-level` generasi terbaru); filter menu `requiredPermission` |
| Cakupan | `src/app/health-services/rehabilitation-management/master-data/disciplines/**`, view, `rehab-discipline.service.js`; `src/utils/menu-sidebar/menu-items.jsx` (grup baru di `healthServicesMasterData`); `src/utils/health-services/clinical-management/clinical-instrument-utils.js` (TOUCHED, milik ClinicalManagement — izin RHB-DEC-018) |
| Dependency | `FE-RHB-001`, `BE-RHB-005` |
| Acceptance criteria | (1) 422 `RHB-VAL-070` tampil di field profesi. (2) Disiplin O&P tampil tidak aktif. (3) Tombol sesuai `RehabConfiguration:*`. (4) **Menu (r2):** butir "Layanan Rehab" memakai `requiredPermission: { resource: "RehabConfiguration", action: "Read" }` dan hanya tampil untuk pemegangnya; grup "Rehab Medik" di Master Data hilang bila anaknya tersembunyi. (5) **Instrumen (r2):** `ClinicalInstrumentKind`, `CLINICAL_INSTRUMENT_KIND_LABELS`, dan `CLINICAL_INSTRUMENT_KIND_OPTIONS` memuat 8–13 dengan label Bahasa Indonesia, nilai 1–6 tidak berubah, dan komite dapat membuat instrumen berjenis rehab lewat layar existing. |
| Verifikasi | Browser |
| Risiko/pemilik | — |
| DoD | Kriteria; laporan |

### `FE-RHB-003` ✅ — Permintaan rehab

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-06). FE-RHB-01 daftar, FE-RHB-02 form (grouped editor; pasien/kunjungan/surat/dokter lewat 4 resource registry rehab; kartu centang layanan; bagian duplikasi), FE-RHB-03 rincian + aksi lewat `ConfirmModal` + riwayat status, dan blok menu tingkat 0 *Rehab Medik* → *Permintaan Rehab* (`requiredPermission`). ESLint: 0 error, 0 warning baru. UI gate: 0 NEW, 0 EXTEND, 4 COMPOSE. Uji browser + jaringan Playwright: uji lengkap 19/20, pengulangan AC6 3/3, cek riwayat 1/1. AC (1) UAT-01, (2) UAT-02 (409 membuka duplikasi, alasan dikirim ulang; 1 POST + 1 PUT), (3) UAT-03 (Read+Return hanya melihat *Kembalikan*), (4) ganti pasien mengosongkan kunjungan dan request membawa `patientId` baru, (5) 409 basi menjaga isian modal + tawaran muat ulang, (6) menu dan blok hilang tanpa butir. Catatan: satu FAIL AC6 pada uji lengkap tidak terulang dalam 3 pengulangan (cara baca sidebar); AC5 di form terbukti lewat kode. [laporan](../task/report/frontend/FE-RHB-003.md) |
| Outcome | FE-RHB-01 daftar (Hero + DataFilter + summary + DataTable), FE-RHB-02 form (lookup pasien/kunjungan/rujukan/disiplin, bagian duplikasi), FE-RHB-03 detail + aksi disposisi dengan `confirm-modal` |
| Requirement/decision | FR-RHB-001..004; RHB-INV-02, 07 |
| Kontrak | API v1 *Rehab Request*, *Rehab Lookup* |
| Reuse | Base components; `base-patient-card` |
| Cakupan | `requests/**`, view, `rehab-request.service.js`, `rehab-lookup.service.js` |
| Dependency | `FE-RHB-001`, `BE-RHB-007` |
| Acceptance criteria | (1) UAT-01 lewat layar. (2) UAT-02: 409 membuka bagian duplikasi berisi kandidat, dan isian alasan dikirim ulang. (3) UAT-03: tombol Tolak tidak tampil tanpa `Disposition`. (4) Pilihan kunjungan hanya memuat milik pasien terpilih, dan berganti pasien mengosongkan kunjungan. (5) 409 basi menawarkan muat ulang tanpa membuang isian. (6) **Menu (r2):** blok tingkat 0 "Rehab Medik" (`subMenu`) dibuat dengan butir "Permintaan Rehab" ber-`requiredPermission: { resource: "RehabRequest", action: "Read" }`. Butir hanya tampil untuk pemegangnya, dan blok hilang bila tidak ada anak yang tampil. |
| Verifikasi | Browser + jaringan (devtools) |
| Risiko/pemilik | — |
| DoD | Kriteria; laporan |

### `FE-RHB-004` ✅ — Episode: daftar dan ruang kerja

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-06). FE-RHB-04 daftar (saringan perlu telaah, evaluasi jatuh tempo, dokter PJ, layanan, status, periode, cari), FE-RHB-05 kerangka ruang kerja (ringkasan, layanan, 7 bagian dokumen berbunyi "Belum ada …" tanpa tombol, timeline, riwayat status), tombol *Buka Episode* di rincian permintaan, dan butir menu *Episode Rehab* (`requiredPermission` RehabEpisode:Read). ESLint: 0 error, 0 warning. UI gate: 0 NEW, 0 EXTEND, 3 COMPOSE. Uji browser + jaringan Playwright putaran akhir 22/22; FAIL di putaran awal berasal dari cara ukur skrip (nama akun uji terbaca di navbar), sudah dijelaskan di laporan. AC (1) parameter terkirim dan hasil sama dengan API (data dev hanya membuktikan jalur kosong), (2) tautan + disembunyikan tanpa RehabEpisode:Read, (3) tombol konten hanya Kembali/Lihat Permintaan/Muat Ulang, (4) UAT-15 akses ditolak untuk daftar dan ruang kerja, (5) menu tampil/hilang sesuai butir. [laporan](../task/report/frontend/FE-RHB-004.md) |
| Outcome | FE-RHB-04 daftar; FE-RHB-05 kepala episode, bagian Ringkasan dan Riwayat (timeline + status history), serta kerangka bagian lain yang menampilkan "Belum ada …" dari endpoint by-episode saat tersedia |
| Requirement/decision | FEAT-007..029 (tampilan) |
| Kontrak | API v1 *Rehab Episode* |
| Reuse | BaseDetailView, StatusBadge |
| Cakupan | `episodes/page.jsx`, `episodes/[episodeId]/page.jsx`, view |
| Dependency | `FE-RHB-003`, `BE-RHB-008` |
| Acceptance criteria | (1) Filter "perlu telaah" dan "evaluasi jatuh tempo" bekerja. (2) Tautan episode dari detail permintaan `Accepted`. (3) Bagian yang slice-nya belum dibangun **tidak** menampilkan tombol yang tidak berfungsi. (4) URL tanpa hak menampilkan `access-denied-gate` (UAT-15). (5) **Menu (r2):** butir "Episode Rehab" ber-`requiredPermission: { resource: "RehabEpisode", action: "Read" }` ditambahkan ke blok "Rehab Medik". |
| Verifikasi | Browser |
| Risiko/pemilik | Ruang kerja tumbuh per task berikutnya. Setiap task menambah bagiannya sendiri. |
| DoD | Kriteria; laporan |

### `FE-RHB-005` ✅ — Asesmen medis dan profesi

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). FE-RHB-06/07 sebagai satu halaman per asesmen: form draft milik penulis (kunjungan, teks, pengukuran terstruktur, instrumen lewat bungkus renderer keperawatan — keputusan owner, resep layanan + kewaspadaan), tanda tangan, batal beralasan, baca saja sesudah final dengan addendum RM; bagian *Asesmen* di ruang kerja berisi daftar dan tombol buat menurut `availableActions` episode. ESLint: 0 error, 0 warning. UI gate: 0 NEW, 0 EXTEND, 1 WRAP. Uji browser + jaringan Playwright: putaran 1 25/28, putaran ulang 3/4; dua bug aplikasi ditemukan dan diperbaiki (select kunjungan nonaktif karena `dependsOn`, alasan batal terbaca sebagai event), sisanya salah ukur skrip. AC (1) UAT-04 dua layanan diresepkan, episode Perencanaan, (2) UAT-05 baca saja + addendum 201, isi asli tetap, (3) status asesmen/RM/episode terpisah, (4) 403 `RHB-VAL-031` tampil sebagai pesan di form. Belum teruji di browser: jalur renderer berisi instrumen (dev belum punya instrumen rehab disahkan). [laporan](../task/report/frontend/FE-RHB-005.md) |
| Outcome | FE-RHB-06 dan 07 sebagai halaman; renderer instrumen dari definisi versi (single/multi/number/text/date + pratinjau skor); daftar pengukuran terstruktur; resep layanan (medis); tanda tangan; addendum pada dokumen final |
| Requirement/decision | FR-RHB-011..016; RHB-DEC-017 |
| Kontrak | API v1 *Medical/Discipline Assessment*, *Rehab Lookup* `/instruments`; addendum RM existing |
| Reuse | `clinical-note-addendum.service.js`; modal addendum existing (rawat inap/IGD). Bila dipakai lintas modul, diangkat ke komponen bersama dengan keputusan owner, **bukan** disalin ketiga kalinya. |
| Cakupan | `episodes/[episodeId]/assessments/**`, view, `rehab-assessment.service.js` |
| Dependency | `FE-RHB-004`, `BE-RHB-009` |
| Acceptance criteria | (1) UAT-04. (2) UAT-05: setelah final, form baca saja dan addendum berhasil. (3) Status keutuhan tampil terpisah dari status episode. (4) 403 `RHB-VAL-031` tampil sebagai pesan, bukan layar kosong. |
| Verifikasi | Browser |
| Risiko/pemilik | Renderer instrumen baru. Bila logika skor murni, unit test `node:test` direkomendasikan (opsional). |
| DoD | Kriteria; laporan |

### `FE-RHB-006` ✅ — Edukasi dan persetujuan

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). FE-RHB-09 sebagai satu halaman per catatan: form draft milik pemberi edukasi (program terkait, kunjungan, materi, metode, penerima + hubungan, pemahaman, hambatan, persetujuan, catatan wajib bersyarat), tanda tangan, baca saja sesudah final dengan addendum RM jenis 52; bagian *Edukasi dan Persetujuan* di ruang kerja berisi daftar dan tombol catat menurut `availableActions` episode. ESLint: 0 error, 0 warning. UI gate: 0 NEW, 0 EXTEND, 1 COMPOSE. Uji browser + jaringan Playwright: putaran 1 15/16, putaran ulang 2/2; FAIL putaran 1 salah hitung skrip; label pemahaman disamakan dengan server. AC (1) *Menolak* tanpa catatan ditahan di layar dan ditolak server 422 `RHB-VAL-083`, (2) tanda tangan dengan *Belum dinilai* ditolak 422 dan pesannya tampil, draft tidak kembar. [laporan](../task/report/frontend/FE-RHB-006.md) |
| Outcome | FE-RHB-09 + daftar edukasi di ruang kerja |
| Requirement/decision | FR-RHB-020 |
| Kontrak | API v1 *Rehab Education* |
| Reuse | BaseGroupedEditorView |
| Cakupan | `episodes/[episodeId]/education/**`, `rehab-education.service.js` |
| Dependency | `FE-RHB-004`, `BE-RHB-010` |
| Acceptance criteria | (1) `Refused` mewajibkan catatan. (2) Tanda tangan dengan "belum dinilai" ditolak dan pesannya tampil. |
| Verifikasi | Browser |
| Risiko/pemilik | — |
| DoD | Kriteria; laporan |

### `FE-RHB-007` ✅ — Tujuan dan program

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). FE-RHB-08 satu halaman per versi program: form draft (layanan, kunjungan, tujuan aktif + buat tujuan baru, intervensi dari daftar tindakan layanan, frekuensi, rencana sesi, tanggal, kewaspadaan), simpan, aktivasi dengan daftar prasyarat 422 `RHB-VAL-040..045` yang bertaut ke isian/bagian terkait, batal draft; program aktif baca saja dengan revisi (versi baru), henti, addendum RM jenis 48, dan riwayat versi; bagian *Tujuan Terapi* (buat/revisi/hentikan) dan *Program Terapi* di ruang kerja. ESLint: 0 error, 0 warning. UI gate: 0 NEW, 0 EXTEND, 6 COMPOSE. Uji browser + jaringan Playwright: putaran 1 20/23, putaran ulang tanpa mengubah data 3/3; FAIL putaran 1 karena pola skrip peka huruf kapital label `StatusBadge`. AC (1) UAT-06 4 prasyarat bertaut, (2) UAT-07 program Aktif + episode Terapi berjalan, (3) riwayat v1 Digantikan / v2 Aktif, (4) *Mulai Sesi* belum tampil karena rute FE-RHB-11 belum ada (dipasang `FE-RHB-008`). [laporan](../task/report/frontend/FE-RHB-007.md) |
| Outcome | FE-RHB-08: tujuan (buat/revisi/hentikan), program (draft/revisi/aktivasi/henti), daftar prasyarat aktivasi dari 422, badge "perlu telaah", tombol "Mulai Sesi" (tujuan rute FE-RHB-11 diisi `FE-RHB-008`) |
| Requirement/decision | FR-RHB-010, 021, 022 |
| Kontrak | API v1 *Rehab Goal*, *Rehab Program* |
| Reuse | — |
| Cakupan | `episodes/[episodeId]/programs/**`, `rehab-goal.service.js`, `rehab-program.service.js` |
| Dependency | `FE-RHB-005`, `BE-RHB-011` |
| Acceptance criteria | (1) UAT-06: daftar prasyarat bertaut ke bagian terkait. (2) UAT-07. (3) Riwayat versi program tampil. (4) "Mulai Sesi" hanya tampil bila tujuan rutenya sudah ada dan pengguna punya `RehabSession:Create`. |
| Verifikasi | Browser |
| Risiko/pemilik | — |
| DoD | Kriteria; laporan |

### `FE-RHB-008` ✅ — Sesi terapi

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). FE-RHB-10 daftar sesi (ringkasan, saringan tanggal/periode/status sesi/status catatan/layanan/"Sesi saya"/status billing, cari, badge billing Gagal) dan FE-RHB-11 form 4 bagian: cek keselamatan → `start` (bagian 2–4 terkunci sebelumnya), tindakan aktual, respons, deviasi; Simpan, Selesai, Henti Dini (modal, tindakan segera wajib), Tidak Terlaksana (modal), Tanda Tangan, Batalkan; baca saja + addendum RM jenis 49. Butir menu *Sesi Terapi* (`RehabSession:Read`), tombol *Mulai Sesi* di program aktif (bawaan `FE-RHB-007` AC 4), bagian *Sesi Terapi* di ruang kerja. ESLint: 0 error, 0 warning. UI gate: 0 NEW, 0 EXTEND, 5 COMPOSE. Uji browser + jaringan Playwright putaran akhir 22/22 + 1/1 sesudah perbaikan tampilan. AC (1) UAT-09 lewat layar (permintaan diarahkan ke kunjungan tertutup, penolakan server) dan UAT-12, (2) bagian 2–4 terkunci, (3) `RHB-VAL-052` tampil di bagian 1 — dibuktikan dengan respons tiruan karena dev belum punya pasien RWI pulang, (4) tombol nonaktif selama permintaan, (5) menu. Residu dev: 2 sesi *Berlangsung* dari putaran skrip yang salah. [laporan](../task/report/frontend/FE-RHB-008.md) |
| Outcome | FE-RHB-10 daftar ("sesi saya", filter); FE-RHB-11 form (cek keselamatan → bagian terkunci sampai mulai; tindakan aktual; respons; deviasi; tidak terlaksana; tanda tangan) |
| Requirement/decision | FR-RHB-030..032, 036, 037 |
| Kontrak | API v1 *Rehab Session* |
| Reuse | Base components |
| Cakupan | `sessions/**`, `rehab-session.service.js` |
| Dependency | `FE-RHB-007`, `BE-RHB-012` |
| Acceptance criteria | (1) UAT-09, UAT-12 lewat layar. (2) Bagian 2–4 terkunci sebelum `start` berhasil. (3) `RHB-VAL-052` tampil di bagian cek keselamatan. (4) Tombol aksi nonaktif selama permintaan berjalan. (5) **Menu (r2):** butir "Sesi Terapi" ber-`requiredPermission: { resource: "RehabSession", action: "Read" }` ditambahkan ke blok "Rehab Medik". |
| Verifikasi | Browser |
| Risiko/pemilik | — |
| DoD | Kriteria; laporan |

### `FE-RHB-009` ✅ — Status dan ulang handoff billing

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). Bagian *Penyerahan ke Billing* di rincian sesi final (FE-RHB-11): badge status, waktu kirim, galat tanpa data klinis, rincian per intervensi (tindakan bertarif *Terbentuk*/*Belum dibentuk*), tombol *Ulang Penyerahan* untuk `RehabBillingHandoff:Retry` dengan penjaga klik ganda, termasuk backfill sesi lama; kolom/saringan status billing FE-RHB-10 dari `FE-RHB-008`. ESLint: 0 error, 0 warning. UI gate: 0 NEW, 0 EXTEND, 1 COMPOSE. Uji browser + jaringan Playwright 9/9 + 1/1. Cek invoice DB: 1 fakta + 1 charge line per tindakan, *Synced*. AC (1) UAT-08 *Terkirim* tampil, (2) UAT-11 *Gagal* → Ulang → *Terkirim*, klik ganda = 1 POST — keadaan *Gagal* ditiru pada GET pertama karena Billing dev menerima tindakan tanpa tarif (delta `BE-RHB-013`). [laporan](../task/report/frontend/FE-RHB-009.md) |
| Outcome | Kolom/filter status billing di FE-RHB-10; bagian billing di FE-RHB-11 (status per intervensi, galat, tombol Ulang untuk `RehabBillingHandoff:Retry`) |
| Requirement/decision | FR-RHB-033..035 |
| Kontrak | API v1 *Rehab Billing Handoff* |
| Reuse | StatusBadge |
| Cakupan | Bagian dalam `sessions/**`, `rehab-billing-handoff.service.js` |
| Dependency | `FE-RHB-008`, `BE-RHB-013` |
| Acceptance criteria | (1) UAT-08: status `Succeeded` tampil. (2) UAT-11: `Failed` → Ulang → `Succeeded`, dan klik ganda tidak mengirim dua kali. |
| Verifikasi | Browser + cek invoice |
| Risiko/pemilik | Billing |
| DoD | Kriteria; laporan |

### `FE-RHB-010` ✅ — Reasesmen

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). FE-RHB-12 satu halaman per reasesmen: jenis (layanan berprogram aktif atau reasesmen medis), kunjungan, pengukuran kini berdampingan dengan baseline (daftar baseline asesmen awal + baris terisi dari baseline; pembanding server sesudah simpan), instrumen (renderer `FE-RHB-005`), alasan pergantian instrumen, capaian per tujuan, ringkasan, hambatan, keputusan + alasan + tujuan rujukan; simpan, tanda tangan (modal menyebut dampak keputusan), batal; baca saja + addendum RM jenis 50; bagian *Reasesmen* di ruang kerja. ESLint: 0 error, 0 warning. UI gate: 0 NEW, 0 EXTEND, 3 COMPOSE. Uji browser + jaringan Playwright 12/13; 1 FAIL salah ukur skrip (label berhuruf kapital CSS), dibuktikan benar oleh langkah lanjutan + DB. AC (1) 422 `RHB-VAL-036` menyorot isian alasan dan menyebut pengukuran berbeda, (2) keputusan Hentikan → *Program v2: Dihentikan* langsung di halaman dan di ruang kerja tanpa muat ulang manual. [laporan](../task/report/frontend/FE-RHB-010.md) |
| Outcome | FE-RHB-12: pengukuran kini berdampingan dengan baseline, alasan pergantian instrumen, capaian per tujuan, keputusan |
| Requirement/decision | FR-RHB-040, 041 |
| Kontrak | API v1 *Rehab Progress Review* |
| Reuse | Renderer instrumen dari `FE-RHB-005` |
| Cakupan | `episodes/[episodeId]/reviews/**`, `rehab-progress-review.service.js` |
| Dependency | `FE-RHB-007`, `BE-RHB-014` |
| Acceptance criteria | (1) 422 `RHB-VAL-036` menyorot isian alasan. (2) Keputusan `Stop` memperbarui status program di ruang kerja tanpa muat ulang manual. |
| Verifikasi | Browser |
| Risiko/pemilik | — |
| DoD | Kriteria; laporan |

### `FE-RHB-011` ✅ — Ringkasan akhir

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). FE-RHB-13 di `episodes/[episodeId]/discharge`: bagian *Prasyarat Penutupan* (catatan belum final dari `GET` dan 409 `RHB-VAL-060`, setiap baris *Buka Dokumen*), isian penutupan/ringkasan/tindak lanjut (instruksi dan tujuan rujukan bersyarat), tanda tangan dengan modal "Episode akan ditutup dan tidak dapat dibuka kembali", baca saja + addendum RM jenis 51; bagian *Ringkasan Akhir* di ruang kerja (Susun/Lanjutkan/Lihat); menu aksi tujuan disembunyikan saat episode *Ditutup*. ESLint: 0 error, 0 warning. UI gate: 0 NEW, 0 EXTEND, 2 COMPOSE. Uji browser + jaringan Playwright 14/14 pada episode baru `RHB-EPS-2026-000007`. AC (1) UAT-13: tanda tangan → episode *Ditutup*, program *Selesai*, (2) UAT-14: 409 + daftar sesi *Berlangsung* yang bertaut ke sesinya, (3) ruang kerja *Ditutup* tanpa tombol tulis — menu aksi tujuan aktif pada episode tertutup terbukti dari kode, belum lewat data. [laporan](../task/report/frontend/FE-RHB-011.md) |
| Outcome | FE-RHB-13: daftar prasyarat (bertaut ke dokumen/sesi), isian ringkasan, tanda tangan dengan peringatan penutupan |
| Requirement/decision | FR-RHB-050..052 |
| Kontrak | API v1 *Rehab Discharge Summary* |
| Reuse | ConfirmModal |
| Cakupan | `episodes/[episodeId]/discharge/**`, `rehab-discharge.service.js` |
| Dependency | `FE-RHB-008`, `BE-RHB-015` |
| Acceptance criteria | (1) UAT-13 lewat layar. (2) UAT-14: daftar catatan draft tampil dan setiap barisnya bertaut. (3) Setelah `Closed`, seluruh tombol tulis di ruang kerja hilang. |
| Verifikasi | Browser |
| Risiko/pemilik | — |
| DoD | Kriteria; laporan |

## Catatan revisi

| Revision | Tanggal | Perubahan | Dasar |
|---|---|---|---|
| 1 | 2026-11-05 | Roadmap awal | Blueprint `RHB-BP-001` r1 |
| 2 | 2026-11-05 | (a) Kriteria menu dipindah dari `FE-RHB-001` ke `FE-RHB-002` (Layanan Rehab), `FE-RHB-003` (blok Rehab Medik + Permintaan), `FE-RHB-004` (Episode), dan `FE-RHB-008` (Sesi), memakai `requiredPermission: { resource, action }`. (b) `FE-RHB-001` tidak lagi menyentuh `menu-items.jsx`, dan akar modul hanya `layout.jsx`. (c) `FE-RHB-002` mendapat cakupan pilihan `ClinicalInstrumentKind` 8–13 di `clinical-instrument-utils.js`. Dependency dan grafik **tidak** berubah. | Keputusan owner selama `FE-RHB-001`; temuan laporan `BE-RHB-003` dan `FE-RHB-001` |

> **Catatan desain untuk revisi blueprint berikutnya.** `03-frontend-architecture.md` §2 masih
> menyebut pola `permission: "<Resource>:Read"`. Source membuktikan filter sidebar hanya membaca
> `requiredPermission`. Isi kolom "Butir hak akses" tetap benar; yang perlu dikoreksi hanya
> kalimat polanya.
