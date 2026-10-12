# Flowchart — Alur utama `episode-rawat-inap`

| Field | Nilai |
|---|---|
| Sub-modul | `episode-rawat-inap` |
| Kontrak | Mengikuti set pada manifest sub-modul; Bed Management draft, last_changed_in 0.12.0. Riwayat metadata: Bagian 0: `0.11.0` amandemen Alur Admisi Pendaftaran (`approved` 10 Oktober 2026, `RWI-DEC-267` s.d. `RWI-DEC-273`). Bagian 1–2: `0.10.0` (`approved`, `RWI-DEC-221`). Bagian 3: `0.11.0` — `approved` 2026-10-08 (`RWI-DEC-265`), Workspace PPRI |
| Lahir | Finishing Rawat Inap ★ 1 Oktober 2026; diamandemen 10 Oktober 2026 untuk menyerap alur admisi pendaftaran terpadu |
| Cakupan file ini | Alur admisi pendaftaran pasien baru & lama (Bagian 0), jalur pasien operasi dari bangsal (Bagian 1–2), dan penerimaan pasien di Workspace PPRI (Bagian 3) |

---

## 0. Alur Admisi Pendaftaran Pasien Rawat Inap ★ Amandemen Revisi Bisnis (`RWI-DEC-267` s.d. `RWI-DEC-273`)

Menyerap 5 poin revisi tim analisis bisnis:
1. Batasan Nomor HP Kontak Darurat maksimal 13 digit angka (`RWI-DEC-267`).
2. Dropdown Unit Tujuan di Langkah Dokter disembunyikan dari UI dan diikat default rawat inap (`RWI-DEC-268`).
3. Form input Surat Persetujuan Rawat Inap & TTD Digital disisipkan sebagai Langkah Mandiri (*Dedicated Step*) sebelum cetak; menu General Consent PPRI berstatus *read-only / print-ready* (`RWI-DEC-272`, `RWI-AC-395`).
4. Langkah 1 Pasien Baru: Jenis Kunjungan (Umum & Rujukan ringkas tekstual tanpa upload fisik, `RWI-DEC-269`).
5. Langkah 2 Pasien Baru: Kategori Pasien disaring menjadi tepat 3 opsi (Umum, Bayi Baru Lahir, Pegawai, `RWI-DEC-270`).
6. Pasien Lama: Pencarian dan verifikasi disatukan ke dalam 1 layar split kanan-kiri (`RWI-DEC-271`).
7. Invariant Hukum Medis: Bayi Baru Lahir mengunci penanda tangan wajib Orang Tua / Wali (`RWI-DEC-273`).

```mermaid
flowchart TD
    subgraph pendaftaran[Loket Admisi: Pendaftaran & Identifikasi]
        Start([Pasien Tiba di Loket Admisi]) --> Tipe{Tipe Pendaftaran?}
        Tipe -->|Pasien Baru| PB1[1. Jenis Kunjungan: Umum / Rujukan Ringkas]
        PB1 --> PB2[2. Kategori Pasien: Umum / Bayi Baru Lahir / Pegawai]
        PB2 --> PB3[3. Form Pasien Baru: Scan KTP & Kontak Darurat max 13 digit]
        PB3 -->|Simpan Pasien| Bayar[4. Pembayaran: Penjamin & Kelas]

        Tipe -->|Pasien Lama| PL1[1. Cari & Verifikasi Pasien: Kanan Cari, Kiri Hasil]
        PL1 --> PL2[2. Jenis Kunjungan & Kategori Pasien Lama]
        PL2 --> BayarPL[3. Pembayaran: Penjamin & Kelas]
    end

    subgraph persiapan[Penetapan Episode & Bed]
        Bayar --> Dep[5. Deposit / Uang Muka]
        BayarPL --> DepPL[4. Deposit / Uang Muka]
        Dep --> Dok[6. Dokter: DPJP & Isolasi<br/>Unit Tujuan Auto Ranap]
        DepPL --> DokPL[5. Dokter: DPJP & Isolasi<br/>Unit Tujuan Auto Ranap]
        Dok -->|Titik Tulis 1: Encounter & Episode Draft| Bed[7. Pilih Bed Tersedia]
        DokPL -->|Titik Tulis 1: Encounter & Episode Draft| BedPL[6. Pilih Bed Tersedia]
        Bed --> Book[8. Booking Bed: Kunci Bed 2 Jam]
        BedPL --> BookPL[7. Booking Bed: Kunci Bed 2 Jam]
        Book -->|Titik Tulis 2| Konf[9. Konfirmasi Admisi]
        BookPL -->|Titik Tulis 2| KonfPL[8. Konfirmasi Admisi]
    end

    subgraph persetujuan[Persetujuan & Tanda Tangan Digital]
        Konf -->|Titik Tulis 3| Consent[10. Form Persetujuan & TTD Digital<br/>Stylus/Touch Pad - Invariant Bayi]
        KonfPL -->|Titik Tulis 3| ConsentPL[9. Form Persetujuan & TTD Digital<br/>Stylus/Touch Pad - Invariant Bayi]
        Consent -->|Titik Tulis 4: Simpan Form & Citra TTD| PrintConsent[11. Cetak Lembar Persetujuan Ranap]
        ConsentPL -->|Titik Tulis 4: Simpan Form & Citra TTD| PrintConsentPL[10. Cetak Lembar Persetujuan Ranap]
        PrintConsent --> Card[12. Cetak Kartu Berobat Pasien Baru]
        Card --> SelesaiPB([Admisi Selesai: Menuju Papan Bed])
        PrintConsentPL --> SelesaiPL([Admisi Selesai: Menuju Papan Bed])
    end
```

| Langkah | Jalur PB | Jalur PL | Masukan Utama | Keluaran / Simpanan | Validasi / Aturan Keras |
|---|:---:|:---:|---|---|---|
| Jenis Kunjungan | 1 | 2 (gabung) | Opsi Umum vs Rujukan | State kunjungan, data rujukan | Bila Rujukan: wajib 5 data teks (No, Tgl/Jam, Faskes, Dokter, Diagnosa) (`RWI-DEC-269`) |
| Kategori Pasien | 2 | 2 (gabung) | 3 Opsi: Umum, Bayi, Pegawai | State kategori, penaut episode ibu | Opsi Ibu, Anak, Korporat dinonaktifkan (`RWI-DEC-270`). Bayi wajib episode ibu |
| Pendaftaran Pasien Baru | 3 | – | Identitas & Kontak Darurat | Entitas `MstPatient` | No HP Kontak Darurat numerik maksimal 13 digit angka (`RWI-DEC-267`) |
| Cari & Verifikasi | – | 1 | Input No. RM / NIK (Kanan) | Pasien terpilih di panel kiri | Penyatuan 1 layar split layout kanan-kiri (`RWI-DEC-271`) |
| Pembayaran & Deposit | 4–5 | 3–4 | Penjamin, kelas, uang muka | State alur pembayaran & deposit | Nominal deposit ditahan di klien; kekurangan tidak memblokir admisi (`RWI-DEC-095`) |
| Dokter (DPJP & Isolasi) | 6 | 5 | DPJP terpilih, sakelar isolasi | **Titik Tulis 1**: Kunjungan & Episode `Draft` | Unit Tujuan disembunyikan dari UI, default ranap dikirim otomatis (`RWI-DEC-268`) |
| Pemilihan & Booking Bed | 7–8 | 6–7 | Tempat tidur tersedia | **Titik Tulis 2**: `InpBedReservation` aktif | Bed terkunci `Reserved` 2 jam |
| Konfirmasi Admisi | 9 | 8 | Catatan admisi akhir | **Titik Tulis 3**: Koreksi episode | Kunci data admisi |
| Form Persetujuan & TTD Digital | 10 | 9 | Form persetujuan, kanvas TTD | **Titik Tulis 4**: Form & citra PNG TTD | *Dedicated step* (`RWI-DEC-272`). Bayi Baru Lahir wajib Orang Tua/Wali (`RWI-DEC-273`) |
| Cetak Persetujuan & Kartu | 11–12 | 10 | Pratinjau dokumen ber-TTD | Cetak fisik dokumen & kartu | Berkas siap print di Workspace PPRI tanpa input ulang (`RWI-AC-395`) |

---

## 1. Pasien rawat inap menjalani operasi

```mermaid
flowchart TD
    subgraph dokter[Dokter bangsal]
        A([Pasien dirawat dan perlu operasi]) --> B[Pesan tindakan operasi]
    end
    subgraph bangsal[Perawat bangsal]
        B --> C[Pesan ruang bedah dari tindakan itu]
        C --> D[Isi dan kirim catatan pra-operasi]
    end
    subgraph ok[Kamar Operasi]
        C --> E[Jadwalkan pesanan]
        D --> F[Konfirmasi catatan pra-operasi]
        E --> G[Kasus siap]
        F --> G
        G --> H[Operasi dan kamar pulih]
        H --> I[Kirim serah terima ke unit tujuan]
    end
    subgraph tujuan[Perawat unit tujuan]
        I --> J[Terima serah terima di bed unit itu]
    end
    subgraph hasil[Hasil]
        J --> K[Kasus operasi selesai]
        K --> L[Biaya operasi masuk invoice]
        K --> M([Ringkasan operasi terbaca di bangsal])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Pesan tindakan operasi | Dokter | Tindakan, dokter operator | Order tindakan aktif | — |
| Pesan ruang bedah | Perawat atau dokter bangsal | Satu order tindakan, tanggal, anestesi, sisi tubuh | Kasus OK "Diminta" | Tanpa order: minta dokter memesan tindakan (`01`) |
| Kirim pra-operasi | Perawat bangsal | Checklist, penandaan, tanda vital terbaru | Catatan terkirim | Tanda vital belum ada: catat dulu (`01`) |
| Jadwalkan | Petugas OK | Pesanan | Kasus "Terjadwal" | OK menolak pesanan (`01`) |
| Konfirmasi pra-operasi | Perawat OK, akun lain | Catatan terkirim | Catatan terkonfirmasi | Butir kurang atau sisi berbeda (`01`) |
| Operasi dan kamar pulih | Tim OK | — | Laporan operasi final, keputusan kamar pulih | Ditunda: pra-operasi perlu diperbarui (`01`) |
| Terima serah terima | Perawat unit tujuan | Serah terima | Diterima | Pasien belum di bed unit tujuan (`02`) |
| Biaya operasi | Sistem | Kasus selesai | Tindakan, anestesi, sewa kamar, bahan di invoice | Tarif belum ada (`02`) |

## 2. Pasien operasi dari poliklinik yang perlu dirawat

```mermaid
flowchart TD
    subgraph ok[Kamar Operasi]
        A([Operasi elektif pasien poliklinik selesai]) --> B[Kamar pulih memutuskan rawat inap]
    end
    subgraph admisi[Petugas admisi]
        B --> C[Pasien muncul di daftar permintaan admisi]
        C --> D[Selesaikan admisi berlangkah]
    end
    subgraph bangsal[Perawat unit tujuan]
        D --> E[Pasien menempati bed]
        E --> F[Terima serah terima dari OK]
    end
    F --> G([Kasus operasi selesai, episode rawat inap berjalan])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Keputusan kamar pulih | Tim kamar pulih | Pasien tanpa episode rawat inap | Permintaan admisi | Pasien sudah dirawat: serah terima biasa (`03`) |
| Admisi | Petugas admisi | Permintaan, penjamin, kelas, DPJP, deposit, bed | Episode rawat inap | Permintaan sudah dibatalkan OK (`03`) |
| Terima serah terima | Perawat unit tujuan | Bed ditempati | Kasus selesai | Belum di bed: tombol terima terkunci (`02`) |

Rincian: `01-pemesanan-dan-pra-operasi.md`, `02-serah-terima-dan-biaya-operasi.md`, `03-admisi-dari-kamar-pulih.md`, `04-serah-terima-transfer.md` (`P2`).

---

## 3. Penerimaan pasien di Workspace PPRI ★ kontrak `0.11.0` (`approved` 8 Oktober 2026)

Jalur normal untuk pasien penjamin asuransi dengan kekurangan deposit, tanpa rencana operasi (`RWI-DEC-234`: enam dokumen wajib). Jalur gagal ada di `05` s.d. `08`.

```mermaid
flowchart TD
    subgraph admisi[Petugas admisi]
        A([Admisi dikonfirmasi & Persetujuan Ber-TTD Digital Selesai]) --> B[Buka Workspace PPRI dari Detail Episode]
        B --> C[Cetak gelang]
        C --> D[Cetak General Consent Ber-TTD Digital: Print-Ready Tanpa Re-Input]
        D --> E[Selesaikan Pelunasan Deposit, Selisih Biaya, dan Nilai Kepercayaan]
        E --> F[Cetak IPD]
        F --> G[Isi dan kunci Serah Terima, tanda tangan Admission]
    end
    subgraph cro[CRO]
        G --> H[Tanda tangan Serah Terima]
    end
    subgraph perawat[Perawat ruangan]
        H --> I[Pasien menempati bed]
        I --> J[Tanda tangan Serah Terima sebagai penerima]
    end
    J --> K([Kelengkapan dokumen admisi penuh; peringatan di Detail Episode hilang])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Buka Workspace PPRI | Petugas admisi | Episode `Admitted`; hak baca dokumen admisi | Header pasien, sembilan menu, kelengkapan "0 dari 6" | Data pasien gagal dimuat (`05`) |
| Cetak gelang | Petugas admisi | Data pasien | Gelang terpasang; catatan cetak | Cetak ulang beralasan (`07`) |
| General Consent | Petugas admisi | Berkas persetujuan admisi ber-TTD digital | Lembar cetak persetujuan lengkap (*print-ready* tanpa re-input, `RWI-AC-395`) | Berkas belum ditandatangani saat admisi: buka kembali form persetujuan |
| Dokumen bertanda tangan | Petugas admisi, keluarga | Isian formulir V1 | Dokumen `Completed` | Siklus dan koreksi (`05`), deposit (`08`) |
| Cetak IPD | Petugas admisi | Data terangkai | Lembar IPD; bagian tanpa sumber diisi tangan | Data wajib gagal dimuat (`07`) |
| Serah Terima | Admisi, CRO, perawat | Butir dari master; pasien di bed | Serah Terima `Completed` | Butir kurang atau pasien belum di bed (`06`) |

Kelengkapan dokumen admisi **tidak menahan** penempatan, perawatan, transfer, keputusan pulang, maupun keluar ruangan (`RWI-DEC-234`). Rincian: `05-workspace-ppri-siklus-dokumen.md`, `06-serah-terima-pasien-baru.md`, `07-gelang-label-dan-ipd.md`, `08-pelunasan-deposit.md`.

## 4. Amandemen Bed Management — 10 Oktober 2026

**Status: draft — Amandemen Bed Management, 10 Oktober 2026.** Set kontrak mengikuti `blueprint-manifest.md`; `last_changed_in: 0.12.0`. Owner produk/domain/API: Muhammad Hamzah (RWI-DEC-061); frontend: pengembang dalam batas RWI-DEC-292; keamanan/privasi: OPEN. `approved_by: null`, `approved_at: null` untuk amandemen ini.

Masukan: decision log revision **46**, RWI-DEC-274–294 dan RWI-AC-396–426; gate revision **1.12**, **BM-RCG-20261010-01**, enam BM-CG siap untuk desain produk terbatas. `DOMAIN_ARCHITECTURE_NOT_RUN` untuk slice ini: ownership existing sudah diketahui dan gate mengizinkan desain langsung. Arsitektur domain lama bagi scope lain tetap berlaku. As-is bersumber audit **BM-AUD-20261010-01** revision 1 (section 7 untuk Swagger), bukan bukti runtime.

Snapshot BE `d4e1eca06fb28c05934c68c1e51a4dca01935a10`, FE `969acfcc04cdf31074a1911e9827c31d25ddadd0`. Semua nama class/field/API baru di bawah adalah **target Rencana (belum tersedia)**. Bila bagian lama bertentangan mengenai bed kembali Available, amandemen ini mengikuti RWI-DEC-281/282. Persetujuan produk bukan persetujuan desain atau SOP. Hash masukan terpusat pada manifest.

```mermaid
flowchart TD
 subgraph admisi[Petugas admisi]
  A["Pilih bed Tersedia"] --> B["Pesan lalu tempatkan pasien"]
 end
 subgraph ruangan[Petugas ruangan berwenang]
  B --> C["Catat kepergian fisik pasien"]
 end
 subgraph hk[Housekeeping]
  C --> D["Mulai lalu selesaikan pembersihan"]
 end
 subgraph verifier[Perawat verifikator]
  D --> E["Periksa dan sahkan kesiapan"]
 end
 subgraph pembaca[Petugas pembaca berwenang]
  E --> F["Bed kembali Tersedia; baca riwayat penggunaannya"]
 end
```

Diagram ini menunjukkan jalur normal penggunaan awal sampai kesiapan kembali. Jalur transfer, penolakan, dan hasil tidak pasti dirinci pada flow per proses di bawah.

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Pilih, pesan, tempatkan | Admisi | Episode sah; bed siap; hak pada unit | Dipesan lalu Terisi | Perbarui data atau pilih bed lain; lihat alur09 |
| Catat kepergian | Petugas ruangan | Kepergian sah dan hunian terkini | Hunian berakhir; Menunggu Pembersihan | Periksa hunian; jangan melepas pasien berikutnya |
| Mulai dan selesai pembersihan | Housekeeping | Siklus terkini; SOP dan penugasan sah | Dalam Pembersihan; tahap Menunggu verifikasi | Pertahankan bed tertahan; lihat alur11 |
| Periksa dan sahkan | Perawat verifikator | Hasil pekerjaan; bukti dan hak sah | Tersedia sesudah seluruh guard lolos | Catat alasan belum siap; lihat alur11 |
| Baca riwayat | Pembaca berwenang | Bed/periode dalam scope | Seluruh segmen penggunaan sesuai hak | Coba baca ulang; lihat alur13 |

Closure master sah membuat Tidak Tersedia, pembukaan kembali belum otomatis Tersedia. Reservasi unused cancel/expiry tidak masuk cleaning. Konflik data/closure/current holder tetap diperiksa pada setiap panah.

- [FLOW-BM-MVP-001 — Pemesanan, hunian dan pelepasan](./09-bed-reservation-release.md)
- [FLOW-BM-MVP-002 — Transfer bed satu langkah](./10-bed-transfer.md)
- [FLOW-BM-MVP-003 — Pembersihan dan pengesahan kesiapan](./11-bed-cleaning-readiness.md)
- [FLOW-BM-MVP-004 — Penutupan administratif dan pembukaan](./12-bed-closure-reopen.md)
- [FLOW-BM-MVP-005 — Riwayat penggunaan dan koreksi](./13-bed-usage-history-correction.md)
- [FLOW-BM-MVP-006 — Hasil tidak pasti dan pemulihan koneksi](./14-bed-uncertain-outcome.md)
