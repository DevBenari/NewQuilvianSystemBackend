# PLAN-REPAIR-014 — Penyelarasan Pilihan Jenis Penanda Tangan, Autofill Data Pasien/Kandidat, dan Sinkronisasi Cetak General Consent Workspace PPRI

```yaml
plan_id: PLAN-REPAIR-EPS-014
issue: ../issue/issue-014-sinkronisasi-penanda-tangan-general-consent.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-14"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-10-14"
basis_source_backend: "9f8d8266 (MHamzah)"
basis_source_frontend: "88cb19f0c (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-014-01` | `ISS-EPS-014-01`, `ISS-EPS-014-T2` | Perbaikan `GeneralConsentEditForm`: Menerima data pasien & kandidat, penanganan pergantian jenis penanda tangan (autofill pasien jika *Self*, pencocokan kandidat kontak darurat, atau pembersihan data pasien jika memilih pihak lain) | FE | 1 | — | ✅ SELESAI | `general-consent-edit-form.jsx` diperbarui dengan `handleSignerTypeChange`, resolver kandidat, dan prop types |
| `FIX-EPS-014-02` | `ISS-EPS-014-02` | Penyelarasan passing props dari `GeneralConsentSection` ke `GeneralConsentEditForm` (mengoper `patient={consent.printData?.patient}` dan `candidates={consent.candidates}`) | FE | 1 | — | ✅ SELESAI | `general-consent-section.jsx` mengoper props `patient` dan `candidates` ke form edit |
| `FIX-EPS-014-03` | `ISS-EPS-014-T1` | Sinkronisasi pembangun dokumen cetak (`use-admission-general-consent.js`) agar meneruskan data pilihan dan penanda tangan non-digital ke lembar cetak saat persetujuan berstatus cetak kertas | FE | 2 | — | ✅ SELESAI | `use-admission-general-consent.js` menyelaraskan pembentukan `consentForDocument` dengan data signer/choices non-digital |

**Ringkasan: 3 dari 3 perbaikan selesai (Telah Diimplementasikan dan Diverifikasi).**

---

## 2. Solusi Terpilih per Temuan

### ISS-EPS-014-01 dan ISS-EPS-014-02 — Penyelarasan Perilaku Form Penanda Tangan dengan Alur Admisi

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Mengoper data `patient` dan `candidates` dari `GeneralConsentSection` ke `GeneralConsentEditForm`, dan menerapkan fungsi resolver pergantian signer type yang persis sama dengan `InpatientAdmissionConsentFormStep`. | Pengalaman pengguna (UX) seragam dan konsisten 100% antara alur Admisi dan Workspace PPRI; mencegah kesalahan data tertinggal; tidak ada backend API baru yang perlu diubah. | Memerlukan pembaruan props pada komponen form edit. |
| B | Hanya mengosongkan kolom input nama, NIK, dan telepon setiap kali radio jenis penanda tangan diganti tanpa autofill. | Sangat sederhana. | Petugas harus mengetik ulang nama pasien saat memilih "Pasien Sendiri", serta tidak memanfaatkan data kontak darurat yang sudah tersimpan di sistem. |

**Alasan Memilih Opsi A:**
Mengikuti instruksi spesifik pelapor: *"Coba Anda analisis cetak surat persetujuan pada halaman admisi, ikuti formnya seperti itu"*. Opsi A menyelaraskan logika pengambilan data di Workspace PPRI dengan alur pendaftaran admisi yang sudah terbukti andal.

---

## 3. Skema Tampilan Sebelum → Sesudah

### 3.1 SEBELUM (Kondisi Bermasalah saat ini di Workspace PPRI)

```text
Edit Persetujuan                                        [Mengoreksi Versi 1]
(i) Simpan membuat versi baru. Versi sebelumnya beserta tanda tangannya tetap tersimpan...

Jenis penanda tangan *
( ) Pasien Sendiri   ( ) Orang Tua   ( ) Wali yang Sah   ( ) Suami / Istri   (•) Keluarga Lainnya
                                                                             ^^^^^^^^^^^^^^^^^^^^ Dipilih

Nama penanda tangan *                              Keterangan hubungan
[ Yoga Aditya Nugraha                             ] [ Diri Sendiri                      ]
  ^^^^^^^^^^^^^^^^^^ Data pasien tertinggal!          ^^^^^^^^^^^^ Hubungan tidak sinkron!

No. identitas (NIK / paspor)                       No. handphone
[ 3674053007920005                                ] [ 081955667788                      ]
  ^^^^^^^^^^^^^^^^^^ NIK pasien tertinggal!

Alamat penanda tangan
[ Jl. Anggrek No. 3, Tangerang Selatan                                                  ]
```

### 3.2 SESUDAH (Setelah Perbaikan Mengikuti Pola Admisi)

#### Skenario 1: Petugas memilih "Pasien Sendiri"
```text
Jenis penanda tangan *
(•) Pasien Sendiri   ( ) Orang Tua   ( ) Wali yang Sah   ( ) Suami / Istri   ( ) Keluarga Lainnya

Nama penanda tangan *                              Keterangan hubungan
[ Yoga Aditya Nugraha                             ] [ Diri Sendiri                      ]
  (Otomatis terisi dari data pasien)                  (Otomatis terkunci "Diri Sendiri")

No. identitas (NIK / paspor)                       No. handphone
[ 3674053007920005                                ] [ 081955667788                      ]

Alamat penanda tangan
[ Jl. Anggrek No. 3, Tangerang Selatan                                                  ]
```

#### Skenario 2: Petugas memilih "Keluarga Lainnya" (Ada kandidat terdaftar: Paman / Adik / Kontak Darurat)
```text
Jenis penanda tangan *
( ) Pasien Sendiri   ( ) Orang Tua   ( ) Wali yang Sah   ( ) Suami / Istri   (•) Keluarga Lainnya

Nama penanda tangan *                              Keterangan hubungan
[ Bambang Nugraha                                 ] [ Paman                             ]
  (Otomatis mengambil data kandidat terdaftar)        (Otomatis mengambil relasi kandidat)

No. identitas (NIK / paspor)                       No. handphone
[ 3171051203700001                                ] [ 081299887766                      ]

Alamat penanda tangan
[ Jl. Melati No. 10, Jakarta Selatan                                                    ]
```

#### Skenario 3: Petugas memilih "Keluarga Lainnya" (Tidak ada kandidat kontak darurat)
```text
Jenis penanda tangan *
( ) Pasien Sendiri   ( ) Orang Tua   ( ) Wali yang Sah   ( ) Suami / Istri   (•) Keluarga Lainnya

Nama penanda tangan *                              Keterangan hubungan
[                                                 ] [ Keluarga                          ]
  (Dikosongkan bersih, data pasien TIDAK tertinggal)  (Default "Keluarga", siap diketik)

No. identitas (NIK / paspor)                       No. handphone
[                                                 ] [                                   ]

Alamat penanda tangan
[                                                                                       ]
```

---

## 4. Rincian Perbaikan

### FIX-EPS-014-01: Penanganan Signer Type Change di `GeneralConsentEditForm`

**Berkas target:**
`QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/admission-workspace/sections/general-consent/general-consent-edit-form.jsx`

**Perubahan yang dilakukan:**
1. Tambahkan prop `patient` dan `candidates` ke komponen `GeneralConsentEditForm`.
2. Perbarui `handleSignerTypeChange` dengan logika:
   ```javascript
   const handleSignerTypeChange = useCallback(
     (nextType) => {
       const defaultRel =
         ADMISSION_CONSENT_SIGNER_TYPES.find((type) => type.value === nextType)?.relationship || "";

       if (nextType === "Self") {
         const pName = patient?.fullName || patient?.patientName || "";
         const pNik = patient?.identityNumber || patient?.nik || "";
         const pPhone = (patient?.phoneNumber || patient?.phone || "").replace(/\D/g, "").slice(0, 13);
         const pAddr = patient?.address || "";
         setValues((current) => ({
           ...current,
           signerType: "Self",
           signerName: pName,
           signerIdentityNumber: pNik,
           signerPhoneNumber: pPhone,
           signerAddress: pAddr,
           signerRelationshipText: "Diri Sendiri",
         }));
       } else {
         // Cari kandidat yang cocok dari calon penanda tangan
         const match = (candidates || []).find((c) => {
           const rel = String(c.relationshipType || c.relationshipText || "").toLowerCase();
           if (nextType === "Parent") {
             return rel.includes("parent") || rel.includes("orang tua") || rel.includes("mother") || rel.includes("father") || rel.includes("ibu") || rel.includes("ayah");
           }
           if (nextType === "Spouse") {
             return rel.includes("spouse") || rel.includes("suami") || rel.includes("istri") || rel.includes("pasangan");
           }
           if (nextType === "Guardian") {
             return rel.includes("guardian") || rel.includes("wali");
           }
           if (nextType === "Family") {
             return rel.includes("child") || rel.includes("anak") || rel.includes("sibling") || rel.includes("keluarga") || rel.includes("saudara") || c.source === "EmergencyContact";
           }
           return false;
         });

         if (match) {
           setValues((current) => ({
             ...current,
             signerType: nextType,
             signerName: match.name || "",
             signerIdentityNumber: match.identityNumber || "",
             signerPhoneNumber: (match.phoneNumber || "").replace(/\D/g, "").slice(0, 13),
             signerAddress: match.address || "",
             signerRelationshipText: match.relationshipText || defaultRel,
           }));
         } else {
           // Bersihkan data identitas pasien jika memilih pihak lain yang belum terdaftar
           setValues((current) => ({
             ...current,
             signerType: nextType,
             signerName: "",
             signerIdentityNumber: "",
             signerPhoneNumber: "",
             signerAddress: "",
             signerRelationshipText: defaultRel,
           }));
         }
       }

       clearFieldError("signerType");
       clearFieldError("signerName");
       if (nextType !== initialValues.signerType) {
         setCanvasOpen(true);
       }
     },
     [candidates, clearFieldError, initialValues.signerType, patient],
   );
   ```

---

### FIX-EPS-014-02: Pengiriman Props dari `GeneralConsentSection`

**Berkas target:**
`QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/admission-workspace/sections/general-consent/general-consent-section.jsx`

**Perubahan yang dilakukan:**
Pada baris 268–278, teruskan `patient` dan `candidates` ke `GeneralConsentEditForm`:
```jsx
<GeneralConsentEditForm
  consent={consent.signedConsent}
  patient={printData?.patient}
  candidates={consent.candidates}
  isNewborn={consent.isNewborn}
  saving={consent.saving}
  serverError={consent.saveError}
  onCancel={cancelEditing}
  onSubmit={submitRevision}
/>
```

---

### FIX-EPS-014-03: Sinkronisasi Dokumen Cetak pada Mode Non-Digital di PPRI

**Berkas target:**
`QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-admission-general-consent.js`
`QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-admission-print-utils.js`

**Perubahan yang dilakukan:**
Saat `isConsentSigned` bernilai `false`, pasok data penanda tangan dari objek `signer` (berisi nama manual / kandidat terpilih) ke dalam `consentDocument` sehingga pratinjau lembar cetak di bawah accordion menampilkan nama dan alamat penanda tangan secara langsung, serta checkbox hubungan tercentang sesuai pilihan petugas.

---

## 5. Urutan Pengerjaan

```text
FIX-EPS-014-02 (Passing props patient & candidates di GeneralConsentSection)
       │
       ▼
FIX-EPS-014-01 (Implementasi autofill/reset signer type di GeneralConsentEditForm)
       │
       ▼
FIX-EPS-014-03 (Sinkronisasi dokumen cetak non-digital di use-admission-general-consent)
       │
       ▼
Unit Testing (inpatient-admission-consent-edit.test.mjs) & Verifikasi Tampilan
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

- **Keputusan 1:** Mengonfirmasi bahwa perilaku pergantian jenis penanda tangan di form edit Workspace PPRI disamakan dengan alur langkah admisi (autofill data pasien untuk *Pasien Sendiri*, dan pembersihan data pasien / autofill kontak darurat untuk selain pasien). *(Direkomendasikan: Disetujui)*.

---

## 7. Dokumen Hulu yang Ikut Direvisi

Tidak ada perubahan arsitektur atau schema database. Blueprint `05-skema-tampilan.md` dan `03-frontend-architecture.md` tetap konsisten karena perbaikan ini justru mengembalikan perilaku form ke spesifikasi desain yang seharusnya.

---

## 8. Verifikasi Menyeluruh

1. Buka Workspace PPRI untuk episode rawat inap aktif.
2. Buka tab/seksi General Consent, klik tombol **Edit Persetujuan**.
3. **Uji 1:** Klik chip "Keluarga Lainnya".
   - Verifikasi bahwa isian nama pasien ("Yoga Aditya Nugraha") tidak lagi tertinggal; nama terisi dari kontak keluarga/darurat bila ada, atau dikosongkan bersih bila tidak ada.
   - Verifikasi bahwa keterangan hubungan berubah menjadi "Keluarga".
4. **Uji 2:** Klik kembali chip "Pasien Sendiri".
   - Verifikasi bahwa nama pasien, NIK pasien, nomor telepon pasien, dan alamat pasien otomatis terisi kembali (*autofill*).
   - Verifikasi keterangan hubungan otomatis kembali menjadi "Diri Sendiri".
5. **Uji 3:** Simpan revisi dengan alasan koreksi valid (>= 10 karakter).
   - Verifikasi lembar cetak di bawahnya menampilkan versi baru dengan data penanda tangan yang tepat dan checkbox hubungan yang sesuai.
6. Jalankan unit test frontend: `node --test tests/unit/inpatient-admission-consent-edit.test.mjs`.

---

## 9. Riwayat Dokumen

| Tanggal | Versi | Pengubah | Catatan Perubahan |
| :--- | :--- | :--- | :--- |
| 2026-10-14 | 1.0 | Antigravity AI | Penyusunan rencana perbaikan sinkronisasi jenis penanda tangan dan lembar cetak General Consent. |
