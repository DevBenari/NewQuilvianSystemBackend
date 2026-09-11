# Traceability Requirement — Accounting Phase 2 (gelombang mandiri)

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 11
roadmap_revision: 1
decision_revision: 2.2
contracts: [ACC-API-0.7, ACC-STATE-0.2, ACC-VALIDATION-0.5, ACC-PERMISSION-0.4]
scope_waves: [P2-3, P2-4, P2-5]
generated_at: 2026-09-08
task_selesai: 8                       # BE-ACC-P2-001, 002, 003, 004, 007, 008, 009, 011 selesai
task_sebagian: 4                      # BE-ACC-P2-005, 006, 010, dan 013 - test integrasi PostgreSQL belum dijalankan
database_state: applied               # migration 20260909060515_AddAccountingPhase2Independent diterapkan owner 9 Sep 2026
```

Dokumen ini memetakan **requirement → task → bukti**. Ia dibuat sekarang, sebelum satu task pun
dikerjakan, justru supaya tidak bernasib seperti `roadmap/requirement-traceability.md` milik MVP
yang membeku pra-implementasi dan tercatat sebagai `ACC-GAP-001`.

**Aturan pemakaian:** setiap kali sebuah task selesai, baris yang bersangkutan diperbarui beserta
bukti nyatanya, dan `task_selesai` pada metadata dinaikkan. Baris yang tidak diperbarui berarti
task-nya belum benar-benar selesai, walau kodenya sudah ada.

---

## 1. Jurnal berulang (`P2-3`)

| Requirement | Isi ringkas | Keputusan asal | Task backend | Task frontend | UAT | Keadaan |
|---|---|---|---|---|---|---|
| `FR-P2-018` | Template tidak seimbang ditolak | `ACC-DEC-050` | `BE-ACC-P2-002` ✅, `007` ✅ | [`FE-ACC-P2-004`](../task/report/frontend/fe-acc-p2-004-form-jurnal-berulang.md) ✅ | `UAT-P2-13` | **`Done`** — `POST /recurring-journals` menolak `400` bila total debit ≠ total kredit, pesannya menyebut selisihnya, dan **nol template maupun baris tersimpan**. Berbeda dari jurnal manual yang boleh timpang (`ACC-DEC-025`): template disimpan sekali lalu terbit sendiri tiap bulan, jadi yang timpang akan melahirkan draft timpang berulang. Bukti `AccRecurringJournalTemplateTests` (32 uji, `Failed: 0`), laporan [`be-acc-p2-007`](../task/report/backend/be-acc-p2-007-crud-template-jurnal-berulang.md) |
| `FR-P2-019` | Template aktif menerbitkan jurnal `Draft` pada tanggalnya | `ACC-DEC-050` | `BE-ACC-P2-002` ✅, `008` ✅ | [`FE-ACC-P2-003`](../task/report/frontend/fe-acc-p2-003-layar-daftar-jurnal-berulang.md) ✅ | `UAT-P2-11` | **`Done`** — `AccRecurringJournalSchedulerHostedService` menerbitkan jurnal `Draft` bertanggal `DayOfMonth` template, **bukan** tanggal penjadwal kebetulan berjalan; tanpa itu jurnal September dapat mendarat di periode Oktober saat penjadwal sempat mati. **Bawaannya MATI** — isi `Accounting:RecurringJournalScheduler:Enabled` = `true` untuk menyalakannya. Bukti `AccRecurringJournalGenerateTests` (14 uji, `Failed: 0`), laporan [`be-acc-p2-008`](../task/report/backend/be-acc-p2-008-penerbitan-jurnal-berulang-dan-penjadwalnya.md) |
| `FR-P2-020` | Satu template satu jurnal per periode, ditegakkan **di database** | `ACC-DEC-050` | `BE-ACC-P2-002` ✅, `008` ✅ | — | `UAT-P2-12` | **`Done` — dibuktikan pada PostgreSQL sungguhan.** Dua koneksi terpisah berangkat bersamaan ke `QuilvianNewDevRizki`: A berhasil, B ditolak constraint `IX_AccRecurringJournalRun_TemplateId_AccountingPeriodId`, tersimpan tepat 1 baris. Data ujinya dihapus seluruhnya. Service menerjemahkan penolakan itu menjadi `409`, bukan `500`. Bukti `AccRecurringJournalGenerateTests` (14 uji, `Failed: 0`), laporan [`be-acc-p2-008`](../task/report/backend/be-acc-p2-008-penerbitan-jurnal-berulang-dan-penjadwalnya.md) |
| `FR-P2-021` | Tidak menerbitkan ke periode yang tidak menerima pencatatan | `ACC-DEC-050` | `BE-ACC-P2-008` ✅ | — | — | **`Done`** — jalur manual ditolak `422`; pada penjadwal template itu **dilewati, bukan menggagalkan siklus**, sehingga satu template bermasalah tidak menghentikan yang lain. "Sudah terbit" dan "bermasalah" dibedakan: hanya yang kedua diperingatkan ke log. Bukti `AccRecurringJournalGenerateTests` (14 uji, `Failed: 0`) |
| `FR-P2-022` | Template baru tidak aktif sampai diaktifkan | `ACC-DEC-050` | `BE-ACC-P2-002` ✅, `007` ✅ | [`FE-ACC-P2-003`](../task/report/frontend/fe-acc-p2-003-layar-daftar-jurnal-berulang.md) ✅ | — | **`Done`** — `PATCH /{id}/activate` dan `/deactivate` berjalan. Bidang `IsActive` **sengaja tidak ada** pada permintaan simpan, sehingga template mustahil lahir aktif; dijaga uji refleksi. Barisnya diperiksa **ulang** saat diaktifkan, karena akun dapat dinonaktifkan sesudah template tersimpan dan kegagalannya kelak terjadi di dalam penjadwal tempat tidak seorang pun melihatnya. Bukti `AccRecurringJournalTemplateTests` (32 uji, `Failed: 0`) |

## 2. Tutup bulan (`P2-4`)

| Requirement | Isi ringkas | Keputusan asal | Task backend | Task frontend | UAT | Keadaan |
|---|---|---|---|---|---|---|
| `FR-P2-023` | Daftar periksa dihitung saat diminta, bukan disimpan | `ACC-DEC-051` | `BE-ACC-P2-005` 🟡 | [`FE-ACC-P2-001`](../task/report/frontend/fe-acc-p2-001-daftar-periksa-penutupan.md) 🟡 | — | **`Done` backend, bukti SQLite** — `GET /{id}/closing-checklist` menghitung ulang tiap panggilan; dibuktikan `TigaJurnalBelumSah_SahkanSatu_AngkanyaTurun` (3 → 2). Nol hasil disimpan. **Bukti PostgreSQL belum ada** |
| `FR-P2-024` | Pengajuan ditolak selama ada jurnal belum sah atau kejadian gagal | `ACC-DEC-051` | `BE-ACC-P2-005` 🟡, `006` 🟡 | [`FE-ACC-P2-002`](../task/report/frontend/fe-acc-p2-002-aksi-penutupan-ajukan-setujui-tolak.md) 🟡 | `UAT-P2-14` | **`Done` sebagian** — pengajuan ditolak `409` bila ada jurnal belum sah, dibuktikan `MasihAdaJurnalBelumSah_PengajuanDitolak409`. Penghalang **kejadian gagal belum dapat diperiksa** sampai `P2-1`, jadi pengajuan saat ini hanya ditahan jurnal |
| `FR-P2-025` | Kejadian tertahan hanya peringatan, tidak menahan | `ACC-DEC-051` | `BE-ACC-P2-005` 🟡 | [`FE-ACC-P2-001`](../task/report/frontend/fe-acc-p2-001-daftar-periksa-penutupan.md) 🟡 | — | **`Done` bentuknya** — `HELD_EVENTS` berada di `Warnings` dan **tidak** di `Blockers`, dibuktikan `KejadianTertahan_AdaSebagaiPeringatan_BukanPenghalang`. Isinya menunggu `P2-1` |
| `FR-P2-026` | Penutupan hanya disetujui `Accounting Director` | `ACC-DEC-055` | `BE-ACC-P2-001` ✅, `006` 🟡 | [`FE-ACC-P2-002`](../task/report/frontend/fe-acc-p2-002-aksi-penutupan-ajukan-setujui-tolak.md) 🟡 | `UAT-P2-17` | **`Done` di kode** — `POST /{id}/approve-closing` memakai hak akses `Approve` yang **terpisah** dari `Close`, dibuktikan `HakAksesMenyetujui_TerpisahDariMengajukan`. **Peran `Accounting Director` belum diisi** di layar Administrator |
| `FR-P2-027` | Penyetuju **bukan** pengaju | `ACC-DEC-016`, `052` | `BE-ACC-P2-001` ✅, `006` 🟡 | [`FE-ACC-P2-002`](../task/report/frontend/fe-acc-p2-002-aksi-penutupan-ajukan-setujui-tolak.md) 🟡 | `UAT-P2-16` | **`Done`** — ditolak `403`, diperiksa terhadap `ClosingSubmittedBy` **tersimpan** (bukan isian permintaan); dibuktikan `PenyetujuSamaDenganPengaju_Ditolak403`. **Bukti PostgreSQL belum ada** |
| `FR-P2-028` | Penolakan wajib beralasan, periode kembali terbuka | `ACC-DEC-052` | `BE-ACC-P2-001` ✅, `006` 🟡 | [`FE-ACC-P2-002`](../task/report/frontend/fe-acc-p2-002-aksi-penutupan-ajukan-setujui-tolak.md) 🟡 | `UAT-P2-18` | **`Done`** — tanpa alasan ditolak `400` (diuji `null`, kosong, spasi); beralasan mengembalikan periode ke `Open` dan mengosongkan pengaju |
| `FR-P2-029` | Periode yang ditutup sebelum Phase 2 tetap sah tanpa riwayat | `ACC-DEC-052` | `BE-ACC-P2-001` ✅, `006` 🟡 | — | — | **`Done`** — riwayat kosong dikembalikan sebagai jawaban sah, bukan error; daftar periksanya tetap dapat dihitung. Dibuktikan `PeriodeSoftClosedSebelumPhase2_TetapSahTanpaRiwayat` |

## 3. Tutup tahun dan pengaturan (`P2-5`, `P2-0a`)

| Requirement | Isi ringkas | Keputusan asal | Task backend | Task frontend | UAT | Keadaan |
|---|---|---|---|---|---|---|
| `FR-P2-030` | Pratinjau menampilkan saldo, **tanpa membuat apa pun** | `ACC-DEC-053` | `BE-ACC-P2-010` 🟡 | [`FE-ACC-P2-006`](../task/report/frontend/fe-acc-p2-006-layar-tutup-tahun.md) ✅ | `UAT-P2-21` | **`Done`** — `GET /year-end-closing/preview` menampilkan saldo tiap akun pendapatan dan beban beserta seluruh baris jurnal yang akan dibuat. Dipanggil sepuluh kali: nol jurnal terbentuk **dan** `AccNumberSeries` tetap kosong. Bukti `AccYearEndClosingTests` (20 uji, `Failed: 0`), laporan [`be-acc-p2-010`](../task/report/backend/be-acc-p2-010-pratinjau-dan-penyusunan-jurnal-penutup-tahun.md) |
| `FR-P2-031` | Penyusunan ditolak bila ada periode belum tertutup | `ACC-DEC-053` | `BE-ACC-P2-010` 🟡 | [`FE-ACC-P2-006`](../task/report/frontend/fe-acc-p2-006-layar-tutup-tahun.md) ✅ | `UAT-P2-19` | **`Done`** — `409` beserta **daftar nama periodenya**, untuk `Open` maupun `PendingClosingApproval`, pada pratinjau maupun penyusunan. `SoftClosed` sudah cukup. Bukti `AccYearEndClosingTests` (20 uji, `Failed: 0`) |
| `FR-P2-032` | Penyusunan ditolak bila akun laba ditahan belum ditetapkan | `ACC-DEC-054` | `BE-ACC-P2-003` ✅, `009` ✅, `010` 🟡 | [`FE-ACC-P2-005`](../task/report/frontend/fe-acc-p2-005-layar-pengaturan-akuntansi.md) ✅, [`FE-ACC-P2-006`](../task/report/frontend/fe-acc-p2-006-layar-tutup-tahun.md) ✅ | `UAT-P2-20` | **`Done` sisi penetapan** — `PUT /configuration/{legalEntityId}` menolak `422` untuk akun bukan Ekuitas, akun induk, akun badan hukum lain, dan akun nonaktif; satu badan hukum tetap satu pengaturan. Dibuktikan `AccAccountingConfigurationTests` (14 uji). **`Done` juga sisi penyusunan** — `POST /year-end-closing/generate` menolak `422` bila akun laba ditahan belum ditetapkan, dan bila akunnya berubah menjadi tidak layak sesudah ditetapkan (dihapus, badan hukum lain, bukan Ekuitas, akun induk, atau nonaktif). Penilaiannya memakai `AccAccountingConfigurationService.AmbilAkunLabaDitahanAsync` yang sama dengan endpoint pengaturan, sehingga tutup tahun dan layar pengaturan tidak pernah berbeda pendapat. Bukti `AccYearEndClosingTests` (20 uji, `Failed: 0`) |
| `FR-P2-033` | Jurnal penutup `Draft` berjenis `JT`, disahkan lewat jalur yang ada | `ACC-DEC-053` | `BE-ACC-P2-003` ✅, `010` 🟡 | [`FE-ACC-P2-006`](../task/report/frontend/fe-acc-p2-006-layar-tutup-tahun.md) ✅ | `UAT-P2-22` | **`Done`** — **jenis `JT` sudah ada di seeder** ber-`RequiresApproval = true`, dibuktikan `AccountingMasterDataSeederTests`. Jurnal penutup lahir `Draft` berjenis `JT` bertanggal akhir tahun buku, dibuat lewat `AccJournalService.CreateAsync` yang sudah ada sehingga penomoran, penentuan periode, dan kesembilan syarat berlaku sama seperti jurnal mana pun; lalu diajukan, disetujui, dan disahkan lewat endpoint jurnal yang sudah ada. **Menuntut satu delta kontrak:** `JT` kini diterima periode `SoftClosed`, tanpa itu jurnalnya tidak akan pernah dapat disahkan. **Jenis jurnal `JT` sudah ada di database dev pemilik** — dinyatakan owner 10 September 2026 saat memberi wewenang `010`; seeder dijalankan lewat endpoint seed pada `AccJournalTypeService`, bukan saat startup. Bukti `AccYearEndClosingTests` (20 uji, `Failed: 0`) |
| `FR-P2-034` | Jurnal penutup dapat dibalik lewat pembalikan yang sudah ada | `ACC-DEC-029` | `BE-ACC-P2-010` 🟡 | — | `UAT-P2-23` | **`Done`** — dibalik lewat `POST /journals/{id}/reverse` `FullReversal` yang sudah ada, lalu disetujui dan disahkan; saldo keempat akun kembali persis ke angka sebelum ditutup dan laba ditahan kembali nol. Nol jalur pembalikan khusus tutup tahun. **Kebijakan koreksinya diratifikasi `ACC-DEC-068`, 10 September 2026** — `DEC-ACC-P2-006` ditutup. Bukti `AccYearEndClosingTests` (20 uji, `Failed: 0`) |

---

## 3b. Control account dan rekonsiliasi (`P2-CTRL`, `P2-RECON`) — amandemen 9 September 2026

| Requirement | Isi ringkas | Keputusan asal | Task backend | Task frontend | UAT | Keadaan |
|---|---|---|---|---|---|---|
| `FR-P2-035` | Akun dapat ditandai sebagai control account | `ACC-DEC-064` | `BE-ACC-P2-011` ✅ | [`FE-ACC-P2-007`](../task/report/frontend/fe-acc-p2-007-penanda-control-account-layar-coa.md) ✅ | — | **`Done` backend** — kolom `IsControlAccount` berbawaan `false` beserta index-nya berdiri, dan penandanya dapat diisi lewat `POST`/`PUT` serta dibaca kembali lewat `GET`. Dibuktikan `AccChartOfAccountControlAccountTests`. **Sudah ada di database** sejak `BE-ACC-P2-004` ✅ diterapkan 9 Sep 2026. **Frontend 🟡 `FE-ACC-P2-007`, 11 Sep 2026** — kotak centang di Form Akun (mati bagi yang tidak berhak `ChartOfAccount : Update`), kolom `Control` di tabel COA, dan baris rincian. Lint 0 error, 651 uji unit lulus; build dan uji peramban `NOT RUN`. **✅ Selesai sisi development, 11 Sep 2026 (lanjutan):** build compiled, 677 uji lulus, `READY FOR UAT` — UAT diserahkan ke tim UAT. Bukti: [laporan](../task/report/frontend/fe-acc-p2-007-penanda-control-account-layar-coa.md) |
| `FR-P2-036` | Jurnal **manual** ke control account ditolak | `ACC-DEC-064` | `BE-ACC-P2-012` 🟡 | [`FE-ACC-P2-007`](../task/report/frontend/fe-acc-p2-007-penanda-control-account-layar-coa.md) ✅ | `UAT-P2-24` | 🟡 **Sebagian — 11 Sep 2026.** Penolakan **sudah ada di source** `BE-ACC-P2-012`: Simpan, Ubah, Ajukan, penyesuaian `JP` (`ACC-DEC-072`), serta simpan dan aktifkan template (`ACC-DEC-073`). **Uji manual terhadap PostgreSQL dev, 11 Sep 2026 (S1–S8 oleh Rizki, A–D lewat API):** Simpan (S1), Ubah (S3), dan **Ajukan (C)** ditolak `422` dengan pesan menyebut akunnya; `PUT` yang ditolak tidak mengubah baris (A); ubah dengan akun non-control berhasil (B); penyesuaian `JP` (S5) dan template (S7) `422`; akun non-control lolos Simpan → Ajukan → Setujui → Sahkan (S2, S4). Build `Release` dan uji SQLite **belum diverifikasi**; `UAT-P2-24` sebagian. **Keputusan owner 11 Sep 2026:** Implementation `COMPLETE`, Developer Manual Test `COMPLETED`, Automated Verification `DEFERRED` ke pipeline quality/UAT, UAT `HANDOFF TO UAT TEAM`. Bukti: [laporan](../task/report/backend/be-acc-p2-012-penolakan-jurnal-manual-ke-control-account.md). **Sisi layar `FE-ACC-P2-007` ✅ (11 Sep 2026, lanjutan):** Form Jurnal, dialog penyesuaian, dan Form Jurnal Berulang mematikan akun control dari pemilihnya beserta keterangannya; `/options` terbukti membawa `isControlAccount` di runtime. `READY FOR UAT`. Bukti: [laporan](../task/report/frontend/fe-acc-p2-007-penanda-control-account-layar-coa.md) |
| `FR-P2-037` | Jurnal dari kejadian, template, dan tutup tahun **tidak** terkena larangan itu | `ACC-DEC-064` | `BE-ACC-P2-012` | — | `UAT-P2-25` | 🟡 **Sebagian — 11 Sep 2026.** Pengecualian dikenali dari **asal-usul** — baris penerbitan template, cermin pembalikan penuh, bentuk jurnal `JT` — **bukan** dari kode jenis. **Uji manual terhadap PostgreSQL dev, 11 Sep 2026:** pembalikan penuh atas jurnal yang menyentuh Kas Kecil diterima saat dibuat (S6, `200` sesuai kontrak) **dan lolos diajukan ulang tanpa diubah** (D1); draft hasil template yang akunnya baru ditandai control **terbit dan lolos diajukan** (D2). Jurnal penutup tahun `JT` **belum** diuji di runtime — penyiapannya menuntut setahun periode tertutup; jalur kejadian akuntansi belum ada di kode. Uji SQLite belum diverifikasi; `UAT-P2-25` sebagian. **Keputusan owner 11 Sep 2026:** verifikasi otomatis `DEFERRED`; uji runtime jurnal `JT` menuntut setahun periode tertutup dan diserahkan sebagai **UAT follow-up** kepada tim UAT/test environment — tidak disiapkan lewat manipulasi database. Bukti: [laporan](../task/report/backend/be-acc-p2-012-penolakan-jurnal-manual-ke-control-account.md) |
| `FR-P2-038` | Shift kasir belum ditutup menjadi penghalang ketiga tutup bulan | `ACC-DEC-065` | `BE-ACC-P2-005` 🟡 sebagian, penuh menunggu `P2-1` | [`FE-ACC-P2-001`](../task/report/frontend/fe-acc-p2-001-daftar-periksa-penutupan.md) 🟡 | `UAT-P2-26` | **`Done` tempatnya** — `OPEN_CASH_SHIFTS` selalu hadir pada `Blockers` ber-`State = NotYetAvailable` beserta alasannya, dibuktikan `PenghalangKetiga_AdaTempatnya_TetapiBelumDapatDiperiksa`. Bentuk respons tidak akan berubah saat `P2-1` datang |
| `FR-P2-039` | Saldo control account dihitung dari baris `Posted` | `ACC-DEC-066` | `BE-ACC-P2-013` 🟡 | [`FE-ACC-P2-008`](../task/report/frontend/fe-acc-p2-008-layar-rekonsiliasi-control-account.md) ✅ | `UAT-P2-27` | **`Done` backend, bukti SQLite** — `GET /reconciliation/gl-balances` menghitung **hanya** dari baris `Posted`; `Draft` dan `Approved` diabaikan, dibuktikan `SaldoDihitungHanyaDariBarisPosted` (Rp 14.500.000 dari 2 baris, bukan Rp 1.614.500.000). Angkanya terbukti sama dengan `HitungSaldoAsync`. **Bukti PostgreSQL belum ada**. **Sisi layar `FE-ACC-P2-008` ✅, 11 Sep 2026:** kolom Saldo Buku Besar (`BalanceInNormalBalance`) tampil; endpoint terbukti menjawab `200` terhadap PostgreSQL dev dengan 6 control account dan bentuk responsnya cocok dengan normalizer. `READY FOR UAT`. Bukti: [laporan](../task/report/frontend/fe-acc-p2-008-layar-rekonsiliasi-control-account.md) |
| `FR-P2-040` | Saldo subledger dibandingkan dan selisihnya dilaporkan | `ACC-DEC-066`, **`ACC-DEC-071`** | `BE-ACC-P2-014` | [`FE-ACC-P2-008`](../task/report/frontend/fe-acc-p2-008-layar-rekonsiliasi-control-account.md) — kolom ada, isi `DEFERRED` | `UAT-P2-28` | **`READY`** — `DEC-ACC-P2-011` ditutup `ACC-DEC-071` 10 Sep 2026; — **sisi buku besarnya sudah berdiri** lewat `013` 🟡; yang tersisa hanya menyambungkan pembandingnya. **Layar `FE-ACC-P2-008` (11 Sep 2026):** kolom Saldo Subledger dan Selisih sudah **tampil** bertuliskan "Belum tersedia" — `DEFERRED BACKEND CAPABILITY` sampai `BE-ACC-P2-014` berdiri. Requirement ini **belum** terpenuhi |

**Tiga skenario UAT baru** perlu ditambahkan ke `04-prd-to-mvp.md`: `UAT-P2-24` sampai `UAT-P2-28`.
Dicatat sebagai coverage gap sampai dokumen itu diperbarui.

## 4. Ringkasan cakupan

| Hal | Jumlah |
|---|---:|
| Requirement dalam lingkup roadmap ini | **23** dari 40 — bertambah 6 lewat amandemen 9 Sep |
| Requirement di luar lingkup (kotak masuk kejadian) | 17 |
| Task backend | **14** — satu di antaranya ⛔ `BLOCKED` |
| Task frontend | **8** |
| Skenario UAT dalam lingkup | **13** dari 23 |
| Task yang sudah selesai | **0** |

## 5. Requirement tanpa UAT — coverage gap

Empat requirement tidak punya skenario UAT tersendiri. Ini **bukan** kelalaian, tetapi tetap
dicatat supaya tidak dianggap sudah teruji:

| Requirement | Kenapa tidak punya UAT | Cara membuktikannya |
|---|---|---|
| `FR-P2-021` | Perilakunya "dilewati diam-diam", tidak terlihat pengguna | Test integrasi: periode `SoftClosed`, penjadwal dijalankan, pastikan nol jurnal terbentuk dan nol galat |
| `FR-P2-022` | Bagian dari alur `UAT-P2-11`, bukan skenario sendiri | Test unit pada nilai bawaan `IsActive` |
| `FR-P2-023` | Sifat internal, tidak punya layar tersendiri | Test integrasi: panggil dua kali dengan perubahan di antaranya, pastikan angkanya berbeda |
| `FR-P2-029` | Menyangkut data lama, tidak dapat dibuat ulang lewat layar | Test integrasi memakai periode yang sengaja dibuat tanpa riwayat persetujuan |

## 6. Gap yang menyangkut bukti, bukan requirement

| Gap | Isi | Pemilik |
|---|---|---|
| `ACC-TD-016` | Nol test backend Accounting yang ada sekarang. Dua task pada roadmap ini menyentuh `AccJournalService` milik MVP tanpa jaring regresi | Rizki |
| `ACC-TEST-0.1` | `testing/acceptance-test-matrix.md` belum punya kolom bukti yang **sudah ada**. Ketiga belas UAT roadmap ini tidak punya tempat dicatat saat dijalankan | Rizki |
| `ACC-GAP-001` | `requirement-traceability.md` milik MVP masih beku pra-implementasi. Dokumen ini sengaja dibuat terpisah agar tidak mewarisi masalah yang sama | Rizki |
| `DEC-ACC-P2-011` ✅ | **DITUTUP 10 Sep 2026 oleh `ACC-DEC-071`.** Dipilih kemungkinan (a): Finance menerbitkan saldo subledger final **per periode akuntansi**, bukan berkala dan bukan API pull. Kejadiannya memuat `LegalEntity`, `AccountingPeriod`, `ControlAccount`, `SubledgerBalance`, `AsOfDate`. Rekonsiliasi menjadi bagian penutupan periode | Rizki + owner Finance |
