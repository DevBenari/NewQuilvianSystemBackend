# Alur Utama Dokter Rawat Inap

| Field | Nilai |
| --- | --- |
| Sub-modul | `dokter-rawat-inap` |
| Revision | **`0.3`** — bagian 4 lahir, menggantikan bagian 1 dan 2 untuk ruang kerja V2 |
| Status | **`draft`** untuk `0.3`. `0.2` `approved` — disetujui Muhammad Hamzah, 2026-09-03 |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-03** |
| Isi | **Jalur normal saja.** Jalur pengecualian ada pada berkas per proses |
| Sumber | `PRD-RWI-FINAL-001` bagian 18 dan 19; arsitektur domain `0.2` bagian U |
| Berkas per proses | [`01-catatan-harian-dan-cppt.md`](./01-catatan-harian-dan-cppt.md), [`02-visite-dokter.md`](./02-visite-dokter.md), [`03-penulisan-penguncian-dan-catatan-saya.md`](./03-penulisan-penguncian-dan-catatan-saya.md), [`04-resep-rekonsiliasi-dan-sliding-scale.md`](./04-resep-rekonsiliasi-dan-sliding-scale.md), [`05-pesanan-tindakan-dan-verifikasi-instruksi.md`](./05-pesanan-tindakan-dan-verifikasi-instruksi.md) |

---

## 1. Dari pasien masuk sampai rencana pulang

Nama keadaan sama persis dengan
[`../contracts/state-transition-matrix.md`](../contracts/state-transition-matrix.md).

```mermaid
flowchart TD
    subgraph EPS["Episode rawat inap"]
        A1["Pasien dikonfirmasi tiba di kamar"]
        A2["DPJP ditetapkan"]
    end

    subgraph DOK["Dokter"]
        B1["Buka daftar pasien yang dirawat"]
        B2["Pilih pasien lalu buka ruang kerjanya"]
        B3["Tulis kajian medis awal"]
        B4["Kajian medis Completed"]
        B5["Catat visite hari ini"]
        B6["Tulis catatan perkembangan harian"]
        B7["Pesan pemeriksaan penunjang bila perlu"]
        B8["Buat resep"]
        B9["Catat tindakan yang dikerjakan"]
        B10["Verifikasi catatan terpadu"]
        B11["Nyatakan pasien boleh pulang"]
    end

    subgraph LAIN["Pihak lain"]
        C1["Farmasi menyiapkan dan menyerahkan obat"]
        C2["Laboratorium memproses lalu mengesahkan hasil"]
        C3["Radiologi menjadwalkan, mengerjakan, lalu mengesahkan hasil"]
        C4["Perawat menulis catatan terpadu"]
    end

    A1 --> A2 --> B1 --> B2 --> B3 --> B4
    B4 --> B5 --> B6
    B6 --> B7 --> B8 --> B9
    B8 -.-> C1
    B7 -.-> C2
    B7 -.-> C3
    C2 -.-> B6
    C3 -.-> B6
    C4 -.-> B10
    B9 --> B5
    B6 --> B11
```

Garis putus-putus berarti **menyerahkan atau menerima**, bukan menunggu. Dokter tidak menahan
langkahnya menunggu Farmasi, Laboratorium, maupun Radiologi.

Panah balik dari tindakan ke visite adalah irama harian: setiap hari dokter datang, mencatat
visitenya, menulis perkembangan, dan bila perlu memesan, meresepkan, atau bertindak.

> **Yang berubah dari revision `0.1`:** langkah membuka daftar pasien dipisah dari langkah membuka
> ruang kerja, karena sumber daftarnya berpindah dari antrean rawat jalan ke census episode; dan
> Radiologi masuk sebagai pihak yang benar-benar ada.

---

## 2. Tabel langkah

| No | Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal, petugas melakukan |
| ---: | --- | --- | --- | --- | --- |
| 1 | Pasien dikonfirmasi tiba di kamar | Admisi atau supervisor | Pasien hadir di kamar | Perawatan resmi berjalan | Bukan pekerjaan dokter. Hubungi admisi |
| 2 | DPJP ditetapkan | Admisi saat admisi, atau supervisor | Daftar dokter | Penanggung jawab tercatat | Tanpa DPJP, dokter jaga tetap dapat menulis; episodenya muncul di daftar pantau |
| 3 | Buka daftar pasien yang dirawat | Dokter | Daftar pasien dirawat, disaring pada dokter yang masuk | Daftar pasiennya sendiri | Muat ulang daftar. **Jangan** memakai daftar antrean poliklinik |
| 4 | Buka ruang kerja satu pasien | Dokter | Baris pasien terpilih | Konteks pasien tampil | Muat ulang. Jangan menulis sebelum konteks pasti |
| 5 | Tulis kajian medis awal | DPJP | Keadaan pasien, riwayat, hasil pemeriksaan | Kajian tersimpan bertahap | Isian tersimpan sebagai belum selesai |
| 6 | Selesaikan kajian medis | DPJP | Kajian lengkap beserta diagnosis | Kajian `Completed` | Sistem menyebut bagian yang masih kosong |
| 7 | Catat visite | Dokter | Kunjungan yang benar-benar dilakukan | Event visite tersimpan | Bila tombol tertekan dua kali, tetap satu event |
| 8 | Tulis catatan perkembangan harian | Dokter | Keadaan pasien hari itu | Catatan baru, **tidak menimpa** kajian awal | Isian tidak hilang |
| 9 | Pesan penunjang | Dokter | Indikasi klinis | Pesanan terkirim ke Laboratorium atau Radiologi | Pesanan gagal dapat diulang; tidak ada pesanan ganda |
| 10 | Buat resep | Dokter | Obat, dosis, aturan pakai, jenis resep | Resep terkirim ke Farmasi | Ulangi dengan kunci yang sama; tidak ada resep ganda |
| 11 | Catat tindakan | Dokter | Tindakan yang dikerjakan | Catatan tersimpan | Kegagalan tagihan **tidak** menghilangkan catatan |
| 12 | Verifikasi catatan terpadu | DPJP | Catatan profesi lain | Terverifikasi; **penulis aslinya tidak berubah** | Bila kebijakan verifikasi belum ada, langkah ini tidak muncul |
| 13 | Nyatakan boleh pulang | DPJP | Keadaan pasien | Keputusan pulang tercatat | **Bukan milik sub-modul ini** — `CAP-026` milik `episode-rawat-inap` |

---

## 3. Yang tidak ada di alur ini, dan kenapa

| Yang tidak ada | Alasan |
| --- | --- |
| Mengambil nomor antrean lebih dulu | Pasien menginap tidak pernah masuk antrean poliklinik — `RWI-RULE-026` aturan 2 |
| Menunggu pengkajian keperawatan selesai sebelum dokter menulis | `AC-CAP020-02` menyatakannya tegas |
| Menghitung visite dari catatan yang ditulis | `INV-DOK-07`. Visite dicatat sebagai kejadian tersendiri |
| Menandai obat sudah diserahkan | `RUL-DOK-01`. Itu pekerjaan Farmasi |
| Mengisi hasil laboratorium atau radiologi | `RUL-DOK-02`. Itu pekerjaan modul pemiliknya |
| Menulis resume pulang | `CAP-026` milik `episode-rawat-inap` |
| Menggabungkan dua visite menjadi satu demi tagihan | `RWI-DEC-085`. Agregasi milik Billing dan tidak menyentuh riwayat klinis |

---

## 4. Alur utama revision `0.3` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

**Status `draft`.** Diagram dan tabel di bawah **menggantikan** bagian 1 dan 2 untuk ruang kerja rawat inap. Bagian 3
tetap berlaku, kecuali baris "Menulis resume pulang": dokter kini menulis resume dari tab Resume Medis, sedangkan
datanya tetap milik `episode-rawat-inap`. Jalur normal saja; pengecualian ada pada berkas proses
[`03-penulisan-penguncian-dan-catatan-saya.md`](./03-penulisan-penguncian-dan-catatan-saya.md),
[`04-resep-rekonsiliasi-dan-sliding-scale.md`](./04-resep-rekonsiliasi-dan-sliding-scale.md), dan
[`05-pesanan-tindakan-dan-verifikasi-instruksi.md`](./05-pesanan-tindakan-dan-verifikasi-instruksi.md).

```mermaid
flowchart TD
    subgraph ruangan[Kepala ruangan atau admisi]
        A([Pasien masuk dan DPJP ditetapkan]) --> B[Menetapkan konsulen atau dokter jaga bila perlu]
    end
    subgraph dokter[Dokter]
        B --> C[Membuka Dokter lalu Rawat Inap]
        C --> D[Memilih pasien dari daftar miliknya]
        D --> E[Menulis kajian medis]
        E --> F[(Kajian Signed)]
        F --> G[Mencatat visit]
        G --> H[Menulis SOAP harian]
        H --> I[(SOAP Signed)]
        I --> J[Memutuskan rekonsiliasi obat bawaan]
        J --> K[Membuat resep dan protokol sliding scale bila perlu]
        K --> L[Memesan tindakan dan penunjang]
        L --> M[Membaca hasil final penunjang]
        M --> N[Memverifikasi CPPT dan pesanan perawat pada Perlu Review]
        N --> O[Menulis dan menandatangani resume dari tab Resume Medis]
    end
    O --> Z([Episode siap diproses pulang oleh episode-rawat-inap])
```

| No | Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal, petugas melakukan |
| ---: | --- | --- | --- | --- | --- |
| 1 | Menetapkan penugasan dokter | Admisi, kepala ruangan, atau supervisor | Dokter, peran, waktu mulai | DPJP, konsulen, atau dokter jaga tercatat | Dokter tanpa penugasan tidak melihat pasien; kepala ruangan membuat penugasan |
| 2 | Membuka Dokter → Rawat Inap | Dokter | Akun tertaut dokter | Daftar pasien miliknya dan metrik | Daftar gagal → Coba Lagi; **jangan** memakai antrean poliklinik |
| 3 | Memilih pasien | Dokter | Kartu pasien | Konteks pasien, alergi, alert, delapan tab | Alergi gagal dimuat tampil sebagai peringatan, bukan "tidak ada alergi" |
| 4 | Menulis dan menyelesaikan kajian medis | Dokter | Tujuh bagian kajian | Kajian `Signed` | Konsep tersimpan; penyelesaian ditolak bila isian belum lengkap |
| 5 | Mencatat visit | Dokter | Kunjungan nyata | Event visit | Tombol tertekan dua kali tetap satu event |
| 6 | Menulis SOAP harian | Dokter | Keadaan hari itu | SOAP `Signed` | Penugasan sudah berakhir → penugasan singkat |
| 7 | Memutuskan rekonsiliasi | Dokter | Obat bawaan dicatat perawat | Keputusan per obat; butir draft resep | Perawat belum mencatat → dokter menunggu atau meminta perawat mencatat |
| 8 | Membuat resep dan protokol | Dokter | Obat; versi protokol sah untuk insulin | Resep aktif; protokol pasien `Active` | Belum ada protokol sah → sliding scale tidak dapat dipesan |
| 9 | Memesan tindakan dan penunjang | Dokter | Indikasi | Pesanan `Ordered` | Pesanan dapat diulang tanpa ganda |
| 10 | Membaca hasil | Dokter | Hasil final | Hasil terbaca pada tab Penunjang | Hasil belum final ditandai berbeda |
| 11 | Memverifikasi | DPJP dan dokter pemberi instruksi | Perlu Review | CPPT dan pesanan perawat `Verified` | Bukan DPJP → entri tetap menunggu DPJP |
| 12 | Resume | DPJP | Delapan bagian resume | Resume bertanda tangan; episode **belum** tertutup | Bukan DPJP aktif → DPJP yang menandatangani |
