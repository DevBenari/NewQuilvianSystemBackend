# Integration Contract — Modul Radiologi

| Field | Value |
|---|---|
| Contract version | `RAD-INT-001` |
| Revision | `1` |
| Status | `approved` |
| Backend SHA | `0e2eb105` |
| Input | `RAD-DA-001-r1`, `RAD-DEC-006`, `RAD-DEC-009` |

Modul Radiologi **tidak memanggil satu pun sistem di luar Quilvian**. Seluruh integrasi di
dokumen ini bersifat internal, antar modul.

---

## 1. Radiologi → Billing Management — Fakta Kelayakan Tagih

Status: **sudah berjalan**.

| Field | Isi |
|---|---|
| Produsen | `RadStudyService` pada `BC-RAD-02` |
| Konsumen | Billing Management |
| Kontrak | `BIL-INTEGRATION-0.4` |
| Nama sumber | `Radiology` |
| Jenis efek yang sah | `RadiologyCharge` |
| Sifat | Sinkron, di dalam alur penilaian mutu |
| Arah | Satu arah, Radiologi ke Billing |
| Pemicu | Study berpindah ke `QualityAccepted` |
| Satuan | **Satu fakta per study**, bukan per pesanan |
| Bukti | `Services/RadStudyService.cs#EmitChargeEligibilityAsync:930-960@0e2eb105` |

### Isi yang dikirim

| Butir | Nilai |
|---|---|
| Pengenal induk | `RadOrderId` |
| Pengenal butir | `RadStudyId` |
| Kunjungan | `EncounterId` |
| Waktu kejadian | Waktu mutu diputuskan |
| Jumlah | `1` |
| Satuan | Pemeriksaan |
| Keterangan | Nomor study, urutan, apakah pengulangan, sebab pengulangan, pesanan tambahan, versi aturan keselamatan |

**Yang tidak dikirim:** nominal rupiah, tarif, penjamin, dan status pembayaran. Radiologi tidak
memilikinya.

### Idempotency dan penanganan gagal

| Aspek | Perilaku |
|---|---|
| Pencegahan pengiriman ganda | Penanda `BillingFactSubmitted` pada study |
| Bila pengiriman gagal | Study **tetap** `QualityAccepted`; penanda tidak diset |
| Cara memulihkan | Fakta dapat dikirim ulang; Billing menerimanya sebagai pengiriman ulang, bukan tagihan kedua |
| Rekonsiliasi | Study berstatus `QualityAccepted` dengan `BillingFactSubmitted` bernilai `false` adalah daftar yang perlu ditinjau |

> **Mengapa penanda diset setelah pengiriman berhasil, bukan sebelumnya.** Kalau diset lebih
> dulu lalu pengiriman gagal, study akan terlihat sudah dikirim padahal belum — dan
> pemeriksaan yang benar-benar dikerjakan tidak akan pernah tertagih.

### Kejadian yang **bukan** pemicu tagihan

| Kejadian | Alasan |
|---|---|
| Pesanan dibuat, diterima, dijadwalkan | Dikunci `RJ-BIL-GATE-DEC-004` |
| Citra diambil tetapi dinilai tidak layak | Bukan pemeriksaan yang dapat dipakai |
| Acquisition dihentikan di tengah jalan | Billing menilai bagian yang sempat dikerjakan dan bahan terpakai |
| **Hasil bacaan dirilis** | Dikunci `RJ-BIL-GATE-DEC-004` |

---

## 2. Radiologi → Clinical Management — Penyajian Hasil Bacaan

Status: **rencana**, `S14`.

| Field | Isi |
|---|---|
| Produsen | `RadReportController` pada `BC-RAD-03` |
| Konsumen | Rekam medis, CPPT, layar dokter |
| Sumber kebenaran | **Radiologi, selalu** |
| Sifat | Sinkron, permintaan baca |
| Arah | Clinical Management **membaca**; tidak menyalin |
| Endpoint utama | `GET /rad-reports/by-encounter/{encounterId}` |
| Decision | `RAD-DEC-006` |

### Yang wajib dan yang dilarang

| Wajib | Dilarang |
|---|---|
| Membaca lewat endpoint Radiologi setiap kali layar dibuka | Menyimpan isi bacaan di tabel modul lain |
| Menampilkan versi yang sedang berlaku | Menyimpan salinan versi lama sebagai "cache" |
| Menampilkan pesan gangguan bila Radiologi tidak dapat dihubungi | Menampilkan daftar kosong seolah pasien tidak punya hasil |

> **Bahaya yang dicegah.** dr. Andi membaca hasil pukul 08.00. Pukul 09.00 dr. Sinta merilis
> koreksi. Pukul 10.00 dr. Andi membuka lagi. Dengan baca langsung, ia melihat versi koreksi.
> Dengan salinan yang lupa diperbarui, ia masih melihat versi pukul 08.00 — dan mengambil
> keputusan pengobatan berdasarkan bacaan yang sudah diralat.

### Bila modul Radiologi tidak dapat dihubungi

| Yang terjadi | Yang ditampilkan |
|---|---|
| Permintaan gagal atau habis waktu | "Hasil radiologi sedang tidak dapat ditampilkan. Coba lagi beberapa saat." |
| **Bukan** | Daftar kosong tanpa keterangan |

Perbedaan itu penting secara klinis. Daftar kosong terbaca sebagai "pasien tidak punya
pemeriksaan radiologi" — kesimpulan yang salah dan berbahaya.

### Slot dokumen rekam medis

`PatientClinicalDocumentSource.Radiology` yang sudah ada di Clinical Management **tetap
dipakai**, tetapi hanya untuk **berkas unggahan dari luar** — misalnya hasil foto yang dibawa
pasien dari rumah sakit lain. Bukan untuk hasil bacaan yang lahir di modul ini.

---

## 3. Modul Pemesan → Radiologi

Status: **sebagian berjalan**.

| Modul pemesan | Keadaan | Catatan |
|---|---|---|
| Rawat Jalan | Endpoint tersedia | — |
| Rawat Inap | Endpoint tersedia, `InpEpisodeId` didukung | Migration `AddRadOrderInpatientContext` |
| **IGD** | **Belum tersambung** | `RAD-CONFLICT-002`; jalan keluar `RAD-DEC-009` |

### Kontraknya seragam

Seluruh modul pemesan memakai endpoint yang sama, `POST /rad-orders`, tanpa perlakuan khusus.
Yang membedakan hanya isi `EncounterId` dan `InpEpisodeId`.

### Yang harus dilakukan modul IGD

| No | Tindakan | Mendesak? |
|---:|---|---|
| 1 | Memperbaiki teks layar yang menyatakan "modul Radiologi belum ada" | **Ya** — pernyataannya sudah tidak benar sejak 31 Agustus 2026 |
| 2 | Menyambungkan pemesanan radiologi ke endpoint resmi | Sebaiknya menunggu frontend Radiologi Rilis 1 |
| 3 | Meninjau `IGD-DEC-099` yang masih `draft` | Ya — alasan penundaannya sudah gugur |

Pesanan berjenis `External` yang terlanjur tercatat **dibiarkan sebagai riwayat**, tidak
dipindahkan (`RAD-DEC-009`).

---

## 4. Integrasi Eksternal

**Tidak ada, dan itu disengaja.**

| Sistem | Keadaan | Dasar |
|---|---|---|
| RIS eksternal | Tidak diaktifkan | `RJ-BIL-GATE-DEC-004` |
| PACS | Tidak diaktifkan | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-001` |
| DICOM | Tidak diaktifkan | Sama |

Kolom `ExternalStudyUid` pada `RadStudy` adalah tempat penampung untuk kelak, dan saat ini
tidak dipakai. **Jangan** merancang kontrak pihak ketiga di atasnya sebelum ada keputusan
terpisah.

---

## 5. Ringkasan Ketergantungan

| Modul | Radiologi bergantung padanya | Bergantung pada Radiologi |
|---|:---:|:---:|
| Registration Management | Ya — kunjungan | Tidak |
| Health Services MasterData | Ya — prosedur | Tidak |
| InPatient Management | Ya — perawatan, opsional | Tidak |
| Billing Management | Tidak | Ya — fakta kelayakan tagih |
| Clinical Management | Tidak | Ya — hasil bacaan |
| IGD, Rawat Jalan, Rawat Inap | Tidak | Ya — pemesanan |

**Radiologi tidak pernah memanggil Billing untuk bertanya soal uang, dan tidak pernah memanggil
Clinical Management untuk menitipkan hasil.** Arahnya selalu satu: Radiologi menerbitkan,
modul lain membaca.


---

## 6. Adendum 23 Oktober 2026 — Identitas Butir Tagihan, Pesanan Rawat Inap, dan Tarif Ambigu

Status: **diterapkan di source, belum dibuild atau diuji runtime; menunggu persetujuan pemilik modul.** Tidak mengubah
pemicu tagihan rawat jalan/IGD (`QualityAccepted`).

### 6.1 Temuan audit

| Temuan | Bukti |
|---|---|
| Pesanan **rawat inap** (`InpEpisodeId` terisi) sudah menerbitkan fakta tagih **saat dibuat** (`milestone = OrderRequested`, identitas butir = **pesanan**), dan fakta pembatalan saat pesanan dibatalkan. Ini keputusan `PLAN-REPAIR-008` / `FIX-DOK-008-06` (CPOE → billing tanpa draf), tetapi belum tertulis di kontrak bagian 1 yang menyebut "pesanan dibuat bukan pemicu tagihan" | `RadOrderService.CreateAsync` / `CancelAsync` |
| Study yang lolos mutu menerbitkan fakta lain dengan identitas butir = **study**. Identitas fakta = (SourceContext, SourceAggregateId, **SourceItemId**, EffectType) dan item invoice unik per (SourceDomain, SourceDetailId), sehingga satu pemeriksaan rawat inap melahirkan **dua item invoice** (pesanan + study): tagihan ganda | `ClinicalMilestoneFactProducer.FindLatestFactAsync`, `BilInvoiceItemConfiguration` |
| Resolver tarif memilih tarif dengan urutan kelas/klinik lalu **kode tarif**; dua tarif sama spesifik berharga berbeda dipilih diam-diam menurut kode | `BillingSourceTariffResolver.FindTariffAsync` |

### 6.2 Aturan baru

| Aturan | Isi |
|---|---|
| Identitas butir study | Rawat jalan/IGD: butir = study (tidak berubah). Rawat inap: study pertama yang lolos mutu memakai identitas **pesanan** (revisi atas fakta pesanan: satu item invoice); study berikutnya (pengulangan yang sah ditagih) memakai identitas study sebagai tagihan tambahan. Pengulangan karena `InternalHospitalError` tetap `NotBillable` |
| Penentu "study pertama" | Tidak ada study lain pada pesanan yang sama dengan `BillingFactSubmitted = true` |
| Tarif radiologi | Dipilih `MstTariffSelector` (kelas pasien + klinik kunjungan, paling spesifik menang). Dua tarif sama spesifik berharga berbeda → efek ditolak dengan kode `TARIFF_AMBIGUOUS` dan masuk antrean rekonsiliasi; tidak dipilih acak, termurah, maupun termahal |

### 6.3 Belum dikerjakan / batas

| Hal | Status |
|---|---|
| Item invoice ganda yang **sudah terbentuk** pada data development (study rawat inap yang lolos mutu sejak 20 Oktober 2026) | Tidak diperbaiki otomatis. Deteksi baca-saja disediakan pada laporan tugas; koreksi lewat mekanisme void/adjustment Billing yang ada, bukan UPDATE langsung |
| Pesanan rawat inap yang study pertamanya ditolak mutu lalu diulang karena faktor pasien | Pengulangan itu kini memakai identitas pesanan (satu tagihan) selama study sebelumnya belum membawa tagihan; aturan bisnis tambahan bila ingin ditagih dua kali ada di pemilik modul |
| Pembuktian | Belum dibuild atau dijalankan; tes xUnit ditulis (`RadiologyBookingTariffTests`) tetapi belum dijalankan |
