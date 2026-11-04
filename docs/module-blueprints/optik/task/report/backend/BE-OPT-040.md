# Laporan Perubahan Backend — `BE-OPT-040`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-040` |
| Judul | Penyerahan |
| Slice | `MVP-3` — `EPIC OPT-06` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-040` |
| Trace | `FR-OPT-060`, `081`; `OPT-DEC-041`, `043`, `048`; `OPT-AC-009`, `025`; `UAT-12`, `13` |
| Contract version | `OPT-API-v1` grup Opt Order, `OPT-STATE-v1` bagian 3, `OPT-VAL-v1` `OPT-VAL-050`..`052`, `OPT-INT-v1` `INT-OPT-02`/`04` — `approved` sisi Optik |
| Penyetuju modul lain | Tidak ada perubahan kode modul lain. Memanggil `BillingFinalizationService.TryCompleteSettledInvoiceAsync` (`OPT-APR-012`, Yasmina) |
| Dependency | `BE-OPT-034` ✅ |
| Klasifikasi | `MEDIUM` — 1 endpoint, 1 tindakan service, pemicu finalisasi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003`, `030`..`034` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-LOG-001` (log tidak memuat nama wakil) |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Kacamata yang sudah siap belum bisa diserahkan. Akibatnya masa garansi tidak pernah mulai, dan tagihan serta
kunjungan Optik tidak pernah tertutup karena barisnya tetap "belum selesai" (`OPT-DEC-043`).

---

## 2. Proses bisnis

**Contoh (`UAT-12`):** pesanan Ibu Sari (samaran) Rp 1.050.000 sudah siap diambil dan dilunasi.

1. **Lunas sebelum serah, kunjungan tetap terbuka.** Begitu Ibu Sari melunasi, tagihan **tidak** difinalisasi karena baris pesanannya masih `READY` (`OPT-AC-025`).
2. **Serahkan.** Petugas mencentang "identitas dicocokkan" dan mencatat *fitting* "gagang dikencangkan". Status menjadi `Diserahkan`. Masa garansi dibekukan dari pengaturan: ukuran 30 hari, cacat lensa 6 bulan, cacat frame 6 bulan (`OPT-DEC-041`). Perubahan pengaturan sesudahnya tidak mengubah pesanan ini.
3. **Tagihan final, kunjungan ditutup.** Baris tagihan berubah ke `HANDED_OVER`. Karena tidak ada pesanan lain yang berjalan di kunjungan itu, Billing diminta memfinalisasi tagihan yang sudah lunas, dan finalisasi itu menutup kunjungan (`Completed`).
4. **Diambil wakil (`OPT-DEC-048`).** Dewi (anak) mengambil; riwayat mencatat "Diambil oleh Dewi (anak)".

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Tagihan masih bersisa Rp 525.000 (`UAT-13`, `OPT-AC-009`) | `409 OPT-VAL-050` "Tagihan belum lunas. Sisa Rp 525.000." |
| Identitas belum dicocokkan | `400 OPT-VAL-051` "Cocokkan identitas pasien sebelum menyerahkan." |
| Nama wakil tanpa hubungan, atau sebaliknya | `400 OPT-VAL-052` "Isi nama dan hubungan pengambil." |
| Masih ada pesanan lain yang berjalan di kunjungan yang sama | Pesanan ini diserahkan; tagihan dan kunjungan tetap terbuka (`InvoiceFinalized = false`) |
| Pesanan belum `Siap Diambil`, atau sudah diserahkan | `409 OPT-VAL-057` |
| `ExpectedVersion` basi | `409 OPT-VAL-058` |
| Status bayar tidak terbaca | `503` (*fail-closed*) |
| Finalisasi Billing gagal atau belum memenuhi syarat | Penyerahan tetap tersimpan; kasir dapat memfinalisasi manual |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OPT-STATE-v1` bagian 3, `OPT-VAL-v1` `050`..`052`, `OPT-DEC-041`/`043`/`048`; `BillingFinalizationService.TryCompleteSettledInvoiceAsync` dan `BuildReadinessAsync` (semua baris selesai, kalkulasi terkini, sisa nol); `OptSetting` (masa garansi); model `OptOrder` (kolom serah dan garansi).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Services/OptOrderService.cs` | **Diperbarui.** `HandOverAsync`; proyeksi memuat data serah dan garansi beku |
| `Areas/HealthServices/OpticalManagement/Services/OptBillingGateway.cs` | **Diperbarui.** `TryCompleteVisitBillingAsync`; dependensi opsional `BillingFinalizationService` |
| `Areas/HealthServices/OpticalManagement/Controllers/OptOrderController.cs` | **Diperbarui.** `POST /{id}/hand-over` |
| `Areas/HealthServices/OpticalManagement/DTOs/OptOrderDtos.cs` | **Diperbarui.** `OptOrderHandOverRequest`; ruas jawaban serah, garansi, `InvoiceFinalized` |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Diperbarui.** `OPT-VAL-050`..`052` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be040HandOverTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 6 uji |
| `Tests/QuilvianSystemBackend.OpticalTests/Infrastructure/PesananUji.cs` | **Diperbarui** (diabaikan Git). Penolong stok dan kirim dipindah dari uji 034; `BillingFinalizationService` didaftarkan |
| `Tests/QuilvianSystemBackend.OpticalTests/Be034SendToWorkTests.cs`, `OpticalPermissionRegistryTests.cs` | **Diperbarui.** Penolong dipindah; hitungan endpoint 31 → 32 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /{id}/hand-over` sesuai `OPT-API-v1`. Tambahan: ruas serah, garansi beku, `InvoiceFinalized` pada jawaban |
| Kontrak integrasi | `INT-OPT-04` revisi `HANDED_OVER`; `INT-OPT-02` pemicu `TryCompleteSettledInvoiceAsync` sesudah pesanan terakhir terminal |
| Database | Tanpa migration |
| Keamanan/Auth | `OptOrder : HandOver`, lolos `PermissionRegistryValidator` (32 endpoint Optik). Nama wakil hanya di riwayat status, tidak di log |

---

## 4. Dokumentasi endpoint

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `api/v1/health-services/optical-management/orders/{id}/hand-over` | `HandoverIdentityVerified`, `ReceiverName?`, `ReceiverRelationship?`, `FittingNote?`, `ExpectedVersion` | `OptOrder : HandOver` |

Kode status: `200`; `400` identitas atau wakil; `404`; `409` belum lunas, status, versi; `503` status bayar tidak terbaca.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 204 lulus, 0 gagal (6 baru) | `PASS` | Keluaran `dotnet test` |
| `UAT-13` / `OPT-AC-009` | `409`, kalimat persis kontrak; status tetap `Siap Diambil` | `PASS` | Uji |
| `UAT-12` / `OPT-AC-025` | Lunas sebelum serah: kunjungan `Registered`, tagihan `OPEN`. Sesudah serah: `HandedOver`, garansi 30/6/6, baris `HANDED_OVER`, tagihan bukan `OPEN`, kunjungan `Completed` | `PASS` | Uji rantai dengan `BillingFinalizationService` asli |
| Garansi beku | Pengaturan diubah sesudahnya; pesanan tetap 30 hari | `PASS` | Uji |
| `OPT-DEC-048` | Identitas wajib; wakil setengah ditolak; wakil lengkap tercatat di riwayat | `PASS` | Uji |
| Pesanan lain masih berjalan | Diserahkan; tagihan dan kunjungan tetap terbuka | `PASS` | Uji |
| Status dan versi | Lulus | `PASS` | Uji |
| Uji mutasi: syarat lunas dibuang + pemicu finalisasi dibuang | `UAT-13` dan `UAT-12` gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` atas 5 berkas | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan terpisah) | Terkena 3 aturan; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:** PostgreSQL dan backend yang menyala. Pemeriksaan "masih ada pesanan berjalan" di sisi Optik juga dijaga oleh Billing sendiri (baris `ORDERED`/`IN_WORK`/`READY` menahan finalisasi), sehingga membuangnya tidak membuat uji gagal; kedua lapis itu sengaja ada.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-009` | Terpenuhi di harness | Uji `UAT_13_…` |
| (2) `UAT-12`, `13` | Terpenuhi di harness | Uji |
| (3) Pesanan terakhir terminal dan lunas → kunjungan `Completed` | Terpenuhi di harness | Uji `UAT_12_…` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan pelaksanaan yang perlu dikonfirmasi | **Lunas dinilai per tagihan kunjungan**, sesuai bunyi `OPT-AC-009` dan `OPT-VAL-050` ("Tagihan belum lunas"). Akibatnya, bila satu kunjungan punya dua pesanan, pesanan yang sudah siap baru bisa diserahkan setelah **seluruh** tagihan kunjungan lunas, termasuk pesanan kedua yang belum dikirim dan tarif pemeriksaan. Ini ketat tetapi aman; hilang saat alokasi per baris Billing (`OPT-APR-003`) tersedia |
| Cakupan | Serah susulan dari `Tidak Diambil` menyusul di `BE-OPT-041`; penyerahan penjualan langsung di `BE-OPT-042`. Keduanya akan memakai `TryCompleteVisitBillingAsync` yang sama |
| Risiko tersisa | Belum dicoba di PostgreSQL. Finalisasi otomatis bergantung pada kalkulasi Billing yang terkini; bila Billing menilai kalkulasi basi, tagihan tetap terbuka dan kasir memfinalisasi manual |
| Perubahan sampingan | `NONE` pada source aplikasi. Probe QBE sementara dibuat dan dihapus |
| Status Git | Tambahan task ini: `M` `OptOrderService.cs`, `OptBillingGateway.cs`, `OptOrderController.cs`, `OptOrderDtos.cs`, `OptOperationResult.cs`, roadmap, traceability; `??` laporan ini. Berkas uji diabaikan Git |
| Langkah berikutnya | `BE-OPT-041` (ajukan dan putuskan batal sesudah pengerjaan, tutup tidak diambil, serah susulan) |
