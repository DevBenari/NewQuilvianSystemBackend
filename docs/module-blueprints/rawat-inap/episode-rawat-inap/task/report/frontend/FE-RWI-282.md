# FE-RWI-282 — Pencabutan & Penghapusan Penuh Kartu serta Jalur Masuk Kamar Pulih dari Antarmuka Admisi Rawat Inap

- **TASK ID:** `FE-RWI-282`
- **TASK TYPE:** Implementasi / Pembersihan Frontend Admisi Rawat Inap
- **COMPLEXITY:** `LOW`
- **CLASSIFICATION SCORE:** 3 — satu repository frontend `QuilvianSystemFrontendDev`; pembersihan konstanta opsi entri dan penambahan unit test verifikasi
- **MODEL:** Google Antigravity (Gemini 3.8 Flash High)
- **TASK MODE:** `FRONTEND`
- **WRITE TARGET:** `QuilvianSystemFrontendDev` (Branch `HamzahV2`) & pelaporan di `NewQuilvianSystemBackend` (Branch `MHamzah`)
- **TRACE:** `RWI-DEC-359`, `OPS-DEC-040`, `DSN-OPR-RWI-20261023`
- **KONTRAK:** `0.19.0`
- **FILES INSPECTED:**
  - `QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`
  - `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`
  - `QuilvianSystemFrontendDev/tests/unit/inpatient-admission-transfer.test.mjs`
  - `QuilvianSystemFrontendDev/tests/unit/recovery-admission-referral.test.mjs`
  - `QuilvianSystemFrontendDev/tests/unit/inpatient-admission-subrouting.test.mjs`
- **FILES CHANGED:**
  - `QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` (Pencabutan objek kartu `RECOVERY` dari `INPATIENT_ADMISSION_ENTRY_OPTIONS`)
  - `QuilvianSystemFrontendDev/tests/unit/inpatient-admission-remove-recovery.test.mjs` (Unit test baru 4/4 PASS)

---

## 1. Latar Belakang & Ketetapan Bisnis

Berdasarkan arahan langsung pemilik domain (Muhammad Hamzah) pada 24 Oktober 2026 yang dikunci dalam **Decision Log Revision 66 (`RWI-DEC-359`)**:
1. Standar rumah sakit di Indonesia mewajibkan setiap pasien yang akan menjalani operasi terencana (elektif maupun *Sectio Caesarea*) **harus masuk rawat inap terlebih dahulu** (*Pre-Op Admission*) dan menempati tempat tidur bangsal sebelum pembedahan.
2. Tempat tidur di bangsal ditahan (*Bed Hold*) dengan status `Occupied` selama pasien di kamar operasi dan kamar pulih (PACU). Pasca-bedah, pasien **langsung kembali ke tempat tidur semula** di bangsal melalui serah terima internal (*internal clinical handover*) antar-perawat tanpa registrasi ulang.
3. Alur *"Admisi dari Kamar Pulih"* (`admissions/recovery`) di loket pendaftaran rawat inap depan **ditiadakan dan dihapus total** dari antarmuka beranda admisi rawat inap.
4. Bila pasien rawat jalan / *One Day Care* (ODC) mengalami komplikasi tak terduga di PACU dan dokter anestesi memutuskan rawat inap, penanganannya dilakukan via mekanisme **Transfer Pasien Antar Unit**, bukan menyuruh keluarga mendaftar di loket depan.

---

## 2. Rincian Implementasi

1. **Pembersihan Kartu Entri Admisi (`inpatient-admission-flow-constants.jsx`):**
   * Menghapus blok objek `INPATIENT_ADMISSION_ENTRY_MODE.RECOVERY` dari array `INPATIENT_ADMISSION_ENTRY_OPTIONS`.
   * Mengembalikan jumlah kartu pintu masuk beranda admisi rawat inap (`/health-services/inpatient-management/admissions`) menjadi tepat **4 kartu resmi**:
     1. `NEW`: Pendaftaran Pasien Baru
     2. `EXISTING`: Pendaftaran Pasien Lama (mencakup rujukan operasi terencana & poliklinik dengan SPRI)
     3. `NEWBORN`: Admisi Bayi Baru Lahir
     4. `TRANSFER`: Admisi Transfer IGD & Poliklinik
2. **Perbaikan Regresi Test Lama:**
   * Menghilangkan kegagalan uji `5 !== 4` pada `tests/unit/inpatient-admission-transfer.test.mjs` sehingga kembali berstatus 100% HIJAU (PASS).
3. **Penyusunan Uji Khusus Task:**
   * Membuat file uji `tests/unit/inpatient-admission-remove-recovery.test.mjs` untuk mengunci kepatuhan terhadap `RWI-DEC-359` (4 kartu resmi, peniadaan kartu kamar pulih, dan urutan kartu).

---

## 3. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Hasil | Bukti Verifikasi |
|---|---|:---:|---|
| 1 | Kartu "Admisi dari Kamar Pulih" dihapus dari `INPATIENT_ADMISSION_ENTRY_OPTIONS` | ✅ LULUS | `tests/unit/inpatient-admission-remove-recovery.test.mjs` AC-2: `recoveryOption === undefined` |
| 2 | Beranda admisi rawat inap menampilkan tepat 4 kartu pintu masuk resmi | ✅ LULUS | `tests/unit/inpatient-admission-remove-recovery.test.mjs` AC-1: `length === 4`; `inpatient-admission-transfer.test.mjs`: `length === 4` PASS |
| 3 | Empat pintu masuk resmi tersusun urut: Baru, Lama, Bayi, dan Transfer | ✅ LULUS | `tests/unit/inpatient-admission-remove-recovery.test.mjs` AC-3 PASS |
| 4 | Seluruh unit test terkait opsi entri admisi rawat inap diperbarui dan lulus 100% | ✅ LULUS | 4/4 uji `inpatient-admission-remove-recovery.test.mjs` PASS; 5/5 uji `inpatient-admission-transfer.test.mjs` PASS; 7/7 uji `recovery-admission-referral.test.mjs` PASS |

---

## 4. Hasil Uji Otomatis (Automated Tests)

* `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-remove-recovery.test.mjs`
  * **Status:** `PASS` (4/4 lulus dalam 9.6ms)
* `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-transfer.test.mjs`
  * **Status:** `PASS` (5/5 lulus dalam 30.2ms)
* `node --import ./tests/helpers/register.mjs --test tests/unit/recovery-admission-referral.test.mjs`
  * **Status:** `PASS` (7/7 lulus dalam 306ms)
* `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-subrouting.test.mjs`
  * **Status:** `PASS` (3/3 lulus dalam 380ms)
* `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-eligibility.test.mjs`
  * **Status:** `PASS` (8/8 lulus dalam 1024ms)
