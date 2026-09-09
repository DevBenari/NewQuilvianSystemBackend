# Laporan Perubahan Backend — `PLT-BE-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PLT-BE-004` |
| Judul | Durabilitas dan antrean dibuktikan di PostgreSQL sungguhan |
| Slice | `PLT-SLICE-01` — gelombang `MVP-1` |
| Roadmap | `docs/module-blueprints/platform/roadmap/backend-roadmap.md` §5 |
| Trace | `AC-PLT-003`, `AC-PLT-004`, `AC-PLT-005`, `AC-PLT-012` · `DEC-PLT-003`, `DEC-PLT-008` · `INV-PLT-001`, `INV-PLT-003` · `CONF-PLT-002`, `NOTE-PLT-001` |
| Contract version | `v1` — ✅ **`approved`** |
| Dependency | `PLT-BE-003` ✅ · **PostgreSQL yang berjalan** ⛔ |
| Klasifikasi | `MEDIUM` — nol source aplikasi, enam uji integrasi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `f0d6855` cabang `sukmagp` |
| Tanggal | `2026-09-09` |
| Status | 🟡 **SELESAI SEBAGIAN.** Uji ditulis dan **compile**, tetapi **nol acceptance criteria terbukti** — tidak ada PostgreSQL yang dapat dipakai di lingkungan ini |

---

## 1. Masalah yang diperbaiki

`PLT-BE-003` membangun alokator yang **mengaku** durabel: pencacahnya ditulis pada transaksi dan
koneksi tersendiri, sehingga nomor seharusnya hangus — bukan dipakai ulang — ketika pekerjaan
bisnis pemanggil dibatalkan.

**Klaim itu belum pernah dibuktikan.** Seluruh pengujian `PLT-BE-003` berjalan di SQLite, yang
tidak punya `pg_advisory_xact_lock` dan yang lingkungan ujinya berbagi satu koneksi. Dua sifat
yang justru paling menentukan modul ini karena itu belum diuji sama sekali.

**Kenapa itu berbahaya bila dibiarkan.** Sembilan task backend Bank Darah menunggu di belakang
alokator ini. Membangunnya di atas mesin yang durabilitasnya belum terbukti berarti, bila
`AC-PLT-003` ternyata gagal, perbaikannya menyentuh mesin yang sudah dipakai order darah.

---

## 2. Proses bisnis

Task ini **tidak** menambah atau mengubah proses bisnis apa pun. Ia menghasilkan **bukti** atas
perilaku yang sudah dibangun `PLT-BE-003`.

Yang dibuktikan enam uji tersebut:

| Uji | Yang dibuktikan |
| --- | --- |
| Pekerjaan pemanggil dibatalkan | Pencacah **tetap naik**; nomor hangus, tidak diterbitkan lagi |
| Pekerjaan pemanggil berhasil | Nomor tetap berurutan — jalur normal tidak rusak oleh perbaikan durabilitas |
| Dua puluh alokasi bersamaan, deret sama | Dua puluh nomor **berbeda**, nol kegagalan |
| Kunci deret A ditahan, alokasi deret B | Deret B **tetap selesai** — kunci per deret, bukan global |
| Deret Billing dibatalkan | Nomor **dipakai ulang** — perilaku lama **sengaja dipertahankan** |
| Tabel Billing dan Platform | Terpisah; nol baris saling menyeberang |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` bagian *Keselamatan Database* dan *Validasi Backend* · `BACKEND_ENGINEERING_CONTRACT.md` |
| Kontrak modul | `platform/testing/acceptance-test-matrix.md` · `platform/contracts/state-transition-matrix.md` §3 · `platform/roadmap/backend-roadmap.md` §5 |
| Pola uji integrasi | `Tests/.../Infrastructure/BillingTestDatabaseFixture.cs` · `Laboratory/LaboratorySpecimenLifecycleTests.cs` — dipakai sebagai contoh pemakaian fixture bersama |
| Mesin yang dibandingkan | `BillingNumberSeriesService.cs` — untuk membuktikan perilakunya **tidak** berubah |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Platform/NumberSeriesDurabilityTests.cs` | **Baru.** Enam uji integrasi |

**Nol source aplikasi disentuh.** Task ini murni menambah bukti; `NumberSeriesAllocator`,
`NumNumberSeries`, `Program.cs`, dan seluruh berkas Billing tidak berubah satu baris pun.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint |
| Database | **Nol perubahan schema.** Nol migration baru. Uji ini **menerapkan migration dan menulis baris** saat dijalankan, sehingga hanya boleh menyasar database test tersendiri |
| Keamanan/Auth | `NOT APPLICABLE` — nol endpoint, nol butir hak akses |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Platform` / `NumberSeriesManagement` |
| Pemilik / prefix registry | `NumberSeriesManagement / Number Series` → `Num`, status ✅ **`ACTIVE`** |
| Keberlakuan | **`NEW CODE`** — berkas uji baru |
| QBE ID yang berlaku | `QBE-MOD-001` penempatan di bawah pemiliknya. **Nol QBE lain berlaku**: task ini tidak membuat entity, tidak mengalokasikan nomor, tidak menambah controller maupun service |

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | `0 Error(s)`, **`210 Warning(s)`** | `PASS` | Sama persis baseline — nol peringatan baru |
| `dotnet build` project integrasi | Berhasil | `PASS` | Keenam uji **compile** terhadap kontrak `PLT-BE-003` yang sebenarnya |
| **`AC-PLT-003`** durabilitas | **Tidak dijalankan** | **`NOT RUN`** | Fixture berhenti fail-closed — lihat §4.1 |
| **`AC-PLT-004`** antrean deret sama | **Tidak dijalankan** | **`NOT RUN`** | Sama |
| **`AC-PLT-005`** deret berbeda tidak menunggu | **Tidak dijalankan** | **`NOT RUN`** | Sama |
| **`AC-PLT-012`** deret Billing tidak berubah | **Tidak dijalankan** | **`NOT RUN`** | Sama |
| Eksekusi migration | Tidak dijalankan | `NOT RUN` | Wewenang terpisah, dan tidak ada database untuk menerapkannya |

### 4.1 Kenapa `NOT RUN`, bukan `FAIL`

Percobaan menjalankan uji ini berakhir dengan **6 kegagalan yang seluruhnya berupa
configuration error**, bukan kegagalan domain:

```text
Failed!  - Failed: 6, Passed: 0, Total: 6, Duration: 4 ms

System.InvalidOperationException : BLOCKED_BY_TEST_DB_CONFIGURATION:
environment variable QUILVIAN_BILLING_TEST_DB belum diisi.
```

**Nol perintah dikirim ke database mana pun** — durasinya 4 milidetik, dan fixture berhenti
sebelum membuka koneksi. Itu perilaku yang **benar** dan disengaja: `BillingTestDatabaseFixture`
bersifat *fail-closed* sejak temuan `RJ-BIL-BE-002`, ketika fallback ke
`appsettings.Development.json` membuat `dotnet test` menerapkan migration ke database dev bersama
`QuilvianNewDevTim01` tanpa ada yang memerintahkannya.

Keadaan lingkungan yang diperiksa:

| Pemeriksaan | Hasil |
| --- | --- |
| `QUILVIAN_BILLING_TEST_DB` (process / user / machine) | **Kosong pada ketiganya** |
| Service bernama `*postgres*` | **Nol** |
| Port `5432` LISTEN | **Tidak** |
| `docker` | **Tidak terpasang** |

Tidak ada PostgreSQL yang dapat dipakai di lingkungan ini, dan **tidak ada satu pun jalan pintas
yang sah**: menyalakan database, menyediakan container, atau mengarahkan uji ke database dev
bersama adalah tindakan infrastruktur yang menuntut wewenang terpisah — dan yang terakhir persis
yang dilarang fixture ini.

Uji manual: `NOT FEASIBLE` — sebabnya sama.

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-PLT-003` pencacah tetap naik setelah pemanggil batal | **BELUM terbukti** | Uji `PekerjaanPemanggilDibatalkan_PencacahTetapNaik_NomorHangus` **ada dan compile**, tetapi belum pernah dijalankan |
| `AC-PLT-004` dua puluh alokasi bersamaan, nomor berbeda semua | **BELUM terbukti** | `DuaPuluhAlokasiBersamaan_DeretSama_MenghasilkanNomorBerbedaSemua` |
| `AC-PLT-005` deret berbeda tidak saling menunggu | **BELUM terbukti** | `DeretBerbeda_TidakSalingMenunggu` |
| `AC-PLT-012` empat deret Billing tidak berubah perilakunya | **BELUM terbukti** | `DeretBilling_TetapMemakaiUlangNomorSaatTransaksiDibatalkan` |

**Nol dari empat terbukti.** Task ini karena itu 🟡 **SEBAGIAN**, bukan ✅.

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| `AC-PLT-003` dan `AC-PLT-004` lulus **di PostgreSQL**, bukan InMemory | **BELUM** — tidak ada PostgreSQL |
| Nol uji inti dinyatakan lulus berdasarkan provider tanpa transaksi | **Terpenuhi** — justru itu sebabnya berkas ini terpisah dari uji SQLite |
| Empat deret Billing terbukti tidak berubah perilakunya | **BELUM** |
| Angka hasil uji dicatat apa adanya, termasuk yang `NOT RUN` | **Terpenuhi** — bagian 4 |

---

## 6. Delta kontrak

| Delta | Isi | Alasan |
| --- | --- | --- |
| **Nol delta** | Keenam uji diturunkan langsung dari `testing/acceptance-test-matrix.md` | — |
| **Tambahan** | Dua uji di luar daftar `AC`: jalur normal tetap berurutan, dan tabel Billing/Platform terpisah | Keduanya penjaga regresi, bukan aturan bisnis baru. Yang pertama memastikan perbaikan durabilitas tidak merusak jalur normal; yang kedua memastikan kedua tabel tidak saling menyeberang |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build bersih **`210 Warning(s)`, sama persis baseline** |
| Masalah yang diketahui | **Keempat acceptance criteria belum terbukti.** Uji `DeretBerbeda_TidakSalingMenunggu` memakai batas waktu 15 detik sebagai bentuk kegagalannya; bila kunci ternyata global, uji itu akan gagal lewat batas waktu, bukan lewat assertion. Ini disengaja — menggantung tanpa batas akan menyembunyikan cacatnya |
| Risiko tersisa | **Alokator masih belum terbukti durabel.** `PLT-BE-003` menyediakan kemampuannya, tetapi klaim `DEC-PLT-008` belum divalidasi terhadap `COMMIT`/`ROLLBACK` sungguhan. **Menjadwalkan sembilan task backend Bank Darah sekarang berarti membangun di atas klaim yang belum diperiksa** |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 8 |
| Langkah berikutnya | **(1)** Sediakan database test PostgreSQL tersendiri, lalu isi `QUILVIAN_BILLING_TEST_DB` dengan connection string-nya — nama database **wajib** memuat `test` dan **tidak boleh** memuat `dev`, `prod`, `staging`, `uat`, atau `shared`. **(2)** Jalankan ulang keenam uji; task ini naik ke ✅ hanya setelah keempat `AC` benar-benar lulus. **(3)** Baru jadwalkan task Bank Darah |

---

## 8. Status Git

```text
?? Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Platform/
```

Berkas lain pada working tree berasal dari task sebelumnya di sesi yang sama.

Nol operasi `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, `stash`, maupun `deploy`
dijalankan. **Nol perintah database dijalankan.** `HEAD` tetap `f0d6855`.
