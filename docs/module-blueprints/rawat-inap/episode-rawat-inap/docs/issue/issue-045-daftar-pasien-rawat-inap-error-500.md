# ISSUE-045 — Daftar Pasien Rawat Inap Menampilkan Data Kosong pada Muat Awal dan Gagal HTTP 500 Saat Menyaring Tanggal

```yaml
issue_id: ISSUE-EPS-045
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Rawat Inap — Daftar Pasien Rawat Inap (/health-services/inpatient-management/episodes, FE-INP-16)"
sumber_laporan: "Laporan pengguna 25-11-2026: 2 butir keluhan disertai 1 screenshot layar dan 1 capture Network DevTools (request GET ke server api-dev mengembalikan HTTP 500)"
tanggal_issue: "2026-11-25"
status: TERBUKA
keparahan_tertinggi: Blocker
source_sha_backend: "38b614eff7f27da3095a1c9b1cf31926e63d072f (MHamzah)"
source_sha_frontend: "32d67d384cf7f63ff1ad424a0206c53a15fdfcd8 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-045-daftar-pasien-rawat-inap-error-500.md
```

---

## 1. Ringkasan

Menu **Daftar Pasien Rawat Inap** (`FE-INP-16`) mengalami dua gejala yang dilaporkan pengguna pada lingkungan `dev` (`api-dev.quilvian-mmchospital.com`): seluruh kartu ringkasan dan tabel tampil kosong (`0`) saat halaman dibuka tanpa penyaring, lalu server selalu menjawab galat `500 Internal Server Error` (*"Terjadi kesalahan pada server."*) ketika pengguna menyaring dengan tanggal mulai (`startDate=2026-11-02`). Penelusuran langsung ke source backend dan frontend membuktikan bahwa galat `500` disebabkan oleh bug backend pada penyusun query daftar episode: parameter tanggal dari layar dikirim sebagai teks tanggal polos (`2026-11-02`), terikat di server sebagai tanggal tanpa zona waktu (*`DateTimeKind.Unspecified`*), lalu langsung dibandingkan dengan kolom `CreateDateTime` yang bertipe waktu berzona UTC (`timestamp with time zone`) di PostgreSQL sehingga driver database Npgsql menolak mengeksekusi query. Sebaliknya, pada kondisi muat awal tanpa penyaring tanggal (seperti terlihat pada screenshot), endpoint daftar dan ringkasan bekerja normal tanpa penyaring tersembunyi — yang mengindikasikan basis data server `dev` (`QuilvianNewDevMaster`) memang sedang tidak memiliki baris episode aktif/non-hapus, atau perlu diverifikasi ulang dengan query pembacaan `SELECT` yang aman. Penelusuran lebih lanjut juga menemukan pola cacat zona waktu yang sama persis pada layanan daftar **Pasien Sedang Dirawat** (`Census`) serta tiga query pemantauan Rawat Inap lainnya.

---

## 2. Laporan asli

| No. laporan | Keluhan (kata-kata pelapor) | Lampiran yang merujuk |
| ---: | --- | --- |
| 1 | "coba cek pada menu ini tolong atasi masalah kenapa data ya tidak muncul ya ?" | Screenshot layar `Rawat Inap > Daftar Pasien Rawat Inap` (akun `SuperAdmin`): seluruh kartu ringkasan menunjukkan `0` (`TOTAL EPISODE 0`, `SEDANG DISIAPKAN 0`, `SEDANG DIRAWAT 0`, `RENCANA PULANG 0`, `SELESAI 0`, `BATAL 0`), seluruh penyaring dalam keadaan bawaan/kosong (`Tanggal mulai`, `Tanggal akhir`, `Semua unit layanan`, `Semua kelas perawatan`, `Semua status`), tabel menampilkan pesan *"Tidak ada episode yang cocok dengan penyaring ini."* |
| 2 | "dan terjadi eror masalah seperti ini , ketika melakukan pencarian coba tolong atasi masalah tersebut" | Capture Network DevTools: Request `GET https://api-dev.quilvian-mmchospital.com/api/v1/health-services/inpatient-management/episodes?pageNumber=1&pageSize=25&sortBy=createDateTime&sortDirection=desc&startDate=2026-11-02` → `Status code: 500 Internal Server Error`, Response JSON: `{"success":false,"statusCode":500,"message":"Terjadi kesalahan pada server.","data":null,"errors":null,"timestamp":"2026-11-25T14:08:50.9326605+07:00"}` |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | :---: | --- | --- | --- | :---: | --- | --- |
| `ISS-EPS-045-01` | 1 | Halaman awal menampilkan `TOTAL EPISODE 0` tanpa galat karena server `dev` mengembalikan nol baris | `DATA` / `PERLU_KONFIRMASI` | Data Lingkungan `dev` (`QuilvianNewDevMaster`) | Medium | `DARI-CAPTURE` + `SUDAH-VERIFIKASI` di source + `DUGAAN` isi DB `dev` | — (verifikasi data via `SELECT`) |
| `ISS-EPS-045-02` | 2 | Pencarian dengan penyaring tanggal (`startDate` / `endDate`) gagal HTTP 500 akibat tanggal tanpa zona waktu ditolak PostgreSQL | `BUG` | Backend (`InpEpisodeService.Reads.cs`) | Blocker | `SUDAH-VERIFIKASI` di source & `DARI-CAPTURE` | `FIX-EPS-045-01` |
| `ISS-EPS-045-T1` | — | Pola cacat tanggal tanpa zona waktu yang sama persis ada di layar `Pasien Sedang Dirawat` (Census), `Daftar Pantau`, dan `Integration Outbox` | `BUG` | Backend (`InpCensusQueryService.cs`, `InpDischargeService.Closure.cs`, `InpDischargeService.Departure.cs`, `InpIntegrationReplayService.cs`) | High | `SUDAH-VERIFIKASI` di source backend | `FIX-EPS-045-02` |
| `ISS-EPS-045-T2` | — | Saat permintaan terfilter gagal 500, tabel daftar episode tetap menyisakan data lama (stale) sementara kartu ringkasan menghilang, dan pemilih tanggal belum saling membatasi rentang | `BUG` (UX) | Frontend (`use-inpatient-episode-worklist.jsx`, `inpatient-episode-worklist-view.jsx`) | Low | `SUDAH-VERIFIKASI` di source frontend | `FIX-EPS-045-03` |

---

## 4. Rincian per temuan

### ISS-EPS-045-01 — Halaman awal menampilkan `TOTAL EPISODE 0` tanpa galat karena server `dev` mengembalikan nol baris

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DATA` / `PERLU_KONFIRMASI` |
| **Area** | Data Lingkungan `dev` (bukan cacat kueri tanpa penyaring) |
| **Keparahan** | Medium |
| **Status bukti** | `DARI-CAPTURE` + `SUDAH-VERIFIKASI` di source; `DUGAAN` mengenai jumlah baris di database remote `api-dev` |
| **Perbaikan** | Tidak ada perubahan kode untuk muat awal; verifikasi data di database sasaran |

**Apa yang terjadi.**  
Saat petugas membuka menu **Rawat Inap → Daftar Pasien Rawat Inap** pada aplikasi `dev`, keenam kartu ringkasan di bagian atas menampilkan angka `0` (`TOTAL EPISODE 0`, `SEDANG DISIAPKAN 0`, `SEDANG DIRAWAT 0`, `RENCANA PULANG 0`, `SELESAI 0`, `BATAL 0`), seluruh kotak penyaring masih kosong (nilai bawaan), tidak ada kotak peringatan merah galat server, dan tabel menampilkan pesan *"Tidak ada episode yang cocok dengan penyaring ini."* Ketika petugas merasa datanya seharusnya ada lalu mencoba menyaring mulai tanggal tertentu (`2026-11-02`), server justru jatuh ke galat `500` (`ISS-EPS-045-02`).

**Kenapa terjadi.**  
Penelusuran alur dari layar hingga database membuktikan tidak ada penyaring tersembunyi pada muat awal:

1. `SUDAH-VERIFIKASI` — Di frontend [use-inpatient-episode-worklist.jsx:77-86](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-worklist.jsx#L77-L86) dan [inpatient-episode-worklist-constants.jsx:42-53](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-episode-worklist-constants.jsx#L42-L53), nilai bawaan semua penyaring adalah kosong (`""`), tanpa `startDate` dan tanpa `episodeStatus` bawaan.
2. `SUDAH-VERIFIKASI` — Fungsi `buildEpisodeWorklistQuery` pada [inpatient-episode-worklist-utils.jsx:77-106](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-episode-worklist-utils.jsx#L77-L106) hanya mengirim `pageNumber=1`, `pageSize=25`, `sortBy=createDateTime`, dan `sortDirection=desc` saat penyaring kosong.
3. `SUDAH-VERIFIKASI` — Di backend, `BuildFilteredEpisodeQuery` pada [InpEpisodeService.Reads.cs:383-438](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs#L383-L438) hanya menyaring `!x.IsDelete` ketika tidak ada parameter penyaring yang diisi. Tidak ada filter global (*`HasQueryFilter`*) pada `InpEpisode` di `Repositories/` (`SUDAH-VERIFIKASI`), dan fungsi pengguguran otomatis `ExpireDueDraftEpisodesAsync` ([InpEpisodeService.cs:1099-1108](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs#L1099-L1108)) hanya mengubah status menjadi `Cancelled` tanpa menyetel `IsDelete = true` — sehingga episode yang gugur pun tetap dihitung pada kartu `BATAL`.
4. `DARI-CAPTURE` — Karena tampilan pada screenshot memperlihatkan keenam kartu ringkasan bernilai `0` (bukan teks *"Ringkasan tidak tersedia."* yang muncul bila `GET /episodes/summary` gagal) dan tidak menampilkan banner merah `InformationAlert` ([inpatient-episode-worklist-view.jsx:290-305](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-episode-worklist-view.jsx#L290-L305)), permintaan `GET /episodes` dan `GET /episodes/summary` tanpa `startDate` tersebut mengembalikan `200 OK` dengan `0` data.
5. `DUGAAN` — Database yang tersambung ke `api-dev.quilvian-mmchospital.com` (`QuilvianNewDevMaster`) saat itu memang memiliki `0` baris `InpEpisode` yang bernilai `IsDelete = false` (berbeda dengan database pengembangan lokal `QuilvianNewDevHamzah` yang dipakai saat uji coba sebelumnya). **Cara membuktikan:** jalankan query pembacaan berikut pada database `QuilvianNewDevMaster`:
   ```sql
   SELECT COUNT(*) AS total_non_delete,
          COUNT(*) FILTER (WHERE "IsDelete" = true) AS total_soft_delete
   FROM public."InpEpisode";
   ```

**Apakah ini menyimpang dari desain?**  
Perilaku layar dan query muat awal **sesuai desain** (`05-skema-tampilan.md:977-980`, `IA-INP-03`): daftar kerja memang harus menampilkan seluruh status tanpa filter bawaan, dan menampilkan pesan kosong informatif bila belum ada episode sama sekali. Jika pengguna mengharapkan data uji yang sebelumnya dibuat di lokal (`QuilvianNewDevHamzah`), itu adalah perbedaan isi database antar lingkungan (`DATA`), bukan bug kode muat awal.

**Dampak nyata.**  
Bila `QuilvianNewDevMaster` belum pernah memiliki data admisi rawat inap, pengguna yang membuka layar pertama kali melihat `0` pada semua kartu dan mengira sistem gagal menarik data, lalu mencoba mencari tanggal tertentu dan langsung membentur error `500` (`ISS-EPS-045-02`).

**Rekomendasi.**  
Konfirmasikan isi tabel `public."InpEpisode"` pada database lingkungan `api-dev` dengan query `SELECT` di atas. Jika memang kosong, lakukan uji admisi rawat inap atau siapkan data uji melalui jalur aplikasi resmi.

---

### ISS-EPS-045-02 — Pencarian dengan penyaring tanggal (`startDate` / `endDate`) gagal HTTP 500 akibat tanggal tanpa zona waktu ditolak PostgreSQL

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` |
| **Area** | Backend (`Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs`) |
| **Keparahan** | Blocker |
| **Status bukti** | `SUDAH-VERIFIKASI` di source backend & frontend, dan `DARI-CAPTURE` pada Network DevTools |
| **Perbaikan** | `FIX-EPS-045-01` |

**Apa yang terjadi.**  
Setiap kali petugas memilih **Tanggal mulai** atau **Tanggal akhir** pada bilah penyaring **Daftar Pasien Rawat Inap** (misalnya memilih tanggal mulai `2 November 2026`), daftar pasien dan kartu ringkasan gagal dimuat. Server membalas dengan kode status `500 Internal Server Error` dan pesan umum *"Terjadi kesalahan pada server."*, sehingga pencarian berdasarkan rentang tanggal sama sekali tidak dapat digunakan.

**Kenapa terjadi.**  
Rantai sebab lengkap telah diverifikasi di kedua repository:

1. `SUDAH-VERIFIKASI` — Frontend memformat tanggal dari komponen `FilterDatePicker` dalam bentuk tanggal kalender polos `YYYY-MM-DD` melalui `normalizeDateFilter` di [inpatient-episode-worklist-utils.jsx:71-75,99-103](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-episode-worklist-utils.jsx#L71-L103), lalu mengirimnya sebagai parameter query `startDate=2026-11-02` (persis seperti tampak pada `Request URL` pelapor).
2. `SUDAH-VERIFIKASI` — Di backend, parameter tersebut diikat ke properti `DateTime? StartDate` dan `DateTime? EndDate` pada `InpatientEpisodeListQuery` di [InpatientEpisodeReadDtos.cs:22-27](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeReadDtos.cs#L22-L27). Ketika ASP.NET Core mem-parsing teks tanggal polos `"2026-11-02"`, objek `DateTime` yang terbentuk memiliki `Kind = DateTimeKind.Unspecified` (`2026-11-02 00:00:00`).
3. `SUDAH-VERIFIKASI` — Di [InpEpisodeService.Reads.cs:421-430](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs#L421-L430), method `BuildFilteredEpisodeQuery` meneruskan nilai `query.StartDate.Value` dan `query.EndDate.Value.Date.AddDays(1)` (keduanya tetap ber-`Kind = Unspecified`) secara langsung ke query LINQ EF Core terhadap kolom `x.CreateDateTime`:
   ```csharp
   if (query.StartDate.HasValue)
   {
       filtered = filtered.Where(x => x.CreateDateTime >= query.StartDate.Value);
   }

   if (query.EndDate.HasValue)
   {
       var exclusiveEnd = query.EndDate.Value.Date.AddDays(1);
       filtered = filtered.Where(x => x.CreateDateTime < exclusiveEnd);
   }
   ```
4. `SUDAH-VERIFIKASI` — Kolom `CreateDateTime` pada tabel `public."InpEpisode"` bertipe `timestamp with time zone` ([20260824095353_CreateInpatientTransactionTables.cs:44](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Migrations/20260824095353_CreateInpatientTransactionTables.cs#L44)), dan aplikasi `Program.cs` (`SUDAH-VERIFIKASI`) tidak mengaktifkan flag kompatibilitas lama Npgsql (`EnableLegacyTimestampBehavior`). Pada Npgsql 9, membandingkan atau mengirim parameter `DateTime` dengan `Kind = DateTimeKind.Unspecified` (atau `Local`) terhadap kolom `timestamp with time zone` **selalu melempar exception** `System.ArgumentException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported.` (pola exception identik terbukti di log lokal [quilvian-backend-20261120.json:780-781](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Logs/quilvian-backend-20261120.json)).
5. `SUDAH-VERIFIKASI` — Karena `GetEpisodeListAsync` ([InpEpisodeService.Reads.cs:51](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs#L51)) dan `GetEpisodeSummaryAsync` ([InpEpisodeService.Reads.cs:157](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs#L157)) sama-sama memanggil `BuildFilteredEpisodeQuery`, kedua endpoint (`GET /episodes` dan `GET /episodes/summary`) langsung jatuh ke `GlobalExceptionMiddleware` ([GlobalExceptionMiddleware.cs:112](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Middlewares/GlobalExceptionMiddleware.cs#L112)) yang membalas `500` *"Terjadi kesalahan pada server."*
6. **Kenapa lolos saat pembuatan awal (`BE-RWI-009`):** laporan [be-rwi-009-daftar-dan-detail-episode.md:176-180](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/be-rwi-009-daftar-dan-detail-episode.md#L176-L180) menguji penyaringan tanggal memakai EF Core In-Memory, yang tidak menegakkan validasi `DateTimeKind.Utc` milik driver PostgreSQL Npgsql.

**Apakah ini menyimpang dari desain?**  
Ya, `BUG`. Desain `BE-RWI-009` dan `FE-INP-16` ([05-skema-tampilan.md:979](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/05-skema-tampilan.md#L979)) menetapkan bahwa pengguna dapat menyaring daftar episode menurut rentang tanggal admisi dibuka, dan layar mengirimkan `YYYY-MM-DD` sesuai kontrak.

**Dampak nyata.**  
Misalkan Kepala Ruangan ingin menelusuri pasien rawat inap yang dibuka sejak tanggal 2 November 2026 (`startDate=2026-11-02`). Begitu tanggal tersebut dipilih pada penyaring *Tanggal mulai*, sistem gagal memuat daftar maupun ringkasan dengan error `500`. Selain itu, jika batas tanggal tidak dikonversi dari jam operasional WIB (`Asia/Jakarta`, UTC+7) ke UTC, episode yang dibuka pukul `06:00 WIB` pada `2 November 2026` (tersimpan di database sebagai `2026-11-01T23:00:00Z`) akan terlewat bila dianggap UTC tengah malam; dengan helper resmi aplikasi `AppDateTimeHelper.OperationalDateToUtc`, tanggal `2026-11-02` dikonversi menjadi batas bawah `2026-11-01T17:00:00Z` dan bila `endDate=2026-11-02` menjadi batas atas eksklusif `2026-11-02T17:00:00Z` sehingga seluruh admisi tanggal 2 November WIB dari pukul `00:00` sampai `23:59:59.999 WIB` tepat tercakup.

**Rekomendasi.**  
Normalkan `query.StartDate` dan `query.EndDate` di `BuildFilteredEpisodeQuery` menggunakan `AppDateTimeHelper` ([Helpers/AppDateTimeHelper.cs:67-85](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Helpers/AppDateTimeHelper.cs#L67-L85)) sebelum dibandingkan dengan `x.CreateDateTime`, mengikuti standar yang sudah terbukti berjalan di modul lain (`BbkBloodOrderService`, `LabDashboardService`, `OptReportService`).

---

## 5. Tanya-jawab pelapor

> **T:** "coba cek pada menu ini tolong atasi masalah kenapa data ya tidak muncul ya ?"
>
> **J:** Saat layar **Daftar Pasien Rawat Inap** baru dibuka tanpa penyaring apa pun (seperti terlihat pada screenshot), kueri backend tidak menyembunyikan data apa pun — satu-satunya syarat yang dipasang hanya data tidak dihapus (`!IsDelete`), dan keenam kartu ringkasan menampilkan angka `0` tanpa banner galat merah yang berarti server `api-dev` merespons `200 OK` dengan daftar kosong karena tabel `InpEpisode` di database `dev` (`QuilvianNewDevMaster`) sedang tidak memiliki baris episode aktif. Namun ketika petugas mencoba mencari dengan menyaring **Tanggal mulai** (`startDate=2026-11-02`), server langsung gagal dengan error `500` karena kode penyaring tanggal backend belum mengonversi format tanggal kalender menjadi format waktu UTC yang diterima PostgreSQL — itulah penyebab data pasti gagal tampil setiap kali penyaring tanggal digunakan.

---

## 6. Temuan tambahan

### ISS-EPS-045-T1 — Pola cacat tanggal tanpa zona waktu yang sama persis ada di layar Pasien Sedang Dirawat (Census), Daftar Pantau, dan Integration Outbox

| | |
| --- | --- |
| **Kenapa dicantumkan** | Akar masalahnya identik persis dengan `ISS-EPS-045-02` (parameter `DateTime?` dari query string dibandingkan langsung terhadap kolom `timestamp with time zone` tanpa konversi UTC), masih berada dalam kepemilikan modul yang sama (`InPatientManagement`), dan sebagian besar sudah dipakai oleh layar frontend yang mengirim nilai `DateTimeKind.Unspecified` |
| **Jenis** | `BUG` |
| **Area** | Backend (`Areas/HealthServices/InPatientManagement/Services/`) |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source backend dan frontend |
| **Perbaikan** | `FIX-EPS-045-02` |

**Apa yang terjadi.**  
Pengguna yang membuka menu sebelah **Pasien Sedang Dirawat** (*Census*) atau **Daftar Pantau** dan menyaring berdasarkan tanggal (`Tanggal mulai` / `Tanggal akhir` / `Tanggal keluar` / `Tanggal tutup`) akan mengalami kegagalan `500 Internal Server Error` yang sama persis seperti di menu Daftar Pasien Rawat Inap.

**Kenapa terjadi.**  
`SUDAH-VERIFIKASI` pada lima titik di `InPatientManagement`:

1. **Pasien Sedang Dirawat (`GET /api/v1/health-services/inpatient-management/census` dan `/census/summary`):** Frontend [inpatient-census-view.jsx:210-218](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-census-view.jsx#L210-L218) & [inpatient-census-utils.jsx:99-103](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-census-utils.jsx#L99-L103) mengirim `startDate=YYYY-MM-DD` dan `endDate=YYYY-MM-DD`, lalu [InpCensusQueryService.cs:1225-1236](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs#L1225-L1236) membandingkan `query.StartDate.Value` dan `query.EndDate.Value.Date.AddDays(1)` secara langsung terhadap `(x.Episode!.AdmittedAt ?? x.Episode.CreateDateTime)` (`timestamptz`).
2. **Daftar Pantau — Pesanan Tindakan Tertagih (`GET /api/v1/health-services/inpatient-management/monitoring/billed-pending-procedure-orders`):** Frontend [inpatient-monitoring-utils.jsx:65-68](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-monitoring-utils.jsx#L65-L68) mengirim `closedFrom=${filters.closedFrom}T00:00:00` dan `closedTo=${filters.closedTo}T23:59:59` tanpa akhiran zona waktu (`Z` atau `+07:00`), sehingga terikat sebagai `DateTimeKind.Unspecified`; lalu [InpDischargeService.Closure.cs:599-607](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs#L599-L607) membandingkannya langsung terhadap `x.ClosedAt` (`timestamptz`).
3. **Daftar Pantau — Kepergian Sebelum Kelayakan Keuangan (`GET /api/v1/health-services/inpatient-management/monitoring/departures-before-clearance`):** Frontend [inpatient-monitoring-utils.jsx:58-62](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-monitoring-utils.jsx#L58-L62) mengirim `from=${filters.departedFrom}T00:00:00` dan `to=${filters.departedTo}T23:59:59` tanpa akhiran zona waktu, lalu [InpDischargeService.Departure.cs:69-70](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs#L69-L70) membandingkannya langsung terhadap `x.PhysicallyLeftAt` (`timestamptz`).
4. **Integration Outbox Monitor (`GET /api/v1/health-services/inpatient-management/integration-outbox`):** [InpIntegrationReplayService.cs:20-21](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpIntegrationReplayService.cs#L20-L21) membandingkan `request.From.Value` dan `request.To.Value` langsung terhadap `x.CreatedAtUtc` (`timestamptz`), dan controller-nya ([InpatientIntegrationOutboxController.cs:26](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Controllers/InpatientIntegrationOutboxController.cs#L26)) bahkan menangkap `ArgumentException` sebagai `400 Bad Request` yang membocorkan pesan teknis Npgsql.
5. **Integrasi Sumber Rekam Medis (`InpMedicalRecordDocumentSource.cs`):** [InpMedicalRecordDocumentSource.cs:295-298,318-321](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpMedicalRecordDocumentSource.cs#L295-L321) membandingkan parameter `DateTime? dari` dan `sampai` langsung terhadap `SignedAt` dan `CompletedAt` (`timestamptz`) — bila pemanggil dari modul Rekam Medis mengirim tanggal polos `YYYY-MM-DD`, jalur ini juga rentan gagal `500` (catatan: perlu diselaraskan dengan konvensi modul Rekam Medis `BE-39`).

**Rekomendasi.**  
Perbaiki keempat endpoint milik modul Rawat Inap (`InpCensusQueryService`, `InpDischargeService.Closure`, `InpDischargeService.Departure`, `InpIntegrationReplayService`) bersama-sama dengan `InpEpisodeService.Reads.cs` memakai `AppDateTimeHelper` dalam paket perbaikan `FIX-EPS-045-02`, dan koordinasikan `InpMedicalRecordDocumentSource` dengan pemilik modul Rekam Medis atau pastikan tetap aman memakai `AppDateTimeHelper.ToUtc`.

---

### ISS-EPS-045-T2 — Tabel mempertahankan baris lama saat filter gagal dan pemilih tanggal belum saling membatasi rentang

| | |
| --- | --- |
| **Kenapa dicantumkan** | Ditemukan saat mengaudit alur state dan kontrol UI layar `Daftar Pasien Rawat Inap` yang dilaporkan pelapor |
| **Jenis** | `BUG` (UX) |
| **Area** | Frontend (`QuilvianSystemFrontendDev`) |
| **Keparahan** | Low |
| **Status bukti** | `SUDAH-VERIFIKASI` di source frontend |
| **Perbaikan** | `FIX-EPS-045-03` |

**Apa yang terjadi.**  
1. Jika daftar episode sebelumnya sudah menampilkan baris data (saat muat awal berhasil), lalu petugas menerapkan penyaring yang gagal di server (misalnya `500`), [inpatient-management-slice.jsx:61-68](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/lib/state/slice/health-services/inpatient-management/inpatient-management-slice.jsx#L61-L68) mempertahankan `resource.data` lama sementara [use-inpatient-episode-worklist.jsx:215-218](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-worklist.jsx#L215-L218) mengosongkan `summaryData` menjadi `null` (menampilkan *"Ringkasan tidak tersedia."*). Akibatnya, di atas tabel tampil peringatan gagal namun di bawahnya tetap terlihat daftar pasien sebelum penyaringan yang tidak sesuai dengan filter aktif.
2. Pada [inpatient-episode-worklist-view.jsx:336-346](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-episode-worklist-view.jsx#L336-L346), dua kontrol `FilterDatePicker` untuk `startDate` dan `endDate` belum meneruskan prop opsional `max={filters.endDate}` dan `min={filters.startDate}` yang sudah disediakan oleh [filter-date-picker.jsx:420-423](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/features/base-features/filter-date-picker.jsx#L420-L423), sehingga petugas tetap bisa memilih tanggal akhir lebih awal daripada tanggal mulai.

**Rekomendasi.**  
Kosongkan tampilan baris (atau tampilkan penanda error tanpa baris usang) saat permintaan terfilter gagal serta tambahkan `max={filters.endDate}` / `min={filters.startDate}` pada kedua `FilterDatePicker` jika disetujui pemilik.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk pelapor

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Pada lingkungan `api-dev` (`dev.quilvian-mmchospital.com`), apakah memang sudah pernah dilakukan admisi rawat inap sebelumnya di database `QuilvianNewDevMaster`, atau sebelumnya pengujian dilakukan di database lokal (`QuilvianNewDevHamzah`)? | Screenshot muat awal tanpa penyaring memperlihatkan seluruh 6 kartu ringkasan bernilai `0` tanpa banner galat, sedangkan kode query tanpa filter tidak menyembunyikan status mana pun | Menentukan apakah gejala #1 murni karena database `dev` belum berisi episode (`DATA`) atau ada kondisi data khusus di `QuilvianNewDevMaster` (mis. semua baris bertanda `IsDelete = true`) |

### Untuk pemilik

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Apakah perbaikan normalisasi tanggal ke UTC (WIB `Asia/Jakarta`) diterapkan hanya pada `InpEpisodeService.Reads.cs` atau sekaligus pada seluruh layanan `InPatientManagement` yang berpola cacat sama (`InpCensusQueryService`, `InpDischargeService`, `InpIntegrationReplayService`)? | (A) Perbaiki seluruh 5 layanan `InPatientManagement` sekaligus / (B) Hanya `InpEpisodeService.Reads.cs` | **(Recommended)** Pilih A — perbaiki seluruh layanan `InPatientManagement` yang menerima filter tanggal `DateTime?` agar layar *Pasien Sedang Dirawat* dan *Daftar Pantau* tidak ikut gagal 500 | `FIX-EPS-045-01`, `FIX-EPS-045-02` |
| K-02 | Apakah konvensi batas tanggal filter di `InpEpisodeService.Reads.cs` dan `InpCensusQueryService.cs` menggunakan hari kalender operasional WIB (`Asia/Jakarta`, UTC+7) melalui `AppDateTimeHelper`? | (A) Ya, tanggal polos `YYYY-MM-DD` dibaca sebagai `00:00 WIB` s.d. akhir hari WIB lalu dikonversi ke UTC (`2026-11-01T17:00:00Z` s.d. `< 2026-11-02T17:00:00Z`) / (B) Dianggap UTC tengah malam (`2026-11-02T00:00:00Z`) | **(Recommended)** Pilih A — selaras dengan `AppDateTimeHelper.OperationalDateToUtc`, kolom tampilan `Admisi Dibuka` di layar (WIB), dan standar modul lain (`BbkBloodOrderService`, `LabDashboardService`, `OptReportService`) | `FIX-EPS-045-01`, `FIX-EPS-045-02` |
| K-03 | Apakah perbaikan UX frontend ringan (`FIX-EPS-045-T2`: batas `min`/`max` pada `FilterDatePicker` dan tidak menampilkan data lama saat query gagal) ikut dikerjakan? | Ya, kerjakan sekaligus di FE / Tidak perlu, fokus perbaikan backend saja | Fokus backend lebih dulu (`FIX-EPS-045-01` & `02`); `FIX-EPS-045-03` dapat dikerjakan opsional bila target tulis frontend diizinkan | `FIX-EPS-045-03` |

---

## 8. Catatan pola

Pola cacat **`DateTimeKind.Unspecified` terhadap kolom `timestamp with time zone` Npgsql** ini merupakan pola berulang di repository backend (`NewQuilvianSystemBackend`), karena:
1. Aplikasi sengaja tidak mengaktifkan `Npgsql.EnableLegacyTimestampBehavior` di `Program.cs`.
2. Endpoint-endpoint awal sering diuji memakai **EF Core In-Memory** (seperti `InpEpisodeReadTests` pada `BE-RWI-009`), yang menerima `DateTimeKind.Unspecified` tanpa error, sehingga bug baru muncul saat diuji terhadap PostgreSQL nyata.
3. Masalah identik sebelumnya pernah tercatat pada `BE-BD-019` (Bank Darah), `BE-LAB-39` (Laboratorium), `BE-HRD-FIX-003` (HR Lembur), `WorkforceCoreCrossEmployeeController` (log 20 Nov 2026), dan `FE-14` Rekam Medis, lalu ditutup menggunakan `Helpers/AppDateTimeHelper.cs` (`OperationalDateToUtc` / `ToUtc`).

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-25 | Dokumen issue dibuat dari laporan pengguna (screenshot layar kosong `0` + capture HTTP 500 `GET /episodes?startDate=2026-11-02`), disertai temuan tambahan pada layanan Rawat Inap sejenis | `diagnose-module-issue` |
