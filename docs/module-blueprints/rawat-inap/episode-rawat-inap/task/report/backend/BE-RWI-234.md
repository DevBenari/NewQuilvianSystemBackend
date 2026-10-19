# Laporan Perubahan Backend — `BE-RWI-234`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-234` |
| Judul | High/Low Dependency tercatat per episode beserta riwayat |
| Slice | Petugas berwenang mencatat High/Low Dependency; riwayat tidak pernah ditimpa |
| Roadmap | [backend-roadmap-integrasi-denah.md](../../../roadmap/backend-roadmap-integrasi-denah.md) — kartu `BE-RWI-234` |
| Trace | FR-DNH-013, 014; RWI-DEC-302, 303; RWI-AC-440, 441; VAL-DNH-14–17; GUARD-DNH-02; INV-DNH-05, 06; NFR-804; DNH-AT-440, 441, 908 |
| Contract version | `0.13.0` approved — Muhammad Hamzah, 16 Oktober 2026 (`RWI-DEC-324`) |
| Dependency | Tidak ada |
| Klasifikasi | `HEAVY` — skor 11: satu repo 0, diperiksa > 20 = 2, diubah > 8 = 2, logika sedang 1, kontrak baru 2, schema/migration 2, auth terkait 1, workflow terbatas 1 |
| Task mode | `BACKEND` — instruksi eksplisit pengguna 16 Oktober 2026 |
| Target tulis | Batas source kartu + `InpatientEpisodeDtos.cs`/`InpEpisodeService.cs` (lima field detail), `Migrations/*` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `ca09cae909597442385ce4513be9c803549be10e` (branch `MHamzah`, belum di-commit) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai — source, migration M-DNH-2, build, dan penerapan ke `QuilvianNewDevHamzah` terbukti; pemberian hak `SetCareDependency` menunggu `BM-G03`, penayangan produksi menunggu `DNH-G04` |

---

## 1. Masalah yang diperbaiki

Rawat inap belum punya catatan tingkat ketergantungan pasien. Denah tidak dapat menampilkan label "High Dependency (1 HD · 1 LD)" dan tidak ada jejak siapa menilai apa dan kapan.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Mencatat High/Low Dependency yang berlaku saat ini beserta riwayat yang tidak pernah ditimpa |
| Pelaku | Petugas admisi (episode `Draft`), perawat penanggung jawab aktif, DPJP aktif — ketiganya juga wajib memegang hak `InpatientEpisode : SetCareDependency` |
| Pemicu | Petugas menyimpan HD/LD dari layar episode |
| Prasyarat | Episode ada dan belum `Closed`/`Cancelled` |

Langkah utama:

1. Server memeriksa isian: level 1 (High) atau 2 (Low); catatan maks. 500 karakter.
2. Draft yang lewat batas waktu digugurkan lebih dulu (perilaku existing) → hanya baca.
3. Server mengunci baris episode (`FOR UPDATE`) lalu membaca ulang nilainya.
4. Status `Closed`/`Cancelled` → hanya baca.
5. Sumber ditentukan dari data, bukan nama peran: `Draft` → AdmissionNote; `Admitted`/`DischargePending` → penugasan perawat aktif dengan `employee_id` pengguna → NurseAssessment, atau DPJP aktif dengan `doctor_id` pengguna → DoctorDecision; selain itu ditolak. SuperAdmin tidak melewati penjaga ini.
6. `ExpectedSetAt` dibandingkan dengan `CareDependencySetAt` terakhir (presisi mikrodetik); keduanya kosong dianggap sama.
7. Server menulis satu baris `InpCareDependencyHistory` (nilai lama, nilai baru, sumber, pelaku, waktu, catatan) dan lima kolom episode dalam satu transaksi.

| Dari | Tindakan | Ke | Siapa | Syarat |
| --- | --- | --- | --- | --- |
| Belum dinilai | Catatan awal | High/Low | Petugas admisi | Episode `Draft`, sumber AdmissionNote |
| Belum dinilai/High/Low | Penilaian perawat | High/Low (boleh sama) | Perawat penanggung jawab aktif | `Admitted`/`DischargePending` |
| Belum dinilai/High/Low | Keputusan DPJP | High/Low | DPJP aktif | `Admitted`/`DischargePending` |
| Nilai apa pun | Episode ditutup/dibatalkan | Tetap | — | Hanya baca |

> **Contoh.** Ny. Sari diadmisi; petugas admisi mencatat Low (AdmissionNote). Hari kedua perawat penanggung jawab mencatat High (NurseAssessment). Hari keempat DPJP mencatat Low (DoctorDecision). Riwayat memuat tiga baris, terbaru di atas; episode bernilai Low. Bila perawat menyimpan Low lagi tanpa perubahan, riwayat bertambah satu baris sebagai penilaian ulang.

Jalur tidak normal:

| Keadaan | Hasil |
| --- | --- |
| Level 3 | 400 `CARE_DEPENDENCY_LEVEL_INVALID` |
| Catatan > 500 karakter | 400 `INVALID_INPUT` |
| Perawat tanpa penugasan aktif / dokter bukan DPJP aktif | 403 `CARE_DEPENDENCY_ACTOR_NOT_ASSIGNED` |
| Episode `Closed`/`Cancelled` atau Draft gugur | 422 `EPISODE_READ_ONLY` |
| Dua perawat menyimpan bersamaan dari nilai yang sama | Yang pertama berhasil; yang kedua menunggu kunci, lalu 409 `STALE_CARE_DEPENDENCY`; riwayat bertambah satu baris |

Hasil akhir: nilai pada episode selalu sama dengan baris riwayat terbaru (INV-DNH-06). HD/LD tidak dibaca kelayakan bed, kategori transfer, maupun tarif (INV-DNH-05).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Backend 15.1/15.4, kamus data 22.2.4/22.2.5/22.3, API 14.2/14.3, hak akses 12.1–12.3, transisi 12.2; `InpEpisode.cs`, `InpEpisodeService.Assignments.cs` (`SetIsolationRequirementAsync`, `IsActiveDoctorAsync`), `InpEpisodeService.cs` (`GetDetailResponseAsync`, `ExpireDraftIfDueAsync`), `InpatientEpisodeController.cs`, `InpatientActorClaims.cs`, `InpNurseAssignment.cs`, `InpEpisodeConfiguration.cs`, `InpStatusHistoryConfiguration.cs`, `ApplicationDbContext.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Enums/InpCareDependencyLevel.cs`, `InpCareDependencySource.cs` | Baru |
| `Areas/HealthServices/InPatientManagement/Models/InpCareDependencyHistory.cs` | Baru, mewarisi `IdentityModel` |
| `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs` | Lima kolom + navigasi pencatat + koleksi riwayat |
| `Repositories/Configurations/HealthServices/InPatientManagement/InpCareDependencyHistoryConfiguration.cs` | Baru: tiga CHECK, INDEX(`EpisodeId`, `RecordedAtUtc`, `Id`), FK Restrict |
| `Repositories/Configurations/HealthServices/InPatientManagement/InpEpisodeConfiguration.cs` | Konversi int, panjang catatan, dua CHECK, FK pencatat |
| `Repositories/ApplicationDbContext.cs` | DbSet `InpCareDependencyHistories`; penjaga append-only riwayat (baris 1263) |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.CareDependency.cs` | Baru: `SetCareDependencyAsync`, `GetCareDependencyHistoryAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientCareDependencyDtos.cs` | Baru: `SetCareDependencyRequest`, `CareDependencyHistoryResponse` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs`, `Services/InpEpisodeService.cs` | Lima field HD/LD pada `InpatientEpisodeDetailResponse` (baris 1188) |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | `PATCH /{id}/care-dependency` (baris 433), `GET /{id}/care-dependency-history` (baris 476) |
| `Migrations/20261016041113_AddCareDependencyToInpEpisode.cs` (+ Designer, snapshot) | M-DNH-2 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru; lima field aditif pada detail episode |
| Database | M-DNH-2: lima kolom nullable `InpEpisode`, tabel `InpCareDependencyHistory`, CHECK, FK, index. **Diterapkan** ke `QuilvianNewDevHamzah` 16 Oktober 2026. Tidak ada backfill; episode lama bernilai "belum dinilai" |
| Keamanan/Auth | Action baru `InpatientEpisode : SetCareDependency` (muncul di layar Akses Role; tidak ada role yang otomatis menerimanya). Kewenangan per episode diturunkan dari data penugasan, bukan nama peran. Catatan HD/LD tidak masuk logger |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/care-dependency` | Mencatat atau mengubah High/Low Dependency | `InpatientEpisode : SetCareDependency` | `SetCareDependencyRequest` {`Level`, `Note?`, `ExpectedSetAt?`} | `InpatientEpisodeDetailResponse` |
| `GET` | `/{id}/care-dependency-history` | Riwayat HD/LD, terbaru dulu | `InpatientEpisode : Read` | `pageNumber`, `pageSize` | `PagedResult<CareDependencyHistoryResponse>` |
| `GET` | `/{id}` | Detail episode, kini dengan lima field HD/LD | `InpatientEpisode : Read` | `id` | `InpatientEpisodeDetailResponse` |

Pemeriksaan kontrak hak akses: `[AccessController(ControllerName = "InpatientEpisode")]`; `[AccessAction("SetCareDependency", …, AccessType = AccessTypes.Update)]` dan `[AccessPermission("InpatientEpisode", "SetCareDependency")]` cocok huruf demi huruf.

Kode status: 400 isian salah; 403 bukan pencatat yang ditugaskan; 404 episode tidak ada; 409 nilai sudah diubah orang lain; 422 episode hanya baca.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | `Inp` ACTIVE; entity baru `InpCareDependencyHistory` (QBE-ENT-001, NAM-002, CFG-001, MOD-002); QBE-SVC-001, PERM-001, LOG-001 (pelaku pada baris riwayat), TXN-001, VAL-001, DTO-001, AUD-001 | `PASS` | Bagian 3.2 |
| Larangan hardcode role | Tidak ada `IsInRole`/nama peran; pencatat dari `InpNurseAssignment.EmployeeId` dan DPJP aktif | `PASS` | `InpEpisodeService.CareDependency.cs:108-130` |
| `dotnet build --no-incremental` akhir | 0 error, 286 warning; 0 warning dari kode baru | `PASS` | Keluaran perintah |
| `dotnet ef migrations add AddCareDependencyToInpEpisode` | Hanya kolom/tabel/CHECK/FK/index baru | `PASS` | Berkas migration |
| Penerapan ke `QuilvianNewDevHamzah` | Lima kolom `InpEpisode`, 18 kolom `InpCareDependencyHistory`, tiga CHECK riwayat + dua CHECK episode, index riwayat ada | `PASS` | Kueri katalog lewat `dotnet fsi` + Npgsql |
| DNH-AT-440 | Sumber dari peran; 403/409/422 sesuai kontrak | `PASS` | `InpEpisodeService.CareDependency.cs:54, 76, 103, 119, 123, 129, 137` |
| DNH-AT-441 | `CareDependency*` tidak dibaca `EvaluatePlacementEligibilityCore`, `CompareAsync`, maupun Billing | `PASS` | Penelusuran pemakaian: hanya service episode, detail, `HoldingContext`, ringkasan denah |
| DNH-AT-908 | Kunci `FOR UPDATE` + cek `ExpectedSetAt` sesudah kunci | `PASS` (penelusuran source) | `InpEpisodeService.CareDependency.cs:86, 133-138` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT FEASIBLE` — backend lokal belum dijalankan ulang dengan build baru; hak `SetCareDependency` belum diberikan ke departemen/posisi mana pun.

**Tidak dijalankan:** uji PostgreSQL dua permintaan bersamaan (DNH-AT-908 dibuktikan lewat penelusuran kunci baris, bukan eksekusi).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Draft + petugas admisi → Low AdmissionNote; perawat aktif → High NurseAssessment; DPJP aktif → Low DoctorDecision; riwayat tiga baris terbaru di atas | Terpenuhi | `SetCareDependencyAsync`; `GetCareDependencyHistoryAsync` urut `RecordedAtUtc` turun |
| Nilai sama tetap menambah satu baris riwayat | Terpenuhi | Baris riwayat selalu ditambahkan (baris 146) |
| Perawat tanpa penugasan → 403 `CARE_DEPENDENCY_ACTOR_NOT_ASSIGNED`; Closed/Cancelled → 422 `EPISODE_READ_ONLY` | Terpenuhi | Baris 103, 129 |
| `ExpectedSetAt` berbeda → 409 `STALE_CARE_DEPENDENCY`; level 3 → 400 `CARE_DEPENDENCY_LEVEL_INVALID` | Terpenuhi | Baris 54, 137 |
| Tidak ada jalur kelayakan, kategori transfer, maupun tarif yang membaca HD/LD | Terpenuhi | Penelusuran pemakaian |
| D-BE | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `InpEpisodeService.cs` memuat satu baris perubahan paralel issue 022 (`AdmittedAt = now` saat Draft dibuat) yang belum di-commit; task ini hanya menambah proyeksi lima field di `GetDetailResponseAsync` |
| Masalah yang diketahui | Kriteria klinis HD/LD milik SOP keperawatan (RWI-OQ-141); sistem hanya mencatat |
| Risiko tersisa | Klaim `employee_id`/`doctor_id` wajib terisi pada token; pengguna tanpa klaim tidak dapat mencatat setelah pasien dirawat |
| Perubahan sampingan | `NONE` khusus task ini |
| Interupsi | Build fase ini sempat gagal OOM compiler; dipulihkan (lihat `BE-RWI-232`) |
| Status Git | Lihat laporan `BE-RWI-240` bagian 7 |
| Langkah berikutnya | Admin memberi hak `SetCareDependency` (`BM-G03`); `FE-RWI-252` |
