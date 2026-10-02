# Laporan Perubahan Backend — `PLT-BE-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PLT-BE-003` |
| Judul | Alokator nomor yang pencacahnya bertahan |
| Slice | `PLT-SLICE-01` — gelombang `MVP-1`. **Inti slice ini** |
| Roadmap | `docs/module-blueprints/platform/roadmap/backend-roadmap.md` §5 |
| Trace | `FR-PLT-001`..`FR-PLT-007` · `DEC-PLT-004`, `DEC-PLT-005`, `DEC-PLT-007`, `DEC-PLT-008` · `INV-PLT-001`..`004` · `CONF-PLT-002` · `VAL-PLT-001`..`007` |
| Contract version | `v1` — ✅ **`approved`** (`Sukma Giri Pratama` / `2026-09-09`) |
| Dependency | `P0` ✅ · `P1` ✅ · `PLT-BE-001` ✅ · `PLT-BE-002` ✅ |
| Klasifikasi | `HEAVY` — menyentuh komposisi aplikasi, membalik perilaku durabilitas pencacah, risiko klinis tidak ada tetapi risiko data tinggi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Platform/**`, `Tests/**`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `f0d6855` cabang `sukmagp` |
| Tanggal | `2026-09-09` |
| Status | **`SELESAI`**. Ketujuh acceptance criteria task ini **terbukti**. Durabilitas dan antrean memang **bukan** kriteria task ini — kartu roadmap menugaskannya ke `PLT-BE-004` |

---

## 1. Masalah yang diperbaiki

`PLT-BE-002` sudah membangun tempat menyimpan pencacah, tetapi **tidak ada yang menaikkannya**.
Tanpa alokator, tabel `NumNumberSeries` hanyalah tabel kosong yang tidak pernah diisi siapa pun.

Akibatnya: gerbang `G4` modul Bank Darah tetap tertutup, dan sembilan task backend di sana tetap
tertahan.

**Masalah kedua yang lebih halus, dan inilah yang benar-benar dipecahkan task ini.** Mesin nomor
yang sudah ada — `BillingNumberSeriesService` — menaikkan pencacahnya **di dalam transaksi
pemanggil**. Ketika pekerjaan bisnis dibatalkan, kenaikan itu ikut dibatalkan, dan nomor yang sama
terbit lagi pada percobaan berikutnya (`CONF-PLT-002`).

**Kenapa itu berbahaya.** Petugas mungkin sudah sempat melihat nomor itu di layar, mencatatnya di
kertas, atau menyebutkannya lewat telepon sebelum pekerjaannya batal. Menerbitkannya lagi untuk
catatan lain membuat satu nomor menunjuk dua hal pada ingatan orang — walaupun di database hanya
ada satu (`INV-PLT-001`).

---

## 2. Proses bisnis

### 2.1 Yang terjadi saat satu nomor diminta

| No | Langkah | Pelaku | Keterangan |
| ---: | --- | --- | --- |
| 1 | Service modul memanggil alokator | Modul pemanggil | Membawa penanda deret, awalan, jumlah digit, kebijakan pengulangan miliknya sendiri |
| 2 | Parameter diperiksa | Alokator | Ditolak sebelum satu baris pun disentuh |
| 3 | Periode dihitung | Alokator | `GLOBAL` untuk deret baru |
| 4 | **Koneksi dan transaksi sendiri dibuka** | Alokator | Terpisah dari transaksi pemanggil |
| 5 | Antre pada deret + periode | PostgreSQL | `pg_advisory_xact_lock` |
| 6 | Pencacah dinaikkan lalu **di-`commit`** | Alokator | **Bertahan walau langkah 8 batal** |
| 7 | Nomor jadi diserahkan | Alokator | `BDO-00000123` |
| 8 | Modul menyimpan catatannya | Modul pemanggil | Berhasil **atau batal** |

### 2.2 Jalur tidak normal — yang paling perlu dipahami pemilik

> Petugas membuat order darah. Alokator menerbitkan **`BDO-00000123`**. Validasi bisnis kemudian
> menolak order itu, dan seluruh pekerjaan penyimpanan dibatalkan. Order tidak jadi tersimpan.
>
> Petugas memperbaiki lalu menyimpan lagi. Order kali ini tersimpan dengan **`BDO-00000124`** —
> bukan `123`.
>
> **Nomor `BDO-00000123` hilang selamanya.** Deret berlubang di posisi 123, dan lubang itu
> **tidak pernah** diisi.

Ini perilaku yang **benar** menurut `DEC-PLT-008`, bukan cacat. Tetapi ia **akan terlihat** oleh
petugas yang jeli, dan karena itu perlu diketahui pemilik proses sebelum rilis.

### 2.3 Jalur tidak normal lain

| Keadaan | Akibat | Kode |
| --- | --- | --- |
| Penanda deret kosong | Ditolak; nol nomor terbit | `VAL-PLT-001` |
| Awalan kosong atau lebih dari 15 karakter | Ditolak | `VAL-PLT-002` |
| Kebijakan pengulangan di luar empat nilai sah | Ditolak | `VAL-PLT-003` |
| Jumlah digit di luar 4–12 | Ditolak | `VAL-PLT-004` |
| Pelaku tidak dikenali | Ditolak | `VAL-PLT-005` |
| Pencacah melampaui batas bilangan | Ditolak | `VAL-PLT-006` |
| Nilai tidak muat pada jumlah digit | **Ditolak, bukan diterbitkan menyimpang** | `VAL-PLT-007` |

**Setiap penolakan terjadi sebelum `commit`**, sehingga nol nomor terbit dan deret **tidak**
berlubang karena permintaan yang ditolak. Lubang hanya lahir ketika nomor sudah terbit lalu
pekerjaan bisnisnya dibatalkan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` · `BACKEND_ENGINEERING_CONTRACT.md` · `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` · `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |
| Kontrak modul | `platform/02-backend-architecture.md` §B · `platform/contracts/integration-contract.md` · `platform/contracts/validation-matrix.md` · `platform/contracts/state-transition-matrix.md` · `platform/roadmap/backend-roadmap.md` |
| Mesin yang diekstrak | `BillingNumberSeriesService.cs` — algoritma diambil utuh dari `AllocateNumberAsync` |
| Komposisi | `Program.cs` baris 157–162 (`AddDbContext`), 165+ (`AddIdentity` + `AddEntityFrameworkStores`) |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Platform/NumberSeriesManagement/Services/NumberSeriesAllocator.cs` | **Baru.** Inti slice ini |
| `Areas/Platform/NumberSeriesManagement/Services/NumberAllocationRequest.cs` | **Baru.** Record enam parameter; empat di antaranya milik modul pemanggil |
| `Areas/Platform/NumberSeriesManagement/Services/NumberSeriesAllocationException.cs` | **Baru.** Membawa `ValidationCode` supaya pemanggil tidak mencocokkan teks pesan |
| `Program.cs` | `AddDbContextFactory<ApplicationDbContext>` **lifetime `Scoped`** + `AddScoped<NumberSeriesAllocator>` + satu `using` |
| `Tests/.../Platform/NumberSeriesAllocatorTests.cs` | **Baru.** 19 pengujian |
| `Tests/.../Platform/NumberSeriesCompositionTests.cs` | **Baru.** 3 pengujian penjaga komposisi DI |

**Nol berkas Billing disentuh.** `BillingNumberSeriesService` dan `BilNumberSeries` tetap seperti
adanya (`INV-PLT-003`, `DEC-PLT-003`).

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — **nol endpoint**, dan itu keputusan kontrak. Nomor yang dapat diminta lewat HTTP dapat terbit tanpa catatan yang menempel padanya (`api-contract.md` §1) |
| Database | **Nol perubahan schema.** Nol tabel, nol kolom, nol index, nol migration. Task ini hanya menulis ke tabel yang dibuat `PLT-BE-002` |
| Keamanan/Auth | `NOT APPLICABLE` — nol endpoint, nol butir hak akses. Alokasi **sengaja tidak** dijaga butir tersendiri: ia langkah di dalam pekerjaan yang sudah dijaga butir milik modul pemanggil (`permission-audit-matrix.md` §3) |
| Komposisi aplikasi | **Satu-satunya sentuhan pada komposisi yang sudah berjalan** — lihat bagian 4 |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | **`Platform`** / `NumberSeriesManagement` |
| Submodule | `NOT APPLICABLE` |
| Pemilik / prefix registry | `NumberSeriesManagement / Number Series` → **`Num`** |
| Status registry | ✅ **`ACTIVE`** — diverifikasi ulang, `Resolved: True` |
| Keberlakuan | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-001/002/003` · `QBE-NAM-001/002/004` · **`QBE-CODE-001`** service memiliki kebutuhan dan format kode, alokasinya deterministik dan aman di database · **`QBE-CODE-002`** nol controller mengalokasikan nomor · **`QBE-CODE-003`** nol `Count+1`/`Max+1`/counter lokal · **`QBE-CODE-006`** provider bersama mendukung alokasi atomik ber-scope yang durabel |
| `QBE-SVC-001` | **Belum berlaku** — task ini nol controller. Alokator adalah service yang di-`inject` ke service lain |

**Ketiganya diverifikasi, bukan diasumsikan:**

```text
QBE-MOD-002  : Resolve-RegistryOwnership → Resolved: True
QBE-CODE-003 : pemindaian Areas/Platform untuk Count()+1, Max(, .Last(, OrderByDescending
               → BERSIH, nol temuan
QBE-CODE-002 : pemindaian seluruh *Controller.cs untuk NumberSeriesAllocator
               → BERSIH, nol controller memanggil alokator
```

---

## 4. Kompatibilitas komposisi — diperiksa lebih dulu, bukan ditambal belakangan

Blueprint menandai `AddDbContextFactory` berdampingan dengan `AddDbContext` sebagai **risiko yang
wajib diverifikasi** (`02-backend-architecture.md` §H), dan instruksi task memerintahkan berhenti
bila ditemukan konflik. Karena itu pemeriksaannya dikerjakan **sebelum** satu baris alokator
ditulis.

**Hasilnya: nol konflik.** Dibuktikan `NumberSeriesCompositionTests`, 3 pengujian lulus dengan
`validateScopes: true` aktif:

| Yang dibuktikan | Hasil |
| --- | --- |
| Keduanya terdaftar bersamaan dan tetap dapat diselesaikan container | ✅ |
| Konteks dari factory adalah **instance berbeda** dari konteks scoped | ✅ |
| Konteks dari factory benar-benar dapat membaca model | ✅ |

**Satu keputusan teknis yang perlu disadari.** Factory didaftarkan dengan
**`lifetime: ServiceLifetime.Scoped`**, bukan `Singleton` yang menjadi bawaannya. Alasannya:
`AddDbContextFactory` mendaftarkan `DbContextOptions<ApplicationDbContext>` dengan lifetime yang
sama, sehingga bawaan `Singleton` akan berdampingan dengan pendaftaran `Scoped` milik
`AddDbContext` — dua pendaftaran untuk satu tipe layanan, dan yang terakhir menang. `Scoped`
membuat keduanya konsisten dan lolos `validateScopes`.

Lifetime factory **tidak** memengaruhi tujuannya: yang dibutuhkan alokator adalah konteks
**baru** setiap kali, dan itu datang dari `CreateDbContextAsync`, bukan dari lifetime factory-nya.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | `0 Error(s)`, **`210 Warning(s)`** | `PASS` | Sama persis baseline — **nol peringatan baru** |
| `NumberSeriesAllocatorTests` | `Failed: 0, Passed: 19` | `PASS` | Keluaran perintah |
| `NumberSeriesCompositionTests` | `Failed: 0, Passed: 3` | `PASS` | Dijalankan dengan `validateScopes: true` |
| `dotnet test` — `UnitTests.Sqlite` | `Failed: 0, Passed: 209` | `PASS` | Naik dari 187; selisih 22 adalah uji baru task ini |
| `dotnet test` — `QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 419` | `PASS` | Tidak berubah |
| `dotnet test` — `UnitTests.InMemory` | `Failed: 9, Passed: 896` | `EXISTING / ENVIRONMENT ISSUE` | Jumlah dan nama identik baseline; seluruhnya `BillingManagement` |
| `QBE-MOD-002` | `Resolved: True` | `PASS` | Dijalankan terhadap checker |
| `QBE-CODE-002` / `QBE-CODE-003` | Nol temuan | `PASS` | Pemindaian source dijalankan |
| **`AC-PLT-003` durabilitas** | **Tidak dijalankan** | **`NOT RUN`** | **Menuntut PostgreSQL — lihat di bawah** |
| **`AC-PLT-004`/`005` antrean** | **Tidak dijalankan** | **`NOT RUN`** | **Menuntut PostgreSQL — lihat di bawah** |
| `AC-PLT-012` empat deret Billing | Tidak dijalankan | `NOT RUN` | Lingkup `PLT-BE-004` |
| Eksekusi migration | Tidak dijalankan | `NOT RUN` | Wewenang terpisah; nol migration baru pada task ini |

### 5.1 Dua batas yang wajib dibaca sebelum mempercayai angka di atas

Keduanya **bukan** kegagalan task ini — keduanya memang lingkup `PLT-BE-004`. Dicatat di sini
karena angka "19 lulus" di atas mudah disalahartikan sebagai bukti modul ini aman saat berebut.

**Pertama — durabilitas belum terbukti.** Uji pada berkas ini berjalan di SQLite, dan lingkungan
ujinya **berbagi satu koneksi** antara konteks scoped dan konteks dari factory. Itu memang
keharusan SQLite in-memory, tetapi konsekuensinya tegas: **pemisahan transaksi yang menjadi inti
`DEC-PLT-008` tidak diuji sama sekali di sini**. Pada produksi dengan Npgsql, tiap konteks membuka
koneksinya sendiri dari pool — dan justru perilaku itulah yang harus dibuktikan.

**Kedua — antrean belum terbukti.** `pg_advisory_xact_lock` tidak ada di SQLite, sehingga
`AcquireSeriesLockAsync` **dilewati seluruhnya** pada pengujian ini. Yang tersisa sebagai penjaga
hanyalah index unik `(SequenceKey, ScopeKey)`.

Keduanya adalah lingkup **`PLT-BE-004`**, dan keduanya **mustahil** dipindahkan ke sini. Menyatakan
`AC-PLT-003` atau `AC-PLT-004` lulus berdasarkan berkas ini akan menghasilkan rasa aman yang
keliru — bentuk kegagalan paling berbahaya pada modul ini.

Uji manual: `NOT FEASIBLE` — tabel belum ada di database mana pun karena migration `PLT-BE-002`
belum dijalankan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-PLT-001` alokasi pertama | **Terpenuhi** | `AlokasiPertama_MenerbitkanNomorUrutSatu` |
| `AC-PLT-002` alokasi berikutnya berurutan | **Terpenuhi** | `AlokasiBerikutnya_MenerbitkanNomorBerurutan` |
| `AC-PLT-006` deret `NEVER` melewati pergantian tahun tidak diulang | **Terpenuhi** | `KebijakanNever_MelewatiPergantianTahun_TidakDiulang` |
| `AC-PLT-007` deret `DAILY` berpindah periode | **Terpenuhi** | `KebijakanDaily_PeriodeBerpindahDanPencacahMulaiDariSatu` |
| `AC-PLT-008` parameter tidak sah ditolak | **Terpenuhi** | `ParameterTidakSah_DitolakDanNolNomorTerbit` — 5 kasus, dan nol baris deret tercipta |
| `AC-PLT-009` kebijakan pengulangan asing ditolak | **Terpenuhi** | `KebijakanPengulanganAsing_Ditolak` |
| `AC-PLT-010` nilai tidak muat pada digit ditolak | **Terpenuhi** | `NilaiTidakMuatPadaJumlahDigit_DitolakBukanDiterbitkanMenyimpang` — dan pencacah terbukti **tidak** naik |

**Ketujuh acceptance criteria task ini terpenuhi.**

`AC-PLT-003`, `AC-PLT-004`, dan `AC-PLT-005` **bukan kriteria task ini** — kartu roadmap
menugaskannya ke `PLT-BE-004` dan menyatakan tegas *"durabilitas dan antrean dibuktikan
`PLT-BE-004`, bukan di sini"*. Ketiganya tetap disebut di bagian 5 sebagai **batas kepercayaan**,
supaya tidak ada yang menyangka modul ini sudah terbukti aman saat berebut hanya karena task ini
selesai.

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Nol jalur kode yang menurunkan, menyetel ulang, atau menghapus pencacah | **Terpenuhi** — nol method semacam itu ada |
| Nol endpoint alokasi | **Terpenuhi** — `QBE-CODE-002` dipindai bersih |
| Nol sentuhan pada `BillingNumberSeriesService` | **Terpenuhi** — `git status` tidak memuat satu pun berkas Billing |
| Nol `Count+1` / `Max+1` / pemindaian nomor | **Terpenuhi** — `QBE-CODE-003` dipindai bersih; dibuktikan pula `NilaiDiambilDariPencacah_BukanDihitungDariData` |
| Modul tetap memiliki awalan dan format | **Terpenuhi** — empat dari enam parameter datang dari pemanggil; dijaga `Alokator_TidakMenerimaKonteksScoped` dan `Awalan_DatangDariModulDanDinormalkan` |
| Kompatibilitas `AddDbContext` + `AddDbContextFactory` diverifikasi | **Terpenuhi** — 3 pengujian, `validateScopes` aktif |

---

## 7. Delta kontrak

| Delta | Isi | Alasan |
| --- | --- | --- |
| **Nol delta perilaku** | Tanda tangan `AllocateAsync`, keenam parameter, dan ketujuh kode validasi dibuat persis seperti `integration-contract.md` §1 dan `validation-matrix.md` | Kontrak `v1` sudah cukup rinci |
| **Tambahan teknis** | `NumberSeriesAllocationException.ValidationCode` tidak disebut kontrak | Supaya modul pemanggil menelusuri sebab penolakan lewat kode, bukan mencocokkan teks pesan yang dapat berubah. Nol aturan bisnis baru |
| **Keputusan teknis** | Factory didaftarkan `Scoped`, bukan `Singleton` bawaan | Kebutuhan mekanis DI — lihat bagian 4. Nol dampak pada perilaku alokasi |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build bersih **`210 Warning(s)`, sama persis baseline** — nol peringatan baru |
| Masalah yang diketahui | **(1)** `AC-PLT-003`, `004`, `005` belum terbukti; keduanya mustahil di luar PostgreSQL. **(2)** Advisory lock **dilewati** pada provider non-PostgreSQL — pada produksi selalu PostgreSQL, tetapi perilaku itu perlu disadari bila kelak ada yang menjalankan modul ini di provider lain |
| Risiko tersisa | **Migration `PLT-BE-002` belum dijalankan**, sehingga alokator belum dapat berjalan di lingkungan mana pun. **Menjadwalkan task Bank Darah sebelum `PLT-BE-004` lulus adalah risiko yang disadari**: bila `AC-PLT-003` ternyata gagal, perbaikannya menyentuh mesin yang sudah dipakai order darah |
| Temuan tata kelola | Registry di suite skill masih **belum** memuat baris `Platform`/`Num`; yang ditegakkan checker ada di `docs/engineering/`. Dilanjutkan sesuai aturan skill (*"source repository target yang berlaku"*), dicatat sebagai `FACT-PLT-012`. Sinkronisasi milik pemilik registry |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 9 |
| Langkah berikutnya | **(1)** **`PLT-BE-004`** — buktikan durabilitas dan antrean di PostgreSQL sungguhan. Ini bukan formalitas; tiga acceptance criteria inti menunggu di sana. **(2)** Jalankan migration `PLT-BE-002`. **(3)** Baru jadwalkan sembilan task backend Bank Darah |

---

## 9. Status Git

```text
 M Program.cs
?? Areas/Platform/NumberSeriesManagement/Services/
?? Tests/QuilvianSystemBackend.UnitTests.Sqlite/Platform/NumberSeriesAllocatorTests.cs
?? Tests/QuilvianSystemBackend.UnitTests.Sqlite/Platform/NumberSeriesCompositionTests.cs
```

Berkas lain pada working tree berasal dari pekerjaan sebelumnya di sesi yang sama — `BE-BD-005`,
`BE-BD-011`, `PLT-BE-001`, `PLT-BE-002`, blueprint, dan roadmap — bukan dari task ini.

Nol operasi `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, `stash`, maupun `deploy`
dijalankan. `HEAD` tetap `f0d6855`.
