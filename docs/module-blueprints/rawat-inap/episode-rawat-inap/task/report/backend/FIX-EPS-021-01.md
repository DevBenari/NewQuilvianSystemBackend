# FIX-EPS-021-01 - Kompilasi ulang backend runtime tanpa validasi metode deposit

Tanggal: 2026-10-15. Status: SELESAI - build runtime & binary assembly.

Masukan: [rencana disetujui](../../../docs/plan-repair/plan-repair-021-peniadaan-input-metode-deposit-admisi-transfer.md), [laporan issue](../../../docs/issue/issue-021-peniadaan-input-metode-deposit-admisi-transfer.md). Wewenang: persetujuan perbaikan oleh Pemilik Sistem (User) pada 15 Oktober 2026; branch backend MHamzah.

## Perubahan dan source

Runtime API backend sebelumnya mengalami *stale binary* di mana `NewQuilvianSystemBackend/bin/Debug/net9.0/QuilvianSystemBackend.dll` tertanggal 14/10/2026 16:59 sebelum commit `bcc663df` masuk. Pada source code `InpAdmissionTransferService.cs`, validasi `if (!request.DepositPaymentMethodId.HasValue) throw new InpBedOperationException(422, "DEPOSIT_METHOD_REQUIRED", ...)` sudah dihapus, namun proses `QuilvianSystemBackend.exe` lama masih mengunci berkas biner sehingga pemanggilan endpoint mengembalikan HTTP 422 `DEPOSIT_METHOD_REQUIRED`.

Tindakan perbaikan:
1. Menghentikan proses host `QuilvianSystemBackend.exe` yang menahan lock file DLL/EXE.
2. Menjalankan kompilasi bersih project backend:
   ```powershell
   dotnet build NewQuilvianSystemBackend/QuilvianSystemBackend.csproj -c Debug --no-restore -p:EnableSourceControlManagerQueries=false
   ```
3. Memastikan berkas biner target terperbarui:
   - `NewQuilvianSystemBackend\bin\Debug\net9.0\QuilvianSystemBackend.dll` (LastWriteTime: 10/15/2026 10:33 AM)
   - `NewQuilvianSystemBackend\bin\Debug\net9.0\QuilvianSystemBackend.exe` (LastWriteTime: 10/15/2026 10:33 AM)

Berkas utama:
- `NewQuilvianSystemBackend/Services/HealthServices/InPatientManagement/InpAdmissionTransferService.cs`
- `NewQuilvianSystemBackend/bin/Debug/net9.0/QuilvianSystemBackend.dll`
- `NewQuilvianSystemBackend/bin/Debug/net9.0/QuilvianSystemBackend.exe`

## Acceptance dan bukti

| Kriteria Penerimaan | Status | Bukti Aktual |
| :--- | :---: | :--- |
| Kompilasi project berhasil tanpa error | PASS | `dotnet build`: Build succeeded. 0 Error(s), 5 Warning(s). Elapsed: 00:00:04.18 |
| Biner backend memuat kode terkini commit `bcc663df` | PASS | Timestamp biner `QuilvianSystemBackend.dll` dan `.exe` berubah menjadi 15 Oktober 2026 10:33 AM |
| Tidak ada validasi `DEPOSIT_METHOD_REQUIRED` saat deposit diisi nominal saja | PASS | Kode validasi telah dihilangkan dari `InpAdmissionTransferService.cs`; admisi hanya menyimpan `PlannedDepositAmount` pada `InpEpisode` tanpa menyentuh mutasi kasir kas/bank |

## Governance dan dampak

- Backend Governance: Modul `HealthServices/InPatientManagement` (Inp).
- Arketipe: Transaksi admisi transfer ranap. Tidak menambah tabel baru atau entitas master baru.
- Peran Bisnis: Loket admisi ranap hanya mencatat rencana deposit (`PlannedDepositAmount`). Kasir / Billing tetap memegang hak prerogatif instrumen pembayaran saat penyetoran nyata dilakukan.
