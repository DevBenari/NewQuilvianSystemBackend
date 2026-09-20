# Finance Management — Roadmap Pengiriman

## 1. Identitas

```yaml
roadmap_id: FIN-ROADMAP-001
roadmap_revision: 3
roadmap_status: ACTIVE
blueprint_id: FIN-BP-001
blueprint_revision: 2
blueprint_status: approved
created_at: 2026-09-20T00:00:00+07:00
planned_by: /quilvian-engineering-skills:plan-module-delivery
children:
  backend: roadmap/01-backend-roadmap.md — FIN-ROADMAP-BE-001
  frontend: roadmap/02-frontend-roadmap.md — FIN-ROADMAP-FE-001

backend_commit_sha: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
frontend_commit_sha: abed49b03

input_hashes:
  00-interview-decisions.md: 74529813218c0354e9289b4f9eea5a2bc68205572e195333b0a784e11cfccc2a
  01-existing-capability-map.md: 0576bf65224c76acd7853c18f04bf5683162f4620a15042ba95486a59a018b8a

approval_basis: >
  Yasmin (Product/Domain Owner Finance), 20 September 2026 — dua approval terpisah:
  (1) keputusan arsitektur FIN-DES-001..024; (2) cakupan MVP dan urutan gelombang pada
  04-prd-to-mvp.md bagian 7, 8, dan 20.1.
  Ditambah pada hari yang sama, sesudah revisi 1 roadmap ini:
  (3) FIN-DES-025..028; (4) penguncian tujuh kontrak turunan ke 1.0.
  Tidak ada lagi keputusan arsitektur Finance yang `draft`.
```

**Roadmap ini tidak menyatakan satu pun pekerjaan selesai.** Ia menurunkan cakupan yang sudah
disetujui menjadi task, dan menyebut dengan tegas apa yang tertahan beserta pemiliknya.

Berkas ini adalah **payung**. Task-nya sendiri ada di dua berkas tersendiri:

| Berkas | Isi |
|---|---|
| `01-backend-roadmap.md` | 21 task `BE-FIN-*`, urutan eksekusi, rincian per gelombang, prasyarat, DoD backend |
| `02-frontend-roadmap.md` | 6 task `FE-FIN-*`, yang MUST diputuskan UI brief, `DEV_DISCRETION`, DoD frontend |

Yang tetap di sini: identitas, penguncian kontrak, gelombang, traceability lintas keduanya,
coverage gap, dan risiko.

## 2. Penguncian versi kontrak — **DISETUJUI 20 September 2026**

Owner menyetujui usulan penguncian revisi 1 apa adanya. Ketujuh kontrak kini `locked`.

| Kontrak | Dari | Terkunci pada | Cakupan penguncian |
|---|---|---|---|
| `contracts/api-contract.md` | `FIN-API-0.1` | `FIN-API-1.0` ✅ | Seluruh permukaan yang berakar `FIN-DES-001`..`028` |
| `contracts/state-transition-matrix.md` | `FIN-STATE-0.1` | `FIN-STATE-1.0` ✅ | Idem |
| `contracts/validation-matrix.md` | `FIN-VAL-0.1` | `FIN-VAL-1.0` ✅ | Idem |
| `contracts/permission-audit-matrix.md` | `FIN-PERM-0.1` | `FIN-PERM-1.0` ✅ | Idem |
| `contracts/integration-contract.md` | `FIN-INTEGRATION-0.1` | `FIN-INTEGRATION-1.0` ✅ | Intake Billing dan kotak keluar Accounting; **tidak** termasuk `BilCollectionHandoff` |
| `testing/acceptance-test-matrix.md` | `FIN-TEST-0.1` | `FIN-TEST-1.0` ✅ | Idem |
| `04-prd-to-mvp.md` | `FIN-MVP-0.1` | `FIN-MVP-1.0` ✅ | Bagian 7, 8, dan 20.1 yang sudah disetujui |

Rumpun Payable **ikut terkunci**: dasar pengecualiannya pada revisi 1 adalah status `draft`
`FIN-DES-025`..`028`, dan status itu sudah tercabut oleh approval hari ini.

**Tiga permukaan tetap tidak terkunci**, dan alasannya bukan status keputusan melainkan
ketergantungan pada pihak lain:

| Permukaan | Menunggu siapa |
|---|---|
| `BilCollectionHandoff` | Owner Billing (`FIN-DEC-005`) |
| Perluasan `BilArHandoff` untuk manfaat karyawan | Owner Billing + HR (`FIN-DEC-006`, `FIN-DEC-016`) |
| Pengiriman kejadian ke Accounting | Rizki — endpoint penerima belum ada (`FIN-CAP-018`) |

Ketiganya kelak **menambah permukaan baru** yang dikunci tersendiri; kedatangannya tidak
menaikkan versi ketujuh kontrak di atas.

**Konsekuensi penguncian:** kerja paralel backend–frontend kini **diizinkan** untuk seluruh task
yang kontraknya terkunci. Yang masih menahan task `FE-FIN-*` hanyalah ketiadaan UI brief —
dan itu keputusan Product Owner, bukan keterikatan kontrak.

## 3. Gelombang

Mengikuti `04-prd-to-mvp.md` bagian 20.1 apa adanya. Tidak ada epic yang dipindah, ditambah,
atau dihapus.

| Gelombang | Epic | Task | Keadaan |
|---|---|---|---|
| `MVP-0` | `FIN-01` | `BE-FIN-001`..`004` · `FE-FIN-001` | **Siap dimulai** |
| `MVP-1` | `FIN-02`, `FIN-03` | `BE-FIN-005`..`009` · `FE-FIN-002` | Siap setelah `MVP-0` |
| `MVP-5` | `FIN-11` | `BE-FIN-010`..`012` · `FE-FIN-006` | Paralel sejak `MVP-1`; selesai setelah `MVP-4` |
| `MVP-4` | `FIN-10` | `BE-FIN-013`..`015` · `FE-FIN-003` | Siap setelah `MVP-1` — lihat catatan 3.1 |
| `MVP-2` | `FIN-05` | `BE-FIN-016`..`017` · `FE-FIN-004` | **BLOCKED** — `BilCollectionHandoff` |
| `MVP-3` | `FIN-06` | `BE-FIN-018` | **BLOCKED** — turunan `MVP-2` |
| `POST-MVP` | `FIN-07` | `BE-FIN-019` | Tidak menunggu siapa pun |
| `POST-MVP` | `FIN-09` | `BE-FIN-020` | **BLOCKED** — hanya `FIN-OQ-010` (ambang nominal); modelnya sudah bebas |
| `POST-MVP` | `FIN-08` | `BE-FIN-021` | **BLOCKED** — menunggu Medical Fee `BE-MDF-014` |
| `POST-MVP` | `FIN-13` | `FE-FIN-005` | Tidak mengunci apa pun |
| **Di luar gelombang** | `FIN-04`, `FIN-12` | — | `OPEN DECISION`, tidak diturunkan menjadi task |

### 3.1 Catatan urutan `MVP-4`

Bagian 20.1 menyatakan `MVP-4` dapat dikerjakan **lebih dahulu** bila `BilCollectionHandoff`
belum tersedia. Roadmap ini mengikuti itu, dengan satu batas yang MUST dihormati:

`FinanceCashManagementService` menghitung kas tersedia dari **kas kasir** (`FIN-CAP-006`,
`BilCashierShift`) — bukan dari `FinReceipt`. Selama `MVP-2` belum ada, task `BE-FIN-014`
MUST membaca kas kasir langsung dan MUST NOT membuat jalur sementara yang kelak dibongkar.
Bila implementer menemukan bahwa angka kas tersedia ternyata menuntut `FinReceipt`, itu temuan
yang MUST dilaporkan balik ke pass desain, bukan diselesaikan dengan improvisasi.

## 4. Indeks task

Rincian lengkap 11 kolom ada di berkas anak. Tabel ini hanya indeks.

### 4.1 Backend — `01-backend-roadmap.md`

| Task | Outcome ringkas | Gelombang | Keadaan |
|---|---|---|---|
| `BE-FIN-001` | Pendaftaran enam submodul ke registry | `MVP-0` | Siap |
| `BE-FIN-002` | Entity dan configuration data induk | `MVP-0` | Siap |
| `BE-FIN-003` | Migration `AddFinanceMasterData` | `MVP-0` | Siap — butuh otorisasi migration |
| `BE-FIN-004` | API data induk | `MVP-0` | Siap |
| `BE-FIN-005` | Pintu masuk fakta Billing | `MVP-1` | Siap |
| `BE-FIN-006` | Entity buku piutang | `MVP-1` | Siap |
| `BE-FIN-007` | Migration intake dan piutang | `MVP-1` | Siap — butuh otorisasi migration |
| `BE-FIN-008` | Layanan piutang dan umur piutang | `MVP-1` | Siap |
| `BE-FIN-009` | API intake dan piutang | `MVP-1` | Siap |
| `BE-FIN-010` | Kotak keluar kejadian Accounting | `MVP-5` | Siap — butuh otorisasi migration |
| `BE-FIN-011` | Outbox ikut transaksi pemanggil | `MVP-5` | Siap |
| `BE-FIN-012` | Pantauan kejadian (baca saja) | `MVP-5` | Siap |
| `BE-FIN-013` | Entity kas dan setoran | `MVP-4` | Siap — butuh otorisasi migration |
| `BE-FIN-014` | Kas tersedia dan penutupan harian | `MVP-4` | Siap — lihat catatan 3.1 |
| `BE-FIN-015` | API setoran dan kas harian | `MVP-4` | Siap |
| `BE-FIN-016` | Penerimaan dari tender kasir | `MVP-2` | **BLOCKED** — owner Billing |
| `BE-FIN-017` | Pembagian bayar-vs-piutang | `MVP-2` | **BLOCKED** — owner Billing |
| `BE-FIN-018` | Alokasi, koreksi, penghapusan | `MVP-3` | **BLOCKED** — turunan `MVP-2` |
| `BE-FIN-019` | Utang supplier | `POST-MVP` | Siap |
| `BE-FIN-020` | Pembayaran keluar dan potongan | `POST-MVP` | **BLOCKED** — `FIN-OQ-010` |
| `BE-FIN-021` | Utang jasa tenaga medis | `POST-MVP` | **BLOCKED** — Medical Fee `BE-MDF-014` |

### 4.2 Frontend — `02-frontend-roadmap.md`

| Task | Outcome ringkas | Menunggu backend | Keadaan |
|---|---|---|---|
| `FE-FIN-001` | Pengelolaan data induk | `BE-FIN-004` | **UI brief** |
| `FE-FIN-002` | Buku piutang dan umur piutang | `BE-FIN-009` | **UI brief** |
| `FE-FIN-003` | Setoran bank dan kas harian | `BE-FIN-015` | **UI brief** |
| `FE-FIN-006` | Pemantauan fakta Billing dan kejadian Accounting | `BE-FIN-009`, `BE-FIN-012` | **UI brief** |
| `FE-FIN-004` | Penerimaan dan alokasi | `BE-FIN-018` | **BLOCKED** — owner Billing |
| `FE-FIN-005` | Merapikan Petty Cash ke rute Finance | — | `POST-MVP` |

`FE-FIN-006` ditambahkan saat roadmap dipecah: `03-frontend-architecture.md` bagian 3.5
menuntut dua layar pemantauan, dan keduanya sebelumnya tidak punya task frontend sama sekali.

### 4.3 Task yang sengaja tidak dibuat

| Epic | Alasan |
|---|---|
| `EPIC FIN-04` — piutang manfaat karyawan | `OPEN DECISION`; menunggu owner Billing **dan** HR (`FIN-DEC-006`, `FIN-DEC-016`, `FIN-CQ-03`) |
| `EPIC FIN-12` — pengiriman kejadian ke Accounting | `OPEN DECISION`; endpoint penerima belum dibangun (`FIN-CAP-018`) |
| `FinDoctorPayable`, `FinDoctorPayableItem` | Dibatalkan pada revisi 2; digantikan `FinMedicalServicePayable` |
| Migration `AddFinanceSubledgerPeriodBalance` | `FIN-OQ-011` belum dijawab; rumpunnya `POST-MVP` |

## 5. Traceability

| Requirement | Decision | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-FIN-001`..`004` | `FIN-DEC-013`, `FIN-DEC-014` | `FIN-DES-001`..`005` | `FIN-API-1.0`, `FIN-VAL-1.0` | `BE-FIN-001`..`004` | `FE-FIN-001` | `UAT-01`, `UAT-02` | Siap |
| `FR-FIN-010`..`013` | `FIN-DEC-005` (konsumsi) | `FIN-DES-008`, `009` | `FIN-INTEGRATION-1.0` | `BE-FIN-005`, `009` | `FE-FIN-006` | `UAT-04` | Siap |
| `FR-FIN-020`..`024` | `FIN-DEC-010`..`012` | `FIN-DES-010`..`013` | `FIN-VAL-1.0` | `BE-FIN-006`..`009` | `FE-FIN-002` | `UAT-03` | Siap |
| `FR-FIN-030`..`035` | `FIN-DEC-005`, `FIN-DEC-015` | `FIN-DES-014` | `FIN-INTEGRATION-1.0` — permukaan collection dikecualikan | `BE-FIN-016`, `017` | `FE-FIN-004` | `UAT-05`, `06`, `20` | **BLOCKED** |
| `FR-FIN-040`..`046` | `FIN-DEC-017` | `FIN-DES-014` | `FIN-STATE-1.0` | `BE-FIN-018` | `FE-FIN-004` | `UAT-08`..`12` | **BLOCKED** |
| `FR-FIN-050`, `051` | `FIN-DEC-019` | `FIN-DES-015`, `026`..`028` | `FIN-API-1.0`, `FIN-VAL-1.0` | `BE-FIN-020` | — | — | **BLOCKED** — `FIN-OQ-010` |
| `FR-FIN-060`..`065` | `FIN-DEC-018` | `FIN-DES-018`..`020` | `FIN-VAL-1.0` | `BE-FIN-013`..`015` | `FE-FIN-003` | `UAT-13`..`16` | Siap |
| `FR-FIN-070`..`074` | `FIN-DEC-001`, `004` | `FIN-DES-017`, `021`..`023` | `FIN-INTEGRATION-1.0`, `ACC-XMOD-0.2` | `BE-FIN-010`..`012` | `FE-FIN-006` | `UAT-07`, `UAT-17`..`19` | Siap |
| `FR-FIN-075` | `FIN-DEC-003`, `019` | `FIN-DES-025` | `FIN-API-1.0` | `BE-FIN-021` | — | — | **BLOCKED** — Medical Fee |
| Manfaat karyawan | `FIN-DEC-006`, `016` | — | — | — | — | — | `OPEN DECISION` |
| Pengiriman ke Accounting | `FIN-DEC-007` | `FIN-DES-024` | — | — | — | — | `OPEN DECISION` |

## 6. Coverage gap

| Gap | Akibat | Pemilik |
|---|---|---|
| `FR-FIN-051` (potongan tidak menyisakan utang) tanpa baris uji pada `FIN-TEST-0.1` | Aturan paling halus pada rumpun pembayaran tanpa penjaga | Pass desain revisi 2 |
| Rumpun manfaat karyawan tanpa requirement yang lengkap | Tidak dapat direncanakan; sudah dikeluarkan dari seluruh gelombang | Owner Billing + HR |
| Katalog 17 jenis kejadian Accounting belum diratifikasi Rizki | Kejadian yang jenisnya belum terdaftar akan tertahan saat pengiriman aktif | Rizki (Accounting) |
| Ambang nominal approval AP (`FIN-OQ-010`) | `BE-FIN-020` tidak dapat mengunci aturan validasi angkanya | Finance Supervisor + Yasmin |

Seluruh 20 skenario UAT pada `04-prd-to-mvp.md` bagian 18 sudah tertaut ke task. Yang tersisa
pada tabel di atas adalah gap yang pemiliknya berada **di luar** roadmap ini.

## 7. Prasyarat eksekusi

Rinciannya ada di berkas anak: `01-backend-roadmap.md` bagian 6 dan `02-frontend-roadmap.md`
bagian 5. Ringkasnya:

| # | Prasyarat | Status |
|---:|---|---|
| 1 | QBE preflight diselesaikan **pada waktu eksekusi**, dari `AGENTS.md` backend target | Berlaku terus |
| 2 | `BE-FIN-001` selesai sebelum file model pertama | **Belum** |
| 3 | Otorisasi terpisah untuk membuat **dan** menjalankan setiap migration | **Belum** |
| 4 | Implementasi lewat `quilvian-engineering-skills:build-module-backend` | Berlaku terus |
| 5 | Task `BLOCKED` MUST NOT dimulai | Berlaku terus |
| 6 | Penguncian versi kontrak | **Terpenuhi** 20 September 2026 |
| 7 | UI brief disetujui Product Owner sebelum task `FE-FIN-*` pertama | **Belum** |

## 8. Risiko

| Risiko | Dampak | Mitigasi |
|---|---|---|
| `BilCollectionHandoff` tidak kunjung dikonfirmasi | `MVP-2` dan `MVP-3` berhenti | `MVP-4` sudah diizinkan berjalan lebih dahulu; `MVP-5` paralel |
| Kontrak terkunci ternyata keliru saat implementasi | Perubahan kontrak `1.0` kini berbiaya lebih mahal daripada saat `draft` | Temuan seperti itu MUST dinaikkan sebagai revisi kontrak bernomor, bukan diselesaikan diam-diam di kode |
| Kas tersedia ternyata menuntut `FinReceipt` | Urutan `MVP-4` sebelum `MVP-2` gugur | Catatan 3.1 mewajibkan temuan itu dilaporkan balik, bukan diakali |
| UI brief tidak kunjung ada | Seluruh `FE-FIN-*` berhenti | Backend tidak tertahan; kontrak fungsional sudah berdiri |
| Endpoint Accounting dibangun lebih cepat dari perkiraan | `EPIC FIN-12` dapat dimulai lebih awal | Kotak keluar sudah terisi sejak `MVP-5`; tinggal mengirim antrean |

## 9. Yang roadmap ini tidak lakukan

- Tidak menandai satu pun task selesai.
- Tidak menetapkan approval sendiri: ketiga approval pada revisi 2 ini diberikan owner secara langsung dan dicatat apa adanya.
- Tidak menyembunyikan satu pun dependency eksternal: keempat pemiliknya disebut namanya.
- Tidak menjalankan builder mana pun.
- Tidak mengubah cakupan MVP yang sudah dikunci owner.
