# Arsitektur Frontend — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| Status | `approved` — disetujui Product/Domain Owner 14 Agustus 2026 sesuai `IGD-DEC-046` |
| Kedalaman | **Kontrak fungsional.** Revisi ini memfokuskan kedalaman detail pada backend |
| Commit diaudit | frontend `08c84d371` |

Dokumen ini menetapkan **apa** yang harus dilakukan layar IGD dan **siapa** yang berwenang
memutuskan tampilannya. Ia tidak menetapkan tata letak, warna, maupun pilihan komponen.

---

## 1. Keadaan frontend saat ini

Impact scan pada commit `08c84d371` menemukan **nol file `src/` yang berubah** sejak snapshot
revision 3, dan belum ada route yang memakai endpoint IGD. Seluruh layar di bawah berstatus
belum dibuat.

---

## 2. Layar yang dibutuhkan

| Layar | Kegunaan | Endpoint yang dikonsumsi | Status |
| --- | --- | --- | --- |
| Antrean triage | Perawat melihat pasien menunggu dinilai beserta penanda melampaui batas | `GET /emergency-triages`, `GET /emergency-triages/sla-breaches` | Belum dibuat |
| Formulir triage | Perawat menilai dan menilai ulang pasien | `POST /emergency-triages`, `POST /{id}/retriage` | Belum dibuat |
| Daftar kunjungan IGD | Melihat pasien yang sedang berada di IGD | `GET /emergency-visits` | Belum dibuat |
| Detail kunjungan | Riwayat triage, observasi, tindakan, disposition | Beberapa endpoint IGD | Belum dibuat |
| Observasi | Mencatat pemantauan berkala | `GET`, `POST` `/emergency-observation-details` | Belum dibuat |
| Disposition | Dokter menetapkan tindak lanjut | `POST /emergency-dispositions` | Belum dibuat |
| Transfer | Mengajukan dan memantau perpindahan | `/emergency-transfers` | Belum dibuat |

---

## 3. Aksi per peran

| Peran | Yang dapat dilakukan |
| --- | --- |
| Petugas pendaftaran | Membuat kunjungan, melengkapi identitas, menandai pasien tidak dikenal |
| Perawat IGD | Menilai dan menilai ulang triage, mencatat observasi, mengajukan transfer |
| Dokter IGD | Menetapkan disposition, menyelesaikan kunjungan |
| Petugas unit tujuan | Menerima, menolak, menyelesaikan transfer |
| Kepala jaga | Membatalkan dengan alasan |

Antarmuka **tidak boleh** menampilkan aksi yang tidak dimiliki penggunanya. Menyembunyikan
tombol bukan pengaman; backend tetap memvalidasi setiap permintaan.

---

## 4. State yang wajib ditangani

Setiap layar yang mengambil data wajib menangani seluruh keadaan berikut:

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka konten, bukan layar kosong |
| Kosong | Kalimat yang menjelaskan, misalnya "Belum ada pasien menunggu triage." |
| Gagal | Penjelasan singkat beserta tombol Coba lagi |
| Tanpa hak akses | "Anda tidak memiliki hak akses untuk melihat data ini." |
| Data usang | Penanda bahwa data perlu dimuat ulang |
| Kirim ganda | Tombol dinonaktifkan selama proses menyimpan |
| Validasi gagal | Kolom bermasalah ditandai beserta alasannya |

Khusus antrean triage, pasien yang melampaui `ResponseDueAt` **wajib** ditandai beserta
keterangan lama menunggu.

---

## 5. Kewenangan UI

Urutan kewenangan:

```text
security/privacy/invariant
  -> approved product/UI brief
  -> design system/convention project
  -> DEV_DISCRETION
```

| Area | Siapa yang memutuskan |
| --- | --- |
| Struktur menu dan route publik | Manajer Sistem Informasi |
| Warna kategori triage | **Master data**, bukan frontend. Diambil dari `ColorName` dan `ColorHex` |
| Target waktu tunggu | **Master data**, bukan frontend |
| Urutan kolom tabel, penempatan tombol, pilihan komponen | `DEV_DISCRETION` |

Warna dan target waktu **dilarang** di-hardcode di frontend. Keduanya kebijakan rumah sakit
yang harus dapat diubah tanpa mengubah source code.

---

## 6. Privasi di antarmuka

| Aturan | Penjelasan |
| --- | --- |
| Tidak menampilkan UUID | Identifier teknis bukan label pengguna; tampilkan nama |
| Ringkasan klinis hanya untuk yang berhak | Kolom bertanda sensitif pada kamus data mengikuti hak akses |
| Tidak menyalin data klinis ke penyimpanan lokal | Termasuk cache peramban yang tidak terenkripsi |

---

## 7. Ketergantungan pada backend

Frontend boleh mulai mengerjakan sebuah layar **hanya** setelah contract version yang
dipakainya berstatus approved dan hash-nya terkunci.

| Layar | Menunggu |
| --- | --- |
| Antrean triage dengan penanda breach | `GET /emergency-triages/sla-breaches` — belum tersedia |
| Formulir retriage | `POST /emergency-triages/{id}/retriage` — belum tersedia |
| Tombol selesaikan kunjungan | `PATCH /emergency-visits/{id}/complete` — belum tersedia |

Frontend **tidak boleh** membuat sumber data palsu sebagai pengganti permanen endpoint yang
belum ada.

Nilai `Completed` pada `EmergencyVisitStatus` adalah perubahan yang berpotensi memutus. Layar
yang memetakan status secara eksklusif wajib menangani nilai baru ini sebelum backend
menerapkannya.
