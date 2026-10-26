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

---

# Amendment RP — Reservasi Pasien Prioritas (`RJ-PRI-RESV-001@1.0.0`, `approved`)

Pola Bank Darah. Tanpa folder `Tests/` dan tanpa project test. Kolom "Jenis" menyatakan **sifat** skenario, bukan
perintah membuat test otomatis. Setiap skenario runtime dijalankan lewat HTTP sungguhan ke backend uji
`https://localhost:7185` terhadap `QuilvianNewDevSukma`, dengan data samaran, dan keadaan DB dicatat sesudahnya. Uji
layar memakai Playwright lewat dev `3000`, dengan route `7184 → 7185`.

Gerbang setiap task backend: build `-p:UseSharedCompilation=false`, `dotnet ef migrations has-pending-model-changes
--no-build`, QBE Strict. `dotnet test`: `NOT RUN — tidak ada project test`.

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
|---|---|---|---|---|
| `R-RP-01` | AC 1, `RJ-PRI-DEC-004` | Satu poli satu hari: daftarkan 3 Reguler dan 2 prioritas bergantian | Integ | Reguler 1, 2, 3; prioritas 5, 10. Tidak ada Reguler kelipatan 5 (kueri `RegQueue`) |
| `R-RP-02` | `GATE-RP-02` | Hari yang sudah memuat nomor lama 1, 3, 5 (aturan lama), lalu daftarkan prioritas | Integ | Prioritas mendapat 10. Tidak ada nomor ganda |
| `R-RP-03` | AC 2, `RJ-PRI-DEC-005` | Display cluster memanggil `/summary`, `/items`, `/called` saat ada antrean prioritas yang dipanggil perawat | Integ | Antrean prioritas tidak muncul dan tidak dihitung |
| `R-RP-04` | AC 22, `RJ-PRI-DEC-025` | Akun display bergabung `JoinQueueDisplayCluster`. Perawat memanggil satu Reguler dan satu prioritas | Integ (SignalR) | Display menerima tepat satu event (Reguler), tanpa `patientId`/`encounterId` |
| `R-RP-05` | `RJ-PRI-DEC-025` (gagal) | Akun display memanggil `JoinNurseStationCluster` | Integ (SignalR) | `HubException`, tidak bergabung |
| `R-RP-06` | AC 3, `RJ-PRI-DEC-006` | Pasien dengan membership Kedaluwarsa kemarin mendaftar | Integ | Nurse station dan antrean sama-sama Reguler (`AT-36` CRM) |
| `R-RP-07` | AC 4, `RJ-PRI-DEC-007` | Booking dibuat hari ini untuk +5 hari, lalu membership dibuat kedaluwarsa (data uji) sebelum hari H | Integ | Snapshot antrean booking tetap prioritas |
| `R-RP-08` | AC 5 (gagal) | Buat reservasi untuk pasien Reguler, lalu untuk pasien Menunggu Pembayaran | Integ | 409 `VAL-RP-02` untuk keduanya |
| `R-RP-09` | AC 6 (gagal) | Buat dua reservasi untuk pasien dan tanggal sama, termasuk dua permintaan paralel | Integ (konkurensi) | Satu berhasil, satu 409 `VAL-RP-05`. Hanya satu baris terbuka |
| `R-RP-10` | AC 7 | Reservasi Menunggu Kedatangan ada | Integ | Tidak muncul di `GET /outpatient-encounters`, antrean, nurse station, maupun billing |
| `R-RP-11` | AC 8 | Link sebelum hari H, saat hari H, lalu dibuka kedua kali | Integ | `NOT_YET_OPEN`; `CONFIRMED`; jam konfirmasi pertama tetap |
| `R-RP-12` | AC 8 (gagal) | Link sesudah dibatalkan, sesudah Tidak Datang, dan kode acak | Integ | Ketiganya 404 dengan isi sama |
| `R-RP-13` | AC 9 | Periksa respons `GET /{code}` | Integ | Tidak ada nama, No. RM, NIK, tingkat, poli, dokter, ID internal |
| `R-RP-14` | AC 10, 17 | Petugas skrining (hanya `Read` + `Screen`) terhubung ke board, pasien konfirmasi | Integ (SignalR) + E2E | Event `PriorityReservationChanged` diterima dan layar berbunyi |
| `R-RP-15` | AC 11 | Petugas tandai tiba, lalu kembalikan dengan alasan | Integ | Riwayat `ARRIVED` (`Staff`) dan `ARRIVAL_REVERTED`. Link aktif lagi |
| `R-RP-16` | AC 12 | Lengkapi dengan kuota dokter penuh, lalu dengan dokter lain | Integ | Pertama ditolak dan reservasi tetap Sudah Tiba. Kedua: kunjungan, antrean prioritas, dan reservasi `Converted` dalam satu transaksi |
| `R-RP-17` | AC 12 (gagal) | Lengkapi reservasi Menunggu Kedatangan; pasien berbeda; tanggal bukan hari ini; tanpa izin `Complete` | Integ | 409, 400, 400, 403. Tidak ada kunjungan terbentuk |
| `R-RP-18` | AC 13 | Reservasi tanggal kemarin, lalu hosted service dijalankan dua kali | Integ | Status Tidak Datang. Riwayat `NO_SHOW` hanya satu |
| `R-RP-19` | AC 14 (gagal) | Batal tanpa alasan; pengguna `Screen` saja mencoba buat, lengkapi, dan batal | Integ | 400; 403 untuk ketiganya |
| `R-RP-20` | AC 15, `RJ-PRI-DEC-024` | Batalkan membership snapshot (data uji) sebelum lengkapi | Integ | Daftar menampilkan `isPriorityRevoked = true`. Kunjungan mendapat nomor Reguler |
| `R-RP-21` | AC 16, 18 | Pasien prioritas lewat kiosk, dengan dan tanpa reservasi terbuka | Integ + E2E | Tiket mengarahkan ke ruang tunggu prioritas. Reservasi terbuka menjadi Didaftarkan lewat Jalur Lain |
| `R-RP-22` | AC 18 (batas) | Pasien dengan reservasi terbuka mendaftar IGD atau Laboratorium langsung | Integ | Reservasi tetap terbuka |
| `R-RP-23` | AC 19, 20 | Kirim ulang, lalu buka link lama dan link baru; periksa kolom DB | Integ | Link lama 404, link baru `READY`. Kolom hanya berisi hash 64 karakter |
| `R-RP-24` | AC 21 | Batal dengan alasan "ganti tanggal", lalu buat reservasi baru setelah membership Kedaluwarsa | Integ | Reservasi baru ditolak 409 `VAL-RP-02` |
| `R-RP-25` | `VAL-RP-09` | Kirim 25 permintaan `GET /{code}` per menit dari satu IP | Integ | Permintaan ke-21 dan seterusnya 429 |
| `R-RP-26` | `VAL-RP-14` | Dua petugas membatalkan reservasi yang sama hampir bersamaan | Integ (konkurensi) | Satu berhasil, satu 409 "baru saja diubah petugas lain" |
| `R-RP-27` | `VAL-RP-19` | Kiosk mengirim `priorityReservationId` | Integ | 400 |
| `R-RP-28` | Scope butir 2, `RJ-PRI-FE-004` | Cari pasien Gold di Pendaftaran Pasien | E2E | Label tingkat tampil. Pasien Kedaluwarsa tanpa label |
| `R-RP-29` | `RJ-PRI-DEC-018` | Worklist Nurse Station dan Klinis Dokter untuk antrean prioritas | E2E | Teks "Ruang Tunggu Prioritas" dari konfigurasi |
| `R-RP-30` | Regresi | Pendaftaran Reguler petugas dan kiosk tanpa reservasi, IGD, dan Laboratorium langsung | Integ | Perilaku sama dengan sebelum perubahan, kecuali nomor Reguler melewati kelipatan 5 |

# Amendment RP-GT — Ganti tanggal reservasi (`RJ-PRI-RESV-001@1.2.0`, `approved`)

Pola yang sama dengan *Amendment RP*: tanpa folder `Tests/`, runtime HTTP ke `https://localhost:7185` terhadap
`QuilvianNewDevSukma` dengan data samaran, dan keadaan DB dicatat sesudahnya. "Hari ini" dibuat lewat data uji dengan
tanggal relatif, misalnya `H+1` = besok, karena backend memakai jam sungguhan.

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
|---|---|---|---|---|
| `R-RP-31` | AC 23, `RJ-PRI-DEC-034` | Reservasi Menunggu Kedatangan untuk `H+1`, diganti ke `H+4` dengan alasan sah | Integ | 200. `visitDate = H+4`, nomor dan status sama, `rowVersion` baru, respons memuat `arrivalLink` dan teks WA versi ganti tanggal |
| `R-RP-32` | AC 24 (gagal) | Reservasi untuk hari ini, lalu coba ganti tanggal | Integ | 409 `VAL-RP-22`. Baris tidak berubah |
| `R-RP-33` | AC 25 (gagal) | Reservasi Dibatalkan, lalu coba ganti tanggal | Integ | 409 `VAL-RP-11` |
| `R-RP-34` | AC 26 (gagal) | Dari reservasi `H+1`: (a) tanggal kemarin, (b) `H+31`, (c) `H+1` (sama), (d) tanggal yang sudah punya reservasi terbuka pasien itu, (e) tanggal yang sudah punya kunjungan RJ aktif, (f) `newVisitDate` kosong | Integ | 400 `VAL-RP-03`, 400 `VAL-RP-04`, 400 `VAL-RP-23`, 409 `VAL-RP-05`, 409 `VAL-RP-06`, 400 `VAL-RP-24`. Untuk keenamnya, `VisitDate`, hash, snapshot, dan `RowVersion` tetap |
| `R-RP-35` | AC 27 (gagal), `RJ-PRI-DEC-035` | Membership pasien dibuat Kedaluwarsa (data uji), lalu ganti tanggal | Integ | 409 `VAL-RP-25` dengan tanggal lama di pesan. Link lama tetap `200 NOT_YET_OPEN` |
| `R-RP-36` | AC 28 | Snapshot reservasi diubah ke nilai lama (data uji, misalnya tingkat lain), lalu ganti tanggal | Integ | Ketujuh kolom snapshot sama dengan hasil klasifikasi hari ini |
| `R-RP-37` | AC 29, `RJ-PRI-DEC-036` | Sesudah `R-RP-31`, buka link lama dan link baru; periksa kolom DB | Integ | Link lama 404, link baru 200 `NOT_YET_OPEN` dengan tanggal baru. `ArrivalCodeHash` hanya hex 64 karakter dan berbeda dari sebelumnya |
| `R-RP-38` | AC 30 (gagal), `RJ-PRI-DEC-037` | Alasan 5 karakter; pengguna `Read` + `Screen` saja | Integ | 400 `VAL-RP-12`; 403 |
| `R-RP-39` | AC 31, `RJ-PRI-DEC-038` | Detail sesudah `R-RP-31`; periksa log aplikasi | Integ | Riwayat `RESCHEDULED` dengan `fromVisitDate = H+1`, `toVisitDate = H+4`, alasan, dan nama pelaku. Log tanpa kode, alasan, maupun tanggal |
| `R-RP-40` | AC 32 | Dua permintaan ganti tanggal paralel dengan `expectedRowVersion` sama | Integ (konkurensi) | Satu 200 dan satu 409 `VAL-RP-14`. Riwayat `RESCHEDULED` hanya satu |
| `R-RP-41` | AC 33 | Klien SignalR bergabung ke board, lalu ganti tanggal; layar daftar yang menyaring tanggal lama terbuka | Integ (SignalR) + E2E | Event `RESCHEDULED` dengan `visitDate` baru dan `previousVisitDate` lama. Baris hilang dari layar tanggal lama tanpa muat ulang manual |
| `R-RP-42` | Regresi | Buat, kirim ulang, tandai tiba, batal, penutupan akhir hari, dan alur hari H `R-RP-24` | Integ | Perilaku sama dengan sebelum revisi `34`. `canReschedule` bernilai `false` pada hari H dan pada status selain Menunggu Kedatangan |
