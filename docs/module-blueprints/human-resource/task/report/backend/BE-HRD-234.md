# Laporan Penyelesaian Task: BE-HRD-234 Layanan Diskresi Penyesuaian Tier Rekredensialing & Audit Rekomendasi Komite Medik

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-234`
* **Nama Task**: Pembangunan Layanan Diskresi Otoritas Penyesuaian Tier Kelayakan Rekredensialing oleh Ketua Komite Medik, Validasi Catatan Justifikasi Klinis, dan Perekaman Audit
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE` (Kontrak API `v19`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/RecredentialingQualityDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/IRecredentialingCompositeProvider.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/RecredentialingQualityService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/RecredentialingQualityController.cs`

---

## 2. Latar Belakang & Wewenang Diskresi Klinis Ketua Komite
Meskipun sistem komputasi menghasilkan rekomendasi tier eligibilitas secara objektif dan deterministik (Tier Hijau, Kuning, Merah), dunia kedokteran rumah sakit mengenal prinsip diskresi pertimbangan klinis (*clinical judgment*) oleh Ketua Komite Medik:
1. **Hak Diskresi Bersyarat (`VR-OPP-19`, `AT-HRD-OPP-21`)**:
   - Ketua Komite Medik memiliki kewenangan sah untuk melakukan penyesuaian tier (misalnya menaikkan dokter dari Tier Kuning ke Tier Hijau jika keterlambatan dokumentasi terbukti akibat beban darurat bencana atau situasi khusus yang dapat diterima).
   - Penyesuaian tier secara sepihak tanpa dasar pertimbangan dilarang keras: Sistem memvalidasi kewajiban input nota justifikasi klinis tertulis **minimal 20 karakter**. Permintaan override tanpa justifikasi ditolak sistem dengan galat `400 Bad Request`.
2. **Perekaman Jejak Audit Permanen (`RecredentialingTierOverridden`)**:
   - Setiap aksi penyesuaian tier dicatat secara permanen ke jejak audit sistem: mencakup identitas dokter target, tier asal, tier tujuan baru, identitas Ketua Komite, alasan klinis, dan stempel waktu presisi.
   - Dokumen telaah rekredensialing akan menampilkan label `IsOverridden = TRUE` beserta nota pertimbangan Ketua Komite untuk menjamin transparansi akreditasi KARS STARKES.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Recredentialing Quality & Composite Scorecard")]`
**Base Route**: `/api/v1/corporate/hrd/credentialing/recredentialing`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/credentialing/recredentialing/scorecards/{staffId}/override-tier` | Penyesuaian diskresi tier kelayakan oleh Ketua Komite Medik dengan justifikasi tertulis. | `[Authorize]` Ketua Komite Medik | `OverrideRecredTierRequestDto` | `200 OK` (`ApiResponse<RecredentialingScorecardDto>`) |

---

## 4. Contoh Konkret Skenario Rumah Sakit

### Skenario: Penyesuaian Diskresi dari Tier Kuning ke Tier Hijau
* **Latar Belakang**: dr. Robert Sp.BS memiliki komposit 78.50 (Tier Kuning akibat keterlambatan resume medis saat penanganan tanggap darurat bencana gempa bumi).
* **Endpoint**: `POST /api/v1/corporate/hrd/credentialing/recredentialing/scorecards/e1f2a3b4-5678-90ab-cdef-1234567890ab/override-tier`
* **Request Body**:
```json
{
  "newTier": "GreenFastTrack",
  "justificationReason": "Keterlambatan resume medis terjustifikasi secara resmi oleh Direktur Pelayanan akibat penugasan operasi darurat korban bencana massal. Kompetensi teknik bedah terbukti prima dan nihil komplikasi.",
  "approvedByCommitteeChairmanId": "f9a8b7c6-d5e4-3a2b-1c0d-ef9876543210"
}
```
* **Response Body (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Diskresi penyesuaian tier kelayakan berhasil diterapkan dan dicatat ke log audit.",
  "data": {
    "workforceProfileId": "e1f2a3b4-5678-90ab-cdef-1234567890ab",
    "doctorName": "dr. Robert Sp.BS",
    "doctorCode": "DOC-BS-009",
    "finalCompositeScore": 78.5,
    "calculatedTier": "YellowClarificationReview",
    "assignedTier": "GreenFastTrack",
    "tierDescription": "Tier Hijau - Rekomendasi Jalur Cepat (Fast-Track) Perpanjangan SPK Tanpa Hambatan",
    "isOverridden": true,
    "tierOverrideReason": "Keterlambatan resume medis terjustifikasi secara resmi oleh Direktur Pelayanan akibat penugasan operasi darurat korban bencana massal. Kompetensi teknik bedah terbukti prima dan nihil komplikasi.",
    "overriddenBy": "f9a8b7c6-d5e4-3a2b-1c0d-ef9876543210",
    "overriddenByName": "dr. Hendra Sp.BS(K)",
    "overriddenAt": "2026-11-09T14:56:00Z"
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Kewajiban Catatan Justifikasi Klinis (`VR-OPP-19`, `AT-HRD-OPP-21`)**: ✅ Lulus. Permintaan diskresi tanpa alasan $\ge 20$ karakter ditolak seketika; justifikasi sah berhasil mengubah `AssignedTier`.
2. **Perekaman Jejak Audit Diskresi**: ✅ Lulus. Rincian pengubah, waktu, dan alasan tersimpan permanen pada metadata scorecard.


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok C — data penting hanya di memori |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | 🟡 **SEBAGIAN 10 November 2026** (dikerjakan ulang; klaim ✅ 9 November 2026 diturunkan) |

### Masalah yang ditemukan audit

Penyesuaian tier oleh Ketua Komite Medik (jejak audit akreditasi) disimpan di memori proses dan hilang saat restart.

### Perubahan yang dikerjakan

- Tabel baru `HrdRecredentialingTierOverride`: menyimpan tier hasil kalkulasi, tier baru, justifikasi, ketua komite, dan waktu. Satu baris aktif per dokter; penyesuaian berikutnya menonaktifkan yang lama tanpa menghapus riwayat.
- Rapor membaca penyesuaian aktif dari database.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/PerformanceManagement/Services/RecredentialingQualityService.cs`
- `Areas/Corporate/HumanResource/PerformanceManagement/Models/HrdRecredentialingTierOverride.cs` (baru)
- `Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/HrdRecredentialingTierOverrideConfiguration.cs` (baru)
- `Repositories/ApplicationDbContext.cs`
- `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs`

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini) | 0 error | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS** | 0 temuan | `PASS` |
| Review diff dan scope | hanya berkas di atas yang disentuh untuk task ini | `PASS` |
| Uji runtime / API | tidak dijalankan pada sesi ini | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria yang disentuh pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Justifikasi minimal 20 karakter dan otorisasi Ketua Komite (VR-OPP-19) | Terpenuhi | validasi tidak berubah |
| Jejak audit penyesuaian tier tersimpan permanen | Terpenuhi di source | tabel baru |

### Yang belum terpenuhi

- Migration belum dijalankan; `AT-HRD-OPP-21` `NOT RUN`.

### Database dan migration

Migration SQL idempoten `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs` disiapkan; **belum dijalankan** ke database mana pun (wewenang pemilik, `dotnet ef database update`). Snapshot model EF tidak dipakai sebagai dasar migration ini karena sudah lama tidak sinkron dengan migration SQL manual slice sebelumnya (lihat catatan risiko).

### Catatan penutup

- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).
