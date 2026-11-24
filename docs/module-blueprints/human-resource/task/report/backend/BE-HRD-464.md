# Laporan Perubahan Backend — `BE-HRD-464`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-464` |
| Judul | Server menolak Departemen Terkait yang tidak ada atau nonaktif |
| Slice | `S-FIN-M05` amandemen — Menu Organisasi (`PLAN-REPAIR-HRD-008`, `FIX-HRD-008-08`) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` bagian 32 |
| Trace | `HRD-DEC-248` butir 5 (departemen dipilih dari master); `ISSUE-HRD-008` temuan `ISS-HRD-008-T5` |
| Contract version | `v30` tetap. Master organisasi tidak diatur `api-contract.md`; menambah satu pesan `400` |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — 1 berkas, satu validasi tambahan, tanpa database |
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
| Pemilik / prefix registry | `Mst` — entity `MstDepartment`, `MstOrganizationUnit` sudah ada |
| Keberlakuan | `TOUCHED LEGACY` — validasi tetap di controller lama (`ValidateRequestAsync`); tidak dipindah ke service (ratchet, di luar scope) |
| Entity / migration baru | Tidak ada |
| QBE yang berlaku | `QBE-VAL-001` (validasi invarian bisnis), `QBE-API-001` (pola `400` + `ApiResponse` yang sudah ada), `QBE-PERM-001` (atribut akses tidak berubah) |

---

## 1. Masalah yang diperbaiki

Saat unit organisasi disimpan, server memeriksa legal entity, fasilitas, dan unit induk, tetapi
**tidak** memeriksa Departemen Terkait (`DepartmentId`). Akibatnya ada dua celah:

1. Departemen yang sudah dinonaktifkan tetap diterima lewat permintaan langsung ke API.
2. ID departemen yang tidak ada baru ditolak oleh foreign key database
   (`FK_MstOrganizationUnit_MstDepartment_DepartmentId`), sehingga petugas berpotensi melihat
   galat umum, bukan pesan yang jelas.

Setelah `HRD-DEC-248`, aturan "departemen dipilih dari master Departemen" menjadi aturan utama
form unit. Server kini ikut menjaga aturan itu.

---

## 2. Proses bisnis

| Langkah | Isi |
| --- | --- |
| Tujuan | Setiap unit hanya terhubung ke departemen yang benar-benar ada dan aktif di master Departemen |
| Pelaku | Petugas HR dengan hak `OrganizationUnit : Create` atau `OrganizationUnit : Update` |
| Pemicu | Petugas menyimpan unit baru atau perubahan unit di menu Struktur Organisasi |
| Prasyarat | Departemen sudah dibuat dan aktif di menu Departemen |
| Langkah | 1. Petugas memilih Departemen Terkait. 2. Server memeriksa departemen itu ada, aktif, dan tidak dihapus. 3. Bila lolos, unit disimpan seperti biasa |
| Aturan | Departemen boleh dikosongkan (`null`). Bila diisi, wajib aktif |
| Jalur tidak normal | Departemen nonaktif, terhapus, atau ID tidak dikenal → `400` "Departemen tidak ditemukan atau tidak aktif." Data tidak tersimpan |
| Hasil akhir | Unit tersimpan dengan departemen yang sah, atau ditolak dengan pesan yang menjelaskan penyebabnya |

Contoh: departemen "Gizi Lama" dinonaktifkan pada 1 November. Petugas mencoba menyimpan unit
"Instalasi Gizi" yang masih menunjuk "Gizi Lama". Server menolak dengan pesan di atas. Petugas
memilih departemen aktif "Gizi Klinik", lalu menyimpan ulang dan berhasil.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/Corporate/HumanResource/MasterData/Organization/Controllers/OrganizationUnitController.cs` (`ValidateRequestAsync`, `Create`, `Update`)
- `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstDepartment.cs`
- `Migrations/20260726161839_initializeBigModulHRD2.cs` (foreign key departemen)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/MasterData/Organization/Controllers/OrganizationUnitController.cs` | `ValidateRequestAsync` menambah pemeriksaan `MstDepartment` (ada, `IsActive`, `!IsDelete`) bila `DepartmentId` terisi dan bukan `Guid.Empty`, mengikuti pola pemeriksaan legal entity/fasilitas di berkas yang sama |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Kode status baru yang mungkin muncul pada `POST /` dan `PUT /{id}`: `400` "Departemen tidak ditemukan atau tidak aktif." Bentuk request dan response tidak berubah |
| Database | Tidak ada perubahan schema. Tanpa migration. Data yang sudah tersimpan tidak disentuh |
| Keamanan/Auth | Tidak ada perubahan atribut akses |

---

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Master Data / Organization Unit

Base URL: `api/v1/corporate/human-resource/master-data/organization-units`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat unit baru | `OrganizationUnit : Create` | `CreateOrganizationUnitRequest` | `object` (`Id`, `UnitCode`, `UnitName`) |
| `PUT` | `/{id}` | Mengubah unit | `OrganizationUnit : Update` | `UpdateOrganizationUnitRequest` | sama seperti sebelumnya |

Kode status: `200` berarti tersimpan. `400` berarti isian tidak valid, termasuk pesan baru
"Departemen tidak ditemukan atau tidak aktif." (pilih departemen lain atau aktifkan dulu di menu
Departemen). `403` berarti tidak punya hak. `404` (khusus `PUT`) berarti unit tidak ditemukan.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratch>` | 0 error, 610 warning (baseline lama), tidak ada warning pada berkas Organization; 4 menit 31 detik | `PASS` | Log build sesi 24 November 2026 |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (controller + DTO) | VIOLATION 0, REVIEW 0, INFO 0 — `PASS` | `PASS` | Keluaran perintah |
| Review diff | +15 baris di `ValidateRequestAsync`, hanya scope task | `PASS` | `git diff` |
| Uji API AC 1–4 | Tidak dijalankan | `NOT RUN` | Backend yang sedang berjalan memakai build lama; menjalankan backend baru di DB dev bersama butuh izin terpisah |

Uji manual: `NOT FEASIBLE` pada sesi ini.

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Tidak dijalankan:** uji API runtime. Setelah backend di-restart, kirim `POST /` dengan
`departmentId` acak; jawabannya harus `400` dengan pesan di atas.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Departemen aktif diterima seperti sekarang | Terpenuhi di source | Pemeriksaan lolos bila `IsActive && !IsDelete` |
| 2. Departemen nonaktif ditolak `400` dengan pesan di atas | Terpenuhi di source; runtime `NOT RUN` | `AnyAsync(... x.IsActive ...)` → `(false, "Departemen tidak ditemukan atau tidak aktif.")`; `Create`/`Update` mengubahnya menjadi `400` |
| 3. `departmentId` yang tidak ada ditolak `400`, bukan `500` | Terpenuhi di source | Pemeriksaan berjalan sebelum `SaveChangesAsync`, jadi foreign key tidak lagi tersentuh |
| 4. `departmentId: null` tetap diterima | Terpenuhi di source | Pemeriksaan hanya berjalan bila `HasValue && != Guid.Empty` |
| DoD: laporan dan tanda roadmap | Terpenuhi | Laporan ini; roadmap backend bagian 32 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 610 warning build adalah baseline repository |
| Masalah yang diketahui | Pesan validasi lain di controller masih berbahasa campur ("Unit type tidak valid.") — dicatat `ISSUE-HRD-008` T4, di luar scope |
| Risiko tersisa | Unit lama yang terhubung ke departemen nonaktif tidak bisa disimpan ulang sebelum departemennya diganti. Ini disengaja, dan pesan `400` menjelaskan penyebabnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M Areas/Corporate/HumanResource/MasterData/Organization/Controllers/OrganizationUnitController.cs` (bersama `BE-HRD-463`), `M .../DTOs/OrganizationUnitDtos.cs` (`BE-HRD-463`) |
| Langkah berikutnya | Restart backend lokal, lalu uji API singkat AC 2–3 |
