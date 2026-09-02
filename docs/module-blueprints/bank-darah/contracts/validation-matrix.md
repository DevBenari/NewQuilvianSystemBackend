# Bank Darah — Validation Matrix

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v1` — `draft` |
| `last_changed_in` | `v1` |
| Owner | Pemilik proses BDRS · pemilik proses klinis |
| `approved_by` / `approved_at` | Kosong — `draft` |
| Sumber | `00-interview-decisions.md` revisi 4 (INV/AC) · `03-domain-architecture.md` revisi 3 |

Pesan ditulis dalam Bahasa Indonesia yang dipahami pengguna, **bukan** istilah teknis. Kolom "Kode
teknis" adalah kode respons yang muncul di log/gerbang, bukan yang dibaca pengguna. Ini **satu-satunya**
tempat kalimat pesan penolakan hidup; `flowcharts/` dan `state-transition-matrix.md` hanya merujuk
kodenya.

Konvensi kode HTTP: `400` isian tidak lengkap/format salah · `403` tidak berhak · `404` data tidak
ditemukan · `409` bentrok konkurensi atau status sudah berubah · `422` melanggar aturan bisnis.

---

## 1. Order darah

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-001` | Buat order | Sudah ada order aktif untuk pasien + kunjungan + komponen yang sama | "Sudah ada order darah aktif untuk pasien dan komponen ini pada kunjungan yang sama. Lanjutkan hanya dengan alasan tertulis." | `422` |
| `VAL-BD-002` | Baris order | Jumlah diminta ≤ 0 | "Jumlah kantong yang diminta harus lebih dari nol." | `400` |
| `VAL-BD-003` | Baris order | Komponen tidak ada di katalog / diketik bebas | "Komponen darah harus dipilih dari katalog, tidak boleh diketik bebas." | `400` |
| `VAL-BD-004` | Order `Expired` | Percobaan mengaktifkan kembali | "Order yang sudah kedaluwarsa tidak dapat dibuka kembali. Buat order baru pada kunjungan yang berjalan." | `422` |
| `VAL-BD-010` | Order manual | Pasien / kunjungan / dokter peminta / unit asal / pelaku input kosong | "Order manual wajib mengisi pasien, kunjungan, dokter peminta, unit asal, dan petugas yang menginput." | `400` |
| `VAL-BD-011` | Order tersimpan | Jejak pelaku input tidak tercatat | "Setiap order wajib menyimpan siapa yang membuatnya." | `422` |
| `VAL-BD-012` | Keputusan klinis | `MstPatient.BloodType` dipakai untuk menilai kesesuaian darah | "Golongan darah pada data pendaftaran tidak boleh dipakai untuk menilai kesesuaian darah. Gunakan hasil pemeriksaan Bank Darah." | `422` |
| `VAL-BD-013` | Buat order | Unit pelayanan `IsAvailableForBloodOrder=false` | "Unit pelayanan ini belum diberi kewenangan memesan darah." | `403` |

## 2. Permintaan PMI & penerimaan

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-006` | Buat permintaan | Sudah ada permintaan aktif untuk kebutuhan yang sama | "Sudah ada permintaan darah yang masih berjalan untuk kebutuhan ini. Tidak boleh dibuat permintaan baru." | `422` |
| `VAL-BD-007` | Buat permintaan | Jumlah kantong kosong | "Jumlah kantong yang diminta wajib diisi." | `400` |
| `VAL-BD-014` | Terima kantong | Kantong berlebih | (Bukan penolakan) "Kiriman melebihi permintaan. Kantong tetap dicatat diterima dan masuk daftar menunggu keputusan." | `200` |
| `VAL-BD-015` | Stok | Percobaan menambah stok tanpa penerimaan fisik | "Stok bertambah hanya setelah kantong diterima secara fisik." | `422` |

## 3. Kantong: alokasi, bukti, pemberian, koreksi

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-017` | Berikan | Kantong belum dialokasikan | "Kantong harus dialokasikan ke order pasien sebelum diberikan." | `422` |
| `VAL-BD-018` | Berikan | Tidak ada bukti kecocokan & bukan jalur darurat | "Bukti pemeriksaan kecocokan belum tercatat. Darah tidak dapat diberikan." | `422` |
| `VAL-BD-019` | Berikan | Bukti kecocokan atas nama pasien lain | "Bukti kecocokan yang ada bukan untuk pasien ini. Catat bukti kecocokan terhadap pasien tujuan." | `422` |
| `VAL-BD-020` | Berikan | Bukti kecocokan sudah lewat masa berlaku | "Bukti kecocokan sudah lewat masa berlaku. Diperlukan bukti kecocokan yang baru." | `422` |
| `VAL-BD-020b` | Berikan | Masa berlaku komponen belum dikonfigurasi | "Masa berlaku bukti kecocokan untuk komponen ini belum ditetapkan. Pemberian ditahan sampai dikonfigurasi." | `422` |
| `VAL-BD-018c` | Alokasi | Ada alokasi aktif lain pada kantong (konkurensi) | "Kantong ini baru saja dialokasikan petugas lain. Muat ulang dan pilih kantong lain." | `409` |
| `VAL-BD-021` | Jalur darurat | Bukan peran berwenang, atau alasan kosong | "Jalur darurat hanya untuk peran berwenang dan wajib mengisi alasan." | `403` |
| `VAL-BD-023` | Batalkan alokasi | Kantong sudah `Issued` | "Kantong sudah diberikan. Pembatalan tidak dapat dilakukan; gunakan catatan koreksi bila pencatatannya keliru." | `422` |
| `VAL-BD-024` | Catat koreksi | Bukan peran berwenang | "Pencatatan koreksi hanya untuk peran berwenang." | `403` |
| `VAL-BD-025` | Hapus/anulir pemberian | Percobaan menghapus atau membalik pemberian | "Pemberian darah tidak dapat dihapus atau dibatalkan. Satu-satunya jalur perbaikan adalah catatan koreksi." | `422` |
| `VAL-BD-033` | Alokasi | Kantong `Excess`/`PendingReview` dialokasikan langsung | "Kantong ini menunggu keputusan dan tidak dapat langsung dialokasikan. Selesaikan statusnya lebih dulu." | `422` |
| `VAL-BD-016` | Pembatalan / penyelesaian | Alasan tidak dipilih dari daftar terkendali | "Alasan wajib dipilih dari daftar, tidak boleh diketik bebas." | `400` |

## 4. Golongan darah & konflik

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-030` | Catat hasil | Pemeriksa atau waktu pemeriksaan kosong | "Hasil golongan darah wajib menyimpan pemeriksa dan waktu pemeriksaan." | `400` |
| `VAL-BD-034` | Gerbang klinis | Pasien sedang `IsConflictHeld` | "Golongan darah pasien ini sedang bertentangan dan ditahan. Selesaikan perbedaannya lebih dulu." | `422` |
| `VAL-BD-037` | Validasi / penyelesaian konflik | Bukan peran validator | "Hanya peran validator yang boleh memvalidasi atau menyelesaikan perbedaan hasil golongan darah." | `403` |
| `VAL-BD-051` | Selesaikan konflik | Tidak menunjuk pemeriksaan ulang tervalidasi | "Perbedaan hasil hanya dapat diselesaikan setelah ada pemeriksaan ulang yang tervalidasi." | `422` |
| `VAL-BD-054` | Selesaikan konflik | Percobaan menutup dengan pilihan "mayoritas" otomatis | "Sistem tidak menentukan hasil yang benar. Validator wajib menyatakan hasil yang berlaku." | `422` |

## 5. Tindakan Bank Darah

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-026` | Catat tindakan | Tidak menunjuk order sah | "Tindakan Bank Darah wajib menunjuk satu order yang sah." | `400` |
| `VAL-BD-027` | Tindakan | Percobaan menghitung tarif sendiri | "Tarif tidak dihitung di modul ini; dirujuk dari data tindakan bertarif." | `422` |

---

## Catatan konsistensi

- Setiap kode di sini dirujuk oleh `state-transition-matrix.md` (kolom "Bila dilanggar") dan diuji oleh
  `testing/acceptance-test-matrix.md`.
- Aturan bergerbang **fail-closed**: bila konfigurasi (mis. masa berlaku `VAL-BD-020b`) atau peran
  (`DEF-BD-004`) belum ditetapkan, gerbang menolak — bukan meloloskan dengan nilai tebakan.
- Pesan **MUST NOT** memuat data medis/pribadi pasien.
