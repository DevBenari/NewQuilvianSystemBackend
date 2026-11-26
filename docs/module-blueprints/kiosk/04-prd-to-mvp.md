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


---

# Amendment PO — PRD → MVP Pendaftaran Pasien Online dan Check-in Kiosk

## PO-1 Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian V2 — layanan mandiri pasien (perangkat kiosk dan halaman publik, `KSK-DEC-026`) |
| Modul | Kiosk (`KSK-BP-001`), Amendment PO |
| Status | `approved` — Sukma Giri Pratama, 19 Nov 2026 (`KSK-DEC-054`) — revisi blueprint `2` |
| Repository target | `NewQuilvianSystemBackend` (`sukmagp`), `QuilvianSystemFrontendDev` (`sukmagpV2`) |
| Baseline | BE `5e777833`, FE `5f18e9f04` |
| Ringkasan cakupan | Pasien mendaftar Rawat Jalan sendiri tanpa login untuk H+1 s.d. H+30, menerima kode, lalu check-in di kiosk pada hari kunjungan. Petugas dapat melihat, mengubah jadwal/kontak, dan membatalkan |

## PO-2 Ringkasan eksekutif

Antrean di loket pendaftaran pagi hari berkurang karena pasien yang sudah merencanakan kunjungan mendaftar dari rumah. Kuota dokter mereka aman sejak mendaftar, dan saat datang mereka cukup memasukkan kode di kiosk untuk mendapat nomor antrean. Rumah sakit tetap memegang kendali: data pasien baru dari internet baru masuk rekam medis setelah KTP/KIA dipindai, dan petugas dapat membersihkan pendaftaran yang tidak sah.

## PO-3 Masalah produk

| Kondisi sekarang | Bukti |
| --- | --- |
| Tidak ada wadah pendaftaran online/appointment Rawat Jalan | `F-PO-1`, `CAP-PO-01` |
| Kartu "Check-in Pasien Online" di beranda kiosk hanya menampilkan pemberitahuan | `F-PO-7`, `CAP-PO-24` |
| Semua endpoint pilihan (poli, jadwal, kuota, kartu) memerlukan login | `CAP-PO-09` |
| Backend belum dapat mengirim email maupun memeriksa captcha | `F-PO-8`, `CAP-PO-04`, `CAP-PO-05` |
| Kuota dokter hanya dihitung dari kunjungan dan belum dikunci per jadwal | `CAP-PO-10`, `CAP-PO-34` |
| Yang sudah ada dan dipakai ulang: jalur pembuatan kunjungan kiosk, pindai KTP/KIA, cetak tiket, pola endpoint publik dan penutupan Reservasi Prioritas, komponen langkah menu RJ | `CAP-PO-02`, `13`, `14`, `15`, `17`, `22`, `25` |

## PO-4 Visi produk

1. Pasien mengisi halaman online dengan langkah yang sama seperti petugas di menu RJ.
2. Pendaftaran tersimpan sebagai Menunggu Check-in dan langsung memakai satu kuota appointment.
3. Kode sampai ke pasien lewat layar, email, atau WhatsApp.
4. Pada hari kunjungan, kiosk memeriksa kode dan tanggal lahir, memverifikasi KTP/KIA pasien baru, lalu menerbitkan kunjungan, nomor antrean, dan tiket lewat jalur kunjungan yang sama dengan petugas.
5. Pendaftaran yang tidak dipakai, didaftarkan lewat jalur lain, atau dibatalkan petugas menutup sendiri dan melepas kuotanya.

## PO-5 Batas MVP

**Titik mulai:**

1. Pasien membuka `/online-registration` dari tautan rumah sakit.
2. Petugas membuka menu Pendaftaran Online.

**Titik akhir:**

1. Kunjungan Rawat Jalan terbit dengan `RegistrationSource = OnlineRegistration` dan tiket tercetak di kiosk.
2. Atau pendaftaran berakhir sebagai Didaftarkan lewat Jalur Lain, Tidak Datang, atau Dibatalkan, dengan kuota dilepas dan riwayat tercatat.

## PO-6 Pelaku sasaran

| Pelaku | Tanggung jawab dalam MVP |
| --- | --- |
| Pasien atau wali (tanpa login) | Mendaftar, menyimpan kode, check-in di kiosk |
| Akun perangkat kiosk | Menjalankan verifikasi dan check-in |
| Petugas admisi RJ | Mencari, mengubah jadwal/kontak, membatalkan |
| Admin Akses Role | Memberi `OnlineRegistration : Read/Update/Cancel` |
| IT/infrastruktur | Domain publik, IP asli, akun SMTP, kunci Turnstile |
| Sistem | Kuota, email, penutupan otomatis |

## PO-7 Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Wadah pendaftaran online, status, riwayat | `CAP-PO-01`, `12` | Wajib; inti amendment |
| Endpoint publik, batas laju, captcha | `CAP-PO-02`, `03`, `04`, `26` | Wajib; halaman publik tidak boleh dibuka tanpa perlindungan (`KSK-DEC-038`) |
| Opsi publik poli, jadwal, kuota, penyedia asuransi, faskes | `CAP-PO-09` | Wajib; tanpa ini halaman tidak dapat diisi |
| Pencocokan pasien lama tersamar | `CAP-PO-11` | Wajib (`KSK-DEC-031`) |
| Kode, email, tombol WhatsApp | `CAP-PO-05`, `06`, `07`, `08` | Wajib (`KSK-DEC-041` s.d. `043`) |
| Kuota memakai pendaftaran online dan dikunci per jadwal | `CAP-PO-10`, `34` | Wajib; tanpa ini kuota ganda (`INV-PO-02`) |
| Consent, rujukan, kartu baru ditampung lalu dipindah saat check-in | `CAP-PO-18`, `19`, `20` | Wajib (`KSK-DEC-034`, `035`) |
| Check-in kiosk, Lupa Kode, pindai KTP/KIA, tiket | `CAP-PO-13`, `14`, `15`, `24`, `25`, `28`, `36`, `37` | Wajib (`KSK-DEC-033`, `036`, `040`, `048`, `053`) |
| Penutupan Tidak Datang dan Jalur Lain | `CAP-PO-16`, `17` | Wajib; tanpa ini kuota tertahan (`KSK-DEC-037`, `045`) |
| Layar petugas: daftar, ubah, batal | `CAP-PO-30` s.d. `33`, `35` | Wajib (`KSK-DEC-050` s.d. `052`) |
| Route publik dan CSP | `CAP-PO-21`, `29` | Wajib; tanpa ini halaman dialihkan ke login atau widget kosong |
| Langkah menu RJ dipakai ulang | `CAP-PO-22`, `23` | Wajib (`KSK-DEC-029`) |

## PO-8 Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| WA otomatis lewat gateway | — (`KSK-DEC-042`) | Belum ada `platform-notification` dan gateway | Tombol WhatsApp yang dikirim pasien sendiri |
| Pembatalan oleh pasien di halaman publik | — | Tidak diminta (`KSK-DEC-050`) | Pasien menelepon; petugas membatalkan |
| Pemberitahuan otomatis sesudah petugas mengubah/membatalkan | — (`GATE-PO-12`) | Tidak diminta | Petugas menyampaikan langsung |
| Kebijakan penghapusan data pendaftaran tidak terpakai | — (`GATE-PO-03`) | Menunggu aturan retensi RS | Data disimpan seperti catatan pendaftaran lain |
| Cek kepesertaan BPJS online | — (`KSK-DEC-027`) | Di luar scope | Diperiksa petugas/kasir saat pasien datang |
| Laporan rekap pendaftaran online | — | Tidak diminta | Ringkasan per status di layar petugas |

## PO-9 Alur bisnis target

`FLOW-KSK-PO-MVP-001`:

1. Pasien membuka halaman online, memilih pasien lama atau baru.
2. Pasien lama dicocokkan dengan NIK/No. RM + tanggal lahir; pasien baru mengisi NIK, nama, tanggal lahir, jenis kelamin.
3. Pasien mengisi kontak, jenis pasien, jadwal H+1 s.d. H+30, rujukan (bila Rujukan), penjamin, dan consent (pasien baru).
4. Pasien menyelesaikan captcha dan mengirim. Sistem menyimpan Menunggu Check-in, memakai kuota, menampilkan kode, dan mengirim email.
5. Pada hari kunjungan, pasien memasukkan kode + tanggal lahir di kiosk, atau memakai Lupa Kode.
6. Pasien baru memindai KTP/KIA; NIK harus sama; master dibuat dari hasil pindai atau No. RM lama dipakai.
7. Kiosk membuat kunjungan appointment; sistem mengubah status menjadi Menjadi Kunjungan, menulis consent dan dokumen rujukan, lalu tiket tercetak.
8. Pendaftaran yang tidak dipakai menjadi Tidak Datang pukul 00.05 hari berikutnya; yang didaftarkan lewat jalur lain menjadi Didaftarkan lewat Jalur Lain; petugas dapat mengubah atau membatalkan selama Menunggu Check-in.

## PO-10 Epic dan functional requirement

### EPIC KSK-PO-01 — Fondasi data, kuota, dan penutupan (backend)

| FR | Requirement | Disposisi |
| --- | --- | --- |
| `FR-KSK-PO-001` | Tiga tabel `RegOnlineRegistration*`, lima enum baru, dua nilai enum tambahan, migration `AddRegOnlineRegistration` | `MISSING / NEW` |
| `FR-KSK-PO-002` | `GetUsageAsync` menghitung pendaftaran Menunggu Check-in; kunci kuota per jadwal dan tanggal di jalur online dan `CreateEncounterCoreAsync` | `EXTEND` |
| `FR-KSK-PO-003` | `CloseForEncounterAsync` di jalur kunjungan (pasien lama lewat `PatientId`, pasien baru lewat NIK) | `EXTEND` |
| `FR-KSK-PO-004` | Penutupan harian Tidak Datang, idempoten, mengejar hari tertinggal | `MISSING / NEW` (pola `CAP-PO-17`) |

### EPIC KSK-PO-02 — Halaman publik backend

| FR | Requirement | Disposisi |
| --- | --- | --- |
| `FR-KSK-PO-010` | Empat endpoint opsi publik dengan field minimum dan batas laju | `EXTEND` |
| `FR-KSK-PO-011` | Pencocokan pasien lama tersamar dengan captcha | `EXTEND` (logika `KioskPatientLookupService`) |
| `FR-KSK-PO-012` | Kirim pendaftaran multipart: validasi, kode, nomor, berkas, consent, riwayat | `MISSING / NEW` |
| `FR-KSK-PO-013` | `TurnstileVerificationService` (fail-closed) dan `SmtpEmailSender` | `MISSING / NEW` |
| `FR-KSK-PO-014` | `PatientNameMasker` bersama | `EXTEND` (dipindah dari `QueueDisplayRuntimeController`) |
| `FR-KSK-PO-015` | **`v2.1` (`KSK-DEC-057`, `058`)** — opsi faskes publik hanya faskes mitra pada `visitDate` (wajib); kirim menolak faskes non-mitra `KSK-VAL-PO-073` | `EXTEND` (`ReferralPartnerEligibility`) |

### EPIC KSK-PO-03 — Check-in backend

| FR | Requirement | Disposisi |
| --- | --- | --- |
| `FR-KSK-PO-020` | `POST online-registrations/kiosk/check-in/verify` dengan token 15 menit | `MISSING / NEW` |
| `FR-KSK-PO-021` | `PrepareCheckInAsync` dan `CompleteCheckInInTransactionAsync` di `CreateEncounterCoreAsync` (consent, dokumen rujukan, `RegistrationSource = 7`) | `EXTEND` |

### EPIC KSK-PO-04 — Layar petugas backend

| FR | Requirement | Disposisi |
| --- | --- | --- |
| `FR-KSK-PO-030` | Daftar, ringkasan, detail, buka berkas | `MISSING / NEW` (pola `CAP-PO-31`) |
| `FR-KSK-PO-031` | Ubah jadwal (poli sama, pertukaran kuota), ubah kontak | `MISSING / NEW` (`CAP-PO-32`) |
| `FR-KSK-PO-032` | Batal dengan alasan; Resource `OnlineRegistration` | `MISSING / NEW` (`CAP-PO-30`, `33`) |

### EPIC KSK-PO-05 — Halaman publik frontend

| FR | Requirement | Disposisi |
| --- | --- | --- |
| `FR-KSK-PO-040` | Route publik di tiga daftar dan CSP Turnstile | `EXTEND` |
| `FR-KSK-PO-041` | Langkah Data Pasien publik dan Selesai publik (kode, email, WhatsApp) | `MISSING / NEW` |
| `FR-KSK-PO-042` | Langkah RJ dipakai ulang lewat hook dan service publik | `EXTEND` (`CAP-PO-22`) |
| `FR-KSK-PO-043` | **`v2.1` (`KSK-DEC-057`, `058`)** — Data Rujukan mencari faskes dengan `visitDate`, pesan kosong khusus faskes mitra, pilihan faskes dikosongkan bila tanggal kunjungan berubah, `KSK-VAL-PO-073` ditampilkan | `EXTEND` |

### EPIC KSK-PO-06 — Check-in kiosk frontend

| FR | Requirement | Disposisi |
| --- | --- | --- |
| `FR-KSK-PO-050` | Layar Check-in: kode/Lupa Kode, ringkasan, check-in, tiket; kartu beranda diberi route | `MISSING / NEW` |
| `FR-KSK-PO-051` | Varian langkah pindai: NIK terkunci, hanya KTP/KIA | `EXTEND` (`CAP-PO-37`) |

### EPIC KSK-PO-07 — Layar petugas frontend

| FR | Requirement | Disposisi |
| --- | --- | --- |
| `FR-KSK-PO-060` | Daftar dengan ringkasan, filter, pencarian; butir menu | `MISSING / NEW` (pola `CAP-PO-35`) |
| `FR-KSK-PO-061` | Detail dengan riwayat, Ubah Jadwal, Ubah Kontak, Batalkan | `MISSING / NEW` |

## PO-11 Model status yang diusulkan

Menunggu Check-in → Menjadi Kunjungan / Didaftarkan lewat Jalur Lain / Tidak Datang / Dibatalkan. Invariant utama `INV-PO-01` s.d. `08` (`02-backend-architecture.md` PO.2). Rinciannya `contracts/state-transition-matrix.md` *Amendment PO*.

## PO-12 Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
| --- | --- | --- |
| Jalur `POST patient-encounters/kiosk`, endpoint kiosk pasien/kartu, pindai KTP/KIA, cetak tiket, `NumberSeriesAllocator`, helper hash, pola endpoint publik RP, pola penutupan RP, komponen langkah RJ, pola layar admisi RP | `CreateEncounterCoreAsync`, `DoctorScheduleQuotaService`, `PatientEncounterCreateRequest`, dua enum, langkah pindai kiosk, kartu beranda, menu, route publik, CSP | Tiga tabel, tiga service, tiga controller, Turnstile, SMTP, hosted service, empat layar |

## PO-13 Sasaran kemampuan API

Rincian lengkap di `contracts/api-contract.md` *Amendment PO*. Seluruhnya **Rencana (belum tersedia)**.

| Grup | Method | Path | Hak akses | Epic |
| --- | --- | --- | --- | --- |
| Online Registration Public | `GET` | `/options/clinics`, `/options/doctor-schedules`, `/options/insurance-providers`, `/options/referral-institutions` | Tanpa login | `EPIC KSK-PO-02` |
| Online Registration Public | `POST` | `/patient-match`, `` | Tanpa login + captcha | `EPIC KSK-PO-02` |
| Online Registration Check-In | `POST` | `/check-in/verify` | `KioskRead` | `EPIC KSK-PO-03` |
| Patient Encounter | `POST` | `/kiosk` (+`onlineRegistrationId`, token) | `KioskRead` | `EPIC KSK-PO-03` |
| Online Registration | `GET` | ``, `/summary`, `/{id}`, `/{id}/referral-documents/{documentId}` | `OnlineRegistration : Read` | `EPIC KSK-PO-04` |
| Online Registration | `PUT` | `/{id}/schedule`, `/{id}/contact` | `OnlineRegistration : Update` | `EPIC KSK-PO-04` |
| Online Registration | `POST` | `/{id}/cancel` | `OnlineRegistration : Cancel` | `EPIC KSK-PO-04` |

## PO-14 Matriks kewenangan

| Tindakan | Siapa | Permission |
| --- | --- | --- |
| Mendaftar online | Pasien/wali | Tanpa login, captcha, batas laju |
| Check-in | Akun kiosk | `KioskRead` |
| Melihat daftar/detail | Petugas | `[AccessPermission("OnlineRegistration", "Read")]` |
| Mengubah jadwal/kontak | Petugas | `[AccessPermission("OnlineRegistration", "Update")]` |
| Membatalkan | Petugas | `[AccessPermission("OnlineRegistration", "Cancel")]` |

## PO-15 Batas integrasi dan billing

| Yang **MUST NOT** dibuat modul ini | Pemilik |
| --- | --- |
| Tagihan atau biaya pendaftaran online | `billing-kasir`; tagihan lahir dari kunjungan seperti biasa |
| Master pasien, kartu asuransi | Patient Management; dibuat lewat endpoint kiosk yang ada |
| Aturan jadwal, kuota, jenis pasien, consent, rujukan | `rawat-jalan` (`KSK-DEC-026` syarat 2) |
| Gateway WA/SMS, antrean notifikasi | `platform-notification` (belum ada) |
| Cek kepesertaan BPJS | Di luar scope |

## PO-16 Guardrail regulasi

| Kewajiban | Penerapan |
| --- | --- |
| Identifikasi pasien dengan dua penanda | Pasien lama: No. RM/NIK + tanggal lahir. Check-in: kode + tanggal lahir. Pasien baru: NIK hasil pindai KTP/KIA |
| UU PDP — minimisasi dan keamanan data pribadi | Data diri pasien baru minimum (`KSK-DSN-PO-004`); halaman publik hanya data tersamar; kolom sensitif tidak masuk log; rahasia tidak di repository |
| Persetujuan umum | Ditandatangani online, ditulis ke consent kunjungan dengan waktu tanda tangan asli (`KSK-DEC-034`) |
| Rekam medis | Master hanya dari dokumen identitas terverifikasi (`INV-PO-06`) |

## PO-17 Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-PO-001` | Atomik: check-in, consent, dokumen rujukan, dan penutupan berada dalam transaksi kunjungan |
| `NFR-PO-002` | Konkurensi: kunci pasien lalu kunci kuota di semua jalur; `RowVersion` untuk aksi petugas |
| `NFR-PO-003` | Waktu: seluruh aturan tanggal memakai kalender WIB |
| `NFR-PO-004` | Ketersediaan: Turnstile 5 detik fail-closed; SMTP 10 detik tanpa membatalkan pendaftaran |
| `NFR-PO-005` | Audit: riwayat tidak dapat diubah; log tanpa data sensitif |
| `NFR-PO-006` | Keamanan: endpoint publik `no-store`, pesan seragam, path tersamar di log |

## PO-18 Skenario UAT

| UAT | Epic | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-PO-01` | 01, 02, 05 | Pasien lama mendaftar online untuk besok sebagai Pribadi | Kode tampil; email terkirim; kuota berkurang 1 |
| `UAT-PO-02` | 02, 05 | Pasien memilih tanggal hari ini | Ditolak dengan arahan ke kiosk rumah sakit |
| `UAT-PO-03` | 02, 05 | Pasien lama salah tanggal lahir | Pesan umum, tanpa nama apa pun |
| `UAT-PO-04` | 02, 05 | Pasien baru Rujukan mengunggah surat 2 MB dan 8 MB | 2 MB diterima, 8 MB ditolak |
| `UAT-PO-05` | 03, 06 | Pasien lama check-in dengan kode pada hari H | Tiket tercetak; status Menjadi Kunjungan |
| `UAT-PO-06` | 03, 06 | Pasien check-in sehari lebih awal | Ditolak dengan tanggal kunjungan |
| `UAT-PO-07` | 03, 06 | Pasien baru check-in dengan KTP miliknya | No. RM terbit dari data KTP; consent tercatat |
| `UAT-PO-08` | 03, 06 | Pasien baru memindai KTP orang lain | Ditolak, diarahkan ke loket; status tetap |
| `UAT-PO-09` | 03, 06 | Pasien lupa kode, memakai NIK + tanggal lahir | Pendaftaran ditemukan, check-in berhasil |
| `UAT-PO-10` | 03, 06 | Jadwal dokter dibatalkan sebelum check-in | Ditolak, diarahkan ke loket |
| `UAT-PO-11` | 01 | Pendaftaran tidak dipakai sampai malam | Pukul 00.05 menjadi Tidak Datang; kuota kembali |
| `UAT-PO-12` | 01 | Pasien punya pendaftaran online lalu memakai kiosk Pasien Lama | Pendaftaran online menjadi Didaftarkan lewat Jalur Lain |
| `UAT-PO-13` | 04, 07 | Petugas memindahkan pendaftaran ke hari lain di poli yang sama | Kuota bertukar; kode lama tetap berlaku pada tanggal baru |
| `UAT-PO-14` | 04, 07 | Petugas memindahkan ke jadwal yang penuh, atau ke poli lain | Ditolak; pendaftaran tidak berubah |
| `UAT-PO-15` | 04, 07 | Petugas membatalkan pendaftaran mencurigakan | Dibatalkan; kode ditolak di kiosk |
| `UAT-PO-16` | 04, 07 | Petugas tanpa hak membuka menu | Menu tidak tampil; akses langsung ditolak |
| `UAT-PO-17` | 01 | Dua orang berebut satu sisa kuota bersamaan | Hanya satu berhasil |

## PO-19 Definition of Done

| Butir | Bukti |
| --- | --- |
| Pasien dapat berjalan dari halaman online sampai tiket tercetak | `UAT-PO-01`, `05`, `07` |
| Setiap jalur gagal pada §13.7 ditolak sesuai validation matrix | `UAT-PO-02`, `03`, `04`, `06`, `08`, `10`, `14` |
| Kuota tidak pernah terpakai ganda | `UAT-PO-17`; `R-PO` kuota pada `testing/acceptance-test-matrix.md` |
| Pendaftaran tidak pernah tertahan tanpa ujung | `UAT-PO-11`, `12`, `15` |
| Data pasien baru dari internet tidak masuk master sebelum check-in | AC 4 pada matriks uji |
| Halaman publik tidak membuka data pasien lain | AC 2 pada matriks uji |
| Butir menu dan kartu kiosk terjangkau dengan hak akses yang benar | `UAT-PO-16`; PO-FE.2 |
| Migration dibuat dan dijalankan dengan persetujuan terpisah | Laporan task backend |
| Kunci Turnstile, akun SMTP, domain publik, dan IP asli tersedia di produksi | `KSK-PO-OQ-010`, `015`, `016` ditutup IT (syarat go-live, bukan syarat selesai build) |

## PO-20 Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | `EPIC KSK-PO-01`: tabel, enum, migration, kuota + kunci, penutupan | Blueprint revisi 2 disetujui; persetujuan migration |
| `MVP-1` | `EPIC KSK-PO-02` dan `05`: halaman publik ujung ke ujung (dev memakai kunci uji Turnstile) | `MVP-0` |
| `MVP-2` | `EPIC KSK-PO-03` dan `06`: check-in kiosk | `MVP-1` |
| `MVP-3` | `EPIC KSK-PO-04` dan `07`: layar petugas | `MVP-0` (dapat paralel dengan `MVP-1`/`MVP-2`) |
| `POST-MVP` | WA gateway otomatis, kebijakan retensi, laporan rekap | Keputusan terpisah |

Pertanyaan terbuka sebelum development lock:

| Pertanyaan | Memblokir |
| --- | --- |
| ~~Approval `KSK-DSN-PO-001` s.d. `017` dan letak menu `KSK-PO-FE-005`~~ | Ditutup — disetujui `KSK-DEC-054` (19 Nov 2026) |
| `KSK-PO-OQ-010`, `015`, `016` (IT) | Tidak untuk build; **ya** untuk go-live |
| Peran yang mendapat `OnlineRegistration : *` di Akses Role | Tidak; diatur admin saat go-live |
