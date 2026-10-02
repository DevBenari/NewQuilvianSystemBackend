# State Transition Matrix — Rawat Jalan Billing

> **Revisi `27` (28 September 2026):** amendment **V2 — Rawat Jalan ke invoice canonical** ada di bagian akhir berkas ini dan **berlaku** untuk jalur fakta klinis → tagihan. Isi di atasnya adalah riwayat desain `RJ-BIL`.

| Field | Nilai |
|---|---|
| Contract version | `RJ-BIL-STATE-001@1.0.0` |
| Status | `draft` |
| Source | Decision revision `10`, domain architecture revision `1` |

## Processing outcome

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `Received` | Mulai proses | `InProgress` | Billing Integration | Identity/version valid | Tolak validation |
| `InProgress` | Semua komponen diterapkan | `Succeeded` | Billing Service | Rule/tariff valid | Masuk failure/review |
| `InProgress` | Komponen sebagian diterapkan | `PartialOutcome` | Billing Service | Komponen gagal terlihat | Jangan rollback komponen yang sudah applied |
| `InProgress` | Gagal sebelum efek | `FailedBeforeEffect` | Billing Integration | Error permanen sebelum mutation | Retry hanya sesuai policy |
| `InProgress` | Outcome tidak diketahui | `OutcomeUnknown` | Billing Integration | Timeout/response loss | Status query/reconciliation wajib |
| `OutcomeUnknown` | Verifikasi ulang | `Succeeded`/`PartialOutcome`/`PendingReconciliation` | Integration owner | Original identity dipakai | Tidak boleh membuat key baru |
| `Succeeded` | Replay sama | `Succeeded` | System | Fingerprint sama | Kembalikan hasil canonical |
| `Succeeded` | Versi lebih baru | `PendingReconciliation` | Billing Service | Newer fact supersedes prior | Jangan menimpa charge lama |

## Folio dan charge

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `Open` | Charge/fact masuk | `ReviewRequired` atau `ReadyToClose` | Billing Service | Hasil kalkulasi valid | Tolak bila identity duplicate |
| `ReviewRequired` | Review selesai | `ReadyToClose` | Billing reviewer | Semua component resolved | Tetap review bila rule belum ada |
| `ReadyToClose` | Close | `Closed` | Billing + policy | Allocation, approval, reconciliation selesai | Tolak close |
| `Closed` | Reopen | `Open`/`ReviewRequired` | Authorized high-risk workflow | Approval valid dan histori dipertahankan | Tolak tanpa approval |
| `Recognized` | Correction | `Superseded` | Billing via approved action | New version dan reason | Original tetap immutable |
| `Recognized` | Void | `Voided` | Financial action executor | Approval/state valid | Tolak direct clinical cancel |
| `Recognized` | Reversal | `Reversed` | Financial action executor | Accounting consequence valid | Gunakan action baru, bukan overwrite |

Transisi lain yang tidak tercantum dianggap tidak sah. Status final target harus mempertahankan
makna ini walaupun nama enum hilir berubah.



---

# Amendment V2 — Rawat Jalan ke Invoice Canonical

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-E2E-CONTRACT-001@1.0.2` (`RJ-E2E-DEC-016`) |
| Status | `approved` — Sukma Giri, 2026-09-28 |
| Owner | Billing + Clinical Integration; titik sentuh Registration dan Pharmacy |
| Traceability | `RJ-E2E-DEC-003`, `005`, `007`, `009`, `010`, `013`, `014` |

Tiga keadaan berikut **tidak boleh dicampur** (PRD §25). Contoh: Lab `ACCEPTED` tidak berarti
invoice `CLOSED`, dan resep `DISPENSED` tidak berarti sudah dibayar.

## V2-A. Sinkron efek folio ke invoice (`BillingInvoiceSyncStatus`)

> **Koreksi kontrak `RJ-E2E-CONTRACT-001@1.0.2` (`RJ-E2E-DEC-016`, 28 September 2026).** Di seluruh amendment V2, setiap penyebutan kolom sinkron/rekonsiliasi pada **`BilChargeLine`** dibaca sebagai **`BilProcessingEffect`**. Sebabnya: folio mencatat setiap versi fakta sebagai satu `BilProcessingEffect` (unik per konteks + fakta + versi + jenis efek) dan tidak membuat `BilChargeLine` baru untuk revisi (`BillingFolioService.cs:183-266`). `BilChargeLine` **tidak diubah**. Tambahan: kolom `InvoiceSyncVersion` (`int`, bawaan `0`) sebagai token konkurensi karena `BilProcessingEffect` tidak punya kolom `Version`. Encounter dicapai lewat `FolioId → BilFolio.EncounterId`. Rincian kolom: `data/data-dictionary.md` bagian 2.2.


| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| (baru) | Folio mencatat baris yang layak (`V2.7.1`) | `Pending` | Sistem (`BillingFolioService`) | Encounter `Outpatient`, konteks termasuk lima konteks | — |
| (baru) | Folio mencatat baris yang tidak layak | `NotApplicable` | Sistem | Salah satu syarat kelayakan gagal | — |
| `Pending` / `Failed` | Jembatan berhasil (item, void, adjustment, atau selisih nol) | `Synced` | Sistem (jembatan, pekerja) | Invoice menerima perubahan | — |
| `Pending` / `Failed` | Jembatan gagal sementara (koneksi, lock, konflik sementara) | `Failed` | Sistem | Percobaan < `MaxAttemptCount` | — |
| `Pending` / `Failed` | Gagal permanen (`TARIFF_NOT_FOUND`, `ADJUSTMENT_REJECTED`) **atau** percobaan = `MaxAttemptCount` **atau** kebijakan tidak aktif | `ReconciliationRequired` | Sistem | — | — |
| `ReconciliationRequired` / `Failed` | Petugas menekan Kirim Ulang | `Pending` | Petugas Billing (`BillingChargeReconciliation : Update`) | Penyebab sudah dibenahi, mis. tarif sudah diisi | `422` bila item `Synced`/`Resolved` |
| `ReconciliationRequired` | Petugas menyatakan selesai manual | `Resolved` | Petugas Billing | Catatan 10–500 karakter | `422` bila catatan kosong |
| `Synced` | Kirim ulang | — | Tidak ada | **Tidak sah** | `422 RJE-VAL-020` |
| `Resolved` | Apa pun | — | Tidak ada | **Tidak sah** — keputusan manual final | `409 RJE-VAL-021` |
| `NotApplicable` | Diteruskan ke invoice | — | Tidak ada | **Tidak sah** | Tidak tampil di antrean |
| (migration) | Backfill baris lama Rawat Jalan | `ReconciliationRequired` (`LEGACY_PRE_BRIDGE`) | Migration | `RJ-E2E-DEC-014` | — |

**Contoh:** Tarif konsultasi Poli Gigi belum diisi. Baris konsultasi Ny. C langsung
`ReconciliationRequired` (`TARIFF_NOT_FOUND`), tanpa percobaan ulang. Admin tarif mengisi tarif;
petugas Billing menekan Kirim Ulang → `Pending` → `Synced`, dan item konsultasi muncul di invoice.

## V2-B. Penyerahan fakta klinis (`ClinicalFactDispatchStatus`, tidak berubah nilainya)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `Pending` / `OutcomeUnknown` | Pekerja atau petugas mengirim ulang, berhasil | `Dispatched` | Sistem (`ClinicalFactDispatchWorker`), petugas Billing | Identitas dan `IdempotencyKey` **sama** | — |
| `Pending` / `OutcomeUnknown` | Kirim ulang, hasil tetap tidak pasti | tetap; `DispatchAttemptCount` + 1; `NextDispatchAttemptAt` dijadwalkan | Sistem | Percobaan < batas | — |
| `Pending` / `OutcomeUnknown` | Batas percobaan tercapai atau kebijakan tidak aktif | tetap; `ReconciliationRequiredAt` diisi | Sistem | — | — |
| `OutcomeUnknown` | Pelayanan yang sama menerbitkan revisi baru | — | Tidak ada | **Tidak sah** sampai direkonsiliasi (perilaku producer yang sudah ada) | Emisi `ReconciliationRequired` (`CLIN_FACT_RECONCILIATION_REQUIRED`) |
| apa pun | Kirim ulang memakai `IdempotencyKey` baru | — | Tidak ada | **Tidak sah** (`AC-RJ-014`) | Tidak ada jalur kode yang membuat kunci baru untuk identitas lama |

## V2-C. Item `PHARMACY` pada invoice (`SourceStatus`)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| (baru) | Dokter Selesai Konsultasi | `PRESCRIBED` v1 | Sistem (jembatan) | Resep difinalkan; semua item punya tarif | `ReconciliationRequired` bila tarif hilang |
| `PRESCRIBED` | Farmasi menyerahkan obat, invoice `OPEN` | `DISPENSED` v2 | Sistem | Versi naik | — |
| `PRESCRIBED` | Farmasi menyerahkan obat, invoice sudah `FINAL`/`CLOSED` | tetap `PRESCRIBED`; adjustment selisih | Sistem | Selisih ≠ 0 | — |
| `PRESCRIBED` | Resep dibatalkan sebelum diproses farmasi, invoice `OPEN` | item `VOIDED` | Sistem | Pembatalan klinis sah | — |
| `DISPENSED` | Pembatalan atau koreksi | tetap; adjustment `CREDIT` | Sistem → persetujuan Billing | `RJ-E2E-DEC-010` | Void biasa ditolak adapter |
| `DISPENSED` | Kembali ke `PRESCRIBED` | — | Tidak ada | **Tidak sah** — versi tidak boleh mundur | `409` "Versi source lebih lama" |

## V2-D. Status kunjungan (titik sentuh Registration)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `InNurseScreening` | Selesai skrining, butuh dokter | `WaitingForDoctor` | Perawat | Tidak berubah | — |
| `InNurseScreening` | Selesai skrining, **tidak** butuh dokter | **`Billing`** | Perawat | `RJ-E2E-DEC-013` | — |
| `InConsultation` | Dokter Selesai Konsultasi | `ConsultationCompleted` | Dokter | Finalisasi canonical sukses | Tidak berubah |
| `ConsultationCompleted` / `Billing` | Aksi Rawat Jalan apa pun ke `Completed` | — | Tidak ada di Rawat Jalan | **Tidak sah** untuk Rawat Jalan (`RJ-E2E-DEC-007`) | Tidak ada tombol maupun panggilan dari workspace Rawat Jalan |
| `Billing` / `ConsultationCompleted` | Ke `Completed` | `Completed` | Belum diputuskan | `RJ-E2E-DEC-004` — `POST-MVP` | — |


---

# Amendment DP — Daftar Pasien Rawat Jalan (`RJ-DOC-ENCLIST-001@1.0.0`, `draft`)

`last_changed_in`: `RJ-DOC-ENCLIST-001@1.0.0` · Owner: Sukma Giri · Traceability: `RJ-DOC-DEC-016`, `019`, `021`, `022`

## DP-A. Pembatalan kunjungan dari Daftar Pasien Rawat Jalan

Pembatalan **tidak** mengubah `EncounterStatus`; ia mengisi penanda batal (`IsCancel`,
`CancelledAt`, …), sama seperti endpoint lama. Status yang tercatat tetap menunjukkan sampai
mana pasien sempat dilayani.

| Dari status | Tindakan | Ke keadaan | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `Draft` (0) .. `WaitingForDoctor` (5) | Batalkan | Batal (`IsCancel = true`), antrean ikut batal | Pemegang `OutpatientEncounter : Cancel` dalam cakupan | Alasan 1-250 karakter | `400` / `403` / `404` |
| `InConsultation` (6) | Batalkan | Batal | Sama | **Tidak ada konsultasi aktif** (`RJ-DOC-DEC-021`) | `400` `RJDP-VAL-005` |
| `InConsultation` (6) dengan konsultasi aktif | Batalkan | — | — | **Tidak sah** | `400` `RJDP-VAL-005` |
| `ConsultationCompleted` (7), `Billing` (8) | Batalkan | — | — | **Tidak sah** — pelayanan sudah selesai; penutupan milik Registration + Billing | `400` `RJDP-VAL-006` |
| `Completed` (9), `Cancelled` (10), `NoShow` (11) | Batalkan | — | — | **Tidak sah** | `400` `RJDP-VAL-006` |
| Sudah batal (`IsCancel`) | Batalkan lagi | — | — | **Tidak sah** | `400` `RJDP-VAL-004` |
| Batal | Dibuka kembali | — | Tidak ada | **Tidak sah** — tidak ada reopen; daftarkan kunjungan baru | — |

**Contoh:** ENC-RSMMC-00146 (status 3, tanpa konsultasi) → Batalkan dengan alasan "Pasien tidak
kembali sejak 30 Jul" → `IsCancel = true`, status tetap 3, antreannya `Cancelled`, pasien dapat
didaftarkan lagi.

## DP-B. Dampak terhadap pemblokir pendaftaran

| Keadaan kunjungan lama pasien | Memblokir pendaftaran poliklinik baru |
|---|---|
| RJ berklinik, status 0-6, belum batal, `CompletedAt` kosong | Ya |
| RJ berklinik, status 7-8 | **Tidak** (sebelumnya ya) |
| Penunjang tanpa klinik, IGD, Rawat Inap | **Tidak** (sebelumnya ya) |
| Batal, selesai, tidak hadir | Tidak (tidak berubah) |
