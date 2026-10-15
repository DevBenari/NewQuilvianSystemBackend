# Roadmap Revisi Pendaftaran Rawat Jalan — 12 Revisi (Amendment REG-REV)

## Metadata

| Field | Isi |
| --- | --- |
| Blueprint | `rawat-jalan` (umbrella `RJ-BIL-BP-001`) |
| Scope | **Registration / Pendaftaran Rawat Jalan** — prefix `RJ-REG`. Bukan scope Dokter (`RJ-DOC`), bukan Billing (`RJ-BIL`), bukan E2E (`RJ-E2E`) |
| Sumber requirement | Sukma Giri, 14 Okt 2026, *Master Prompt Rawat Jalan Revision V2* (12 revisi, 5 batch) |
| Keputusan | `RJ-REG-DEC-001`..`011` (bagian 2) |
| Baseline source | Backend `a52b3c98e7e9b0fae42f9a889b84c6c8332f8923` (`sukmagp`), frontend `70b3735dbae7e270a9eb0945f7f603541dfd42ca` (`sukmagpV2`) |
| `IMPLEMENTATION_AUTHORITY` | `GRANTED` dalam `CROSS-REPO MODE` (`RJ-REG-DEC-001`): backend dan frontend target tulis. Migration boleh dibuat dan diterapkan **hanya** ke `QuilvianNewDevSukma`. Tanpa commit, push, merge, deploy |
| Verifikasi | Backend pola Bank Darah (build Release, EF `has-pending-model-changes`, QBE Strict, runtime HTTP terhadap `QuilvianNewDevSukma`); frontend ESLint, `npm run build`, uji fungsi `node --test`, uji layar Playwright bila backend uji tersedia |
| Tanggal | 14 Oktober 2026 |

Legenda tanda status: `✅` selesai, `🟡` sebagian, `⛔` terblokir, tanpa tanda = belum dimulai.

Status requirement memakai klasifikasi master prompt (`NOT_STARTED`, `IN_PROGRESS`, `PARTIAL`,
`IMPLEMENTED_UNVERIFIED`, `TESTED`, `UAT_PENDING`, `DONE`, `BLOCKED`, `NEEDS_DECISION`).
Pemetaannya ke tanda roadmap resmi: `✅` = `TESTED`/`UAT_PENDING` (belum `DONE` sampai UAT
pengguna), `🟡` = `PARTIAL`/`IMPLEMENTED_UNVERIFIED`, `⛔` = `BLOCKED`/`NEEDS_DECISION`,
tanpa tanda = `NOT_STARTED`.

## 1. Pemetaan requirement → task

ID requirement dari master prompt dipakai apa adanya; belum pernah dipakai di roadmap mana pun
(pencarian `RJ-REG` pada `docs/` 14 Okt 2026: nihil). Task diberi infix `BE`/`FE` sehingga tidak
bentrok dengan ID requirement.

| Requirement | Isi | Batch | Task backend | Task frontend |
| --- | --- | ---: | --- | --- |
| `RJ-REG-REV-001` (REV-01) | Cari pasien berdasarkan tanggal lahir | 1 | `RJ-REG-REV-BE-001` | `RJ-REG-REV-FE-001` |
| `RJ-REG-REV-002` (REV-08) | Detail dan Edit di Daftar Pasien RJ | 1 | `RJ-REG-REV-BE-003` | `RJ-REG-REV-FE-002` |
| `RJ-REG-REV-003` (REV-09) | Input kewarganegaraan | 1 | `RJ-REG-REV-BE-002` | `RJ-REG-REV-FE-003` |
| `RJ-REG-REV-004` (REV-02) | Tampilan KTP penuh | 2 | — | `RJ-REG-REV-FE-004` |
| `RJ-REG-REV-005` (REV-05) | Scanner / kamera / upload manual | 2 | — | `RJ-REG-REV-FE-004` |
| `RJ-REG-REV-006` (REV-06) | Upload identitas pasien lama | 2 | `RJ-REG-REV-BE-004` | `RJ-REG-REV-FE-004` |
| `RJ-REG-REV-007` (REV-03) | Pemilihan dokter via card | 3 | — | `RJ-REG-REV-FE-005` |
| `RJ-REG-REV-008` (REV-10) | Tracking dan reschedule booking | 3 | `RJ-REG-REV-BE-005` | `RJ-REG-REV-FE-006` |
| `RJ-REG-REV-009` (REV-04) | Tampilan kartu asuransi | 4 | — | `RJ-REG-REV-FE-007` |
| `RJ-REG-REV-010` (REV-07) | Edit Penjamin asuransi | 4 | — | `RJ-REG-REV-FE-007` |
| `RJ-REG-REV-011` (REV-11) | Hapus Keluhan Utama dari Data Kunjungan | 5 | — | `RJ-REG-REV-FE-008` |
| `RJ-REG-REV-012` (REV-12) | Redesign Data Rujukan | 5 | — | `RJ-REG-REV-FE-008` |
| `RJ-REG-REV-013` (amendemen 15 Okt 2026) | Kartu jadwal dokter yang kuotanya penuh dinonaktifkan dan diberi keterangan *Penuh* | 6 | `RJ-REG-REV-BE-006` | `RJ-REG-REV-FE-009` |
| `RJ-REG-REV-014` (amendemen 15 Okt 2026) | Step baru *Jenis Kunjungan* sebelum Data Kunjungan; pilihan Jenis Kunjungan dihapus dari Data Kunjungan | 7 | — | `RJ-REG-REV-FE-010` |

## 2. Keputusan pemilik (14 Okt 2026, Sukma Giri)

| ID | Keputusan |
| --- | --- |
| `RJ-REG-DEC-001` | `CROSS-REPO MODE`, backend + frontend target tulis; migration dibuat dan diterapkan hanya ke `QuilvianNewDevSukma`; tanpa commit/push/merge/deploy |
| `RJ-REG-DEC-002` | REV-01 berlaku pada **cari pasien di Pendaftaran Rawat Jalan** (langkah 1). Backend `GET /patients` mendapat filter `birthDate` (date-only) |
| `RJ-REG-DEC-003` | REV-11: Keluhan Utama dihapus dari **langkah Data Kunjungan Pendaftaran RJ**. Skrining perawat, skrining dokter, SOAP, dan IGD tidak berubah |
| `RJ-REG-DEC-004` | REV-07: Edit Penjamin hanya **saat pendaftaran** (langkah Pembayaran, sebelum submit) lewat endpoint asuransi pasien yang sudah ada. Penjamin kunjungan yang sudah terdaftar **tidak** diubah (tetap milik `RJ-BIL-CONFLICT-001`) |
| `RJ-REG-DEC-005` | REV-08: Detail = kunjungan + data pasien (baca, bercakupan). Edit = kontak pasien (No. HP, WhatsApp, email, alamat), kewarganegaraan, dan Catatan kunjungan, hanya bila status kunjungan < `Sedang Konsultasi` (6), tidak batal/selesai. No. RM, NIK, nama, tanggal lahir, status, poli/dokter **tidak** diedit dari menu ini |
| `RJ-REG-DEC-006` | REV-09: `MstPatient.Citizenship` (`Wni=1`, `Wna=2`) + `NationalityCountryId` (FK `MstCountry`), wajib bila WNA; WNI tidak menyimpan negara. Data lama `null` |
| `RJ-REG-DEC-007` | REV-10: reschedule hanya appointment (`IsAppointment`) Rawat Jalan berklinik yang **belum datang**: status ≤ `Menunggu Dokter`, belum check-in, antrean belum dipanggil/diskrining; tanggal baru hari ini atau mendatang; poli tetap, jadwal dokter boleh berganti. Antrean lama dibatalkan dan antrean baru dialokasikan alokator yang sama dalam satu transaksi; kuota appointment, bentrok, dan duplikat diperiksa; alasan wajib; ID kunjungan tetap; riwayat tercatat |
| `RJ-REG-DEC-008` | REV-06: KTP pasien lama diunggah sebagai `MstPatientIdentityDocument` ke **penyimpanan privat** (`FileStorage:PrivateRootPath`), unduh berizin `no-store`, mengganti dokumen = dokumen lama dinonaktifkan (tidak ditimpa), tercatat audit. `PhotoPath` publik lama tidak disentuh |
| `RJ-REG-DEC-009` | Delta implementasi (`DEV_DISCRETION`): REV-05 untuk **pasien baru** memakai jalur kanonik yang sama dengan scanner (`photoBase64` pada create pasien); kamera/upload manual menghasilkan gambar yang sama bentuknya. Upload untuk **pasien lama** memakai jalur `RJ-REG-DEC-008` |
| `RJ-REG-DEC-010` | Amendemen 15 Okt 2026 (Sukma Giri): badge kuota di kartu jadwal dokter dihapus. Jadwal yang kuotanya penuh pada tanggal kunjungan dinonaktifkan dan diberi keterangan *Penuh*. Data kuota terpakai disediakan **endpoint backend baru di area Registration**, bukan field tambahan di master data `doctor-schedules/admin`. Hitungannya wajib sama dengan validasi simpan `PatientEncounterController.ValidateDoctorScheduleQuotaAsync`: antrean `RegQueue` aktif (tidak dihapus, tidak batal) per `QueueDate` + `DoctorScheduleId`; total dibandingkan `MaxPatientQuota`, walk-in dengan `MaxWalkInQuota`, appointment dengan `MaxAppointmentQuota`; nilai kuota `0` berarti tanpa batas. Validasi simpan tetap menjadi penjaga akhir |
| `RJ-REG-DEC-011` | Amendemen 15 Okt 2026 (Sukma Giri): Pendaftaran RJ mendapat step tersendiri **Jenis Kunjungan** (Umum / Rujukan-Tindak Lanjut) tepat sesudah Cari/Input Pasien dan sebelum Data Kunjungan. Pilihan Jenis Kunjungan dihapus dari step Data Kunjungan. Urutan step: Umum = Pasien → Jenis Kunjungan → Data Kunjungan → Pembayaran → General Consent → Verifikasi → Selesai; Rujukan menyisipkan Data Rujukan sesudah Data Kunjungan seperti sebelumnya (`RJ-DOC-DEC-072`). Mengganti Rujukan → Umum yang sudah berisi data rujukan tetap meminta konfirmasi. Payload `visitType` dan backend tidak berubah. Tampilan kartu pilihan bersifat `DEV_DISCRETION` |

## Grafik Urutan Dependency

```text
Batch 1
RJ-REG-REV-BE-001 ✅ ─> RJ-REG-REV-FE-001 ✅
RJ-REG-REV-BE-002 ✅ ─┬─> RJ-REG-REV-FE-003 ✅
                     └─> RJ-REG-REV-BE-003 ✅ ─> RJ-REG-REV-FE-002 ✅

Batch 2
RJ-REG-REV-BE-004 ✅ ─> RJ-REG-REV-FE-004 ✅

Batch 3
RJ-REG-REV-FE-005 ✅
RJ-REG-REV-BE-005 ⛔ ─> RJ-REG-REV-FE-006 ⛔   (dicabut 15 Okt 2026)

Batch 4
RJ-REG-REV-FE-007 ✅

Batch 5
RJ-REG-REV-FE-008 ✅

Batch 6
RJ-REG-REV-BE-006 ✅ ─> RJ-REG-REV-FE-009 ✅

Batch 7
RJ-REG-REV-FE-010 ✅
```

| Gelombang | Task | Boleh mulai bila |
| ---: | --- | --- |
| 1 | `BE-001`, `BE-002`, `BE-004`, `BE-005`, `FE-005`, `FE-007`, `FE-008` | Wewenang `RJ-REG-DEC-001` |
| 2 | `BE-003`, `FE-001`, `FE-003`, `FE-004`, `FE-006` | Prasyarat backend masing-masing selesai |
| 3 | `FE-002` | `BE-003` selesai |
| 4 | `BE-006` | Keputusan `RJ-REG-DEC-010` |
| 5 | `FE-009` | `BE-006` selesai |
| 6 | `FE-010` | Keputusan `RJ-REG-DEC-011` |

Urutan eksekusi mengikuti batch master prompt (1 → 5), lalu batch amendemen 6 dan 7.

## 3. Task backend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-REG-REV-BE-001` | **Filter tanggal lahir pasien.** `GET /patients` dan `GET /patients/admin` menerima `birthDate` (`yyyy-MM-dd`, date-only) yang dapat digabung dengan `search`. Kiosk tidak berubah | — | 1. `birthDate` saja → hanya pasien bertanggal lahir itu. 2. `birthDate` + `search` → irisan keduanya. 3. Format salah → `400`. 4. Tanpa `birthDate` perilaku lama identik. 5. Izin `Patient : Read` tetap | Build; QBE; runtime HTTP | ✅ `14 Okt 2026` — build Release `0 Error`, EF tanpa drift, QBE Strict `PASS` (18 berkas), runtime B1–B7 `PASS`. `403` tanpa izin lewat kode (akun uji SuperAdmin). [Laporan](../task/report/backend/RJ-REG-REV-BE-001.md) |
| ✅ `RJ-REG-REV-BE-002` | **Kewarganegaraan pasien.** Enum `PatientCitizenship`, kolom `Citizenship` + `NationalityCountryId` pada `MstPatient` (configuration + satu migration); create/update/response pasien; `PUT` tanpa `citizenship` mempertahankan nilai lama | — | 1. Create WNI tersimpan tanpa negara. 2. Create WNA tanpa negara → `400`; dengan negara valid tersimpan. 3. Negara tidak ada/nonaktif → `400`. 4. Detail mengembalikan `citizenship`, `citizenshipName`, `nationalityCountryId`, `nationalityCountryName`. 5. `PUT` tanpa field mempertahankan nilai. 6. Migration hanya dua kolom + FK + index | Build; EF; QBE; migration ke `QuilvianNewDevSukma`; runtime HTTP | ✅ `14 Okt 2026` — migration `20261014044508_AddPatientCitizenship` diterapkan ke `QuilvianNewDevSukma`, runtime C1–C7 `PASS` (Batch 1 total 29/29). Catatan: master negara DB uji hanya Indonesia. [Laporan](../task/report/backend/RJ-REG-REV-BE-002.md) |
| ✅ `RJ-REG-REV-BE-003` | **Detail dan edit data pendaftaran di Daftar Pasien RJ.** `GET /outpatient-encounters/{id}` dan `PATCH /outpatient-encounters/{id}/registration-data` pada `OutpatientEncounterController`, bercakupan `ClinicalActorScopeService` | `BE-002` | 1. Detail dalam cakupan `200`; di luar cakupan/tidak ada `404`; tanpa izin `403`. 2. Edit kontak, kewarganegaraan, catatan tersimpan dan terbaca ulang. 3. Status ≥ 6, batal, atau selesai → `400` dengan alasan. 4. Field identitas (nama, No. RM, NIK, tanggal lahir) tidak dapat diubah lewat endpoint ini. 5. Perubahan tercatat audit (nama field, tanpa nilai PII). 6. `canEdit` pada detail sesuai izin + status | Build; QBE; runtime HTTP | ✅ `14 Okt 2026` — runtime D1–D10 `PASS`; `403` lewat kode. Action baru `OutpatientEncounter : Update` perlu dicentang admin. [Laporan](../task/report/backend/RJ-REG-REV-BE-003.md) |
| ✅ `RJ-REG-REV-BE-004` | **Dokumen identitas privat.** `PatientIdentityDocumentFileService` (pola `ReferralDocumentStorageService`) + `POST /patient-identity-documents/admin/upload` (multipart) dan `GET /patient-identity-documents/admin/{id}/content` | — | 1. JPG/PNG/PDF ≤ batas → `200`, dokumen primer baru tertaut ke pasien. 2. Ekstensi palsu (isi bukan gambar) → `400`; melebihi batas → `400`. 3. Pasien tidak ada → `404`. 4. Sudah ada dokumen berkas aktif tanpa `replaceDocumentId` → `409`; dengan `replaceDocumentId` milik pasien yang sama → dokumen lama nonaktif, baru primer. 5. Unduh berizin `inline` + `no-store`; berkas tidak dapat dibuka lewat `/uploads`. 6. Tanpa izin `403`. 7. Tidak membuat pasien baru; audit tanpa isi berkas | Build; QBE; runtime HTTP | ✅ `14 Okt 2026` — runtime I0–I14 `PASS` (run 2; run 1 menemukan bentrok unique index → dokumen lama dihapus lunak). `403` lewat kode; antivirus belum ada. [Laporan](../task/report/backend/RJ-REG-REV-BE-004.md) |
| ⛔ `RJ-REG-REV-BE-005` | **[DICABUT 15 Okt 2026 — lihat bagian 7]** **Tracking dan reschedule appointment.** `OutpatientAppointmentController` (`GET /`, `GET /filters/metadata`, `GET /{id}`, `POST /{id}/reschedule`), `OutpatientAppointmentService`, entity `RegEncounterReschedule` + satu migration | — | 1. Daftar appointment RJ berhalaman dengan filter tanggal, poli, dokter, status, cari. 2. Detail memuat pasien, dokter, poli, jadwal, antrean, status, riwayat reschedule. 3. Reschedule valid → `200`: ID kunjungan tetap, antrean lama `Cancelled`, antrean baru pada tanggal baru, riwayat tersimpan. 4. Ditolak `400`: bukan appointment, sudah datang/dipanggil, tanggal lampau, jadwal tidak cocok tanggal/poli, tidak menerima appointment, kuota penuh, penjamin tidak berlaku, tanggal+jadwal sama, alasan kosong. 5. Bentrok: pasien punya kunjungan aktif lain di poli+tanggal yang sama → `409`. 6. Dua reschedule bersamaan tidak menghasilkan antrean ganda. 7. Tanpa izin `403` | Build; EF; QBE; migration ke `QuilvianNewDevSukma`; runtime HTTP | ✅ `14 Okt 2026` — migration `20261014050013_AddEncounterRescheduleHistory` diterapkan, runtime R0–R15 `PASS` (37/37 bersama BE-004). Delta: antrean dipindah di tempat (`IX_RegQueue_EncounterId`). R12 bentrok `409` `NOT FEASIBLE` (dicegah penjaga pendaftaran). [Laporan](../task/report/backend/RJ-REG-REV-BE-005.md) |
| ✅ `RJ-REG-REV-BE-006` | **Kuota terpakai jadwal dokter per tanggal kunjungan.** `GET /api/v1/health-services/registration-management/patient-encounters/doctor-schedule-quotas?visitDate=yyyy-MM-dd&clinicId={guid?}` pada `PatientEncounterController`, izin `PatientEncounter : Read`. Respons per jadwal: `doctorScheduleId`, `maxPatientQuota`, `maxWalkInQuota`, `maxAppointmentQuota`, `usedTotal`, `usedWalkIn`, `usedAppointment`, `isFullForWalkIn`, `isFullForAppointment`. Logika hitung diekstrak menjadi satu helper yang juga dipakai `ValidateDoctorScheduleQuotaAsync`, sehingga tampilan dan validasi simpan tidak bisa berbeda. Contoh: `MaxPatientQuota = 18` dengan 18 antrean aktif → kedua `isFull*` bernilai `true`. Tanpa migration | `RJ-REG-DEC-010` | 1. Jadwal dengan antrean aktif = `MaxPatientQuota` → `isFullForWalkIn` dan `isFullForAppointment` `true`. 2. Kuota walk-in penuh, total belum → hanya `isFullForWalkIn` `true`; berlaku sama untuk appointment. 3. Antrean batal/terhapus tidak dihitung. 4. Kuota `0` → tidak pernah penuh. 5. `visitDate` salah format/kosong → `400`; tanpa izin → `403`. 6. Validasi simpan berperilaku identik dengan sebelumnya. QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi dari `AGENTS.md` backend | Build Release; EF tanpa drift; QBE Strict; runtime HTTP terhadap `QuilvianNewDevSukma` | ✅ `15 Okt 2026` — build Release `0 Error`, EF tanpa drift, QBE Strict `PASS` (4 berkas), runtime R0–R12 13/13 `PASS`. Belum: uji `403` akun tanpa izin dan simpan saat penuh (UAT). [Laporan](../task/report/backend/RJ-REG-REV-BE-006.md) |

## 4. Task frontend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-REG-REV-FE-001` | **Tanggal lahir pada cari pasien Pendaftaran RJ.** Date picker di samping kolom cari (khusus RJ), terkirim `birthDate=yyyy-MM-dd`; reset mengosongkan | `BE-001` | 1. Cari dengan tanggal lahir saja / gabungan menghasilkan pasien yang sesuai. 2. Tanggal tidak bergeser zona waktu. 3. Reset mengembalikan default. 4. Pendaftaran IGD tidak berubah | ESLint; build; uji fungsi; uji layar | ✅ `14 Okt 2026` — lint/build `PASS`, uji layar U1–U3 `PASS`. [Laporan](../task/report/frontend/RJ-REG-REV-FE-001.md) |
| ✅ `RJ-REG-REV-FE-002` | **Detail dan Edit di Daftar Pasien RJ.** Aksi baris *Detail* (halaman detail) dan *Edit* (form) memakai endpoint `BE-003` | `BE-003` | 1. Detail terbuka untuk kunjungan dalam cakupan. 2. Edit hanya tampil bila `canEdit`. 3. Simpan berhasil memperbarui detail; galat backend tampil. 4. Field identitas baca saja | ESLint; build; uji layar | ✅ `14 Okt 2026` — lint/build `PASS`, uji layar L1–L5 `PASS`. [Laporan](../task/report/frontend/RJ-REG-REV-FE-002.md) |
| ✅ `RJ-REG-REV-FE-003` | **Kewarganegaraan pada pasien baru RJ.** Pilihan WNI/WNA; negara wajib bila WNA (dari master negara); terkirim pada create pasien; tampil di review | `BE-002` | 1. WNI default. 2. WNA tanpa negara ditolak di layar. 3. Nilai tersimpan dan terbaca ulang. 4. IGD tidak berubah | ESLint; build; uji fungsi | ✅ `14 Okt 2026` — uji layar N4–N7 `PASS`, terbaca ulang via API. [Laporan](../task/report/frontend/RJ-REG-REV-FE-003.md) |
| ✅ `RJ-REG-REV-FE-004` | **Identitas: KTP utuh, metode scanner/kamera/upload, lengkapi KTP pasien lama.** Preview `contain` + perbesar; pilihan metode pada pasien baru; panel identitas pasien lama memakai `BE-004` | `BE-004` | 1. KTP tampil utuh (landscape/portrait). 2. Scanner existing tetap jalan. 3. Upload file dan kamera menghasilkan preview + Ganti Foto/Batalkan. 4. Pasien tanpa dokumen dapat menambah identitas; yang sudah ada menampilkan dokumen dan Ganti hanya bila berizin. 5. Berkas salah ditolak dengan pesan | ESLint; build; uji fungsi; uji layar | ✅ `14 Okt 2026` — uji layar U4–U11, N1–N3 `PASS`. Kamera & scanner fisik `NOT FEASIBLE` (uji UAT). [Laporan](../task/report/frontend/RJ-REG-REV-FE-004.md) |
| ✅ `RJ-REG-REV-FE-005` | **Kartu dokter.** Setelah poliklinik dipilih, jadwal dokter tampil sebagai kartu (nama, spesialis, jam/sesi/ruang, kuota bila ada, terpilih); tanpa dropdown dokter | — | 1. Kartu tampil otomatis. 2. Tidak ada dropdown dokter kedua. 3. Ganti poli mengosongkan pilihan dokter. 4. Loading/kosong/galat tersedia. 5. Step Rujukan ikut memakai kartu | ESLint; build; uji fungsi; uji layar | ✅ `14 Okt 2026` — uji layar U13–U16, N11 `PASS`. [Laporan](../task/report/frontend/RJ-REG-REV-FE-005.md) |
| ⛔ `RJ-REG-REV-FE-006` | **[DICABUT 15 Okt 2026 — lihat bagian 7]** **Menu Tracking Jadwal Booking.** Halaman daftar (Hero, DataFilter, DataTable, StatusBadge), detail, dan modal reschedule memakai `BE-005` | `BE-005` | 1. Menu sesuai izin. 2. Detail terbuka. 3. Reschedule valid tersimpan dan tampil. 4. Penolakan backend tampil. 5. Tidak ada antrean ganda | ESLint; build; uji layar | ✅ `14 Okt 2026` — uji layar T1–T7 `PASS` (termasuk 390 px). [Laporan](../task/report/frontend/RJ-REG-REV-FE-006.md) |
| ✅ `RJ-REG-REV-FE-007` | **Kartu asuransi utuh dan Edit Penjamin.** Preview kartu `contain` + perbesar; tombol *Edit Penjamin* bila pembayaran Asuransi, form memakai `PUT /patient-insurances/admin/{id}` | — | 1. Kartu tidak terpotong. 2. Data hasil scan tetap. 3. Edit Penjamin hanya pada Asuransi. 4. Data valid tersimpan dan panel pembayaran sinkron. 5. Tanpa izin `PatientInsurance : Update` tombol tidak tampil | ESLint; build; uji layar | ✅ `14 Okt 2026` — uji layar U17–U20 `PASS`; eligibility/kartu tetap sesudah PUT. Pemotongan kartu asli belum tereproduksi (UAT). [Laporan](../task/report/frontend/RJ-REG-REV-FE-007.md) |
| ✅ `RJ-REG-REV-FE-008` | **Data Kunjungan tanpa Keluhan Utama + redesign Data Rujukan.** Hapus field dari langkah 2 RJ (payload tidak mengirim nilai palsu); susun ulang step Data Rujukan menjadi kelompok Sumber Rujukan, Surat Rujukan, Informasi Klinis & Lampiran, Unit Tujuan | — | 1. Keluhan Utama tidak tampil di langkah 2 RJ. 2. Pendaftaran tetap dapat diselesaikan. 3. IGD, skrining, SOAP tidak berubah. 4. Field wajib dan dependency rujukan tetap; payload sama. 5. Rujukan tersimpan dan terbaca ulang | ESLint; build; uji fungsi; uji layar | ✅ `14 Okt 2026` — uji layar U12, N8–N11, R1–R3 `PASS` (rujukan tersimpan & terbaca ulang). [Laporan](../task/report/frontend/RJ-REG-REV-FE-008.md) |
| ✅ `RJ-REG-REV-FE-009` | **Kartu jadwal dokter penuh dinonaktifkan.** Step Data Kunjungan (dan step Data Rujukan yang memakai kartu yang sama) memanggil endpoint `BE-006` untuk tanggal kunjungan terpilih. Pendaftaran hari ini dibaca sebagai walk-in (`isFullForWalkIn`), tanggal mendatang sebagai appointment (`isFullForAppointment`). Kartu penuh: nonaktif (`disabled`, `aria-disabled`), tombol aksi bertuliskan *Penuh*, tidak bisa dipilih. Bila jadwal yang sudah terpilih ternyata penuh setelah tanggal diganti, pilihan dikosongkan. Bentuk visual keterangan *Penuh* bersifat `DEV_DISCRETION` | `BE-006` | 1. Jadwal penuh tampil nonaktif dengan keterangan *Penuh* dan tidak bisa dipilih. 2. Jadwal belum penuh tetap bisa dipilih. 3. Ganti tanggal memperbarui status penuh. 4. Gagal memuat kuota tidak menonaktifkan kartu; validasi simpan backend tetap menolak bila penuh. 5. Badge kuota tidak tampil | ESLint; build; uji layar | ✅ `15 Okt 2026` — ESLint exit 0, `npm run build` sukses, uji layar F1–F6 6/6 `PASS` (respons kuota di-mock; endpoint asli teruji di BE-006). Belum: ganti tanggal di layar dan data penuh asli (UAT). [Laporan](../task/report/frontend/RJ-REG-REV-FE-009.md) |
| ✅ `RJ-REG-REV-FE-010` | **Step Jenis Kunjungan.** Step baru *Jenis Kunjungan* di antara Cari/Input Pasien dan Data Kunjungan, pada bar step Umum maupun Rujukan. Pilihan Umum / Rujukan-Tindak Lanjut dipindah dari Data Kunjungan ke step ini beserta konfirmasi penghapusan data rujukan. Data Kunjungan hanya memuat tanggal, poliklinik, dan jadwal dokter. Contoh: petugas memilih *Rujukan / Tindak Lanjut* di step 2, lalu bar step berubah menampilkan Data Rujukan sesudah Data Kunjungan | — | 1. Bar step menampilkan *Jenis Kunjungan* sebagai step 2 (Umum dan Rujukan). 2. Lanjut dari pasien membuka step Jenis Kunjungan. 3. Data Kunjungan tidak lagi memuat pilihan Jenis Kunjungan. 4. Memilih Rujukan menambah step Data Rujukan; Rujukan → Umum dengan data rujukan meminta konfirmasi. 5. Kembali dari Data Kunjungan membuka Jenis Kunjungan. 6. Pendaftaran Umum dan Rujukan tetap terkirim dengan `visitType` yang benar | ESLint; build; uji layar | ✅ `15 Okt 2026` — ESLint exit 0, `npm run build` sukses, uji layar V1–V11 11/11 `PASS`, tata letak kartu 6 lebar `PASS`. Belum: konfirmasi Rujukan → Umum dengan data terisi dan submit penuh (UAT). [Laporan](../task/report/frontend/RJ-REG-REV-FE-010.md) |

## 5. Progres — `CURRENT STATE` 14 Oktober 2026

Dihitung per requirement (bagian 1). Progres scope ini **tidak** digabung dengan `RJ-DOC`, `RJ-BIL`, atau
`RJ-E2E` (lihat [MODULE-STATUS.md](../MODULE-STATUS.md)).

> **15 Okt 2026:** requirement `RJ-REG-REV-008` (REV-10) beserta `BE-005` dan `FE-006` dicabut pemilik (bagian 7). Angka di tabel ini adalah snapshot 14 Okt dan masih menghitungnya.

| Ukuran | Requirement | Task |
| --- | ---: | ---: |
| Total | 14 | 16 (6 BE, 10 FE) |
| `DONE` (UAT pengguna lulus) | 0 | 0 |
| `TESTED` (acceptance teknis terverifikasi, `✅`) | 14 | 16 |
| `UAT_PENDING` | 14 | — |
| `IN_PROGRESS` / `PARTIAL` / `IMPLEMENTED_UNVERIFIED` | 0 | 0 |
| `NOT_STARTED` | 0 | 0 |
| `BLOCKED` / `NEEDS_DECISION` | 0 | 0 |

Requirement `RJ-REG-REV-001`..`012` berstatus **`TESTED` → `UAT_PENDING`**, bukan `DONE`: UAT pengguna, perangkat fisik
(kamera, scanner Plustek), dan uji `403` dengan akun tanpa izin belum dilakukan.

| Requirement | Status |
| --- | --- |
| `RJ-REG-REV-001`..`012` | `TESTED` / `UAT_PENDING` |
| `RJ-REG-REV-013` | `TESTED` / `UAT_PENDING` |
| `RJ-REG-REV-014` | `TESTED` / `UAT_PENDING` |

## 6. Rekonsiliasi per batch

Snapshot source: backend `a52b3c98` + working tree, frontend `70b3735d` + working tree (belum di-commit).
Validasi bersama: backend build Release `0 Error`, EF tanpa drift, QBE Strict `PASS` (18 berkas); frontend
`npm run lint:errors` exit 0, `npm run build` exit 0, unit test 2823 lulus / 30 gagal identik dengan `HEAD`
(modul lain).

| Batch | Requirement | Bukti runtime | Bukti layar | Hasil |
| ---: | --- | --- | --- | --- |
| 1 | 001, 002, 003 | `rt_batch1` 29/29 | U1–U3, L1–L5, N4–N7 | ✅ |
| 2 | 004, 005, 006 | `rt_batch23` I0–I14 | U4–U11, U9b, N1–N3 | ✅ |
| 3 | 007, 008 | `rt_batch23` R0–R15 (37/37) | U13–U16, T1–T7 | ✅ |
| 4 | 009, 010 | — (endpoint existing) | U17–U20 + verifikasi `GET` | ✅ |
| 5 | 011, 012 | — | U12, N8–N11, R1–R3 | ✅ |

### Blocker, keputusan, dan dependency terbuka

| Butir | Pemilik | Menahan |
| --- | --- | --- |
| Centang action baru di Akses Role: `OutpatientEncounter : Update` (`OutpatientAppointment` sudah dicabut, lihat bagian 7) | Admin | Pemakaian oleh petugas non-SuperAdmin |
| Master `MstCountry` hanya berisi Indonesia di DB uji | Pemilik master wilayah | WNA dengan negara asing |
| Kebijakan antivirus/malware scanning unggahan | Security | Aktivasi production unggah identitas |
| `MstPatient.PhotoPath` (Kiosk/IGD) masih di `/uploads` publik | Pemilik Patient/Kiosk | Privasi foto KTP lama (di luar scope) |
| Migration ke DB master/production | Pemilik rilis | Rilis |
| UAT perangkat fisik (kamera, Plustek), kartu asuransi nyata | Product owner | `DONE` |

## 7. Revisi pemilik 15 Oktober 2026 — Tracking Jadwal Booking dicabut, Daftar Pasien RJ dirapikan

Keputusan pemilik (Sukma Giri, 15 Okt 2026): menu **Tracking Jadwal Booking** dihapus; penelusuran
kunjungan (termasuk appointment) dilakukan dari **Daftar Pasien Rawat Jalan**. Aksi baris *Koreksi
Rujukan* dan *Pengajuan Rawat Inap* tidak diperlukan dan dihapus.

| Area | Perubahan |
| --- | --- |
| Backend dicabut | `OutpatientAppointmentController`, `OutpatientAppointmentService`, `OutpatientAppointmentDtos` (resource izin `OutpatientAppointment`); entity `RegEncounterReschedule` + migration `20261015075431_DropEncounterRescheduleHistory` (drop tabel `RegEncounterReschedule`, riwayat reschedule ikut hilang); `GET /outpatient-encounters/{id}/inpatient-admission` beserta `DoctorCertificateService.GetInpatientAdmissionMonitorAsync` dan `OutpatientInpatientAdmissionMonitorResponse`; saringan `referralStatus` pada `GET /outpatient-encounters` |
| Backend ditambah | List: `scheduleLabel`, `createdAt`, `createdByName`, `updatedAt`, `updatedByName`. Detail: `canCancel`, `cancelBlockedReason` |
| Frontend dicabut | Halaman/menu `outpatient-appointments`; modal + hook rujukan dan pengajuan rawat inap di Daftar Pasien RJ; kolom Aksi; saringan Status Rujukan |
| Frontend — daftar | Saringan satu baris (≥ 1200px): Tanggal Awal, Tanggal Akhir, Periode, Klinik, Dokter, Status, cari, Jumlah baris, ↻ (atur ulang + muat ulang). Kolom: No, Tanggal & Jadwal, Pasien, Klinik, Dokter, Penjamin, Dibuat Oleh, Diupdate Oleh, Status. Detail dibuka dengan klik ganda pada baris |
| Frontend — detail | Hero: Kembali, Perbarui (aktif bila `canEdit`), Hapus = batalkan kunjungan (aktif bila `canCancel`); alasan nonaktif sebagai tooltip. Kartu samping *Edit Data Pendaftaran* dihapus |

Bukti: build backend 0 error, `has-pending-model-changes` bersih, QBE Strict PASS, uji HTTP 7/7 (backend
uji 7185, DB `QuilvianNewDevSukma`), uji layar Playwright 6/6, ESLint bersih. Laporan task `BE-005` dan
`FE-006` dipertahankan sebagai catatan historis.

### 7.1 Tambahan 15 Oktober 2026 — status Booking, jam dokter, kartu saringan

| Area | Perubahan |
| --- | --- |
| Status Booking | Kunjungan yang didaftarkan **sebelum** hari kunjungannya (`RegisteredAt` < tengah malam WIB `EncounterDate`) dan pasiennya belum datang: belum batal/selesai/tidak hadir, belum check-in, status ≤ Menunggu Perawat. Satu aturan: `OutpatientEncounterRules.IsBookingState`. List mengirim `isBooking`; metadata Status menambah opsi `booking` (paling atas); saringan `bookingOnly=true`. Badge Status menampilkan *Booking* |
| Rentang otomatis | Memilih Status *Booking* menggeser rentang ke hari ini s.d. +30 hari, karena preset Periode berakhir hari ini sementara booking selalu bertanggal ke depan |
| Jam dokter | `scheduleLabel` memakai jadwal yang tersimpan di kunjungan; bila `DoctorScheduleId` kosong, diambil dari jadwal aktif dokter di klinik kunjungan pada hari itu (tanggal khusus lebih dulu, lalu jadwal mingguan dalam masa berlaku). Tetap kosong bila dokter tidak punya jadwal di klinik tersebut |
| Kartu saringan | Jarak bar ke tepi kartu disamakan di keempat sisi; lebar tanggal dan Jumlah baris disesuaikan agar tidak terpotong |

Bukti: build 0 error, QBE Strict PASS, uji HTTP B1–B6 PASS (Booking API = hitungan SQL), uji layar K1–K2 PASS.
Data uji yang dibuat: pasien `RJBOOK UJI CEDE` + appointment 19 Okt 2026 (Poli Penyakit Dalam, dr. Dewi Lestari).

### 7.2 Tambahan 15 Oktober 2026 — detail dan form Perbarui berkelompok, ganti jadwal dokter yang sama

| Area | Perubahan |
| --- | --- |
| Detail | Kartu dibagi *Data Kunjungan* (kunjungan + pasien, urut berpasangan) dan *Jadwal Dokter* (Poliklinik, Dokter, Jadwal Praktik, Antrean). Status detail ikut menampilkan *Booking* (`isBooking`) |
| Form Perbarui | `BaseGroupedEditorView` dengan kelompok yang sama. Jadwal Dokter: Poliklinik dan Dokter hanya tampil (tidak dapat diganti); Tanggal Kunjungan + Jadwal Praktik dapat diubah |
| Backend | `OutpatientEncounterScheduleService`: `GET /outpatient-encounters/{id}/schedule-options?visitDate=` (sesi praktik dokter yang sama, klinik dan unit sama, beserta kuota); `PATCH /{id}/registration-data` menerima `visitDate` + `doctorScheduleId` (keduanya kosong = tidak diubah) dalam transaksi yang sama. Aturan: belum datang/diproses, tanggal ≥ hari ini, jadwal aktif dokter yang sama, kuota (`DoctorScheduleQuotaService`), tidak bentrok kunjungan pasien di poli yang sama, penjamin berlaku. Antrean hidup dipindah (ID tetap, nomor baru dari `OutpatientQueueNumberAllocator`), notifikasi antrean sesudah commit, audit mencatat jadwal lama/baru. Detail menambah `doctorScheduleId`, `canChangeSchedule`, `scheduleChangeBlockedReason` |

Bukti: build 0 error, QBE Strict PASS, registry izin valid, uji HTTP S1–S10 PASS (termasuk tolak jadwal dokter lain, tanggal lampau, sesi kosong; antrean tetap satu dan pindah), uji layar E1–E3 PASS. Catatan kontrak: field kontak bernilai `null` pada `PATCH registration-data` berarti dikosongkan — klien wajib mengirim semua field kontak.

### 7.3 Revisi 15 Oktober 2026 — jadwal tersedia satu pilihan

Form Perbarui tidak lagi meminta pilih tanggal lalu sesi. Kelompok *Jadwal Dokter* berisi satu select
**Jadwal Tersedia**: semua pasangan tanggal + sesi praktik dokter yang sama (poliklinik dan unit sama) mulai
**besok** sampai 30 hari ke depan yang kuotanya belum penuh, urut tanggal lalu jam, ditambah jadwal kunjungan
saat ini (bertanda *jadwal saat ini*). `GET /outpatient-encounters/{id}/schedule-options` kini tanpa parameter
dan mengirim `value` = `yyyy-MM-dd|doctorScheduleId`, `visitDate`, `doctorScheduleId`, `label`
(mis. `Jum, 16 Okt 2026 · Sore · 13:00–21:00`), kuota. Simpan tetap lewat `PATCH registration-data`
(`visitDate` + `doctorScheduleId`), aturan server bagian 7.2 tidak berubah.

Bukti: build 0 error, QBE Strict PASS, uji HTTP T1–T7 PASS, uji layar U1–U3 PASS (pilih slot 16 Okt di form →
detail 16 Okt; data uji dikembalikan ke 19 Okt).

