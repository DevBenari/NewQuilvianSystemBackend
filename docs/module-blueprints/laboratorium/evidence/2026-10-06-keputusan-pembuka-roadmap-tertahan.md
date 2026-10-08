# Bukti `LAB-EVD-012` — Keputusan pemilik modul untuk membuka roadmap BE/FE yang tertahan

| Field | Value |
|---|---|
| `evidence_id` | `LAB-EVD-012` |
| Menjawab | `DEC-LAB-029`, `DEC-LAB-028`, `DEC-LAB-022`, `DEC-LAB-020`, `DEC-LAB-021`, `DEC-LAB-012`, `LAB-P0-004`/`LAB-OPEN-014`, `DEC-LAB-014`, `DEC-LAB-025`, `LAB-OPEN-029`, `LAB-COORD-011`, `LAB-COORD-015`, `DEC-LAB-016`, `LAB-COORD-018`, serta penggolongan ulang penahan roadmap dan penahan go-live |
| Penulis | Yoga Aji Pratama, pemilik modul |
| Diterima | Sesi 2026-10-06, sebagai **teks tertulis** berjudul *Keputusan untuk Membuka Roadmap FE/BE Laboratorium yang masih tertahan* — 14 butir ditambah dua bagian penutup |
| Bentuk | Teks lengkap, tidak terpotong |
| Klarifikasi | Enam pertanyaan pilihan pada sesi yang sama — bagian B |
| Dicatat pada | `00-interview-decisions.md` — Amendment Pass Putaran 21 |

**Bagian A disalin apa adanya dari teks yang diterima dan tidak boleh disunting.** Tafsirannya ada
pada decision log, bukan di sini. Label `Rekomendasi status` di dalam teks adalah bagian dari teks
pemilik modul; status resmi tiap keputusan tercatat pada decision log.

---

## A. Teks keputusan yang diterima

> # Keputusan untuk Membuka Roadmap FE/BE Laboratorium yang masih tertahan
>
> ## 1. DEC-LAB-029 — Lokasi Specimen per Container
>
> **Keputusan**
>
> Satu container specimen hanya boleh mewakili **satu lokasi anatomi/sumber specimen**.
>
> Apabila specimen berasal dari lebih dari satu lokasi, maka setiap lokasi harus dibuat sebagai container terpisah dan mendapatkan identitas/nomor container masing-masing.
>
> Satu order laboratorium tetap boleh memiliki banyak container.
>
> **Contoh:**
>
> Order:
> `LAB-001`
>
> Container:
> - Container 1 — Lambung
> - Container 2 — Duodenum
> - Container 3 — Ileum
>
> Tidak diperbolehkan satu container memiliki beberapa lokasi anatomi yang berbeda.
>
> **Dampak keputusan:**
>
> `S2b-1` dapat dibentuk roadmap BE/FE.
>
> **Rekomendasi status:**
>
> `APPROVE`
>
> ---
>
> ## 2. DEC-LAB-022 — Nomor Sitologi dan FNAB
>
> **Keputusan**
>
> Sitologi dan FNAB menggunakan **seri nomor pemeriksaan operasional masing-masing**, terpisah dari nomor Histopatologi/PA.
>
> Namun ID order utama pada sistem tetap menggunakan satu identitas global.
>
> Contoh konsep:
>
> - Histopatologi → seri Histopatologi
> - Sitologi → seri Sitologi
> - FNAB → seri FNAB
>
> Format prefix dan nomor tidak perlu di-hardcode dan harus dapat dikonfigurasi.
>
> Jika bukti dokumen cetak lama nantinya menunjukkan proses berbeda, Kepala Instalasi dapat memberikan keputusan eksplisit untuk mempertahankan pola lama.
>
> **Catatan:**
>
> `LAB-OPEN-039` saat ini masih meminta bukti cetak Sitologi/FNAB. Keputusan eksplisit dari owner dapat digunakan untuk menggantikan ketergantungan terhadap pola lama apabila memang proses V2 sengaja didesain baru.
>
> **Dampak keputusan:**
>
> `S2b-2` dapat dilanjutkan ke roadmap BE/FE.
>
> **Rekomendasi status:**
>
> `APPROVE WITH OWNER CONFIRMATION`
>
> ---
>
> ## 3. DEC-LAB-028 — Pencatatan Fiksasi Specimen
>
> **Keputusan**
>
> Fiksasi dicatat **per container specimen** apabila pemeriksaan membutuhkan fiksasi.
>
> Minimal sistem menyimpan:
>
> - jenis cairan/metode fiksasi;
> - tanggal dan waktu mulai fiksasi;
> - petugas yang melakukan/mencatat;
> - container terkait;
> - catatan bila diperlukan.
>
> Jenis fiksasi menggunakan controlled master, bukan free text sebagai pilihan utama.
>
> Jika specimen tidak membutuhkan fiksasi, sistem dapat mencatat `Tidak Memerlukan Fiksasi`.
>
> Petugas yang melakukan atau menerima informasi fiksasi bertanggung jawab mencatatnya.
>
> **Dampak keputusan:**
>
> `S2b-3` dapat dibuat roadmap BE/FE.
>
> **Rekomendasi status:**
>
> `APPROVE`
>
> ---
>
> ## 4. DEC-LAB-020 — Hasil Sementara Mikrobiologi
>
> **Keputusan**
>
> Hasil Mikrobiologi dengan status **Sementara / Preliminary** boleh divalidasi dan dirilis kepada pihak klinis.
>
> Tetapi hasil harus jelas diberi status:
>
> `HASIL SEMENTARA`
>
> Ketika hasil Definitif tersedia, hasil Definitif menjadi tahap berikutnya dari pemeriksaan yang sama dan **bukan dianggap sebagai koreksi S6**.
>
> Lifecycle:
>
> `Draft → Sementara → Definitif`
>
> S6 hanya digunakan jika hasil yang sebelumnya diterbitkan ternyata salah dan perlu dikoreksi.
>
> Semua versi hasil tetap tersimpan dalam audit trail.
>
> **Dampak keputusan:**
>
> Roadmap `S4d-2` dapat dibuat.
>
> **Rekomendasi status:**
>
> `APPROVE`
>
> ---
>
> ## 5. DEC-LAB-021 — Validasi Hasil Patologi Anatomi
>
> **Keputusan**
>
> Gunakan mekanisme khusus PA:
>
> **Dokter Sp.PA yang membuat laporan diperbolehkan melakukan Validasi terhadap laporannya sendiri.**
>
> Namun tahap:
>
> `RELEASE`
>
> dilakukan oleh **dokter lain yang memiliki kewenangan release PA**.
>
> Dengan demikian prinsip four-eyes diterapkan pada tahap release, bukan pada penulisan laporan.
>
> Alur:
>
> `Draft oleh Sp.PA A`
> →
> `Validasi oleh Sp.PA A`
> →
> `Release oleh Sp.PA B`
>
> Tidak menggunakan analis sebagai validator/releaser PA.
>
> **Dampak keputusan:**
>
> Seluruh desain `S4e` dapat dibentuk menjadi roadmap BE/FE.
>
> **Rekomendasi status:**
>
> `APPROVE OPTION C`
>
> ---
>
> ## 6. DEC-LAB-012 — Siapa yang Menyetujui Perubahan Nilai Kritis
>
> **Keputusan**
>
> Pemegang persetujuan akhir terhadap perubahan batas/nilai kritis laboratorium adalah:
>
> **Kepala Instalasi Laboratorium.**
>
> Kepala Instalasi dapat memberikan delegasi kepada dokter penanggung jawab disiplin tertentu apabila delegasi tersebut tercatat secara resmi.
>
> Contoh:
>
> - Patologi Klinik → dokter Sp.PK yang ditunjuk;
> - Mikrobiologi → dokter yang memiliki kewenangan Mikrobiologi;
> - Patologi Anatomi → dokter Sp.PA yang ditunjuk.
>
> Perubahan nilai kritis harus mempunyai:
>
> - nilai lama;
> - nilai baru;
> - alasan;
> - pengusul;
> - approver;
> - waktu persetujuan;
> - tanggal mulai berlaku;
> - version history.
>
> Nilai baru tidak aktif sebelum disetujui.
>
> **Dampak keputusan:**
>
> Bagian desain S5 yang tertahan oleh `DEC-LAB-012` dapat dilanjutkan.
>
> **Rekomendasi status:**
>
> `APPROVE`
>
> ---
>
> # 7. LAB-P0-004 / LAB-OPEN-014 — Flow Nilai Kritis
>
> Untuk membuka roadmap S5 sepenuhnya, saya putuskan berikut.
>
> Ketika hasil memenuhi aturan nilai kritis:
>
> `Hasil Pemeriksaan`
> →
> `Critical Rule Triggered`
> →
> `Status Critical`
> →
> `Notifikasi`
> →
> `Acknowledgement`
> →
> `Escalation jika belum direspons`
>
> Penerima utama:
>
> 1. dokter pemesan; dan
> 2. DPJP/unit pelayanan pasien apabila pasien masih berada dalam episode perawatan aktif.
>
> Jika penerima utama belum memberikan acknowledgement sesuai batas waktu yang ditentukan kebijakan RS, sistem melakukan escalation.
>
> Batas waktu tidak sebaiknya di-hardcode dalam source code.
>
> Buat sebagai konfigurasi:
>
> `Critical Notification SLA`
>
> sehingga dapat berbeda menurut disiplin atau kebijakan rumah sakit.
>
> Sistem wajib menyimpan:
>
> - siapa yang dihubungi;
> - waktu pengiriman;
> - channel;
> - waktu acknowledgement;
> - siapa yang melakukan acknowledgement;
> - percobaan notifikasi;
> - escalation;
> - hasil escalation.
>
> Untuk Mikrobiologi dan PA, jenis organisme/kondisi/nilai kritis dikelola sebagai master rule oleh disiplin terkait.
>
> Konten klinis master tersebut tetap harus diberikan dokter terkait sebelum go-live.
>
> **Dampak:**
>
> Roadmap teknis S5 dapat dibuat meskipun daftar klinis final masih menjadi konfigurasi sebelum release.
>
> **Rekomendasi status:**
>
> `APPROVE ARCHITECTURE — CLINICAL CONTENT REQUIRED BEFORE GO-LIVE`
>
> ---
>
> ## 8. DEC-LAB-014 — Koreksi Hasil Setelah Release
>
> ### Kewenangan
>
> **Keputusan**
>
> Hasil yang sudah RELEASED dapat dikoreksi.
>
> Pengajuan koreksi dilakukan oleh petugas/dokter yang memiliki kewenangan validasi.
>
> Persetujuan koreksi dilakukan oleh **user lain yang memiliki kewenangan release pada disiplin tersebut**.
>
> Jadi tidak perlu selalu Kepala Instalasi, tetapi tidak diperbolehkan satu user melakukan seluruh proses sendiri.
>
> Alur:
>
> `Released Result`
> →
> `Request Correction`
> →
> `Reason Required`
> →
> `Second Authorized Approval`
> →
> `Corrected Version`
> →
> `Release Corrected Result`
>
> ### Pihak yang diberi tahu
>
> Koreksi harus diberitahukan kepada:
>
> - dokter pemesan; dan
> - DPJP/unit pelayanan aktif.
>
> Apabila hasil lama sudah pernah dikirim kepada pasien, hasil lama diberi status:
>
> `Superseded`
>
> dan hasil yang telah dikoreksi menjadi versi aktif.
>
> ### Batas waktu
>
> Tidak direkomendasikan adanya batas waktu absolut yang membuat hasil lama tidak bisa dikoreksi.
>
> Kesalahan klinis yang ditemukan kemudian tetap harus dapat diperbaiki.
>
> Sebagai gantinya gunakan:
>
> - alasan koreksi wajib;
> - approval;
> - versioning;
> - immutable audit trail.
>
> **Dampak keputusan:**
>
> Roadmap `S6` dapat dibentuk.
>
> **Rekomendasi status:**
>
> `APPROVE`
>
> ---
>
> ## 9. DEC-LAB-025 — Delapan Laporan S16b
>
> Untuk keputusan ini **jangan mengarang delapan jenis laporan**.
>
> keputusan:
>
> Tiga laporan yang sudah memiliki definisi pada `S16a` tetap masuk roadmap saat ini.
>
> Delapan laporan `S16b` dipindahkan menjadi:
>
> `PHASE 2 — DEFERRED PENDING REPORT CATALOG`
>
> Setiap laporan baru hanya dapat dibentuk menjadi BE/FE Task setelah owner memberikan:
>
> - nama laporan;
> - tujuan;
> - sumber data;
> - definisi setiap kolom;
> - formula;
> - periode;
> - tanggal mulai/akhir;
> - perlakuan pembatalan;
> - filter;
> - role pembaca;
> - format export.
>
> Dengan keputusan ini, S16b **tidak lagi menahan roadmap Laboratorium lainnya**.
>
> Begitu definisi laporan diberikan, laporan dapat ditambahkan sebagai roadmap baru.
>
> **Dampak keputusan:**
>
> Roadmap utama dapat berjalan tanpa menunggu delapan laporan yang belum terdefinisi.
>
> **Rekomendasi status:**
>
> `DEFER S16b TO PHASE 2`
>
> ---
>
> # 10. LAB-OPEN-029 — Persetujuan Sebelum Hasil Dikirim ke Pasien
>
> Istilah **“Profesor” tidak disarankan menjadi permission sistem**, karena merupakan gelar/individu dan bukan kewenangan aplikasi.
>
> keputusan:
>
> Gunakan permission berbasis role/clinical privilege, misalnya secara konsep:
>
> `LabResultPatientDeliveryApprover`
>
> Kewenangan diberikan kepada orang yang ditunjuk Kepala Instalasi.
>
> Untuk hasil normal yang sudah:
>
> `RELEASED`
>
> tidak diperlukan approval akademik tambahan hanya karena seseorang memiliki gelar Profesor.
>
> Untuk kategori pemeriksaan tertentu yang dianggap sensitif dan membutuhkan persetujuan tambahan, kebutuhan second approval dapat dikonfigurasi.
>
> Alur normal:
>
> `Validated`
> →
> `Released`
> →
> `Eligible for Patient Delivery`
> →
> `Send`
>
> Semua aksi pengiriman dicatat pada audit trail.
>
> **Dampak keputusan:**
>
> Desain S17 tidak lagi bergantung kepada identitas seseorang yang disebut “Profesor”.
>
> **Rekomendasi status:**
>
> `REPLACE PERSON/TITLE-BASED APPROVAL WITH ROLE-BASED AUTHORIZATION`
>
> ---
>
> ## 11. LAB-COORD-011 — PDF dan Pengiriman WhatsApp
>
> keputusan arsitektur:
>
> Hasil laboratorium FINAL/RELEASED menghasilkan dokumen PDF yang memiliki:
>
> - nomor dokumen;
> - versi;
> - waktu penerbitan;
> - dokter releaser;
> - QR/verifikasi;
> - hubungan ke order/result.
>
> Pengiriman WhatsApp menggunakan service/platform notification terpisah.
>
> Untuk perlindungan data pasien, rekomendasi utama adalah mengirim:
>
> **secure link menuju hasil**
>
> daripada URL file publik permanen.
>
> PDF tetap dapat diunduh dari Quilvian sesuai hak akses.
>
> Kegagalan WhatsApp tidak boleh membatalkan status RELEASED hasil.
>
> Gunakan status delivery terpisah, misalnya:
>
> `Pending → Sent → Delivered / Failed`
>
> **Dampak:**
>
> Kontrak FE/BE S17 dapat dirancang sementara adapter/provider WhatsApp dikerjakan platform.
>
> ---
>
> ## 12. LAB-COORD-015 — Verifikasi QR Tanpa Login
>
> **Keputusan**
>
> Halaman verifikasi QR diperbolehkan tanpa login, tetapi QR **tidak membawa Patient ID, Order ID berurutan, atau URL file publik secara langsung**.
>
> Gunakan opaque/signed verification token.
>
> Halaman publik hanya menampilkan informasi minimum untuk memverifikasi keaslian dokumen.
>
> Contoh:
>
> - status dokumen valid/tidak valid;
> - nomor hasil;
> - tanggal penerbitan;
> - fasilitas penerbit;
> - nama pemeriksaan secara terbatas bila disetujui kebijakan privasi.
>
> Jangan menyediakan pencarian pasien dari halaman publik.
>
> Token harus dapat dicabut apabila hasil di-supersede/dikoreksi.
>
> **Dampak:**
>
> Bagian public verification pada S17 dapat dibuat roadmap setelah security/platform menyetujui model ini.
>
> **Rekomendasi status:**
>
> `APPROVE SIGNED PUBLIC VERIFICATION TOKEN`
>
> ---
>
> # 13. DEC-LAB-016 — Penyimpanan File PA dan Hasil Laboratorium Eksternal
>
> **Keputusan**
>
> File klinis tidak disimpan sebagai file publik pada web server.
>
> Gunakan **private object/file storage**.
>
> Database menyimpan metadata dan relasi dokumen, sedangkan binary file berada pada storage privat.
>
> Akses dilakukan melalui:
>
> - endpoint terautentikasi; atau
> - signed URL sementara dengan masa berlaku terbatas.
>
> Wajib memiliki:
>
> - role/permission;
> - audit akses;
> - ukuran/type file validation;
> - hubungan dengan pasien;
> - encounter;
> - order;
> - pemeriksaan;
> - uploader;
> - waktu upload;
> - version/status.
>
> File lama tidak dihapus apabila terjadi koreksi. File diberi status:
>
> `Superseded / Invalidated`
>
> kemudian file yang benar menjadi versi aktif.
>
> **Dampak keputusan:**
>
> File portion pada S18 dan kebutuhan image/file PA dapat dibuat roadmap.
>
> **Rekomendasi status:**
>
> `APPROVE PRIVATE CLINICAL DOCUMENT STORAGE`
>
> ---
>
> # 14. LAB-COORD-018 — Verified dan Approved untuk Dokumen Lab Eksternal
>
> Untuk menghilangkan ambigu, keputusannya:
>
> ### Verified
>
> `Verified` berarti:
>
> petugas memastikan bahwa dokumen:
>
> - dapat dibaca;
> - benar milik pasien tersebut;
> - benar terkait pemeriksaan/order/kunjungan yang dimaksud;
> - berasal dari sumber laboratorium eksternal yang dicatat.
>
> Verified **bukan** persetujuan klinis terhadap isi hasil.
>
> ### Approved
>
> `Approved` berarti:
>
> dokumen/hasil telah ditinjau dan diterima secara klinis oleh tenaga medis yang memiliki kewenangan.
>
> Sehingga:
>
> `Uploaded`
> →
> `Verified`
> →
> `Approved`
>
> Lab bertanggung jawab terhadap upload dan verifikasi administratif/lab.
>
> Clinical Management/dokter yang berwenang bertanggung jawab terhadap penerimaan klinis jika memang approval klinis diperlukan.
>
> **Dampak:**
>
> Boundary FE/BE S18 menjadi jelas.
>
> **Rekomendasi status:**
>
> `APPROVE WITH CLINICAL MANAGEMENT CONFIRMATION`
>
> ---
>
> # Keputusan yang Tidak Perlu Menahan Pembuatan Roadmap
>
> Item berikut jangan dimasukkan lagi sebagai alasan roadmap tidak dapat dibuat:
>
> | ID | Kondisi |
> |---|---|
> | `DEC-LAB-011` | Sudah ditutup |
> | `DEC-LAB-018` | Sudah ditutup |
> | `LAB-CONFLICT-014` | Sudah diputuskan |
> | `DEC-LAB-023` | Sudah ditutup |
> | `DEC-LAB-024` | Sudah ditutup |
> | `DEC-LAB-026` | Sudah ditutup |
> | `DEC-LAB-027` | Sudah ditutup |
> | `LAB-OPEN-025` | Bukan blocker Kiosk Encounter |
> | `LAB-OPEN-026` | Bukan blocker Kiosk Encounter |
>
> `DEC-LAB-017`, `LAB-OPEN-044`, `LAB-OPEN-045`, `UNK-P14-03`, kebutuhan validator kedua, serta privilege HR sebaiknya tetap dipantau, tetapi dikategorikan sebagai **release/go-live blockers**, bukan blocker untuk menyusun roadmap teknis.
>
> # Urutan Keputusan yang Direkomendasikan
>
> Untuk membuka roadmap paling cepat:
>
> `DEC-LAB-029`
> →
> `DEC-LAB-028`
> →
> `DEC-LAB-022`
> →
> `DEC-LAB-020`
> →
> `DEC-LAB-021`
> →
> `DEC-LAB-012`
> →
> `DEC-LAB-014`
> →
> `LAB-OPEN-029`
> →
> `DEC-LAB-016 / LAB-COORD-018`
>
> Sedangkan `DEC-LAB-025` direkomendasikan **defer ke Phase 2**, sehingga tidak perlu menahan roadmap yang sekarang.

---

## B. Klarifikasi pada sesi yang sama

Enam pertanyaan diajukan karena keputusan di atas bersinggungan dengan aturan yang sudah terkunci,
atau karena satu penahan desain (`DEC-LAB-019`) tidak disebut dalam teks. Setiap pertanyaan memuat
pilihan dan rekomendasi; **keenamnya dijawab dengan pilihan yang direkomendasikan.**

| No | Menyentuh | Pertanyaan | Jawaban pemilik modul |
|---:|---|---|---|
| Q1 | `DEC-LAB-012` lawan BR-19 (`LAB-DEC-023`) | Bolehkah Kepala Instalasi menyetujui usulan perubahan nilai kritis yang ia ajukan sendiri? | **Pengusul ≠ penyetuju.** Bila Kepala Instalasi mengusulkan, penyetujunya dokter disiplin penerima delegasi resmi; sistem menolak menyetujui usulan sendiri |
| Q2 | Butir 7 lawan `LAB-DEC-004`, `LAB-DEC-151` | Apa yang dihitung sebagai *Acknowledgement*? | **Acknowledgement = laporan baca ulang `LAB-DEC-004`.** Eskalasi berhenti hanya bila kelima isian tercatat; klik "dibaca" di aplikasi dan balasan WhatsApp hanya bukti komunikasi |
| Q3 | `DEC-LAB-019` — tidak ada pada teks | Hasil terrilis yang ternyata milik pasien lain: dibatalkan atau ditarik lewat koreksi? | **Selalu lewat koreksi `S6`.** Pembatalan sesudah rilis tetap ditolak; `S6` punya dua jenis koreksi — ganti nilai dan tarik hasil |
| Q4 | Butir 8 dan butir 11 | Bagaimana versi koreksi sampai ke pasien yang sudah menerima versi lama? | **Antrean kirim otomatis.** Sistem membuat tugas kirim `Pending`; pengiriman tetap lewat gerbang `S17`; token QR dokumen lama dicabut |
| Q5 | Butir 4 lawan `LAB-DEC-154`, `LAB-DEC-139` | Bolehkah order sudah *Selesai* bila hasil Mikrobiologinya baru dirilis `Sementara`? | **Belum Selesai** sampai setiap pemeriksaan Mikrobiologi dirilis `Definitif` atau dirilis tanpa kualifikasi |
| Q6 | Butir 4 lawan `LAB-DEC-163` | Saat `Definitif` dirilis sesudah `Sementara` terrilis, apakah dokter pemesan diberi tahu? | **Ya, hanya pada kasus itu.** `LAB-DEC-163` diamandemen di satu titik ini |
