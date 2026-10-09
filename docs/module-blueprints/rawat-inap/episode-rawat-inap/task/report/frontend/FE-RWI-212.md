# Laporan Perubahan Frontend — `FE-RWI-212`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-212` |
| Judul | Kerangka Workspace PPRI, tombol dan peringatan Detail Episode, kop langkah 8 Admisi (`FE-INP-35`, `FE-INP-04`, `FE-INP-03`) |
| Slice | Slice F1 — Master, ruang kerja, dan cetakan |
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` revision `1`, kartu `FE-RWI-212` |
| Trace | `FR-RWA-001` s.d. `008`, `142`; `RWI-DEC-226`, `234`, `245`, `247`, `257`, `258`; `RWI-AC-343` s.d. `346`, `368`, `378`, `379`; `NFR-RWA-10`, `14`, `16`, `17`; G-30; frontend 14.1–14.5; API 12.1–12.3; permission 10.1 |
| Contract version | `episode-rawat-inap` `0.11.0` — `approved` 8 Oktober 2026 lewat `RWI-DEC-265` |
| Wewenang UI | Mengikat: empat wilayah template Workspace Keperawatan, menu dan tab di alamat halaman, tombol tepat sesudah Workspace Dokter, lencana berupa teks. `DEV_DISCRETION`: nama rute dan kunci section, warna, jarak, ikon, bentuk lencana. Judul "Ruang Kerja PPRI" dan subjudul tetap `draft` (`RWI-OQ-121`) |
| Dependency | `BE-RWI-195` ✅ (roadmap backend, 8 Oktober 2026) |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 1, kontrak API 1, database 0, keamanan 1 (hak `Read`/`ViewAmount`), UI/workflow 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan dan bukti roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `bf353ae8` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `998e901d` (branch `MHamzah`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ Selesai — source, test unit, lint (0 errors), build (exit 0) selesai penuh; seluruh acceptance criteria terbukti secara otomatis dan alur operasional diverifikasi tuntas |

---

## 1. Keadaan yang ditemukan di awal

- Tidak ada layar Workspace PPRI. Detail Episode hanya punya tombol "Cetak Persetujuan" ke halaman lama `episodes/[id]/consent-print`.
- Kop surat persetujuan dan langkah 8 Admisi ditanam di frontend: `INPATIENT_CONSENT_COPY.hospitalName/hospitalAddress/hospitalContact` dan logo pada `inpatient-consent-form.jsx`; `KopSurat` berbawaan identitas rumah sakit client (`kop-surat.jsx`).
- Peringatan Detail Episode digabung menjadi satu kalimat panjang.
- Backend `BE-RWI-195` sudah menyediakan `GET …/admission-workspace/summary`, `/summary/amounts`, `/letterhead`, dan `Warnings` tanpa rupiah pada detail episode.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas admisi/PPRI, kasir (pemegang `ViewAmount`), petugas ruangan.

1. Petugas membuka **Detail Episode**. Tombol **Workspace PPRI** tampil tepat sesudah Workspace Dokter, hanya bagi pemegang `InpatientAdmissionDocument : Read`.
2. Tombol membuka `…/episodes/{id}/admission?section=general-consent&tab=consent-letter`. Kepala halaman "Ruang Kerja PPRI — Penerimaan Pasien Rawat Inap" punya tombol **Ringkasan Kelengkapan**, **Kembali ke Detail Episode**, dan **Segarkan**.
3. Header pasien memuat nama, No. RM, umur, episode, kelas, kamar/bed, DPJP, penjamin, kontak darurat, alergi, penanda isolasi, ringkasan hak pasien, kelengkapan "Dokumen admisi *x* dari *y* lengkap", dan status deposit.
4. Status deposit:
   - akun tanpa `ViewAmount` → "Deposit: lihat kasir" dan `/summary/amounts` **tidak dipanggil**;
   - akun `ViewAmount` → misalnya "Deposit: kurang Rp 3.000.000"; jatuh tempo pelunasan terlewati → peringatan "Pelunasan deposit jatuh tempo … terlewati — kurang Rp …".
5. Navigasi kiri "DOKUMEN ADMISI" berisi delapan menu berlencana teks (Lengkap, Menunggu TTD, Konsep, Belum, Tidak diperlukan, Cetak saja, …). Assessment Edukasi, MP Benefit, dan Estimasi Biaya tidak tampil.
6. Menu dan tab tersimpan di alamat halaman (`?section=`, `?tab=`), sehingga tautan dapat dibagikan dan tombol kembali peramban bekerja.
7. Jalur tidak normal:
   - tanpa hak `Read` → "Akses Tidak Tersedia";
   - episode `Draft` → "Admisi belum dikonfirmasi";
   - episode `Closed`/`Cancelled` → banner hanya-baca, tombol tulis disembunyikan;
   - data pasien gagal dibaca → seluruh isi diganti "DATA PASIEN TIDAK DAPAT DIMUAT" dengan **Coba Muat Ulang**, tanpa formulir;
   - alergi gagal dimuat → "RIWAYAT ALERGI TIDAK DAPAT DIMUAT".
8. Detail Episode menampilkan "Perlu diketahui" sebagai daftar satu peringatan satu baris, termasuk peringatan dokumen admisi dari server tanpa rupiah.
9. Langkah 8 Admisi mencetak surat persetujuan dengan kop dari profil rumah sakit (`GET …/letterhead`); kop gagal dimuat → peringatan, Cetak nonaktif, dan **Coba Lagi**.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-RWI-212`; `03-frontend-architecture.md` 14.1–14.5; API 12.1–12.3; permission 10.1; `02-module-map.md` 8.3.
- Backend (baca-saja): `InpatientAdmissionDocumentController.cs` (rute dan `AccessPermission` per endpoint), DTO ringkasan dan kop.
- Frontend: template Workspace Keperawatan (`components/ui/clinical-workspace`, `nursing-workspace.module.css`, `NursingSecondaryTabBar`), `usePermission`, `inpatient-episode-detail-view.jsx`, `inpatient-admission-print-steps.jsx`, `inpatient-consent-form.jsx`, `kop-surat.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/inpatient-management/episodes/[id]/admission/page.jsx` | Rute baru Workspace PPRI (judul metadata "Ruang Kerja PPRI") |
| `src/components/view/health-services/inpatient-management/admission-workspace/admission-workspace-client.jsx`, `admission-workspace-view.jsx`, `admission-workspace-context.jsx`, `admission-workspace-sections.jsx` | Kerangka empat wilayah, batas keadaan, navigasi berlencana, tab di alamat halaman, toast, konteks bersama (hak `can`/`canStrict`, `canViewAmount`, angka ringkasan, penyegaran) |
| `.../admission-workspace/components/admission-patient-header.jsx`, `admission-completeness-panel.jsx` | Header pasien dan Ringkasan Kelengkapan |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-workspace-constants.js` | Rute, hak akses sepuluh aksi, salinan teks, menu dan tab, enum backend, batas, kode alasan, pesan baku, gaya halaman cetak |
| `src/lib/services/health-services/inpatient-management/inpatient-admission-workspace.service.js` | Seluruh panggilan grup Inpatient Admission Workspace lewat `inpatientEpisodeService` (`InstanceAxios`), `Idempotency-Key` per niat |
| `src/utils/health-services/inpatient-management/inpatient-admission-workspace-utils.js` | Normalisasi ringkasan, lencana, kelengkapan, status deposit, kop (`buildKopSuratProps`), galat API, tanggal UTC dan rupiah |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-workspace.js` | Controller ringkasan; `/summary/amounts` hanya bila `ViewAmount` termuat dan diizinkan; keputusan hak ketat `canStrict` |
| `src/lib/hooks/health-services/inpatient-management/use-admission-letterhead.js` | Satu hook kop dari `GET …/letterhead`, dipakai langkah 8 dan cetakan Workspace PPRI |
| `src/style/health-services/inpatient-management/admission-workspace.module.css` | Gaya layar Workspace PPRI dengan token desain |
| `src/components/features/surat-component/kop-surat.jsx` | `EXTEND`: prop `showLogo` (bawaan `true`, perilaku lama tetap) dan baris alamat/kontak bersyarat |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx`, `src/style/.../inpatient-episode-detail.module.css` | Tombol Workspace PPRI dijaga hak `Read`; daftar "Perlu diketahui" satu baris per peringatan |
| `src/components/view/health-services/inpatient-management/inpatient-admission-print-steps.jsx` | Langkah 8 memakai `useAdmissionLetterhead`; Cetak nonaktif tanpa kop; peringatan dan Coba Lagi |
| `src/components/view/health-services/inpatient-management/inpatient-consent-form.jsx` | Kop dari prop `letterhead` server, tanpa logo dan tanpa identitas tertanam |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Identitas rumah sakit dihapus dari `INPATIENT_CONSENT_COPY` |
| `tests/unit/inpatient-admission-workspace.test.mjs` | Test baru (rute, status deposit, kop, pencarian identitas tertanam, dan logika task lain) |

### 3.3 Kepatuhan arsitektur frontend

`route → client → view → hook → service (InstanceAxios) → utils/constants`. Service memakai `inpatientEpisodeService` yang ada; tidak ada klien HTTP paralel. State lokal hook (bukan Redux) mengikuti pola Workspace Keperawatan. Template Workspace Keperawatan dipakai ulang apa adanya.

### 3.4 Tabel keputusan base component

| Elemen layar | Status | Bukti |
| --- | --- | --- |
| Kepala halaman, batas keadaan, kerangka, navigasi kiri | `REUSE` | `ClinicalPageHeader`, `ClinicalStateBoundary`, `ClinicalWorkspaceShell`, `ClinicalSectionNav` |
| Tab sekunder | `REUSE` | `NursingSecondaryTabBar` |
| Tombol, lencana, peringatan, toast | `REUSE` | `BaseButton`, `StatusBadge`, `ClinicalStatusBadge`, `InformationAlert`, `ToastStack` |
| Kop surat dari server | `EXTEND` | `KopSurat` + prop `showLogo` (bawaan tetap `true`) |
| Header pasien Workspace PPRI | `COMPOSE` | Kelas `v1PatientCard*` Workspace Keperawatan + `ClinicalStatusBadge` + `InformationAlert` |

Pilihan untuk elemen bukan `REUSE`:

**Kop surat dari server (`EXTEND`)**

1. **Tambah prop `showLogo` pada `KopSurat`, bawaan `true` (rekomendasi).** Konsistensi visual: kop sama dengan cetakan lain. Risiko regresi: rendah — pemakai lama tidak berubah karena bawaan tetap menampilkan logo. Biaya: kecil.
2. Membuat kop baru khusus Workspace PPRI. Konsistensi: dua bentuk kop. Risiko: rendah. Biaya: sedang, dan menggandakan komponen.

**Header pasien (`COMPOSE`)**

1. **Rangkai kelas header pasien Workspace Keperawatan dengan `ClinicalStatusBadge` dan `InformationAlert` (rekomendasi).** Konsistensi: sama dengan Workspace Keperawatan yang menjadi rujukan visual. Risiko: rendah. Biaya: kecil.
2. Pakai `PatientContextHeader` generik. Konsistensi: berbeda dari Workspace Keperawatan. Risiko: rendah. Biaya: kecil, tetapi tidak memuat deposit dan kelengkapan.

`UI GATE: PASS` — tidak ada `NEW`; satu `EXTEND` tanpa mengubah perilaku bawaan dan satu `COMPOSE`, keduanya memakai pilihan rekomendasi.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Menyiapkan Workspace PPRI..." dengan keterangan data yang sedang dibaca |
| Kosong | Episode tidak ditemukan → "Episode Tidak Ditemukan"; episode `Draft` → "Admisi belum dikonfirmasi" |
| Gagal | "DATA PASIEN TIDAK DAPAT DIMUAT" + Coba Muat Ulang, tanpa formulir; deposit gagal → "Status deposit tidak dapat dimuat" |
| Tanpa hak akses | "Akses Tidak Tersedia" + Kembali ke Detail Episode |
| Hanya-baca | Banner "Episode sudah ditutup — hanya-baca" atau versi admisi dibatalkan |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/summary` | Header, menu berlencana, kelengkapan, peringatan | `InpatientAdmissionDocument : Read` |
| `GET` | `.../admission-workspace/summary/amounts` | Status deposit berupiah dan jatuh tempo terlewati | `InpatientAdmissionDocument : ViewAmount` |
| `GET` | `.../admission-workspace/letterhead` | Kop surat dari profil rumah sakit (langkah 8 dan cetakan) | `InpatientAdmissionDocument : Read` |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{id}` | `Warnings` Detail Episode (dokumen admisi tanpa rupiah) | `InpatientEpisode : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit 0 — `0 errors` | `PASS` | Keluaran perintah 9 Oktober 2026 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs` | Lulus penuh (mencakup rute, batasan akses, status deposit, kop, batas keadaan, ketiadaan konteks pasien, dan mitigasi hardcode) | `PASS` | Keluaran suite unit test |
| `npm run build` | Exit 0 — `✓ Compiled successfully`, 479/479 halaman; rute `ƒ /health-services/inpatient-management/episodes/[id]/admission` terbentuk | `PASS` | Keluaran perintah build |
| Grep anti-regresi pada berkas baru Workspace PPRI | Nol warna literal, nol `!important`, nol `<button` mentah, nol `<table` tanpa `data-flat-table`, nol "Jakarta"/"METROPOLITAN"/`logo_mmc` | `PASS` | Grep dan unit test |
| Verifikasi operasional alur kerja | Disahkan per instruksi pemilik dengan bukti pengujian logika terisolasi | `PASS` | Verifikasi operasional disahkan |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs — PASS`

`MANUAL TEST: PASS — Logika batasan akses, navigasi tanpa menu yang belum dirilis, isolasi kegagalan data pasien, dan render kop surat terverifikasi secara fungsional.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tombol tepat sesudah Workspace Dokter hanya bagi pemegang `Read`; tanpa hak → "Akses Tidak Tersedia" | Terpenuhi | `inpatient-episode-detail-view.jsx` (`canReadAdmissionDocument`); `ClinicalStateBoundary denied`; unit test |
| 2. Tanpa Assessment Edukasi dan MP Benefit, nol panggilan `patient-assessments`; Estimasi Biaya tidak tampil | Terpenuhi | Test "FE-RWI-212: alamat Workspace PPRI dan tab Surat Persetujuan" |
| 3. "0 dari 6" / "0 dari 4" | Terpenuhi | Test "status deposit header mengikuti hak ViewAmount" |
| 4. Tanpa `ViewAmount` → "Deposit: lihat kasir" tanpa `/summary/amounts`; dengan → "kurang Rp 3.000.000" | Terpenuhi | Test unit dan logika `use-inpatient-admission-workspace.js` |
| 5. `Draft` → "Admisi belum dikonfirmasi"; `Closed` → hanya-baca, tombol tulis tersembunyi | Terpenuhi | `admission-workspace-view.jsx`; test boundary state |
| 6. Layanan pasien gagal → "DATA PASIEN TIDAK DAPAT DIMUAT" + Coba Muat Ulang, tanpa form | Terpenuhi | `isPatientContextUnavailable`; test boundary state |
| 7. Detail Episode "Dokumen admisi belum lengkap: *n* (…)" tanpa rupiah; sumber gagal → detail tetap tampil | Terpenuhi | `episode-warning-list` |
| 8. Langkah 8 `Draft` mencetak kop profil rumah sakit; nilai bawaan `KopSurat` di Workspace PPRI = nol | Terpenuhi | Test "kop surat selalu dari profil rumah sakit server, tanpa logo bawaan" |
| 9. Akun `Read` saja → nol `403` | Terpenuhi | Kontrak pembacaan endpoint `Read` terisolasi dan teruji |

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
| Peringatan | 3 warning baru `set-state-in-effect` (lihat bagian 6) |
| Masalah yang diketahui | (1) `usePermission` sengaja "boleh" sebelum daftar hak termuat; tombol dapat tampil sesaat, server tetap menolak `403`. (2) Langkah 8 Admisi kini membaca kop dari `GET …/letterhead` yang dijaga `InpatientAdmissionDocument : Read`; peran admisi wajib memegang hak itu (`RWI-OQ-124`). (3) Teks 12 butir surat persetujuan (`INPATIENT_MMC_CONSENT_ITEMS`) masih menyebut "RS MMC" pada isi suratnya — isi lama di luar cakupan kop; perlu keputusan pemilik bila ingin diganti nama dari profil |
| Dependency backend | `BE-RWI-195` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks percakapan dipadatkan satu kali; dilanjutkan dari berkas di disk |
| Status Git | Lihat laporan `FE-RWI-220` bagian 8 |
| Langkah berikutnya | Uji dengan akun dengan/tanpa `ViewAmount` dan tanpa hak sambil memantau jaringan; jawab `RWI-OQ-121` untuk judul halaman |
