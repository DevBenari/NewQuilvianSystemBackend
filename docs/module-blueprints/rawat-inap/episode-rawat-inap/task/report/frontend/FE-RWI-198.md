# Laporan Task FE-RWI-198: Permintaan Admisi dari Kamar Pulih

| Metadata | Nilai |
|---|---|
| **Task ID** | `FE-RWI-198` |
| **Layar Blueprint** | `FE-INP-29` (Permintaan Admisi dari Kamar Pulih pada Layar Admisi `FE-INP-03`) |
| **Status** | Selesai (Completed) |
| **Tanggal Selesai** | 5 Oktober 2026 |
| **Pelaksana** | Muhammad Hamzah (Agentic Pair Programming) |
| **Requirement Terkait** | `FR-RWF-080`, `FR-RWF-089`, `RWI-DEC-201`, `RWI-DEC-220` butir 1, `AC-RWF-080`, `088`, `089`, `UAT-RWF-16`, `33`, `41` |
| **Dependency** | `BE-RWI-181` |

---

## 1. Ringkasan Pekerjaan
Mengintegrasikan daftar pasien dari Kamar Pulih (Recovery Room IBS) yang membutuhkan perawatan rawat inap langsung pada layar muka Admisi Rawat Inap (`FE-INP-03`). Petugas admisi dapat melihat antrean permintaan rujukan admisi berstatus `Pending` beserta lamanya menunggu (dengan indikator peringatan merah berkedip saat melewati ambang batas waktu). Menekan tombol "Admisi" akan mengarahkan alur ke pendaftaran pasien lama dengan nomor rekam medis dan data rujukan terisi awal, sementara penetapan penjamin, kelas perawatan, DPJP, deposit, dan pemesanan bed tetap dilakukan oleh petugas admisi sesuai SOP.

---

## 2. Berkas yang Dibuat dan Diubah

1. **`src/lib/services/health-services/inpatient-management/inpatient-admission-referral.service.js`**
   - Menambahkan pemanggilan API `GET /v1/health-services/inpatient-management/admission-referrals` dan detail `GET /{id}`.
2. **`src/components/view/health-services/inpatient-management/admission-referrals/recovery-admission-referrals-list.jsx`**
   - Komponen tabel daftar rujukan dari kamar pulih dengan kolom: Pasien, Kunjungan Asal, Operasi, Dokter Bedah, Tujuan Perawatan, Lamanya Menunggu, dan Tombol Admisi.
   - Keadaan kosong: *"Tidak ada pasien dari kamar pulih yang menunggu admisi."*
   - Dilindungi hak akses `InpatientAdmissionReferral : Read` dan tombol Admisi dilindungi `InpatientEpisode : Create`.
3. **`src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx`**
   - Menambahkan field `admissionReferralId` pada `buildInpatientAdmissionEpisodePayload`.
4. **`src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx`**
   - Menerima `admissionReferralId` dan meneruskannya ke pembuatan episode.
   - Menangani pesan penolakan server 409 `INP-ADM-REF-001` (*"Pasien memiliki permintaan admisi dari kamar pulih; buka admisi dari permintaan itu"*) dan 422 `INP-ADM-REF-002` (*"Permintaan admisi ini sudah selesai atau dibatalkan"*).
5. **`src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`**
   - Merender `RecoveryAdmissionReferralsList` di atas pemilihan tipe pendaftaran dan menangani callback `handleSelectReferral`.
6. **`tests/unit/recovery-admission-referral.test.mjs`**
   - Unit test otomatis memvalidasi service, komponen list, payload episode, dan kode penolakan.

---

---

## 4. Pembaruan 6 Oktober 2026: Refinement 3-Card Admission Grid & Stepper Kamar Pulih (`RWI-UI-SPEC-029`)

Berdasarkan tinjauan alur kerja operasional, tata letak daftar rujukan yang awalnya berada di atas form kartu kini ditingkatkan menjadi **3 Kartu Pilihan Tipe Pendaftaran Berdampingan** (*3-Card Admission Grid*):
1. **Pendaftaran Pasien Baru**
2. **Pendaftaran Pasien Lama**
3. **Admisi dari Kamar Pulih (Recovery Room)**

### Perubahan Berkas Tambahan:
1. **`src/components/view/health-services/registration-management/emergency-registration/patient-entry-choice-step.jsx` & CSS Module**
   - Menambahkan kartu ke-3 dengan ilustrasi 3D khusus (`RecoveryPatientIllustration`), tema warna indigo/purple medis, dan dukungan alert kedaruratan (overdue).
2. **`src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`**
   - Menambahkan mode `INPATIENT_ADMISSION_ENTRY_MODE.RECOVERY` dan daftar 10 langkah stepper `INPATIENT_ADMISSION_RECOVERY_STEPS`.
3. **`src/components/view/health-services/inpatient-management/admission-referrals/inpatient-admission-recovery-referral-step.jsx`**
   - Komponen Langkah 1 Stepper Kamar Pulih: Pencarian cepat, filter status (Semua, ICU, Rawat Inap, Overdue), daftar rujukan kartu interaktif, dan panel pratinjau resume tindakan bedah & instruksi anestesi.
4. **`src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`**
   - Menghapus banner tabel di atas pilihan kartu, menyajikan grid 3 kartu terpadu dengan realtime badge count, dan menghubungkan langkah stepper kamar pulih.

### Bukti Verifikasi Unit Test Terbaru:
```bash
cmd.exe /c node --import ./tests/helpers/register.mjs --test tests/unit/recovery-admission-referral.test.mjs
```
Hasil:
- ✔ FE-RWI-198 AC-1: Service memuat endpoint admission-referrals untuk daftar dan detail
- ✔ FE-RWI-198 AC-2: Komponen RecoveryAdmissionReferralsList memenuhi teks keadaan kosong dan proteksi izin
- ✔ FE-RWI-198 AC-3: Payload episode dan hook dokter mendukung admissionReferralId dan penanganan error 409/422
- ✔ RWI-UI-SPEC-029 AC-4: 3-Card admission grid dan 10 langkah stepper kamar pulih
- ✔ RWI-UI-SPEC-029 AC-5: InpatientAdmissionRecoveryReferralStep memuat pencarian, filter, dan detail pratinjau
- ℹ tests 5, pass 5, fail 0 (PASS)

