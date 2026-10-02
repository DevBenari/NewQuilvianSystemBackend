# Laporan Perubahan Backend — `BE-SEC-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-SEC-013` |
| Judul | Penegakan otorisasi `WorkScheduleAssignment` dan pengosongan baseline naked endpoint |
| Slice | Task terpisah di luar rantai — menutup utang yang dibekukan `BE-SEC-012` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) — bagian *Task terpisah di luar rantai* |
| Trace | [`BE-SEC-012.md`](BE-SEC-012.md) bagian 7; [`evidence/12`](../../../evidence/12-authorization-orphan-audit.md) bagian G |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API yang berubah; hanya atribut otorisasi yang ditambahkan |
| Dependency | `BE-SEC-012` — **selesai**, belum di-commit; task ini berdiri di atas perubahan itu |
| Klasifikasi | `LIGHT` — 1 controller, 8 endpoint, tanpa perubahan skema, tanpa perubahan perilaku bisnis |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/HumanResource/SchedulingManagement/Controllers/`, `Services/Security/`, `docs/module-blueprints/platform-authorization/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `4ff7b9987cda7d11df22080ff1b2f849bbc97319` + perubahan `BE-SEC-012` yang belum di-commit |
| Tanggal | 16 September 2026 |
| Status | **Selesai** untuk source dan invarian. Penerapan tetap ditahan [`evidence/13`](../../../evidence/13-workschedule-dormant-grant-deployment-blocker.md) milik `BE-SEC-012` |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module | `HUMAN_RESOURCE_SCHEDULING` |
| Submodule | `SchedulingManagement` |
| Pemilik/prefix pada registry | `Wfp` = *Workforce Profile*. `SchedulingManagement` tercatat sebagai submodule legacy pemegang entity `Wfp*` pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `TOUCHED LEGACY` |
| Status registry | Tidak berubah. **Tidak ada entity baru**, tidak ada rename, tidak ada prefix baru |
| QBE ID yang berlaku | Tidak ada. `QBE-MOD-002` **tidak** terpicu: task ini tidak membuat entity operasional, hanya menambahkan atribut otorisasi pada controller yang sudah ada |

---

## 1. Masalah yang diperbaiki

Layar penugasan jadwal kerja pegawai — daftar, detail, tambah, ubah, aktif/nonaktif, dan hapus —
**tidak memeriksa hak akses sama sekali**. Yang diperiksa hanya "apakah orang ini sudah login".

Kondisinya lebih parah daripada yang diperbaiki `BE-SEC-012`. Di sana empat dari sembilan endpoint
yang telanjang; di sini **kedelapan endpoint** telanjang, termasuk seluruh endpoint baca. Controller
ini membawa `[AccessController]` tetapi tidak satu pun `[AccessAction]` atau `[AccessPermission]`,
sehingga resource `WorkScheduleAssignment` **tidak pernah terdaftar** di registry. Akibatnya ganda:

1. Siapa pun yang punya login dapat membaca, membuat, mengubah, dan menghapus penugasan jadwal
   kerja pegawai mana pun.
2. Admin **tidak dapat membatasinya**, karena kemampuannya tidak pernah muncul di layar Akses Role
   untuk dicentang.

Contoh konkretnya: seorang petugas Gizi dapat membuka penugasan jadwal seluruh pegawai rumah sakit,
memindahkan dokter ke pola shift lain, atau menghapus penugasannya — dan tidak ada satu pun layar
pengaturan yang dapat mencegahnya.

Cacat ini ditemukan oleh invarian naked endpoint yang dibuat `BE-SEC-012`, yaitu invarian yang
memang dibuat untuk menemukan kelas kesalahan semacam ini.

---

## 2. Bukti database — dan apa yang sebenarnya tersedia

> **Tidak ada bukti database baru yang disertakan pada task ini.** Instruksi task meminta
> pemeriksaan bukti DEV untuk `WorkScheduleAssignment`, tetapi tidak ada ekspor yang dilampirkan,
> dan koneksi database dilarang. Bagian ini karena itu **tidak** menyajikan angka database baru.

Pertanyaannya — *apakah sudah ada baris registry tertutup atau `SysAccessPolicy` yang menunjuk
`WorkScheduleAssignment`* — tetap dapat dijawab **tanpa** menyentuh database, dari tiga bukti bebas
yang sudah ada:

| No | Bukti | Kesimpulan |
| ---: | --- | --- |
| 1 | Identitas `WorkScheduleAssignment` **tidak pernah** dideklarasikan di sepanjang riwayat repository. `git log -S 'WorkScheduleAssignment"' --all` kosong, dan satu-satunya versi historis controller ini (`52f35a29`) memuat nol `[AccessAction]`/`[AccessPermission]` | Seeder tidak pernah punya bahan untuk membuat baris `SysControllerAccess` maupun `SysActionAccess` bagi resource ini |
| 2 | Bukti DEV yang disediakan pemilik pada audit orphan mencatat `SysControllerAccess` aktif = **339**, dan hitungan resource dari source juga **339** sebelum task ini | `WorkScheduleAssignment` bukan salah satu dari 339 itu |
| 3 | Himpunan "identitas tertutup yang policy-nya masih hidup" bersifat **lengkap**: 17 policy efektif atas 5 identitas, divalidasi `452 + 17 = 469` pada [`evidence/07`](../../../evidence/07-global-registry-drift-audit.md) bagian B.1. `WorkScheduleAssignment` tidak ada di dalamnya | Tidak ada `SysAccessPolicy` yang menunjuk resource ini |

**Kesimpulan: nol baris registry tertutup, nol policy tertidur.** Sebuah `SysAccessPolicy` menunjuk
baris `SysActionAccess`; bila baris action-nya tidak pernah ada, tidak ada yang dapat menunjuknya.

**Konsekuensinya penting dan membedakannya dari `WorkSchedule`.** Perbaikan ini melahirkan resource
dan empat identitas yang benar-benar baru, **tanpa pemegang**. Tidak ada hak tertidur yang bisa
hidup kembali, sehingga **`BE-SEC-013` tidak menambah blocker penerapan baru**. Blocker
[`evidence/13`](../../../evidence/13-workschedule-dormant-grant-deployment-blocker.md) tetap berlaku,
tetapi ia milik `BE-SEC-012` dan cakupannya tidak melebar karena task ini.

---

## 3. Audit semantik endpoint dan pemetaan kanonik

Hipotesis awal task diverifikasi terhadap isi method, bukan terhadap nama endpoint.

| Endpoint | Yang benar-benar dilakukan | Pemetaan | Alasan |
| --- | --- | --- | --- |
| `GET filters/metadata` | Mengembalikan pilihan filter statis | `Read` | Tidak menyentuh data |
| `GET summary` | Menghitung total, aktif, primary, rotating, temporary | `Read` | Hanya `CountAsync` |
| `GET /` | Daftar berhalaman | `Read` | Hanya baca |
| `GET {id}` | Detail satu penugasan | `Read` | Hanya baca |
| `POST /` | Membuat baris penugasan baru | `Create` | `Add` + `SaveChanges` |
| `PUT {id}` | Mengganti seluruh atribut penugasan, **termasuk `IsActive`** | `Update` | Penyuntingan atribut |
| `PATCH {id}/status` | Mengubah **hanya** `IsActive` | `Update` | Lihat catatan di bawah |
| `DELETE {id}` | *Soft delete* — `IsDelete=true`, `IsActive=false` | `Delete` | Penghapusan |

**Kenapa `PATCH {id}/status` tidak diberi identitas sendiri.** `PUT` sudah menyetel `IsActive`
([baris 140](../../../../Areas/Corporate/HumanResource/SchedulingManagement/Controllers/WfpWorkScheduleAssignmentController.cs#L140)).
Jadi `PATCH {id}/status` adalah bagian sejati dari apa yang sudah dibuka `Update` — memberinya
identitas terpisah tidak menambah kendali apa pun, karena pemegang `Update` tetap dapat mencapai
hasil yang sama lewat `PUT`. Identitas yang tidak menambah kendali hanya menambah baris untuk
dicentang admin tanpa manfaat.

**Tidak ada action yang lebih halus dibuat.** Tidak ada kontrak bisnis yang menuntutnya, dan
mengarang kewenangan baru bukan wewenang task ini.

**Tidak ada endpoint yang memerlukan policy bernama khusus.** Kedelapan endpoint dipanggil pegawai
lewat layar aplikasi, bukan oleh perangkat. Tidak ada satu pun yang layak masuk
`AuthorizationPolicies.ApprovedAlternativeAuthorization`, dan tidak ada yang disembunyikan di
baseline.

---

## 4. Perubahan yang dikerjakan

### 4.1 Berkas yang diperiksa

| Berkas / dokumen | Alasan dibaca |
| --- | --- |
| `AGENTS.md`, `rules/backend/role-access-rules.md` | Governance dan kontrak penamaan atribut |
| `rules/backend/master-data-endpoint-standard.md`, `transaction-endpoint-standard.md` | Menentukan bentuk capability sebelum memetakan izin |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Pemilik dan prefix `Wfp` |
| `WfpWorkScheduleAssignmentController.cs` | Seluruh isi 8 method, bukan hanya atributnya |
| `Services/Security/PermissionRegistryDescriptor.cs` | Baseline naked endpoint |
| `evidence/07`, `evidence/12`, `evidence/13`, `BE-SEC-012.md` | Bukti registry, orphan, dan utang yang dibekukan |
| Riwayat Git controller ini | Membuktikan identitasnya tidak pernah ada |

### 4.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/SchedulingManagement/Controllers/WfpWorkScheduleAssignmentController.cs` | 8 endpoint diberi `[AccessAction]` + `[AccessPermission]` |
| `Services/Security/PermissionRegistryDescriptor.cs` | `KnownUnenforcedBusinessEndpoints` **dikosongkan**; dokumentasinya ditulis ulang |

Diff pada controller seluruhnya berupa penambahan atribut. **Nol baris logika bisnis berubah.**

### 4.3 Baseline naked endpoint dikosongkan

Delapan baris dihapus dari `KnownUnenforcedBusinessEndpoints`, dan daftarnya kini kosong.
Mekanismenya **tetap ada** — yang berubah hanya isinya.

Mengosongkannya dengan cara yang benar berarti menutup utangnya lebih dulu, baru menghapus
barisnya. Urutan sebaliknya — menghapus baris tanpa memperbaiki endpoint — akan langsung
menggagalkan verifier, karena endpoint-nya muncul kembali sebagai naked yang tidak dikenal. Itu
memang perilaku yang diinginkan.

Dokumentasi barunya menyatakan tegas bahwa menambah baris ke daftar ini berarti sengaja membiarkan
sebuah endpoint tanpa penegakan otorisasi, sehingga menuntut keputusan pemilik modul dan alasan
tertulis — dan bahwa endpoint perangkat seharusnya memakai policy bernama, bukan baseline.

### 4.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` untuk bentuk request/response — route, payload, dan kode status sukses tidak berubah. Yang bertambah: kedelapan endpoint kini dapat menjawab `403` |
| Database | `NOT APPLICABLE` — tidak ada perubahan skema, tidak ada EF migration, tidak ada eksekusi database. **Migration status: NONE** |
| Keamanan/Auth | 8 endpoint berpindah dari "cukup login" menjadi "harus dicentang admin". Satu resource dan empat identitas kanonik baru lahir **tanpa pemegang**. Tidak ada hak tertidur yang hidup kembali — lihat bagian 2 |

---

## 5. Dokumentasi endpoint

#### Corporate / Human Resource / Scheduling Management / Work Schedule Assignment

Seluruh route diawali `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/work-schedule-assignments`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan filter untuk layar daftar | **`WorkScheduleAssignment : Read`** |
| `GET` | `/summary` | Ringkasan jumlah penugasan | **`WorkScheduleAssignment : Read`** |
| `GET` | `/` | Daftar penugasan berhalaman | **`WorkScheduleAssignment : Read`** |
| `GET` | `/{id}` | Detail satu penugasan | **`WorkScheduleAssignment : Read`** |
| `POST` | `/` | Menambah penugasan jadwal kerja | **`WorkScheduleAssignment : Create`** |
| `PUT` | `/{id}` | Mengubah penugasan | **`WorkScheduleAssignment : Update`** |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan penugasan | **`WorkScheduleAssignment : Update`** |
| `DELETE` | `/{id}` | Menghapus penugasan (*soft delete*) | **`WorkScheduleAssignment : Delete`** |

Seluruh baris dicetak tebal karena **tidak satu pun** sebelumnya ditegakkan.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj -c Release` | Berhasil. `0 Error(s)`, `189 Warning(s)` | `PASS` | Warning seluruhnya CS1573/CS1734 dokumentasi XML yang sudah ada; nol pada berkas yang disentuh |
| `tools/authorization-verifier/verify-authorization.sh --configuration Release` | `AUTHORIZATION VERIFIER: PASS` | `PASS` | Bagian 6.1 |
| Invarian `[1]` metadata gap | `0` | `PASS` | Keluaran verifier |
| Invarian `[2]` himpunan fallback | `69`, cocok persis | `PASS` | Tidak berubah oleh task ini |
| Invarian `[3]` identitas kanonik ganda | Tidak ganda; satu resource satu modul | `PASS` | Keluaran verifier — inilah pemeriksaan duplikat yang kanonik |
| Invarian `[4]` identitas wajib `BE-SEC-003` | `24 / 24` | `PASS` | Keluaran verifier |
| Invarian `[5]` naked endpoint | **`0` baru, `0` utang baseline** | `PASS` | Keluaran verifier |
| Pemindaian source bebas, seluruh repository | `0` endpoint telanjang | `PASS` | Bagian 6.3 |
| Perbandingan registry kanonik | Resource `+1`, Action `+4`, Module `0` | `PASS` | Bagian 6.2 |
| Kontrak penamaan `role-access-rules` §3 | Cocok huruf demi huruf | `PASS` | Dibuktikan invarian `[1]` = 0 |

**Uji manual:** `NOT FEASIBLE` — membuktikan `403` menuntut aplikasi dinyalakan dan database
dihubungi; keduanya dilarang task ini.

**`AUTOMATED TEST: NOT APPLICABLE`** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan, dan sengaja:** `AccessMenuSeeder`, koneksi/tulis database, `dotnet ef database
update`, menyalakan aplikasi, `BE-SEC-003B` Tahap 1 dan Tahap 2, fetch/merge Integration, serta
commit/push.

### 6.1 Keluaran authorization verifier

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

### 6.2 Perbandingan registry kanonik

| Metrik | Awal (DEV, sebelum `BE-SEC-012`) | Sesudah `BE-SEC-012` | Sesudah `BE-SEC-013` | Selisih task ini |
| --- | ---: | ---: | ---: | ---: |
| Actions / `SysActionAccess` | 1.286 | 1.296 | **1.300** | **+4** |
| Resources / `SysControllerAccess` | 339 | 339 | **340** | **+1** |
| Modules / `SysApplicationModule` | 48 | 48 | **48** | 0 |
| Fallback kompatibilitas | 69 | 69 | 69 | 0 |
| Metadata gap | 0 | 0 | 0 | 0 |
| Naked baru | — | 0 | **0** | 0 |
| Utang baseline naked | — | 8 | **0** | **−8** |

Selisihnya tepat seperti yang diharapkan: satu resource baru (`WorkScheduleAssignment`) dan empat
identitas (`Read`, `Create`, `Update`, `Delete`). Modul tidak bertambah karena
`HUMAN_RESOURCE_SCHEDULING` memang sudah terdaftar — sebelumnya sebagai modul tanpa resource
sama sekali.

### 6.3 Pemeriksaan bebas di luar verifier

Pemindaian source seluruh repository dengan kriteria "endpoint tanpa `[AccessPermission]`, tanpa
`[AccessAction]`, tanpa `[AllowAnonymous]`, tanpa `[Authorize(Policy = ...)]`" menghasilkan
**0 baris**. Pemeriksaan ini sengaja tidak memakai logika verifier, supaya hasilnya tidak bergantung
pada kebenaran kode yang sedang diuji.

Rincian identitas `WorkScheduleAssignment` dari source: `Read` dipakai 4 endpoint, `Create` 1,
`Update` 2, `Delete` 1 — **4 identitas unik**, nol duplikat.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Bukti DB diperiksa untuk registry/policy tertidur | Terpenuhi, dengan catatan | Bagian 2 — tidak ada ekspor baru disertakan; dijawab dari tiga bukti bebas |
| 2. Semantik endpoint diaudit sebelum dipetakan | Terpenuhi | Bagian 3 — dari isi method, bukan nama endpoint |
| 3. Pemetaan kanonik terkecil diterapkan | Terpenuhi | Bagian 5 |
| 4. Tidak ada action lebih halus yang dikarang | Terpenuhi | Bagian 3 |
| 5. `[AccessAction]` + `[AccessPermission]` konsisten pada 8 endpoint | Terpenuhi | Bagian 6.3 |
| 6. Baseline naked dikosongkan | Terpenuhi | Bagian 4.3 |
| 7. `unexpected naked endpoints = 0` | Terpenuhi | Invarian `[5]` |
| 8. `known naked baseline = 0` | Terpenuhi | Invarian `[5]` |
| 9. Endpoint ber-policy khusus didokumentasikan, bukan disembunyikan | Terpenuhi — **nihil**; tidak ada endpoint yang memerlukannya | Bagian 3 |
| 10. Build `Release` lulus | Terpenuhi | `0 Error(s)` |
| 11. `MetadataGaps = 0`; 24/24; fallback tidak berubah | Terpenuhi | Bagian 6.1 |
| 12. Perbandingan registry kanonik dibuat | Terpenuhi | Bagian 6.2 |
| 13. Pemeriksaan identitas ganda | Terpenuhi | Invarian `[3]` + bagian 6.3 |
| 14. Invarian A0 terjaga | Terpenuhi | Identitas tetap diturunkan `BuildCore` dari `[AccessPermission]`; tidak ada otoritas penemuan kedua |
| 15. Tidak ada database write, seeder, atau aplikasi dinyalakan | Terpenuhi | Bagian 6 |

**Butir Definition of Done yang belum terpenuhi — disebut apa adanya:**

| Butir | Keadaan |
| --- | --- |
| Pencentangan hak oleh admin | **Belum.** Keempat identitas lahir tanpa pemegang. Sampai admin mencentang, **tidak seorang pun** dapat membuka layar penugasan jadwal kerja — termasuk yang sah. Lihat risiko tersisa |
| `GET /options` | **Tidak ada** pada controller ini, padahal termasuk sembilan endpoint baseline master data. Dilaporkan, tidak didiamkan; membuatnya berada di luar scope remediasi otorisasi |
| Bentuk endpoint | Controller ini sub-resource ter-scope induk tetapi memakai bentuk master data (`PATCH /{id}/status`, `DELETE /{id}`). Dicatat sebagai delta kontrak; mengubahnya adalah perubahan kontrak API, bukan remediasi otorisasi |
| Penegakan data-scope | **Belum ada.** Pemegang izin dapat membaca dan mengubah penugasan milik profil pegawai mana pun; tidak ada pembatasan "hanya milik sendiri" atau "hanya bawahan". Konsisten dengan keputusan yang sudah tercatat bahwa data-scope adalah lapisan terpisah yang belum ditegakkan |
| Uji runtime `403` | **Belum dijalankan.** Menuntut aplikasi dan database |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build memunculkan 189 warning dokumentasi XML yang sudah ada sebelumnya; nol berasal dari berkas yang disentuh task ini |
| Masalah yang diketahui | `GET /options` tidak ada; bentuk endpoint sub-resource memakai pola master data; data-scope belum ditegakkan. Ketiganya dicatat pada bagian 7, tidak diperbaiki karena berada di luar remediasi otorisasi |
| Risiko tersisa | **1.** Begitu perbaikan ini diterapkan, layar penugasan jadwal kerja **kosong bagi semua orang** sampai admin mencentang hak pada Akses Role. Berbeda dari `BE-SEC-012` yang endpoint bacanya sudah ditegakkan, di sini seluruh endpoint baca ikut tertutup. Ini konsekuensi *fail closed* yang disengaja dan harus diketahui sebelum penerapan. **2.** Baseline yang kini kosong membuat setiap endpoint naked baru langsung menggagalkan verifier — memang tujuannya, tetapi berarti penambahan controller baru yang lalai akan menghentikan gerbang, bukan lolos diam-diam |
| Perubahan sampingan | `NONE`. Perubahan `BE-SEC-012` yang belum di-commit dibiarkan utuh dan tidak disentuh selain pengosongan baseline yang memang diperintahkan task ini |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |
| Langkah berikutnya | **1.** Pemilik modul HR menutup [`evidence/13`](../../../evidence/13-workschedule-dormant-grant-deployment-blocker.md) — satu-satunya blocker penerapan yang tersisa. **2.** Siapkan pencentangan hak awal bagi `WorkScheduleAssignment` dan kesepuluh identitas `BE-SEC-012` supaya layarnya tidak kosong saat rilis. **3.** Pemilik sistem memutuskan kelanjutan `BE-SEC-003B`. **4.** Commit/push menunggu instruksi terpisah |

```text
 M Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftController.cs
 M Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftGroupController.cs
 M Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftPatternController.cs
 M Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/WorkCalendarController.cs
 M Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/WorkScheduleController.cs
 M Areas/Corporate/HumanResource/SchedulingManagement/Controllers/WfpWorkScheduleAssignmentController.cs
 M Program.cs
 M Services/Security/PermissionRegistryDescriptor.cs
 M Services/Security/PermissionRegistryValidator.cs
 M docs/module-blueprints/platform-authorization/roadmap/backend-roadmap.md
 M docs/module-blueprints/platform-authorization/roadmap/requirement-traceability.md
 M tools/authorization-verifier/verify-authorization.sh
?? Constants/AuthorizationPolicies.cs
?? docs/module-blueprints/platform-authorization/evidence/12-authorization-orphan-audit.md
?? docs/module-blueprints/platform-authorization/evidence/13-workschedule-dormant-grant-deployment-blocker.md
?? docs/module-blueprints/platform-authorization/task/report/backend/BE-SEC-012.md
?? docs/module-blueprints/platform-authorization/task/report/backend/BE-SEC-013.md
```

Tidak ada `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, atau `checkout` yang dijalankan.
