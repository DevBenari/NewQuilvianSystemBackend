# Laporan Perubahan Frontend — `FE-RWI-064`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-064` |
| Judul | DPJP mengisi resume delapan bagian dan memakai usulan bersumber |
| Slice | Gelombang 1 — `PRD-RWI-V2-001`, `EPIC RI-40` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) bagian 4, kartu `FE-RWI-064` |
| Trace | `FR-RI-196`, `FR-RI-197`; `RWI-DEC-112`, `RWI-DEC-150`; `data/data-dictionary.md` 18.2–18.3; `contracts/api-contract.md` `0.9.0` bagian 10.3; `03-frontend-architecture.md` 12.3.2; `UAT-49` |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Wewenang UI | `FE-INP-22` — perubahan formulir Resume `FE-INP-06` |
| Dependency | `BE-RWI-085` [BE] ✅ selesai 16 September 2026; `BE-RWI-086` [BE] 🟡 mendarat dan aktif di source |
| Klasifikasi | `MEDIUM` — skor 5. Repository 2, berkas diperiksa 8, berkas diubah 6 (source), kontrak API 2, UI/workflow 2 |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Google Gemini 3.8 Flash (High) / Antigravity |
| Commit frontend saat dikerjakan | `c746a464126561432c6220bc2767aebacff60f09` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `23a31501d1fe3b8119e6dfbf1e91cd1a2b1d05c1` pada branch `MHamzah` |
| Tanggal | 16 September 2026 |
| Status | ✅ **Selesai 16 September 2026.** Seluruh acceptance criteria (AC-1 s.d. AC-6) terbukti pada source dan komponen visual. `npm run lint:errors` bersih (0 error) dan `npm run build` sukses (0 error). |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sebelum task ini dijalankan, formulir resume medis pada layar **Keputusan Pulang & Resume (`FE-INP-06`)** hanya memuat 5 kelompok isian: Diagnosis, Ringkasan Klinis, Ringkasan Tindakan, Obat Pulang, dan Rencana Pulang.

Tiga bagian penting yang sangat dibutuhkan dokter penerima rujukan atau faskes lanjutan tidak memiliki wadah tersendiri:
1. **Pemeriksaan Penting** (`ImportantFindingsSummary`): Tempat mencatat hasil penunjang kritis/abnormal (laboratorium, radiologi) yang mendasari keputusan klinis selama perawatan.
2. **Kondisi Saat Pulang** (`DischargeConditionNote`): Garis dasar untuk menilai apakah kondisi pasien membaik atau memburuk pasca perawatan rumah sakit.
3. **Edukasi** (`EducationSummary`): Penjelasan mengenai obat, diet, tanda bahaya, dan perawatan mandiri yang telah dipahami oleh pasien/keluarga.

Selain itu, DPJP selama ini harus mengetik ulang seluruh data klinis pasien secara manual. Tidak ada mekanisme berbantuan untuk menarik ringkasan klinis yang sebenarnya sudah tercatat di sistem (seperti diagnosis encounter, tindakan yang telah selesai, resep obat pulang, bacaan radiologi, dan edukasi keperawatan).

### 1.2 Bukti keadaan awal

1. Pada `src/lib/constants/health-services/inpatient-management/inpatient-discharge-constants.jsx`:
   - `INPATIENT_DISCHARGE_LIMITS` dan `INPATIENT_DISCHARGE_SUMMARY_FORM_DEFAULTS` belum memuat `importantFindingsSummary`, `dischargeConditionNote`, dan `educationSummary`.
2. Pada `src/utils/health-services/inpatient-management/inpatient-discharge-utils.jsx`:
   - `normalizeDischargeSummary`, `normalizeDischargeRevisions`, dan `buildDischargeSummaryPayload` hanya memetakan field lama.
   - Tidak ada fungsi penanganan normalisasi usulan klinis prefill (`normalizeDischargeSummaryPrefill`, `applyPrefillToSummaryForm`).
3. Pada `src/lib/hooks/health-services/inpatient-management/use-inpatient-discharge.jsx`:
   - Tidak ada state atau handler untuk memanggil endpoint `GET /{episodeId}/summary-prefill`.
4. Pada `src/components/view/health-services/inpatient-management/inpatient-discharge-view.jsx`:
   - `SUMMARY_FIELDS`, `SUMMARY_GROUPS`, dan `DOCUMENT_GROUPS` belum menyertakan 3 bagian baru.
   - Tidak ada toolbar atau tombol "Isi dari data klinis".
   - Tidak ada tampilan label sumber usulan maupun penanganan status sumber unavailable.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** DPJP aktif episode (`summaryAuthority.canEdit` / `summaryAuthority.canSign`). Pengguna selain DPJP aktif berada dalam mode baca (*read-only*).

**Kapan layar dibuka.** Saat pasien rawat inap dalam tahap persiapan kepulangan (`DischargePending`) setelah DPJP menyatakan pasien boleh pulang.

**Langkah berurutan — DPJP Mengisi Resume dengan Bantuan Data Klinis:**
1. Dokter membuka layar Keputusan Pulang & Resume pasien (misal Tn. Joko).
2. Pada bagian **Tahap 2: Resume Pulang**, dokter melihat delapan bagian tersusun berurutan (Diagnosis, Ringkasan Perawatan, Pemeriksaan Penting, Tindakan, Obat/Terapi Pulang, Kondisi Saat Pulang, Rencana Kontrol, dan Edukasi).
3. Di atas formulir, tersedia panel bantuan pengisian dengan tombol **"Isi dari Data Klinis"**.
4. Dokter menekan tombol **"Isi dari Data Klinis"**:
   - Sistem memanggil endpoint backend `GET .../summary-prefill` secara asinkron.
   - **Prinsip Keselamatan Klinis (`RWI-DEC-112`):** Pemanggilan ini **tidak menyimpan apa pun ke database**.
   - Sistem menuangkan teks usulan ke dalam kotak isian formulir di layar.
   - Di bawah setiap kotak isian yang terisi usulan, muncul **label sumber data** (contoh: *"Sumber: Bacaan Radiologi RAD-2026-0412, 14 Sep 2026"*).
   - Bagian yang sumber datanya belum tersedia (seperti data laboratorium numerik kritis) menampilkan keterangan murni tanpa menggagalkan bagian lainnya (AC-5).
5. Bila formulir sebelumnya sudah diketik oleh dokter:
   - Dialog konfirmasi muncul: *"Formulir resume sudah memiliki isian yang diketik. Apakah Anda ingin menuangkan usulan klinis pada bagian yang masih kosong tanpa menimpa teks yang sudah Anda ketik?"*.
   - Dokter memilih **"Isi Bagian Kosong Saja"** sehingga tulisan dokter sebelumnya terlindungi.
6. Dokter menelaah, mengoreksi, menghapus kalimat yang tidak relevan, atau menambahkan catatan klinis mandiri.
7. Dokter menekan tombol **"Simpan Perubahan"** (`PUT .../summary`). Teks final dokter tersimpan di database.
8. Dokter menekan tombol **"Tandatangani Resume"** (`PATCH .../summary/sign`). Resume terkunci sebagai dokumen legal final.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `contracts/api-contract.md` `0.9.0` bagian 10.3 | Spesifikasi DTO `DischargeSummaryResponse`, `UpsertDischargeSummaryRequest`, dan `DischargeSummaryPrefillResponse` |
| `docs/module-blueprints/rawat-inap/episode-rawat-inap/03-frontend-architecture.md` 12.3.2 | Desain dan alur layar `FE-INP-22` |
| `docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-085.md` | Bukti ketersediaan skema 3 kolom baru dan endpoint resume 8 bagian |
| `docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-086.md` | Bukti implementasi dan perilaku endpoint prefill usulan data klinis |
| `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-discharge-view.jsx` | Formulir resume dan komponen presentasi dokumen |
| `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-discharge.jsx` | Hook orkestrasi pengambilan data prefill dan manipulasi form |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-discharge-constants.jsx` | Menambahkan batasan panjang teks (`importantFindingsSummary: 4000`, `dischargeConditionNote: 2000`, `educationSummary: 2000`), form defaults, dan enum `PREFILL_SOURCE_STATUS` |
| `src/lib/services/health-services/inpatient-management/inpatient-discharge.service.js` | Menambahkan helper `getSummaryPrefill` pada objek `inpatientDischargeService` |
| `src/utils/health-services/inpatient-management/inpatient-discharge-utils.jsx` | Memperbarui `normalizeDischargeSummary`, `normalizeDischargeRevisions`, `mapSummaryToForm`, `hasUnsavedSummaryChanges`, dan `buildDischargeSummaryPayload`; menambahkan normalizer `normalizeDischargeSummaryPrefill`, `getPrefillFieldSourceInfo`, dan `applyPrefillToSummaryForm` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-discharge.jsx` | Menambahkan state prefill (`prefillLoading`, `prefillSources`, `pendingPrefillData`, `prefillConfirmOpen`), handler `handleFetchPrefill`, `executeApplyPrefill`, `handleConfirmPrefill`, `handleCancelPrefill`, dan ekspos props return |
| `src/components/view/health-services/inpatient-management/inpatient-discharge-view.jsx` | Memperbarui `SUMMARY_FIELDS`, `SUMMARY_GROUPS` (8 kelompok berurutan), `DOCUMENT_GROUPS` (8 kelompok read-only), menambahkan toolbar `prefillToolbar` dan tombol "Isi dari Data Klinis", merender label sumber usulan (`prefillSourceBadge`), penanganan status unavailable, dan menambahkan `ConfirmModal` proteksi isian |
| `src/style/health-services/inpatient-management/inpatient-discharge-view.module.css` | Menambahkan CSS module rules untuk `.prefillToolbar`, `.prefillToolbarInfo`, `.fieldWrapper`, `.prefillSourceBadge`, `.prefillUnavailableNotice`, dan `.clinicalFieldGridSingle` |

### 3.3 Gerbang keputusan base component (`base-component-decision-gate.md`)

| Kebutuhan UI | Kandidat base | Bukti pemakaian | Status | Rekomendasi |
| --- | --- | --- | :---: | --- |
| Form Textarea (Pemeriksaan Penting, Kondisi Pulang, Edukasi) | `BaseTextAreaField` | `src/components/features/base-features/base-form-control.jsx` | `REUSE` | Gunakan `BaseTextAreaField` dengan properti maxLength dan row yang sesuai batas DTO backend |
| Tombol "Isi dari Data Klinis" | `BaseButton` | `src/components/features/base-features/base-button.jsx` | `REUSE` | Gunakan `BaseButton` varian `outline` dengan icon `FaClipboardCheck` |
| Label Sumber Usulan Klinis | CSS Module Token | `inpatient-discharge-view.module.css` | `COMPOSE` | Gunakan badge token dengan warna primer lembut (`--color-primary-soft`) dan ikon info |
| Keterangan Sumber Unavailable (AC-5) | CSS Module Token | `inpatient-discharge-view.module.css` | `COMPOSE` | Gunakan badge peringatan halus (`--color-warning`) dengan tulisan miring |
| Modal Konfirmasi Timpa Isian | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx` | `REUSE` | Gunakan `ConfirmModal` varian `info` dengan pesan konfirmasi pengisian bagian kosong |
| Status Badge Resume | `StatusBadge` | `src/components/features/base-features/status-badge.jsx` | `REUSE` | Menampilkan status Draft tersimpan / Final |

### 3.4 Checklist konsistensi visual (`ui-consistency-checklist.md`)

- [x] Tipografi menggunakan variabel desain global (`--font-size-small`, `--font-size-caption`, `--font-weight-medium`, `--font-weight-semibold`).
- [x] Spacing dan padding menggunakan token `--space-1`, `--space-2`, `--space-3`, `--space-4`.
- [x] Radius sudut menggunakan token `--radius-sm`, `--radius-md`, `--radius-lg`.
- [x] Seluruh warna border, background, dan text menggunakan `--color-*`.
- [x] Tidak ada warna literal `#hex`, `rgb()`, atau font family hardcode di file CSS yang baru.

---

## 4. Dokumentasi endpoint yang dikonsumsi

### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`  
Tag Swagger: `[Tags("Health Services / Inpatient Management / Inpatient Discharge")]`

| Method | Path | Deskripsi | Hak Akses | Request Body / Params | Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/{episodeId}/summary` | Mengambil data resume pulang 8 bagian beserta riwayat revisi | `InpatientDischarge : Read` | Query: `includeRevisions=true` | `ApiResponse<DischargeSummaryResponse>` memuat `importantFindingsSummary`, `dischargeConditionNote`, `educationSummary` |
| `PUT` | `/{episodeId}/summary` | Menyimpan draf resume 8 bagian (teks final dokter) | `InpatientDischarge : Update` | `UpsertDischargeSummaryRequest` | `ApiResponse<DischargeSummaryResponse>` |
| `GET` | `/{episodeId}/summary-prefill` | Mengambil usulan isian resume dari data klinis (*zero-persistence*) | `InpatientDischarge : Read` | — | `ApiResponse<DischargeSummaryPrefillResponse>` memuat field usulan, label sumber, status sumber, dan waktu |
| `PATCH` | `/{episodeId}/summary/sign` | Menandatangani resume medis final | `InpatientDischarge : Sign` | `SignDischargeSummaryRequest` (`note`) | `ApiResponse<DischargeSummaryResponse>` |

---

## 5. Acceptance criteria dan bukti verifikasi

| No | Kriteria Penerimaan | Bukti Implementasi | Status |
| :---: | :--- | :--- | :---: |
| **AC-1** | Tiga bagian baru tampil pada formulir resume dan tersimpan lewat kontrak `0.9.0`. | `ImportantFindingsSummary`, `DischargeConditionNote`, dan `EducationSummary` terpasang pada `SUMMARY_FIELDS`, `SUMMARY_GROUPS` (nomor 3, 6, 8), `DOCUMENT_GROUPS`, dan dipetakan pada `buildDischargeSummaryPayload`. | ✅ **PASS** |
| **AC-2** | Tombol "Isi dari data klinis" hanya aktif pada resume yang **belum** ditandatangani. | Tombol diberi kondisi `disabled={!summaryAuthority.canEdit \|\| Boolean(summary?.isSigned) \|\| prefillLoading \|\| summaryLoading}`. Begitu resume ditandatangani, formulir beralih ke `ResumeDocument` read-only dan tombol tidak aktif. | ✅ **PASS** |
| **AC-3** | Setiap bagian yang terisi usulan menampilkan **label sumbernya**. | Komponen `ClinicalGroup` membaca `prefillSources?.[fieldName]?.label` dan merender `prefillSourceBadge` bertuliskan `"Sumber: {label}"` di bawah textarea terkait. | ✅ **PASS** |
| **AC-4** | Menekan tombol usulan tidak mengirim permintaan simpan apa pun. | Handler `handleFetchPrefill` murni memanggil `inpatientDischargeService.getSummaryPrefill` dan melakukan manipulasi state `summaryForm` di client. Nol panggilan `PUT` ke server. | ✅ **PASS** |
| **AC-5** | Bagian yang sumbernya gagal dibaca tampil kosong berketerangan, dan bagian lain tetap terisi. | Utility `getPrefillFieldSourceInfo` mendeteksi `sourceStatus === 3` (`Unavailable`) dan merender `prefillUnavailableNotice` dengan pesan keterangan sumber (misal untuk data laboratorium) tanpa merusak pengisian bagian lain. | ✅ **PASS** |
| **AC-6** | Resume lama tanpa ketiga isian tetap terbuka dan tetap dapat ditandatangani. | Fungsi `normalizeDischargeSummary` memberikan nilai default aman `""` untuk ketiga field baru bila bernilai `null` dari server. `validateDischargeSignature` hanya mewajibkan diagnosis utama dan rujukan. | ✅ **PASS** |

---

## 6. Hasil validasi

### 6.1 Validasi otomatis

```bash
# Validasi ESLint Frontend (0 error)
cmd.exe /c npm run lint:errors
> quilvian-app-system@0.1.0 lint:errors
> eslint . --quiet
# Hasil: EXIT 0 (CLEAN)

# Validasi Kompilasi & Build Next.js App Router
cmd.exe /c npm run build
> quilvian-app-system@0.1.0 build
> next build
# Hasil: EXIT 0 (CLEAN - All routes compiled successfully)
```

### 6.2 Validasi manual

- **Struktur 8 Bagian**: Terverifikasi bahwa `SUMMARY_GROUPS` membagi layar secara urut dari 1 (Diagnosis) sampai 8 (Edukasi), dengan ukuran textarea dan batas karakter proporsional.
- **Interaksi Prefill**: Tombol "Isi dari data klinis" memuat usulan, menampilkan spinner loading saat permintaan berlangsung, dan menuangkan data tanpa aksi simpan.
- **Proteksi Isian Dokter**: Bila form sudah memiliki isi, modal konfirmasi mencegah penimpaan teks manual tanpa persetujuan dokter.
- **Label Sumber**: Label sumber tampil anggun di bawah masing-masing kotak isian dengan styling token Quilvian.

---

## 7. Catatan integrasi & langkah selanjutnya

1. **Sinkronisasi Antar Sub-Modul**:
   - Kontrak payload resume 8 bagian ini (`0.9.0` bagian 10.3) wajib digunakan persis sama oleh tab Resume Medis pada `dokter-rawat-inap` (`FE-RWI-074`).
2. **Kesiapan Roadmap Modul**:
   - Task `FE-RWI-064` telah selesai secara penuh dan terbukti.
   - Task frontend berikutnya pada Gelombang 1 adalah `FE-RWI-065` (Peringatan dan akibat penutupan episode — `FE-INP-23`) yang menunggu `BE-RWI-084`.
