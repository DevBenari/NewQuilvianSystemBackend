# Kontrak Spesifikasi — Transfer Pasien Masuk (IGD/Rajal ke Rawat Inap)

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `episode-rawat-inap` (kolaborasi dengan `ClinicalManagement` dan `Workspace PPRI`) |
| Versi Kontrak | `0.14.0` — `draft` |
| Tanggal | 16 Oktober 2026 |
| Target Repositori | Backend: `NewQuilvianSystemBackend` (`MHamzah`), Frontend: `QuilvianSystemFrontendDev` (`HamzahV2`) |
| Acuan Keputusan | Keputusan Wawancara Bisnis (`/grill-me`), `admission-transfer-repair-006.md`, `00-interview-decisions.md` |

---

## 1. Ringkasan Ruang Lingkup dan Batasan

Dokumen kontrak ini mengatur interaksi transfer pasien masuk dari Instalasi Gawat Darurat (IGD) maupun Poliklinik Rawat Jalan (Rajal) menuju Ruang Perawatan Rawat Inap (Ranap) setelah proses pendaftaran admisi transfer dieksekusi.

Terdapat dua antarmuka kerja utama yang terlibat:
1. **Workspace PPRI (Pusat Penerimaan Pasien Rawat Inap / Dokumen Admisi)**:
   - Dikelola oleh Petugas Admisi & CRO.
   - Bertanggung jawab atas pencetakan Gelang Rawat Inap resmi & label stiker medis (menggantikan gelang identitas sementara IGD).
   - Memproses dokumen persetujuan admisi: General Consent, Formulir Serah Terima Pasien Baru (Checklist 15 butir administrasi), Lembar Selisih Biaya, dan Bukti Pelunasan Deposit.
   - Sifat: *Asinkron / Non-blocking terhadap fisik pasien*. Jika berkas belum lengkap saat pasien didorong ke bangsal, pelayanan medis di bangsal **TETAP BOLEH BERJALAN**, dengan alert informasi kuning pada Detail Episode.
2. **Detail Episode Rawat Inap (`InpatientEpisodeDetailView`)**:
   - Dikelola oleh Perawat Bangsal Rawat Inap & Dokter DPJP.
   - Menampilkan banner serah terima transfer aktif dan membuka Laci Serah Terima Klinis (`TransferHandoverDrawer`).
   - Bertanggung jawab memverifikasi kondisi fisik, TTV, alat terpasang, dan **Rekonsiliasi Obat & Alergi**.
   - **Wajib menetapkan Perawat Penanggung Jawab Asuhan (PPJA)** saat menerima pasien (`[Terima Pasien]`).

---

## 2. Spesifikasi Endpoint API (Swagger-Style)

### Grup 1: Clinical Management / Transfer Handover

Grup ini menangani serah terima klinis antarunit kerja (IGD/Poli ke Bangsal Ranap).

#### [Tags("Health Services / Clinical Management / Transfer Handover")]

| Method | Path | Deskripsi & Kegunaan | Hak Akses (Permission) | Status |
|---|---|---|---|---|
| `GET` | `/api/v1/health-services/clinical-management/transfer-handovers` | Mengambil daftar dokumen serah terima transfer berdasarkan episode atau unit ruangan tujuan | `ClinicalTransferHandover:Read` | `Sudah ada` |
| `GET` | `/api/v1/health-services/clinical-management/transfer-handovers/{id}` | Mengambil detail lengkap dokumen serah terima klinis beserta snapshot TTV, alat, obat & alergi | `ClinicalTransferHandover:Read` | `Sudah ada` |
| `POST` | `/api/v1/health-services/clinical-management/transfer-handovers/{id}/send` | Mengirim dokumen serah terima klinis dari unit asal (IGD/Poli) ke unit tujuan | `ClinicalTransferHandover:Update` | `Sudah ada` |
| `PATCH` | `/api/v1/health-services/clinical-management/transfer-handovers/{id}/accept` | Menerima serah terima pasien di bangsal rawat inap sekaligus menetapkan Perawat PPJA | `ClinicalTransferHandover:Receive` | `Diperbarui` (Tambah field PPJA) |
| `PATCH` | `/api/v1/health-services/clinical-management/transfer-handovers/{id}/reject` | Menolak / meminta klarifikasi serah terima pasien ke unit asal dengan alasan beralasan | `ClinicalTransferHandover:Receive` | `Sudah ada` |

---

### Rincian Endpoint yang Diperbarui: `PATCH /transfer-handovers/{id}/accept`

- **Endpoint**: `PATCH /api/v1/health-services/clinical-management/transfer-handovers/{id}/accept`
- **Tujuan**: Mengonfirmasi penerimaan klinis pasien di bangsal rawat inap, membekukan data serah terima, mengonfirmasi tempat tidur, dan menetapkan Perawat PPJA.
- **Header**:
  - `Authorization: Bearer <token>`
  - `Content-Type: application/json`
- **Request Payload**:
```json
{
  "notes": "Pasien tiba di bangsal Mawar Kamar 201 Bed A dalam kondisi stabil, infus lancar.",
  "assignedNurseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "medicationReconciliations": [
    {
      "medicationName": "Ceftriaxone 1g",
      "dosage": "1 vial",
      "route": "IV",
      "lastAdministeredAt": "2026-10-16T08:30:00Z",
      "action": "Continue",
      "notes": "Dosis kedua jam 20:30"
    },
    {
      "medicationName": "Amlodipine 10mg",
      "dosage": "1 tablet",
      "route": "Oral",
      "lastAdministeredAt": null,
      "action": "Continue",
      "notes": "Obat bawaan pasien, diminum malam hari"
    }
  ]
}
```

- **Validasi Bisnis**:
  1. `assignedNurseId` **WAJIB** diisi (Guid tidak boleh `Guid.Empty`). Jika kosong, kembalikan `422 UNPROCESSABLE ENTITY` dengan error `NURSE_PPJA_REQUIRED: "Penetapan Perawat Penanggung Jawab Asuhan (PPJA) wajib diisi saat menerima pasien."`
  2. Perawat penerima (berdasarkan akun login) **TIDAK BOLEH** merupakan perawat yang sama dengan perawat pengirim serah terima di unit asal. Jika sama, kembalikan `400 BAD REQUEST` dengan error `SENDER_RECEIVER_SAME_ACCOUNT: "Penerima serah terima harus perawat ruangan tujuan yang berbeda dari perawat pengirim."`
  3. Status dokumen serah terima harus `Sent`. Jika masih `Draft` atau sudah `Accepted`, kembalikan `409 CONFLICT`.

- **Response Sukses (HTTP 200 OK)**:
```json
{
  "statusCode": 200,
  "message": "Serah terima pasien transfer berhasil diterima dan perawat PPJA telah ditetapkan.",
  "data": {
    "handoverId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "episodeId": "4c6c10e3-95ad-4f52-8703-95568bc5bfe7",
    "status": "Accepted",
    "acceptedAt": "2026-10-16T10:15:00Z",
    "acceptedByUserId": "e1f18579-2ef4-4f16-8f2b-7bc924376378",
    "assignedNurseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "assignedNurseName": "Ns. Ratna Sari, S.Kep",
    "assignedRole": "PrimaryNursePPJA"
  }
}
```

---

### Grup 2: Inpatient Management / Admission Documents (Workspace PPRI)

Grup ini menangani berkas administrasi admisi dan pencetakan gelang identitas resmi di Workspace PPRI.

#### [Tags("Health Services / Inpatient Management / Admission Documents")]

| Method | Path | Deskripsi & Kegunaan | Hak Akses (Permission) | Status |
|---|---|---|---|---|
| `GET` | `/api/v1/health-services/inpatient-management/admission-documents/wristband-and-label` | Mengambil data pratinjau cetak gelang pasien ranap resmi & label stiker medis | `InpatientAdmissionDocument:Read` | `Sudah ada` |
| `POST` | `/api/v1/health-services/inpatient-management/admission-documents/wristband-and-label/print` | Mencatat audit log pencetakan gelang ranap resmi & label medis | `InpatientAdmissionDocument:Print` | `Sudah ada` |
| `GET` | `/api/v1/health-services/inpatient-management/admission-documents/new-patient-handover` | Mengambil formulir checklist 15 butir serah terima pasien baru admisi | `InpatientAdmissionDocument:Read` | `Sudah ada` |
| `POST` | `/api/v1/health-services/inpatient-management/admission-documents/new-patient-handover/lock` | Mengunci checklist serah terima admisi sebelum penandatanganan | `InpatientAdmissionDocument:Update` | `Sudah ada` |
| `POST` | `/api/v1/health-services/inpatient-management/admission-documents/new-patient-handover/sign` | Membubuhkan tanda tangan digital petugas (Admisi, CRO, atau Perawat) | `InpatientAdmissionDocument:Sign` | `Sudah ada` |

---

## 3. Matriks Transisi Status (State-Transition Matrix)

### A. Dokumen Serah Terima Klinis (`CliTransferHandover`)

| Status Awal | Aksi / Trigger | Pelaku | Status Akhir | Invariant / Syarat |
|---|---|---|---|---|
| *None* | Admisi Transfer Disubmit (`POST /admission-transfers/admit`) | Sistem (Otomatis) | `PendingSend` (Draft) | Snapshot TTV, keluhan, dan obat tersalin dari IGD/Poli. |
| `PendingSend` | Perawat Unit Asal memvalidasi dan klik `[Kirim Serah Terima]` | Perawat Unit Asal (IGD/Poli) | `Sent` | Catatan klinis dibekukan, notifikasi serah terima terbit di bangsal tujuan. |
| `Sent` | Perawat Bangsal klik `[Tolak]` dengan alasan | Perawat Bangsal Ranap | `Rejected` | Wajib alasan penolakan min. 10 karakter; unit asal menerima pemberitahuan. |
| `Sent` | Perawat Bangsal menetapkan PPJA dan klik `[Terima Pasien]` | Perawat Bangsal Ranap | `Accepted` | `AssignedNurseId` wajib terisi; akun penerima != pengirim; bed terkonfirmasi. |

### B. Dokumen Serah Terima Admisi PPRI (`InpAdmissionDocument` - Jenis `NewPatientHandover`)

| Status Awal | Aksi / Trigger | Pelaku | Status Akhir | Invariant / Syarat |
|---|---|---|---|---|
| `Draft` | Petugas Admisi melengkapi 15 butir checklist dan klik `[Kunci Dokumen]` | Petugas Admisi | `AwaitingSignature` | Seluruh 15 butir terverifikasi (Sudah/Belum); data dikunci. |
| `AwaitingSignature` | Petugas Admisi menandatangani kolom Admisi | Petugas Admisi | `AwaitingSignature` | Kolom Admisi terisi tanda tangan & timestamp. |
| `AwaitingSignature` | CRO menandatangani kolom CRO | Petugas CRO | `AwaitingSignature` | Akun CRO berbeda dari akun Petugas Admisi. |
| `AwaitingSignature` | Perawat Ruangan menandatangani kolom Perawat | Perawat Ruangan | `Completed` | Pasien sudah menempati bed (`Occupied`); kolom Perawat terisi. |

---

## 4. Matriks Validasi (Validation Matrix)

| Kode Error | Kondisi Pemicu | HTTP Status | Pesan Error Standar |
|---|---|---|---|
| `CLI-TRH-001` | Menerima serah terima tanpa memilih Perawat PPJA | `422 Unprocessable Entity` | "Penetapan Perawat Penanggung Jawab Asuhan (PPJA) wajib diisi saat menerima pasien transfer." |
| `CLI-TRH-002` | Menerima serah terima menggunakan akun yang sama dengan pembuat/pengirim dari unit asal | `400 Bad Request` | "Penerima serah terima klinis tidak boleh akun yang sama dengan perawat pengirim." |
| `CLI-TRH-003` | Menolak serah terima tanpa menyertakan alasan minimal 10 karakter | `422 Unprocessable Entity` | "Alasan penolakan serah terima minimal 10 karakter." |
| `CLI-TRH-004` | Menerima serah terima yang statusnya bukan `Sent` (misal masih `Draft` atau sudah `Accepted`) | `409 Conflict` | "Dokumen serah terima klinis tidak dalam status siap diterima (Sent)." |
| `INP-ADM-005` | Perawat menandatangani dokumen serah terima PPRI saat pasien belum berstatus menempati bed | `422 Unprocessable Entity` | "Tanda tangan perawat ruangan hanya dapat dilakukan setelah pasien menempati tempat tidur." |
| `INP-ADM-006` | Menandatangani lebih dari satu kolom jabatan oleh satu pengguna yang sama | `400 Bad Request` | "Pengguna tidak dapat menandatangani lebih dari satu kolom jabatan pada dokumen yang sama." |

---

## 5. Spesifikasi Komponen Frontend & UI/UX

### A. Detail Episode Rawat Inap (`InpatientEpisodeDetailView.jsx`)

1. **Banner Serah Terima Klinis (`TransferHandoverBanner.jsx`)**:
   - Terpasang di bagian atas halaman Detail Episode jika terdapat `TransferHandover` berstatus `Sent`.
   - Warna: Amber / Alert Warning.
   - Konten: *"Serah Terima Pasien Masuk dari Instalasi Gawat Darurat (IGD) sedang menunggu verifikasi penerimaan ruangan."*
   - Aksi: Tombol `[Lihat Detail Serah Terima]` berikon drawer.
2. **Alert Toleransi Dokumen Admisi PPRI**:
   - Jika dokumen di Workspace PPRI belum `Completed`:
     - Warna: Info Alert (Biru/Kuning muda).
     - Konten: *"Dokumen admisi di Workspace PPRI masih dalam proses penyelesaian berkas. Layanan dan perawatan klinis pasien di bangsal tetap berjalan normal."*
     - Tombol: `[Buka Dokumen PPRI]` (membuka tab Workspace PPRI pada episode ini).
3. **Laci Serah Terima Klinis (`TransferHandoverDrawer.jsx`)**:
   - **Bagian 1: Header & Kondisi Pasien**:
     - No RM, Nama Pasien, Unit Asal (IGD/Poli), Dokter Pengirim, Waktu Pengiriman.
   - **Bagian 2: Tanda-Tanda Vital (TTV) & Kondisi Fisik**:
     - Tekanan Darah (TD), Nadi, Laju Nafas (RR), Suhu Tubuh, Saturasi O2 (SpO2), Skala Nyeri (0-10), Tingkat Kesadaran (GCS / Compos Mentis).
   - **Bagian 3: Alat Medis Terpasang**:
     - Daftar alat aktif: IV Line / Infus (lokasi & jenis cairan), Kateter Urine (ukuran & volume), Selang Oksigen (lpm), NGT, Drain.
   - **Bagian 4: Rekonsiliasi Obat & Riwayat Alergi**:
     - Badge peringatan alergi pasien (warna merah tegas).
     - Tabel Terapi Obat dari Unit Asal: Nama Obat, Bentuk/Rute, Dosis Terakhir Diberikan, Waktu Pemberian, Rekomendasi Terapi Bangsal (Lanjut / Stop / Konfirmasi DPJP).
   - **Bagian 5: Penetapan Perawat PPJA & Konfirmasi**:
     - Form Dropdown: `Perawat Penanggung Jawab Asuhan (PPJA) *` (Menampilkan daftar perawat bertugas di unit ruangan tersebut).
     - Kolom Catatan Penerimaan (Opsional).
     - Tombol Aksi:
       - `[Tolak / Minta Klarifikasi]` (Varian Secondary/Danger Outline, membuka modal alasan).
       - `[Terima Pasien]` (Varian Primary, hanya aktif jika PPJA sudah dipilih).

---

### B. Workspace PPRI (`AdmissionWorkspaceClient.jsx`)

1. **Sub-Menu Gelang & Label (`wristband-and-label`)**:
   - Menampilkan pratinjau cetak Gelang Pasien Dewasa / Anak (warna biru/merah muda) memuat: No RM, Nama Lengkap Pasien, Tanggal Lahir, Jenis Kelamin, Nomor Kamar & Bed, Barcode 1D / QR Code.
   - Tombol `[Cetak Gelang Ranap Resmi]` dan `[Cetak Label Medis (Stiker)]`.
   - Audit trail: Mencatat siapa yang mencetak dan waktu cetak gelang.
2. **Sub-Menu Formulir Serah Terima Pasien Baru (`new-patient-handover`)**:
   - Checklist 15 butir administrasi:
     1. Surat Pengantar Rawat Inap / Disposisi Transfer.
     2. Identitas Diri (KTP/SIM/Paspor/KK).
     3. Kartu Asuransi / Bukti Kelayakan BPJS (SEP).
     4. General Consent bertanda tangan.
     5. Gelang Pengenal Pasien Resmi telah terpasang.
     6. Surat Pernyataan Selisih Biaya (bila naik kelas).
     7. Bukti Pelunasan / Pembayaran Uang Muka (Deposit).
     8. Catatan Edukasi Pasien Terintegrasi awal.
     9. Dokumen Penunjang Medis (Laboratorium, Radiologi dari IGD/Poli).
     10. Berkas Rekam Medis Lama (bila ada).
     11. Kartu Tunggu Pasien Rawat Inap.
     12. Inventaris Barang Bawaan Pasien (bila ada titipan).
     13. Formulir Informasi Hak dan Kewajiban Pasien.
     14. Formulir Tata Tertib Rawat Inap.
     15. Kesiapan Tempat Tidur dan Fasilitas Ruangan.
   - Kolom Tanda Tangan:
     - Tanda tangan Petugas Admisi.
     - Tanda tangan CRO (Customer Relations Officer).
     - Tanda tangan Perawat Ruangan (aktif saat pasien sudah berstatus menempati bed).
