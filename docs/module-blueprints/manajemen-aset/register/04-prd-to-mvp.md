# Register Aset — PRD → MVP

## 1. Identitas dokumen

| Field | Value |
|---|---|
| Produk / modul | Quilvian — Manajemen Aset & Inventaris / sub-modul `register` |
| Status | `draft` · contract set `AST-REG-1.0.0` |
| Repository target | `NewQuilvianSystemBackend` (`rizkiG`), `QuilvianSystemFrontendDev` (`RizkiV2`) |
| SHA baseline | BE `1f54a175`, FE `e49e8a81` |
| Cakupan | Master klasifikasi, pencatatan dan penomoran, label QR, dokumen kepemilikan, penempatan, kondisi, pemanfaatan, pengamanan, impor data awal, dashboard, laporan register, History & Audit |
| `input_revision` | decision revision `4`; `02-backend-architecture.md` dan lima kontrak sub-modul ini |

## 2. Ringkasan eksekutif

Bagian Aset dapat mengetahui, untuk setiap barang tahan lama milik RS, nomor permanennya, unit dan
ruangan tempatnya, siapa yang memegangnya, dan bagaimana kondisinya — dengan bukti persetujuan untuk
setiap perubahan. Rilis ini juga memindahkan seluruh aset lama ke sistem dalam satu batch yang
diperiksa Bagian Aset dan Akuntansi.

## 3. Masalah produk

Belum ada satu pun kemampuan aset di Quilvian (`AST-FACT-001`). Yang ada di sekitar: unit organisasi,
pegawai, penugasan pegawai ke unit, Number Series, GR Finance, serta pustaka QR, PDF, dan Excel
(`01-existing-capability-map.md` `TS-01..TS-24`). Satu-satunya catatan "aset" adalah `TrxAssetReturn` HR
yang berupa teks bebas (`TS-25`).

## 4. Visi produk

1. Barang diterima → dicatat → disetujui → bernomor permanen dan berlabel QR.
2. Aset diserahkan ke unit, ruangan, dan penanggung jawab → penerima mengonfirmasi → berita acara.
3. Perubahan kondisi dilaporkan unit → ditetapkan Bagian Aset → tercatat di linimasa.
4. Setiap aset membawa satu linimasa yang dibaca oleh semua sub-modul lain dan oleh auditor.

## 5. Batas MVP

**Titik mulai:**

1. Registry `Ast` disetujui dan delapan master terisi.
2. Pengaturan Aset (tanggal awal) dan parameter nilai dasar (`nilai`, `EPIC AST-11`) terisi.

**Titik akhir:**

1. Aset lama sudah masuk lewat impor dan aset baru dapat dicatat sampai `Terdaftar`.
2. Setiap aset dapat ditempatkan, dilaporkan kondisinya, dicetak labelnya, dan dimanfaatkan pihak ketiga.
3. Dashboard, laporan per unit, laporan kondisi, dan History & Audit menampilkan angka yang sama dengan daftar.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab dalam MVP |
|---|---|
| Petugas Aset | Mencatat, mengajukan, mencetak label, menyerahkan, menetapkan kondisi, mengunggah impor, mengelola dokumen |
| Kepala Bagian Aset | Mengelola master, menyetujui pencatatan, menyetujui Hilang, menyetujui pemanfaatan sampai batas, menyetujui data impor |
| Kepala Unit / penanggung jawab | Mengonfirmasi penerimaan, melaporkan kondisi, melihat aset unitnya |
| Kepala Akuntansi | Menyetujui angka impor |
| Direktur | Menyetujui pemanfaatan besar |
| Auditor | Membaca seluruh data dan linimasa, tanpa rupiah |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID asal | Keputusan MVP |
|---|---|---|
| Master klasifikasi dan lokasi | `CAP-02.1`–`02.6`, `CAP-EXT-05` | Wajib; tanpa master aset tidak dapat dicatat |
| Pencatatan, perolehan, nomor | `CAP-03.1`–`03.3` | Wajib; inti register |
| Label QR | `CAP-03.4` | Wajib; dibutuhkan opname (`inventarisasi`) |
| Dokumen kepemilikan | `CAP-03.5`, `08.2`, `08.3` | Wajib; tanah, gedung, kendaraan tidak sah tanpa dokumen |
| Daftar inventaris | `CAP-03.6`, `11.1` | Wajib |
| Penempatan dan penyerahan | `CAP-04.1`–`04.4` | Wajib; tanpa ini aset tidak punya penanggung jawab |
| Kondisi dan status | `CAP-06.1`–`06.6`, `11.3` | Wajib; prasyarat mutasi dan penghapusan |
| Pemanfaatan | `CAP-08.1`, `08.5` | Wajib; sewa/KSO berjalan tanpa jejak bila tidak ada |
| Aset menganggur | `CAP-08.4` | Wajib; turunan murah dari kondisi |
| Impor data awal | `CAP-EXT-01` | Wajib; tanpa ini ribuan aset lama tidak tercatat |
| Dashboard 1.1–1.3, 1.6 dan History & Audit | `CAP-01.*`, `11.7` | Wajib |

## 8. Kemampuan yang ditunda

| Kemampuan | ID asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Tautan ke pengembalian aset pegawai keluar | `AST-OQ-012` | Butuh keputusan pemilik HR | HR tetap mencatat `TrxAssetReturn` teks bebas; Petugas Aset menyerahkan ulang aset lewat penyerahan |
| Laporan ASPAK | Di luar cakupan | Kontrak pelaporan Kemenkes belum dirancang | Ekspor Excel daftar alat medis beserta atributnya |

## 9. Alur bisnis target

`FLOW-AST-MVP-001`: barang diterima → Petugas Aset membuat draft → mengajukan → Kepala Bagian Aset
menyetujui → nomor terbit dan buku nilai terbentuk → label dicetak → aset diserahkan → penerima
mengonfirmasi → aset dipakai → kondisi dilaporkan dan ditetapkan bila berubah. Rincian: `flowcharts/00..05`.

## 10. Epic dan functional requirement

### EPIC AST-01 — Fondasi dan master · disposisi `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-001` | Baris registry `AssetManagement`/`Ast` terdaftar sebelum model pertama | Verifier QBE lulus dengan entity `AstAsset` |
| `FR-AST-002` | Delapan master dapat ditambah, diubah, dinonaktifkan; kode unik | Kode `ALKMED` kedua ditolak 409 |
| `FR-AST-003` | Kode inventaris 2–11 huruf besar/angka, bukan kode terlarang, terkunci setelah dipakai | `MUT` ditolak 400; mengubah `ALKMED` setelah ada `AST-ALKMED-000001` ditolak 409 |
| `FR-AST-004` | `AssetActorResolver` menurunkan unit dan Kepala Unit dari penugasan aktif; tanpa penugasan → nol aset | Pegawai tanpa penugasan aktif melihat daftar kosong |
| `FR-AST-005` | Lampiran fail-closed bila pengaturan kosong | Unggah foto → 503 |

### EPIC AST-02 — Pencatatan, persetujuan, nomor, label · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-010` | Buat 1–200 draft sekali isi | "Buat 40 unit" → 40 draft |
| `FR-AST-011` | Atribut alat medis wajib saat diajukan | Monitor tanpa izin edar ditolak |
| `FR-AST-012` | Persetujuan menerbitkan nomor `AST-<KODE>-######` dan membuat buku nilai | `AST-ALKMED-000123`; buku `IsCapitalized=true` untuk Rp 85 jt |
| `FR-AST-013` | Persetujuan massal: tiap aset transaksi sendiri, hasil per aset | 38 berhasil, 2 gagal tanpa membatalkan 38 |
| `FR-AST-014` | Pengaju tidak dapat menyetujui sendiri | 403 |
| `FR-AST-015` | Penolakan mengembalikan ke Draft tanpa memakan nomor | — |
| `FR-AST-016` | Hibah masuk yang disetujui menyiapkan satu kejadian perolehan hibah | Kursi roda Rp 3 jt → satu baris kotak keluar |
| `FR-AST-017` | Label QR PDF hanya memuat nomor inventaris | QR terbaca `AST-MBL-000201` |

### EPIC AST-03 — Penempatan dan penyerahan · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-020` | Penempatan berlaku hanya saat penerima mengonfirmasi | Disetujui 3 Mei, dikonfirmasi 5 Mei → lokasi berubah 5 Mei |
| `FR-AST-021` | Hanya Kepala Unit tujuan atau penanggung jawab yang dituju yang dapat mengonfirmasi | Kepala Unit lain → 403 |
| `FR-AST-022` | Satu aset satu proses aktif | Aset dalam usulan penghapusan tidak dapat diserahkan |
| `FR-AST-023` | Penyerahan ditolak bila unit sedang opname | 409 |
| `FR-AST-024` | Berita Acara Penyerahan dicetak dengan nama, jabatan, dan waktu persetujuan (`AST-DEC-046`) | PDF memuat nama penyerah dan penerima |

### EPIC AST-04 — Kondisi dan status · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-030` | Kondisi berubah hanya setelah ditetapkan | Laporan 3 Juni, ditetapkan 4 Juni |
| `FR-AST-031` | Hilang wajib berita acara dan persetujuan Kepala Bagian Aset bukan pelapor | Tanpa berita acara → 400 |
| `FR-AST-032` | Satu laporan terbuka per aset | Laporan kedua → 409 |
| `FR-AST-033` | Daftar per kondisi dan riwayat perubahan status | Menu 6.2–6.6 |

### EPIC AST-05 — Dokumen, pengamanan, menganggur, pemanfaatan · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-040` | Kelengkapan dokumen wajib per kategori | Kendaraan tanpa STNK tampil di kelengkapan |
| `FR-AST-041` | Dokumen hampir habis sesuai pengaturan hari | STNK habis 15 Des tampil sejak 15 Nov |
| `FR-AST-042` | Aset menganggur = Tidak Digunakan > N hari | 91 hari dengan N=90 tampil |
| `FR-AST-043` | Penyetuju pemanfaatan ditentukan saat diajukan dari batas nilai dan masa | KSO 60 bulan → Direktur |
| `FR-AST-044` | Pemanfaatan berjalan dan berakhir otomatis menurut tanggal | — |

### EPIC AST-06 — Impor data awal · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-050` | Semua atau tidak sama sekali | 1 baris salah dari 2.400 → nol tersimpan |
| `FR-AST-051` | Dua persetujuan: Kepala Bagian Aset lalu Kepala Akuntansi, bukan pengunggah | — |
| `FR-AST-052` | Saldo awal tidak dikirim ke Akuntansi | Nol baris kotak keluar |

### EPIC AST-07 — Dashboard, laporan, History & Audit · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-060` | Kartu dashboard = hitungan daftar bersaringan yang sama | 12 aset Rusak Berat di kartu = 12 baris di daftar |
| `FR-AST-061` | Kepala Unit melihat angka unitnya saja | — |
| `FR-AST-062` | History & Audit menampilkan kejadian kelima sub-modul dari satu linimasa | Mutasi, opname, penghapusan tampil bersama |

## 11. Model status yang diusulkan

Status pencatatan `Draft → Diajukan → Terdaftar` (+ `Dibatalkan`, `Dihapus`); kondisi `Aktif`,
`RusakRingan`, `RusakBerat`, `TidakDigunakan`, `Hilang`; penyerahan, laporan kondisi, pemanfaatan, dan
batch impor sesuai `contracts/state-transition-matrix.md`. Invariant utama: nomor permanen; satu proses
aktif per aset; kondisi dan penempatan hanya berubah lewat langkah yang disetujui.

## 12. Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
|---|---|---|
| `MstOrganizationUnit`, `MstRoom`, `MstEmployee`, `WfpOrganizationAssignment`, `NumberSeriesAllocator`, QRCoder, QuestPDF, ClosedXML, komponen dasar FE | `menu-items.jsx` (grup baru) | 20 tabel, 17 service, 16 controller (`02-backend-architecture.md`) |

## 13. Sasaran kemampuan API

Seluruh endpoint pada [`contracts/api-contract.md`](contracts/api-contract.md) grup 2.1–2.17, semuanya
**Rencana (belum tersedia)**. Pemetaan ke epic: 2.1–2.7 `EPIC AST-01`; 2.8 `EPIC AST-02`; 2.9–2.10
`EPIC AST-03`; 2.11 `EPIC AST-04`; 2.12–2.14 `EPIC AST-05`; 2.15 `EPIC AST-06`; 2.16–2.17 `EPIC AST-07`.

## 14. Matriks kewenangan

Sama persis dengan [`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 2.

## 15. Batas integrasi dan billing

Sub-modul ini **MUST NOT** membuat jurnal, menyalin unit/ruangan/pegawai, membuat PO/GR, atau menagih
sewa pemanfaatan. Ia hanya menyiapkan kejadian hibah lewat `nilai`. Tidak ada dampak billing pasien.

## 16. Guardrail regulasi

Tidak menyentuh rekam medis pasien. Atribut izin edar alat medis disimpan apa adanya tanpa validasi ke
sistem Kemenkes. Data pegawai yang ditampilkan hanya nama dan nomor pegawai.

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-REG-001` | Persetujuan massal: setiap aset transaksi sendiri |
| `NFR-REG-002` | Konkurensi optimistis `RowVersion` pada semua dokumen yang berubah status |
| `NFR-REG-003` | Penerapan impor dalam satu transaksi; 5.000 baris selesai < 2 menit pada server uji |
| `NFR-REG-004` | Daftar aset 10.000 baris: halaman pertama < 1 detik dengan index pada bagian `data-dictionary.md` |
| `NFR-REG-005` | Linimasa tidak dapat diubah atau dihapus |
| `NFR-REG-006` | Waktu disimpan UTC; tanggal bisnis (`DateOnly`) mengikuti zona RS (WIB) |

## 18. Skenario UAT

| ID | Kondisi awal | Langkah | Hasil yang diharapkan | Epic |
|---|---|---|---|---|
| `UAT-REG-01` | Master terisi | Catat 40 kursi, setujui 38, tolak 2 | 38 nomor berurutan; 2 kembali Draft | AST-02 |
| `UAT-REG-02` | Draft monitor tanpa izin edar | Ajukan | Ditolak dengan pesan atribut medis | AST-02 |
| `UAT-REG-03` | 5 kursi roda Terdaftar | Serahkan ke Poli Anak, Kepala Ruang konfirmasi | Lokasi berubah; berita acara tercetak | AST-03 |
| `UAT-REG-04` | Penyerahan ke Poli Anak | Kepala Ruang IGD mencoba konfirmasi | Ditolak hak | AST-03 |
| `UAT-REG-05` | Monitor Aktif | Lapor Rusak Ringan dengan foto, Petugas menetapkan | Kondisi berubah setelah ditetapkan | AST-04 |
| `UAT-REG-06` | Laptop dilaporkan hilang | Setujui tanpa berita acara | Ditolak | AST-04 |
| `UAT-REG-07` | STNK habis 30 hari lagi | Buka Pengamanan Administrasi | STNK tampil | AST-05 |
| `UAT-REG-08` | KSO 60 bulan | Kepala Bagian Aset mencoba menyetujui | Ditolak; hanya bisa diteruskan ke Direktur | AST-05 |
| `UAT-REG-09` | Berkas 2.400 baris benar | Unggah, setujui dua tahap | 2.400 aset terdaftar dengan saldo awal | AST-06 |
| `UAT-REG-10` | Berkas dengan 1 baris salah | Unggah | Seluruh berkas ditolak, baris salah disebut | AST-06 |
| `UAT-REG-11` | Data campuran | Bandingkan kartu dashboard dengan daftar | Angka sama | AST-07 |
| `UAT-REG-12` | Kepala Unit IGD | Buka laporan per unit dan detail aset ICU | Laporan hanya IGD; detail ICU ditolak | AST-07 |
| `UAT-REG-13` | Pengaturan lampiran kosong | Unggah foto kondisi | Pesan pengaturan belum diisi | AST-01 |

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Registry `Ast` terdaftar dan verifier QBE lulus | Laporan task registry |
| Delapan master terisi minimum | `02-backend-architecture.md` bagian 9 + tangkapan layar master |
| Satu aset dapat berjalan dari draft sampai ditempatkan | `UAT-REG-01`, `UAT-REG-03` |
| Pengaju tidak dapat menyetujui sendiri di semua persetujuan | `AT-REG-31`, `AT-REG-44` |
| Rupiah tidak terkirim ke Kepala Unit | `AT-REG-12`, `AT-REG-47` |
| Impor semua-atau-tidak terbukti | `UAT-REG-09`, `UAT-REG-10`, `AT-REG-45` |
| Semua layar terjangkau dari menu atau layar induk | `AT-REG-49` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-0` | `EPIC AST-01` + tabel dasar `nilai` + `EPIC AST-11` Parameter nilai | Blueprint disetujui |
| `MVP-1` | `EPIC AST-02` s.d. `AST-07` | `MVP-0` selesai |
| `POST-MVP` | Bagian 8 | — |

| Pertanyaan | Siapa | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| `AST-OQ-001` batas kapitalisasi awal | Kepala Akuntansi | Persetujuan aset pertama tidak dapat menentukan kapitalisasi | Ya untuk go-live, tidak untuk pengerjaan |
| `AST-OQ-002` umur dan residu per kelompok | Kepala Akuntansi | Impor tidak dapat divalidasi | Ya untuk impor, tidak untuk pengerjaan |
| `AST-OQ-004` batas nilai pemanfaatan untuk Direktur | Rizki/Direktur | Penyetuju pemanfaatan tidak dapat dihitung | Ya untuk go-live pemanfaatan |
| `AST-OQ-006` tanggal awal | Rizki | Impor ditolak `AST-VAL-REG-068` | Ya untuk impor |
| `AST-OQ-015` letak grup menu | Rizki | Menu memakai letak sementara | Tidak |
| `AST-OQ-016` ukuran label | Bagian Aset | Label memakai ukuran umum | Tidak |

Tidak ada pertanyaan yang memblokir **pengerjaan** sub-modul ini.
