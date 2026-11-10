# Laporan Perubahan Backend — `BE-OPT-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-042` |
| Judul | Penjualan langsung |
| Slice | `MVP-3` — `EPIC OPT-05` (menutup `MVP-3` backend) |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-042` |
| Trace | `FR-OPT-050`; `OPT-DEC-020`, `046`; `OPT-AC-010`, `029`; `UAT-10`, `11` |
| Contract version | `OPT-STATE-v1` bagian 3, `OPT-VAL-v1` `OPT-VAL-042`, `049`, `050` — `approved` sisi Optik |
| Penyetuju modul lain | Tidak ada perubahan kode modul lain |
| Dependency | `BE-OPT-040` ✅ |
| Klasifikasi | `SMALL` — perluasan service pesanan; tanpa endpoint baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/Services/OptOrderService.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003`, `030`..`041` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Optik belum bisa menjual barang tanpa pengerjaan, misalnya cairan lensa kontak, kacamata baca jadi, atau lensa
kontak. Pembuatan pesanan `DirectSale` sebelumnya ditolak (`BE-OPT-032`).

---

## 2. Proses bisnis

**Contoh (`UAT-10`):** Pak Joko (samaran) membeli 1 botol cairan lensa kontak Rp 85.000.

1. **Buat penjualan langsung** di kunjungan Optik. Tanpa resep, minimum uang muka 0%.
2. **Tagihkan.** Satu baris tagihan Rp 85.000. Status tetap `Draf` (tertagih); stok belum dipotong.
3. **Kasir menerima Rp 85.000.**
4. **Serahkan.** Identitas dicocokkan, stok cairan berkurang 1 **dalam transaksi yang sama** dengan perubahan status, dan status menjadi `Diserahkan` tanpa melewati pengerjaan (`OPT-AC-010`). Tagihan difinalisasi dan kunjungan ditutup.
5. **Lensa kontak (`UAT-11`, `OPT-AC-029`).** Wajib menautkan resep lensa kontak yang masih berlaku, termasuk lensa kosmetik. Cairan pembersih dan aksesoris tetap bebas resep.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Lensa kontak tanpa resep, dengan resep kacamata, atau dengan resep kedaluwarsa | `409 OPT-VAL-042` "Lensa kontak wajib memakai resep lensa kontak yang masih berlaku." |
| Lensa kacamata pada penjualan langsung | `400` "Lensa kacamata dijual lewat pesanan berpengerjaan, bukan penjualan langsung." |
| Serahkan sebelum lunas | `409 OPT-VAL-050` "Tagihan belum lunas. Sisa Rp 85.000." |
| Serahkan sebelum ditagihkan | `409 OPT-VAL-057` |
| Kirim penjualan langsung ke pengerjaan | `409 OPT-VAL-057` |
| Stok kurang saat diserahkan | `409 OPT-VAL-049` "Stok aksesoris & cairan CAIRAN-CL di depo Optik tidak cukup."; status, stok, dan tagihan tidak berubah |
| Batal sebelum diserahkan | Diterima; baris tagihan di-void (masih `ORDERED`) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OPT-STATE-v1` bagian 3 (baris penjualan langsung), `OPT-DEC-020`/`046`; `OptOrderService` (buat, tagihkan, serahkan, validasi isi), `OptStockGateway`, `OptOrderPaymentGate` (penjualan langsung tidak termasuk pesanan berpengerjaan pada penahan `OPT-VAL-047`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Services/OptOrderService.cs` | **Diperbarui.** `DirectSale` diterima; resep hanya wajib untuk lensa kontak; lensa kacamata ditolak pada penjualan langsung; minimum uang muka 0%; tagihkan tetap `Draf`; serah dari `Draf` tertagih dengan potong stok dalam transaksi |
| `Tests/QuilvianSystemBackend.OpticalTests/Be042DirectSaleTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 5 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tanpa endpoint baru. `POST /orders` kini menerima `OrderKind = DirectSale`; `bill`, `hand-over`, dan `cancel` melayani penjualan langsung |
| Database | Tanpa migration |
| Keamanan/Auth | Izin yang sama dengan pesanan (`OptOrder : Create/Bill/HandOver/Cancel`) |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Perubahan perilaku pada grup Opt Order:

| Method | Path | Perubahan |
| --- | --- | --- |
| `POST` | `/orders` | `OrderKind = DirectSale` diterima; `PrescriptionVersionId` hanya wajib bila ada lensa kontak |
| `POST` | `/orders/{id}/bill` | Penjualan langsung tetap `Draft` dengan `BilledAt` terisi |
| `POST` | `/orders/{id}/hand-over` | Penjualan langsung tertagih diserahkan dari `Draft`; stok dipotong saat itu |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 216 lulus, 0 gagal (5 baru) | `PASS` | Keluaran `dotnet test` |
| `UAT-10` / `OPT-AC-010` | Draf → tertagih (Draf, `ORDERED` Rp 85.000, stok tetap 3) → belum lunas ditolak → lunas → `HandedOver`, stok 3→2, baris `HANDED_OVER`, tagihan final, kunjungan `Completed` | `PASS` | Uji rantai |
| `UAT-11` / `OPT-AC-029` | Tanpa resep, resep kacamata, resep kedaluwarsa → `OPT-VAL-042`; resep lensa kontak berlaku → diterima; cairan tanpa resep → diterima | `PASS` | Uji |
| Lensa kacamata, kirim ke pengerjaan, serah sebelum tagih | Ditolak | `PASS` | Uji |
| Stok kurang saat serah | `409`; tetap `Draf`; nol mutasi stok; baris tetap `ORDERED` | `PASS` | Uji |
| Batal tertagih | Baris tagihan `VOIDED` | `PASS` | Uji |
| Regresi pesanan berpengerjaan (`Be032OrderTests`) | 15 lulus | `PASS` | Uji |
| Uji mutasi: potong stok saat serah dibuang + tagihkan memindah ke `AwaitingDownPayment` | 3 uji gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan terpisah) | Terkena 3 aturan; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:** PostgreSQL dan backend yang menyala.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `OPT-AC-010` | Terpenuhi di harness | Uji `UAT_10_…` |
| `OPT-AC-029` | Terpenuhi di harness | Uji `UAT_11_…` |
| `UAT-10`, `11` | Terpenuhi di harness | Uji |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan pelaksanaan | (a) Lensa kacamata tidak boleh dijual langsung, karena selalu butuh pengerjaan. (b) Penjualan langsung tidak ikut aturan sementara satu pesanan berpengerjaan (`OPT-VAL-047`), tetapi lunas tetap dinilai per tagihan kunjungan (`BE-OPT-040`). (c) Riwayat mencatat "Ditagihkan" sebagai baris `Draf → Draf`, agar waktu penagihan terlihat |
| Risiko tersisa | Belum dicoba di PostgreSQL. Barang penjualan langsung tidak punya kunci idempoten saat **Buat** (sama dengan `BE-OPT-032`) |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Status Git | Tambahan task ini: `M` roadmap, traceability; `??` laporan ini; `OptOrderService.cs` masih berstatus belum dilacak sejak `BE-OPT-032`. Berkas uji diabaikan Git |
| Langkah berikutnya | `MVP-3` backend selesai. Lanjut `MVP-4`: `BE-OPT-050` (keluhan garansi + remake tanpa tagihan), `051` (jasa medis), `052` (laporan), `053` (uji menyeluruh) |
