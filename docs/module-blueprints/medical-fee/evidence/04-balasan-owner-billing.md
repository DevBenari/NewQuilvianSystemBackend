# Balasan Owner Billing atas Permintaan Medical Fee

| Field | Nilai |
|---|---|
| Dari | Owner modul Billing dan Kasir (`BIL-CASH-001`) |
| Untuk | Yasmin, owner modul Medical Fee |
| Menjawab | `evidence/01-permintaan-untuk-owner-billing.md` — lima pertanyaan |
| Menutup | `MF-CQ-08` (pertanyaan 1–3) dan `MF-CQ-05` (pertanyaan 4–5) |
| Membuka | `BE-MDF-016` dan `BE-MDF-015` |
| Status pertanyaan | **`MF-CQ-08` dan `MF-CQ-05` DITUTUP 26 Oktober 2026** (`00-interview-decisions.md` bagian "Penutupan `MF-CQ-05` dan `MF-CQ-08`"). Keputusan Billing: `BKC-DEC-168`..`178`; desain Billing revisi `2.4` approved |
| Status | **Jawaban bagian 1 disetujui owner Billing; butir bagian 2.3 menunggu keputusan owner Billing.** Belum menjadi keputusan blueprint Billing (`BKC-DEC-*`) maupun task `BE-BKC-*` |
| Persetujuan owner Billing | Pemilik modul Billing, pernyataan langsung dalam sesi, 26 Oktober 2026. Pemilik memilih bentuk A, nol bila tanpa aturan, dan pelaksana wajib hanya bila ditandai berjasa, lalu menyatakan "saya owner billing, lanjutkan pekerjaannya untuk bagian billing". Persetujuan ini **bukan** otorisasi migration atau implementasi kode |

Berkas ini berdiri sendiri, dapat dibaca tanpa membuka blueprint Medical Fee.

---

## 1. Jawaban

| # | Pertanyaan | Jawaban |
|---:|---|---|
| 1 | Apakah `DoctorShare` tidak lagi diketik petugas, melainkan diisi dari hasil perhitungan Medical Fee? | **Setuju.** Bagian dokter pada item tagihan berasal dari aturan tarif Medical Fee, bukan dari isian petugas |
| 2 | Bentuk teknisnya? | **Bentuk A — Billing menarik.** Saat item dicatat, Billing memanggil endpoint `sharing-rules/resolve` milik Medical Fee. Billing tetap satu-satunya penulis `BilInvoiceItem` |
| 3 | Bila Medical Fee belum punya aturan tarif? | **`DoctorShare` diisi nol dan tagihan tetap dapat difinalisasi.** Tidak ada jalur ketik manual. Layanan itu tercatat di daftar belum dapat dihitung Medical Fee, sehingga tidak hilang tanpa jejak |
| 4 | Apakah Billing menambahkan rujukan pelaksana pada `ADHOC` dan `ADHOC_CATALOG`? | **Bersedia.** Kolom nullable pada `BilInvoiceItem`, diisi lewat kedua jalur entri kasir |
| 5 | Wajib untuk seluruh entri bebas? | **Tidak.** Wajib hanya bila kasir menandai layanan itu berjasa |

---

## 2. Syarat dan bentuk yang disepakati

### 2.1 Bentuk A (jawaban 2)

| Aspek | Ketentuan |
|---|---|
| Pemanggil | Billing, di dalam alur pencatatan item (`UpsertChargeAsync`) |
| Endpoint | `GET api/v1/health-services/medical-fee-management/sharing-rules/resolve`, bentuk pada `integration-contract.md` §4.3 |
| `resolved: false` | `DoctorShare = 0`, item tetap tercatat (jawaban 3) |
| Medical Fee tidak dapat dijangkau | Item tetap tercatat dengan `DoctorShare = 0`, kegagalan dicatat pada log audit Billing. Pencatatan tagihan **tidak boleh** gagal karena Medical Fee tidak tersedia |
| Batas nilai | Hasil panggilan tetap tunduk pada batas yang sudah ada: tidak melebihi nilai kotor baris |
| Jalur ketik | Parameter `DoctorShare` pada `POST /invoices/from-source` diabaikan untuk sumber yang dicakup Medical Fee, dan dihapus pada versi kontrak berikutnya |

### 2.2 Entri kasir (jawaban 4 dan 5)

Usulan bentuk; nama final milik Billing.

| Kolom pada `BilInvoiceItem` | Tipe | Aturan |
|---|---|---|
| `IsFeeBearing` | `bool`, default `false` | Penanda kasir bahwa layanan berjasa |
| `PerformerReferenceId` | `Guid?` | **Wajib** bila `IsFeeBearing = true`; harus menunjuk tenaga medis yang ada |
| `PerformerRoleCode` | `string?` | Opsional; kosong dianggap peran utama |
| `PerformerType` | `string?` | **Wajib** bersama `PerformerReferenceId` (`BKC-DEC-178`). Menyatakan jenis rujukan, mis. `DOCTOR` atau `WORKFORCE`, karena Operating Room, Clinical, dan Laboratorium memakai identitas pelaksana yang berbeda. Nilai final dikunci desain Billing |

Entri `IsFeeBearing = true` tanpa pelaksana ditolak dengan `422` saat dicatat. Entri biasa (materai, administrasi, sewa) tidak berubah.

### 2.3 Keputusan owner Billing atas tiga butir yang menyertai

Diputuskan pemilik modul Billing, 26 Oktober 2026.

| # | Keputusan |
|---|---|
| a | `BilApHandoff` **tetap terbit** tanpa perubahan kode Billing dan diperlakukan hanya sebagai rujukan kesiapan. Medical Fee dan Finance menegaskan bahwa satu-satunya sumber utang jasa adalah `MdfFinanceHandoff` |
| b | Diskon bertipe dokter **boleh dipakai** pada item yang `DoctorShare`-nya berasal dari Medical Fee, dengan aturan yang sudah ada (maksimal sebesar `DoctorShare`, dengan persetujuan dokter). Dicatat sebagai keputusan agar tidak mengejutkan kasir |
| c | `DoctorShare` pada item = nilai kotor item x persentase **peran utama**. Pembagian tim dihitung Medical Fee saat periode dan tidak masuk `BilInvoiceItem` |

Alasan masing-masing butir:

| # | Hal | Mengapa |
|---|---|---|
| a | `BilApHandoff` **bukan** sumber pembayaran jasa | `BillingArApHandoffService` membentuk `BilApHandoff.Amount` dari jumlah `DoctorShare` dikurangi diskon dokter, dengan penerima dokter pada encounter. Hari ini jumlahnya nol karena semua jalur internal menulis `DoctorShare = 0`. Begitu Bentuk A mengisinya, handoff itu mulai bernilai. Kontrak Finance (`FIN-INTEGRATION-0.2` §3) sudah menyebutnya "rujukan kesiapan, bukan sumber nilai"; Medical Fee dan Finance diminta menegaskan bahwa **satu-satunya** sumber utang jasa adalah `MdfFinanceHandoff`, supaya dokter tidak dibayar dua kali |
| b | Diskon dokter menjadi dapat dipakai | `BillingDiscountService` menolak diskon bertipe dokter bila `DoctorShare <= 0`. Hari ini diskon itu praktis tidak dapat dipakai pada item sumber internal; setelah `DoctorShare` terisi, ia dapat. Ini perubahan perilaku yang disengaja atau tidak? Owner Billing perlu memutuskannya sebelum rilis |
| c | Tim banyak pelaksana | Endpoint `resolve` mengembalikan satu persentase untuk satu penerima dan satu peran. Satu item operasi dengan tiga anggota tim tidak terwakili. Usulan: `DoctorShare` pada item = persentase peran **utama**, sedangkan pembagian tim tetap dihitung Medical Fee saat periode. Mohon dikunci pada `BE-MDF-016` |

---

## 3. Koreksi atas permintaan awal

`evidence/01` bagian 2.1 menyebut `DoctorShare` "diketik petugas". Pemeriksaan source saat ini lebih sempit:

| Fakta | Bukti |
|---|---|
| Jalur entri kasir `ADHOC_CATALOG` dan `ADHOC` menulis `DoctorShare = 0` | `BillingInvoiceService.cs:1444`, `:1503` |
| Jembatan klinis menulis `DoctorShare = 0`, dengan komentar bahwa pembagian jasa milik modul medical-fee | `BillingClinicalChargeBridgeService.cs:305` |
| Satu-satunya jalur yang dapat membawa nilai bukan nol adalah isian pemanggil pada `POST /invoices/from-source` | `BillingInvoicesController.cs:253-260`, `BillingInvoiceService.cs:2531` |
| Batas nilai: tidak melebihi nilai kotor baris | `BillingInvoiceService.cs:2556-2557` |

Nomor baris pada `evidence/01` (`1855`, `1880-1881`) dan commit `09101d05` sudah bergeser. Dampaknya: perubahan pada Billing lebih kecil dari yang diperkirakan, karena hampir seluruh jalur sudah menulis nol.

---

## 4. Yang dibuka setelah balasan ini disetujui

| Task | Syarat tersisa | Catatan |
|---|---|---|
| `BE-MDF-015` | `BE-MDF-010` selesai; kolom pada 2.2 tersedia di Billing | Kolom dan migration milik tim Billing |
| `BE-MDF-016` | `BE-MDF-014` selesai; endpoint `resolve` dikunci pada `MDF-API` | Dapat dikerjakan paralel dengan sisi Billing |

Langkah dokumen di sisi Medical Fee setelah persetujuan:

1. `integration-contract.md` §4 dan §5: ubah `OPEN DECISION` menjadi bentuk terkunci; naik ke `MDF-INTEGRATION-1.1`.
2. `api-contract.md` baris 292: kunci endpoint `resolve`.
3. `00-interview-decisions.md`: tutup `MF-CQ-05` dan `MF-CQ-08`.
4. `roadmap/00-delivery-roadmap.md` dan `01-backend-roadmap.md`: ubah `BE-MDF-015` dan `BE-MDF-016` dari `BLOCKED` menjadi `READY`.
5. `blueprint-manifest.md`, `04-prd-to-mvp.md` (`MDF-07`, `MDF-11`), dan `02-frontend-roadmap.md`: sinkronkan.

Dari sisi Billing: kontrak charge naik dari `BIL-INTEGRATION-0.4` (versi yang dicatat pada `evidence/01`), migration kolom pelaksana, dan perubahan `UpsertChargeAsync`. Pekerjaan itu masuk roadmap Billing, bukan roadmap Medical Fee.
