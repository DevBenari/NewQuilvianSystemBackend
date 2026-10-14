# ISSUE-014 — Jenis Penanda Tangan pada Form Edit General Consent Workspace PPRI Tidak Mengikuti Data Subjek dan Dokumen Cetak Admisi

```yaml
issue_id: ISSUE-EPS-014
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Workspace PPRI — Menu General Consent, Form Edit Persetujuan (FE-RWI-241); Langkah Form Persetujuan Admisi (FE-INP-03 / FE-RWI-028) dan Cetak Surat Persetujuan (FE-INP-18 / FE-RWI-028)"
sumber_laporan: "Laporan pengguna dan 1 tangkapan layar tanggal 14-10-2026: Form Edit Persetujuan dengan Jenis Penanda Tangan 'Keluarga Lainnya', namun isian nama tetap 'Yoga Aditya Nugraha' (pasien) dan keterangan hubungan tetap 'Diri Sendiri'"
tanggal_issue: "2026-10-14"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "9f8d8266 (MHamzah)"
source_sha_frontend: "88cb19f0c (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-014-sinkronisasi-penanda-tangan-general-consent.md
```

---

## 1. Ringkasan

Pada Workspace PPRI (Pelayanan Pasien Rawat Inap), petugas dapat melakukan koreksi data persetujuan melalui tombol **Edit Persetujuan** (`GeneralConsentEditForm`). Namun, saat petugas mengganti pilihan **Jenis Penanda Tangan** (misalnya dari *Pasien Sendiri* menjadi *Keluarga Lainnya* atau *Orang Tua*), form tidak memperbarui isian identitas dan tidak membersihkan data subjek sebelumnya. Akibatnya, nama pasien, NIK pasien, nomor telepon pasien, alamat pasien, dan teks "Diri Sendiri" tetap tertinggal di form meskipun jenis penanda tangan sudah diubah ke pihak lain.

Kondisi ini bertolak belakang dengan perilaku pada **Halaman Admisi** (`inpatient-admission-consent-form-step.jsx`). Pada alur admisi, saat jenis penanda tangan diganti ke *Pasien Sendiri*, sistem otomatis mengisi (*autofill*) seluruh data pasien; dan saat diganti ke *Orang Tua*, *Suami / Istri*, *Wali yang Sah*, atau *Keluarga Lainnya*, sistem otomatis mencari data kontak darurat/wali yang terdaftar atau mengosongkan kolom identitas agar petugas tidak salah mencatat data pasien sebagai identitas keluarga. Selain itu, lembar pratinjau cetak surat persetujuan di Workspace PPRI tidak langsung tersinkronisasi saat isian form diubah.

Masalah ini berstatus **High** karena persetujuan rawat inap adalah dokumen hukum (General Consent) yang dicetak dan dimasukkan ke dalam berkas rekam medis resmi rumah sakit; ketidaksinkronan data penanggung jawab dapat menyebabkan persetujuan medis menjadi cacat hukum.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (asli / diringkas) | Lampiran |
| ---: | --- | --- |
| 1 | "Pada workspace PPRI form general consent: Perbaiki form pada jenis penanda tangan tidak ikuti sesuai form yang diisi dan tidak sesuai datanya yang diambil. Coba Anda analisis cetak surat persetujuan pada halaman admisi ikuti formnya seperti itu, tolong diperbaiki ya dan berikan planning-nya." | 1 tangkapan layar `media_1791963505955_2a7756a1.png`: Form Edit Persetujuan (Mengoreksi Versi 1), opsi *Keluarga Lainnya* aktif, namun nama penanda tangan tetap Yoga Aditya Nugraha, keterangan hubungan "Diri Sendiri", NIK 3674053007920005, dan alamat Jl. Anggrek No. 3. |

*Catatan privasi: Nama pasien dan NIK pada tangkapan layar berasal dari data uji coba lingkungan staging/development.*

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-014-01` | 1 | Pergantian jenis penanda tangan pada Form Edit General Consent tidak menyinkronkan data subjek, relasi, dan kandidat | `BUG` | Frontend | High | `SUDAH-VERIFIKASI` | `FIX-EPS-014-01` |
| `ISS-EPS-014-02` | 1 | Ketidaksinkronan perilaku form Workspace PPRI terhadap langkah admisi (`InpatientAdmissionConsentFormStep`) | `BUG` | Frontend | High | `SUDAH-VERIFIKASI` | `FIX-EPS-014-02` |
| `ISS-EPS-014-T1` | — | Dokumen cetak surat persetujuan (`InpatientConsentForm`) di Workspace PPRI tidak membaca `signer` saat persetujuan berstatus cetak kertas | `BUG` | Frontend | Medium | `SUDAH-VERIFIKASI` | `FIX-EPS-014-03` |
| `ISS-EPS-014-T2` | — | Nilai `signerRelationshipText` terkunci oleh fallback lama saat berganti jenis penanda tangan | `BUG` | Frontend | Medium | `SUDAH-VERIFIKASI` | `FIX-EPS-014-01` |

---

## 4. Rincian per Temuan

### ISS-EPS-014-01 — Pergantian Jenis Penanda Tangan Tidak Menyinkronkan Data Subjek dan Relasi

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend (`general-consent-edit-form.jsx`, `general-consent-section.jsx`) |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` pada source code dan tangkapan layar |
| **Perbaikan** | `FIX-EPS-014-01` |

**Apa yang terjadi.**
Saat petugas menekan tombol **Edit Persetujuan** di Workspace PPRI, data versi aktif dimuat. Ketika petugas mengganti pilihan chip **Jenis penanda tangan** (misalnya dari *Pasien Sendiri* menjadi *Keluarga Lainnya*), isian Nama, NIK, No. Handphone, dan Alamat tidak berubah sama sekali, dan kolom Keterangan Hubungan tetap bertuliskan "Diri Sendiri". Petugas yang tidak teliti dapat langsung menyimpan data tersebut, menghasilkan versi baru yang aneh: jenis penanda tangan "Keluarga Lainnya" tetapi nama dan NIK-nya milik pasien sendiri dengan hubungan "Diri Sendiri".

**Kenapa terjadi.**
Pada berkas `src/components/view/health-services/inpatient-management/admission-workspace/sections/general-consent/general-consent-edit-form.jsx` baris 182–198:
```javascript
const handleSignerTypeChange = useCallback(
  (nextType) => {
    setValues((current) => {
      const defaultRelationship =
        ADMISSION_CONSENT_SIGNER_TYPES.find((type) => type.value === nextType)?.relationship || "";
      return {
        ...current,
        signerType: nextType,
        signerRelationshipText: current.signerRelationshipText || defaultRelationship,
      };
    });
    clearFieldError("signerType");
    if (nextType !== initialValues.signerType) setCanvasOpen(true);
  },
  [clearFieldError, initialValues.signerType],
);
```
Ada dua penyebab teknis:
1. Kode `current.signerRelationshipText || defaultRelationship` selalu mempertahankan teks lama jika sudah ada isinya. Jika awalnya bernilai `"Diri Sendiri"`, teks tersebut tidak pernah diganti menjadi `"Keluarga"`, `"Orang Tua"`, atau `"Wali"`.
2. Komponen `GeneralConsentEditForm` tidak menerima prop data pasien (`patient`) maupun daftar calon penanda tangan (`candidates`) dari komponen induknya (`GeneralConsentSection`), sehingga fungsi `handleSignerTypeChange` sama sekali tidak memiliki sumber data untuk melakukan pengisian otomatis (*autofill*) atau pembersihan identitas pasien saat memilih pihak selain pasien sendiri.

**Dampak nyata.**
Data persetujuan hukum rawat inap menjadi kontradiktif: penanda tangan tercatat sebagai "Keluarga Lainnya", namun NIK dan nama terang yang tercetak adalah milik pasien. Bila terjadi sengketa medis, keabsahan surat persetujuan dapat digugat karena identitas penanda tangan rancu.

---

### ISS-EPS-014-02 — Ketidaksinkronan Perilaku Form Workspace PPRI terhadap Alur Admisi

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend (`inpatient-admission-consent-form-step.jsx` vs `general-consent-edit-form.jsx`) |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code alur admisi |
| **Perbaikan** | `FIX-EPS-014-02` |

**Apa yang terjadi.**
Pada alur admisi pendaftaran rawat inap (`InpatientAdmissionConsentFormStep`), terdapat standar pengelolaan perubahan jenis penanda tangan yang aman dan teruji:
1. Jika memilih `Self` (*Pasien Sendiri*):
   - Nama otomatis diisi `patient.fullName`.
   - NIK otomatis diisi `patient.identityNumber` atau `patient.nik`.
   - No. HP otomatis diisi `patient.phoneNumber`.
   - Alamat otomatis diisi `patient.address`.
   - Keterangan hubungan otomatis diisi `"Diri Sendiri"`.
2. Jika memilih `Parent`, `Spouse`, `Guardian`, atau `Family`:
   - Sistem menelusuri daftar `candidates` (calon penanda tangan dari riwayat keluarga / kontak darurat pasien).
   - Bila ditemukan kecocokan, data nama, NIK, telepon, alamat, dan hubungan diambil dari kandidat tersebut.
   - Bila tidak ditemukan kecocokan, data identitas pasien **wajib dibersihkan** (dikosongkan) agar tidak tertinggal, dan hubungan di-reset ke nilai bawaan (`Orang Tua`, `Pasangan`, `Wali`, `Keluarga`).

Namun di form Workspace PPRI, mekanisme cerdas dan aman ini tidak diikutsertakan, sehingga perilaku form menjadi tidak konsisten antara halaman pendaftaran admisi dan halaman workspace PPRI.

---

### ISS-EPS-014-T1 — Dokumen Cetak Surat Persetujuan di Workspace PPRI Tidak Mengambil Data Pilihan Cetak Kertas

| | |
| --- | --- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `BUG` |
| **Area** | Frontend (`use-admission-general-consent.js`, `inpatient-admission-print-utils.js`) |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code |
| **Perbaikan** | `FIX-EPS-014-03` |

**Apa yang terjadi.**
Pada Workspace PPRI, jika episode belum memiliki tanda tangan digital (`isConsentSigned = false`), petugas mengisi pilihan hubungan dan nama di panel ringkasan untuk dicetak di kertas (*print-ready*).
Namun pada `use-admission-general-consent.js` baris 92–95:
```javascript
const consentDocument = useMemo(
  () => buildConsentDocumentFromPrintData(printData, new Date(), signedConsent),
  [printData, signedConsent],
);
```
Parameter yang dikirim adalah `signedConsent` (yang bernilai `null`). Pilihan yang dipilih petugas di layar (`choices` dan `signer`) sama sekali tidak diteruskan ke pembangun dokumen cetak. Akibatnya, lembar cetak di bawahnya tetap kosong pada bagian nama dan alamat penanda tangan, serta checkbox hubungan tidak tercentang.

---

## 5. Tanya-Jawab Pelapor

**T: "Coba Anda analisis cetak surat persetujuan pada halaman admisi, ikuti formnya seperti itu?"**
**J:** 
Analisis menunjukkan bahwa komponen lembar cetak yang digunakan pada alur admisi dan Workspace PPRI sebenarnya **sudah memakai komponen cetak yang sama**, yaitu `InpatientConsentForm.jsx`. 
Perbedaannya terletak pada **sumber data yang memasok dokumen tersebut**:
1. **Pada Halaman Admisi:** Formulir input (`InpatientAdmissionConsentFormStep`) memiliki logika lengkap untuk mengambil data pasien saat memilih `Pasien Sendiri`, atau mencocokkan kontak keluarga dari `candidates` saat memilih keluarga/wali. Hasil simpanan form ini (`activeConsent`) langsung dipasok ke `InpatientAdmissionConsentStep` sehingga lembar cetak admisi selalu sinkron dan checkbox hubungan tercentang dengan benar.
2. **Pada Workspace PPRI:** Form Edit Persetujuan (`GeneralConsentEditForm`) kehilangan logika pengambilan data tersebut karena tidak menerima data `patient` dan `candidates`. Akibatnya, saat petugas mengklik `Keluarga Lainnya`, form tidak mengambil data keluarga dan tidak membersihkan data pasien. Hal inilah yang menyebabkan tampilan form edit dan data yang tercetak menjadi tidak sesuai.

Solusi yang tepat adalah menyelaraskan `GeneralConsentEditForm` agar mengadopsi logika penanganan penanda tangan yang persis sama dengan form admisi.

---

## 6. Temuan Tambahan

1. **`ISS-EPS-014-T2` — Bug Fallback Teks Hubungan:**
   Pada `general-consent-edit-form.jsx`, ekspresi `current.signerRelationshipText || defaultRelationship` menghalangi pembaruan keterangan hubungan ketika jenis penanda tangan berganti. Nilai harus otomatis disesuaikan dengan `defaultRelationship` dari jenis baru yang dipilih, kecuali jika pengguna sengaja mengetik teks kustom baru.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pemilik Sistem
1. **Konfirmasi Alur Pengisian Data Keluarga:**
   Apakah disetujui jika saat petugas memilih opsi selain *Pasien Sendiri* (seperti *Orang Tua* / *Keluarga Lainnya*), form otomatis mengambil data kontak darurat/wali yang terdaftar di sistem jika tersedia, dan mengosongkan kolom jika tidak tersedia?
   *Rekomendasi:* **Ya, disetujui.** Pola ini identik dengan langkah Formulir Persetujuan di alur admisi (`inpatient-admission-consent-form-step.jsx`).

---

## 8. Catatan Pola dan Pencegahan

- **Pola Komponen Form Terpisah:** Masalah ini terjadi karena `GeneralConsentEditForm` dibuat belakangan (pada perbaikan issue 008) tanpa menduplikasi atau mengekstraksi hook/utilitas manajemen penanda tangan yang sudah bekerja baik di admisi.
- **Pencegahan:** Ekstraksi fungsi resolver penanda tangan (pencocokan kandidat dan autofill data pasien) ke dalam utilitas bersama (`inpatient-admission-consent-edit-utils.js`) sehingga alur Admisi dan alur Workspace PPRI memakai fungsi murni yang sama.

---

## 9. Riwayat Dokumen

| Tanggal | Versi | Pengubah | Catatan Perubahan |
| :--- | :--- | :--- | :--- |
| 2026-10-14 | 1.0 | Antigravity AI | Penyusunan analisis akar masalah, perbandingan admisi vs workspace PPRI, dan rekomendasi perbaikan. |
