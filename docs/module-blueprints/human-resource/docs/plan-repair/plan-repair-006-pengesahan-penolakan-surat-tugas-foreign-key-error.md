# PLAN-REPAIR-006 — Resolusi Pelanggaran Kunci Asing ApproverWorkforceProfileId pada Pengesahan dan Penolakan Surat Tugas

```yaml
plan_id: PLAN-REPAIR-HRD-006
issue: ../issue/issue-006-pengesahan-penolakan-surat-tugas-foreign-key-error.md
status_rencana: SELESAI
tanggal_rencana: "2026-11-16"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-11-16"
basis_source_backend: "80c94e8e (MHamzah)"
basis_source_frontend: "f5a0717bd (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-HRD-006-01` | `ISS-HRD-006-01` | Hapus fallback ke `currentUser?.id` pada fungsi `handleExecuteAction` pengesahan/penolakan surat tugas | FE | 1 | — | SELESAI | `official-duties-view.jsx:268` diubah menjadi `currentUser?.workforceProfileId \|\| undefined`, ESLint 0 error |
| `FIX-HRD-006-02` | `ISS-HRD-006-02` | Tambahkan validasi & resolusi profil pegawai pada `ApproveAsync` & `RejectAsync` di service backend | BE | 1 | — | SELESAI | `OfficialDutyRequestService.cs:324` & `466` memvalidasi `MstWorkforceProfile`, memasang `null` bila approver SuperAdmin |
| `FIX-HRD-006-03` | `ISS-HRD-006-03` | Jadikan `ApproverWorkforceProfileId` opsional (*nullable*) pada DTO Approve & Reject dinas luar | BE | 1 | — | SELESAI | `OfficialDutyRequestDtos.cs:70, 79` properti diubah menjadi `Guid?` tanpa atribut `[Required]` |

**Ringkasan: 3 dari 3 perbaikan selesai (100%).**

---

## 2. Solusi Terpilih per Temuan

### ISS-HRD-006-01, ISS-HRD-006-02, & ISS-HRD-006-03 — Pelanggaran Kunci Asing saat Sahkan / Tolak Surat Tugas

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | 1. Di Frontend: Hapus `|| currentUser?.id` sehingga hanya mengirim `workforceProfileId` jika benar-benar ada.<br>2. Di Backend DTO: Jadikan `ApproverWorkforceProfileId` bertipe `Guid?` (opsional).<br>3. Di Backend Service: Validasi keberadaan GUID di `MstWorkforceProfile`. Jika valid, pasang ke `ApprovedByWorkforceId`. Jika tidak (misal akun SuperAdmin), pasang `null` dan tetap catat audit di `UpdateBy = currentUserId`. | Memperbaiki kedua sisi secara kokoh (*defense-in-depth*), mendukung akun SuperAdmin/admin tanpa profil tenaga kerja, menjaga integritas kunci asing database PostgreSQL, dan meniadakan galat HTTP 500. | Memerlukan perubahan kecil pada DTO, service backend, dan berkas tampilan frontend. |
| B | Hanya mengubah frontend agar tidak mengirim apa pun jika bukan karyawan. | Paling sedikit perubahan kodenya. | Backend tetap rentan jika dipanggil oleh klien API lain, dan DTO tetap melempar 400 Bad Request jika `ApproverWorkforceProfileId` wajib diisi. |
| C | Membuat profil tenaga kerja dummy untuk setiap akun admin sistem di database. | Tidak mengubah kode backend. | Mengotori master data kepegawaian rumah sakit dengan data semu yang melanggar aturan tata kelola SDM. |

**Solusi terpilih: Opsi A.**
Solusi ini menyelesaikan akar masalah secara tuntas di lapisan DTO, Service, dan Antarmuka tanpa kompromi integritas data.

---

## 3. Skema Tampilan Sebelum → Sesudah

### Alur Tindakan Modal Konfirmasi Sahkan / Tolak

```text
SEBELUM:
[ Klik Tombol "Sahkan" / "Tolak" ]
           │
           ▼
[ Modal Konfirmasi Tindakan ]
Catatan: [ TEST                       ]
[ Batal ] [ Ya, Sahkan Surat Tugas    ] ──► Klik
           │
           ▼
Frontend mengambil: approverProfileId = currentUser?.workforceProfileId || currentUser?.id
           │
           ▼
Kirim payload: { approverWorkforceProfileId: "0ba84a1a-2559-49ba-a320-10fb1f399d70", ... }
           │
           ▼
Backend langsung pasang ke ApprovedByWorkforceId tanpa validasi
           │
           ▼
Basis Data PostgreSQL: ERROR 23503 Foreign Key Violation!
           │
           ▼
Tampilan: Banner Merah "Gagal memproses tindakan pengesahan" (HTTP 500 Internal Server Error)


SESUDAH:
[ Klik Tombol "Sahkan" / "Tolak" ]
           │
           ▼
[ Modal Konfirmasi Tindakan ]
Catatan: [ Disahkan sesuai disposisi  ]
[ Batal ] [ Ya, Sahkan Surat Tugas    ] ──► Klik
           │
           ▼
Frontend mengambil: approverProfileId = currentUser?.workforceProfileId || null
           │
           ▼
Kirim payload: { approverWorkforceProfileId: null, approvalNote: "..." }
           │
           ▼
Backend OfficialDutyRequestService:
- Jika approverProfileId valid di MstWorkforceProfile -> simpan ke ApprovedByWorkforceId
- Jika null / akun SuperAdmin -> ApprovedByWorkforceId = null, UpdateBy = currentUserId
           │
           ▼
Basis Data PostgreSQL: UPDATE BERHASIL (Status: Approved/Rejected)
           │
           ▼
Tampilan: Banner Hijau "Surat tugas SPT/RSQ-SDM/... berhasil disahkan secara resmi."
Data berpindah dari "Antrean Pengesahan" ke status "Disahkan & Terbit".
```

---

## 4. Rincian Perbaikan

### FIX-HRD-006-01 — Hapus fallback ke `currentUser?.id` pada fungsi `handleExecuteAction` di `official-duties-view.jsx`

| | |
| :--- | :--- |
| **Menutup** | `ISS-HRD-006-01` |
| **Area** | Frontend View |
| **Jenis Perubahan** | Refactoring handler aksi |
| **Berkas yang Diubah** | `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/official-duties-view.jsx` |
| **Radius Dampak** | Modal Sahkan dan Tolak pada layar Surat Tugas Resmi |
| **Bergantung pada** | — |

**Langkah Pengerjaan:**
1. Pada baris 268 berkas `official-duties-view.jsx`, ubah:
   ```javascript
   // Sebelum:
   const approverProfileId = currentUser?.workforceProfileId || currentUser?.id;
   
   // Sesudah:
   const approverProfileId = currentUser?.workforceProfileId || null;
   ```
2. Pastikan payload yang dikirimkan ke `OfficialDutyService.approveDuty` dan `OfficialDutyService.rejectDuty` hanya menyertakan `approverWorkforceProfileId` bila nilainya memang merupakan ID profil tenaga kerja yang valid.

**Acceptance Criteria:**
1. Payload request `approve` dan `reject` tidak lagi menyertakan ID akun login `AspNetUsers` sebagai `approverWorkforceProfileId`.
2. Jika pengguna login adalah SuperAdmin (tanpa profil tenaga kerja), properti `approverWorkforceProfileId` dikirim `null` atau dihilangkan.

---

### FIX-HRD-006-02 — Tambahkan validasi & resolusi profil pegawai pada `ApproveAsync` & `RejectAsync` di service backend

| | |
| :--- | :--- |
| **Menutup** | `ISS-HRD-006-02` |
| **Area** | Backend Service |
| **Jenis Perubahan** | Pengamanan integritas referensial data |
| **Berkas yang Diubah** | `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/AttendanceManagement/Services/OfficialDutyRequestService.cs` |
| **Radius Dampak** | Endpoint pengesahan dan penolakan tugas dinas luar |
| **Bergantung pada** | `FIX-HRD-006-03` |

**Langkah Pengerjaan:**
1. Buat fungsi bantu privat atau logika pengecekan di dalam `ApproveAsync` dan `RejectAsync`:
   ```csharp
   Guid? approverWfpId = null;
   if (request.ApproverWorkforceProfileId.HasValue && request.ApproverWorkforceProfileId.Value != Guid.Empty)
   {
       var exists = await _context.Set<MstWorkforceProfile>().AsNoTracking()
           .AnyAsync(w => w.Id == request.ApproverWorkforceProfileId.Value && !w.IsDelete, cancellationToken);
       if (exists)
       {
           approverWfpId = request.ApproverWorkforceProfileId.Value;
       }
   }

   if (!approverWfpId.HasValue)
   {
       var linkedWfpId = await _context.Users.AsNoTracking()
           .Where(u => u.Id == currentUserId && u.WorkforceProfileId.HasValue)
           .Select(u => u.WorkforceProfileId)
           .FirstOrDefaultAsync(cancellationToken);

       if (linkedWfpId.HasValue && await _context.Set<MstWorkforceProfile>().AsNoTracking().AnyAsync(w => w.Id == linkedWfpId.Value && !w.IsDelete, cancellationToken))
       {
           approverWfpId = linkedWfpId.Value;
       }
   }

   entity.ApprovedByWorkforceId = approverWfpId;
   ```
2. Terapkan logika yang sama pada metode `RejectAsync` (baris 442).
3. Simpan perubahan dengan `await _context.SaveChangesAsync(cancellationToken);`.

**Acceptance Criteria:**
1. Jika `request.ApproverWorkforceProfileId` dikirim sembarangan/tidak terdaftar di `MstWorkforceProfile`, backend tidak lagi melempar `23503 foreign key violation`.
2. Pengesahan oleh akun SuperAdmin berhasil dengan status HTTP 200 OK.
3. Transaksi kehadiran `HrdAttendanceDaily` (status `OnDuty`) terbentuk secara tepat untuk pegawai yang ditugaskan.

---

### FIX-HRD-006-03 — Jadikan `ApproverWorkforceProfileId` opsional (*nullable*) pada DTO Approve & Reject dinas luar

| | |
| :--- | :--- |
| **Menutup** | `ISS-HRD-006-03` |
| **Area** | Backend DTO |
| **Jenis Perubahan** | Pelonggaran validasi kontrak DTO |
| **Berkas yang Diubah** | `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/AttendanceManagement/DTOs/OfficialDutyRequestDtos.cs` |
| **Radius Dampak** | Kontrak API Swagger & model binding controller |
| **Bergantung pada** | — |

**Langkah Pengerjaan:**
1. Pada `ApproveOfficialDutyRequest` (baris 70):
   ```csharp
   // Sebelum:
   [Required]
   public Guid ApproverWorkforceProfileId { get; set; }

   // Sesudah:
   public Guid? ApproverWorkforceProfileId { get; set; }
   ```
2. Pada `RejectOfficialDutyRequest` (baris 79):
   ```csharp
   // Sebelum:
   [Required]
   public Guid ApproverWorkforceProfileId { get; set; }

   // Sesudah:
   public Guid? ApproverWorkforceProfileId { get; set; }
   ```

**Acceptance Criteria:**
1. Request JSON tanpa `approverWorkforceProfileId` atau bernilai `null` lolos validasi model (*ModelState.IsValid*).

---

## 5. Urutan Pengerjaan

```text
FIX-HRD-006-03 (Pelonggaran DTO backend menjadi nullable)
     │
     ▼
FIX-HRD-006-02 (Penerapan validasi & fallback profil pada service backend)
     │
     ▼
FIX-HRD-006-01 (Pembersihan fallback auth user id pada frontend view)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Status | Dampak pada Rencana |
| :---: | :--- | :---: | :--- |
| K-01 | Pengesahan dinas oleh SuperAdmin/Admin sistem diizinkan menyimpan `ApprovedByWorkforceId = null` dengan audit jejak `UpdateBy = currentUserId`. | Direkomendasikan (Pilihan A) | Menjadi dasar implementasi `FIX-HRD-006-02` dan `FIX-HRD-006-03`. |

---

## 7. Dokumen Hulu yang Ikut Direvisi

Tidak ada perubahan arsitektur data atau alur bisnis hulu. Kolom database `ApprovedByWorkforceId` sejak awal sudah dirancang bersifat *nullable* (`Guid?`).

---

## 8. Verifikasi Menyeluruh

1. Buka menu **Surat Tugas Resmi** (`/hr/workforce-core/official-duties`).
2. Masuk ke tab **Antrean Pengesahan**:
   - Temukan surat tugas yang berstatus *"Menunggu Pengesahan"*.
3. Klik tombol hijau **Sahkan**:
   - Masukkan catatan persetujuan: *"Disahkan untuk tugas dinas pelatihan luar kota."*
   - Tekan tombol **Ya, Sahkan Surat Tugas**.
   - **Verifikasi**: Tidak ada galat HTTP 500. Muncul notifikasi sukses warna hijau *"Surat tugas SPT/... berhasil disahkan secara resmi"*.
   - Surat tugas berpindah status menjadi *"Disahkan (Resmi)"*.
4. Uji kasus penolakan (**Tolak**):
   - Buat satu surat tugas pengujian baru.
   - Klik tombol merah **Tolak**, isi alasan penolakan *"Agenda dibatalkan oleh panitia penyelenggara"*, lalu konfirmasi.
   - **Verifikasi**: Berhasil ditolak tanpa galat, status berubah menjadi *"Ditolak"*.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | :--- | :---: |
| 2026-11-16 | Dokumen rencana perbaikan disusun dengan 3 butir perbaikan terkoordinasi (Status: MENUNGGU_PERSETUJUAN). | `diagnose-module-issue` |
| 2026-11-16 | Disetujui oleh Pemilik Sistem (K-01 Pilihan A). Seluruh butir perbaikan (FIX-01 s/d FIX-03) telah selesai diimplementasikan di backend dan frontend. Status: SELESAI. | Antigravity AI |
