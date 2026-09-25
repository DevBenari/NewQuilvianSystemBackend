# Module Artifact - Shift Kasir

## 1. Ringkasan Modul

Shift Kasir adalah domain operasional dalam modul **Kasir & Billing** yang mengelola tanggung jawab kasir dari pembukaan shift, kas awal, aktivitas transaksi, logout tanpa menutup shift, tutup shift, serah terima, rekonsiliasi, verifikasi closing, tindak lanjut selisih, hingga kontrol pembukaan shift berikutnya.

Artifact ini menggabungkan attachment terbaru dengan keputusan pemilik proses tanggal 25 September 2026. Rekomendasi untuk aktivitas tanpa penerimaan pembayaran ditandai terpisah sebagai rekomendasi, bukan ketentuan yang diklaim berasal langsung dari regulasi.

## 2. Evidence yang Diproses

| Source | Type | Status | Coverage | Notes |
|---|---|---|---|---|
| `Shift Kasir - Updated Artifact(1).md` | Dokumen Markdown | processed | Seluruh dokumen | Baseline aktor, capability, alur, status, rumus, aturan, audit trail, dan keputusan terdahulu |
| Klarifikasi pemilik proses, 25 September 2026 | Keputusan bisnis dalam percakapan | incorporated | Jawaban 1-7 | Status `CLOSED`, field wajib, cakupan validasi menu, dan konfirmasi aturan baseline |
| Permenkes No. 82 Tahun 2013 | Referensi resmi eksternal | consulted | Lampiran arsitektur dan keamanan SIMRS | Basis rekomendasi pemisahan hak akses dan aksi sistem |
| Permenkes No. 24 Tahun 2022 | Referensi resmi eksternal | consulted | Pasal 29-30 | Basis rekomendasi kontrol melihat, menginput, dan memperbaiki data |
| Ketentuan SATUSEHAT Data | Referensi resmi eksternal | consulted | Manajemen hak akses dan audit trail | Basis rekomendasi pencatatan aktivitas dan otorisasi |

## 3. Actors / Roles

| Actor / Role | Tanggung Jawab | Evidence |
|---|---|---|
| Kasir | Login, memilih lokasi/loket, membuka dan menutup shift, mencatat kas per pecahan, menjalankan transaksi, logout, serta melakukan serah terima. | `Shift Kasir - Updated Artifact(1).md`, Bagian 3, 6, 11-12 |
| Supervisor/Kepala Kasir | Memeriksa closing berselisih, menetapkan hasil verifikasi, memberi catatan tindak lanjut, dan menyelesaikan tindak lanjut. | Attachment, CAP-010 s.d. CAP-011; BP-005 |
| Manajemen | Memutuskan penyelesaian selisih kas. | Attachment, RULE-013 |
| Petugas medis penginput | Melakukan koreksi obat/pelayanan sebelum billing closed; menjadi sumber catatan/verifikasi bila perubahan dilakukan kasir. | Attachment, jawaban terdahulu No. 5 dan 7; RULE-019 s.d. RULE-020 |
| Sistem | Memvalidasi user, shift, kasir/loket, field wajib, menghitung rekonsiliasi, menjaga status, memblokir shift berikutnya, dan merekam audit trail. | Attachment, Bagian 3-8 dan 12; klarifikasi pemilik proses |

## 4. Capability / Feature Registry

| ID | Capability / Feature | Description | Actor | Evidence | Confidence |
|---|---|---|---|---|---|
| CAP-001 | Login dan validasi user | Mengautentikasi user tanpa membuka shift otomatis. | Kasir, Sistem | Attachment, BP-001; klarifikasi mengikuti baseline | High |
| CAP-002 | Pemilihan lokasi dan loket | Menetapkan konteks operasional kasir. | Kasir | Attachment, CAP-002 dan BP-001 | High |
| CAP-003 | Pembukaan shift | Membuka shift dengan seluruh field/parameter wajib diisi. | Kasir, Sistem | Attachment, CAP-003; klarifikasi No. 3 | High |
| CAP-004 | Kas awal per pecahan | Mencatat nominal, jumlah, subtotal, dan total kas awal. | Kasir | Attachment, CAP-004; RULE-002 dan RULE-016 | High |
| CAP-005 | Status shift | Mengelola `OPEN` dan `CLOSED`; closing berhasil menghasilkan `CLOSED`. | Sistem | Attachment, CAP-005; klarifikasi No. 2 | High |
| CAP-006 | Logout tanpa tutup shift | Mengakhiri sesi tanpa mengakhiri shift atau tanggung jawab kas. | Kasir | Attachment, CAP-006 dan BP-002 | High |
| CAP-007 | Tutup shift | Menutup tanggung jawab kas dengan seluruh field/parameter wajib dan memulai rekonsiliasi. | Kasir, Sistem | Attachment, CAP-007; klarifikasi No. 1 dan 3 | High |
| CAP-008 | Rekonsiliasi kas | Menghitung kas seharusnya, kas aktual, selisih, dan status hasil. | Sistem | Attachment, CAP-008 s.d. CAP-009; BP-003 | High |
| CAP-009 | Verifikasi closing | Memproses closing berselisih melalui supervisor/kepala kasir. | Supervisor, Sistem | Attachment, CAP-010; BP-004 s.d. BP-005 | High |
| CAP-010 | Tindak lanjut selisih | Mewajibkan catatan dan menyelesaikan status tindak lanjut. | Supervisor | Attachment, CAP-011; BP-005 | High |
| CAP-011 | Serah terima shift | Mencatat kasir pengganti pada loket yang sama sebagai pernyataan serah terima; seluruh field/parameter wajib diisi. | Kasir, Sistem | Attachment, jawaban terdahulu No. 8; klarifikasi No. 1 dan 3 | High |
| CAP-012 | Kontrol shift berikutnya | Memeriksa hasil closing sebelumnya sebelum mengizinkan shift baru. | Sistem | Attachment, CAP-012; BP-006; RULE-010 dan RULE-014 | High |
| CAP-013 | Audit trail | Merekam pembukaan, closing, verifikasi, serta aktivitas perubahan penting. | Sistem | Attachment, CAP-013; RULE-012 | High |
| CAP-014 | Validasi akses menu | Mewajibkan shift `OPEN` untuk membuka Running Invoice; menu lain yang ditetapkan dapat dibuka tanpa shift. | Sistem | Klarifikasi No. 7 | High |

## 5. Menu / Screen / UI Elements

### Struktur Kasir & Billing

1. **Shift Kasir**: Shift Aktif, Buka Shift, Tutup Shift, Serah Terima Shift, dan Riwayat Shift.
2. **Invoice & Billing Kasir**.
3. **Running Invoice**.
4. **Riwayat Pembayaran**.
5. **Petty Cash**.

### Matriks Akses Menu

| Menu | Dapat Dibuka Tanpa Shift `OPEN` | Catatan |
|---|---:|---|
| Shift Kasir | Ya | Digunakan untuk membuka shift dan melihat status operasional. |
| Invoice & Billing Kasir | Ya | Akses menu tidak membutuhkan shift; aksi penerimaan pembayaran tetap mengikuti validasi aksi finansial. |
| Running Invoice | Tidak | Shift aktif `OPEN` wajib tersedia sebelum menu dapat dibuka. |
| Riwayat Pembayaran | Ya | Akses baca dapat dilakukan tanpa shift sesuai hak akses. |
| Petty Cash | Ya | Menu dapat dibuka tanpa shift; aksi yang mengubah uang/saldo tetap tunduk pada otorisasi dan audit trail. |

[Evidence: klarifikasi pemilik proses No. 7. Pemisahan validasi menu dan aksi finansial digunakan untuk menjaga konsistensi dengan aturan transaksi penerimaan pembayaran.]

### Form Shift

- Seluruh field/parameter pada **Buka Shift**, **Tutup Shift**, dan **Serah Terima Shift** wajib diisi oleh kasir.
- Field pembukaan yang telah disebut: kasir, lokasi, loket, jenis/shift, tanggal, kas awal, dan detail pecahan.
- Field closing yang telah disebut: kas aktual per pecahan dan data yang diperlukan untuk rekonsiliasi.
- Serah terima mencakup identitas kasir pengganti pada loket yang sama.
- Pola pesan minimum yang direkomendasikan: **“{Nama field} wajib diisi.”** Keputusan ini merupakan rekomendasi UX karena teks pesan aktual belum ditetapkan oleh evidence.

## 6. Business Process / Ordered Flow

### BP-001 - Login dan Pembukaan Shift

1. Kasir login; sistem memvalidasi identitas dan hak akses. Hasil: sesi terautentikasi, tanpa pembukaan shift otomatis.
2. Kasir membuka Kasir & Billing lalu Shift Kasir.
3. Kasir memilih lokasi, loket, serta konteks shift.
4. Kasir mengisi seluruh field/parameter pembukaan dan kas awal per pecahan.
5. Sistem menolak input negatif, memperbolehkan nilai 0, dan memastikan satu kasir atau satu loket tidak memiliki lebih dari satu shift `OPEN`.
6. Jika valid, sistem membuat shift berstatus `OPEN`, mencatat waktu buka, dan memberi akses ke aktivitas yang mensyaratkan shift.

### BP-002 - Logout Saat Shift Masih Aktif

1. Shift berada pada status `OPEN`.
2. Kasir logout; sistem mengakhiri sesi.
3. Shift tetap `OPEN` dan tanggung jawab kas tetap berlangsung.
4. Saat login kembali, sistem menghubungkan kasir dengan shift aktif yang masih berlaku sesuai kasir/loket.

### BP-003 - Tutup Shift dan Rekonsiliasi

1. Kasir memulai tutup shift dari status `OPEN`.
2. Kasir mengisi seluruh field/parameter tutup shift dan kas aktual per pecahan.
3. Sistem menghitung `Kas Seharusnya = Kas Awal + Total Penerimaan Tunai - Total Pengeluaran`.
4. Sistem menghitung `Selisih = Kas Aktual - Kas Seharusnya`.
5. Sistem menetapkan `SESUAI`, `KURANG`, atau `LEBIH`.
6. Jika closing berhasil, shift berubah dari `OPEN` menjadi `CLOSED` dan waktu tutup dicatat.

### BP-004 - Closing Tanpa Selisih

1. Rekonsiliasi menghasilkan `SESUAI`.
2. Sistem menetapkan `Tidak Perlu Verifikasi`.
3. Status dianggap selesai dan tidak memblokir pembukaan shift berikutnya.

### BP-005 - Closing dengan Selisih dan Tindak Lanjut

1. Rekonsiliasi menghasilkan `KURANG` atau `LEBIH`.
2. Sistem menetapkan `Menunggu Verifikasi`.
3. Supervisor/kepala kasir memeriksa closing dan menetapkan `Terverifikasi` atau `Perlu Tindak Lanjut`.
4. Untuk `Perlu Tindak Lanjut`, supervisor wajib mengisi catatan.
5. Setelah tindak lanjut selesai, status berubah menjadi `Terverifikasi`; hasil akhirnya dapat tetap memiliki nominal selisih sesuai keputusan tindak lanjut/manajemen.

### BP-006 - Serah Terima Shift

1. Pada pergantian kasir di loket yang sama, kasir menjalankan proses serah terima.
2. Kasir mengisi seluruh field/parameter serah terima, termasuk identitas kasir pengganti.
3. Nama kasir pengganti menjadi pernyataan serah terima pada loket yang sama.
4. Sistem mencatat serah terima dalam audit trail.

Catatan: evidence tidak merinci apakah serah terima selalu menutup shift lama dan membuka shift baru atau memindahkan penanggung jawab pada shift yang sama. Keputusan implementasi ini masih diperlukan.

### BP-007 - Pemeriksaan Sebelum Shift Berikutnya

1. Sistem memeriksa closing sebelumnya untuk kasir/loket terkait.
2. `Terverifikasi` atau `Tidak Perlu Verifikasi` mengizinkan shift berikutnya.
3. `Menunggu Verifikasi` atau `Perlu Tindak Lanjut` memblokir shift berikutnya.
4. Setelah status blokir diselesaikan, kasir dapat membuka shift baru sesuai batas satu shift `OPEN` per kasir atau loket.

## 7. Business Rules, Validation, and Exceptions

| ID | Rule / Validation / Exception | Evidence | Confidence |
|---|---|---|---|
| RULE-001 | Login tidak membuka shift otomatis. | Attachment baseline | High |
| RULE-002 | Shift `OPEN` diperlukan untuk transaksi penerimaan pembayaran. | Attachment baseline; klarifikasi mengikuti artifact | High |
| RULE-003 | Logout tidak menutup shift atau mengakhiri tanggung jawab kas. | Attachment, RULE-004 | High |
| RULE-004 | Status shift hanya `OPEN` dan `CLOSED`; closing berhasil mengubah status menjadi `CLOSED`. | Klarifikasi No. 2; attachment CAP-005 | High |
| RULE-005 | Semua field/parameter Buka Shift, Tutup Shift, dan Serah Terima Shift wajib diisi. | Klarifikasi No. 3 | High |
| RULE-006 | Input nominal negatif ditolak; nilai 0 diperbolehkan. | Attachment, RULE-016 | High |
| RULE-007 | Satu kasir atau satu loket hanya boleh memiliki satu shift `OPEN` pada satu waktu. | Klarifikasi No. 4; attachment RULE-015 | High |
| RULE-008 | Kas aktual saat closing wajib dimasukkan per pecahan. | Attachment, RULE-017 | High |
| RULE-009 | Kas seharusnya dan selisih dihitung dengan rumus pada BP-003. | Attachment, RULE-006 s.d. RULE-007 | High |
| RULE-010 | `SESUAI` menghasilkan `Tidak Perlu Verifikasi`; `KURANG`/`LEBIH` menghasilkan `Menunggu Verifikasi`. | Attachment, RULE-008 | High |
| RULE-011 | `Perlu Tindak Lanjut` mewajibkan catatan supervisor. | Attachment, RULE-009 | High |
| RULE-012 | `Menunggu Verifikasi` dan `Perlu Tindak Lanjut` memblokir shift berikutnya. | Attachment, RULE-010 | High |
| RULE-013 | `Terverifikasi` dan `Tidak Perlu Verifikasi` mengizinkan shift berikutnya. | Attachment, RULE-014 | High |
| RULE-014 | Selisih kas tidak otomatis menjadi denda; keputusan penyelesaian berada pada manajemen. | Attachment, RULE-011 dan RULE-013 | High |
| RULE-015 | Koreksi obat/pelayanan dilakukan petugas medis sebelum billing closed; perubahan oleh kasir memerlukan catatan/verifikasi pihak medis. | Attachment, RULE-019 s.d. RULE-020 | High |
| RULE-016 | Semua proses pembukaan, closing, serah terima, perubahan penting, dan verifikasi harus memiliki audit trail. | Attachment, RULE-012; rekomendasi kontrol | High |
| RULE-017 | Hanya Running Invoice yang mensyaratkan shift `OPEN` pada saat membuka menu. | Klarifikasi No. 7 | High |
| RULE-018 | Invoice & Billing Kasir, Riwayat Pembayaran, dan Petty Cash dapat dibuka tanpa shift `OPEN`. | Klarifikasi No. 7 | High |
| RULE-019 | Walaupun menu dapat dibuka, aksi yang menerima pembayaran atau mengubah posisi kas tetap memerlukan shift `OPEN`, otorisasi, dan audit trail. | Konsolidasi RULE-002 dengan klarifikasi No. 7 | High |

## 8. Data / Input / Output Observed

### Data Pembukaan Shift

- `ShiftId`, `KasirId`, `LoketId`, lokasi, tanggal/`TanggalShift`, jenis/`JenisShift`, kas awal/`OpeningAmount`, `ShiftStatus`, waktu buka.
- Detail pecahan: `DenominationId`, `Nominal`, `Jumlah`, `Subtotal`, dan total kas awal.
- Output: shift `OPEN` dan akses operasional sesuai kewenangan.

### Data Tutup Shift dan Rekonsiliasi

- `ClosingId`, `ShiftId`, `ExpectedAmount`, `ActualAmount`, `DifferenceAmount`.
- Komponen: kas awal, total penerimaan tunai, total pengeluaran, dan kas aktual per pecahan.
- Status rekonsiliasi: `SESUAI`, `KURANG`, `LEBIH`.
- Output: shift `CLOSED`, status verifikasi, kasir penutup, dan waktu tutup.

### Data Verifikasi

- `VerificationStatus`, `VerificationNote`, `VerifiedBy`, `VerifiedDate`.
- Nilai: `Tidak Perlu Verifikasi`, `Menunggu Verifikasi`, `Terverifikasi`, `Perlu Tindak Lanjut`.

### Data Serah Terima

- Shift/loket terkait, kasir yang menyerahkan, kasir pengganti, waktu serah terima, serta seluruh field/parameter pada form.
- Daftar field serah terima selain identitas kasir pengganti belum dirinci dalam evidence.

### Audit Trail Minimum

- Identitas user, peran, timestamp, aksi, objek yang diproses, nilai sebelum/sesudah untuk perubahan, status hasil, serta catatan/otorisasi bila diperlukan.

## 9. Integrations / Dependencies Mentioned

- Autentikasi dan manajemen hak akses.
- Kasir & Billing, khususnya Invoice & Billing Kasir dan Running Invoice.
- Riwayat Pembayaran dan Petty Cash.
- Data lokasi, gedung, lantai, loket, dan unit pelayanan.
- Petugas medis untuk koreksi layanan/obat.
- Supervisor/kepala kasir dan manajemen untuk verifikasi serta penyelesaian selisih.
- Audit trail dan pencatatan aktivitas sistem.

## 10. Rekomendasi Aktivitas Tanpa Shift Aktif

### REK-001 - Prinsip Keputusan

Tanpa shift `OPEN`, sistem **direkomendasikan tetap mengizinkan aktivitas baca dan administratif yang tidak memindahkan uang, tidak membentuk penerimaan/pengeluaran, dan tidak mengubah nilai finansial final**. Sistem harus menolak aksi yang memposting pembayaran, refund, pengeluaran kas, koreksi finansial, settlement, atau perubahan lain yang memengaruhi posisi kas.

### Matriks Rekomendasi

| Aktivitas saat tidak ada shift `OPEN` | Rekomendasi | Kontrol |
|---|---|---|
| Melihat Invoice & Billing Kasir | Izinkan | Sesuai hak akses; read-only bila menyangkut nilai final |
| Melihat Riwayat Pembayaran | Izinkan | Read-only; akses tercatat bila memuat data sensitif |
| Membuka Petty Cash | Izinkan | Melihat daftar/status diperbolehkan |
| Mencatat pengeluaran atau pemasukan Petty Cash | Tolak sampai shift `OPEN` atau gunakan proses kas terpisah yang disetujui | Otorisasi dan audit trail wajib |
| Membuka Running Invoice | Tolak | Arahkan ke Shift Kasir sesuai keputusan pemilik proses |
| Pencarian pasien, pengecekan tagihan, atau persiapan administratif | Izinkan | Tidak boleh memposting transaksi finansial |
| Mencatat pembayaran, deposit, refund, pembatalan finansial, atau settlement | Tolak | Shift `OPEN`, hak akses, dan audit trail wajib |
| Koreksi data klinis/layanan | Batasi sesuai kewenangan | Petugas medis sebagai pemilik koreksi; kasir hanya dengan catatan/verifikasi |

### Dasar Rekomendasi

- Lampiran [Permenkes No. 82 Tahun 2013 tentang SIMRS](https://jdih.kemkes.go.id/storage/documents/pdfs/2013permenkes082.pdf) memisahkan fungsi billing, status pembayaran, riwayat, dan keuangan dalam SIMRS serta mensyaratkan identifikasi user/peran dan pembatasan akses/perubahan kepada pihak berwenang.
- [Permenkes No. 24 Tahun 2022 tentang Rekam Medis](https://jdih.kemkes.go.id/documents/peraturan-menteri-kesehatan-nomor-24-tahun-2022) membedakan hak melihat, menginput, dan memperbaiki data serta menempatkan penetapan hak akses pada kebijakan fasilitas pelayanan kesehatan.
- [Ketentuan SATUSEHAT Data](https://satusehat.kemkes.go.id/data/terms-and-privacy) menerapkan hak akses sesuai fungsi/kewenangan dan audit trail atas akses, penyimpanan, perubahan, serta penghapusan data.

Regulasi tersebut tidak menetapkan desain shift kasir secara rinci. Matriks di atas adalah inferensi kontrol operasional: shift dipakai sebagai batas pertanggungjawaban untuk aksi yang mengubah posisi kas, sedangkan akses baca/administratif tetap tersedia berdasarkan peran.

## 11. Conflicts in Evidence

Tidak ada konflik yang tersisa setelah klarifikasi pemilik proses.

Potensi ambigu antara “menu dapat dibuka tanpa shift” dan “penerimaan pembayaran membutuhkan shift” diselesaikan dengan dua lapis validasi:

1. **Validasi akses menu** mengikuti matriks pada Bagian 5.
2. **Validasi aksi finansial** tetap mewajibkan shift `OPEN` untuk penerimaan pembayaran atau perubahan posisi kas.

## 12. Resolved Decisions dan Remaining Details

### Keputusan yang Diselesaikan

1. Alur logout, closing, rekonsiliasi, verifikasi, tindak lanjut, dan shift berikutnya mengikuti attachment terbaru.
2. Status shift terdiri dari `OPEN` dan `CLOSED`; closing berhasil menghasilkan `CLOSED`.
3. Semua field/parameter pada Buka, Tutup, dan Serah Terima Shift wajib diisi.
4. Satu kasir atau satu loket hanya boleh memiliki satu shift `OPEN`.
5. Otorisasi kasir, supervisor, petugas medis, dan manajemen mengikuti attachment terbaru.
6. Aktivitas tanpa penerimaan pembayaran mengikuti rekomendasi pada Bagian 10.
7. Hanya Running Invoice yang divalidasi shift saat membuka menu; tiga menu lain dapat dibuka tanpa shift.

### Detail yang Masih Perlu Ditetapkan

1. Daftar field lengkap, format, rentang nilai, dan teks pesan error untuk setiap form.
2. Apakah serah terima menutup shift lama dan membuka shift baru atau memindahkan penanggung jawab dalam shift yang sama.
3. Apakah aksi finansial Petty Cash memakai Shift Kasir atau mekanisme kas terpisah.
4. Matriks hak akses rinci per aksi, bukan hanya per menu.

## 13. Source Coverage

| Source | Status | Coverage | Hasil |
|---|---|---|---|
| `Shift Kasir - Updated Artifact(1).md` | Processed new/changed | Seluruh dokumen | Baseline capability, aktor, flow, rules, data, status, otorisasi, dan audit trail dikonsolidasikan. |
| Klarifikasi pemilik proses, 25 September 2026 | Incorporated, non-file | Jawaban 1-7 | Keputusan baru diterapkan pada alur, status, field wajib, aturan, dan matriks menu. |
| Referensi resmi Kementerian Kesehatan | Consulted, external references | Kontrol akses dan audit | Digunakan hanya sebagai basis rekomendasi Bagian 10. |

- ZIP containers: 0
- Evidence files/members total: 1
- Processed new/changed: 1
- Reused from cache: 0
- Unreadable/unsupported: 0
- Supplemental decision records: 1
- External references consulted: 3
- Extraction scope: `/workspace/agent_files/Shift Kasir`
- Workspace mode: preferred `/workspace/agent_files/...`
