# Laporan Perubahan Backend — `BE-SEC-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-SEC-014` |
| Judul | Matriks hak akses yang disetujui pemilik dan persiapan penerapan yang aman |
| Slice | Task terpisah di luar rantai — gerbang penerapan `BE-SEC-012` dan `BE-SEC-013` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) — bagian *Task terpisah di luar rantai* |
| Trace | [`evidence/13`](../../../evidence/13-workschedule-dormant-grant-deployment-blocker.md), [`evidence/14`](../../../evidence/14-owner-policy-matrix-and-deployment-preparation.md) |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API yang disentuh |
| Dependency | `BE-SEC-012` dan `BE-SEC-013` — keduanya selesai, belum di-commit |
| Klasifikasi | `MEDIUM` — nol perubahan source aplikasi; tiga skrip database dan dua dokumen keputusan |
| Task mode | `BACKEND` |
| Target tulis | `Migrations/scripts/`, `docs/module-blueprints/platform-authorization/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `4ff7b9987cda7d11df22080ff1b2f849bbc97319` + perubahan `BE-SEC-012` dan `BE-SEC-013` yang belum di-commit |
| Tanggal | 16 September 2026 |
| Status | **Selesai.** Seluruh artefak siap; tidak satu pun dijalankan |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` (HR) dan Shared Platform (otorisasi) |
| Module | `HUMAN_RESOURCE_MASTER_DATA`, `HUMAN_RESOURCE_SCHEDULING` |
| Submodule | `AttendanceAndSchedule`, `SchedulingManagement` |
| Pemilik/prefix pada registry | `Hrd`, `Wfp` — tidak ada perubahan registry |
| Keberlakuan | `TOUCHED LEGACY` — **nol berkas source aplikasi berubah pada task ini** |
| Status registry | Tidak berubah |
| QBE ID yang berlaku | Tidak ada. Tidak ada entity, rename, migration, maupun eksekusi database |
| Wewenang database | **TIDAK DIBERIKAN dan TIDAK DIPAKAI.** Ketiga skrip disiapkan, tidak dijalankan |

---

## 1. Masalah yang diperbaiki

`BE-SEC-012` dan `BE-SEC-013` menutup lubang otorisasi di source, tetapi keduanya **belum boleh
diterapkan**. Dua alasan:

1. Menerapkannya tanpa persiapan akan **menghidupkan kembali hak yang tidak pernah disetujui** —
   dua policy tertidur milik Finance × Manajer Finance atas `WorkSchedule.Update`/`Delete`.
2. Sesudah diterapkan, **tidak seorang pun** dapat memakai layar master data kepegawaian sampai
   admin mencentang hak, karena 14 identitas baru lahir tanpa pemegang.

Task ini menyiapkan keduanya: aturan siapa yang berhak, dan urutan yang membuat penerapannya tidak
merusak apa pun.

---

## 2. Proses bisnis

**Keputusan pemilik.** Untuk master data, otorisasi mengikuti kepemilikan departemen. Manajer
departemen pemilik memperoleh Read/Create/Update/Delete; staff memperoleh Read/Create saja. Jadwal
kerja dan turunannya milik Human Resource, sehingga Finance tidak mempertahankan hak ubah/hapus
hanya karena policy warisan kebetulan ada.

**Alur penerapannya, berurutan:** backup dikonfirmasi → traffic ditutup → hak tertidur Finance
dicabut → source beku dijalankan sekali → registry diverifikasi 1.300/340/48 → hak HR diberikan →
otorisasi diverifikasi → dry-run `BE-SEC-003B` diulang → baru Tahap 1 dan Tahap 2.

**Jalur tidak normal yang sudah diantisipasi:**

| Keadaan | Yang terjadi |
| --- | --- |
| Jumlah policy Finance bukan tepat 2 | Skrip pencabutan membatalkan transaksi; tidak ada yang berubah |
| UUID operator kosong atau tidak ada di `AspNetUsers` | Kedua skrip tulis membatalkan diri |
| Nama departemen/posisi tidak menunjuk tepat satu baris | Transaksi dibatalkan |
| Registry belum direkonsiliasi | Skrip pemberian hak membatalkan diri sebelum menyisipkan apa pun |
| Posisi staff belum disetujui pemilik | Hanya Manajer HR yang dilayani — kurang memberi, bukan mengarang |
| Finance ternyata masih memegang Update/Delete | Penegasan akhir membatalkan seluruh pemberian hak |

**Hasil akhir.** Layar master data kepegawaian hanya dapat dipakai Departemen × Posisi yang
dicentang admin, dan Finance tidak memperoleh apa pun yang tidak disetujui.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan dibaca |
| --- | --- |
| `Models/SysAccessPolicy.cs`, `SysActionAccess.cs`, `SysControllerAccess.cs`, `SysApplicationModule.cs`, `IdentityModel.cs` | Nama tabel dan kolom sebenarnya untuk skrip SQL |
| `Models/MstDepartment.cs`, `MstPosition.cs`, `ApplicationUserOrganization.cs` | Kunci bisnis dan nama tabel `AspNetUserOrganization` |
| `Services/Security/AccessPermissionService.cs` | Predikat "policy efektif" yang sebenarnya |
| `Seeders/AccessMenuSeeder.cs` | Perilaku reaktivasi baris tertutup |
| `Migrations/scripts/be-sec-003b-policy-expansion.sql` | Kebiasaan penulisan skrip repository |
| `Migrations/scripts/diagnose-be-sec-003-permission-state-dbeaver.sql` | Kebiasaan varian DBeaver — tanpa meta-command psql |
| `evidence/07`, `evidence/12`, `evidence/13`, laporan `BE-SEC-012`, `BE-SEC-013` | Bukti registry dan hak tertidur |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/scripts/be-sec-014-current-holders-readonly-dbeaver.sql` | **Baru.** Pembacaan pemegang hak, posisi HR, dan pengukuran registry. Baca-saja |
| `Migrations/scripts/be-sec-014-pre-seeder-revoke-finance-workschedule.sql` | **Baru.** Pencabutan hak tertidur Finance. Default `ROLLBACK` |
| `Migrations/scripts/be-sec-014-post-seeder-hr-initial-grants.sql` | **Baru.** Pemberian hak awal HR, idempoten. Default `ROLLBACK` |
| `docs/.../evidence/14-owner-policy-matrix-and-deployment-preparation.md` | **Baru.** Matriks pemilik, klasifikasi identitas, urutan penerapan, baseline baru |
| `docs/.../roadmap/backend-roadmap.md` | Kartu `BE-SEC-014`, baseline baru, gerbang terbuka diperbarui |
| `docs/.../roadmap/requirement-traceability.md` | Bukti matriks dan persiapan penerapan |

**Nol berkas source aplikasi berubah pada task ini.**

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | **Tidak ada eksekusi.** Tiga skrip disiapkan; dua di antaranya menulis tetapi berakhir `ROLLBACK`. Tidak ada EF migration. **Migration status: NONE** |
| Keamanan/Auth | Menetapkan siapa yang berhak sesudah penerapan, dan mencegah dua hak tertidur hidup tanpa persetujuan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat, mengubah, maupun menyentuh endpoint mana pun.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `git diff --check` | Bersih, nol galat spasi | `PASS` | Peringatan yang muncul hanyalah pemberitahuan `autocrlf`, bukan galat |
| `dotnet build -c Release --no-incremental` | Berhasil. `0 Error(s)`, `189 Warning(s)` | `PASS` | Kompilasi penuh, bukan inkremental. Seluruh warning CS1573/CS1734 yang sudah ada |
| `verify-authorization.sh --configuration Release` | `AUTHORIZATION VERIFIER: PASS` | `PASS` | Bagian 5.1 |
| Metadata gap | `0` | `PASS` | Invarian `[1]` |
| Fallback himpunan disetujui | `69` cocok persis | `PASS` | Invarian `[2]` |
| Identitas kanonik ganda | `0` | `PASS` | Invarian `[3]` |
| Identitas wajib `BE-SEC-003` | `24 / 24` | `PASS` | Invarian `[4]` |
| Naked endpoint | `0` baru, `0` baseline | `PASS` | Invarian `[5]` |
| Baseline source baru | `1.300 / 340 / 48` | `PASS` | Bagian 5.1 |
| Skema SQL diverifikasi terhadap model | Nama tabel dan kolom cocok | `PASS` | Bagian 5.2 |

**Uji manual:** `NOT FEASIBLE` — menjalankan skrip menuntut koneksi database, yang dilarang task ini.

**`AUTOMATED TEST: NOT APPLICABLE`** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan, dan sengaja:** ketiga skrip SQL, `AccessMenuSeeder`, koneksi/tulis database,
menyalakan aplikasi, `BE-SEC-003B` Tahap 1 dan Tahap 2, fetch/merge Integration, commit/push.

### 5.1 Keluaran authorization verifier

```text
Registry diagnostics (informational only):
  Modules       = 48
  Resources     = 340
  Actions       = 1300
  Fallback      = 69
  Metadata gaps = 0
  Naked (new)   = 0
  Naked (known) = 0

Required identities: 24 / 24 present.

AUTHORIZATION VERIFIER: PASS
  [1] metadata gap                 : 0
  [2] fallback himpunan disetujui  : 69 cocok persis
  [3] identitas kanonik            : tidak ganda, satu resource satu modul
  [4] identitas wajib BE-SEC-003   : 24 / 24
  [5] endpoint bisnis telanjang    : 0 baru, 0 utang baseline
```

### 5.2 Kebenaran skrip SQL — diperiksa terhadap model, bukan diasumsikan

| Hal | Nilai sebenarnya | Cara diperiksa |
| --- | --- | --- |
| Nama tabel | `public."SysAccessPolicy"`, `"SysActionAccess"`, `"SysControllerAccess"`, `"SysApplicationModule"`, `"MstDepartment"`, `"MstPosition"` | Atribut `[Table(..., Schema = "public")]` pada tiap model |
| Tabel organisasi pengguna | **`public."AspNetUserOrganization"`** — bukan nama kelasnya | `ApplicationUserOrganization.cs` memakai `[Table("AspNetUserOrganization")]`. Kekeliruan ini ditemukan dan diperbaiki sebelum berkas selesai |
| Predikat policy efektif | `IsAllowed AND IsActive AND NOT IsDelete AND policy.ControllerAccessId = action.ControllerAccessId AND action aktif AND NOT IsSystemOnly AND controller aktif AND NOT IsSystemOnly` | Disalin dari `AccessPermissionService.cs:294-303` |
| Kolom audit | `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteBy`, `CancelBy`, `IsDelete`, `IsCancel` | `IdentityModel.cs` |
| Kolom `NOT NULL` tanpa default database | **`UpdateBy`, `DeleteBy`, `CancelBy`** — ketiganya `uuid NOT NULL` dan, berbeda dari tabel lain di repository ini, `SysAccessPolicy` **tidak** memberi `defaultValue` pada ketiganya | `20260516060830_initializeSetup.cs` bagian `CreateTable "SysAccessPolicy"`; bandingkan `initializeNurseStationCluster.cs` yang justru memberi `defaultValue` |
| Kontrak `INSERT` policy baru | 14 kolom `NOT NULL` disebut seluruhnya. `CreateBy` = UUID operator; `UpdateBy` = `DeleteBy` = `CancelBy` = `Guid.Empty` (`'00000000-0000-0000-0000-000000000000'::uuid`). Tiga kolom timestamp nullable sengaja dibiarkan kosong | Konvensi repository pada `RoleAccessController.cs` bagian pembuatan `SysAccessPolicy`: hanya `CreateBy` yang memuat pelaku |
| Kebiasaan penonaktifan | `IsActive = false`, baris tidak dihapus | `be-sec-003b-policy-expansion.sql` bagian 4.3 |

**Satu jebakan yang sengaja dihindari.** Varian `-dbeaver` pada repository ini tidak memakai
meta-command psql, dan `\set` memang tidak dikenal DBeaver. Lebih penting lagi: psql **tidak**
menginterpolasi `:'variabel'` di dalam blok dollar-quoted (`$$ ... $$`), sehingga penegasan di dalam
`DO` tidak akan pernah membaca nilai yang dimaksud — gerbang keamanannya akan lolos secara semu.
Ketiga skrip karena itu memakai `set_config(..., true)` dan `current_setting()`, yang merupakan SQL
biasa dan berperilaku sama di DBeaver maupun psql.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `git diff --check` dijalankan, perubahan dua task dipisahkan | Terpenuhi | Bagian 7 laporan ini dan bagian A jawaban task |
| 2. Usulan pemisahan commit yang dapat diaudit | Terpenuhi | Bagian 7 |
| 3. Matriks lengkap enam resource × empat action | Terpenuhi | [`evidence/14`](../../../evidence/14-owner-policy-matrix-and-deployment-preparation.md) bagian B |
| 4. Pemisahan policy lama / tertidur / baru / dicabut / disisipkan | Terpenuhi | [`evidence/14`](../../../evidence/14-owner-policy-matrix-and-deployment-preparation.md) bagian C |
| 5. Pemegang saat ini tidak ditebak | Terpenuhi | Query baca-saja disediakan; tidak ada nama pemegang yang dikarang |
| 6. Query baca-saja untuk memastikan pemegang | Terpenuhi | `be-sec-014-current-holders-readonly-dbeaver.sql` |
| 7. Skrip pencabutan pra-seeder, tanpa GUID, kardinalitas = 2, transaksi, rollback, operator terverifikasi, default `ROLLBACK` | Terpenuhi | `be-sec-014-pre-seeder-revoke-finance-workschedule.sql` |
| 8. Human Resource × Manajer HR tidak disentuh | Terpenuhi | Sasaran dibatasi departemen Finance; bagian 1.3 dan 2.7 membuktikannya |
| 9. Skrip pemberian hak idempoten, kunci alami, tanpa duplikat, tanpa Finance, dry-run, rollback, default `ROLLBACK` | Terpenuhi | `be-sec-014-post-seeder-hr-initial-grants.sql` |
| 10. `WorkScheduleAssignment` diperlakukan sebagai resource baru | Terpenuhi | Bagian 3.5 skrip; `evidence/14` bagian C.3 |
| 11. Query enumerasi posisi HR sebelum menyebut "staff" | Terpenuhi | Bagian 2.1 skrip pembacaan |
| 12. Baseline lama dinyatakan tidak berlaku; baseline baru ditetapkan | Terpenuhi | [`evidence/14`](../../../evidence/14-owner-policy-matrix-and-deployment-preparation.md) bagian F |
| 13. Build `Release` dan verifier dijalankan ulang | Terpenuhi | Bagian 5.1 |
| 14. Urutan resume `BE-SEC-003B` dijelaskan | Terpenuhi | [`evidence/14`](../../../evidence/14-owner-policy-matrix-and-deployment-preparation.md) bagian E |
| 15. Orphan di luar scope tetap terpisah | Terpenuhi | `evidence/14` bagian G; tidak ada skrip yang menyentuhnya |
| 16. Tidak ada eksekusi database, seeder, atau aplikasi | Terpenuhi | Bagian 5 |

**Butir Definition of Done yang belum terpenuhi — disebut apa adanya:**

| Butir | Keadaan |
| --- | --- |
| Daftar posisi staff HR | **Belum ada.** Tidak tersedia sebagai bukti dan sengaja tidak ditebak. Menunggu pemilik sistem memilih dari keluaran bagian 2.1 |
| Pemegang `Read`/`Create` saat ini | **Belum terukur.** Menunggu operator menjalankan skrip pembacaan |
| Skrip diuji terhadap database | **Belum.** Ketiganya rancangan yang belum pernah dijalankan; sifat itu ditulis di kepala tiap berkas |
| Penerapan | **Belum.** Seluruh sepuluh langkah urutan penerapan belum dijalankan |

---

## 7. Usulan pemisahan commit

Perubahan dua task berada di pohon kerja yang sama. Pemisahan per berkas **tidak cukup**, dan
alasannya perlu dilihat sebelum commit pertama dibuat.

`Services/Security/PermissionRegistryDescriptor.cs` memuat perubahan **kedua** task:
`BE-SEC-012` membuat baseline berisi delapan entri, `BE-SEC-013` mengosongkannya. Pohon kerja hari
ini hanya menyimpan keadaan akhir — baseline kosong.

> **Akibatnya: bila commit pertama dibuat apa adanya, verifier pada commit itu GAGAL.** Pada
> `BE-SEC-012`, kedelapan endpoint `WfpWorkScheduleAssignmentController` masih telanjang. Dengan
> baseline kosong, invarian `[5]` melaporkan 8 naked endpoint yang tidak dikenal dan menolak.

Supaya kedua commit hijau secara mandiri, baseline harus berisi delapan entri pada commit 1, lalu
dikosongkan pada commit 2.

**Commit 1 — `BE-SEC-012`**

```
Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/WorkScheduleController.cs
Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftController.cs
Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftGroupController.cs
Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftPatternController.cs
Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/WorkCalendarController.cs
Constants/AuthorizationPolicies.cs
Program.cs
Services/Security/PermissionRegistryDescriptor.cs   <- dengan baseline 8 entri
Services/Security/PermissionRegistryValidator.cs
tools/authorization-verifier/verify-authorization.sh
docs/module-blueprints/platform-authorization/evidence/12-authorization-orphan-audit.md
docs/module-blueprints/platform-authorization/evidence/13-workschedule-dormant-grant-deployment-blocker.md
docs/module-blueprints/platform-authorization/task/report/backend/BE-SEC-012.md
```

**Commit 2 — `BE-SEC-013`**

```
Areas/Corporate/HumanResource/SchedulingManagement/Controllers/WfpWorkScheduleAssignmentController.cs
Services/Security/PermissionRegistryDescriptor.cs   <- baseline dikosongkan
docs/module-blueprints/platform-authorization/task/report/backend/BE-SEC-013.md
```

**Commit 3 — `BE-SEC-014`**

```
Migrations/scripts/be-sec-014-current-holders-readonly-dbeaver.sql
Migrations/scripts/be-sec-014-pre-seeder-revoke-finance-workschedule.sql
Migrations/scripts/be-sec-014-post-seeder-hr-initial-grants.sql
docs/module-blueprints/platform-authorization/evidence/14-owner-policy-matrix-and-deployment-preparation.md
docs/module-blueprints/platform-authorization/task/report/backend/BE-SEC-014.md
docs/module-blueprints/platform-authorization/roadmap/backend-roadmap.md
docs/module-blueprints/platform-authorization/roadmap/requirement-traceability.md
```

Kedua berkas roadmap memuat tanda status ketiga task sekaligus. Menaruhnya di commit terakhir
membuat register itu konsisten sekali saja, alih-alih tiga kali setengah benar.

**Tidak ada commit yang dibuat pada task ini.** Urutan di atas adalah usulan; eksekusinya menunggu
instruksi terpisah.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build memunculkan 189 warning dokumentasi XML yang sudah ada sebelumnya |
| Masalah yang diketahui | Ketiga skrip belum pernah dijalankan terhadap database mana pun. Statusnya ditulis di kepala tiap berkas supaya tidak dikira sudah tervalidasi |
| Risiko tersisa | **1.** Bila langkah 3 dilewati, dua hak Finance hidup kembali tanpa persetujuan. **2.** Bila langkah 7 dilewati, layar master data kepegawaian kosong bagi semua orang. **3.** Nama departemen `'Finance'`, `'Human Resource'`, dan `'Manajer HR'` diambil dari bukti pemilik dan belum diverifikasi terhadap `MstDepartment`/`MstPosition`; skrip membatalkan diri bila tidak cocok, tetapi operator sebaiknya menjalankan bagian 2.2 dan 2.3 skrip pembacaan lebih dulu |
| Perubahan sampingan | `NONE`. Perubahan `BE-SEC-012` dan `BE-SEC-013` yang belum di-commit tidak disentuh |
| Interupsi | `NONE` |
| Status Git | Lihat bagian I jawaban task |
| Langkah berikutnya | **1.** Operator menjalankan skrip pembacaan dan menyerahkan hasilnya kepada pemilik. **2.** Pemilik menyetujui daftar posisi staff HR. **3.** Jalankan urutan sepuluh langkah `evidence/14` bagian E. **4.** Commit/push menunggu instruksi terpisah |
