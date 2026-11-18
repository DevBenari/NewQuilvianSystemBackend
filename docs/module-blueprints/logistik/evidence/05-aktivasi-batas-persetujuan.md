# Aktivasi batas persetujuan permintaan barang per lingkungan

| Field | Nilai |
| --- | --- |
| Blueprint | `LGS-BP-001` r5 |
| Keputusan | `LGS-DEC-006` |
| Konfigurasi | `PharmacyManagement:StockRequest:ApprovalEnforcedFromUtc` |
| Variabel lingkungan | `PharmacyManagement__StockRequest__ApprovalEnforcedFromUtc` |
| Disusun | 2026-11-17 |

Dokumen ini menjelaskan cara memberlakukan persetujuan atasan unit di setiap lingkungan. Nilai
tanggal **tidak diisi di sini**. Nilai ditentukan pada saat aktivasi oleh petugas yang
berwenang, lalu dicatat pada tabel di bagian 5.

## 1. Apa yang diatur

Batas aktivasi memisahkan permintaan lama dari permintaan baru:

| Permintaan | Aturan |
| --- | --- |
| Diajukan **sebelum** batas | Aturan peralihan: gudang masih boleh menyerahkan langsung. Atasan unit tetap boleh menyetujui atau menolak |
| Diajukan **pada atau sesudah** batas | Wajib disetujui atasan unit (atau diotorisasi CITO) sebelum diserahkan |

Contoh: batas diaktifkan `2026-12-01T00:00:00Z`.

- Permintaan yang diajukan 30 November pukul 23.00 UTC adalah permintaan lama, jadi boleh diserahkan langsung.
- Permintaan yang diajukan 1 Desember pukul 00.05 UTC wajib disetujui.

## 2. Aturan pengaman yang ditegakkan sistem

| Keadaan nilai | Perilaku | Catatan log |
| --- | --- | --- |
| Kosong | Semua permintaan wajib disetujui | Audit, sekali |
| Tidak dapat dibaca | Semua permintaan wajib disetujui | Error, setiap kali nilai baru terlihat |
| Di masa depan (lebih dari 5 menit) | Semua permintaan wajib disetujui | Error |
| Sah dan sudah lewat | Aturan peralihan berlaku | Audit saat pertama terlihat, Warning bila berubah |

Aturan pengaman lain:

- **Waktu pembanding** adalah `SubmittedAt`, yang ditulis server saat permintaan diajukan. Waktu dari browser tidak dipakai.
- **Permintaan tanpa `SubmittedAt`** tidak pernah dianggap permintaan lama.
- **Pengguna aplikasi tidak dapat mengubah batas.** Tidak ada endpoint untuk itu; batas hanya dapat diubah lewat konfigurasi lingkungan.
- **Batas di masa depan ditolak.** Kalau batas boleh dijadwalkan ke depan, permintaan yang diajukan sebelum waktu itu kelak menjadi "permintaan lama" secara surut.

## 3. Prosedur aktivasi

1. **Audit data, hanya baca.** Hitung permintaan berstatus `Submitted` di lingkungan tersebut:

   ```sql
   select count(*), min("SubmittedAt"), max("SubmittedAt")
   from "PhmStockRequest"
   where not "IsDelete" and "Status" = 2;
   ```

2. **Tentukan waktu aktivasi**, yaitu saat versi aplikasi dengan persetujuan atasan mulai dipakai di lingkungan tersebut. Tulis dalam UTC dengan akhiran `Z`, misalnya format `YYYY-MM-DDTHH:MM:SSZ`. Waktu aktivasi tidak boleh di masa depan.
3. **Bila hasil langkah 1 nol**, batas boleh dibiarkan kosong. Akibatnya semua permintaan wajib disetujui, dan tidak ada transaksi lama yang terdampak.
4. **Bila ada permintaan `Submitted`**, isi variabel lingkungan `PharmacyManagement__StockRequest__ApprovalEnforcedFromUtc` dengan waktu aktivasi lewat mekanisme konfigurasi resmi lingkungan tersebut. Jangan lewat `appsettings.json` yang ikut ke git.
5. **Mulai ulang aplikasi**, lalu periksa log `StockRequest.ApprovalCutover`. Nilai yang tercatat harus sama dengan waktu aktivasi, dan keadaannya `Active`.
6. **Catat aktivasi** pada tabel bagian 5: lingkungan, nilai, pelaksana, penyetuju, dan hasil langkah 1.

## 4. Perubahan setelah aktivasi

Batas dikunci pada aktivasi (`LGS-DEC-006`). Mengubahnya, terutama memajukannya, berarti
mengubah status permintaan yang sudah diajukan secara surut.

- Perubahan membutuhkan persetujuan tersendiri dan dicatat pada tabel bagian 5.
- Sistem mencatat setiap nilai baru sebagai `StockRequest.ApprovalCutoverChanged` (Warning), lengkap dengan nilai sebelumnya.
- **Keterbatasan:** sistem belum dapat menolak perubahan itu secara teknis. Penolakan teknis membutuhkan tabel aktivasi tersendiri (migration). Sampai tabel itu ada, penguncian bergantung pada kendali perubahan konfigurasi.

## 5. Catatan aktivasi per lingkungan

Diisi pada saat aktivasi. **Jangan diisi sebelumnya.**

| Lingkungan | Nilai batas (UTC) | Permintaan `Submitted` saat aktivasi | Pelaksana | Penyetuju | Tanggal pencatatan |
| --- | --- | --- | --- | --- | --- |
| Pengembangan lokal | ditentukan saat aktivasi | — | — | — | — |
| Basis data tim (`QuilvianNewDevMaster`) | ditentukan saat aktivasi | — | — | — | — |
| Staging | ditentukan saat aktivasi | — | — | — | — |
| Produksi | ditentukan saat aktivasi | — | — | — | — |

Hasil audit 2026-11-17 (hanya baca), sebagai rujukan saja:

| Basis data | Permintaan `Submitted` |
| --- | --- |
| `QuilvianNewDevMaster` | 0 (tabel kosong) |
| `QuilvianNewDevIkbalFr` (lokal) | 0 (satu permintaan, berstatus Selesai) |
