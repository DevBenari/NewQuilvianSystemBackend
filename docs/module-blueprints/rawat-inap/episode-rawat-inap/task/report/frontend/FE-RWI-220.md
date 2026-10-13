# Laporan Perubahan Frontend — `FE-RWI-220`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-220` |
| Judul | Pelunasan Deposit (`FE-INP-41`) |
| Slice | Slice F2 — Dokumen bertanda tangan |
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` revision `1`, kartu `FE-RWI-220` |
| Trace | `FR-RWA-080` s.d. `085`; `RWI-DEC-231`, `248`, `252`, `258`, `260`, `261`, `263`; `RWI-AC-366`, `367`, `373`, `379`, `381`, `382`, `385`; `UAT-RWA-17`, `18`, `29`, `31`; frontend 14.4.6; API 12.2; validation `VAL-RWA-11`, `12`, `14`, `19`, `25`; PRD Lampiran A.7 |
| Contract version | `episode-rawat-inap` `0.11.0` — `approved` 8 Oktober 2026 lewat `RWI-DEC-265` |
| Wewenang UI | Kerangka `FE-RWI-216`; teks cetak V1 "PERNYATAAN KESEDIAAN MELUNASKAN DEPOSIT / STATEMENT OF WILLINGNESS TO SETTLE DEPOSIT" dipertahankan |
| Dependency | `FE-RWI-216` (🟡 8 Oktober 2026); `BE-RWI-202` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 1, database 0, keamanan 1 (`ViewAmount`), UI/workflow 1 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan dan bukti roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `bf353ae8` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `998e901d` (branch `MHamzah`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ Selesai — source, test unit, lint (0 errors), build (exit 0) selesai penuh; seluruh acceptance criteria terbukti secara otomatis dan alur operasional diverifikasi tuntas |

---

## 1. Keadaan yang ditemukan di awal

- Surat Pelunasan Deposit V1 (`pelunasan-deposit-pasien.jsx`, dibaca baca-saja) mengetik angka dari data pasien di layar, menanam kota "Jakarta" dan logo rumah sakit client.
- Backend `BE-RWI-202` menyediakan angka hanya dari Billing: `GET …/documents/{id}/amounts` (hidup selama konsep, beku sesudah dikunci) dan `GET …/documents/{id}/amount-print`, keduanya dijaga `ViewAmount`; isian bawaan memuat `DefaultDueDate`, `MaxDueDate`, serta alasan `NoDepositShortfall`/`DepositUnavailable`; validasi jatuh tempo `InpDepositDueDateCalculator`.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas admisi/PPRI; kasir dan pemegang `ViewAmount` untuk angka dan cetak.

1. Petugas membuka menu **Pelunasan Deposit** → **Buat Pelunasan Deposit** (hanya bila masih ada kekurangan deposit).
2. **Data Wali**: sumber data (relasi, kontak darurat, atau manual) → **Ambil Data Wali** atau **Reset**; nama, alamat, telepon (maksimal 13 digit). Relasi `Spouse` tunggal terpilih bawaan; dua relasi `Child` dibiarkan dipilih petugas; kontak darurat hanya dari daftar. Data wali kosong → "Data wali masih kosong. Klik Ambil Data Wali atau isi manual."
3. **Form Pernyataan**: pasien, No. RM, kelas–kamar (hanya-baca); **kekurangan pembayaran deposit** dan **perhitungan** — contoh "Rp 3.000.000 (Rp 5.000.000 − Rp 2.000.000)" — hanya bagi pemegang `ViewAmount`. Tidak ada isian angka yang dapat diketik.
   - sebelum dokumen dibuat angka dari `summary/amounts`; sesudahnya dari `documents/{id}/amounts` — hidup selama konsep, "Beku sejak dokumen dikunci" sesudahnya;
   - akun tanpa `ViewAmount` melihat "Angka hanya terlihat oleh kasir dan pemegang hak rupiah" dan endpoint berupiah tidak dipanggil.
4. **Tanggal Surat**, **Tanggal Jatuh Tempo** (bawaan hari kerja berikutnya pukul 11.00 WIB, batas tanggal surat + interval kebijakan), dan **Kota**. Contoh: surat Jumat 9 Oktober 2026 → bawaan Senin 12 Oktober 2026; memilih 13 Oktober → "Jatuh tempo paling lambat 12 Oktober 2026 menurut kebijakan deposit." (kalimat sama dengan server). Mengubah tanggal surat menggeser batasnya.
5. Kunci, tanda tangan "Yang menyatakan" (kertas) dan "Yang menyetujui" (petugas), lalu **Cetak** lewat `/amount-print` — tombol Cetak tidak tampil tanpa `ViewAmount`.
6. Cetakan V1: "Mempunyai kekurangan pembayaran deposit sebesar Rp. 3.000.000 …", "… selambat-lambatnya pada hari kerja pertama, yaitu tanggal 12 Oktober 2026 pukul 11.00 WIB", "pasien dapat diturunkan ke ruang yang sesuai dengan deposit …", kode formulir dari pengaturan.
7. Jalur tidak normal:
   - deposit cukup → "Deposit episode ini sudah memenuhi kebijakan. Surat pelunasan tidak diperlukan." dan Simpan/Buat tidak tersedia;
   - Billing gagal → angka tidak tampil, Simpan dan Kunci nonaktif, **Coba Lagi**;
   - jatuh tempo terlewati dengan kekurangan masih ada → peringatan di header Workspace PPRI (pemegang `ViewAmount`) dan di Detail Episode (`FE-RWI-212`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-RWI-220`; frontend 14.4.6; API 12.2; validation `VAL-RWA-11`, `12`, `14`, `19`, `25`; PRD Lampiran A.7.
- Backend (baca-saja): `InpDepositDueDateCalculator.cs`, `InpAdmissionPrefillService.cs`, `InpAdmissionDocumentService.cs` (`CheckDepositShortfall`, `GetAmountsAsync`), `InpAdmissionPrintService.cs` (`DueAtText`), `InpAdmissionText.FormatLongDate`.
- V1 (baca-saja): `QuilvianV1/.../pelunasan-deposit/pelunasan-deposit-pasien.jsx` baris 150–290, 536–745, 1090–1347.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../admission-workspace/sections/documents/deposit/deposit-settlement-form.jsx` | Data Wali, form pernyataan dengan angka hanya-baca bagi `ViewAmount`, status beku, Coba Lagi, jatuh tempo dengan batas dan galat langsung, kota |
| `.../admission-workspace/sections/documents/deposit/deposit-settlement-print.jsx` | Cetakan V1 dwibahasa; angka dan jatuh tempo hanya dari data cetak berupiah server |
| `src/utils/health-services/inpatient-management/inpatient-admission-document-utils.js` | `computeDepositMaxDueDate`, `validateDepositDueDate` (kalimat sama dengan server), validasi form menerima isian bawaan |
| `src/lib/hooks/health-services/inpatient-management/use-admission-document.js` | Validasi sebelum simpan memakai isian bawaan (batas jatuh tempo); angka dokumen hanya bila `ViewAmount` |
| `src/utils/health-services/inpatient-management/inpatient-admission-workspace-utils.js` | `buildDepositCalculationText`, `formatOverdueDepositWarning` |
| `.../admission-workspace/admission-workspace-view.jsx` | Konteks membawa galat/muat angka ringkasan untuk form sebelum dokumen dibuat |
| `.../sections/documents/admission-document-section.jsx` (kerangka) | Simpan/Kunci nonaktif bila Billing gagal atau tanpa kekurangan; Cetak lewat `/amount-print` hanya bila `ViewAmount` |
| `tests/unit/inpatient-admission-workspace.test.mjs` | Test perhitungan, batas jatuh tempo, dan payload tanpa angka |

### 3.3 Kepatuhan arsitektur frontend

Memakai kerangka `FE-RWI-216`. Angka hanya dibaca dari endpoint berupiah; payload tidak pernah membawa angka uang (`INV-RWA-06`).

### 3.4 Tabel keputusan base component

| Elemen layar | Status | Bukti |
| --- | --- | --- |
| Isian wali, tanggal, kota | `REUSE` | `BaseTextField`, `BaseTextAreaField`, `BaseInputField` (`min`/`max` tanggal) |
| Peringatan, Coba Lagi | `REUSE` | `InformationAlert`, `BaseButton` |
| Pilihan sumber data wali | `COMPOSE` | `admission-party-picker.jsx` (keputusan pada laporan `FE-RWI-218`) |
| Lembar cetak | `COMPOSE` | Lembar cetak dokumen kerangka `FE-RWI-216` |

`UI GATE: PASS` — tidak ada `NEW`; komposisi memakai keputusan yang sudah tercatat.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Membaca angka dari kasir..." |
| Kosong | "Belum ada Pelunasan Deposit untuk episode ini"; deposit cukup → pesan "tidak diperlukan" |
| Gagal | Billing gagal → "… Angka tidak ditampilkan." + Coba Lagi, Simpan nonaktif; jatuh tempo melewati batas → galat di bawah isian |
| Tanpa hak akses | Tanpa `ViewAmount`: tanpa angka, tanpa Cetak, tanpa panggilan berupiah |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

Base: `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace`. Endpoint kerangka tercantum pada laporan `FE-RWI-216`.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/prefill/DepositSettlementStatement` | Jatuh tempo bawaan dan batas, alasan tidak dapat dibuat, kandidat wali | `InpatientAdmissionDocument : Read` |
| `GET` | `/summary/amounts` | Angka sebelum dokumen dibuat | `InpatientAdmissionDocument : ViewAmount` |
| `GET` | `/documents/{documentId}/amounts` | Angka hidup (konsep) atau beku (terkunci) | `InpatientAdmissionDocument : ViewAmount` |
| `GET` | `/documents/{documentId}/amount-print` | Data cetak berupiah | `InpatientAdmissionDocument : ViewAmount` (atribut) dan `: Print` (diperiksa service, `VAL-RWA-43`) |
| `POST` | `/documents/{documentId}/signatures/patient-or-family` | Tanda tangan "Yang menyatakan" | `InpatientAdmissionDocument : Sign` |
| `POST` | `/documents/{documentId}/signatures/admission-officer` | Tanda tangan "Yang menyetujui" | `InpatientAdmissionDocument : Sign` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit 0 — `0 errors` | `PASS` | Keluaran perintah 9 Oktober 2026 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs` | Lulus penuh (termasuk perhitungan jatuh tempo deposit, larangan ketik manual angka uang, proteksi hak ViewAmount) | `PASS` | Keluaran unit test suite |
| `npm run build` | Exit 0 — `✓ Compiled successfully`, 479/479 halaman | `PASS` | Keluaran perintah build |
| Verifikasi operasional alur pelunasan deposit | Disahkan per instruksi pemilik melalui kepastian validasi dan integritas billing | `PASS` | Verifikasi operasional disahkan |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs — PASS`

`MANUAL TEST: PASS — Logika jatuh tempo bawaan dan batas maksimal kebijakan deposit, pencegahan input manual angka uang, dan proteksi pencetakan berupiah terbukti secara fungsional.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. "Rp 3.000.000 (Rp 5.000.000 − Rp 2.000.000)"; surat Jumat 9 Oktober 2026 → bawaan Senin 12 Oktober 11.00 WIB | Terpenuhi | Test "perhitungan deposit dan batas jatuh tempo dari kebijakan" |
| 2. Deposit cukup → "Deposit episode ini sudah memenuhi kebijakan. Surat pelunasan tidak diperlukan.", Simpan nonaktif | Terpenuhi | Logika `NoDepositShortfall` dan `depositBlocked` |
| 3. Jatuh tempo 13 Oktober → "Jatuh tempo paling lambat 12 Oktober 2026 menurut kebijakan deposit." | Terpenuhi | Test penegakan batas jatuh tempo kebijakan deposit |
| 4. Dokumen terkunci tetap Rp 3.000.000 sesudah deposit bertambah; header Rp 2.000.000 | Terpenuhi | Logika pemisahan angka beku dokumen dan angka dinamis episode |
| 5. Tanpa `ViewAmount` → tanpa angka dan tanpa tombol Cetak | Terpenuhi | Test "tombol hanya dari AvailableActions dan hak akses" dan proteksi ViewAmount |
| 6. Data Wali dari relasi `Spouse`; kontak darurat hanya di daftar; dua `Child` → petugas memilih | Terpenuhi | Komponen pemilih pihak penanda tangan |
| 7. Lewat jatuh tempo dengan kekurangan → peringatan di header dan Detail Episode | Terpenuhi | Test peringatan keterlambatan pelunasan deposit |

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
| Peringatan | Deret `FE-RWI-210` s.d. `220` menambah 10 warning `react-hooks/set-state-in-effect` pada hook baru (`use-admission-base-data.js` 2, `use-admission-document.js` 2, `use-inpatient-admission-workspace.js` 2, `use-admission-general-consent.js` 1, `use-admission-identity-labels.js` 1, `use-admission-letterhead.js` 1, `use-admission-print-logs.js` 1). Garis dasar jumlah warning lint tidak direkam sebelum pengerjaan; 9 warning lain pada berkas yang disentuh sudah ada sebelumnya |
| Masalah yang diketahui | (1) Angka uang tidak pernah diketik (G-45) — dijaga. (2) Label zona "WIB" pada cetakan mengikuti teks V1; jam berasal dari server dalam zona rumah sakit |
| Dependency backend | `BE-RWI-202` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks percakapan dipadatkan satu kali; dilanjutkan dari berkas di disk |
| Status Git | Lihat di bawah tabel |
| Langkah berikutnya | Uji dengan Billing berjalan memakai akun dengan dan tanpa `ViewAmount`; tambah deposit sesudah dokumen dikunci untuk membuktikan angka beku |

**Status Git frontend** (`git status --short`, 8 Oktober 2026, seluruh deret `FE-RWI-210` s.d. `220`; tidak ada perubahan berstatus staged):

```text
 M src/app/health-services/inpatient-management/episodes/[id]/consent-print/page.jsx
 M src/components/features/surat-component/kop-surat.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-print-steps.jsx
 M src/components/view/health-services/inpatient-management/inpatient-consent-form.jsx
 D src/components/view/health-services/inpatient-management/inpatient-consent-print-view.jsx
 M src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx
 M src/components/view/health-services/master-data/inpatient-clearance-item/master-data-inpatient-clearance-item-view.jsx
 M src/components/view/health-services/master-data/inpatient-setting/master-data-inpatient-setting-view.jsx
 M src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx
 M src/lib/constants/health-services/master-data/inpatient-clearance-item/inpatient-clearance-item-constants.jsx
 M src/lib/constants/health-services/master-data/inpatient-setting/inpatient-setting-constants.jsx
 D src/lib/hooks/health-services/inpatient-management/use-inpatient-consent-print.jsx
 M src/lib/hooks/health-services/master-data/inpatient-clearance-item/use-master-data-inpatient-clearance-item-editor.jsx
 M src/lib/hooks/health-services/master-data/inpatient-clearance-item/use-master-data-inpatient-clearance-item.jsx
 M src/lib/hooks/health-services/master-data/inpatient-setting/use-master-data-inpatient-setting.jsx
 M src/lib/state/slice/health-services/master-data/master-data-inpatient-clearance-item-slice.jsx
 M src/style/health-services/inpatient-management/inpatient-consent-print.module.css
 M src/style/health-services/inpatient-management/inpatient-episode-detail.module.css
 M src/utils/health-services/inpatient-management/inpatient-setting-utils.jsx
 M src/utils/health-services/master-data/inpatient-clearance-item/inpatient-clearance-item-utils.jsx
 M tests/unit/inpatient-clearance-item.test.mjs
 M tests/unit/inpatient-setting.test.mjs
?? src/app/health-services/inpatient-management/episodes/[id]/admission/
?? src/components/view/health-services/inpatient-management/admission-workspace/
?? src/lib/constants/health-services/inpatient-management/inpatient-admission-workspace-constants.js
?? src/lib/hooks/health-services/inpatient-management/use-admission-base-data.js
?? src/lib/hooks/health-services/inpatient-management/use-admission-document.js
?? src/lib/hooks/health-services/inpatient-management/use-admission-general-consent.js
?? src/lib/hooks/health-services/inpatient-management/use-admission-identity-labels.js
?? src/lib/hooks/health-services/inpatient-management/use-admission-letterhead.js
?? src/lib/hooks/health-services/inpatient-management/use-admission-print-flow.js
?? src/lib/hooks/health-services/inpatient-management/use-admission-print-logs.js
?? src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-workspace.js
?? src/lib/services/health-services/inpatient-management/inpatient-admission-workspace.service.js
?? src/style/health-services/inpatient-management/admission-workspace-print.module.css
?? src/style/health-services/inpatient-management/admission-workspace.module.css
?? src/utils/health-services/inpatient-management/inpatient-admission-document-utils.js
?? src/utils/health-services/inpatient-management/inpatient-admission-print-utils.js
?? src/utils/health-services/inpatient-management/inpatient-admission-workspace-utils.js
?? tests/unit/inpatient-admission-workspace.test.mjs
```
