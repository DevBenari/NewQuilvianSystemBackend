# Gap Frontend Farmasi

Dicatat 1 Oktober 2026. Ditemukan saat pengujian manual alur resep ujung ke ujung.

## `GAP-PHA-FE-001` — Workflow farmasi tengah belum memiliki UI

| Hal | Isi |
|---|---|
| Prioritas | **P0** |
| Status | **Dikerjakan** 1 Oktober 2026 |
| Dampak | Alur resep terputus: petugas tidak dapat menelaah, meracik, maupun menelaah akhir dari layar mana pun |

### Keadaan sebelum perbaikan

Dari layar, alurnya hanya sampai `Farmasi → Daftar Resep`, lalu melompat ke
`Farmasi → Penyerahan Obat`. Empat tahap di antaranya tidak punya pintu:

| Tahap | Status yang dituju | Layar |
|---|---|---|
| Telaah resep | `QueuedAtPharmacy` → `VerifiedByPharmacy` | tidak ada |
| Penyiapan/racik | `InPreparation` | tidak ada |
| Selesai penyiapan | `AwaitingFinalCheck` | tidak ada |
| Telaah obat akhir | `ReadyToDispense` | tidak ada |

Akibatnya layar Penyerahan Obat — yang hanya menerima `ReadyToDispense` dan
`PartiallyDispensed` — tidak akan pernah menerima resep apa pun yang ditulis lewat aplikasi.

### Yang ternyata sudah tersedia

Gap ini **bukan** pekerjaan dari nol. Yang hilang hanya komponennya:

| Lapisan | Berkas | Keadaan |
|---|---|---|
| Endpoint backend | `prescription-reviews`, `prescription-preparations`, `prescription-final-checks` | lengkap |
| Service frontend | `prescription-pharmacy-workflow.service.js` | lengkap, 11 fungsi |
| Hook | `use-prescription-pharmacy-workflow.js` | lengkap, 430 baris |
| Konstanta | `prescription-review.constants.js`, termasuk `FINAL_CHECK_CRITERIA` | lengkap |
| CSS | `prescription-pharmacy-workflow.module.css` | lengkap, 543 baris |
| **Komponen** | — | **tidak ada** |

Service dan hook itu tidak dipakai komponen mana pun, sehingga menganggur.

### Keputusan penempatan

Workflow ditempatkan sebagai **panel pada detail resep**, bukan submenu baru. Alasannya:
petugas sudah berada di sana ketika memeriksa resep, dan tiap tahap bekerja pada satu resep
tertentu. Struktur menu Farmasi tidak perlu bertambah.

### Catatan endpoint

Tidak ada endpoint baru dibuat. Tiga hal yang perlu diketahui pembaca berikutnya:

1. **Penyiapan dan telaah akhir tidak punya `GET`.** Mulanya diduga `start` dapat dipanggil
   ulang untuk membaca keadaan, karena `PrescriptionPreparationService.StartAsync` memang
   memakai ulang baris yang sudah ada. **Dugaan itu salah**: gerbang di depannya menolak begitu
   tahapnya bukan lagi `VerifiedByPharmacy`, dengan *"Penyiapan hanya dapat dimulai setelah
   telaah farmasi disetujui."* Panel karena itu tidak bersandar pada `start` untuk memulihkan
   keadaan; ia membaca tahap dari `workspace.fulfillmentStatus`, dan barisnya disusun hook dari
   item resep. Memuat ulang halaman di tengah penyiapan tetap menampilkan obat yang sama.
2. **Kriteria telaah akhir berasal dari frontend.** Backend menerima `CriterionCode`/`Name` apa
   adanya tanpa memeriksanya ke master. Daftarnya sudah ditetapkan tim pada
   `FINAL_CHECK_CRITERIA` — sembilan butir ketepatan — dan tidak ditambah sendiri.
3. **`fulfillmentStatus` berada di tingkat atas `workspace`**, bukan di dalam `summary`. Status
   bar detail resep sudah membacanya begitu; panel ini mengikuti, dengan `summary` sebagai
   cadangan.

## Cacat backend yang ditemukan saat pengujian

`GAP-PHA-BE-001` — **resep yang dibuat lewat `POST /prescriptions` tidak pernah dapat
difinalkan.** Prioritas P0. **Selesai** 1 Oktober 2026.

| Sisi | Yang terjadi |
|---|---|
| `PrescriptionController` saat membuat resep | menetapkan `FulfillmentStatus = WaitingForPayment` (2) |
| `PrescriptionValidationService` saat konsultasi diselesaikan | menuntut `WaitingForClinicalFinalization` (1), selainnya ditolak `INVALID_PRESCRIPTION_FULFILLMENT_STATUS` dengan keparahan **Error** |

Keparahan `Error` tidak dapat diakui lewat `AcknowledgedWarningKeys`, sehingga jalannya tertutup
sama sekali. Resep yang lahir dari layar konsultasi dokter tidak terkena karena jalurnya berbeda;
yang terkena adalah resep yang dibuat lewat endpoint resep langsung.

### Akar masalah

Bukan dua jalur pembuatan yang berbeda perilaku — **hanya ada satu jalur**, dan layar konsultasi
dokter memakai jalur yang sama (`use-doctor-prescription.js` → `createPrescription` →
`POST /prescriptions`). Jadi resep dari layar dokter pun terkena; yang "bekerja" hanyalah
pembuatannya, bukan finalisasinya.

Akarnya satu baris pada `PrescriptionController`: resep ditandai `PrescriptionStatus = Draft`
tetapi sekaligus `FulfillmentStatus = WaitingForPayment`. Dua pernyataan yang bertentangan —
`Draft` berarti dokter belum memfinalkan, `WaitingForPayment` berarti finalisasi itu sudah lewat.

Penguat bahwa itu memang keliru: **entitas `PhmPrescription` sudah berdefault
`WaitingForClinicalFinalization`**. Controller sengaja menimpa default yang benar.

Satu jalur lain, `MedicationReconciliationService`, sudah melakukannya dengan benar sejak awal —
ia menetapkan `WaitingForClinicalFinalization`. Itu yang dipakai sebagai referensi perilaku.

### Perbaikan

Keputusan status awal dipindahkan dari controller ke domain service sebagai
`PrescriptionWorkflowService.ApplyInitialClinicalState(entity)`, yang menetapkan ketiga status
sekaligus karena ketiganya saling mengunci. Controller memanggilnya; validation service **tidak**
disentuh, dan aturan Billing `2 → 3/4` tidak diubah.

### Tangga tahap sesudah perbaikan

| Dari | Ke | Dipicu oleh |
|---|---|---|
| — | 1 Menunggu Finalisasi Klinis | pembuatan resep |
| 1 | 2 Menunggu Pembayaran | finalisasi konsultasi yang sah |
| 2 | **4 Dalam Antrean Farmasi** | konsumsi surat clearance Billing |
| 4 | 5 Diverifikasi Farmasi | telaah disetujui |
| 5 | 6 Sedang Disiapkan | penyiapan dimulai |
| 6 | 12 Menunggu Telaah Obat Akhir | penyiapan diselesaikan |
| 12 | 7 Siap Diserahkan | telaah obat akhir lolos |

Catatan yang perlu diketahui: **tangga 2 → 4 melompati tahap 3** (`ReadyForPharmacy`). Itu
perilaku yang sudah ada, bukan akibat perbaikan ini — `PrescriptionFinancialClearanceService`
memindahkan langsung ke `QueuedAtPharmacy` sesuai `PHA-DEC-069`. Tahap 3 karena itu praktis tidak
pernah terjadi. Panel frontend menerima 3 maupun 4 sebagai pembuka tab telaah.

## Hasil pengujian alur lengkap

Diuji 1 Oktober 2026 pada `localhost/QuilvianNewDevIkbalFr`, lewat endpoint yang sama dengan yang
dipanggil panel.

| Langkah | Tahap sesudahnya | Hasil |
|---|---|---|
| Mulai telaah | 4 — Dalam Antrean Farmasi | 200 |
| Simpan 12 kriteria sebagai `Compliant` | 4 | 200 |
| Setujui telaah | 5 — Diverifikasi Farmasi | 200 |
| Mulai penyiapan | 6 — Sedang Disiapkan | 200 |
| Selesai penyiapan | 12 — Menunggu Telaah Obat Akhir | 200 |
| Telaah obat akhir | **7 — Siap Diserahkan** | 200 |

Dua gerbang harus dilewati dengan data uji karena penerbitnya belum berdiri, dan keduanya
dicatat apa adanya di `bin/uji-alur-farmasi.sh`:

- tahap 2 → 3 menunggu surat clearance Billing (`BE-BKC-067`);
- `PhmPrescriptionFinancialProjection` disisipkan sebagai pengganti salinan surat itu.

Penelusuran peramban: 12 pemeriksaan, seluruhnya lulus — panel tampil, tiga tab berpindah, tahap
terbaca `SIAP DISERAHKAN` dari backend, kriteria telaah termuat, tabel penyiapan tampil, sembilan
butir ketepatan tampil, tombol Etiket Obat membuka halaman etiketnya.

## `GAP-PHA-FE-002` — Etiket Obat tidak memiliki entry point

| Hal | Isi |
|---|---|
| Prioritas | **P1** |
| Status | **Selesai** 1 Oktober 2026 |
| Dampak | Fitur lengkap tetapi tidak dapat ditemukan petugas |

Halaman `prescription-labels/[slug]` beserta view, service, dan endpoint
`GET /prescription-labels/{prescriptionId}` seluruhnya ada dan berfungsi. Tetapi tidak ada satu
pun tautan menuju ke sana: nol butir menu, nol tombol dari layar mana pun. Satu-satunya rujukan
ke rutenya berada di dalam folder label itu sendiri.

Hanya dapat dibuka dengan mengetik URL-nya langsung.

Diselesaikan: bagian "Etiket Obat" pada detail resep, dengan tombol yang mendaftarkan token rute
privat seperti halaman ber-token lain, lalu membuka `prescription-labels/{token}`. Tidak dibuat
butir menu baru.

## Gap lain yang terlihat dan belum ditangani

**Penyangga footer.** Footer aplikasi berposisi `fixed`, dan penyangganya hanya dipasang pada
elemen gulir yang dikenali `Footer.jsx`. Halaman Farmasi bukan salah satunya, sehingga isi
paling bawah tertutup footer. Sudah diperbaiki pada `prescription-list-detail.module.css` dan
`prescription-list.module.css`. **Belum** diperbaiki pada `prescription-workspace.module.css`
(`padding-bottom: 0.65rem`) dan `prescription-pharmacy-workflow.module.css` (tidak ada
`padding-bottom`), karena gejalanya belum terlihat sendiri di layar.
