# Kiosk — PRD → MVP

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Kiosk — Revisi Module Kiosk & Cek Nomor Rekam Medis (`KSK-BP-001`) |
| Status | `approved` — Sukma Giri Pratama, 30 Sep 2026 |
| Repository target | `NewQuilvianSystemBackend` (branch `sukmagp`), `V2QuilvianSystemFrontendDev` |
| SHA baseline | BE `419b910f`, FE `4ec51b0b` |
| Kontrak | `KSK-CONTRACT-v1` (`draft`) |
| `input_revision` | `00` r2, `01` r1, `02-requirement-completeness-assessment` r1, `02-backend-architecture` r1, `03-frontend-architecture` r1, `contracts/*` v1 |
| Cakupan | Cek No. RM (KTP/HP), urutan 8 step Pasien Lama, Penjamin Utama per kunjungan, dropdown Jadwal Dokter, pembersihan sesi |

## 2. Ringkasan eksekutif

Pasien lama sering tidak tahu dirinya sudah terdaftar, lalu mendaftar ulang sebagai pasien baru dan menimbulkan rekam medis ganda. MVP ini memberi Kiosk satu pintu **Cek Nomor Rekam Medis**: pasien mengetik No. KTP atau No. HP, lalu Kiosk menjawab dengan pasti (terdaftar, belum terdaftar, atau perlu petugas) dan mengarahkan ke alur yang benar. Alur Pasien Lama disusun ulang agar data ditinjau sebelum memilih layanan. Pasien yang dijamin perusahaan akhirnya dapat mendaftar dari Kiosk.

## 3. Masalah produk

| Kondisi sekarang | Bukti |
| --- | --- |
| Tile Cek No. RM ada, tetapi hanya menampilkan "akan dilanjutkan pada tahap berikutnya" | `KSK-CAP-011` |
| Tidak ada pencarian persis KTP/HP; pencarian yang ada free-text dan nilainya masuk URL | `KSK-CAP-003`, `KSK-CAP-024` |
| No. HP tersimpan dalam berbagai format (`08…`, `+628…`, `+62` telepon rumah) | `KSK-FACT-006` |
| Tujuan Layanan ditanyakan **sebelum** pasien teridentifikasi | `KSK-CAP-020` |
| Memilih Perusahaan di Kiosk selalu berakhir error di Konfirmasi | `KSK-CAP-031/032` |
| "Jadikan Utama" mengubah data master pasien dari Kiosk | `KSK-CAP-033` |
| Dropdown Poliklinik/Spesialis tertutup card dokter | `KSK-CAP-040` |
| Tidak ada pembersihan otomatis bila pasien meninggalkan Kiosk | `KSK-CAP-025` |
| Data pasien tercetak di console browser Kiosk | `KSK-CAP-024` |

## 4. Visi produk

Pasien datang → Kiosk memeriksa No. KTP/HP ke backend → jawaban pasti (satu pasien / belum ada / perlu petugas) → pasien lama masuk alur dengan identitas sudah terisi → meninjau data → memilih tujuan layanan (sesi kiosk tercatat sekali) → memilih jenis kunjungan dan **satu** penanggung → memilih dokter → kunjungan terdaftar → antrean tercetak → seluruh data pasien hilang dari layar.

## 5. Batas MVP

**Titik mulai:**

1. Pasien menyentuh tile "Cek Nomor Rekam Medis" atau "Pendaftaran Pasien Lama" di Beranda Kiosk.
2. Perangkat Kiosk sudah login dengan akun perangkat.

**Titik akhir:**

1. Pasien lama memegang nomor antrean poliklinik, **atau**
2. Pasien lama diarahkan ke Laboratorium dengan sesi kiosk tercatat, **atau**
3. Pasien belum terdaftar tiba di halaman Pendaftaran Pasien Baru, **atau**
4. Pasien diarahkan ke petugas (cocok ganda / pasien tidak aktif), **atau**
5. Layar kembali ke Beranda dengan data dibersihkan.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab dalam MVP |
| --- | --- |
| Pasien (akun perangkat Kiosk) | Mencari, meninjau, memilih, mendaftar |
| Petugas pendaftaran | Menerima pasien yang diarahkan (cocok ganda, tidak aktif, penggantian penjamin); memakai layar petugas existing |
| Sukma Giri Pratama | Approver blueprint dan pengambil keputusan bisnis |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Lookup persis KTP/HP dengan empat hasil | `KSK-CAP-003`, `KSK-CAP-004` | Wajib; inti PRD dan penahan pasien ganda |
| Rate limit lookup | `KSK-CAP-007` | Wajib; tanpa ini lookup membuka enumerasi KTP/HP |
| Respons Kartu Pasien minimal | `KSK-CAP-008`, `KSK-CAP-009` | Wajib; SEC-KSK-004 |
| Menu dan layar Cek No. RM | `KSK-CAP-011` | Wajib |
| Handoff ke Pasien Lama tanpa URL | `KSK-CAP-023` | Wajib; AC-OLD-001 dan PRIV-3 |
| Urutan 8 step | `KSK-CAP-020` | Wajib; KSK-OLD-001 |
| Sesi kiosk sekali di Step 3 | `KSK-CAP-021`, `KSK-CAP-022` | Wajib; tanpa ini Laboratorium kehilangan target |
| Step 1 tanpa KTP/HP di URL dan console | `KSK-CAP-024` | Wajib; `KSK-DEC-016` |
| Penjamin Utama per kunjungan, termasuk Perusahaan | `KSK-CAP-030..034` | Wajib; memperbaiki defect existing |
| Inactivity timeout | `KSK-CAP-025` | Wajib; SEC-KSK-007 |
| Bulan pendek & nama utuh | `KSK-CAP-026`, `KSK-CAP-027` | Wajib; regresi PRD |
| Dropdown jadwal dokter | `KSK-CAP-040` | Wajib; KSK-SCH-001 |

## 8. Kemampuan yang ditunda

| Kemampuan | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- |
| Pengaturan timeout **per perangkat** dari backend | Butuh kolom baru di master perangkat Kiosk (modul Administrator); di luar scope | Satu nilai untuk seluruh Kiosk, dapat ditimpa env build (`KSK-GAP-009`) |
| Kolom HP ternormalisasi / index ekspresi | Mengubah master pasien (PRD §46) | Normalisasi di query; diukur lewat `NFR-002` |
| Penutupan `PATCH …/kiosk/{id}/primary` | Konsumen lain belum diaudit | Kiosk berhenti memanggilnya (`KSK-RISK-003`) |
| Tabel log pencarian | Tidak diminta; menambah jejak data sensitif | Log terstruktur tersamar |
| Membawa KTP/HP ke form Pasien Baru | Alur Pasien Baru di luar scope | Pasien mengetik ulang di Pasien Baru |
| Metode pencarian paspor/KITAS | Di luar KSK-RM-002 (`KSK-DEC-019`) | Jalur No. HP atau petugas |

## 9. Alur bisnis target

**`FLOW-KSK-MVP-001` — Pasien lama mendaftar poliklinik lewat Cek No. RM**

1. Pasien memilih Cek Nomor Rekam Medis.
2. Pasien memilih No. KTP, mengetik 16 digit, menekan Cek.
3. Backend menemukan tepat satu pasien Aktif dan Kiosk menampilkan Kartu Pasien.
4. Pasien menekan Lanjut Pendaftaran Pasien Lama.
5. Step 1 menampilkan pasien itu, dan pasien menekan Lanjut.
6. Step 2 Review Data, dengan tanggal lahir `12 Sep 1990`.
7. Step 3 Pasien memilih Poliklinik; sesi kiosk tercatat.
8. Step 4 Jenis Kunjungan.
9. Step 5 Pasien (punya asuransi dan perusahaan) memilih Perusahaan sebagai penjamin utama.
10. Step 6 Layanan & Dokter.
11. Step 7 Konfirmasi, lalu kunjungan terdaftar dengan Penjamin Perusahaan.
12. Step 8 Antrean tercetak; data dibersihkan.

Diagram: `flowcharts/00-alur-utama.md`.

## 10. Epic dan functional requirement

### EPIC KSK-01 — Layanan Lookup No. RM (backend)

Disposisi: `MISSING / NEW` (endpoint, service, DTO, enum, rate limiter). Slice `S1`, `S2`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-KSK-001` | `POST kiosk-patient-lookups` dengan `{searchType: 1, value: "3273010101900001"}` milik satu pasien Aktif menjawab `200`, `result = 1`, dan `patient` berisi tepat 7 field Kartu Pasien. |
| `FR-KSK-002` | Input HP `0812-3456-7890` cocok dengan data tersimpan `+6281234567890`; input `021 555 1234` cocok dengan `+62215551234`. |
| `FR-KSK-003` | Nomor yang cocok ke dua pasien menjawab `result = 3` dan `patient = null`. |
| `FR-KSK-004` | Pasien `IsDeceased = true` / `Inactive` / `Blacklisted` menjawab `result = 4` tanpa data. Pasien `Merged` diikuti ke tujuan (maks. 3 langkah). |
| `FR-KSK-005` | KTP 15 digit, HP `12345`, atau nilai berisi `<` menjawab `400`. |
| `FR-KSK-006` | Permintaan ke-11 dalam satu menit dari satu akun perangkat menjawab `429`, sedangkan akun perangkat lain tetap `200`. |
| `FR-KSK-007` | Log lookup hanya memuat 4 digit terakhir nilai. |
| `FR-KSK-008` | `searchType = 3/4` menemukan pasien dari nomor kartu asuransi/member aktif. |
| `FR-KSK-009` | Authorization verifier exit `0` tanpa perubahan `approved-compatibility-fallback.txt`. |

### EPIC KSK-02 — Layar Cek No. RM dan handoff (frontend)

Disposisi: `EXTEND` (tile) + `MISSING / NEW` (layar, hook, service, slice). Slice `S1`, `S3`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-KSK-010` | Tile Beranda membuka `FE-KSK-02`. |
| `FR-KSK-011` | Validasi KTP/HP di layar sesuai `KSK-VAL-001..006`, tanpa request saat tidak sah. |
| `FR-KSK-012` | Lima hasil tampil dengan teks persis `validation-matrix.md` §2. |
| `FR-KSK-013` | `429`/`5xx`/timeout 20 detik menampilkan `ERROR` + Coba Lagi dan **tidak pernah** tombol Pasien Baru. |
| `FR-KSK-014` | Tekan Cek dua kali cepat menghasilkan satu request. |
| `FR-KSK-015` | "Lanjut Pendaftaran Pasien Lama" membuka Step 1 dengan pasien terisi; URL tanpa query. |

### EPIC KSK-03 — Urutan 8 step Pasien Lama (frontend)

Disposisi: `EXTEND`. Slice `S4`, `S5`, `S9`. Syarat mulai: `KSK-OQ-004`, EPIC KSK-01 tersedia.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-KSK-020` | Bar langkah poliklinik: Identifikasi, Review Data, Tujuan Layanan, Jenis Kunjungan, Pembayaran, Layanan & Dokter, Konfirmasi, Cetak Antrean. |
| `FR-KSK-021` | Halaman dibuka langsung → step awal Identifikasi. |
| `FR-KSK-022` | Memindai kartu di Step 1 **tidak** membentuk sesi kiosk; memilih tujuan di Step 3 membentuk tepat satu sesi (poliklinik tanpa target; Laboratorium `targetService = 2`). |
| `FR-KSK-023` | Mengetik KTP/HP di Step 1 memakai lookup POST; nama/No. RM memakai pencarian existing. |
| `FR-KSK-024` | Tidak ada `console.log` data pasien di service Kiosk. |
| `FR-KSK-025` | Review & Konfirmasi menampilkan `12 Sep 1990`; nama 5 kata tampil utuh. |

### EPIC KSK-04 — Penjamin Utama per kunjungan

Disposisi: `EXTEND` (BE: satu argumen di `CreateEncounterForKiosk`; FE: step Pembayaran & payload). Slice `S7`. Syarat mulai BE: `KSK-OQ-005`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-KSK-030` | `POST patient-encounters/kiosk` dengan `paymentType = 3` + `patientCompanyGuarantorId` sah menjawab `200`. |
| `FR-KSK-031` | Relasi perusahaan kedaluwarsa menjawab `400`; tidak ada kunjungan tersimpan. |
| `FR-KSK-032` | Kondisi C menampilkan Pilih Penjamin Utama tanpa preselect; tidak bisa lanjut sebelum memilih. |
| `FR-KSK-033` | Memilih Perusahaan menghasilkan kunjungan `PaymentType = 3`; penanda utama di data pasien tidak berubah. |
| `FR-KSK-034` | Tidak ada tombol "Jadikan Utama" dan tidak ada request `PATCH …/primary` dari Kiosk. |

### EPIC KSK-05 — Pembersihan sesi Kiosk (frontend)

Disposisi: `MISSING / NEW` (hook) + `EXTEND` (reset existing). Slice `S6`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-KSK-040` | 105 detik tanpa sentuhan memunculkan peringatan; 120 detik membawa layar ke Beranda dengan state pasien kosong. |
| `FR-KSK-041` | Menekan Lanjutkan pada peringatan mengulang hitungan. |
| `FR-KSK-042` | Selama request berjalan, timeout tidak terpicu. |
| `FR-KSK-043` | Setelah kembali ke Beranda, tombol Back browser tidak menampilkan data pasien. |

### EPIC KSK-06 — Dropdown Jadwal Dokter (frontend)

Disposisi: `EXTEND` (CSS lokal). Slice `S8`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-KSK-050` | Menu Poliklinik dan Spesialis tampil di atas seluruh card, termasuk card yang di-hover, pada 1080×1920 dan 1920×1080. |
| `FR-KSK-051` | `filter-select.jsx` dan CSS-nya tidak berubah. |

## 11. Model status yang diusulkan

Tidak ada status database baru. Keadaan layar: `IDLE`, `CHECKING`, `FOUND`, `NOT_FOUND`, `MULTIPLE_MATCH`, `CONTACT_STAFF`, `ERROR` (Cek No. RM); `IDENTIFICATION`, `DATA_REVIEW`, `SERVICE_DESTINATION`, `VISIT_TYPE`, `PAYMENT`, `SERVICE_AND_DOCTOR`, `CONFIRMATION`, `QUEUE_PRINT`, `LAB_HANDOFF`, `SESSION_CLEARED` (Pasien Lama). Invariant utama: `KSK-INV-001..009`. Rincian: `contracts/state-transition-matrix.md`.

## 12. Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
| --- | --- | --- |
| `MstPatient` dan tabel pasien terkait (baca), `RegPatientEncounterGuarantor`, `scan-result`, `BasePatientCard`, `InstanceAxios`, policy `KioskRead` | `PatientEncounterController.CreateEncounterForKiosk`, `Program.cs`, alur & step Pasien Lama, tile Beranda, CSS jadwal dokter | `KioskPatientLookupController/Service/Dtos`, dua enum, rate limiter `KioskPatientLookup`, layar `FE-KSK-02`, slice handoff, hook inactivity |

Tidak ada migration. Rincian: `02-backend-architecture.md`, `03-frontend-architecture.md`.

## 13. Sasaran kemampuan API

### Health Services / Registration Management / Kiosk Patient Lookup

Base URL: `api/v1/health-services/registration-management/kiosk-patient-lookups`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Lookup pasien KTP/HP/kartu | Policy `KioskRead`; rate limit `KioskPatientLookup` | `KioskPatientLookupRequest` | `ApiResponse<KioskPatientLookupResponse>` | `EPIC KSK-01` | **Rencana (belum tersedia)** |

### Health Services / Registration Management / Patient Encounter

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/kiosk` | Create encounter, kini menerima `paymentType = 3` | Policy `KioskRead` | `PatientEncounterCreateRequest` | `ApiResponse<PatientEncounterCreateResponse>` | `EPIC KSK-04` | **Sudah ada — perilaku baru belum tersedia** |

## 14. Matriks kewenangan

| Pelaku | Lookup | Create encounter kiosk | Keterangan |
| --- | :---: | :---: | --- |
| Akun perangkat Kiosk | Ya | Ya | Policy `KioskRead`; **tanpa** `[AccessPermission]` (`KSK-DSN-006`) |
| `SuperAdmin` / `Administrator` | Ya | Ya | Diterima policy `KioskRead` (existing) |
| Petugas | — | Lewat `/admin` (`PatientEncounter : Create`) | Tidak berubah |

## 15. Batas integrasi dan billing

Kiosk **MUST NOT**:

- mengubah master pasien, asuransi, atau perusahaan (termasuk penanda utama);
- membuat tarif, tagihan, atau aturan billing baru; penanggung kunjungan dibaca billing-kasir seperti kunjungan dari route admin;
- mengubah kontrak `scan-result`;
- memanggil sistem luar (Dukcapil, BPJS, OTP).

Rincian: `contracts/integration-contract.md`.

## 16. Guardrail regulasi

| Kewajiban | Penerapan |
| --- | --- |
| Kerahasiaan data pasien | Respons lookup hanya field Kartu Pasien; status meninggal/diblokir tidak pernah tampil di lobby; KTP/HP tidak di URL, storage browser, console, atau log utuh |
| Identifikasi pasien yang benar | Pencocokan persis, tepat-satu, Review wajib sebelum layanan |
| Integritas rekam medis | Gagal teknis dan cocok ganda tidak pernah mengarah ke pendaftaran pasien baru |

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-001` | Lookup tanpa transaksi tulis; create encounter atomik (existing) |
| `NFR-002` | Lookup p95 < 500 ms di data produksi; bila terlampaui, index ekspresi HP diajukan sebagai perubahan PatientManagement |
| `NFR-003` | Satu request lookup aktif per layar; request lama dibatalkan |
| `NFR-004` | Timeout layar lookup 20 detik |
| `NFR-005` | Inactivity 120 detik + peringatan 15 detik, ditunda selama request berjalan |
| `NFR-006` | Rate limit 10/menit/akun perangkat, dapat diatur lewat `KioskPatientLookup:PermitPerMinute` |
| `NFR-007` | Seluruh query berparameter |

## 18. Skenario UAT

| Epic | Berhasil | Gagal |
| --- | --- | --- |
| KSK-01 | **UAT-01** KTP pasien Aktif menghasilkan kartu dengan 7 field | **UAT-02** Kiosk mengirim 11 lookup dalam semenit; yang ke-11 ditolak "Terlalu banyak percobaan", bukan "belum terdaftar" |
| KSK-01 | **UAT-03** HP `0812-3456-7890` menemukan pasien yang tersimpan `+6281234567890` | **UAT-04** HP dipakai ibu dan anak menghasilkan "Data Perlu Diverifikasi" tanpa kartu |
| KSK-02 | **UAT-05** Kartu tampil → Lanjut → Step 1 terisi, URL bersih | **UAT-06** Server dimatikan saat cek menghasilkan "Data Belum Dapat Diperiksa" + Coba Lagi, tanpa tombol Pasien Baru |
| KSK-02 | **UAT-07** KTP tidak terdaftar menghasilkan "Pasien Belum Terdaftar" → Pasien Baru | **UAT-08** KTP 15 digit menghasilkan pesan validasi tanpa request |
| KSK-03 | **UAT-09** Jalur poliklinik 8 step dari awal sampai tiket | **UAT-10** Coba lanjut dari Tujuan Layanan tanpa memilih → tetap di step itu |
| KSK-03 | **UAT-11** Pindai kartu → Laboratorium → tepat satu sesi bertarget | **UAT-12** Penyimpanan tujuan layanan gagal → tetap di Step 3 dengan pesan |
| KSK-04 | **UAT-13** Asuransi + Perusahaan → pilih Perusahaan → kunjungan `PaymentType = 3` | **UAT-14** Relasi perusahaan kedaluwarsa → Konfirmasi ditolak, tidak ada kunjungan |
| KSK-05 | **UAT-15** Tekan Lanjutkan pada peringatan → sesi berlanjut | **UAT-16** Diam 120 detik di Review → Beranda, Back tidak menampilkan data |
| KSK-06 | **UAT-17** Dropdown Poliklinik terlihat penuh di atas card | **UAT-18** Dropdown dibuka saat card di-hover → tetap di atas (regresi yang dulu terjadi) |

Rincian langkah dan bukti: `testing/acceptance-test-matrix.md`.

## 19. Definition of Done

| Butir | Bukti |
| --- | --- |
| Menu Cek No. RM membuka layar yang berfungsi | `FR-KSK-010`, UAT-05 |
| Lookup KTP dan HP berjalan dengan empat hasil | UAT-01, 03, 04, 07 |
| Gagal teknis tidak pernah dianggap pasien baru | UAT-02, UAT-06 |
| Kirim ganda dicegah | `FR-KSK-014` |
| Rate limit aktif hanya pada lookup | UAT-02, `FR-KSK-006` |
| Respons lookup tanpa KTP/HP/alamat | `FR-KSK-001`, `acceptance-test-matrix.md` §1 SEC-KSK-004 |
| Log tanpa KTP/HP utuh | `FR-KSK-007` |
| Flow Pasien Lama tepat 8 step, urutan PRD | UAT-09, `FR-KSK-020` |
| Tujuan Layanan tidak dapat tampil sebelum Review | `FR-KSK-021`, UAT-10 |
| Satu sesi kiosk per perjalanan | UAT-11, `KSK-AC-010` |
| Penjamin Perusahaan dapat dipakai dari Kiosk | UAT-13 |
| Satu penanggung per kunjungan; default pasien tidak berubah | `FR-KSK-033` |
| Dropdown Poliklinik & Spesialis tidak tertutup card | UAT-17, UAT-18 |
| Nama > 3 kata tidak terpotong; bulan pendek | `FR-KSK-025` |
| Session cleanup berjalan | UAT-16 |
| Backend: build, `has-pending-model-changes` = tidak ada, QBE Strict, authorization verifier lulus | Laporan task BE |
| Frontend: lint + build lulus | Laporan task FE |
| Amendment `laboratorium` dan `rawat-inap` tercatat | `KSK-OQ-004`, `KSK-OQ-005` ditutup |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Epic | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | EPIC KSK-01 (lookup + rate limit, BE); EPIC KSK-06 (dropdown, FE, mandiri) | Blueprint disetujui |
| `MVP-1` | EPIC KSK-02 (layar Cek No. RM + handoff); EPIC KSK-05 (inactivity) | EPIC KSK-01 tersedia di dev |
| `MVP-2` | EPIC KSK-03 (urutan 8 step, sesi Step 3, privasi Step 1, format tanggal) | EPIC KSK-01 tersedia; `KSK-OQ-004` ditutup |
| `MVP-3` | EPIC KSK-04 (Penjamin Utama: BE lalu FE) | `KSK-OQ-005` ditutup; EPIC KSK-03 selesai (step Pembayaran sama) |
| `POST-MVP` | Seluruh butir §8 | Di luar rilis pertama |

Tidak ada epic berstatus `OPEN DECISION`.

| Pertanyaan / prasyarat | Siapa | Dampak bila belum | Memblokir |
| --- | --- | --- | :---: |
| Persetujuan blueprint dan `KSK-CONTRACT-v1` (termasuk `KSK-DSN-001..009` dan butir `PROPOSED` `KSK-GAP-001..011`) | Sukma | Tidak ada task yang boleh dikerjakan | **Ya** — seluruh gelombang |
| `KSK-OQ-004` — catat amendment `FE-LAB-13`/`AC-93` di blueprint Laboratorium | Sukma | `MVP-2` tidak dapat dimulai | Ya — hanya `MVP-2` (keputusan sudah ada; tinggal pencatatan) |
| `KSK-OQ-005` — catat `RWI-ENC-PAYER-001` v1.1.0 di blueprint rawat-inap | Sukma / Muhammad Hamzah | `MVP-3` BE tidak dapat dimulai | Ya — hanya `MVP-3` (keputusan sudah ada; tinggal pencatatan) |
| Penunjukan security/privacy owner | Sukma | Tidak ada; dicatat sebagai kekosongan kepemilikan | Tidak |
