# Laporan Perubahan Frontend — `FE-IGD-060`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-060` |
| Judul | *Perlu TTD* di Ruang Kerja Dokter IGD: tanda tangan dan pengembalian |
| Slice | `EPIC IGD-15` · `MVP-11` · R3.16 · `SCR-IGD-D01` (ringkasan) + `SCR-IGD-D04` (dialog) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.16 |
| Requirement | `FR-IGD-112`; `AT-IGD-225`…`227` |
| Keputusan | `IGD-DEC-272`…`274`, `285`, `291`, `294` (wewenang UI U2) |
| Contract version | API **`0.17.1`** §12.2 (`pending-signature`, `sign` + `Idempotency-Key`, `return`, `GET /{id}`); validation **`0.16.0`** §14 aturan 13–18; `03-frontend-architecture.md` §17.4 C, §17.6 |
| Pasangan backend | `BE-IGD-072` (commit `f6b07432`), `BE-IGD-074` (build Rizki 14 Oktober 2026, 0 error) |
| Wewenang UI | U2 disetujui: butir *Perlu TTD* pada `ClinicalSummaryBar` Ruang Kerja Dokter IGD + tanda pada kartu pasien |
| Task mode | `FRONTEND` — izin pemilik 14 Oktober 2026: *"sekarang lanjutkan"* (urutan `BE-IGD-075` → `FE-IGD-060`/`061`/`062`, BE dan FE berpasangan) |
| Repository / branch | `QuilvianSystemFrontendDev` / `RizkiV2`, HEAD `2ab4d6e46` + working tree |
| Model | Claude Opus 5.5 |
| Tanggal | 14 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 14 Oktober 2026, disesuaikan `IGD-DEC-304`.** Kode lengkap; teks dialog tanda tangan kini menyebut Antrean Transfer IGD/Poli; ESLint 0 error. build ulang 14 Oktober 2026 16.42 lulus (`BUILD_ID` `ZLYxPkgFfju8K4gb4uRL0`, 492/492, 0 warning). **Belum:** uji layar pada putaran B baru |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti |
| --- | --- |
| Ruang Kerja Dokter IGD sudah punya ringkasan `ClinicalSummaryBar` (*Pasien IGD berjalan*, *Pasien saya*) dan daftar pasien berkartu | `doctor-emergency-view.jsx`, `emergency-physician-patient-card.jsx` |
| `ClinicalSummaryBar` sudah mendukung butir dengan `onClick` dan `active` | `ClinicalSummaryBar.jsx` `:12`, `:43` |
| Service form IGD (`FE-IGD-059`) belum punya `pending-signature`, `sign`, `return` | `emergency-admission-request.service.js` |
| Normalisasi rincian form belum membaca `canSign`; ringkasan belum membaca `emergencyVisitId` | `emergency-admission-request.utils.js` |
| Hak akses dibaca lewat `usePermission(resource, action)` — `allowed` bernilai benar selama daftar izin belum termuat | `src/lib/hooks/auth/use-permission.jsx` |
| Pembangkit `Idempotency-Key` sudah ada | `inpatient-admission-workspace-utils.js` `createIdempotencyKey` |
| Working tree berisi pekerjaan sesi lain (resep dokter IGD, SOAP perawat, CSS pengkajian IGD) | `git status`; tidak disentuh |

## 2. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Yang terlihat di layar |
| ---: | --- | --- |
| 1 | dr. Andi (DPJP IGD aktif) membuka Ruang Kerja Dokter IGD | Ringkasan bertambah butir **Perlu TTD 1**; kartu Tn. Budi bertanda *Perlu TTD* |
| 2 | Klik **Perlu TTD** | Daftar pasien tersaring hanya yang formnya menunggu tanda tangan dr. Andi; klik lagi untuk melepas saringan |
| 3 | Pilih Tn. Budi | Di atas tab muncul peringatan *"Form Permintaan Rawat Inap menunggu tanda tangan Anda"* dengan tombol **Buka Form · Bangsal Melati · CITO** |
| 4 | Buka form | Dialog *Tanda Tangan Form Permintaan Rawat Inap*: status, identitas, alergi, penjamin, diagnosis, indikasi, rencana penempatan, rencana perawatan, dokter pengirim — seluruhnya baca-saja |
| 5a | **Tandatangani** | Tombol nonaktif selama permintaan; sukses → dialog tertutup, *"Form ditandatangani dan dikirim ke petugas admisi."*, butir *Perlu TTD* berkurang, tanda pada kartu hilang |
| 5b | **Kembalikan ke Perawat…** → isi catatan → **Kembalikan** | Catatan wajib (maks. 500); sukses → *"Form dikembalikan ke perawat."*; form kembali *Draf* di layar perawat dengan catatan itu (`FE-IGD-059`) |

**Jalur tidak normal.**

| Keadaan | Di layar |
| --- | --- |
| Akun tanpa izin `EmergencyAdmissionRequest : Sign` | Butir *Perlu TTD*, tanda kartu, dan peringatan tidak tampil |
| Daftar *Perlu TTD* gagal dimuat | Butir menampilkan `—` dan catatan *"Daftar Perlu TTD tidak dapat dimuat."* di bawah saringan |
| Nol form menunggu | Butir tetap tampil dengan angka 0; bila saringan aktif, keadaan kosong *"Tidak ada form yang menunggu tanda tangan Anda."* + **Tampilkan Semua Pasien** |
| Rincian form gagal dimuat | Pesan galat di dialog + **Coba Lagi** |
| `canSign = false` (DPJP baru dialihkan atau form tidak lagi menunggu) | Peringatan di dialog; **Tandatangani** dan **Kembalikan** nonaktif |
| `403` / `409` / `503` dari server | Pesan server tampil di dialog apa adanya; rincian dan daftar dimuat ulang |
| Klik ganda *Tandatangani* | Satu `Idempotency-Key` per pembukaan dialog; kiriman ulang dijawab backend dengan hasil yang sama (`BE-IGD-074` delta 1) |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`doctor-emergency-view.jsx`, `emergency-physician-patient-card.jsx`, `use-emergency-physician-patients.jsx`,
`emergency-physician-constant.jsx`, `ClinicalSummaryBar.jsx`, `ClinicalPatientCard.jsx`, `confirm-modal.jsx`,
`information-alert.jsx`, `status-badge.jsx`, `use-permission.jsx`, `emergency-admission-request.service.js`,
`emergency-admission-request-constant.jsx`, `emergency-admission-request.utils.js`, `use-emergency-admission-request.jsx`,
`emergency-assessment-admission-request-form.jsx`, `inpatient-admission-workspace-utils.js` (`createIdempotencyKey`),
`emergency-physician-utils.js` (`readEmergencyPhysicianErrorMessage`); `AGENTS.md` frontend; aturan `rules/frontend/`.

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `src/lib/services/health-services/emergency-management/emergency-admission-request.service.js` | `M` +36 | `getEmergencyAdmissionRequestsPendingSignature`, `signEmergencyAdmissionRequest` (header `Idempotency-Key`), `returnEmergencyAdmissionRequest` |
| `src/lib/constants/health-services/emergency-installation-management/emergency-admission-request-constant.jsx` | `M` +42 | `ADMISSION_SIGNATURE_MODE`, `ADMISSION_SIGNATURE_TEXT` |
| `src/utils/health-services/emergency-installation-management/emergency-admission-request.utils.js` | `M` +12 | Ringkasan membaca `emergencyVisitId`, nomor kunjungan, nama, No. RM; rincian membaca `canSign`; `groupAdmissionRequestsByVisit` |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-physician/use-emergency-admission-signature.jsx` | `??` baru, 268 baris | Daftar *Perlu TTD* (hanya bila izin `Sign` termuat dan diberikan), dialog, tandatangani, kembalikan, muat ulang sesudah aksi |
| `src/components/view/health-services/emergency-installation-management/doctor-emergency/emergency-admission-signature-dialog.jsx` | `??` baru, 189 baris | Dialog ringkasan baca-saja + mode kembalikan berbasis `ConfirmModal`; ringkasan alergi memakai `describeAdmissionAllergies` bersama (dirapikan saat `FE-IGD-061`) |
| `…/doctor-emergency/emergency-physician-patient-card.jsx` | `M` +12 | Prop `needsSignature` → `StatusBadge` *Perlu TTD* di baris meta |
| `…/doctor-emergency/doctor-emergency-view.jsx` | `M` +125/−15 | Butir ringkasan dan saringan *Perlu TTD*, keadaan kosong saringan, catatan galat daftar, peringatan + tombol buka form, pemberitahuan sukses, dialog |
| `…/emergency-assessment-view/components/emergency-assessment-admission-request-form.jsx` | `M` +2/−2 | `ReadOnlyGroup` dan `ReadOnlyItem` diekspor agar dialog dokter memakai tampilan baca-saja yang sama dengan layar perawat |

Nol CSS baru atau berubah. Nol komentar baru. Seluruh berkas CRLF tanpa BOM.

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Pemenuhan |
| --- | --- |
| HTTP lewat service + `InstanceAxios` | Ya — tiga fungsi baru di service yang sudah ada |
| State | Hook lokal non-Redux, sama dengan pola hook layar dokter IGD dan `FE-IGD-059` |
| Teks layar | Konstanta `ADMISSION_SIGNATURE_TEXT` |
| Hak akses | `usePermission("EmergencyAdmissionRequest", "Sign")`; butir tampil hanya bila daftar izin **termuat** dan memberi izin. Penentu boleh menandatangani tetap `canSign` dari server (`03-frontend-architecture.md` §17.6 nomor 1) |
| Data sensitif | Indikasi, rencana, dan catatan tidak ditulis ke `console` maupun penyimpanan peramban |

#### Gerbang keputusan base component

`UI GATE: 6 elemen — REUSE 4, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Elemen | Status | Base component | Bukti |
| --- | --- | --- | --- |
| Butir *Perlu TTD* + saringan | `REUSE` | `ClinicalSummaryBar` (`items[].onClick`, `active`) | Prop sudah ada, belum pernah dipakai `onClick` di layar ini |
| Tanda kartu pasien | `REUSE` | `StatusBadge` di slot `meta` `ClinicalPatientCard` | Pola lencana `FE-IGD-059` |
| Peringatan + tombol buka form | `REUSE` | `InformationAlert` (`title` + `children`) + `BaseButton` | `information-alert.jsx` `:29` |
| Keadaan kosong saringan | `REUSE` | `ClinicalEmptyState` + `BaseButton` | Pola keadaan kosong di layar yang sama |
| Dialog tanda tangan | `COMPOSE` | `ConfirmModal` (`size="lg"`, `children`, `confirmProps`) + `ReadOnlyGroup`/`ReadOnlyItem` `FE-IGD-059` | Tidak ada modal ringkasan form di base-features |
| Mode kembalikan | `COMPOSE` | `ConfirmModal` (`requireReason`, `maxReasonLength`) dalam dialog yang sama | Pola batal form `FE-IGD-059` |

**Pilihan untuk elemen `COMPOSE` — dialog tanda tangan:**

1. **`ConfirmModal` berukuran besar dengan ringkasan baca-saja, mode kembalikan di dalam dialog yang sama (Direkomendasikan, dipakai).** Tampilan dan perilaku tombol sama dengan konfirmasi di layar perawat, tanpa komponen baru; satu dialog berarti tidak ada modal bertumpuk. Konsekuensi: tombol *Kembalikan ke Perawat…* berada di badan dialog, bukan di kaki.
2. Halaman atau panel *Perlu TTD* tersendiri di kolom kanan. Lebih lega untuk form panjang, tetapi menambah layar dan route yang tidak diminta U2, serta memisahkan dokter dari konteks pasien.
3. Memakai ulang komponen form `FE-IGD-059` dalam mode baca-saja. Paling sedikit kode tampilan, tetapi komponen itu memuat pilihan unit/kelas/DPJP dan aksi perawat sehingga harus diberi banyak cabang — risiko regresi pada layar perawat.

**Pilihan untuk elemen `COMPOSE` — mode kembalikan:**

1. **Satu `ConfirmModal` berganti mode (Direkomendasikan, dipakai).** *Kembali* membawa dokter ke ringkasan tanpa menutup dialog.
2. `ConfirmModal` kedua di atas dialog. Modal bertumpuk di React Bootstrap sulit diatur fokus dan latarnya.

## 4. State yang ditangani di layar

| State | Penanganan |
| --- | --- |
| Loading daftar | Butir menunggu angka; daftar pasien tidak tersaring sampai data datang |
| Loading rincian | *"Memuat Form Permintaan Rawat Inap..."* di dialog |
| Empty | Angka 0; keadaan kosong saringan beserta tombol lepas saringan |
| Error daftar / rincian | Catatan galat / pesan galat + **Coba Lagi** |
| Unauthorized | Butir tidak tampil; `403` rincian → pesan hak akses |
| Stale | `409` → pesan server, rincian dan daftar dimuat ulang |
| Disabled | Tombol nonaktif saat `canSign = false` atau permintaan berjalan |
| Duplicate submit | Satu `Idempotency-Key` per dialog |

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Admission Request

Base URL: `/api/v1/health-services/emergency-installation-management/emergency-admission-requests`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/pending-signature` | Angka butir, saringan, tanda kartu, tombol buka form | `EmergencyAdmissionRequest : Sign` |
| `GET` | `/{id}` | Ringkasan dialog, `canSign` | `EmergencyAdmissionRequest : Read` |
| `POST` | `/{id}/sign` | *Tandatangani*; body `{ rowVersion }`, header `Idempotency-Key` | `EmergencyAdmissionRequest : Sign` |
| `POST` | `/{id}/return` | *Kembalikan*; body `{ note, rowVersion }` | `EmergencyAdmissionRequest : Sign` |

## 6. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` atas 8 berkas | 0 error, 0 warning (satu warning `react-hooks/set-state-in-effect` di hook baru diperbaiki sebelum laporan) | `PASS` |
| Komentar baru | Nol pada baris tambahan dan dua berkas baru | `PASS` |
| Akhiran baris | 8 berkas CRLF, tanpa BOM, nol LF tunggal | `PASS` |
| Grep anti-regresi UI (warna literal, `style={}`, `<button`/kelas `btn`, `<table`, `fw-*`/`fs-*`, `!important`) | Nol temuan pada tiga berkas tampilan | `PASS` |
| CSS | Nol berkas CSS diubah oleh task ini | `PASS` |
| `npm run build` | Lulus 14 Oktober 2026 13.01.48–13.03.33: *Compiled successfully in 59s*, 492/492 halaman statis, 0 baris peringatan, `postbuild` standalone siap; `BUILD_ID` `CdZrb6rn2PX6y3xbZLSNX`; `data-testid` baru task ini ditemukan di `.next/static/chunks`. Server standalone pemilik (PID 27640) sudah berhenti saat izin diberikan; tidak dinyalakan ulang | `PASS` |

`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tim: unit test tidak ditulis atau dijalankan untuk task ini.`

`MANUAL TEST: NOT FEASIBLE — agent tidak memegang kredensial akun dokter uji; diuji pada putaran B.`

**Skenario uji layar untuk putaran B.**

| ID | Langkah | Hasil yang diharapkan | AT |
| --- | --- | --- | --- |
| 060-U1 | Masuk sebagai DPJP IGD aktif (`ranger.biru@admin.com`) sesudah perawat meminta TTD | Butir *Perlu TTD 1*; kartu pasien bertanda; akun tanpa izin `Sign` tidak melihat butir | — |
| 060-U2 | Klik butir, pilih pasien, **Buka Form** | Daftar tersaring; dialog berisi seluruh isian baca-saja, tanpa kolom isian | — |
| 060-U3 | **Tandatangani**, klik dua kali cepat | Satu permintaan admisi; dialog tertutup; butir berkurang; pada rekam jaringan kedua `POST /sign` membawa `Idempotency-Key` yang sama | `AT-IGD-225`, `235` |
| 060-U4 | Alihkan DPJP ke dokter lain lalu buka form dengan akun lama | Form hilang dari *Perlu TTD* akun lama; bila dialog terbuka, peringatan `canSign` dan/atau `403` tampil di dialog | `AT-IGD-226` |
| 060-U5 | **Kembalikan ke Perawat…** tanpa catatan, lalu dengan catatan | Tanpa catatan tombol nonaktif; dengan catatan form hilang dari daftar dan tampil *Draf* + catatan di layar perawat | `AT-IGD-227` |

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Butir *Perlu TTD* hanya bagi pemegang `EmergencyAdmissionRequest : Sign`, angka dari server | Kode terpenuhi; uji layar belum | `usePermission` + `GET /pending-signature` |
| 2 | Dialog menampilkan seluruh isian baca-saja; form tidak dapat diubah dari layar dokter | Kode terpenuhi; uji layar belum | `emergency-admission-signature-dialog.jsx` |
| 3 | *Tandatangani* berhasil sekali walau diklik ganda; form keluar dari daftar | Kode terpenuhi; uji layar belum | `Idempotency-Key` per dialog; muat ulang daftar |
| 4 | Sesudah DPJP dialihkan, form pindah ke daftar DPJP baru; penolakan `403` tampil di dialog | Kode terpenuhi; uji layar belum | Daftar dari server; `canSign`; pesan server di dialog |
| 5 | *Kembalikan* wajib bercatatan; form hilang dari daftar dan kembali *Draf* di layar perawat | Kode terpenuhi; uji layar belum | `requireReason`; `FE-IGD-059` menampilkan catatan pengembalian |
| 6 | Diff hanya berkas Cakupan; nol komentar baru; `eslint` 0 error; `npm run build` lulus | Terpenuhi — build 14 Oktober 2026 lulus | Bagian 6 |

DoD: laporan ini ✅; acceptance 1–5 terpetakan ke kode; 6 terpenuhi (build 14 Oktober 2026); uji layar pada putaran B.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta terhadap kartu | Tanda kartu pasien dilengkapi peringatan di atas tab pasien terpilih sebagai jalan masuk dialog — kartu pasien sendiri adalah tombol, jadi tombol di dalamnya akan bersarang. `ReadOnlyGroup`/`ReadOnlyItem` `FE-IGD-059` diekspor (+2/−2) agar tampilan baca-saja tidak digandakan |
| Keterbatasan | Saringan bekerja atas daftar pasien yang sudah termuat (maks. 100 kunjungan per halaman, `EMERGENCY_PHYSICIAN_LIST_PAGE_SIZE`). Pasien di luar halaman itu tetap terhitung di angka butir tetapi tidak tampil saat disaring |
| Risiko | Sedang — bila izin `Sign` belum diberikan pengurus Akses Role, butir tidak tampil sama sekali (sesuai acceptance 1) |
| Perubahan sampingan | `NONE` |
| Status Git | 6 `M` + 2 `??` milik task ini; berkas `M`/`??` lain di working tree milik sesi lain dan tidak disentuh. Nol stage, nol commit |
| Langkah berikutnya | Rizki: build ulang backend, nyalakan server standalone hasil build 16.42, lalu jalankan [panduan putaran B baru](../../../testing/2026-10-14-panduan-uji-putaran-b2-antrean-transfer-igd.md) |

## Penyesuaian — 14 Oktober 2026 (`IGD-DEC-304`)

Teks dialog tanda tangan di `emergency-admission-request-constant.jsx` (`bannerMessage`, `signStatement`, `signed`) kini menyebut pasien masuk Antrean Transfer IGD/Poli, bukan dikirim ke petugas admisi. Kode dialog dan hook tidak berubah. ESLint 0 error. Server uji standalone PID 10068 dihentikan atas izin pemilik; build ulang 14 Oktober 2026 16.42 lulus (`BUILD_ID` `ZLYxPkgFfju8K4gb4uRL0`, 492/492, 0 warning); server tidak dinyalakan ulang agent.
