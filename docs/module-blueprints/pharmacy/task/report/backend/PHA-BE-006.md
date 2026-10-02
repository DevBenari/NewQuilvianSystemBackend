# PHA-BE-006 — Keadaan Finansial Terbaca pada Layar Kerja

## Ringkasan untuk Pembaca Umum

Ketika sebuah resep tidak dapat dilanjutkan, petugas farmasi sebelumnya tidak punya cara
mengetahui sebabnya dari layar tempat ia bekerja. Yang tersisa hanyalah menelepon kasir sambil
menebak. Task ini menambahkan keadaan finansial resep — sudah beres atau belum, jenis kebersannya,
alasan penahanannya bila tertahan, dan seberapa segar salinannya — pada dua tampilan yang memang
sudah dibuka petugas setiap hari: layar kerja resep dan halaman detail satu resep.

Tidak ada halaman baru, tidak ada tombol baru, dan tidak ada hak akses baru. Isinya keterangan,
bukan kewenangan: menampilkannya tidak memberi siapa pun kemampuan mengubahnya.

Satu hal lagi yang menyertainya: saat kedua layar itu dibuka, surat clearance dari Billing yang
belum tersalin langsung dikonsumsi di dalam proses. Resep yang tagihannya baru saja dibereskan
kasir karena itu sudah berada di antrean apoteker ketika layarnya terbaca — tanpa petugas menekan
apa pun. Tombol "sinkronkan" sengaja tidak dibuat; tombol semacam itu mengundang kebiasaan
menekannya berulang sampai hasilnya menyenangkan.

---

- TASK ID: PHA-BE-006
- TASK TYPE: Fitur (penambahan aditif pada response yang sudah ada + pemicu konsumsi in-process)
- COMPLEXITY: LOW
- MODEL: Claude Opus 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/PharmacyManagement/**`
- FILES INSPECTED:
  - `docs/module-blueprints/pharmacy/roadmap/backend-roadmap.md` (`PHA-BE-006`)
  - `docs/module-blueprints/pharmacy/contracts/api-contract.md` (`PHA-API-CLEARANCE-v1`)
  - `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs`, `PrescriptionWorkspaceController.cs`
  - `Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkspaceService.cs`
- FILES CHANGED:
  - **Dibuat**: `DTOs/PrescriptionFinancialClearanceDtos.cs`
  - **Diperbarui**: `DTOs/PrescriptionDtos.cs` (`PrescriptionDetailResponse.FinancialClearance`)
  - **Diperbarui**: `DTOs/PrescriptionWorkspaceDtos.cs` (`PrescriptionWorkspaceResponse.FinancialClearance`)
  - **Diperbarui**: `Services/PrescriptionFinancialClearanceService.cs` (`DescribeAsync`, `ClearanceHoldReasonCodes`)
  - **Diperbarui**: `Services/PrescriptionWorkspaceService.cs` (konsumsi in-process + pengisian field)
  - **Diperbarui**: `Controllers/PrescriptionController.cs`, `Controllers/PrescriptionWorkspaceController.cs`
- IMPLEMENTATION:
  1. `PrescriptionFinancialClearanceResponse` memuat keadaan clearance, hasil finansial, kode
     sebab, penanda `IsCleared`/`IsKnown`, kode dan kalimat alasan penahanan, kesegaran salinan,
     nomor versi, serta waktu berlaku.
  2. `DescribeAsync` membaca salinan secara `AsNoTracking`. Resep tanpa salinan dijawab
     `UNKNOWN` beserta alasannya — **200**, bukan `404`, dan bukan galat.
  3. Kalimat alasan penahanan disalin **kata demi kata** dari `PHA-VAL-CLEARANCE-v1`, beserta
     kodenya (`PHA_CLR_ON_HOLD`, `PHA_CLR_UNKNOWN`, `PHA_CLR_STALE`, `PHA_CLR_NOT_CLEARED`).
  4. Pemicu konsumsi dipasang pada jalur baca: `GET /prescriptions/{id}`,
     `GET /prescription-workspaces/{prescriptionId}`, dan `GET .../by-consultation/{id}`. Pada
     jalur konsultasi, identitas resep dicari lebih dulu dengan bacaan ringan supaya konsumsi
     berjalan **sebelum** isinya dibaca — bila dibalik, keadaan yang terbaca layar tertinggal
     satu langkah.
- API CONTRACT IMPACT: **Nol endpoint baru**, nol field dihapus, nol field berubah arti, nol butir hak akses baru. Seluruhnya aditif sesuai `PHA-API-CLEARANCE-v1`.
- DATABASE IMPACT: Nol perubahan skema. Jalur baca melakukan penulisan **hanya** lewat konsumsi surat, dan penulisannya idempoten.
- SECURITY IMPACT: Field bersifat keterangan. Tidak ada permukaan yang memungkinkan petugas mengubah keadaan finansial.
- VALIDATION:
  | Command/Check | Result | Classification | Evidence/Note |
  | --- | --- | --- | --- |
  | `dotnet build -p:SkipMigrationMetadata=true` | `0 Error` | VERIFIED | 23,9 detik |
  | `PHA-AT-CLR-10` (keadaan finansial terbaca) | Terpenuhi pada source | VERIFIED (Logic) | Kedua response membawa `FinancialClearance`; resep tanpa salinan tetap `200` dengan penanda `UNKNOWN` |
  | Review diff dan scope | Selesai | VERIFIED | Nol endpoint baru; nol `AccessAction`/`AccessPermission` baru |
  | Runtime | **Belum** | NOT VERIFIED | Menunggu migration diterapkan pada database uji |
- WARNINGS: Jalur baca kini melakukan penulisan ketika ada surat yang belum tersalin. Itu memang bentuk yang diminta kontrak (konsumsi in-process saat dibutuhkan), tetapi perlu diketahui pembaca berikutnya: `GET` di sini tidak sepenuhnya bebas efek samping.
- KNOWN ISSUES: Daftar resep (`GET /prescriptions`) belum memicu konsumsi — hanya detail dan layar kerja. Menyapu seluruh halaman daftar akan menarik surat milik resep yang tidak sedang dibuka petugas; bila antrean perlu menyegarkan dirinya sendiri, itu keputusan tersendiri.
- NEXT TASKS: `PHA-FE-002` (alasan penahanan terbaca pada layar kerja Farmasi).
