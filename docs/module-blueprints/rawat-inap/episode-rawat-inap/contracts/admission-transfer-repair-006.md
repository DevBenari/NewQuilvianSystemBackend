# Amandemen admisi transfer — EPS-006 versi 1.0

Disetujui untuk implementasi melalui instruksi pengguna 12 Oktober 2026: kerjakan seluruh `plan-repair-006-admisi-transfer-igd-rawat-jalan.md` sampai tuntas. Cakupan tulis: backend `MHamzah`, frontend `HamzahV2`, dan dokumentasi terkait. Tidak mencakup penerapan migration, publikasi Git, atau deployment.

## Keputusan yang berlaku

| Referensi | Hasil |
| --- | --- |
| EPS-006/K-01 | Dokter memvalidasi isian tanpa membuat kunjungan. Booking memanggil satu orchestrator transfer. Konfirmasi membaca episode hasil server; consent disimpan pada episode yang sama. Pendaftaran selesai setelah consent tersimpan. Reservasi belum berarti pasien tiba. |
| EPS-006/K-02 | Poli memakai `CliDoctorCertificate` bertipe `InpatientReferral`, berstatus `Issued`, milik kunjungan rawat jalan yang sah. Surat tidak diubah menjadi disposisi IGD. Satu sumber hanya boleh membentuk satu kunjungan hasil, termasuk bila hasil kemudian dibatalkan. |
| EPS-006/K-03 | Registrasi menyimpan sumber pembayaran dan hubungan kunjungan. Billing menerima deposit dengan idempotensi sendiri, di dalam transaksi transfer yang sama. Deposit nol tetap boleh sesuai kebijakan existing; nominal positif wajib memiliki metode penerimaan dan hak `BillingDeposit:Create`. |

`Executed` berarti permintaan telah dipakai untuk membuka admisi dan memesan bed. Penutupan fisik IGD tetap melalui departure/arrival/handover existing. Persetujuan `IGD-DEC-067` tetap tercatat; amandemen ini membedakan persiapan admisi dari pelaksanaan perpindahan fisik.

## Health Services / Inpatient Management / Inpatient Admission Transfer

| Method | Path relatif terhadap `/api/v1/health-services/inpatient-management` | Hak akses | Hasil |
| --- | --- | --- | --- |
| GET | `/admission-transfers` | `InpatientAdmissionTransfer:Read` | Pagination server, filter `sourceType`, `status`, `search`, `overdueOnly`; `totalData` adalah seluruh hasil filter. |
| GET | `/admission-transfers/{id}?sourceType=Emergency atau Outpatient` | `InpatientAdmissionTransfer:Read` | Detail sumber, status, kunjungan asal, dan episode hasil bila sudah diproses. |
| POST | `/admission-transfers/admit` | `InpatientEpisode:Create`; `PatientEncounter:Create`; isolasi juga `InpatientEpisode:Update`; deposit positif juga `BillingDeposit:Create` | HTTP 201, episode detail dengan `operationMeta`, reservasi, referensi asal dan receipt deposit. |

Request submit memakai `transferDispositionId` sebagai ID sumber pada kedua jenis, `sourceType`, `patientId`, `serviceUnitId`, `patientClassId`, `doctorId`, `bedId`, `expectedBedVersion`, `notes`, `paymentType` (Cash/Insurance/CompanyGuarantor sesuai enum Registrasi), `paymentMethodId`, `patientInsuranceId`, `patientCompanyGuarantorId`, `depositAmount`, `depositPaymentMethodId`, `requiresIsolation`, dan `isolationNote`. Header `Idempotency-Key` wajib, maksimum 100 karakter. `paymentScheme` legacy hanya diterima bila cocok dengan jenis pembayaran yang diterapkan; nilai tak didukung ditolak.

Contoh: deposit Rp500.000 dengan metode tunai menghasilkan satu movement/receipt Billing. Replay dengan key dan isi sama mengembalikan episode dan receipt yang sama. Key sama dengan isi berbeda menghasilkan 409. Sumber batal/sudah dipakai menghasilkan 409; salah pasien menghasilkan 422; sumber non-ranap/tidak ditemukan menghasilkan 404; jenis sumber tidak dikenal menghasilkan 400 tanpa kunjungan, episode, reservasi atau receipt baru. Versi bed lama menghasilkan 409. Hak/scope tidak sesuai menghasilkan 403.

Relasi Registrasi: `SourceEncounterId`, `AdmissionTransferId`, `AdmissionTransferSourceType`. Foreign key asal bersifat nullable untuk pasien datang langsung; index unik sumber menghalangi hasil ganda. Catatan klinis tidak dipindahkan. Migration disiapkan, penerapannya oleh pemilik.

## Keputusan komponen frontend

Referensi visual: alur admisi dan daftar transfer current workspace. Tidak merombak CSS/desain bersama.

| Kebutuhan | Kandidat dan bukti source | Status | Pemakaian |
| --- | --- | --- | --- |
| Tombol | `features/base-features/base-button.jsx` | REUSE | Aksi lanjut, retry dan navigasi halaman |
| Status/pesan | `status-badge.jsx`, `information-alert.jsx` | REUSE | Sumber IGD/Poli, loading/error/akses |
| Pencarian/filter | `data-filter.jsx`, `filter-select.jsx` | REUSE | Debounce dan filter server |
| Pilihan deposit | `filter-select.jsx` | REUSE | Metode penerimaan Billing |
| Pemilihan bed | `inpatient-admission-bed-step.jsx` dan hook bed existing | REUSE | Props tambahan khusus transfer; default jalur lain tetap |
| Langkah transfer | Rangkaian review, pembayaran, dokter, bed, konfirmasi, consent existing | COMPOSE | Satu titik penciptaan episode pada booking |

Pilihan komposisi:

1. **Rangkai langkah existing — Rekomendasi.** Konsistensi tampilan terjaga, risiko terbatas pada cabang transfer, biaya perubahan kecil.
2. Form dan stepper transfer terpisah. Membutuhkan duplikasi UI/state dan pemeliharaan lebih besar; tidak dipilih.

UI GATE: REUSE 5, COMPOSE 1, EXTEND default 0, WRAP 0, NEW 0.

## Preflight dan urutan

Backend: HealthServices / InPatientManagement (`Inp`), RegistrationManagement (`Reg`), BillingManagement (`Bil`), semuanya ACTIVE pada registry. TOUCHED LEGACY pada model/service existing; NEW CODE hanya helper orchestration/readers di owner yang sama. Aturan QBE-SVC-001, API-001, PERM-001, VAL-001, DTO-001, TXN-001, PAGE-001, LOG-001, CFG-002, CODE-004 berlaku. Tidak ada entity atau prefix operasional baru.

Task memakai ID FIX yang sudah disetujui pada rencana. Urutan: FIX-01 → FIX-07/FIX-02/FIX-03 → FIX-04/FIX-05/FIX-06 → FIX-08. Laporan mengikuti ID FIX persis di `task/report/backend` atau `task/report/frontend` sesuai pemilik perubahan. Bukti runtime hanya ditandai lulus jika benar-benar dijalankan.

## Detail hasil dan batas rilis

HTTP 201 memakai detail episode ber-ID `id`, ditambah `sourceEncounterId`, `admissionTransferId`, `admissionTransferSourceType`, `operationMeta`, `transferReservation` dan `transferDeposit`. Deposit berupa account Billing dan movements penerimaan yang dapat ditelusuri; tidak membuat ledger kedua. Metode tunai mengikuti keharusan shift kasir aktif milik Billing.

Antrean menghitung jumlah seluruh hasil pencarian dan filter asal; jumlah overdue memakai filter yang sama dengan tambahan overdueOnly. Ambang existing tetap lebih dari 60 menit. Frontend menunggu keputusan permission Read berstatus allowed sebelum membaca antrean/detail.

### Revisi PLAN-REPAIR-EPS-041 (27 Oktober 2026) — respons antrean dan kode galat `admit`

Diperluas tanpa mengubah nama maupun nilai kolom lama (`FIX-EPS-041-01`, `-04`, `-09`; laporan
[`FIX-EPS-041-BE`](../task/report/backend/FIX-EPS-041-BE.md)). Kolom berikut ditambahkan pada setiap item
`GET /admission-transfers` dan `GET /admission-transfers/{id}`:

| Kolom | Tipe | Sumber IGD (`EmgAdmissionRequest` berstatus `Signed`) | Sumber Poli (`CliDoctorCertificate`) |
| --- | --- | --- | --- |
| `needsIsolation`, `needsIntensiveCare`, `needsOxygenSupport`, `needsSpecialMonitoring` | boolean | Tanda kebutuhan pada permintaan | Selalu `false` |
| `estimatedLengthOfStayDays` | integer, boleh kosong | Perkiraan lama rawat | Kosong |
| `proposedInpatientDoctorId`, `proposedInpatientDoctorName` | GUID/teks, boleh kosong | DPJP rawat inap yang diusulkan IGD (ID hanya bila dokter aktif) | Kosong |
| `diagnosis` | teks, boleh kosong | Kosong | `ReferralDiagnosis`, atau `Diagnosis` bila kosong |
| `requestedRoomClass` | teks, boleh kosong | Kosong | Kelas kamar yang diminta, contoh "Kelas I" |
| `gender`, `nationalId`, `phoneNumber` | teks, boleh kosong | Kini terisi dari master pasien ("Laki-laki"/"Perempuan") | Sama |

Contoh: permintaan IGD suspek TB → `needsIsolation: true`, `needsOxygenSupport: false`,
`proposedInpatientDoctorName: "dr. Rendy Pangalila"`, `decidedByDoctorName: "dr. Joni Abraham Pratama"`. Surat pengantar
Poli DBD → `diagnosis: "Demam Berdarah Dengue"`, `requestedRoomClass: "Kelas I"`.

`POST /admission-transfers/admit`: kegagalan reservasi kini meneruskan kode aturan aslinya pada `errors.code`
(contoh `422 ISOLATION_REQUIRED` untuk pasien isolasi pada bed non-isolasi); `BED_NOT_READY` hanya dipakai bila hasil
reservasi tidak menyebut kode. Respons sukses ber-HTTP 201 kini juga ber-`statusCode: 201` di badan. Kontrak EPS-006/K-02
tetap: admisi transfer yang dibatalkan tidak mengembalikan permintaan ke antrean.

Migration `20261012041938_AddAdmissionTransferOrigin` disiapkan bersama Designer dan snapshot. Belum diterapkan; rilis dan uji integrasi PostgreSQL membutuhkan lingkungan uji berwenang yang sudah memakai migration ini. Pembuatan automated test backend NOT APPLICABLE menurut kebijakan repository; validasi backend memakai build, QBE dan review transaksi.
