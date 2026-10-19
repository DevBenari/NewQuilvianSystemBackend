# PLAN-REPAIR-029 — Penanganan Tuntas Kontaminasi State dan Isolasi Session Storage Antarmode Admisi Rawat Inap

```yaml
plan_id: PLAN-REPAIR-EPS-029
issue: ../issue/issue-029-kontaminasi-state-dan-session-storage-antar-mode-admisi.md
status_rencana: SELESAI_DIIMPLEMENTASIKAN
tanggal_rencana: "2026-10-17"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-10-17"
basis_source_backend: "d420c2fbe094a2d70776c44955233bce9c87bda8 (MHamzah)"
basis_source_frontend: "0accf387138a3fcc1ff40fbb5bdeb960076b4d59 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| `FIX-EPS-029-01` | `ISS-EPS-029-03` | Hapus seluruh klausul mutasi dinamis pada `resolveInpatientAdmissionSteps` sehingga alur `new` dan `existing` kebal dari perubahan ke alur bayi | Frontend | 1 | `FIX-EPS-029` | ✅ SELESAI | `resolveInpatientAdmissionSteps` (inpatient-admission-flow-constants.jsx), unit test 9/9 lulus |
| `FIX-EPS-029-02` | `ISS-EPS-029-04` | Kunci hook `useInpatientNewbornAdmission` agar `enabled` secara eksklusif hanya pada `entryMode === "newborn"` | Frontend | 1 | `FIX-EPS-029` | ✅ SELESAI | `use-inpatient-newborn-admission.js` (enabled = flow.entryMode === "newborn") |
| `FIX-EPS-029-03` | `ISS-EPS-029-01` | Tambahkan auto-reset state `patientType` dan `newbornDraft` saat `entryMode !== "newborn"` di `useInpatientAdmissionFlow` | Frontend | 1 | `FIX-EPS-029` | ✅ SELESAI | `use-inpatient-admission-flow.jsx` (effect auto-reset saat entryMode non-newborn) |
| `FIX-EPS-029-04` | `ISS-EPS-029-02` | Hentikan persistensi sesi di menu awal (`!entryMode`), sanitasi data saat rehidrasi, dan sapu residu kunci sesi tercemar | Frontend | 1 | `FIX-EPS-029` | ✅ SELESAI | `use-inpatient-admission-flow.jsx` (storageKey kosong saat !entryMode, auto-purge unknown:draft) |

**Ringkasan: 4 dari 4 perbaikan selesai (100% selesai).**

---

## 2. Solusi Terpilih Per Temuan

### FIX-EPS-029-01: Pemutusan Mutasi Dinamis pada `resolveInpatientAdmissionSteps`
- **Tindakan**:
  Ubah fungsi `resolveInpatientAdmissionSteps` pada `inpatient-admission-flow-constants.jsx` menjadi:
  ```javascript
  export const resolveInpatientAdmissionSteps = (
    entryMode,
    { includeDeposit = true } = {},
  ) => {
    if (entryMode === INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN) {
      return NEWBORN_ADMISSION_STEPS;
    }

    const steps = getInpatientAdmissionSteps(entryMode);
    if (includeDeposit) return steps;

    return Object.freeze(
      steps
        .filter((step) => step.slug !== INPATIENT_DEPOSIT_STEP_SLUG)
        .map((step, index) => Object.freeze({ ...step, value: index + 1 })),
    );
  };
  ```
- **Tujuan**:
  Menghilangkan sepenuhnya parameter `isNewborn` dari penentuan langkah. Alur Pasien Baru (`new`) akan **selalu dan mutlak 12 langkah**, dan alur Pasien Lama (`existing`) akan **selalu dan mutlak 10/11 langkah**. Tidak ada celah logika apa pun yang dapat membelokkannya ke langkah bayi.

---

### FIX-EPS-029-02: Penguncian Eksklusif Hook Admisi Bayi
- **Tindakan**:
  Pada `use-inpatient-newborn-admission.js`:
  ```javascript
  const enabled = flow.entryMode === INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN;
  ```
  Dan efek deteksi profil/episode:
  ```javascript
  useEffect(() => {
    if (flow.entryMode !== INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN) return;
    ...
  }, [flow, profile, resumeEpisode]);
  ```
- **Tujuan**:
  Menjamin hook bayi, fetch kandidat ibu, dan form DPJP Bayi (`NewbornCareStep`) tidak akan pernah aktif saat pengguna berada pada alur Pasien Baru, Pasien Lama, Kamar Pulih, atau Transfer.

---

### FIX-EPS-029-03: Sinkronisasi dan Auto-Reset State Memory React
- **Tindakan**:
  Pada `useInpatientAdmissionFlow.jsx`:
  Tambahkan sinkronisasi dua arah terhadap `entryMode`:
  ```javascript
  useEffect(() => {
    if (entryMode === INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN) {
      if (patientType !== INPATIENT_PATIENT_TYPE.NEWBORN) {
        setPatientType(INPATIENT_PATIENT_TYPE.NEWBORN);
      }
    } else {
      // Jika berada di alur non-bayi (new, existing, recovery, transfer, atau menu utama)
      if (patientType === INPATIENT_PATIENT_TYPE.NEWBORN) {
        setPatientType("");
        setNewbornDraft({ ...DEFAULT_NEWBORN_ADMISSION });
        setMotherEpisodeId("");
      }
    }
  }, [entryMode, patientType]);
  ```
- **Tujuan**:
  Menjamin seketika URL berubah keluar dari `entry=newborn`, memori React langsung bersih dari sisa tipe `newborn` dan draf bayi.

---

### FIX-EPS-029-04: Penataan Kunci dan Sanitasi Session Storage
- **Tindakan**:
  1. Jangan buat `storageKey` saat berada di menu utama (`!entryMode`):
     ```javascript
     const storageKey = useMemo(() => {
       if (!entryMode) return "";
       const id = resumeEpisodeId || selectedPatientId || "active";
       return `${FLOW_STORAGE_PREFIX}${entryMode}:${id}`;
     }, [entryMode, resumeEpisodeId, selectedPatientId]);
     ```
  2. Jangan pernah menulis ke `sessionStorage` saat `!entryMode` atau `!storageKey`:
     ```javascript
     useEffect(() => {
       if (!entryMode || !storageKey) return;
       // Jangan izinkan penyimpanan patientType "newborn" pada mode non-bayi
       const safePatientType =
         entryMode !== INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN &&
         patientType === INPATIENT_PATIENT_TYPE.NEWBORN
           ? ""
           : patientType;

       saveFlowDraft(storageKey, {
         patientType: safePatientType,
         newbornDraft: entryMode === INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN ? newbornDraft : DEFAULT_NEWBORN_ADMISSION,
         motherEpisodeId: entryMode === INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN ? motherEpisodeId : "",
         visitType,
         referralData,
       });
     }, [entryMode, motherEpisodeId, newbornDraft, patientType, referralData, storageKey, visitType]);
     ```
  3. Sanitasi saat Rehidrasi (`readSavedFlowDraft`):
     Jika membaca kunci dan menemukan `saved.patientType === "newborn"` pada `entryMode !== "newborn"`, abaikan dan hapus kunci tersebut dari penyimpanan.
  4. Sweeper Pembersih Kunci Tercemar:
     Saat aplikasi di-mount atau saat `clearInpatientAdmissionSession` dipanggil, hapus kunci yang memiliki prefix `unknown` (`quilvian:inpatient-admission:flow:unknown:*`).

---

## 3. Rencana Pengujian dan Validasi

1. **Uji Kasus Beruntun (Sequential Flow Test)**:
   - Skenario 1: Buka `Admisi Rawat Inap` -> Klik **Admisi Bayi Baru Lahir** (`entry=newborn`). Verifikasi Langkah 1 (Pilih Ibu Persalinan) terbuka normal.
   - Skenario 2: Klik tombol "Kembali" ke menu utama. Verifikasi `sessionStorage` **tidak memiliki** kunci `unknown:draft`.
   - Skenario 3: Klik kartu **Pendaftaran Pasien Lama** (`entry=existing`). Verifikasi sistem membuka **Langkah 1 dari 10: Pasien Lama** (Form Pencarian Nomor RM/NIK), **BUKAN** Langkah 3 Bayi.
   - Skenario 4: Klik tombol "Kembali" ke menu utama -> Klik kartu **Pendaftaran Pasien Baru** (`entry=new`). Verifikasi sistem membuka **Langkah 1 dari 12: Jenis Kunjungan**, **BUKAN** alur bayi.
2. **Uji Unit Test**:
   - Seluruh unit test pada `tests/unit/inpatient-newborn-admission.test.mjs` harus tetap lulus 100%.
3. **Uji Production Build**:
   - Jalankan `npm run build` dan pastikan kompilasi Turbopack selesai dengan exit code 0.
