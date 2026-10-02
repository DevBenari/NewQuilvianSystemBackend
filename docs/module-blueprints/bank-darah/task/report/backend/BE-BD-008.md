# Laporan Perubahan Backend — `BE-BD-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-008` |
| Judul | Pemberian jalur darurat tercatat penuh |
| Slice | Jalur darurat — turunan `BE-BD-007` |
| Roadmap | [roadmap/backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian 5, revisi **11** |
| Trace | `DEC-BD-017`, `DEC-BD-038`, `DEC-BD-040`, **`DEC-BD-050`**; **`BD-DOM-08`** Otorisasi Darurat; `INV-BD-012`, `INV-BD-030`, `INV-BD-032`; api-contract `v4` baris 100 dan 109; state-transition §3; validation-matrix §3, §5, §7 |
| Contract version | `v4` — **`approved`** |
| Dependency | `G1` ✅, `G2b` ✅, `BE-BD-007` ✅ — seluruhnya tertutup |
| Klasifikasi | `MEDIUM` — satu entity baru, dua enum baru, satu konfigurasi EF baru, satu endpoint baru, satu saringan daftar baru; nol modul lain tersentuh |
| Task mode | `BACKEND` |
| Target tulis | `DevBenari/NewQuilvianSystemBackend` cabang `sukmagp` |
| Model | `claude-opus-5` |
| Commit backend saat dikerjakan | Mulai dari `14dde962` (`feat(bank-darah): close BE-BD-007 …`), working tree bersih. **Belum ter-commit** — seluruh perubahan masih di working tree atas instruksi pemilik |
| Tanggal | 16 September 2026 |
| Status | ✅ **SELESAI 16 September 2026 — nol remaining contract gap.** Kesembilan acceptance terbukti runtime, kelima kode `VAL-BD-*` terbukti dengan pesan persis kontrak, otorisasi dibuktikan dengan aktor non-SuperAdmin, build `0 Error(s)`, migration terterapkan dengan `0 pending`, QBE Strict `PASS`. Tegangan `VAL-BD-021`/`VAL-BD-072` **ditutup `DEC-BD-050`** tanpa perubahan source behavior |

---

## 1. Berkas implementasi

| Berkas | Sifat |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkEmergencyBypassScope.cs` | **BARU** — `CompatibilityEvidence`/`InactiveStorageLocation`/`Both` |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkEmergencyAuthorizerRole.cs` | **BARU** — `BloodBankDoctor`/`AttendingPhysician` |
| `Areas/HealthServices/BloodBankManagement/Models/BbkEmergencyAuthorization.cs` | **BARU** — entity `BD-DOM-08` |
| `Areas/HealthServices/BloodBankManagement/DTOs/EmergencyAuthorizationDtos.cs` | **BARU** — `EmergencyIssueRequest`, `EmergencyAuthorizationDto`, `BloodUnitEmergencyBypassState` |
| `Repositories/Configurations/…/BbkEmergencyAuthorizationConfiguration.cs` | **BARU** — pemetaan EF |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs` | **DISENTUH** — `EvaluateEmergencyBypassAsync`, `EmergencyIssueAsync`, saringan daftar kerja #3, ekstraksi `EvaluateCompatibilityEvidenceGateAsync`, outcome `Forbidden` |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodUnitController.cs` | **DISENTUH** — endpoint `POST /{id}/emergency-issue`, parameter `emergencyPendingEvidence`, cabang `403` pada `MapFailure` |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitDtos.cs` | **DISENTUH** — `EmergencyAuthorizations` pada detail |
| `Repositories/ApplicationDbContext.cs` | **DISENTUH** — satu `DbSet` |

### 1.1 Reuse `BE-BD-007`, bukan penyalinan aturan

Aturan bukti kecocokan **tidak disalin**. Blok penilaian bukti pada `EvaluateIssuanceGateAsync`
diekstrak apa adanya menjadi `EvaluateCompatibilityEvidenceGateAsync`, lalu dipakai bersama oleh
gerbang normal `BE-BD-007` dan penilaian bypass `BE-BD-008`. Urutan penilaiannya tidak diubah:
`VAL-BD-018` → `VAL-BD-019` → `VAL-BD-020b` → `VAL-BD-079` → `VAL-BD-020`.

Alasannya bukan kerapian: kalau aturannya disalin, jalur darurat dapat menyatakan gerbang bukti
"terbuka" pada keadaan yang justru ditahan jalur normal, dan penanda daruratnya menjadi keterangan
palsu pada rekam klinis (`INV-BD-030`). Gerbang normal berhenti pada penolakan pertama, sehingga
`EvaluateEmergencyBypassAsync` menilai kedua gerbang terpisah — jalur darurat wajib menjawab
**keduanya**, bukan yang pertama tertutup saja.

---

## 2. Migration

`20260916051204_AddBbkEmergencyAuthorization`.

| Pemeriksaan | Hasil |
| --- | --- |
| Scope `Up` | **Satu** tabel `public.BbkEmergencyAuthorization`; tiga index (`BloodUnitId`, `PatientId`, `AuthorizerRole`); dua FK `Restrict` ke `BbkBloodUnit` dan `MstPatient`. **Nol operasi pada tabel modul lain** |
| Scope `Down` | Hanya `DropTable` tabel yang sama |
| `has-pending-model-changes` | **Bersih** — "No changes have been made to the model since the last migration" |
| `migrations list` sebelum penerapan | **Tepat satu** `(Pending)`, yaitu migration ini; nol migration modul lain ikut tertunda |
| Penerapan | `database update` **Done** ke `QuilvianNewDevSukma` |
| `migrations list` sesudah | **0 pending** |
| Verifikasi PostgreSQL | Tabel ada dengan 20 kolom sesuai kamus data; keempat index (`PK` + 3) terverifikasi ada |

---

## 3. Keputusan kontrak yang diambil saat implementasi

### 3.1 Peran penerbit direkam, tidak diverifikasi

`contracts/validation-matrix.md` menyatakan eksplisit: *"sistem **tidak** memeriksa apakah penerbit
otorisasi darurat benar DPJP dari pasien yang bersangkutan … Bank Darah bukan pemilik data penugasan
DPJP. Yang dijaga adalah kelengkapan rekam."* Implementasi mengikuti itu apa adanya: `AuthorizerRole`
disimpan sebagai pernyataan penerbit, dan tidak ada pencocokan terhadap `RequestingDoctorId` maupun
data penugasan mana pun. Menambahkan verifikasi itu akan **melanggar** kontrak, bukan memperkuatnya.

### 3.2 `VAL-BD-021` dan `VAL-BD-072` — tegangan kontrak, kini **CLOSED** oleh `DEC-BD-050`

Keduanya `403` dan pemicunya beririsan. `AC-BD-021` menuntut `VAL-BD-021` untuk "jalur darurat oleh
peran tak berwenang", sementara `AC-BD-083` menuntut `VAL-BD-072` untuk "Petugas Bank Darah tanpa
wewenang darurat" — dua kalimat yang menggambarkan **kejadian runtime yang sama**.

Pemetaan yang dipakai, seluruhnya bersandar pada pemicu yang memang tertulis di matriks:

| Pemicu | Kode | Lapisan |
| --- | --- | --- |
| Pelaku tidak memegang `BloodUnit : EmergencyIssue` | **`VAL-BD-072`** | Hak akses |
| Alasan kosong atau tidak dikenali | **`VAL-BD-021`** | Service |

`VAL-BD-072` dipilih untuk penolakan kewenangan karena pemicunya persis itu, kalimatnya menyebut
kedua peran yang berwenang, dan ia lahir pada *role & authority closure pass* (`DEC-BD-040`) yang
lebih baru dan lebih spesifik daripada `DEC-BD-017`. `VAL-BD-021` tetap punya rumah runtime yang
nyata lewat separuh pemicunya sendiri yang saat itu tertulis di matriks — *"atau alasan kosong"* —
dan sejak `DEC-BD-050` separuh itulah satu-satunya pemicunya.

**Akibatnya `AC-BD-021` terbukti secara perilaku** (aktor tak berwenang ditolak `403`) **dengan kode
literal `VAL-BD-072`, bukan `VAL-BD-021`.**

> **`CLOSED` 16 September 2026 — `DEC-BD-050`, disetujui `Sukmagp`.** Pemilik kontrak menetapkan
> pembagian yang persis sama dengan yang sudah berjalan di runtime: `VAL-BD-072` untuk pelaku tanpa
> kewenangan `BloodUnit : EmergencyIssue`, `VAL-BD-021` untuk alasan darurat yang kosong atau tidak
> sah. **Nol source behavior berubah dan nol runtime acceptance dijalankan ulang** — implementasi
> memang sudah mengikuti mapping itu sejak semula. Dokumen kanonik yang diselaraskan:
> `validation-matrix.md` (pemicu `VAL-BD-021` dipersempit + tabel pembagian),
> `acceptance-test-matrix.md` (`AC-BD-021` kini menuntut `VAL-BD-072`), dan
> `00-interview-decisions.md` (`CONF-BD-007` beserta `DEC-BD-050`). `state-transition-matrix.md`
> tidak perlu diubah — dokumen itu sudah memakai pembagian ini sejak semula.
>
> **Sisa yang sengaja tidak dikerjakan:** kalimat `VAL-BD-021` masih menyebut "hanya untuk peran
> berwenang". Menyempitkannya menuntut perubahan source dan build ulang, di luar lingkup
> `DEC-BD-050` yang hanya membagi pemicu.

### 3.3 Daftar kerja #3

`api-contract.md` baris 100 mendefinisikan `emergencyPendingEvidence=true` sebagai daftar kerja #3
tanpa merinci predikatnya. Predikat yang dipakai diturunkan dari kolom yang sudah ada, bukan state
baru: `IssuedViaEmergency = true` **dan** `CompatibilityEvidenceIdUsed IS NULL` — yaitu pemberian
darurat yang benar-benar melewati gerbang bukti, sehingga buktinya masih harus disusulkan. Pemberian
darurat yang hanya melewati gerbang lokasi tetap menunjuk bukti yang sah dan karena itu **tidak**
masuk daftar. Terbukti runtime pada bagian 4.

---

## 4. Runtime acceptance — 9 dari 9

Seluruhnya lewat panggilan API sungguhan terhadap `QuilvianNewDevSukma`, ditambah verifikasi database.

| Acceptance | Fixture | HTTP | Bukti | Hasil |
| --- | --- | --- | --- | --- |
| `AC-BD-020` | `-01` | `200` | `UnitStatus 4`, `IssuedViaEmergency=True`, `CompatibilityEvidenceIdUsed NULL` (ditandai tanpa bukti), **muncul di daftar #3** | ✅ |
| `AC-BD-021` | `-03` | `403` | Ditolak dengan `VAL-BD-072`, **kini sesuai kontrak** sesudah `DEC-BD-050` menyelaraskan `acceptance-test-matrix.md` (bagian 3.2) | ✅ |
| `AC-BD-074` | `-05` | `200` | Jalur normal ditolak `VAL-BD-065` lebih dulu sebagai kontrol; jalur darurat berhasil; alasan, pelaku, waktu, peran, dan keterangan tercatat; penanda melekat | ✅ |
| `AC-BD-075` | `-03` | `422` | `VAL-BD-066` ketika gerbang yang dilewati tidak disebut | ✅ |
| `AC-BD-081` | `-02` | `200` | `AuthorizerRole = 1` **`AttendingPhysician`** tersimpan | ✅ |
| `AC-BD-082` | `-01` | `200` | `AuthorizerRole = 0` **`BloodBankDoctor`** tersimpan | ✅ |
| `AC-BD-083` | `-03` | `403` | `VAL-BD-072` dengan aktor non-SuperAdmin tanpa `EmergencyIssue` | ✅ |
| `AC-BD-084` | `-03` | `422` | `VAL-BD-070` ketika keterangan kondisi kedaruratan kosong | ✅ |
| `AC-BD-085` | `-03` | `422` | `VAL-BD-071` ketika peran penerbit tidak dinyatakan | ✅ |

### 4.1 Bukti `INV-BD-030` — ketiga nilai `BypassScope`

| Keadaan kantong | Scope dinyatakan | Hasil |
| --- | --- | --- |
| Tanpa bukti, lokasi aktif | `CompatibilityEvidence` | `200` |
| Bukti sah, lokasi nonaktif | `InactiveStorageLocation` | `200`, `CompatibilityEvidenceIdUsed` **tetap terisi** |
| Tanpa bukti, lokasi nonaktif | `Both` | `200` |
| Tanpa bukti, lokasi **aktif** | `InactiveStorageLocation` | `422 VAL-BD-066` — tidak sesuai keadaan kantong |
| Tanpa bukti, lokasi **nonaktif** | `CompatibilityEvidence` | `422 VAL-BD-066` — menyembunyikan satu gerbang yang benar-benar dilewati |

Keadaan "darurat yang tidak melewati gerbang apa pun" tidak dapat ditulis: enumnya tidak punya nilai
untuk itu, dan pernyataan yang tidak sesuai keadaan kantong ditolak.

---

## 5. Contract acceptance — 5 dari 5

Pesan dibandingkan **secara terprogram** terhadap `contracts/validation-matrix.md`, bukan dicocokkan
dengan mata.

| Kode | HTTP | `errors.code` | Pesan persis kontrak |
| --- | --- | --- | --- |
| `VAL-BD-021` | `403` | ✅ | ✅ |
| `VAL-BD-066` | `422` | ✅ | ✅ |
| `VAL-BD-070` | `422` | ✅ | ✅ |
| `VAL-BD-071` | `422` | ✅ | ✅ |
| `VAL-BD-072` | `403` | ✅ | ✅ |

**Nol partial state mutation.** Sesudah enam penolakan berturut-turut, keenam kantong fixture tetap
`UnitStatus 3`, `Version` tidak bergeser, `IssuedAt`/`IssuedViaEmergency`/`CompatibilityEvidenceIdUsed`
tetap kosong, dan `BbkEmergencyAuthorization` tetap **0 baris**.

---

## 6. Otorisasi

Aktor uji `test.bd008.dokter@rsmmc.local`, `UserType` `Employee`, `roles` kosong — **bukan SuperAdmin**,
pada Department dan Position `TEST-BD008` tersendiri sehingga identitas `TEST-BD007` tidak tersentuh.

| Keadaan policy | Endpoint | Hasil |
| --- | --- | --- |
| `Read` + `Issue`, **tanpa** `EmergencyIssue` | `GET /blood-units/{id}` | `200` — akun dan token sah |
| `Read` + `Issue`, **tanpa** `EmergencyIssue` | `POST /{id}/emergency-issue` | `403` + `VAL-BD-072` + pesan kontrak persis |
| Ditambah `EmergencyIssue` | `POST /{id}/emergency-issue` — **request identik** | `200`, otorisasi tersimpan |

Variabel tunggal antara ditolak dan diterima benar-benar hanya permission. Butir
`BloodUnit : EmergencyIssue` ter-seed otomatis dari `[AccessAction]` saat aplikasi start; **nol action
dummy dibuat**, dan butir hak akses `BE-BD-007` tidak berubah.

---

## 7. Regression

Hanya jalur `BE-BD-007` yang benar-benar dipakai `BE-BD-008` yang di-smoke; acceptance `BE-BD-007`
tidak dijalankan ulang.

| Uji | Hasil |
| --- | --- |
| Gerbang bukti normal pada kantong tanpa evidence | `422 VAL-BD-018` — ekstraksi helper tidak mengubah perilaku |
| Catat evidence sah lalu `POST /{id}/issue` | `200`, `IssuedViaEmergency=False`, `CompatibilityEvidenceIdUsed` terisi, **nol** baris otorisasi darurat |
| Gerbang lokasi normal | `422 VAL-BD-065` pada kantong di lokasi nonaktif |
| Fixture `TEST-BD006` | 8 kantong, **0** bertanda darurat — tidak tersentuh |
| Fixture `TEST-BD007` | 8 kantong, **0** bertanda darurat — tidak tersentuh |
| Otorisasi darurat di luar `TEST-BD008` | **0 baris** |

---

## 8. Build dan QBE

| Gate | Hasil |
| --- | --- |
| `dotnet build` | **`0 Error(s)`** / `198 Warning(s)` — jumlah warning **sama persis** dengan baseline `BE-BD-007`, jadi nol warning baru dilahirkan task ini |
| QBE Strict | **`PASS`** — `WorkingTree`, 12 berkas, `VIOLATION 0` / `REVIEW 0` / `INFO 0`, nol suppression |
| `git diff --check` | bersih |

> **Catatan build.** Dua full build dijalankan, dan keduanya diperlukan alur EF: build pertama
> memvalidasi source final dan menyediakan assembly untuk `migrations add --no-build`; build kedua
> mengompilasi berkas migration beserta snapshot yang baru lahir, tanpanya
> `has-pending-model-changes` dan `database update` membaca snapshot lama dari DLL.

---

## 9. Fixture ledger `TEST-BD008` — belum dibersihkan

Seluruhnya dibuat lewat API, bukan tulis database langsung. Prefix `TEST-BD008-20260916125637`.

**Master.** Komponen `TBD008-V` `d9c5e10b-2fc7-4e8d-8f68-c3a177a2f826` (validity `24`) · lokasi
`TBD008-LOCA` `8a703c4e-b5d7-4429-b243-bda44ea4d337` (aktif) dan `TBD008-LOCB`
`0732d508-195d-4eb9-8b05-9f9e02ef8690` (**sengaja nonaktif** untuk `AC-BD-074`) · alasan
`TBD008-DARURAT` kategori `Emergency`.

**Identitas.** Department `d60c27fa-ffdb-4a74-a6b1-cf866b2d7b81` · Position
`ca021cee-9814-4388-b492-95ac9de739ae` · `AspNetUsers` `168feed0-a551-4ac5-a3c2-536436b0b8c3`
(`test.bd008.dokter@rsmmc.local`) beserta employee, workforce profile, dan satu baris
`AspNetUserOrganization` · **tiga baris `SysAccessPolicy`** (`Read`, `Issue`, `EmergencyIssue`).

**Transaksional.** Order `ORD-00000085` `49a882ed-6e1c-4aee-a7dd-1c1c1110f8a5` · permintaan PMI
`PMI-00000041` `647dd1f4-8ed2-4ee9-9edf-ce3a7351ca9e` · enam kantong beserta penempatan dan alokasinya
· dua baris `BbkCompatibilityEvidence` · **empat baris `BbkEmergencyAuthorization`**.

**Keadaan akhir kantong.**

| Kantong | Status | Darurat | Pakai bukti | Scope | Peran |
| --- | --- | --- | --- | --- | --- |
| `-01` | `Issued` | ya | tidak | `CompatibilityEvidence` | `BloodBankDoctor` |
| `-02` | `Issued` | ya | tidak | `CompatibilityEvidence` | `AttendingPhysician` |
| `-03` | `Allocated` | — | — | — | — |
| `-04` | `Issued` | ya | tidak | `Both` | `AttendingPhysician` |
| `-05` | `Issued` | ya | **ya** | `InactiveStorageLocation` | `BloodBankDoctor` |
| `-06` | `Issued` | **tidak** (jalur normal) | ya | — | — |

**Cleanup belum dijalankan dan belum disetujui.** Lima kantong `Issued` bersifat terminal; tiga baris
`SysAccessPolicy` perlu dicabut terpisah.

---

## 10. Sisa pekerjaan yang bukan milik task ini

- ~~Tegangan kontrak `VAL-BD-021` versus `VAL-BD-072`~~ — **CLOSED 16 September 2026** oleh
  `DEC-BD-050` (`CONF-BD-007`), disetujui `Sukmagp`. Dokumen kanonik sudah diselaraskan; nol source
  behavior berubah, nol runtime acceptance dijalankan ulang. Lihat bagian 3.2.
- **Trace kartu roadmap `BE-BD-008` menulis `BD-DOM-09`**, padahal `BD-DOM-09` adalah *Pemeriksaan
  Golongan Darah*; konsep domain jalur darurat adalah **`BD-DOM-08`** Otorisasi Darurat. Dicatat
  sebagai koreksi trace, bukan perubahan requirement.
- **Cleanup fixture** `TEST-BD008` menunggu persetujuan pemilik.
- **Commit** belum dilakukan atas instruksi pemilik.
- `BE-BD-009` dan `BE-BD-010` **tidak** dikerjakan dan **tidak** ditandai selesai pada sesi ini.
