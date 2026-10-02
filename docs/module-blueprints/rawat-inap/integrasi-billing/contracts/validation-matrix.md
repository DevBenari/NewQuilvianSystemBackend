# Matriks Validasi (Validation Matrix) — Integrasi Rawat Inap ↔ Billing

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| `contract_version` | **`1.0.0`** |
| Status | **`draft`** |

---

## 1. Matriks Validasi Bisnis & Pesan Penolakan Sistem

| Kode Validasi | Target Endpoint / Operasi Bisnis | Kondisi Pelanggaran | Aturan Acuan | HTTP Status | Pesan Kesalahan Sistem (Bahasa Indonesia) |
|---|---|---|---|:---:|---|
| **`VAL-INT-001`** | `ConfirmPhysicalDischargeCommand` | Perawat mencoba memulangkan pasien saat status clearance kasir masih `Pending` atau `Revoked` tanpa otorisasi override. | `RWI-DEC-158`, `RWI-AC-238` | `422 Unprocessable Entity` | *"Pelepasan fisik pasien ditolak: Tagihan kasir belum disetujui (Clearance Pending/Revoked). Pasien hanya dapat dilepaskan setelah kasir menerbitkan persetujuan lunas atau melalui otorisasi Supervisor Override."* |
| **`VAL-INT-002`** | `TransferBedCommand` / `CorrectOccupancyCommand` | Supervisor mencoba melakukan mutasi kamar atau koreksi kelas saat folio tagihan di kasir sudah berstatus `CLOSED` / `LOCKED`. | `RWI-DEC-157`, `RWI-AC-237` | `422 Unprocessable Entity` | *"Mutasi kamar ditolak: Tagihan kasir pasien sudah berstatus CLOSED. Data hunian kamar tidak dapat diubah kembali. Hubungi bagian Kasir/Keuangan bila diperlukan pembukaan kembali tagihan."* |
| **`VAL-INT-003`** | `TransferBedCommand` / `CorrectOccupancyCommand` | Kolom alasan mutasi atau koreksi kamar dikosongkan atau kurang dari 10 karakter. | `RWI-DEC-157`, `RWI-AC-237` | `400 Bad Request` | *"Alasan perubahan kamar wajib diisi minimal 10 karakter untuk keperluan jejak rekam audit."* |
| **`VAL-INT-004`** | `SupervisorOverrideCommand` | Kolom alasan kedaruratan klinis dikosongkan atau kurang dari 20 karakter. | `RWI-DEC-158`, `RWI-DEC-015` | `400 Bad Request` | *"Alasan supervisor override wajib diisi minimal 20 karakter dengan menyebutkan kondisi darurat medis atau rumah sakit rujukan secara jelas."* |
| **`VAL-INT-005`** | `SupervisorOverrideCommand` | Pengguna tidak memiliki peran Supervisor Bangsal atau PIN otorisasi salah. | `RWI-DEC-158`, `RWI-AC-238` | `403 Forbidden` | *"Otorisasi ditolak: Anda tidak memiliki hak wewenang Supervisor Rawat Inap atau PIN otorisasi yang dimasukkan salah."* |
| **`VAL-INT-006`** | `GetBillingDetailsQuery` | Pengguna mencoba mengakses endpoint rincian nominal rupiah tanpa permission `InpatientBilling:View`. | `RWI-DEC-160`, `RWI-AC-240` | `403 Forbidden` | *"Akses ditolak: Anda tidak memiliki hak akses untuk melihat rincian finansial dan nominal rupiah tagihan rawat inap."* |
| **`VAL-INT-007`** | `EnqueueOutboxCommand` | Kunci idempoten outbox kosong atau tidak mematuhi format `SourceDomain:SourceType:SourceDetailId:Version`. | `RWI-DEC-161`, `RWI-AC-241` | `400 Bad Request` | *"Format IdempotencyKey tidak valid. Kunci harus mengikuti format baku gabungan domain, tipe, ID detail, dan versi."* |
| **`VAL-INT-008`** | `ConfirmPhysicalDischargeCommand` | Jam kepulangan fisik (`PhysicallyLeftAt`) dimasukkan lebih awal dari jam masuk admisi atau jam mulai hunian kamar terakhir. | `RWI-DEC-159`, `RWI-AC-239` | `422 Unprocessable Entity` | *"Jam kepulangan fisik tidak valid: Waktu keluar fisik tidak boleh mendahului waktu pasien mulai menempati tempat tidur."* |

---

## 2. Perubahan pada `contract_version` `1.1.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `1.1.0` |
| Status | **`draft`** |
| Traceability | `FR-RWF-005` s.d. `008`, `014`, `017`, `019`, `022`, `024`; `RWI-DEC-167`, `186`, `187`, `192` |

Baris bagian 1 yang menyangkut webhook, supervisor override pulang fisik, PIN, dan syarat kasir pada pulang fisik **dicabut**.

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-RWF-01` | Keluar ruangan | Status kasir bukan `CLEARED` atau tidak terbaca, dan peringatan belum diakui | "Kasir belum memberi izin pulang. Pastikan keluarga sudah diarahkan ke kasir, lalu konfirmasi bila pasien tetap meninggalkan ruangan." | `409` `INP-DEP-001` |
| `VAL-RWF-02` | Keluar ruangan | Status kasir tidak dapat dibaca (bagian dari `VAL-RWF-01`) | "Status kasir tidak dapat dibaca saat ini. Kepergian tetap dapat dicatat, dan episode ini akan masuk daftar pulang sebelum izin kasir." | `409` `INP-DEP-001` |
| `VAL-RWF-03` | Penutupan normal | Bacaan langsung bukan `CLEARED` | "Episode belum dapat ditutup: kasir belum memberi izin." | `422` `INP-CLS-010` |
| `VAL-RWF-04` | Penutupan normal | Billing tidak dapat dibaca | "Status kasir tidak dapat dibaca. Coba lagi beberapa saat, atau minta supervisor menutup dengan alasan." | `422` `INP-CLS-011` |
| `VAL-RWF-05` | Penutupan dengan override | Alasan kosong atau hanya tanda baca | "Alasan penutupan tanpa izin kasir wajib diisi dengan kalimat yang jelas." | `400` `INP-CLS-012` |
| `VAL-RWF-06` | Penutupan dengan override | Tanpa permission `InpatientDischarge : CloseOverride` | "Anda tidak berhak menutup episode tanpa izin kasir." | `403` |
| `VAL-RWF-07` | Koreksi penempatan | Invoice rawat inap bukan `OPEN` | "Penempatan tidak dapat dikoreksi karena tagihan rawat inap sudah difinalkan. Hubungi kasir untuk penyesuaian." | `422` `INP-COR-001` |
| `VAL-RWF-08` | Koreksi penempatan | Status invoice tidak dapat dibaca | "Status tagihan tidak dapat dibaca. Koreksi penempatan belum dapat disimpan." | `422` `INP-COR-002` |
| `VAL-RWF-09` | Koreksi penempatan | Versi berubah sejak layar dibuka | "Data penempatan sudah diubah pengguna lain. Muat ulang lalu coba lagi." | `409` `INP-COR-003` |
| `VAL-RWF-10` | Koreksi penempatan | Bed tujuan tidak lolos kelayakan penempatan | Pesan kelayakan yang sudah ada pada transfer, misalnya aturan jenis kelamin atau isolasi | `422` `INP-COR-004` |
| `VAL-RWF-11` | Koreksi penempatan | Tidak ada satu pun field koreksi | "Pilih sekurang-kurangnya satu hal yang dikoreksi: bed, kelas, atau waktu." | `400` |
| `VAL-RWF-12` | Koreksi penempatan | Waktu koreksi menimpa penempatan lain pada episode yang sama, atau mendahului admisi | "Waktu koreksi bertabrakan dengan penempatan lain pada episode ini." | `400` |
| `VAL-RWF-13` | Pendaftaran outbox | Payload memuat field di luar daftar putih | Tidak tampil ke pengguna; dicatat sebagai kesalahan pemrograman | `500`, transaksi dibatalkan |
| `VAL-RWF-14` | Putar ulang | `Reason` kosong | "Alasan putar ulang wajib diisi." | `400` |
| `VAL-RWF-15` | Penyelesaian pemeriksaan invoice | Masih ada biaya kamar manual dan otomatis yang sama-sama aktif | "Masih ada biaya kamar yang dicatat manual bersamaan dengan hitungan otomatis. Batalkan salah satunya lebih dulu." | `422` `BIL-REV-001` |
| `VAL-RWF-16` | Finalisasi invoice | Invoice "perlu diperiksa" | "Invoice ini perlu diperiksa sebelum difinalkan." | `422` `BIL-FIN-020` |
| `VAL-RWF-17` | Finalisasi invoice | Masih ada baris "tarif belum ada" | "Masih ada layanan yang tarifnya belum diatur. Lengkapi master tarif sebelum memfinalkan." | `422` `BIL-FIN-021` |
| `VAL-RWF-18` | Rupiah tagihan di bangsal | Pemanggil `…/amounts` tanpa `PatientBillingSummary : ViewAmount` | "Anda tidak berhak melihat nominal tagihan." Layar tidak memanggil endpoint ini bila pengguna tidak berhak | `403` |
