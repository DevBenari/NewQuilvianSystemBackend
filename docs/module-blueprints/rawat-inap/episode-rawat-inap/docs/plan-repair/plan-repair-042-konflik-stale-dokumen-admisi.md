# PLAN-REPAIR-042 — Perbaikan Penolakan Konkurensi Palsu 409 INP-ADM-DOC-004 pada Perubahan Dokumen Admisi Workspace PPRI

```yaml
plan_id: PLAN-REPAIR-EPS-042
issue: ../issue/issue-042-konflik-stale-dokumen-admisi.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-28"
diputuskan_oleh: "Pemilik (USER)"
tanggal_keputusan: "2026-10-28"
basis_source_backend: "c6b5d95071bed9d12ddb30cd0d81876be1324aaa (MHamzah)"
basis_source_frontend: "cde815cf455e51166803c24957c30ea227dcd696 (HamzahV2)"
```

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-042-01` | `ISS-EPS-042-01`, `ISS-EPS-042-T1` | Pendaftaran eksplisit entitas anak baru (`BeliefItems`, `PrivacyEntries`, `Party`, dll.) ke ChangeTracker DbContext saat `ApplyContentAsync` | Backend | 1 | — | ✅ SELESAI | [`InpAdmissionDocumentService.Content.cs:155,528,576,627,678,719`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionDocumentService.Content.cs); `dotnet build` PASS (0 error, exit 0) |

**Ringkasan: 1 dari 1 perbaikan selesai dikerjakan pada 28 Oktober 2026.**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-042-01 & ISS-EPS-042-T1 — Penolakan konkurensi palsu 409 INP-ADM-DOC-004 saat menyimpan entitas anak dokumen admisi

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A** | **Panggil `_dbContext.Set<T>().Add(entity)` eksplisit di setiap pembuatan entitas anak baru di `InpAdmissionDocumentService.Content.cs`** | Mengikuti pola yang sudah terbukti pada perbaikan Bug #3 di `InpAdmissionSignatureService.cs:285`; tidak mengubah schema basis data, model, DTO, atau frontend; ChangeTracker EF Core secara tegas menetapkan `EntityState.Added` | Memerlukan penyisipan baris penambahan pada 6 titik pembuatan entitas anak |
| B | Hapus inisialisasi inline `Id = Guid.NewGuid()` pada seluruh model entitas anak (`InpAdmissionBeliefItem`, `InpAdmissionPrivacyEntry`, dll.) dan biarkan bernilai default `Guid.Empty` | EF Core mendeteksi `Guid.Empty` sebagai entitas baru secara otomatis saat navigasi ditambahkan | Berisiko memengaruhi bagian kode lain yang mengasumsikan ID sudah terisi sebelum disimpan; membutuhkan tinjauan luas |
| C | Gunakan `_dbContext.Entry(entity).State = EntityState.Added` secara manual setelah objek dibuat | Hasil sama dengan Opsi A | Lebih tidak lazim dibandingkan pemanggilan standar `_dbContext.Set<T>().Add(...)` yang sudah digunakan di service lain |

**Solusi terpilih: Opsi A.**

1. Selaras dengan standar yang telah diterapkan di [InpAdmissionSignatureService.cs:285](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionSignatureService.cs#L285) (`_dbContext.Set<InpAdmissionDocumentSignature>().Add(signature);`).
2. Lingkup perubahan sempit, hanya pada satu berkas service backend (`InpAdmissionDocumentService.Content.cs`), tanpa efek samping pada migrasi atau frontend.
3. Menjamin setiap entitas baru dieksekusi dengan perintah SQL `INSERT` ke PostgreSQL alih-alih `UPDATE`, sehingga tidak akan pernah melempar `DbUpdateConcurrencyException` palsu.

**Kenapa bukan opsi lain.**
- Opsi B ditolak karena berisiko regresi pada method atau fungsi pemetaan DTO yang mengandalkan ketersediaan ID saat pembuatan instansi in-memory.
- Opsi C ditolak karena pola `_dbContext.Set<T>().Add(...)` lebih konsisten dengan konvensi kode di repository `NewQuilvianSystemBackend`.

---

## 3. Skema tampilan sebelum → sesudah

### SEBELUM (Kondisi Saat Ini Sesuai Screenshot Pelapor)

```text
+----------------------------------------------------------------------------------------------------+
| Nilai Kepercayaan                                                                                  |
| Hal yang bertentangan dengan nilai dan kepercayaan pasien, dicatat per episode.                   |
|                                                                                                    |
| [Konsep] Versi 1 · dibuat 28 Okt 2026, 11:14 oleh superadmin · Ada perubahan yang belum disimpan   |
|                                                                                                    |
| ⚠ Data basi                                                                                    [x] |
|   Dokumen sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda.                        |
|   [ Muat Ulang ]                                                                                   |
|                                                                                                    |
| +------------------------------------------------------------------------------------------------+ |
| | Yang Bertanda Tangan di Bawah Ini                                        [ Pihak Penanda Tangan] |
| |                                                                                                |
| | Nama Lengkap *                      Tanggal Lahir                                              |
| | [ JAnuaer                      ]    [ 28/10/2026                 ]                             |
| | Hubungan dengan Pasien *                                                                        |
| | [ Diri sendiri                ▾]                                                               |
| | Alamat *                                                                                       |
| | [ oke                          ]                                                               |
| +------------------------------------------------------------------------------------------------+ |
| +------------------------------------------------------------------------------------------------+ |
| | Hal-hal yang bertentangan dengan nilai-nilai dan kepercayaan saya/pasien      [+ Tambah (Maks 5)]|
| | 1. [ test                                                                                 ] [H] |
| | 2. [ test                                                                                 ] [H] |
| +------------------------------------------------------------------------------------------------+ |
|                                                                                                    |
| [ Simpan Konsep ]  [ Simpan & Lanjut ke Tanda Tangan ]                                             |
+----------------------------------------------------------------------------------------------------+
```

### SESUDAH (Setelah Perbaikan Diterapkan)

```text
+----------------------------------------------------------------------------------------------------+
| Nilai Kepercayaan                                                                                  |
| Hal yang bertentangan dengan nilai dan kepercayaan pasien, dicatat per episode.                   |
|                                                                                                    |
| [Konsep] Versi 1 · diperbarui 28 Okt 2026, 11:30 oleh superadmin                                    |
|                                                                                                    |
| ✓ Konsep tersimpan (Toast: Perubahan konsep tersimpan.)                                            |
|                                                                                                    |
| +------------------------------------------------------------------------------------------------+ |
| | Yang Bertanda Tangan di Bawah Ini                                        [ Pihak Penanda Tangan] |
| |                                                                                                |
| | Nama Lengkap *                      Tanggal Lahir                                              |
| | [ JAnuaer                      ]    [ 28/10/2026                 ]                             |
| | Hubungan dengan Pasien *                                                                        |
| | [ Diri sendiri                ▾]                                                               |
| | Alamat *                                                                                       |
| | [ oke                          ]                                                               |
| +------------------------------------------------------------------------------------------------+ |
| +------------------------------------------------------------------------------------------------+ |
| | Hal-hal yang bertentangan dengan nilai-nilai dan kepercayaan saya/pasien      [+ Tambah (Maks 5)]|
| | 1. [ test                                                                                 ] [H] |
| | 2. [ test                                                                                 ] [H] |
| +------------------------------------------------------------------------------------------------+ |
|                                                                                                    |
| [ Simpan Konsep ]  [ Simpan & Lanjut ke Tanda Tangan ]                                             |
+----------------------------------------------------------------------------------------------------+
```

### Tabel Perubahan Layar

| Wilayah | Sebelum | Sesudah |
| --- | --- | --- |
| **Spanduk Peringatan** | Muncul spanduk kuning *"Data basi — Dokumen sudah diubah petugas lain"* secara keliru | Spanduk kuning **TIDAK MUNCUL**. Muncul notifikasi toast hijau *"Konsep tersimpan — Perubahan konsep tersimpan."* |
| **Status Dokumen** | Tertahan pada status perubahan belum disimpan karena penyimpanan ditolak | Tanggal/waktu perbaruan konsep dan `RowVersion` diperbarui secara mulus di header |
| **Tombol Aksi** | Tombol *Simpan & Lanjut ke Tanda Tangan* terblokir karena penyimpanan draft gagal | Tombol *Simpan & Lanjut ke Tanda Tangan* dapat diklik dan berhasil mengunci dokumen ke tahap tanda tangan |

---

## 4. Rincian perbaikan

### FIX-EPS-042-01 — Pendaftaran eksplisit entitas anak baru ke DbContext saat ApplyContentAsync

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-042-01`, `ISS-EPS-042-T1` |
| **Area** | Backend |
| **Jenis perubahan** | Source C# backend (`InpAdmissionDocumentService.Content.cs`) |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/Services/InpAdmissionDocumentService.Content.cs` |
| **Radius dampak** | Seluruh proses update dokumen admisi di Workspace PPRI (`PUT .../admission-workspace/documents/{id}`): Nilai Kepercayaan, Permintaan Privasi, Selisih Biaya, Pelunasan Deposit, dan Pihak Penanda Tangan |
| **Bergantung pada** | Tidak ada |

**Langkah perubahan kode:**

1. Buka [InpAdmissionDocumentService.Content.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionDocumentService.Content.cs).
2. Pada method `ApplyPartyAsync` (sekitar baris 146–156), saat instansi `party` baru dibuat:
   ```csharp
   if (party == null)
   {
       party = new InpAdmissionDocumentParty
       {
           Id = Guid.NewGuid(),
           DocumentId = document.Id,
           CreateDateTime = now,
           CreateBy = actorUserId
       };
       _dbContext.Set<InpAdmissionDocumentParty>().Add(party); // <-- Daftarkan ke DbContext
       document.Party = party;
   }
   ```
3. Pada method `ApplyPrivacy` (sekitar baris 520–529), saat instansi `PrivacyRequest` baru dibuat:
   ```csharp
   if (document.PrivacyRequest == null)
   {
       document.PrivacyRequest = new InpAdmissionPrivacyRequest
       {
           Id = Guid.NewGuid(),
           DocumentId = document.Id,
           CreateDateTime = now,
           CreateBy = actorUserId
       };
       _dbContext.Set<InpAdmissionPrivacyRequest>().Add(document.PrivacyRequest); // <-- Daftarkan ke DbContext
   }
   ```
4. Pada method `SyncPrivacyEntries` (sekitar baris 564–574), saat instansi `InpAdmissionPrivacyEntry` baru dibuat:
   ```csharp
   var entry = new InpAdmissionPrivacyEntry
   {
       Id = Guid.NewGuid(),
       DocumentId = document.Id,
       EntryType = entryType,
       LineNo = lineNo,
       Text = texts[index],
       CreateDateTime = now,
       CreateBy = actorUserId
   };
   _dbContext.Set<InpAdmissionPrivacyEntry>().Add(entry); // <-- Daftarkan ke DbContext
   document.PrivacyEntries.Add(entry);
   ```
5. Pada method `ApplyBelief` (sekitar baris 614–624), saat instansi `InpAdmissionBeliefItem` baru dibuat:
   ```csharp
   var beliefItem = new InpAdmissionBeliefItem
   {
       Id = Guid.NewGuid(),
       DocumentId = document.Id,
       ItemNo = itemNo,
       Text = texts[index],
       CreateDateTime = now,
       CreateBy = actorUserId
   };
   _dbContext.Set<InpAdmissionBeliefItem>().Add(beliefItem); // <-- Daftarkan ke DbContext
   document.BeliefItems.Add(beliefItem);
   ```
6. Pada method `ApplyCostDifference` (sekitar baris 666–675), saat instansi `CostDifferenceStatement` baru dibuat:
   ```csharp
   if (document.CostDifferenceStatement == null)
   {
       document.CostDifferenceStatement = new InpAdmissionCostDifferenceStatement
       {
           Id = Guid.NewGuid(),
           DocumentId = document.Id,
           CreateDateTime = now,
           CreateBy = actorUserId
       };
       _dbContext.Set<InpAdmissionCostDifferenceStatement>().Add(document.CostDifferenceStatement); // <-- Daftarkan ke DbContext
   }
   ```
7. Pada method `ApplyDeposit` (sekitar baris 703–712), saat instansi `DepositStatement` baru dibuat:
   ```csharp
   if (document.DepositStatement == null)
   {
       document.DepositStatement = new InpAdmissionDepositStatement
       {
           Id = Guid.NewGuid(),
           DocumentId = document.Id,
           CreateDateTime = now,
           CreateBy = actorUserId
       };
       _dbContext.Set<InpAdmissionDepositStatement>().Add(document.DepositStatement); // <-- Daftarkan ke DbContext
   }
   ```

**Acceptance criteria:**

1. Pemanggilan `PUT /api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/documents/{documentId}` dengan penambahan butir kepercayaan baru (misal `beliefItems: ["test", "test"]`) pada dokumen berstatus `Draft` mengembalikan HTTP `200 OK` (bukan `409 Conflict`).
2. Seluruh butir kepercayaan tersimpan dengan benar di tabel PostgreSQL `InpAdmissionBeliefItem`, dengan nomor urut (`ItemNo`) yang sesuai dan relasi `DocumentId` yang valid.
3. Penambahan data penanda tangan (`party`), entri privasi (`PrivacyEntries`), maupun pernyataan selisih biaya / deposit pada dokumen konsep yang sudah ada tidak memicu `DbUpdateConcurrencyException`.
4. Penolakan konkurensi sejati (`409 INP-ADM-DOC-004`) tetap berfungsi sebagaimana mestinya jika dua pengguna secara nyata mengirimkan `RowVersion` yang tidak cocok dengan data di database.

**Verifikasi:**
- `dotnet build QuilvianSystemBackend.sln` harus berhasil dengan 0 galat.
- Menjalankan kembali simulasi penyimpanan formulir Nilai Kepercayaan melalui browser atau REST client memastikan response `200 OK` dan data tersimpan tanpa spanduk data basi.

**Risiko:**
- Sangat rendah. Perubahan hanya menegaskan state `EntityState.Added` pada entitas baru yang memang belum ada di database, tanpa mengubah relasi, kolom, maupun tipe data.

---

## 5. Urutan pengerjaan

```text
FIX-EPS-042-01 (Backend)   tanpa dependency — dapat langsung diimplementasikan
```

---

## 6. Keputusan pemilik yang dibutuhkan

Disalin dari dokumen issue:

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Apakah perbaikan konkurensi palsu ini mencakup seluruh entitas anak dokumen admisi di `InpAdmissionDocumentService.Content.cs`? | Ya, seluruh entitas anak / Hanya `BeliefItems` | **(Recommended)** Perbaiki seluruh entitas anak sekaligus | `FIX-EPS-042-01` |

---

## 7. Dokumen hulu yang ikut direvisi

Tidak ada dokumen hulu (seperti blueprint atau skema kontrak API) yang perlu direvisi karena perbaikan ini sepenuhnya merupakan koreksi bug implementasi ORM backend (`BUG`), bukan perubahan spesifikasi desain (`DESIGN_CHANGE`).

---

## 8. Verifikasi menyeluruh

1. Buka kembali halaman Workspace PPRI untuk episode rawat inap contoh: `episodes/2bf607ae-cde3-4e24-9838-02249a8fcf7b/admission-workspace`.
2. Masuk ke menu *Nilai Kepercayaan*.
3. Tambahkan 2 butir kepercayaan (`test 1`, `test 2`), isi nama pihak penanda tangan (`JAnuaer`), lalu klik *Simpan Konsep*.
4. Pastikan response API adalah `200 OK`, notifikasi sukses hijau muncul, dan spanduk kuning *"Data basi"* **tidak muncul**.
5. Klik tombol *Simpan & Lanjut ke Tanda Tangan*, pastikan dokumen berhasil beralih status ke *Menunggu Tanda Tangan* (`AwaitingSignature`).

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-28 | Rencana perbaikan dibuat dengan 1 butir perbaikan terpadu (`FIX-EPS-042-01`), menunggu persetujuan | `diagnose-module-issue` |
| 2026-10-28 | Rencana disetujui pemilik, implementasi diterapkan pada `InpAdmissionDocumentService.Content.cs`, kompilasi `dotnet build` sukses (0 error) | `diagnose-module-issue` |
