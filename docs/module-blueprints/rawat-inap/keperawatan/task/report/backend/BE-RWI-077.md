# Laporan Perubahan Backend — `BE-RWI-077`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-077` |
| Judul | Deret waktu tanda vital tidak dapat diputus diam-diam |
| Slice | Gelombang `KEP-1A` — Rawat Inap Safety Corrections |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian `S5` |
| Trace | `RWI-DEC-098`; `FR-KEP-029`, `FR-KEP-030`; `04-prd-to-mvp.md` bagian 21.3 |
| Contract version | API `0.4.0` bagian 0.A.1; `state-transition-matrix.md` `0.4.0` bagian 3A — `approved` 11 September 2026 lewat `RWI-DEC-105` |
| Dependency | Approval kontrak `0.4.0` — terpenuhi |
| Klasifikasi | `LOW` — repository 1 (0), berkas diperiksa ≤ 8 (0), berkas diubah 1 (0), logika bisnis rendah (0), memakai kontrak yang sudah ada (1), pencabutan permukaan yang sudah ada (0), keamanan menyentuh hak akses (1), workflow terbatas (1). Total **3** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Garis dasar `c11904ea`, branch `MHamzah` |
| Tanggal | 11 September 2026 |
| Status | **Selesai untuk source.** Keempat acceptance criteria terpetakan ke source. Butir verifikasi yang tidak dijalankan ditulis `NOT RUN` apa adanya pada bagian 5 |

---

## Backend Governance Preflight

| Field | Isi |
| --- | --- |
| Area | `HealthServices` |
| Module | `ClinicalManagement` |
| Submodule | — |
| Pemilik / prefix registry | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 15 — `HealthServices \| ClinicalManagement / Clinical \| BUSINESS DOMAIN / MODULE \| Cli \| ACTIVE / LEGACY` |
| Keberlakuan | `TOUCHED LEGACY` — `PatientVitalSignController` adalah controller lama, dan yang dikerjakan adalah pencabutan satu action di dalamnya |
| Status registry | `ACTIVE / LEGACY`, sudah terdaftar. `QBE-MOD-002` tidak berlaku: **nol** entity operasional baru diminta task ini |
| QBE ID yang berlaku | `QBE-MOD-002` — tidak memblokir, tidak ada module/entity baru. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — tidak berlaku, tidak ada rename dan tidak ada pekerjaan database |
| Legacy ratchet | Ditegakkan. Tidak ada refactor massal pada legacy yang tidak disentuh; `TrxPatientVitalSign` dan pemakaian `ApplicationDbContext` langsung pada controller ini **tidak** diubah |

---

## 1. Masalah yang diperbaiki

`PatientVitalSignController` memiliki satu action `HttpDelete("{id:guid}")` yang menghapus satu
baris tanda vital secara soft delete. Action itu:

- **tidak** memeriksa status dokumen — tanda vital yang sudah `Verified` pun dapat dihapus;
- **tidak** meminta alasan — tidak ada satu kolom pun yang menyimpan kenapa barisnya hilang;
- **tidak** memeriksa penulis — siapa pun yang memegang `PatientVitalSign : Delete` dapat menghapus
  catatan orang lain;
- ikut menyetel `NeedDoctorNotification = false`, sehingga penanda pemberitahuan ke dokter padam
  bersama barisnya.

Akibatnya bukan sekadar satu angka hilang. Tanda vital adalah dasar penilaian perburukan pasien, dan
yang dibaca perawat maupun dokter adalah **kecenderungannya**, bukan satu titiknya. Baris yang
dihapus tidak meninggalkan lubang yang terlihat, sehingga grafik tetap tampak wajar justru karena
datanya sudah tidak ada. Jalur ini juga menjadi cara paling mudah mematikan pemberitahuan ke dokter
tanpa jejak.

`RWI-DEC-098` menutup jalur itu, dan `api-contract.md` `0.4.0` bagian 0.A.1 menetapkan jawabannya
menjadi `404` karena routenya tidak lagi terdaftar.

---

## 2. Proses bisnis

**Tujuan.** Tanda vital yang sudah tercatat tidak dapat dihilangkan dari deret waktu pasien.

**Pelaku.** Perawat pelaksana, kepala ruangan, dan siapa pun yang mencatat tanda vital di rawat
jalan maupun IGD.

**Jalur yang sah sejak `0.4.0`.**

| Keadaan dokumen | Jalur yang dipakai | Yang tersimpan |
| --- | --- | --- |
| Salah catat, belum final | `PATCH /patient-vital-signs/{id}/cancel` | `CancelReason`, `CancelledAt`, `CancelledByUserId`, status `Cancelled`; barisnya **tetap terbaca** |
| Sudah final atau terverifikasi | Addendum milik `MedicalRecordManagement` | Isi asli tidak berubah; koreksinya bernomor |

**Yang berubah bagi pengguna.** Tombol hapus pada tanda vital tidak lagi punya endpoint. Butir hak
akses **"Delete Patient Vital Sign"** ikut hilang dari layar Pengaturan → Manajemen Role → Akses
Role, karena `[AccessAction]`-nya ikut tercabut. Itu disengaja: selama butirnya masih terdaftar,
admin akan melihat kemampuan yang sebenarnya sudah tidak ada.

**Yang tidak berubah.** Pembatalan beralasan sudah ada sebelum task ini dan tidak disentuh sama
sekali. Pencatatan, pembaruan, verifikasi, dan penandaan pemberitahuan dokter tetap seperti
sebelumnya, pada ketiga jenis pelayanan.

---

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| [`Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs`](../../../../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs) | Action `DeleteVitalSign` beserta `[HttpDelete("{id:guid}")]`, `[AccessAction("Delete", "Delete Patient Vital Sign", …)]`, dan `[AccessPermission("PatientVitalSign", "Delete")]` **dihapus seluruhnya**. Tempatnya diisi blok keterangan yang menyebut apa yang dulu ada di situ, kenapa dicabut, dan ke mana penggantinya |

**Satu berkas, satu action.** Tidak ada berkas lain yang disentuh, tidak ada DTO yang dihapus, dan
tidak ada model yang berubah.

**Kenapa tempatnya ditinggalkan bertanda, bukan dikosongkan.** Controller ini memakai pola sembilan
endpoint master data, dan `DELETE /{id}` adalah salah satu barisnya. Tanpa keterangan, pembaca
berikutnya akan membaca ketiadaannya sebagai kelalaian dan memasangnya kembali. Blok keterangan itu
menyebut `RWI-DEC-098` dan nomor task ini supaya alasannya dapat ditelusuri.

**Nol tabel, nol kolom, nol migration.** `TrxPatientVitalSign` tidak berubah satu kolom pun, dan
kolom warisan `IsDelete`, `DeleteDateTime`, serta `DeleteBy` tetap ada — baris lama yang sudah
terlanjur dihapus tetap terbaca apa adanya. Tabelnya milik `ClinicalManagement` sesuai
`RWI-DEC-081`, dan task ini hanya mencabut route.

---

## 4. Dokumentasi endpoint

| Method | Path | Sebelum | Sesudah |
| --- | --- | --- | --- |
| `DELETE` | `/api/patient-vital-signs/{id}` | `200` beserta soft delete | **Route tidak terdaftar.** Routing menjawab `404` |
| `PATCH` | `/api/patient-vital-signs/{id}/cancel` | Tidak berubah | Tidak berubah — inilah jalur pembatalan beralasan |

Delapan endpoint lain pada controller ini — `filters/metadata`, `critical-alerts`, daftar, `options`,
detail, `active-by-encounter`, `active-by-queue`, `POST`, `PUT`, `verify`, dan `notify-doctor` —
tidak tersentuh.

**Kenapa `404` dan bukan `403`.** `403` menyatakan "Anda tidak boleh", yang berarti ada hak akses
yang dapat membukanya. Di sini tidak ada: kemampuannya sudah tidak ada pada sistem, dan tidak ada
baris hak akses mana pun yang dapat menghidupkannya kembali. `api-contract.md` `0.4.0` bagian 0.A.1
menuliskannya persis begitu.

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis
(rules/backend/TEST_POLICY.md)`

| Butir | Hasil |
| --- | --- |
| QBE preflight dan conformance | **Lulus.** Lihat bagian Backend Governance Preflight. Nol module baru, nol entity baru, nol rename |
| Review diff dan cakupan | **Lulus.** Satu berkas berubah, satu action dihapus. `grep -c "HttpDelete"` pada berkas itu bernilai `1`, dan satu-satunya kemunculan tersisa ada **di dalam teks komentar**, bukan sebagai atribut. `grep -c "AccessTypes.Delete"` bernilai `0` |
| Pemeriksaan pemakaian lintas repository | **Lulus.** Penelusuran `patient-vital-signs` yang mengandung kata `delete` pada `QuilvianSystemFrontendDev/src` bernilai nol hasil — tidak ada layar frontend yang memanggil route ini |
| Verifikasi kontrak API | **Lulus secara telaah.** Ketetapan `api-contract.md` `0.4.0` bagian 0.A.1 terpenuhi apa adanya: routenya tidak lagi terdaftar |
| `dotnet restore` | `NOT RUN` — pemilik meminta task ditutup tanpa build pada sesi ini |
| `dotnet build QuilvianSystemBackend.sln` | `NOT RUN` — pemilik meminta task ditutup tanpa build pada sesi ini. Ini **butir DoD yang dikecualikan pemilik**, bukan butir yang lulus |
| `AC-KEP-040` s.d. `AC-KEP-042` — uji integrasi | `NOT RUN` — backend tidak memelihara project test otomatis, dan pemilik tidak meminta pembuatannya pada task ini |
| `AC-KEP-043` — regresi Rawat Jalan dan IGD, uji integrasi | `NOT RUN` — sebab yang sama. Penalarannya ada pada bagian 6, dan **bukan** pengganti uji |
| Pembatalan tanda vital **final** | `NOT RUN` **dan memang belum dapat diuji.** Lihat bagian 7 butir `V2-UNK-01` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Bunyi | Pemetaan ke source | Keadaan |
| --- | --- | --- | --- |
| `AC-KEP-040` | `DELETE /patient-vital-signs/{id}` dijawab `404`, bukan `403` | Action beserta `[HttpDelete]`-nya tidak ada lagi pada `PatientVitalSignController`, sehingga routing ASP.NET Core tidak menemukan endpoint dan menjawab `404` | **Terpetakan.** Uji `NOT RUN` |
| `AC-KEP-041` | Tanda vital yang sudah tercatat tetap terbaca pada lini masa | `BuildBaseQuery` tetap menyaring `!x.IsDelete` dan tidak ada lagi jalur yang menyetel `IsDelete = true` pada tabel ini; pembatalan hanya mengubah `VitalSignStatus` dan `IsActive`, sehingga barisnya tetap lolos saringan | **Terpetakan.** Uji `NOT RUN` |
| `AC-KEP-042` | Penanda pemberitahuan dokter tidak dapat dimatikan lewat penghapusan | Satu-satunya jalur tersisa yang menyetel `NeedDoctorNotification = false` adalah `CancelVitalSign`, dan jalur itu **mewajibkan** `CancelReason` serta menyimpan `CancelledAt` dan `CancelledByUserId` | **Terpetakan.** Uji `NOT RUN` |
| `AC-KEP-043` | Regresi Rawat Jalan dan IGD: pencatatan tanda vital di luar rawat inap tidak ikut berubah | Yang dicabut adalah **satu** action. Sebelas action lain pada controller yang sama tidak disentuh satu baris pun, dan tidak satu pun dari mereka membaca atau menulis kolom `IsDelete`. Controller ini tidak membedakan jenis pelayanan, sehingga ketiadaan perubahan pada action lain berlaku sama bagi ketiganya | **Terpetakan lewat penalaran diff.** Uji integrasi `NOT RUN`, dan penalaran ini **bukan** penggantinya |

**Definition of Done.**

| Butir DoD | Keadaan |
| --- | --- |
| Keempat acceptance criteria terpetakan ke source | **Terpenuhi** |
| Nol `HttpDelete` tersisa pada berkas itu | **Terpenuhi.** Nol atribut; satu kemunculan kata di dalam komentar |
| Regresi Rawat Jalan dan IGD lulus | **Tidak terpenuhi lewat uji.** `NOT RUN`; hanya penalaran diff yang tersedia |
| Butir yang tidak dapat diuji ditulis `NOT RUN` beserta sebabnya | **Terpenuhi** |
| `dotnet build` tanpa error baru | **Dikecualikan pemilik pada sesi ini.** `NOT RUN` |
| Kesesuaian QBE dan preflight engineering | **Terpenuhi** |

---

## 7. Catatan penutup

**Satu prasyarat yang belum terpenuhi, dan itu sudah diketahui sejak kontraknya disetujui.**
`ClinicalDocumentKind.VitalSign` bernomor `6`, tetapi **belum termasuk** jenis dokumen yang
ditegakkan mesin keutuhan `MedicalRecordManagement`. Yang ditegakkan hari ini hanya `ProgressNote`,
`Consultation`, `Assessment`, dan `Procedure`. Selama `VitalSign` belum masuk daftar itu:

- `CancelVitalSign` **tidak dapat** memeriksa apakah dokumennya sudah final, sehingga tanda vital
  yang sudah `Verified` masih dapat dibatalkan lewat jalur pembatalan;
- penggantinya karena itu **belum utuh**, dan itu dinyatakan terbuka, bukan disembunyikan.

Dilacak sebagai `V2-UNK-01` pada `01-existing-capability-map.md` bagian 16.5. Keputusan menaikkan
`VitalSign` ke daftar yang ditegakkan milik pemilik `MedicalRecordManagement`, bukan milik task ini.

**Akibat yang diterima secara sadar.** Menutup `DELETE` tetap menghilangkan cara menyembunyikan
catatan, walaupun pembatalan draf belum terjaga terhadap dokumen final. Itu lebih baik daripada
keadaan sebelumnya, dan **bukan** pengganti penegakan yang utuh.

**Delta kontrak.** Tidak ada. Ketetapan `0.4.0` bagian 0.A.1 dilaksanakan apa adanya.

**Delta hak akses yang perlu diketahui admin.** Baris hak akses `PatientVitalSign : Delete` yang
sudah terlanjur dicentang pada peran mana pun kini menunjuk kemampuan yang tidak ada. Ia tidak
menimbulkan kesalahan, tetapi sebaiknya dibersihkan oleh admin agar layar Akses Role tidak
menampilkan kemampuan yang sudah dicabut. Pembersihan itu **bukan** pekerjaan task ini, karena
menyentuh data hak akses yang sedang berjalan.

**Migration dan database.** Nol. Tidak ada migration yang dibuat, dan tidak ada eksekusi database
yang diminta maupun dijalankan.

**Git.** Tidak ada stage, commit, push, pull, merge, rebase, maupun deploy yang dilakukan.

**Task berikutnya.** `BE-RWI-078` pada gelombang yang sama, dikerjakan paralel dan tidak menunggu
task ini.
