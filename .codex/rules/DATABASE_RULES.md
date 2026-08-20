# Aturan Database dan EF Quilvian

Aturan ini menjaga implementasi EF Core dan PostgreSQL yang sudah ada. `AGENTS.md` tetap menjadi pemegang wewenang; periksa model pemiliknya, `ApplicationDbContext`, controller/service, dan migration terdekat sebelum mengambil keputusan persistence.

## Keselarasan dengan QBE canonical

Baca `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` beserta registry-nya sebelum mengerjakan persistence. Terapkan QBE-ENT-001, QBE-CFG-001, QBE-NAM-001–003, QBE-DB-001–002, QBE-CODE-004, dan QBE-MOD-002. Implementasi rujukan tidak menggantikan kontrak canonical.

## Disiplin model dan context

- Pertahankan ownership entity/model di dalam domain yang sudah mapan. Jaga konvensi tabel/schema, base model, relasi, audit, soft delete, dan status aktif.
- Periksa registrasi/konfigurasi pada `Repositories/ApplicationDbContext.cs` beserta model terkait sebelum mengubah perilaku persistence. Jangan mengarang lapisan repository bila implementasi terdekat memakai `ApplicationDbContext` secara langsung.
- Jaga konfigurasi dan registrasi dependency tetap selaras dengan `Program.cs`; jangan memperkenalkan arsitektur persistence atau jalur konfigurasi baru tanpa wewenang eksplisit.

## Disiplin query dan relasi

- Pertahankan pola query, filter, urutan, tracking, projection, dan pagination yang sudah ada. Pakai `AsNoTracking` untuk query hanya-baca bila pola terdekat melakukannya; tetap pakai tracking bila mutasi atau perilaku yang sudah ada memang membutuhkannya.
- Utamakan projection/select bila domain pemiliknya sudah mendukungnya, daripada memuat entity atau graf navigation yang tidak diperlukan.
- Muat relasi secara sengaja memakai pola `Include`, projection, atau query terdekat. Hindari penulisan ulang query secara luas, perilaku N+1 yang tidak disengaja, atau perubahan relasi yang tidak berkaitan.
- Pertahankan perilaku soft delete, status aktif, metadata audit, concurrency, transaksi, retry, dan idempotency yang sudah ada bila relevan.

## Mutasi, transaksi, dan keselamatan

- Ikuti pola create/update/delete dan `SaveChangesAsync` milik controller/service pemiliknya. Pakai transaksi hanya di tempat workflow/service multi-langkah yang sudah ada memang menetapkan batas transaksi.
- Jangan pernah secara otomatis menghapus database/tabel, mengosongkan data bisnis, menghapus record secara massal, mereset migration, menimpa konfigurasi, atau memperbarui database production/bersama.
- Perintah database bukan validasi source yang rutin. Laporkan validasi database sebagai belum dilakukan bila memang tidak diberi wewenang eksplisit.

## Wewenang migration dan eksekusi

Perubahan entity **tidak** dengan sendirinya memberi wewenang atas hal-hal berikut:

- pembuatan migration;
- `Update-Database`;
- eksekusi database; atau
- deployment.

Dampak schema/entity, pembuatan migration, eksekusi database, dan deployment adalah tindakan terpisah yang masing-masing perlu wewenang eksplisit. Jangan membuat, menghapus, mereset, menulis ulang, atau menjalankan migration tanpa wewenang yang berlaku dan target yang jelas batasnya.

## Bukti representatif

- Ownership DbContext dan DbSet/configuration: `Repositories/ApplicationDbContext.cs`
- Komposisi serta registrasi DbContext/service: `Program.cs`
- Padanan entity/persistence master data: `Areas/Administrator/MasterData/Models/MstBank.cs`; `Areas/Administrator/MasterData/Controllers/BankController.cs`
- Bukti transaksi/query/current-user: `Areas/SelfServices/HumanResource/Services/OvertimeSelfServiceService.cs`; `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs`
- Contoh konvensi migration: `Migrations/20260521081743_initializeLeaveBalanceAndLeaveRequest.cs`

## Konsistensi lintas developer

Modul baru mengikuti implementasi domain matang terdekat. Jangan memperkenalkan model persistence, konvensi migration, abstraksi repository, arsitektur query, atau model error yang baru hanya karena modulnya baru.
