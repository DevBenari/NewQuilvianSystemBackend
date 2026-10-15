# ISSUE-006 — Kelas kamar, pilihan layanan, dan cakupan asuransi belum konsisten

```yaml
issue_id: ISSUE-DOK-006
module_id: rawat-inap
submodule: dokter-rawat-inap
layar: "Dokter Rawat Inap — Resep, Tindakan, Penunjang; lintas batas dengan admisi, penempatan kamar, dan Billing"
sumber_laporan: "Permintaan audit pemilik 12-10-2026: dua pertanyaan dan ketetapan kelas perawatan fleksibel"
tanggal_issue: "2026-10-12"
status: SELESAI   # 2026-10-13: ISS-DOK-006-01..04 diperbaiki lewat FIX-DOK-006-01..06 ✅; uji UI/runtime belum dilakukan; ISS-DOK-006-T1 tindak lanjut terpisah
tanggal_selesai: "2026-10-13"
keparahan_tertinggi: High
source_sha_backend: "d908988248f6652ed7f2ce84aae9eb3e38dd36a0 (MHamzah), pemeriksaan akhir"
source_sha_frontend: "affcfcb4ea9f5177b1ff9a3ca0c09d15183af248 (HamzahV2), pemeriksaan akhir"
source_sha_awal_backend: "05634d1f498a57c990789367521c8b430e08c32e"
source_sha_awal_frontend: "7d44526ff11822e04d2ed558994f602986b4e2ef"
basis_pemeriksaan: "Working tree dibaca langsung; HEAD berubah di luar tindakan audit; source inti yang dikutip telah dibandingkan dan tidak berubah"
rencana_perbaikan: ../plan-repair/plan-repair-006-kelas-perawatan-dan-cakupan-asuransi.md
ditulis_dengan: diagnose-module-issue
```

## 1. Ringkasan

Sistem **belum sepenuhnya sesuai** dengan ketetapan pelapor. Katalog resep dan tindakan yang diperiksa tidak langsung membuang semua item dari kelas lain, tetapi tarif rumah sakit masih dipilih menurut kelas kunjungan, sehingga obat bisa tetap terlihat namun tidak dapat dipilih atau disimpan. Kelas pada riwayat penempatan sudah mengikuti kamar, sedangkan perhitungan klinis dan sebagian cakupan Billing masih membaca kelas pada kunjungan. Ada kesalahan terpisah: mesin cakupan klinis mengabaikan kolom kelas pada aturan asuransi, sementara mesin kasir memeriksanya. Status obat yang tidak ditanggung juga ditulis sebagai `Cover 0%`, belum secara tegas `Tidak Di-cover`. Audit menghasilkan empat temuan utama dan satu temuan tambahan; pilihan kelas awal di admisi sesuai desain lama, sehingga penyesuaiannya merupakan perubahan desain.

### Batas bukti dan kepemilikan

Semua perilaku berstatus **SUDAH-VERIFIKASI** berarti dibaca dari source atau dokumen, bukan sudah diuji pada aplikasi yang sedang berjalan. Tidak ada screenshot, capture database, pengujian browser, atau query database dalam audit ini. Contoh di bawah adalah simulasi berdasarkan cabang kode, bukan hasil transaksi pasien nyata. Kedua repository memiliki perubahan lokal pada awal audit; source tersebut tidak diubah oleh audit. HEAD kemudian berubah di luar tindakan audit ke SHA pemeriksaan akhir di metadata. Perbandingan Git atas berkas inti dan dokumen desain yang dikutip menunjukkan tidak ada perubahan antara SHA awal dan akhir, termasuk mesin tarif/cakupan, penyimpanan resep, pemetaan label, katalog, dan pemilih kelas pada pembayaran. Kutipan baris untuk berkas tersebut berlaku pada SHA akhir. Pada pemeriksaan akhir, perubahan audit hanya dua dokumen issue/rencana ini; tidak ada tindakan commit, merge, atau push oleh audit.

Pemilik dokumen ditetapkan dari `rawat-inap/02-module-map.md:48,158,159,267` dan `dokter-rawat-inap/03-frontend-architecture.md:773-784`: resep dan tindakan merupakan bagian ruang kerja dokter rawat inap. Model dan mesin perhitungan tetap milik Pharmacy Management, Clinical Management, dan Billing Management; kelas kamar serta riwayat hunian milik `episode-rawat-inap`. Dokumen ini mencatat radius dampak, tidak memberi kewenangan implementasi pada modul-modul tersebut.

## 2. Laporan asli

| No. laporan | Isi pelapor | Lampiran |
| --- | --- | --- |
| 1 | "Coba anda audit system yang di terapkan di system ini seperti apa?" | Tidak ada |
| 2 | "Apakah sudah sesuai dengan yang saya sebutkan sebelumnya?" | Tidak ada |
| — | Kelas perawatan fleksibel; kelas mengikuti kamar yang dipilih; semua item resep, tindakan, dan lainnya tetap tampil; item yang tidak ditanggung diberi keterangan tidak di-cover | Ketetapan tertulis pada pesan yang sama |
| — | Asuransi kelas 1 dan kamar kelas 1; dokter wajib memberi obat yang hanya ditanggung kelas 2: obat tetap diberikan dan tidak di-cover | Contoh bisnis pada pesan yang sama |
| — | Tambahan pelapor: pada admisi rawat inap, kelas perawatan yang ada saat penambahan asuransi tidak perlu diinput; kelas terpilih ketika memilih kamar | Klarifikasi tertulis 12-10-2026 |

Tidak ada lampiran yang belum dipetakan.

### Ketetapan pelapor yang sudah tersedia

| ID lokal | Ketetapan, 12-10-2026 | Implikasi |
| --- | --- | --- |
| A-01 | Kelas perawatan mengikuti kamar yang dipilih | Kelas rencana admisi, kelas kamar aktual, dan hak polis harus dapat dibedakan |
| A-02 | Item tidak disembunyikan karena kelas perawatan | Penentuan tarif dan cakupan tidak boleh mempersempit katalog klinis berdasarkan kelas |
| A-03 | Item yang tidak di-cover tetap dapat digunakan | Tidak ditanggung berbeda dari item tidak tersedia atau tidak aman secara klinis |
| A-04 | Contoh obat hanya untuk cakupan kelas 2 tidak ditanggung bagi kasus kelas 1 tersebut | Kesamaan nama obat tidak otomatis menentukan tanggungan; perlu aturan item dan kelas yang tepat |
| A-05 | Pada admisi rawat inap, kelas perawatan tidak diinput saat menambah atau memilih asuransi; kelas otomatis mengikuti kamar yang dipilih | Hapus pemilih kelas dan syarat wajib kelas pada tahap penjamin/pembayaran; tampilkan kelas turunan pada pilihan kamar dan ringkasan admisi |

Aturan ini sudah diberikan pelapor; tidak perlu ditanyakan ulang. Belum ada keputusan untuk menganggap semua item dengan kelas sama otomatis ditanggung tanpa memeriksa polis, kontrak, pengecualian, atau batas manfaat.

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ISS-DOK-006-01 | 1, 2 | Obat tetap tampil tetapi tarif kelas dapat menahan pemilihan dan penyimpanan | BUG, RULE_VIOLATION | BE dan FE resep | High | SUDAH-VERIFIKASI | FIX-DOK-006-01, FIX-DOK-006-03 |
| ISS-DOK-006-02 | 1, 2, tambahan | Kelas kamar tercatat, tetapi konteks cakupan belum mengikuti riwayat penempatan; input kelas masih ada pada panel asuransi | BUG; DESIGN_CHANGE pada admisi | Episode, Clinical, Billing, FE admisi | High | SUDAH-VERIFIKASI; transaksi terdampak belum diperiksa | FIX-DOK-006-01, FIX-DOK-006-02, FIX-DOK-006-06 |
| ISS-DOK-006-03 | 1, 2 | Mesin cakupan klinis mengabaikan kelas pada aturan asuransi | BUG, RULE_VIOLATION | Clinical; perbandingan Billing | High | SUDAH-VERIFIKASI | FIX-DOK-006-04 |
| ISS-DOK-006-04 | 1, 2 | Status obat tidak ditanggung tampil `Cover 0%` | RULE_VIOLATION, BUG | FE komponen bersama | Medium | SUDAH-VERIFIKASI | FIX-DOK-006-05 |
| ISS-DOK-006-T1 | — | Cakupan Gizi dan Bank Darah belum dihitung oleh adapter penunjang | DESIGN_CHANGE terhadap cakupan desain saat ini | Penunjang lintas modul | Medium | SUDAH-VERIFIKASI | Tindak lanjut terpisah |

## 4. Rincian per temuan

Semua path `Areas/...` di bagian ini relatif terhadap repository backend. Path `src/...` relatif terhadap repository frontend. Dasar SHA masing-masing ada pada metadata.

### ISS-DOK-006-01 — Filter tarif kelas memengaruhi kemampuan meresepkan

| Atribut | Nilai |
| --- | --- |
| No. laporan | 1, 2 |
| Jenis | BUG, RULE_VIOLATION |
| Area | Clinical Management, Pharmacy Management, frontend resep |
| Keparahan | High |
| Status bukti | SUDAH-VERIFIKASI pada source |
| Perbaikan | FIX-DOK-006-01, FIX-DOK-006-03 |

**Apa yang terjadi.** Pernyataan bahwa semua katalog otomatis difilter kelas **tidak terbukti** pada jalur yang diperiksa. Namun, terdapat pembatasan lain yang menghasilkan masalah bisnis serupa: obat terlihat tetapi tombol pilih mati ketika tarif yang sesuai kelas tidak ditemukan.

**Kenapa terjadi.** Rantai source yang dibaca:

| Tahap | Bukti | Perilaku |
| --- | --- | --- |
| Permintaan FE | `src/lib/hooks/health-services/inpatient-management/use-inpatient-drug-catalog.jsx:73-96` | Mengirim `encounterId`, pencarian, paginasi, jenis obat/bahan racikan; tidak memfilter kelas |
| Daftar BE | `Areas/HealthServices/ClinicalManagement/Controllers/PrescribingDrugController.cs:133-163,309-319` | Memuat obat aktif yang dapat diresepkan, lalu menambahkan hasil tarif/cakupan per obat; hasil gagal tidak dibuang dari daftar |
| Pemilihan tarif | `Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs:499-550` | Kandidat tarif harus sesuai unit, klinik, kelas atau berlaku umum; jalur khusus tunai dapat memakai tarif lain bila semua kandidat berharga sama |
| Bila tarif tidak cocok | Berkas yang sama `:51-55` | Menghasilkan kegagalan `ConfigurationMissing`, sebelum pemeriksaan cakupan asuransi |
| Kesiapan obat | `PrescribingDrugController.cs:619-642,666-675` | `CanSelect` mensyaratkan tarif valid serta satuan dosis dan penyerahan |
| Tombol FE | `src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-drug-catalog-panel.jsx:80-88,139-144` | `canSelect=false` mematikan tombol pilih |
| Simpan resep | `Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkspaceService.cs:359-367,626-636` | Resep reguler maupun bahan racikan ditolak bila resolusi tarif/cakupan tidak valid |
| Jalur item resep lain | `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionItemController.cs:216-227` | Menolak kondisi yang sama |

**Tindakan berbeda.** `PatientProcedureController.cs:140-208,213-255` memuat daftar tindakan menurut jenis layanan/pelaku, kemudian memilih tarif menurut kelas bila parameter kelas diberikan. Tindakan tanpa tarif cocok tetap dikembalikan. Hook rawat inap `src/lib/hooks/health-services/inpatient-management/use-inpatient-procedure-tab.jsx:109-123,155-160` tidak mengirim filter kelas. Source terbaru `PatientProcedureOrderService.cs:201-221` sudah membolehkan pesanan dengan harga belum tersedia; jangan mengulang klaim penolakan dari issue lama tanpa membaca source terbaru.

**Tarif umum memang punya filter kelas.** `Areas/HealthServices/MasterData/Controllers/TariffController.cs:239-264,669-675` menyaring daftar tarif menjadi kelas yang diminta atau tarif umum. Ini bukti filter daftar **tarif**, bukan bukti semua daftar obat dan layanan tersembunyi. Audit tidak menetapkan seluruh consumer endpoint tarif itu sebagai layar klinis.

**Apakah menyimpang dari desain?** Ya, pada resep: `dokter-rawat-inap/03-frontend-architecture.md:784` dan keputusan induk `RWI-DEC-218` (`rawat-inap/00-interview-decisions.md:2743`) menetapkan harga sebagai informasi yang tidak menahan pengiriman pesanan. Ketetapan A-02 dan A-03 menegaskan pemisahan pilihan klinis dari tanggungan.

**Dampak nyata.** Simulasi pasien asuransi kelas 1, obat aktif dengan satuan lengkap, dan hanya tarif rumah sakit kelas 2: obat dapat tetap tampil, tetapi `ConfigurationMissing` membuat tombol pilih mati. Ini belum menjadi transaksi obat tidak di-cover sebagaimana diminta.

**Rekomendasi.** Pertahankan katalog lintas kelas. Pisahkan kelayakan klinis, ketersediaan item, kesiapan harga, dan cakupan; harga belum diketahui tidak boleh diam-diam menjadi harga final nol. Jangan menghapus seluruh filter tarif tanpa menentukan sumber harga yang benar.

### ISS-DOK-006-02 — Kelas penempatan dan kelas untuk cakupan berasal dari sumber berbeda

| Atribut | Nilai |
| --- | --- |
| No. laporan | 1, 2 |
| Jenis | BUG pada konteks kelas; DESIGN_CHANGE pada input kelas awal |
| Area | Episode Rawat Inap, Clinical Management, Billing Management |
| Keparahan | High |
| Status bukti | SUDAH-VERIFIKASI; frekuensi kasus pada data belum diketahui |
| Perbaikan | FIX-DOK-006-01, FIX-DOK-006-02, FIX-DOK-006-06 |

**Apa yang terjadi.** Kelas tagihan dalam riwayat hunian sudah mengikuti kamar. Namun, ini belum menjamin seluruh tarif obat/tindakan dan cakupan ikut memakai kelas kamar pada waktu layanan.

**Kenapa terjadi.**

| Bukti | Makna |
| --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-admission-payment-step.jsx:31-64` | Petugas memilih kelas perawatan tersendiri saat tahap pembayaran |
| Berkas yang sama `:89-97,485-490` | `ServiceClassBlock` berisi pemilih kelas disematkan sebagai isi `BasePayerWorkspace`, sehingga tampil dalam area asuransi/penjamin pada admisi |
| `src/utils/health-services/inpatient-management/inpatient-admission-payment-utils.jsx:267-310` | Validasi pemilihan pembayaran juga memeriksa `patientClassId`; menyembunyikan field saja belum menghilangkan kewajiban memilih kelas |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs:1167-1178` | Kunjungan rawat inap baru dibangun dengan kelas pada permintaan admisi |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:652,925,1947-1951` | Kelas penempatan mengikuti `MstRoom.PatientClassId`; jika kamar tidak memiliki kelas, memakai kelas episode |
| Berkas yang sama `:956-967` | Transfer mempertahankan kelas awal pada episode dan mencatat kelas baru pada penempatan; riwayat dipakai untuk penghitungan ulang tarif kamar |
| `Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs:289-298` | Konteks asuransi mengambil `encounter.PatientClassId`, bukan penempatan yang berlaku pada waktu layanan |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs:346-361` | Pemilihan aturan asuransi Billing juga memakai kelas pada kunjungan |

**Apakah menyimpang dari desain?** Penempatan sesuai `RWI-DEC-013` (`rawat-inap/00-interview-decisions.md:2468`): kelas tagihan mengikuti kamar dengan riwayat. Pilihan kelas awal sesuai desain admisi `episode-rawat-inap/05-skema-tampilan.md:413-441`, sehingga mengubahnya menjadi kelas turunan dari kamar memerlukan penyesuaian desain. Ketidakselarasan konteks kelas lintas modul bertentangan dengan A-01; jangan memperbaikinya dengan menimpa seluruh riwayat ke kelas terakhir.

**Dampak nyata.** Simulasi kunjungan dibuka kelas 1, kemudian pasien menempati kamar kelas 2: penempatan menyimpan kelas 2, sedangkan mesin klinis tetap membaca kelas kunjungan bila belum diubah lewat proses lain. Belum ada bukti data pasien tertentu mengalami ini; untuk membuktikan transaksi diperlukan perbandingan kunjungan, riwayat penempatan, waktu layanan, dan hasil kalkulasi yang berwenang.

**Rekomendasi.** Gunakan kelas efektif berdasarkan penempatan dan waktu layanan untuk rawat inap. Sesuai klarifikasi A-05, hapus input kelas perawatan dari penambahan/pemilihan asuransi dan tahap pembayaran admisi, termasuk syarat wajib kelas pada validasi langkah tersebut. Kelas dipilih secara tidak langsung lewat kamar dan ditampilkan sebagai informasi turunan. Data kelas awal yang sudah tersimpan tetap dipertahankan untuk penelusuran; jangan meminta petugas menginputnya kembali. Hak/manfaat polis tetap data berbeda, bukan sumber pengisian otomatis kelas perawatan. Sebelum kamar dipilih, kelas perawatan belum ditetapkan; kontrak penyimpanan draft admisi dan layanan sebelum penempatan masih perlu diselaraskan.

### ISS-DOK-006-03 — Kolom kelas aturan asuransi tidak diperiksa mesin klinis

| Atribut | Nilai |
| --- | --- |
| No. laporan | 1, 2 |
| Jenis | BUG, RULE_VIOLATION |
| Area | Clinical Management; pembanding Billing Management |
| Keparahan | High |
| Status bukti | SUDAH-VERIFIKASI; salah tanggung pada transaksi nyata belum dibuktikan |
| Perbaikan | FIX-DOK-006-04 |

**Apa yang terjadi.** Ada dukungan status tidak ditanggung, tetapi kecocokan kelas belum diterapkan konsisten.

**Kenapa terjadi.** `Areas/HealthServices/MasterData/Models/MstInsuranceCoverageRule.cs:30` menyimpan `PatientClassId`. Mesin klinis `InsuranceCoverageService.cs:608-628` memeriksa kelas pada **buku tarif asuransi**, tetapi `:646-671` tidak memeriksa kelas pada **aturan cakupan**. Mesin kasir `BillingCoverageAdapter.cs:356-364` justru memeriksa `x.PatientClassId == null || x.PatientClassId == encounter.PatientClassId`. Mesin perusahaan `CompanyGuarantorCoverageService.cs:269-272` juga memiliki pemeriksaan kelas; itu pembanding pola yang sudah tersedia, bukan alasan menyamakan kontrak perusahaan dan asuransi.

**Yang sudah sesuai.** Pada mesin klinis, bila tarif rumah sakit valid tetapi buku tarif asuransi tidak cocok, atau tidak ada aturan yang cocok, hasilnya `NotCovered` dengan `IsValid=true` (`InsuranceCoverageService.cs:271-310,734-763`). Obat bisa dipilih apabila syarat klinis/satuannya lengkap. Sama kelas saja tidak cukup: paket manfaat, kontrak item, persentase, batas manfaat, serta persetujuan juga memengaruhi hasil.

**Apakah menyimpang dari desain?** Ya terhadap makna kriteria kelas yang disimpan pada master aturan dan terhadap A-04. Perbedaan implementasi mesin klinis/kasir merupakan bukti inkonsistensi; audit tidak menganggap salah satu mesin otomatis merupakan kontrak final untuk seluruh perhitungan.

**Dampak nyata.** Simulasi: tarif rumah sakit berlaku umum; buku tarif asuransi berlaku kelas 1; satu-satunya aturan `Drug` untuk parasetamol berlaku kelas 2 dan menanggung 100%. Mesin klinis dapat memilih aturan kelas 2 itu untuk pasien kelas 1, karena kelas tidak masuk kondisi pencarian. Mesin kasir menyingkirkan aturan kelas 2. Ini dapat menampilkan tanggungan yang berbeda antara layar resep dan kalkulasi kasir, bergantung pada data konfigurasi dan komponen tagihan.

**Rekomendasi.** Terapkan kelas efektif pada pencocokan aturan klinis, tetapkan prioritas aturan khusus/umum secara eksplisit, dan buktikan keselarasan dengan Billing menggunakan konfigurasi uji yang sama. Jangan membuat asumsi bahwa kelas 1 otomatis menanggung semua item kelas 2; contoh pelapor justru menetapkan pengecualian itu.

### ISS-DOK-006-04 — Label tidak ditanggung pada resep belum tegas

| Atribut | Nilai |
| --- | --- |
| No. laporan | 1, 2 |
| Jenis | RULE_VIOLATION, BUG |
| Area | Frontend komponen resep bersama |
| Keparahan | Medium |
| Status bukti | SUDAH-VERIFIKASI |
| Perbaikan | FIX-DOK-006-05 |

**Apa yang terjadi.** Untuk hasil asuransi valid yang tidak ditanggung, lencana obat dapat menulis `Cover 0%` alih-alih `Tidak Di-cover`.

**Kenapa terjadi.** `src/components/ui/doctor-clinical-base/doctor-prescription-presentation-utils.js:168-192` membangun label dari persentase terlebih dahulu dan selalu mengembalikan `isCovered:true` ketika ada informasi cakupan. Helper tidak memutuskan label berdasarkan `CoverageStatus` atau `IsCovered`. `DoctorPrescriptionDrugRow.jsx:19-26` menampilkan label helper tersebut. Pada kegagalan harga, panel rawat inap juga menyembunyikan lencana bila `isCoverageApplicable` tidak benar (`prescription-drug-catalog-panel.jsx:116`), sehingga status harga belum diketahui tidak dapat dianggap sama dengan tidak ditanggung.

**Apakah menyimpang dari desain?** Ya terhadap A-03 dan makna status hasil backend. Penunjang sudah membedakan `Ditanggung` dan `Tidak Di-cover` pada `InpAncillaryOrderAdapter.cs:64-72`; pola label dapat diselaraskan tanpa menduplikasi mesin perhitungan di frontend.

**Dampak nyata.** Dokter melihat persentase nol tanpa alasan yang jelas, padahal perlu mengetahui obat tetap boleh diresepkan dan tanggungan asuransi bernilai nol. Nilai internal `isCovered:true` pada helper juga salah untuk kasus itu, meskipun komponen lencana yang diperiksa hanya membaca label.

**Rekomendasi.** Tampilkan status backend beserta alasannya: ditanggung, ditanggung sebagian, tidak di-cover, perlu persetujuan, atau belum dapat ditentukan. Persentase menjadi rincian tambahan. Jangan mengubah kegagalan konfigurasi menjadi klaim pasti tidak ditanggung.

## 5. Tanya-jawab pelapor

**T: Apakah pilihan kelas sekarang memfilter item?**

**J: Ada filter kelas pada tarif, tetapi bukan berarti semua katalog klinis menyembunyikan item lintas kelas.** Katalog resep dan tindakan yang ditelusuri tetap memuat item; masalah utamanya resep dapat ditahan karena tarif kelas tidak cocok. Katalog laboratorium juga memuat pemeriksaan terlebih dahulu, lalu mencari kontrak asuransi menurut kelas (`Areas/HealthServices/LaboratoryManagement/Services/LabCatalogService.cs:50-105,363-395`). Penunjang rawat inap menambahkan status bagi item Lab/Radiologi/Tindakan melalui adapter terpisah. Audit ini tidak mengklaim seluruh menu rumah sakit sudah diuji.

**T: Apakah sudah sesuai dengan proses bisnis yang dijelaskan?**

**J: Belum.** Penempatan kamar dan sebagian hasil `NotCovered` sudah mendukung maksud pelapor, tetapi pemilihan resep, sumber kelas, pencocokan aturan asuransi, dan label masih memiliki selisih.

**T: Bagaimana contoh asuransi kelas 1 dan parasetamol yang hanya ditanggung kelas 2?**

| Konfigurasi simulasi | Hasil source saat ini | Sesuai? |
| --- | --- | --- |
| Obat hanya punya tarif RS kelas 2; pasien asuransi kelas 1 | Katalog dapat menampilkan obat, tetapi harga `ConfigurationMissing`, tombol pilih tidak aktif | Tidak |
| Obat punya tarif RS berlaku umum; kontrak asuransi hanya kelas 2 | `NotCovered`, nominal ditanggung 0, dapat dipilih bila syarat klinis/satuan lengkap | Ya pada mesin cakupan; label masih perlu dibenahi |
| Tarif RS umum; kontrak asuransi kelas 1; aturan obat hanya kelas 2 | Mesin klinis berpotensi memakai aturan kelas 2; Billing menyaringnya | Tidak |
| Tarif RS valid; tidak ada aturan yang cocok untuk item | `NotCovered`, dapat dipilih bila syarat lainnya lengkap | Ya pada mesin cakupan |

Istilah **obat kelas 2** belum merupakan atribut tetap pada master `MstDrug`; kelas ditemukan pada tarif RS, buku tarif asuransi, dan aturan cakupan. Ketiga bentuk itu menghasilkan cabang berbeda. Tidak perlu menambah kolom kelas pada obat sebelum sumber klasifikasi bisnisnya disepakati.

## 6. Temuan tambahan

### ISS-DOK-006-T1 — Gizi dan Bank Darah belum memiliki hasil cakupan di adapter penunjang

| Atribut | Nilai |
| --- | --- |
| Jenis | DESIGN_CHANGE terhadap cakupan desain yang diperiksa |
| Area | Inpatient Management, Nutrition Management, Blood Bank Management, Billing |
| Keparahan | Medium |
| Status bukti | SUDAH-VERIFIKASI |
| Kenapa dicantumkan | Pelapor menyebut resep, tindakan, dan item lain; status cakupan belum tersedia merata |
| Perbaikan | Tindak lanjut terpisah, tidak dimasukkan sebagai implementasi modul dokter |

`Areas/HealthServices/InPatientManagement/Services/InpAncillaryOrderAdapter.cs:51-58` mengembalikan `Tarif belum tersedia` untuk Gizi/Bank Darah tanpa menghitung cakupan. `dokter-rawat-inap/03-frontend-architecture.md:793` memang mencatat kondisi ini. Ini belum membuktikan layanan tersebut tidak ditanggung; sumber pemetaan tarif/komponen dan pemiliknya perlu ditetapkan bila seluruh layanan harus menampilkan hasil cakupan.

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk pelapor

Tidak ada pertanyaan yang menghalangi kesimpulan audit. Jika kelak diperlukan pembuktian kasus runtime tertentu, diperlukan nama menu dan capture tersamarkan untuk membandingkan hasil layar dengan konfigurasi; audit ini tidak meminta data pasien nyata.

### Untuk pemilik

| ID | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| --- | --- | --- | --- | --- |
| K-01 | **Sebagian diputuskan lewat A-05:** tidak ada input kelas pada penambahan/pemilihan asuransi atau pembayaran; kelas mengikuti kamar. Sisa: kontrak draft/layanan sebelum penempatan, hak polis berbeda dari kamar, dan waktu efektif layanan | Draft belum berkelas hingga kamar dipilih; detail transaksi sebelum penempatan dan naik/turun kelas perlu dikunci | Hak polis terpisah; jangan meminta kelas manual atau mengambil kelas polis sebagai kelas perawatan; gunakan riwayat penempatan pada waktu layanan | Sisa detail FIX-DOK-006-01, FIX-DOK-006-02; integrasi FIX-DOK-006-06 |
| K-02 | Makna "obat kelas 2" dan harga ketika tarif RS hanya ada untuk kelas lain | Batas cakupan / variasi harga / varian item | Tetapkan kelas cakupan pada aturan penjamin; jangan membentuk daftar obat terpisah atau menebak harga kelas lain; ikuti RWI-DEC-218 untuk harga belum tersedia | FIX-DOK-006-01, FIX-DOK-006-03 |
| K-03 | Prioritas aturan cakupan ketika aturan umum dan khusus kelas sama-sama cocok | Khusus kelas menang / prioritas master lain | Aturan khusus kelas menang dalam tingkat sasaran item yang sama; pastikan urutannya sama dengan Billing setelah diputuskan | Bagian urutan aturan FIX-DOK-006-04 |

Persetujuan implementasi belum diminta pada audit ini. Ketetapan A-01 sampai A-05 sudah dicatat; penghapusan input kelas pada tahap asuransi/pembayaran sudah diputuskan pelapor dan tidak ditanyakan ulang. K-01 sampai K-03 hanya menutup detail yang belum dijelaskan.

## 8. Catatan pola

Source lama mencampur kelas rencana admisi, kelas aktual per periode kamar, dan hak/manfaat asuransi. Menggunakan satu `PatientClassId` kunjungan untuk semuanya menyebabkan penyesuaian kamar tidak otomatis konsisten pada pelayanan dan penagihan. Setelah klarifikasi A-05, alur target tidak lagi meminta input kelas rencana pada tahap asuransi/pembayaran; nilai historis tetap ditelusuri. Kelayakan memilih layanan juga masih bercampur dengan kesiapan harga pada resep.

Issue terdahulu [ISSUE-DOK-004](issue-004-tipe-pembayaran-encounter-resep.md) menelusuri cara bayar perusahaan; [ISSUE-DOK-005](issue-005-tindakan-tarif-tanggungan-gagal-simpan.md) menelusuri tarif/status tindakan. Source tindakan terbaru sudah memiliki jalur harga belum tersedia; audit baru ini tidak membalik perbaikan tersebut. Radius komponen resep mencakup rawat inap reguler/racikan dan IGD; helper serta layanan backend juga dipakai rawat jalan. Usulkan invariant umum A-01 sampai A-05 pada blueprint lintas modul melalui proses yang berwenang, tanpa mengubah rules global pada audit ini.

### Endpoint yang dibaca

#### Health Services / Clinical Management / Prescribing Drug

Base URL: `api/v1/health-services/clinical-management/prescribing-drugs`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/` | Katalog obat dengan tarif dan cakupan per item | `PrescribingDrug : Read` | Query `encounterId`, pencarian, kategori, paginasi, filter jenis obat | `PrescribingDrugPagedResponse` di dalam `ApiResponse<T>` |

`200`: daftar berhasil dimuat, termasuk item dengan konfigurasi belum lengkap. `400`: konteks kunjungan tidak valid. `401/403`: akses tidak tersedia sesuai autentikasi/hak pengguna.

#### Health Services / Clinical Management / Patient Procedure

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/master-options` | Memuat tindakan yang sesuai jenis layanan/pelaku | `PatientProcedure : Read` | Query pencarian, `careSetting`, `audience`, opsional `patientClassId` dan `serviceUnitId`, `take` | `List<PatientProcedureMasterOptionResponse>` di dalam `ApiResponse<T>` |

`200`: opsi berhasil dimuat. `400`: konteks layanan/pelaku tidak valid. `401/403`: akses tidak tersedia.

#### Health Services / Inpatient Management / Inpatient Ancillary Order

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/ancillary-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/coverage-status` | Membaca cakupan dan perkiraan harga item penunjang/tindakan | `InpatientEpisode : Read`; harga mengikuti hak membuat order terkait | Query `AncillaryCoverageQuery` | `List<CoverageStatusItem>` di dalam `ApiResponse<T>` |

`200`: hasil per item tersedia; harga dapat berstatus belum dapat diperkirakan. `400`: jenis/daftar item tidak valid. Episode yang tidak tersedia dan penolakan domain mengikuti `InpAncillaryOrderException.StatusCode`; `401/403` mengikuti akses. Tidak ada endpoint baru yang diusulkan sebagai seolah sudah tersedia.

## 9. Hasil implementasi

Perbaikan dilaksanakan menurut [PLAN-REPAIR-DOK-006](../plan-repair/plan-repair-006-kelas-perawatan-dan-cakupan-asuransi.md) dan kontrak [RWI-KELAS-006-1.0](../../contracts/kelas-perawatan-repair-006.md) (SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112`). Source di-commit pemilik pada backend `8551291f` dan frontend `2d0a6890e`; penutupan diperiksa terhadap HEAD `4ec02370` / `f4495aa14`.

| Temuan | Perbaikan dan task | Yang berubah pada sistem | Status |
| --- | --- | --- | --- |
| ISS-DOK-006-01 | FIX-DOK-006-01, FIX-DOK-006-03 — ✅ [BE-RWI-226](../../task/report/backend/BE-RWI-226.md) | Obat lintas kelas tetap tampil. Harga memakai tarif yang cocok dengan kelas kamar atau tarif umum; tarif kelas lain tidak lagi diambil hanya karena harganya sama. Tanpa tarif sesuai/umum, resep ditahan dengan pesan "Lengkapi tarif sebelum menyimpan resep" | ✅ Diperbaiki |
| ISS-DOK-006-02 | FIX-DOK-006-02, FIX-DOK-006-06 — ✅ [BE-RWI-224](../../../episode-rawat-inap/task/report/backend/BE-RWI-224.md), ✅ [BE-RWI-225](../../../integrasi-billing/task/report/backend/BE-RWI-225.md), ✅ [FE-RWI-240](../../../episode-rawat-inap/task/report/frontend/FE-RWI-240.md) | Kelas layanan dibaca dari riwayat penempatan pada waktu layanan; Billing menilai tiap item menurut waktunya. Admisi tidak lagi meminta kelas pada pembayaran/konfirmasi; kelas ditetapkan dari kamar yang dipilih dan divalidasi server | ✅ Diperbaiki |
| ISS-DOK-006-03 | FIX-DOK-006-04 — ✅ [BE-RWI-223](../../task/report/backend/BE-RWI-223.md), ✅ BE-RWI-225 | Mesin klinis kini memeriksa kelas aturan asuransi; Clinical dan Billing memakai urutan aturan yang sama, aturan khusus kelas didahulukan | ✅ Diperbaiki |
| ISS-DOK-006-04 | FIX-DOK-006-05 — ✅ [FE-RWI-239](../../task/report/frontend/FE-RWI-239.md) | `Cover 0%` diganti label dari status backend: Ditanggung, Ditanggung sebagian, Tidak Di-cover, Perlu persetujuan, atau Belum dapat ditentukan | ✅ Diperbaiki |
| ISS-DOK-006-T1 | Tindak lanjut terpisah | Tidak dikerjakan pada plan ini; tetap menjadi tindak lanjut pemilik modul Gizi/Bank Darah | Terbuka |

### Jawaban pelapor sesudah perbaikan

Untuk contoh pelapor — pasien kamar kelas 1, Parasetamol hanya punya aturan cakupan kelas 2 — Parasetamol tetap tampil dan dapat diresepkan bila tarif kelas 1 atau tarif umum tersedia. Aturan kelas 2 tidak dipakai, sehingga lencana menunjukkan "Tidak Di-cover" dan bagian asuransi nol. Bila tarif juga tidak ada, resep ditahan sampai tarif dilengkapi. Kelas pasien tidak lagi dipilih manual; kelas mengikuti kamar yang dipilih, dan setelah pindah kamar layanan baru memakai kelas kamar baru.

### Temuan saat penutupan

- Audit ulang 13 Oktober 2026 menemukan konteks asuransi/perusahaan masih menyimpan nama kelas awal encounter (atau nama kelas polis) sebagai `PatientClassNameSnapshot` pada resep dan tindakan. Diperbaiki dua baris di `EncounterInsuranceService.cs`; build 0 error dan QBE PASS. Perbaikan belum di-commit.
- BOM UTF-8 tertambah pada tujuh berkas source backend; kosmetik, tidak dikembalikan.
- Salinan helper cakupan lama di `doctor-queues/tabs/prescription/doctor-prescription-shared.jsx` tidak diimpor di mana pun; kode mati di luar scope.

### Batas bukti

Bukti berupa review source/kontrak, build backend, QBE, ESLint, unit test, dan build produksi frontend; rinciannya di plan bagian 9. Belum ada uji layar, transaksi, atau kalkulasi Billing pada data nyata, dan aplikasi yang berjalan belum memuat build baru. Langkah berikutnya: uji manual admisi tanpa kelas → pilih kamar kelas 1 → resep obat yang hanya dijamin kelas 2, lalu periksa tagihan setelah pindah kamar.

## 10. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-12 | Audit source atas dua pertanyaan pemilik; empat temuan utama, satu tambahan; tanpa perubahan aplikasi atau database | `diagnose-module-issue` |
| 2026-10-12 | Catat pergeseran HEAD eksternal dan cocokkan ulang source inti yang dikutip; temuan tetap berlaku pada SHA pemeriksaan akhir | `diagnose-module-issue` |
| 2026-10-12 | Tambahan pelapor A-05: kelas perawatan tidak diinput pada penambahan asuransi admisi; perluas ISS-DOK-006-02 ke penghapusan input/validasi kelas dan tautkan FIX-DOK-006-06. K-01 ditutup sebagian, tanpa perubahan source aplikasi | `diagnose-module-issue` |
| 2026-10-13 | Hasil implementasi PLAN-REPAIR-DOK-006 ditambahkan (bagian 9); status issue SELESAI untuk ISS-DOK-006-01..04 dengan batas bukti tanpa uji runtime; ISS-DOK-006-T1 tetap terbuka | Penutupan plan repair |
