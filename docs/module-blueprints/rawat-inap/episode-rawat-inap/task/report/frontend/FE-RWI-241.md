# Laporan Perubahan Frontend — `FE-RWI-241`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-241` |
| Judul | Edit Persetujuan General Consent (mode lihat dan mode edit) |
| Slice | Perbaikan `PLAN-REPAIR-EPS-008`, `FIX-EPS-008-02` |
| Roadmap | [`../../../roadmap/frontend-roadmap-admisi-pendaftaran.md`](../../../roadmap/frontend-roadmap-admisi-pendaftaran.md) — register dan kartu 4.6 |
| Trace | [`ISSUE-EPS-008`](../../../docs/issue/issue-008-edit-riwayat-general-consent.md) `ISS-EPS-008-01`, `-T1`, `-T2`; [`PLAN-REPAIR-EPS-008`](../../../docs/plan-repair/plan-repair-008-edit-riwayat-general-consent.md) bagian 3.2, 3.3, dan 4; `RWI-DEC-272`, `RWI-DEC-273` |
| Contract version | Kontrak `0.11.0` + delta `BE-RWI-227` (`POST …/admission-workspace/general-consent/revisions`; field versi dan panduan pada `GET …/consent`), disetujui lewat rencana perbaikan 13 Oktober 2026 |
| Wewenang UI | Skema tampilan SESUDAH pada rencana perbaikan bagian 3.2–3.3, disetujui pemilik. Tidak mengubah langkah admisi |
| Dependency | `BE-RWI-227` ✅ |
| Klasifikasi | `MEDIUM` — satu section diubah, satu komponen view baru, satu utility baru, satu hook diperluas |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `HamzahV2` (berkas di bagian 3.2), laporan ini, roadmap, traceability, register rencana perbaikan |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f4495aa1` (`HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `4ec02370` (`MHamzah`) + perubahan `BE-RWI-227` yang belum di-commit |
| Tanggal | 13 Oktober 2026 |
| Status | ✅ Selesai pada tingkat source, lint, unit test, dan build. Uji layar terhadap backend baru `NOT FEASIBLE` (lihat bagian 6) |

---

## 1. Keadaan yang ditemukan di awal

- Tab Formulir General Consent menampilkan persetujuan bertanda tangan sebagai teks baca-saja,
  tanpa jalur koreksi (`general-consent-section.jsx` versi `f4495aa1`, baris 181–207).
- Panel "Penerimaan Panduan Rawat Inap" tetap dapat diklik walau dokumen bertanda tangan. Pilihannya
  hanya state layar dan hilang saat halaman ditutup (`use-admission-general-consent.js:21-28`,
  section baris 307–324).
- Cetakan V1 mencentang hubungan dari `choices.relationship`, yang kosong untuk dokumen bertanda
  tangan, sehingga tidak ada hubungan yang tercentang (`general-consent-form-print.jsx:97-104`).

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas PPRI membuka Workspace PPRI → General Consent → tab Formulir General Consent.
2. Bila persetujuan sudah ditandatangani digital, layar menampilkan:
   - jenis penanda tangan, keterangan hubungan, nama, NIK, telepon, alamat, dan waktu tanda tangan;
   - panduan rawat inap yang tersimpan;
   - lencana "Versi N" dan baris "Versi N · terakhir diubah <nama>, <waktu>".
3. Pengguna yang berhak `InpatientAdmissionDocument : Update`, pada episode yang masih berjalan,
   melihat tombol **Edit Persetujuan**.
4. Edit Persetujuan membuka form yang terisi dari versi aktif: jenis penanda tangan, nama,
   keterangan hubungan, NIK, telepon, alamat, panduan, tanda tangan, dan alasan perubahan.
5. Petugas memperbaiki isian lalu menekan **Simpan Perubahan**. Tombol ini selalu dapat ditekan:
   - Bila ada isian yang belum sesuai, muncul "Periksa N isian yang belum sesuai.", lalu layar
     menggulir dan fokus ke isian pertama yang salah.
   - Bila valid, layar mengirim tepat satu permintaan koreksi. Bila berhasil, muncul toast
     "Persetujuan diperbarui — Tersimpan sebagai versi 2…", form tertutup, lalu data dan cetakan
     dimuat ulang.
6. Kedua tab (Surat Persetujuan dan Formulir) memakai data versi baru.

**Jalur tidak normal**

| Kejadian | Yang dilihat petugas |
| --- | --- |
| Alasan kosong atau kurang dari 10 karakter | "Alasan perubahan minimal 10 karakter.", fokus ke isian alasan, tanpa permintaan ke server |
| Tidak ada isian yang diubah | "Tidak ada data yang berubah. Ubah isian yang salah sebelum menyimpan." |
| Jenis penanda tangan diganti | Kanvas tanda tangan terbuka otomatis, dengan peringatan "Jenis penanda tangan berubah. Bubuhkan tanda tangan penanda tangan yang baru." Simpan tanpa goresan ditolak di layar |
| Bayi baru lahir | Pilihan "Pasien Sendiri" tidak dapat dipilih, ditambah peringatan |
| Diubah petugas lain (409) | Form tetap terbuka dengan isian petugas, pesan server "Persetujuan sudah diubah petugas lain…". Batal memuat ulang versi terbaru |
| Episode tertutup atau tanpa hak `Update` | Tombol Edit tidak tampil; tampilan baca dan cetak tetap ada |
| Episode lama tanpa persetujuan tersimpan | Perilaku cetak saja yang lama tetap berlaku |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`general-consent-section.jsx`, `general-consent-form-print.jsx`, `use-admission-general-consent.js`,
`inpatient-admission-workspace.service.js`, `inpatient-admission-print-utils.js`,
`inpatient-admission-workspace-utils.js` (`extractApiError`, `formatLongDateTime`),
`admission-radio-group.jsx`, `base-form-control.jsx` (`BaseTextField`, `BaseTextAreaField`,
`useFieldIdentity`), `information-alert.jsx`, `status-badge.jsx`, `admission-workspace-view.jsx`
(`addToast`, `can`, `isWritable`), `use-inpatient-admission-workspace.js`,
`inpatient-admission-consent-form-step.jsx` (pola kanvas; tidak diubah), CSS module Workspace PPRI dan
admisi, serta kontrak backend `BE-RWI-227`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | `ADMISSION_CONSENT_SIGNER_TYPES`: lima jenis penanda tangan dengan label yang sama dengan langkah admisi |
| `src/utils/health-services/inpatient-management/inpatient-admission-consent-edit-utils.js` (baru) | Fungsi murni: nilai awal, validasi yang mencerminkan server, deteksi perubahan, payload revisi, label jenis dan panduan, serta pemetaan hubungan cetakan (`ISS-EPS-008-T2`) |
| `src/lib/services/health-services/inpatient-management/inpatient-admission-workspace.service.js` | `reviseAdmissionConsent` → `POST {episodeId}/admission-workspace/general-consent/revisions` |
| `src/lib/hooks/health-services/inpatient-management/use-admission-general-consent.js` | `saveRevision`, `saving`, `saveError`, `clearSaveError`, `isNewborn`, dan `printChoices` (hubungan dan panduan dari data tersimpan) |
| `src/components/view/.../sections/general-consent/general-consent-edit-form.jsx` (baru) | Form Edit Persetujuan sesuai skema 3.3 |
| `src/components/view/.../sections/general-consent/general-consent-section.jsx` | Mode lihat dan mode edit; tombol Edit (hak `Update` + episode berjalan); lencana versi; panduan baca-saja; cetakan memakai `printChoices`; teks *read-only* lama diganti |
| `src/style/health-services/inpatient-management/admission-workspace.module.css` | Class `signaturePad*` untuk kotak tanda tangan, token saja |
| `tests/unit/inpatient-admission-consent-edit.test.mjs` (baru) | 8 test fungsi murni |

Berkas yang **tidak** disentuh karena sedang diubah pekerjaan lain: `inpatient-admission-consent-form-step.jsx`,
`inpatient-consent-form.jsx`, `inpatient-admission.module.css`, `inpatient-consent-print.module.css`.

### 3.3 Kepatuhan arsitektur frontend

Alurnya view → hook → service (`inpatientEpisodeService`/`InstanceAxios`); view tidak memanggil
Axios langsung. Normalisasi, validasi, dan payload berada di `utils`. Konstanta ada di `constants`.
Tidak ada Redux slice baru, karena data hanya dipakai satu section, sama dengan pola hook ini
sebelumnya.

**Gerbang keputusan base component**

`UI GATE: 10 elemen — REUSE 8, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Lencana tanda tangan dan versi | `StatusBadge` | `base-features/status-badge.jsx`, sudah dipakai section | REUSE | Dua lencana di `actions` panel |
| Baris "terakhir diubah" | class `formCardHint` | `admission-workspace.module.css` | REUSE | — |
| Tombol Edit, Batal, Simpan, Ganti/Pakai Tanda Tangan | `BaseButton` | `base-features/base-button.jsx` | REUSE | `loading`/`loadingLabel` pada Simpan |
| Jenis penanda tangan, panduan Ya/Tidak | `AdmissionRadioGroup` (`chips`) | `admission-workspace/components/admission-radio-group.jsx`, mendukung `option.disabled` dan `error` | REUSE | — |
| Nama, keterangan hubungan, NIK, telepon, keterangan panduan | `BaseTextField` | `base-form-control.jsx:233` | REUSE | `field` + `data-testid` untuk fokus |
| Alamat, alasan perubahan | `BaseTextAreaField` | `base-form-control.jsx:424` | REUSE | — |
| Ringkasan galat, info, galat server | `InformationAlert` | `base-features/information-alert.jsx` (kosong → tidak dirender) | REUSE | — |
| Panel baca penanda tangan dan panduan | `readOnlyPanel`/`readOnlyGrid` | Pola section yang ada | REUSE | — |
| Kotak tanda tangan | `react-signature-canvas` + CSS module | Tidak ada base component tanda tangan (`ls base-features` dan `grep -rln react-signature-canvas` → hanya dua layar langkah yang memakainya langsung) | COMPOSE | Lihat keputusan di bawah |
| Kerangka form edit | `formCard`/`formCardSection`/`formGrid` | CSS module Workspace PPRI | COMPOSE | Dirangkai di view |

> **Keputusan: kotak tanda tangan pada form edit**
>
> - **A. Rangkai `react-signature-canvas` langsung di view, dengan class CSS milik modul Workspace PPRI — Rekomendasi (dipakai).** Polanya sama dengan dua layar langkah yang sudah ada, tanpa dependency baru dan tanpa menyentuh berkas yang sedang diubah pekerjaan lain. Biaya kecil.
> - **B. Memakai ulang class kanvas dari `inpatient-admission.module.css`.** Visualnya identik dengan langkah admisi. Namun lima dari sembilan class itu hanya ada di working tree pekerjaan lain yang belum di-commit (diperiksa dengan `git show HEAD:…`), sehingga tampilan bisa rusak diam-diam bila pekerjaan itu berubah.
> - **C. Mengangkat komponen `SignaturePad` baru ke `components/ui` dan memakainya di tiga layar.** Paling rapi jangka panjang, tetapi berstatus `NEW` dan mengubah dua layar langkah. Perlu keputusan pemilik; dicatat sebagai usulan.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil data cetak General Consent..." (tetap) |
| Kosong (belum ada persetujuan tersimpan) | Perilaku cetak saja yang lama: pilihan hubungan dan panduan dari layar; tombol Edit tidak tampil |
| Gagal memuat | "Data cetak tidak dapat dimuat" + Coba Lagi (tetap) |
| Gagal menyimpan | Alert "Persetujuan belum tersimpan" berisi pesan server; form tetap terbuka |
| Tanpa hak akses `Update` / episode tertutup | Tombol Edit tidak tampil; data dan cetak tetap |
| Menyimpan | Tombol "Menyimpan..." terkunci selama permintaan berjalan; tombol lain di form terkunci |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/general-consent/revisions` | Simpan Perubahan | `InpatientAdmissionDocument : Update` |
| `GET` | `…/admission-workspace/general-consent/print-data` | Data cetak (tetap) | `InpatientAdmissionDocument : Read` |

#### InpatientAdmissionConsent

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/consent` | Versi aktif beserta `versionNo`, `rowVersion`, panduan, dan "terakhir diubah" | `InpatientEpisode : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --quiet` pada 7 berkas source + 1 test yang disentuh | exit 0 | `PASS` | Keluaran perintah |
| `npx eslint` (dengan warning) pada berkas yang sama | 0 error, 1 warning `react-hooks/set-state-in-effect` di `use-admission-general-consent.js:61` — baris lama `setLoading(true)` di effect, bukan baris baru | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| Koreksi 13-10-2026 saat `FE-RWI-242` | Perbaikan `penColor` sempat memakai `useMemo(readPenColor, [])` dan memunculkan warning baru `react-hooks/use-memo` di `general-consent-edit-form.jsx`, yang tidak terlihat karena lint terakhir dijalankan `--quiet`. Diperbaiki menjadi `useMemo(() => readPenColor(), [])`; lint ulang kembali ke 0 error, 1 warning lama | `PASS` sesudah perbaikan | Laporan `FE-RWI-242` bagian 6 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-consent-edit.test.mjs tests/unit/inpatient-admission-consent.test.mjs` | 15/15 lulus | `PASS` | Keluaran perintah |
| `tests/unit/inpatient-admission-workspace.test.mjs` | 14/15. Kegagalan pertama dari berkas baru (literal warna `penColor`) **diperbaiki** dengan token `--color-text-strong`. Sisa 1 gagal: "FE-RWI-212: alamat Workspace PPRI…" mengharapkan 10 menu, kenyataannya 9 — konstanta menu tidak disentuh (commit terakhir `2d0a6890e`); termasuk garis dasar 13-10-2026 relokasi menu Transfer | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah; `git status` berkas konstanta bersih |
| `npm run build` | Berhasil: "Compiled successfully in 2.4min", 492/492 halaman, postbuild standalone siap | `PASS` | Log `fe-build-241.log` di scratchpad sesi. Dev server pengguna (`next dev`, keluaran `.next/dev`) tidak dihentikan |
| Grep anti-regresi checklist UI G-1…G-6 pada berkas yang disentuh | Kosong semua; literal hex di JSX baru: tidak ada | `PASS` | Keluaran perintah |
| Pengenal UUID tampil atau diinput | Tidak ada; `rowVersion` hanya dikirim, tidak ditampilkan | `PASS` | Review diff |

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-consent-edit.test.mjs tests/unit/inpatient-admission-consent.test.mjs` — PASS (15/15)

MANUAL TEST: NOT FEASIBLE — backend lokal pengguna berjalan `dotnet run --no-build` dengan DLL lama (proses 18808, sejak 10.49), sehingga endpoint `general-consent/revisions` belum aktif. Menghentikan proses pengguna tidak diizinkan. Skenario uji ada pada `PLAN-REPAIR-EPS-008` bagian 8.

**Tidak dijalankan:** `npm run lint:errors` (rusak oleh lingkungan, lihat memori `perintah-validasi-frontend-rusak`; diganti `npx eslint --quiet` per berkas) dan suite unit penuh (garis dasar 13-10-2026 sudah 34 gagal, bukan gerbang).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (`FIX-EPS-008-02`) | Status | Bukti |
| --- | --- | --- |
| 1. Tombol Edit hanya untuk persetujuan bertanda tangan + hak `Update` + episode berjalan | Terpenuhi di source | `canEditConsent` = `isConsentSigned && isWritable && can("Update")` |
| 2. Edit mengisi form dari versi aktif | Terpenuhi di source + unit test | `buildConsentEditInitialValues`; test "nilai awal…" |
| 3. Alasan kosong → tanpa permintaan, pesan, fokus; tombol tidak pernah abu-abu sebelum diklik | Terpenuhi di source + unit test | `handleSubmit` + `focusField`; Simpan hanya `loading`; test "alasan kurang dari 10 karakter…" |
| 4. Ubah NIK → tepat satu `POST`, lalu versi baru tampil di mode lihat dan tab Surat Persetujuan | Terpenuhi di source; uji layar `NOT FEASIBLE` | `saveRevision` → `setSignedConsent` + muat ulang; `consentDocument` memakai `signedConsent` |
| 5. Ganti jenis penanda tangan tanpa tanda tangan baru → peringatan, tidak terkirim | Terpenuhi di source + unit test | `handleSignerTypeChange` membuka kanvas; `validateConsentEdit`; test K-01 |
| 6. Hubungan tercentang pada cetakan: `Spouse`+"Istri" → Istri, `Self` → Diri Sendiri, `Guardian` → Lainnya | Terpenuhi + unit test | `resolveGeneralConsentRelationship` lewat `printChoices` |
| 7. Panduan tersimpan baca-saja dan tercentang di cetakan; tidak ada radio aktif di mode lihat | Terpenuhi di source | Blok `gc-guidance-readonly`; radio hanya untuk persetujuan yang belum tersimpan |
| 8. Episode lama tanpa persetujuan → perilaku cetak saja tetap | Terpenuhi di source | Cabang `!consent.isConsentSigned` tidak diubah isinya |
| 9. Tidak ada UUID tampil | Terpenuhi | Review diff dan grep |
| DoD: lint dan build | Terpenuhi | Bagian 6 |
| DoD: uji layar | Belum | `MANUAL TEST: NOT FEASIBLE`; dikecualikan sesuai keputusan pemilik bahwa uji runtime dijalankan pemilik |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu warning ESLint lama di hook (bukan baris baru) |
| Masalah yang diketahui | Langkah admisi (`inpatient-admission-consent-form-step.jsx`) masih punya daftar jenis penanda tangan sendiri dan tombol simpan yang dimatikan (`ISS-EPS-008-T3`); tidak disentuh karena sedang diubah pekerjaan lain |
| Dependency backend | `BE-RWI-227` ✅ di source; perlu backend dibangun ulang dan dijalankan sebelum uji layar |
| Perubahan sampingan | `NONE`. Build memperbarui `.next/` (diabaikan Git) |
| Interupsi | `NONE` |
| Status Git | Source task ini: 5 berkas `M` dan 3 berkas `??` baru (`general-consent-edit-form.jsx`, `inpatient-admission-consent-edit-utils.js`, `tests/unit/inpatient-admission-consent-edit.test.mjs`). Perubahan pekerjaan lain di working tree tidak disentuh |
| Langkah berikutnya | `FE-RWI-242` (panel Riwayat Perubahan), lalu `FIX-EPS-008-04` (dokumen blueprint); pemilik menjalankan ulang backend dan menguji skenario rencana bagian 8 |
