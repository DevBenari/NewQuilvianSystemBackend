# Laporan Perubahan Backend — `BE-HRD-469`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-469` |
| Nomor lama | `BE-HRD-463` (dinomori ulang saat merge `QuilvianIntegrationBackEnd` 27 November 2026 agar `BE-HRD-463` milik `A52-B` tidak berubah) |
| Judul | Respons detail unit membawa nama legal entity, fasilitas, dan departemen |
| Slice | `S-FIN-M05` amandemen — Menu Organisasi (`PLAN-REPAIR-HRD-008`, `FIX-HRD-008-06`) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` bagian 33 (nomor lama bagian 32) |
| Trace | `HRD-DEC-257` (nomor lama `HRD-DEC-248`); `ISSUE-HRD-008` temuan `ISS-HRD-008-T3`; aturan output 5.3 |
| Contract version | `v30` tetap. Master organisasi tidak diatur `api-contract.md`; perubahan ini aditif |
| Dependency | Tidak ada. Dipakai `FE-HRD-422` (nomor lama `FE-HRD-414`) |
| Klasifikasi | `LIGHT` — 2 berkas, perubahan aditif pada satu endpoint baca, tanpa database |
| Task mode | `BACKEND` (disetujui pengguna 24 November 2026: "oke saya setujui kerjakan semua dan selesaikan") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/HumanResource/MasterData/Organization/**` dan laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `3ef27c96` (branch `MHamzah`), perubahan belum di-commit |
| Tanggal | 2026-11-24 |
| Status | ✅ Selesai — build dan QBE lulus; uji API runtime `NOT RUN` (lihat bagian 5) |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Organization` |
| Pemilik / prefix registry | `Mst` (Master / Reference / MasterData) — entity `MstOrganizationUnit`, `MstLegalEntity`, `MstHospitalSite`, `MstDepartment` sudah ada |
| Keberlakuan | `TOUCHED LEGACY` — controller lama memakai `ApplicationDbContext` langsung; tidak di-refactor (ratchet, di luar scope) |
| Entity / migration baru | Tidak ada |
| QBE yang berlaku | `QBE-API-001`, `QBE-DTO-001` (respons tetap DTO, bukan entity), `QBE-PERM-001` (atribut akses tidak berubah) |

---

## 1. Masalah yang diperbaiki

Halaman detail unit organisasi menampilkan baris "ID Legal Entity", "ID Fasilitas Rumah Sakit",
dan "ID Departemen" berisi deretan huruf-angka acak (UUID). Sebabnya, respons
`GET .../organization-units/{id}` hanya membawa ID ketiga relasi itu. Nama yang dikirim hanya nama
unit induk (`ParentUnitName`).

Contoh: petugas membuka detail "Instalasi Farmasi" dan hanya melihat
"ID Departemen: c93e1b77-…". Untuk tahu departemennya, ia harus mencari ID itu di menu lain.

---

## 2. Proses bisnis

| Langkah | Isi |
| --- | --- |
| Tujuan | Petugas HR membaca keterkaitan unit dengan legal entity, fasilitas, dan departemen tanpa menerjemahkan ID |
| Pelaku | Petugas HR dengan hak `OrganizationUnit : Read` |
| Pemicu | Petugas membuka detail unit dari daftar atau dari pohon Struktur Organisasi |
| Langkah | 1. Layar meminta `GET /{id}`. 2. Server memuat unit beserta relasi legal entity, fasilitas, dan departemen. 3. Server mengirim nama ketiganya bersama ruas lama. 4. Layar (`FE-HRD-422`) menampilkan nama |
| Aturan | Nama dikirim apa pun status aktif relasinya, supaya unit yang terhubung ke departemen nonaktif tetap terbaca |
| Jalur tidak normal | Unit tanpa fasilitas atau departemen → nama bernilai `null`, layar menampilkan "-". Unit tidak ada → `404` seperti sebelumnya |
| Hasil akhir | Tidak ada data yang berubah. Ini hanya perubahan baca |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/Corporate/HumanResource/MasterData/Organization/Controllers/OrganizationUnitController.cs` (`GetById`, `BaseQuery`)
- `Areas/Corporate/HumanResource/MasterData/Organization/DTOs/OrganizationUnitDtos.cs`
- `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstOrganizationUnit.cs`, `MstLegalEntity.cs`, `MstHospitalSite.cs`, `MstDepartment.cs`
- `docs/module-blueprints/human-resource/docs/plan-repair/plan-repair-008-gabung-unit-organisasi-ke-struktur-organisasi.md`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/MasterData/Organization/DTOs/OrganizationUnitDtos.cs` | `OrganizationUnitDetailResponse` ditambah `LegalEntityName`, `HospitalSiteName`, `DepartmentName` (`string?`) |
| `Areas/Corporate/HumanResource/MasterData/Organization/Controllers/OrganizationUnitController.cs` | `GetById` menambah `Include` untuk `LegalEntity`, `HospitalSite`, `Department`, hanya pada kueri detail. `BaseQuery` yang dipakai daftar tidak diubah. Ketiga nama diisi dari `LegalEntityName`, `SiteName`, dan `DepartmentName` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: tiga ruas baru pada respons `GET /{id}`. Ruas lama tetap sama nama dan tipenya. Daftar, opsi, dan pohon tidak berubah |
| Database | Tidak ada perubahan schema. Tanpa migration |
| Keamanan/Auth | Tidak ada. `[AccessAction("Read", …)]` dan `[AccessPermission("OrganizationUnit", "Read")]` tidak berubah |

---

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Master Data / Organization Unit

Base URL: `api/v1/corporate/human-resource/master-data/organization-units`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}` | Detail satu unit beserta nama legal entity, fasilitas, dan departemen terkait | `OrganizationUnit : Read` | - | `OrganizationUnitDetailResponse` (+ `legalEntityName`, `hospitalSiteName`, `departmentName`) |

Kode status tidak berubah. `200` berarti data ditemukan. `404` berarti unit tidak ada atau sudah
dihapus. `403` berarti pengguna tidak punya hak melihat unit.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratch>` | 0 error, 610 warning (baseline lama), tidak ada warning pada berkas Organization; 6 menit 34 detik | `PASS` | Log build sesi 24 November 2026. `bin/` terkunci backend yang sedang berjalan, jadi keluaran diarahkan ke folder sementara |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` pada 2 berkas | VIOLATION 0, REVIEW 0, INFO 0 — `PASS` | `PASS` | Keluaran perintah |
| Review diff | 2 berkas, +13/−1 baris, hanya scope task | `PASS` | `git diff` |
| Uji API `GET /{id}` (AC 1–3) | Tidak dijalankan | `NOT RUN` | Backend yang sedang berjalan memakai build lama. Menjalankan backend baru di DB dev bersama butuh izin terpisah (worker latar ikut menulis) |

Uji manual: `NOT FEASIBLE` pada sesi ini (alasan di atas).

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Tidak dijalankan:** uji API runtime. Setelah backend di-restart dengan kode ini, cukup buka
detail satu unit yang punya departemen dan periksa ruas `departmentName`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Unit yang terhubung ke departemen "Farmasi" mengembalikan `departmentName: "Farmasi"` | Terpenuhi di source; runtime `NOT RUN` | `DepartmentName = entity.Department?.DepartmentName` dengan `Include(x => x.Department)` |
| 2. Unit tanpa fasilitas mengembalikan `hospitalSiteName: null` tanpa galat | Terpenuhi di source | Operator `?.` pada `entity.HospitalSite` |
| 3. Unit yang terhubung ke departemen nonaktif tetap mengembalikan namanya | Terpenuhi di source | `Include` tanpa saringan `IsActive`; tidak ada query filter global pada entity terkait |
| 4. Field lama respons tetap sama nama dan tipenya | Terpenuhi | Diff hanya menambah properti |
| DoD: laporan dan tanda roadmap | Terpenuhi | Laporan ini; roadmap backend bagian 33 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 610 warning build adalah baseline repository, tidak ada yang baru dari task ini |
| Masalah yang diketahui | Pesan server berbahasa campur ("Detail organization unit berhasil diambil.") tidak diubah — dicatat `ISSUE-HRD-008` T4 |
| Risiko tersisa | Kueri detail memuat tiga relasi tambahan untuk satu baris — dampak kecil |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M Areas/Corporate/HumanResource/MasterData/Organization/Controllers/OrganizationUnitController.cs`, `M Areas/Corporate/HumanResource/MasterData/Organization/DTOs/OrganizationUnitDtos.cs` (berkas controller juga memuat perubahan `BE-HRD-470`) |
| Langkah berikutnya | Restart backend lokal agar `FE-HRD-422` menerima ruas nama; `BE-HRD-470` |
