# Laporan Perubahan Backend — `BE-IGD-039`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-039` |
| Judul | Kewenangan unit diperiksa lewat simpul organisasi penugasan HR |
| Slice | `IGD-S07` · `EPIC IGD-08` · `MVP-6`; sekaligus membuka acceptance 2–3 `BE-IGD-061` (`MVP-8`, syarat C2 [kesiapan `MVP-8`](../../../evidence/2026-10-04-kesiapan-mvp-8.md)) |
| Roadmap | [roadmap/backend-roadmap.md](../../../roadmap/backend-roadmap.md), bagian R3.7, kartu `BE-IGD-039` (ditulis ulang 5 Oktober 2026, manifest bagian 0l.1) |
| Trace | `IGD-DEC-086`; `IGD-DEC-092` (bagian fail-closed); `IGD-DEC-193`…`197`; approval `IGD-DEC-199`; fakta `IGD-FACT-044`…`048`; selisih `IGD-CONFLICT-006` (tidak dikerjakan); validation §7 aturan 1–9; permission/audit §1, §3, §3.1 |
| Contract version | Validation **`0.12.0`** dan permission/audit **`0.7.0`** — `approved` lewat `IGD-DEC-199` (Rizki Gunawan, 5 Oktober 2026; sementara, pola `IGD-DEC-174`; approver akhir Security/Privacy owner belum ditunjuk). API `0.14.0` tidak berubah |
| Dependency | — (nol task prasyarat) |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1 (9–20), berkas diubah 0 (1 berkas source), logika bisnis 1, kontrak API 1 (memakai kontrak yang ada), database 1 (perilaku query saja), keamanan/auth 2 (inti otorisasi), UI/workflow 0 |
| Task mode | `BACKEND` — atas jawaban pemilik *"Ya, kerjakan sekarang"*, 5 Oktober 2026 |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyUnitAuthorityService.cs`, laporan ini, roadmap backend (baris status), `requirement-traceability.md` (baris status) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `rizkiG` **`8d81d361`** (sama dengan origin) + working tree dokumen blueprint 5 Oktober 2026 yang belum di-commit |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ **Selesai 5 Oktober 2026 — atas penilaian pemilik** (`IGD-DEC-201`). Build pemilik (DLL 11.51 WIB) dan uji API 5 Oktober pada bukti mentah: acceptance 1, 2, 7, 8, 9, 10, 12, 13 terpenuhi; **acceptance 3, 4, 5, 6, 11 dikecualikan** dengan bukti source karena akun opsional tidak tersedia. Bukti diterima dengan penyimpangan tercatat (`IGD-DEC-200`). Rincian: bagian *Pemeriksaan bukti uji — 5 Oktober 2026*. Tanpa UAT. *Sebelumnya:* 🟡 Implementation Complete — source ditulis, QBE checker Strict `PASS`; menunggu build pemilik dan uji API |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `EmergencyInstallationManagement` |
| Submodule | — |
| Registry | `HealthServices` / `EmergencyInstallationManagement / Emergency` / `BUSINESS DOMAIN / MODULE` / `Emg` / `ACTIVE / LEGACY` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`). Dibaca **tanpa ditulis**: `Corporate` / `WorkforceCore / WorkforceProfileManagement / Workforce Profile` / `Wfp` — tabel `WfpOrganizationAssignment` |
| Keberlakuan | `TOUCHED LEGACY` — mengubah service yang sudah ada; nol entity, nol berkas baru, nol prefix baru |
| QBE yang berlaku | `QBE-SVC-001` (logika tetap di module service; controller tidak disentuh), `QBE-API-001` (bentuk respons dan status kode tidak berubah), `QBE-PERM-001` (metadata `[AccessPermission]` tidak disentuh; kewenangan unit melekat pada data, bukan hardcode peran), `QBE-VAL-001` (invarian kewenangan diperiksa di backend), `QBE-AUD-001` (nol perubahan logging) |
| QBE yang tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*`, `QBE-CODE-*`, `QBE-DTO-001`, `QBE-DB-*` — tidak ada entity, configuration, nomor bisnis, DTO, atau perubahan database; `QBE-TXN-001` — baca saja, tanpa transaksi; `QBE-LOG-001` — penjaga tidak mengubah state |
| Hardcode peran | Nol. Tidak ada `IsInRole`, nama peran, nama departemen, nama posisi, atau `UserType` |
| Selisih governance | Registry di repository (`docs/engineering/`) berbeda dari salinan di suite skill; sesuai `AGENTS.md`, versi repository yang dipakai. Baris `Emg` dan `Wfp` sama di keduanya. Kontrak rekayasa identik |
| Branch | `rizkiG` = `origin/rizkiG` — branch kerja pemilik modul |

---

## 1. Masalah yang diperbaiki

Penjaga kewenangan unit memeriksa apakah seorang petugas bertugas di unit pelayanan tujuan sebelum ia boleh mencatat
kedatangan pasien, menerima atau menolak serah terima, dan menerima, menolak, atau menetapkan sikap atas pesanan.
Pemeriksaannya membandingkan **departemen** penempatan petugas (`AspNetUserOrganization.DepartmentId`, penunjuk ke
`MstDepartment`) dengan **simpul organisasi** unit (`MstServiceUnit.OrganizationUnitId`, penunjuk ke `MstOrganizationUnit`).
Keduanya menunjuk tabel berbeda, sehingga tidak pernah sama (`IGD-FACT-045`).

*Akibat nyata.* Pada uji gabungan `MVP-8` 4 Oktober 2026, tidak seorang pun dapat menerima serah terima. Pasien IGD yang
pindah ke bangsal membuat kunjungan IGD-nya terus *menunggu penutupan*, dan satu-satunya cara menutupnya adalah membatalkan
kepergian — padahal pasien memang sudah pindah. Selain itu, untuk unit yang belum dipetakan, pesan penolakannya berbunyi
*"Lanjutkan dengan menyertakan alasan"*, padahal tidak ada satu pun isian alasan pada permintaan mana pun.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Hanya petugas yang benar-benar bertugas di unit tujuan yang boleh menyatakan penerimaan pasien atau pesanan; hanya petugas unit asal yang boleh menetapkan sikap pesanan |
| Pelaku | Perawat bangsal unit tujuan (catat kedatangan, terima/tolak serah terima, terima/tolak pesanan); Perawat IGD unit asal (sikap pesanan, pesanan luar sistem); HR (penugasan pegawai pada simpul organisasi); Master Data (pemetaan unit pelayanan ke simpul) |
| Pemicu | Petugas menekan *Catat Tiba*, *Terima Dokumen*, atau *Tolak Dokumen* di tab Transfer Ruang Kerja IGD, atau memanggil endpoint pesanan |
| Prasyarat | Petugas memegang `EmergencyDeparture : Update`; unit pelayanan sudah dipetakan ke simpul organisasi; HR sudah menugaskan petugas pada simpul itu |

**Langkah pemeriksaan** (validation §7, *Urutan pemeriksaan*):

1. Lapis izin kemampuan: atribut `[AccessPermission("EmergencyDeparture", "Update")]` diperiksa lebih dulu. Tanpa izin itu →
   `403` dari lapis izin, penjaga unit tidak tercapai.
2. Unit pelayanan dicari. Tidak ditemukan → `403` *"Unit tujuan tidak ditemukan."* (tidak berubah).
3. Unit belum dipetakan ke simpul organisasi → `403` dengan kalimat **baru**: *"Unit {nama unit} belum dipetakan ke simpul
   organisasi, sehingga kewenangan {tindakan} belum dapat diperiksa sistem. Minta Master Data melengkapi pemetaan unit ini."*
4. Penempatan petugas dicari: aktif, tidak dihapus, dalam masa berlaku, **bersumber** penugasan HR (`SourceAssignmentId`
   terisi), dan penugasan HR itu menunjuk simpul organisasi yang **sama persis** dengan simpul unit. Ada → berwenang.
5. Tidak ada → `403` *"Anda tidak bertugas di unit {nama unit}, sehingga tidak dapat {tindakan}."* (tidak berubah).

*Contoh.* Unit *Rawat Inap Melati* dipetakan ke simpul *Bangsal Melati*, di bawah simpul induk *Instalasi Rawat Inap*.

| Petugas | Penugasan HR | Hasil *Terima Dokumen* | Langkah |
| --- | --- | --- | --- |
| Ns. Wati | Simpul *Bangsal Melati*, berlaku | Berwenang → `200` | 4 |
| Ns. Budi | Simpul *Bangsal Mawar*, departemen sama dengan Ns. Wati | `403` *"Anda tidak bertugas di unit Rawat Inap Melati, sehingga tidak dapat meninjau serah terima."* | 5 |
| Kepala Instalasi Rawat Inap | Simpul *Instalasi Rawat Inap* saja | `403`, pesan sama — simpul induk tidak mencakup turunan | 5 |
| Kepala Instalasi Rawat Inap | Ditambah penugasan **sekunder** pada simpul *Bangsal Melati* | `200` | 4 |
| Ns. Wati | Penugasan berakhir kemarin | `403`, pesan sama | 5 |
| Siapa pun | — (unit belum dipetakan) | `403` dengan kalimat langkah 3 | 3 |

**Perubahan status.** Penjaga tidak mengubah status apa pun; ia hanya memutuskan boleh atau tidak. Bila boleh, aksi
pemanggilnya berjalan seperti sebelumnya — termasuk penutupan kunjungan susulan dari `BE-IGD-061` bila aksi itu membereskan
penahan terakhir.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat baru dari task ini |
| --- | --- | --- | --- | --- |
| Dokumen serah terima `Diajukan`/`Tertunda` | Terima | `Diterima` | Petugas unit tujuan | Penugasan HR pada simpul unit tujuan |
| Dokumen serah terima `Diajukan`/`Tertunda` | Tolak (beralasan) | `Ditolak` | Petugas unit tujuan | Sama |
| Fisik `Berangkat` | Catat tiba | `Tiba` | Petugas unit tujuan | Sama |
| Pesanan tanpa sikap | Tetapkan sikap | `Continue`/`Handover`/`Cancel` | Petugas unit asal | Penugasan HR pada simpul unit asal |

**Jalur tidak normal.** Unit belum dipetakan, petugas di simpul lain, hanya di simpul induk, penugasan berakhir, penempatan
warisan tanpa sumber, atau tanpa izin kemampuan — semuanya `403`, tanpa perubahan data. Pelayanan klinis darurat
(observasi, triage, tindak lanjut, pengkajian) **tidak** melewati penjaga ini.

**Hasil akhir.** Petugas yang benar ditugaskan dapat menyelesaikan serah terima dan sikap pesanan; kunjungan IGD yang
tertahan karena kepergian atau pesanan dapat tertutup tanpa membatalkan kepergian.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyUnitAuthorityService.cs` | Berkas sasaran |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDepartureService.cs` | Kelima pemanggil `PeriksaAsync` dan `PeriksaUnitAsalAsync` — tidak diubah |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyDepartureController.cs` | Route, `[Tags]`, dan `[AccessPermission]` endpoint yang terdampak |
| `Models/ApplicationUserOrganization.cs` | `SourceAssignmentId` (`Guid?`), kondisi berlaku penempatan |
| `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpOrganizationAssignment.cs` | `OrganizationUnitId` (`Guid?`), namespace |
| `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstOrganizationUnit.cs` | Simpul organisasi, `ParentOrganizationUnitId` (tidak ditelusuri) |
| `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs` | `OrganizationUnitId` |
| `Services/Security/OrganizationAuthorizationProjectionService.cs` | Proyeksi penempatan dan `IsAssignmentValid` (tidak disalin) |
| `Repositories/ApplicationDbContext.cs`, `Repositories/Configurations/Global/ApplicationUserOrganizationConfiguration.cs` | `DbSet<WfpOrganizationAssignment>`; index bersaring `SourceAssignmentId` |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodOrderService.cs` | Preseden subkueri `_dbContext.Set<T>().Any(...)` di dalam kueri |
| `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/*` | Governance |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyUnitAuthorityService.cs` (+8/−4) | (1) `using` model HR `WorkforceCore`. (2) Kalimat unit belum dipetakan diganti persis sesuai `IGD-DEC-195`. (3) Syarat `x.DepartmentId == unit.OrganizationUnitId.Value` diganti `x.SourceAssignmentId != null`, ditambah subkueri `EXISTS` ke `WfpOrganizationAssignment` dengan `Id == x.SourceAssignmentId` dan `OrganizationUnitId == unit.OrganizationUnitId.Value`. Kondisi aktif, terhapus, dan masa berlaku penempatan tidak diubah. Nol baris komentar baru; komentar lama tidak disunting |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol endpoint, nol ruas request/response, status kode tetap. Perilaku berubah sesuai validation `0.12.0` §7: pengguna yang penugasan HR-nya cocok kini lolos (sebelumnya selalu `403`), dan pesan `403` unit belum dipetakan berganti |
| Database | Nol schema, nol entity, nol migration. Satu kueri baca bertambah subkueri `EXISTS` ke `public."WfpOrganizationAssignment"` lewat kunci utama; filter penempatan memakai index bersaring `SourceAssignmentId` yang sudah ada |
| Keamanan/Auth | **Inti.** Penjaga tetap fail-closed (`IGD-DEC-092`). Lapis izin `[AccessPermission]` tidak berubah. Kewenangan diturunkan dari data penugasan, bukan dari peran (permission/audit §1). Simpul induk sengaja tidak ditelusuri (`IGD-DEC-194`) |

### 3.4 Selisih terhadap kartu

| Butir kartu | Yang dikerjakan | Alasan |
| --- | --- | --- |
| *"Satu kueri baca dengan join"* | Satu kueri baca dengan subkueri `EXISTS` (`Set<WfpOrganizationAssignment>().Any(...)`) | Hasilnya setara join semi; bentuk `AnyAsync` lama dipertahankan sehingga diff minimal, dan pola ini sudah dipakai modul lain (`BbkBloodOrderService`) |

---

## 4. Dokumentasi endpoint

Nol endpoint baru dan nol perubahan bentuk. Tabel di bawah mencantumkan endpoint yang **perilakunya** berubah karena
memanggil penjaga ini.

#### Health Services / Emergency Installation Management / Emergency Departure

Base URL: `api/v1/health-services/emergency-installation-management/emergency-departures`

| Method | Path | Kegunaan | Hak akses | Unit yang diperiksa |
| --- | --- | --- | --- | --- |
| `POST` | `/{id}/arrive` | Petugas unit tujuan mencatat pasien tiba | `EmergencyDeparture : Update` | Tujuan |
| `POST` | `/{id}/accept-handover` | Petugas unit tujuan menerima dokumen serah terima | `EmergencyDeparture : Update` | Tujuan |
| `POST` | `/{id}/reject-handover` | Petugas unit tujuan menolak dokumen serah terima beserta alasan | `EmergencyDeparture : Update` | Tujuan |
| `POST` | `/{id}/order-items/{itemId}/accept` | Unit tujuan menerima sebuah pesanan | `EmergencyDeparture : Update` | Tujuan |
| `POST` | `/{id}/order-items/{itemId}/reject` | Unit tujuan menolak sebuah pesanan beserta alasan | `EmergencyDeparture : Update` | Tujuan |
| `PATCH` | `/{id}/order-items/{itemId}/action` | Unit asal menetapkan sikap pesanan | `EmergencyDeparture : Update` | Asal |
| `POST` | `/{id}/order-items` | Unit asal mendaftarkan pesanan luar sistem | `EmergencyDeparture : Update` | Asal |

**Kode status yang terkait penjaga.** `403` — pengguna tidak bertugas di unit itu, unit belum dipetakan, unit tidak
ditemukan, atau unit asal kepergian kosong; pesannya menjelaskan sebabnya dan apa yang harus dilakukan. `200` — berwenang;
aksi berjalan seperti sebelumnya. Kode lain (`400`, `404`, `409`) berasal dari aturan aksi masing-masing dan tidak berubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <berkas> -Mode Strict` | 1 berkas, VIOLATION 0, REVIEW 0, INFO 0 — *Final result: PASS* | `PASS` | Keluaran perintah, 5 Oktober 2026 |
| `git diff -U0` — baris komentar baru (`//`, `/*`, `*`) | Nol | `PASS` | Keluaran perintah |
| `git diff --numstat -- Areas/` | Satu berkas, +8/−4; nol `Program.cs`, nol `Migrations/` | `PASS` | Keluaran perintah |
| Pemeriksaan tipe dan namespace (manual) | `WfpOrganizationAssignment` unik di `QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models`; `Id` `Guid` vs `SourceAssignmentId` `Guid?` dan `OrganizationUnitId` `Guid?` vs `Guid` — perbandingan *lifted* yang sah di C# dan diterjemahkan EF | `PASS` (pembacaan source, bukan kompilasi) | Berkas model |
| Akhiran baris berkas source | LF seragam, sama dengan sebelum disunting | `PASS` | Hitungan byte Node |
| `dotnet build -p:RunAnalyzers=false` | — | `NOT RUN` | Milik Rizki. Build pemilik pukul 10.49 (DLL) dan ulangannya hari ini terjadi **sebelum** perubahan ini, jadi tidak membuktikan task ini |
| Uji API acceptance 1–9, 11 | — | `NOT RUN` | Menunggu persiapan data P1–P5 (kartu) dan build |

Uji manual: `REQUIRED` — lewat agen Antigravity dari panduan uji yang disiapkan agent sesudah build dan persiapan data.

**Tidak dijalankan:** build dan uji runtime — wewenang pemilik dan bergantung data dev yang belum disiapkan.

### 5.1 Perintah build untuk pemilik

```powershell
cd C:\Users\BenariDev03\QuilvianV2\NewQuilvianSystemBackend
dotnet build -p:RunAnalyzers=false
```

Laporkan jumlah error **dan** warning. Bila backend sedang berjalan, hentikan dulu supaya DLL dapat ditimpa.

### 5.2 Kueri baca pemeriksa persiapan data

Disiapkan agent sesuai `IGD-DEC-196`; **hanya `SELECT`**. Dijalankan pemilik, atau agent atas izin pemilik.

```sql
-- P1: pemetaan unit pelayanan ke simpul organisasi
SELECT su."ServiceUnitName", su."OrganizationUnitId", ou."UnitName", ou."IsActive"
FROM public."MstServiceUnit" su
LEFT JOIN public."MstOrganizationUnit" ou ON ou."Id" = su."OrganizationUnitId"
WHERE su."IsDelete" = false
ORDER BY (su."OrganizationUnitId" IS NULL) DESC, su."ServiceUnitName";

-- P2: penempatan akun uji beserta simpul penugasan HR sumbernya
SELECT u."Email", p."IsActive", p."IsPrimary", p."EffectiveStartDate", p."EffectiveEndDate",
       p."SourceAssignmentId", a."OrganizationUnitId", ou."UnitName"
FROM public."AspNetUserOrganization" p
JOIN public."AspNetUsers" u ON u."Id" = p."UserId"
LEFT JOIN public."WfpOrganizationAssignment" a ON a."Id" = p."SourceAssignmentId"
LEFT JOIN public."MstOrganizationUnit" ou ON ou."Id" = a."OrganizationUnitId"
WHERE p."IsDelete" = false AND u."Email" IN ('<akun-uji-1>', '<akun-uji-2>');

-- P5: penempatan warisan tanpa sumber yang masih aktif
SELECT COUNT(*) FROM public."AspNetUserOrganization"
WHERE "IsDelete" = false AND "IsActive" = true AND "SourceAssignmentId" IS NULL;
```

Baris komentar SQL di atas adalah bagian dokumen, bukan source aplikasi.

### 5.3 Skenario uji untuk panduan Antigravity

| ID | Skenario | Harapan | Acceptance |
| --- | --- | --- | --- |
| `039-S1` | Perawat bangsal (penugasan simpul bangsal tujuan) menerima serah terima pada kunjungan yang menunggu penutupan karena kepergian | `200`; kunjungan IGD `Completed` atas nama perawat itu; `ClosedByDispositionId` terisi | 1 (`061-S2`) |
| `039-S2` | Petugas simpul saudara (departemen sama) pada `arrive` dan `accept-handover` | `403` *"Anda tidak bertugas di unit …"*; nol perubahan | 2 |
| `039-S3` | Petugas yang hanya ditugaskan pada simpul induk | `403`; nol perubahan | 3 |
| `039-S4` | Penugasan yang sudah berakhir | `403`; nol perubahan | 4 |
| `039-S5` | Penempatan warisan tanpa sumber (bila ada) | `403`; bila datanya tidak ada → `NOT RUN` beralasan | 5 |
| `039-S6` | Penugasan sekunder pada simpul tujuan | `200` | 6 |
| `039-S7` | Unit tujuan belum dipetakan | `403` dengan kalimat `IGD-DEC-195` persis | 7 |
| `039-S8` | Perawat IGD (simpul IGD) menetapkan sikap pesanan; unit IGD dipetakan | `200`; kunjungan tertutup bila penahan terakhir | 8 (`061-S3`, `S12`) |
| `039-S9` | Membatalkan kepergian | `200` tanpa pemeriksaan unit | 9 (`061-S19-S11` kaki `cancel`) |
| `039-S11` | Penugasan simpul sama tetapi tanpa `EmergencyDeparture : Update` | `403` dari lapis izin | 11 |
| `061-S4` | Kaki kepergian dan pesanan pada kunjungan yang **tidak** menunggu penutupan | Aksi berhasil, status kunjungan tidak berubah | `BE-IGD-061` acceptance 4 |
| `061-S11` | Kaki `reject-handover` pada kunjungan menunggu penutupan | `200`; kunjungan tertutup bila penahan terakhir | `BE-IGD-061` acceptance 2 |

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Penempatan bersumber penugasan HR pada simpul sama → terima serah terima `200`; kunjungan tertutup bila penahan terakhir | Belum terpenuhi — terpetakan ke source | Subkueri `EXISTS` pada `PeriksaAsync`; uji `039-S1` belum |
| 2 | Simpul lain dalam departemen sama → `403` | Belum terpenuhi — terpetakan ke source | Syarat `DepartmentId` dihapus; uji belum |
| 3 | Hanya simpul induk → `403` | Belum terpenuhi — terpetakan ke source | Nol penelusuran `ParentOrganizationUnitId`; uji belum |
| 4 | Penempatan berakhir → `403` | Belum terpenuhi — terpetakan ke source | Kondisi masa berlaku tidak diubah; uji belum |
| 5 | Penempatan warisan tanpa sumber → `403` | Belum terpenuhi — terpetakan ke source | Syarat `SourceAssignmentId != null`; uji atau `NOT RUN` beralasan |
| 6 | Penempatan sekunder berlaku → `200` | Belum terpenuhi — terpetakan ke source | `IsPrimary` tidak diperiksa; uji belum |
| 7 | Unit belum dipetakan → `403` kalimat `IGD-DEC-195` persis | Belum terpenuhi — terpetakan ke source | Kalimat baru pada `PeriksaAsync`; uji belum |
| 8 | Perawat IGD simpul IGD → sikap pesanan `200` | Belum terpenuhi — terpetakan ke source | `PeriksaUnitAsalAsync` memakai `PeriksaAsync` yang sama; uji belum |
| 9 | Regresi: batal kepergian tidak dijaga | Belum terpenuhi — terpetakan ke source | `EmergencyDepartureService` tidak diubah; uji belum |
| 10 | Diff: satu berkas service; nol `Program.cs`, migration, endpoint; nol komentar baru | **Terpenuhi** | Bagian 5 |
| 11 | Tanpa `EmergencyDeparture : Update` → `403` dari lapis izin | Belum terpenuhi | Atribut tidak diubah; uji belum |
| 12 | Tindakan klinis tidak memanggil penjaga | **Sebagian** — terpenuhi lewat source; regresi `061-S19` belum | `EmergencyUnitAuthorityService` hanya dipakai `EmergencyDepartureService` |
| 13 | Build 0 error | Belum terpenuhi | Build pemilik sesudah perubahan ini belum ada |

**DoD yang belum:** build pemilik, uji API (acceptance 1–9, 11, 12 bagian regresi), dan penilaian ulang `BE-IGD-061`
acceptance 2–3 dari putaran uji yang sama.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Dua komentar lama di berkas sasaran kini **basi** dan sengaja tidak disunting (aturan pemilik: nol baris komentar, komentar lama tidak disunting): (a) remarks kelas tentang *"jalan keluar beralasan yang meninggalkan jejak"*; (b) komentar `// IGD-DEC-092 …` di atas pemeriksaan unit belum dipetakan yang menyebut *"jalan keluar beralasan yang dipanggil terpisah oleh controller"*. Keduanya bertentangan dengan `IGD-DEC-195` |
| Masalah yang diketahui | Tindakan unit asal yang belum dijaga (`IGD-CONFLICT-006`) — tidak dikerjakan sesuai `IGD-DEC-197` |
| Risiko tersisa | (1) Kewenangan bergantung pada ketepatan waktu proyeksi penempatan milik platform-authorization: penugasan HR yang dicabut tetap memberi kewenangan sampai proyeksinya ditutup, karena IGD sengaja tidak menyalin predikat kelayakan (`IGD-DEC-193`). (2) Tanpa pemetaan unit dan penugasan HR, seluruh tindakan tetap `403` — perbaikan tidak terlihat bedanya sebelum data P1–P2 diisi. (3) Peran perawat rawat inap yang memegang `Update` dapat membatalkan kepergian IGD (`IGD-CONFLICT-006`, diterima `IGD-DEC-198`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` berkas source di atas, ditambah 9 berkas dokumen blueprint pass desain dan perencanaan 5 Oktober 2026 yang belum di-commit, laporan ini (baru), serta baris status roadmap dan traceability |
| Langkah berikutnya | (1) Rizki: `dotnet build -p:RunAnalyzers=false`, laporkan error dan warning. (2) Rizki: persiapan data P1–P5 (kartu `BE-IGD-039`). (3) Agent: panduan uji Antigravity dari bagian 5.3. (4) Agent: periksa bukti mentah, lalu nilai ulang `BE-IGD-039` dan `BE-IGD-061` |

---

## Pemeriksaan bukti uji — 5 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-20261005/` (JSON per skenario, PNG,
`persiapan.json`, `P6-*`, `P7-*`, `data-uji.json`), dicocokkan dengan log backend `Logs/quilvian-backend-20261005.json`.
Putaran dijalankan agen Antigravity dari [panduan uji](../../../testing/2026-10-05-panduan-uji-be-igd-039.md);
[laporan penguji](../../../testing/2026-10-05-laporan-uji-be-igd-039.md) **tidak** dipakai sebagai bukti — selisihnya ada
di laporan itu bagian 7.2.

**Keabsahan putaran.** Backend dinyalakan ulang pukul 12.24 WIB dari DLL 11.51 WIB, yang lebih baru dari berkas service
task ini (11.41 WIB) — kode yang diuji adalah kode task ini. Putaran resmi 13.14.07–13.15.36 WIB. 115 dari 115 badan
respons yang terekam cocok dengan log backend (method, path, kode status, waktu). Lima akun nyata dipakai, semuanya
`isSuperAdmin: false`; tidak ada panggilan ke endpoint peran, penugasan HR, atau unit pelayanan. Penyimpangan prosedural
— skrip tidak disimpan, dua percobaan awal tidak dilaporkan, rekaman jaringan layar tidak lengkap — diterima pemilik
(`IGD-DEC-200`).

**Data uji.** `UNIT-IGD` = *Instalasi Gawat Darurat* (dipetakan ke simpul *Instalasi Gawat Darurat*), `UNIT-TUJUAN` =
*Rawat Inap* (simpul *Instalasi Rawat Inap*), `UNIT-BELUM` = *HCU* (belum dipetakan). Akun `KLINIS` ditugaskan HR pada
simpul IGD, `PENERIMA` pada simpul Instalasi Rawat Inap, `SAUDARA` pada simpul *Unit Perawatan Intensif (ICU)* — ketiganya
dengan `SourceAssignmentId` terisi (`P7-penempatan.json`).

| Skenario | Putusan | Yang teramati pada bukti mentah dan log |
| --- | --- | --- |
| `039-S1` (`061-S2`) | **Terbukti** | Sesudah `RX`: kunjungan 7, alasan *"Masih ada proses kepergian pasien yang belum selesai."*. `RT` `PENERIMA` `200`, fisik 3, kunjungan tetap 7. `accept-handover` `PENERIMA` 13.14.11 WIB `200`, dokumen 3, kunjungan 9; Q-VISIT: `ClosedByDispositionId` = tindak lanjut `V1`, `UpdateBy` = `PENERIMA`, `EncounterStatus` 9 |
| `039-S2` | **Terbukti** | `SAUDARA` (simpul ICU): `arrive`, `accept-handover`, `order-items/{id}/accept` masing-masing `403` dengan kalimat *"Anda tidak bertugas di unit Rawat Inap, sehingga tidak dapat …"* persis; fisik 2, dokumen 2, pesanan 2 tidak berubah |
| `039-U2` | **API terbukti**; tampilan layar gagal karena celah frontend | Browser `KLINIS` 13.14.44 WIB: `POST …/arrive` `403` kalimat persis. PNG 1440 × 900 hasil build: modal *Catat Tiba?* tetap terbuka **tanpa** pesan (`IGD-FACT-050`) |
| `039-S7` | **Terbukti** | `PENERIMA` pada kepergian ke HCU: `arrive` `403` *"Unit HCU belum dipetakan ke simpul organisasi, sehingga kewenangan mencatat kedatangan pasien belum dapat diperiksa sistem. Minta Master Data melengkapi pemetaan unit ini."*; `accept-handover` `403` kalimat *meninjau serah terima* yang sama polanya. Tanpa *"menyertakan alasan"* |
| `039-U1` | **API terbukti**; tampilan layar gagal karena celah frontend | Browser `KLINIS` 13.15.21 WIB: `POST …/accept-handover` `403` kalimat persis. PNG: modal *Terima Dokumen?* tanpa pesan |
| `039-S8a` | **Terbukti** | `PENERIMA` tolak pesanan `200` (diterima 4); `KLINIS` (simpul IGD) tetapkan sikap `Continue` `200`; Q-PESANAN: baris lama tidak efektif, baris pengganti `Action` 1, `AcceptanceStatus` 1, `SupersedesOrderItemId` benar; kunjungan tetap 4 |
| `039-S8` | **Terbukti** | Kunjungan `V4` 7 sesudah `RT`; tolak pesanan `PENERIMA` `200` → tetap 7, alasan *"Masih ada pesanan yang belum ditentukan sikapnya: Uji pesanan 039 V4."*; sikap `KLINIS` `200` → 9, `ClosedByDispositionId` = tindak lanjut `V4`, `UpdateBy` = `KLINIS` |
| `039-S9` | **Terbukti** | `KLINIS` membatalkan kepergian ke HCU (belum dipetakan) `200`; fisik 9, dokumen 9; kunjungan tetap 4 — pembatalan tidak dijaga kewenangan unit (`IGD-DEC-197`) |
| `039-S12` | **Terbukti** | 27 permintaan klinis (`start-triage`, `visit-status`, tindak lanjut) dari `KLINIS` dan `DOKTER`: nol `403`, nol kalimat kewenangan unit |
| `039-S3`, `S4`, `S5`, `S6`, `S11` | `NOT RUN` — dikecualikan | Akun `INDUK`, `BERAKHIR`, `WARISAN`, `SEKUNDER`, `TANPAIZIN` tidak tersedia. `039-S6` dijalankan dengan `PENERIMA` hanya untuk kaki `061-S4` |
| `PROBE-1` | Bukan acceptance task ini | Pesanan tanpa sikap tidak menahan penutupan — `IGD-CONFLICT-007`, ditangani pengerjaan ulang `BE-IGD-041` (`IGD-DEC-203`) |

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Penempatan bersumber penugasan HR pada simpul sama → terima serah terima `200`; kunjungan tertutup | **Terpenuhi** | `039-S1` |
| 2 | Simpul lain → `403` | **Terpenuhi** | `039-S2`; API `039-U2` |
| 3 | Hanya simpul induk → `403` | **Dikecualikan** (`IGD-DEC-201`) | Source: satu perbandingan `OrganizationUnitId ==`, tanpa penelusuran simpul induk |
| 4 | Penempatan berakhir → `403` | **Dikecualikan** | Source: syarat masa berlaku penempatan tidak diubah task ini |
| 5 | Penempatan warisan tanpa sumber → `403` | **Dikecualikan** | Source: syarat `SourceAssignmentId != null` |
| 6 | Penempatan sekunder → `200` | **Dikecualikan** | Source: `IsPrimary` tidak diperiksa |
| 7 | Unit belum dipetakan → `403` kalimat `IGD-DEC-195` | **Terpenuhi** | `039-S7`; API `039-U1` |
| 8 | Sikap pesanan oleh perawat simpul IGD → `200`; kunjungan tertutup | **Terpenuhi** | `039-S8`, `039-S8a` |
| 9 | Regresi: batal kepergian tidak dijaga | **Terpenuhi** | `039-S9` |
| 10 | Diff: satu berkas, nol `Program.cs`/migration/endpoint, nol komentar baru | **Terpenuhi** | Bagian 5 (tidak berubah sejak itu: `git diff --numstat` +8/−4) |
| 11 | Tanpa `EmergencyDeparture : Update` → `403` lapis izin | **Dikecualikan** | Source: atribut `[AccessPermission]` tidak disentuh |
| 12 | Tindakan klinis tidak memanggil penjaga | **Terpenuhi** | Source + `039-S12` |
| 13 | Build 0 error | **Terpenuhi** — jumlah warning tidak dilaporkan | DLL 11.51 WIB dihasilkan build pemilik; backend berjalan dari build itu sejak 12.24 WIB |

**Putusan: ✅ — atas penilaian pemilik** (`IGD-DEC-201`), dengan acceptance 3, 4, 5, 6, 11 dikecualikan. Kelima jalur
penolakan itu belum pernah diamati pada runtime.

**Temuan yang lahir dari putaran ini, di luar cakupan task:**

| Temuan | Tindak lanjut |
| --- | --- |
| Tab Transfer tidak menampilkan galat aksi kepergian; modal konfirmasi tetap terbuka tanpa pesan (`IGD-FACT-050`) | Kartu frontend pasangan `BE-IGD-039` (`IGD-DEC-202`), direncanakan `plan-module-delivery`; `039-U1`, `039-U2` diuji ulang sesudahnya |
| Pesanan tanpa sikap tidak menahan penutupan (`PROBE-1`, `IGD-CONFLICT-007`) | Pengerjaan ulang `BE-IGD-041` (`IGD-DEC-203`) |
| Kartu pasien Ruang Kerja selalu *"Pasien belum teridentifikasi"*, RM dan unit kosong (`IGD-FACT-052`) | `IGD-OQ-116`, belum diputuskan |
| Perawat IGD belum memegang `ServiceUnit : Read` — `GET master-data/service-units` `403`, pilihan unit tujuan pada formulir kepergian kosong | Pemberian hak ke peran oleh pemilik (kelas *Periksa* tabel konfigurasi C3) |

**Komentar basi** (dicatat, tidak disunting): remarks kelas dan komentar `// IGD-DEC-092 …` di atas pemeriksaan unit belum
dipetakan masih menyebut jalan keluar beralasan — lihat bagian 7.

**Status Git sesudah pemeriksaan:** source tidak berubah; berubah hanya laporan ini, baris status roadmap backend, dan
`requirement-traceability.md`, ditambah dokumen blueprint lain 5 Oktober 2026 yang belum di-commit.
