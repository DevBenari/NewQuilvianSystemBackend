# Modul Operasi — Peta Kapabilitas Legacy HiSys terhadap Sistem Baru

| Field | Value |
|---|---|
| Blueprint ID | `OPS-BP-001` |
| Dokumen | Rekonsiliasi artifact analis `01-Operasi-1.mp4` terhadap `OperatingRoomManagement` |
| Tanggal | 4 September 2026 |
| Sumber legacy | Artifact analis Modul Operasi HiSys RS Metropolitan Medical Centre, 13 kapabilitas |
| Sumber sistem baru | `Areas/HealthServices/OperatingRoomManagement` dan layar `operating-room-management` |

Dokumen ini melengkapi `01-existing-capability-map.md`, yang mengaudit kemampuan yang ada
**sebelum** modul dibangun. Yang di bawah membandingkan kemampuan legacy dengan modul yang
**sudah** dibangun.

## Kesimpulan

Perbandingannya tidak berat sebelah ke satu arah. Sistem baru lebih kuat pada keselamatan
dan keterlacakan klinis; legacy lebih lengkap pada sisi tarif dan biaya.

- **3 kapabilitas tercakup penuh**, **7 tercakup sebagian**, **1 belum ada**, **1 diputuskan
  tidak dibangun**, dan **1 terjawab dengan bentuk yang berbeda**.
- Kekurangan yang tersisa mengelompok pada satu rumpun: tarif tindakan (`CAP-009`), katalog
  alat medis berbayar (`CAP-012`), dan share/diskon dokter (bagian dari `CAP-007`). Ketiganya
  menyentuh uang, bukan klinis, dan ketiganya milik Billing per `OPS-DEC-029`.
- **Satu temuan material di luar daftar kapabilitas** — pemakaian material operasi tidak
  mengurangi stok Farmasi — sudah ditutup pada 4 September 2026. Lihat bagian Temuan dan
  Penyelesaian.

Sistem baru memiliki hal-hal yang tidak terlihat sama sekali pada legacy: checklist
keselamatan tiga tahap WHO dengan bypass darurat terkendali, verifikasi kewenangan klinis
tim, addendum untuk koreksi catatan final, dan outbox idempoten. Ketiadaannya pada legacy
bukan berarti legacy salah — video hanya merekam apa yang didemonstrasikan.

## Peta Kapabilitas

| ID Legacy | Kapabilitas | Status | Di sistem baru |
|---|---|---|---|
| CAP-001 | Navigasi ruang operasi | Sebagian | Menu `Kasus Operasi` dan `Laporan Operasi`. VK dan Data Kelahiran Bayi ada pada menu yang sama di legacy tetapi berada di luar scope per `OPS-DEC-001`. |
| CAP-002 | Daftar dan filter verifikasi OK | Sebagian | Daftar kasus dengan filter status, pasien, encounter, dan rentang tanggal permintaan. Filter kamar dan tipe OK belum ada; antrean verifikasi tidak ada karena tahapannya berbeda. |
| CAP-003 | Verifikasi order operasi | Bentuk berbeda | Legacy memakai satu gerbang approve/reject dengan keterangan. Sistem baru memakai tiga sign-off kesiapan oleh dokter bedah, dokter anestesi, dan perawat (`OPS-DEC-005`, `OPS-DEC-026`). Gerbang administratif tidak dibangun (`OPS-DEC-034`). |
| CAP-004 | Informasi tindakan operasi | Tercakup | Detail kasus, `OprCaseProcedure`, dan `OprExecutionRecord`. Consent ditarik dari `TrxPatientConsent` untuk tipe Surgery dan Anesthesia. |
| CAP-005 | Daftar pasien ruang OK | Sebagian | Daftar kasus yang sama. Filter kamar, ruangan rawat, dan opsi menampilkan pasien selesai/pulang belum ada. |
| CAP-006 | Keterangan dan jadwal operasi | Sebagian | `OprSchedule` mencakup ruang, waktu, buffer, revisi, dan alasan perubahan. Recovery room, tipe ASA, kelompok pasien anestesi, tipe departemen, dan pemakaian lebih ruangan sengaja tidak distrukturkan (`OPS-DEC-036`). |
| CAP-007 | Penyusunan tim operasi | Sebagian | `OprTeamMember` dengan peran, lead, dan status verifikasi kewenangan — lebih ketat dari legacy. Share dan diskon dokter tidak ada. |
| CAP-008 | Katalog kategori dan tindakan | Sebagian | Tindakan berasal dari `TrxPatientProcedure`. Pengelompokan "kategori jenis operasi" tidak ada. |
| CAP-009 | Indikator ketersediaan tarif | **Belum ada** | Tidak ada rujukan tarif sama sekali di area Operasi. |
| CAP-010 | Registrasi tindakan yang dilakukan | Tercakup | `OprCaseProcedure` dengan urutan dan penanda primer; jejak pelaku dan waktu pada `OprStatusHistory`. |
| CAP-011 | Pemakaian obat dan alkes | Tercakup, lebih kuat | `OprMaterialUsage` mencatat batch, serial, revisi, dan hasil `Used`/`Returned`/`Wasted`/`Corrected`. Legacy hanya memperlihatkan jumlah dan satuan. Sejak `OPS-DEC-028` pemakaiannya juga membukukan pengurangan stok di depo Farmasi. |
| CAP-012 | Pemilihan medical equipment | **Tidak dibangun** | `OPS-DEC-035`; alat dicatat sebagai material lewat master Farmasi, harganya milik Billing. |
| CAP-013 | Audit order dan approval | Sebagian | `OprStatusHistory` mencatat pelaku, waktu, status sebelum dan sesudah untuk setiap transisi. Tidak ada user approve karena tidak ada tahap approve. |

## Temuan

### OPS-FND-001 — Pemakaian material operasi tidak mengurangi stok Farmasi (ditutup 4 Sep 2026)

`OperatingRoomMaterialService` menyelesaikan item terhadap `MstDrug`, lalu menulis baris
`OprIntegrationDelivery` bertujuan `Inventory` di dalam transaksi yang sama. Itu benar sebagai
outbox. Persoalannya: **tidak ada yang membaca kotak itu**. Satu-satunya pembaca
`OprIntegrationDelivery` adalah controller Operasi sendiri untuk rekonsiliasi dan retry manual.

Akibatnya, obat yang dipakai di kamar operasi tetap tercatat sebagai stok yang ada di Farmasi.
Pada saat artifact legacy dibuat, hal ini tidak dapat dibandingkan karena modul Farmasi belum
punya ledger. Sekarang ledger itu ada dan authoritative (`TrxDrugStockMutation`), sehingga
selisihnya menjadi nyata dan akan tumbuh setiap kali operasi mencatat pemakaian.

Legacy justru memperlihatkan stok ruangan dan stok RS pada layar pemakaiannya, yang
menunjukkan sisi ini memang diharapkan tersambung.

Perbaikannya bukan menambah kapabilitas baru: `DrugUsageService` dan `DrugStockService` sudah
menyediakan pemotongan stok berbasis batch. Yang belum ada adalah konsumen outbox yang
memanggilnya. Keputusan yang diperlukan sebelum dikerjakan ada pada `OPS-OQ-102`.

### OPS-FND-002 — Outbox Billing juga tanpa konsumen (diperjelas 4 Sep 2026)

Outbox Operasi bertujuan `Billing` juga tidak punya pembaca.

Audit `04-billing-dependency-audit.md` memperjelas duduk perkaranya, dan hasilnya berbeda dari
dugaan awal saya. Penerima penagihan **sudah ada dan sudah bekerja**:
`ClinicalMilestoneFactProducer` memanggil `BillingFolioService.RecognizeMilestoneAsync`, dipakai
Clinical Management, Laboratory, dan resep Farmasi, lengkap dengan idempotency dan pelacakan
hasil pengiriman.

Operasi tidak memakainya, melainkan membangun outbox kedua. Jadi yang perlu diputuskan bukan
kapan Billing dibangun, melainkan mana dari dua mekanisme itu yang menjadi kontrak resmi —
`OPS-FND-003` pada audit tersebut.

## Pertanyaan Terbuka Baru

| ID | Pertanyaan | Status |
|---|---|---|
| `OPS-OQ-101` | Apakah perlu gerbang administratif di samping tiga sign-off kesiapan klinis? | **closed** — `OPS-DEC-034`, tidak dibangun sekarang; sign-off tetap satu-satunya gerbang |
| `OPS-OQ-102` | Depo mana yang menjadi sumber stok pemakaian kamar operasi? | **closed** — `OPS-DEC-027`, 4 Sep 2026 |
| `OPS-OQ-103` | Apakah share dan diskon dokter menjadi milik Modul Operasi atau Billing? | **closed** — `OPS-DEC-029`, milik Billing |
| `OPS-OQ-104` | Apakah alat medis berbayar perlu master tersendiri? | **closed** — `OPS-DEC-035`, tidak dibangun sebelum keputusan final |
| `OPS-OQ-105` | Apakah ASA, kelompok pasien anestesi, dan recovery room perlu terstruktur? | **closed** — `OPS-DEC-036`, tetap berupa teks; master anestesi kompleks tidak dibangun |

Seluruh pertanyaan terbuka modul ini ditutup pemilik kebutuhan pada 4 September 2026.

Tiga di antaranya ditutup dengan keputusan untuk **tidak membangun** — gerbang administratif,
master alat medis, dan data anestesi terstruktur. Itu tetap keputusan, bukan pekerjaan yang
tertunda: sampai dibuka kembali, tidak boleh ada master alat medis maupun master anestesi
kompleks yang dibuat.

## Yang Tidak Dibandingkan

- `Daftar Pasien Ruang Bersalin (VK)` dan `Data Kelahiran Bayi` berada pada menu legacy yang
  sama tetapi merupakan modul kebidanan, di luar `OPS-DEC-001`.
- Perbedaan versi footer `3.0.1` dan `3.0.4` pada artifact legacy tidak relevan bagi sistem baru.
- Audio bukti legacy tidak ditranskripsikan, sehingga aturan yang hanya diucapkan tidak masuk
  ke dalam perbandingan ini.

## Penyelesaian OPS-FND-001 — 4 September 2026

Keputusan pemilik kebutuhan menutup `OPS-OQ-102` dan `OPS-OQ-103`, dan integrasinya dibangun.

### Yang dibangun

| Bagian | Isi |
|---|---|
| Pemetaan depo | `MstOperatingRoomStockSource` — kamar operasi → depo farmasi, satu sumber aktif per kamar, ditegakkan indeks unik terfilter |
| Consumer outbox | `OperatingRoomInventoryDispatchService` — membaca pesan `Inventory` dan memanggil `DrugStockService` |
| Endpoint | `POST .../cases/{caseId}/integration/inventory/dispatch`, `GET`/`PUT .../stock-sources` |
| Penelusuran koreksi | Kolom `CorrectionOfUsageId` pada `OprMaterialUsage` |
| Validasi satuan | `DrugUnitConversionResolver` milik Farmasi; kolom `UnitMeasurementId` pada `OprMaterialUsage` |
| Retur operasi | Kolom `SourceOprMaterialUsageId` pada `TrxDrugReturn`; pembukuan mengajukan draft retur |

### Aturan pembukuannya

- `Used` dan `Wasted` mengurangi stok; `Returned` menambah. Sebabnya tercatat pada alasan mutasi.
- `Corrected` membukukan **selisihnya saja** terhadap catatan yang dikoreksi. Membukukan jumlah
  penuh akan memotong barang yang sama dua kali. Selisih negatif membalik arah: koreksi yang
  mengurangi jumlah terpakai berarti barang kembali ke depo.
- Nomor batch yang dicatat di kamar operasi mengalahkan FEFO, karena ia menyebut barang yang
  benar-benar dipakai pada pasien itu. FEFO dipakai hanya bila nomor batchnya tidak dicatat.
- Barang yang kembali **tidak dibukukan ke stok sama sekali** di tahap ini. Ia menjadi draft
  Retur Obat Farmasi (`OPS-DEC-033`), dan stoknya bertambah hanya setelah apoteker memeriksa —
  pemeriksaan yang sama dengan retur dari bangsal, sehingga tidak ada aturan kedua yang harus
  dijaga tetap sejalan.
- Satuan pemakaian wajib merujuk `MstMeasurement` dan diterjemahkan ke satuan stok sebelum
  dibukukan (`OPS-DEC-031`). Satuan yang tidak dikenal ditolak, bukan dianggap sama dengan
  satuan stok.
- Pesan yang gagal tidak menghentikan pesan lain, dan tidak meninggalkan potongan stok separuh
  jalan. Sebabnya tercatat per pesan dan terlihat pada rekonsiliasi.
- Pemanggilan berulang tidak memotong stok dua kali: pesan yang sudah diterima dilewati.

### Yang tetap tidak dilakukan

- Kamar operasi tidak diberi saldo stok sendiri.
- Consumer tidak pernah menulis saldo secara langsung; seluruhnya melalui `DrugStockService`.
- Outbox `Billing` tetap tanpa consumer (`OPS-DEC-029`).

### Pertanyaan terbuka baru dari implementasi

| ID | Pertanyaan | Status |
|---|---|---|
| `OPS-OQ-106` | Apakah satuan pemakaian operasi harus divalidasi terhadap satuan Farmasi? | **closed** — `OPS-DEC-031`, wajib divalidasi dan dikonversi |
| `OPS-OQ-107` | Kapan pembukuan stok dijalankan, dan berapa jeda yang dapat diterima? | **closed** — `OPS-DEC-032`, sinkron dengan retry, tanpa SLA detik |
| `OPS-OQ-108` | Apakah retur material operasi memakai alur Retur Obat Farmasi yang sudah ada? | **closed** — `OPS-DEC-033`, memakai alur yang sama |
