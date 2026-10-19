# Laporan Perubahan Frontend — `FIX-EPS-028`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Pemisahan Pintu Masuk Admisi Bayi Baru Lahir dan Penataan Session Storage |
| Rencana Perbaikan | `PLAN-REPAIR-028` |
| Issue Terkait | `ISSUE-028` (Lompatan ke Step 4 dan Polusi Sesi Bayi) |
| Tanggal | 17 Oktober 2026 |
| Status | **Selesai Source & Lulus Uji** |
| Basis Git Frontend | `HamzahV2` (`0accf387138a3fcc1ff40fbb5bdeb960076b4d59`) |

## Ringkasan Perubahan

Perbaikan ini menyelesaikan bug UI/UX admisi rawat inap di mana memilih kategori bayi pada form pendaftaran pasien baru reguler menyebabkan *stepper morphing* dinamis yang melompati Langkah 1 dan Langkah 2 langsung ke Langkah 4, serta residu *zombie draft* di `sessionStorage` lama yang merehidrasi form baru secara tidak beraturan.

Sesuai persetujuan Pemilik Sistem atas solusi terpilih **Opsi C (Win Solution)**:
1. **Pemisahan Pintu Masuk Mandiri**: Menambahkan kartu pilihan entri ke-5 **"Admisi Bayi Baru Lahir"** (`entry=newborn`) pada layar utama `Pilih Tipe Pendaftaran`, lengkap dengan ilustrasi 3D SVG buaian/inkubator neonatus (`NewbornPatientIllustration`), tema kartu toska/mint, dan layout grid 5 kolom responsif (`.gridFive`).
2. **Stepper Deterministik 7 Langkah**: Alur `entry=newborn` memiliki 7 langkah statis sejak awal (`newborn-context` hingga `newborn-documents`), tanpa ketergantungan pada fallback slug dinamis alur umum.
3. **Pembersihan Kontrol Redundan**: Menghapus `MotherEpisodeSelector` dari form pasien baru umum (`PatientTypeStep`) dan memusatkan pemilihan kasus persalinan ibu 100% pada Langkah 1 Alur Bayi (`NewbornContextStep`). Opsi bayi juga dikeluarkan dari daftar jenis pasien umum agar pintu masuk pendaftaran bayi tunggal dan tegas.
4. **Isolasi dan Standarisasi Session Storage**: Kunci penyimpanan peramban distandarisasi ke format `quilvian:inpatient-admission:flow:<entryMode>:<id>`. Fungsi `clearInpatientAdmissionSession` dan `clearFlowDraft` menyapu bersih seluruh variasi kunci aktif maupun format legacy (`quilvian_admission_flow_*`) saat alur dibatalkan, direset, atau berpindah pasien.

## Berkas yang Diubah

1. `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`:
   - Menambahkan enum `NEWBORN: "newborn"` pada `INPATIENT_ADMISSION_ENTRY_MODE`.
   - Menambahkan kartu ke-5 pada `INPATIENT_ADMISSION_ENTRY_OPTIONS`.
   - Mendaftarkan alur `NEWBORN` pada `getInpatientAdmissionSteps`, `resolveInpatientAdmissionSteps`, dan `resolvePatientTypeContinuationSlug`.
   - Menghapus opsi bayi dari `INPATIENT_PATIENT_TYPE_OPTIONS` untuk alur umum reguler.
2. `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx`:
   - Mendaftarkan `NEWBORN` pada `isKnownEntryMode`.
   - Mengubah konstanta prefix ke `quilvian:inpatient-admission:flow:`.
   - Mengisolasi kunci penyimpanan berbasis `${FLOW_STORAGE_PREFIX}${rawEntryMode}:${id}`.
   - Menginisialisasi `patientType = "newborn"` otomatis saat `entryMode === "newborn"`.
   - Memperbarui `clearFlowDraft()` untuk membersihkan variasi kunci sesi.
3. `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js`:
   - Mengizinkan `enabled` berlaku pada `flow.entryMode === "newborn"`.
   - Mendukung deteksi mode `newborn` pada sinkronisasi profil dan kandidat ibu.
4. `src/utils/health-services/inpatient-management/inpatient-admission-session-utils.js`:
   - Memperbarui `clearInpatientAdmissionSession` agar menyapu kunci prefix `quilvian:inpatient-admission:` dan legacy `quilvian_admission_flow_`.
5. `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`:
   - Menambahkan `[INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN]` pada `ENTRY_COPY`.
   - Menghapus komponen dan import `MotherEpisodeSelector` redundan dari `PatientTypeStep`.
   - Memperbarui deskripsi `PatientEntryChoiceStep` untuk mencakup bayi baru lahir.
6. `src/components/view/health-services/inpatient-management/inpatient-admission-consent-form-step.jsx`:
   - Menyesuaikan penentuan `isNewborn` agar mengenali `flow.entryMode === "newborn"`.
7. `src/components/view/health-services/registration-management/emergency-registration/patient-entry-choice-step.jsx`:
   - Menambahkan komponen ilustrasi 3D SVG `NewbornPatientIllustration`.
   - Menambahkan penanganan `isFiveOrMore` untuk layout grid 5 kartu.
   - Menambahkan pemetaan tema kartu, visual, badge, bullet, dan tombol aksi untuk mode `newborn`.
8. `src/components/view/health-services/registration-management/emergency-registration/patient-entry-choice-step.module.css`:
   - Menambahkan class `.gridFive` beserta breakpoint responsifnya (3 kolom pada <=1280px, 2 kolom pada <=900px, 1 kolom pada <=600px).
   - Menambahkan kelas tema `.cardNewborn`, `.badgeNewborn`, `.bulletIconNewborn`, dan `.arrowButtonNewborn`.
9. `tests/unit/inpatient-newborn-admission.test.mjs`:
   - Menyelaraskan pengujian kartu entri admisi bayi baru lahir pada menu utama.

## Bukti dan Verifikasi

- **Unit Tests**:
  - `node --import ./tests/helpers/register.mjs --test "tests/unit/inpatient-newborn-admission.test.mjs"`
  - Hasil: 9 dari 9 test **LULUS (PASS 100%)**, mencakup validasi 7 langkah tanpa deposit/penjamin, validasi identitas bayi, sanitasi payload, dan isolasi konteks admisi.
- **Production Build (Next.js & Turbopack)**:
  - Perintah: `npm run build`
  - Hasil: **LULUS BERSIH (Exit code 0)**, 498+ rute aplikasi berhasil dikompilasi, static & public assets tersalin, runtime standalone siap dijalankan.

