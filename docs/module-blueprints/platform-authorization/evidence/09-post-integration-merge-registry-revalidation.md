# Revalidasi Registry Sesudah Merge `QuilvianIntegrationBackend`

> **Mode:** revalidasi baca-saja. Tidak ada aplikasi dijalankan, tidak ada `AccessMenuSeeder`,
> tidak ada database write, tidak ada policy expansion, tidak ada push.
> Database `QuilvianNewDevAndryZain` **tidak tersentuh**.

| Field | Nilai |
|---|---|
| Jenis dokumen | **Revalidation evidence** — bukan laporan task |
| Sebab | Source HEAD berubah oleh merge, sehingga hasil `SAFE` pada `evidence/07` wajib diuji ulang |
| HEAD sesudah merge | `2c88abd9` |
| Otoritas database | Ekspor DBeaver 11 September 2026 — **masih berlaku**, database tidak berubah |
| Tanggal | 11 September 2026 |
| Hasil | **`POST_INTEGRATION_REGISTRY_SAFE_FOR_BE_SEC_003B`** |

---

## A. Urutan commit

| Commit | Isi |
|---|---|
| `40b6636a` | Checkpoint Fase A′ — `WriteSoap`, paritas `IsCancel`, evidence 05/06/07, laporan task, 4 skrip SQL diagnostik |
| `ee242356` | `evidence/08` — 73 invarian otorisasi dilestarikan **sebelum** Tests dihapus |
| `2c88abd9` | Merge `origin/QuilvianIntegrationBackend` (12 commit) |

Merge, bukan rebase. Tidak ada `reset --hard`, tidak ada force push, tidak ada push sama sekali.

---

## B. Apa yang sebenarnya dibawa Integration

| Berkas | Perubahan |
|---|---|
| `Tests/**` | **193 berkas dihapus** (71.831 baris) — seluruh backend test project |
| `QuilvianSystemBackend.sln` | 4 project test dicabut (−60 baris) |
| `QuilvianSystemBackend.csproj` | **−7 baris**, termasuk `DefaultItemExcludes` untuk `Tests\**` |
| `.github/workflows/integration-to-dev.yml` | Gerbang validasi Integration (+84 baris) |

**Nol perubahan** pada `Areas/`, `Services/`, `Attributes/`, `Filters/`, `Seeders/`. Dua belas commit
Integration seluruhnya bersifat ops/CI, penghapusan test, dan satu perbaikan metadata migration
farmasi.

---

## C. Konflik dan penyelesaiannya

### C.1 Konflik tunggal

`Tests/.../BillingManagement/AccessPermissionEnforcementTests.cs` — dihapus Integration, diubah
AndryZain (Fase A′ menambahkan regression test `IsCancel`). **Diselesaikan mengikuti penghapusan
Integration**, sesuai kebijakan.

### C.2 Lima berkas yang tidak ikut konflik — dan kenapa tetap dihapus

`Tests/.../Security/` berisi lima berkas yang **hanya pernah ada di AndryZain** dan tidak pernah
sampai ke Integration. Karena Integration tidak memilikinya, tidak ada penghapusan untuk
diterapkan — kelimanya lolos merge tanpa konflik:

`CanonicalSecurityContractTests` · `OrganizationAuthorizationProjectionTests` ·
`PermissionRegistryInvariantTests` · `PermissionSplitPreparationTests` · `StaleRegistryAuthorizationTests`

Membiarkannya **bukan pilihan netral**, karena Integration juga mencabut baris berikut dari
`QuilvianSystemBackend.csproj`:

```xml
<!-- Folder test terpisah dari web project. Tidak boleh ikut compile ke aplikasi utama. -->
<DefaultItemExcludes>$(DefaultItemExcludes);Tests\**</DefaultItemExcludes>
```

Pencabutan itu masuk akal bagi Integration — di sana `Tests/` memang tidak ada lagi, sehingga tidak
ada yang perlu dikecualikan. Tetapi di AndryZain, dengan `.csproj` test sudah terhapus **dan**
exclusion sudah dicabut, kelima berkas itu akan ikut ter-glob dan **dikompilasi ke dalam web project
utama**. Karena itu kelimanya dihapus, setelah intent-nya dilestarikan pada `evidence/08`.

### C.3 Build pertama gagal — sisa artefak, bukan source

Build pertama sesudah merge **gagal** dengan galat `CS0579 Duplicate ... attribute` yang menunjuk
`Tests/*/obj/**/AssemblyInfo.cs`.

Sebabnya sama: tanpa `DefaultItemExcludes`, web project juga meng-glob **artefak build** yang masih
tertinggal di `Tests/*/obj/` dan `Tests/*/bin/`. Berkas-berkas itu tidak terlacak Git (gitignored),
sehingga merge tidak menghapusnya. Setelah direktori sisa dibersihkan, build berhasil.

> **Konsekuensi yang perlu diketahui tim lain.** Clone baru tidak akan mengalami ini — `Tests/`
> memang tidak ada. Yang terdampak adalah **working copy yang sudah ada**: siapa pun yang menarik
> merge ini ke atas salinan kerja lamanya akan mendapat build gagal sampai `Tests/` dibersihkan.

---

## D. Hasil build

`dotnet build QuilvianSystemBackend.csproj -c Debug` → **`Build succeeded. 186 Warning(s), 0 Error(s)`**

Hanya project utama yang dibangun. Aplikasi **tidak** dijalankan, seeder **tidak** dipanggil.

---

## E. Snapshot source sesudah merge

Diturunkan ulang dengan mekanisme kanonik yang sama —
`PermissionRegistryDescriptor.BuildFromAssembly(typeof(AccessPermissionService).Assembly)` —
lewat runner baca-saja sekali pakai di scratchpad. Tidak ada algoritma penemuan tandingan, tidak ada
host yang dinyalakan, tidak ada koneksi database. Artefak runner-nya sudah dihapus kembali.

| Metrik | Sebelum merge | **Sesudah merge** | Selisih |
|---|---:|---:|---:|
| Module | 45 | **45** | 0 |
| Resource | 329 | **329** | 0 |
| Action | 1.246 | **1.246** | 0 |
| Fallback kompatibilitas | 69 | **69** | 0 |
| Metadata gap | 0 | **0** | 0 |

Angkanya tidak sekadar sama jumlahnya. Perbandingan berkas snapshot sebelum dan sesudah merge
menghasilkan **`IDENTICAL`** — nol baris berbeda, baik pada daftar registry maupun pada himpunan
fallback. Itu memang yang diharapkan dari merge yang tidak menyentuh satu pun berkas di `Areas/`
atau `Services/`, dan kini terbukti, bukan diasumsikan.

---

## F. Klasifikasi drift sesudah merge

Otoritas database masih ekspor 11 September; database tidak berubah. Himpunan kritis tetap
**299 kunci** yang memegang **452** policy efektif (+17 pada baris yang sudah tertutup = 469 ✓).

| Kategori | Sebelum | **Sesudah** |
|---|---:|---:|
| `MATCH` | 293 | **293** |
| `DB_ONLY_ACTIVE_WITH_EFFECTIVE_POLICY` | 6 | **6** |
| `SOURCE_ONLY` | ≈170 bersih | **≈170 bersih** |
| `METADATA_CHANGED` — `ModuleCode` pada resource hidup | 0 | **0** |
| `DB_ONLY_CLOSED` | tidak berubah | tidak berubah |

Keenam kunci `DB_ONLY_ACTIVE_WITH_EFFECTIVE_POLICY` persis sama:

| Kunci | Policy | Pengguna | Sifat |
|---|---:|---:|---|
| `BillingItemCategory.{Create,Delete,Read,Update}` | 3 masing-masing | 14 | Milik **Billing** — digabung ke `TariffCategory`; sudah tidak punya endpoint |
| `DoctorQueue.Update` | 3 | 6 | Identitas pensiun `BE-SEC-003`, diganti Fase B |
| `PatientProcedure.Update` | 1 | 2 | Identitas pensiun `BE-SEC-003`, diganti Fase B |

Kesimpulan keselamatan `evidence/07` **tetap berlaku tanpa perubahan**: setiap kunci `DB_ONLY_ACTIVE`
adalah kemampuan yang tidak lagi punya endpoint, sehingga izinnya tidak pernah diperiksa dan tidak
ada kemampuan **terjangkau** yang hilang karena rekonsiliasi.

---

## G. Rekonsiliasi jumlah identitas — angka "22" tidak dipakai lagi

Ditelusuri ulang dari source sesudah merge, bukan dari laporan lama.

| Tahap | Identitas | Jumlah |
|---|---|---:|
| **Split set asli** Fase A (`85fcc3fd`) | `PatientProcedure` 6 · `DoctorConsultation` 2 · `DoctorQueue` 6 · `PatientVitalSign` 3 · `PatientAssessment` 2 · `PatientDiagnosis` 3 | **22** |
| **+ Fase A′** | `DoctorConsultation.WriteSoap` | **23** |
| **+ di luar split set** | `PatientAssessment.Amend` — ditambahkan tim lain | **24** |

Diverifikasi per resource terhadap source sesudah merge:

| Resource | Identitas di database | Identitas di source | Kunci baru | Pensiun |
|---|---|---|---:|---:|
| `PatientProcedure` | Create, Read, Update | Approve, Cancel, Create, Edit, Execute, Read, RemoveDraft, Select | **6** | 1 |
| `DoctorConsultation` | Create, Read, Update | Cancel, Complete, Create, Read, Update, WriteSoap | **3** | 0 |
| `DoctorQueue` | Read, Update | Call, FinishConsultation, NoShow, Read, Requeue, Skip, StartConsultation | **6** | 1 |
| `PatientAssessment` | Create, Read, Update | Amend, Cancel, Complete, Create, Read, Update | **3** | 0 |
| `PatientDiagnosis` | Create, Read, Update | Cancel, Create, Read, Resolve, SetPrimary, Update | **3** | 0 |
| `PatientVitalSign` | Create, Delete, Read, Update | Cancel, Create, Delete, NotifyDoctor, Read, Update, Verify | **3** | 0 |
| | | **Total** | **24** | **2** |

**Total kunci registry yang wajib ada sebelum Fase B Tahap 1 = 24.**
Seluruh 24 terbukti ada di snapshot source sesudah merge, dan seluruh 24 berstatus
`TIDAK_TERDAFTAR` pada ekspor database. Perubahan bersih registry pada keenam resource pilot:
**+24 − 2 = +22**.

Jumlah baris policy Fase B **tidak berubah**: **35 pelestarian + 1 `Amend` = 36**. `Amend` menambah
identitas tetapi hanya satu baris policy, dan `WriteSoap` sudah ikut terhitung dalam 35 sejak
`evidence/06` bagian F.1.

---

## H. Strategi validasi otorisasi sesudah Tests dihapus

`BillingDepositServiceTests` dan `PatientEncounterCompanyGuarantorTests` **tidak lagi blocker** —
keduanya ikut terhapus bersama seluruh test project. Blocker itu gugur dengan sendirinya.

Yang **tidak** gugur adalah kewajiban membuktikan otorisasi. `evidence/08` mencatat 73 invarian
beserta tuntutan minimumnya. Usulan bentuk penggantinya, dari yang paling murah:

| Tingkat | Bentuk | Menutup | Biaya |
|---|---|---|---|
| **1** | **Verifier baca-saja atas assembly** — satu perintah yang memanggil `BuildFromAssembly` lalu memeriksa: argumen `[AccessPermission]` ↔ `[AccessAction]` cocok; nol endpoint terproteksi tanpa kunci; fallback tetap himpunan yang disetujui; nol identitas ganda; satu resource satu modul; `AccessType` hanya empat nilai | Prioritas 1–3 `evidence/08` | Rendah — sudah terbukti jalan; runner audit ini melakukannya persis |
| **2** | Verifier tingkat 1 dipasang pada `integration-to-dev.yml` yang baru dibawa Integration | idem, otomatis | Rendah |
| **3** | Satu test project **khusus otorisasi** yang sangat kecil — bukan menghidupkan kembali pohon `Tests/` — untuk dua tuntutan yang menuntut database: `ReconcileNeverCreatesAccessPolicy` dan paritas `HasAccessAsync` ↔ `GetEffectivePermissionsAsync` | Prioritas 4–5 | Sedang |

Tingkat 1 **sudah terbukti dapat dijalankan tanpa database dan tanpa menyalakan aplikasi** — audit
ini dan `evidence/07` keduanya memakai jalur itu.

**Belum diimplementasikan.** Bentuk akhirnya keputusan pemilik sistem. Yang perlu disadari: sampai
salah satu tingkat dipasang, pelanggaran kontrak penamaan `[AccessPermission]` ↔ `[AccessAction]`
tidak akan tertangkap apa pun, dan akibatnya adalah **403 permanen yang tidak dapat diperbaiki dari
layar Akses Role**.

---

## I. Batas revalidasi ini

| Hal | Status |
|---|---|
| Build project utama | **Berhasil** — 0 error |
| Snapshot source sesudah merge | **Dijalankan** — identik byte-per-byte dengan sebelum merge |
| Klasifikasi drift | **Dihitung ulang** — tidak berubah |
| 24 kunci target | **Terverifikasi** ada di source, nihil di database |
| Otoritas database | Ekspor 11 September — **masih sah**, database tidak tersentuh |
| Eksekusi test otomatis | **Tidak ada** — seluruh test project sudah dihapus Integration |
| Fase B | **Belum dijalankan** |

Tidak ada aplikasi dijalankan. Tidak ada `AccessMenuSeeder`. Tidak ada database write. Tidak ada
policy expansion. Tidak ada push.
