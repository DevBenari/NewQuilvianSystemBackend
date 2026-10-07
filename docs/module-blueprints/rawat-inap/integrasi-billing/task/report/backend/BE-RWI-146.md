# Laporan Perubahan Backend — `BE-RWI-146`

## Pembaruan eksekusi menyeluruh — 5 Oktober 2026

Bagian ini adalah bukti terbaru. Bagian laporan sebelumnya, bila ada, dipertahankan sebagai riwayat.

### Metadata eksekusi terakhir

| Field | Nilai |
| --- | --- |
| Task | `BE-RWI-146` — Webhook izin pulang dan override pulang fisik dicabut |
| Roadmap / slice | [backend-roadmap-finishing.md](../../../roadmap/backend-roadmap-finishing.md), kartu `BE-RWI-146` |
| Tanggal / mode | 5 Oktober 2026 / BACKEND |
| Status | SELESAI KODE — build dan validasi terbatas PASS; UAT klinis lengkap NOT RUN |
| Branch / upstream | `MHamzah` / `origin/MHamzah`; sesuai manifest |
| SHA awal sesi | `0a10899435bcd4cd54e998a060465589b6652643` |
| Kontrak / trace | `RWI-BP-001` revision 8; API `1.1.0 approved`; `1.1.0`: API 3.1 (tiga endpoint dihapus), 3.2 (`close-with-override`); validasi 2; permission 5 |
| Wewenang | Pengguna meminta seluruh BE-RWI-146–159, build dan migration pada akhir; database update development diotorisasi roadmap 5 Oktober 2026 |
| Target tulis | NewQuilvianSystemBackend; source dalam slice task, migration I6, laporan, roadmap dan traceability integrasi-billing |
| Dependency | —; source/schema dependency tersedia; bukti klinis lengkap tidak diturunkan dari status kode |
| Area / Module / prefix | HealthServices / InPatientManagement (Inp) dan BillingManagement (Bil), ACTIVE |
| Applicability | Review ulang source existing; tidak ada perubahan source task ini pada sesi terbaru; LEGACY MIGRATION NOT APPLICABLE |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 2, database 0, keamanan 2, workflow 1 |
| QBE applicable | QBE-API-001, QBE-PERM-001, QBE-VAL-001, QBE-DTO-001, QBE-AUD-001 |
| Governance | AGENTS.md, engineering repository, dan rules/backend suite terbaca; engineering identik dengan suite, registry repository lebih baru pada domain lain; Bil/Inp/Phm sama ACTIVE. Repository canonical mengikuti AGENTS.md |
| Model | Codex, GPT-6 |

### Masalah, proses bisnis, dan perubahan

Petugas masuk, mengajukan penutupan dengan alasan, lalu sistem memeriksa `InpatientDischarge : CloseOverride`. Penutupan khusus tetap berjejak; PIN dan nama peran tidak dipakai. Sesi ini memeriksa ulang implementasi existing dan membangunnya bersama seluruh roadmap.

Tidak ada perubahan source baru untuk task ini; controller dan service penutupan diperiksa ulang.

Berkas bukti:

- `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs#CloseWithOverride`
- `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs`

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Kontrak existing dipertahankan sesuai kartu; perubahan removal yang disetujui tetap berlaku |
| Database | Tidak menambah schema pada task ini; dependency I1/I2/E6/I6 telah diterapkan |
| Audit / keamanan | Database audit tetap melalui IdentityModel/SaveChanges existing; application log terpisah. Hak akses mengikuti atribut Access; tidak ada wewenang dari nama peran |
| Consumer frontend | Frontend dibaca saja; consumer lama masih perlu mengikuti pemisahan Read/ViewAmount dan pencabutan route sesuai task pendamping |

### Endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/close-with-override` | Penutupan khusus dengan alasan | `InpatientDischarge : CloseOverride` |

### Verifikasi terbaru

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE Strict WorkingTree akhir | 27 file; VIOLATION 0, REVIEW 0, INFO 0 | PASS |
| Build awal | Dua CS1061 replay (`Status`); diperbaiki menjadi `EpisodeStatus` | NEW ERROR — sudah diperbaiki |
| `dotnet build QuilvianSystemBackend.csproj --nologo --no-restore` | Build akhir mencakup semua source dan metadata migration; 0 Error(s), 233 Warning(s), 00:05:31.27, exit 0 | PASS |
| `dotnet ef migrations add AddRawatInapBillingEncounterLink --project QuilvianSystemBackend.csproj --no-build` | Migration I6 dibuat setelah implementasi source lengkap; scope Up hanya tabel tautan | PASS |
| `dotnet ef database update --project QuilvianSystemBackend.csproj --no-build` dengan Development | Applying I6, Done., exit 0; pemeriksaan `GetPendingMigrationsAsync` menghasilkan 0 | PASS |
| Konfirmasi database setelah perbaikan penutup | Database already up to date, Done., exit 0; tidak ada migration tambahan | PASS |
| Verifikasi runtime terbatas terhadap DLL akhir | 35 pemeriksaan PASS: serialisasi, 11 metadata action, filter 401/403, whitelist, DeadLetter, EF model, pending migration, DryRun, query rincian | PASS |
| DryRun database development | 3 kandidat; data outbox dan RequiresReview/RowVersion sebelum/sesudah identik | PASS |
| Pembacaan rincian tersimpan | Breakdown dan aggregate konsisten pada 3 episode development; tidak memanggil kalkulasi/tulis | PASS |
| Sampel penyerahan resep untuk retur | 0 prescription sumber ditemukan; perhitungan retur nyata tidak dibuktikan | NOT RUN |
| UAT klinis lengkap, akun HTTP nyata, replay tulis, gangguan worker, regresi rawat jalan dan rollback Down | Tidak dijalankan; hasil pemeriksaan terbatas tidak menjadi bukti skenario ini | NOT RUN |
| QBE percobaan ulang | Git stderr peringatan CRLF memenuhi pipe checker dan menahan proses; dipulihkan dengan core.safecrlf=false hanya pada environment proses, tanpa mengubah konfigurasi repository atau mode Strict | EXISTING / ENVIRONMENT ISSUE, dipulihkan |
| Warning compiler dan harness | 233 warning aplikasi; harness sementara menampilkan MSB3277 DependencyModel 9.0.12/9.0.18; tidak mengubah package aplikasi | EXISTING / ENVIRONMENT ISSUE — warning aplikasi tidak seluruhnya dibuktikan sebagai baseline |



Uji manual: **PASS terbatas**. Principal filter berupa identitas sintetis bernama Cashier tanpa ID pengguna valid; hasil 403 membuktikan filter berjalan, bukan UAT akun rumah sakit dengan role-permission nyata. Pemeriksaan dijalankan melalui console sementara di luar repository; tidak ada task/project automated test baru. Database memakai konfigurasi Development, nama database bermarker development dan tanpa marker produksi; host remote, bukan loopback. Connection string dan data pasien tidak dicatat.

Bukti output lokal (`%TEMP%` = `C:/Users/Admin/AppData/Local/Temp`):

| File | SHA256 |
| --- | --- |
| `Quilvian-billing-finishing-final-build.log` | `7B241A1273B063C92AA2F5043DC119557AA202166AC296A66478215DD360FA41` |
| `Quilvian-billing-finishing-database-update.log` | `6FE5D4EFB20FA0C5267EFDE2783F8698FEC643E2BE47982E9097DB5CDA894B14` |
| `Quilvian-billing-finishing-database-final-check.log` | `DA95D2A434379F615CAE4C85266F2E21C49E4E8A120A4CE9F9F5EBEEFE2DFA7F` |
| `Quilvian-billing-finishing-runtime-final.log` | `9322A732FA830AA4F40F9BB9BD53172B1D7F6C8D35D2436C85E70B7932A75F66` |
| `Quilvian-billing-finishing-qbe-final.log` | `D9FB75C1901806EBA0CF386A83275FCB4598223561A895830C327013D9FB47DF` |

### Acceptance criteria dan Definition of Done terbaru

| Kriteria roadmap | Status | Bukti |
| --- | --- | --- |
| 1. Ketiga route lama tidak terdaftar (404). | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs#CloseWithOverride`; `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` |
| 2. `close-with-override` tanpa token → 401; tanpa `InpatientDischarge : CloseOverride` → 403, apa pun nama perannya. | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs#CloseWithOverride`; `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` |
| 3. Alasan kosong atau terlalu pendek → 400 sesuai validasi 2. | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs#CloseWithOverride`; `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` |
| 4. Tidak ada sisa pembacaan PIN atau nama peran pada alur penutupan. | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs#CloseWithOverride`; `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` |
| 5. Baris registry `InpatientDischarge : CloseOverride` lahir dari atribut endpoint | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs#CloseWithOverride`; `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` |

Implementasi, build, migration development, dan laporan tersedia. DoD alur klinis lengkap tetap **belum terbukti** pada butir berstatus NOT RUN; status kode tidak sama dengan sign-off rilis.

### Catatan penutup

Dokumen `rules/backend/TEST_POLICY.md` yang dirujuk roadmap tidak tersedia pada suite ini. Semua governance canonical wajib lainnya terbaca; validasi mengikuti AGENTS.md dan review rules yang tersedia. Tidak ada project test repository. Dokumen luar slice yang sudah berubah sebelum atau selama sesi tidak disentuh. Tidak ada stage, commit, push, deployment, atau replay produksi.

Status Git task: laporan M; source existing task diperiksa ulang. Laporan dibuat pada working tree yang belum di-commit. Interupsi: NONE. Langkah berikutnya: consumer frontend pendamping dan UAT klinis sesuai acceptance matrix.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-146` |
| Judul | Webhook izin pulang dan override pulang fisik dicabut |
| Slice | `MVP-0` / `RWF-W0` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-146` |
| Trace | `FR-RWF-001`, `FR-RWF-005`; `RWI-DEC-166`, `RWI-DEC-167`, `RWI-DEC-186`, `RWI-DEC-187`; `UAT-RWF-02`, `UAT-RWF-12` |
| Contract version | `integrasi-billing` `1.1.0` **`approved`** (`RWI-DEC-221`): API 3.1 (tiga endpoint dihapus), 3.2 (`close-with-override`); validasi 2 (`VAL-RWF-05`, `06`); permission 5 |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 2, database 0, keamanan 2, workflow 1 |
| Task mode | `BACKEND` (dependency `BE-RWI-168` keperawatan, disetujui pengguna 5 Oktober 2026: "Ya, selesaikan dependency yang diperlukan") |
| Target tulis | `Areas/HealthServices/InPatientManagement/**`, `Program.cs` |
| Model | Codex (implementasi awal); Claude Opus 5.5 (review, perbaikan, build akhir, laporan) |
| Commit backend saat dikerjakan | `f32b2308` (branch `MHamzah`), working tree |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ Selesai. Kelima acceptance criteria terpetakan ke source; build akhir `PASS` |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (penghapusan controller lama; perubahan `close-with-override`) |
| QBE yang berlaku | `QBE-PERM-001` (kewenangan lewat metadata Access, bukan nama peran), `QBE-API-001`, `QBE-VAL-001`, `QBE-LOG-001` |
| Wewenang | Source: ya. Task ini tidak mengubah skema. Deployment: tidak |
| Branch | `MHamzah`; tidak ada operasi Git tulis |

## 1. Masalah yang diperbaiki

Audit paritas 30 September 2026 menemukan tiga jalan masuk yang mengubah status kepulangan tanpa pengaman yang layak:

| Endpoint lama | Masalah |
| --- | --- |
| `POST episodes/{episodeId}/discharge-clearance/webhook` | `[AllowAnonymous]`: siapa pun tanpa login dapat mengubah izin pulang |
| `POST episodes/{episodeId}/supervisor-override` | Override pulang fisik terpisah dari penutupan episode |
| `POST episodes/{episodeId}/confirm-physical-discharge` | Jalur kepulangan fisik kedua di luar `record-departure` |

Selain itu `close-with-override` memeriksa nama peran supervisor di kode (`User.IsSupervisor()`), sehingga admin tidak dapat mengatur siapa yang boleh menutup tanpa izin kasir lewat layar Akses Role.

## 2. Proses bisnis

**Tujuan.** Tidak ada lagi jalan masuk tanpa login atau berbasis nama peran yang mengubah status kepulangan. Penutupan tanpa izin kasir hanya bisa dilakukan pemegang `InpatientDischarge : CloseOverride` dengan alasan tertulis (`RWI-DEC-187`).

**Pelaku.** Petugas yang diberi `InpatientDischarge : CloseOverride` oleh admin.

**Langkah utama.**

1. Petugas membuka penutupan episode yang izin kasirnya belum `CLEARED`.
2. Petugas menulis alasan dan mengirim `close-with-override` beserta `ExpectedVersion`.
3. Filter hak akses memeriksa permission `InpatientDischarge : CloseOverride`; nama peran tidak dibaca.
4. Service menolak alasan kosong atau hanya tanda baca, lalu menutup episode dengan syarat lain tetap berlaku, dan mencatat status kasir saat itu (`BE-RWI-153`).

**Contoh.** Keluarga pasien sudah pulang membawa jenazah sebelum kasir menyelesaikan administrasi. Supervisor dengan permission `CloseOverride` menulis alasan "pasien meninggal, administrasi diselesaikan keluarga besok". Episode tertutup dengan status kasir `PENDING` tercatat. Petugas lain yang hanya punya `Close` mendapat `403`, apa pun nama perannya.

**Aturan bisnis.**

| Aturan | Akibat |
| --- | --- |
| Tanpa token | `401` |
| Tanpa `InpatientDischarge : CloseOverride` | `403` dari filter hak akses bersama (`VAL-RWF-06`; teks pesan mengikuti filter, belum diselaraskan dengan kalimat validation matrix) |
| Alasan kosong atau hanya tanda baca | `400` `INP-CLS-012` "Alasan penutupan tanpa izin kasir wajib diisi dengan kalimat yang jelas." (`VAL-RWF-05`) |
| Route lama | Tidak terdaftar (`404`) |

**Hasil akhir.** Satu-satunya jalan menutup tanpa izin kasir adalah `close-with-override` yang dijaga permission dan alasan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

API 3.1–3.2, validation matrix 2, permission matrix 5, `InpatientDischargeClearanceController` di HEAD, `InpatientDischargeController`, `InpDischargeService.Closure.cs`, `InpatientActorClaims` (anti-pola nama peran), aturan hak akses.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeClearanceController.cs` | **Dihapus** beserta tiga endpoint lama |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | `close-with-override` tidak lagi meneruskan `User.IsSupervisor()`; respons gagal membawa kode (`INP-CLS-0xx`). Review Claude: mengembalikan argumen `IsSupervisor` pada `UpsertSummary` dan mencabut argumen yang salah tempel pada `DecideDischarge` (atas izin pengguna) |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | `CloseWithOverrideAsync` tanpa parameter dan pemeriksaan supervisor; validasi alasan `INP-CLS-012` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientClosureDtos.cs` | `CloseEpisodeOverrideRequest` membawa `ExpectedVersion` (kontrak 3.2) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Breaking sesuai kontrak:** tiga endpoint dihapus; `close-with-override` mewajibkan `ExpectedVersion`. Pemanggil frontend lama (`FE-RWI-098`, `FE-RWI-099`) harus dirilis bersama `FE-RWI-167` dan `FE-RWI-168` |
| Database | `NOT APPLICABLE` pada task ini (kolom `Version` lahir di `BE-RWI-153`) |
| Keamanan/Auth | Satu endpoint `[AllowAnonymous]` hilang. Kewenangan override kini sepenuhnya dari `InpatientDischarge : CloseOverride` di layar Akses Role; resource lama `InpatientDischargeClearance` tidak lagi dideklarasikan |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{episodeId}/close-with-override` | Menutup episode tanpa izin kasir dengan alasan; menyimpan status kasir saat itu | `InpatientDischarge : CloseOverride` | `CloseEpisodeOverrideRequest { Reason, ExpectedVersion }` | `InpatientEpisodeDetailResponse` |

Kode status: `400` `INP-CLS-012` alasan tidak jelas atau `ExpectedVersion` kosong; `401` belum login; `403` tanpa permission; `404` episode tidak ditemukan; `409` versi episode berubah; `422` syarat penutupan lain belum terpenuhi.

**Endpoint yang dihapus (tidak lagi tersedia):** `POST api/v1/health-services/inpatient-management/episodes/{episodeId}/discharge-clearance/webhook`, `…/supervisor-override`, `…/confirm-physical-discharge`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` — build akhir | `Build succeeded`, `0 Error(s)`, `233 Warning(s)` | `PASS` | Log build akhir |
| Build tahap penyelesaian | Codex salah menempelkan `User.IsSupervisor()` ke `DecideDischargeAsync` dan mencabutnya dari `UpsertSummaryAsync` (CS1501, CS1503). Diperbaiki Claude; pencabutan dari `DecideDischarge` sempat ditahan classifier lalu dikerjakan atas izin eksplisit pengguna | `NEW ERROR` (sudah diperbaiki) | Log build Codex dan Claude |
| Pemeriksaan diff terhadap HEAD | Satu-satunya perubahan terkait supervisor di controller adalah pencabutan dari `close-with-override` | `PASS` | `git diff` controller |
| Pencarian PIN, kata sandi, nama peran di alur penutupan | Nol temuan di `InpDischargeService.Closure.cs`, `.Departure.cs`; `CloseEpisodeOverrideRequest` hanya `ExpectedVersion` dan `Reason` | `PASS` | Pencarian source |
| Validator registry permission (baseline naked) | `KnownUnenforcedBusinessEndpoints` kosong, sehingga penghapusan endpoint tidak meninggalkan entri basi | `PASS` | `PermissionRegistryDescriptor.cs:59-60` |
| Pemeriksaan statis atribut hak akses | `CloseEpisodeWithOverride` cocok `InpatientDischarge` ↔ `CloseOverride` | `PASS` | Skrip pemeriksa atribut |
| Percobaan Swagger `UAT-RWF-02` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas instruksi pengguna 5 Oktober 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` pada sesi ini (aplikasi tidak dijalankan).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ketiga route lama tidak terdaftar (404) | Terpenuhi | Controller dihapus; tidak ada deklarasi route serupa di source |
| 2. `close-with-override` tanpa token → 401; tanpa `InpatientDischarge : CloseOverride` → 403, apa pun nama perannya | Terpenuhi | `[Authorize]` kelas; `[AccessPermission("InpatientDischarge", "CloseOverride")]`; tanpa `IsSupervisor` |
| 3. Alasan kosong atau terlalu pendek → 400 sesuai validasi 2 | Terpenuhi | `CloseWithOverrideAsync` → `INP-CLS-012` (`VAL-RWF-05`: kosong atau hanya tanda baca) |
| 4. Tidak ada sisa pembacaan PIN atau nama peran pada alur penutupan | Terpenuhi | Pencarian source nol temuan |
| 5. Baris registry `InpatientDischarge : CloseOverride` lahir dari atribut endpoint | Terpenuhi | `[AccessAction("CloseOverride", …)]` |
| DoD: build tanpa error; laporan; roadmap dan traceability | Terpenuhi | Bagian 5 |
| DoD: percobaan Swagger | **Dikecualikan atas instruksi pengguna 5 Oktober 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Rilis backend ini memutus pemanggil frontend lama (`FE-RWI-098`, `FE-RWI-099`); rilis bersama `FE-RWI-167`, `FE-RWI-168` |
| Masalah yang diketahui | Temuan legacy di luar scope: `UpsertSummary` masih meneruskan `User.IsSupervisor()` untuk amandemen resume (anti-pola nama peran). Tidak diubah tanpa wewenang |
| Risiko tersisa | Admin perlu memberikan `CloseOverride` kepada petugas yang tepat; tanpa itu hanya SuperAdmin yang dapat menutup tanpa izin kasir |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi Codex terhenti karena batas penggunaan penyedia; satu edit ditahan classifier auto mode dan dikerjakan sesudah izin pengguna |
| Status Git | 151 entri `git status --short` (63 `M`, 3 `D`, 85 `??`); berkas task ini pada 3.2; tidak ada stage/commit/push |
| Langkah berikutnya | `FE-RWI-167`, `FE-RWI-168` |
