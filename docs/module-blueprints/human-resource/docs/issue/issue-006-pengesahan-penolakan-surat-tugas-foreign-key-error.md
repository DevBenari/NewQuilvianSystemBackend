# ISSUE-006 — Pengesahan dan Penolakan Surat Tugas Gagal HTTP 500 Akibat Pelanggaran Foreign Key ApproverWorkforceProfileId

```yaml
issue_id: ISSUE-HRD-006
module_id: human-resource
submodule: null   # blueprint berbentuk SINGLE (HRD-DEC-003); blueprint root = human-resource/
layar: "Administrasi Kepegawaian — Surat Tugas Resmi (/hr/workforce-core/official-duties)"
sumber_laporan: "Laporan pengguna 16-11-2026: Keluhan galat saat klik Sahkan (Approve) & Tolak (Reject) + screenshot antrean pengesahan + stack trace Npgsql 23503 DbUpdateException"
tanggal_issue: "2026-11-16"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "80c94e8e (MHamzah)"
source_sha_frontend: "f5a0717bd (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-006-pengesahan-penolakan-surat-tugas-foreign-key-error.md
```

---

## 1. Ringkasan

Petugas/admin rumah sakit membuka menu **Surat Tugas Resmi** (`/hr/workforce-core/official-duties`), beralih ke tab **Antrean Pengesahan**, lalu menekan tombol **Sahkan** (atau **Tolak**) pada surat perintah tugas dinas yang diajukan. Setelah mengisi catatan dan menekan konfirmasi, aplikasi memunculkan pesan galat *"Gagal memproses tindakan pengesahan"* dan server mengembalikan respons **HTTP 500 Internal Server Error**.

Pemeriksaan log backend membuktikan bahwa Entity Framework Core melempar `DbUpdateException` dengan penyebab PostgreSQL `23503 foreign_key_violation`: pembaruan baris pada tabel `public."HrdOfficialDutyRequest"` melanggar batasan kunci asing `FK_HrdOfficialDutyRequest_MstWorkforceProfile_ApprovedByWorkfo~`.

Penyebab langsungnya adalah kekeliruan fallback identitas pada frontend: saat menyiapkan payload pengesahan, kode mengambil `currentUser?.workforceProfileId || currentUser?.id`. Akun administratif (seperti SuperAdmin) tidak memiliki baris profil pada tabel master pegawai (`MstWorkforceProfile`), sehingga nilai yang terkirim ke backend adalah User ID akun login (`AspNetUsers.Id`: `0ba84a1a-2559-49ba-a320-10fb1f399d70`). Di sisi backend, service langsung menugaskan nilai tersebut ke kolom `ApprovedByWorkforceId` tanpa validasi keberadaan master, memicu penolakan keras oleh basis data PostgreSQL. Masalah ini berkategori **High Severity** karena memblokir alur persetujuan surat tugas kedinasan rumah sakit.

---

## 2. Laporan Asli

| No. laporan | Keluhan (kata-kata pelapor, diringkas) | Lampiran |
| :---: | :--- | :--- |
| 1 | "Terjadi Eror Pada Saat melakukan klik approva dan di tolak , lakukan /diagnose-module-issue" | Tangkapan layar layar `Surat Tugas Resmi Kepegawaian` (16-11-2026), antrean 1 data status *Menunggu Pengesahan* dengan tombol aksi hijau *Sahkan* dan merah *Tolak*. |
| 2 | "Request URL: POST https://localhost:7184/api/v1/corporate/human-resource/attendance-management/official-duties/2e6073ed-09f8-495e-a500-fd6d29cf08c4/approve — Status code: 500 Internal Server Error — Payload: {approverWorkforceProfileId: '0ba84a1a-2559-49ba-a320-10fb1f399d70', approvalNote: 'TEST'}" | Salinan lengkap payload request JSON dan respons server HTTP 500. |
| 3 | "Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details. ---> Npgsql.PostgresException (0x80004005): 23503: insert or update on table 'HrdOfficialDutyRequest' violates foreign key constraint 'FK_HrdOfficialDutyRequest_MstWorkforceProfile_ApprovedByWorkfo~'" | Log konsol backend ASP.NET Core dan jejak panggilan (*stack trace*) di `OfficialDutyRequestService.cs:line 402`. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| :--- | :---: | :--- | :---: | :---: | :---: | :---: | :--- |
| `ISS-HRD-006-01` | 1, 2 | Frontend mengirim User ID Auth sebagai ApproverWorkforceProfileId saat klik Sahkan/Tolak | `BUG` | Frontend | High | SUDAH-VERIFIKASI | `FIX-HRD-006-01` |
| `ISS-HRD-006-02` | 2, 3 | Backend ApproveAsync & RejectAsync tidak memvalidasi keberadaan Profil Pegawai sebelum menugaskan Foreign Key | `BUG` | Backend Service | High | SUDAH-VERIFIKASI | `FIX-HRD-006-02` |
| `ISS-HRD-006-03` | 2, 3 | DTO Approve & Reject mewajibkan ApproverWorkforceProfileId padahal Approver adalah User Login | `CONTRACT_MISMATCH` | Backend DTO | Medium | SUDAH-VERIFIKASI | `FIX-HRD-006-03` |

---

## 4. Rincian per Temuan

### ISS-HRD-006-01 — Frontend mengirim User ID Auth sebagai ApproverWorkforceProfileId saat klik Sahkan/Tolak

| | |
| :--- | :--- |
| **No. laporan** | 1, 2 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/official-duties-view.jsx:268` |
| **Perbaikan** | `FIX-HRD-006-01` |

**Apa yang terjadi.** Petugas yang memiliki hak otorisasi/superadmin membuka modal pengesahan atau penolakan surat tugas dinas, mengisi catatan penugasan, lalu menekan tombol konfirmasi. Alih-alih surat berhasil disahkan, sistem memunculkan galat merah dan surat tetap tertahan di antrean *Menunggu Pengesahan*.

**Kenapa terjadi.** Pada berkas `official-duties-view.jsx` baris 268:
```javascript
// QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/official-duties-view.jsx:268
const approverProfileId = currentUser?.workforceProfileId || currentUser?.id;
if (actionType === "approve") {
  await OfficialDutyService.approveDuty(actionItem.id, {
    approverWorkforceProfileId: approverProfileId,
    approvalNote: actionNote.trim() || "Disahkan sesuai arahan dinas resmi.",
  });
...
```
Ketika pengguna yang login adalah akun administrator sistem (seperti `superadmin`), nilai `currentUser.workforceProfileId` kosong/undefined. Frontend keliru melakukan fallback ke `currentUser.id` yang merupakan ID tabel otentikasi `AspNetUsers` (`0ba84a1a-2559-49ba-a320-10fb1f399d70`), bukan ID tabel master pegawai `MstWorkforceProfile`.

**Apakah ini menyimpang dari desain?** `BUG`. Sesuai relasi arsitektur data, User ID otentikasi tidak pernah dapat dipertukarkan dengan ID profil tenaga kerja kepegawaian.

**Dampak nyata.** Pengguna dengan peran SuperAdmin atau Manajer HR yang tidak memiliki data profil dokter/karyawan tersendiri tidak dapat menyetujui atau menolak permohonan dinas luar apa pun di rumah sakit.

**Rekomendasi.** Di frontend, jangan pernah melakukan fallback ke `currentUser?.id`. Jika `currentUser.workforceProfileId` tidak ada, kirim `null` atau `undefined`, sehingga backend menangani resolusi identitas otorisasi akun pengguna secara benar.

---

### ISS-HRD-006-02 — Backend ApproveAsync & RejectAsync tidak memvalidasi keberadaan Profil Pegawai sebelum menugaskan Foreign Key

| | |
| :--- | :--- |
| **No. laporan** | 2, 3 |
| **Jenis** | `BUG` |
| **Area** | Backend Service |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/AttendanceManagement/Services/OfficialDutyRequestService.cs:324, 402, 442, 447` |
| **Perbaikan** | `FIX-HRD-006-02` |

**Apa yang terjadi.** Server mengembalikan HTTP 500 Internal Server Error yang tidak tertangani (*Unhandled Exception*) saat mengeksekusi endpoint `POST /official-duties/{id}/approve` dan `POST /official-duties/{id}/reject`.

**Kenapa terjadi.** Pada `OfficialDutyRequestService.cs`:
```csharp
// Baris 323-328 (ApproveAsync)
entity.Status = "Approved";
entity.ApprovedByWorkforceId = request.ApproverWorkforceProfileId;
entity.ApprovedAt = DateTime.UtcNow;
entity.UpdateBy = currentUserId;
entity.UpdateDateTime = DateTime.UtcNow;

// Baris 402:
await _context.SaveChangesAsync(cancellationToken);
```
Kolom `ApprovedByWorkforceId` pada entity `HrdOfficialDutyRequest` memiliki relasi kunci asing:
```csharp
[ForeignKey(nameof(ApprovedByWorkforceId))]
public MstWorkforceProfile? ApprovedByWorkforce { get; set; }
```
Service menerima GUID dari DTO request dan langsung menugaskannya ke entity tanpa memeriksa apakah GUID tersebut benar-benar ada di tabel `MstWorkforceProfile`. Ketika basis data PostgreSQL mengeksekusi `UPDATE`, terjadi pelanggaran integritas referensial:
`23503: insert or update on table "HrdOfficialDutyRequest" violates foreign key constraint "FK_HrdOfficialDutyRequest_MstWorkforceProfile_ApprovedByWorkfo~"`.

Perlu dicatat bahwa kolom `ApprovedByWorkforceId` di model bertipe `Guid?` (*nullable*), yang berarti basis data memperbolehkan nilai `NULL` apabila pengesahan dilakukan oleh administrator tingkat sistem yang tidak memegang peran klinis/karyawan. Selain itu, audit jejak pengguna yang sebenarnya tetap tercatat aman di kolom `UpdateBy = currentUserId`.

**Apakah ini menyimpang dari desain?** `BUG`. Service backend wajib memiliki pertahanan terhadap integritas referensial basis data dan tidak boleh melempar unhandled database exception (HTTP 500) ke pengguna akhir.

**Dampak nyata.** Server mengalami kegagalan 500 dan transaksi kehadiran `HrdAttendanceDaily` tidak pernah terbentuk.

**Rekomendasi.** Di backend `OfficialDutyRequestService`, validasi apakah `request.ApproverWorkforceProfileId` sah terdaftar di `MstWorkforceProfile`. Jika tidak, coba resolusikan dari `UserAccount.WorkforceProfileId` milik `currentUserId`. Jika pengguna login adalah SuperAdmin/admin tanpa profil tenaga kerja, simpan `ApprovedByWorkforceId = null` (audit tetap tercatat di `UpdateBy`).

---

### ISS-HRD-006-03 — DTO Approve & Reject mewajibkan ApproverWorkforceProfileId padahal Approver adalah User Login

| | |
| :--- | :--- |
| **No. laporan** | 2, 3 |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Backend DTO |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/AttendanceManagement/DTOs/OfficialDutyRequestDtos.cs:70, 79` |
| **Perbaikan** | `FIX-HRD-006-03` |

**Apa yang terjadi.** Kontrak DTO `ApproveOfficialDutyRequest` dan `RejectOfficialDutyRequest` menandai ruas `ApproverWorkforceProfileId` dengan anotasi `[Required] public Guid ApproverWorkforceProfileId { get; set; }`.

**Kenapa terjadi.** Anotasi `[Required]` memaksa pemanggil mengirimkan GUID yang valid (bukan null/kosong). Akibatnya, frontend terpaksa melakukan fallback ke `currentUser.id` karena takut ditolak validasi DTO jika nilainya null.

Padahal, endpoint `[HttpPost("{id:guid}/approve")]` sudah terproteksi oleh `[Authorize]` dan controller sudah mengambil `userId` dari claim token JWT:
```csharp
// OfficialDutyRequestController.cs:76-77
if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
return ToActionResult(await _service.ApproveAsync(id, request, userId, cancellationToken));
```

**Apakah ini menyimpang dari desain?** `CONTRACT_MISMATCH`. Identitas pejabat yang mengesahkan surat tugas secara hierarki sistem berasal dari sesi otentikasi pengguna (`currentUserId`), sedangkan profil tenaga kerjanya bersifat komplementer (*optional*).

**Dampak nyata.** Memaksa frontend mengirimkan identitas palsu/auth GUID yang memicu kegagalan kunci asing di database.

**Rekomendasi.** Ubah properti DTO menjadi *nullable* `public Guid? ApproverWorkforceProfileId { get; set; }` tanpa atribut `[Required]`.

---

## 5. Tanya-Jawab Pelapor

> **T:** Mengapa klik tombol "Sahkan" (Approve) dan "Tolak" (Reject) menghasilkan pesan galat 500 "Terjadi kesalahan pada server"?
>
> **J:** Karena sistem mencoba menyimpan ID akun login Anda (`0ba84a1a-2559-49ba-a320-10fb1f399d70`) ke dalam kolom `ApprovedByWorkforceId` di database. Kolom tersebut memiliki aturan kunci asing (*Foreign Key*) yang mewajibkan nilainya terdaftar di tabel data master pegawai (`MstWorkforceProfile`). Karena akun login SuperAdmin Anda terdaftar di tabel akun (`AspNetUsers`) dan bukan di tabel pegawai, basis data PostgreSQL menolak pembaruan data tersebut demi menjaga integritas data rumah sakit.

---

## 6. Temuan Tambahan

Tidak ada temuan tambahan di luar alur pengesahan dan penolakan surat tugas resmi dinas luar.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor
Tidak ada pertanyaan penahan; bukti log backend dan stack trace yang diserahkan telah mengisolasi akar masalah secara 100% deterministik.

### Untuk Pemilik Sistem

| No | Keputusan | Pilihan | Rekomendasi | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :---: |
| K-01 | Apakah persetujuan oleh SuperAdmin (yang tidak memiliki profil tenaga kerja) diperbolehkan mengesahkan surat dinas luar? | **A.** Diperbolehkan: `ApprovedByWorkforceId` diset `null`, sedangkan `UpdateBy` mencatat User ID SuperAdmin.<br>**B.** Ditolak: SuperAdmin wajib menautkan dirinya ke salah satu profil pegawai sebelum dapat mengesahkan. | **Pilihan A**: Lebih fleksibel untuk administrasi rumah sakit dan menghindari kebuntuan otorisasi saat masa transisi atau pengujian. | `FIX-HRD-006-02` |

---

## 8. Catatan Pola

Masalah ini adalah akibat langsung dari pola anti-pattern:
`const approverProfileId = currentUser?.workforceProfileId || currentUser?.id;`
Pola ini mengasumsikan bahwa jika `workforceProfileId` kosong, maka `id` (User Auth ID) dapat dijadikan pengganti. Hal ini salah besar di seluruh ekosistem backend Quilvian karena:
1. `Users.Id` merujuk ke tabel otentikasi `AspNetUsers`.
2. `WorkforceProfileId` merujuk ke tabel domain kepegawaian `MstWorkforceProfile`.
3. Kolom-kolom bertipe `*WorkforceProfileId` di database memiliki Foreign Key ke `MstWorkforceProfile`, sehingga menyuntikkan User Auth ID ke dalamnya **pasti menghasilkan error 23503 foreign key violation**.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | :--- | :---: |
| 2026-11-16 | Dokumen issue dibuat dari laporan galat 500 DbUpdateException Foreign Key saat Sahkan/Tolak Surat Tugas. Status: TERBUKA. | `diagnose-module-issue` |
| 2026-11-16 | Perbaikan telah selesai diimplementasikan sesuai PLAN-REPAIR-HRD-006 (FIX-01 s/d FIX-03). Status issue ditutup: SELESAI. | Antigravity AI |
