# Acceptance Test Matrix — Rawat Jalan Billing

> **Revisi `27` (28 September 2026):** amendment **V2 — Rawat Jalan ke invoice canonical** ada di bagian akhir berkas ini dan **berlaku** untuk jalur fakta klinis → tagihan. Isi di atasnya adalah riwayat desain `RJ-BIL`.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| Exactly-once | Request milestone dikirim dua kali dengan key/fingerprint sama | Integration | Satu charge line dan replay canonical |
| Idempotency conflict | Key sama dikirim dengan amount/snapshot berbeda | Integration | HTTP 409 `BIL_IDEMPOTENCY_CONFLICT` |
| Stale version | Version 1 masuk setelah version 2 applied | Integration | HTTP 409 `BIL_VERSION_CONFLICT`; histori version 2 tetap |
| Outcome unknown | Commit terjadi lalu response timeout | Integration/recovery | Status query menemukan outcome; retry tidak menggandakan charge |
| Partial component | Satu component berhasil, component lain gagal | Domain/integration | Component applied tetap ada; failed component visible |
| Lab milestone | Requested/Collected/Received tidak membentuk final charge; Accepted membentuk eligibility | Domain | Transition dan charge eligibility sesuai rule |
| Radiology safety | Acquisition dimulai tanpa safety clearance | Domain/security | Request ditolak dan audit reason tersedia |
| Multi-payer | Net Rp1.000.000 dialokasikan A Rp600.000, B Rp250.000, patient Rp150.000 | Domain | Total tepat Rp1.000.000; tidak over-allocate |
| Payer replacement | Payer diganti setelah partial approval | Integration/domain | Allocation version baru; keputusan lama tetap terlihat |
| Financial correction | Void/reversal/refund diajukan tanpa approval | API/security | Ditolak; canonical charge tidak berubah |
| Maker-checker | Maker mencoba approve request sendiri | Authorization | Ditolak `BIL_SELF_APPROVAL` |
| Folio close | Mandatory reconciliation masih pending | Domain/API | Close ditolak `BIL_FOLIO_NOT_READY_TO_CLOSE` |
| Clinical boundary | Pharmacy mencoba menandai Paid | Authorization/contract | Tidak ada clinical endpoint authoritative untuk Paid |
| Urgent dispensing | Billing unavailable tetapi clinical exception disahkan | Workflow/integration | Dispensing tercatat; financial obligation tetap outstanding |
| External adapter | Adapter belum punya UAT/contract/credential | Release gate | Adapter tetap disabled; manual flow tetap berjalan |
| Privacy | Audit action terjadi pada source klinis sensitif | Security | Logger hanya menyimpan reference/hash, bukan raw payload |



---

# Amendment V2 — Rawat Jalan ke Invoice Canonical

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-E2E-CONTRACT-001@1.0.0` |
| Status | `approved` — Sukma Giri, 2026-09-28 |
| Sumber | `00-interview-decisions.md` revisi `18`; `contracts/*` bagian V2; PRD `AC-RJ-001`..`015` |

> **Cara membaca kolom "Jenis".** Kolom itu menyatakan **sifat skenario** — aturan murni (`Unit`),
> aturan bersama database (`Integ`), perebutan data (`Concurrency`), atau jalur pengguna utuh
> (`E2E/UAT`). Ia **bukan** perintah membuat project test. Repository backend tidak memuat project
> test dan folder `Tests/` **dilarang dibuat**. Bukti berbentuk build produksi, pemeriksaan model EF,
> QBE strict, dan validasi runtime lewat HTTP sungguhan terhadap `QuilvianNewDevSukma` dengan data
> samaran — pola Bank Darah (`02-backend-architecture.md` bagian `V2.13`).

## V2-1. Invoice dan idempotency

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
|---|---|---|---|
| `AC-RJ-001` | Tindakan dan Lab satu kunjungan diteruskan hampir bersamaan | Concurrency | Tepat satu `BilInvoice` untuk kunjungan itu |
| `AC-RJ-002`, `AC-RJ-003` | Baris folio yang sama dikirim jembatan dua kali (langsung + pekerja) | Integ | Satu `BilInvoiceItem`; percobaan kedua replay |
| `AC-RJ-003` | Dokter menekan Selesai Konsultasi dua kali | E2E | Satu item `CONSULTATION`, satu item `PHARMACY` per resep |
| `AC-RJ-004` | Nebulizer Tn. A dieksekusi | E2E | Item `PROCEDURE` di invoice kunjungan Tn. A, harga = `MstTariff` |
| `AC-RJ-005` | Lab dua pasien diteruskan bergantian dengan kirim ulang | Integ | Setiap item di invoice kunjungannya sendiri |
| `AC-RJ-006` | Study Radiologi dikirim ulang | Integ | `SourceDetailId` = id study, stabil antar versi |
| `RJ-E2E-DEC-006` | `from-source` dikirim untuk `PROCEDURE` kunjungan Rawat Jalan dengan `UnitPrice` Rp1 | Integ | `422 RJE-VAL-010`; tidak ada item baru |
| `RJ-E2E-DEC-006` | `from-source` dikirim untuk `ADHOC` | Integ | Tetap diterima seperti sebelumnya |
| `RJ-E2E-DEC-006` | Producer mengirim `TariffSnapshot.unitPrice` Rp99.999 | Integ | Harga item = `MstTariff.NormalPrice`, bukan snapshot |

## V2-2. Sumber tagihan

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
|---|---|---|---|
| `AC-RJ-008`, `RJ-E2E-DEC-012` | Konsultasi `Completed`, dokter punya rule dengan tarif | E2E | Satu item `CONSULTATION` dengan tarif rule |
| `RJ-E2E-DEC-012` | Tanpa rule, ada tarif klinik + kelas | Integ | Item memakai tarif klinik + kelas |
| `RJ-E2E-DEC-012` | Tidak ada tarif konsultasi sama sekali | Integ | Baris `ReconciliationRequired` `TARIFF_NOT_FOUND`; **tidak** ada item Rp0 |
| `RJ-E2E-DEC-001` | Konsultasi dibatalkan sebelum selesai | Integ | Tidak ada fakta dan tidak ada item konsultasi |
| `AC-RJ-007`, `RJ-E2E-DEC-005` | Resep difinalkan | E2E | Item `PHARMACY` `PRESCRIBED` v1, harga = Σ jumlah × tarif |
| `RJ-E2E-DEC-005` | Invoice berisi item `PRESCRIBED` dilunasi | E2E | Clearance resep `CLEARED`; farmasi dapat menelaah dan menyerahkan obat — **deadlock hilang** |
| `RJ-E2E-DEC-005` | Farmasi menyerahkan 8 dari 10 Vitamin C, invoice masih `OPEN` | Integ | Item `DISPENSED` v2 dengan harga baru |
| `RJ-E2E-DEC-005` | Sama, invoice sudah `FINAL` | Integ | Adjustment `CREDIT` selisih `SUBMITTED`; item tidak diubah |
| `RJ-E2E-DEC-002` | Invoice Rawat Jalan dihitung | Integ | Tidak ada item `REGISTRATION`; biaya admin hanya di `AdministrationFeeAmount` |
| `REPEAT_INTERNAL_ERROR` | Study Radiologi diulang karena kesalahan rumah sakit | Integ | Baris `NotApplicable`; tidak ada item tambahan |
| `NOT_OUTPATIENT` | Tindakan pada kunjungan rawat inap | Integ | Baris `NotApplicable`; invoice rawat inap tidak berubah |

## V2-3. Pembatalan dan koreksi

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
|---|---|---|---|
| `AC-RJ-011` | Lab dibatalkan sebelum pernah diterima | Integ | `SuppressedNoPriorCharge`; tidak ada void maupun adjustment |
| `AC-RJ-012` | Lab `ACCEPTED` dibatalkan, invoice `OPEN` | Integ | Item `VOIDED`; baris versi lama tetap terbaca |
| `RJ-E2E-DEC-010` | Tindakan `PERFORMED` dibatalkan | Integ | Adjustment `CREDIT` `SUBMITTED`; item tetap `ACTIVE` sampai adjustment disetujui |
| `RJ-E2E-DEC-010` | Resep `PRESCRIBED` dibatalkan sebelum diproses farmasi | Integ | Item `VOIDED` |
| `ADJUSTMENT_REJECTED` | Adjustment pada invoice `CLOSED` ditolak service | Integ | Baris `ReconciliationRequired`, kode `ADJUSTMENT_REJECTED` |

## V2-4. Keandalan dan rekonsiliasi

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
|---|---|---|---|
| `AC-RJ-013` | Billing gagal saat tindakan dieksekusi | Integ | Tindakan tetap `Completed`; fakta `Pending`/`OutcomeUnknown` |
| `AC-RJ-014`, `RJ-E2E-DEC-009` | Pekerja mengirim ulang fakta `OutcomeUnknown` | Integ | `IdempotencyKey` sama; tidak ada fakta baru |
| `RJ-E2E-DEC-009` | Sinkron invoice gagal 5 kali (kebijakan usulan) | Integ | Jadwal 60/120/240/480 detik; lalu `ReconciliationRequired` `RETRY_EXHAUSTED` |
| `SYNC_POLICY_INACTIVE` | Kebijakan `INVOICE_SYNC` dinonaktifkan | Integ | Kegagalan pertama langsung `ReconciliationRequired` |
| `RJ-E2E-DEC-014` | Migration pada database dengan baris folio lama | Integ | Baris Rawat Jalan lama `ReconciliationRequired` `LEGACY_PRE_BRIDGE`; nol item invoice baru |
| `RJE-VAL-020` | Petugas menekan Kirim Ulang pada item `Synced` | Integ | `422` |
| `RJE-VAL-022` | Penyelesaian manual tanpa catatan | Integ | `422` |
| `RJE-VAL-025` | Petugas dan pekerja memproses item yang sama bersamaan | Concurrency | Satu berhasil; lainnya `409`; tidak ada item ganda |
| `RJE-VAL-031` | Dua admin mengubah kebijakan bersamaan | Concurrency | Satu berhasil; lainnya `409` |

## V2-5. Ringkasan Billing, hak akses, dan status kunjungan

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
|---|---|---|---|
| `RJ-E2E-DEC-008` | Kunjungan tanpa invoice dibuka | E2E | `200`, `BillingStatus = NO_INVOICE`; layar menampilkan keadaan normal, bukan galat |
| `AC-RJ-009`, `AC-RJ-010` | Ringkasan tampil | E2E | Semua angka sama dengan `calculation-preview` invoice; frontend tidak menjumlah ulang |
| `SEC-RJ-005` | Respons ringkasan diperiksa | Integ | Tidak ada `UnitPrice`/`TotalPrice` per pelayanan |
| `AC-RJ-015` | Pengguna dengan `EncounterBillingSummary : Read` saja memanggil `GET /billing/invoices/{id}` | Integ | `403` |
| `SEC-RJ-001` | Tanpa sesi memanggil ringkasan | Integ | `401` |
| `RJ-E2E-FE-004` | Selesai Konsultasi saat Billing gagal | E2E | Konsultasi selesai; dokter melihat pemberitahuan masalah penyerahan |
| `RJ-E2E-DEC-013` | Skrining selesai, kunjungan tidak butuh dokter | Integ | Kunjungan `Billing`; `CompletedAt` kosong |
| `SEC-RJ-004` | Deskripsi pelayanan berisi `<script>` | E2E | Tampil sebagai teks biasa |


---

# Amendment DP — Daftar Pasien Rawat Jalan (`RJ-DOC-ENCLIST-001@1.0.0`, `draft`)

Verifikasi pola Bank Darah: uji runtime HTTP terhadap `QuilvianNewDevSukma`, data uji dibuat dan
dibersihkan lewat endpoint aplikasi. Asal AC: `00-interview-decisions.md` (AC 1-13 amendment).

| ID | Skenario | Langkah | Hasil yang diharapkan | AC |
|---|---|---|---|---|
| `AT-DP-01` | Cakupan dokter | dr. A `GET /?mode=active` | Hanya kunjungan `DoctorId` = A | 1 |
| `AT-DP-02` | Dokter memaksa dokter lain | dr. A `GET /?doctorId=<C>` | Daftar kosong | 1 |
| `AT-DP-03` | Cakupan perawat | Perawat B (cluster: Poli Dalam, Jantung) `GET /` | Semua kunjungan dua poli itu, tidak ada poli lain | 2 |
| `AT-DP-04` | Tanpa cakupan | Akun tanpa dokter/cluster/`ReadAll` | `403` `RJDP-VAL-001` | 3 |
| `AT-DP-05` | Lihat semua tanpa role Super Admin | Akun pendaftaran ber-`ReadAll` | Semua klinik | 4 |
| `AT-DP-06` | Bukan RJ | Ada kunjungan IGD, Rawat Inap, lab walk-in | Tidak muncul | 5 |
| `AT-DP-07` | Batal status 0-5 | `PATCH cancel` status 3 + alasan | `200`; `IsCancel`; antrean batal; `POST /patient-encounters/admin` pasien sama `200` | 6 |
| `AT-DP-08` | Batal status 6 tanpa konsultasi aktif | Konsultasi dibatalkan dokter, lalu `PATCH cancel` | `200` | 6 (`RJ-DOC-DEC-021`) |
| `AT-DP-09` | Batal status 6 dengan konsultasi aktif | `PATCH cancel` | `400` `RJDP-VAL-005` | 7 |
| `AT-DP-10` | Batal status 7 | `PATCH cancel` | `400` `RJDP-VAL-006` | 7 |
| `AT-DP-11` | Alasan kosong / 251 karakter | `PATCH cancel` | `400` `RJDP-VAL-002` | 8 |
| `AT-DP-12` | Tanpa `Cancel` | `PATCH cancel` | `403` | 9 |
| `AT-DP-13` | Di luar cakupan | dr. A batalkan pasien dr. C | `404` `RJDP-VAL-003` | 9 |
| `AT-DP-14` | Batal ganda bersamaan | Dua `PATCH cancel` paralel | Satu `200`, satu `400` `RJDP-VAL-004` | — |
| `AT-DP-15` | Status 7 tidak memblokir | Pasien punya kunjungan RJ status 7 → `POST /patient-encounters/admin` poli lain | `200` | 10 |
| `AT-DP-16` | Status 0-6 tetap memblokir | Pasien punya kunjungan RJ status 5 → daftar | `400` pesan `RJ-DOC-REV-BE-007` | 10 |
| `AT-DP-17` | Penunjang/IGD tidak memblokir | Pasien punya kunjungan lab walk-in aktif → daftar poli | `200` | 10 |
| `AT-DP-18` | Akses rekam medis tidak berubah | Bandingkan hasil endpoint akses rekam medis sebelum/sesudah untuk kunjungan status 7 | Sama | 11 |
| `AT-DP-19` | Endpoint lama | `PATCH /patient-encounters/{id}/cancel` status 7 | Tetap `200` seperti sebelumnya | 12 |
| `AT-DP-20` | Kunjungan pemblokir terlihat | Ambil nomor dari pesan penolakan `AT-DP-16`, cari di `GET /?mode=active&search=` dengan akun `ReadAll` | Ditemukan | 13 |
| `AT-DP-21` | Summary menggantung | `GET /summary?mode=today` | `hanging` tetap menghitung kunjungan lintas tanggal | `RJ-DOC-FE-007` |
| `AT-DP-22` | FE: tombol bersyarat | Login dokter tanpa `Cancel`; baris status 6 berkonsultasi | Tidak ada tombol Batalkan; keterangan `cancelBlockedReason` tampil | `RJ-DOC-FE-008` |
| `AT-DP-23` | FE: menu | Login pemegang `Read` | Butir "Daftar Pasien Rawat Jalan" tepat di bawah "Skrining Pasien"; tanpa `Read` tidak tampil | `RJ-DOC-FE-005` |
| `AT-DP-24` | FE: kasus pemicu | Kartu Menggantung → cari ENC-RSMMC-00146 → Batalkan → daftar ulang pasien | Pendaftaran berhasil | Pemicu |

---

# Amendment KT — Konsultasi Tertunda (`RJ-DOC-PENDCONS-001@1.0.0`, `draft`)

Pola Bank Darah: uji runtime HTTP terhadap `QuilvianNewDevSukma`; data uji dibuat dan dibersihkan
lewat endpoint aplikasi.

| ID | Skenario | Hasil yang diharapkan | Trace |
|---|---|---|---|
| `AT-KT-01` | Dokter A, antrean kemarin `InConsultation`, kunjungan 6, konsultasi `InProgress` | Muncul di `pending-consultations` | `RJ-DOC-DEC-029`, `030` |
| `AT-KT-02` | Antrean yang sama tetapi bertanggal hari ini | Tidak muncul | KT.3.1 |
| `AT-KT-03` | Antrean kemarin, status `WaitingForDoctor`; kunjungan status 7; konsultasi `Cancelled` | Tidak muncul (tiga kasus) | `RJ-DOC-DEC-030` |
| `AT-KT-04` | Dokter B memanggil, juga dengan `doctorId` = A | Antrean dokter A tidak muncul | KT.3.2 |
| `AT-KT-05` | Pengguna tanpa data dokter dan bukan super admin | `403` | `RJKT-VAL-001` |
| `AT-KT-06` | Konsultasi punya 1 resep draf dan 2 tindakan | `draftPrescriptionCount = 1`, `procedureCount = 2`; `queueId` mempersempit ke satu baris | KT.3.3 |
| `AT-KT-07` | `finish-consultation` pada antrean `AT-KT-01` | `200`; kunjungan 7; baris hilang; dengan `BlockActiveEncounter = true` pasien dapat didaftarkan | `RJ-DOC-DEC-031` |
| `AT-KT-08` | Batal konsultasi antrean tertunda lain, lalu batal kunjungan lewat Daftar Pasien Rawat Jalan | Baris hilang; batal kunjungan `200` | `RJ-DOC-DEC-031`, `RJ-DOC-DEC-021` |
| `AT-KT-09` | `GET /doctor-queues`, `/summary`, `/call-lock` hari ini sebelum dan sesudah perubahan | Hasil sama | Kompatibilitas |
| `AT-KT-10` | Batal kunjungan status 6 dengan konsultasi aktif | `400` dengan bunyi baru `RJDP-VAL-005` | KT.3.4 |
| `AT-KT-11` | Frontend: `UAT-KT-01`..`07` | Sesuai `04-prd-to-mvp.md` *Amendment KT* | `RJ-DOC-FE-010`..`012` |

# Amendment MT — Menu Konsultasi Tertunda (revisi `30`, `draft`)

Frontend-only. Uji layar Playwright terhadap FE dev dan backend dev dengan DB
`QuilvianNewDevSukma` (pola `runtime-ui-test-setup`). Data uji dibuat dan dibersihkan lewat
endpoint aplikasi. `dotnet test` tidak berlaku (tidak ada perubahan backend).

| ID | Skenario | Hasil yang diharapkan | Trace |
|---|---|---|---|
| `AT-MT-01` | Sidebar Dokter → Rawat Jalan | Butir Klinis Dokter dan Konsultasi Tertunda; masing-masing menyala sendiri saat aktif | `RJ-DOC-FE-014` |
| `AT-MT-02` | Klinis Dokter, antrean hari ini | Tanpa tab; perilaku Panggil/Mulai/Selesaikan hari ini sama dengan sebelumnya | `RJ-DOC-FE-014` |
| `AT-MT-03` | Pengingat dengan `n > 0` dan `n = 0` | Tampil dengan tautan / tidak tampil | `RJ-DOC-FE-015` |
| `AT-MT-04` | Daftar, pencarian, jumlah baris, pagination | Isi sama dengan `GET pending-consultations` pada parameter yang sama | `MT-FR-02` |
| `AT-MT-05` | Batalkan dari daftar | Alasan wajib; `PATCH …/cancel` terkirim sekali; baris hilang; pesan petugas | `RJ-DOC-DEC-047` |
| `AT-MT-06` | Tanpa `canCancelConsultation` | Aksi batal tidak tampil di daftar dan workspace | `RJ-DOC-DEC-047` |
| `AT-MT-07` | Simpan Konsultasi | Klinis Dokter membuka item, modal Simpan tertutup, banner tampil, parameter URL bersih | `RJ-DOC-DEC-046` |
| `AT-MT-08` | Selesaikan sukses dari item daftar | Kembali ke daftar dengan pesan; kunjungan status 7 | `RJ-DOC-DEC-048` |
| `AT-MT-09` | Batalkan dari workspace | Kembali ke daftar dengan pesan | `RJ-DOC-DEC-047`, `048` |
| `AT-MT-10` | Item sudah tidak tertunda / id tidak valid | Pesan tidak ditemukan dan tautan kembali | `MT-FR-05` |
| `AT-MT-11` | Finalisasi ditolak validasi | Tetap di Klinis Dokter dengan pesan validasi | `RJ-DOC-DEC-048` |
| `AT-MT-12` | Lint berkas tersentuh dan `next build` | Lulus | DoD |
