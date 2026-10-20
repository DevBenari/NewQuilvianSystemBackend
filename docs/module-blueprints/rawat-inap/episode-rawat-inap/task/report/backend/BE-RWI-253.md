# Laporan Pelaksanaan Task Backend: BE-RWI-253

| Metadata | Nilai |
| --- | --- |
| **Task ID** | `BE-RWI-253` |
| **Blueprint / Sub-modul** | `RWI-BP-001` / `episode-rawat-inap` (bentuk `COMPOSITE`) |
| **Delivery Planning ID** | `TRF-DEL-20261019-01` |
| **Kontrak Target** | `0.17.0` (Status: `APPROVED_DESIGN_PLANNED`) |
| **Tanggal Eksekusi** | 20 Oktober 2026 |
| **Branch Backend** | `MHamzah` |
| **Status Penyelesaian** | `✅ SELESAI (KODE SUMBER DIPERBARUI)` |

---

## 1. Backend Governance Preflight

- **Area / Module / Submodule**: `HealthServices` / `InPatientManagement` / `episode-rawat-inap`.
- **Owner & Entity Prefix**: `InPatientManagement` / `Inp`.
- **Keberlakuan**: `TOUCHED LEGACY` (Pembaruan algoritma pengurutan kueri antrean transfer pada `InpAdmissionTransferService.cs`).
- **Status Registry**: `APPROVED_DESIGN_PLANNED` (Kontrak target `0.17.0`; Bab 18 API Contract, Bab 20 Backend Architecture).
- **QBE Conformance**: `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001`, `QBE-LOG-001`, `QBE-AUD-001`.
- **Kebijakan Pengujian**: `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.
- **Kebijakan Build & Migrasi**: *Bypassed per instruksi eksplisit pengguna* ("tidak perlu melakukan dotnet build , saya lakukan dotnet build secara mandiri saja. Cukup Implementasi Code saja. jika ada yang ingin di lakukan migration buatkan file migration ya saja , nanti saya tinggal lakukan migration with command terminal").

---

## 2. Ringkasan Kebutuhan dan Desain

Sesuai dengan keputusan bisnis `RWI-DEC-338`, `RWI-DEC-339`, `RWI-DEC-340`, dan requirement `FR-TRF-008`:
1. Pengurutan daftar antrean transfer pasien IGD dan Poliklinik menuju rawat inap (`GetPagedTransfersAsync`) diubah dari pengurutan waktu sederhana menjadi pengurutan multi-kriteria bertingkat yang deterministik di sisi server.
2. Tingkat 1 (`OrderByDescending(x => x.IsCito)`): Pasien gawat darurat (CITO) IGD mutlak menempati urutan teratas antrean demi keselamatan klinis (*clinical safety first*), mengalahkan seluruh pasien member maupun reguler.
3. Tingkat 2 (`ThenByDescending(x => x.IsPriorityMember)`): Pasien yang memiliki keanggotaan aktif CRM dan berhak atas antrean prioritas (`PriorityQueue == true`) berada di atas kelompok pasien reguler.
4. Tingkat 3 (`ThenBy(x => x.MemberPriorityLevel ?? int.MaxValue)`): Di antara sesama pasien member prioritas, tier yang lebih tinggi (angka `PriorityLevel` lebih kecil, misal Level 1 Platinum > Level 2 Gold > Level 3 Silver) didahulukan. Pasien non-member mendapatkan fallback `int.MaxValue`.
5. Tingkat 4 (`ThenBy(x => x.DecidedAt)`): Di antara pasien dengan tingkat prioritas yang setara, pasien yang waktu keputusannya lebih dahulu diterbitkan (FIFO tertua) didahulukan.
6. Tingkat 5 (`ThenBy(x => x.TransferId)`): Tie-breaker deterministik terakhir untuk menjamin kestabilan paginasi dan mencegah item berpindah-pindah antar halaman.
7. Tidak ada pemesanan tempat tidur otomatis (*no auto-booking*); pemilihan kamar dan tempat tidur tetap menjadi wewenang manual petugas loket admisi rawat inap (`RWI-AC-489`).

---

## 3. Rincian Perubahan Kode Sumber

### 3.1 `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs`
Pada method `GetPagedTransfersAsync`, pengurutan kueri `itemsQuery` diperbarui:
```csharp
// SEBELUM:
// var items = await itemsQuery.OrderBy(x => x.DecidedAt).ThenBy(x => x.TransferId)
//     .Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize)
//     .ToListAsync(cancellationToken);

// SESUDAH (Kontrak 0.17.0 - RWI-DEC-338 / BE-RWI-253):
var items = await itemsQuery
    .OrderByDescending(x => x.IsCito)
    .ThenByDescending(x => x.IsPriorityMember)
    .ThenBy(x => x.MemberPriorityLevel ?? int.MaxValue)
    .ThenBy(x => x.DecidedAt)
    .ThenBy(x => x.TransferId)
    .Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize)
    .ToListAsync(cancellationToken);
```

---

## 4. Status Database & Migrasi EF Core

- **Kebutuhan Kolom Baru**: **TIDAK ADA (0 Migrasi EF Core Baru)**.
- **Dampak Skema Database**: Nol tabel baru, nol kolom baru, dan nol migrasi EF Core. Seluruh field pengurutan berasal dari DTO proyeksi relasional yang telah disediakan pada task `BE-RWI-252`.
- Tidak diperlukan eksekusi migrasi database.

---

## 5. Matriks Acceptance Criteria & Bukti Verifikasi

| ID Kriteria | Deskripsi Kriteria | Target Verifikasi | Status | Catatan / Bukti |
| --- | --- | --- | :---: | --- |
| `RWI-AC-482` | Urutan deterministik: CITO > Member Prioritas > Reguler FIFO | `InpAdmissionTransferService.cs:48` | `TERVERIFIKASI` | Multi-level OrderBy diterapkan: CITO DESC → Member Prioritas DESC → Level ASC → DecidedAt ASC → TransferId ASC |
| `RWI-AC-489` | Pemilihan kamar tetap manual, tanpa auto-booking server | `InpAdmissionTransferService.cs` | `TERVERIFIKASI` | Alur eksekusi `ExecuteTransferAdmissionCoreAsync` tetap mensyaratkan `BedId` eksplisit dari petugas loket admisi |
| `RWI-AC-490` | Expired di bangsal tidak mencabut atau downgrade kamar | Policy / Invariant | `TERVERIFIKASI` | Evaluasi keanggotaan hanya dijalankan saat antrean transfer kamar; tidak ada proses latar belakang yang menurunkan kelas bed bangsal |
| `RWI-AC-491` | Bebas diskriminasi asuhan klinis dan non-interference | Architecture / Scope | `TERVERIFIKASI` | Logika pengurutan diisolasi pada query transfer kamar rawat inap; tidak mengintervensi rekam medis, visite, atau resep obat |
| `git diff --check` | Pemeriksaan whitespace & conflict marker | CLI git | `PASS` | Kode bersih tanpa conflict marker atau trailing whitespace issue |
| Kompilasi & Build | `dotnet build` mandiri oleh pengguna | CLI dotnet | `DITANGGUHKAN MANDIRI` | Sesuai instruksi eksplisit pengguna, build dan migrasi dijalankan mandiri oleh pengguna |

---

## 6. Berkas yang Disentuh

1. `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs`
2. `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-253.md` (Berkas laporan ini)

---

## 7. Penyerahan ke Lapisan Frontend

Dengan tuntasnya **`BE-RWI-252`** dan **`BE-RWI-253`**, seluruh kontrak backend `0.17.0` untuk fitur **Prioritas Kamar Pasien Member di Antrean Transfer & Lencana Tier** telah siap 100% untuk dikonsumsi oleh lapisan antarmuka frontend:
- **`FE-RWI-268`**: Visualisasi lencana CITO (Merah) dan Tier Membership (Kuning/Emas) pada Tabel Antrean Transfer Rawat Inap (`InpatientAdmissionTransferList.jsx`).
- **`FE-RWI-269`**: Lencana CITO & Tier pada Header Langkah Admisi PPRI dan penegakan isolasi privasi pada Denah Bed Management.
- **`FE-RWI-270`**: Lencana Tier pada hasil pencarian No. RM dan verifikasi keanggotaan CRM pasien lama.
