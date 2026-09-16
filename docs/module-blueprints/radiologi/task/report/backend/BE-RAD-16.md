# Laporan Perubahan Backend — `BE-RAD-16`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-16` |
| Judul | Kelahiran bacaan otomatis dan penyajian hasil ke rekam medis |
| Slice | `S14` — penyajian hasil bacaan ke Clinical Management; ditambah penutupan celah `S12` |
| Roadmap | Di luar `roadmap/backend-roadmap.md` — task lahir dari instruksi pemilik modul 2026-09-14, bukan dari baris roadmap |
| Trace | `RAD-STATE-001` bagian 3 baris pertama; `RAD-INT-001` bagian 2; `RAD-DEC-006`; `FR-RAD-030`, `FR-RAD-032`; `RAD-API-001` revision 11 |
| Contract version | `RAD-API-001` revision 11, status `approved` — amandemennya ditulis task ini, disetujui Yoga Aji Pratama 2026-09-14 |
| Dependency | `BE-RAD-09` **selesai**, `BE-RAD-10` **selesai**, `BE-RAD-11` **selesai**, `BE-RAD-15` **selesai sebagian** |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, **kontrak API 2**, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `466a7127`, branch `yoga` |
| Tanggal | 2026-09-14 |
| Status | **Selesai untuk source.** Build 0 error. **Verifikasi otomatis tidak dapat dijalankan** — proyek uji sudah dikeluarkan dari repository; lihat bagian 5 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `RadiologyManagement` / — |
| Pemilik & prefix pada registry | `Radiology`, prefix `Rad` |
| Status registry | **`ACTIVE`** — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 22 |
| Keberlakuan | `TOUCHED LEGACY` untuk `RadStudyService` dan `RadReportService`; `NEW CODE` untuk `GetByPatientAsync` dan endpoint `by-patient` |
| QBE ID yang berlaku | `QBE-MOD-002` **terpenuhi** (entry `ACTIVE`, bukan `PLANNED`); `QBE-NAM-003` tidak berlaku — tidak ada rename |
| `.codex/` | **Tidak ada, dan itu memang benar.** `AGENTS.md` baris 45 menyatakan folder itu sudah dicabut; aturan operasional pindah ke `rules/backend/` pada suite Skill, kontrak rekayasa dan registry ke `docs/engineering/`. Keduanya terbaca, sehingga **bukan** kondisi `BLOCKED — canonical governance unavailable` |
| Wewenang database | **Tidak dipakai.** Tidak ada perubahan schema, entity, maupun migration |

---

## 1. Masalah yang diperbaiki

### 1.1 Koreksi atas anggapan awal — dua dari tiga "penghalang" ternyata sudah selesai

Pemilik modul meminta tiga penghalang backend dikerjakan. Diperiksa langsung ke source, **dua di
antaranya sudah selesai sejak 2026-09-11** dan yang tersisa bukan pekerjaan kode:

| Yang disebut penghalang | Keadaan sebenarnya |
| --- | --- |
| `RadReport : ActAsRadiologist` tidak dapat diberikan | **Sudah dapat diberikan.** `AccessMenuSeeder` mendaftarkannya lewat `PenandaTanpaEndpointYangDidaftarkan`, dipanggil `EnsurePenandaTanpaEndpoint` baris 137. Ditutup `approval-requests/2026-09-11-keputusan-empat-penghalang.md` bagian 1. **Sisanya pekerjaan Administrator:** mencentangnya pada peran yang memang dokter radiolog |
| Tidak ada aturan keselamatan `Active` | **Bukan cacat, melainkan rancangan.** `BE-RAD-15` sudah mengisi enam alat, empat butir, dan dua belas usulan aturan — seluruhnya `Draft`. Seeder **sengaja** tidak menerbitkan `Active`, karena seeder yang menerbitkannya berarti sebuah program menetapkan kapan seorang pasien aman disinari. **Sisanya milik Komite Medis** lewat pemegang akun yang belum ditunjuk — `RAD-OPEN-011` |

Laporan `FE-RAD-11`, `FE-RAD-12`, dan `FE-RAD-13` menuliskan keduanya sebagai penghalang yang
masih terbuka. **Itu keliru**, dan sumbernya dokumen blueprint yang belum disegarkan, bukan
source. Koreksinya dicatat di bagian 7.

### 1.2 Penghalang yang benar-benar ada

`EnsurePendingReportAsync` pada `RadReportService` **tidak pernah dipanggil siapa pun**.
Diperiksa ke seluruh berkas `.cs`: satu-satunya kemunculan adalah definisinya sendiri.

`RAD-STATE-001` bagian 3 baris pertama sudah menetapkan perilakunya:

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | (otomatis) | `Pending` | Sistem | Study berpindah ke `QualityAccepted` |

Yang terjadi sebenarnya: tidak ada. Akibatnya berantai — tidak satu pun baris `RadReport`
berstatus `Pending` pernah lahir, `GET /rad-reports?reportStatus=Pending` selalu kosong, rekap
`MenungguDraf` dan `BelumDirilis` selalu nol, dan daftar pemeriksaan yang menunggu dibaca tidak
punya sumber data sama sekali.

**Akibat nyatanya bagi radiolog:** ia membuka daftar bacaan yang menunggu, melihatnya kosong, dan
menyimpulkan tidak ada pekerjaan — padahal ada pemeriksaan yang citranya sudah dinyatakan layak
dan menunggu dibaca. Tidak ada satu layar pun yang dapat menunjukkannya kepadanya.

### 1.3 Dua keputusan pemilik modul

**`GET /by-encounter` tidak membawa isi bacaan sama sekali.** Dokter yang membuka rekam medis
melihat daftar hasil, bukan hasilnya — kesimpulan bacaan justru bagian yang ia datangi.

**Rekam medis tidak dapat menampilkan hasil radiologi.** Layarnya berorientasi dokumen dan tidak
punya `encounterId`, sedangkan endpoint yang tersedia beralamat pada kunjungan.

---

## 2. Proses bisnis

**Tujuan.** Bacaan radiologi lahir sendiri begitu citranya dinyatakan layak, dan hasil yang sudah
dirilis terbaca dokter pengirim tanpa salinan.

**Pelaku dan pemicu.** Radiografer atau radiolog menilai mutu citra layak lewat
`POST /rad-studies/{id}/decide-quality`. Sistem — bukan orang — yang melahirkan wadah bacaannya.

**Langkah yang berurutan pada penilaian mutu layak:**

1. Status study berpindah ke `QualityAccepted`, tersimpan lewat `SaveWithConcurrencyGuardAsync`.
2. Fakta kelayakan tagih diserahkan ke Billing lewat `EmitChargeEligibilityAsync`, **di luar
   transaksi** — `ClinicalMilestoneFactProducer` menolak dipanggil di dalamnya.
3. **Baru** wadah bacaan disiapkan lewat `EnsurePendingReportAsync`, juga di luar transaksi.
4. Bacaan lahir berstatus `Pending`, dan pemeriksaan itu muncul pada daftar bacaan yang menunggu.

**Aturan yang berlaku.** Wadah bacaan hanya lahir untuk citra yang **layak**. Citra yang
dinyatakan tidak layak tidak menerbitkan apa pun — tidak fakta tagih, tidak wadah bacaan — dan
jalur itu tidak disentuh task ini.

**Jalur tidak normal.** Bila penyiapan wadah bacaan gagal, penilaian mutu **tetap berhasil**.
Alasannya di bagian 3.3.

**Hasil akhir.** Study `QualityAccepted`, fakta tagih terkirim, dan satu baris `RadReport`
berstatus `Pending` menunggu drafnya ditulis.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `Areas/.../Services/RadStudyService.cs` | Tempat pemasangan kelahiran bacaan; urutan transaksi |
| `Areas/.../Services/RadReportService.cs` | `EnsurePendingReportAsync`, `MapList`, `GetByEncounterAsync`, `LoadCurrentVersionsAsync` |
| `Areas/.../Controllers/RadReportController.cs` | Bentuk endpoint baca yang sudah ada |
| `Areas/.../DTOs/RadReportDtos.cs` | `RadReportListResponse` beserta keterangan privasinya |
| `Areas/.../Services/RadOrderService.cs` | Pola pembacaan konteks pasien lewat `RegPatientEncounters` |
| `Seeders/AccessMenuSeeder.cs` | Memastikan penanda `ActAsRadiologist` benar-benar terdaftar |
| `Services/Logging/LoggerService.cs` | Tanda tangan `ErrorAsync` |
| `Program.cs` | Lifetime `RadStudyService` dan `RadReportService` |
| `docs/module-blueprints/radiologi/` — kontrak, keputusan, laporan `BE-RAD-08`/`15`, approval request | Menelusuri keadaan penghalang yang sebenarnya |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Services/RadStudyService.cs` | Menginjeksi `RadReportService`; `LahirkanBacaanAsync` baru; dipanggil di akhir `DecideQualityAsync` pada cabang citra layak |
| `Areas/.../Services/RadReportService.cs` | `MapList` menerima `sertakanKesimpulan`; `GetByEncounterAsync` menyalakannya; `GetByPatientAsync` baru |
| `Areas/.../Controllers/RadReportController.cs` | Endpoint `GET /by-patient/{patientId}` |
| `Areas/.../DTOs/RadReportDtos.cs` | `RadReportListResponse.Impression`; keterangan privasi diperbarui agar tidak berbohong |
| `docs/module-blueprints/radiologi/contracts/api-contract.md` | Amandemen revision 11 |

**Tidak disentuh, dan bukan pekerjaan task ini:**
`docs/module-blueprints/laboratorium/00-interview-decisions.md` dan `01-existing-capability-map.md`
sudah berubah di working tree sebelum task ini dimulai. Dibiarkan apa adanya.

### 3.3 Dampak kontrak API, database, dan keamanan

**Kontrak API.** `RAD-API-001` revision 11 — satu endpoint baru dan satu field baru. Keduanya
aman bagi pemanggil lama: penambahan field tidak merusak, dan endpoint baru tidak mengubah yang
sudah ada.

**Database.** **Tidak ada perubahan schema, entity, maupun migration.** `Impression` sudah ada
pada `RadReportVersion`; `GetByPatientAsync` hanya membaca. Wewenang database tidak dipakai.

**Keamanan.** `GET /by-patient` memakai `RadReport : Read` yang sudah ada — **tidak ada string
hak akses baru**. Penyaring bacaan belum dirilis **disalin apa adanya** dari `GetByEncounterAsync`,
bukan dilonggarkan: dua penyaring yang saling menguatkan, status **dan** `FirstReleasedAt`.
Frontend `FE-RAD-13` sudah memasang lapis kedua yang menolak versi bukan-`Released`; keduanya
tetap saling menguatkan.

**Keputusan yang paling menentukan di task ini: `Impression` sengaja tidak ikut pada `GET /`.**
Menambahkannya ke DTO membuatnya tersedia di mana pun `MapList` dipakai, termasuk papan kerja
radiologi yang sering terbuka lebar di monitor bersama. `sertakanKesimpulan` dibuat **opt-in**
supaya menyalakannya selalu menjadi tindakan yang disengaja dan terbaca saat review.

**Kegagalan kelahiran bacaan tidak menggagalkan penilaian mutu.** Pada titik itu citra sudah
dinyatakan layak dan faktanya sudah terkirim ke Billing — keduanya tidak dapat ditarik kembali.
Membiarkan kegagalan menjalar hanya akan membuat petugas melihat galat atas pekerjaan yang
sebenarnya berhasil, lalu mencoba menilai mutu lagi dan ditolak karena statusnya sudah berpindah.

Kegagalannya juga bukan kehilangan permanen: `EnsurePendingReportAsync` idempoten, dan
`CreateDraftAsync` tetap melahirkan bacaannya sendiri bila wadahnya belum ada. Yang hilang
hanyalah kemunculan study itu pada daftar bacaan yang menunggu — dan itulah sebabnya kegagalannya
dicatat sebagai galat yang dapat dicari lewat `RadStudy.EnsureReportFailed`, bukan ditelan diam-diam.

---

## 4. Dokumentasi endpoint

### `GET /api/v1/health-services/radiology-management/rad-reports/by-patient/{patientId}`

| Field | Nilai |
| --- | --- |
| Kegunaan | Bacaan seorang pasien yang sudah dirilis, lintas kunjungan, terbaru lebih dulu |
| Hak akses | `RadReport : Read` |
| Request | `patientId` pada route; tidak ada badan permintaan |
| Response | `ApiResponse<List<RadReportListResponse>>`, `Impression` **terisi** |
| Kosong | `200` dengan daftar kosong dan pesan "Pasien ini belum memiliki hasil bacaan radiologi yang sudah dirilis." — **bukan** `404` |

Daftar kosong sengaja dijawab `200`, bukan `404`: pasien yang belum punya bacaan adalah keadaan
yang wajar, dan membedakannya dari pasien yang tidak ada justru yang penting bagi pembaca rekam
medis. Bentuknya sama dengan `by-encounter`.

---

## 5. Verifikasi

| Yang dijalankan | Hasil |
| --- | --- |
| `dotnet build -p:RunAnalyzers=False` | **0 error**, 188 warning — seluruhnya gaya XML yang sudah ada sebelumnya di modul lain. **Tidak ada warning baru dari modul Radiologi**; dua warning `CS1573` yang sempat muncul dari perubahan ini sudah ditutup |
| Pemeriksaan lingkaran dependency | **Aman.** `RadReportService` tidak menginjeksi `RadStudyService`; konstruktornya hanya `ApplicationDbContext`, `IHttpContextAccessor`, `AccessPermissionService`, `LoggerService`, `RadReportNumberService` |
| Pemeriksaan lifetime DI | **Aman.** Keduanya `AddScoped` — `Program.cs` baris 306 dan 310 |
| Pemeriksaan transaksi pada titik panggil | **Aman.** `SaveWithConcurrencyGuardAsync` hanya memanggil `SaveChangesAsync` tanpa transaksi, dan `EmitChargeEligibilityAsync` memang dijalankan di luar transaksi. Tidak ada transaksi aktif ketika `EnsurePendingReportAsync` membuka miliknya sendiri |
| Pemeriksaan penanda hak akses | `RadReport : ActAsRadiologist` terdaftar pada `PenandaTanpaEndpointYangDidaftarkan`, dipanggil baris 137, module code terdaftar lewat `[AccessController]` pada keenam controller radiologi |

### `AUTOMATED TEST: NOT FEASIBLE` — dan ini perlu diketahui

**Proyek uji sudah dikeluarkan dari repository.** `QuilvianSystemBackend.sln` hanya memuat satu
proyek, dan riwayat Git mencatat `chore: exclude test projects from repository` beserta
`Remove backend test projects and simplify Integration gate`.

Artinya jaring pengaman yang dipakai `BE-RAD-08` sampai `BE-RAD-15` — yang laporannya menyebut
162 sampai 203 uji radiologi dan 1.408 uji in-memory — **tidak ada lagi di working tree ini**.
Uji seperti `RadiologyRoleAccessContractTests`, `TidakSatuPunAturanLahirBerlaku`, dan
`MengisiEnamAlatDanEmpatButirKeselamatan` tidak dapat dijalankan maupun ditambahi.

**Verifikasi task ini karena itu terbatas pada build dan penelusuran source**, dan itu **lebih
lemah** daripada yang berlaku saat perilaku ini dirancang. Perubahan yang paling perlu diuji —
kelahiran bacaan pada penilaian mutu — menyentuh jalur yang sama dengan penyerahan fakta tagih,
dan **belum ada satu pun uji otomatis yang membuktikannya tidak merusak jalur itu.**

Ini bukan alasan menahan pekerjaan yang sudah diminta, tetapi **wajib diketahui sebelum
dijalankan pada data sungguhan.** Mengembalikan proyek uji adalah task tersendiri dan perlu
keputusan pemilik repository.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Bacaan lahir otomatis saat study menjadi `QualityAccepted` — `RAD-STATE-001` bagian 3 | **Terpenuhi di source** | `LahirkanBacaanAsync` dipanggil pada cabang citra layak `DecideQualityAsync` |
| Kegagalan kelahiran tidak menggagalkan penilaian mutu | **Terpenuhi** | Dibungkus `try/catch`, dicatat `RadStudy.EnsureReportFailed` |
| Penyaring bacaan belum dirilis tidak dilonggarkan | **Terpenuhi** | `GetByPatientAsync` menyalin dua penyaring `GetByEncounterAsync` apa adanya |
| Kesimpulan terbaca pada pembacaan berpusat pasien | **Terpenuhi** | `sertakanKesimpulan: true` pada `by-encounter` dan `by-patient` |
| Kesimpulan **tidak** bocor ke papan kerja | **Terpenuhi** | `GET /` memakai bawaan `false` |
| Rekam medis dapat membaca hasil per pasien | **Terpenuhi di backend** | `GET /by-patient/{patientId}`. **Layarnya belum dibuat** — task frontend tersendiri |
| Tidak ada perubahan schema | **Terpenuhi** | Tidak ada migration; `Impression` sudah ada pada `RadReportVersion` |
| Terbukti lewat uji otomatis | **TIDAK TERPENUHI** | Proyek uji tidak ada di repository. Lihat bagian 5 |

---

## 7. Catatan penutup

### 7.1 Koreksi atas laporan frontend

`FE-RAD-11` bagian 3, `FE-RAD-12` bagian 9, dan `FE-RAD-13` bagian 9 menuliskan
`RadReport : ActAsRadiologist` sebagai penghalang yang masih terbuka, dan `FE-RAD-13` bagian 9
menuliskan ketiadaan aturan keselamatan `Active` sebagai penghalang backend. **Keduanya keliru.**

Sumbernya dokumen blueprint yang belum disegarkan setelah `approval-requests/2026-09-11-keputusan-empat-penghalang.md`
menutup penghalang pertama, dan `BE-RAD-15` menjelaskan bahwa yang kedua memang bukan pekerjaan
kode. Saya membacanya dari dokumen, bukan dari source. **Yang berlaku adalah source.**

Pelajarannya lugas dan sudah tertulis di governance: keadaan penghalang diperiksa ke source, dan
dokumen dipakai untuk memahami maksudnya — bukan sebaliknya.

### 7.2 Yang masih menahan modul dipakai, dan ini bukan pekerjaan kode

| Langkah | Pelaku | Keadaan |
| --- | --- | --- |
| Menetapkan peran Quilvian yang memegang `RadSafetyRule : Approve`, `: Reject`, `: Deactivate` | Komite Medis, dijalankan Administrator | **`RAD-OPEN-011` masih terbuka** |
| Memberikan penanda `RadReport : ActAsRadiologist` kepada peran yang memang dokter radiolog | Administrator | Penandanya **sudah dapat dicentang**; belum diberikan |
| Mengesahkan dua belas usulan aturan keselamatan dari `Draft` menjadi `Active` | Pemegang akun yang ditunjuk Komite Medis | Drafnya **sudah tersedia** sejak `BE-RAD-15` |

Ketiganya berurutan: tanpa langkah pertama tidak ada yang berwenang menjalankan langkah ketiga,
dan tanpa langkah ketiga setiap pemeriksaan tetap ditolak gerbang keselamatan.

**Seeder yang menerbitkan aturan `Active` sengaja tidak dibuat**, dan tidak boleh dibuat: itu
berarti sebuah program menetapkan kapan seorang pasien aman disinari, dan meruntuhkan
`RAD-DEC-002` beserta `RAD-DEC-005` sekaligus.

### 7.3 Task berikutnya

| Task | Isi |
| --- | --- |
| Frontend | Menampilkan `GET /by-patient/{patientId}` pada layar rekam medis; menyederhanakan `FE-RAD-13` agar memakai `Impression` yang kini ikut pada `by-encounter` |
| Backend | Mengembalikan proyek uji ke repository — **perlu keputusan pemilik repository** |
| Dokumen | Menyegarkan `blueprint-manifest.md` dan `00-interview-decisions.md` agar penghalang yang sudah ditutup tidak terbaca terbuka |

### 7.4 Status Git

Tidak ada `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun `deploy` yang dijalankan.
Tidak ada migration yang dibuat maupun dieksekusi. Tidak ada eksekusi database langsung.

---

## 8. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-14 | Laporan dibuat. 4 berkas source diubah, 1 kontrak diamandemen. Build 0 error tanpa warning baru. Menutup celah `EnsurePendingReportAsync` yang tidak pernah dipanggil, menambah `GET /by-patient/{patientId}`, dan membawa `Impression` pada pembacaan yang berpusat pada pasien. **Dua dari tiga penghalang yang diminta ternyata sudah selesai sejak 2026-09-11**; koreksinya dicatat pada bagian 7.1. **Proyek uji tidak ada di repository**, sehingga verifikasi terbatas pada build dan penelusuran source. | `draft` |
