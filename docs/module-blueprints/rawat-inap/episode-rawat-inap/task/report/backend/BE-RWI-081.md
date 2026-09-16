# Laporan Perubahan Backend — `BE-RWI-081`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-081` |
| Judul | Census dokter dari penugasan aktif |
| Slice | Gelombang 2 — `RI-V2-1`, `EPIC RI-38` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-081` |
| Trace | `FR-RI-191`, `FR-RI-192`, `FR-DOK-070`; `RWI-DEC-111`; `INV-INP-13`; `NFR-026`; `contracts/api-contract.md` `0.9.0` bagian 10.1; `02-backend-architecture.md` 11.5.6 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Dependency | `BE-RWI-079` — **selesai di source** pada sesi yang sama (index `IX_InpDoctorAssignment_DoctorId_Active` dan kolom `AssignmentPurpose`) |
| Klasifikasi | `MEDIUM` — satu penyaring baru, tiga field balasan baru, dua metode service diperluas |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**`, `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-16 |
| Status | **SELESAI, dengan satu bagian di luar acceptance criteria yang belum penuh.** `dotnet build` `0 Error(s)`. `NeedsReviewCount` baru mencakup satu dari dua sumber yang dirancang — lihat bagian 6. Rencana eksekusi query `NFR-026` **`NOT RUN`** |

---

## 1. Masalah yang diperbaiki

Rumah sakit merawat 122 pasien rawat inap. dr. Ahmad bertanggung jawab atas dua di antaranya.
Sampai hari ini, layar census yang ia buka menampilkan **seluruh 122 pasien** — atau, kalau ia
menyaringnya sendiri lewat `doctorId`, menampilkan pasien siapa pun yang identifier-nya ia kirim.

Ada dua masalah di situ, dan yang kedua jauh lebih serius daripada yang pertama:

1. **Merepotkan.** Dokter harus mencari dua barisnya sendiri di antara 122 baris.
2. **Membuka data pasien yang bukan urusannya.** Penyaring `doctorId` diambil dari parameter yang
   dikirim peramban. Akun dr. Ahmad yang mengirim `doctorId` milik dr. Rina akan menerima daftar
   pasien dr. Rina — lengkap dengan nama, nomor rekam medis, dan lokasi tempat tidurnya.

Task ini menutup keduanya dengan satu aturan: **identitas dokter diambil dari akun login, bukan dari
parameter.**

---

## 2. Proses bisnis

**Tujuan.** Daftar pasien seorang dokter memuat persis pasien yang boleh ia tulis saat ini — tidak
lebih, dan tidak kurang.

**Pelaku.** Dokter yang sedang masuk ke sistem.

**Pemicu.** Dokter membuka layar "Pasien Saya".

**Langkah yang berurutan.**

1. Layar memanggil `GET /census?assignedToMe=true`.
2. Backend membaca identitas dokter dari **klaim akun** yang terautentikasi, bukan dari parameter.
3. Backend menyaring census menjadi episode yang punya penugasan aktif milik dokter itu.
4. Ringkasan `totalPatients` dihitung dari **daftar yang sama**, bukan dari query terpisah.
5. Setiap baris membawa `myAssignmentRole` dan `myAssignmentPurpose` — peran dan tujuan penugasan
   **pemanggil** atas pasien itu.

**Aturan yang berlaku — `INV-INP-13`.** Penugasan dinilai aktif bila
`StartDateTime <= sekarang` **dan** (`EndDateTime` kosong **atau** `EndDateTime > sekarang`).
Penilaiannya terjadi **pada saat query dijalankan**, tanpa satu pun proses latar.

**Contoh berangka.** dr. Yoga jaga 22.00–07.00 atas pasien Joko.

| Waktu query | Joko muncul? | Kenapa |
| --- | :---: | --- |
| 21.59 | Tidak | `StartDateTime` belum lewat |
| 22.00 | Ya | `StartDateTime <= sekarang` |
| 06.59 | Ya | `EndDateTime` 07.00 masih lebih besar dari sekarang |
| 07.01 | Tidak | `EndDateTime` sudah lewat |

Tidak ada yang perlu dijalankan di antara 06.59 dan 07.01. Daftarnya berubah sendiri karena
keaktifannya dihitung, bukan disimpan.

**Seluruh peran ikut, bukan DPJP saja.** Konsulen dan dokter jaga boleh menulis catatan klinis.
Daftar yang hanya memuat pasien DPJP akan menyembunyikan justru pasien yang sedang mereka tangani —
`permission-audit-matrix.md` bagian 4-A.1.

**Contoh.** dr. Ahmad DPJP Budi, dan konsulen atas Sari yang DPJP-nya dr. Rina. Daftarnya memuat
**dua** baris. Pada baris Sari, kolom DPJP tetap menyebut "dr. Rina", sedangkan
`myAssignmentRole` bernilai `2` (konsulen). Keduanya benar dan tidak saling menggantikan.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Akun tidak terhubung dengan data dokter | `200` dengan daftar **kosong** dan `emptyReason` "Akun Anda tidak terhubung dengan data dokter." — **bukan** `403` |
| `doctorId` dikirim bersama `assignedToMe=true` | Diabaikan seluruhnya; daftar tetap milik dokter login |
| `assignedToMe=false` atau tidak dikirim | Census unit berperilaku persis seperti sebelumnya; `myAssignmentRole`, `myAssignmentPurpose`, dan `needsReviewCount` bernilai kosong |

**Kenapa akun tanpa data dokter tidak dijawab `403`.** Hak baca census-nya sah; yang tidak ada
adalah kaitan akun itu dengan seorang dokter — itu masalah data induk, bukan kewenangan. Menjawabnya
`403` menyamarkan masalah data induk menjadi masalah hak akses, dan petugas yang menghadapinya akan
meminta hak akses yang sebenarnya sudah ia punya.

**Bagaimana `doctorId` "diabaikan" secara teknis.** Kedua penyaring ditulis sebagai satu
`if` / `else if`, bukan dua `if` sejajar. Ketika `assignedToMe` menyala, cabang `doctorId` **tidak
pernah dijalankan** — parameter itu tidak ditolak, tidak diperiksa, dan tidak pernah sampai ke
query.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `contracts/api-contract.md` `0.9.0` bagian 10.1 | Bentuk query dan field balasan yang mengikat |
| `.../02-backend-architecture.md` 11.5.6 | Perilaku `AssignedToMe`, sumber identitas dokter, isi `NeedsReviewCount` |
| `Areas/.../Services/InpCensusQueryService.cs` | `BuildCensusQuery`, proyeksi census, ringkasan |
| `Areas/.../Helpers/InpatientActorClaims.cs` | Cara membaca `doctor_id` dari klaim |
| `Models/ApplicationUser.cs` | Keberadaan kolom `DoctorId` yang menjadi asal klaim |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs` dan `Enums/CpptVerificationStatus.cs` | Sumber angka CPPT yang menunggu verifikasi |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../DTOs/InpatientCensusDtos.cs` | `CensusQuery` bertambah `AssignedToMe`; `CensusItemResponse` bertambah `MyAssignmentRole` dan `MyAssignmentPurpose`; `CensusPagedResult` bertambah `EmptyReason`; `CensusSummaryResponse` bertambah `NeedsReviewCount` |
| `Areas/.../Services/InpCensusQueryService.cs` | `GetCensusAsync` dan `GetCensusSummaryAsync` menerima `currentDoctorId`; `BuildCensusQuery` menerima `currentDoctorId` dan waktu penilaian; penyaring `assignedToMe`; konstanta `AkunTanpaDataDokter`; perhitungan `NeedsReviewCount` |
| `Areas/.../Controllers/InpatientCensusController.cs` | Meneruskan `User.GetDoctorId()` ke kedua endpoint; pesan balasan menyebut `emptyReason` bila terisi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Satu query baru, empat field balasan baru. Perilaku tanpa `assignedToMe` tidak berubah sama sekali |
| Database | Tidak ada perubahan schema. Task ini **memakai** index `IX_InpDoctorAssignment_DoctorId_Active` dan kolom `AssignmentPurpose` dari `BE-RWI-079`. Keduanya terbukti lahir pada container Postgres sekali pakai, tetapi migration `E1` belum diterapkan ke database dev; sampai itu terjadi, jalur `assignedToMe` gagal pada runtime di sana |
| Keamanan/Auth | **Ada, dan ini inti task.** Identitas dokter diturunkan dari klaim akun, tidak pernah dari parameter permintaan. Tidak ada perubahan pada atribut akses; `InpatientCensus : Read` tetap |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Census

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/census` | Daftar pasien dirawat. **Berubah:** `assignedToMe=true` menyaring menjadi pasien penugasan aktif dokter login | `InpatientCensus : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/census/summary` | Ringkasan dari daftar yang sama. **Berubah:** bertambah `needsReviewCount` bila `assignedToMe=true` | `InpatientCensus : Read` |

```yaml
GET /api/v1/health-services/inpatient-management/census:
  summary: Daftar pasien rawat inap, dapat disaring penugasan dokter login
  x-permission: "InpatientCensus : Read"
  parameters:
    - { name: assignedToMe, in: query, schema: { type: boolean, default: false } }
    - { name: doctorId,     in: query, schema: { type: string, format: uuid },
        description: "DIABAIKAN seluruhnya bila assignedToMe=true — FR-DOK-070" }
    - { name: pageNumber,   in: query, schema: { type: integer, default: 1 } }
    - { name: pageSize,     in: query, schema: { type: integer, default: 25 } }
  responses:
    "200":
      description: Daftar pasien beserta ringkasan yang dihitung dari daftar yang sama
      content:
        application/json:
          schema:
            type: object
            properties:
              items:
                type: array
                items:
                  type: object
                  properties:
                    myAssignmentRole:    { type: integer, nullable: true, description: "1 Dpjp, 2 Consultant, 3 OnCallDoctor" }
                    myAssignmentPurpose: { type: integer, nullable: true, description: "0 Regular, 1 LateDocumentation" }
              totalData:   { type: integer }
              emptyReason: { type: string, nullable: true, description: "Terisi bila akun tidak punya data dokter — FR-RI-192" }
```

**Delta kontrak yang dicatat.**

| Hal | Kontrak `0.9.0` 10.1 | Yang dibuat | Alasan |
| --- | --- | --- | --- |
| Nama field jumlah pasien | `TotalCount` pada contoh | `totalData` | `PagedResult<T>` repository ini sudah memakai `TotalData`, dan seluruh layar Rawat Inap membacanya. Menambah nama kedua untuk angka yang sama akan melahirkan dua sumber yang harus disamakan |
| Alasan daftar kosong | Kontrak menaruhnya pada `Message` | `emptyReason` **dan** `Message` | Keduanya diisi. `emptyReason` menempel pada datanya sehingga tetap terbaca bila layar hanya membaca badan `data`; `Message` diisi nilai yang sama agar kontrak tetap terpenuhi |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | **`0 Error(s)`, `211 Warning(s)`, `Time Elapsed 00:04:31.02`** | `PASS` | Dijalankan 16 September 2026 pada commit `36db5e6d`. Nol `error CS` |
| Index `IX_InpDoctorAssignment_DoctorId_Active` benar-benar lahir di database | `CREATE INDEX "IX_InpDoctorAssignment_DoctorId_Active" ON public."InpDoctorAssignment" USING btree ("DoctorId", "EndDateTime") WHERE ("IsDelete" = false)` | `PASS` | Dibaca dari `pg_indexes` pada container Postgres 16 sekali pakai |
| **Rencana eksekusi query menunjukkan index itu dipakai (`NFR-026`)** | Tidak dijalankan | `NOT RUN` | Menuntut tabel berisi data dalam jumlah nyata. Pada tabel kosong, PostgreSQL memilih `Seq Scan` apa pun index-nya, sehingga `EXPLAIN` di sana **tidak membuktikan apa pun** — dan menuliskannya sebagai bukti justru menyesatkan |
| Uji batas 06.59 dan 07.01 | Tidak dijalankan | `NOT RUN` | Menuntut aplikasi berjalan beserta data penugasan berperiode |
| Verifikasi kontrak API terhadap `api-contract.md` `0.9.0` 10.1 | Query, field balasan, dan perilaku `doctorId` dibandingkan baris per baris. Dua selisih penamaan ditemukan dan dicatat | `PASS` dengan delta tercatat | Tabel "Delta kontrak yang dicatat" |
| Pemeriksaan statis — index yang dituju `NFR-026` benar-benar ada | Index `IX_InpDoctorAssignment_DoctorId_Active` atas `(DoctorId, EndDateTime)` berfilter `"IsDelete" = false` dibuat `BE-RWI-079`, dan penyaring `assignedToMe` menyaring persis pada `DoctorId` lalu menilai `EndDateTime` | `PASS` | `InpDoctorAssignmentConfiguration.cs` dan `BuildCensusQuery` |
| Regresi census unit tanpa `assignedToMe` | Cabang `assignedToMe` tidak pernah dijalankan ketika nilainya salah; `BuildCensusQuery` mengembalikan query yang sama persis seperti sebelumnya; tiga field baru bernilai kosong | `PASS` pemeriksaan source | `git diff` `InpCensusQueryService.cs` — tidak ada satu pun penyaring lama yang berubah |
| QBE Backend Governance Preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` `ACTIVE`. Keberlakuan `TOUCHED LEGACY` | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Review diff dan scope | Tiga berkas disentuh, seluruhnya di dalam `InPatientManagement` | `PASS` | `git diff` |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Catatan cara build.** Perintahnya memakai `-m:1`, `-p:BuildInParallel=false`, `-p:UseSharedCompilation=false`, dan `-p:RunAnalyzers=false` atas permintaan pemilik pekerjaan supaya build tidak membebani mesin. Solution ini kini hanya memuat satu project — folder `Tests/` sudah tidak ada — sehingga build penuh selesai 4 menit 31 detik.

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta data pasien dan penugasan.

**Tidak dijalankan:** pengambilan rencana eksekusi query dan uji batas waktu 06.59/07.01. Keduanya menuntut database berisi data dalam jumlah nyata. **Rencana eksekusi query adalah bukti yang diminta kartu task secara eksplisit untuk `NFR-026`, dan ketiadaannya disebut di sini apa adanya** — yang sudah terbukti barulah bahwa index-nya lahir dan bahwa penyaringnya menyaring persis pada kolom yang diindeks.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — `assignedToMe=true` hanya mengembalikan pasien yang punya penugasan **aktif** milik dokter login | Terpenuhi di source | Penyaring pada `BuildCensusQuery`: `d.DoctorId == currentDoctorId && d.StartDateTime <= evaluatedAt && (d.EndDateTime == null || d.EndDateTime > evaluatedAt)` |
| AC-2 — `doctorId` yang dikirim bersama `assignedToMe=true` diabaikan seluruhnya | Terpenuhi di source | Struktur `if` / `else if`; cabang `doctorId` tidak pernah dijalankan ketika `assignedToMe` menyala |
| AC-3 — Ringkasan dihitung dari daftar yang sama, bukan dari query terpisah | Terpenuhi di source | `GetCensusSummaryAsync` memanggil `BuildCensusQuery` dengan argumen yang sama; `NeedsReviewCount` memakai `filtered.Select(x => x.EpisodeId)` sebagai subquery, bukan penyaring yang disalin ulang |
| AC-4 — Akun tanpa data dokter menerima `200` dengan daftar kosong dan `emptyReason` terisi — bukan `403` | Terpenuhi di source | Penjaga di awal `GetCensusAsync` mengembalikan `CensusPagedResult` kosong berisi `EmptyReason`; tidak ada jalur yang menghasilkan `403` |
| AC-5 — Keaktifan dinilai saat query dijalankan, tanpa proses latar; uji batas 06.59 dan 07.01 membedakan hasilnya | **Terpenuhi di source, belum terbukti runtime** | `evaluatedAt = DateTime.UtcNow` diambil pada setiap pemanggilan dan diteruskan ke penyaring; tidak ada hosted service maupun kolom turunan di seluruh jalur ini. Uji batasnya `NOT RUN` |

**Butir yang belum penuh.**

`NeedsReviewCount` dirancang `02-backend-architecture.md` 11.5.6 sebagai **penjumlahan dua sumber**:
entri CPPT yang menunggu verifikasi dokter itu, **ditambah** pesanan tindakan yang menunggu
verifikasi instruksinya. Yang terpasang baru bagian pertama. Bagian kedua dibaca lewat
`PatientProcedureOrderService` milik `ClinicalManagement`, yang dibuat `BE-RWI-097` pada sub-modul
`dokter-rawat-inap` dan **belum mendarat**. Bagian Lab/Radiologi memang belum masuk cakupan dan
menunggu persetujuan pemiliknya. Angkanya dihitung dari sumber yang benar-benar tersedia dan tidak
pernah ditebak; keterbatasannya ditulis pada dokumentasi field itu sendiri.

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Regresi census unit — daftar per unit tanpa `assignedToMe` — tetap sama seperti sebelumnya | Terpenuhi pada pemeriksaan source; **belum terbukti runtime** |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol `error CS`. `InpCensusQueryService.cs` tidak menambah satu pun warning baru |
| Masalah yang diketahui | `NeedsReviewCount` baru mencakup satu dari dua sumber — lihat bagian 6 |
| Risiko tersisa | **Dua.** (1) Jalur `assignedToMe` bersandar pada kolom `AssignmentPurpose` yang sudah terbukti lahir pada container sekali pakai tetapi belum diterapkan ke database dev; sampai itu terjadi, proyeksi `myAssignmentPurpose` gagal pada runtime di sana. (2) **`NFR-026` belum terbukti.** Index-nya ada dan penyaringnya menyaring pada kolom yang tepat, tetapi tanpa rencana eksekusi pada tabel berisi data nyata, tidak ada yang memastikan perencana query benar-benar memakainya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-086`. Branch `MHamzah`, upstream `origin/MHamzah`. Tidak ada operasi Git yang dilakukan |
| Langkah berikutnya | Setelah migration `E1` diterapkan ke database dev yang berisi data, ambil rencana eksekusi query `assignedToMe` lalu tempelkan ke bagian 5. Lengkapi `NeedsReviewCount` ketika `BE-RWI-097` mendarat |
