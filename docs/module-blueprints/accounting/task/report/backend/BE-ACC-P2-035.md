# Laporan Perubahan Backend — `BE-ACC-P2-035`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-035` |
| Judul | Hapus, sunting, dan tolak draft jurnal hasil kejadian |
| Slice | `P2-2` lanjutan |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-035` (revisi 8, `APPROVED` Rizki 29 September 2026) |
| Trace | `ACC-DEC-116`..`121`; `FR-P2-045`, `046`, `047`, sisi backend `FR-P2-048`; `UAT-P2-34`..`38` |
| Contract version | `ACC-API-0.15`, `ACC-STATE-0.6`, `ACC-VALIDATION-0.11` — approved `GATE-DESAIN-0929`; `02-backend-architecture.md` bagian 24 |
| Dependency | `BE-ACC-P2-005` ✅, `021` ✅, `025` ✅, `034` ✅ |
| Pasangan frontend | `FE-ACC-P2-019` — diuji sekali bersama |
| Klasifikasi | `MEDIUM` — skor 6: berkas diperiksa 9–20 (1), berkas diubah 4 (1), logika bisnis sedang dengan transaksi lintas dua submodule (2), perubahan kontrak aditif yang sudah approved (1), perilaku query pada tabel yang ada (1); database, keamanan, UI 0 |
| Task mode | `BACKEND` (lanjutan audit kesiapan 29 September 2026, perintah Rizki "build berpasangan") |
| Target tulis | `AccJournalService`, `JournalDetailResponse`, `AccAccountingEventService`, `AccPeriodClosingService`; laporan ini; baris status roadmap, traceability, MODULE-STATUS. **Tidak** termasuk migration, build, maupun commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `618b206e` (branch `rizkiG`) + working tree `BE-ACC-P2-029`/`030`/`034` dan dokumen yang belum di-commit |
| Status | **✅ SELESAI — 29 September 2026.** 11 dari 11 acceptance terpetakan ke source; empat berkas, +135/−10 milik task ini; nol migration, nol endpoint baru, nol hak baru, nol `Program.cs`, nol `//` baru (diperiksa ulang pada commit `8f530926`). Build Rizki terbukti tak langsung: `QuilvianSystemBackend.dll` 29 September 2026 15.51, sesudah source terakhir diubah 15.37; jumlah warning tidak terbukti. Uji gabungan S1–S13 + L1–L6 dijalankan Rizki 16.10.14–16.10.53 WIB lewat skrip Playwright agen Antigravity, tanpa SQL; JSON mentah dan delapan tangkapan layar diperiksa agent 30 September 2026 (bagian 5.3). Acceptance (1)–(5) dan (8) terbukti runtime; (6) dan (7) runtime untuk jurnal hasil kejadian, sisi jurnal manual dan `submit-closing` dari source; (9), (10), (11) dari source sesuai resep. UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: 🟡 29 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `JournalManagement`, `AccountingEvent`, `AccountingPeriod` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (registry repository yang berlaku, `ACC-TD-015`) |
| Keberlakuan | `TOUCHED LEGACY` — `AccJournalService` (MVP, `012`, `034`), `AccAccountingEventService` (`021`..`025`), `AccPeriodClosingService` (`005`, `026`, `029`), `JournalDetailResponse` (MVP). Nol entity, nol configuration, nol berkas baru |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service; perpindahan status kejadian tetap milik `AccAccountingEventService`), `QBE-API-001` (`AccountingServiceResult` → kode status yang ada), `QBE-VAL-001` (`409` berpesan persis kontrak), `QBE-DTO-001` (bidang aditif). Tidak berlaku: `QBE-PERM-001` (endpoint dan atribut akses tidak berubah), `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-002/003`, `QBE-LOG-001` (pencatatan jalur hapus yang ada tidak diubah) |
| Governance yang dibaca | `AGENTS.md` backend; `rules/backend/` suite; registry repository — sama dengan `BE-ACC-P2-034` pada hari yang sama |

## 1. Masalah yang diperbaiki

Tiga celah pada jurnal yang lahir dari kejadian keuangan (`ACC-DEC-116`..`121`):

1. **Hapus draft membuat kejadian yatim.** Kejadian tetap `Terjurnal` menunjuk jurnal yang sudah
   terhapus — transaksinya keluar dari buku tanpa jejak di Kotak Masuk dan tanpa menahan tutup bulan.
2. **Draft kejadian dapat disunting** selama tidak menyentuh control account, sehingga angkanya bisa
   berbeda dari catatan Finance.
3. **Jurnal kejadian yang ditolak penyetuju** tidak lagi dihitung sebagai jurnal belum disahkan, jadi
   lolos tutup bulan diam-diam; dan karena status Ditolak tidak dapat dihapus maupun (bila menyentuh
   control account) disunting, ia menjadi jalan buntu.

## 2. Proses bisnis

1. Finance mengirim kejadian; aturan posting berperlakuan Buat Draft menyusun jurnal draft.
2. Bila draft itu salah — misalnya aturan posting salah akun — petugas membetulkan aturannya lalu
   **menghapus** draft. Kejadian kembali **Gagal**, dengan catatan "Jurnal draft JU/… dihapus oleh …".
3. Petugas menekan **Coba Ulang** di Kotak Masuk; draft baru terbit dengan akun yang benar. Bila
   kejadian memang tidak perlu dijurnal, petugas memilih **Abaikan** beralasan.
4. Selama kejadian Gagal, tutup bulan tertahan. Jurnal kejadian yang ditolak penyetuju juga menahan
   tutup bulan sampai diajukan ulang atau dihapus.
5. Draft kejadian tidak pernah disunting; ia mencerminkan catatan Finance apa adanya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `JournalManagement/Services/AccJournalService.cs` | `UpdateAsync`, `DeleteAsync`, `PeriksaDapatDisunting`, `PetakanRincianAsync`, `TindakanTersedia`, `AmbilNamaAktorAsync`, `BerasalDariJalurOtomatisAsync` (`034`) |
| `JournalManagement/Controllers/JournalController.cs` | Satu-satunya pemanggil `UpdateAsync`/`DeleteAsync`; atribut akses `Journal : Update`/`Delete` |
| `AccountingEvent/Services/AccAccountingEventService.cs`, `Models/AccAccountingEvent*.cs` | Pola perpindahan ke `Gagal` bersyarat, `TambahPercobaan`, `PanjangPesanGagalMaksimum`, `AttemptCount` (hanya dipakai penjadwal untuk `Diterima`) |
| `AccountingPeriod/Services/AccPeriodClosingService.cs` | `HitungJurnalBelumDisahkanAsync` — satu-satunya sumber hitungan, dipakai daftar periksa dan `submit-closing` |
| `Repositories/Configurations/.../AccAccountingEventConfiguration.cs` | `HasIndex(x => x.JournalId)` sudah ada — nol migration |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
|---|---|
| `AccountingEvent/Services/AccAccountingEventService.cs` | + `public static KembalikanKeGagalKarenaJurnalDihapusAsync(db, accountingEventId, journalId, pesan, actorUserId, sekarang, ct)`: `ExecuteUpdate` bersyarat (`Terjurnal` **dan** `JournalId` = jurnal itu, tidak terhapus) → `Gagal`, `JournalId` kosong; bila berubah, tambah satu `AccAccountingEventAttempt` gagal bernomor terakhir + 1. Tidak memanggil `SaveChanges` — pemanggil yang menyimpan di dalam transaksinya. +41/−0 |
| `JournalManagement/Services/AccJournalService.cs` | + `using ...AccountingEvent.Services`. **`UpdateAsync`**: sesudah pemeriksaan status, jurnal hasil kejadian → `409` berpesan kontrak. **`DeleteAsync`**: jurnal hasil kejadian `Rejected` dikecualikan dari "yang ditolak tidak dapat dihapus"; jurnal hasil kejadian dihapus di dalam **satu transaksi** bersama `KembalikanKeGagalKarenaJurnalDihapusAsync`; kejadian berubah bersamaan → rollback, `409`; pesan sukses menyebut kejadiannya. Jalur jurnal manual tidak berubah. Penandaan terhapus diangkat ke `TandaiJurnalTerhapus` (komentar lamanya ikut pindah, tidak ditulis ulang). **`PetakanRincianAsync`**: mengisi `SourceAccountingEventId`/`Number`. **`TindakanTersedia`**: + parameter `hasilKejadian` — tanpa `update` pada jurnal hasil kejadian; `delete` pada `Rejected` hasil kejadian. + `CariKejadianSumberAsync` dan record `KejadianSumber`. Satu komentar lama yang kini keliru ("Tidak dapat dihapus." pada status Ditolak) **dihapus**, bukan diganti. +86/−9 |
| `JournalManagement/DTOs/JournalDtos.cs` | `JournalDetailResponse` + `SourceAccountingEventId` (`Guid?`), `SourceAccountingEventNumber` (`string?`). +4/−0 |
| `AccountingPeriod/Services/AccPeriodClosingService.cs` | `HitungJurnalBelumDisahkanAsync`: `StatusBelumDisahkan` **atau** (`Rejected` **dan** ada kejadian tidak terhapus yang menunjuknya) — subquery terkorelasi. +4/−1 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Hal | Dampak |
|---|---|
| API | Sesuai `ACC-API-0.15`: dua bidang aditif pada rincian jurnal; `AvailableActions` sesuai asal-usul; `409` baru pada `PUT` dan `DELETE`; `DELETE` jurnal hasil kejadian mengubah kejadiannya. Nol endpoint baru |
| Database | Query baca tambahan lewat `JournalId` (ber-index). Satu `ExecuteUpdate` + satu `INSERT` percobaan per penghapusan jurnal hasil kejadian, di dalam transaksi. Nol migration |
| Keamanan | Atribut akses tidak berubah; hapus jurnal hasil kejadian memakai `Journal : Delete` (`ACC-DEC-121`). Nol hardcode peran |
| `Program.cs`, `ApplicationDbContext` | Tidak tersentuh |

### 3.4 Keputusan implementasi yang perlu diketahui

1. **`PeriksaDapatDisunting` tidak diubah.** Arsitektur bagian 24.2 menyebutnya, tetapi pengecualian
   `Rejected` hasil kejadian cukup dan lebih jelas ditempatkan di `DeleteAsync` sendiri — method bersama
   itu dipakai jalur lain yang tidak boleh ikut terkecualikan.
2. **Urutan pemeriksaan sesuai kontrak.** Status jurnal lebih dulu (jurnal `PendingApproval`,
   `Approved`, `Posted` tetap `409` dengan pesan lama), baru asal-usul.
3. **Kejadian berubah bersamaan** — misalnya dua petugas menghapus pada saat yang sama, atau penjadwal
   memproses kejadian itu — terdeteksi karena `ExecuteUpdate` bersyarat mengubah nol baris; jurnal tidak
   ikut terhapus.
4. **Nama pada catatan riwayat** memakai pembaca nama aktor `BE-ACC-015`; bila pengguna tidak
   ditemukan, id penggunanya yang ditulis.
5. **Tidak ada `SaveChanges` di method static kejadian**, sehingga penandaan jurnal, perpindahan
   kejadian, dan baris percobaan tersimpan dalam satu `SaveChanges` + satu commit.

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Journal Management / Journal — perilaku yang berubah

| Method | Path | Sebelum | Sesudah |
|---|---|---|---|
| `GET` | `api/v1/corporate/accounting/journals/{id}` | Tanpa asal; `update` dan `delete` hanya menurut status | + `sourceAccountingEventId`, `sourceAccountingEventNumber`; jurnal hasil kejadian tanpa `update`, dengan `delete` pada `Draft` dan `Rejected` |
| `PUT` | `api/v1/corporate/accounting/journals/{id}` | Draft kejadian ke akun biasa dapat disunting | `409` "Jurnal {nomor} dibentuk dari kejadian {nomor kejadian} dan tidak dapat diubah. Hapus jurnal ini untuk menjurnal ulang kejadiannya, atau minta Finance mengirim kejadian pembalik." |
| `DELETE` | `api/v1/corporate/accounting/journals/{id}` | Kejadian tetap `Terjurnal`; `Rejected` selalu `409` | `200` "Jurnal draft {nomor} berhasil dihapus. Kejadian {nomor kejadian} kembali berstatus Gagal."; `Rejected` hasil kejadian diterima; `409` "Kejadian {nomor kejadian} berubah bersamaan. Muat ulang rincian jurnal lalu coba lagi." |

#### Dampak pada grup lain — bentuk tidak berubah

| Method | Path | Perilaku |
|---|---|---|
| `GET` | `api/v1/corporate/accounting/periods/{id}/closing-checklist` | `UNPOSTED_JOURNALS` ikut menghitung jurnal hasil kejadian `Rejected` |
| `POST` | `api/v1/corporate/accounting/periods/{id}/submit-closing` | `409` jurnal belum disahkan dengan hitungan yang sama |
| `GET` | `api/v1/corporate/accounting/accounting-events/{id}` | Kejadian yang jurnalnya dihapus tampil `Gagal`, `journalId` kosong, satu baris percobaan baru |

## 5. Verifikasi

### 5.1 Yang sudah dijalankan agent

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
|---|---|---|---|
| `git diff --numstat` empat berkas, dikurangi bagian `BE-ACC-P2-029` (+86/−2) dan `034` (+7/−0) | +135/−10 milik task ini | `PASS` | — |
| Penelusuran `//` pada baris tambahan | Nol | `PASS` | `git diff -U0` |
| Pemanggil `UpdateAsync`/`DeleteAsync` jurnal dan `TindakanTersedia` | Hanya `JournalController` dan `PetakanRincianAsync` | `PASS` | Grep `Areas/` |
| Sumber hitungan jurnal belum disahkan | Satu — `HitungJurnalBelumDisahkanAsync`, dipakai daftar periksa dan pengajuan | `PASS` | Grep `Areas/` |
| Index `JournalId` | Ada | `PASS` | `AccAccountingEventConfiguration` |
| Pemetaan acceptance ke source | 11/11 | `PASS` | Bagian 6 |
| `dotnet build` (Rizki) | `bin/Debug/net9.0/QuilvianSystemBackend.dll` 15.51.52, sesudah source terakhir (`AccJournalService.cs` 15.37.19); keluaran build tidak dilampirkan | `PASS` (tak langsung) | `date -r`, 30 September 2026 |
| Uji Swagger dan layar | Dijalankan Rizki 16.10 WIB (bagian 5.3) | `PASS` | JSON + PNG `test-with-agy/` |
| Automated test | Bukan acceptance (`ACC-DEC-081`) | `NOT RUN` | — |

### 5.2 Resep uji gabungan — Swagger (S) dan layar (L), tanpa SQL

**Build lebih dahulu**, backend lalu frontend:

```bash
dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false
npm run build
```

Periode **Januari 2031** dipakai lagi: sudah terbuka dan tidak sedang ditutup. Aturan `PATIENT_PAYMENT`
berperlakuan Buat Draft dan menyentuh Kas Kasir serta Piutang Pasien Umum (terbukti `BE-ACC-P2-034`).
Langkah layar (L) ada di laporan [`FE-ACC-P2-019`](../frontend/FE-ACC-P2-019.md) bagian 6.2.

| # | Langkah | Hasil yang diharapkan | Membuktikan |
|---:|---|---|---|
| S1 | `POST api/v1/corporate/accounting/accounting-events`, body S1 di bawah | `201` `Terjurnal`; catat `accountingEventId` dan nomor jurnal Draft baru (sebut **J1**) | Persiapan |
| S2 | `GET …/journals/<ID_J1>` | `sourceAccountingEventNumber` "EVT-UJI-035A"; `availableActions` memuat `delete` dan `submit`, **tanpa** `update` | (8) |
| S3 | `PUT …/journals/<ID_J1>` — kirim ulang isi J1, ubah `Description` saja | `409` "Jurnal J1 dibentuk dari kejadian EVT-UJI-035A dan tidak dapat diubah. …" | (6) |
| S4 | `GET …/periods/<ID_JAN_2031>/closing-checklist` | Catat `count` `FAILED_EVENTS` (**F0**) dan `UNPOSTED_JOURNALS` (**U0**) | Persiapan |
| S5 | `DELETE …/journals/<ID_J1>` | `200` "Jurnal draft J1 berhasil dihapus. Kejadian EVT-UJI-035A kembali berstatus Gagal." | (1) |
| S6 | `GET …/accounting-events/<ID_KEJADIAN>` | `Gagal`; `journalId` kosong; percobaan terakhir "Jurnal draft J1 dihapus oleh {nama Anda}." | (1) |
| S7 | Ulangi S4 | `FAILED_EVENTS` = F0 + 1; `UNPOSTED_JOURNALS` = U0 − 1 | (3) |
| S8 | `POST …/accounting-events/<ID_KEJADIAN>/retry` | Kejadian `Terjurnal` dengan jurnal Draft baru (**J2**) | (2) |
| S9 | `POST …/journals/<ID_J2>/submit`, lalu `GET` daftar periksa Januari 2031 — catat `UNPOSTED_JOURNALS` (**U1**) | `200`; J2 Menunggu Persetujuan | Persiapan |
| S10 | `POST …/journals/<ID_J2>/reject`, body `{ "reason": "Uji 035 - salah akun" }`, lalu daftar periksa lagi | `200`, J2 `Rejected`. `UNPOSTED_JOURNALS` **tetap U1** — sebelum task ini turun satu | (7) |
| S11 | `GET …/journals/<ID_J2>` | `availableActions` memuat `delete` dan `submit`, **tanpa** `update` | (8) |
| S12 | `DELETE …/journals/<ID_J2>` | `200`; kejadian kembali `Gagal` | (4) |
| S13 *(opsional)* | Jurnal **manual** berakun biasa: buat → ajukan → tolak → `DELETE` | `409` "Jurnal yang sudah pernah ditolak tidak dapat dihapus. …" — seperti sebelumnya | (5) |

Body S1:

```json
{
  "EventNumber": "EVT-UJI-035A",
  "EventTypeCode": "PATIENT_PAYMENT",
  "SourceModule": "Finance",
  "SourceTransactionId": "UJI-035-2031-01",
  "SourceVersion": "1",
  "EventOccurredAt": "2031-01-21T08:00:00+07:00",
  "AccountingDate": "2031-01-21",
  "Amount": 150000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "<LEGAL_ENTITY_ID>",
  "CorrelationId": "0f035000-0000-4000-8000-000000000001",
  "CausationId": "0f035000-0000-4000-8000-000000000001"
}
```

Acceptance (9) kejadian berubah bersamaan dan (10) tanpa hak hapus dibuktikan lewat source. (11)
lewat `git status`.

**Bersih-bersih.** Sesudah S12, kejadian `EVT-UJI-035A` berstatus Gagal: **Abaikan** di Kotak Masuk
dengan alasan "Membersihkan data uji 035". Jurnal manual dari S13 dihapus bila masih Draft.

### 5.3 Hasil uji Rizki — 29 September 2026, diperiksa agent 30 September 2026

Rizki menjalankan `test-with-agy/test-p2-035-and-fe-019.mjs` (disusun agen Antigravity) terhadap backend
`https://localhost:7184` dan layar `http://localhost:3000`, akun SuperAdmin, badan hukum PT Metropolitan
Medical Centre, periode Januari 2031 (`e300b651-…`), 16.10.14–16.10.53 WIB. Skrip hanya mengimpor
`playwright` dan `fs`; setiap panggilan API berupa `fetch` dari peramban yang sudah login — nol SQL.
Skrip pendahulunya, `inspect-p2-035-init.mjs` (15.57), hanya membaca. Tabel di bawah dicocokkan agent
dengan `be_p2_035_fe_p2_019_report.json` dan delapan tangkapan layar `p2_035_*.png`. Langkah layar (L)
dirinci di laporan [`FE-ACC-P2-019`](../frontend/FE-ACC-P2-019.md) bagian 6.3.

**Yang diluruskan dari ringkasan agen**
([`testing/test-report-be-acc-p2-035-fe-acc-p2-019-2026-09-29.md`](../../../testing/test-report-be-acc-p2-035-fe-acc-p2-019-2026-09-29.md)
dan versi awal bagian ini):

1. **S1 menjawab `200`, bukan `201`.** `EVT-UJI-035A` sudah diterima pukul 16.08 oleh run sebelumnya
   yang tidak meninggalkan JSON (tangkapan L3: "diterima 29 Sep 2026, 16.08", percobaan #1 Berhasil
   16.08). J1 lahir dari percobaan itu; S1 pukul 16.10 hanya balasan anti-ganda. Bukti tidak berubah:
   S1 hanya persiapan, dan J1 belum pernah dihapus sebelum run ini (S6 mencatat dua percobaan).
2. **Kalimat riwayat "… dihapus oleh SuperAdmin." tidak ada di JSON.** Bidang `latestAttemptMessage`
   kosong karena nama bidangnya tidak cocok; JSON hanya mencatat `attemptsCount: 2` dan bahwa riwayat
   memuat kata "dihapus". Kalimat utuhnya terbukti dari tangkapan L3, baris percobaan #2.
3. **(7) terbukti separuh di runtime.** S10 membuktikan jurnal hasil kejadian `Rejected` tetap dihitung.
   Bahwa jurnal **manual** `Rejected` tidak dihitung dan `submit-closing` menjawab `409` tidak diuji:
   skrip tidak membaca daftar periksa sesudah jurnal manual ditolak dan tidak memanggil
   `submit-closing`. Keduanya dari source — `HitungJurnalBelumDisahkanAsync` hanya menambah `Rejected`
   yang ditunjuk kejadian, dan `SubmitClosingAsync` memakai hitungan yang sama.
4. **(6) terbukti separuh di runtime.** S3 membuktikan `409` pada jurnal hasil kejadian. `PUT` jurnal
   manual tidak dijalankan; L5 hanya membuktikan tombol Ubah membuka rute `/update`.
5. **S13: ajukan dan tolak jurnal manual tidak diperiksa skrip.** Pesan `409` yang diterima hanya
   dikeluarkan `PeriksaDapatDisunting` untuk status `Rejected`, jadi jurnal itu memang sudah ditolak.
6. **"Build terverifikasi aktif (PID 13400)" bukan bukti build.** Proses yang berjalan tidak
   menunjukkan kapan ia dibangun; yang dipakai adalah tanggal DLL (bagian 5.1). Bahwa backend yang
   berjalan memuat source ini terlihat dari perilakunya: pesan `409` baru dan bidang
   `sourceAccountingEventNumber`.
7. **(9), (10), (11) bukan "live test".** Ketiganya dari source sesuai resep. (11) diperiksa ulang pada
   commit `8f530926`: nol berkas migration, `Program.cs`, atau controller; nol baris tambahan ber-`//`
   di empat berkas task.
8. **Berkas bukti tersentuh sesudah run.** Skrip berubah 16.11.19 dan JSON 16.11.41, sedangkan run
   selesai 16.10.53. Isi JSON tetap cocok dengan bentuk keluaran skrip, dan cap waktu tiap langkah
   layar sama dengan jam berkas PNG-nya sampai milidetik (L1 16.10.23.081, L2 16.10.28.606, L3
   16.10.32.655). Angkanya karena itu dipercaya, tetapi pegangan untuk (1) dan (4) tetap tangkapan layar.

| # | Langkah | Hasil aktual (JSON dan PNG) | Status | Membuktikan |
|---:|---|---|---|---|
| S1 | `POST /accounting-events` `EVT-UJI-035A` (150.000, 21 Jan 2031) | `200` anti-ganda (butir 1), `Terjurnal`; kejadian `ca77fa59-b2c1-4b67-8230-456797b7c4d7`; J1 `JU/2031/01/00004` (`af50eaef-039f-4adf-94aa-f72202b16282`) Draft, Kas Kasir D 150.000 / Piutang Pasien Umum K 150.000 (tangkapan L1) | Persiapan | — |
| S2 | `GET /journals/af50eaef-…` | `sourceAccountingEventNumber` "EVT-UJI-035A", `sourceAccountingEventId` = id kejadian; `availableActions` `["delete","submit"]` | `PASS` | (8) |
| S3 | `PUT /journals/af50eaef-…`, `Description` diganti | `409` "Jurnal JU/2031/01/00004 dibentuk dari kejadian EVT-UJI-035A dan tidak dapat diubah. Hapus jurnal ini untuk menjurnal ulang kejadiannya, atau minta Finance mengirim kejadian pembalik." — persis kontrak | `PASS` | (6) sisi hasil kejadian |
| S4 | `GET …/periods/e300b651-…/closing-checklist` | `FAILED_EVENTS` F0 = 0, `UNPOSTED_JOURNALS` U0 = 4 | Persiapan | — |
| S5 → L2 | Hapus J1 **lewat layar**: Hapus → Ya, Hapus | Kartu Jurnal Dihapus memuat pesan backend "Jurnal draft JU/2031/01/00004 berhasil dihapus. Kejadian EVT-UJI-035A kembali berstatus Gagal." (`p2_035_l2_deleted_card.png`); kode HTTP tidak dicatat skrip | `PASS` | (1) |
| S6 | `GET /accounting-events/ca77fa59-…` | `eventStatus` 3 Gagal, `journalId` null, dua percobaan; percobaan #2 16.10 Tidak Berhasil "Jurnal draft JU/2031/01/00004 dihapus oleh SuperAdmin." (tangkapan L3) | `PASS` | (1) |
| S7 | Ulangi S4 | `FAILED_EVENTS` 1 (F0 + 1), `UNPOSTED_JOURNALS` 3 (U0 − 1) | `PASS` | (3) |
| S8 | `POST /accounting-events/ca77fa59-…/retry` | `200`, `eventStatus` 4 Terjurnal, J2 `JU/2031/01/00005` (`4678c3b0-26ea-4c2e-99fb-fd599bdc5596`), nomor baru | `PASS` | (2) |
| S9 | `POST /journals/4678c3b0-…/submit`, lalu daftar periksa | `200` "Jurnal berhasil diajukan."; U1 = 4 | Persiapan | — |
| S10 | `POST /journals/4678c3b0-…/reject` "Uji 035 - salah akun", lalu daftar periksa | `200` "Jurnal ditolak."; `UNPOSTED_JOURNALS` tetap 4. Tangkapan L4: status Ditolak, riwayat Diajukan dan Ditolak oleh SuperAdmin 16.10 | `PASS` | (7) sisi hasil kejadian |
| S11 | `GET /journals/4678c3b0-…` | `journalStatus` 5, `availableActions` `["delete","submit"]` | `PASS` | (8) |
| S12 | `DELETE /journals/4678c3b0-…` | `200` "Jurnal draft JU/2031/01/00005 berhasil dihapus. Kejadian EVT-UJI-035A kembali berstatus Gagal."; `GET` kejadian → `eventStatus` 3 | `PASS` | (4) |
| S13 | Jurnal manual `JU/2031/01/00006` (Bank Operasional D 50.000 / Pendapatan Rawat Jalan K 50.000): buat → ajukan → tolak → `DELETE` | `409` "Jurnal yang sudah pernah ditolak tidak dapat dihapus. Perbaiki lalu ajukan kembali." (butir 5) | `PASS` | (5) |
| Bersih | `PATCH /accounting-events/ca77fa59-…/ignore` "Membersihkan data uji 035" | `200` "Kejadian EVT-UJI-035A ditandai Diabaikan.", `eventStatus` 5 | Selesai | — |

**Sisa data uji di dev.** `EVT-UJI-035A` Diabaikan, sehingga tidak menahan tutup bulan. `JU/2031/01/00004`
dan `00005` terhapus, dan nomornya tidak dipakai ulang. `JU/2031/01/00006` jurnal manual **Ditolak**
tidak dapat dihapus (justru itu yang dibuktikan S13) dan tidak dihitung jurnal belum disahkan. Januari
2031 tetap memuat tiga jurnal belum disahkan sisa uji sebelumnya.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti di source & runtime |
|---|---|---|
| (1) Hapus draft hasil kejadian → `200`, kejadian `Gagal`, `JournalId` kosong, riwayat +1 | Terpenuhi — runtime | `DeleteAsync` → `KembalikanKeGagalKarenaJurnalDihapusAsync`; L2 (pesan backend di kartu) dan S6 (JSON + kalimat riwayat di tangkapan L3) |
| (2) Coba Ulang → draft baru, `Terjurnal` | Terpenuhi — runtime | Coba ulang menerima `Gagal` (`025`); S8 `JU/2031/01/00005` |
| (3) `FAILED_EVENTS` +1 sebelum dicoba ulang | Terpenuhi — runtime | `HitungKejadianAsync`; S7 F 0 → 1, U 4 → 3 |
| (4) Hapus jurnal hasil kejadian `Rejected` → `200`, kejadian `Gagal` | Terpenuhi — runtime | `DeleteAsync`: `sumber is not null && Rejected`; S12 |
| (5) Hapus jurnal manual `Rejected` → `409` | Terpenuhi — runtime | `sumber is null` → `PeriksaDapatDisunting`; S13 |
| (6) Sunting jurnal hasil kejadian → `409`; jurnal manual tidak berubah | Terpenuhi — runtime untuk hasil kejadian, source untuk manual | `UpdateAsync` menolak hanya bila `sumber` ada; S3. `PUT` jurnal manual tidak dijalankan (bagian 5.3 butir 4) |
| (7) `Rejected` hasil kejadian dihitung jurnal belum disahkan dan `submit-closing` `409`; manual tidak | Terpenuhi — runtime untuk hitungan hasil kejadian, source untuk sisanya | `HitungJurnalBelumDisahkanAsync`, dipakai juga `SubmitClosingAsync`; S10 (tetap 4). Bagian 5.3 butir 3 |
| (8) Bidang asal dan `availableActions` | Terpenuhi — runtime | `PetakanRincianAsync`, `TindakanTersedia`; S2 dan S11. Jurnal manual tanpa asal terlihat sebagai "Jurnal manual" di tangkapan L5 |
| (9) Kejadian berubah bersamaan → `409`, nol perubahan | Terpenuhi — source | `ExecuteUpdate` bersyarat; nol baris → rollback sebelum jurnal ditandai |
| (10) Tanpa `Journal : Delete` → `403` | Terpenuhi — source, tidak berubah | `[AccessPermission("Journal", "Delete")]` |
| (11) Nol migration, endpoint baru, `Program.cs`, `//` baru | Terpenuhi | `git show --name-only 8f530926`; `git show -U0 8f530926` pada empat berkas task |

| Butir DoD | Keadaan |
|---|---|
| Source berubah | ✅ |
| Build Rizki 0 error | ✅ tak langsung — DLL 15.51 sesudah source 15.37; jumlah warning tidak terbukti |
| Uji Swagger tercatat | ✅ bagian 5.3 — dijalankan Rizki 16.10, JSON dan PNG diperiksa agent 30 September 2026 |
| Laporan task tertulis | ✅ |

## 7. Catatan penutup

| Hal | Isi |
|---|---|
| Status Git | Source dan versi awal laporan ini di-commit Rizki `8f530926` (29 September 2026 16.28, bersama `029`/`030`/`034`); pelurusan 30 September 2026 di laporan ini, roadmap, traceability, MODULE-STATUS, dan laporan audit belum di-commit |
| Migration / database | Tidak ada. Sisa data uji di bagian 5.3 |
| Risiko tersisa | (a) Nomor jurnal yang dihapus tidak dipakai ulang — perilaku yang sudah ada. (b) `AccJournalService` memanggil method static milik `AccAccountingEventService`, sementara `AccAccountingEventService` memakai instance `AccJournalService`: bukan siklus DI karena panggilan static tidak melewati kontainer |
| Temuan di luar cakupan | Komentar XML di atas `BerasalDariJalurOtomatisAsync` masih menulis "Jalur kejadian akuntansi belum ada di kode" — usang sejak `034`, dibiarkan karena aturan nol komentar baru |
| Task berikutnya | Tidak ada kartu tersisa di roadmap backend Phase 2. Sisa modul: T-3..T-9 [audit kesiapan](../../../testing/readiness-report-2026-09-29.md) bagian 8 |
