# Accounting — Laporan Kesiapan End-to-End, 29 September 2026

| Field | Nilai |
|---|---|
| `blueprint_id` | `ACC-BP-001` |
| Revision manifest | `13` (`blueprint-manifest.md`) — **`MODULE-STATUS.md` masih menulis `11`**, lihat G-11 |
| Status blueprint | `approved` |
| Bentuk blueprint | `SINGLE` — satu verdict |
| Revision decision log | `00-interview-decisions.md` revision `14` (`ACC-DEC-001`..`115`) |
| Contract version berlaku | `ACC-API-0.14`, `ACC-STATE-0.5`, `ACC-VALIDATION-0.10`, `ACC-PERMISSION-0.7`, `ACC-INTEGRATION-0.5`, `ACC-XMOD-0.4` |
| Roadmap | MVP backend (15 kartu), MVP frontend (11), Phase 2 backend revisi 6 (33), Phase 2 frontend revisi 7 (18) |
| Backend source SHA | `618b206e5241d44c9f872a3e8a8396df4d3df7a5`, branch `rizkiG` — **ditambah working tree belum di-commit**: source `BE-ACC-P2-029`/`030` dan dokumen. 6 commit di depan, 1 di belakang `origin/QuilvianIntegrationBackend` (`579f9f61`) |
| Frontend source SHA | `bf0a225376ed45bf19f5f8701e47a4def53f68ce`, branch `RizkiV2`; hanya `.gitignore` belum di-commit. 6 commit di depan `origin/QuilvianIntegrationFrontend` (`033bf3490`) |
| Sifat audit | **Read-only** atas source dan database. Nol source diubah, nol query database, nol build, nol commit. Unit test frontend Accounting dijalankan (hanya membaca) |
| Menggantikan | [`readiness-report-2026-09-07.md`](readiness-report-2026-09-07.md). Laporan lama tidak dihapus |

---

## 1. Verdict

# `NOT_READY`

**Pekerjaan development sudah tuntas; modulnya belum.** Seluruh 77 kartu task pada empat roadmap
punya laporan dan sudah ditandai selesai. 68 endpoint berdiri, semuanya berpenjaga hak akses dengan
nama izin yang cocok, dan setiap service memanggil penjaga badan hukum. Itu bukti kuat bahwa kode
sudah ditulis lengkap.

"100% finish" menuntut lebih dari itu, dan tiga hal menahannya:

| # | Penghalang | Kenapa ia menahan |
|---|---|---|
| 1 | **Cacat di jalur utama Phase 2** (G-01) | Draft jurnal dari kejadian keuangan yang menyentuh control account — Kas Kasir, Piutang, Utang — **tidak dapat diajukan**. Ditolak `422` "…hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual", padahal jurnal itu memang lahir dari kejadian. Perlakuan bawaan aturan posting adalah Buat Draft, jadi ini menyentuh kasus paling umum |
| 2 | **Definition of Done Phase 2 baru 5 dari 12 butir** (bagian 5) | Butir yang tersisa menuntut bagan akun sah, aturan posting nyata, UAT tercatat, dan catatan migration — sebagian besar di luar tangan developer |
| 3 | **Belum pernah dijalankan sebagai operasi sungguhan** (bagian 6) | Penjadwal jurnal berulang mati, pelaku sistem belum diatur, tujuh peran belum dipastikan, gerbang cutover Finance G2/G3/G4/G6 terbuka |

### Kenapa bukan `READY_WITH_CONDITIONS`

Kondisi hanya sah bila risikonya terbatas dan ada mitigasinya. G-01 tidak begitu: begitu Finance
mulai mengirim kejadian, setiap draft Kas Kasir akan menumpuk tanpa bisa diajukan, dan satu-satunya
jalan keluar yang tersedia bagi petugas adalah menghapus draft lalu menjurnal manual — yang justru
ditolak aturan yang sama. Perbaikannya kecil (bagian 8, T-1). Begitu T-1 selesai dan terbukti, jarak
ke `READY_WITH_CONDITIONS` tinggal urusan pihak lain yang pemilik dan jalannya sudah jelas.

---

## 2. Yang berubah sejak 7 September

| Penghalang 7 September | Keadaan 29 September | Dasar |
|---|---|---|
| Nol test otomatis backend (`ACC-TD-016`) | **Risiko diterima**, `CLOSED` 14 September 2026 | `ACC-DEC-081`, arahan lead `cefd927d`; `UTANG-TEKNIS.md` |
| Bukti UAT tidak punya tempat | **Belum bergerak** — `acceptance-test-matrix.md` tetap `draft`, hanya MVP, kolomnya tetap "bukti yang diharapkan" | Bagian 5 butir 2 |
| `ACC-TD-002` penyaringan badan hukum per pengguna | **Tetap `OPEN`, `Tinggi`** — dimitigasi penjaga `IsDefault` (`ACC-DEC-041`, `043`) selama hanya satu badan hukum | `UTANG-TEKNIS.md` |
| Frontend 11 task | **Frontend 29 task**; Phase 2 18/18 ✅ | Bagian 3 |
| Backend 31 endpoint | **68 endpoint**, 12 controller | Bagian 4 |

---

## 3. Kelengkapan task dan laporan

| Roadmap | Kartu | Tanda selesai | Laporan ada | Grafik dependency |
|---|---:|---:|---:|---|
| `backend-roadmap.md` (MVP) | 15 | 15 `DONE` — kosakata lama | 15 | **Tidak ada** (G-12) |
| `frontend-roadmap.md` (MVP) | 11 | 11 `IMPLEMENTED — menunggu verifikasi owner di peramban` | 11 | **Tidak ada** (G-12) |
| `backend-roadmap-phase2.md` | 33 | 33 ✅ | 33 | Ada; 33 node cocok dengan kartu |
| `frontend-roadmap-phase2.md` | 18 | 18 ✅ | 18 | Ada; 18 node cocok dengan kartu |

Nol task selesai tanpa laporan. Pemeriksaan node lawan kartu dijalankan dengan skrip atas kedua
berkas Phase 2: nol selisih tanda, nol selisih kelas warna.

---

## 4. Papan skor

Bobot mengikuti audit 7 September, ditambah dimensi dokumentasi.

| Dimensi | Bobot | Skor | Bukti | Gap / blocker |
|---|---:|---:|---|---|
| **Fondasi** | 10% | **10 / 10** | Migration Accounting `20260902081432`, `20260909060515`, `20260914044507`, `20260924042630`, `20260928041937` ada di `Migrations/`@`618b206e` | — |
| **Backend — kelengkapan dan kontrak** | 20% | **8 / 10** | 68/68 action ber-`[AccessPermission]` + `[AccessAction]`, nama izin = `ControllerName` pada 68/68; 12 controller | **G-01** (jalur draft kejadian), G-13 (`FR-P2-038` hanya tempat) |
| **Frontend** | 15% | **8 / 10** | 29/29 task, 140 unit test Accounting: **138 lulus, 2 gagal** | **G-02** — regresi acceptance (3) `FE-ACC-009` |
| **Keamanan / privasi** | 10% | **7 / 10** | Penjaga badan hukum di setiap service `Acc*Service`; nol kolom pengenal pasien pada model `Acc*` (`ACC-DEC-056`) | `ACC-TD-002` `Tinggi` — dimitigasi penjaga `IsDefault` |
| **Integrasi / runtime** | 20% | **3 / 10** | Kotak masuk kejadian berjalan dengan pesan tiruan (`BE-ACC-P2-021`..`030`) | G-03..G-07: konfigurasi `Accounting` tidak ada, peran belum dipastikan, bagan akun belum sah, cutover G2/G3/G4/G6 terbuka |
| **Verifikasi** | 15% | **5 / 10** | Uji Swagger + layar developer tercatat untuk task Phase 2 (termasuk JSON mentah 29 September); 5/12 butir DoD Phase 2 | G-08 UAT nol, G-09 matriks acceptance draft |
| **Dokumentasi / governance** | 10% | **6 / 10** | Laporan lengkap, grafik Phase 2 konsisten | G-10..G-15 |

**Kemajuan scaffold** (kode ditulis, laporan ada): **±100%**. **Kesiapan sesungguhnya** (terbobot
di atas): **±62%**.

---

## 5. Definition of Done Phase 2 — `04-prd-to-mvp.md` bagian 27

| # | Butir | Jawaban | Bukti |
|---:|---|:---:|---|
| 1 | 44 FR punya kode yang berjalan | **Belum** | 43/44. `FR-P2-038` (shift kasir menahan tutup bulan) baru punya **tempat** di daftar periksa — butir `OPEN_CASH_SHIFTS` selalu `NotYetAvailable` dan tidak ada kartu yang membangun penegakannya (G-13) |
| 2 | 33 skenario UAT dijalankan dan tercatat di matriks berkolom bukti | **Belum** | Nol skenario `UAT-P2` di `acceptance-test-matrix.md`; UAT belum dijalankan untuk task mana pun (G-08, G-09) |
| 3 | `UAT-P2-02`, `12` terbukti lewat test integrasi PostgreSQL | **Tidak dapat dipenuhi apa adanya** | Proyek test dihapus 11 September 2026; `ACC-DEC-081` menyatakan test otomatis bukan acceptance. Butir DoD ini bertentangan dengan keputusan itu (G-14) |
| 4 | Seluruh endpoint Phase 2 ber-`[AccessPermission]` | **Ya** | 68/68, dihitung ulang hari ini |
| 5 | Seluruh service Phase 2 memanggil `AccountingLegalEntityGuard` | **Ya** | Nol service `Acc*Service` tanpa penjaga |
| 6 | Nol kolom pengenal pasien | **Ya** | Grep model `Acc*`: nol `Patient*`/`MedicalRecord*` |
| 7 | Build pada konfigurasi yang membangun project test | **Tidak dapat dipenuhi apa adanya** | Project test sudah tidak ada (G-14) |
| 8 | `ACC-XM-001` diratifikasi Billing dan Finance | **Ya** | `CLOSED` 24 September 2026, `ACC-DEC-082` |
| 9 | Daftar jenis kejadian ditetapkan bersama Finance | **Ya** | `DEC-ACC-P2-002` → `ACC-DEC-083` (17 kode, kini 26 di `ACC-XMOD` 3a) |
| 10 | Aturan posting terisi untuk setiap jenis kejadian | **Belum** | Dev hanya `PATIENT_PAYMENT`; aturan sungguhan menunggu bagan akun sah (G-05) |
| 11 | Akun laba ditahan dibuat dan ditetapkan | **Belum untuk produksi** | Layar dan endpoint ada (`FE-ACC-P2-005`), akunnya menunggu bagan akun sah (G-05) |
| 12 | Migration Phase 2 lewat Migration Coordination Gate | **Belum tercatat** | `evidence/04-migration-coordination-gate.md` hanya memuat `20260902081432`; empat migration Phase 2 tidak tercatat (G-10) |

**5 dari 12 ya.** Empat yang belum (1, 2, 10, 11) dan satu yang belum tercatat (12) adalah pekerjaan
nyata; dua (3, 7) adalah dokumen yang perlu diselaraskan dengan `ACC-DEC-081`.

---

## 6. Gap, diurutkan menurut dampak

### G-01 — Draft jurnal dari kejadian tidak dapat diajukan bila menyentuh control account · **Tinggi · backend**

> **DITUTUP 29 September 2026 sore — [`BE-ACC-P2-034`](../task/report/backend/BE-ACC-P2-034.md) ✅.** Draft kejadian
> `JU/2031/01/00003` (Kas Kasir + Piutang Pasien Umum) diajukan `200`; jurnal manual ke Kas Kasir tetap `422`.

**Proses bisnisnya.** Finance mengirim kejadian pembayaran pasien. Aturan posting berperlakuan
**Buat Draft** menyusun jurnal debit Kas Kasir, kredit Pendapatan. Petugas akuntansi memeriksa draft
itu lalu menekan **Ajukan**.

**Yang terjadi di source.**

1. `AccAccountingEventService` baris 606 membuat jurnal lewat `AccJournalService.CreateAsync` — tanpa
   larangan control account, benar.
2. Perlakuan Langsung Disahkan (baris 609–612) lewat `SahkanDariKejadianAsync` — tanpa larangan,
   benar.
3. Perlakuan Buat Draft berhenti sebagai draft. Saat diajukan, `AccJournalService.SubmitAsync` baris
   547 memanggil `PeriksaControlAccountSaatDiajukanAsync`, yang hanya mengecualikan tiga asal-usul di
   `BerasalDariJalurOtomatisAsync` (baris 1730–1752): hasil template, pembalikan penuh, dan jurnal
   penutup tahun. **Jurnal dari kejadian tidak dikenali** — `AccJournalService` sama sekali tidak
   merujuk `AccAccountingEvent`. Komentar di atas method itu masih menulis "Jalur kejadian akuntansi
   (`P2-1`) belum ada di kode".

**Akibat.** `POST /api/v1/corporate/accounting/journals/{id}/submit` atas draft hasil kejadian →
`422` "Akun 1-1002 Kas Kasir hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual."
Ini bertentangan dengan `ACC-DEC-064` dan `FR-P2-037`: jurnal dari kejadian **tidak** terkena larangan.

**Kenapa lolos sampai sekarang.** `FR-P2-037` di traceability menunda bukti jalur kejadian "saat
kotak masuk kejadian dibangun", dan penundaan itu tidak pernah ditagih. Draft kejadian yang ada di
dev, `JU/2031/01/00001` (`BE-ACC-P2-026` B5, aturan `PATIENT_PAYMENT` Buat Draft), belum pernah
diajukan.

**Cara membuktikan tanpa mengubah apa pun.** Buka rincian `JU/2031/01/00001`, tekan **Ajukan**.
Bila aturan `PATIENT_PAYMENT` memakai akun control, hasilnya `422` di atas. Bila tidak memakai akun
control, draft lolos — cacatnya tetap ada di source, hanya belum tersentuh data dev.

**Arah perbaikan (bukan dikerjakan di sini).** Tambahkan asal-usul keempat pada
`BerasalDariJalurOtomatisAsync`: ada `AccAccountingEvent` yang `JournalId`-nya menunjuk jurnal ini.
Tanpa migration. Lihat T-1.

### G-02 — Regresi acceptance (3) `FE-ACC-009` Neraca Saldo · **Sedang · frontend**

> **DITUTUP 29 September 2026 sore** atas keputusan Rizki "kembalikan ringkas" — test Accounting
> 140/140; build Rizki 15.04 memuat kalimatnya; `FE-ACC-009` ✅, pemeriksaan layar diserahkan ke tim UAT
> ([laporan bagian 10](../task/report/frontend/fe-acc-009-neraca-saldo.md)).

Acceptance (3): "Layar menyebutkan bahwa laporan hanya memuat jurnal yang sudah disahkan."
Commit `95ea41cd9` (25 September 2026, "memperbaiki tampilan neraca saldo") menjadikan
`postedOnlyNotice` dan `scopeNotice` komentar di `trial-balance-constants.jsx`. Kalimat itu kini hanya
muncul saat tabel kosong. Dua unit test menangkapnya:

```text
node --import ./tests/helpers/register.mjs --test tests/unit/accounting-trial-balance.test.mjs
not ok 2 - ringkasan memuat total debit, total kredit, dan keseimbangan
not ok 6 - layar menyebutkan hanya jurnal disahkan dan batas cakupan akun
```

Ini selama ini dicatat sebagai "9 gagal di luar task" pada laporan frontend lain — benar di luar
task-task itu, tetapi **di dalam modul ini**. Butuh keputusan owner: kembalikan pemberitahuannya,
atau ubah acceptance (3) lewat `manage-module-blueprint`, lalu selaraskan test.

### G-03 — Konfigurasi runtime Accounting tidak ada · **Tinggi · operasional**

`appsettings.json` dan `appsettings.Development.json` tidak punya bagian `Accounting`, sehingga
bawaan kode yang berlaku (`Program.cs` baris 710–717):

| Pengaturan | Nilai berlaku | Akibat |
|---|---|---|
| `Accounting:RecurringJournalScheduler:Enabled` | `false` | Jurnal berulang tidak pernah terbit otomatis; hanya lewat `POST /recurring-journals/{id}/generate` |
| `Accounting:RecurringJournalScheduler:SystemActorUserId` | kosong | Bila dinyalakan, draft terbit atas nama `Guid.Empty` |
| `Accounting:AccountingEventScheduler:SystemActorUserId` | kosong | Coba ulang otomatis dan pengesahan Langsung Disahkan tercatat atas nama `Guid.Empty` — jejak audit tanpa pelaku |

Sudah tercatat sebagai langkah 9 `MODULE-STATUS.md`; belum dikerjakan.

### G-04 — Peran dan pemberian hak akses belum dipastikan · **Tinggi · operasional**

Hak akses diberikan admin lewat layar Akses Role, bukan kode. Tujuh peran (`ACC-DEC-055`, termasuk
`Accounting Director` yang satu-satunya berhak `AccountingPeriod : Approve`) belum terbukti ada.
Bukti tidak langsung: uji `BE-ACC-P2-029` S4 hanya dapat dijalankan satu pengguna, dan catatan
11 September menemukan nol pemberian untuk `RecurringJournal`, `AccountingConfiguration`,
`YearEndClosing`. Audit ini tidak memeriksa database. Query baca-saja untuk Rizki:

```sql
SELECT c."ControllerName", a."ActionName", COUNT(p."Id") AS pemberian
FROM "SysControllerAccess" c
JOIN "SysActionAccess" a ON a."ControllerAccessId" = c."Id"
LEFT JOIN "SysAccessPolicy" p ON p."ActionAccessId" = a."Id"
WHERE c."ControllerName" IN ('ChartOfAccount','JournalType','AccountingPeriod','Journal','GeneralLedger',
  'AccountingEvent','EventType','PostingRule','RecurringJournal','AccountingConfiguration',
  'YearEndClosing','AccountingReconciliation')
GROUP BY 1, 2 ORDER BY 1, 2;
```

### G-05 — Bagan akun sah belum disusun (`ACC-TD-022`) · **Tinggi · data**

Menahan butir DoD 10 dan 11 serta gerbang cutover G2. Pemilik: pemilik proses akuntansi.

### G-06 — Gerbang cutover Finance G2, G3, G4, G6 terbuka · **Tinggi · integrasi**

Accounting sanggup menerima pesan tiruan, tetapi pengiriman sungguhan dari Finance belum aktif.
`evidence/14` (balasan atas kode Finance `05`, `06`, `07`) ditulis 28 September dan **belum dikirim**
ke Yasmin.

### G-07 — Pekerjaan belum masuk integration · **Sedang · governance**

Backend: working tree memuat source `BE-ACC-P2-029`/`030` dan belasan dokumen yang belum di-commit;
6 commit belum di-PR. Frontend: 6 commit belum di-PR. Catatan PR yang sudah menunggu: snapshot
`7509e18c` menghapus empat blok ganda Pharmacy (langkah 5 `MODULE-STATUS.md`).

### G-08 — UAT belum dijalankan untuk task mana pun · **Tinggi · verifikasi**

Sesuai aturan kerja 11 September, UAT milik tim terpisah dan bukan penghalang development. Tetapi
"100% finish" modul memang menuntutnya (DoD butir 2). Langkah 7 `MODULE-STATUS.md` — serahkan ke
tim UAT — belum dikerjakan.

### G-09 — Matriks acceptance belum layak menampung bukti · **Sedang · verifikasi**

`acceptance-test-matrix.md`: `ACC-TEST-0.1`, `draft`, tanpa `approved_by`, hanya MVP, kolom
"Bukti yang diharapkan" saja. Skenario `UAT-P2-01`..`33` hanya ada di `04-prd-to-mvp.md`. Dua
coverage gap roadmap frontend revisi 7 ikut di sini: belum ada `UAT-P2` untuk `ACC-DEC-095` dan
`096`.

### G-10 — Migration Phase 2 tidak tercatat di gerbang koordinasi · **Rendah · dokumentasi**

`evidence/04` tidak memuat `20260909060515_AddAccountingPhase2Independent`,
`20260914044507_AddAccountingPostingRuleMaster`, `20260924042630_AddAccountingEventInbox`,
`20260928041937_AddAccSubledgerBalance`.

### G-11 — `MODULE-STATUS.md` kepala usang · **Rendah · dokumentasi**

Menulis revision `11`, status `IN_PROGRESS`, verifikasi terakhir 3 September, SHA verifikasi
`f879944`. Manifest menulis revision `13` dan `last_verified_at` 7 September. Keduanya perlu
diselaraskan ke audit ini.

### G-12 — Roadmap MVP tanpa grafik dependency dan kosakata status lama · **Rendah · dokumentasi**

Kedua roadmap MVP tidak punya bagian `## Grafik Urutan Dependency` dan memakai `DONE`/`IMPLEMENTED`
alih-alih ✅ 🟡 ⛔. Kartu `FE-ACC-004` masih menulis "Sisa: `npm run build` owner" padahal
`FE-ACC-P2-014` yang menjawabnya sudah ✅. Kembali ke `plan-module-delivery`.

### G-13 — `FR-P2-038` shift kasir tanpa kartu penegakan · **Sedang · desain**

Daftar periksa menyediakan butir `OPEN_CASH_SHIFTS` yang selalu "belum dapat diperiksa". Tidak ada
kartu di roadmap mana pun yang membangun penegakannya, dan penegakan itu menunggu Finance mengirim
`CASH_SHIFT_CLOSED`. Empat peringatan lain juga masih "belum tersedia" (`INTEGRATION_MISMATCH`,
`DEPRECIATION_NOT_RUN`, `OPENING_CLOSING_MISMATCH`, dan `SUSPENSE_ACCOUNT_BALANCE` yang menurut
`ACC-TD-025` seharusnya dicabut) — peringatan, bukan penghalang, tetapi statusnya perlu diputuskan.

### G-14 — DoD Phase 2 butir 3 dan 7 bertentangan dengan `ACC-DEC-081` · **Rendah · desain**

Kedua butir menuntut proyek test yang sudah dihapus atas keputusan owner dan arahan lead.

### G-15 — Utang teknis terbuka lainnya · **Rendah–Sedang**

`ACC-TD-018` performa buku besar (`Sedang`); `ACC-TD-001`, `005`, `006`, `010`, `012`, `015`, `025`
(`Rendah`). Jumlah warning build backend untuk build 29 September belum pernah dilaporkan.

---

## 7. Yang sudah terbukti dan tidak perlu diulang

- Tutup bulan lengkap: daftar periksa, ajukan, empat mata (`403`), tolak beralasan, rekonsiliasi
  subledger sebagai penghalang keempat, kejadian Gagal limpahan (`BE-ACC-P2-029`, JSON 29 September).
- Aturan posting menolak jenis Saldo Subledger (`BE-ACC-P2-030`, JSON + tangkapan layar).
- Rekonsiliasi saldo subledger per periode (`BE-ACC-P2-014`, `FE-ACC-P2-016`).
- Kotak masuk kejadian: anti-ganda, tertahan, gagal, coba ulang, abaikan (`BE-ACC-P2-019`..`026`).
- Laporan uji ringkas buatan agen di folder `testing/` (`live-browser-testing-report-2026-09-29.md`,
  `test-report-be-acc-p2-029-2026-09-29.md`) **tidak** dipakai sebagai bukti; yang dipakai JSON mentah
  dan tangkapan layar di `QuilvianSystemFrontendDev/test-with-agy/`.
- *(ditambahkan 30 September 2026)* Hapus draft dan jurnal Ditolak hasil kejadian mengembalikan kejadian
  ke Gagal; jurnal hasil kejadian tidak dapat disunting; jurnal hasil kejadian Ditolak tetap dihitung
  belum disahkan (`BE-ACC-P2-035`, `FE-ACC-P2-019`, JSON + delapan tangkapan 29 September 16.10).
  Ringkasan agen `test-report-be-acc-p2-035-fe-acc-p2-019-2026-09-29.md` juga tidak dipakai; klaimnya
  diluruskan di laporan `BE-ACC-P2-035` bagian 5.3.

---

## 8. Task berikutnya — urutan menuju "100% finish"

| # | Task | Arah | Pemilik | Menutup |
|---:|---|---|---|---|
| T-1 | Kenali jurnal dari kejadian sebagai asal-usul otomatis di `BerasalDariJalurOtomatisAsync`; uji dengan mengajukan `JU/2031/01/00001` | `plan-module-delivery` (kartu `BE-ACC-P2-034`) → `build-module-backend` | Rizki | G-01, `FR-P2-037` |
| T-2 | Putuskan nasib pemberitahuan Neraca Saldo; kembalikan atau ubah acceptance, lalu selaraskan test | Keputusan owner → `build-module-frontend` | Rizki | G-02 |
| T-3 | Commit dokumen dan source 29 September; PR `rizkiG` → integration dan `RizkiV2` → integration | Manual | Rizki | G-07 |
| T-4 | Isi bagian `Accounting` di appsettings: `SystemActorUserId` kedua penjadwal, `RecurringJournalScheduler:Enabled` sesuai keputusan | `manage-module-blueprint` (keputusan) → task konfigurasi | Rizki + Platform | G-03 |
| T-5 | Buat tujuh peran dan centang hak di layar Akses Role menurut `permission-audit-matrix.md`; jalankan query G-04 | Operasional | Admin sistem | G-04 |
| T-6 | Susun bagan akun sah, lalu aturan posting untuk 26 kode dan akun laba ditahan | Pemilik proses akuntansi | Pemilik proses akuntansi | G-05, DoD 10, 11 |
| T-7 | Kirim `evidence/14` ke Yasmin; tutup G3, G4, G6 | Rizki → Yasmin | Rizki, Yasmin, Platform | G-06 |
| T-8 | Susun matriks acceptance Phase 2 berkolom bukti, tambah `UAT-P2` untuk `ACC-DEC-095`/`096`, serahkan ke tim UAT | `plan-module-delivery` | Rizki → tim UAT | G-08, G-09, DoD 2 |
| T-9 | Amandemen dokumen: DoD 3 dan 7 selaras `ACC-DEC-081`; status `FR-P2-038` dan empat peringatan; `evidence/04`; kepala `MODULE-STATUS.md`; grafik dan kosakata roadmap MVP | `manage-module-blueprint` + `plan-module-delivery` | Rizki | G-10..G-14 |

Sesudah T-1 dan T-2 terbukti, modul layak dinilai ulang sebagai `READY_WITH_CONDITIONS`: sisa
syaratnya (T-4..T-8) berpemilik jelas dan tidak menyentuh kode Accounting lagi.

> **Pembaruan 30 September 2026 — bukan penilaian ulang.** Verdict di atas tetap `NOT_READY` sampai
> audit dijalankan lagi lewat `verify-module-readiness`.
>
> | # | Keadaan |
> |---|---|
> | T-1 | ✅ `BE-ACC-P2-034`, 29 September — G-01 ditutup |
> | T-2 | ✅ `FE-ACC-009`, 29 September — G-02 ditutup |
> | Susulan T-1 | ✅ Keputusan terbuka OQ-034-1/2 yang muncul saat T-1 → `ACC-DEC-116`..`121` → `BE-ACC-P2-035` + `FE-ACC-P2-019`, uji gabungan 29 September 16.10 diperiksa 30 September. Roadmap Phase 2 kini backend 35/35, frontend 19/19 |
> | T-3 | 🟡 Commit sudah — backend `8f530926`, frontend `2c2190858`, keduanya sejajar dengan origin branch masing-masing; PR ke integration belum (7 commit di depan pada tiap repository). Pelurusan dokumen 30 September belum di-commit |
> | T-4..T-9 | Belum dikerjakan |
