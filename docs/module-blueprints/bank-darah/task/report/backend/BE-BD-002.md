# Laporan Perubahan Backend — `BE-BD-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-002` |
| Judul | Unit pelayanan dapat dikonfigurasi berwenang memesan darah |
| Slice | `MVP-0` — fondasi master Bank Darah |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/00-delivery-plan.md` §D.1 |
| Trace | `DEC-BD-012` rev 2 · `BD-DOM-18` · `BD-CAP-005` · `contracts/integration-contract.md` §2 (titipan kolom) · `data/data-dictionary.md` §status tabel |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`) |
| Dependency | `G1` approval ✅ · pemilik Master Data — kolom dititipkan pada tabel milik mereka |
| Klasifikasi | `LIGHT` — satu kolom bool pada entity yang sudah ada, satu migration aditif, nol entity baru, nol endpoint baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/MasterData/**`, `Repositories/Configurations/**`, `Migrations/**`, `QuilvianSystemBackend.Tests/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e4972b0` cabang `sukmagp` |
| Tanggal | `2026-09-03` |
| Status | **`SELESAI`** untuk scope task ini. Penegakan `VAL-BD-013` menjadi milik `BE-BD-003`; lihat bagian 6 |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, tidak ada cara menyatakan **unit pelayanan mana yang boleh memesan darah**.
Padahal `DEC-BD-012` sudah memutuskan dua hal sekaligus: MVP dibuka untuk Rawat Inap, IGD, dan
Rawat Jalan; dan daftar itu **tidak boleh dikunci di dalam kode**, karena kewenangan memesan darah
adalah sifat konfigurasi yang berubah mengikuti kebijakan rumah sakit.

Tanpa kolom ini, satu-satunya cara memenuhi keputusan tersebut adalah menuliskan daftar tiga unit
di dalam kode — persis yang dilarang. Akibatnya berlipat: setiap kali rumah sakit menambah unit
yang boleh memesan darah, dibutuhkan perubahan kode, pengujian ulang, dan penyebaran ulang
aplikasi.

**Contoh.** Kamar Operasi mulai membutuhkan darah untuk operasi besar. Dengan daftar yang dikunci
di kode, permintaan sesederhana itu berubah menjadi tiket pengembangan. Dengan penanda konfigurasi,
admin cukup menyalakannya dari layar master unit pelayanan.

---

## 2. Proses bisnis

**Tujuan.** Memberi rumah sakit satu tempat untuk menyatakan unit mana yang berwenang memesan darah,
tanpa perubahan kode.

**Pelaku.** Admin master data unit pelayanan, lewat butir hak akses `ServiceUnit : Update` yang
sudah ada. **Tidak ada butir hak akses baru** — pengelolaannya memang milik Master Data, bukan
milik Bank Darah (`contracts/integration-contract.md` §2).

**Pemicu.** Penyiapan modul Bank Darah, atau perubahan kebijakan unit pemesan di kemudian hari.

**Langkah pada jalur normal:**

1. Admin membuka layar master unit pelayanan dan mencari unit yang dimaksud.
2. Pada bagian **Rule** kini tersedia sakelar **Berwenang Memesan Darah**. Bawaannya **mati**.
3. Admin menyalakannya untuk Rawat Inap, IGD, dan Rawat Jalan, lalu menyimpan lewat `PUT /{id}`.
4. Sejak saat itu ketiga unit terbaca sebagai unit berwenang. Tidak ada penyebaran ulang aplikasi
   yang dibutuhkan.
5. Admin dapat memeriksa hasilnya lewat penyaring `isAvailableForBloodOrder=true` pada daftar unit,
   dan lewat kartu statistik yang kini memuat jumlah unit berwenang memesan darah.

**Aturan yang berlaku:**

| Aturan | Perilakunya |
| --- | --- |
| Bawaan menolak | Unit baru maupun unit lama yang belum dikonfigurasi bernilai **tidak berwenang**. Berlaku di tiga lapisan: nilai bawaan entity, nilai bawaan permintaan API, dan nilai bawaan kolom database |
| Tidak ada daftar unit di kode | Kewenangan hanya berasal dari kolom ini. Tidak ada nama unit, tipe unit, atau kode unit yang ditanam sebagai penentu kewenangan |
| Kewenangan dapat dicabut | Menyalakan dan mematikan sama-sama lewat konfigurasi biasa |
| Unit terhapus tidak berwenang | Unit yang sudah ditandai terhapus tidak terbaca sebagai unit berwenang walaupun penandanya menyala saat dihapus |

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Frontend lama mengirim permintaan tanpa menyebut penanda ini | Unit tetap lahir **tidak berwenang**. Pemanggil lama tidak dapat memberi kewenangan secara tidak sengaja |
| Unit tidak berwenang mencoba membuat order darah | **Belum ditegakkan pada task ini.** Penolakan `VAL-BD-013` adalah milik `BE-BD-003`; lihat bagian 6 |

**Hasil akhir.** Kewenangan memesan darah menjadi sifat konfigurasi yang dapat dilihat, disaring,
dihitung, dan diubah admin — sebagaimana dituntut `DEC-BD-012`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan diperiksa |
| --- | --- |
| `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` | Menetapkan keberlakuan `TOUCHED LEGACY` dan QBE ID |
| `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Memastikan pemilik `MstServiceUnit` adalah Master Data dengan prefix `Mst` yang `ACTIVE` |
| `rules/backend/master-data-endpoint-standard.md` | Memastikan penyaring yang diumumkan metadata benar-benar didukung `GET /` |
| `rules/backend/role-access-rules.md` | Memastikan tidak ada kewenangan yang di-hardcode |
| `docs/module-blueprints/bank-darah/contracts/integration-contract.md` §2 | Bentuk titipan kolom dan batas kepemilikannya |
| `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs` | Pola penanda `IsAvailableFor*` yang sudah ada |
| `Repositories/Configurations/HealthServices/MstServiceUnitConfiguration.cs` | Nilai bawaan dan susunan index yang sudah ada |
| `Areas/HealthServices/MasterData/Controllers/ServiceUnitController.cs` | Enam belas titik sentuh penanda saudara |
| `Areas/HealthServices/MasterData/DTOs/ServiceUnitDtos.cs` | Empat bentuk DTO yang memuat penanda |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs` | Menambah `IsAvailableForBloodOrder` (bool, bawaan `false`) beserta keterangan asal keputusannya |
| `Repositories/Configurations/HealthServices/MstServiceUnitConfiguration.cs` | Menambah `HasDefaultValue(false)` untuk kolom baru. **Nol index diubah** |
| `Areas/HealthServices/MasterData/DTOs/ServiceUnitDtos.cs` | Menambah penanda pada respons daftar, respons detail, penyaring bawaan, dan permintaan create/update; menambah `BloodOrderAvailableServiceUnit` pada ringkasan |
| `Areas/HealthServices/MasterData/Controllers/ServiceUnitController.cs` | Empat belas titik sentuh: pilihan pengurutan, ringkasan, parameter `GET /` dan `GET /options`, dua pemanggilan penyaring, proyeksi daftar, create, update, tanda tangan dan badan penyaring, pengurutan, dua pemetaan respons, keterangan query parameter, dan metadata isian form |
| `Migrations/20260903060228_AddServiceUnitBloodOrderFlag.cs` beserta `.Designer.cs` | **Baru.** Migration penambahan kolom |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui otomatis oleh `dotnet ef` |
| `QuilvianSystemBackend.Tests/HealthServices/MasterData/ServiceUnitBloodOrderFlagTests.cs` | **Baru.** 8 pengujian |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol endpoint baru.** Sembilan endpoint `ServiceUnit` yang sudah ada bertambah satu field dan satu parameter penyaring. Perubahannya **aditif dan kompatibel mundur**: pemanggil lama yang tidak mengirim `isAvailableForBloodOrder` berperilaku persis seperti sebelumnya |
| Database | Satu kolom baru `public."MstServiceUnit"."IsAvailableForBloodOrder"` bertipe `boolean NOT NULL DEFAULT false`. **Nol index dibuat dan nol index diubah**, sehingga migration tetap aditif. **Migration sudah dibuat tetapi BELUM dijalankan** |
| Keamanan/Auth | **Nol butir hak akses baru.** Kolom dikelola lewat `ServiceUnit : Update` yang sudah ada, sesuai batas kepemilikan pada integration contract. Nol pemakaian `IsInRole`, nama peran, nama departemen, jabatan, maupun `UserType` |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Sembilan endpoint milik Master Data berikut **bertambah satu field**
pada muatannya; hak aksesnya tidak berubah.

#### Health Services / Master Data / Service Unit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Kini mengumumkan penyaring `isAvailableForBloodOrder`, pilihan pengurutan `isAvailableForBloodOrder`, dan isian form **Berwenang Memesan Darah** | `ServiceUnit : Read` |
| `GET` | `/summary` | Kini memuat `BloodOrderAvailableServiceUnit` | `ServiceUnit : Read` |
| `GET` | `/` | Kini menerima `isAvailableForBloodOrder` dan mengembalikan penanda itu pada setiap baris | `ServiceUnit : Read` |
| `GET` | `/options` | Kini menerima `isAvailableForBloodOrder` sebagai penyaring pilihan | `ServiceUnit : Read` |
| `GET` | `/{id}` | Kini mengembalikan penanda pada detail | `ServiceUnit : Read` |
| `POST` | `/` | Kini menerima penanda; bila tidak dikirim, bernilai `false` | `ServiceUnit : Create` |
| `PUT` | `/{id}` | Kini menerima penanda | `ServiceUnit : Update` |
| `PATCH` | `/{id}/status` | Tidak berubah | `ServiceUnit : Update` |
| `DELETE` | `/{id}` | Tidak berubah | `ServiceUnit : Delete` |

Contoh penyaringan unit yang berwenang memesan darah:

```http
GET /api/v1/health-services/master-data/service-units?isAvailableForBloodOrder=true&isActive=true
```

**Satu delta kontrak yang perlu diketahui frontend.** Isian form baru disisipkan bersama saudara
`IsAvailableFor*` lainnya pada bagian **Rule** dengan `SortOrder = 10`, sehingga empat isian
sesudahnya bergeser satu: `isQueueRequired` 10→11, `isDoctorRequired` 11→12, `isScreeningRequired`
12→13, `sortOrder` 13→14, `description` 14→15. Menempatkannya di akhir daftar akan memisahkannya
dari kelompok sakelar yang secara makna satu keluarga, dan itu lebih membingungkan admin daripada
pergeseran nomor urut yang memang dibaca dari metadata, bukan ditanam di frontend.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil — `0 Error(s)`, `186 Warning(s)` | `PASS` | Jumlah warning **identik** dengan sebelum task ini; nol warning baru |
| `dotnet ef migrations add AddServiceUnitBloodOrderFlag` | Migration terbentuk | `PASS` | `Migrations/20260903060228_AddServiceUnitBloodOrderFlag.cs` |
| Migration hanya menambah kolom, tidak menyentuh index | Terbukti | `PASS` | Isi migration hanya satu `AddColumn`; `git diff` pada configuration mengembalikan **nol** baris `HasIndex` |
| Snapshot model memuat kolom baru | Terbukti | `PASS` | `Migrations/ApplicationDbContextModelSnapshot.cs` memuat `IsAvailableForBloodOrder` |
| 8 pengujian `ServiceUnitBloodOrderFlagTests` | `Failed: 0, Passed: 8` | `PASS` | Dijalankan bersama 26 pengujian `BE-BD-001`; total **34 lulus** |
| `dotnet test QuilvianSystemBackend.Tests` | **Tidak dapat dijalankan** | `EXISTING / ENVIRONMENT ISSUE` | Kerusakan pre-existing pada `PatientEncounterTestWorld.cs`, sama seperti yang dicatat `BE-BD-001.md` bagian 5. Belum diperbaiki pemiliknya |

**Rincian 8 pengujian:**

| Pengujian | Yang dibuktikan |
| --- | --- |
| `UnitBaru_LahirTanpaKewenanganMemesanDarah` | Lapisan 1 — nilai bawaan entity |
| `PermintaanPembuatanUnit_TanpaMenyebutPenanda_TetapMenolak` | Lapisan 2 — pemanggil lama tidak dapat memberi kewenangan tanpa sengaja |
| `KolomKewenangan_BawaanDatabasenyaMenolak` | Lapisan 3 — nilai bawaan kolom pada model EF, sumber yang sama yang dipakai `dotnet ef` menurunkan migration |
| `PenandaSaudara_NilaiBawaannyaTidakBergeser` | Penjaga regresi: kelima penanda lain nilai bawaannya tidak tergeser |
| `UnitDiberiKewenanganLewatKonfigurasi_NilainyaBertahan` | `AC-BD-015` |
| `KewenanganDapatDicabutKembali` | Kewenangan bukan jalan satu arah |
| `DaftarUnit_DapatDisaringBerdasarkanKewenanganMemesanDarah` | Penyaring yang diumumkan metadata benar-benar bekerja |
| `UnitTerhapus_TidakTerbacaSebagaiUnitBerwenang` | Soft delete dihormati |

Uji manual: `NOT FEASIBLE` — menuntut database PostgreSQL yang sudah dimigrasikan beserta akun
ber-hak-akses; eksekusi database adalah wewenang terpisah yang tidak diminta pada task ini.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Eksekusi migration ke database | Wewenang terpisah |
| Pemeriksaan bahwa baris lama benar-benar terisi `false` setelah migration | Menuntut database nyata. Nilai bawaan kolom sudah dibuktikan lewat metadata model EF, dan migration menuliskan `defaultValue: false` secara eksplisit |
| Smoke test endpoint lewat HTTP | Menuntut aplikasi berjalan dengan database yang sudah dimigrasikan |
| `dotnet test` seluruh solusi | Terhalang kerusakan pre-existing |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-015` — unit diberi kewenangan lewat konfigurasi, tanpa ubah kode | **Terpenuhi** | `UnitDiberiKewenanganLewatKonfigurasi_NilainyaBertahan`, ditambah sakelar **Berwenang Memesan Darah** pada metadata isian form |
| `AC-BD-016` — unit tanpa konfigurasi kewenangan, tidak ada kewenangan bawaan | **Terpenuhi** | Tiga pengujian lapisan bawaan menolak |
| `AC-BD-013` — unit tak dikonfigurasi mencoba membuat order, ditolak `VAL-BD-013` | **Belum terpenuhi — bukan milik task ini** | Penolakan terjadi di jalur pembuatan order darah, dan `BbkBloodOrder` belum ada. Menjadi acceptance `BE-BD-003` |
| DoD — unit tak dikonfigurasi ditolak | **Terpenuhi sebagian** | Sisi data sudah menolak secara bawaan; sisi penegakan menunggu `BE-BD-003` |
| DoD — migration tanpa downtime | **Terpenuhi** | Satu `AddColumn` dengan `defaultValue: false`, nol index dibuat, nol index diubah, nol pengisian data susulan |
| DoD — bawaan menolak | **Terpenuhi** | Tiga lapisan terbukti |

### Catatan tentang `AC-BD-013`

`AC-BD-013` menuntut sebuah **penolakan** yang hanya dapat terjadi ketika ada order darah yang
dicoba dibuat. Entity `BbkBloodOrder` belum ada di source, dan task ini secara eksplisit dilarang
membuatnya. Menuliskan pengujian tiruan yang seolah-olah membuktikannya akan menghasilkan bukti
palsu, jadi kriteria itu **disebut belum terpenuhi apa adanya** dan diteruskan ke `BE-BD-003`.

Yang task ini jamin adalah prasyaratnya: ketika `BE-BD-003` membaca penanda ini, penanda tersebut
sudah ada, bawaannya menolak, dan tidak ada satu pun daftar unit yang dikunci di kode.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 186 warning build, **jumlahnya identik** dengan sebelum task ini. Nol warning baru |
| Masalah yang diketahui | **Nol index untuk penanda baru, dan itu keputusan sadar.** `MstServiceUnit` memasangkan ketiga penanda `IsAvailableFor*` lamanya dalam satu index gabungan. Penanda baru **tidak** dimasukkan ke sana, karena mengubah index gabungan berarti membangun ulang index pada tabel yang sudah berisi data — dan itu membatalkan sifat tanpa-downtime yang diminta DoD. Index tersendiri juga tidak dibuat: `MstServiceUnit` adalah master kecil berisi puluhan baris, sehingga index untuk satu penanda boolean tidak membeli apa pun yang terukur. Bila kelak tabel ini tumbuh besar atau penyaringnya menjadi jalur panas, penambahan index dapat dilakukan sebagai migration tersendiri |
| Risiko tersisa | Migration **belum dijalankan**, sehingga penanda belum tersedia di lingkungan mana pun. Sampai `BE-BD-003` menegakkan `VAL-BD-013`, penanda ini tersimpan tetapi belum menahan apa pun — ia data, bukan gerbang |
| Perubahan sampingan | `Migrations/ApplicationDbContextModelSnapshot.cs` berubah otomatis oleh `dotnet ef migrations add`. Perilaku wajar, bukan suntingan manual |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |
| Langkah berikutnya | 1. Lanjut `BE-BD-014` (master lokasi penyimpanan darah) untuk menuntaskan `MVP-0`. 2. Selesaikan sisa `BE-BD-001` bagian `MstBloodBankReason`. 3. `BE-BD-016` seeder hak akses. 4. Jalankan kedua migration `MVP-0` lewat wewenang eksekusi database yang terpisah. 5. Pemilik Registration Management memperbaiki `PatientEncounterTestWorld.cs` |

```text
 M Areas/HealthServices/MasterData/Controllers/ServiceUnitController.cs
 M Areas/HealthServices/MasterData/DTOs/ServiceUnitDtos.cs
 M Areas/HealthServices/MasterData/Models/MstServiceUnit.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/Configurations/HealthServices/MstServiceUnitConfiguration.cs
?? Migrations/20260903060228_AddServiceUnitBloodOrderFlag.Designer.cs
?? Migrations/20260903060228_AddServiceUnitBloodOrderFlag.cs
?? QuilvianSystemBackend.Tests/HealthServices/MasterData/ServiceUnitBloodOrderFlagTests.cs
?? docs/module-blueprints/bank-darah/task/report/backend/BE-BD-002.md
```

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MasterData` |
| Submodule | Tidak berlaku |
| Pemilik/prefix registry | `Administrator / HealthServices` · `Master / Reference` · prefix **`Mst`** · Lifecycle **`ACTIVE`** |
| Keberlakuan | **`TOUCHED LEGACY`** — `MstServiceUnit` dan `ServiceUnitController` adalah kode existing. Kolom baru itu sendiri `NEW CODE` |
| Status registry | Terdaftar dan `ACTIVE`. Nol entri registry baru dibutuhkan; kolom menempel pada entity yang pemiliknya sudah tercatat |

**QBE ID yang berlaku dan cara pemenuhannya:**

| QBE ID | Pemenuhan |
| --- | --- |
| `QBE-ENT-002` | Kolom bool non-nullable dengan bawaan menolak, mengikuti semantik `DEC-BD-012` |
| `QBE-ENT-003` | Kolom ini bukan kebutuhan presentasi; ia menyimpan kewenangan bisnis yang dibaca modul lain |
| `QBE-NAM-002`, `QBE-NAM-004` | Nol prefix baru; kolom menempel pada entity `Mst*` yang sudah terdaftar |
| `QBE-CFG-002` | Configuration diperbaiki dalam cakupan — hanya menambah nilai bawaan kolom baru, tanpa menyentuh mapping lain |
| `QBE-MOD-001` | Kolom ditempatkan pada Area/Module pemiliknya, yaitu Master Data — bukan disalin ke folder Bank Darah |
| `QBE-API-001` | Bentuk response dan pembungkus `ApiResponse<T>` tidak berubah |
| `QBE-PERM-001` | Memakai butir hak akses `ServiceUnit` yang sudah ada; nol butir baru |
| `QBE-VAL-001` | Tidak ada validasi baru yang dibutuhkan — kolom boolean tanpa kombinasi terlarang. Validasi kewenangan yang sesungguhnya (`VAL-BD-013`) hidup di jalur order darah, milik `BE-BD-003` |
| `QBE-PAGE-001` | Penyaring baru mengikuti pola penyaring yang sudah mapan pada controller yang sama |
| `QBE-DEL-001` | Soft delete dihormati; unit terhapus tidak terbaca sebagai unit berwenang |
| `QBE-AUD-001` | Audit database (`IdentityModel`) tidak berubah |

**QBE ID yang TIDAK berlaku, beserta alasannya:**

| QBE ID | Alasan tidak berlaku |
| --- | --- |
| `QBE-ENT-001` | Tidak ada entity baru; `MstServiceUnit` sudah mewarisi `IdentityModel` |
| `QBE-CFG-001` | Configuration sudah ada; task ini hanya menambahkan satu baris nilai bawaan |
| `QBE-MOD-002`, `QBE-MOD-003` | Tidak ada modul atau folder baru yang memuat model persisted |
| `QBE-SVC-001` | **Dicatat sebagai temuan, bukan diperbaiki.** `ServiceUnitController` memakai `ApplicationDbContext` secara langsung — pola legacy yang mendahului kontrak ini. Memindahkannya ke Module Service adalah penulisan ulang 1.200-an baris di luar cakupan task, dan legacy ratchet melarang refactor massal terhadap legacy yang tidak diminta. Penambahan pada task ini mengikuti pola berkas itu apa adanya |
| `QBE-CODE-001`..`006` | Nol nomor bisnis dialokasikan |
| `QBE-DTO-001` | Entity EF tetap tidak diekspos; seluruh muatan lewat DTO yang sudah ada |
| `QBE-TXN-001` | Satu kolom pada satu baris; tidak ada konsistensi lintas record |
| `QBE-ENUM-001` | Nol enum baru |
| `QBE-LOG-001` | Pencatatan perubahan unit pelayanan sudah ada dan tidak berubah |
| `QBE-OPT-001` | `/options` sudah ada; task ini hanya menambah satu penyaring padanya |
| `QBE-NAM-001`, `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` | Nol `Trx*`, dan task ini bukan `LEGACY MIGRATION` |
