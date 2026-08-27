# Matriks Validasi — Modul IGD

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.4.0` — revisi 6. **Bukan aditif**: bagian 5 aturan 2 dan 4 berubah teksnya. Bagian 2 aturan 4-5 yang `approved` isinya utuh. Lihat manifest 0a.2 |
| Status | `draft`, **kecuali bagian 2 aturan 4 dan 5 yang `approved`** |
| Owner | Product/Domain Owner IGD: **Rizki Gunawan** (`IGD-DEC-089`) |
| `approved_by` / `approved_at` | **Rizki Gunawan / 2026-08-24** — terbatas pada bagian 2 aturan 4 dan 5 lewat `IGD-DEC-093`. Seluruh aturan lain tetap `draft` |
| Versi sebelumnya | `0.3.0`, sebelumnya `0.2.0` |

Aturan penulisan pesan: pesan penolakan **wajib** menyebut apa yang salah dan apa yang harus
dilakukan petugas. Pesan yang hanya menyebut nama kolom teknis dianggap belum selesai.

---

## 1. Pendaftaran kunjungan IGD

| No | Aturan | Kode | Pesan | Keputusan |
| ---: | --- | :-: | --- | --- |
| 1 | Unit pelayanan harus IGD sesuai pengaturan aktif | `400` | "Asal kunjungan harus IGD." | Sudah ada |
| 2 | Jenis kunjungan wajib `Emergency` | `400` | "Jenis kunjungan pasien IGD harus Gawat Darurat." | `IGD-DEC-074` |
| 3 | Pasien tanpa identitas wajib punya nama sementara | `400` | "Nama sementara wajib diisi untuk pasien yang belum diketahui identitasnya." | Sudah ada |
| 4 | Pasien tidak boleh punya dua kunjungan IGD aktif | `409` | "Pasien ini masih memiliki kunjungan IGD aktif bernomor {nomor}, tiba pukul {waktu}. Buka kunjungan tersebut, jangan mendaftar ulang." | `IGD-DEC-084` |
| 5 | Master kelas pasien IGD wajib ada dan tepat satu | `400` | Bila kosong: "Master kelas pasien untuk IGD belum diisi. Hubungi penanggung jawab data master." Bila lebih dari satu: "Ditemukan lebih dari satu master kelas pasien IGD. Rapikan data master agar pemilihan tarif tidak ambigu." | `IGD-DEC-076` |
| 6 | Satu encounter hanya boleh punya satu kunjungan IGD | `409` | "Kunjungan ini sudah memiliki kunjungan IGD." | Sudah ada |
| 7 | Nilai kelas pasien yang dikirim pemanggil **diabaikan** | — | Tidak menolak; backend menetapkan sendiri | `IGD-DEC-076` |

### 1.1 Jalan keluar beralasan untuk aturan 4

`IGD-DEC-084` mensyaratkan tersedianya jalan keluar. Bentuknya: field `duplicateOverrideReason`
pada request. Bila terisi, aturan 4 dilewati dan alasannya disimpan beserta pelaku dan waktu
server. Pemakaian jalan keluar **wajib** muncul pada daftar pantau.

Aturan 4 **tidak pernah** menahan penanganan klinis; yang tertahan hanya pembuatan kunjungan
kedua.

---

## 2. Triase

| No | Aturan | Kode | Pesan | Keputusan |
| ---: | --- | :-: | --- | --- |
| 1 | Kunjungan harus ada dan belum tertutup | `400` | "Kunjungan IGD tidak ditemukan atau sudah ditutup." | Sudah ada |
| 2 | Target waktu yang belum ditetapkan **tidak** dianggap nol menit | — | Tidak menolak; `ResponseDueAt` dibiarkan kosong | `IGD-DEC-027` |
| 3 | Hanya penilaian `Completed` yang dapat dinilai ulang | `409` | "Hanya penilaian yang sudah selesai yang dapat dinilai ulang." | Sudah ada |
| 4 | Menyelesaikan triase pada kunjungan tertutup ditolak | `409` | "Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan." | **Baru** — `IGD-GAP-014` |
| 5 | Perubahan status kunjungan akibat triase wajib transisi yang sah | `409` | "Penilaian ini tidak dapat mengubah status kunjungan dari {status}." | **Baru** — `IGD-GAP-014`; artinya dipertegas `IGD-DEC-104` |

### 2.1 Arti aturan 5 — dipertegas `IGD-DEC-104`

Teks aturan 4 dan 5 di atas **tidak berubah** dan tetap berstatus `approved` sejak
`IGD-DEC-093`. Yang ditambahkan di sini adalah artinya, yang ditetapkan Product/Domain Owner
pada 26 Agustus 2026 setelah implementasi `BE-IGD-019` memunculkan dua pembacaan.

> Setiap perubahan status yang **benar-benar dilakukan** akibat triase wajib melewati
> `CanTransition`. Penilaian ulang yang **tidak mengubah status** bukan transisi ilegal.

Penerapannya per status kunjungan:

| Status kunjungan | Perlakuan | Kode |
| --- | --- | :-: |
| `WaitingForTriage` | Menjadi `Triaged` lewat `CanTransition` | `200` |
| `Triaged`, `InTreatment`, `UnderObservation`, `AwaitingDisposition` | Penilaian **disimpan**, status **tetap**. Sistem **tidak mencoba** mengubahnya | `200` |
| `Disposed`, `Completed`, `Cancelled` | Ditolak — aturan 4 | `409` |
| **`Arrived`** | Perubahan status memang diminta, tetapi melompati `WaitingForTriage` tidak sah | **`409`** |

**Yang membedakan bukan kunjungan terbuka lawan tertutup, melainkan sudah lawan belum melewati
tahap triase.** `Arrived` adalah satu-satunya status tempat kedua pembacaan itu berbeda
hasilnya: pembacaan yang lebih longgar akan membiarkannya berhasil diam-diam dengan status
tetap `Arrived`, menyembunyikan transisi ilegal.

Semantik `AT-IGD-086`, skenario UAT "Ny. Sari sedang ditangani" pada `04-prd-to-mvp.md`, dan
`IGD-DEC-083` **dipertahankan seluruhnya**. Nol acceptance test yang perlu ditinjau ulang.

---

## 3. Penetapan dokter

| No | Aturan | Kode | Pesan | Keputusan |
| ---: | --- | :-: | --- | --- |
| 1 | Dokter harus ada dan aktif | `400` | "Dokter tidak ditemukan atau tidak aktif." | Sudah ada |
| 2 | Kunjungan yang sudah punya dokter aktif menolak penetapan baru | `409` | "Kunjungan ini sudah memiliki dokter penanggung jawab. Gunakan aksi pengalihan dokter." | `IGD-DEC-082` |
| 3 | Pengalihan wajib menyertakan alasan | `400` | "Alasan pengalihan dokter wajib diisi." | `IGD-DEC-082` |
| 4 | Waktu berlaku tidak boleh mendahului kedatangan pasien | `400` | "Waktu penugasan tidak boleh lebih awal dari waktu kedatangan pasien." | `IGD-DEC-082` |
| 5 | Pencabutan dokter tanpa pengganti | `400` | "Dokter penanggung jawab tidak dapat dicabut tanpa pengganti selama kunjungan masih berjalan." | `IGD-DEC-082` |

---

## 4. Kepergian pasien dan serah terima

| No | Aturan | Kode | Pesan | Keputusan |
| ---: | --- | :-: | --- | --- |
| 1 | Unit tujuan wajib berbeda dari unit asal | `400` | "Unit tujuan harus berbeda dengan unit asal." | Sudah ada |
| 2 | Empat bagian SBAR wajib terisi atau ditandai tidak dapat diisi | `400` | "Bagian {nama bagian} belum diisi. Isi, atau tandai tidak dapat diisi beserta alasannya." | `IGD-DEC-079` |
| 3 | Penandaan tidak dapat diisi wajib beralasan | `400` | "Alasan wajib diisi untuk bagian yang ditandai tidak dapat diisi." | `IGD-DEC-056`, `IGD-DEC-079` |
| 4 | Penolakan serah terima wajib menyebut bagian yang kurang | `400` | "Sebutkan bagian mana yang dianggap kurang." | `IGD-DEC-079` |
| 5 | Mencatat kedatangan wajib berwenang atas unit tujuan | `403` | "Anda tidak bertugas di unit tujuan, sehingga tidak dapat mencatat kedatangan pasien." | `IGD-DEC-064`, `IGD-DEC-086` |
| 6 | Menerima serah terima wajib berwenang atas unit tujuan | `403` | Pesan serupa | `IGD-DEC-086` |
| 7 | Dokumen `Accepted` sementara fisik belum `Departed` ditolak | `409` | "Serah terima tidak dapat diterima sebelum pasien berangkat dari IGD." | `IGD-DEC-070` |
| 8 | Fisik `Arrived` sementara dokumen `Pending` **diterima** | — | Tidak menolak. Ini keadaan sah | `IGD-DEC-070` |
| 9 | Waktu kedatangan jauh dari waktu server wajib menyebut rujukan catatan downtime | `400` | "Kedatangan yang dicatat menyusul wajib menyertakan rujukan catatan manual." | `IGD-DEC-065` |
| 10 | Waktu kejadian tidak boleh di masa depan | `400` | "Waktu kejadian tidak boleh melampaui waktu sekarang." | `IGD-DEC-065` |
| 11 | Pembalikan kejadian wajib disetujui orang kedua | `403` | "Pembalikan kejadian memerlukan persetujuan petugas lain." | `IGD-DEC-066` |
| 12 | Penyetuju pembalikan wajib berbeda dari pengaju | `400` | "Pemberi persetujuan harus berbeda dari pengaju." | `IGD-DEC-066` |
| 13 | Pembatalan kepergian wajib beralasan | `400` | "Alasan pembatalan wajib diisi." | `IGD-DEC-069` |

### 4.1 Yang tidak pernah ditahan

| Tindakan | Ditahan oleh dokumen? |
| --- | :-: |
| `POST /{id}/depart` | **Tidak** |
| `POST /{id}/arrive` | **Tidak** |
| Tindakan klinis apa pun | **Tidak** |

`IGD-DEC-055`, `IGD-DEC-070`, dan `IGD-DEC-078` sama-sama melarang dokumentasi menahan
pelayanan. Yang tertahan hanya pengajuan dokumen serah terima.

---

## 5. Sikap atas pesanan yang belum selesai

Diperluas revisi 6 oleh `IGD-DEC-100`, `101`, `102`, dan `103`.

| No | Aturan | Kode | Pesan | Keputusan |
| ---: | --- | :-: | --- | --- |
| 1 | Setiap pesanan wajib punya tepat satu sikap sebelum dokumen diajukan | `400` | "Masih ada {n} pesanan yang belum ditentukan sikapnya." | `IGD-DEC-078` |
| 2 | Sikap **`Cancel`** wajib beralasan | `400` | "Alasan pembatalan pesanan wajib diisi." | `IGD-DEC-078`, `IGD-DEC-100` |
| 3 | Sikap yang sudah tersimpan tidak dapat diubah tanpa jejak | `409` | "Sikap atas pesanan ini sudah tercatat. Perubahannya dicatat sebagai koreksi." | `IGD-DEC-078` |
| 4 | Pemeriksaan penunjang **tidak** ikut dihitung otomatis | — | Tidak menolak. Layar **wajib** menampilkan keterangan bahwa penunjang belum dapat dihitung sistem | `IGD-DEC-087` |
| 5 | **Sikap pesanan laboratorium ditetapkan manual klinisi.** Sistem **dilarang** menampilkan atau menyimpan sikap itu seolah berasal dari `LabOrder` | — | Layar wajib menyatakan bahwa sikap ini ditetapkan petugas, bukan dibaca dari sistem laboratorium | **`IGD-DEC-101`** |
| 6 | Setiap penetapan sikap wajib menyimpan **pelaku, waktu, dan alasan** | `400` | "Penetapan sikap pesanan wajib menyertakan petugas dan waktunya." | **`IGD-DEC-101`** |
| 7 | Pesanan `External` wajib punya `ExternalReference` **dan** `OrderDescription` | `400` | "Pesanan di luar sistem wajib menyertakan nomor rujukan dan uraiannya." | **`IGD-DEC-103`** |
| 8 | Pesanan `Internal` wajib punya `OrderReferenceId` | `400` | "Pesanan internal wajib menunjuk pesanan yang ada." | **`IGD-DEC-103`** |
| 9 | Sikap **`Cancel` hanya untuk pesanan yang belum dimulai**, dan hanya oleh klinisi berwenang | `409` | "Pesanan yang sudah dikerjakan tidak dapat dibatalkan; tetapkan Continue atau Handover." | **`IGD-DEC-100`** butir (a) dan (c) |
| 10 | Sikap `Handover` menuntut unit penerima, dan menunggu **penerimaan eksplisit** | `400` | "Sikap serah terima wajib menyebutkan unit penerima." | **`IGD-DEC-102`** |
| 11 | Penolakan sebuah pesanan **tidak** membatalkan penerimaan pasien | — | Tidak menolak. Perpindahan pasien tetap sah | **`IGD-DEC-102`** butir (a) dan (d) |
| 12 | Pesanan yang **ditolak** unit penerima wajib diberi **sikap pengganti** sebelum kunjungan ditutup | `409` | "Ada pesanan yang ditolak unit penerima dan belum ditetapkan sikap penggantinya." | **`IGD-DEC-102`** butir (b) dan (c) |
| 13 | `AcceptanceStatus` = `Rejected` wajib beralasan | `400` | "Alasan penolakan pesanan wajib diisi." | **`IGD-DEC-102`** |
| 14 | Sikap pengganti ditulis sebagai **baris baru** yang menunjuk baris lama; baris lama ditandai tidak berlaku dan **tidak dihapus** | — | — | **`IGD-DEC-102`**, mengikuti `IGD-DEC-090` |

### 5.1 Sikap `Continue` tidak menahan penutupan kunjungan

`Continue` berarti pesanan **memang sengaja** dibiarkan berjalan sampai hasil final meski
pasien sudah pergi — `IGD-DEC-100` butir (a). Karena itu ia **bukan** pesanan "tanpa sikap",
dan tidak boleh ikut menahan penutupan kunjungan pada bagian 6 aturan 4.

Yang menahan penutupan hanyalah pesanan **tanpa sikap sama sekali** (aturan 1) dan pesanan
**ditolak yang belum diberi sikap pengganti** (aturan 12).

### 5.2 Yang **tidak** ditegakkan sistem, dan sebabnya

`IGD-DEC-101` menetapkan sikap pesanan laboratorium ditetapkan manual. Sebabnya bukan pilihan
desain melainkan keterbatasan data: `LabOrder` **tidak memiliki kolom status sama sekali**,
sehingga sistem tidak dapat membedakan pesanan yang spesimennya sudah diambil dari yang belum
dimulai. Aturan 9 karena itu **tidak dapat ditegakkan otomatis** untuk laboratorium — klinisi
yang menentukan, dan sistem mencatat siapa serta kapan.

Otomatisasi menyusul setelah pemilik `LaboratoryManagement` melengkapi lifecycle pesanan.
Sampai saat itu, aturan 5 melarang sistem berpura-pura tahu.

---

## 6. Penyelesaian kunjungan

| No | Aturan | Kode | Pesan |
| ---: | --- | :-: | --- |
| 1 | Status wajib `Disposed` | `409` | "Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan." |
| 2 | Tidak boleh ada observasi `Active` | `409` | "Masih ada observasi yang belum diselesaikan." |
| 3 | Tidak boleh ada kepergian yang fisiknya belum `Arrived` atau `Cancelled` | `409` | "Masih ada proses kepergian pasien yang belum selesai." |
| 4 | Tidak boleh ada pesanan tanpa sikap | `409` | "Masih ada pesanan yang belum ditentukan sikapnya." |
| 5 | Status tagihan **tidak** diperiksa | — | Sesuai `IGD-DEC-021` |

---

## 7. Kewenangan unit

| No | Aturan | Kode | Keputusan |
| ---: | --- | :-: | --- |
| 1 | Pengguna wajib punya penugasan organisasi yang sedang berlaku pada simpul organisasi milik unit | `403` | `IGD-DEC-086` |
| 2 | Penugasan yang `EffectiveEndDate`-nya sudah lewat tidak memberi kewenangan | `403` | `IGD-DEC-086` |
| 3 | Unit yang kolom simpul organisasinya kosong | **Perilaku wajib disengaja dan tertulis** | `IGD-DEC-086` butir 4 |
| 4 | Kewenangan unit **tidak** dengan sendirinya memberi kemampuan klinis | `403` | `IGD-DEC-058` |
| 5 | Pelayanan klinis darurat **tidak pernah** diblokir ketiadaan penugasan | — | `IGD-DEC-086` butir 7 |

> **Aturan 3 belum diputuskan.** Dua kemungkinan: menolak semua orang (fail-closed) atau
> mengizinkan semua orang (fail-open). Untuk data master yang belum lengkap, fail-closed
> menghentikan pelayanan dan fail-open menghapus penjagaan. Keduanya buruk, dan pilihannya
> milik Security/Privacy owner. Dicatat sebagai `IGD-OQ-071`.
