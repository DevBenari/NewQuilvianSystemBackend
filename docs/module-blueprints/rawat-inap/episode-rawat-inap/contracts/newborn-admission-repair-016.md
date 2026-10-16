# Kontrak admisi bayi — RWI-BAYI-016-1.0

Status: disetujui melalui persetujuan pengguna atas plan repair 016 pada 14 Oktober 2026. Target: admisi pasien baru dan pasien lama. Implementasi berurutan di backend MHamzah (origin/MHamzah) dan frontend HamzahV2 (origin/HamzahV2), mengikuti working tree yang dipertahankan. Instruksi melaksanakan rencana memberi kewenangan perubahan terkait pada kedua repository; bukan publikasi Git atau penerapan database.

## Proses dan invariant

Kategori bayi membuka sembilan tahap pada plan repair bagian 3. Pasien baru menyimpan master bayi dan hubungan ibu; pasien lama memakai RM dan hubungan yang ada. Pasien/kunjungan/episode bayi tetap tersendiri. DPJP wajib sebelum episode Draft dibuat. Tempat dipesan sebelum persetujuan, lalu penempatan hanya diaktifkan setelah persetujuan sah. Retry memakai operasi/versi bed existing.

Ibu tidak diketahui tidak dibuat fiktif; jalur tersebut tetap melalui prosedur identitas sementara yang belum tersedia pada kontrak master pasien. Tidak menetapkan batas umur atau minimum deposit baru. Rencana deposit tidak membuat settlement/penerimaan uang. Preferensi gabungan mencatat keinginan dua invoice sumber; pembayaran lintas invoice merupakan handoff Billing, bukan memindahkan item ke invoice ibu.

## Health Services / Inpatient Management / Inpatient Episode

Base: api/v1/health-services/inpatient-management/episodes.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| POST | / | Buat Draft bayi dengan DPJP dan konteksnya | InpatientEpisode : Create | OpenAdmissionRequest + newbornAdmission opsional | ApiResponse<InpatientEpisodeDetailResponse> |
| PUT | /{id} | Koreksi Draft dengan konteks bayi | InpatientEpisode : Update | UpdateAdmissionRequest + newbornAdmission opsional | ApiResponse<InpatientEpisodeDetailResponse> |
| GET | /{id} | Pulihkan data bayi tersimpan | InpatientEpisode : Read | id | Detail termasuk newbornAdmission |

newbornAdmission bersifat opsional untuk kompatibilitas. Jika ada, pasien master wajib IsNewborn, ibu diketahui, source adalah HospitalBirth/External/Readmission, careMode adalah RoomingIn/Neonatal, careSource wajib teks maksimal 200 karakter, requiresIntensiveCare boolean, familyBillingPreference Separate/Combined, plannedDepositAmount nullable desimal >=0. Metadata adalah fakta domain episode, bukan nomor langkah atau state visual. Model InpEpisode menyimpan field tersebut; EF configuration dan migration menyertai, database tidak diterapkan otomatis.

RoomingIn mensyaratkan episode ibu yang cocok dengan MotherPatientId bayi dan penempatan ibu yang aktif. Hubungan historis yang sama boleh tetap dipertahankan setelah ibu Closed, tetapi pemesanan rawat gabung baru tetap memerlukan ibu hadir. Ibu berbeda, diri sendiri, atau referensi yang dibatalkan ditolak. HTTP 400 untuk format; 422 untuk invariant; 409 untuk konflik status/versi.

## Health Services / Inpatient Management / Bed Occupancy

Base: api/v1/health-services/inpatient-management/bed-occupancies.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | /available-beds | Kelayakan sebelum/sesudah episode | InpatientBedOccupancy : Read | Query existing + patientId, newbornCareMode, motherEpisodeId, requiresIntensiveCare, requiresIsolation | ApiResponse<AvailableBedPagedResult> |
| POST | /reservations | Reservasi | InpatientBedOccupancy : Create | DTO dan versi existing | Reservasi |
| POST | /placements | Aktifkan penempatan | InpatientBedOccupancy : Create | PlacePatientRequest dan versi existing | Penempatan |

Server menurunkan kategori dari master pasien. Pasien bayi tidak boleh bed dewasa; dewasa tidak boleh boks bayi. RoomingIn hanya boks kamar penempatan ibu saat ini. Kebutuhan intensif memerlukan flag intensif pada bed/kamar; isolasi memakai aturan existing. Pembatasan kamar ibu/intensif dari rencana admisi diterapkan pada Draft (termasuk simulasi pencarian sebelum episode); setelah penempatan aktif, perubahan klinis mengikuti workflow transfer existing. Kategori boks bayi tetap ditegakkan pada semua status. Nama unit/kamar tidak menjadi bukti kemampuan. Query episode memakai metadata server, bukan override dari browser.

## Keputusan komponen UI

| Elemen | Komponen dan bukti source | Status | Pilihan |
| --- | --- | --- | --- |
| Stepper/header/tombol | InpatientAdmissionStepper, Hero, BaseButton | REUSE | Props existing |
| Pencarian ibu | ResourceFilterSelect dengan selectedOption, serverSide, loadMore | REUSE | Query existing pasien/episode, abort dan urutan respons |
| Field bayi | BaseInputField, BaseDateField, BaseTimeField, BaseSelectField | REUSE | Props existing; field controlled |
| Konteks/identitas/perawatan | Rangkaian field, BaseCheckboxCard dan InformationAlert dalam view domain | COMPOSE | A direkomendasikan: komposisi base tanpa mengubah default, konsisten dan regresi terbatas; B: menyalin form umum, biaya lebih besar dan berisiko menyimpang |
| Penempatan | InpatientAdmissionBedSelectionStep/BookingStep | COMPOSE | A direkomendasikan: rangkai dua keadaan dalam satu tahap, reuse transaksi; B: mempertahankan dua langkah, tidak memenuhi sembilan tahap |
| Persetujuan/dokumen | ConsentFormStep dan print flow PPRI | REUSE | Wali bayi, versi dokumen dan identitas server |

UI GATE: REUSE 5 kelompok, COMPOSE 2, NEW base component 0. Komposisi domain mengikuti skema yang sudah disetujui; tidak ada perubahan perilaku default base.
