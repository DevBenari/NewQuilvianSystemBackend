# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-020` |
| Judul | Rujukan belum lengkap di Daftar Kunjungan RJ |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `19` |
| Trace | `RJ-DOC-DEC-077`, `078`; `03` *PM-FE.3*, *PM-FE.6*; FR-PM-11 |
| Contract version | `RJ-DOC-REFERRAL-001@1.0.0` (**approved**) |
| Wewenang UI | Bentuk form = modal, sama dengan aksi baris *Batalkan Kunjungan* pada layar ini (`DEV_DISCRETION` PM-FE.3) |
| Dependency | `RJ-DOC-REV-BE-019` ✅ |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-DOC-DEC-083`) |
| Commit frontend | `de323430` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Proses dari sisi pengguna

1. **Daftar Pasien Rawat Jalan** menampilkan badge kuning **"Rujukan belum lengkap"** di bawah status kunjungan bila `referralStatus = Incomplete`. Pelayanan tidak diblokir.
2. Filter baru **Status Rujukan** berisi pilihan Semua, *Rujukan Lengkap*, dan *Rujukan Belum Lengkap* (query `referralStatus`).
3. Menu aksi baris menampilkan satu dari tiga aksi untuk kunjungan rujukan, bila sesi memegang `PatientEncounter : Update`:

   | Keadaan | Aksi |
   | --- | --- |
   | Rujukan belum lengkap | **Lengkapi Rujukan** |
   | Rujukan lengkap | **Koreksi Rujukan** |
   | Status ≥ Dalam Konsultasi atau batal | **Lihat Rujukan** |

   Menu Batalkan Kunjungan tetap seperti sebelumnya.
4. Aksi membuka modal dengan wilayah A, B, dan C, memakai isian yang sama dengan step Data Rujukan (`FE-019`):
   - **Unit Tujuan baca saja.** Untuk rujukan tanpa rincian, yang tampil adalah poliklinik kunjungan.
   - **Isian belum lengkap** tampil sebagai daftar dari `missingFields` backend, misalnya "Lengkapi: diagnosa, alasan rujukan, surat rujukan."
   - **Tanggal/jam kosong** pada rujukan lama atau Kiosk diisi waktu sekarang dan tetap dapat diubah.
   - **Surat:** *Unggah Surat* dan *Hapus* langsung tersimpan, lalu rincian dimuat ulang. *Lihat* membuka surat di tab baru lewat endpoint berizin; URL blob sementara dilepas sesudah 60 detik dan tidak disimpan.
   - **Simpan Rujukan** mengirim `PUT …/referral` dengan `expectedRowVersion`. Hasilnya ditampilkan sebagai toast "Rujukan lengkap" atau "Rujukan tersimpan, tetapi belum lengkap". Daftar dimuat ulang saat modal ditutup.
5. **Data basi** (`409 RJ-VAL-PM-11`): alert "Data rujukan sudah diubah pengguna lain" dengan tombol **Muat ulang**. Tombol Simpan dikunci sampai rincian dimuat ulang.
6. **Terkunci** (`isLocked`): alert "Konsultasi dokter sudah dimulai". Semua isian nonaktif, tanpa tombol Simpan, Unggah, atau Hapus; surat tetap dapat dilihat. `409 RJ-VAL-PM-09` saat menyimpan juga memuat ulang form ke keadaan terkunci.

## 2. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/…/outpatient-encounter-constants.js` | `referralStatus` di filter bawaan, `OUTPATIENT_REFERRAL_STATUS(_OPTIONS)`, `PATIENT_ENCOUNTER_RESOURCE`, label `missingFields` |
| `src/utils/…/outpatient-encounter-display-utils.js` | Parameter `referralStatus` |
| `src/utils/…/outpatient-referral.utils.js` | `mapEncounterReferralToFormValues`, `buildReferralCorrectionPayload` (tanpa unit tujuan), `validateReferralCorrection`, deteksi `RJ-VAL-PM-09/11` |
| `src/lib/services/…/outpatient-registration.service.js` | `getOutpatientEncounterReferral`, `deleteOutpatientReferralDocument`, `fetchOutpatientReferralDocumentBlob`; galat membawa `status` HTTP |
| `src/lib/hooks/…/outpatient-encounters/use-outpatient-encounter-referral.jsx` | **Baru** — controller modal (izin, muat, simpan, unggah, hapus, lihat, data basi, terkunci) |
| `src/components/view/…/outpatient-encounters/outpatient-encounter-referral-modal.jsx` | **Baru** — modal Lengkapi/Koreksi/Lihat Rujukan |
| `src/components/view/…/outpatient-encounters/outpatient-encounter-table-columns.jsx` | Badge "Rujukan belum lengkap", aksi rujukan, kolom Aksi tampil bila izin batal **atau** ubah rujukan |
| `src/components/view/…/outpatient-encounters/outpatient-encounter-list-view.jsx` | Filter Status Rujukan, modal, toast digabung |
| `src/components/view/…/outpatient-registration/outpatient-referral-step.jsx` | `ReferralDateTimeField` diekspor dengan prop `disabled` |
| `src/components/view/…/outpatient-registration/outpatient-practice-schedule-modal.jsx` | Dialog dilebarkan (`wideModal`) supaya tabel lima kolom terbaca |
| `src/style/…/outpatient-registration.module.css` | `referralModalBody`, `referralReadOnlyValue`, `referralFileActions`, `wideModal` (specificity ganda, tanpa `!important`, mengikuti `lab-order-print.module.css`) |

**UI GATE: 9 elemen — REUSE 8, EXTEND 1, COMPOSE 0, WRAP 0, NEW 0**

| Kebutuhan UI | Base component | Status |
| --- | --- | --- |
| Badge | `StatusBadge` `warning` | REUSE |
| Filter | `FilterSelect` di `DataFilter` | REUSE |
| Aksi baris | `RowActionMenu` | REUSE |
| Modal form | `ConfirmModal` (`children`, `hideCancel`, `disabled`, `loading`) | REUSE; lebar lewat `modalClassName` (pola yang sudah ada) |
| Isian A/B/C | `EmergencyTextField`, `EmergencySelectField` + `useSelectResource` | REUSE |
| Tanggal + jam | `ReferralDateTimeField` dari `FE-019` | EXTEND — prop opsional `disabled` |
| Alert | `EmergencyInlineAlert` | REUSE |
| Tombol | `BaseButton` | REUSE |
| Toast | `ToastStack` | REUSE |

## 3. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/registration-management/outpatient-encounters?referralStatus=` | Badge + filter | `OutpatientEncounter : Read` |
| `GET` | `/v1/…/patient-encounters/{id}/referral` | Isi form | `PatientEncounter : Read` |
| `PUT` | `/v1/…/patient-encounters/{id}/referral` | Lengkapi/koreksi (`expectedRowVersion`) | `PatientEncounter : Update` |
| `POST` | `/v1/…/patient-encounters/{id}/referral/documents` | Unggah surat | `PatientEncounter : Update` |
| `DELETE` | `/v1/…/patient-encounters/{id}/referral/documents/{docId}` | Hapus surat | `PatientEncounter : Update` |
| `GET` | `/v1/…/patient-encounters/{id}/referral/documents/{docId}/content` | Lihat surat | `PatientEncounter : Read` |

## 4. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` berkas yang berubah | exit 0 | `PASS` |
| `npm run build` | exit 0 | `PASS` |
| Uji layar Playwright `FE-020` (build 3100 → backend uji 7185) | **8/8 PASS** | `PASS` |
| Regresi uji layar `FE-019` sesudah modal dilebarkan | **17/17 PASS** | `PASS` |

Data uji dibuat lewat `POST /patient-encounters/admin` (skrip `fe020_setup.py`):

- **A**: rujukan tanpa rincian.
- **B**: rujukan dengan rincian. Statusnya dipaksa `6` lewat SQL ke `QuilvianNewDevSukma` hanya untuk uji keadaan terkunci, karena tidak ada jalur layar yang singkat untuk memulai konsultasi.

| ID | Skenario | Hasil |
| --- | --- | --- |
| L1 | `RJ-AC-PM-09`: kunjungan A bertanda "Rujukan belum lengkap" | PASS |
| L2 | Filter *Rujukan Belum Lengkap* → request `referralStatus=Incomplete`, semua baris bertanda | PASS |
| L3 | *Lengkapi Rujukan* → modal; unit tujuan baca saja "Poli Penyakit Dalam"; daftar isian belum lengkap; tanpa pilihan unit tujuan | PASS |
| L4 | `RJ-VAL-PM-11`: rincian diubah dari luar layar → Simpan menampilkan pesan + *Muat ulang*; muat ulang memuat nilai terbaru | PASS |
| L6 | *Lihat* surat → `GET …/content` `200`, `application/pdf`, `no-store`; dibuka lewat URL blob sementara di tab baru | PASS |
| L5 | Diagnosa + alasan + surat dilengkapi → kunjungan hilang dari filter *Belum Lengkap*, badge hilang, aksi menjadi *Koreksi Rujukan* | PASS |
| L8 | Koreksi alasan tersimpan dan rujukan tetap lengkap | PASS |
| L7 | Status ≥ 6 → *Lihat Rujukan*, alert "Konsultasi dokter sudah dimulai", isian nonaktif, tanpa Simpan/Unggah | PASS |

Semua kunjungan uji dibatalkan (`PATCH …/admin/{id}/cancel`, `200`). Pasangan terakhir: `ENC-RSMMC-00260`, `00261`.

`AUTOMATED TEST: SKIPPED (opsional) — tidak ada test baru di repository.`

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `RJ-AC-PM-09` (tampilan) | Terpenuhi | L1, L2 |
| 2. Melengkapi diagnosa + alasan menghapus badge | Terpenuhi (bersama surat, karena surat termasuk isian wajib `RJ-DOC-DEC-075`) | L5 |
| 3. Status ≥ 6 → form baca saja | Terpenuhi | L7 |
| 4. `RJ-VAL-PM-11` → pesan muat ulang | Terpenuhi | L4 |
| 5. Tanpa `PatientEncounter : Update` → aksi tidak tampil | Terpenuhi lewat source: aksi rujukan dan kolom Aksi bergantung pada `usePermission("PatientEncounter", "Update")`. Tidak diuji dengan akun tanpa izin | Source |
| 6. Lint, build | Terpenuhi | Bagian 4 |

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Kunjungan Laboratorium tidak tampil di Daftar RJ (delta `BE-018`), sehingga rujukan Lab hanya dapat dilengkapi lewat layar Selesai pendaftaran |
| Dependency backend | Tidak ada yang terbuka |
| Perubahan sampingan | Galat service pendaftaran RJ kini membawa `status` HTTP; pesan galat tidak berubah |

## 7. Revisi 9 Oktober 2026

Modal Lengkapi/Koreksi Rujukan memakai field dokter perujuk bersama (`referral-doctor-select-field.jsx`), sehingga dokter dapat diketik manual atau dipilih. Tata letak wilayah B disamakan dengan step Data Rujukan: Unit Tujuan (baca saja) + Diagnosa ICD-10 sebaris, Catatan Diagnosa satu baris penuh. Regresi uji layar `FE-020` **8/8 PASS**. Rincian di [RJ-DOC-REV-FE-019](RJ-DOC-REV-FE-019.md) bagian 9.
