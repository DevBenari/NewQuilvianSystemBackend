# Laporan Perubahan Backend — `BE-ACC-P2-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-021` |
| Judul | `POST /accounting-events` — terima dan jurnal seketika |
| Slice | `P2-1` — Wave B kotak masuk kejadian |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-021` (revisi 4, `APPROVED`) |
| Trace | `FR-P2-001`..`006`, `009`, `012`; `ACC-DEC-020`, `035`, `045`, `046`, `047`, `048`, `056`, `058`, `060`, `074`, `075`, `084`, `085`, `086`, `088` |
| Contract version | `ACC-API-0.12` grup Accounting Event; `ACC-VALIDATION-0.8` bagian 1–2; `ACC-STATE-0.4`; `ACC-XMOD-0.3` bagian 3–7; `02-backend-architecture.md` bagian 22.4 |
| Dependency | `BE-ACC-P2-019` ✅, `BE-ACC-P2-020` ✅, `BE-ACC-P2-018` ✅ |
| Klasifikasi | `HIGH` — endpoint baru yang menulis ke buku besar, dua transaksi database, perubahan pada `AccJournalService` |
| Task mode | `BACKEND` |
| Target tulis | Source backend: controller (`POST /` saja), service, DTO, options, satu method baru di `AccJournalService`, registrasi di blok Accounting `Program.cs`; laporan ini; baris status roadmap. **Tidak** termasuk migration, build, commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `8535dd56` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 24 September 2026 |
| Status | **✅ SELESAI** — 24 September 2026. 11 dari 11 acceptance terpetakan ke source; build Rizki berhasil; uji panggil Swagger oleh Rizki (dibantu agent lain) lulus seluruh butir yang diminta kolom Verifikasi — lihat bagian 9. Riwayat: 🟡 pada hari yang sama, menunggu build dan uji |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `AccountingEvent`; menyentuh `JournalManagement` |
| Registry | `Acc` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (controller, service, DTO, options); `TOUCHED LEGACY` pada `AccJournalService` (satu method baru, nol baris lama diubah) dan `Program.cs` (dua baris di blok Accounting) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `DbContext`), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001` (log beraktor, tanpa nominal), `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-CODE-002`/`003` (nomor jurnal tetap dialokasikan `AccJournalService`) |
| Standar endpoint | **Transaksi**, arketipe *worklist* penerimaan — **bukan** master data. Tidak ada `GET /options`, `PATCH /status`, `DELETE`. Endpoint baca, coba ulang, dan abaikan adalah task `024`/`025` |

## 1. Yang dikerjakan

Pintu masuk kejadian keuangan dari Finance kini ada:
`POST api/v1/corporate/accounting/accounting-events`. Satu permintaan membawa satu kejadian.
Kejadian **disimpan lebih dahulu**, baru dijurnal. Finance langsung tahu hasilnya lewat tanda
terima:

| Hasil | Status kejadian | HTTP | Tanda terima |
| --- | --- | ---: | --- |
| Aturan posting ada, jurnal terbentuk | `Terjurnal` | `201` | Nomor jurnal dan kode periode |
| Jenis belum terdaftar, aturan kosong, komponen tak dipakai, komponen kurang | `Tertahan` | `422` | `HoldReasonCode` |
| Kejadian tersimpan tetapi penjurnalan gagal (gangguan teknis, periode belum ada, jurnal ditolak aturan jurnal) | `Diterima` | `201` | Tanpa nomor jurnal |
| Kiriman ulang (kunci pertama **atau** kedua) | Keadaan terkini | `200` | Tanda terima keadaan terkini |
| Bidang kosong, data pasien, panjang berlebih, komponen tidak sah | — | `400` | Tidak ada, nol baris tersimpan |
| Bukan rupiah | — | `409` | Tidak ada |
| Badan hukum tidak ditemukan | — | `422` | Tidak ada |
| Badan hukum bukan badan hukum utama | — | `403` | Tidak ada |

## 2. Alur — langkah demi langkah

| Langkah | Isi | Transaksi |
| ---: | --- | --- |
| 1 | Penjaga badan hukum utama; validasi isian; larangan data pasien; badan hukum | — |
| 2 | Cari kiriman ulang lewat kedua kunci → `200` | — |
| 3 | Cari jenis kejadian aktif berdasarkan kode; validasi yang bergantung jenis | — |
| 4 | Simpan kejadian `Diterima` + komponennya. Tabrakan unique index akibat kiriman bersamaan → baca ulang → `200` | **T1** |
| 5 | Jenis / aturan / komponen tidak lengkap → `Tertahan` bersyarat + percobaan 1 | **T2** |
| 6 | Tentukan tanggal jurnal: periode tanggal kejadian; bila periode itu tidak menerima jenis jurnal aturan, **periode terbuka berikutnya** (`ACC-DEC-047`) — tanggal dokumen asli tetap | — |
| 7 | Buat jurnal lewat `AccJournalService.CreateAsync`; bila aturan `LangsungSahkan`, sahkan lewat `SahkanDariKejadianAsync`; ubah status `Diterima` → `Terjurnal` **bersyarat**; percobaan 1 berhasil | **T2** |
| 8 | Bila langkah 6–7 gagal: T2 dibatalkan seluruhnya (termasuk nomor jurnal), percobaan 1 gagal dicatat, status tetap `Diterima` | **T3** |

**Contoh berangka.** Aturan `PENGAKUAN-PIUTANG` (perlakuan `LangsungSahkan`, jenis jurnal `JU`):
`TOTAL` debit `1-1201 Piutang Penjamin`, `TOTAL` kredit `4-1001 Pendapatan Rawat Jalan`. Kejadian
`EVT-100` Rp 10.000.000 tanggal 8 September 2026 menghasilkan jurnal `JU/2026/09/000xx` berstatus
`Posted`, dua baris Rp 10.000.000, nomor dokumen `EVT-100`. Bila periode September sudah
ditutup, jurnal bertanggal 1 Oktober 2026, tanggal dokumen tetap 8 September.

## 3. Pemetaan acceptance

| # | Acceptance kartu | Bukti di source | Status |
| ---: | --- | --- | :---: |
| 1 | `[AccessPermission("AccountingEvent", "Receive")]` | `AccountingEventController.Receive` — `ControllerName = "AccountingEvent"`, `[AccessAction("Receive", …)]`, `[AccessPermission("AccountingEvent", "Receive")]` | ✅ |
| 2 | `400`/`409`/`403` tanpa baris tersimpan | `PeriksaIsian`, `PeriksaBadanHukumAsync`, `PeriksaIsianMenurutJenis` — seluruhnya sebelum `SaveChangesAsync` pertama | ✅ |
| 3 | Kejadian di-commit (T1) sebelum dijurnal (T2) | `TerimaAsync` → `SaveChangesAsync` tanpa transaksi terbuka, lalu `ProsesKejadianAsync` membuka `BeginTransactionAsync` | ✅ |
| 4 | Aturan ada → `201` + `JournalNumber`, `Terjurnal` | `ProsesKejadianAsync` langkah 7; `BalasKejadianBaru` | ✅ |
| 5 | Empat `HoldReasonCode` → `422`, nol jurnal | `TahanAsync` dengan konstanta `AlasanJenisBelumTerdaftar`, `AlasanAturanPostingKosong`, `AlasanKomponenTidakDipakai`, `AlasanKomponenKurang` | ✅ |
| 6 | Gangguan teknis di T2 → `201` tanpa nomor jurnal, `Diterima`, percobaan gagal tercatat | `catch` pada `ProsesKejadianAsync` → `CatatPercobaanGagalAsync` | ✅ |
| 7 | Kiriman ulang kunci pertama atau kedua → `200`, nol jurnal baru | `CariKirimanUlangAsync` (`EventNumber` **atau** empat kolom) → `BalasKirimanUlangAsync` | ✅ |
| 8 | Dua kiriman bersamaan → satu kejadian, satu jurnal | Unique index (`019`) + `catch (DbUpdateException) when MelanggarUniqueIndex` → baca ulang → `200` | ✅ |
| 9 | Periode tertutup → jurnal di periode terbuka berikutnya | `TentukanTanggalAkuntansiAsync` memakai `AccAccountingPeriodService.AlasanPenolakanJenisJurnal` | ✅ |
| 10 | Perpindahan dari `Diterima` bersyarat | `ExecuteUpdateAsync` dengan `Where(x => x.EventStatus == statusAsal)`; nol baris → rollback, tanda terima keadaan terkini | ✅ |
| 11 | `SubledgerBalance` pada jenis `Transaksi` → `400` | `PeriksaIsianMenurutJenis` | ✅ |

## 4. Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Controllers/AccountingEventController.cs` | Baru | `POST /`; `422` tetap membawa tanda terima |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Services/AccAccountingEventService.cs` | Baru | `TerimaAsync`, `ProsesKejadianAsync` (dibuat `public` supaya `023`/`025` memakai jalur yang sama) |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Services/AccAccountingEventSchedulerOptions.cs` | Baru | `SystemActorUserId`, `GracePeriodSeconds` (dirancang bagian 22.10) |
| `Areas/Corporate/AccountingManagement/AccountingEvent/DTOs/AccountingEventDtos.cs` | Baru | `ReceiveAccountingEventRequest`, komponen, `SubledgerBalance`, `AccountingEventReceiptDto` |
| `Areas/Corporate/AccountingManagement/JournalManagement/Services/AccJournalService.cs` | Diperbarui | + `SahkanDariKejadianAsync` — Draft → Posted dengan sembilan syarat diperiksa ulang dan riwayat `Posted` tercatat. Nol baris lama diubah |
| `Program.cs` | Diperbarui | Satu `using`; di blok Accounting: `Configure<AccAccountingEventSchedulerOptions>` dan `AddScoped<AccAccountingEventService>` |

## 5. Keputusan implementasi dan delta kontrak

| Hal | Pilihan | Alasan |
| --- | --- | --- |
| Deteksi data pasien | `[JsonExtensionData]` menangkap bidang tak dikenal; nama yang mengandung *patient/pasien/medicalrecord/rekammedis/mrn/visit/kunjungan/encounter/doctor/dokter* → `400` | Satu-satunya cara menegakkan `ACC-DEC-056` tanpa membaca ulang body; sekaligus membuat `RawPayload` utuh |
| Pelaku jurnal | `Accounting:AccountingEventScheduler:SystemActorUserId`; bila kosong, **akun pemanggil** | `ACC-DEC-074`. Jatuh ke pemanggil lebih mudah ditelusuri daripada `Guid.Empty` yang dipakai jurnal berulang. **Bagian konfigurasi `Accounting` belum ada di `appsettings.json`** |
| Pengesahan otomatis | Method baru `SahkanDariKejadianAsync`, bukan `PostAsync` | `PostAsync` menuntut `Approved`; `ACC-DEC-045` "langsung disahkan" tanpa pengajuan. Sembilan syarat tetap diperiksa |
| Jurnal ditolak aturan jurnal (tak seimbang, akun nonaktif, periode belum ada) | Diperlakukan seperti gangguan: `201` tanpa jurnal, `Diterima`, percobaan gagal | Kontrak hanya punya empat alasan tahan. Sesudah `023` berdiri, kejadian ini menjadi `Gagal` setelah 3 coba ulang dan **menahan tutup bulan** — tidak diam-diam hilang |
| Baris aturan bernilai nol | Dilewati | Aturan jurnal menolak baris bernilai nol |
| Jenis berkode `SaldoSubledger` | `409` sementara, tidak disimpan | Jalurnya `BE-ACC-P2-028`. Praktis tidak terjangkau: `EventKind` baru dapat diubah lewat `022` |
| Jenis nonaktif | Diperlakukan belum terdaftar → `Tertahan` | Jenis nonaktif tidak boleh menghasilkan jurnal |
| `SourceModule` | Tidak dibatasi `Finance` | Kontrak tidak menetapkan penolakan |
| `EventOccurredAt` | Disimpan dalam UTC | Npgsql menolak `DateTimeOffset` ber-offset non-nol pada `timestamptz` |
| Komentar kode | Nol `//` baru | Konvensi owner |

## 6. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| Tinjauan diff dan cakupan | Berkas pada bagian 4 saja |
| Akses (`role-access-rules`) | `ControllerName` = argumen 1 `[AccessPermission]`; argumen 2 = argumen 1 `[AccessAction]`; `AccessType` dari `AccessTypes`; nol `IsInRole` |
| Log | Muatan: nomor, kode jenis, modul, kode HTTP, status, alasan tahan, nomor jurnal. **Tanpa** `SourceTransactionId`, nominal, dan `RawPayload`; pesan disaring `TanpaNominal` |
| `dotnet build` | **Berhasil** — Rizki, 24 September 2026. Jumlah warning tidak dilaporkan |
| Uji panggil Swagger | **Lulus** — bagian 9 |
| Migration | Tidak dibutuhkan |

## 7. Skenario uji Swagger (untuk menaikkan ke ✅)

Prasyarat data: jenis `PENGAKUAN-PIUTANG` aktif; aturan posting aktif untuk jenis itu pada badan
hukum utama; periode akuntansi terbuka untuk tanggal uji; pengguna ber-hak `AccountingEvent : Receive`
(SuperAdmin untuk uji pengembang).

| # | Kiriman | Diharapkan |
| ---: | --- | --- |
| 1 | `EVT-UJI-001`, `PENGAKUAN-PIUTANG`, `IDR`, lengkap | `201`, `Terjurnal`, `JournalNumber` terisi |
| 2 | Kirim ulang #1 persis | `200`, nomor jurnal sama, jumlah jurnal tidak bertambah |
| 3 | `EVT-UJI-002`, `EventTypeCode` = `JENIS-TIDAK-ADA` | `422`, `Tertahan`, `EVENT_TYPE_NOT_REGISTERED` |
| 4 | `EVT-UJI-003`, `CurrencyCode` = `USD` | `409`, tidak tersimpan |
| 5 | `EVT-UJI-004` tanpa `AccountingDate` | `400`, menyebut `AccountingDate` |
| 6 | `EVT-UJI-005` dengan bidang tambahan `PatientName` | `400` identitas pasien |
| 7 | `EVT-UJI-006` = transaksi, jenis, versi sama dengan #1 tetapi nomor baru | `200`, menunjuk kejadian #1 |

## 8. Risiko dan langkah berikutnya

| Hal | Isi |
| --- | --- |
| Kejadian yang tertinggal `Diterima` | Belum ada penjadwal (`023`); sampai itu, kejadian tanpa jurnal diam di `Diterima` |
| Menaikkan ke ✅ | Build owner `0 error` dan skenario 1–5 dijalankan |
| Berikutnya | `022`, `024` (paralel), lalu `023`, `025`, `026` |

## 9. Hasil uji Swagger — dilaporkan Rizki, 24 September 2026

Dijalankan Rizki lewat Swagger dengan bantuan agent lain. Yang diterima di sini adalah **ringkasan
hasil**, bukan tangkapan respons mentah:

| Butir | Hasil |
| --- | :---: |
| Kejadian dapat diterima (`201`) | ✅ PASS |
| Validasi bidang wajib (`400`) | ✅ PASS |
| Validasi jenis kejadian (`422` `EVENT_TYPE_NOT_REGISTERED`) | ✅ PASS |
| Mekanisme tahan | ✅ PASS |
| Pembentukan jurnal | ✅ PASS |
| Idempotensi (`200` kiriman ulang) | ✅ PASS |
| Pembatasan mata uang (`409`) | ✅ PASS |
| Tidak membuat jurnal ganda | ✅ PASS |
| Bentuk balasan galat sesuai kontrak | ✅ PASS |

**Tidak tercantum pada ringkasan:** skenario 6 (bidang `PatientName` → `400`). Penolakan itu ada di
source (`PeriksaIsian`) tetapi belum terbukti di runtime; dicatat sebagai sisa verifikasi, bukan
syarat kolom Verifikasi kartu.
