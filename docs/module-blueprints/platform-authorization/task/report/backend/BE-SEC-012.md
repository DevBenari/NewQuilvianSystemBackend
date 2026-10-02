# Laporan Perubahan Backend — `BE-SEC-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-SEC-012` |
| Judul | Remediasi *naked endpoint* otorisasi dan invarian verifier |
| Slice | Task terpisah di luar rantai — blocker keamanan yang menahan `BE-SEC-003B` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) — bagian *Task terpisah di luar rantai* |
| Trace | [`evidence/12`](../../../evidence/12-authorization-orphan-audit.md) bagian C.3, D, E; [`evidence/13`](../../../evidence/13-workschedule-dormant-grant-deployment-blocker.md) |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API yang berubah; hanya atribut otorisasi yang ditambahkan |
| Dependency | Audit orphan `evidence/12` — `CLOSED` |
| Klasifikasi | `MEDIUM` — 5 controller + 2 service keamanan bersama + 1 verifier; tanpa perubahan skema, tanpa perubahan perilaku bisnis |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/`, `Services/Security/`, `Constants/`, `Program.cs`, `tools/authorization-verifier/`, `docs/module-blueprints/platform-authorization/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `4ff7b9987cda7d11df22080ff1b2f849bbc97319` |
| Tanggal | 16 September 2026 |
| Status | **Selesai** untuk perbaikan source dan invarian. Penerapan ke lingkungan **ditahan** oleh [`evidence/13`](../../../evidence/13-workschedule-dormant-grant-deployment-blocker.md) |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` (HR Master Data) dan `Shared Platform` (Services/Security) |
| Module | `HUMAN_RESOURCE_MASTER_DATA`; `Platform Authorization` |
| Submodule | `AttendanceAndSchedule` |
| Pemilik/prefix pada registry | `Hrd` — Human Resource. Lihat `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `TOUCHED LEGACY` |
| Status registry | Tidak berubah. **Tidak ada entity baru**, tidak ada rename, tidak ada prefix baru |
| QBE ID yang berlaku | Tidak ada QBE-MOD/QBE-NAM/QBE-DB yang terpicu: task ini tidak membuat entity operasional, tidak mengganti nama, dan tidak menyentuh database |

---

## 1. Masalah yang diperbaiki

Lima layar master data kepegawaian — Jadwal Kerja, Shift, Grup Shift, Pola Shift, dan Kalender
Kerja — memiliki tombol Ubah, Aktif/Nonaktif, dan Hapus yang **tidak memeriksa hak akses sama
sekali**. Yang diperiksa hanyalah "apakah orang ini sudah login".

Contoh konkretnya: seorang petugas Farmasi yang tidak punya satu pun hak pada modul Kepegawaian
tetap dapat menghapus jadwal kerja seluruh rumah sakit, karena backend tidak pernah bertanya
apakah ia berwenang. Jadwal kerja adalah dasar perhitungan kehadiran dan penggajian, sehingga
kerusakannya tidak berhenti di satu layar.

Total **20 endpoint** terdampak: untuk setiap controller, `GET /{id}`, `PUT /{id}`,
`PATCH /{id}/status`, dan `DELETE /{id}`.

Cacat ini lolos dari seluruh audit otorisasi sebelumnya karena alat pemeriksanya memang tidak
dirancang untuk melihatnya — penjelasan lengkapnya pada
[`evidence/12`](../../../evidence/12-authorization-orphan-audit.md) bagian E. Karena itu task ini
memperbaiki dua hal sekaligus: endpoint-nya, dan alat yang seharusnya menemukannya.

---

## 2. Proses bisnis

**Tujuan.** Memastikan setiap kemampuan pada lima master data kepegawaian hanya dapat dipakai oleh
Departemen × Posisi yang memang dicentang admin pada layar Pengaturan → Manajemen Role → Akses Role.

**Pelaku.** Admin yang mengatur hak akses; petugas Kepegawaian yang memakai layarnya.

**Pemicu.** Petugas membuka layar master data kepegawaian, lalu menekan Lihat Detail, Simpan,
Aktif/Nonaktif, atau Hapus.

**Langkah setelah perbaikan:**

1. Petugas menekan salah satu tombol di atas.
2. Backend memeriksa apakah ia sudah login. Bila belum → `401`.
3. Backend memeriksa apakah Departemen × Posisi-nya memegang izin yang dibutuhkan
   (`Read`, `Update`, atau `Delete` pada resource yang bersangkutan). Bila tidak → `403`
   dengan pesan "Anda tidak memiliki akses ke menu atau fitur ini."
4. Bila berwenang, aturan bisnis yang sudah ada berjalan apa adanya — tidak ada yang diubah.

**Jalur tidak normal — semuanya tetap sama seperti sebelumnya:**

| Keadaan | Hasil |
| --- | --- |
| Data tidak ditemukan | `404` "Data tidak ditemukan." |
| Nama sudah dipakai data lain | `400` "Nama sudah digunakan." |
| Data sudah dipakai shift atau penugasan | `400` "Data tidak dapat dihapus karena sudah digunakan." |
| Penghapusan | Tetap *soft delete* — `IsDelete = true`, data tidak hilang dari database |

**Hasil akhir.** Perilaku bisnisnya identik. Yang berubah hanya: sebelumnya semua orang boleh, kini
hanya yang dicentang admin.

> **Penting bagi admin.** Kemampuan `Update` dan `Delete` pada kelima master data itu **baru muncul**
> di layar Akses Role setelah perbaikan ini diterapkan, dan awalnya **belum dipegang siapa pun**.
> Admin perlu mencentangnya untuk Departemen × Posisi yang berwenang. Satu pengecualian penting
> dijelaskan pada [`evidence/13`](../../../evidence/13-workschedule-dormant-grant-deployment-blocker.md).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Alasan dibaca |
| --- | --- |
| `AGENTS.md` | Governance level-repository, mode task, keselamatan Git |
| `rules/backend/role-access-rules.md` | Kontrak penamaan `[AccessAction]` ↔ `[AccessPermission]` |
| `rules/backend/TEST_POLICY.md`, `REPORT_TEMPLATE.md` | Bukti verifikasi dan bentuk laporan |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Pemilik dan prefix modul HR |
| `Attributes/*.cs`, `Filters/AccessPermissionFilter.cs` | Jalur penegakan yang sebenarnya |
| `Services/Security/AccessPermissionService.cs` | Syarat sebuah policy memberi hak |
| `Services/Security/PermissionRegistryDescriptor.cs`, `PermissionRegistryValidator.cs` | Otoritas penemuan identitas dan titik butanya |
| `Seeders/AccessMenuSeeder.cs` | Perilaku reaktivasi baris registry yang tertutup |
| `tools/authorization-verifier/verify-authorization.sh` | Invarian permanen yang berlaku |
| `evidence/03`, `evidence/05`, `evidence/07`, `evidence/09` | Bukti pemegang hak dan scope pilot `BE-SEC-003B` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../AttendanceAndSchedule/Controllers/WorkScheduleController.cs` | 4 endpoint diberi `[AccessAction]` + `[AccessPermission]` |
| `.../ShiftController.cs` | sama, resource `Shift` |
| `.../ShiftGroupController.cs` | sama, resource `ShiftGroup` |
| `.../ShiftPatternController.cs` | sama, resource `ShiftPattern` |
| `.../WorkCalendarController.cs` | sama, resource `WorkCalendar` |
| `Constants/AuthorizationPolicies.cs` | **Baru.** Nama policy perangkat yang disetujui sebagai otorisasi alternatif, dalam satu konstanta bersama |
| `Program.cs` | Pendaftaran `KioskRead`, `QueueDisplayRuntimeRead`, `QueueDisplayRead` memakai konstanta itu. Nilai string-nya tidak berubah |
| `Services/Security/PermissionRegistryDescriptor.cs` | Titik buta ditutup; endpoint naked diklasifikasikan; baseline warisan dibekukan |
| `Services/Security/PermissionRegistryValidator.cs` | Invarian naked endpoint menjadi kriteria gagal |
| `tools/authorization-verifier/verify-authorization.sh` | Invarian `[5]` dan dua baris diagnostik |

**Tidak ada satu baris pun logika bisnis yang berubah.** Diff pada kelima controller seluruhnya
berupa penambahan atribut.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` untuk bentuk request/response — route, payload, dan kode status sukses tidak berubah. Yang bertambah: 20 endpoint kini dapat menjawab `403` bagi pengguna tanpa izin |
| Database | `NOT APPLICABLE` — tidak ada perubahan skema, tidak ada EF migration, tidak ada eksekusi database. **Migration status: NONE** |
| Keamanan/Auth | **Inti task ini.** 20 endpoint berpindah dari "cukup login" menjadi "harus dicentang admin". 10 identitas kanonik baru lahir tanpa pemegang. Satu blocker penerapan dicatat pada `evidence/13` |

---

## 4. Dokumentasi endpoint

Kelima controller memakai pola endpoint master data baku yang sama. Kolom **Hak akses** menunjukkan
keadaan **setelah** perbaikan; yang dicetak tebal adalah yang sebelumnya tidak ditegakkan.

#### Corporate / Human Resource / Master Data / Attendance And Schedule

Ganti `<Resource>` dengan `WorkSchedule`, `Shift`, `ShiftGroup`, `ShiftPattern`, atau
`WorkCalendar`, dan `<basis>` dengan `workschedules`, `shifts`, `shiftgroups`, `shiftpatterns`,
atau `workcalendars`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/master-data/<basis>/filters/metadata` | Pilihan filter untuk layar daftar | `<Resource> : Read` |
| `GET` | `/api/v1/corporate/human-resource/master-data/<basis>/summary` | Ringkasan jumlah data | `<Resource> : Read` |
| `GET` | `/api/v1/corporate/human-resource/master-data/<basis>` | Daftar data berhalaman | `<Resource> : Read` |
| `GET` | `/api/v1/corporate/human-resource/master-data/<basis>/options` | Daftar pilihan untuk dropdown | `<Resource> : Read` |
| `GET` | `/api/v1/corporate/human-resource/master-data/<basis>/{id}` | Detail satu data | **`<Resource> : Read`** |
| `POST` | `/api/v1/corporate/human-resource/master-data/<basis>` | Menambah data baru | `<Resource> : Create` |
| `PUT` | `/api/v1/corporate/human-resource/master-data/<basis>/{id}` | Mengubah data | **`<Resource> : Update`** |
| `PATCH` | `/api/v1/corporate/human-resource/master-data/<basis>/{id}/status` | Mengaktifkan atau menonaktifkan | **`<Resource> : Update`** |
| `DELETE` | `/api/v1/corporate/human-resource/master-data/<basis>/{id}` | Menghapus data (*soft delete*) | **`<Resource> : Delete`** |

`PATCH /{id}/status` sengaja memakai `Update`, bukan identitas tersendiri. Mengaktifkan dan
menonaktifkan adalah penyuntingan atribut pada master data, bukan transisi workflow, dan menambah
identitas baru berarti memutuskan kewenangan yang belum diputuskan pemilik modul.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj -c Release` | Berhasil. `0 Error(s)`, `189 Warning(s)` | `PASS` | Seluruh warning adalah CS1573/CS1734 dokumentasi XML yang sudah ada; nol warning pada berkas yang disentuh task ini |
| `tools/authorization-verifier/verify-authorization.sh --configuration Release` | `AUTHORIZATION VERIFIER: PASS` | `PASS` | Keluaran lengkap pada bagian 5.1 |
| Invarian `[1]` metadata gap | `0` | `PASS` | Setiap kunci baru punya `[AccessAction]` |
| Invarian `[2]` himpunan fallback | `69`, cocok persis | `PASS` | Tidak berubah oleh task ini |
| Invarian `[3]` identitas kanonik | Tidak ganda; satu resource satu modul | `PASS` | Keluaran verifier |
| Invarian `[4]` identitas wajib `BE-SEC-003` | `24 / 24` | `PASS` | Keluaran verifier |
| Invarian `[5]` naked endpoint (**baru**) | `0` baru, `8` utang baseline | `PASS` | Keluaran verifier |
| Audit ulang 20 endpoint | Nol endpoint `[Authorize]`-saja | `PASS` | Bagian 5.2 |
| Kontrak penamaan `role-access-rules` §3 | Argumen cocok huruf demi huruf | `PASS` | Dibuktikan invarian `[1]` = 0 |

**Uji manual:** `NOT FEASIBLE` — membuktikan `403` bagi pengguna tanpa izin menuntut aplikasi
dinyalakan dan database dihubungi, dan keduanya dilarang task ini. Penegakannya dibuktikan secara
statis: `[AccessPermission]` memasang `AccessPermissionFilter`, yang memanggil `HasAccessAsync`
sebelum action berjalan.

**`AUTOMATED TEST: NOT APPLICABLE`** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`). Invariannya dijaga authorization verifier permanen.

**Tidak dijalankan, dan sengaja:** `AccessMenuSeeder`, koneksi/tulis database, `dotnet ef database
update`, menyalakan aplikasi, `BE-SEC-003B` Tahap 1 dan Tahap 2, fetch/merge Integration, serta
commit/push. Seluruhnya dilarang eksplisit oleh instruksi task.

### 5.1 Keluaran authorization verifier

```text
Registry diagnostics (informational only):
  Modules       = 48
  Resources     = 339
  Actions       = 1296
  Fallback      = 69
  Metadata gaps = 0
  Naked (new)   = 0
  Naked (known) = 8

Required identities: 24 / 24 present.

Known unenforced business endpoints (baseline debt): 8
  WorkScheduleAssignment.CreateWorkScheduleAssignment  [POST]  modul HUMAN_RESOURCE_SCHEDULING
  WorkScheduleAssignment.DeleteWorkScheduleAssignment  [DELETE]  modul HUMAN_RESOURCE_SCHEDULING
  WorkScheduleAssignment.GetFilterMetadata  [GET]  modul HUMAN_RESOURCE_SCHEDULING
  WorkScheduleAssignment.GetSummary  [GET]  modul HUMAN_RESOURCE_SCHEDULING
  WorkScheduleAssignment.GetWorkScheduleAssignmentById  [GET]  modul HUMAN_RESOURCE_SCHEDULING
  WorkScheduleAssignment.GetWorkScheduleAssignments  [GET]  modul HUMAN_RESOURCE_SCHEDULING
  WorkScheduleAssignment.UpdateWorkScheduleAssignment  [PUT]  modul HUMAN_RESOURCE_SCHEDULING
  WorkScheduleAssignment.UpdateWorkScheduleAssignmentStatus  [PATCH]  modul HUMAN_RESOURCE_SCHEDULING

AUTHORIZATION VERIFIER: PASS
  [1] metadata gap                 : 0
  [2] fallback himpunan disetujui  : 69 cocok persis
  [3] identitas kanonik            : tidak ganda, satu resource satu modul
  [4] identitas wajib BE-SEC-003   : 24 / 24
  [5] endpoint bisnis telanjang    : 0 baru, 8 utang baseline
```

### 5.2 Hitungan registry baru

| Metrik | Sebelum | Sesudah | Selisih |
| --- | ---: | ---: | ---: |
| `SysActionAccess` / Actions | 1.286 | **1.296** | **+10** |
| `SysControllerAccess` / Resources | 339 | **339** | 0 |
| `SysApplicationModule` / Modules | 48 | **48** | 0 |
| Fallback kompatibilitas | 69 | 69 | 0 |
| Metadata gap | 0 | 0 | 0 |

Angka "sebelum" adalah hasil rekonsiliasi terhadap DEV atas kandidat beku yang sama, bukan angka
`1.246` pada `evidence/07` yang berasal dari HEAD lebih lama.

Selisih **+10** adalah tepat sepuluh identitas baru — `Update` dan `Delete` untuk kelima resource.
`Read` dan `Create` sudah ada sebelumnya, dan `GET /{id}` memakai `Read` yang sudah terdaftar.
Resource dan module tidak bertambah karena kelima resource itu memang sudah terdaftar.

> **Satu divergensi lama ikut tertutup.** Sebelum task ini, `BuildFromAssembly` melewati setiap
> method yang tidak membawa atribut akses, sehingga `WfpWorkScheduleAssignmentController` — yang
> seluruh endpoint-nya naked — tidak menyumbang apa pun dan modul `HUMAN_RESOURCE_SCHEDULING` tidak
> pernah terbaca jalur verifier. Jalur seeder `Build(provider)` selalu membacanya, dan database
> memang mencatat 48 modul. Kedua jalur kini menghasilkan angka yang sama.

### 5.3 Audit ulang 20 endpoint yang sebelumnya telanjang

Seluruh 45 endpoint pada kelima controller (masing-masing 9) diperiksa ulang dari source.

| Controller | `[Authorize]`-saja sebelum | `[Authorize]`-saja sesudah | Identitas baru |
| --- | ---: | ---: | --- |
| `WorkSchedule` | 4 | **0** | `WorkSchedule.Update`, `WorkSchedule.Delete` |
| `Shift` | 4 | **0** | `Shift.Update`, `Shift.Delete` |
| `ShiftGroup` | 4 | **0** | `ShiftGroup.Update`, `ShiftGroup.Delete` |
| `ShiftPattern` | 4 | **0** | `ShiftPattern.Update`, `ShiftPattern.Delete` |
| `WorkCalendar` | 4 | **0** | `WorkCalendar.Update`, `WorkCalendar.Delete` |

Tidak ada identitas kanonik ganda — dibuktikan invarian `[3]`. Tidak ada route, verb, DTO, atau
aturan bisnis yang berubah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Seluruh 20 endpoint telanjang memakai `[AccessPermission]` kanonik | Terpenuhi | Bagian 5.3 |
| 2. `[AccessPermission]` tetap satu-satunya identitas; `[AccessAction]` tetap metadata | Terpenuhi | Identitas tetap diturunkan `BuildCore` dari `[AccessPermission]`; invarian `[3]` `PASS` |
| 3. Tidak ada otoritas penemuan kedua | Terpenuhi | Invarian baru memakai `BuildCore` yang sama; allowlist policy ada pada satu konstanta yang juga dipakai `Program.cs` |
| 4. Perilaku bisnis endpoint tidak berubah | Terpenuhi | Diff kelima controller murni penambahan atribut |
| 5. Invarian naked endpoint menggagalkan verifier bila ada endpoint baru | Terpenuhi | Invarian `[5]`; `IsValid` juga menolak entri baseline basi |
| 6. Policy perangkat yang disetujui tidak ikut dilaporkan | Terpenuhi | `KioskRead`, `QueueDisplayRuntimeRead`, `QueueDisplayRead` pada `AuthorizationPolicies` |
| 7. `[AllowAnonymous]` tidak dilaporkan | Terpenuhi | Diperiksa pada method dan class |
| 8. Build `Release` lulus | Terpenuhi | `0 Error(s)` |
| 9. `MetadataGaps = 0`, 24/24 identitas, fallback tidak berubah | Terpenuhi | Bagian 5.1 |
| 10. Hitungan registry baru dibuat, bukan diasumsikan | Terpenuhi | Bagian 5.2 |
| 11. Blocker hak tertidur `WorkSchedule` dicatat | Terpenuhi | [`evidence/13`](../../../evidence/13-workschedule-dormant-grant-deployment-blocker.md) |
| 12. Tidak ada database write, seeder, atau aplikasi dinyalakan | Terpenuhi | Bagian 5, *Tidak dijalankan* |

**Butir Definition of Done yang belum terpenuhi — disebut apa adanya:**

| Butir | Keadaan |
| --- | --- |
| Penerapan ke lingkungan | **Belum boleh.** Ditahan `evidence/13` sampai pemilik modul HR memutuskan hak `WorkSchedule.Update`/`Delete` |
| Pencentangan hak oleh admin | **Belum dilakukan.** 10 identitas baru lahir tanpa pemegang; itu memang perilaku `AccessMenuSeeder` yang benar |
| Delapan endpoint `WorkScheduleAssignment` | **Belum diperbaiki.** Berada di luar scope task; dibekukan pada baseline dan dilaporkan setiap kali verifier berjalan |
| Uji runtime `403` | **Belum dijalankan.** Menuntut aplikasi dan database, keduanya dilarang |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build memunculkan 189 warning dokumentasi XML yang sudah ada sebelumnya. Nol di antaranya berasal dari berkas yang disentuh task ini |
| Masalah yang diketahui | `WfpWorkScheduleAssignmentController` memiliki 8 endpoint tanpa penegakan otorisasi — termasuk `POST`, `PUT`, `PATCH`, dan `DELETE`. **Ditemukan oleh invarian baru task ini**, berada di modul `HUMAN_RESOURCE_SCHEDULING` di luar scope, dan sengaja tidak diperbaiki karena memperbaikinya mengubah siapa yang boleh memanggilnya — keputusan pemilik modul. Dibekukan pada `KnownUnenforcedBusinessEndpoints` dan dilaporkan setiap kali verifier berjalan |
| Risiko tersisa | **1.** Bila source ini diterapkan tanpa menutup `evidence/13`, dua pasangan Departemen × Posisi memperoleh hak `WorkSchedule.Update`/`Delete` tanpa persetujuan. **2.** Sampai admin mencentang, tidak ada seorang pun dapat mengubah atau menghapus kelima master data itu — termasuk petugas yang sah. Ini konsekuensi *fail closed* yang disengaja, tetapi perlu diketahui sebelum penerapan |
| Perubahan sampingan | `NONE`. Tidak ada pekerjaan pengguna yang dibuang. Pohon kerja bersih sebelum task dimulai |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |
| Langkah berikutnya | **1.** Pemilik modul HR menutup `evidence/13`. **2.** Buka task lanjutan untuk 8 endpoint `WorkScheduleAssignment`. **3.** Sesudah keduanya, pemilik sistem memutuskan apakah `BE-SEC-003B` dilanjutkan. **4.** Commit/push menunggu instruksi terpisah |

```text
 M Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftController.cs
 M Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftGroupController.cs
 M Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftPatternController.cs
 M Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/WorkCalendarController.cs
 M Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/WorkScheduleController.cs
 M Program.cs
 M Services/Security/PermissionRegistryDescriptor.cs
 M Services/Security/PermissionRegistryValidator.cs
 M tools/authorization-verifier/verify-authorization.sh
?? Constants/AuthorizationPolicies.cs
?? docs/module-blueprints/platform-authorization/evidence/12-authorization-orphan-audit.md
?? docs/module-blueprints/platform-authorization/evidence/13-workschedule-dormant-grant-deployment-blocker.md
?? docs/module-blueprints/platform-authorization/task/report/backend/BE-SEC-012.md
```

Tidak ada `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, atau `checkout` yang dijalankan.
