# Accounting — State Transition Matrix

| Field | Value |
|---|---|
| `contract_version` | `ACC-STATE-0.1` |
| Status | `draft` — approval adalah tindakan manusia |
| Owner | Rizki (Product/Domain Owner Accounting) |
| `approved_by` / `approved_at` | Belum ada |
| `input_revision` | `00-interview-decisions.md@3`, `02-backend-architecture.md@3` |
| Traceability | `ACC-DEC-006`, `ACC-DEC-010`, `ACC-DEC-012`, `ACC-DEC-015`, `ACC-DEC-016`, `ACC-DEC-017`, `ACC-DEC-025`, `ACC-DEC-026`, `ACC-DEC-027`, `ACC-DEC-028`, `ACC-DEC-029` |
| Dampak kompatibilitas | Kontrak baru, tidak menggantikan kontrak mana pun |

## 1. Status jurnal

### 1.1 Perpindahan yang sah

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat | `Draft` | Accounting Staff | Punya hak `Journal : Create`; badan hukum dan jenis jurnal terisi | `403` bila tidak berhak; `400` bila isian kurang |
| `Draft` | Ubah | `Draft` | Pembuatnya, atau Accounting Staff lain yang berhak | Jurnal masih `Draft` | `409` — jurnal sudah tidak dapat disunting |
| `Draft` | Hapus | *(terhapus)* | Pembuatnya | Jurnal masih `Draft` | `409` — hanya draft yang boleh dihapus |
| `Draft` | Ajukan | `PendingApproval` | Accounting Staff | Sembilan syarat pengajuan pada bagian 1.3 terpenuhi | `400` atau `422` sesuai syarat yang gagal |
| `PendingApproval` | Setujui | `Approved` | Accounting Approver | Penyetuju **bukan** pembuat jurnal (`ACC-DEC-016`) | `403` — jurnal buatan sendiri tidak boleh disetujui sendiri |
| `PendingApproval` | Tolak | `Rejected` | Accounting Approver | Alasan penolakan wajib diisi | `400` bila alasan kosong |
| `Rejected` | Sunting kembali | `Draft` | Pembuatnya | — | — |
| `Approved` | Sahkan | `Posted` | Accounting Manager | Periode masih menerima jenis jurnal ini; keseimbangan dihitung ulang dan tetap seimbang | `422` bila periode menolak; `400` bila ternyata tidak seimbang |
| `Approved` | Tolak | `Rejected` | Accounting Manager | Alasan wajib diisi | `400` bila alasan kosong |
| `Posted` | Balik | `Posted` *(tetap)* + jurnal pembalik baru berstatus `PendingApproval` | Accounting Manager | Alasan wajib; periode tujuan jurnal pembalik masih menerima penyesuaian | `422` bila periode menolak |

Perhatikan baris terakhir: membalik jurnal **tidak mengubah status jurnal asal**. Jurnal asal
tetap `Posted` dan isinya tetap utuh, sesuai `ACC-DEC-006`. Yang lahir adalah jurnal baru.

### 1.2 Perpindahan yang TIDAK sah

Bagian ini sama pentingnya dengan bagian sebelumnya. Semua yang tidak terdaftar di bagian 1.1
adalah tidak sah, dan yang paling mungkin dicoba adalah berikut ini.

| Percobaan | Kenapa ditolak | Kode | Pesan bagi pengguna |
|---|---|---|---|
| `Posted` → Ubah | `ACC-DEC-006`, riwayat yang sudah disahkan permanen | `409` | "Jurnal yang sudah disahkan tidak dapat diubah. Gunakan pembalikan atau jurnal penyesuaian." |
| `Posted` → Hapus | Sama seperti di atas, termasuk lewat penandaan `IsDelete` | `409` | "Jurnal yang sudah disahkan tidak dapat dihapus." |
| `Draft` → Sahkan | Melompati persetujuan | `409` | "Jurnal harus diajukan dan disetujui lebih dahulu." |
| `PendingApproval` → Sahkan | Melompati persetujuan | `409` | "Jurnal belum disetujui." |
| `PendingApproval` → Ubah | Sedang dinilai penyetuju | `409` | "Jurnal sedang menunggu persetujuan dan tidak dapat diubah." |
| `Approved` → Ubah | Sudah disetujui, isinya tidak boleh berubah diam-diam | `409` | "Jurnal sudah disetujui dan tidak dapat diubah. Minta penolakan lebih dahulu bila perlu diperbaiki." |
| `Rejected` → Setujui | Harus disunting dan diajukan ulang | `409` | "Jurnal yang ditolak harus diperbaiki dan diajukan kembali." |
| Menyetujui jurnal sendiri | `ACC-DEC-016`, tanpa pengecualian | `403` | "Anda tidak dapat menyetujui jurnal yang Anda buat sendiri." |
| `Posted` → Balik dua kali | Satu jurnal hanya boleh dibalik sekali | `409` | "Jurnal ini sudah pernah dibalik dengan jurnal {nomor}." |
| Membalik jurnal yang belum `Posted` | Tidak ada yang perlu dibalik | `409` | "Hanya jurnal yang sudah disahkan yang dapat dibalik." |

### 1.3 Sembilan syarat pengajuan

Diperiksa saat `Draft` → `PendingApproval`, dan **diperiksa ulang** saat `Approved` → `Posted`.

| No | Syarat | Keputusan asal |
|---:|---|---|
| 1 | Total debit sama persis dengan total kredit | `ACC-DEC-021` |
| 2 | Jurnal punya sekurang-kurangnya dua baris | Turunan aturan keseimbangan |
| 3 | Setiap baris mengisi tepat satu sisi, debit atau kredit, dan nilainya lebih besar dari nol | Struktur baris |
| 4 | Seluruh akun yang dipakai berstatus aktif | `ACC-DEC-024` |
| 5 | Seluruh akun yang dipakai menerima transaksi | `ACC-DEC-022` |
| 6 | Seluruh akun yang dipakai milik badan hukum yang sama dengan jurnalnya | `ACC-DEC-037` |
| 7 | Baris yang akunnya berjenis beban wajib menyebutkan Cost Center | `ACC-DEC-019` |
| 8 | Cost Center yang dipilih aktif dan milik badan hukum yang sama | `ACC-DEC-019`, `ACC-DEC-037` |
| 9 | Periode akuntansi tujuan menerima jenis jurnal ini | `ACC-DEC-012` |

Syarat 9 sengaja diperiksa dua kali. Periode bisa saja ditutup di antara pengajuan dan
pengesahan, dan pemeriksaan kedua inilah yang mencegah jurnal masuk ke periode yang sudah
terkunci.

## 2. Status periode akuntansi

### 2.1 Perpindahan yang sah

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Bangkitkan setahun | `Open` | Accounting Administrator | Tahun buku itu belum pernah dibangkitkan untuk badan hukum tersebut | `409` — periode sudah ada |
| `Open` | Tutup sementara | `SoftClosed` | Accounting Manager | — | `403` bila bukan Manager |
| `Open` | Tutup permanen | `Closed` | Accounting Manager | — | `403` bila bukan Manager |
| `SoftClosed` | Tutup permanen | `Closed` | Accounting Manager | — | `403` bila bukan Manager |
| `SoftClosed` | Buka kembali | `Open` | Accounting Manager | Alasan tertulis wajib | `400` bila alasan kosong |
| `Closed` | Buka kembali | **`SoftClosed`** | Accounting Manager | Alasan tertulis wajib | `400` bila alasan kosong |

Baris terakhir adalah bagian terpenting pada tabel ini. Periode yang sudah tutup permanen
**tidak** kembali ke Terbuka, melainkan ke Tutup Sementara. Ini yang mewujudkan `ACC-DEC-028`:
setelah dibuka kembali, hanya penyesuaian dan pembalikan yang boleh masuk, bukan jurnal
operasional baru.

Tidak ada batas waktu pembukaan kembali. Owner memilih cukup dengan alasan tertulis yang tercatat
di jejak audit.

### 2.2 Apa yang diterima setiap status

| Status periode | Jurnal Umum `JU` | Jurnal Penyesuaian `JP` | Jurnal Pembalik `JB` | Saldo Awal `SA` |
|---|:---:|:---:|:---:|:---:|
| `Open` — Terbuka | Diterima | Diterima | Diterima | Diterima |
| `SoftClosed` — Tutup Sementara | **Ditolak** | Diterima | Diterima | **Ditolak** |
| `Closed` — Tutup Permanen | **Ditolak** | **Ditolak** | **Ditolak** | **Ditolak** |

Pesan penolakan menyebut nama periodenya, bukan istilah teknis. Contoh:
"Periode September 2026 sudah ditutup sementara. Hanya jurnal penyesuaian dan pembalikan yang
masih dapat disahkan."

### 2.3 Perpindahan yang TIDAK sah

| Percobaan | Kenapa ditolak | Kode | Pesan bagi pengguna |
|---|---|---|---|
| `Closed` → `Open` langsung | Melanggar `ACC-DEC-028` | `409` | "Periode yang sudah ditutup permanen hanya dapat dibuka kembali menjadi tutup sementara." |
| Tutup periode oleh selain Manager | `ACC-DEC-026` | `403` | "Hanya Manajer Akuntansi yang dapat menutup periode." |
| Buka kembali tanpa alasan | `ACC-DEC-027` | `400` | "Alasan pembukaan kembali wajib diisi." |
| Bangkitkan ulang tahun buku yang sudah ada | Akan menghasilkan periode ganda | `409` | "Periode tahun buku {tahun} sudah pernah dibuat untuk badan hukum ini." |
| Hapus periode | Periode adalah kerangka pembukuan, bukan data yang boleh hilang | `409` | "Periode akuntansi tidak dapat dihapus." |

## 3. Status daftar akun

Daftar akun tidak punya alur berstatus banyak. Yang ada hanya penanda aktif.

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat | `IsActive = true` | Accounting Administrator | Kode belum dipakai badan hukum yang sama | `409` — kode akun sudah dipakai |
| Aktif | Nonaktifkan | `IsActive = false` | Accounting Administrator | Saldo akun tepat nol (`ACC-DEC-024`) | `409` — "Akun masih bersaldo {jumlah} dan tidak dapat dinonaktifkan." |
| Nonaktif | Aktifkan kembali | `IsActive = true` | Accounting Administrator | — | — |
| Aktif | Ubah kode | Kode baru | Accounting Administrator | Belum ada baris jurnal `Posted` yang memakainya (`ACC-DEC-023`) | `409` — "Kode akun tidak dapat diubah karena sudah dipakai pada jurnal yang disahkan." |
| Aktif | Jadikan menerima transaksi | `IsPostable = true` | Accounting Administrator | Akun tidak punya anak (`ACC-DEC-022`) | `409` — "Akun induk tidak dapat menerima transaksi." |


---

# PHASE 2 (`ACC-PH-006`) — Rencana, belum tersedia

| Field | Nilai |
|---|---|
| `contract_version` | `ACC-STATE-0.3` — 10 September 2026, `ACC-DEC-067` menetapkan periode `SoftClosed` menerima jurnal `JT`. Sebelumnya `0.2` |
| `last_changed_in` | `ACC-STATE-0.2` — 8 September 2026 |
| Status | **`approved`** |
| `approved_by` / `approved_at` | Rizki / 8 September 2026 |
| Traceability | `ACC-DEC-045`, `046`, `047`, `049`, `052`, `055` |

## 1. Kejadian keuangan (`AccountingEventStatus`)

Status awal: **`Diterima`**. Status akhir: `Terjurnal` dan `Diabaikan`.

| Dari | Ke | Pemicu | Wewenang | Prasyarat |
|---|---|---|---|---|
| — | `Diterima` | Pesan masuk dari Finance | `AccountingEvent : Receive` | Kedua belas bidang terisi, mata uang rupiah |
| `Diterima` | `Terjurnal` | Aturan posting ketemu, pemrosesan berhasil | Sistem | Periode menerima pencatatan; jurnal berhasil dibuat |
| `Diterima` | `Tertahan` | Aturan posting **tidak** ketemu | Sistem | — |
| `Diterima` | `Gagal` | Tiga percobaan otomatis habis | Sistem | `AttemptCount = 3` |
| `Tertahan` | `Terjurnal` | Akuntansi menambah aturan posting, lalu kejadian diproses ulang | Sistem atau `AccountingEvent : Retry` | Aturan posting untuk jenis itu kini ada dan aktif |
| `Gagal` | `Terjurnal` | Coba ulang manual berhasil | `AccountingEvent : Retry` | — |
| `Gagal` | `Diabaikan` | Akuntansi menyatakan kejadian tidak perlu dijurnal | `AccountingEvent : Ignore` | **Alasan tertulis wajib** |

### Perpindahan yang DILARANG

| Dari | Ke | Kenapa dilarang |
|---|---|---|
| `Terjurnal` | mana pun | Jurnal sudah terbentuk. Membatalkannya menuntut pembalikan jurnal, bukan pengubahan status kejadian |
| `Tertahan` | `Diabaikan` | Kejadian tertahan itu **sah** dan menunggu pemetaan. Mengabaikannya berarti kehilangan angka yang seharusnya masuk buku besar. Yang boleh diabaikan hanya yang `Gagal` |
| `Diabaikan` | mana pun | Status akhir. Bila keliru diabaikan, kejadian dikirim ulang oleh Finance dengan nomor baru |
| `Diterima` | `Diabaikan` | Belum pernah dicoba, jadi belum ada dasar menyatakannya tidak perlu dijurnal |

**Kenapa `Tertahan` tidak boleh langsung diabaikan** adalah aturan yang paling mudah salah
dirancang. Contohnya: kejadian penyusutan Rp 4.000.000 tertahan karena akun belum dipetakan.
Bila petugas boleh mengabaikannya untuk membersihkan layar, beban Rp 4.000.000 hilang dari
laporan tanpa jejak apa pun bahwa ia pernah ada.

## 2. Periode akuntansi (`AccountingPeriodStatus`) — diperluas

Nilai `PendingClosingApproval = 4` **ditambahkan di belakang**, bukan disisipkan. Menyisipkan di
tengah akan mengubah arti angka yang sudah tersimpan di database.

| Dari | Ke | Pemicu | Wewenang | Prasyarat |
|---|---|---|---|---|
| `Open` | `PendingClosingApproval` | Pengajuan penutupan | `Period : Close` (Accounting Manager) | **Nol penghalang** — kini **tiga**: jurnal belum disahkan, kejadian gagal, dan shift kasir belum ditutup (`ACC-DEC-051` diperluas `ACC-DEC-065`) |
| `PendingClosingApproval` | `SoftClosed` | Persetujuan penutupan | `Period : Approve` (**Director**) | Penyetuju **bukan** pengaju |
| `PendingClosingApproval` | `Open` | Penolakan penutupan | `Period : Approve` | Alasan tertulis wajib |
| `SoftClosed` | `Open` | Pembukaan kembali | `Period : Close` | Alasan tertulis wajib (`ACC-DEC-027`) |
| `SoftClosed` | `Closed` | Penutupan permanen | `Period : Close` | Sudah `SoftClosed` |

### Perpindahan yang DILARANG

| Dari | Ke | Kenapa dilarang |
|---|---|---|
| `Open` | `SoftClosed` | Melompati persetujuan. Inilah yang diubah `ACC-DEC-052` — pada MVP perpindahan ini sah, pada Phase 2 tidak lagi |
| `Closed` | mana pun | Tertutup permanen |
| `PendingClosingApproval` | `Closed` | Penutupan permanen hanya dari `SoftClosed` |

**Catatan kompatibilitas.** Periode yang sudah `SoftClosed` sebelum Phase 2 berdiri **tidak punya**
riwayat persetujuan, dan itu benar — mereka ditutup ketika aturannya memang belum ada. Sistem tidak
boleh menolak atau menandai mereka janggal.

## 3. Jurnal berulang — tidak punya status sendiri

Template hanya punya `IsActive`. Jurnal yang dihasilkannya adalah `AccJournal` biasa yang lahir
berstatus `Draft`, lalu menempuh daur hidup jurnal yang sudah ada (`ACC-STATE-0.1` bagian 1).
Tidak ada state machine kedua.

## 4. Jurnal penutup tahun — tidak punya status sendiri

Sama alasannya. Jurnal penutup lahir `Draft` berjenis `JT`, lalu diajukan, disetujui, dan
disahkan lewat jalur yang sudah ada. Koreksinya memakai pembalikan jurnal (`ACC-DEC-029`), bukan
mekanisme "buka kembali tahun buku" — **diratifikasi `ACC-DEC-068`, 10 September 2026, menutup
`DEC-ACC-P2-006`**.

### Periode mana yang menerima jurnal `JT` — `ACC-DEC-067`

| Status periode | Menerima `JT`? | Alasan |
|---|:---:|---|
| `Open` | Ya | Menerima seluruh jenis jurnal |
| `SoftClosed` | **Ya** | **Diratifikasi 10 September 2026.** Masa tenggang tutup buku adalah satu-satunya tempat jurnal penutup tahun dapat tinggal |
| `PendingClosingApproval` | Tidak | Menunggu persetujuan penutupan; tidak menerima jurnal apa pun |
| `Closed` | Tidak | Tutup permanen; tidak menerima jurnal apa pun, termasuk jurnal penutupnya sendiri |

**Kenapa `SoftClosed` wajib menerimanya.** Tutup tahun baru boleh disusun setelah seluruh periode
tahun itu tertutup, sementara tanggal akuntansi jurnal penutup wajib berada **di dalam** tahun
yang ditutup — yaitu tepat pada periode yang barusan ditutup. Menolak `JT` di sini membuat jurnal
penutup lahir sebagai draft yang **tidak akan pernah dapat diajukan maupun disahkan**, karena
syarat ke-9 `ACC-STATE-0.1` bagian 1.3 memeriksa aturan yang sama saat pengajuan dan pengesahan.

**Akibat yang perlu diketahui:** menutup Desember secara **permanen** sebelum tutup tahun
dijalankan akan mengunci tutup tahun selamanya. Urutan yang benar adalah tutup sementara seluruh
bulan, jalankan tutup tahun, baru tutup permanen.
