# ISSUE-042 — Penolakan Konkurensi Palsu 409 INP-ADM-DOC-004 Saat Mengubah Konsep Dokumen Admisi Workspace PPRI

```yaml
issue_id: ISSUE-EPS-042
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Workspace PPRI — Dokumen Admisi Rawat Inap (Nilai Kepercayaan FE-INP-44 / FE-RWI-218 dan dokumen admisi sejenis)"
sumber_laporan: "Laporan pengguna 28-10-2026: keluhan selalu konflik HTTP 409 INP-ADM-DOC-004 pada PUT documents/{id}, capture Network tab, dan screenshot layar Nilai Kepercayaan"
tanggal_issue: "2026-10-28"
status: SELESAI
keparahan_tertinggi: Blocker
source_sha_backend: "c6b5d95071bed9d12ddb30cd0d81876be1324aaa (MHamzah)"
source_sha_frontend: "cde815cf455e51166803c24957c30ea227dcd696 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-042-konflik-stale-dokumen-admisi.md
```

---

## 1. Ringkasan

Issue ini menganalisis kegagalan penyimpanan konsep dokumen admisi pada Workspace PPRI (khususnya formulir Nilai Kepercayaan `FE-INP-44`, dan berpotensi pada Permintaan Privasi, Selisih Biaya, Pelunasan Deposit, serta Pihak Penanda Tangan). Setiap kali petugas menambahkan atau memperbarui butir nilai kepercayaan pada dokumen konsep yang sudah ada lalu menekan simpan, server selalu menolak dengan HTTP `409 Conflict`, kode `INP-ADM-DOC-004`, dan pesan *"Dokumen sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda."*. 

Penelusuran membuktikan bahwa masalah ini **bukan tabrakan data riil antarpetugas**, melainkan **penolakan konkurensi palsu (false concurrency conflict)** di backend. Akar masalahnya terletak pada mekanisme *change tracking* Entity Framework Core: ketika entitas anak baru (`InpAdmissionBeliefItem`, dan entitas anak sejenis) dibuat dengan primary key yang sudah terisi secara *inline* (`Id = Guid.NewGuid()`) lalu dimasukkan ke dalam koleksi navigasi dokumen yang sedang dilacak (`document.BeliefItems.Add(...)`), EF Core secara keliru menganggap entitas tersebut sebagai data lama yang dimodifikasi (`EntityState.Modified`) alih-alih data baru (`EntityState.Added`). Saat `SaveChangesAsync` dipanggil, EF Core mengeksekusi perintah SQL `UPDATE` yang menghasilkan 0 baris terpengaruh karena ID tersebut belum ada di database, memicu `DbUpdateConcurrencyException`, yang kemudian salah diterjemahkan menjadi kode data basi `INP-ADM-DOC-004`. Hal ini menyebabkan tombol *Muat Ulang* tidak pernah menyelesaikan masalah karena siklus kegagalan berulang terus-menerus.

---

## 2. Laporan asli

| No. laporan | Keluhan (kata-kata pelapor) | Lampiran yang merujuk |
| ---: | --- | --- |
| 1 | "terjadi masalah selalu konflik tolong hindari mengenai konflik ini tolong atasi masalah ya" | Request URL: `PUT https://localhost:7184/api/v1/health-services/inpatient-management/episodes/2bf607ae-cde3-4e24-9838-02249a8fcf7b/admission-workspace/documents/f063e0bb-4fb5-40bc-a4c5-62dc5405e78a`<br>Status: `409 Conflict`<br>Response Body: `{"success":false,"statusCode":409,"message":"Dokumen sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda.","errors":{"code":"INP-ADM-DOC-004"}}`<br>Screenshot: Layar Nilai Kepercayaan dengan spanduk peringatan kuning *"Data basi — Dokumen sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda. [Muat Ulang]"* dan status konsep *"Konsep Versi 1 · dibuat 28 Okt 2026, 11:14 oleh superadmin · Ada perubahan yang belum disimpan"*. |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | :---: | --- | --- | --- | :---: | --- | --- |
| `ISS-EPS-042-01` | 1 | Penolakan konkurensi palsu 409 `INP-ADM-DOC-004` saat menyimpan butir Nilai Kepercayaan | `BUG` | Backend | Blocker | `SUDAH-VERIFIKASI` di source backend & capture | `FIX-EPS-042-01` |
| `ISS-EPS-042-T1` | — | Kerentanan konkurensi palsu serupa pada penambahan `PrivacyEntries`, `Party`, `PrivacyRequest`, `CostDifferenceStatement`, dan `DepositStatement` | `BUG` | Backend | High | `SUDAH-VERIFIKASI` di source backend | `FIX-EPS-042-01` |

---

## 4. Rincian per temuan

### ISS-EPS-042-01 — Penolakan konkurensi palsu 409 INP-ADM-DOC-004 saat menyimpan butir Nilai Kepercayaan

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend (`InpAdmissionDocumentService.Content.cs` & `InpAdmissionDocumentService.cs`) |
| **Keparahan** | Blocker |
| **Status bukti** | `SUDAH-VERIFIKASI` di source kode backend dan capture network/layar pelapor |
| **Perbaikan** | `FIX-EPS-042-01` |

**Apa yang terjadi.**  
Petugas administrasi membuka menu *Nilai Kepercayaan* pada Workspace PPRI untuk episode rawat inap aktif. Petugas mengisi atau memperbarui data pihak penanda tangan serta memasukkan butir-butir kepercayaan pasien (misalnya butir ke-1 dan ke-2). Saat menekan tombol simpan, layar langsung menampilkan peringatan kuning bertuliskan *"Data basi — Dokumen sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda."*. Ketika petugas menekan tombol *Muat Ulang* dan mencoba menyimpan kembali, kesalahan yang sama selalu muncul berulang kali, sehingga dokumen konsep tidak pernah dapat disimpan maupun dilanjutkan ke tahap penandatanganan.

**Kenapa terjadi.**  
Rantai sebab penolakan konkurensi palsu ini ditelusuri dari peramban hingga lapisan akses data EF Core:

1. Frontend memanggil endpoint `PUT /admission-workspace/documents/{documentId}` dengan membawa `rowVersion` dokumen aktif saat ini (`"db58e1ec-f4bb-461c-8a80-d5ee842844d4"`), data pihak (`party`), dan daftar butir kepercayaan (`beliefItems: ["test", "test"]`).
2. Pada [InpAdmissionDocumentService.cs:377](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionDocumentService.cs#L377), method `UpdateAsync` memanggil `LoadForCommandAsync(episodeId, documentId, request.RowVersion, cancellationToken)`.
3. Pada [InpAdmissionDocumentService.cs:834-851](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionDocumentService.cs#L834-L851), dokumen dimuat dengan opsi `tracked: true`, sehingga entitas induk `InpAdmissionDocument` terdaftar di ChangeTracker EF Core dengan status `EntityState.Unchanged`.
4. Nilai `request.RowVersion` yang dikirim frontend cocok dengan `document.RowVersion` di database, sehingga penjaga versi di baris 885 lolos.
5. Pembaruan isi dokumen diproses oleh `ApplyContentAsync` yang kemudian memanggil `ApplyBelief` di [InpAdmissionDocumentService.Content.cs:602-624](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionDocumentService.Content.cs#L602-L624):
   ```csharp
   for (var index = 0; index < texts.Count; index++)
   {
       var itemNo = index + 1;
       var existing = document.BeliefItems.FirstOrDefault(x => x.ItemNo == itemNo);

       if (existing != null)
       {
           existing.Text = texts[index];
           existing.UpdateDateTime = now;
           existing.UpdateBy = actorUserId;
           continue;
       }

       document.BeliefItems.Add(new InpAdmissionBeliefItem
       {
           Id = Guid.NewGuid(),
           DocumentId = document.Id,
           ItemNo = itemNo,
           Text = texts[index],
           CreateDateTime = now,
           CreateBy = actorUserId
       });
   }
   ```
6. Model `InpAdmissionBeliefItem` di [InpAdmissionBeliefItem.cs:12](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Models/InpAdmissionBeliefItem.cs#L12) memiliki inisialisasi inline: `public Guid Id { get; set; } = Guid.NewGuid();`.
7. Saat instansi baru `InpAdmissionBeliefItem` ditambahkan ke koleksi relasi `document.BeliefItems.Add(...)`, EF Core ChangeTracker mendeteksi entitas baru tersebut melalui navigasi entitas induk yang berstatus ter-track (`Unchanged`). Karena primary key `Id` sudah terisi nilai non-default (`!= Guid.Empty`), EF Core ChangeTracker memperlakukannya sebagai entitas lama yang diubah (`EntityState.Modified`), bukan entitas baru yang perlu disisipkan (`EntityState.Added`).
8. Pada [InpAdmissionDocumentService.cs:420](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionDocumentService.cs#L420), dipanggil `SaveAsync(cancellationToken)` yang menjalankan `await _dbContext.SaveChangesAsync(cancellationToken);`.
9. Saat `SaveChangesAsync` dieksekusi, EF Core mengirimkan perintah SQL `UPDATE "InpAdmissionBeliefItem" SET ... WHERE "Id" = @id;`. Karena baris dengan ID baru tersebut belum pernah ada di tabel database, perintah UPDATE menghasilkan **0 baris terpengaruh**.
10. EF Core melempar exception `Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException`.
11. Blok penanganan exception pada [InpAdmissionDocumentService.cs:902-906](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionDocumentService.cs#L902-L906):
    ```csharp
    catch (DbUpdateConcurrencyException)
    {
        _dbContext.ChangeTracker.Clear();
        return Stale();
    }
    ```
    menangkap exception tersebut dan mengasumsikannya secara keliru sebagai tabrakan data basi konkurensi antarpetugas (`Stale()`), lalu mengembalikan HTTP `409 Conflict` dengan kode `INP-ADM-DOC-004`.

**Apakah ini menyimpang dari desain?**  
Ini adalah `BUG` implementasi backend. Kontrak konkurensi optimistik (`VAL-RWA-04` pada `contracts/validation-matrix.md`) bertujuan mendeteksi perubahan nyata oleh dua petugas berbeda yang mengedit versi yang sama secara bersamaan. Penolakan terhadap satu petugas tunggal yang sedang menyimpan butir baru adalah kegagalan mekanisme ORM EF Core, bukan perilaku yang diinginkan oleh desain.

**Dampak nyata.**  
Petugas admisi RS (misalnya petugas bernama "superadmin") yang sedang mendampingi pasien rawat inap Ny. Clarissa Putri Maheswari tidak dapat menyimpan butir-butir kepercayaan pasien ke dalam sistem. Alur admisi rawat inap terhenti di dokumen Nilai Kepercayaan karena dokumen tidak dapat beralih ke status dikunci (`AwaitingSignature`) maupun ditandatangani.

**Rekomendasi.**  
Pada `InpAdmissionDocumentService.Content.cs`, daftarkan setiap instansi `InpAdmissionBeliefItem` baru secara eksplisit ke DbContext dengan memanggil `_dbContext.Set<InpAdmissionBeliefItem>().Add(newItem);` sebelum atau saat dimasukkan ke dalam `document.BeliefItems`. Hal ini memastikan ChangeTracker EF Core menandai status entitas sebagai `EntityState.Added`, sehingga SQL yang dieksekusi adalah `INSERT` ke database, bukan `UPDATE`.

---

## 5. Tanya-jawab pelapor

> **T:** Mengapa sistem selalu memunculkan pesan konflik "Dokumen sudah diubah petugas lain" padahal saya bekerja sendirian di komputer lokal (localhost)?
>
> **J:** Pesan tersebut bukan karena ada orang lain yang mengubah dokumen. Pesan itu muncul karena ada kesalahan teknis di server backend saat menyimpan butir kepercayaan baru: server salah mengira data baru sebagai data lama yang harus di-update, sehingga saat database melaporkan data tersebut belum ada, server salah menyimpulkannya sebagai tabrakan data (data basi/stale) dan menampilkan pesan peringatan tersebut.

> **T:** Mengapa tombol "Muat Ulang" tidak menyelesaikan masalah dan pesan data basi tetap muncul saat disimpan kembali?
>
> **J:** Tombol "Muat Ulang" membaca data terakhir dari database. Karena proses penyimpanan sebelumnya selalu gagal di server, data di database sebenarnya tidak pernah berubah. Begitu Anda mengetikkan kembali butir kepercayaan yang baru dan menekan simpan, server kembali mengulangi kesalahan yang sama, sehingga masalah terus berulang tanpa henti.

---

## 6. Temuan tambahan

### ISS-EPS-042-T1 — Kerentanan konkurensi palsu serupa pada entitas anak lain di Workspace PPRI

| | |
| --- | --- |
| **Kenapa dicantumkan** | Ditemukan pola kode yang identik pada entitas anak dokumen admisi lainnya di `InpAdmissionDocumentService.Content.cs` yang belum dilindungi penambahan eksplisit ke DbContext |
| **Jenis** | `BUG` |
| **Area** | Backend (`InpAdmissionDocumentService.Content.cs`) |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code backend |
| **Perbaikan** | `FIX-EPS-042-01` |

**Apa yang terjadi.**  
Pola pembuatan entitas anak baru dengan ID `Guid.NewGuid()` tanpa pemanggilan `_dbContext.Set<T>().Add(...)` juga ditemukan pada:
1. `ApplyPartyAsync` baris 148–156: Pembuatan instansi `InpAdmissionDocumentParty` saat dokumen sebelumnya belum memiliki data pihak penanda tangan (`document.Party == null`).
2. `ApplyPrivacy` baris 522–529: Pembuatan instansi `InpAdmissionPrivacyRequest` saat dokumen sebelumnya belum memiliki objek permintaan privasi.
3. `SyncPrivacyEntries` baris 564–573: Penambahan baris baru kerabat yang diizinkan (`AllowedVisitor`) atau permintaan khusus (`SpecialServiceRequest`) pada formulir Permintaan Privasi.
4. `ApplyCostDifference` baris 668–675: Pembuatan instansi `InpAdmissionCostDifferenceStatement` baru.
5. `ApplyDeposit` baris 705–712: Pembuatan instansi `InpAdmissionDepositStatement` baru.

Jika formulir-formulir tersebut diedit dan disimpan saat konsep sudah ada di database, seluruh operasi tersebut akan mengalami `DbUpdateConcurrencyException` yang sama persis dan ditolak dengan 409 `INP-ADM-DOC-004`.

**Rekomendasi.**  
Perbaiki seluruh titik pembuatan entitas anak di `InpAdmissionDocumentService.Content.cs` sekaligus dalam satu perbaikan terpadu `FIX-EPS-042-01` dengan menambahkan `_dbContext.Set<T>().Add(...)`.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk pelapor

*Tidak ada pertanyaan untuk pelapor.* Bukti laporan URL, request payload, response body, kode error, dan screenshot layar sudah sangat lengkap dan memadai untuk mereproduksi serta membuktikan akar masalah secara deterministik.

### Untuk pemilik

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Apakah perbaikan konkurensi palsu ini mencakup seluruh entitas anak dokumen admisi di `InpAdmissionDocumentService.Content.cs`? | Ya, perbaiki seluruh entitas anak (`BeliefItems`, `PrivacyEntries`, `Party`, `PrivacyRequest`, `CostDifferenceStatement`, `DepositStatement`) / Hanya perbaiki `BeliefItems` | **(Recommended)** Perbaiki seluruh entitas anak sekaligus agar alur dokumen admisi lain tidak mengalami kebuntuan yang sama di masa mendatang | `FIX-EPS-042-01` |

---

## 8. Catatan pola

Akar masalah ini memiliki pola yang **persis sama** dengan **Bug #3** yang pernah didiagnosis pada laporan verifikasi runtime terdahulu:
- Rujukan: [issue-005-verifikasi-runtime-workspace-ppri.md](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/issue/issue-005-verifikasi-runtime-workspace-ppri.md) dan [verifikasi-runtime-workspace-ppri-2026-10-08.md:310-348](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/verifikasi-runtime-workspace-ppri-2026-10-08.md#L310-L348).
- Pada saat itu, tanda tangan dokumen admisi (`InpAdmissionDocumentSignature`) mengalami `DbUpdateConcurrencyException` saat `SaveSignatureAsync` karena ditambahkan langsung ke koleksi navigasi `document.Signatures.Add(signature)`. Perbaikan saat itu dilakukan di [InpAdmissionSignatureService.cs:285](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionSignatureService.cs#L285) dengan menyisipkan `_dbContext.Set<InpAdmissionDocumentSignature>().Add(signature);`.
- Namun, perbaikan tersebut saat itu hanya diterapkan pada penandatanganan di `InpAdmissionSignatureService.cs` dan belum ditelusuri ke method penanganan isi dokumen di `InpAdmissionDocumentService.Content.cs`. Issue ini menutup sisa celah tersebut secara tuntas.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-28 | Dokumen issue dibuat dari laporan pengguna mengenai penolakan HTTP 409 `INP-ADM-DOC-004` pada penyimpanan konsep Nilai Kepercayaan | `diagnose-module-issue` |
| 2026-10-28 | Perbaikan `FIX-EPS-042-01` selesai diimplementasikan pada `InpAdmissionDocumentService.Content.cs` dan lulus kompilasi `dotnet build` (0 error) | `diagnose-module-issue` |
