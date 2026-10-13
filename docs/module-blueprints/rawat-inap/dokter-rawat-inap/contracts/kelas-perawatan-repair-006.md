# Kontrak perbaikan kelas perawatan dan cakupan asuransi

Versi: `RWI-KELAS-006-1.0`. Tanggal: 2026-10-12. Wewenang: instruksi pemilik “kerjakan seluruh task tersebut sampai tuntas”, sesudah PLAN-REPAIR-DOK-006. Berlaku khusus delta pada plan tersebut; tidak mengubah approval blueprint lain.

## Aturan yang sudah ditetapkan

1. Asuransi dan pembayaran admisi tidak meminta kelas perawatan. Pilihan kamar menentukan kelas. Data hak polis tetap terpisah.
2. Katalog klinis tidak disaring berdasarkan kelas. Status tidak ditanggung tidak membatalkan pesanan.
3. Resolusi kelas memakai waktu layanan dengan interval penempatan `[StartDateTime, EndDateTime)`. Penempatan transfer yang sudah berakhir tetap merupakan riwayat sah. Hanya baris dihapus atau digantikan koreksi (`SupersededByCorrectionId`) yang dikecualikan; `IsSuperseded` sendiri bukan alasan menghapus riwayat transfer.
4. Sebelum kamar dipilih, kelas aktual belum ditetapkan. Draft pada klien boleh dilanjutkan; transaksi yang membutuhkan kelas dibuat setelah kamar dipilih. Nilai kelas polis tidak digunakan sebagai pengganti.
5. Aturan asuransi hanya cocok jika kelasnya kosong (umum) atau sama dengan kelas aktual. Dalam sasaran item yang sama, aturan khusus kelas didahulukan, kemudian manfaat khusus, prioritas, masa berlaku, urutan legacy, dan ID. Sasaran: tarif, obat/tindakan, kategori obat, kategori layanan. Tidak ada inferensi bahwa kelas 1 mencakup kelas 2.
6. Status tampilan berasal dari backend: `Covered` → Ditanggung; `PartiallyCovered` → Ditanggung sebagian; `NotCovered` → Tidak Di-cover; `NeedApproval` → Perlu persetujuan; `ConfigurationMissing`/`Unknown` → Belum dapat ditentukan; asuransi tidak berlaku → tanpa lencana asuransi. Persentase nol bukan status ditanggung.

Contoh: kamar kelas 1 dan aturan Parasetamol khusus kelas 2 tidak cocok. Obat tetap tersedia. Bila harga tersedia dan tidak ada aturan lain yang cocok, bagian asuransi nol.

## Keputusan tambahan pemilik, 2026-10-12

K-02 diputuskan: gunakan tarif umum item bila ada; bila tarif yang sesuai maupun tarif umum tidak tersedia, tahan penyimpanan resep. Item tetap tampil dengan penjelasan harga belum tersedia. Ketetapan ini menggantikan rekomendasi lama agar resep tanpa harga tetap disimpan. Tidak mengambil tarif kelas lain secara sembarang atau menyimpan nol sebagai harga final. Harga nol yang memang dikonfigurasi pada tarif sah tetap boleh.

K-01 diputuskan: ketika hak polis berbeda dari kamar, periksa kelas kamar pada waktu layanan dan aturan asuransi tersimpan; tidak menerapkan hierarki naik/turun kelas implisit. Sebelum pemesanan kamar, draft tetap di klien; buka episode dengan kelas kamar yang dipilih. K-03 mengikuti rekomendasi solusi terpilih plan yang diperintahkan untuk dilaksanakan: khusus kelas didahulukan dalam sasaran item yang sama.

## Kontrak API terdampak

### Health Services / Inpatient Management / Inpatient Episode

Base URL: `/api/v1/health-services/inpatient-management/episodes`.

| Method | Path | Request dan perilaku | Respons/hak akses |
| --- | --- | --- | --- |
| POST | `/` | `OpenAdmissionRequest.SelectedBedId` opsional untuk kompatibilitas klien lama. Klien admisi baru selalu mengirim bed pilihan. Server membaca kelas/unit dari kamar bed tersebut dan mengabaikan kelas kiriman yang berbeda. Bed/kamar harus aktif, tidak dihapus, berkelas dan sesuai unit | Wrapper existing; kegagalan konteks 400; `InpatientEpisode : Create` |
| PUT | `/{id}` | `UpdateAdmissionRequest` existing. Kelas diturunkan dari kamar reservasi aktif, atau dipertahankan dari episode bila belum ada reservasi. Kelas tidak dapat diganti melalui konfirmasi. Unit harus sesuai kamar reservasi | Wrapper existing; 400 konteks, 409 lifecycle; `InpatientEpisode : Update` |

`SelectedBedId` memvalidasi konteks pembukaan; bukan reservasi atau penempatan. Booking dan placement tetap memakai operasi existing beserta pemeriksaan ulang konflik. Booking menyelaraskan kelas draft dengan kamar yang dipesan, termasuk setelah pemilihan ulang. Nilai kelas awal episode merupakan informasi kamar pilihan, bukan bukti pasien sudah ditempatkan. `InpServiceClassResolver` hanya menganggap penempatan sah sebagai kelas aktual pada waktu layanan.

### Health Services / Clinical Management / Prescribing Drug

Base URL: `/api/v1/health-services/clinical-management/prescribing-drugs`.

| Method | Path | Request dan perilaku | Respons/hak akses |
| --- | --- | --- | --- |
| GET | `/` dan `/{drugId}` | Query konteks kunjungan existing, tanpa menambah filter kelas pada master obat | `PrescribingDrugPagedResponse`/detail existing; `PrescribingDrug : Read` |

Harga memakai tarif yang cocok konteks kelas/unit/klinik, lalu tarif umum yang kompatibel. `TariffId` membedakan tarif sah bernilai nol dari tarif yang tidak tersedia. `CanSelect` tetap membutuhkan tarif dan satuan yang sah. Tarif tidak ada: `ConfigurationMissing`, `IsValid=false`, pesan lengkapi tarif, tanpa harga final palsu. `NotCovered` tetap `IsValid=true`, nominal ditanggung nol dan dapat dipesan jika syarat lainnya terpenuhi.

### Health Services / Pharmacy Management / Prescription Workspace

Base URL: `/api/v1/health-services/pharmacy-management/prescription-workspaces`.

| Method | Path | Perilaku | Hak akses |
| --- | --- | --- | --- |
| PATCH | `/{prescriptionId}/autosave` | Penyimpanan existing memakai resolver yang sama untuk reguler/racikan. Tanpa tarif sesuai/umum ditolak; tidak ditanggung bukan penolakan klinis | `PrescriptionWorkspace : Update` existing |

### Health Services / Pharmacy Management / Prescription

Base URL: `/api/v1/health-services/pharmacy-management/prescriptions`.

| Method | Path | Perilaku | Hak akses |
| --- | --- | --- | --- |
| POST | `/` | Membuka resep sesuai kontrak existing; item memakai resolver tarif/cakupan saat ditambahkan atau disimpan | `Prescription : Create` |

Jalur prescription-items, prescription-compound-items, dan rekonsiliasi juga tetap memeriksa `IsValid` resolver; tidak membuat endpoint harga tertunda.

## Integrasi dan batas waktu

`EncounterInsuranceContext.ServiceOccurredAt` menyimpan timestamp layanan; `ServiceDate` tetap tanggal untuk masa berlaku aturan. Billing item dan pajak turunannya mengirim `SourceOccurredAt` ke `BillingCoverageComponent.ServiceOccurredAt`. Setiap komponen mendapat kelas menurut timestamp tersebut; retur memakai waktu item asal. Untuk komponen agregat/administrasi yang tidak memiliki waktu layanan sendiri, fallback adalah waktu kalkulasi, bukan tanggal layanan rekaan. Snapshot kalkulasi yang sudah tersimpan tidak diubah.

Clinical dan Billing memakai urutan `MstInsuranceCoverageRule` yang sama. Cakupan perusahaan memakai kelas per komponen dan mengutamakan aturan kelas khusus pada sasaran yang sama. Validitas polis/penjamin dan rumus finansial existing tetap berlaku; preview per item bukan janji nominal final agregat, karena limit kumulatif, pembulatan, pajak dan komponen tagihan diperiksa Billing. Delta ini menyamakan kelas, masa aturan per layanan, dan urutan aturan; tidak merombak kontrak buku tarif asuransi atau rumus limit lintas modul.

Hak akses, validasi satuan/dosis, lifecycle, transaksi, dan jejak audit existing tetap berlaku. Tidak ada entity, schema atau migration baru; tidak ada eksekusi database atau deployment.
