# ISSUE-029 — Kontaminasi State dan Session Storage Antarmode Admisi yang Menyebabkan Alur Pasien Baru dan Lama Bermutasi Menjadi Alur Bayi

```yaml
issue_id: ISSUE-EPS-029
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Layar Utama Pilihan Tipe Pendaftaran dan Alur Pasien Lama / Pasien Baru (FE-INP-01 / FE-INP-02)"
sumber_laporan: "Laporan pengguna 17 Oktober 2026: setelah membuka alur bayi, saat membuka Pendaftaran Pasien Lama tiba-tiba langsung masuk ke Langkah 3 Bayi (Perawatan dan DPJP Bayi) tanpa pernah input nomor RM, dan session storage menyimpan patientType: newborn di mode unknown dan existing"
tanggal_issue: "2026-10-17"
status: TERATASI
keparahan_tertinggi: High
source_sha_backend: "d420c2fbe094a2d70776c44955233bce9c87bda8 (MHamzah)"
source_sha_frontend: "0accf387138a3fcc1ff40fbb5bdeb960076b4d59 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-029-kontaminasi-state-dan-session-storage-antar-mode-admisi.md
```

---

## 1. Ringkasan

Laporan ini menindaklanjuti temuan regresi kritis pada layar Admisi Rawat Inap setelah pemisahan kartu admisi bayi baru lahir. Ketika pengguna membuka kartu pendaftaran bayi baru lahir lalu kembali ke menu utama dan membuka **Pendaftaran Pasien Lama** (`entry=existing`) atau **Pendaftaran Pasien Baru** (`entry=new`), sistem secara tidak terduga langsung mengarahkan pengguna ke **Langkah 3 dari 7 Bayi: Perawatan dan DPJP Bayi**, melewati tahapan pencarian nomor rekam medis dan data pasien lama sama sekali.

Pemeriksaan pada *Application Storage* peramban membuktikan adanya kebocoran kunci sesi:
1. `quilvian:inpatient-admission:flow:unknown:draft` menyimpan `{"patientType":"newborn", ...}` saat berada di menu utama.
2. `quilvian:inpatient-admission:flow:existing:existing` menyimpan `{"patientType":"newborn", ...}` saat berpindah ke mode Pasien Lama.

Akar masalah berasal dari:
1. State React `patientType` di hook `useInpatientAdmissionFlow` tidak di-reset saat URL berpindah dari `newborn` ke mode non-bayi (`existing` atau `new`).
2. Efek persistensi sesi (`saveFlowDraft`) otomatis menuliskan `patientType: "newborn"` yang tertahan di memory ke dalam kunci sesi mode baru.
3. Fungsi `resolveInpatientAdmissionSteps` masih mempertahankan klausul mutasi dinamis warisan: `(isNewborn && [NEW, EXISTING].includes(entryMode))`, sehingga saat `patientType === "newborn"`, alur Pasien Lama dan Pasien Baru otomatis dipaksa menjadi 7 langkah bayi.
4. Hook `useInpatientNewbornAdmission` masih mengizinkan `enabled` pada mode `existing` dan `new`, sehingga komponen form dokter berubah menjadi form DPJP Bayi (`<NewbornCareStep>`).
5. Mekanisme *fallback slug* URL mendeteksi `step=existing-patient` tidak ada pada 7 langkah bayi, lalu mencari langkah bersama pertama di daftar bayi, yaitu `doctor` (Langkah 3), sehingga pengguna terlempar ke Langkah 3 Bayi.

---

## 2. Laporan Asli Pelapor

| No. Laporan | Ringkasan Keluhan Pelapor | Lampiran Bukti |
| :---: | :--- | :--- |
| **1** | Saat membuka admisi rawat inap pendaftaran bayi memang berhasil terbuka normal di Langkah 1. | Screenshot 1: Layar `Langkah 1 dari 7 — Pilih Ibu Persalinan`. |
| **2** | Tetapi saat membuka pendaftaran pasien lama, yang terjadi adalah tiba-tiba masuk ke **Step ke-3 Bayi (Perawatan dan DPJP Bayi)** dengan informasi tidak jelas, padahal belum input nomor RM sama sekali. | Screenshot 2: Layar `Langkah 3 dari 7 — Perawatan dan DPJP Bayi` pada alur pasien lama. |
| **3** | Session Storage menangkap adanya dua kunci yang tercemar data bayi: `quilvian:inpatient-admission:flow:unknown:draft` dan `quilvian:inpatient-admission:flow:existing:existing`. Masalah ini juga berdampak pada Pendaftaran Pasien Baru. | Capture JSON Session Storage dari DevTools pengguna. |

---

## 3. Rincian Temuan Akar Masalah

### ISS-EPS-029-01: Bertahannya State Memory `patientType` saat Berpindah Antarmode
- **Lokasi Kode**: `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx` (baris 57, 151-158).
- **Mekanisme Galat**:
  Halaman admisi rawat inap (`/health-services/inpatient-management/admissions`) adalah satu halaman Next.js tunggal (*Single Page Application*). Saat pengguna beralih dari alur bayi ke menu utama atau ke alur pasien lama, komponen tidak mengalami *unmount*, melainkan hanya nilai `searchParams` yang berubah.
  Karena hook hanya memiliki *effect* untuk mengubah `patientType` menjadi `"newborn"` saat `entryMode === "newborn"`, tetapi **tidak memiliki effect untuk membersihkan `patientType`** saat `entryMode !== "newborn"`, maka nilai `patientType` tetap tertinggal sebagai `"newborn"` di memory React.

### ISS-EPS-029-02: Efek Samping Persistensi Otomatis dan Kunci `unknown:draft` di Menu Utama
- **Lokasi Kode**: `use-inpatient-admission-flow.jsx` (baris 146-149, 177-188).
- **Mekanisme Galat**:
  - Saat pengguna berada di menu awal (`rawEntryMode === ""`), formula `storageKey` membentuk:
    `quilvian:inpatient-admission:flow:unknown:draft`.
  - Efek persistensi sesi (`saveFlowDraft`) terus berjalan karena `patientType` bernilai `"newborn"`, sehingga menu utama menyimpan draf bayi ke `unknown:draft`.
  - Saat pengguna mengklik "Pasien Lama" (`entry=existing`), `storageKey` berubah ke `flow:existing:existing`. Efek persistensi langsung menyalin `patientType: "newborn"` ke dalam kunci sesi Pasien Lama.

### ISS-EPS-029-03: Klausul Mutasi Dinamis yang Masih Tertinggal pada `resolveInpatientAdmissionSteps`
- **Lokasi Kode**: `QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` (baris 359-368).
- **Mekanisme Galat**:
  ```javascript
  if (
    entryMode === INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN ||
    (isNewborn && [INPATIENT_ADMISSION_ENTRY_MODE.NEW, INPATIENT_ADMISSION_ENTRY_MODE.EXISTING].includes(entryMode))
  ) {
    return NEWBORN_ADMISSION_STEPS;
  }
  ```
  Klausul `(isNewborn && [NEW, EXISTING].includes(entryMode))` adalah pintu belakang yang menyebabkan alur Pasien Lama (`existing`) dan Pasien Baru (`new`) dipaksa berubah menjadi 7 langkah bayi seketika `patientType === "newborn"`.

### ISS-EPS-029-04: Hook `useInpatientNewbornAdmission` Aktif di Luar Mode Bayi
- **Lokasi Kode**: `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js` (baris 44).
- **Mekanisme Galat**:
  ```javascript
  const enabled = (flow.isNewborn || flow.entryMode === "newborn") && ["new", "existing", "newborn"].includes(flow.entryMode);
  ```
  Karena `enabled` bernilai `true` saat `isNewborn` aktif di mode `existing`, komponen `AdmissionStep` di `inpatient-admission-view.jsx` mengecek `if (newborn?.enabled)` dan merender formulir `<NewbornCareStep>` saat `stepSlug === "doctor"`, menggantikan form dokter umum.

---

## 4. Matriks Rencana Perbaikan

| ID Temuan | Perbaikan yang Direncanakan | Area | Dokumen Perbaikan |
| :--- | :--- | :--- | :--- |
| `ISS-EPS-029-01` | Tambahkan auto-reset state `patientType`, `newbornDraft`, dan `motherEpisodeId` saat `entryMode !== "newborn"`. | Frontend (Hook) | `PLAN-REPAIR-029` |
| `ISS-EPS-029-02` | Hentikan persistensi sesi saat berada di menu utama (`!entryMode`), isolasi kunci sesi murni per mode aktif, dan sanitasi data yang dibaca saat rehidrasi. | Frontend (Storage) | `PLAN-REPAIR-029` |
| `ISS-EPS-029-03` | Hapus seluruh klausul mutasi dinamis pada `resolveInpatientAdmissionSteps`. Alur `new` dan `existing` tidak boleh pernah menjadi `NEWBORN_ADMISSION_STEPS`. | Frontend (Constants) | `PLAN-REPAIR-029` |
| `ISS-EPS-029-04` | Kunci `newborn.enabled` agar 100% hanya aktif saat `flow.entryMode === "newborn"`. | Frontend (Hook) | `PLAN-REPAIR-029` |
