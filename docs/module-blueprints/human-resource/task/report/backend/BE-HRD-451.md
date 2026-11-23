# Laporan Perubahan Backend — `BE-HRD-451`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-451` |
| Judul | Daftar nama di balik angka dashboard (`GET /drilldown`) |
| Slice | `S-PKB` — Paket B Navigasi SDM / `MVP-73` · `EPIC HRD-124` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-451` |
| Trace | `FR-PKB-03`; `HRD-DEC-235`, `HRD-DEC-237`; Permission 33.2; `VR-PKB-01`, `VR-PKB-02`, `VR-PKB-03`; `AT-HRD-PKB-07`, `AT-HRD-PKB-08` |
| Contract version | `v29` — API 35.1, 35.6; State 33.1 — **approved** 23 November 2026 |
| Dependency | `BE-HRD-449` ✅, `BE-HRD-450` ✅ |
| Klasifikasi | `MEDIUM` — lima jenis daftar, validasi, privasi nomor lisensi |
| Task mode | `BACKEND` — instruksi pengguna 23 November 2026 |
| Target tulis | `WorkforceCore/Controllers/HrdWorkforceDashboardController.cs`, `Services/HrdWorkforceDashboardService.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7e4c3774` (branch `MHamzah`); source sudah di-commit pengguna di `9062bd61` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpetakan ke source; build 0 error, QBE Strict `PASS`, registri `IsValid=True`; uji API runtime 23 November 2026 `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `NEW CODE` (arketipe read-only / worklist) |
| QBE yang berlaku | `QBE-API-001`, `QBE-PERM-001`, `QBE-PAGE-001`, `QBE-VAL-001`, `QBE-DTO-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Angka di dashboard tidak berguna bila HR tidak tahu **siapa** di balik angka itu. Rina perlu mengklik "SIP sudah habis 2" dan langsung melihat nama dr. Dodi dan perawat Sari.

## 2. Proses bisnis

1. Layar memanggil `GET /drilldown?panel=…&bucket=…` (opsional `professionId`, `documentType`, `search`, `pageNumber`, `pageSize`).
2. Validasi berurutan: `pageSize` 1–100 dan `search` ≤ 100 karakter (`VR-PKB-03`); panel wajib dan dikenal (`VR-PKB-01`); kelompok sah untuk panelnya, `professionId` hanya untuk `PROFESSION`, `documentType` hanya untuk `DOCUMENT_EXPIRY` dan bernilai `SIP_STR`/`SPK_RKK`/`MANDATORY_CERTIFICATE` (`VR-PKB-02`). Gagal → `400` dengan pesan kontrak.
3. Daftar memakai **aturan kelompok yang sama persis** dengan `/summary`, sehingga jumlah baris = angka panel:

| Panel | Sumber baris | `detailLabel` | `referenceDate` / `daysRemaining` |
| --- | --- | --- | --- |
| `EMPLOYEE_OVERVIEW`, `ACTIVE_STATUS` | `MstEmployee` | "Pegawai aktif", "Masuk bulan ini", "Keluar bulan ini", "Nonaktif" | Tanggal masuk atau keluar / kosong |
| `PROFESSION` | `MstEmployee` aktif per profesi atau tanpa profesi | Nama profesi atau "Belum diisi" | Tanggal masuk / kosong |
| `CONTRACT_EXPIRY` | Kontrak menurut `ContractRenewalBuckets`; `PENDING_DECISION` dari usulan perpanjangan | "PKWT berakhir", "… lewat jatuh tempo", "Menunggu keputusan perpanjangan" | Tanggal akhir kontrak / sisa hari |
| `DOCUMENT_EXPIRY` | Baris dokumen yang sama dengan panel 5 | "SIP …0123", "SPK/RKK …0007", nama pelatihan | Tanggal kedaluwarsa / sisa hari |

4. Setiap baris memuat `workforceProfileId` untuk membuka Employee 360. Nama, nomor pegawai, dan profesi diambil dari `MstEmployee`; bila tidak ada, dari profil tenaga kerja.

**Contoh.** `panel=DOCUMENT_EXPIRY&bucket=EXPIRED&documentType=SIP_STR` → dua baris: dr. Dodi, "SIP …0123", kedaluwarsa 18 Desember 2026, `daysRemaining = -2`; perawat Sari, "SIP …8841".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`Shared/HumanResource/Helpers/HrCrossEmployeeLookup.cs` (pola pencarian ILIKE aman dan `ToPaged`), `MasterData/Workforce/Models/MstWorkforceProfile.cs`, `MstContractType.cs`, `LifecycleManagement/Models/HrdContractDecision.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/HrdWorkforceDashboardController.cs` | Action `GetDrilldown` (`GET drilldown`), atribut akses sama dengan `summary`; hasil tidak sah → `400` |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdWorkforceDashboardService.cs` | `GetDrilldownAsync` (validasi), daftar pegawai, kontrak, dan dokumen; label pegawai; pencarian profil; penyamaran nomor |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `GET /workforce-dashboard/drilldown` sesuai kontrak 35.1 |
| Database | Tidak ada |
| Keamanan/Auth | `HrdWorkforceDashboard : Read`. Nomor lisensi/SPK hanya 4 karakter terakhir; tanpa gaji, NIK, disiplin, kesehatan. Pemegang hak dashboard dapat melihat nama pemilik dokumen kedaluwarsa walau tanpa `ProfessionalLicense : Read` — disengaja (Permission 33.2) |

**Delta kecil:** untuk panel `PROFESSION`, profesi dapat dikirim lewat `bucket` (GUID atau `UNASSIGNED`) maupun `professionId`; bila keduanya dikirim dan berbeda → `400` `VR-PKB-02`.

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Workforce Dashboard

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-dashboard/drilldown` | Daftar nama di balik satu angka panel | `HrdWorkforceDashboard : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` penuh Paket B | 0 error; tanpa warning di berkas task | `PASS` | Log build 23 November 2026 |
| QBE Strict (19 berkas) | `PASS` | `PASS` | Keluaran checker |
| Validator registri | `IsValid=True`; kedua action dashboard memakai identitas `HrdWorkforceDashboard.Read` yang sama | `PASS` | Skrip `dotnet fsi` |
| Penelusuran: `panel=PROFESSION&bucket=EXPIRED` | `IsValidBucket` gagal → `400` "Panel atau kelompok dashboard tidak dikenal." | `PASS` | `GetDrilldownAsync` |
| Penelusuran: `pageSize=101` / `search` 101 karakter | `400` "Parameter halaman atau pencarian melebihi batas." | `PASS` | `GetDrilldownAsync` |
| Penelusuran: jumlah daftar = angka panel | Predikat yang sama (pegawai, `ContractRenewalBuckets`, baris dokumen yang sama) | `PASS` | Diff service |
| Uji API runtime di DB dev (`QuilvianNewDevHamzah`), 23 November 2026 — backend `9062bd61` di port lokal 5199, akun superadmin + akun non-admin: jumlah daftar vs angka panel | 29 kombinasi panel/kelompok (termasuk tiap profesi dan tiap jenis dokumen) — seluruhnya sama | `PASS` | Skrip uji API (27 skenario: 26 `PASS`, 1 gagal karena login akun uji atasan ditolak `401` — masalah akun, bukan kode) |
| Validasi runtime | `PROFESSION&EXPIRED`, panel `XYZ`, `documentType` pada panel pegawai → `400` "Panel atau kelompok dashboard tidak dikenal."; `pageSize=101`, `search` 101 karakter → `400` "Parameter halaman atau pencarian melebihi batas." | `PASS` | Skrip uji API |
| Isi baris | Baris memuat `workforceProfileId`, nomor dan nama pegawai, tanpa gaji/NIK; penyamaran nomor lisensi belum teruji runtime karena 0 lisensi di DB dev (dibuktikan lewat source `MaskReferenceNumber`) | `PASS` | Skrip uji API |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `DOCUMENT_EXPIRY/EXPIRED/SIP_STR` → pemilik SIP; nomor 4 karakter terakhir; tanpa gaji/NIK (`AT-HRD-PKB-07` bagian backend) | Terpenuhi | `MaskReferenceNumber`; DTO tanpa ruas sensitif |
| 2. `PROFESSION&EXPIRED` → `400` `VR-PKB-02`; panel tak dikenal → `400` `VR-PKB-01`; batas halaman/cari → `400` `VR-PKB-03` (`AT-HRD-PKB-08`) | Terpenuhi | Urutan validasi |
| 3. Kelima panel: jumlah daftar = angka `/summary` | Terpenuhi | Aturan kelompok bersama |
| 4. Setiap baris memuat `workforceProfileId` | Terpenuhi | `WorkforceDashboardDrilldownItem` |
| DoD: laporan tracked, tanda status roadmap | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Daftar dokumen dipaging di memori (ratusan baris pada RS sekelas RS MMC — sepadan); daftar pegawai dan kontrak dipaging di database |
| Risiko tersisa | Rendah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pengguna (`9062bd61`) |
| Langkah berikutnya | `BE-HRD-452`; `FE-HRD-399` |
