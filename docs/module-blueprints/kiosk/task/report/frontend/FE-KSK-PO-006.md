# Laporan Perubahan Frontend — `FE-KSK-PO-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-PO-006` |
| Judul | Check-in pasien baru |
| Slice | `EPIC KSK-PO-02` — check-in di kiosk (`FR-KSK-PO-051`), rilis `MVP-2` |
| Roadmap | [online-registration-frontend-roadmap.md](../../../roadmap/online-registration-frontend-roadmap.md) — kartu `FE-KSK-PO-006` |
| Trace | `KSK-DEC-032`, `039`, `048`, `053`; desain `FE-KSK-PO-02` baris "Pindai kartu (pasien baru)" di [03-frontend-architecture.md](../../../03-frontend-architecture.md) |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`): `POST patients/kiosk`, `POST patient-identity-documents/kiosk`, `POST patient-insurances/kiosk` (sudah ada), `POST patient-encounters/kiosk`; validation matrix `KSK-VAL-PO-050` s.d. `052` |
| Wewenang UI | Langkah pindai mengikuti desain (KTP/KIA saja, NIK terkunci). Letak langkah di antara Ringkasan dan Cetak Antrean `DEV_DISCRETION` |
| Dependency | `FE-KSK-PO-005` ✅ 2026-11-25, `FE-KSK-PO-004` ✅ 2026-11-19, `BE-KSK-PO-007` ✅ 2026-11-25 |
| Klasifikasi | `HIGH` — kiosk membuat master pasien, dokumen identitas, dan kunjungan |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: view dan hook check-in (berkas `FE-KSK-PO-005`), satu hook pindai baru, service/konstanta/util check-in; laporan dan baris status blueprint kiosk (repository backend) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `7bba5cd1c` (`sukmagpV2`) + `FE-KSK-PO-005` dan rework `FE-KSK-PO-002` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `356bf463` (`sukmagp`) |
| Tanggal | 2026-11-25 |
| Status | ✅ Selesai — AC 9 dan AC 17 (tampilan dan alur) serta kriteria tambahan terbukti di browser dengan backend uji nyata dan agen pemindai tiruan; pemindai Plustek fisik belum diuji |

## 1. Keadaan yang ditemukan di awal

- `FE-KSK-PO-005` menampilkan ringkasan pendaftaran pasien baru, tetapi tombol Check-in nonaktif dan pasien diarahkan ke loket.
- `FE-KSK-PO-004` sudah menyiapkan varian komponen pindai Pasien Baru: prop `allowedIdentityTypes` (KTP/KIA saja) dan `lockedFieldNames` (NIK terkunci). Komponen itu belum dipakai di mana pun.
- Logika pemindai Pasien Baru (cek status Plustek, pindai, OCR) ada di dalam view Pasien Baru sepanjang 2.985 baris. Fungsi-fungsi pembentuknya (`scanKtpOcrFromLocalAgent`, `mapAgentIdentityFieldsToForm`, `getMissingScanIdentityFields`, `buildMasterPatientRequestBody`, `buildPatientIdentityDocumentRequest`) sudah diekspor dari helper dan service kiosk. Hook bersama `usePlustekKtpScanner` (IGD, admisi rawat inap) menerima fungsi pindai sendiri.

## 2. Proses bisnis dari sisi pengguna

1. Pasien baru memasukkan kode atau Lupa Kode seperti pasien lama. Ringkasan menulis "Pendaftaran ini untuk pasien baru. Langkah berikutnya: pindai KTP atau KIA pasien."
2. Bar langkah menjadi **Verifikasi → Ringkasan → Pindai Kartu → Cetak Antrean**. Pasien menekan **Lanjut Pindai KTP/KIA**.
3. Di langkah **Pindai Kartu**, kiosk hanya menawarkan **KTP** dan **KIA** (`KSK-VAL-PO-050`). Pasien menekan tombol pindai, lalu gambar kartu dan data hasil baca tampil.
4. Kolom NIK terkunci. Contoh hasil:
   - NIK tidak terbaca: "NIK tidak terbaca. Pindai ulang kartu, atau ke loket pendaftaran." (`051`), Check-in nonaktif.
   - NIK kartu berbeda dari NIK saat mendaftar online: "Data kartu identitas tidak sesuai dengan pendaftaran online. Silakan ke loket pendaftaran." (`052`), Check-in nonaktif.
   - NIK sama: "NIK pada kartu sama dengan pendaftaran online. Periksa data kartu, lalu tekan Check-in Sekarang."
5. Pasien boleh membetulkan kolom lain (nama, alamat, dan seterusnya), sama seperti Pasien Baru. Kolom wajib yang belum terbaca harus dilengkapi.
6. Saat **Check-in Sekarang** ditekan:
   1. Kiosk mencari master pasien dengan NIK yang sama persis (`KSK-DEC-053`). Bila ada, misalnya orang yang ternyata sudah pernah berobat, master itu yang dipakai beserta No. RM-nya.
   2. Bila belum ada, kiosk membuat master dari data kartu dan kontak dari isian online (`KSK-DEC-048`), lalu dokumen identitasnya.
   3. Bila pendaftaran membawa kartu asuransi baru, kartu dibuat untuk master itu.
   4. Kunjungan dibuat dengan `isNewPatient: true`, `onlineRegistrationId`, dan token.
7. Tiket antrean tampil dan dicetak seperti `FE-KSK-PO-005`.

Contoh kegagalan di tengah: master sudah terbuat, tetapi dokumen identitas gagal tersimpan. Pesan tampil dan pasien tetap di langkah pindai. Saat Check-in Sekarang ditekan lagi, kiosk **tidak** membuat master kedua; hanya dokumen identitas yang dicoba ulang, lalu kunjungan dibuat. Bila pasien memulai sesi baru, master yang sudah terbuat ditemukan lewat NIK.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`kiosk-new-patient-view.jsx` (pemindai, `checkScannerStatus`, `handleScanKtpFromAgent`, `handleUseScannedKtpData`, kelas `isScannerActivePage`); `kiosk-new-patient-step-scan-identity.jsx`; `kiosk-new-patient-registration.helpers.jsx`; `kiosk-new-patient-submit.helpers.jsx`; `kiosk-new-patient-registration.service.js` (`findExistingKioskPatientByIdentity`, `createKioskNewPatient`, `createKioskPatientIdentityDocument`); `use-plustek-ktp-scanner.js`; `plustek-scanner-agent`. Backend: `PatientController` (`POST /kiosk`), `CreatePatientRequest`, aturan `052` di `OnlineRegistrationCheckInService.Encounter.cs`.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/lib/hooks/kiosk/registration/use-kiosk-online-check-in-scan.js` | Baru | Langkah pindai: `usePlustekKtpScanner` dengan pindai kiosk (`scanKtpOcrFromLocalAgent`, pemindai Plustek pilihan agen), pemetaan OCR, kolom hilang seperti Pasien Baru, status NIK, props komponen pindai |
| `src/components/view/kiosk/registration/online-check-in/kiosk-online-check-in-step-scan.jsx` | Baru | `KioskNewPatientStepScanIdentity` (KTP/KIA, NIK terkunci) + pesan status NIK |
| `src/lib/hooks/kiosk/registration/use-kiosk-online-check-in.js` | Diubah | Langkah `scan`, `continueToScan`, `backToSummary`, persiapan master pasien baru per sesi, payload `isNewPatient` |
| `src/lib/services/kiosk/registration/kiosk-online-check-in.service.js` | Diubah | `findOrCreateOnlineCheckInPatient`, `createOnlineCheckInIdentityDocument`; kartu asuransi baru menerima `patientId` master baru |
| `src/components/view/kiosk/registration/online-check-in/kiosk-online-check-in-view.jsx` | Diubah | Render langkah pindai; footer Ringkasan pasien baru dan footer langkah pindai |
| `.../online-check-in/kiosk-online-check-in-step-summary.jsx` | Diubah | Catatan pasien baru menjadi arahan ke langkah pindai |
| `src/lib/constants/kiosk/registration/kiosk-online-check-in.constants.js` | Diubah | Langkah dan header Pindai Kartu, jenis kartu KTP/KIA, kolom terkunci, pesan `050`–`052` |
| `src/utils/kiosk/registration/kiosk-online-check-in.utils.js` | Diubah | `normalizeScanNik`, `getScanNikStatus`, `buildNewPatientContactFields`; payload kunjungan menerima `patientId` dan `isNewPatient` |
| `src/style/kiosk/registration/kiosk-online-check-in.module.css` | Diubah | `.scanStepContext` menetralkan tinggi layar penuh kelas `isScannerActivePage` di pembungkus langkah pindai |

Layar Pendaftaran Pasien Baru kiosk **tidak diubah**.

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Penerapan |
| --- | --- |
| Tidak menambah arsitektur paralel | Pemindai memakai hook bersama `usePlustekKtpScanner`; pindai, OCR, kolom hilang, body master, dan dokumen identitas memakai fungsi kiosk Pasien Baru yang sudah diekspor |
| View tidak memanggil HTTP | View → hook → service |
| Privasi | NIK pembanding (`identityNumberForScan`) tidak pernah tampil; NIK yang tampil hanya hasil baca kartu pasien sendiri, sama dengan Pasien Baru |
| Idempoten | Master per sesi disimpan di `ref`; master lintas sesi ditemukan lewat NIK (`matchType: "identityNumber"`); kartu asuransi dipakai ulang bila sudah tercatat |

### 3.4 Gerbang keputusan base component

`UI GATE: PASS — REUSE 3, COMPOSE 1, EXTEND 0, NEW 0`

| Elemen | Kandidat | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Langkah pindai (pilihan kartu, status pemindai, pratinjau, kolom OCR) | `KioskNewPatientStepScanIdentity` dengan prop `FE-KSK-PO-004` | Pasien Baru kiosk | REUSE | Tanpa diubah |
| Pesan status NIK | `errorAlert`, `oldPatientNotice` kiosk | `FE-KSK-PO-005` | REUSE | — |
| Footer aksi | `ActionFooter` layar check-in | `FE-KSK-PO-005` | REUSE | — |
| Konteks gaya langkah pindai | Kelas halaman `isScannerActivePage` Pasien Baru + `.scanStepContext` | Gaya komponen pindai bergantung pada kelas halaman itu | COMPOSE | Lihat pilihan di bawah |

Pilihan untuk konteks gaya langkah pindai:

1. **Pasang `isScannerActivePage` hanya di pembungkus langkah, lalu netralkan tinggi dan overflow-nya dengan kelas modul sendiri (direkomendasikan, dipilih).** Tampilan langkah pindai sama persis dengan Pasien Baru dan berkas Pasien Baru tidak tersentuh. Biayanya satu aturan CSS bertoken.
2. Salin aturan gaya langkah pindai ke CSS modul check-in. Tidak bergantung pada kelas halaman, tetapi menduplikasi ratusan baris gaya dengan warna literal lama, dan tampilannya bisa menyimpang bila Pasien Baru berubah.
3. Ubah komponen pindai agar tidak bergantung pada kelas halaman. Ini paling bersih jangka panjang, tetapi menyentuh komponen dan CSS Pasien Baru, sehingga risiko regresinya pada layar yang sudah dipakai.

## 4. State yang ditangani di layar

| Keadaan | Penanganan |
| --- | --- |
| Pemindai belum dicek / offline | Komponen pindai Pasien Baru: tombol cek pemindai, status Online/Offline, pesan agen |
| Sedang memindai | Lapisan "Sedang membaca KTP/KIA" komponen Pasien Baru; Check-in nonaktif |
| Belum pindai | Check-in Sekarang nonaktif; bila dipaksa: "Pindai KTP atau KIA pasien lebih dulu." |
| NIK tidak terbaca / berbeda | Pesan `051` / `052`, Check-in nonaktif, footer mengarahkan ke loket |
| Kolom wajib belum terbaca | Kolom ditandai seperti Pasien Baru; Check-in nonaktif sampai dilengkapi |
| Gagal menyimpan master/dokumen identitas | Pesan backend tampil; tetap di langkah pindai; coba ulang tanpa master ganda |
| `052` dari backend, `409`, token habis | Seperti `FE-KSK-PO-005` |
| Kembali | Tombol Kembali ke Ringkasan; Ulangi dan batas waktu membuang hasil pindai dan master sesi |

## 5. Endpoint yang dikonsumsi

| Endpoint | Kegunaan |
| --- | --- |
| Agen lokal `GET http://127.0.0.1:9100/status`, `POST /scanner/scan-ocr` | Status Plustek dan pindai + OCR AI kartu utuh (KTP/KIA), sama dengan Pasien Baru |
| `GET /api/v1/health-services/patient-management/master-data/patients/kiosk/options?search={NIK}` | Mencari master dengan NIK sama persis |
| `POST /api/v1/health-services/patient-management/master-data/patients/kiosk` | Membuat master dari data kartu + kontak online |
| `POST /api/v1/health-services/patient-management/master-data/patient-identity-documents/kiosk` | Dokumen identitas master baru |
| `POST /api/v1/health-services/patient-management/master-data/patient-insurances` | Kartu asuransi baru bila ada (sama dengan `FE-KSK-PO-005`) |
| `POST /api/v1/health-services/registration-management/patient-encounters/kiosk` | Kunjungan check-in, `isNewPatient: true`, `patientId` master |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint berkas task | 0 error, 0 warning | `PASS` | Keluaran eslint |
| `npm run lint:errors` | Exit 0 (`eslint . --quiet`) | `PASS` | `scratchpad/fe-po006/lint-errors.log` |
| `npm run build` | Exit 0; 856/856 halaman, `○ /kiosk/registration/online-check-in` | `PASS` | `scratchpad/fe-po006/build.log` |
| Grep anti-regresi | Tanpa warna literal, tanpa `console` | `PASS` | Grep |
| Uji browser `PO-20261125-0009` (backend uji 7185 nyata, agen pemindai tiruan di `127.0.0.1:9100` dengan payload OCR KTP yang dibaca parser kiosk) | 12/12 PASS (rincian di bawah) | `PASS` | `scratchpad/fe-po006/browser.cjs`, `browser.log`, `n-*.png` |
| Uji AC 9 "NIK lama" `PO-20261125-0005` (master dengan NIK sama dibuat lebih dulu lewat API) | 1/1 PASS (N15): master yang ada dipakai, tanpa `POST patients/kiosk`, kunjungan dengan `patientId` master itu, pendaftaran status 2 dengan No. RM | `PASS` | `browser-r6.cjs`, `browser-r6.log` |
| Uji tampilan + sukses kedua `PO-20261125-0002` | 2/2 PASS: nama pemindai di kotak status berwarna terang (terbaca), tanpa gulir mendatar; check-in pasien baru kedua `200`, status 2 dengan No. RM | `PASS` | `browser-r3.cjs`, `browser-r3.log`, `n-scan-r3.png` |
| Regresi alur pasien lama `FE-KSK-PO-005` (respons tiruan) | 11/11 PASS; K24 diperbarui: paket pasien baru kini berlanjut ke Pindai Kartu | `PASS` | `scratchpad/fe-po005/regression-mock.cjs`, `regression-mock.log` |
| Pemindai Plustek fisik | Tidak tersedia di sesi ini; agen ditiru dengan payload yang dibaca parser kiosk yang sama | `NOT RUN` | Uji di perangkat kiosk bersama pemilik |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — repository tidak memakai test runner untuk layar ini | — | `test-policy.md` |

Data uji `PO-20261125-*` semula bertanggal 2026-11-26; dengan izin pemilik (2026-11-25) tanggal kunjungannya dipindah ke 2026-11-25 lewat satu `UPDATE` 9 baris supaya token hari H terbit hari itu.

| Kode | Skenario | Hasil |
| --- | --- | --- |
| N1 | Paket pasien baru: bar 4 langkah, tombol Lanjut Pindai KTP/KIA | PASS |
| N2 | `050`: hanya KTP dan KIA, tanpa SIM | PASS |
| N3 | Sebelum pindai: Check-in nonaktif | PASS |
| N4 | `052`: NIK kartu berbeda → pesan, Check-in nonaktif | PASS |
| N5 | `051`: NIK tidak terbaca → pesan, Check-in nonaktif | PASS |
| N6 | NIK cocok: kolom NIK terkunci (diketik tetap), pesan cocok, Check-in aktif | PASS |
| N7 | Kegagalan di tengah (dokumen identitas `500`): pesan, master 1×, kunjungan belum dikirim | PASS |
| N8 | Coba lagi: master tetap 1×, dokumen identitas dicoba ulang, kunjungan 1× dengan `isNewPatient` | PASS |
| N9 | Tiket: antrean `P003` di layar = thermal, cetak otomatis 1× | PASS |
| N10 | AC 9 backend: pendaftaran status 2 dengan No. RM; master dengan NIK itu 0 → 1 | PASS |
| N11 | Satu dokumen identitas KTP tersimpan untuk master baru | PASS |
| N12 | `KSK-DEC-048`: nama dari kartu, kontak dari isian online | PASS |
| N15 | AC 9 NIK lama: master yang ada dipakai, tanpa master baru | PASS |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 9 (tampilan dan alur): pasien baru, master dibuat sesudah pindai; NIK lama memakai master lama | Terpenuhi | N8–N11, sukses kedua R3 (master baru); N15 (NIK lama memakai master yang ada) |
| AC 17 (tampilan dan alur): NIK KTP/KIA wajib sama; SIM ditolak; master dari pindai | Terpenuhi | N2, N4–N6, N12 |
| Tambahan: coba ulang sesudah gagal di tengah tidak membuat master ganda | Terpenuhi | N7, N8, N10 |
| DoD: tabel keputusan base component dan `UI GATE` | Terpenuhi | Bagian 3.4 |
| Uji di perangkat kiosk dengan pemindai fisik | Dikecualikan | Agen fisik tidak tersedia; parser dan alur sama dengan Pasien Baru |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | (1) Kode pemindaian memakai fungsi Pasien Baru tetapi orkestrasinya di hook sendiri; bila Pasien Baru mengubah parameter agen, hook ini perlu ikut diperbarui. (2) Gaya langkah pindai bergantung pada kelas halaman Pasien Baru `isScannerActivePage` (pilihan 1 bagian 3.4) |
| Data uji | Master pasien uji `KSKTEST PO026 R9`, `KSKTEST PO026 R3`, dan `KSKTEST PO026 R6 LAMA` beserta dokumen identitas dan kunjungannya (`scratchpad/fe-po006/fe006-ledger*.json`). Dibersihkan bersama data uji modul |
| Perubahan sampingan | `NONE`. Dev server 3000 dinyalakan agent untuk uji dan dihentikan sesudahnya |
| Status Git | Belum di-commit. Tidak ada commit, push, atau migration |
| Langkah berikutnya | Uji di kiosk fisik bersama pemilik (Plustek + printer thermal); pembersihan data uji modul |
