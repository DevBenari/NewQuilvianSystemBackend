# Registry Sistem Quilvian

| Field | Nilai |
| --- | --- |
| Versi registry | 1 |
| Mode pemindaian | `full` |
| Tanggal pemindaian | 2026-08-27 |
| Backend | `NewQuilvianSystemBackend` cabang `Ikbal` commit `f2c5090` |
| Frontend | `QuilvianSystemFrontendDev` cabang `Ikbalv2` commit `847be1fc0` |
| Status kesegaran | `SEGAR` |
| Batas berlaku scan penuh | 2026-09-26 |
| Cakupan yang tidak diperiksa | Database runtime, service eksternal, environment produksi, isi data |

## Ringkasan angka

| Yang dihitung | Jumlah | Cara menghitung |
| --- | ---: | --- |
| Area | 4 | Folder tingkat satu di `Areas/` ditambah `Models/` bersama |
| Modul | 14 | Folder modul di dalam setiap area |
| `DbSet` terdaftar | 516 | Baris `public DbSet<...>` pada `Repositories/ApplicationDbContext.cs` |
| File EF configuration | 481 | Berkas `*Configuration.cs` di `Repositories/` |
| Entity punya configuration | 478 | Tipe unik pada `IEntityTypeConfiguration<T>` |
| Controller | 276 | Berkas `*Controller.cs` di `Areas/` dan `Controllers/` |
| Migration | 104 | Berkas migration di `Migrations/`, di luar Designer dan Snapshot |
| Tabel dibuat atau diganti nama migration | 553 | `CreateTable` dan `RenameTable` pada seluruh migration |
| Grup Swagger `[Tags(...)]` | 269 | Nilai unik atribut `[Tags(...)]` |
| Endpoint dipanggil frontend | 191 | String path `/v1/...` pada `src/` frontend |
| Zona konflik terbuka | 7 | Baris berstatus Terbuka pada berkas 05 |

Selisih 481 berkas configuration dengan 478 entity yang punya configuration berasal dari
berkas configuration untuk tipe yang tidak terdaftar sebagai `DbSet` tersendiri, misalnya
konfigurasi tabel relasi. Selisih 553 tabel migration dengan 516 `DbSet` berasal dari tabel
yang pernah dibuat lalu diganti nama, sehingga nama lama dan nama baru sama-sama tercatat.

## Sebaran tingkat kesiapan

| Tingkat | Jumlah entity | Bagian |
| --- | ---: | ---: |
| `L4 Terpakai` | 205 | 40% |
| `L3 Berlayanan` | 95 | 18% |
| `L2 Berskema` | 216 | 42% |
| `L1 Terdaftar` | 0 | 0% |
| `⚠ Bermasalah` | 0 | 0% |

Tidak ada entity yang berhenti di `L1`. Artinya setiap `DbSet` yang terdaftar sudah punya
tabel yang benar-benar dibuat migration. Ini kondisi yang sehat.

Yang perlu dibaca hati-hati adalah 216 entity `L2`. Tabelnya ada, tetapi belum ada controller
maupun service yang memakainya. Modul yang membutuhkannya harus memperhitungkan pekerjaan
tambahan, bukan menganggapnya siap. Sebagian besar berasal dari Billing Management dan
sebagian modul Human Resource.

## Cara membaca kolom Consumer

Kolom Consumer diisi `✓` bila entity dipakai controller yang **base URL-nya benar-benar
dipanggil frontend**. Penentuannya memakai perbandingan antara daftar `[Route(...)]` backend
dan daftar path `/v1/...` yang muncul di source frontend.

Cara ini menjawab "apakah kemampuannya sampai ke pengguna", bukan "apakah baris kode ini
dieksekusi". Entity yang dipakai antar modul backend tanpa layar frontend tetap tercatat
`L3`, bukan `L4`.

Lima awalan berikut ada di backend tetapi tidak dipanggil frontend mana pun:

| Awalan backend | Artinya |
| --- | --- |
| `/v1/health-services/billing-management` | Modul Billing belum punya layar |
| `/v1/health-services/inpatient-management` | Modul Rawat Inap belum punya layar |
| `/v1/health-services/laboratory-management` | Modul Laboratorium belum punya layar |
| `/v1/administrator/setting` | Belum dipanggil |
| `/v1/self-services/human-resource` | Belum dipanggil |

## Cara menghasilkan ulang

Jalankan `/qv-scan full` untuk pemindaian menyeluruh, atau `/qv-scan refresh` untuk memperbarui
hanya bagian yang berubah sejak commit di atas.
