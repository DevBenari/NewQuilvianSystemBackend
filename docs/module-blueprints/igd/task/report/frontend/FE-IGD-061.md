# Laporan Perubahan Frontend — `FE-IGD-061`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-061` |
| Judul | Sesudah terkirim: status, penahanan *Jalankan*, pembatalan, dan cetak |
| Slice | `EPIC IGD-15` · `MVP-11` · R3.16 · `SCR-IGD-P01` (tab *Tindak Lanjut*), `P02` (form), `P03` (cetak) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.16 |
| Requirement | `FR-IGD-114`, `115`, `116`; `AT-IGD-228`, `231`…`234` |
| Keputusan | `IGD-DEC-277`…`281`, `294` (wewenang UI U3), pilihan pemilik 14 Oktober 2026 untuk cetak (bagian 3.4) |
| Contract version | API **`0.17.1`** §12.2 (`cancel`, `print-data`), §12.5; validation **`0.16.0`** §14 aturan 19, 23, 24; state **`0.11.0`** §11.2; `03-frontend-architecture.md` §17.4–17.6 |
| Pasangan backend | `BE-IGD-074` (build Rizki 14 Oktober 2026, 0 error) |
| Dependency | `FE-IGD-059` (sudah di-commit pemilik) |
| Task mode | `FRONTEND` — izin pemilik 14 Oktober 2026: *"yaa lanjutkan"* |
| Repository / branch | `QuilvianSystemFrontendDev` / `RizkiV2`, HEAD `2ab4d6e46` + working tree |
| Model | Claude Opus 5.5 |
| Tanggal | 14 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — kode lengkap.** Uji Putaran B2 (15 Oktober 2026): tombol Jalankan nonaktif (C-02), pesan panduan admisi, pratinjau cetak (C-10) ✅ LULUS. End-to-end terblokir di langkah admisi C-12 karena hak akses RBAC petugas admisi (`Doctor:Read`, `InpatientBedOccupancy:Read/Create/Update`); skenario P3–P5 NOT RUN (panduan §6.2). |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti |
| --- | --- |
| Panel form (`FE-IGD-059`) sudah menampilkan lencana keenam `displayStatus`, tetapi keterangan *Selesai* kosong dan tombol *Buat Form* tetap muncul ketika form sudah terkirim | `emergency-assessment-admission-request-panel.jsx` (`hasOpenForm` hanya *Draf*/*Menunggu TTD*) |
| Form terbuka hanya mengizinkan batal pada *Draf*/*Menunggu TTD* | `emergency-assessment-admission-request-form.jsx` `canCancel` |
| Tombol *Jalankan* di tab *Tindak Lanjut* tidak memeriksa form; penolakan `409` dari server sudah tampil di modal | `emergency-assessment-disposition-tab.jsx` `runAction` |
| Pola cetak surat dokter rawat jalan (U3) memakai `doctor-certificate.module.css`, yang memuat aturan cetak global `@media print { body * { visibility: hidden } }` — terbukti di CSS hasil kompilasi `.next/dev/static/chunks/…doctor-certificate_module_css….css` baris 789–792 | Bila dimuat di halaman IGD, Ctrl+P halaman pengkajian IGD tercetak kosong |
| Kit cetak dokumen PPRI (`inpatient-consent-print.module.css`) mengisolasi cetak hanya selama `.printArea` ada (`body:has(.printArea) *`) | Baris 457–480 |
| Kop surat dari profil rumah sakit Rawat Inap butuh `episodeId` | `use-admission-letterhead.js` |

## 2. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Yang terlihat |
| ---: | --- | --- |
| 1 | Dimas (perawat) membuka tab *Tindak Lanjut* sebelum dokter menandatangani | *Jalankan* pada tindak lanjut rawat inap nonaktif, di bawahnya *"Form Permintaan Rawat Inap belum ditandatangani DPJP."* |
| 2 | dr. Andi menandatangani (`FE-IGD-060`) | Lencana form *Terkirim*, keterangan *"Menunggu petugas admisi."*; *Jalankan* aktif; *Buat Form* tidak muncul |
| 3 | Dimas membuka form (*Lihat Form*) | Isian baca-saja + baris *Permintaan admisi: Terkirim — menunggu petugas admisi*; tombol **Batalkan Form** dan **Cetak** |
| 4 | **Cetak** | Pratinjau di bawah form: kop rumah sakit, judul, identitas, informasi klinis, rencana penempatan, rencana perawatan, blok tanda tangan *"Ditandatangani secara elektronik oleh …, … WIB"*; **Cetak Sekarang** membuka dialog cetak peramban — hanya kertas yang tercetak |
| 5a | Petugas admisi membuka admisi | Lencana *Selesai*, *"Admisi rawat inap sudah dibuka dari permintaan ini."*; di form *"Selesai — admisi dibuka {tanggal jam}"*; batal tidak lagi ditawarkan, cetak tetap ada |
| 5b | Petugas admisi membatalkan | Lencana *Dibatalkan admisi* + alasan; *Jalankan* nonaktif dengan *"Permintaan admisi dibatalkan petugas admisi. Ajukan Form Permintaan Rawat Inap baru."*; *Buat Form* muncul lagi |
| 5c | Dimas membatalkan form terkirim | Konfirmasi menjelaskan permintaan admisi ikut batal; sesudahnya lencana *Dibatalkan* + alasan |
| 5d | Dokter membatalkan tindak lanjut | Form tampil *Dibatalkan* dengan alasan pembatalan tindak lanjut |

**Jalur tidak normal.**

| Keadaan | Di layar |
| --- | --- |
| Daftar form sedang dimuat | *Jalankan* nonaktif sementara, *"Memeriksa Form Permintaan Rawat Inap..."* |
| Daftar form gagal dimuat | *Jalankan* tetap dapat ditekan; server yang memutuskan dan penolakan `409` tampil di modal (gagal terbuka di layar, gagal tertutup di server) |
| Batal sesudah admisi dibuka (balapan) | Pesan server aturan 19 tampil di modal batal |
| Isi cetak gagal dimuat | Pesan galat + **Coba Lagi**; **Cetak Sekarang** nonaktif |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`emergency-assessment-disposition-tab.jsx`, `emergency-assessment-admission-request-panel.jsx`,
`emergency-assessment-admission-request-form.jsx`, `use-emergency-admission-request.jsx`, `emergency-assessment-constant.jsx`
(`NURSE_DISPOSITION_STATUS_ACTIONS`, `DISPOSITION_STATUS`), `doctor-certificate-tab.jsx` dan `doctor-certificate.module.css`
(pola U3), `inpatient-consent-form.jsx`, `inpatient-admission-print-steps.jsx`, `inpatient-consent-print.module.css`,
`base-letterhead.jsx`, `use-admission-letterhead.js`, CSS hasil kompilasi `.next/dev`.

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `src/lib/services/health-services/emergency-management/emergency-admission-request.service.js` | `M` (bersama `FE-IGD-060`) | `getEmergencyAdmissionRequestPrintData` |
| `src/lib/constants/health-services/emergency-installation-management/emergency-admission-request-constant.jsx` | `M` (bersama `FE-IGD-060`) | `ADMISSION_REFERRAL_STATUS`, `ADMISSION_REFERRAL_CANCELLATION_SOURCE`; teks penahanan *Jalankan*, batal sesudah kirim, status permintaan admisi; `ADMISSION_PRINT_TEXT` |
| `src/utils/health-services/emergency-installation-management/emergency-admission-request.utils.js` | `M` (bersama `FE-IGD-060`) | Rincian membaca `admissionReferral`; `isAdmissionRequestSentOrCompleted`, `isAdmissionRequestCancellableAfterSend`, `resolveAdmissionExecuteBlock`, `describeAdmissionReferral`, `normalizeAdmissionPrintData`, `describeAdmissionAllergies`; pemetaan alergi dipakai bersama |
| `…/emergency-assessment-view/components/emergency-assessment-disposition-tab.jsx` | `M` +24/−1 | *Jalankan* tindak lanjut `requiresAdmissionRequest` nonaktif + keterangan sampai form *Terkirim*/*Selesai* |
| `…/emergency-assessment-view/components/emergency-assessment-admission-request-panel.jsx` | `M` +9/−2 | Keterangan *Selesai*; *Buat Form* hanya bila tidak ada form aktif, terkirim, atau selesai |
| `…/emergency-assessment-view/components/emergency-assessment-admission-request-form.jsx` | `M` (+42/−8 total, termasuk ekspor `FE-IGD-060`) | Batal sesudah kirim, pesan konfirmasinya, baris *Permintaan admisi*, tombol **Cetak**, pratinjau cetak |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-admission-print.jsx` | `??` baru, 67 baris | Muat `print-data`, muat ulang, `window.print()` |
| `…/emergency-assessment-view/components/emergency-assessment-admission-request-print.jsx` | `??` baru, 145 baris | Pratinjau dan kertas cetak dengan kit cetak PPRI + `BaseLetterhead` |
| `…/doctor-emergency/emergency-admission-signature-dialog.jsx` (`FE-IGD-060`) | `??` | Memakai `describeAdmissionAllergies` bersama (duplikasi lokal dihapus) |

Nol CSS baru atau berubah. Nol komentar baru. Seluruh berkas CRLF tanpa BOM.

### 3.3 Gerbang keputusan base component

`UI GATE: 7 elemen — REUSE 5, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Elemen | Status | Base component / gaya | Bukti |
| --- | --- | --- | --- |
| Lencana dan keterangan `displayStatus` | `REUSE` | `StatusBadge` + `mutedText` panel `FE-IGD-059` | Sudah ada |
| *Jalankan* nonaktif + keterangan | `REUSE` | Tombol aksi tab *Tindak Lanjut* (pola lama, `disabled`) + `mutedText` | Sudah ada |
| Penolakan `409` di modal | `REUSE` | `ConfirmModal` + `InformationAlert` tab *Tindak Lanjut* | Sudah ada |
| Batal sesudah kirim | `REUSE` | `ConfirmModal requireReason` form `FE-IGD-059` | Sudah ada |
| Baris status permintaan admisi | `REUSE` | `ReadOnlyItem` | Sudah ada |
| Pratinjau cetak | `COMPOSE` | `formSection` + `BaseButton` + `InformationAlert` | Bagian 3.4 |
| Kertas cetak | `COMPOSE` | `BaseLetterhead` + kelas `inpatient-consent-print.module.css` (`printStage`, `printArea`, `consentPaper`, `docTitle`, `sectionBar`, `patientInfoGrid`, `signatureBox`) | Bagian 3.4 |

### 3.4 Keputusan cetak (pemilik, 14 Oktober 2026)

Pilihan yang diajukan:

1. **Tata letak surat dengan kit cetak PPRI (Direkomendasikan, dipilih pemilik).** Rupa surat tetap: kop, judul, isian *label : nilai*, blok tanda tangan elektronik. Isolasi cetak hanya aktif selama pratinjau ada. Tanpa CSS baru dan tanpa efek samping. Menyimpang dari sumber CSS U3; dicatat sebagai delta 1.
2. CSS surat dokter rawat jalan persis (U3). Aturan cetak globalnya membuat Ctrl+P di halaman IGD tercetak kosong.
3. CSS cetak IGD baru (±80 baris bertoken). Rapi, tetapi ada CSS baru yang perlu ditinjau.

### 3.5 Delta terhadap kartu dan kontrak

| No | Kartu/kontrak | Implementasi | Alasan |
| ---: | --- | --- | --- |
| 1 | U3: pola cetak surat dokter rawat jalan | Rupa surat, kelas cetak dokumen PPRI | Pilihan pemilik (3.4) |
| 2 | Layar cetak `SCR-IGD-P03` | Pratinjau di dalam form (`P02`), bukan modal | Pola PPRI mencetak area di halaman; modal `fixed` berisiko memotong cetakan menjadi satu halaman |
| 3 | Kop surat | `BaseLetterhead` bawaan (`DEFAULT_HOSPITAL_LETTERHEAD`), sama dengan surat dokter rawat jalan | Endpoint kop Rawat Inap butuh episode rawat inap yang belum ada |
| 4 | §17.5: *Selesai* — *"Admisi dibuka {jam}"* | Panel: *"Admisi rawat inap sudah dibuka dari permintaan ini."* tanpa jam; jam tampil di form (*"Selesai — admisi dibuka {tanggal jam}"*) | Ringkasan daftar backend tidak membawa waktu admisi; rincian membawanya |
| 5 | *Jalankan* bila daftar form gagal dimuat | Tidak ditahan di layar; server yang menahan (aturan 23) dan pesannya tampil | Layar tidak menahan jalur berdasarkan data yang tidak terbaca |

## 4. State yang ditangani di layar

| State | Penanganan |
| --- | --- |
| Loading | *Jalankan* nonaktif + *"Memeriksa…"*; *"Memuat isi cetak..."* |
| Empty | Tidak ada form → keterangan panel `FE-IGD-059` |
| Error | Pesan + **Coba Lagi** (cetak); *Jalankan* diserahkan ke server |
| Stale | `409` batal/jalankan tampil di modal; daftar dimuat ulang sesudah aksi |
| Disabled | *Jalankan*, **Cetak Sekarang** |
| Privacy | Isi klinis tidak ditulis ke `console`/penyimpanan peramban; cetak hanya kertas |

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Admission Request

Base URL: `/api/v1/health-services/emergency-installation-management/emergency-admission-requests`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/?emergencyVisitId=` | Lencana, keterangan, penahanan *Jalankan* | `EmergencyAdmissionRequest : Read` |
| `GET` | `/{id}` | Baris *Permintaan admisi* (`admissionReferral`) | `EmergencyAdmissionRequest : Read` |
| `POST` | `/{id}/cancel` | Batal sesudah kirim | `EmergencyAdmissionRequest : Cancel` |
| `GET` | `/{id}/print-data` | Kertas cetak dari salinan beku | `EmergencyAdmissionRequest : Read` |

#### Health Services / Emergency Installation Management / Emergency Disposition

`PATCH …/emergency-dispositions/{id}/disposition-status` (`EmergencyDisposition : Update`, sudah ada) — penolakan aturan 23 tampil
di modal *Jalankan*.

## 6. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` atas berkas `FE-IGD-060`/`061` (16 berkas termasuk dua berkas resep sesi lain yang ikut tersaring pola) | 0 error, 0 warning | `PASS` |
| Komentar baru | Nol pada baris tambahan 8 berkas `M` dan 4 berkas baru | `PASS` |
| Akhiran baris | 9 berkas task CRLF, tanpa BOM, nol LF tunggal | `PASS` |
| Grep anti-regresi UI (warna literal, `style={}`, kelas `btn`, `<table`, `fw-*`/`fs-*`, `!important`) | Nol pada baris tambahan dan berkas cetak baru | `PASS` |
| CSS | Nol berkas CSS diubah | `PASS` |
| `npm run build` | Lulus 14 Oktober 2026 13.01.48–13.03.33: *Compiled successfully in 59s*, 492/492 halaman statis, 0 baris peringatan, `postbuild` standalone siap; `BUILD_ID` `CdZrb6rn2PX6y3xbZLSNX`; `data-testid` baru task ini ditemukan di `.next/static/chunks`. Server standalone pemilik (PID 27640) sudah berhenti saat izin diberikan; tidak dinyalakan ulang | `PASS` |

`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tim: unit test tidak ditulis atau dijalankan untuk task ini.`

`MANUAL TEST: NOT FEASIBLE — agent tidak memegang kredensial akun uji; diuji pada putaran B.`

**Skenario uji layar untuk putaran B.**

| ID | Langkah | Hasil yang diharapkan | AT |
| --- | --- | --- | --- |
| 061-U1 | Tab *Tindak Lanjut* sebelum TTD | *Jalankan* nonaktif + keterangan aturan 23 | `AT-IGD-228` |
| 061-U2 | Sesudah TTD dokter | Lencana *Terkirim*; *Jalankan* aktif; *Buat Form* tidak muncul | `AT-IGD-228` |
| 061-U3 | Petugas admisi membatalkan permintaan | Lencana *Dibatalkan admisi* + alasan; *Jalankan* nonaktif dengan kalimat kedua aturan 23; *Buat Form* muncul | `AT-IGD-231` |
| 061-U4 | Perawat membatalkan form terkirim | Lencana *Dibatalkan* + alasan; sesudah admisi dibuka, pembatalan ditolak dan pesan tampil di modal | `AT-IGD-232`, `233` |
| 061-U5 | Dokter membatalkan tindak lanjut rawat inap | Form tampil *Dibatalkan* dengan alasan tindak lanjut | `AT-IGD-233` |
| 061-U6 | **Cetak** → **Cetak Sekarang** (simpan PDF); ubah alergi pasien; cetak lagi | Kertas memuat baris tanda tangan elektronik berjam WIB; hanya kertas yang tercetak; isi cetakan kedua sama dengan pertama | `AT-IGD-234` |
| 061-U7 | Ctrl+P di halaman pengkajian IGD tanpa pratinjau terbuka | Halaman tercetak seperti biasa (isolasi tidak aktif) | — |

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | *Jalankan* nonaktif dengan keterangan sebelum form terkirim; aktif sesudahnya; nonaktif lagi sesudah dibatalkan admisi | Kode terpenuhi; uji layar belum | `resolveAdmissionExecuteBlock` |
| 2 | Lencana dan keterangan mengikuti state §11.2 untuk keenam keadaan | Kode terpenuhi (delta 4); uji layar belum | Panel + konstanta `FE-IGD-059` |
| 3 | Batal sesudah kirim beralasan; sesudah admisi dibuka, penolakan tampil apa adanya | Kode terpenuhi; uji layar belum | `canCancel`/`isSent`; pesan server di modal |
| 4 | Pembatalan tindak lanjut oleh dokter tampil sebagai form dibatalkan dengan alasannya | Kode terpenuhi; uji layar belum | `cancellationReason` ringkasan (backend mengisinya dari permintaan admisi) |
| 5 | Cetak memuat baris tanda tangan elektronik dan tidak berubah sesudah data pasien berubah | Kode terpenuhi; uji PNG/PDF belum | `print-data` dari salinan beku; `signatureStatement` |
| 6 | Diff hanya berkas Cakupan; nol komentar baru; `eslint` 0 error; `npm run build` lulus | Terpenuhi — build 14 Oktober 2026 lulus | Bagian 6 |

DoD: laporan ini ✅; acceptance 1–5 terpetakan ke kode; 6 terpenuhi (build 14 Oktober 2026); uji layar pada putaran B; rilis bersama `BE-IGD-074`.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Tinggi menurut kartu — task ini yang mencegah rilis `BE-IGD-074` menghentikan pemindahan pasien; layar *Jalankan* sekarang menjelaskan sebabnya |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas task: 6 `M` + 2 `??` (`FE-IGD-061`) di atas berkas `FE-IGD-060`; berkas `M`/`??` lain milik sesi lain dan tidak disentuh. Nol stage, nol commit |
| Langkah berikutnya | Rizki: build ulang backend, nyalakan server standalone hasil build 16.42, lalu jalankan [panduan putaran B baru](../../../testing/2026-10-14-panduan-uji-putaran-b2-antrean-transfer-igd.md) |

## Pengerjaan ulang — 14 Oktober 2026 (`IGD-DEC-304`)

| Berkas | Perubahan |
| --- | --- |
| `emergency-admission-request-constant.jsx` | Enum status dan sumber pembatalan permintaan admisi dibuang; teks *Terkirim*, *Selesai*, pembatalan sesudah kirim, dan dua pesan *Jalankan* menyebut Antrean Transfer IGD/Poli |
| `emergency-admission-request.utils.js` | Normalisasi `admissionReferral` dan `describeAdmissionReferral` dibuang; `resolveAdmissionExecuteBlock` selalu mengembalikan pesan penahan |
| `emergency-assessment-admission-request-form.jsx` | Baris *Permintaan admisi* dibuang; alasan pembatalan tampil langsung dari form |
| `emergency-assessment-admission-request-panel.jsx` | Keterangan *Terkirim* memakai teks baru |
| `emergency-assessment-disposition-tab.jsx` | *Jalankan* rawat inap tetap tertahan saat daftar form gagal dimuat |

**Validasi:** ESLint 7 berkas (5 di atas + 2 berkas Rawat Inap yang dipulihkan) — 0 error, 1 warning lama `fetchMothers` di `HEAD`; nol komentar baru; CRLF. Server uji standalone PID 10068 (mulai 14.04) dihentikan atas izin pemilik; build ulang 14 Oktober 2026 16.42 lulus (`BUILD_ID` `ZLYxPkgFfju8K4gb4uRL0`, 492/492, 0 warning); teks baru ditemukan di `.next/static/chunks`, teks lama `DARI IGD` tidak ada; server tidak dinyalakan ulang agent.

## Build ulang sesudah merge integration — 15 Oktober 2026

Frontend di-merge dengan `QuilvianIntegrationFrontend` `35e030284` (71 commit, termasuk layar admisi Rawat Inap terbaru milik Hamzah). Satu konflik di `patient-selection-step.jsx` (pendaftaran IGD) diselesaikan dengan mempertahankan kedua sisi: mode pencarian IGD *No. RM / NIK / No. HP* dan *Nama + Tanggal Lahir*, serta pencarian bertanggal lahir dan daftar kandidat untuk rawat jalan (`RJ-REG-REV-FE-001`). ESLint berkas itu 0 error, 5 warning — kelimanya sudah ada pada kedua versi sebelum merge. `npm run build` 15 Oktober 2026 10.01–10.03 lulus: *Compiled successfully in 73s*, 497/497, `BUILD_ID` `sI7Ygp3HmbcNzoJNuCQ0r`; teks task ini ditemukan di `.next/static/chunks`, sisa `FE-IGD-062` tidak ada.
