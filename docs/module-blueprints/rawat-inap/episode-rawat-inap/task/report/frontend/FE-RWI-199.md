# Laporan Implementasi Task FE-RWI-199: Dua Daftar Pantau Tertunda & Pengaturan Ambang Batas

## Metadata Task
- **Task ID**: `FE-RWI-199`
- **Modul**: Rawat Inap (`inpatient-management`) & Master Data Pengaturan (`inpatient-settings`)
- **Status**: Selesai
- **Requirement / Keputusan**: `FR-RWF-088`, `RWI-DEC-201`, `RWI-DEC-216`, `RWI-DEC-220` butir 6
- **Kontrak**: Frontend 13.1 (`FE-INP-12`), 13.4.6, 13.7 (`FE-INP-30`), API 11.9
- **Pemilik**: Muhammad Hamzah

---

## 1. Ringkasan Perubahan
1. **Pengaturan Rawat Inap (`inpatient-settings`)**:
   - Menambahkan dua field ambang batas baru ke `INPATIENT_SETTING_LIMITS`, `INPATIENT_SETTING_FORM_DEFAULTS`, dan `INPATIENT_SETTING_FORM_FIELDS`:
     - `pendingSurgicalHandoverAlertMinutes` (Ambang Serah Terima Pasca Operasi Tertunda, min 1, max 1440 menit, default 60 menit).
     - `pendingAdmissionReferralAlertMinutes` (Ambang Permintaan Admisi Tertunda, min 1, max 1440 menit, default 30 menit).
   - Memperbarui helper utility di `inpatient-setting-utils.jsx` (`mapInpatientSettingToForm`, `buildInpatientSettingPayload`, `validateInpatientSettingForm` di `RANGE_FIELDS`, dan `isSettingFormChanged`). Nilai di luar 1–1440 menit ditolak.
2. **Dua Daftar Pantau Baru di Layar Monitoring (`inpatient-monitoring`)**:
   - Menambahkan dua kunci list baru ke `INPATIENT_MONITORING_LIST_KEYS` dan konfigurasi ke `INPATIENT_MONITORING_LISTS`:
     - `PENDING_SURGICAL_HANDOVER` (`pending-surgical-handovers`): "Serah Terima Pasca Operasi Tertunda". Teks kosong: "Tidak ada serah terima pasca operasi yang tertunda."
     - `PENDING_ADMISSION_REFERRAL` (`pending-admission-referrals`): "Permintaan Admisi Tertunda". Teks kosong: "Tidak ada permintaan admisi yang tertunda."
   - Menempatkan kedua tab daftar pantau tersebut di akhir urutan kelompok episode sesuai keputusan `RWI-DEC-216`.
3. **Normalisasi & Kolom Tampilan**:
   - Di `inpatient-monitoring-utils.jsx`, menambahkan fungsi `normalizePendingSurgicalHandoverItem`, `normalizePendingSurgicalHandoverList`, `normalizePendingAdmissionReferralItem`, dan `normalizePendingAdmissionReferralList`.
   - Di `inpatient-monitoring-table-columns.jsx`:
     - Kolom `buildPendingSurgicalHandoverColumns`: menampilkan Pasien, Nomor Kasus OK, Unit Tujuan & Unit Sekarang, Pengirim & Waktu, Lama Menunggu, serta tanda badge tegas `Pasien belum di unit tujuan` (`data-testid="monitoring-not-in-destination-unit"`) jika `patientInDestinationUnit` bernilai false.
     - Kolom `buildPendingAdmissionReferralColumns`: menampilkan Pasien, Nomor Permintaan & Kasus, Usulan Ruangan & DPJP, Perujuk & Waktu, Lama Menunggu, serta aksi tombol `Buka Admisi` yang mengarahkan langsung ke form pendaftaran rawat inap.
   - Di `use-inpatient-monitoring.jsx`, mendaftarkan normalizer dan pesan fallback error untuk kedua endpoint.

---

## 2. Bukti Verifikasi
- **Unit Test**: `tests/unit/inpatient-monitoring-settings.test.mjs`
  - Hasil: `3 passed, 0 failed` (100% lulus).
  - Menguji pemenuhan urutan tab RWI-DEC-216, validasi batas ambang menit 1-1440, teks keadaan kosong, dan deteksi badge status penempatan pasien.
