# Laporan Perubahan Backend — `BE-OPT-041`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-041` |
| Judul | Batal sesudah pengerjaan, tidak diambil, serah susulan |
| Slice | `MVP-3` — `EPIC OPT-06` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-041` |
| Trace | `FR-OPT-061`, `062`; `OPT-DEC-010`, `018`, `038`, `049`, `060`; `OPT-AC-007`, `017`, `021`; `UAT-14`, `15` |
| Contract version | `OPT-API-v1` grup Opt Order, `OPT-STATE-v1` bagian 3, `OPT-VAL-v1` `053`..`056`, `OPT-INT-v1` `INT-OPT-02`/`04`/`05`/`07` — `approved` sisi Optik |
| Penyetuju modul lain | Tidak ada perubahan kode modul lain. Bentuk koreksi tagihan ditetapkan Billing (`OPT-APR-001`/`012`, Yasmina) |
| Dependency | `BE-OPT-034` ✅, `BE-OPT-040` ✅ |
| Klasifikasi | `MEDIUM` — 3 endpoint, 3 tindakan service, perluasan serah |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003`, `030`..`040` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-LOG-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Pesanan yang sudah dikerjakan belum bisa dibatalkan dengan persetujuan Kepala Optik, kacamata yang tidak
diambil tidak bisa ditutup, dan pasien yang datang belakangan tidak bisa menerima kacamatanya.

---

## 2. Proses bisnis

1. **Ajukan batal (`UAT-14`).** **Contoh:** Pak Andi (samaran) memesan frame Rp 450.000 dan lensa custom Rp 600.000 yang sudah dikirim ke Lab Lensa X. Ia pindah kota. Petugas mengajukan batal dengan alasan "pasien pindah domisili". Setiap lensa stok wajib ditandai sudah/belum dipotong. Status pesanan belum berubah.
2. **Kepala Optik menyetujui.** Dalam satu transaksi, frame kembali ke stok depo (batch yang sama) dan barisnya batal; lensa custom **tetap ditagih**. Pesanan menjadi `Batal`. Di tagihan, frame dikredit Rp 450.000 (adjustment menunggu persetujuan Billing), lensa tetap Rp 600.000, dan uang muka terpakai (`OPT-AC-017`).
3. **Lensa stok (`OPT-AC-021`).** Ditandai "belum dipotong" → kembali ke stok dan barisnya dikredit. Ditandai "sudah dipotong" → tetap ditagih seperti lensa custom.
4. **Kepala Optik menolak.** Catatan wajib; pesanan tetap berjalan, dan pengajuan baru boleh dibuat.
5. **Tidak diambil (`OPT-DEC-018`, `049`).** Kacamata siap sejak lebih dari 90 hari (sesuai pengaturan) ditutup `Tidak Diambil`. Semua barang tetap ditagih, frame tidak dibongkar ke stok, dan sisa kurang bayar menjadi piutang pasien di Billing.
6. **Serah susulan.** Pasien datang, melunasi, dan menerima kacamatanya. Riwayat mencatat "Penyerahan susulan", tagihan difinalisasi, dan kunjungan ditutup.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Petugas membatalkan langsung pesanan `Dalam Pengerjaan` (`UAT-15`, `OPT-AC-007`) | `409` "Pembatalan sesudah pengerjaan perlu persetujuan Kepala Optik." |
| Alasan kosong | `400 OPT-VAL-053` |
| Lensa stok belum ditandai | `400 OPT-VAL-054` "Tandai apakah lensa stok sudah dipotong." |
| Pengajuan kedua saat yang pertama menunggu | `409 OPT-VAL-055` "Masih ada pengajuan batal yang menunggu keputusan." (juga dijaga unique index) |
| Tolak tanpa catatan | `400` "Tuliskan alasan penolakan." |
| Memutuskan pengajuan yang sudah diputuskan | `409 OPT-VAL-057` |
| Tutup tidak diambil sebelum 90 hari | `409 OPT-VAL-056` "Pesanan baru bisa ditutup sesudah 90 hari sejak siap diambil." |
| Serah susulan saat belum lunas | `409 OPT-VAL-050` |
| Stok gagal dikembalikan | Seluruh keputusan batal (status, baris, stok) di-rollback |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OPT-STATE-v1` bagian 3, `OPT-VAL-v1` `053`..`056`, `OPT-DEC-010`/`018`/`038`/`049`; model dan configuration `OptOrderCancellation` (unique index pengajuan `Requested`); `OptStockGateway.ReturnAsync`; jembatan Billing (`BuildCancellationPlanAsync`: baris yang tidak boleh di-void normal menjadi adjustment kredit); `BillingFinalizationService.AreAllOrdersCompleteAsync` (menilai status baris aktif).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Services/OptOrderService.cs` | **Diperbarui.** `RequestCancellationAsync`, `DecideCancellationAsync`, `CloseNotCollectedAsync`, `CloseBillingAsync`; `HandOverAsync` menerima `Tidak Diambil` (serah susulan); proyeksi memuat pengajuan batal, tanda potong, waktu stok kembali, waktu tidak diambil |
| `Areas/HealthServices/OpticalManagement/Controllers/OptOrderController.cs` | **Diperbarui.** `POST /{id}/cancellation-requests`, `POST /{id}/cancellation-requests/{requestId}/decision`, `POST /{id}/close-not-collected` |
| `Areas/HealthServices/OpticalManagement/DTOs/OptOrderDtos.cs` | **Diperbarui.** Permintaan pengajuan/keputusan/tidak diambil; `OptOrderCancellationResponse`; ruas jawaban baru |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Diperbarui.** `OPT-VAL-054`..`056` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be041CancelNotCollectedTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 7 uji |
| `Tests/QuilvianSystemBackend.OpticalTests/OpticalPermissionRegistryTests.cs` | **Diperbarui.** Hitungan endpoint Optik 32 → 35 |
| `docs/module-blueprints/optik/00-interview-decisions.md` | **Diperbarui.** Satu baris penyegaran `INT-OPT-02`/`04` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint `OPT-API-v1` tersedia. Permintaan pengajuan batal memakai nama `OptOrderCancellationCreateRequest` (kontrak menyebut bentuk `OptOrderCancelRequest` + `Items[]`) dan membawa `ExpectedVersion` sesuai konvensi |
| Kontrak integrasi | **Tafsiran yang perlu dikunci:** pesanan yang berakhir tanpa diserahkan membawa seluruh baris tagihannya ke `NOT_COLLECTED`, lalu baris yang batal diterbitkan pembatalannya. Tanpa langkah pertama, baris `IN_WORK`/`READY` yang dikredit tetap aktif dengan status belum selesai dan menahan finalisasi tagihan selamanya |
| Database | Tanpa migration. Menulis `OptOrderCancellation`, `OptOrderItem.IsLensCut/StockReturnedAt/CancelledAt`, `OptOrder.NotCollectedAt`; stok lewat `OptStockGateway` |
| Keamanan/Auth | `OptOrder : RequestCancel` (petugas), `DecideCancel` dan `CloseNotCollected` (Kepala Optik); lolos `PermissionRegistryValidator` (35 endpoint Optik) |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/optical-management/orders`

| Method | Path | Isi | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/cancellation-requests` | `Reason`, `Items[]{ItemId, IsLensCut}`, `ExpectedVersion` | `OptOrder : RequestCancel` |
| `POST` | `/{id}/cancellation-requests/{requestId}/decision` | `Approve`, `DecisionNote?`, `ExpectedVersion` | `OptOrder : DecideCancel` |
| `POST` | `/{id}/close-not-collected` | `Reason`, `ExpectedVersion` | `OptOrder : CloseNotCollected` |
| `POST` | `/{id}/hand-over` | Kini juga untuk serah susulan dari `Tidak Diambil` | `OptOrder : HandOver` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 211 lulus, 0 gagal (7 baru) | `PASS` | Keluaran `dotnet test` |
| `UAT-15` / `OPT-AC-007` batal langsung | `409` kalimat persis | `PASS` | Uji |
| `UAT-14` / `OPT-AC-007` / `017` | `Batal`; frame batal dan stok 4→5; lensa custom aktif; kedua baris `NOT_COLLECTED`; satu adjustment kredit Rp 450.000 | `PASS` | Uji rantai Farmasi + producer + jembatan Billing |
| `OPT-AC-021` (dua kasus) | Belum dipotong: lensa kembali (stok 5) dan dikredit (total kredit Rp 1.050.000). Sudah dipotong: lensa tetap (stok 4), kredit hanya frame Rp 450.000 | `PASS` | Uji teori |
| `OPT-VAL-053`..`055`, tolak tanpa catatan, tolak, putuskan ulang, ajukan lagi | Lulus | `PASS` | Uji |
| `OPT-VAL-056` dan tutup tidak diambil | Ditolak sebelum 90 hari; sesudahnya `NotCollected`, baris `NOT_COLLECTED`, tanpa kredit, stok tidak kembali, tagihan belum final | `PASS` | Uji |
| Serah susulan | Belum lunas `OPT-VAL-050`; sesudah lunas `HandedOver`, riwayat "Penyerahan susulan", tagihan final, kunjungan `Completed` | `PASS` | Uji |
| Uji mutasi: revisi `NOT_COLLECTED` dibuang + tanda potong diabaikan | 3 uji gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` atas 4 berkas | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan terpisah) | Terkena 3 aturan; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:** persetujuan adjustment kredit di Billing (wewenang Billing). "Sisa piutang Rp 200.000" pada contoh `OPT-AC-017` baru terlihat sesudah Billing menyetujui kreditnya. PostgreSQL belum dicoba.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-007` | Terpenuhi di harness | Uji `UAT_15_…`, `UAT_14_…` |
| (1) `OPT-AC-017` | Sisi Optik terpenuhi (baris frame dikredit, lensa tetap); angka piutang menunggu persetujuan kredit di Billing | Uji `UAT_14_…` |
| (1) `OPT-AC-021` | Terpenuhi di harness | Uji teori |
| (2) `UAT-14`, `15` | Terpenuhi di harness | Uji |
| (3) `OPT-DEC-049` | Terpenuhi di harness | Uji tutup tidak diambil + serah susulan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan di Billing | Adjustment kredit atas baris yang sudah dikerjakan **tidak mengubah status baris**, sedangkan finalisasi menilai status baris aktif. Untuk Optik hal ini diatasi di sisi Optik (revisi `NOT_COLLECTED` lebih dulu). Modul lain yang membatalkan baris sesudah dikerjakan bisa mengalami penahanan yang sama; perlu disampaikan ke pemilik Billing (Yasmina) |
| Keputusan pelaksanaan | (a) Hanya lensa kacamata yang bisa "tetap ditagih"; frame dan barang lain selalu batal dan kembali ke stok bila pernah dipotong. (b) Keputusan menolak tidak mengubah status pesanan, tetapi menaikkan `Version`. (c) Batas 90 hari dihitung dari tanggal WIB saat siap diambil |
| Risiko tersisa | Belum dicoba di PostgreSQL. Bila penerbitan ke Billing gagal sesudah commit, jawaban membawa `BillingMessage`; fakta yang sudah tercatat dikirim ulang producer |
| Perubahan sampingan | `NONE` pada source aplikasi. Probe QBE sementara dibuat dan dihapus |
| Status Git | Tambahan task ini: `M` `OptOrderService.cs`, `OptOrderController.cs`, `OptOrderDtos.cs`, `OptOperationResult.cs`, roadmap, traceability, `00-interview-decisions.md`; `??` laporan ini. Berkas uji diabaikan Git |
| Langkah berikutnya | `BE-OPT-042` (penjualan langsung + syarat resep lensa kontak) menutup `MVP-3`. Sesudah itu `MVP-4`: `BE-OPT-050` (keluhan garansi + remake), `051` (jasa medis), `052` (laporan), `053` (uji menyeluruh) |
