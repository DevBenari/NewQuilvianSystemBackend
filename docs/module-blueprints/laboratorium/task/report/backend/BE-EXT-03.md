# Laporan Perubahan Backend — `BE-EXT-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-EXT-03` |
| Judul | [Registrasi] Penunjuk perujuk pada kunjungan dan kontrak pemanggilan |
| Slice | `S13a`, `S13b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4, gelombang `MVP-1` |
| Trace | `LAB-DEC-032`, `LAB-DEC-035`, `LAB-COORD-003`, `LAB-COORD-004` — disetujui 2026-09-01; `AC-44`, `AC-45`, `AC-46` bergantung padanya |
| Contract version | `LAB-INT-v1` r3 `INT-05`; `erd/data-dictionary.md` bagian 9b.4 |
| Dependency | `BE-EXT-02` **`SELESAI`**. **Bukan milik Laboratorium** — dikerjakan atas instruksi eksplisit pemilik modul pada sesi ini |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RegistrationManagement`, configuration, migration, project test, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `259d53c`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **`SELESAI` untuk kolom dan kontrak.** Endpoint pemanggilan `INT-05` tetap milik `registration-management` — lihat bagian 6 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RegistrationManagement / Registration` |
| Pemilik dan prefix registry | Prefix `Reg`, lifecycle `ACTIVE / LEGACY`. Persetujuan menambah kolom penunjuk diberikan `andryzainhome` dan `sukmagp` pada 2026-09-01 lewat `LAB-REQ-001` (`LAB-COORD-004`) |
| Keberlakuan | `TOUCHED LEGACY` — `TrxPatientEncounter` sudah ada; dua kolom ditambahkan |
| QBE ID yang berlaku | `QBE-ENT-002`, `QBE-CFG-002` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002` — tidak ada entity baru. `QBE-NAM-001` — nama `TrxPatientEncounter` adalah legacy milik Registrasi dan **tidak** dinamai ulang oleh task ini |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Kunjungan menyimpan **bahwa** pasien dirujuk, tetapi tidak menyimpan **oleh siapa**.

> `TrxPatientEncounter` sudah punya `IsReferral`, `ReferralNumber`, `IsReferralRequired`, dan
> `IsReferralVerified` — seluruhnya penanda dan nomor surat. Tidak ada satu pun ruas yang
> menunjuk instansi maupun dokter perujuknya.

Akibatnya asal rujukan hanya dapat direkonstruksi dari nomor surat, dan laporan dokter pengirim
tidak dapat disusun sama sekali.

---

## 2. Proses bisnis

Tidak ada perilaku Registrasi yang berubah. Yang ditambahkan adalah **tempat menyimpan**
penunjuk perujuk, sehingga jalur pendaftaran pasien laboratorium (`BE-LAB-08`) punya sasaran
untuk mengisinya lewat `INT-05`.

| Keadaan | Yang berlaku |
| --- | --- |
| Kunjungan biasa | Kedua penunjuk kosong. Tetap sah |
| Kunjungan lama | Kedua penunjuk kosong. Tidak ada baris yang menjadi tidak sah |
| Kunjungan rujukan baru | Kedua penunjuk terisi dari data induk perujuk, bukan teks bebas |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../RegistrationManagement/Models/TrxPatientEncounter.cs` | Bertambah `ReferralInstitutionId`, `ReferralDoctorId`, dan dua navigasinya |
| `.../Configurations/HealthServices/TrxPatientEncounterConfiguration.cs` | Dua relasi `Restrict` dan dua index bersyarat |
| `Migrations/20260904072427_AddReferralPointerToPatientEncounter.cs` | **Baru.** Aditif |
| `Migrations/scripts/20260904072427_...sql` dan `README.md` | Skrip idempotent beserta barisnya pada daftar |
| `contracts/integration-contract.md` bagian 2b | Bentuk teknis `INT-05` ditulis |
| `Tests/.../RegistrationManagement/EncounterReferralPointerTests.cs` | **Baru.** Empat uji |

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Tidak ada endpoint Registrasi yang ditambah maupun berubah bentuk. Bentuk permintaan dan jawaban `INT-05` ditulis sebagai kontrak, belum sebagai kode |
| Kontrak integrasi | `LAB-INT-v1` r3 `INT-05` **bertambah bagian bentuk teknis**. Pemetaan setiap ruas permintaan ke kolom `TrxPatientEncounter` kini tertulis |
| Database | **Aditif.** Dua kolom `uuid` boleh kosong, dua foreign key `Restrict`, dua index bersyarat. Dijalankan dua arah pada `QuilvianNewDevYoga` |
| Keamanan/Auth | `NOT APPLICABLE` |

### 3.3 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Keduanya boleh kosong** | Kunjungan yang bukan rujukan memang tidak punya perujuk, dan puluhan ribu kunjungan lama tidak pernah menyimpannya. Mewajibkannya akan membatalkan seluruh data yang sudah ada |
| 2 | **`Restrict`, mengikuti seluruh relasi data induk pada tabel ini** | Instansi atau dokter perujuk yang masih ditunjuk kunjungan tidak boleh terhapus. Yang tidak lagi bekerja sama **dinonaktifkan** lewat `IsActive`, bukan dihapus |
| 3 | **Index-nya bersyarat** | Mayoritas kunjungan bukan rujukan. Index difilter `IS NOT NULL` supaya hanya memuat baris yang benar-benar merujuk |
| 4 | **Endpoint `INT-05` tidak dibuat** | Kartu task menyebut cakupannya "dua kolom penunjuk, ditambah kesepakatan bentuk permintaan dan jawaban". Membangun jalur pendaftaran idempoten beserta penyimpanan kunci idempotensinya adalah pekerjaan pemilik `registration-management`, dan pemakaiannya adalah `BE-LAB-08`. Karena itu butir DoD "idempotensi terbukti lewat uji" **belum** terpenuhi — buktinya menuntut endpoint yang belum ada |
| 5 | **Kontrak ditulis sebagai bentuk, bukan sebagai kesepakatan baru** | `LAB-COORD-003` dan `LAB-COORD-004` sudah ditutup `andryzainhome` dan `sukmagp` pada 2026-09-01. Yang ditulis pada bagian 2b adalah pemetaan ruas ke kolom yang kini benar-benar ada — bukan kesepakatan baru atas nama pihak lain |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Task ini tidak menyentuh satu pun endpoint. Bentuk permintaan dan jawaban
`INT-05` beserta pemetaannya ke kolom `TrxPatientEncounter` ada pada
`contracts/integration-contract.md` bagian 2b.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | `0 Error(s)` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 271, Total: 271` | `PASS` | Naik dari 259 |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 176, Total: 176` | `PASS` | Keluaran perintah |
| Checker QBE `Strict` | `VIOLATION: 0`, `Final result: PASS` | `PASS` | `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |
| Dua kolom ada dan boleh kosong | Terbukti pada model relasional | `PASS` | `Kunjungan_MemilikiDuaPenunjukPerujukYangBolehKosong` |
| Relasi `Restrict` ke kedua data induk | Tepat satu foreign key ke masing-masing, keduanya `Restrict` | `PASS` | `KeduaPenunjuk_BertautKeDataIndukPerujukDenganRestrict` |
| Index bersyarat | Filter `IS NOT NULL` pada keduanya | `PASS` | `KeduaPenunjuk_BerindexHanyaUntukBarisYangBenarBenarMerujuk` |
| Kunjungan rujukan dan kunjungan biasa | Yang rujukan menyimpan instansi dan dokternya; yang biasa tetap sah dengan kedua penunjuk kosong | `PASS` | `KunjunganRujukan_MenyimpanPenunjukInstansiDanDokternya` |
| Migration maju, mundur, maju | `Done.` ketiganya; daftar migration bersih | `PASS` | `dotnet ef database update` terhadap `QuilvianNewDevYoga` |

Uji manual: `NOT FEASIBLE`.

### 5.1 Yang tidak dijalankan, dan alasannya

| Pemeriksaan | Alasan |
| --- | --- |
| Bukti idempotensi `INT-05` | Menuntut endpoint pendaftaran yang belum ada — lihat bagian 3.3 butir 4 |
| Perilaku penolakan Registrasi diteruskan apa adanya | Alasan yang sama |
| 52 uji `IntegrationTests.Postgres` | Terhalang `QUILVIAN_BILLING_TEST_DB` |

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status |
| --- | --- |
| Dua kolom ada | **Terpenuhi** |
| Kontrak `INT-05` disepakati tertulis | **Terpenuhi pada tingkat bentuk.** Pemetaan setiap ruas ke kolom yang benar-benar ada sudah tertulis; endpoint pelaksananya milik `registration-management` |
| Idempotensi terbukti lewat uji | **Belum** — menuntut endpoint yang belum ada |

**Satu butir DoD belum terpenuhi**, dan disebut apa adanya. `AC-44`, `AC-45`, dan `AC-46` juga
belum dapat dibuktikan sebelum jalur pendaftarannya ada.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | Kedua kolom akan tetap kosong sampai ada jalur yang mengisinya. Sampai saat itu, laporan asal rujukan belum dapat disusun walaupun tempat penyimpanannya sudah ada |
| Risiko tersisa | **Rendah untuk schema** — aditif dan boleh kosong, kunjungan yang sudah ada tidak tersentuh. **Perlu diketahui:** `TrxPatientEncounter` adalah tabel yang dipakai hampir seluruh modul, sehingga perubahan apa pun padanya berdampak luas. Yang dilakukan di sini hanya penambahan, tanpa satu pun perubahan pada kolom atau relasi yang sudah ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. Pemilik `registration-management` membangun endpoint `INT-05` beserta penyimpanan kunci idempotensinya. 2. `BE-LAB-08` — pendaftaran pasien laboratorium, yang memanggilnya dan membuktikan idempotensinya. 3. `BE-LAB-07` — katalog laboratorium, yang penahannya sudah dicabut `BE-EXT-01` |
