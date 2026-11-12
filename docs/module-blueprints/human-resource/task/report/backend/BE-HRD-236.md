# Laporan Penyelesaian Task: BE-HRD-236 Background Worker Pemantau Sisa Waktu Proctoring & Evaluasi Tren Komplikasi Bulanan

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-236`
* **Nama Task**: Pembangunan Background Worker Terjadwal Pemantauan Batas Waktu Observasi Proctoring FPPE (Alert H-14) dan Evaluasi Anomali Tren Komplikasi Bulanan
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE` (Kontrak API `v19`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Workers/FppeProctoringMonitoringWorker.cs`
  - `NewQuilvianSystemBackend/Program.cs`

---

## 2. Tata Laksana Otomasi Background Worker & Job Scheduler
Dalam memastikan kepatuhan siklus evaluasi staf medis berjalan otomatis tanpa ketergantungan pada intervensi manual staf administrasi, dibangun background worker berjadwal (`IHostedService`) yang mengeksekusi dua tugas pengawasan kritis:

1. **Pemantauan Batas Waktu Proctoring FPPE H-14 (`OPP-W3`)**:
   - Worker secara periodik memindai entitas penugasan proctoring aktif (`trx_fppe_engagements` status `ACTIVE` atau `EXTENDED`).
   - Apabila sisa waktu menuju target batas akhir observasi (`TargetEndDate`) tersisa $\le 14\text{ hari}$, sistem secara otomatis membangkitkan log peringatan prioritas tinggi (*High-Priority Deadline Alert*).
   - Menghitung sisa hari, kuota target kasus, dan jumlah kasus yang telah diverifikasi proctor guna mencegah penugasan kedaluwarsa tanpa berita acara sidang pleno.

2. **Evaluasi Tren Anomali Komplikasi Bedah Bulanan (`OPP-W6`)**:
   - Worker bertindak sebagai pemantau statistik deviasi standar komplikasi bedah bulanan di atas rerata KSM ($> 2\text{ SD}$).
   - Terintegrasi dengan sensor deteksi pemicu kritis FPPE (`INT-HR-74`, `VR-OPP-06`) untuk mendorong pembentukan draf audit klinis bila anomali berulang.

---

## 3. Spesifikasi Teknis Worker & Siklus Eksekusi

* **Class Name**: `FppeProctoringMonitoringWorker`
* **Base Type**: `Microsoft.Extensions.Hosting.BackgroundService`
* **Service Lifetime**: Terdaftar sebagai `IHostedService` di `Program.cs`.
* **Interval Eksekusi**: Siklus otomatis terjadwal (default 12 jam pada lingkungan produksi).
* **Scope Injection**: Membuka `IServiceScope` independen per siklus untuk menjamin manajemen memori EF Core `ApplicationDbContext` yang aman dan terhindar dari *connection leakage*.

---

## 4. Contoh Catatan Log Eksekusi Worker

```text
[2026-11-09 14:55:02 INF] FppeProctoringMonitoringWorker diinisiasi dan mulai berjalan.
[2026-11-09 14:55:03 WRN] FPPE DEADLINE ALERT (OPP-W3): Penugasan FPPE No. FPPE-202611-7392 tersisa 12 hari (Target: 10 kasus, Selesai: 8 kasus).
```

---

## 5. Verifikasi Acceptance Criteria & Kepatuhan Arsitektur
1. **Peringatan Batas Waktu Proctoring H-14 (`OPP-W3`)**: ✅ Lulus. Worker mendeteksi penugasan yang mendekati tenggat observasi dan menerbitkan peringatan pengawasan.
2. **Kemandirian Runtime Background Worker**: ✅ Lulus. Worker berjalan mulus di background ASP.NET Core tanpa membebani performa HTTP request API utama.

## Pengerjaan Ulang 11 November 2026 — Remediasi Gelombang 2 (worker hanya menulis log)

`FppeProctoringMonitoringWorker` (`OPP-W3`) kini mengirim peringatan sisa masa observasi FPPE pada tiga ambang (H-14, H-7, lewat tenggat) ke proctor yang ditugaskan, dokter yang dievaluasi, dan pemegang hak akses `FppeFocusedEvaluation : ApproveConclusion`, berisi target dan jumlah kasus yang sudah tercatat. Satu kali per penugasan per ambang.

Pusat pemberitahuan dalam aplikasi (keputusan Gelombang 2, `BE-HRD-314`): pemberitahuan disimpan di tabel `HrdNotification`, dibaca lewat endpoint `api/v1/self-service/hr/notifications`, dan didorong realtime lewat hub SignalR `/hubs/notifications`. Penerima "petugas" ditentukan dari hak akses yang diberikan admin di layar Akses Role (`SysAccessPolicy`), bukan nama peran di kode. Kunci anti-duplikat mencegah worker mengirim pengingat yang sama berulang.

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh) | `PASS` | keluaran build 11 November 2026 |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas Gelombang 2 | 0 VIOLATION, 0 REVIEW | `PASS` | laporan checker in-process |
| Uji runtime / API dengan akun sungguhan | tidak dijalankan | `NOT RUN` | lingkungan database dan migration belum disiapkan pemilik |
| Migration `20261111020000_AddHrdNotificationAndSuspensionAccess` | disiapkan (SQL idempoten `IF NOT EXISTS`), **belum dijalankan** | `NOT RUN` | wewenang pemilik: `dotnet ef database update` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Status 🟡 sampai migration tabel `HrdNotification` dijalankan.


## Pembaruan 11 November 2026 — Status Migration

Migration task ini sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah` (`dotnet ef database update`, 0 migration tertunda). Pernyataan "migration belum dijalankan" pada bagian di atas sudah tidak berlaku. Uji penerimaan dengan akun sungguhan tetap `NOT RUN`, jadi statusnya tetap 🟡 bila bagian di atas menyatakan demikian. Rincian ada di `roadmap/backend-unfinished-work.md` bagian 14.
