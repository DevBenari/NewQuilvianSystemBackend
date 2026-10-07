# Laporan Perubahan Backend — `BE-RWI-158`

## Pembaruan eksekusi menyeluruh — 5 Oktober 2026

Bagian ini adalah bukti terbaru. Bagian laporan sebelumnya, bila ada, dipertahankan sebagai riwayat.

### Metadata eksekusi terakhir

| Field | Nilai |
| --- | --- |
| Task | `BE-RWI-158` — Retur obat membatalkan tagihan |
| Roadmap / slice | [backend-roadmap-finishing.md](../../../roadmap/backend-roadmap-finishing.md), kartu `BE-RWI-158` |
| Tanggal / mode | 5 Oktober 2026 / BACKEND |
| Status | SELESAI KODE — build dan validasi terbatas PASS; UAT klinis lengkap NOT RUN |
| Branch / upstream | `MHamzah` / `origin/MHamzah`; sesuai manifest |
| SHA awal sesi | `0a10899435bcd4cd54e998a060465589b6652643` |
| Kontrak / trace | `RWI-BP-001` revision 8; API `1.1.0 approved`; Integrasi 4.5 |
| Wewenang | Pengguna meminta seluruh BE-RWI-146–159, build dan migration pada akhir; database update development diotorisasi roadmap 5 Oktober 2026 |
| Target tulis | NewQuilvianSystemBackend; source dalam slice task, migration I6, laporan, roadmap dan traceability integrasi-billing |
| Dependency | `BE-RWI-155`; source/schema dependency tersedia; bukti klinis lengkap tidak diturunkan dari status kode |
| Area / Module / prefix | HealthServices / PharmacyManagement (Phm) dan BillingManagement (Bil), ACTIVE |
| Applicability | NEW CODE dan/atau TOUCHED LEGACY sesuai berkas; LEGACY MIGRATION NOT APPLICABLE |
| Klasifikasi | HEAVY — skor 10: repository 0, diperiksa 2, diubah 1, logika 2, API 1, database 1, auth 1, workflow 2 |
| QBE applicable | QBE-SVC-001, QBE-VAL-001, QBE-TXN-001, QBE-LOG-001, QBE-AUD-001 |
| Governance | AGENTS.md, engineering repository, dan rules/backend suite terbaca; engineering identik dengan suite, registry repository lebih baru pada domain lain; Bil/Inp/Phm sama ACTIVE. Repository canonical mengikuti AGENTS.md |
| Model | Codex, GPT-6 |

### Masalah, proses bisnis, dan perubahan

Farmasi memeriksa jumlah layak kembali terhadap penyerahan, batch, dan satuan asli. Stok serta riwayat retur disimpan dalam transaksi Farmasi. Setelah commit, fakta pembatalan memakai stream sumber penyerahan dan nomor retur; retur rusak tidak mengurangi tagihan. Pada invoice OPEN, baris asli di-VOIDED tanpa mengubah angka historis dan dibuat revisi tersisa; pada FINAL dipakai jalur adjustment. Contoh review: 3 vial × Rp50.000 = Rp150.000, retur 1 layak menyisakan Rp100.000.

Menambah handoff retur, service revisi Billing dan jalur bridge retur; mengubah Verify serta emisi penyerahan menjadi jumlah netto setelah retur. Tidak ada data penyerahan resep pada sampel database; contoh berangka belum diuji runtime.

Berkas bukti:

- `Areas/HealthServices/PharmacyManagement/Services/DrugReturnBillingHandoffService.cs`
- `Areas/HealthServices/PharmacyManagement/Services/DrugReturnService.cs#VerifyAsync`
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingDrugReturnService.cs#ApplyAsync`
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.DrugReturn.cs`

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Kontrak existing dipertahankan sesuai kartu; perubahan removal yang disetujui tetap berlaku |
| Database | Tidak menambah schema pada task ini; dependency I1/I2/E6/I6 telah diterapkan |
| Audit / keamanan | Database audit tetap melalui IdentityModel/SaveChanges existing; application log terpisah. Hak akses mengikuti atribut Access; tidak ada wewenang dari nama peran |
| Consumer frontend | Frontend dibaca saja; consumer lama masih perlu mengikuti pemisahan Read/ViewAmount dan pencabutan route sesuai task pendamping |

### Endpoint

NOT APPLICABLE — task ini memproses service/event/schema tanpa endpoint HTTP baru. Endpoint existing yang tidak berubah dijelaskan pada bagian riwayat bila ada.

### Verifikasi terbaru

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE Strict WorkingTree akhir | 27 file; VIOLATION 0, REVIEW 0, INFO 0 | PASS |
| Build awal | Dua CS1061 replay (`Status`); diperbaiki menjadi `EpisodeStatus` | NEW ERROR — sudah diperbaiki |
| `dotnet build QuilvianSystemBackend.csproj --nologo --no-restore` | Build akhir mencakup semua source dan metadata migration; 0 Error(s), 233 Warning(s), 00:05:31.27, exit 0 | PASS |
| `dotnet ef migrations add AddRawatInapBillingEncounterLink --project QuilvianSystemBackend.csproj --no-build` | Migration I6 dibuat setelah implementasi source lengkap; scope Up hanya tabel tautan | PASS |
| `dotnet ef database update --project QuilvianSystemBackend.csproj --no-build` dengan Development | Applying I6, Done., exit 0; pemeriksaan `GetPendingMigrationsAsync` menghasilkan 0 | PASS |
| Konfirmasi database setelah perbaikan penutup | Database already up to date, Done., exit 0; tidak ada migration tambahan | PASS |
| Verifikasi runtime terbatas terhadap DLL akhir | 35 pemeriksaan PASS: serialisasi, 11 metadata action, filter 401/403, whitelist, DeadLetter, EF model, pending migration, DryRun, query rincian | PASS |
| DryRun database development | 3 kandidat; data outbox dan RequiresReview/RowVersion sebelum/sesudah identik | PASS |
| Pembacaan rincian tersimpan | Breakdown dan aggregate konsisten pada 3 episode development; tidak memanggil kalkulasi/tulis | PASS |
| Sampel penyerahan resep untuk retur | 0 prescription sumber ditemukan; perhitungan retur nyata tidak dibuktikan | NOT RUN |
| UAT klinis lengkap, akun HTTP nyata, replay tulis, gangguan worker, regresi rawat jalan dan rollback Down | Tidak dijalankan; hasil pemeriksaan terbatas tidak menjadi bukti skenario ini | NOT RUN |
| QBE percobaan ulang | Git stderr peringatan CRLF memenuhi pipe checker dan menahan proses; dipulihkan dengan core.safecrlf=false hanya pada environment proses, tanpa mengubah konfigurasi repository atau mode Strict | EXISTING / ENVIRONMENT ISSUE, dipulihkan |
| Warning compiler dan harness | 233 warning aplikasi; harness sementara menampilkan MSB3277 DependencyModel 9.0.12/9.0.18; tidak mengubah package aplikasi | EXISTING / ENVIRONMENT ISSUE — warning aplikasi tidak seluruhnya dibuktikan sebagai baseline |



Uji manual: **PASS terbatas**. Principal filter berupa identitas sintetis bernama Cashier tanpa ID pengguna valid; hasil 403 membuktikan filter berjalan, bukan UAT akun rumah sakit dengan role-permission nyata. Pemeriksaan dijalankan melalui console sementara di luar repository; tidak ada task/project automated test baru. Database memakai konfigurasi Development, nama database bermarker development dan tanpa marker produksi; host remote, bukan loopback. Connection string dan data pasien tidak dicatat.

Bukti output lokal (`%TEMP%` = `C:/Users/Admin/AppData/Local/Temp`):

| File | SHA256 |
| --- | --- |
| `Quilvian-billing-finishing-final-build.log` | `7B241A1273B063C92AA2F5043DC119557AA202166AC296A66478215DD360FA41` |
| `Quilvian-billing-finishing-database-update.log` | `6FE5D4EFB20FA0C5267EFDE2783F8698FEC643E2BE47982E9097DB5CDA894B14` |
| `Quilvian-billing-finishing-database-final-check.log` | `DA95D2A434379F615CAE4C85266F2E21C49E4E8A120A4CE9F9F5EBEEFE2DFA7F` |
| `Quilvian-billing-finishing-runtime-final.log` | `9322A732FA830AA4F40F9BB9BD53172B1D7F6C8D35D2436C85E70B7932A75F66` |
| `Quilvian-billing-finishing-qbe-final.log` | `D9FB75C1901806EBA0CF386A83275FCB4598223561A895830C327013D9FB47DF` |

### Acceptance criteria dan Definition of Done terbaru

| Kriteria roadmap | Status | Bukti |
| --- | --- | --- |
| 1. Serah 3 vial, retur 1 layak → invoice 2 vial dengan pembatalan merujuk nomor retur. | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/PharmacyManagement/Services/DrugReturnBillingHandoffService.cs`; `Areas/HealthServices/PharmacyManagement/Services/DrugReturnService.cs#VerifyAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingDrugReturnService.cs#ApplyAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.DrugReturn.cs` |
| 2. Retur dinilai rusak → tagihan tidak berubah. | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/PharmacyManagement/Services/DrugReturnBillingHandoffService.cs`; `Areas/HealthServices/PharmacyManagement/Services/DrugReturnService.cs#VerifyAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingDrugReturnService.cs#ApplyAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.DrugReturn.cs` |
| 3. Retur sesudah invoice final tidak otomatis mengubah invoice (jalur adjustment Billing) | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/PharmacyManagement/Services/DrugReturnBillingHandoffService.cs`; `Areas/HealthServices/PharmacyManagement/Services/DrugReturnService.cs#VerifyAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingDrugReturnService.cs#ApplyAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.DrugReturn.cs` |

Implementasi, build, migration development, dan laporan tersedia. DoD alur klinis lengkap tetap **belum terbukti** pada butir berstatus NOT RUN; status kode tidak sama dengan sign-off rilis.

### Catatan penutup

Bila penerbitan gagal sebelum fakta tersimpan, pemulihan tersedia lewat pengulangan Verify dengan nomor retur yang sama; belum ada worker handoff khusus untuk tahap sebelum penyimpanan fakta. Dokumen `rules/backend/TEST_POLICY.md` yang dirujuk roadmap tidak tersedia pada suite ini. Semua governance canonical wajib lainnya terbaca; validasi mengikuti AGENTS.md dan review rules yang tersedia. Tidak ada project test repository. Dokumen luar slice yang sudah berubah sebelum atau selama sesi tidak disentuh. Tidak ada stage, commit, push, deployment, atau replay produksi.

Status Git task: source/laporan M atau ?? sesuai berkas baru; tiga berkas tarif kamar lama D pada BE-RWI-147. Laporan dibuat pada working tree yang belum di-commit. Interupsi: NONE. Langkah berikutnya: consumer frontend pendamping dan UAT klinis sesuai acceptance matrix.


